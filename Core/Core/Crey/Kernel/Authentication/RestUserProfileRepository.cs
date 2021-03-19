/*using Core.Functional;
using Crey.Contracts;
using Crey.Utils;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crey.Kernel.Authentication
{
    /public static class RestUserProfileRepository
    {
        public static async Task<UserProfileInfo> SetNewsletterSubscribedAsync(this CreyRestClient creyRestClient, bool subscribed)
        {
            var headerContent = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("newslettersubscribe", subscribed.ToString())
            };

            var requestUri = $"{creyRestClient.GetBaseURI(AuthenticationDefaults.SERVICE_NAME)}/api/v1/userprofiles/newslettersubscribe";
            return await creyRestClient.PostAsync<UserProfileInfo>(requestUri, headerContent, null);
        }

        public static async Task<UserProfileInfo> GetAsync(this CreyRestClient creyRestClient, int accountId)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("accountId", accountId.ToString()),
            };

            var requestUri = $"{creyRestClient.GetBaseURI(AuthenticationDefaults.SERVICE_NAME)}/api/v1/userprofile";
            return await creyRestClient.GetAsync<UserProfileInfo>(requestUri, null, new FormUrlEncodedContent(content));
        }

        public static async Task<UserProfileInfo> GetMyAsync(this CreyRestClient creyRestClient)
        {
            var requestUri = $"{creyRestClient.GetBaseURI(AuthenticationDefaults.SERVICE_NAME)}/api/v1/userprofile";
            return await creyRestClient.GetAsync<UserProfileInfo>(requestUri, null, null);
        }

        public static async Task<List<UserProfileInfo>> ListByIdAsync(this CreyRestClient creyRestClient, List<int> accountIds)
        {
            var content = new List<KeyValuePair<string, string>>();
            foreach (var id in accountIds)
            {
                content.Add(new KeyValuePair<string, string>("accountId", id.ToString()));
            }

            var requestUri = $"{creyRestClient.GetBaseURI(AuthenticationDefaults.SERVICE_NAME)}/api/v1/userprofiles";
            return await creyRestClient.GetAsync<List<UserProfileInfo>>(requestUri, null, new FormUrlEncodedContent(content));
        }

        public static async Task<BinaryContent> GetProfileImageAsync(this CreyRestClient creyRestClient, int accountId, string etag)
        {
            var headerContent = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(HeaderNames.IfNoneMatch, etag)
            };

            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("accountId", accountId.ToString())
            };

            var requestUri = $"{creyRestClient.GetBaseURI(AuthenticationDefaults.SERVICE_NAME)}/api/v2/userprofiles/profileimage";
            return await creyRestClient.GetBinaryContentAsync(requestUri, headerContent, new FormUrlEncodedContent(content));
        }

        public static async Task<Result<BinaryContent, Error>> GetMyProfileImageAsync(this CreyRestClient creyRestClient, string etag)
        {
            var headerContent = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(HeaderNames.IfNoneMatch, etag)
            };

            var requestUri = $"{creyRestClient.GetBaseURI(AuthenticationDefaults.SERVICE_NAME)}/api/v2/userprofiles/profileimage";
            return await creyRestClient.GetBinaryContentAsync(requestUri, headerContent, null);
        }
    }
}*/