using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Models;
using Bmcs.Data;

namespace Bmcs.Pages
{
    public class PrivacyModel : PageModelBase
    {
        private readonly ILogger<IndexModel> _logger;

        public PrivacyModel(ILogger<IndexModel> logger, BmcsContext context) : base(context)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
