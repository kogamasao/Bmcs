using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Constans;
using Bmcs.Function;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {
            
        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        [BindProperty]
        public string UrlAfterLogin { get; set; }

        public void OnGet()
        {
            //ログイン情報クリア
            HttpContext.Session.SetString(SessionConstant.UserAccountID, string.Empty);
            HttpContext.Session.SetString(SessionConstant.TeamID, string.Empty);
            HttpContext.Session.SetString(SessionConstant.AdminFLG, string.Empty);
            //インデックス
            IsIndex = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                //ユーザIDチェック
                var dbUserAccount = await Context.UserAccounts.Include(u => u.Team).FirstOrDefaultAsync(r => r.UserAccountID == UserAccount.UserAccountID
                                                                                && r.Password == UserAccount.Password
                                                                                && r.DeleteFLG == false);

                if (dbUserAccount == null
                    || dbUserAccount.UserAccountID != UserAccount.UserAccountID
                    || dbUserAccount.Password != UserAccount.Password)
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.UserAccountID), "入力したユーザID、またはパスワードが間違っています。パスワードをお忘れの場合はお問い合わせをお願いします。");

                    return Page();
                }
                else
                {
                    //ログイン情報セット
                    HttpContext.Session.SetString(SessionConstant.UserAccountID, dbUserAccount.UserAccountID.NullToEmpty());
                    HttpContext.Session.SetString(SessionConstant.TeamID, dbUserAccount.TeamID.NullToEmpty());

                    //管理者権限
                    if(dbUserAccount.UserAccountID == SystemConstant.AdminUserAccountID)
                    { 
                        HttpContext.Session.SetString(SessionConstant.AdminFLG, "1");
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            //管理者でない、かつチーム未登録の場合
            if (!base.IsAdmin() && string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.TeamID)))
            {
                return RedirectToPage("./Team/Create");
            }
            else
            { 
                if(string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.UrlAfterLogin)))
                {
                    return RedirectToPage("./Top/Index");
                }
                else
                { 
                    return Redirect(HttpContext.Session.GetString(SessionConstant.UrlAfterLogin));
                }
            }
        }
    }
}
