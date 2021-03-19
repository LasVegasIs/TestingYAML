using IAM.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Text;
using System.Threading.Tasks;

namespace IAM
{
    public static class Helpers
    {
        public static async Task<string> GetSharedKeyAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey)) 
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
            }

            return FormatKey(unformattedKey);
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length) {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length) {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }
    }
}
