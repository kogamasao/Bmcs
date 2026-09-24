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

        /// <summary>
        /// 未ログインで使用する画面のため、POSTを許可する
        /// </summary>
        public override bool AllowAnonymousPost
        {
            get { return true; }
        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.UserAccountCreate);

            //このページ自身を正規URLとする（canonicalがトップを指すと個別ページとして扱われない）
            IsIndex = true;

            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            //入力エラーでの再表示でもヘルプを表示できるようにする
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.UserAccountCreate);

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

                //メールアドレス必須チェック
                //※ID・パスワードを忘れた際の復旧に必要なため、新規登録では必須とする
                if (string.IsNullOrWhiteSpace(UserAccount.EmailAddress))
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.EmailAddress), "メールアドレスは必須です。ユーザIDやパスワードを忘れた際の復旧に使用します。");

                    return Page();
                }

                //チームパスワードチェック
                if (!string.IsNullOrEmpty(UserAccount.TeamID))
                { 
                    //※削除済みのチームには参加できない（以前は削除済みでも参加できた）
                    var dbTeam = Context.Teams.FirstOrDefault(r => r.TeamID == UserAccount.TeamID && !r.DeleteFLG);

                    //サンプルチームには参加できない（管理者は除く）
                    //※入力値ではなく DB の値で判定する。DBの照合順序は全角・半角も区別しないため、
                    //  入力値で判定すると「ＹＧ」（全角）で判定をすり抜けてサンプルチームに一致してしまう
                    if (!base.IsAdmin() && dbTeam != null && IsSampleTeamID(dbTeam.TeamID))
                    {
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.TeamID), "体験用のチームには参加できません。");

                        return Page();
                    }

                    if (dbTeam == null || dbTeam.TeamPassword != UserAccount.TeamPassword.NullToEmpty().ChangeHashValue())
                    {
                        //チームが存在しない場合も同じ文言とする（チームの有無を判別させないため）
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.TeamPassword), "チームIDまたはチームパスワードが間違っています。");

                        return Page();
                    }

                    //チームIDは DB の値で保存する
                    //※DBの照合順序は大文字小文字・末尾空白を区別しないため「yg」でも照合は通るが、
                    //  入力値のまま保存すると、セッションと試合等の TeamID の比較（完全一致）が合わず、自チームのデータを開けなくなる
                    UserAccount.TeamID = dbTeam.TeamID;
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
