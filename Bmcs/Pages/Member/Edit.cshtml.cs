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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;

namespace Bmcs.Pages.Member
{
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Member Member { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!base.IsLogin())
            {
                return NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            Member = await Context.Members
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.MemberID == id);

            if (Member == null
                || (Member.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID)
                    && !base.IsAdmin())
                )
            {
                return NotFound();
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberEdit);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                //データ作成
                var member = await Context.Members.FindAsync(Member.MemberID);

                if (member == null)
                {
                    return NotFound();
                }

                //POST値セット
                this.TryUpdateModel(member);
                //エントリ情報セット
                base.SetUpdateInfo(member);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("./Index", new { teamID = Member.TeamID });

        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="member"></param>
        private void TryUpdateModel(Models.Member member)
        {
            member.MemberName = Member.MemberName;
            member.MemberClass = Member.MemberClass;
            member.BatClass = Member.BatClass;
            member.ThrowClass = Member.ThrowClass;
            member.PositionGroupClass = Member.PositionGroupClass;
            member.UniformNumber = Member.UniformNumber;
            member.MessageDetail = Member.MessageDetail;
        }

    }
}
