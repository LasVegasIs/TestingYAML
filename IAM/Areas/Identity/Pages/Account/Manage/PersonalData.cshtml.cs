using System.Threading.Tasks;
using Crey.FeatureControl;
using Crey.Kernel.ServiceDiscovery;
using IAM.Areas.Authentication;
using IAM.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IAM.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;
        private readonly ICreyService<IFeatureGate> _featureGate;
        public bool IsPersonalDataManagementEnabled { get; set; }

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<PersonalDataModel> logger,
            ICreyService<IFeatureGate> featureGate)
        {
            _userManager = userManager;
            _logger = logger;
            _featureGate = featureGate;
        }

        public async Task<IActionResult> OnGet()
        {
            IsPersonalDataManagementEnabled = await _featureGate.Value.IsFeatureEnabledAsync(FeatureGatesInUse.ManagePersonalData);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }
    }
}