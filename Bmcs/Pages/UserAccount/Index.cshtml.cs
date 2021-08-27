using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.Extensions.Logging;

namespace Bmcs.Pages.UserAccount
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public IList<Models.UserAccount> UserAccount { get;set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!base.IsLogin() || !base.IsAdmin())
            {
                return NotFound();
            }

            UserAccount = await Context.UserAccounts
                .Include(u => u.Team)
                .OrderBy(r => r.TeamID)
                .ThenBy(r => r.UserAccountID)
                .ToListAsync();

            return Page();
        }
    }
}
