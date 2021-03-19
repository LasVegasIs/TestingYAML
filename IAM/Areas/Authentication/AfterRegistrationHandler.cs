using Crey.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class AfterRegistrationHandler
    {
        private readonly ILogger<RegistrationHandler> logger_;
        private readonly SignatureFlowService signatureFlowService_;

        public AfterRegistrationHandler(
            ILogger<RegistrationHandler> logger,
            SignatureFlowService signatureFlowService)
        {
            logger_ = logger;
            signatureFlowService_ = signatureFlowService;
        }

        public async Task AfterRegistration(int accountId, string creyTicket)
        {
            var referral = await Try(async () => {
                if (!string.IsNullOrWhiteSpace(creyTicket)) {
                    await StartExecutingReferralFlow(accountId, creyTicket);
                }
            });

            Throw(referral);
        }

        private static void Throw(params Exception[] exceptions) {
            var nonNull = exceptions.Where(x => x != null);
            if (nonNull.Any())
                throw new AggregateException(nonNull).Flatten();
        }

        #nullable enable
        private static async Task<Exception?> Try(Func<Task> act) {
            try {
                await act();
            }
            catch (Exception ex) {
                return ex;
            }

            return null;
        }
        #nullable restore

        private async Task StartExecutingReferralFlow(int accountId, string creyticket) {
            logger_.LogInformation(EventIds.CreyTicketApplied, "{AccountId} used {CreyTicket} during registration", accountId, creyticket);
            try {
                var callback = await signatureFlowService_.VerifyUnsign(creyticket);
                logger_.LogInformation(
                    "{AccountId} verified ticket issued by {CreyTicketIssuer} ",
                    accountId, callback.issuer);
                var httpResult = await signatureFlowService_.ExecuteCallback(
                        callback.callback,
                        new PostData {
                            issuer = callback.issuer,
                            payload = callback.payload,
                            timestamp = callback.timestamp,
                            version = callback.version,
                        }
                        );
                if (httpResult != System.Net.HttpStatusCode.OK) {
                    throw new HttpStatusErrorException(httpResult, callback.callback.ToString());
                }
            }
            catch (Exception ex) {
                logger_.LogCritical(ex, "Singed ticket flow failed for {AccountId} with {CreyTicket}", accountId, creyticket);
                throw;
            }
        }
    }
}