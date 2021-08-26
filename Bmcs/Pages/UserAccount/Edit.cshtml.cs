using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Function;

namespace Bmcs.Pages.UserAccount
{
    public class EditModel : PageModelBase
    {
        public EditModel(BmcsContext context) : base(context)
        {

        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (!base.IsLogin())
            {
                return NotFound();
            }

            if (id == null)
            {
                id = HttpContext.Session.GetString(SessionConstant.UserAccountID);
            }
            else
            {
                if (!base.IsAdmin())
                {
                    return NotFound();
                }
            }

            UserAccount = await Context.UserAccounts
                .Include(u => u.Team).FirstOrDefaultAsync(m => m.UserAccountID == id);

            if (UserAccount == null)
            {
                return NotFound();
            }

            base.GetSelectList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            base.GetSelectList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                //チームパスワードチェック
                if (!string.IsNullOrEmpty(UserAccount.TeamID))
                {
                    var dbTeam = Context.Teams.FirstOrDefault(r => r.TeamID == UserAccount.TeamID
                                                            && r.TeamPassword == UserAccount.TeamPassword.NullToEmpty());

                    if (dbTeam == null)
                    {
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.TeamPassword), "パスワードが間違っています。");

                        return Page();
                    }
                }

                //データ更新
                var userAccount = await Context.UserAccounts.FindAsync(UserAccount.UserAccountID);

                if (userAccount == null)
                {
                    return NotFound();
                }

                //POST値セット
                this.TryUpdateModel(userAccount);
                //更新情報セット
                base.SetUpdateInfo(userAccount);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (base.IsAdmin())
            {
                return RedirectToPage("./Index");
            }
            else
            {
                return RedirectToPage("../Top/Index");
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="userAccount"></param>
        private void TryUpdateModel(Models.UserAccount userAccount)
        {
            userAccount.UserAccountName = UserAccount.UserAccountName;
            userAccount.Password = UserAccount.Password;
            userAccount.EmailAddress = UserAccount.EmailAddress;
            userAccount.TeamID = UserAccount.TeamID;
        }
    }
}
