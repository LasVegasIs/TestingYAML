using Crey.Instrumentation.Web;
using Crey.Utils;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crey.Authentication
{
    public class CheckKeyParams
    {
        [Required]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Key length should be between 10 and 100.")]
        public string Key { get; set; }
    }

    public static class RestAccountRepository
    {
        private const string IAM_SERVICE_NAME = "iam";

        public static Task<Result<SessionInfo, HttpResponseMessage>> ValidateKeyAsync(this CreyRestClient creyClient, string key, string userAgent)
        {
            var request = creyClient.CreateRequest(HttpMethod.Post, IAM_SERVICE_NAME, "/iam/s2s/accounts/validate/key").AddS2SHeader();
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.AddUserAgentHeader(userAgent);
            }

            request.SetContentJsonBody(new CheckKeyParams { Key = key });
            return request.SendAndTryParseAsync<SessionInfo>();
        }

        public static Task<UserInfo> GetUserInfoAsync(this CreyRestClient creyClient, int accountId)
        {
            var request = creyClient.CreateRequest(HttpMethod.Get, IAM_SERVICE_NAME, $"/iam/s2s/accounts/{accountId}/roles").AddS2SHeader();
            return request.SendAndParseAsync<UserInfo>();
        }

        public static Task<SessionInfo> ImpersonateAccount(this CreyRestClient creyClient, int accountId)
        {
            var request = creyClient.CreateRequest(HttpMethod.Post, IAM_SERVICE_NAME, $"/iam/s2s/accounts/{accountId}/signin").AddS2SHeader();
            return request.SendAndParseAsync<SessionInfo>();
        }

        public static async Task ValidateContentAccessRightAsync(this CreyRestClient creyRestClient, UserInfo userInfo, int contentOwnerId)
        {
            if (userInfo.AccountId != contentOwnerId)
            {
                if (userInfo.AccountId == 0)
                    throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "Login required");
                // from this point only ContentDev role is required for the logged in user
                if (!userInfo.Roles.Contains(UserRoles.ContentDev))
                    throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, "Content dev role required");
                // both user (requesting and owner) are content dev
                var contentOwner = await creyRestClient.GetUserInfoAsync(contentOwnerId);
                if (!contentOwner.Roles.Contains(UserRoles.ContentDev))
                    throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, $"Cannot acces content of owner ({contentOwnerId}) as user has no ContentDev role ({contentOwner}). (You have: {userInfo.AccountId})");
            }
        }
    }
}

