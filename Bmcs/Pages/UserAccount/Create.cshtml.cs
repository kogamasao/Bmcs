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
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Microsoft.Extensions.Logging;
using Bmcs.Enum;

namespace Bmcs.Pages.UserAccount
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.UserAccountCreate);

            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }
               
                //ユーザIDチェック
                var dbUserAccount = Context.UserAccounts.FirstOrDefault(r => r.UserAccountID == UserAccount.UserAccountID);

                if (dbUserAccount != null || (dbUserAccount != null && dbUserAccount.UserAccountID == UserAccount.UserAccountID))
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.UserAccountID), "入力したユーザIDは既に使用されています。");

                    return Page();
                }

                //パスワード必須チェック
                if (string.IsNullOrEmpty(UserAccount.Password))
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.Password), "パスワードは必須です。");

                    return Page();
                }

                //確認パスワード必須チェック
                if (string.IsNullOrEmpty(UserAccount.ConfirmPassword))
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.ConfirmPassword), "確認用パスワードは必須です。");

                    return Page();
                }

                //チームパスワードチェック
                if (!string.IsNullOrEmpty(UserAccount.TeamID))
                { 
                    var dbTeam = Context.Teams.FirstOrDefault(r => r.TeamID == UserAccount.TeamID);

                    if (dbTeam == null || dbTeam.TeamPassword != UserAccount.TeamPassword.NullToEmpty().ChangeHashValue())
                    {
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.TeamPassword), "パスワードが間違っています。");

                        return Page();
                    }
                }

                //データ作成
                var userAccount = new Models.UserAccount();

                //POST値セット
                this.TryUpdateModel(userAccount);
                //エントリ情報セット
                base.SetEntryInfo(userAccount);

                Context.UserAccounts.Add(userAccount);
                await Context.SaveChangesAsync();

                if(!base.IsAdmin())
                { 
                    //ログイン情報セット
                    HttpContext.Session.SetString(SessionConstant.UserAccountID, userAccount.UserAccountID.NullToEmpty());
                    HttpContext.Session.SetString(SessionConstant.TeamID, userAccount.TeamID.NullToEmpty());
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            //管理者でない、かつチーム未登録の場合
            if (!base.IsAdmin() && string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.TeamID)))
            {
                return RedirectToPage("/Team/Create");
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
            userAccount.UserAccountID = UserAccount.UserAccountID;
            userAccount.UserAccountName = UserAccount.UserAccountName;
            userAccount.Password = UserAccount.Password.ChangeHashValue();
            userAccount.EmailAddress = UserAccount.EmailAddress;
            userAccount.TeamID = UserAccount.TeamID;
            userAccount.DeleteFLG = false;
        }
    }
}
