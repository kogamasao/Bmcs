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
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.Member
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Member Member { get; set; }

        public async Task<IActionResult> OnGetAsync(string teamID)
        {
            if (!base.IsLogin())
            {
                return NotFound();
            }

            //マイチーム以外を指定して管理者でない
            if (!string.IsNullOrEmpty(teamID)
                && teamID != HttpContext.Session.GetString(SessionConstant.TeamID)
                && !base.IsAdmin())
            {
                return NotFound();
            }

            if (teamID == null)
            {
                teamID = HttpContext.Session.GetString(SessionConstant.TeamID);
            }

            if (string.IsNullOrEmpty(teamID))
            {
                return NotFound();
            }

            Member = new Models.Member();
            Member.Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            if (Member.Team == null)
            {
                return NotFound();
            }

            //チームID
            Member.TeamID = Member.Team.TeamID;

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
                var member = new Models.Member();

                //POST値セット
                this.TryUpdateModel(member);
                //エントリ情報セット
                base.SetEntryInfo(member);

                Context.Members.Add(member);

                if (!base.IsAdmin())
                {
                    //ユーザ情報取得
                    var team = Context.Teams.FirstOrDefault(r => r.TeamID == Member.TeamID);

                    if (team != null)
                    {
                        //チーム人数更新
                        team.TeamNumber = Context.Members.Where(r => r.TeamID == Member.TeamID).Count() + 1;

                        //更新情報セット
                        base.SetUpdateInfo(team);
                    }
                }

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
            member.TeamID = Member.TeamID;
            member.MemberName = Member.MemberName;
            member.MemberClass = Member.MemberClass;
            member.BatClass = Member.BatClass;
            member.ThrowClass = Member.ThrowClass;
            member.PositionGroupClass = Member.PositionGroupClass;
            member.UniformNumber = Member.UniformNumber;
            member.MessageDetail = Member.MessageDetail;
            member.DeleteFLG = false;
        }
    }
}
