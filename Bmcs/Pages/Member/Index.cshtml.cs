using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;
using Bmcs.PageHelper;

namespace Bmcs.Pages.Member
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public PaginatedList<Models.Member> Member { get; set; }

        public Models.Team Team { get; set; }

        /// <summary>
        /// メンバー追加で登録した人数（登録直後のみ。「◯人を登録しました」を表示する）
        /// </summary>
        public int? RegisteredCount { get; set; }

        public async Task<IActionResult> OnGetAsync(string teamID, int? pageIndex, int? registered)
        {
            RegisteredCount = registered > 0 ? registered : null;

            IsMyTeam = false;

            if (teamID == null
                || teamID == HttpContext.Session.GetString(SessionConstant.TeamID))
            {
                IsMyTeam = true;
                teamID = HttpContext.Session.GetString(SessionConstant.TeamID);
            }

            if(string.IsNullOrEmpty(teamID))
            {
                return NotFound();
            }

            List<Models.Member> memberList;

            if (!base.IsAdmin())
            {
                memberList = await Context.Members
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID
                        && r.Team.DeleteFLG == false
                        && ((r.Team.PublicFLG == true && !IsMyTeam) || IsMyTeam)
                        && r.DeleteFLG == false)
                    .ToListAsync();
            }
            else
            {
                memberList = await Context.Members
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID)
                    .ToListAsync();
            }

            Member = PaginatedList<Models.Member>.Create(
                    memberList
                    .OrderBy(r => r.MemberClass)
                    .ThenBy(r => r.PositionGroupClass)
                    .ThenBy(r => r.OrderUniformNumber)
                    .ThenBy(r => r.UniformNumber).AsQueryable().AsNoTracking(), pageIndex ?? 1, 20);

            //存在しないページを指定された場合は1ページ目へ戻す
            //※そのまま表示すると「メンバーが登録されていません」と誤表示され、
            //  ページ送りも描画されないため一覧へ戻れなくなる
            if (Member.TotalPages > 0 && Member.PageIndex > Member.TotalPages)
            {
                return RedirectToPage("./Index", new { teamID });
            }

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            //非公開・削除済みの他チームは、チーム名も表示しない（URL直接指定での確認を防ぐ）
            if (Team == null
                || (!base.IsAdmin() && !IsMyTeam && (Team.DeleteFLG || !Team.PublicFLG)))
            {
                return NotFound();
            }

            //システム管理データ
            if (IsMyTeam)
            {
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MyTeamMemberIndex);
            }
            else
            {
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.PublicMemberIndex);
            }


            return Page();
        }
    }
}
