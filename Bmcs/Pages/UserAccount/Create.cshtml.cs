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
using Bmcs.Function;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.UserAccount
{
    public class CreateModel : PageModelBase
    {
        public CreateModel(BmcsContext context) : base(context)
        {

        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        public IActionResult OnGet()
        {
            base.GetSelectList();

            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                base.GetSelectList();

                if (!ModelState.IsValid)
                {
                    return Page();
                }
               
                //ユーザIDチェック
                var dbUserAccount = Context.UserAccounts.FirstOrDefault(r => r.UserAccountID == UserAccount.UserAccountID);

                if (dbUserAccount != null)
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.UserAccountID), "入力したユーザIDは既に使用されています。");

                    return Page();
                }

                //チームパスワードチェック
                if(!string.IsNullOrEmpty(UserAccount.TeamID))
                { 
                    var dbTeam = Context.Teams.FirstOrDefault(r => r.TeamID == UserAccount.TeamID
                                                            && r.TeamPassword == UserAccount.TeamPassword.NullToEmpty());

                    if (dbTeam == null)
                    {
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.TeamPassword), "パスワードが間違っています。");

                        return Page();
                    }
                }

                //データ作成
                var userAccount = new Models.UserAccount();

                if (await TryUpdateModelAsync<Models.UserAccount>(
                    userAccount
                  , nameof(Models.UserAccount)
                  , s => s.UserAccountID
                  , s => s.UserAccountName
                  , s => s.Password
                  , s => s.ConfirmPassword
                  , s => s.EmailAddress
                  , s => s.TeamID
                  , s => s.TeamPassword
                  ))
                {
                    userAccount.DeleteFLG = false;
                    Context.UserAccounts.Add(userAccount);
                    base.SetEntryInfo(userAccount);

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
