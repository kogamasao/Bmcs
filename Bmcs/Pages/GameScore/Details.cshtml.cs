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
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;
using Bmcs.Function;

namespace Bmcs.Pages.GameScore
{
    public class DetailsModel : PageModelBase<EditModel>
    {
        public DetailsModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public IList<Models.InningScore> InningScoreList { get; set; }

        [BindProperty]
        public IList<Models.GameScorePitcher> GameScorePitcherList { get; set; }

        [BindProperty]
        public IList<Models.GameScoreFielder> GameScoreFielderList { get; set; }

        [BindProperty]
        public Models.Game Game { get; set; }

        public async Task<IActionResult> OnGetAsync(int? gameID)
        {
            if (gameID == null)
            {
                return NotFound();
            }

            Game = await Context.Games
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == gameID);

            if (Game == null)
            {
                return NotFound();
            }

            //管理者以外で、削除されている、非公開チーム
            if (!base.IsAdmin()
               && (Game.Team.DeleteFLG == true
                   || (Game.Team.PublicFLG == false && Game.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID))
                   || Game.DeleteFLG == true
                   )
               )
            {
                return NotFound();
            }

            //チームID
            base.TeamID = Game.TeamID;

            //イニングスコア
            InningScoreList = await Context.InningScores
                        .Where(r => r.GameID == Game.GameID)
                        .OrderBy(r => r.Inning)
                        .ThenBy(r => r.TopButtomClass)
                        .ToListAsync();

            //投手スコア
            GameScorePitcherList = await Context.GameScorePitchers
                      .Include(r => r.Member)
                      .Where(r => r.GameID == Game.GameID)
                      .OrderBy(r => r.ScoreIndex)
                      .ToListAsync();

            //勝敗HS
            foreach (var gameScorePitcher in GameScorePitcherList)
            {
                if (gameScorePitcher.Win == 1)
                {
                    gameScorePitcher.GameScorePitcherClass = GameScorePitcherClass.Win;
                }
                else if (gameScorePitcher.Lose == 1)
                {
                    gameScorePitcher.GameScorePitcherClass = GameScorePitcherClass.Lose;
                }
                else if (gameScorePitcher.Hold == 1)
                {
                    gameScorePitcher.GameScorePitcherClass = GameScorePitcherClass.Hold;
                }
                else if (gameScorePitcher.Save == 1)
                {
                    gameScorePitcher.GameScorePitcherClass = GameScorePitcherClass.Save;
                }
            }

            //野手スコア
            GameScoreFielderList = await Context.GameScoreFielders
                      .Include(r => r.Member)
                      .Where(r => r.GameID == Game.GameID)
                      .OrderBy(r => r.ScoreIndex)
                      .ToListAsync();

            //タイトル
            ViewData[ViewDataConstant.Title] = "試合結果詳細";

            return Page();
        }
    }
}
