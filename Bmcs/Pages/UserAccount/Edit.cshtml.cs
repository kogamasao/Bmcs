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

        /// <summary>
        /// 入力エラーでの再表示に必要なデータを取り直す
        /// ※以前は取り直しておらず、ヘルプと現在の所属チームが表示されなかった
        /// </summary>
        private async Task<IActionResult> ShowErrorAsync()
        {
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.UserAccountEdit);

            var dbUserAccount = await Context.UserAccounts.Include(r => r.Team).AsNoTracking()
                                    .FirstOrDefaultAsync(r => r.UserAccountID == UserAccount.UserAccountID);

            UserAccount.Team = dbUserAccount?.Team;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //体験用ユーザは変更できない（誰でもログインできるため、変更されると体験が全員に対して動かなくなる）
            if (base.IsSampleUser())
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return await ShowErrorAsync();
            }

            try
            {
                //データ更新
                var userAccount = await Context.UserAccounts.FindAsync(UserAccount.UserAccountID);

                if (userAccount == null)
                {
                    return NotFound();
                }

                //本人以外のユーザは更新できない（管理者は除く）
                if (!base.IsAdmin()
                    && userAccount.UserAccountID != HttpContext.Session.GetString(SessionConstant.UserAccountID))
                {
                    return NotFound();
                }

                //メールアドレスの削除チェック
                //※既存ユーザ（未登録のまま利用中の方）に影響を与えないため、
                //　登録済みのメールアドレスを空に戻すことのみ禁止する
                if (!string.IsNullOrWhiteSpace(userAccount.EmailAddress)
                    && string.IsNullOrWhiteSpace(UserAccount.EmailAddress))
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.EmailAddress), "メールアドレスは削除できません。ユーザIDやパスワードを忘れた際の復旧に使用します。");

                    return await ShowErrorAsync();
                }

                //チームパスワードチェック
                if (userAccount.TeamID != UserAccount.TeamID
                    && !string.IsNullOrEmpty(UserAccount.TeamID))
                {
                    //※削除済みのチームには参加できない（以前は削除済みでも参加できた）。
                    //  チームIDの有無を判別できないよう、存在しない場合もパスワード誤りと同じ文言にする
                    var dbTeam = Context.Teams.FirstOrDefault(r => r.TeamID == UserAccount.TeamID && !r.DeleteFLG);

                    if (dbTeam == null || dbTeam.TeamPassword != UserAccount.TeamPassword.NullToEmpty().ChangeHashValue())
                    {
                        ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.TeamPassword), "チームIDまたはチームパスワードが間違っています。");

                        return await ShowErrorAsync();
                    }
                }

                //POST値セット
                this.TryUpdateModel(userAccount);
                //更新情報セット
                base.SetUpdateInfo(userAccount);

                await Context.SaveChangesAsync();

                //本人の所属チームを変えた場合は、セッションのチームも切り替える
                //※以前は切り替えておらず、チームを抜けてもログインし直すまで元のチームのデータを編集できた
                if (userAccount.UserAccountID == HttpContext.Session.GetString(SessionConstant.UserAccountID))
                {
                    HttpContext.Session.SetString(SessionConstant.TeamID, userAccount.TeamID.NullToEmpty());
                }
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
