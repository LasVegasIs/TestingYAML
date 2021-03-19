using Crey.Exceptions;
using Crey.Kernel.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Crey.Moderation
{
    public class TextModerationResult
    {
        public bool ReviewRecommended { get; set; }
        public double ExplicitSexuality { get; set; }
        public double SuggestiveSexuality { get; set; }
        public double Offensive { get; set; }

        public string Language { get; set; }
        public List<string> Terms { get; set; }

    }

    public class ImageModerationResult
    {
        public double AdultContent { get; set; }
        public double RacyContent { get; set; }
        public bool ReviewRecommended { get; set; }
    }

    class Category
    {
        public double Score { get; set; }
    }

    class TextClassification
    {
        public Category Category1 { get; set; }
        public Category Category2 { get; set; }
        public Category Category3 { get; set; }
        public bool ReviewRecommended { get; set; }
    }

    class TextTerm
    {
        public string Term { get; set; }
    }

    class ModerateTextResponse
    {
        public TextClassification Classification { get; set; }
        public string Language { get; set; }
        public List<TextTerm> Terms { get; set; }
    }

    public class CustomTermResult
    {
        public string Result { get; set; }
    }

    class ImageRepresentation
    {
        public string DataRepresentation { get; set; } = "URL";
        public string Value { get; set; }
    }

    class RefreshIndex
    {
        public bool IsUpdateSuccess { get; set; }
    }

    class ModerateImageResponse
    {
        public double AdultClassificationScore { get; set; }
        public bool IsImageAdultClassified { get; set; }
        public double RacyClassificationScore { get; set; }
        public bool IsImageRacyClassified { get; set; }
    }

    public class AzureModeration
    {
        private readonly string apiKey_;
        private readonly string textModerationEP_;
        private readonly string imageModerationEP_;
        private readonly string CustomTermEP;
        private readonly string listId;
        private readonly IHttpClientFactory httpClientFactory_;

        public AzureModeration(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            string moderationEP = config.GetValue<string>("AzureModerationEndpoint") ?? "https://creymoderator.cognitiveservices.azure.com/";
            apiKey_ = config.GetValue<string>("AzureModerationKey");
            httpClientFactory_ = httpClientFactory;

            listId = "1439";
            textModerationEP_ = moderationEP + "/contentmoderator/moderate/v1.0/ProcessText/Screen";
            imageModerationEP_ = moderationEP + "/contentmoderator/moderate/v1.0/ProcessImage/Evaluate";
            CustomTermEP = moderationEP + "/contentmoderator/lists/v1.0/termlists";
        }

        private async Task RefreshSearchIndex()
        {
            var uri = CustomTermEP + $"/{listId}/RefreshIndex?language=eng";

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", apiKey_);

                var httpClient = httpClientFactory_.CreateClient();
                var httpResponse = await httpClient.SendAsync(requestMessage);
                var response = await httpResponse.Content.ReadAsAsync<RefreshIndex>();
                if (!response.IsUpdateSuccess)
                    throw new CommunicationErrorException($"Failed to call Refresh" +
                                                          $" Search Index: {httpResponse.StatusCode} and " +
                                                          $"result {await httpResponse.Content.ReadAsStringAsync()}");

            }
        }

        public async Task<CustomTermResult> AddTerm(string term)
        {
            var uri = CustomTermEP + $"/{listId}/terms/{term}?language=eng";

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", apiKey_);

                var httpClient = httpClientFactory_.CreateClient();
                var httpResponse = await httpClient.SendAsync(requestMessage);
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.Created: /*noop*/ break;
                    case HttpStatusCode.Ambiguous: throw new Crey.Exceptions.HttpStatusErrorException(HttpStatusCode.Conflict, $"'{term}' already added to the list");
                    default: throw new CommunicationErrorException($"Failed to call AddTerm: {httpResponse.StatusCode} and result {await httpResponse.Content.ReadAsStringAsync()}");
                }
                await RefreshSearchIndex();
                return new CustomTermResult
                {
                    Result = $"Added '{term}' Successfully"
                };
            }
        }

        public async Task<CustomTermResult> DeleteTerm(string term)
        {
            var uri = CustomTermEP + $"/{listId}/terms/{term}?language=eng";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", apiKey_);

                var httpClient = httpClientFactory_.CreateClient();
                var httpResponse = await httpClient.SendAsync(requestMessage);
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.NoContent: /*noop*/ break;
                    default: throw new CommunicationErrorException($"Failed to call DeleteTerm: {httpResponse.StatusCode} and result {await httpResponse.Content.ReadAsStringAsync()}");
                }
                await RefreshSearchIndex();
                return new CustomTermResult
                {
                    Result = $"Deleted '{term}' Successfully"
                };
            }
        }

        public async Task<TextModerationResult> ModerateText(string text)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["autocorrect"] = "True";
            queryString["classify"] = "True";
            queryString["listId"] = "1439";
            //queryString["language"] = "eng";
            //queryString["PII"] = "{boolean}";

            var uri = textModerationEP_ + "?" + queryString;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", apiKey_);
                requestMessage.Content = new StringContent(text);

                var httpClient = httpClientFactory_.CreateClient();
                HttpResponseMessage httpResponse = await httpClient.SendAsync(requestMessage);
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.OK: /*noop*/ break;
                    case HttpStatusCode.TooManyRequests: throw new Crey.Exceptions.HttpStatusErrorException(HttpStatusCode.TooManyRequests, "Moderation quota exceeded limits", new { ServerResponse = await httpResponse.Content.ReadAsStringAsync() });
                    default: throw new CommunicationErrorException($"Failed to call moderation: {httpResponse.StatusCode} and result {await httpResponse.Content.ReadAsStringAsync()}");
                }

                var response = await httpResponse.Content.ReadAsAsync<ModerateTextResponse>();
                if (response.Classification != null && response.Terms != null)
                {
                    return new TextModerationResult
                    {
                        ExplicitSexuality = response.Classification.Category1.Score,
                        SuggestiveSexuality = response.Classification.Category2.Score,
                        Offensive = response.Classification.Category2.Score,
                        ReviewRecommended = response.Classification.ReviewRecommended,
                        Language = response.Language,
                        Terms = response.Terms?.Select(x => x.Term).ToList(),
                    };
                }
                else if (response.Terms != null && response.Terms.Any())
                {
                    return new TextModerationResult
                    {
                        ReviewRecommended = true,
                        Terms = response.Terms.Select(x => x.Term).ToList(),
                        Language = response.Language,
                    };
                }
                else
                {
                    return new TextModerationResult
                    {
                        ReviewRecommended = false,
                        Language = response.Language,
                    };
                }
            }
        }

        public async Task<ImageModerationResult> ModerateImage(string imageUri)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, imageModerationEP_))
            {
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", apiKey_);
                var json = JsonConvert.SerializeObject(new ImageRepresentation { Value = imageUri });
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = httpClientFactory_.CreateClient();
                HttpResponseMessage httpResponse = await httpClient.SendAsync(requestMessage);
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.OK: /*noop*/ break;
                    case HttpStatusCode.TooManyRequests: throw new Crey.Exceptions.HttpStatusErrorException(HttpStatusCode.TooManyRequests, "Moderation quota exceeded limits", new { ServerResponse = await httpResponse.Content.ReadAsStringAsync() });
                    default: throw new CommunicationErrorException($"Failed to call moderation: {httpResponse.StatusCode} and result {await httpResponse.Content.ReadAsStringAsync()}");
                }

                var response = await httpResponse.Content.ReadAsAsync<ModerateImageResponse>();
                return new ImageModerationResult
                {
                    AdultContent = response.AdultClassificationScore,
                    RacyContent = response.RacyClassificationScore,
                    ReviewRecommended = response.IsImageAdultClassified | response.IsImageRacyClassified,
                };
            }
        }
    }

    public static class AzureModerationExtensions
    {
        public static IServiceCollection AddAzureModeration(this IServiceCollection services)
        {
            services.AddScopedCreyServiceInternal<AzureModeration>();
            return services;
        }
    }
}
