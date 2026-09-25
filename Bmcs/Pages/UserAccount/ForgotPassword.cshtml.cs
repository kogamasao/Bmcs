using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bmcs.Constans;
using Bmcs.Data;
using Bmcs.Enum;
using Bmcs.Function;
using Bmcs.Models;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.UserAccount
{
    /// <summary>
    /// パスワード再設定（再設定用URLの送信）
    /// </summary>
    public class ForgotPasswordModel : PageModelBase<ForgotPasswordModel>
    {
        private readonly IEmailSender EmailSender;

        private readonly IRateLimiter RateLimiter;

        public ForgotPasswordModel(ILogger<ForgotPasswordModel> logger, BmcsContext context, IEmailSender emailSender, IRateLimiter rateLimiter) : base(logger, context)
        {
            EmailSender = emailSender;
            RateLimiter = rateLimiter;
        }

        /// <summary>
        /// 未ログインで使用する画面のため、POSTを許可する
        /// </summary>
        public override bool AllowAnonymousPost
        {
            get { return true; }
        }

        [BindProperty]
        public string UserAccountID { get; set; }

        [BindProperty]
        public string EmailAddress { get; set; }

        /// <summary>
        /// 完了フラグ
        /// </summary>
        public bool IsComplete { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserAccountID))
            {
                ModelState.AddModelError(nameof(UserAccountID), "ユーザIDは必須です。");
            }

            if (string.IsNullOrWhiteSpace(EmailAddress))
            {
                ModelState.AddModelError(nameof(EmailAddress), "メールアドレスは必須です。");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            //試行回数制限（IP単位とユーザ単位。ユーザ単位はメールの大量送信・再設定の妨害を防ぐため）
            if (!RateLimiter.TryAttempt($"ForgotPassword:IP:{base.GetRemoteIpAddress()}", SystemConstant.RecoverLimitCount, SystemConstant.RecoverLimitMinute)
             || !RateLimiter.TryAttempt($"ForgotPassword:User:{UserAccountID}", SystemConstant.SendMailLimitCount, SystemConstant.SendMailLimitMinute))
            {
                ModelState.AddModelError(string.Empty, "受け付け回数が上限に達しました。しばらく時間をおいてからお試しください。");

                return Page();
            }

            var userAccount = await Context.UserAccounts
                .FirstOrDefaultAsync(r => r.UserAccountID == UserAccountID
                                       && r.DeleteFLG == false);

            //ユーザIDとメールアドレスの両方が一致した場合のみ再設定用URLを送信する
            //※アカウントの有無を画面上で判別できないよう、一致しない場合も同じ完了メッセージを表示する
            //※体験用ユーザは対象外（共有アカウントのため、メールアドレスを書き換えた第三者が再設定できてしまう）
            if (userAccount != null
                && !IsSampleUserAccountID(userAccount.UserAccountID)
                && userAccount.UserAccountID == UserAccountID
                && !string.IsNullOrWhiteSpace(userAccount.EmailAddress)
                && string.Equals(userAccount.EmailAddress.Trim(), EmailAddress.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                //発行済みの未使用トークンを失効させる
                var oldResetTokenList = await Context.ResetTokens
                    .Where(r => r.ResetTokenClass == ResetTokenClass.UserPassword
                             && r.TargetID == userAccount.UserAccountID
                             && r.UsedFLG == false)
                    .ToListAsync();

                foreach (var oldResetToken in oldResetTokenList)
                {
                    oldResetToken.UsedFLG = true;
                    oldResetToken.UpdateUserID = userAccount.UserAccountID;
                    oldResetToken.UpdateDatetime = DateTime.Now;
                }

                var resetToken = new ResetToken
                {
                    ResetTokenID = TokenGenerator.CreateToken(),
                    ResetTokenClass = ResetTokenClass.UserPassword,
                    TargetID = userAccount.UserAccountID,
                    ExpireDatetime = DateTime.Now.AddHours(SystemConstant.ResetTokenExpireHour),
                    UsedFLG = false,
                    EntryUserID = userAccount.UserAccountID,
                    EntryDatetime = DateTime.Now,
                    UpdateUserID = userAccount.UserAccountID,
                    UpdateDatetime = DateTime.Now,
                };

                Context.ResetTokens.Add(resetToken);

                await Context.SaveChangesAsync();

                var resetUrl = base.CreateAbsoluteUrl(Url.Page("/UserAccount/ResetPassword", new { token = resetToken.ResetTokenID }));

                var subject = "【Bmcs】パスワード再設定のご案内";
                var body = $"{MailBody.Escape(userAccount.UserAccountName)} 様<br /><br />"
                         + "パスワード再設定のご案内です。<br />"
                         + "以下のURLからパスワードを再設定してください。<br /><br />"
                         + $"<a href=\"{resetUrl}\">{resetUrl}</a><br /><br />"
                         + $"ユーザID：{MailBody.Escape(userAccount.UserAccountID)}<br />"
                         + $"有効期限：{resetToken.ExpireDatetime:yyyy/MM/dd HH:mm}（発行から{SystemConstant.ResetTokenExpireHour}時間）<br />"
                         + "※URLは1回のみ使用できます。<br />"
                         + "※このメールに心当たりがない場合は、そのまま破棄してください。パスワードは変更されません。<br />";

                try
                {
                    await EmailSender.SendEmailAsync(userAccount.EmailAddress, subject, body);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"パスワード再設定メールの送信に失敗しました。UserAccountID:{userAccount.UserAccountID}");
                }
            }

            IsComplete = true;

            return Page();
        }
    }
}
