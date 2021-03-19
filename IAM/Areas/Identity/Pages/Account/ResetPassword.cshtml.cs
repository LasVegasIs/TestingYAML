using System.Text;
using System.Threading.Tasks;
using IAM.Areas.Authentication.Models;
using IAM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IAM.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; }

        public string MobileMode { get; set; }        

        public IActionResult OnGet(string code = null, string mobile = null, string mobileDeepLink = null)
        {
            MobileMode = mobile;

            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            
            if (!string.IsNullOrWhiteSpace(mobileDeepLink))
            {
                return Redirect($"{mobileDeepLink}&code={code}");
            }
            
            Input = new ResetPasswordInputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };
            
            return Page();            
        }

        public async Task<IActionResult> OnPostAsync(string mobile = null)
        {
            MobileMode = mobile;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation", routeValues: new { mobile });
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation", routeValues: new { mobile });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
