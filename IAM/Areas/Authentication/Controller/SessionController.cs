using Crey.Contracts;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web.Analytics;
using Crey.Web.Service2Service;
using IAM.Clients;
using IAM.Contracts;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    [ApiController]
    [EnableCors]
    public class SessionController : ControllerBase
    {
        [Obsolete("only for auth --> iam transition")]
        private readonly ICreyService<SessionTokenRepository> sessionTokenRepository_;
        private readonly ICreyService<SessionRepository> sessionRepository_;
        private readonly CreySignInManager signInManager_;
        private readonly IIDInfoAccessor idInfo_;
        private readonly ILogger<SessionController> logger_;

        public SessionController(
            ICreyService<SessionTokenRepository> sessionTokenRepository,
            ICreyService<SessionRepository> sessionRepository,
            CreySignInManager signInManager,
            IIDInfoAccessor idInfo,
            ILogger<SessionController> logger)
        {
            sessionTokenRepository_ = sessionTokenRepository;
            sessionRepository_ = sessionRepository;
            signInManager_ = signInManager;
            idInfo_ = idInfo;
            logger_ = logger;
        }

        [HttpPost("/api/v1/session/refresh")]
        [Authorize]
        public async Task<ActionResult> RefreshSession()
        {
            SiteInfo siteInfo = await Request.HttpContext.GetSiteInfo();
            var sessionInfo = idInfo_.GetSessionInfo();
            await signInManager_.SignInAsync(sessionInfo.AccountId, true, CredentialType.RefreshKey.ToString());
            return Ok();
        }

        [Obsolete("only for auth --> iam transition")]
        [HttpPost("/iam/api/v1/accounts/signin/key")]
        public async Task SignInWithKey(SignInWithKeyParams signInWithKeyParams)
        {
            SiteInfo siteInfo = await Request.HttpContext.GetSiteInfo();
            var accountId = await sessionTokenRepository_.Value.FindUserByToken(signInWithKeyParams.Key, siteInfo);
            await signInManager_.SignInAsync(accountId, true, CredentialType.MultiAccessKey.ToString());
        }

        [HttpPost("/iam/api/v1/accounts/signout")]
        [Authorize]
        public async Task SignOutAsync([FromServices] AnalyticsClient analyticsClient)
        {
            await signInManager_.SignOutAsync();
            logger_.LogInformation("User logged out.");
            analyticsClient.SendLogoutEvent();
        }

        /// <summary>
        /// Given token, validates it and returns account info.
        /// </summary>
        [HttpPost("/iam/s2s/accounts/validate/key")]
        [ServerToServer]
        public async Task<SessionInfo> CheckKey(
            CheckKeyParams checkKeyParams,
            [FromServices] ICreyService<SessionRepository> sessionTokenRepository)
        {
            SiteInfo siteInfo = await Request.HttpContext.GetSiteInfo();
            return await sessionTokenRepository.Value.GetSessionInfoByToken(checkKeyParams.Key, siteInfo);
        }

        [HttpPost("/iam/api/v1/accounts/signin/google")]
        public Task SignInWithGoogleAccessTokenAsync(
            [FromServices] ICreyService<OAuthRepository> oAuthRepository,
            [FromBody] AccessTokenInput accessTokenInput)
        {
            return oAuthRepository.Value.GoogleSignIn(accessTokenInput.AccessToken, HttpContext.RequestAborted);
        }

        [HttpPost("/iam/api/v1/accounts/signin/facebook")]
        public Task SignInWithFacebookAccessTokenAsync(
            [FromServices] ICreyService<OAuthRepository> oAuthRepository,
            [FromBody] AccessTokenInput accessTokenInput)
        {
            return oAuthRepository.Value.FacebookSignIn(accessTokenInput.AccessToken, HttpContext.RequestAborted);
        }
    }
}