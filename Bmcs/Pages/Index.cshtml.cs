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
using Bmcs.Enum;
using Bmcs.Function;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {
            
        }

        /// <summary>
        /// 未ログインで使用する画面のため、POSTを許可する
        /// </summary>
        public override bool AllowAnonymousPost
        {
            get { return true; }
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
            //アンケートの「あとで回答する」の保留も解除する（次回ログイン時に再表示するため）
            HttpContext.Session.SetString(SessionConstant.SurveySkip, string.Empty);
            //インデックス
            IsIndex = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                    //ユーザIDチェック
                    var dbUserAccount = await Context.UserAccounts.Include(u => u.Team).FirstOrDefaultAsync(r => r.UserAccountID == UserAccount.UserAccountID
                                                                                    && r.DeleteFLG == false);

                    if (dbUserAccount == null
                        || dbUserAccount.UserAccountID != UserAccount.UserAccountID
                        || dbUserAccount.Password != UserAccount.Password.ChangeHashValue())
                    {
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.UserAccountID), "入力したユーザID、またはパスワードが間違っています。お忘れの場合は「パスワードをお忘れの場合」「ユーザIDをお忘れの場合」からお手続きください。");

                        return Page();
                    }
                    else
                    {
                        //最終ログイン日時
                        //※長期間使用されていないデータを判別するために保持する。
                        //  チーム側にも持たせ、所属ユーザを1人ずつ調べずに抽出できるようにする。
                        //※SaveChanges を使うと rowversion による排他制御が働き、
                        //  同じチームのユーザが同時にログインした際に競合してエラーログが出る。
                        //  記録のためだけの更新なので、UPDATE文で直接更新する。
                        try
                        {
                            var loginDatetime = DateTime.Now;

                            await Context.Database.ExecuteSqlRawAsync(
                                "UPDATE dbo.UserAccount SET LastLoginDatetime = {0} WHERE UserAccountID = {1}",
                                loginDatetime, dbUserAccount.UserAccountID);

                            if (!string.IsNullOrEmpty(dbUserAccount.TeamID))
                            {
                                await Context.Database.ExecuteSqlRawAsync(
                                    "UPDATE dbo.Team SET LastLoginDatetime = {0} WHERE TeamID = {1}",
                                    loginDatetime, dbUserAccount.TeamID);
                            }
                        }
                        catch (Exception ex)
                        {
                            //ログインを妨げないよう、記録に失敗しても処理は続行する
                            Logger.LogError(ex, "最終ログイン日時の更新に失敗しました。");
                        }

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

                //未回答のアンケートがある場合は回答ページへ誘導する
                //※ログイン前にアクセスしていたページがある場合は、そちらを優先する
                if (!base.IsAdmin()
                    && string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.UrlAfterLogin))
                    && string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.SurveySkip))
                    && (await base.GetUnansweredSurveyAsync()) != null)
                {
                    return RedirectToPage("./Survey/Answer");
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
