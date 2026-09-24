using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bmcs.Data;
using Bmcs.Enum;
using Bmcs.Function;
using Bmcs.Models;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.Team
{
    /// <summary>
    /// チームパスワード再設定（メールのURLからの再設定）
    /// </summary>
    public class ResetTeamPasswordModel : PageModelBase<ResetTeamPasswordModel>
    {
        private readonly IEmailSender EmailSender;

        public ResetTeamPasswordModel(ILogger<ResetTeamPasswordModel> logger, BmcsContext context, IEmailSender emailSender) : base(logger, context)
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
        public string TeamPassword { get; set; }

        [BindProperty]
        public string ConfirmTeamPassword { get; set; }

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
            //インデックス
            IsIndex = true;

            IsValidToken = await GetValidTokenAsync() != null;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //インデックス
            IsIndex = true;

            var resetToken = await GetValidTokenAsync();

            if (resetToken == null)
            {
                IsValidToken = false;

                return Page();
            }

            IsValidToken = true;

            if (string.IsNullOrWhiteSpace(TeamPassword))
            {
                ModelState.AddModelError(nameof(TeamPassword), "チームパスワードは必須です。");
            }

            if (TeamPassword != ConfirmTeamPassword)
            {
                ModelState.AddModelError(nameof(ConfirmTeamPassword), "チームパスワードが一致していません。");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var team = await Context.Teams
                .FirstOrDefaultAsync(r => r.TeamID == resetToken.TargetID
                                       && r.DeleteFLG == false
                                       && r.SystemDataFLG == false);

            //サンプルチームは再設定できない（修正前に発行されたトークンも拒否する）
            if (team == null || IsSampleTeamID(team.TeamID))
            {
                IsValidToken = false;

                return Page();
            }

            //チームパスワード更新
            team.TeamPassword = TeamPassword.ChangeHashValue();
            team.UpdateUserID = team.TeamID;
            team.UpdateDatetime = DateTime.Now;

            //トークンを使用済にする
            resetToken.UsedFLG = true;
            resetToken.UpdateUserID = team.TeamID;
            resetToken.UpdateDatetime = DateTime.Now;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //同一URLが同時に使用された場合は無効なURLとして扱う
                Logger.LogWarning(ex, $"チームパスワード再設定が競合しました。TeamID:{team.TeamID}");

                IsValidToken = false;

                return Page();
            }

            //チームへ変更完了を通知する
            await SendCompleteNotificationAsync(team);

            IsComplete = true;

            return Page();
        }

        /// <summary>
        /// チームパスワード変更完了をチームのメールアドレスへ通知する
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        private async Task SendCompleteNotificationAsync(Models.Team team)
        {
            if (string.IsNullOrWhiteSpace(team.TeamEmailAddress))
            {
                return;
            }

            var subject = "【Bmcs】チームパスワードを変更しました";
            var body = $"チーム「{MailBody.Escape(team.TeamName)}」のチームパスワードを変更しました。<br />"
                     + $"日時：{DateTime.Now:yyyy/MM/dd HH:mm}<br /><br />"
                     + "※お心当たりがない場合は、「チーム情報変更」からチームパスワードを変更し、お問い合わせページよりご連絡ください。<br />";

            try
            {
                await EmailSender.SendEmailAsync(team.TeamEmailAddress, subject, body);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"チームパスワード変更通知メールの送信に失敗しました。TeamID:{team.TeamID}");
            }
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
                                       && r.ResetTokenClass == ResetTokenClass.TeamPassword
                                       && r.UsedFLG == false
                                       && r.ExpireDatetime > DateTime.Now);
        }
    }
}
