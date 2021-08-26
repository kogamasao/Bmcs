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
    public class DetailsModel : PageModelBase
    {
        public DetailsModel(BmcsContext context) : base(context)
        {

        }

        public Models.UserAccount UserAccount { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null || !base.IsLogin() || !base.IsAdmin())
            {
                return NotFound();
            }
            UserAccount = await Context.UserAccounts
                      .Include(u => u.Team).FirstOrDefaultAsync(m => m.UserAccountID == id);

            if (UserAccount == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
