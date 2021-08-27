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
    public class DeleteModel : PageModelBase<DeleteModel>
    {
        public DeleteModel(ILogger<DeleteModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                UserAccount = await Context.UserAccounts.FindAsync(id);

                if (UserAccount != null)
                {
                    UserAccount.DeleteFLG = true;
                    await Context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("./Index");
        }
    }
}
