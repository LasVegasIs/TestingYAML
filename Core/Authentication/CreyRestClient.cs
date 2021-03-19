using Crey.Instrumentation.Configuration;
using Crey.Instrumentation.Web;
using Crey.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crey.Authentication
{
    public class NoData { }

    public class HttpLoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            return response;
        }
    }

    public class HttpRetryHandler : DelegatingHandler
    {
        private int maxRetries_ = 5;
        private int waitTime_ = 100;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int i = 0;
            for (; ; )
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await base.SendAsync(request, cancellationToken);
                    if (response != null)
                    {
                        bool retry = false;
                        // to test maxretry and eror handling on failures: retry = retry | response.StatusCode == HttpStatusCode.OK;
                        retry = retry | response.StatusCode == HttpStatusCode.ServiceUnavailable;
                        retry = retry | response.StatusCode == HttpStatusCode.GatewayTimeout;
                        retry = retry | response.StatusCode == HttpStatusCode.InternalServerError;
                        retry = retry | response.StatusCode == HttpStatusCode.TooManyRequests;
                        if (!retry)
                            return response;
                    }
                }
                catch (Exception)
                {
                    //todo: log, but we have no DI here for now
                    if (i + 1 >= maxRetries_)
                        throw;
                }

                i += 1;
                if (i >= maxRetries_)
                {
                    // return response whatever it was and let the client handle it.
                    return response;
                }

                if (response != null)
                {
                    try { response.Dispose(); } catch { }
                }
                if (waitTime_ > 0)
                {
                    var timeout = (int)(waitTime_ * Math.Pow(1.8, i));
                    await Task.Delay(timeout);
                }
            }
        }
    }

    public class CreyRestClient
    {
        internal readonly IConfiguration configuration_;
        internal readonly HttpClient httpClient_;

        public CreyRestClient(HttpClient httpClient, IConfiguration configuration)
        {
            configuration_ = configuration;
            httpClient_ = httpClient;
        }

        public string GetBaseURI(string service)
        {
            return configuration_.GetBaseURI(service);
        }

        /// Use this function to call non-crey services only due to the service base uri resolution.
        public CreyRequest CreateRequestForExternalService(HttpMethod method, Uri uri)
        {
            return new CreyRequest(this, uri, method);
        }

        public CreyRequest CreateRequest(HttpMethod method, string service, string path)
        {
            if (!path.StartsWith("/")) path = "/" + path;
            var uriStr = $"{GetBaseURI(service)}{path}";
            Uri uri;
            try
            {
                uri = new Uri(uriStr);
            }
            catch (Exception)
            {
                throw new InternalServerErrorException($"Malformed uri: {uriStr}");
            }
            return CreateRequestForExternalService(method, uri);
        }
    }

    public class CreyRequest
    {
        private readonly CreyRestClient client_;

        public Uri Uri { get; }
        public HttpMethod Method { get; }
        public List<KeyValuePair<string, string>> Headers;
        public HttpContent Content { get; set; }

        internal CreyRequest(CreyRestClient client, Uri uri, HttpMethod method)
        {
            client_ = client;
            Uri = uri;
            Method = method;
            Headers = new List<KeyValuePair<string, string>>();
        }

        public CreyRequest AddHeader(string name, string value)
        {
            Headers.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }

        public CreyRequest AddUserAgentHeader(string userAgent)
        {
            return AddHeader(Microsoft.Net.Http.Headers.HeaderNames.UserAgent, userAgent);
        }

        public CreyRequest AddS2SHeader(Service2ServicePolicy policiy)
        {
            Headers.AddService2ServiceHeaderPolicy(client_.configuration_, policiy);
            return this;
        }

        public CreyRequest AddS2SHeader()
        {
            return AddS2SHeader(Service2ServicePolicy.InternalPolicy);
        }

        public CreyRequest SetContentUrlEncoded(List<KeyValuePair<string, string>> content)
        {
            Content = new FormUrlEncodedContent(content);
            return this;
        }
        public CreyRequest SetContentJsonBody(object content)
        {
            var json = JsonConvert.SerializeObject(content);
            Content = new StringContent(json, Encoding.UTF8, "application/json");
            return this;
        }

        public CreyRequest SetContentText(string content)
        {
            Content = new StringContent(content, Encoding.UTF8, "text/plain");
            return this;
        }

        public async Task<HttpResponseMessage> SendAsync()
        {
            using (var requestMessage = new HttpRequestMessage(Method, Uri))
            {
                foreach (var item in Headers)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
                requestMessage.Content = Content;

                return await client_.httpClient_.SendAsync(requestMessage);
            }
        }

        public async Task<Result<BinaryContent, HttpResponseMessage>> SendAndTryGetBinaryAsync()
        {
            var response = await SendAsync();
            if (!response.IsSuccessStatusCode)
            {
                return response;
            }

            var res = new BinaryContent
            {
                Data = await response.Content.ReadAsByteArrayAsync(),
                ContentHash = response.Headers.ETag.Tag,
                MimeType = response.Content.Headers.ContentType.MediaType
            };
            response.Dispose();
            return res;
        }

        public async Task<BinaryContent> SendAndGetBinaryAsync()
        {
            return await (await SendAndTryGetBinaryAsync())
                .UnwrapOrAsync(
                    async (resp) =>
                        {
                            using (resp)
                                throw new HttpStatusErrorException(HttpStatusCode.InternalServerError, $"{Uri} returned error code: {resp.StatusCode} and result {await resp.Content.ReadAsStringAsync()}");
                        }
                );
        }

        public async Task<Result<T, HttpResponseMessage>> SendAndTryParseAsync<T>()
        {
            var response = await SendAsync();
            if (!response.IsSuccessStatusCode)
            {
                return response;
            }

            var res = await response.Content.ReadAsAsync<T>();
            response.Dispose();
            return res;
        }

        public async Task<T> SendAndParseAsync<T>()
        {
            return await (await SendAndTryParseAsync<T>())
                .UnwrapOrAsync(
                    async (resp) =>
                    {
                        using (resp)
                            throw new HttpStatusErrorException(HttpStatusCode.InternalServerError, $"{Uri} returned error code: {resp.StatusCode} and result {await resp.Content.ReadAsStringAsync()}");
                    }
                );
        }

        public async Task<Result<NoData, HttpResponseMessage>> SendAndTryAckAsync()
        {
            return await SendAndTryParseAsync<NoData>();
        }

        public async Task SendAndAckAsync()
        {
            await SendAndParseAsync<NoData>();
        }


        // In acceptResponse return null to throw the usual communication error.
        public async Task<bool> SendAndCheckResult(Func<HttpStatusCode, bool?> acceptResponse)
        {
            using (var response = await SendAsync())
            {
                var r = acceptResponse(response.StatusCode);
                if (r.HasValue)
                    return r.Value;
                throw new HttpStatusErrorException(HttpStatusCode.InternalServerError, $"{Uri} returned error code: {response.StatusCode} and result {await response.Content.ReadAsStringAsync()}");
            };
        }
    }

    public static class CreyRestClientExtensions
    {
        public static IServiceCollection TryAddHttpRestClient(this IServiceCollection collectionBuilder)
        {
            if (!collectionBuilder.Any(x => x.ServiceType == typeof(IHttpClientFactory)))
            {
                collectionBuilder.AddTransient<HttpRetryHandler>();
                collectionBuilder.AddTransient<HttpLoggingHandler>();

                collectionBuilder.AddHttpClient<CreyRestClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        UseCookies = false, // <- set this as false to stop automatic cookie handling
                        // can allow running s2s no cert locally if needed:
                        //ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                    })
                    .AddHttpMessageHandler<HttpRetryHandler>()
                    .AddHttpMessageHandler<HttpLoggingHandler>();
            }
            return collectionBuilder;
        }

        public static IServiceCollection AddCreyRestClientFactory(this IServiceCollection collectionBuilder)
        {
            collectionBuilder.TryAddHttpRestClient();
            collectionBuilder.TryAddTransient<CreyRestClient>();
            return collectionBuilder;
        }
    }
}
