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
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;

namespace Bmcs.Pages.Inquiry
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public async Task<IActionResult> OnGetAsync()
        {
            Inquiry = new Models.Inquiry();
            
            if(base.IsLogin())
            {
                var userAccount = await Context.UserAccounts.FindAsync(HttpContext.Session.GetString(SessionConstant.UserAccountID));

                //メールアドレス
                Inquiry.EmailAddress = userAccount.EmailAddress;
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.Inquiry);
            //インデックス
            IsIndex = true;

            return Page();
        }

        [BindProperty]
        public Models.Inquiry Inquiry { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
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

            return RedirectToPage("/Top/Index");
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
