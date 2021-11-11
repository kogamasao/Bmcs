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

        public async Task<IActionResult> OnGetAsync(string teamID, int? pageIndex)
        {
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

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            if (Team == null)
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

            //インデックス
            IsIndex = true;

            return Page();
        }
    }
}
