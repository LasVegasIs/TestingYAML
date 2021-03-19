using Core.Functional;
using Crey.Contracts;
using Crey.Exceptions;
using Crey.Extensions;
using Crey.Kernel.IAM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crey.Kernel.Authentication
{
    public static class RestAccountRepository
    {
        [Obsolete]
        public static async Task<Result<SignInResult, Error>> SignInAsGuestAsync(this CreyRestClient creyClient, string siteId)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("siteId", siteId)
            };

            var result = await creyClient.PostAsync<SignInResult, Error>(AuthenticationDefaults.SERVICE_NAME, "api/v1/accounts/signin/guest", null, new FormUrlEncodedContent(content));
            if (!result.IsOk)
            {
                return result;
            }

            return result;
        }

        [Obsolete]
        public static async Task<SignInResult> SignInWithEmailAsync(this CreyRestClient creyClient, string siteId, string emailOrNick, string password)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("siteId", siteId),
                new KeyValuePair<string, string>("emailOrUsername", emailOrNick),
                new KeyValuePair<string, string>("password", password),
            };

            var result = await creyClient.PostAsync<SignInResult, Error>(AuthenticationDefaults.SERVICE_NAME, "api/v1/accounts/signin/user", null, new FormUrlEncodedContent(content));
            if (!result.IsOk)
            {
                throw new ServerErrorException($"Sign in with email failed with {result.Error.ErrorCode} - {result.Error.Message}");
            }

            return result.Ok;
        }

        [Obsolete]
        public static async Task<Result<SignInResult, Error>> SignInWithKeyAsync(this CreyRestClient creyClient, string key)
        {
            var content = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("key", key),
                };

            var result = await creyClient.PostAsync<SignInResult, Error>(AuthenticationDefaults.SERVICE_NAME, "api/v1/accounts/signin/key", null, new FormUrlEncodedContent(content));
            if (!result.IsOk)
            {
                return result;
            }
            return result;
        }

        public static async Task<SignInResult> SignInWithAccountIdAsync(this CreyRestClient creyClient, int accountId)
        {
            var content = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("accountId", accountId.ToString()),
                };

            var result = (await creyClient.PostAsync<SignInResult, Error>(AuthenticationDefaults.SERVICE_NAME, "api/v1/accounts/signin/accountId", null, new FormUrlEncodedContent(content)))
                .UnwrapOr(err =>
                {
                    throw new CommunicationErrorException($"sigin in ba account id failed: {err}");
                });

            return result;
        }

        [Obsolete]
        public static async Task<Result<NoData, Error>> SignOutAsync(this CreyRestClient creyClient)
        {
            var result = await creyClient.PostAsync<NoData, Error>(AuthenticationDefaults.SERVICE_NAME, "api/v1/accounts/signout", null, null);
            return result;
        }

        public static Task<Result<SessionInfo, HttpResponseMessage>> ValidateKeyAsync(this CreyRestClient creyClient, string key, string userAgent)
        {
            var request = creyClient.CreateRequest(HttpMethod.Post, IAMDefaults.SERVICE_NAME, "/iam/s2s/accounts/validate/key").AddS2SHeader();
            if (!userAgent.IsNullOrEmpty())
            {
                request.AddUserAgentHeader(userAgent);
            }

            request.SetContentJsonBody(new CheckKeyParams { Key = key });
            return request.SendAndTryParseAsync<SessionInfo>();
        }

        public static Task<UserInfo> GetUserInfoAsync(this CreyRestClient creyClient, int accountId)
        {
            var request = creyClient.CreateRequest(HttpMethod.Get, IAMDefaults.SERVICE_NAME, $"/iam/s2s/accounts/{accountId}/roles").AddS2SHeader();
            return request.SendAndParseAsync<UserInfo>();
        }

        public static Task<SessionInfo> ImpersonateAccount(this CreyRestClient creyClient, int accountId)
        {
            var request = creyClient.CreateRequest(HttpMethod.Post, IAMDefaults.SERVICE_NAME, $"/iam/s2s/accounts/{accountId}/signin").AddS2SHeader();
            return request.SendAndParseAsync<SessionInfo>();
        }

        [Obsolete]
        public static async Task<Result<SignInResult, Error>> CheckKeyAsync(this CreyRestClient creyClient, string key, string userAgent)
        {
            var request = creyClient.CreateRequest(HttpMethod.Post, AuthenticationDefaults.SERVICE_NAME, "/api/v1/accounts/signin/test");
            if (!userAgent.IsNullOrEmpty())
            {
                // C++ client and asset uploader user "siteId" parameter instead of User-Agent header, to be fixed when they migrate to IAM login
                request.AddUserAgentHeader(userAgent);
            }

            return await request
                .SetContentUrlEncoded(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", key) })
                .SendAndParseAsync<SignInResult>();
        }

        [Obsolete]
        public static async Task<HashSet<string>> GetRolesSetAsync(this CreyRestClient creyClient, int accountId)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("accountId", accountId.ToString()),
            };

            return (await creyClient.GetAsync<List<string>, Error>(AuthenticationDefaults.SERVICE_NAME, $"api/v2/accounts/roles", null, new FormUrlEncodedContent(content)))
                .UnwrapOr(err => throw new CommandErrorException<NoData>(err.Message))
                .ToHashSet();
        }

        public static async Task ValidateContentAccessRightAsync(this CreyRestClient creyRestClient, UserInfo userInfo, int contentOwnerId)
        {
            if (userInfo.AccountId != contentOwnerId)
            {
                if (userInfo.AccountId == 0)
                    throw new AccountNotFoundException("Login required");
                // from this point only ContentDev role is required for the logged in user
                if (!userInfo.Roles.Contains(UserRoles.ContentDev))
                    throw new AccessDeniedException("Content dev role required");
                // both user (requesting and owner) are content dev
                var contentOwner = await creyRestClient.GetUserInfoAsync(contentOwnerId);
                if (!contentOwner.Roles.Contains(UserRoles.ContentDev))
                    throw new AccessDeniedException($"Cannot acces content of owner ({contentOwnerId}) as user has no ContentDev role ({contentOwner}). (You have: {userInfo.AccountId})");
            }
        }

        [Obsolete]
        public static async Task<Error> CheckContentAccessRightAsync(this CreyRestClient creyRestClient, SessionInfo sessionInfo, int contentOwnerId)
        {
            if (sessionInfo.AccountId == 0)
                return ErrorCodes.AccountNotFound.IntoError($"Login required");

            if (sessionInfo.AccountId == contentOwnerId)
                return Error.NoError;

            // from this point only ContentDev role is required for the logged in user
            if (!sessionInfo.Roles.Contains(UserRoles.ContentDev))
                return ErrorCodes.AccessDenied.IntoError($"Content dev role required");

            // both user (requesting and owner) are content dev
            var res = await creyRestClient.GetRolesSetAsync(contentOwnerId);
            if (!res.Contains(UserRoles.ContentDev))
                return ErrorCodes.AccessDenied.IntoError($"Cannot acces content of owner ({contentOwnerId}) as user has no contentdev role ({res}). (You have: {sessionInfo.AccountId})");
            else
                return Error.NoError;
        }
    }
}

