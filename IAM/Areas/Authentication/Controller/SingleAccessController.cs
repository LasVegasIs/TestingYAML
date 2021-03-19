using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Web;

namespace IAM.Areas.Authentication
{
    [ApiController]
    [EnableCors]
    public class SingleAccessController : ControllerBase
    {
        private readonly IIDInfoAccessor idInfo_;
        private readonly ICreyService<SingleAccessKeyRepository> singleAccesKey_;
        private readonly CreySignInManager signInManager_;

        public SingleAccessController(IIDInfoAccessor idInfo,
            ICreyService<SingleAccessKeyRepository> singleAccesKey,
            CreySignInManager signInManager)
        {
            idInfo_ = idInfo;
            singleAccesKey_ = singleAccesKey;
            signInManager_ = signInManager;
        }

        [HttpPost("/api/v1/satoken/request")]
        [Authorize]
        public async Task<ActionResult<string>> CreateToken()
        {
            var info = idInfo_.GetSessionInfo();
            if (!info.IsUser)
            {
                throw new Crey.Exceptions.AccessDeniedException($"Login required");
            }

            var token = await singleAccesKey_.Value.CreateKey(info.AccountId);
            return HttpUtility.UrlEncode(token);
        }

        [HttpPost("/api/v1/satoken/use")]
        public async Task<ActionResult> UseKey(string token)
        {
            var accountId = await singleAccesKey_.Value.FindUserByKey(token);
            await signInManager_.SignInAsync(accountId, true, CredentialType.SingleAccessKey.ToString());
            return Ok();
        }
    }
}