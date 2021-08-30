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
using Bmcs.Enum;

namespace Bmcs.Pages.Game
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Game Game { get; set; }

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

            Game = new Models.Game();
            Game.Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            if (Game.Team == null)
            {
                return NotFound();
            }

            //チームID
            Game.TeamID = Game.Team.TeamID;
            //日時
            Game.GameDate = DateTime.Now;
            //表裏タイプ
            Game.BatFirstBatSecondClass = BatFirstBatSecondClass.First;
            //試合入力タイプ
            Game.GameInputTypeClass = GameInputTypeClass.ByBatter;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var gameID = 0;

            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                //データ作成
                var game = new Models.Game();

                //POST値セット
                this.TryUpdateModel(game);
                //エントリ情報セット
                base.SetEntryInfo(game);

                Context.Games.Add(game);

                await Context.SaveChangesAsync();

                gameID = game.GameID;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("/Order/Edit", new { gameID = gameID });

        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="game"></param>
        private void TryUpdateModel(Models.Game game)
        {
            game.TeamID = Game.TeamID;
            game.GameDate = Game.GameDate;
            game.GameClass = Game.GameClass;
            game.OpponentTeamName = Game.OpponentTeamName;
            game.StadiumName = Game.StadiumName;
            game.WeatherClass = Game.WeatherClass;
            game.BatFirstBatSecondClass = Game.BatFirstBatSecondClass;
            game.GameInputTypeClass = Game.GameInputTypeClass;
            game.StatusClass = StatusClass.BeforeGame;
            game.DeleteFLG = false;
        }
    }
}
