using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.UserAccount
{
    public class IndexModel : PageModelBase
    {
        public IndexModel(BmcsContext context) : base(context)
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
                .Include(u => u.Team).ToListAsync();

            return Page();
        }
    }
}
