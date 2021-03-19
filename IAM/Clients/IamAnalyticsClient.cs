
using Crey.Web.Analytics;
using IAM.Data;

namespace IAM.Clients
{
    public static class IamAnalyticsClient
    {
        public static void SendLogoutEvent(this AnalyticsClient analyticsClient)
        {
            analyticsClient.TrackEvent("Webfront", "SignOut");
        }

        public static void SendLoginEvent(this AnalyticsClient analyticsClient, CredentialType credentialType)
        {
            switch (credentialType)
            {
                case CredentialType.LoginPage:
                case CredentialType.MultiFactorAuthentication:
                    analyticsClient.TrackEvent("Webfront", "SignIn");
                    break;
                case CredentialType.Facebook:
                    analyticsClient.TrackEvent("Webfront", "FacebookSignIn");
                    break;
                case CredentialType.Google:
                    analyticsClient.TrackEvent("Webfront", "GoogleSignIn");
                    break;
            }
        }

        public static void SendRegisterEvent(this AnalyticsClient analyticsClient)
        {
            analyticsClient.TrackEvent("Webfront", "SignUp");
        }

        public static void SendReferralStartEvent(this AnalyticsClient analyticsClient, string creyTicket)
        {
            analyticsClient.TrackEvent("Webfront-Referrals", "RefSignUp", "Start", creyTicket);
        }

        public static void SendReferralFinishEvent(this AnalyticsClient analyticsClient, string creyTicket)
        {
            analyticsClient.TrackEvent("Webfront-Referrals", "RefSignUp", "Success", creyTicket);
        }
    }
}