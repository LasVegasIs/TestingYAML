using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using IAM.Data;
using Crey.Kernel.ServiceDiscovery;
using IAM.Areas.Authentication;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICreyService<EmailSender> _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, ICreyService<EmailSender> emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string MobileMode { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet(string mobile = null)
        {
            MobileMode = mobile;
        }

        public async Task<IActionResult> OnPostAsync(string mobile = null)
        {
            MobileMode = mobile;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation", routeValues: new { mobile });
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, mobile },
                    protocol: "https"); // TODO: change back to Request.Scheme once I got to finish the URL rewrite fix, Ferenc Tükör, 2020.10.12. 14:57
                await _emailSender.Value.SendPasswordResetEmailAsync(Input.Email, callbackUrl);

                return RedirectToPage("./ForgotPasswordConfirmation", routeValues: new { mobile });
            }

            return Page();
        }
    }
}
