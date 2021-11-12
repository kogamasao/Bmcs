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
using Microsoft.Extensions.Logging;
using Bmcs.Enum;

namespace Bmcs.Pages.UserAccount
{
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (!base.IsLogin())
            {
                return ReLogin();
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

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.UserAccountEdit);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                //データ更新
                var userAccount = await Context.UserAccounts.FindAsync(UserAccount.UserAccountID);

                if (userAccount == null)
                {
                    return NotFound();
                }

                //チームパスワードチェック
                if (userAccount.TeamID != UserAccount.TeamID
                    && !string.IsNullOrEmpty(UserAccount.TeamID))
                {
                    var dbTeam = Context.Teams.FirstOrDefault(r => r.TeamID == UserAccount.TeamID);

                    if (dbTeam == null || dbTeam.TeamPassword != UserAccount.TeamPassword.NullToEmpty().ChangeHashValue())
                    {
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.TeamPassword), "パスワードが間違っています。");

                        return Page();
                    }
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
                return RedirectToPage("/Top/Index");
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="userAccount"></param>
        private void TryUpdateModel(Models.UserAccount userAccount)
        {
            userAccount.UserAccountName = UserAccount.UserAccountName;

            //パスワード変更時のみ
            if(!string.IsNullOrEmpty(UserAccount.Password))
            { 
                userAccount.Password = UserAccount.Password.ChangeHashValue();
            }
            
            userAccount.EmailAddress = UserAccount.EmailAddress;
            userAccount.TeamID = UserAccount.TeamID;
        }
    }
}
