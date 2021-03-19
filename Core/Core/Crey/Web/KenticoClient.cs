using Crey.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Crey.Web
{
    public class KenticoClient
    {
        private readonly string baseUri_;
        private readonly IConfiguration configuration_;
        private readonly IHttpClientFactory httpClientFactory_;

        public KenticoClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            configuration_ = configuration;
            var kenticoProjectId = configuration_.GetValue<string>("Kentico:ProjectId");
            baseUri_ = $"https://deliver.kenticocloud.com/{kenticoProjectId}";
            httpClientFactory_ = httpClientFactory;
        }

        public Task<JObject> Get(string requestUri, IEnumerable<KeyValuePair<string, string>> content)
        {
            return InvokeHttpMethod(HttpMethod.Get, requestUri, content);
        }

        private async Task<JObject> InvokeHttpMethod(HttpMethod httpMethod, string requestUri, IEnumerable<KeyValuePair<string, string>> content)
        {
            var kenticoToken = configuration_.GetValue<string>("Kentico:Token");

            var uri = $"{baseUri_}/{requestUri}";
            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            if (content != null)
            {
                foreach (var item in content)
                {
                    query[item.Key] = item.Value;
                }
            }
            builder.Query = query.ToString();

            using (var requestMessage = new HttpRequestMessage(httpMethod, builder.ToString()))
            {
                requestMessage.Headers.Add(HeaderNames.Authorization, $"Bearer {kenticoToken}");

                var httpClient = httpClientFactory_.CreateClient();
                HttpResponseMessage httpResponse = await httpClient.SendAsync(requestMessage);
                string responseContent = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new ServerErrorException($"Calling {uri} failed: {httpResponse.StatusCode} - {responseContent}");
                }

                return JObject.Parse(responseContent);
            }
        }
    }

    public static class KenticoClientExtensions
    {
        public static IServiceCollection AddKenticoClient(this IServiceCollection collectionBuilder)
        {
            if (!collectionBuilder.Any(x => x.ServiceType == typeof(KenticoClient)))
            {
                collectionBuilder.TryAddSingleton<KenticoClient>();
            }
            return collectionBuilder;
        }
    }
}
