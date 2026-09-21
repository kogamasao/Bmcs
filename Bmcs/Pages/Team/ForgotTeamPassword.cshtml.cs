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

namespace Bmcs.Pages.Team
{
    /// <summary>
    /// チームパスワード再設定（再設定用URLの送信）
    /// </summary>
    public class ForgotTeamPasswordModel : PageModelBase<ForgotTeamPasswordModel>
    {
        private readonly IEmailSender EmailSender;

        private readonly IRateLimiter RateLimiter;

        public ForgotTeamPasswordModel(ILogger<ForgotTeamPasswordModel> logger, BmcsContext context, IEmailSender emailSender, IRateLimiter rateLimiter) : base(logger, context)
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
        public new string TeamID { get; set; }

        [BindProperty]
        public string TeamEmailAddress { get; set; }

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

            if (string.IsNullOrWhiteSpace(TeamID))
            {
                ModelState.AddModelError(nameof(TeamID), "チームIDは必須です。");
            }

            if (string.IsNullOrWhiteSpace(TeamEmailAddress))
            {
                ModelState.AddModelError(nameof(TeamEmailAddress), "メールアドレスは必須です。");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            //試行回数制限
            if (!RateLimiter.TryAttempt($"ForgotTeamPassword:IP:{base.GetRemoteIpAddress()}", SystemConstant.RecoverLimitCount, SystemConstant.RecoverLimitMinute)
             || !RateLimiter.TryAttempt($"ForgotTeamPassword:Team:{TeamID}", SystemConstant.SendMailLimitCount, SystemConstant.SendMailLimitMinute))
            {
                ModelState.AddModelError(string.Empty, "受け付け回数が上限に達しました。しばらく時間をおいてからお試しください。");

                return Page();
            }

            var team = await Context.Teams
                .FirstOrDefaultAsync(r => r.TeamID == TeamID
                                       && r.DeleteFLG == false
                                       && r.SystemDataFLG == false);

            //チームIDとチーム登録メールアドレスの両方が一致した場合のみ再設定用URLを送信する
            //※チームの有無を画面上で判別できないよう、一致しない場合も同じ完了メッセージを表示する
            //※この時点ではチームパスワードを変更しない（第三者による一方的な変更を防ぐため）
            if (team != null
                && !string.IsNullOrWhiteSpace(team.TeamEmailAddress)
                && string.Equals(team.TeamEmailAddress.Trim(), TeamEmailAddress.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                //発行済みの未使用トークンを失効させる
                var oldResetTokenList = await Context.ResetTokens
                    .Where(r => r.ResetTokenClass == ResetTokenClass.TeamPassword
                             && r.TargetID == team.TeamID
                             && r.UsedFLG == false)
                    .ToListAsync();

                foreach (var oldResetToken in oldResetTokenList)
                {
                    oldResetToken.UsedFLG = true;
                    oldResetToken.UpdateUserID = team.TeamID;
                    oldResetToken.UpdateDatetime = DateTime.Now;
                }

                var resetToken = new ResetToken
                {
                    ResetTokenID = TokenGenerator.CreateToken(),
                    ResetTokenClass = ResetTokenClass.TeamPassword,
                    TargetID = team.TeamID,
                    ExpireDatetime = DateTime.Now.AddHours(SystemConstant.ResetTokenExpireHour),
                    UsedFLG = false,
                    EntryUserID = team.TeamID,
                    EntryDatetime = DateTime.Now,
                    UpdateUserID = team.TeamID,
                    UpdateDatetime = DateTime.Now,
                };

                Context.ResetTokens.Add(resetToken);

                await Context.SaveChangesAsync();

                var resetUrl = base.CreateAbsoluteUrl(Url.Page("/Team/ResetTeamPassword", new { token = resetToken.ResetTokenID }));

                var subject = "【Bmcs】チームパスワード再設定のご案内";
                var body = $"チーム「{MailBody.Escape(team.TeamName)}」のチームパスワード再設定のご案内です。<br />"
                         + "以下のURLからチームパスワードを再設定してください。<br /><br />"
                         + $"<a href=\"{resetUrl}\">{resetUrl}</a><br /><br />"
                         + $"チームID：{MailBody.Escape(team.TeamID)}<br />"
                         + $"有効期限：{resetToken.ExpireDatetime:yyyy/MM/dd HH:mm}（発行から{SystemConstant.ResetTokenExpireHour}時間）<br />"
                         + "※URLは1回のみ使用できます。<br />"
                         + "※このメールに心当たりがない場合は、そのまま破棄してください。チームパスワードは変更されません。<br />";

                try
                {
                    await EmailSender.SendEmailAsync(team.TeamEmailAddress, subject, body);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"チームパスワード再設定メールの送信に失敗しました。TeamID:{team.TeamID}");
                }
            }

            IsComplete = true;

            return Page();
        }
    }
}
