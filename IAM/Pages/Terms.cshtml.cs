using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Crey.Web;
using IAM.Clients;

namespace IAM.Pages
{
    public class TermsModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        private readonly KenticoClient _kenticoClient;

        public string TermsAndConditions { get; private set; }

        public TermsModel(ILogger<PrivacyModel> logger, KenticoClient kenticoClient)
        {
            _logger = logger;
            _kenticoClient = kenticoClient;
        }

        public async Task<IActionResult> OnGet()
        {
            TermsAndConditions = await _kenticoClient.GetTermsAndConditions();

            return Page();
        }
    }
}