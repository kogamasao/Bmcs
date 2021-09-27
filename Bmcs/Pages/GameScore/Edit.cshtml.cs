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

            //投手オーダー
            var tempPitcherOrderList = await Context.Orders
                       .Include(r => r.GameScene)
                       .Where(r => r.GameID == Game.GameID && r.PositionClass == PositionClass.Pitcher && r.GameSceneID != null)
                       .Select(r => new { MemberID = r.MemberID, Inning = r.GameScene.Inning, InningIndex = r.GameScene.InningIndex })
                       .ToListAsync();

            var pitcherOrderList = tempPitcherOrderList
                        .GroupBy(r => r.MemberID)
                        .Select(r => new { MemberID = r.Key, Inning = r.Min(s => s.Inning), InningIndex = r.Min(s => s.InningIndex) })
                        .ToList();

            //投手スコア
            GameScorePitcherList = new List<Models.GameScorePitcher>();

            var tempGameScorePitcherList = await Context.GameScorePitchers
                      .Where(r => r.GameID == Game.GameID)
                      .ToListAsync();

            //先発
            var starterPitcher = tempGameScorePitcherList.Where(r => r.Starter > 0).FirstOrDefault();

            if(starterPitcher != null)
            {
                GameScorePitcherList.Add(starterPitcher);
            }

            //リリーフ登板順
            foreach (var order in pitcherOrderList.OrderBy(r => r.Inning).ThenBy(r => r.InningIndex))
            {
                var gameScorePitcher = tempGameScorePitcherList.Where(r => r.MemberID == order.MemberID && r.Starter == 0).FirstOrDefault();

                if (gameScorePitcher != null)
                {
                    GameScorePitcherList.Add(gameScorePitcher);
                }
            }

            //野手オーダー
            var fielderOrderList = await Context.Orders
                      .Where(r => r.GameID == Game.GameID && r.BattingOrder != null)
                        .GroupBy(r => r.MemberID)
                        .Select(r => new { MemberID = r.Key, BattingOrder = r.Min(s => s.BattingOrder), ParticipationIndex = r.Min(s => s.ParticipationIndex) })
                      .ToListAsync();

            //野手スコア
            GameScoreFielderList = new List<Models.GameScoreFielder>();

            var tempGameScoreFielderList = await Context.GameScoreFielders
                      .Where(r => r.GameID == Game.GameID)
                      .ToListAsync();

            //打順順
            foreach (var order in fielderOrderList.OrderBy(r => r.BattingOrder).ThenBy(r => r.ParticipationIndex))
            {
                var gameScoreFielder = tempGameScoreFielderList.Where(r => r.MemberID == order.MemberID).FirstOrDefault();

                if(gameScoreFielder != null)
                {
                    GameScoreFielderList.Add(gameScoreFielder);
                }
            }

            //タイトル
            ViewData[ViewDataConstant.Title] = "試合結果";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                using (var tran = await Context.Database.BeginTransactionAsync())
                {
                    //再取得
                    Game = await Context.Games
                        .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == Game.GameID);

                    //チームID
                    base.TeamID = Game.TeamID;

                    if (!ModelState.IsValid)
                    {
                        return Page();
                    }

                    //削除対象試合シーン
                    var deleteGameSceneList = await Context.GameScenes
                                            .Where(r => r.GameID == Game.GameID).ToListAsync();

                    Context.GameScenes.RemoveRange(deleteGameSceneList);

                    var deleteGameSceneDetailList = new List<Models.GameSceneDetail>();
                    var deleteGameSceneRunnerList = new List<Models.GameSceneRunner>();
                    var deleteOrderList = new List<Models.Order>();


                    await Context.SaveChangesAsync();

                    //データ作成
                    var gameScene = new Models.GameScene();
                    gameScene.GameSceneDetails = new List<Models.GameSceneDetail>();
                    gameScene.GameSceneRunners = new List<Models.GameSceneRunner>();
                    gameScene.Orders = new List<Models.Order>();

                    //POST値セット
                    TryUpdateModel(gameScene);

               

                    await tran.CommitAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (GameScoreSubmitClass == Enum.GameScoreSubmitClass.ReCount)
            {
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
        /// <param name="gameScene"></param>
        private void TryUpdateModel(Models.GameScene gameScene)
        {
            ////試合シーン
            //gameScene.GameID = Game.GameID;
            //gameScene.TeamID = Game.TeamID;
            //gameScene.Inning = GameScene.Inning;
            //gameScene.TopButtomClass = GameScene.TopButtomClass;
            //gameScene.InningIndex = GameScene.InningIndex;
            //gameScene.BattingOrder = GameScene.BattingOrder;
            //gameScene.OffenseDefenseClass = GameScene.OffenseDefenseClass;
            //gameScene.PitcherMemberID = GameScene.PitcherMemberID;
            //gameScene.BatterMemberID = GameScene.BatterMemberID;
            //gameScene.HittingDirectionClass = GameScene.HittingDirectionClass;
            //gameScene.HitBallClass = GameScene.HitBallClass;
            //gameScene.ResultClass = GameScene.ResultClass;

            //base.SetEntryInfo(gameScene);

            ////試合詳細シーン
            //foreach (var gameSceneDetail in BeforeGameSceneDetailList.Where(r => r.DetailResultClass != null))
            //{
            //    var newGameSceneDetail = new Models.GameSceneDetail();

            //    newGameSceneDetail.GameID = gameScene.GameID;
            //    newGameSceneDetail.TeamID = gameScene.TeamID;
            //    newGameSceneDetail.MemberID = gameSceneDetail.MemberID;
            //    newGameSceneDetail.SceneResultClass = SceneResultClass.SceneChange;
            //    newGameSceneDetail.DetailResultClass = gameSceneDetail.DetailResultClass;

            //    base.SetEntryInfo(newGameSceneDetail);

            //    gameScene.GameSceneDetails.Add(newGameSceneDetail);
            //}

            ////試合ランナーシーン
            //foreach (var gameSceneRunner in BeforeGameSceneRunnerList)
            //{
            //    var newGameSceneRunner = new Models.GameSceneRunner();

            //    newGameSceneRunner.GameID = gameScene.GameID;
            //    newGameSceneRunner.TeamID = gameScene.TeamID;
            //    newGameSceneRunner.MemberID = gameSceneRunner.MemberID;
            //    newGameSceneRunner.BattingOrder = gameSceneRunner.BattingOrder;
            //    newGameSceneRunner.SceneResultClass = SceneResultClass.SceneChange;
            //    newGameSceneRunner.BeforeRunnerClass = gameSceneRunner.BeforeRunnerClass;
            //    newGameSceneRunner.RunnerClass = gameSceneRunner.RunnerClass;
            //    newGameSceneRunner.RunnerResultClass = gameSceneRunner.RunnerResultClass;

            //    base.SetEntryInfo(newGameSceneRunner);

            //    gameScene.GameSceneRunners.Add(newGameSceneRunner);
            //}

            ////試合シーンアウト＆ランナー
            //gameScene.OutCount = GameScene.OutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange && r.RunnerResultClass == RunnerResultClass.Out).Count();
            //gameScene.RunnerSceneClass = GetRunnerSceneClass(gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange));

            ////試合詳細結果
            //foreach (var gameSceneDetail in AfterGameSceneDetailList.Where(r => r.DetailResultClass != null))
            //{
            //    var newGameSceneDetail = new Models.GameSceneDetail();

            //    newGameSceneDetail.GameID = gameScene.GameID;
            //    newGameSceneDetail.TeamID = gameScene.TeamID;
            //    newGameSceneDetail.MemberID = gameSceneDetail.MemberID;
            //    newGameSceneDetail.SceneResultClass = SceneResultClass.Result;
            //    newGameSceneDetail.DetailResultClass = gameSceneDetail.DetailResultClass;

            //    base.SetEntryInfo(newGameSceneDetail);

            //    gameScene.GameSceneDetails.Add(newGameSceneDetail);
            //}

            ////試合ランナー結果
            //foreach (var gameSceneRunner in AfterGameSceneRunnerList)
            //{
            //    var newGameSceneRunner = new Models.GameSceneRunner();

            //    newGameSceneRunner.GameID = gameScene.GameID;
            //    newGameSceneRunner.TeamID = gameScene.TeamID;
            //    newGameSceneRunner.MemberID = gameSceneRunner.MemberID;
            //    newGameSceneRunner.BattingOrder = gameSceneRunner.BattingOrder;
            //    newGameSceneRunner.SceneResultClass = SceneResultClass.Result;
            //    newGameSceneRunner.BeforeRunnerClass = gameSceneRunner.BeforeRunnerClass;
            //    newGameSceneRunner.RunnerClass = gameSceneRunner.RunnerClass;
            //    newGameSceneRunner.RunnerResultClass = gameSceneRunner.RunnerResultClass;

            //    base.SetEntryInfo(newGameSceneRunner);

            //    gameScene.GameSceneRunners.Add(newGameSceneRunner);
            //}

            ////試合シーン結果アウト＆ランナー
            //gameScene.ResultOutCount = GameScene.OutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result && r.RunnerResultClass == RunnerResultClass.Out).Count();
            //gameScene.ResultRunnerSceneClass = GetRunnerSceneClass(gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result));

            //if(gameScene.ResultClass == ResultClass.Change)
            //{ 
            //    //得点
            //    gameScene.Run = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
            //                                                    && (r.RunnerResultClass == RunnerResultClass.Run
            //                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptRBI
            //                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun
            //                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptRBIEarnedRun)).Count();
            //    //打点
            //    gameScene.RBI = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
            //                                                && (r.RunnerResultClass == RunnerResultClass.Run
            //                                                 || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun)).Count();
            //    //自責点
            //    gameScene.EarnedRun = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
            //                                                    && (r.RunnerResultClass == RunnerResultClass.Run
            //                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptRBI)).Count();
            //}
            //else
            //{
            //    //得点
            //    gameScene.Run = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
            //                                                    && (r.RunnerResultClass == RunnerResultClass.Run
            //                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptRBI
            //                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun
            //                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptRBIEarnedRun)).Count();
            //    //打点
            //    gameScene.RBI = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
            //                                                && (r.RunnerResultClass == RunnerResultClass.Run
            //                                                 || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun)).Count();
            //    //自責点
            //    gameScene.EarnedRun = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
            //                                                    && (r.RunnerResultClass == RunnerResultClass.Run
            //                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptRBI)).Count();
            //}

            ////チェンジ
            //if (GameSceneSubmitClass != Enum.GameSceneSubmitClass.NextBatter)
            //{
            //    gameScene.ChangeFLG = true;
            //}

            ////オーダー
            //var beforeOrders = Context.Orders
            //                    .Where(r => r.GameID == gameScene.GameID
            //                        && r.OrderDataClass == OrderDataClass.Change).ToList();

            ////変更オーダーなし
            //if (!beforeOrders.Any())
            //{
            //    var lastGameSceneID = GetLastGameSceneID(gameScene.GameID, GameScene.GameSceneID.ZeroToNull());

            //    beforeOrders = Context.Orders
            //                    .Where(r => r.GameID == gameScene.GameID
            //                        && r.GameSceneID == lastGameSceneID
            //                        && r.OrderDataClass == OrderDataClass.Normal).ToList();
            //}

            ////オーダー作成
            //foreach (var order in beforeOrders)
            //{
            //    var newOrder = new Models.Order();

            //    newOrder.GameID = order.GameID;
            //    newOrder.TeamID = order.TeamID;
            //    newOrder.MemberID = order.MemberID;
            //    newOrder.BattingOrder = order.BattingOrder;
            //    newOrder.ParticipationIndex = order.ParticipationIndex;
            //    newOrder.PositionClass = order.PositionClass;
            //    newOrder.ParticipationClass = order.ParticipationClass;
            //    newOrder.OrderDataClass = OrderDataClass.Normal;

            //    //代打、代走
            //    if (gameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
            //    {
            //        //シーン前ランナーID
            //        int? runnerID = null;

            //        var gameSceneRunner = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
            //                                                        && r.BattingOrder == order.BattingOrder).FirstOrDefault();

            //        if (gameSceneRunner != null)
            //        {
            //            runnerID = gameSceneRunner.MemberID;
            //        }

            //        //代走
            //        if (runnerID != null
            //            && newOrder.MemberID != runnerID)
            //        {
            //            newOrder.MemberID = runnerID;
            //            newOrder.ParticipationIndex = order.ParticipationIndex + 1;
            //            newOrder.ParticipationClass = ParticipationClass.PinchRunner;
            //        }

            //        //代打
            //        if (gameScene.BattingOrder == order.BattingOrder
            //            && gameScene.BatterMemberID != order.MemberID)
            //        {
            //            newOrder.MemberID = gameScene.BatterMemberID;
            //            newOrder.ParticipationIndex = order.ParticipationIndex + 1;
            //            newOrder.ParticipationClass = ParticipationClass.PinchHitter;
            //        }

            //        //シーン後ランナーID
            //        runnerID = null;

            //        gameSceneRunner = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
            //                                                       && r.BattingOrder == order.BattingOrder).FirstOrDefault();

            //        if (gameSceneRunner != null)
            //        {
            //            runnerID = gameSceneRunner.MemberID;
            //        }

            //        //代走
            //        if (runnerID != null
            //            && newOrder.MemberID != runnerID)
            //        {
            //            newOrder.MemberID = runnerID;
            //            newOrder.ParticipationIndex = order.ParticipationIndex + 1;
            //            newOrder.ParticipationClass = ParticipationClass.PinchRunner;
            //        }
            //    }
            //    //投手交代(守備は仮オーダーで対応)
            //    else
            //    {
            //        if (order.PositionClass == PositionClass.Pitcher
            //            && order.MemberID != gameScene.PitcherMemberID)
            //        {
            //            newOrder.MemberID = gameScene.PitcherMemberID;
            //            newOrder.ParticipationIndex = order.ParticipationIndex + 1;
            //            newOrder.ParticipationClass = ParticipationClass.Defense;
            //        }
            //    }

            //    base.SetEntryInfo(newOrder);

            //    gameScene.Orders.Add(newOrder);
            //}
        }

    
    }

}
