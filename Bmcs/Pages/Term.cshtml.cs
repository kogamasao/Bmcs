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
    public class TermModel : PageModelBase<TermModel>
    {
        public TermModel(ILogger<TermModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public void OnGet()
        {
        }
    }
}
