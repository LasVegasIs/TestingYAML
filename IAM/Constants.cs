namespace IAM
{
    internal static class Constants
    {
        internal const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        //Messages
        internal const string UserIsLockedOutMessage = "User account is locked out.";
        internal const string UserIsLogedInMessage = "User account is locked out.";
        internal const string ModelIsNotValidMessage = "Model is not valid.";
    }
}
