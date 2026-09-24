using System;
using System.Collections.Generic;
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
    /// チーム情報によるアカウント復旧
    /// メールアドレス未登録のユーザ向けに、チームID＋チームパスワードで本人確認を行い、
    /// ユーザIDの確認とパスワードの再設定を可能にする。
    /// ※メールアドレス登録済みのユーザはメールでの再設定（ForgotPassword）が使えるため、
    /// 　チームパスワードを知る第三者による乗っ取りを防ぐ目的で復旧対象から除外する。
    /// </summary>
    public class RecoverByTeamModel : PageModelBase<RecoverByTeamModel>
    {
        private readonly IRateLimiter RateLimiter;

        private readonly IEmailSender EmailSender;

        public RecoverByTeamModel(ILogger<RecoverByTeamModel> logger, BmcsContext context, IRateLimiter rateLimiter, IEmailSender emailSender) : base(logger, context)
        {
            RateLimiter = rateLimiter;
            EmailSender = emailSender;
        }

        /// <summary>
        /// 未ログインで使用する画面のため、POSTを許可する
        /// </summary>
        public override bool AllowAnonymousPost
        {
            get { return true; }
        }

        [BindProperty]
        public string RecoverTeamID { get; set; }

        [BindProperty]
        public string TeamPassword { get; set; }

        [BindProperty]
        public string SelectedUserAccountID { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// 認証済フラグ（チームIDとチームパスワードが一致）
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// 完了フラグ
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// チーム名
        /// </summary>
        public string RecoverTeamName { get; set; }

        /// <summary>
        /// 復旧対象のユーザ一覧
        /// </summary>
        public List<Models.UserAccount> UserAccountList { get; set; } = new List<Models.UserAccount>();

        /// <summary>
        /// 認証したチーム（通知用）
        /// </summary>
        private Models.Team AuthenticatedTeam { get; set; }

        public void OnGet()
        {
            //インデックス
            IsIndex = true;
        }

        /// <summary>
        /// チーム認証（ユーザ一覧の表示）
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAuthenticateAsync()
        {
            //インデックス
            IsIndex = true;

            if (!ValidateTeamInput())
            {
                return Page();
            }

            await AuthenticateTeamAsync();

            return Page();
        }

        /// <summary>
        /// パスワード再設定
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostResetAsync()
        {
            //インデックス
            IsIndex = true;

            //入力チェックを先に行う（入力ミスで試行回数を消費しないため）
            var isValidInput = ValidateTeamInput();

            if (string.IsNullOrWhiteSpace(SelectedUserAccountID))
            {
                ModelState.AddModelError(nameof(SelectedUserAccountID), "パスワードを再設定するユーザを選択してください。");

                isValidInput = false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(nameof(Password), "パスワードは必須です。");

                isValidInput = false;
            }

            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(nameof(ConfirmPassword), "パスワードが一致していません。");

                isValidInput = false;
            }

            if (!isValidInput)
            {
                //チームID・チームパスワードが入力されている場合のみ、一覧を再表示するために再認証する
                if (!string.IsNullOrWhiteSpace(RecoverTeamID)
                    && !string.IsNullOrWhiteSpace(TeamPassword))
                {
                    await AuthenticateTeamAsync();
                }

                return Page();
            }

            //再設定時もチームパスワードを再検証する
            if (!await AuthenticateTeamAsync())
            {
                return Page();
            }

            //選択されたユーザが対象チームの復旧対象であることを確認する
            var userAccount = UserAccountList.FirstOrDefault(r => r.UserAccountID == SelectedUserAccountID);

            if (userAccount == null)
            {
                ModelState.AddModelError(nameof(SelectedUserAccountID), "選択したユーザはこのチームの復旧対象ではありません。");

                return Page();
            }

            userAccount.Password = Password.ChangeHashValue();
            userAccount.UpdateUserID = userAccount.UserAccountID;
            userAccount.UpdateDatetime = DateTime.Now;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //他の画面で同時に更新された場合は、やり直しを促す
                Logger.LogWarning(ex, $"チーム情報によるパスワード再設定が競合しました。UserAccountID:{userAccount.UserAccountID}");

                ModelState.AddModelError(string.Empty, "他の処理と競合したため、再設定できませんでした。お手数ですが、もう一度お試しください。");

                return Page();
            }

            //第三者による不正な再設定を検知できるよう、チームのメールアドレスへ通知する
            //※復旧対象のユーザ自身はメールアドレス未登録のため、チーム宛に送信する
            await SendCompleteNotificationAsync(userAccount);

            IsComplete = true;

            return Page();
        }

        /// <summary>
        /// パスワード再設定をチームのメールアドレスへ通知する
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>
        private async Task SendCompleteNotificationAsync(Models.UserAccount userAccount)
        {
            if (AuthenticatedTeam == null
                || string.IsNullOrWhiteSpace(AuthenticatedTeam.TeamEmailAddress))
            {
                return;
            }

            var subject = "【Bmcs】ユーザのパスワードが再設定されました";
            var body = $"チーム「{MailBody.Escape(AuthenticatedTeam.TeamName)}」のユーザについて、"
                     + "チーム情報による復旧機能からパスワードが再設定されました。<br /><br />"
                     + $"ユーザID：{MailBody.Escape(userAccount.UserAccountID)}<br />"
                     + $"日時：{DateTime.Now:yyyy/MM/dd HH:mm}<br /><br />"
                     + "※お心当たりがない場合は、チームパスワードが第三者に知られている可能性があります。<br />"
                     + "　チーム編集画面からチームパスワードを変更し、お問い合わせページよりご連絡ください。<br />";

            try
            {
                await EmailSender.SendEmailAsync(AuthenticatedTeam.TeamEmailAddress, subject, body);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"パスワード再設定の通知メールの送信に失敗しました。TeamID:{AuthenticatedTeam.TeamID}");
            }
        }

        /// <summary>
        /// チーム情報の入力チェック
        /// </summary>
        /// <returns></returns>
        private bool ValidateTeamInput()
        {
            if (string.IsNullOrWhiteSpace(RecoverTeamID))
            {
                ModelState.AddModelError(nameof(RecoverTeamID), "チームIDは必須です。");
            }

            if (string.IsNullOrWhiteSpace(TeamPassword))
            {
                ModelState.AddModelError(nameof(TeamPassword), "チームパスワードは必須です。");
            }

            return ModelState.IsValid;
        }

        /// <summary>
        /// チームIDとチームパスワードを検証し、復旧対象のユーザを取得する
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AuthenticateTeamAsync()
        {
            var ipKey = $"RecoverByTeam:IP:{base.GetRemoteIpAddress()}";

            //IP単位の制限を先に判定する
            if (RateLimiter.IsBlocked(ipKey, SystemConstant.RecoverLimitCount, SystemConstant.RecoverLimitMinute))
            {
                ModelState.AddModelError(string.Empty, $"チームパスワードの入力に何度も失敗したため、一時的に受け付けを停止しています。{SystemConstant.RecoverLimitMinute}分ほど時間をおいてからお試しください。");

                return false;
            }

            var team = await Context.Teams
                .FirstOrDefaultAsync(r => r.TeamID == RecoverTeamID
                                       && r.DeleteFLG == false
                                       && r.SystemDataFLG == false);

            //チーム単位のキーは、DBに存在する場合はDBの値（正規値）を使用する
            //※DBの照合順序は大文字小文字・全角半角・末尾空白を区別しないため、
            //　入力値のままでは別のキーとして数えられ、制限を回避されてしまう
            var teamKey = $"RecoverByTeam:Team:{(team != null ? team.TeamID : RecoverTeamID)}";

            if (RateLimiter.IsBlocked(teamKey, SystemConstant.RecoverLimitCount, SystemConstant.RecoverLimitMinute))
            {
                ModelState.AddModelError(string.Empty, $"チームパスワードの入力に何度も失敗したため、一時的に受け付けを停止しています。{SystemConstant.RecoverLimitMinute}分ほど時間をおいてからお試しください。");

                return false;
            }

            if (team == null
                || team.TeamPassword != TeamPassword.ChangeHashValue())
            {
                //失敗時のみ回数を加算する
                RateLimiter.AddFailure(ipKey, SystemConstant.RecoverLimitMinute);
                RateLimiter.AddFailure(teamKey, SystemConstant.RecoverLimitMinute);

                ModelState.AddModelError(nameof(TeamPassword), "チームIDまたはチームパスワードが間違っています。");

                return false;
            }

            //認証成功時は対象チームの失敗回数のみリセットする
            //※IP単位はリセットしない（正規の認証を混ぜてIP制限を消せてしまうため）
            RateLimiter.Reset(teamKey);

            //復旧対象はメールアドレス未登録のユーザのみとする（管理者・体験用ユーザは除外）
            //※体験用ユーザは誰でもログインできる共有アカウント。サンプルチームのチームパスワードは推測しやすく、
            //  パスワードを再設定されると「サンプルチームで体験する」が全員に対して動かなくなる
            UserAccountList = await Context.UserAccounts
                .Where(r => r.TeamID == team.TeamID
                         && r.DeleteFLG == false
                         && r.UserAccountID != SystemConstant.AdminUserAccountID
                         && r.UserAccountID != SystemConstant.SampleUserAccountID
                         //空白のみのメールアドレスも未登録として扱う（編集画面の判定と揃える）
                         && (r.EmailAddress == null || r.EmailAddress.Trim() == ""))
                .OrderBy(r => r.UserAccountID)
                .ToListAsync();

            RecoverTeamName = team.TeamName;
            AuthenticatedTeam = team;
            IsAuthenticated = true;

            return true;
        }
    }
}
