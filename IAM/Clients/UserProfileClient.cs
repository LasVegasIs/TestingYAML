using Core.Functional;
using Crey.Kernel;
using Crey.Kernel.Authentication;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace IAM.Clients
{
    public class UserProfileInfo
    {
        public int AccountId { get; set; }
        public string DisplayName { get; set; }
        public Uri ProfileImage { get; set; }
    }

    public static class UserProfileClient
    {
        public class UserProfileRegistrationInfo
        {
            public string DisplayName { get; set; }
            public bool NewsletterSubscribed { get; set; }
            public DateTime? DateOfBirth { get; set; }
        }

        private class CreateUserProfileData
        {
            public string DisplayName { get; set; }
            public bool NewsletterSubscribed { get; set; }
            public DateTime? DateOfBirth { get; set; }
        };

        public static Task CreateUserProfileAsync(
            this CreyRestClient creyRestClient,
            int accountId,
            string displayName,
            bool newsletterSubscribed,
            DateTime? dateOfBirth)
        {
            return creyRestClient
                .CreateRequest(HttpMethod.Put, AuthenticationDefaults.SERVICE_NAME, $"/userprofiles/s2s/v1/users/{accountId}")
                .AddS2SHeader()
                .SetContentJsonBody(new CreateUserProfileData
                {
                    DisplayName = displayName,
                    NewsletterSubscribed = newsletterSubscribed,
                    DateOfBirth = dateOfBirth
                })
                .SendAndAckAsync();
        }

        public static Task<UserProfileInfo> GetUserProfileAsync(this CreyRestClient creyRestClient, int accountId)
        {
            return creyRestClient
                .CreateRequest(HttpMethod.Get, AuthenticationDefaults.SERVICE_NAME, $"/api/v1/userprofile")
                .AddUserAuthentication()
                .SetContentUrlEncoded(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("accountId", accountId.ToString()),
                })
                .SendAndParseAsync<UserProfileInfo>();
        }

        public static Task<Result<Crey.Contracts.NoData, HttpResponseMessage>> PseudonymizeUserProfileAsync(this CreyRestClient creyRestClient, int accountId)
        {
            return creyRestClient
                .CreateRequest(HttpMethod.Put, AuthenticationDefaults.SERVICE_NAME, $"/userprofile/s2s/v1/userprofile/{accountId}/delete")
                .AddUserAuthentication()
                .AddS2SHeader()
                .SendAndTryAckAsync();
        }
    }
}
