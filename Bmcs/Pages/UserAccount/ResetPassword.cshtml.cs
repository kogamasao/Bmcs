using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bmcs.Data;
using Bmcs.Enum;
using Bmcs.Function;
using Bmcs.Models;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.UserAccount
{
    /// <summary>
    /// パスワード再設定（メールのURLからの再設定）
    /// </summary>
    public class ResetPasswordModel : PageModelBase<ResetPasswordModel>
    {
        private readonly IEmailSender EmailSender;

        public ResetPasswordModel(ILogger<ResetPasswordModel> logger, BmcsContext context, IEmailSender emailSender) : base(logger, context)
        {
            EmailSender = emailSender;
        }

        /// <summary>
        /// 未ログインで使用する画面のため、POSTを許可する
        /// </summary>
        public override bool AllowAnonymousPost
        {
            get { return true; }
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// トークン有効フラグ
        /// </summary>
        public bool IsValidToken { get; set; }

        /// <summary>
        /// 完了フラグ
        /// </summary>
        public bool IsComplete { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IsValidToken = await GetValidTokenAsync() != null;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resetToken = await GetValidTokenAsync();

            if (resetToken == null)
            {
                IsValidToken = false;

                return Page();
            }

            IsValidToken = true;

            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(nameof(Password), "パスワードは必須です。");
            }

            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(nameof(ConfirmPassword), "パスワードが一致していません。");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userAccount = await Context.UserAccounts
                .FirstOrDefaultAsync(r => r.UserAccountID == resetToken.TargetID
                                       && r.DeleteFLG == false);

            //体験用ユーザは再設定できない（ForgotPassword で発行しないが、修正前に発行されたトークンも拒否する）
            if (userAccount == null || IsSampleUserAccountID(userAccount.UserAccountID))
            {
                IsValidToken = false;

                return Page();
            }

            //パスワード更新
            userAccount.Password = Password.ChangeHashValue();
            userAccount.UpdateUserID = userAccount.UserAccountID;
            userAccount.UpdateDatetime = DateTime.Now;

            //トークンを使用済にする
            resetToken.UsedFLG = true;
            resetToken.UpdateUserID = userAccount.UserAccountID;
            resetToken.UpdateDatetime = DateTime.Now;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //同一URLが同時に使用された場合は無効なURLとして扱う
                Logger.LogWarning(ex, $"パスワード再設定が競合しました。UserAccountID:{userAccount.UserAccountID}");

                IsValidToken = false;

                return Page();
            }

            //変更後は既存のログイン状態を破棄する
            HttpContext.Session.Clear();

            //本人へ変更完了を通知する
            await SendCompleteNotificationAsync(userAccount);

            IsComplete = true;

            return Page();
        }

        /// <summary>
        /// 有効なトークンを取得する（存在し、未使用で、期限内のもの）
        /// </summary>
        /// <returns></returns>
        private async Task<ResetToken> GetValidTokenAsync()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                return null;
            }

            return await Context.ResetTokens
                .FirstOrDefaultAsync(r => r.ResetTokenID == Token
                                       && r.ResetTokenClass == ResetTokenClass.UserPassword
                                       && r.UsedFLG == false
                                       && r.ExpireDatetime > DateTime.Now);
        }

        /// <summary>
        /// パスワード変更完了を本人へ通知する
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>
        private async Task SendCompleteNotificationAsync(Models.UserAccount userAccount)
        {
            if (string.IsNullOrWhiteSpace(userAccount.EmailAddress))
            {
                return;
            }

            var subject = "【Bmcs】パスワードを変更しました";
            var body = $"{MailBody.Escape(userAccount.UserAccountName)} 様<br /><br />"
                     + $"ユーザID：{MailBody.Escape(userAccount.UserAccountID)} のパスワードを変更しました。<br />"
                     + $"日時：{DateTime.Now:yyyy/MM/dd HH:mm}<br /><br />"
                     + "※お心当たりがない場合は、第三者による操作の可能性があります。お問い合わせページよりご連絡ください。<br />";

            try
            {
                await EmailSender.SendEmailAsync(userAccount.EmailAddress, subject, body);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"パスワード変更通知メールの送信に失敗しました。UserAccountID:{userAccount.UserAccountID}");
            }
        }
    }
}
