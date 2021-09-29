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
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public IList<Models.InningScore> InningScoreTopList { get; set; }

        [BindProperty]
        public IList<Models.InningScore> InningScoreButtomList { get; set; }

        [BindProperty]
        public IList<Models.GameScorePitcher> GameScorePitcherList { get; set; }

        [BindProperty]
        public IList<Models.GameScoreFielder> GameScoreFielderList { get; set; }

        [BindProperty]
        public Models.Game Game { get; set; }

        [BindProperty]
        public GameScoreSubmitClass? GameScoreSubmitClass { get; set; }

        public async Task<IActionResult> OnGetAsync(int? gameID)
        {
            if (!base.IsLogin())
            {
                return NotFound();
            }

            if (gameID == null)
            {
                return NotFound();
            }

            Game = await Context.Games
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == gameID);

            //試合データなし、管理者以外でマイチームでない、試合前以外でスタメン修正
            if (Game == null
                || (Game.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID)
                    && !base.IsAdmin())
                )
            {
                return NotFound();
            }

            //チームID
            base.TeamID = Game.TeamID;

            //イニング表
            InningScoreTopList = await Context.InningScores
                        .Where(r => r.GameID == Game.GameID && r.TopButtomClass == TopButtomClass.Top)
                        .OrderBy(r => r.Inning)
                        .ThenBy(r => r.TopButtomClass)
                        .ToListAsync();

            if(InningScoreTopList == null || !InningScoreTopList.Any())
            { 
                for(int i = InningScoreTopList.Count(); i < 9; i++)
                {
                    InningScoreTopList.Add(new Models.InningScore()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        Inning = i + 1,
                        TopButtomClass = TopButtomClass.Top,
                        Score = null
                    });
                }
            }

            //イニング裏
            InningScoreButtomList = await Context.InningScores
                              .Where(r => r.GameID == Game.GameID && r.TopButtomClass == TopButtomClass.Buttom)
                              .OrderBy(r => r.Inning)
                              .ThenBy(r => r.TopButtomClass)
                              .ToListAsync();

            if (InningScoreButtomList == null || !InningScoreButtomList.Any())
            {
                for (int i = InningScoreButtomList.Count(); i < 8; i++)
                {
                    InningScoreButtomList.Add(new Models.InningScore()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        Inning = i + 1,
                        TopButtomClass = TopButtomClass.Buttom,
                        Score = null
                    });
                }
            }

            //表のみ
            if(InningScoreTopList.Count() != InningScoreButtomList.Count())
            {
                InningScoreButtomList.Add(new Models.InningScore()
                {
                    GameID = Game.GameID,
                    TeamID = Game.TeamID,
                    Inning = InningScoreTopList.Select(r => r.Inning).Max(),
                    TopButtomClass = TopButtomClass.Buttom,
                    Score = null
                });
            }

            //投手スコア
            GameScorePitcherList = await Context.GameScorePitchers
                      .Where(r => r.GameID == Game.GameID)
                      .OrderBy(r => r.ScoreIndex)
                      .ToListAsync();        

            //データなし
            if (!GameScorePitcherList.Any())
            {
                GameScorePitcherList.Add(new GameScorePitcher());
            }

            //勝敗HS
            foreach(var gameScorePitcher in GameScorePitcherList)
            {
                if(gameScorePitcher.Win == 1)
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
                      .Where(r => r.GameID == Game.GameID)
                      .OrderBy(r => r.ScoreIndex)
                      .ToListAsync();

            //データなし
            if (!GameScoreFielderList.Any())
            {
                GameScoreFielderList.Add(new GameScoreFielder());
            }

            //タイトル
            ViewData[ViewDataConstant.Title] = "試合結果";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                //再取得
                Game = await Context.Games
                    .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == Game.GameID);

                //チームID
                base.TeamID = Game.TeamID;

                if (GameScoreSubmitClass == Enum.GameScoreSubmitClass.Fix)
                {
                    using (var tran = await Context.Database.BeginTransactionAsync())
                    {
                   
                        if (!ModelState.IsValid)
                        {
                            return Page();
                        }

                        //削除対象イニングスコア
                        var deleteInningScores = await Context.InningScores
                                                        .Where(r => r.GameID == Game.GameID).ToListAsync();

                        Context.InningScores.RemoveRange(deleteInningScores);

                        //削除対象試合投手スコア
                        var deleteGameScorePitchers = await Context.GameScorePitchers
                                                        .Where(r => r.GameID == Game.GameID).ToListAsync();

                        Context.GameScorePitchers.RemoveRange(deleteGameScorePitchers);

                        //削除対象試合野手スコア
                        var deleteGameScoreFielders = await Context.GameScoreFielders
                                                        .Where(r => r.GameID == Game.GameID).ToListAsync();

                        Context.GameScoreFielders.RemoveRange(deleteGameScoreFielders);

                        await Context.SaveChangesAsync();

                        //POST値セット
                        TryUpdateModel();

                        await Context.SaveChangesAsync();

                        await tran.CommitAsync();
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (GameScoreSubmitClass == Enum.GameScoreSubmitClass.ReCount)
            {
                try
                {
                    //再集計
                    await base.GameSet(Game.GameID);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

                return RedirectToPage("/GameScore/Edit", new { gameID = Game.GameID });
            }
            else
            {
                //試合一覧へ
                return RedirectToPage("/Game/Index");
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        private void TryUpdateModel()
        {
            //試合
            var game = Context.Games.Find(Game.GameID);
            
            //イニングスコア
            var inningScores = new List<Models.InningScore>();

            foreach(var x in InningScoreTopList.Select((inningScoreTop, index) => new { inningScoreTop, index }))
            {
                x.inningScoreTop.TopButtomClass = TopButtomClass.Top;
                x.inningScoreTop.Inning = x.index + 1;
            }

            foreach (var x in InningScoreButtomList.Select((inningScoreButtom, index) => new { inningScoreButtom, index }))
            {
                x.inningScoreButtom.TopButtomClass = TopButtomClass.Buttom;
                x.inningScoreButtom.Inning = x.index + 1;
            }

            //イニング
            foreach (var inningScore in InningScoreTopList.Union(InningScoreButtomList))
            {
                var newInningScore = new Models.InningScore()
                {
                    GameID = game.GameID,
                    TeamID = game.TeamID,
                    Inning = inningScore.Inning,
                    TopButtomClass = inningScore.TopButtomClass,
                    Score = inningScore.Score,
                };

                base.SetEntryInfo(newInningScore);

                inningScores.Add(newInningScore);
            }

            //最終イニング
            var maxInning = inningScores.DefaultIfEmpty().Max(r => r.Inning);

            foreach (var inningScore in inningScores)
            {
                if (inningScore.Inning == maxInning)
                {
                    inningScore.LastInningFLG = true;

                    //表
                    if (inningScore.TopButtomClass == TopButtomClass.Top)
                    {
                        inningScore.Score = inningScore.Score.NullToZero();
                    }
                }
                else
                {
                    inningScore.LastInningFLG = false;
                    inningScore.Score = inningScore.Score.NullToZero();
                }
            }

            Context.InningScores.AddRange(inningScores);

            //表裏
            var myTeamOffenceTopButtomClass = game.BatFirstBatSecondClass == BatFirstBatSecondClass.First ? TopButtomClass.Top : TopButtomClass.Buttom;

            //得点
            game.Score = inningScores.Where(r => r.TopButtomClass == myTeamOffenceTopButtomClass).DefaultIfEmpty().Sum(r => r.Score);
            game.OpponentTeamScore = inningScores.Where(r => r.TopButtomClass != myTeamOffenceTopButtomClass).DefaultIfEmpty().Sum(r => r.Score);

            //勝敗
            if (game.Score > game.OpponentTeamScore)
            {
                game.WinLoseClass = WinLoseClass.Win;
            }
            else if (game.Score < game.OpponentTeamScore)
            {
                game.WinLoseClass = WinLoseClass.Lose;
            }
            else
            {
                game.WinLoseClass = WinLoseClass.Draw;
            }

            //試合終了
            game.StatusClass = StatusClass.EndGame;

            SetUpdateInfo(game);

            //投手スコア
            foreach (var x in GameScorePitcherList.Where(r => r.MemberID != null).Select((gameScorePitcher, index) => new { gameScorePitcher, index }))
            {
                var newGameScorePitcher = new GameScorePitcher()
                {
                    GameID = game.GameID,
                    TeamID = game.TeamID,
                    ScoreIndex = x.index + 1,
                    MemberID = x.gameScorePitcher.MemberID,
                    Detail = x.gameScorePitcher.Detail,
                    Win = x.gameScorePitcher.GameScorePitcherClass == GameScorePitcherClass.Win ? 1 : 0,
                    Lose = x.gameScorePitcher.GameScorePitcherClass == GameScorePitcherClass.Lose ? 1 : 0,
                    Hold = x.gameScorePitcher.GameScorePitcherClass == GameScorePitcherClass.Hold ? 1 : 0,
                    Save = x.gameScorePitcher.GameScorePitcherClass == GameScorePitcherClass.Save ? 1 : 0,
                    Starter = x.gameScorePitcher.Starter.NullToZero(),
                    CompleteGame = x.gameScorePitcher.CompleteGame.NullToZero(),
                    Inning = x.gameScorePitcher.Inning.NullToZero(),
                    PlateAppearance = x.gameScorePitcher.PlateAppearance.NullToZero(),
                    AtBat = x.gameScorePitcher.AtBat.NullToZero(),
                    Hit = x.gameScorePitcher.Hit.NullToZero(),
                    HomeRun = x.gameScorePitcher.HomeRun.NullToZero(),
                    Run = x.gameScorePitcher.Run.NullToZero(),
                    EarnedRun = x.gameScorePitcher.EarnedRun.NullToZero(),
                    FourBall = x.gameScorePitcher.FourBall.NullToZero(),
                    DeadBall = x.gameScorePitcher.DeadBall.NullToZero(),
                    ScoringPositionPlateAppearance = x.gameScorePitcher.ScoringPositionPlateAppearance.NullToZero(),
                    ScoringPositionAtBat = x.gameScorePitcher.ScoringPositionAtBat.NullToZero(),
                    ScoringPositionHit = x.gameScorePitcher.ScoringPositionHit.NullToZero(),
                    StrikeOut = x.gameScorePitcher.StrikeOut.NullToZero(),
                    PickOffBallOut = x.gameScorePitcher.PickOffBallOut.NullToZero(),
                    WildPitch = x.gameScorePitcher.WildPitch.NullToZero(),
                    Balk = x.gameScorePitcher.Balk.NullToZero(),
                };

                SetEntryInfo(newGameScorePitcher);

                Context.GameScorePitchers.Add(newGameScorePitcher);
            }

            //野手スコア
            foreach (var x in GameScoreFielderList.Where(r => r.MemberID != null).Select((gameScoreFielder, index) => new { gameScoreFielder, index }))
            {
                var newGameScoreFielder = new GameScoreFielder()
                {
                    GameID = game.GameID,
                    TeamID = game.TeamID,
                    ScoreIndex = x.index + 1,
                    MemberID = x.gameScoreFielder.MemberID,
                    Detail = x.gameScoreFielder.Detail,
                    PlateAppearance = x.gameScoreFielder.PlateAppearance.NullToZero(),
                    AtBat = x.gameScoreFielder.AtBat.NullToZero(),
                    Hit = x.gameScoreFielder.Hit.NullToZero(),
                    DoubleHit = x.gameScoreFielder.DoubleHit.NullToZero(),
                    TripleHit = x.gameScoreFielder.TripleHit.NullToZero(),
                    HomeRun = x.gameScoreFielder.HomeRun.NullToZero(),
                    TotalBase = (x.gameScoreFielder.HomeRun.NullToZero() * 4)
                                + (x.gameScoreFielder.TripleHit.NullToZero() * 3)
                                + (x.gameScoreFielder.DoubleHit.NullToZero() * 2)
                                + (x.gameScoreFielder.Hit.NullToZero() - x.gameScoreFielder.DoubleHit.NullToZero() - x.gameScoreFielder.TripleHit.NullToZero() - x.gameScoreFielder.HomeRun.NullToZero()),
                    RBI = x.gameScoreFielder.RBI.NullToZero(),
                    Run = x.gameScoreFielder.Run.NullToZero(),
                    StolenBasePlan = x.gameScoreFielder.StolenBasePlan.NullToZero(),
                    StolenBase = x.gameScoreFielder.StolenBase.NullToZero(),
                    FourBall = x.gameScoreFielder.FourBall.NullToZero(),
                    DeadBall = x.gameScoreFielder.DeadBall.NullToZero(),
                    Sacrifice = x.gameScoreFielder.Sacrifice.NullToZero(),
                    SacrificeFly = x.gameScoreFielder.SacrificeFly.NullToZero(),
                    LeftOnBase = x.gameScoreFielder.LeftOnBase.NullToZero(),
                    ScoringPositionPlateAppearance = x.gameScoreFielder.ScoringPositionPlateAppearance.NullToZero(),
                    ScoringPositionAtBat = x.gameScoreFielder.ScoringPositionAtBat.NullToZero(),
                    ScoringPositionHit = x.gameScoreFielder.ScoringPositionHit.NullToZero(),
                    StrikeOut = x.gameScoreFielder.StrikeOut.NullToZero(),
                    DoublePlay = x.gameScoreFielder.DoublePlay.NullToZero(),
                    Error = x.gameScoreFielder.Error.NullToZero(),
                    StolenBasePlaned = x.gameScoreFielder.StolenBasePlaned.NullToZero(),
                    StopStolenBase = x.gameScoreFielder.StopStolenBase.NullToZero(),
                    Assist = x.gameScoreFielder.Assist.NullToZero(),
                    OwnError = x.gameScoreFielder.OwnError.NullToZero(),
                    PassBall = x.gameScoreFielder.PassBall.NullToZero(),
                };

                SetEntryInfo(newGameScoreFielder);

                Context.GameScoreFielders.Add(newGameScoreFielder);
            }
        }
    }

}
