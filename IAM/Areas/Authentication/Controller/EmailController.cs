using Crey.Contracts.Authentication;
using Crey.Kernel;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using Crey.Web.Service2Service;
using IAM.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class ConfirmationEmailParameters
    {
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string OrderLocation { get; set; }
        public int InvoiceId { get; set; }
        public string Currency { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public string ItemDescription { get; set; }
    }

    [EnableCors]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ICreyService<EmailSender> emailSender_;

        public EmailController(ICreyService<EmailSender> emailSender)
        {
            emailSender_ = emailSender;
        }

        [HttpPost("/iam/s2s/v1/email/purchase/confirmation")]
        [Authorize]
        [ServerToServer]
        [Obsolete("Use /iam/s2s/v1/email/purchase/confirmation/{accountId}")]
        public async Task<ActionResult> SendPurchaseConfirmation(ConfirmationEmailParameters confirmationParameters)
        {
            await emailSender_.Value.SendPurchaseConfirmationAsync(confirmationParameters, User.IntoSessionInfo().AccountId);
            return Ok();
        }

        [HttpPost("/iam/s2s/v1/email/purchase/confirmation/{accountId}")]
        [ServerToServer]
        public async Task<ActionResult> SendPurchaseConfirmation(ConfirmationEmailParameters confirmationParameters, int accountId)
        {
            await emailSender_.Value.SendPurchaseConfirmationAsync(confirmationParameters, accountId);
            return Ok();
        }

        [HttpPost("/iam/api/v1/email/feedback")]
        [Authorize]
        public async Task<ActionResult> SendFeedback([FromBody] FeedbackParameters feedbackParameters)
        {
            await emailSender_.Value.SendFeedback(feedbackParameters);
            return Ok();
        }

        #region Move into AccountController

        [HttpGet("/iam/api/v1/email/confirmed")]
        [Authorize]
        // Consider deprecate in favor of /iam/api/v1/email/owned/status
        public Task<bool> GetEmailConfirmed(
            [FromServices] ICreyService<AccountRepository> accounts,
            [FromServices] IIDInfoAccessor idInfo
            )
        {
            var sessionInfo = idInfo.GetSessionInfo();
            return accounts.Value.GetEmailConfirmedStatusAsync(sessionInfo.AccountId);
        }

        public class EmailStatus
        {
            public bool Confirmed { get; set; }
            public bool Newsletter { get; set; }
        }

        [HttpGet("/iam/api/v1/email/owned/status")]
        [Authorize]
        public async Task<EmailStatus> GetEmailStatus(
            [FromServices] ICreyService<AccountRepository> accounts,
            [FromServices] IIDInfoAccessor idInfo,
            [FromServices] CreyRestClient creyRestClient)
        {
            var sessionInfo = idInfo.GetSessionInfo();
            var status = await accounts.Value.GetEmailStatusAsync(sessionInfo.AccountId);
            return status;
        }

        public class PatchEmailStatus
        {
            public bool? Newsletter { get; set; }
            //public bool? Notifications { get; set; }
        }

        [HttpPatch("/iam/api/v1/email/owned/status")]
        [Authorize]
        public async Task<ActionResult> SetEmailStatusAsync(PatchEmailStatus param,
            [FromServices] ICreyService<AccountRepository> accounts,
            [FromServices] IIDInfoAccessor idInfo,
            [FromServices] CreyRestClient creyRestClient)
        {
            var sessionInfo = idInfo.GetSessionInfo();            
            await accounts.Value.SetEmailStatusAsync(sessionInfo.AccountId, param);
            return Ok();
        }

        #endregion
    }
}