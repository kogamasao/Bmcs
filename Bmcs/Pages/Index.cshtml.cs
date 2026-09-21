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
                    && await HasUnansweredSurveyAsync())
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

        /// <summary>
        /// 未回答のアンケートがあるかどうか
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HasUnansweredSurveyAsync()
        {
            //このセッションで「あとで回答する」を選択済みの場合は誘導しない
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.SurveySkip)))
            {
                return false;
            }

            try
            {
                var userAccountID = HttpContext.Session.GetString(SessionConstant.UserAccountID);

                var surveyList = await Context.Surveys
                    .Where(r => r.DeleteFLG == false
                             && r.StatusClass == SurveyStatusClass.Open)
                    .ToListAsync();

                if (!surveyList.Any(r => r.IsOpen()))
                {
                    return false;
                }

                var answeredSurveyIDList = await Context.SurveyAnswers
                    .Where(r => r.UserAccountID == userAccountID)
                    .Select(r => r.SurveyID)
                    .ToListAsync();

                return surveyList.Any(r => r.IsOpen() && !answeredSurveyIDList.Contains(r.SurveyID));
            }
            catch (Exception ex)
            {
                //アンケート用テーブルが未作成の場合などでも、ログインは通す
                Logger.LogError(ex, "アンケートの未回答判定に失敗しました。");

                return false;
            }
        }
    }
}
