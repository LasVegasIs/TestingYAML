using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Contracts.Authentication;
using Crey.Exceptions;
using Crey.Kernel;
using Crey.Kernel.Authentication;
using Crey.Kernel.ServiceDiscovery;
using IAM.Clients;
using Mandrill;
using Mandrill.Models;
using Mandrill.Requests.Messages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class EmailSender
    {
        private readonly MandrillApi mandrillApi_;
        private readonly IIDInfoAccessor idInfoAccessor_;
        private readonly ICreyService<AccountRepository> accountRepository_;
        private readonly CreyRestClient creyRestClient_;
        private readonly string internalInvoiceEmailAddress_;
        private readonly string feedbackEmailAddress_;

        public EmailSender(
            MandrillApi mandrillApi,
            IIDInfoAccessor idInfoAccessor,
            ICreyService<AccountRepository> accountRepository,
            CreyRestClient creyRestClient,
            IConfiguration configuration)
        {
            mandrillApi_ = mandrillApi;
            idInfoAccessor_ = idInfoAccessor;
            accountRepository_ = accountRepository;
            creyRestClient_ = creyRestClient;

            var isProduction = configuration.IsProductionSlot();

            internalInvoiceEmailAddress_ = configuration.GetValue<string>("internalInvoiceEmailAddress");
            if (string.IsNullOrEmpty(internalInvoiceEmailAddress_))
            {
                internalInvoiceEmailAddress_ = isProduction
                ? "invoice@creygames.com"
                : "bitglobetest.meli@gmail.com";
            }

            feedbackEmailAddress_ = configuration.GetValue<string>("feedbackEmailAddress");
            if (string.IsNullOrEmpty(feedbackEmailAddress_))
            {
                feedbackEmailAddress_ = isProduction
                ? "report@creygames.com"
                : "bitglobetest.meli@gmail.com";
            }
        }

        public async Task SendEmailTemplateAsync(string emailAddress, string template, Dictionary<string, string> templateParameters)
        {
            var emailMessage = new EmailMessage();
            emailMessage.To = new[] { new EmailAddress(emailAddress) };

            foreach (var item in templateParameters)
            {
                emailMessage.AddGlobalVariable(item.Key, item.Value);
            }

            var messageTemplateRequest = new SendMessageTemplateRequest(emailMessage, template);
            List<EmailResult> emailResults = await mandrillApi_.SendMessageTemplate(messageTemplateRequest);
            foreach (var item in emailResults)
            {
                if (item.Status == EmailResultStatus.Rejected)
                {
                    throw new ServerErrorException(
                        $"Sending email template {template} to {item.Email} with parameters: {string.Join(",", templateParameters)} failed with reason: {item.RejectReason}");
                }

                if (item.Status == EmailResultStatus.Invalid)
                {
                    throw new InvalidArgumentException(
                        $"Sending email template {template} to {item.Email} with parameters: {string.Join(",", templateParameters)} is invalid");
                }
            }
        }

        public Task SendVerificationEmailAsync(string email, string callbackUrl)
        {
            return SendEmailTemplateAsync(email, "crey-email-confirmation", new Dictionary<string, string> { { "URL", HtmlEncoder.Default.Encode(callbackUrl) } });
        }

        public Task SendPasswordResetEmailAsync(string email, string callbackUrl)
        {
            return SendEmailTemplateAsync(email, "crey-password-reset", new Dictionary<string, string> { { "URL", HtmlEncoder.Default.Encode(callbackUrl) } });
        }

        public async Task SendPurchaseConfirmationAsync(ConfirmationEmailParameters confirmationParameters, int accountId)
        {
            var applicationUser = await accountRepository_.Value.FindUserByAccountIdAsync(accountId);
            var userProfileInfo = await creyRestClient_.GetUserProfileAsync(accountId);

            var templateParameters = new Dictionary<string, string>()
            {
                { "USER_NAME", userProfileInfo.DisplayName },
                { "CLIENT_ID", accountId.ToString() },
                { "CLIENT_COUNTRY", confirmationParameters.OrderLocation },
                { "INVOICE_NO", confirmationParameters.InvoiceId.ToString() },
                { "PAYMENT_DATE", $"{confirmationParameters.PaymentDate.UtcDateTime.ToLongDateString()}" },
                { "CURRENCY", confirmationParameters.Currency },
                { "PAYMENT_METHOD", confirmationParameters.PaymentMethod },
                { "ITEM_DESCRIPTION", confirmationParameters.ItemDescription },
                { "VAT_PERCENTAGE", confirmationParameters.VatPercentage.ToString() },
                { "VAT_AMOUNT", confirmationParameters.VatAmount.ToString() },
                { "TOTAL_AMOUNT", confirmationParameters.TotalAmount.ToString() }
            };

            await SendEmailTemplateAsync(applicationUser.Email, "invoice", templateParameters);

            var internalInvoiceParameters = templateParameters;
            internalInvoiceParameters.Add("USER_EMAIL", applicationUser.Email);
            internalInvoiceParameters.Add("ITEM_PRICE", confirmationParameters.ItemPrice.ToString());

            await SendEmailTemplateAsync(internalInvoiceEmailAddress_, "internal-invoice", internalInvoiceParameters);
        }

        public async Task SendFeedback(FeedbackParameters feedbackParameters)
        {
            SessionInfo sessionInfo = idInfoAccessor_.GetSessionInfo();
            var applicationUser = await accountRepository_.Value.FindUserByAccountIdAsync(sessionInfo.AccountId);



            var emailMessage = new EmailMessage();
            emailMessage.FromName = applicationUser.UserName;
            emailMessage.FromEmail = "info@playcrey.com";
            emailMessage.To = new[] { new EmailAddress(feedbackEmailAddress_) };
            emailMessage.Text = $"User Email: {applicationUser.Email}\n\n{JavaScriptEncoder.Default.Encode(HtmlEncoder.Default.Encode(feedbackParameters.Feedback))}";

            var messageRequest = new SendMessageRequest(emailMessage);
            List<EmailResult> emailResults = await mandrillApi_.SendMessage(messageRequest);
            foreach (var item in emailResults)
            {
                if (item.Status == EmailResultStatus.Rejected)
                {
                    throw new ServerErrorException($"Sending email to {item.Email} failed with reason: {item.RejectReason}");
                }

                if (item.Status == EmailResultStatus.Invalid)
                {
                    throw new InvalidArgumentException($"Sending email to {item.Email} is invalid");
                }
            }
        }
    }
}
