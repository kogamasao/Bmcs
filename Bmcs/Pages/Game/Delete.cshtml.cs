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
using Bmcs.Enum;

namespace Bmcs.Pages.Game
{
    public class DeleteModel : PageModelBase<DeleteModel>
    {
        public DeleteModel(ILogger<DeleteModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Game Game { get; set; }

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

            Game = await Context.Games
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == id);

            if (Game == null
                || (Game.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID)
                    && !base.IsAdmin())
                )
            {
                return NotFound();
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.GameDelete);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var game = await Context.Games.FindAsync(id);

                if (game != null)
                {
                    game.DeleteFLG = true;
                    await Context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("./Index", new { teamID = Game.TeamID });
        }
    }
}
