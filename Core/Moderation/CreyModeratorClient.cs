using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Crey.Instrumentation.Web;
using Crey.Instrumentation.Configuration;

namespace Crey.Moderation
{
    public class ScreenTextResult
    {
        public bool IsModerated { get; set; }
        public bool ReviewRecommended { get; set; }
        public double ExplicitSexuality { get; set; }
        public double SuggestiveSexuality { get; set; }
        public double Offensive { get; set; }
        public string Language { get; set; }
        public IEnumerable<string> Terms { get; set; }
    }

    public class EvaluateImageResult
    {
        public double AdultContent { get; set; }
        public double RacyContent { get; set; }
        public bool ReviewRecommended { get; set; }
    }

    public class CreyModeratorClient
    {
        private const string LANGUAGE = "eng";

        private readonly ContentModeratorClient _contentModeratorClient;
        private readonly string _deploymentSlot;

        public CreyModeratorClient(IConfiguration configuration)
        {
            _contentModeratorClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(configuration.GetValue<string>("AzureModerationKey")));
            _contentModeratorClient.Endpoint = configuration.GetValue<string>("AzureModerationEndpoint");
            _deploymentSlot = configuration.GetDeploymentSlot();
        }

        public async Task CreateTermListAsync(CancellationToken cancellationToken = default)
        {
            var termList = await GetTermListAsync(cancellationToken);
            if (termList == null)
            {
                await CreateTermListAsync(_deploymentSlot, _deploymentSlot, cancellationToken);
            }
        }

        private async Task<string> CreateTermListAsync(string name, string description, CancellationToken cancellationToken = default)
        {
            try
            {
                var body = new Body(name, description);
                TermList list = await _contentModeratorClient.ListManagementTermLists.CreateAsync("application/json", body, cancellationToken);
                return list.Id.Value.ToString();
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        public async Task<TermList> GetTermListAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var termLists = await _contentModeratorClient.ListManagementTermLists.GetAllTermListsAsync(cancellationToken);
                return termLists.FirstOrDefault(x => x.Name == _deploymentSlot);
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        public async Task AddTermAsync(string term, CancellationToken cancellationToken = default)
        {
            try
            {
                var termList = await GetTermListAsync(cancellationToken);
                if (termList != null)
                {
                    await _contentModeratorClient.ListManagementTerm.AddTermAsync(termList.Id.ToString(), term, LANGUAGE, cancellationToken);
                    await RefreshSearchIndexAsync(termList.Id.ToString());
                }
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        public async Task<IEnumerable<string>> GetAllTermsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var termList = await GetTermListAsync(cancellationToken);
                if (termList != null)
                {
                    Terms terms = await _contentModeratorClient.ListManagementTerm.GetAllTermsAsync(termList.Id.ToString(), LANGUAGE, cancellationToken: cancellationToken);
                    TermsData data = terms.Data;
                    return data.Terms.Select(x => x.Term);
                }

                return new List<string>();
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        public async Task DeleteTermAsync(string term, CancellationToken cancellationToken = default)
        {
            try
            {
                var termList = await GetTermListAsync(cancellationToken);
                if (termList != null)
                {
                    await _contentModeratorClient.ListManagementTerm.DeleteTermAsync(termList.Id.ToString(), term, LANGUAGE, cancellationToken);
                    await RefreshSearchIndexAsync(termList.Id.ToString());
                }
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        public async Task DeleteAllTermsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var termList = await GetTermListAsync(cancellationToken);
                if (termList != null)
                {
                    await _contentModeratorClient.ListManagementTerm.DeleteAllTermsAsync(termList.Id.ToString(), LANGUAGE, cancellationToken);
                    await RefreshSearchIndexAsync(termList.Id.ToString());
                }
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        private Task RefreshSearchIndexAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return _contentModeratorClient.ListManagementTermLists.RefreshIndexMethodAsync(id, LANGUAGE, cancellationToken);
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        // mimeType can be "text/html", "text/xml", "text/markdown", or "text/plain".
        public async Task<ScreenTextResult> ScreenTextAsync(string text, string mimeType = "text/plain", CancellationToken cancellationToken = default)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            try
            {
                var termList = await GetTermListAsync(cancellationToken);
                if (termList == null)
                {
                    return new ScreenTextResult();
                }

                Screen screen = await _contentModeratorClient.TextModeration.ScreenTextAsync(mimeType, stream, LANGUAGE, autocorrect: true, pII: true, termList.Id.ToString(), classify: true, cancellationToken);
                return new ScreenTextResult
                {
                    IsModerated = screen.Terms != null && screen.Terms.Any(),
                    ReviewRecommended = screen.Classification.ReviewRecommended.GetValueOrDefault(),
                    ExplicitSexuality = screen.Classification.Category1.Score.GetValueOrDefault(),
                    SuggestiveSexuality = screen.Classification.Category2.Score.GetValueOrDefault(),
                    Offensive = screen.Classification.Category3.Score.GetValueOrDefault(),
                    Language = screen.Language,
                    Terms = screen.Terms?.Select(x => x.Term)
                };
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        public async Task<EvaluateImageResult> EvaluateImageAsync(string imageUri, CancellationToken cancellationToken = default)
        {
            try
            {
                var body = new BodyModel(imageUri);
                var evaluationResult = await _contentModeratorClient.ImageModeration.EvaluateUrlInputAsync("application/json", body, null, cancellationToken);
                return new EvaluateImageResult
                {
                    AdultContent = evaluationResult.AdultClassificationScore.GetValueOrDefault(),
                    RacyContent = evaluationResult.RacyClassificationScore.GetValueOrDefault(),
                    ReviewRecommended = evaluationResult.IsImageAdultClassified.GetValueOrDefault() | evaluationResult.IsImageRacyClassified.GetValueOrDefault(),
                };
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

#region TEMPORARY_MIGRATION
        public Task<IList<TermList>> GetTermListsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return _contentModeratorClient.ListManagementTermLists.GetAllTermListsAsync(cancellationToken);
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }

        public async Task DeleteTermListAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _contentModeratorClient.ListManagementTermLists.DeleteAsync(id, cancellationToken);
            }
            catch (APIErrorException e)
            {
                throw new HttpStatusErrorException(e.Response.StatusCode, e.Response.Content);
            }
        }
#endregion
    }

    public static class CreyModeratorClientExtensions
    {
        public static IServiceCollection AddCreyModeratorClient(this IServiceCollection services)
        {
            return services.AddScoped<CreyModeratorClient>();
        }
    }
}