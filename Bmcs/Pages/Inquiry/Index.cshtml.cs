using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Inquiry
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public IList<Models.Inquiry> Inquiry { get;set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!base.IsLogin() || !base.IsAdmin())
            {
                return NotFound();
            }

            Inquiry = await Context.Inquirys.ToListAsync();

            return Page();
        }
    }
}
