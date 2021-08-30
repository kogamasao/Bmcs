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
using Bmcs.Constans;
using Microsoft.AspNetCore.Http;

namespace Bmcs.Pages.Game
{
    public class DetailsModel : PageModelBase<DetailsModel>
    {
        public DetailsModel(ILogger<DetailsModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public Models.Game Game { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Game = await Context.Games
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == id);

            if (Game == null)
            {
                return NotFound();
            }

            if (!base.IsAdmin()
                && (Game.Team.DeleteFLG == true
                    || (Game.Team.PublicFLG == false && Game.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID))
                    || Game.DeleteFLG == true
                    )
                )
            {
                return NotFound();
            }

            return Page();
        }
    }
}
