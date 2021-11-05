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

namespace Bmcs.Pages.Team
{
    public class DetailsModel : PageModelBase<DetailsModel>
    {
        public DetailsModel(ILogger<DetailsModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public Models.Team Team { get; set; }

        public List<Models.GameScoreTeam> GameScoreTeamList { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == id);

            if (Team == null)
            {
                return NotFound();
            }

            if (!base.IsAdmin()
                && (Team.DeleteFLG == true
                    || (Team.PublicFLG == false && Team.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID))
                    )
                )
            {
                return NotFound();
            }

            //集計項目
            var totalingItem = new TotalingItem()
            {
                Year = 0,
                GameClass = GameClass.All,
                TeamCategoryClass = TeamCategoryClass.All,
                UseBallClass = UseBallClass.All,
            };

            //チームスコア
            GameScoreTeamList = new List<GameScoreTeam>();

            //試合データ
            var gameList = await Context.Games
                      .Include(r => r.Team)
                      .Where(r => r.TeamID == id && r.StatusClass == StatusClass.EndGame && r.DeleteFLG == false)
                      .ToListAsync();

            //投手スコアデータ
            var gameScorePitcherList = await Context.GameScorePitchers
                      .Include(r => r.Game)
                      .Include(r => r.Team)
                      .Where(r => r.TeamID == id && r.Game.StatusClass == StatusClass.EndGame && r.Game.DeleteFLG == false)
                      .ToListAsync();

            //野手スコアデータ
            var gameScoreFielderList = await Context.GameScoreFielders
                      .Include(r => r.Game)
                      .Include(r => r.Team)
                      .Where(r => r.TeamID == id && r.Game.StatusClass == StatusClass.EndGame && r.Game.DeleteFLG == false)
                      .ToListAsync();

            if (gameList != null)
            {
                //通算集計処理
                GameScoreTeamList.AddRange(base.TotalingGameScoreTeam(gameList, gameScorePitcherList, gameScoreFielderList, totalingItem));

                foreach (var year in gameList.GroupBy(r => r.GameDate.Year).Select(r => r.Key))
                {
                    //対象年
                    totalingItem.Year = year;
                    //対象年集計
                    GameScoreTeamList.AddRange(base.TotalingGameScoreTeam(gameList, gameScorePitcherList, gameScoreFielderList, totalingItem));
                }
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TeamDetails);
            //インデックス
            IsIndex = true;

            return Page();
        }
    }
}
