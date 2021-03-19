using Crey.Contracts;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web.Analytics;
using Crey.Web.Service2Service;
using IAM.Clients;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class PersistentTokenParams
    {
        [Required]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Token length should be between 10 and 100.")]
        public string Token { get; set; }
    }


    [ApiController]
    [EnableCors]
    public class PersistentTokenController : ControllerBase
    {
        ICreyService<PersistentTokenRepository> persistentTokenRepository_;
        private readonly CreySignInManager signInManager_;
        private readonly IIDInfoAccessor idInfo_;
        private readonly ILogger<PersistentTokenController> logger_;

        public PersistentTokenController(
            ICreyService<PersistentTokenRepository> persistentTokenRepository,
            CreySignInManager signInManager,
            IIDInfoAccessor idInfo,
            ILogger<PersistentTokenController> logger)
        {
            persistentTokenRepository_ = persistentTokenRepository;
            signInManager_ = signInManager;
            idInfo_ = idInfo;
            logger_ = logger;
        }

        [HttpPost("/iam/api/v1/signin/persistenttoken")]
        public async Task SignInWithPersistentToken(PersistentTokenParams developerTokenParams)
        {
            int accountId = await persistentTokenRepository_.Value.FindUserByPersistentToken(developerTokenParams.Token);
            await signInManager_.SignInAsync(accountId, true, CredentialType.MultiAccessKey.ToString());
        }

        [HttpPost("/iam/api/v1/persistenttoken")]
        [Authorize]
        public Task<string> CreatePersistentToken()
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return persistentTokenRepository_.Value.CreatePersistentToken(sessionInfo.AccountId);
        }


        [HttpDelete("/iam/api/v1/persistenttoken")]
        [Authorize]
        public Task RevokePersistentToken(PersistentTokenParams developerTokenParams)
        {
            var sessionInfo = idInfo_.GetSessionInfo();
            return persistentTokenRepository_.Value.RevokePersistentToken(sessionInfo.AccountId, developerTokenParams.Token);
        }
    }
}