using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.UserAccount
{
    public class CreateModel : PageModelBase
    {
        public CreateModel(Bmcs.Data.BmcsContext context) : base(context)
        {

        }

        public IActionResult OnGet()
        {
            base.GetSelectList();

            return Page();
        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }
               
                var dbUserAccount = Context.UserAccounts.FirstOrDefault(r => r.UserAccountID == UserAccount.UserAccountID);

                if (dbUserAccount != null)
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.UserAccountID), "入力したユーザIDは既に使用されています。");

                    return Page();
                }


                var userAccount = new Models.UserAccount();

                if (await TryUpdateModelAsync<Models.UserAccount>(
                    userAccount
                  , "userAccount"
                  , s => s.UserAccountID
                  , s => s.UserAccountName
                  , s => s.Password
                  , s => s.ConfirmPassword
                  , s => s.TeamID
                  , s => s.TeamPassword
                  , s => s.EmailAddress))
                {
                    userAccount.DeleteFLG = false;
                    Context.UserAccounts.Add(userAccount);
                    base.SetEntryInfo(userAccount);

                    await Context.SaveChangesAsync();
                }
            }
            catch
            {
                throw;
            }

            return RedirectToPage("./Index");
        }
    }
}
