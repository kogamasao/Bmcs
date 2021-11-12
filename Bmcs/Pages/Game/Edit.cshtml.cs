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
using Bmcs.Constans;
using Microsoft.AspNetCore.Http;
using Bmcs.Enum;

namespace Bmcs.Pages.Game
{
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
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
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.GameEdit);

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
                var game = await Context.Games.FindAsync(Game.GameID);

                if (game == null)
                {
                    return NotFound();
                }

                //POST値セット
                this.TryUpdateModel(game);
                //エントリ情報セット
                base.SetUpdateInfo(game);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (Game.StatusClass == StatusClass.BeforeFix
                || Game.StatusClass == StatusClass.EndGame)
            {
                return RedirectToPage("/Game/Index");
            }
            else
            { 
                if (Game.GameInputTypeClass == GameInputTypeClass.ByPlay)
                {
                    return RedirectToPage("/Order/Edit", new { gameID = Game.GameID });
                }
                else
                {
                    return RedirectToPage("/GameScore/Edit", new { gameID = Game.GameID });
                }
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="member"></param>
        private void TryUpdateModel(Models.Game game)
        {
            game.GameDate = Game.GameDate;
            game.GameClass = Game.GameClass;
            game.OpponentTeamName = Game.OpponentTeamName;
            game.OpponentTeamAbbreviation = Game.OpponentTeamAbbreviation;
            game.StadiumName = Game.StadiumName;
            game.WeatherClass = Game.WeatherClass;
            game.BatFirstBatSecondClass = Game.BatFirstBatSecondClass;
            game.GameInputTypeClass = Game.GameInputTypeClass;
        }
    }
}
