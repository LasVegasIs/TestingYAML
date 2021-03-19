using Crey.Exceptions;
using IAM.Data;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class OAuthRepository
    {
        private readonly ILogger logger_;

        private readonly SignInManager<ApplicationUser> signInManager_;
        private readonly UserManager<ApplicationUser> userManager_;
        private readonly GoogleOptions googleOptions_;
        private readonly FacebookOptions facebookOptions_;

        public OAuthRepository(
            ILogger<SessionRepository> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IOptionsMonitor<GoogleOptions> googleOptionsMonitor,
            IOptionsMonitor<FacebookOptions> facebookOptionsMonitor)
        {
            logger_ = logger;
            signInManager_ = signInManager;
            userManager_ = userManager;
            googleOptions_ = googleOptionsMonitor.Get(GoogleDefaults.AuthenticationScheme);
            facebookOptions_ = facebookOptionsMonitor.Get(FacebookDefaults.AuthenticationScheme);
        }

        public async Task GoogleSignIn(string accessToken, CancellationToken cancellationToken)
        {
            var userInformation = await GetUserInformationFromGoogle(accessToken, cancellationToken);
            await SignInAsync(userInformation, GoogleDefaults.AuthenticationScheme);
        }

        public async Task FacebookSignIn(string accessToken, CancellationToken cancellationToken)
        {
            var userInformation = await GetUserInformationFromFacebook(accessToken, cancellationToken);
            await SignInAsync(userInformation, FacebookDefaults.AuthenticationScheme);
        }

        private async Task<ClaimsPrincipal> GetUserInformationFromGoogle(string accessToken, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, googleOptions_.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await googleOptions_.Backchannel.SendAsync(request, cancellationToken);
            return await ParseUserInformationAsync(
                response,
                googleOptions_.ClaimsIssuer ?? GoogleDefaults.AuthenticationScheme,
                googleOptions_.ClaimActions);
        }

        private async Task<ClaimsPrincipal> GetUserInformationFromFacebook(string accessToken, CancellationToken cancellationToken)
        {
            var endpoint = QueryHelpers.AddQueryString(facebookOptions_.UserInformationEndpoint, "access_token", accessToken);
            if (facebookOptions_.SendAppSecretProof)
            {
                endpoint = QueryHelpers.AddQueryString(endpoint, "appsecret_proof", GenerateFacebookAppSecretProof(accessToken));
            }

            if (facebookOptions_.Fields.Count > 0)
            {
                endpoint = QueryHelpers.AddQueryString(endpoint, "fields", string.Join(",", facebookOptions_.Fields));
            }

            var response = await facebookOptions_.Backchannel.GetAsync(endpoint, cancellationToken);
            return await ParseUserInformationAsync(
                response,
                facebookOptions_.ClaimsIssuer ?? FacebookDefaults.AuthenticationScheme,
                facebookOptions_.ClaimActions);
        }

        private string GenerateFacebookAppSecretProof(string accessToken)
        {
            using (var algorithm = new HMACSHA256(Encoding.ASCII.GetBytes(facebookOptions_.AppSecret)))
            {
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
                var builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                }
                return builder.ToString();
            }
        }

        private async Task<ClaimsPrincipal> ParseUserInformationAsync(HttpResponseMessage response, string claimsIssuer, ClaimActionCollection claimActions)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving user information from {claimsIssuer}: ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            var claimsIdentity = new ClaimsIdentity(claimsIssuer);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                foreach (var action in claimActions)
                {
                    action.Run(payload.RootElement, claimsPrincipal.Identity as ClaimsIdentity, claimsIssuer);
                }
            }

            return claimsPrincipal;
        }

        private async Task SignInAsync(ClaimsPrincipal claimsPrincipal, string loginProvider)
        {
            var providerKey = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await signInManager_.ExternalLoginSignInAsync(
                loginProvider,
                providerKey,
                isPersistent: true,
                bypassTwoFactor: true);

            if (result.Succeeded)
            {
                return;
            }

            // TODO: registration and account linking is waiting for design decisions
            // if (userInformation.HasClaim(c => c.Type == ClaimTypes.Email))
            // {
            //     string emailClaim = userInformation.FindFirstValue(ClaimTypes.Email);
            //     ApplicationUser user = await userManager_.FindByEmailAsync(emailClaim);
            //     if (user == null)
            //     {
            //         throw new HttpStatusErrorException(HttpStatusCode.NotFound,"Failed to sign in with Google, create a new a account!");
            //     }
            //     throw new HttpStatusErrorException(HttpStatusCode.Forbidden, emailClaim);
            // }

            throw new HttpStatusErrorException(HttpStatusCode.Unauthorized, $"Failed to sign in with {loginProvider}");
        }
    }
}