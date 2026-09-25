using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;

namespace Bmcs.Pages.Inquiry
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        private readonly Bmcs.Function.IEmailSender EmailSender;

        private readonly IConfiguration Configuration;

        private readonly Bmcs.Function.IRateLimiter RateLimiter;

        public CreateModel(ILogger<CreateModel> logger, BmcsContext context, Bmcs.Function.IEmailSender emailSender, IConfiguration configuration, Bmcs.Function.IRateLimiter rateLimiter) : base(logger, context)
        {
            EmailSender = emailSender;
            Configuration = configuration;
            RateLimiter = rateLimiter;
        }

        /// <summary>
        /// 未ログインで使用する画面のため、POSTを許可する
        /// </summary>
        public override bool AllowAnonymousPost
        {
            get { return true; }
        }

        /// <summary>
        /// 送信完了フラグ
        /// ※以前は送信後に何も表示せずトップへ戻しており、送れたかどうかが分からなかった
        /// </summary>
        public bool IsComplete { get; set; }

        public async Task<IActionResult> OnGetAsync(bool isComplete = false)
        {
            IsComplete = isComplete;

            Inquiry = new Models.Inquiry();
            
            //※体験用ユーザは共有アカウントのため、登録されているメールアドレスを出さない（返信が本人に届かない）
            if(base.IsLogin() && !base.IsSampleUser())
            {
                var userAccount = await Context.UserAccounts.FindAsync(HttpContext.Session.GetString(SessionConstant.UserAccountID));

                //メールアドレス
                Inquiry.EmailAddress = userAccount?.EmailAddress;
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.Inquiry);
            //検索エンジンに登録する（運営への連絡先が分かるように）
            SetIndex("/Inquiry/Create");

            return Page();
        }

        [BindProperty]
        public Models.Inquiry Inquiry { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                //入力エラーでの再表示でもヘルプを表示できるようにする
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.Inquiry);
                //検索エンジンに登録する（運営への連絡先が分かるように）
                SetIndex("/Inquiry/Create");

                return Page();
            }

            //大量送信の踏み台にならないよう、送信回数を制限する
            if (!RateLimiter.TryAttempt($"Inquiry:{base.GetRemoteIpAddress()}", SystemConstant.InquiryLimitCount, SystemConstant.InquiryLimitMinute))
            {
                ModelState.AddModelError(string.Empty, "お問い合わせの送信回数が上限に達しました。しばらく時間をおいてからお試しください。");

                //システム管理データ
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.Inquiry);
                //検索エンジンに登録する（運営への連絡先が分かるように）
                SetIndex("/Inquiry/Create");

                return Page();
            }

            //データ作成
            var inquiry = new Models.Inquiry();

            //POST値セット
            this.TryUpdateModel(inquiry);
            //エントリ情報セット
            base.SetEntryInfo(inquiry);

            Context.Inquirys.Add(inquiry);
            await Context.SaveChangesAsync();

            //管理者へ通知
            await SendAdminNotificationAsync(inquiry);

            //完了表示へ（再読み込みで二重に送信されないよう、リダイレクトする）
            return RedirectToPage("./Create", new { isComplete = true });
        }

        /// <summary>
        /// 問い合わせ登録を管理者へメール通知する
        /// </summary>
        /// <param name="inquiry"></param>
        private async Task SendAdminNotificationAsync(Models.Inquiry inquiry)
        {
            var emailSettings = Configuration.GetSection("EmailSettings");
            //管理者アドレス未設定時は送信元アドレスへ送る
            var adminEmailAddress = emailSettings["AdminEmail"];

            if (string.IsNullOrWhiteSpace(adminEmailAddress))
            {
                adminEmailAddress = emailSettings["SenderEmail"];
            }

            if (string.IsNullOrWhiteSpace(adminEmailAddress))
            {
                Logger.LogWarning("問い合わせ通知先のメールアドレスが未設定です。");

                return;
            }

            var loginUserAccountID = HttpContext.Session.GetString(SessionConstant.UserAccountID);

            var subject = $"【Bmcs】お問い合わせ受信：{Bmcs.Function.MailBody.EscapeSubject(inquiry.InquiryTitle)}";
            var body = $"お問い合わせを受信しました。<br /><br />"
                     + $"問い合わせID：{inquiry.InquiryID}<br />"
                     + $"日時：{inquiry.EntryDatetime:yyyy/MM/dd HH:mm:ss}<br />"
                     + $"メールアドレス：{Bmcs.Function.MailBody.Escape(inquiry.EmailAddress)}<br />"
                     + $"ログインユーザID：{(string.IsNullOrEmpty(loginUserAccountID) ? "(未ログイン)" : Bmcs.Function.MailBody.Escape(loginUserAccountID))}<br />"
                     + $"タイトル：{Bmcs.Function.MailBody.Escape(inquiry.InquiryTitle)}<br /><br />"
                     + $"内容：<br />{Bmcs.Function.MailBody.Escape(inquiry.InquiryDetail)}<br />";

            try
            {
                //メール送信の失敗で問い合わせ登録自体を失敗させないため、例外はログのみとする
                await EmailSender.SendEmailAsync(adminEmailAddress, subject, body);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"問い合わせ通知メールの送信に失敗しました。InquiryID:{inquiry.InquiryID}");
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="inquiry"></param>
        private void TryUpdateModel(Models.Inquiry inquiry)
        {
            inquiry.EmailAddress = Inquiry.EmailAddress;
            inquiry.InquiryTitle = Inquiry.InquiryTitle;
            inquiry.InquiryDetail = Inquiry.InquiryDetail;
            inquiry.ReplyFLG = false;
            inquiry.CompleteFLG = false;
        }
    }
}
