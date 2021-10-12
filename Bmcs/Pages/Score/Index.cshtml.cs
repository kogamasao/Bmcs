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
using System.Reflection;

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
        public ScorePageClass ScorePageClass { get; set; }

        [BindProperty]
        public string SelectTeamID { get; set; }

        [BindProperty]
        public bool IsPublic { get; set; }

        [BindProperty]
        public int? Year { get; set; }

        [BindProperty]
        public GameClass? GameClass { get; set; }

        [BindProperty]
        public TeamCategoryClass? TeamCategoryClass { get; set; }

        [BindProperty]
        public UseBallClass? UseBallClass { get; set; }

        [BindProperty]
        public int? RegulationGames { get; set; }

        [BindProperty]
        public int? RegulationInnings { get; set; }

        [BindProperty]
        public int? RegulationAtBatting { get; set; }

        [BindProperty]
        public bool IsIgnoreRegulationGames { get; set; }

        [BindProperty]
        public bool IsIgnoreRegulationInnings { get; set; }

        [BindProperty]
        public bool IsIgnoreRegulationAtBatting { get; set; }

        [BindProperty]
        public string SortItem { get; set; }

        public List<Models.GameScoreTeam> GameScoreTeamList { get; set; }

        public List<Models.GameScorePitcher> GameScorePitcherList { get; set; }

        public List<Models.GameScoreFielder> GameScoreFielderList { get; set; }

        public async Task<IActionResult> OnGetAsync(ScorePageClass scorePageClass, string teamID, bool isPublic, int? year, GameClass? gameClass, TeamCategoryClass? teamCategoryClass, UseBallClass? useBallClass, bool isIgnoreRegulationGames, bool isIgnoreRegulationInnings, bool isIgnoreRegulationAtBatting, string sortItem)
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
                      .Where(r => ((r.TeamID == teamID && !string.IsNullOrEmpty(teamID)) || (string.IsNullOrEmpty(teamID) && r.Team.PublicFLG == isPublic)) && r.StatusClass == StatusClass.EndGame && r.DeleteFLG == false)
                      .ToListAsync();

            //投手スコアデータ
            var gameScorePitcherList = await Context.GameScorePitchers
                      .Include(r => r.Game)
                      .Include(r => r.Team)
                      .Include(r => r.Member)
                      .Where(r => ((r.TeamID == teamID && !string.IsNullOrEmpty(teamID)) || (string.IsNullOrEmpty(teamID) && r.Team.PublicFLG == isPublic)) && r.Game.StatusClass == StatusClass.EndGame && r.Game.DeleteFLG == false)
                      .ToListAsync();

            //野手スコアデータ
            var gameScoreFielderList = await Context.GameScoreFielders
                      .Include(r => r.Game)
                      .Include(r => r.Team)
                      .Include(r => r.Member)
                      .Where(r => ((r.TeamID == teamID && !string.IsNullOrEmpty(teamID)) || (string.IsNullOrEmpty(teamID) && r.Team.PublicFLG == isPublic)) && r.Game.StatusClass == StatusClass.EndGame && r.Game.DeleteFLG == false)
                      .ToListAsync();

            //年初期値
            if (year == null && gameList.Any())
            {
                year = gameList.GroupBy(r => r.GameDate.Year).Select(r => new { Year = r.Key }).Max(r => r.Year);
            }

            //試合種別初期値
            if (gameClass == null)
            {
                gameClass = Enum.GameClass.All;
            }

            //カテゴリ初期値
            if (teamCategoryClass == null && MyTeam != null)
            {
                teamCategoryClass = MyTeam.TeamCategoryClass;
            }
            //全て
            else if (teamCategoryClass == null)
            {
                teamCategoryClass = Enum.TeamCategoryClass.All;
            }

            //使用球初期値
            if (useBallClass == null && MyTeam != null)
            {
                useBallClass = MyTeam.UseBallClass;
            }
            //全て
            else if (useBallClass == null)
            {
                useBallClass = Enum.UseBallClass.All;
            }

            //集計項目
            var totalingItem = new TotalingItem()
            {
                Year = year,
                GameClass = gameClass,
                TeamCategoryClass = teamCategoryClass,
                UseBallClass = useBallClass,
            };

            if(scorePageClass == ScorePageClass.Index
                || scorePageClass == ScorePageClass.Team)
            { 
                //チームスコア集計処理
                if (gameList != null && gameList.Any())
                {
                    GameScoreTeamList.AddRange(base.TotalingGameScoreTeam(gameList, gameScorePitcherList, gameScoreFielderList, totalingItem));
                }

                //ソート項目セット
                SetOrderValue(GameScoreTeamList, sortItem);
            }

            if (scorePageClass == ScorePageClass.Index
              || scorePageClass == ScorePageClass.Pitcher)
            {
                //投手スコア集計処理
                if (gameScorePitcherList.Any(r => !r.Member.DeleteFLG))
                {
                    GameScorePitcherList.AddRange(base.TotalingGameScorePitcher(gameScorePitcherList.Where(r => !r.Member.DeleteFLG).ToList(), totalingItem));
                }

                //ソート項目セット
                SetOrderValue(GameScorePitcherList, sortItem);
            }

            if (scorePageClass == ScorePageClass.Index
              || scorePageClass == ScorePageClass.Fielder)
            {
                //野手スコア集計処理
                if (gameScoreFielderList.Any(r => !r.Member.DeleteFLG))
                {
                    GameScoreFielderList.AddRange(base.TotalingGameScoreFielder(gameScoreFielderList.Where(r => !r.Member.DeleteFLG).ToList(), totalingItem));
                }

                //ソート項目セット
                SetOrderValue(GameScoreFielderList, sortItem);
            }

            //規定
            var regulation = GetRegulation(gameList, totalingItem);
            RegulationGames = regulation.RegulationGames;
            RegulationInnings = regulation.RegulationInnings;
            RegulationAtBatting = regulation.RegulationAtBatting;
            IsIgnoreRegulationGames = isIgnoreRegulationGames;
            IsIgnoreRegulationInnings = isIgnoreRegulationInnings;
            IsIgnoreRegulationAtBatting = isIgnoreRegulationAtBatting;

            //タイトル
            if (isPublic)
            {
                ViewData[ViewDataConstant.Title] = "公開チーム成績";
            }
            else
            {
                ViewData[ViewDataConstant.Title] = "チーム成績";
            }

            if (scorePageClass == ScorePageClass.Team)
            {
                ViewData[ViewDataConstant.Title] += "(チーム)";
            }
            else if (scorePageClass == ScorePageClass.Pitcher)
            {
                ViewData[ViewDataConstant.Title] += "(投手)";
            }
            else if (scorePageClass == ScorePageClass.Fielder)
            {
                ViewData[ViewDataConstant.Title] += "(野手)";
            }

            //引数セット
            ScorePageClass = scorePageClass;
            SelectTeamID = teamID;
            IsPublic = isPublic;
            Year = year;
            GameClass = gameClass;
            TeamCategoryClass = teamCategoryClass;
            UseBallClass = useBallClass;
            SortItem = sortItem;

            return Page();

        }

        private void SetOrderValue<T>(List<T> scoreList, string sortItem)
        {
            if (!string.IsNullOrEmpty(sortItem))
            {
                var propertyInfos = typeof(T).GetProperties();
                var sortItemPropertyInfo = propertyInfos.FirstOrDefault(r => r.Name == sortItem);
                var orderValuePropertyInfo = propertyInfos.FirstOrDefault(r => r.Name == nameof(GameScoreTeam.OrderValue));

                if (sortItemPropertyInfo != null && orderValuePropertyInfo != null)
                {
                    foreach (var score in scoreList)
                    {
                        if(((typeof(T) == typeof(GameScoreTeam))
                            && (sortItem == nameof(GameScoreTeam.EarnedRunAverage)
                                || sortItem == nameof(GameScoreTeam.PitcherBattingAverage)
                                || sortItem == nameof(GameScoreTeam.PitcherScoringPositionBattingAverage)
                                || sortItem == nameof(GameScoreTeam.Whip)))
                            ||
                            ((typeof(T) == typeof(GameScorePitcher))
                            && (sortItem == nameof(GameScorePitcher.EarnedRunAverage)
                                || sortItem == nameof(GameScorePitcher.BattingAverage)
                                || sortItem == nameof(GameScorePitcher.ScoringPositionBattingAverage)
                                || sortItem == nameof(GameScorePitcher.Whip)))
                        )
                        {
                            orderValuePropertyInfo.SetValue(score, (-1) * System.Convert.ToDecimal(sortItemPropertyInfo.GetValue(score)));
                        }
                        else
                        {
                            orderValuePropertyInfo.SetValue(score, System.Convert.ToDecimal(sortItemPropertyInfo.GetValue(score)));
                        }
                    }
                }
            }
        }
    }
}
