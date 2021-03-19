using Crey.FeatureControl;
using Crey.Kernel;
using Crey.Kernel.ServiceDiscovery;
using Crey.Moderation;
using Crey.Web.Analytics;
using IAM.Clients;
using IAM.Contracts;
using IAM.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class RegistrationHandler
    {
        private readonly UserManager<ApplicationUser> userManager_;
        private readonly ICreyService<AccountRepository> accountRepo_;
        private readonly ICreyService<IFeatureGate> featureGate_;
        private readonly AnalyticsClient analyticsClient_;
        private readonly CreyModeratorClient moderation_;
        private readonly CreyRestClient creyRestClient_;
        private readonly ICreyService<EmailSender> emailSender_;

        public RegistrationHandler(
            UserManager<ApplicationUser> userManager,
            ICreyService<AccountRepository> accountRepo,
            ICreyService<IFeatureGate> featureGate,
            AnalyticsClient analyticsClient,
            CreyModeratorClient moderation,
            CreyRestClient creyRestClient,
            ICreyService<EmailSender> emailSender)
        {
            userManager_ = userManager;
            accountRepo_ = accountRepo;
            featureGate_ = featureGate;
            analyticsClient_ = analyticsClient;
            moderation_ = moderation;
            creyRestClient_ = creyRestClient;
            emailSender_ = emailSender;
        }

        public void BeforeRegistration(string creyTicket)
        {
            if (!string.IsNullOrEmpty(creyTicket))
            {
                analyticsClient_.SendReferralStartEvent(creyTicket);
            }
        }

        public async Task AfterRegistrationAsync(
            ApplicationUser applicationUser,
            DateTime utcDateOfBirth,
            IUrlHelper urlHelper,
            string creyTicket,
            int? avatarId)
        {
            if (!await featureGate_.Value.IsFeatureEnabledAsync(Features.LazyUserProfile))
            {
                await creyRestClient_.CreateUserProfileAsync(applicationUser.AccountId, applicationUser.UserName, applicationUser.NewsletterSubscribed, utcDateOfBirth);
            }

            if (avatarId.HasValue)
            {
                await creyRestClient_.SetUserAvatar(avatarId.Value);
            }

            await SendVerificationEmailAsync(applicationUser, urlHelper);

            if (!string.IsNullOrWhiteSpace(creyTicket))
            {
                analyticsClient_.SendReferralFinishEvent(creyTicket);
            }
            analyticsClient_.SendRegisterEvent();
        }

        public async Task<IdentityResult> RegisterUserAsync(
            ApplicationUser applicationUser,
            string password,
            DateTime utcDateOfBirth)
        {
            var identityErrors = new IEnumerable<IdentityError>[] {
                await ValidateUserName(applicationUser.UserName),
                ValidateAge(utcDateOfBirth) }
                .SelectMany(x => x)
                .ToList();

            if (identityErrors.Any())
            {
                return IdentityResult.Failed(identityErrors.ToArray());
            }

            //todo: check password strength ???

            var result = password == null
                ? await userManager_.CreateAsync(applicationUser)
                : await userManager_.CreateAsync(applicationUser, password);

            if (result.Succeeded)
            {
                await accountRepo_.Value.CreateUserDataAsync(applicationUser.AccountId, utcDateOfBirth);
            }
            return result;
        }

        public async Task SendVerificationEmailAsync(ApplicationUser applicationUser, IUrlHelper urlHelper)
        {
            var code = await userManager_.GenerateEmailConfirmationTokenAsync(applicationUser);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = urlHelper.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: "https");

            await emailSender_.Value.SendVerificationEmailAsync(applicationUser.Email, callbackUrl);
        }

        private static IEnumerable<IdentityError> ValidateAge(DateTime utcDateOfBirth)
        {
            const int minimumRegistrationAge = 13;
            bool isOldEnough = utcDateOfBirth.AddYears(minimumRegistrationAge) <= DateTime.UtcNow.Date;
            if (!isOldEnough)
            {
                yield return new IdentityError
                {
                    Code = string.Empty, //"DateOfBirthInvalid",
                    Description = $"User has to be at least {minimumRegistrationAge} years old in every timezone."
                };
            }
        }

        private async Task<IEnumerable<IdentityError>> ValidateUserName(string userName)
        {
            if (await featureGate_.Value.IsFeatureEnabledAsync(Features.ModerateUserName))
            {
                var mod = await moderation_.ScreenTextAsync(userName);
                if (mod.IsModerated)
                {
                    return new IdentityError[] {
                        new IdentityError {
                            Code = "Input.Username",
                            Description = $"Please watch your language, this username is not allowed by our policy",
                        }
                    };
                }
            }
            return Array.Empty<IdentityError>();
        }
    }
}