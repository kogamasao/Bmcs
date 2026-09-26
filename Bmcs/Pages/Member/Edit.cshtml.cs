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
using Bmcs.Function;
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
                return ReLogin();
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

        /// <summary>
        /// 画面の表示に必要なデータを取得する（入力エラーでの再表示にも使用する）
        /// </summary>
        /// <returns></returns>
        private async Task SetPageDataAsync()
        {
            if (Member != null)
            {
                var dbMember = await Context.Members
                    .Include(r => r.Team)
                    .FirstOrDefaultAsync(r => r.MemberID == Member.MemberID);

                Member.Team = dbMember?.Team;
                Member.TeamID = dbMember?.TeamID;
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberEdit);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                //背番号の入力チェック（全角数字・前後の空白を直してから確認する）
                //※入力チェックを追加する前に登録された数字以外の背番号は、変更していなければそのまま保存できるようにする。
                //  チェックで止めると、名前やメッセージだけを直したいときにも保存できなくなるため
                Member.UniformNumber = Member.UniformNumber.ToHalfWidthDigits();

                if (!Models.Member.IsValidUniformNumber(Member.UniformNumber))
                {
                    var dbUniformNumber = await Context.Members
                                                .Where(r => r.MemberID == Member.MemberID)
                                                .Select(r => r.UniformNumber)
                                                .FirstOrDefaultAsync();

                    if (Member.UniformNumber != dbUniformNumber.ToHalfWidthDigits())
                    {
                        ModelState.AddModelError($"{nameof(Member)}.{nameof(Models.Member.UniformNumber)}", Models.Member.UniformNumberErrorMessage);
                    }
                }

                if (!ModelState.IsValid)
                {
                    //再表示に使うチームは、POST 値ではなく DB の TeamID から取得する
                    //※TeamID は画面から送っていないが、POST に追加されればそのまま結び付けられるため、
                    //  書き換えで他チーム（非公開を含む）のチーム名を表示できてしまう
                    var dbTeamID = await Context.Members
                                        .Where(r => r.MemberID == Member.MemberID)
                                        .Select(r => r.TeamID)
                                        .FirstOrDefaultAsync();

                    if (!base.IsMyTeamData(dbTeamID))
                    {
                        return NotFound();
                    }

                    Member.TeamID = dbTeamID;

                    //再表示に必要なデータを取り直す
                    //※取り直さないと、画面でチーム名を参照している箇所で例外となる
                    await SetPageDataAsync();

                    return Page();
                }

                //データ作成
                var member = await Context.Members.FindAsync(Member.MemberID);

                if (member == null)
                {
                    return NotFound();
                }


                //自チーム以外のデータは更新できない（管理者は除く）
                if (!base.IsMyTeamData(member.TeamID))
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
            member.UniformNumber = Member.UniformNumber.ToHalfWidthDigits();
            member.MessageDetail = Member.MessageDetail;
        }

    }
}
