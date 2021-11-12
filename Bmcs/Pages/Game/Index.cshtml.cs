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

namespace Bmcs.Pages.Game
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public PaginatedList<Models.Game> Game { get; set; }

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

            if (string.IsNullOrEmpty(teamID))
            {
                return NotFound();
            }

            List<Models.Game> gameList;

            if (!base.IsAdmin())
            {
                gameList = await Context.Games
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID
                        && r.Team.DeleteFLG == false
                        && ((r.Team.PublicFLG == true && !IsMyTeam) || IsMyTeam)
                        && r.DeleteFLG == false)
                    .OrderBy(r => r.GameDate)
                    .ThenBy(r => r.GameID)
                    .ToListAsync();
            }
            else
            {
                gameList = await Context.Games
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID
                        && r.DeleteFLG == false)
                    .OrderBy(r => r.GameDate)
                    .ThenBy(r => r.GameID)
                    .ToListAsync();
            }

            Game = PaginatedList<Models.Game>.Create(
                   gameList.AsQueryable().AsNoTracking(), pageIndex ?? 1, 20);

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            if (Team == null)
            {
                return NotFound();
            }

            //システム管理データ
            if (IsMyTeam)
            {
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MyTeamGameIndex);
            }
            else
            {
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.PublicGameIndex);
            }

            //インデックス
            IsIndex = true;

            return Page();
        }
    }
}
