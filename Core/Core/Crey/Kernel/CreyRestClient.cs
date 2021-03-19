using Core.Functional;
using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Exceptions;
using Crey.Kernel.Authentication;
using Crey.Web.Service2Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crey.Kernel
{
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
                catch (Exception ex)
                {
                    //todo: log, but we have no DI here for now
                    if (i + 1 >= maxRetries_)
                        throw ex;
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
        internal readonly SessionInfoStore sessionInfoStore_;
        internal readonly HttpClient httpClient_;

        public CreyRestClient(
            HttpClient httpClient,
            IConfiguration configuration,
            SessionInfoStore sessionInfoStore)
        {
            configuration_ = configuration;
            sessionInfoStore_ = sessionInfoStore;
            httpClient_ = httpClient;
        }

        public string GetBaseURI(string service)
        {
            return configuration_.GetBaseURI(service);
        }

        public CreyRequest CreateRequest(HttpMethod method, Uri uri)
        {
            return new CreyRequest(this, uri, method);
        }

        public CreyRequest CreateRequest(HttpMethod method, string service, string path)
        {
            if (!path.StartsWith("/")) path = "/" + path;
            return CreateRequest(method, new Uri($"{GetBaseURI(service)}{path}"));
        }

        public async Task<BinaryContent> GetBinaryContentAsync(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await InvokeHttpMethodBinaryAsync(HttpMethod.Get, requestUri, headerContent, content);
        }

        public async Task<Result<BinaryContent, HttpResponseMessage>> TryGetBinaryContentAsync(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await TryInvokeHttpMethodBinaryAsync(HttpMethod.Get, requestUri, headerContent, content);
        }

        public async Task<T> GetAsync<T>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await InvokeHttpMethodAsync<T>(HttpMethod.Get, requestUri, headerContent, content);
        }

        public async Task<Result<T, HttpResponseMessage>> TryGetAsync<T>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await TryInvokeHttpMethodAsync<T>(HttpMethod.Get, requestUri, headerContent, content);
        }

        public async Task<T> PostAsync<T>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await InvokeHttpMethodAsync<T>(HttpMethod.Post, requestUri, headerContent, content);
        }

        public async Task<Result<T, HttpResponseMessage>> TryPostAsync<T>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await TryInvokeHttpMethodAsync<T>(HttpMethod.Post, requestUri, headerContent, content);
        }

        public async Task<T> DeleteAsync<T>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await InvokeHttpMethodAsync<T>(HttpMethod.Delete, requestUri, headerContent, content);
        }

        public async Task<Result<T, HttpResponseMessage>> TryDeleteAsync<T>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await TryInvokeHttpMethodAsync<T>(HttpMethod.Delete, requestUri, headerContent, content);
        }

        private async Task<T> InvokeHttpMethodAsync<T>(HttpMethod httpMethod, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await (await TryInvokeHttpMethodAsync<T>(httpMethod, requestUri, headerContent, content))
                .UnwrapOrAsync(async (resp) => throw new CommunicationErrorException($"{requestUri} returned error code: {resp.StatusCode} and result {await resp.Content.ReadAsStringAsync()}"));
        }

        private async Task<Result<T, HttpResponseMessage>> TryInvokeHttpMethodAsync<T>(HttpMethod httpMethod, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            List<KeyValuePair<string, string>> modifiedHeaderContent = AddCreyAuth(headerContent, requestUri);
            using (var requestMessage = new HttpRequestMessage(httpMethod, requestUri))
            {
                foreach (var item in modifiedHeaderContent)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
                requestMessage.Content = content;

                HttpResponseMessage httpResponse = await httpClient_.SendAsync(requestMessage);
                if (!httpResponse.IsSuccessStatusCode)
                    return httpResponse;
                return await httpResponse.Content.ReadAsAsync<T>();
            }
        }

        private async Task<BinaryContent> InvokeHttpMethodBinaryAsync(HttpMethod httpMethod, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await (await TryInvokeHttpMethodBinaryAsync(httpMethod, requestUri, headerContent, content))
                .UnwrapOrAsync(async resp => throw new CommunicationErrorException($"{requestUri} returned error code: {resp.StatusCode} and result {await resp.Content.ReadAsStringAsync()}"));
        }

        private async Task<Result<BinaryContent, HttpResponseMessage>> TryInvokeHttpMethodBinaryAsync(HttpMethod httpMethod, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            List<KeyValuePair<string, string>> modifiedHeaderContent = AddCreyAuth(headerContent, requestUri);
            using (var requestMessage = new HttpRequestMessage(httpMethod, requestUri))
            {
                foreach (var item in modifiedHeaderContent)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
                requestMessage.Content = content;

                HttpResponseMessage httpResponse = await httpClient_.SendAsync(requestMessage);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    return httpResponse;
                }

                return new BinaryContent
                {
                    Data = await httpResponse.Content.ReadAsByteArrayAsync(),
                    ContentHash = httpResponse.Headers.ETag.Tag,
                    MimeType = httpResponse.Content.Headers.ContentType.MediaType
                };
            }
        }




        #region DEPRECATED
        public async Task<Result<TOk, Error>> GetAsync<TOk, TError>(string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Get, service, requestUri, headerContent, content);
        }

        public async Task<Result<BinaryContent, Error>> GetBinaryContentAsync<TError>(string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<Error>(HttpMethod.Get, service, requestUri, headerContent, content);
        }

        public async Task<Result<TOk, Error>> DeleteAsync<TOk, TError>(string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Delete, service, requestUri, headerContent, content);
        }

        public async Task<Result<TOk, Error>> PostAsync<TOk, TError>(string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Post, service, requestUri, headerContent, content);
        }

        public async Task<HttpStatusCode> PostNoDataAsync(string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await InvokeHttpMethodNoDataAsync(HttpMethod.Post, service, requestUri, headerContent, content);
        }

        public async Task<Result<TOk, Error>> PutAsync<TOk, TError>(string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Put, service, requestUri, headerContent, content);
        }
        public async Task<Result<TOk, Error>> GetForRequestUriAsync<TOk, TError>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Get, requestUri, headerContent, content);
        }

        public async Task<Result<BinaryContent, TError>> GetBinaryContentForRequestUriAsync<TError>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TError>(HttpMethod.Get, requestUri, headerContent, content);
        }

        public async Task<Result<TOk, Error>> DeleteForRequestUriAsync<TOk, TError>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Delete, requestUri, headerContent, content);
        }

        public async Task<Result<TOk, Error>> PostForRequestUriAsync<TOk, TError>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Post, requestUri, headerContent, content);
        }

        public async Task<HttpStatusCode> PostNoDataForRequestUriAsync(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await InvokeHttpMethodNoDataAsync(HttpMethod.Post, requestUri, headerContent, content);
        }

        public async Task<Result<TOk, Error>> PutForRequestUriAsync<TOk, TError>(string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(HttpMethod.Put, requestUri, headerContent, content);
        }

        private async Task<Result<TOk, Error>> DeprecatedInvokeHttpMethodAsync<TOk, TError>(HttpMethod httpMethod, string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            var uri = $"{GetBaseURI(service)}/{requestUri}";

            return await DeprecatedInvokeHttpMethodAsync<TOk, Error>(httpMethod, uri, headerContent, content);
        }

        private async Task<HttpStatusCode> InvokeHttpMethodNoDataAsync(HttpMethod httpMethod, string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            var uri = $"{GetBaseURI(service)}/{requestUri}";
            return await InvokeHttpMethodNoDataAsync(httpMethod, uri, headerContent, content);
        }

        private async Task<HttpStatusCode> InvokeHttpMethodNoDataAsync(HttpMethod httpMethod, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            List<KeyValuePair<string, string>> modifiedHeaderContent = AddCreyAuth(headerContent, requestUri);
            using (var requestMessage = new HttpRequestMessage(httpMethod, requestUri))
            {
                foreach (var item in modifiedHeaderContent)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
                requestMessage.Content = content;

                HttpResponseMessage httpResponse = await httpClient_.SendAsync(requestMessage);
                return httpResponse.StatusCode;
            }
        }

        private async Task<Result<BinaryContent, Error>> DeprecatedInvokeHttpMethodAsync<TError>(HttpMethod httpMethod, string service, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            var uri = $"{GetBaseURI(service)}/{requestUri}";
            return await DeprecatedInvokeHttpMethodAsync<BinaryContent, Error>(httpMethod, uri, headerContent, content);
        }

        private async Task<Result<BinaryContent, TError>> DeprecatedInvokeHttpMethodAsync<TError>(HttpMethod httpMethod, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            List<KeyValuePair<string, string>> modifiedHeaderContent = AddCreyAuth(headerContent, requestUri);
            using (var requestMessage = new HttpRequestMessage(httpMethod, requestUri))
            {
                foreach (var item in modifiedHeaderContent)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
                requestMessage.Content = content;

                HttpResponseMessage httpResponse = await httpClient_.SendAsync(requestMessage);
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    return await httpResponse.Content.ReadAsAsync<TError>();
                }

                return new BinaryContent
                {
                    Data = await httpResponse.Content.ReadAsByteArrayAsync(),
                    ContentHash = httpResponse.Headers.ETag.Tag,
                    MimeType = httpResponse.Content.Headers.ContentType.MediaType
                };
            }
        }

        private async Task<Result<TOk, Error>> DeprecatedInvokeHttpMethodAsync<TOk, TError>(HttpMethod httpMethod, string requestUri, List<KeyValuePair<string, string>> headerContent, HttpContent content)
        {
            try
            {
                return await InvokeHttpMethodAsync<TOk>(httpMethod, requestUri, headerContent, content);
            }
            catch (Exception e)
            {
                return new Error(ErrorCodes.CommunicationError, e.Message);
            }
        }

        #endregion

        private List<KeyValuePair<string, string>> AddCreyAuth(List<KeyValuePair<string, string>> originalHeaderContent, string uri)
        {
            List<KeyValuePair<string, string>> headerContent = originalHeaderContent;
            if (headerContent == null)
                headerContent = new List<KeyValuePair<string, string>>();

            if (!headerContent.Any(header => header.Key.Equals("crey-internal-key")))
            {
                var cookies = new List<CookieHeaderValue>();

                {
                    string cookieValue = sessionInfoStore_.GetSignedTokenString();
                    string cookieName = configuration_.GetSessionCookieName();
                    if (!string.IsNullOrEmpty(cookieValue) && !string.IsNullOrEmpty(cookieName))
                    {
                        cookies.Add(new CookieHeaderValue(cookieName, cookieValue));
                    }
                }

                // Add headers
                headerContent.AddService2ServiceHeaderPolicy(configuration_, Service2ServicePolicy.InternalPolicy);
                headerContent.Add(new KeyValuePair<string, string>(Microsoft.Net.Http.Headers.HeaderNames.Cookie, string.Join("; ", cookies)));
            }

            return headerContent;
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

        public CreyRequest AddUserAuthentication()
        {
            var cookies = new List<CookieHeaderValue>();
            string cookieValue = client_.sessionInfoStore_.GetSignedTokenString();
            string cookieName = client_.configuration_.GetSessionCookieName();
            if (!string.IsNullOrEmpty(cookieValue) && !string.IsNullOrEmpty(cookieName))
            {
                cookies.Add(new CookieHeaderValue(cookieName, cookieValue));
            }

            if (Headers.Any(header => header.Key.Equals(Microsoft.Net.Http.Headers.HeaderNames.Cookie)))
            {
                // if required we can add cookies to the headers and remove this exception
                throw new Exception("Cookie header already set");
            }

            return AddHeader(Microsoft.Net.Http.Headers.HeaderNames.Cookie, string.Join("; ", cookies));
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
                                throw new CommunicationErrorException($"{Uri} returned error code: {resp.StatusCode} and result {await resp.Content.ReadAsStringAsync()}");
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
                            throw new CommunicationErrorException($"{Uri} returned error code: {resp.StatusCode} and result {await resp.Content.ReadAsStringAsync()}");
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
                throw new CommunicationErrorException($"{Uri} returned error code: {response.StatusCode} and result {await response.Content.ReadAsStringAsync()}");
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
