using System;
using IAM.Data;

namespace IAM.Areas.Authentication
{
    public static class CredentialTypeExtensions
    {
        public static CredentialType ToCredentialType(this string authenticationMethod)
        {
            if (authenticationMethod == CredentialType.Facebook.ToString()) return CredentialType.Facebook;
            if (authenticationMethod == CredentialType.Google.ToString()) return CredentialType.Google;
            if (authenticationMethod == CredentialType.LoginPage.ToString()) return CredentialType.LoginPage;
            if (authenticationMethod == CredentialType.MultiFactorAuthentication.ToString()) return CredentialType.MultiFactorAuthentication;
            if (authenticationMethod == CredentialType.RefreshKey.ToString()) return CredentialType.RefreshKey;
            if (authenticationMethod == CredentialType.SingleAccessKey.ToString()) return CredentialType.SingleAccessKey;
            if (authenticationMethod == CredentialType.UserPassword.ToString()) return CredentialType.UserPassword;
            if (authenticationMethod == CredentialType.MultiAccessKey.ToString()) return CredentialType.MultiAccessKey;
            if (authenticationMethod == CredentialType.Impersonation.ToString()) return CredentialType.Impersonation;

            if (authenticationMethod == "pwd") return CredentialType.LoginPage;
            if (authenticationMethod == "mfa") return CredentialType.MultiFactorAuthentication;

            throw new Exception($"Unknown credential type {authenticationMethod}");
        }
    }
}
