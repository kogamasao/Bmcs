using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Enum;
using Bmcs.Function;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;

namespace Bmcs.Pages.Score
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }


        [BindProperty]
        public Models.Team Team { get; set; }

        [BindProperty]
        public string SelectTeamID { get; set; }

        [BindProperty]
        public bool IsPublic { get; set; }

        [BindProperty]
        public int? Year { get; set; }

        [BindProperty]
        public GameClass? GameClass { get; set; }

        [BindProperty]
        public int? RegulationInnings { get; set; }

        [BindProperty]
        public int? RegulationAtBatting { get; set; }

        public List<Models.GameScoreTeam> GameScoreTeamList { get; set; }

        public List<Models.GameScorePitcher> GameScorePitcherList { get; set; }

        public List<Models.GameScoreFielder> GameScoreFielderList { get; set; }



        public async Task<IActionResult> OnGetAsync(string teamID = "", bool isPublic = false, int? year = null, GameClass? gameClass = null)
        {
            if (string.IsNullOrEmpty(teamID) && !isPublic)
            {
                teamID = HttpContext.Session.GetString(SessionConstant.TeamID);
            }

            if (!string.IsNullOrEmpty(teamID))
            {
                Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

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
            }

            //チームスコア
            GameScoreTeamList = new List<GameScoreTeam>();
            //投手スコア
            GameScorePitcherList = new List<GameScorePitcher>();
            //野手スコア
            GameScoreFielderList = new List<GameScoreFielder>();

            //試合データ
            var gameList = await Context.Games
                      .Include(r => r.Team)
                      .Where(r => ((r.TeamID == teamID && teamID != string.Empty) || (teamID == string.Empty && r.Team.PublicFLG == isPublic)) && r.StatusClass == StatusClass.EndGame && r.DeleteFLG == false)
                      .ToListAsync();

            //投手スコアデータ
            var gameScorePitcherList = await Context.GameScorePitchers
                      .Include(r => r.Game)
                      .Include(r => r.Team)
                      .Include(r => r.Member)
                      .Where(r => ((r.TeamID == teamID && teamID != string.Empty) || (teamID == string.Empty && r.Team.PublicFLG == isPublic)) && r.Game.StatusClass == StatusClass.EndGame && r.Game.DeleteFLG == false)
                      .ToListAsync();

            //野手スコアデータ
            var gameScoreFielderList = await Context.GameScoreFielders
                      .Include(r => r.Game)
                      .Include(r => r.Team)
                      .Include(r => r.Member)
                      .Where(r => ((r.TeamID == teamID && teamID != string.Empty) || (teamID == string.Empty && r.Team.PublicFLG == isPublic)) && r.Game.StatusClass == StatusClass.EndGame && r.Game.DeleteFLG == false)
                      .ToListAsync();

            //年初期値
            if(year == null)
            {
                year = gameList.GroupBy(r => r.GameDate.Year).Select(r => new { Year = r.Key }).Max(r => r.Year);
            }
            //通算
            else if (year == 0)
            {
                year = null;
            }

            //集計項目
            var totalingItem = new TotalingItem()
            {
                Year = year,
                GameClass = gameClass,
            };

            //チームスコア集計処理
            if (gameList != null)
            {
                GameScoreTeamList.AddRange(base.TotalingGameScoreTeam(gameList, gameScorePitcherList, gameScoreFielderList, totalingItem));
            }

            //投手スコア集計処理
            if (gameScorePitcherList.Any(r => !r.Member.DeleteFLG))
            {
                GameScorePitcherList.AddRange(base.TotalingGameScorePitcher(gameScorePitcherList.Where(r => !r.Member.DeleteFLG).ToList(), totalingItem));
            }

            //野手スコア集計処理
            if (gameScoreFielderList.Any(r => !r.Member.DeleteFLG))
            {
                GameScoreFielderList.AddRange(base.TotalingGameScoreFielder(gameScoreFielderList.Where(r => !r.Member.DeleteFLG).ToList(), totalingItem));
            }

            //規定値
            var regulationValue = GetRegulationValue(gameList, totalingItem);
            RegulationInnings = regulationValue.RegulationInnings;
            RegulationAtBatting = regulationValue.RegulationAtBatting;

            //タイトル
            if (isPublic)
            {
                ViewData[ViewDataConstant.Title] = "公開チーム成績";
            }
            else
            {
                ViewData[ViewDataConstant.Title] = "チーム成績";
            }

            //引数セット
            SelectTeamID = teamID;
            IsPublic = isPublic;
            Year = year;
            GameClass = gameClass;

            return Page();

        }

    }
}
