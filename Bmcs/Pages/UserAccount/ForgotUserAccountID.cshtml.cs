using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bmcs.Constans;
using Bmcs.Data;
using Bmcs.Function;
using Bmcs.Models;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.UserAccount
{
    /// <summary>
    /// ユーザID確認（登録メールアドレス宛に通知）
    /// </summary>
    public class ForgotUserAccountIDModel : PageModelBase<ForgotUserAccountIDModel>
    {
        private readonly IEmailSender EmailSender;

        private readonly IRateLimiter RateLimiter;

        public ForgotUserAccountIDModel(ILogger<ForgotUserAccountIDModel> logger, BmcsContext context, IEmailSender emailSender, IRateLimiter rateLimiter) : base(logger, context)
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
        public string EmailAddress { get; set; }

        /// <summary>
        /// 完了フラグ
        /// </summary>
        public bool IsComplete { get; set; }

        public void OnGet()
        {
            //インデックス
            IsIndex = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //インデックス
            IsIndex = true;

            if (string.IsNullOrWhiteSpace(EmailAddress))
            {
                ModelState.AddModelError(nameof(EmailAddress), "メールアドレスは必須です。");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            //試行回数制限（IP単位とメールアドレス単位。メールアドレス単位は大量送信を防ぐため）
            if (!RateLimiter.TryAttempt($"ForgotUserAccountID:IP:{base.GetRemoteIpAddress()}", SystemConstant.RecoverLimitCount, SystemConstant.RecoverLimitMinute)
             || !RateLimiter.TryAttempt($"ForgotUserAccountID:Email:{EmailAddress}", SystemConstant.SendMailLimitCount, SystemConstant.SendMailLimitMinute))
            {
                ModelState.AddModelError(string.Empty, "受け付け回数が上限に達しました。しばらく時間をおいてからお試しください。");

                return Page();
            }

            var inputEmailAddress = EmailAddress.Trim();

            var userAccountList = await Context.UserAccounts
                .Where(r => r.DeleteFLG == false
                         && r.EmailAddress != null
                         && r.EmailAddress.Trim().ToLower() == inputEmailAddress.ToLower())
                .ToListAsync();

            //※アカウントの有無を画面上で判別できないよう、該当なしの場合も同じ完了メッセージを表示する
            if (userAccountList.Any())
            {
                //ユーザIDは他人が登録した値が混在しうるため、必ずエスケープする
                var userAccountIDList = string.Join("<br />", userAccountList.Select(r => MailBody.Escape(r.UserAccountID)));

                var subject = "【Bmcs】ユーザIDのお知らせ";
                var body = "Bmcsにご登録のユーザIDをお知らせします。<br /><br />"
                         + $"{userAccountIDList}<br /><br />"
                         + "パスワードがわからない場合は、ログイン画面の「パスワードをお忘れの場合」から再設定用URLをお送りします。<br />"
                         + "※このメールに心当たりがない場合は、そのまま破棄してください。<br />";

                try
                {
                    //送信先は登録されている値を使用する
                    await EmailSender.SendEmailAsync(userAccountList.First().EmailAddress, subject, body);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "ユーザIDのメール送信に失敗しました。");
                }
            }

            IsComplete = true;

            return Page();
        }
    }
}
