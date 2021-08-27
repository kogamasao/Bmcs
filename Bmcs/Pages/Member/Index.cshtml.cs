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

namespace Bmcs.Pages.Member
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public IList<Models.Member> Member { get;set; }

        public Models.Team Team { get; set; }

        public async Task<IActionResult> OnGetAsync(string teamID)
        {
            bool isMyTeam = false;

            if (!base.IsLogin())
            {
                return NotFound();
            }

            if (teamID == null)
            {
                isMyTeam = true;
                teamID = HttpContext.Session.GetString(SessionConstant.TeamID);
            }

            if(string.IsNullOrEmpty(teamID))
            {
                return NotFound();
            }

            if (!base.IsAdmin())
            {
                Member = await Context.Members
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID
                        && r.Team.DeleteFLG == false
                        && ((r.Team.PublicFLG == true && !isMyTeam) || isMyTeam)
                        && r.DeleteFLG == false).ToListAsync();
            }
            else
            {
                Member = await Context.Members
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID).ToListAsync();
            }

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            if (Team == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
