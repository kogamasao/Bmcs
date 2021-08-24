using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Models;
using Bmcs.Constans;

namespace Bmcs.Pages
{
    public class IndexModel : PageModelBase
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, Bmcs.Data.BmcsContext context) : base(context)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            HttpContext.Session.SetString(SessionConstant.UserID, "value");
        }
    }
}
