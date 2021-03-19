using Crey.Kernel.Authentication;
using Crey.Web.Service2Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    /// <summary>
    /// Consider it as part of initial user activity (like tutorial progress), so retain it here until new usage appears or it will be `one button to create service`.
    /// Definitely this is not `iam` either.
    /// </summary>
    [EnableCors]
    [ApiController]
    public class SignatureFlowController : ControllerBase
    {
        /// <summary>
        /// Sign the data and return same data + signature.
        /// It is up to frontend to glue proper reference link from this.
        /// </summary>
        [HttpPost("/iam/api/signature/callback/sign")]
        [ServerToServer(Service2ServicePolicy.Internal)]
        [Authorize]
        [Obsolete("Use /iam/s2s/v1/signature/callback/sign/{accountId}")]
        public SignedPayload SignCallback([FromBody] Callback data, [FromServices] SignatureFlowService signer)
        {
            // TODO: make sure all exceptions are properly mapped to HTTP codes

            // ensures we do not create callback to random site, may be read that from settings
            // will need to add other pattern in k8s with internal s2s used
            if (!(data.callback.Host.EndsWith("playcrey.com") || data.callback.Host.EndsWith("localhost")))
                throw new ValidationException($"Invalid {nameof(data.callback)}. Must be DNS based and point to playcrey.com or localhost");
            var issuer = User.IntoSessionInfo();
            return signer.Sign(data, issuer.AccountId, DateTimeOffset.UtcNow);
        }

        [HttpPost("/iam/s2s/v1/signature/callback/sign/{accountId}")]
        [ServerToServer(Service2ServicePolicy.Internal)]
        public SignedPayload SignCallback([FromBody] Callback data, [FromServices] SignatureFlowService signer, int accountId)
        {
            // TODO: make sure all exceptions are properly mapped to HTTP codes

            // ensures we do not create callback to random site, may be read that from settings
            // will need to add other pattern in k8s with internal s2s used
            if (!(data.callback.Host.EndsWith("playcrey.com") || data.callback.Host.EndsWith("localhost")))
                throw new ValidationException($"Invalid {nameof(data.callback)}. Must be DNS based and point to playcrey.com or localhost");
            return signer.Sign(data, accountId, DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Unsign data (given signature + original data). Can be used directly for testing, but probably its functional will be invoked as part of other processes.
        /// E.g. registration POST with signed data, etc.
        /// </summary>
        [HttpPost("/iam/api/signature/callback/unsign")]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser)]
        public Task<UnsignedPayload> Unsign(
            [FromBody]SignedParams creyticket,
            [FromServices]SignatureFlowService signer)
        {            
            return signer.VerifyUnsign(creyticket.creyticket);     
        }

        /// <summary>
        /// Unsign data (given signature + original data). Can be used directly for testing, but probably its functional will be invoked as part of other processes.
        /// E.g. registration POST with signed data, etc.
        /// </summary>
        [HttpPost("/iam/api/signature/callback/execute")]
        [AuthenticateStrict]
        [Authorize(Policy = CreyAuthorizationDefaults.CreyUser)]
        public async Task<HttpStatusCode> UnsignCallback(
            [FromBody]SignedParams creyticket, 
            [FromServices]SignatureFlowService signer)
        {
            var callback = await signer.VerifyUnsign(creyticket.creyticket);
            return await signer.ExecuteCallback(callback.callback, new PostData {issuer = callback.issuer, payload = callback.payload, timestamp = callback.timestamp, version = callback.version });
        }
    }
}