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

namespace Bmcs.Pages.GameScene
{
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.GameScene GameScene { get; set; }

        [BindProperty]
        public IList<Models.GameSceneDetail> BeforeGameSceneDetailList { get; set; }

        [BindProperty]
        public IList<Models.GameSceneDetail> AfterGameSceneDetailList { get; set; }

        [BindProperty]
        public IList<Models.GameSceneRunner> BeforeGameSceneRunnerList { get; set; }

        [BindProperty]
        public IList<Models.GameSceneRunner> AfterGameSceneRunnerList { get; set; }

        [BindProperty]
        public IList<Models.InningScore> InningScoreList { get; set; }

        [BindProperty]
        public Models.Order Order { get; set; }

        [BindProperty]
        public Models.Game Game { get; set; }

        [BindProperty]
        public GameSceneSubmitClass? GameSceneSubmitClass { get; set; }

        public async Task<IActionResult> OnGetAsync(int? gameID, int? gameSceneID)
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
            //メンバーID空不要
            base.IsMemberIDNeedEmpty = false;

            //シーン指定なし
            if (gameSceneID == null)
            {
                var lastGameSceneID = GetLastGameSceneID(Game.GameID);

                if (lastGameSceneID == null)
                {
                    //イニングスコア
                    InningScoreList = new List<InningScore>()
                    {
                        new InningScore()
                        {
                            GameID = Game.GameID,
                            TeamID = Game.TeamID,
                            Inning = 1,
                            TopButtomClass = TopButtomClass.Top,
                            Score = null
                        }
                    };

                    //試合シーン
                    GameScene = new Models.GameScene()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        Inning = 1,
                        TopButtomClass = TopButtomClass.Top,
                        InningIndex = 1,
                        BattingOrder = 1,
                        OutCount = 0,
                        RunnerSceneClass = RunnerSceneClass.None,
                    };

                    //先攻
                    if (Game.BatFirstBatSecondClass == BatFirstBatSecondClass.First)
                    {
                        Order = await Context.Orders
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.BattingOrder == 1)
                                .FirstOrDefaultAsync();

                        GameScene.OffenseDefenseClass = OffenseDefenseClass.Offense;
                        GameScene.PitcherMemberID = System.Convert.ToInt32(base.OpponentPitcherMemberIDList.FirstOrDefault().Value);
                        GameScene.BatterMemberID = Order.MemberID;
                    }
                    //後攻
                    else
                    {
                        Order = await Context.Orders
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.PositionClass == PositionClass.Pitcher)
                                .FirstOrDefaultAsync();

                        GameScene.OffenseDefenseClass = OffenseDefenseClass.Defense;
                        GameScene.PitcherMemberID = Order.MemberID;
                        GameScene.BatterMemberID = System.Convert.ToInt32(base.OpponentFielderMemberIDList.FirstOrDefault().Value);
                    }

                    //結果ランナー(打者を初期表示)
                    AfterGameSceneRunnerList = new List<Models.GameSceneRunner>()
                    {
                        new GameSceneRunner()
                        {
                            GameID = Game.GameID,
                            TeamID = Game.TeamID,
                            MemberID = GameScene.BatterMemberID,
                            BattingOrder = GameScene.BattingOrder,
                            RunnerClass = RunnerClass.Batter,
                            SceneResultClass = SceneResultClass.Result,
                            RunnerResultClass = RunnerResultClass.Out,
                        }
                    };
                }
                else
                {
                    //イニングスコア
                    InningScoreList = await Context.InningScores
                                .Where(r => r.GameID == Game.GameID)
                                .ToListAsync();

                    //最新試合シーン
                    var lastGameScene = await Context.GameScenes
                                .FindAsync(lastGameSceneID);

                    //試合シーン
                    GameScene = new Models.GameScene()
                    {
                        GameID = lastGameScene.GameID,
                        TeamID = lastGameScene.TeamID,
                    };

                    //チェンジ後
                    if (lastGameScene.ChangeFLG)
                    {
                        GameScene.InningIndex = 1;
                        GameScene.OutCount = 0;
                        GameScene.RunnerSceneClass = RunnerSceneClass.None;

                        if (lastGameScene.TopButtomClass == TopButtomClass.Top)
                        {
                            GameScene.Inning = lastGameScene.Inning;
                            GameScene.TopButtomClass = TopButtomClass.Buttom;
                        }
                        else
                        {
                            GameScene.Inning = lastGameScene.Inning + 1;
                            GameScene.TopButtomClass = TopButtomClass.Top;
                        }

                        if (lastGameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
                        {
                            GameScene.OffenseDefenseClass = OffenseDefenseClass.Defense;
                        }
                        else
                        {
                            GameScene.OffenseDefenseClass = OffenseDefenseClass.Offense;
                        }

                        //var lastOffenceGameScene = await Context.GameScenes.Where(r => r.GameID == lastGameScene.GameID
                        //                                && r.OffenseDefenseClass == OffenseDefenseClass.Offense
                        //                                && r.Inning == lastGameScene.Inning - 1)
                        //                            .FirstOrDefaultAsync();

                    }
                    else
                    {
                        GameScene.Inning = lastGameScene.Inning;
                        GameScene.TopButtomClass = lastGameScene.TopButtomClass;
                        GameScene.InningIndex = lastGameScene.InningIndex + 1;
                        GameScene.OffenseDefenseClass = lastGameScene.OffenseDefenseClass;
                        GameScene.OutCount = lastGameScene.ResultOutCount;
                        GameScene.RunnerSceneClass = lastGameScene.ResultRunnerSceneClass;
                    }

                    //攻撃
                    if (GameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
                    {
                        var orderList = await Context.Orders
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == lastGameSceneID)
                                .ToListAsync();

                        //前回が最終打者
                        if(orderList.DefaultIfEmpty().Max(r => r.BattingOrder) == lastGameScene.BattingOrder)
                        {
                            Order = orderList.OrderBy(r => r.BattingOrder).FirstOrDefault();
                        }
                        else
                        {
                            Order = orderList.Where(r => r.BattingOrder > lastGameScene.BattingOrder).OrderBy(r => r.BattingOrder).FirstOrDefault();
                        }

                        GameScene.BattingOrder = Order.BattingOrder;
                        GameScene.PitcherMemberID = lastGameScene.PitcherMemberID;
                        GameScene.BatterMemberID = Order.MemberID;
                    }
                    //守備
                    else
                    {
                        Order = await Context.Orders
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == lastGameSceneID && r.PositionClass == PositionClass.Pitcher)
                                .FirstOrDefaultAsync();

                        //相手チームは9人想定
                        if(lastGameScene.BattingOrder == 9)
                        {
                            GameScene.BattingOrder = 1;
                        }
                        else
                        { 
                            GameScene.BattingOrder = lastGameScene.BattingOrder + 1;
                        }

                        GameScene.PitcherMemberID = Order.MemberID;
                        GameScene.BatterMemberID = lastGameScene.BatterMemberID;
                    }

                    //結果ランナー(打者を初期表示)
                    AfterGameSceneRunnerList = new List<Models.GameSceneRunner>()
                    {
                        new GameSceneRunner()
                        {
                            GameID = Game.GameID,
                            TeamID = Game.TeamID,
                            MemberID = GameScene.BatterMemberID,
                            BattingOrder = GameScene.BattingOrder,
                            RunnerClass = RunnerClass.Batter,
                            SceneResultClass = SceneResultClass.Result,
                            RunnerResultClass = RunnerResultClass.Out,
                        }
                    };
                }

                //タイトル
                ViewData[ViewDataConstant.Title] = GameScene.Inning.ToString() + "回"
                    + GameScene.TopButtomClass.GetEnumName()
                    + " "
                    + GameScene.OutCount.ToString() + "アウト";

            }
            //修正
            else
            {
                //イニングスコア
                InningScoreList = await Context.InningScores.Where(r => r.GameID == Game.GameID)
                                                            .OrderBy(r => r.Inning)
                                                            .ToListAsync();
            }

            //試合シーンID(仮番号)
            GameScene.GameSceneID = SystemConstant.TempGameSceneID;

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
                    //メンバーID空不要
                    base.IsMemberIDNeedEmpty = false;

                    if (!ModelState.IsValid)
                    {
                        return Page();
                    }

                    ////未入力チェック
                    //if (OrderList.Any(r => r.MemberID == null || r.PositionClass == null)
                    //    || OnlyDefenseList.Any(r => r.MemberID == null || r.PositionClass == null))
                    //{
                    //    ModelState.AddModelError(nameof(Models.Game) + "." + nameof(Models.Game.GameID), "未指定の行があります。不要であれば行削除してください。");

                    //    return Page();
                    //}

                    ////投手チェック
                    //if (OrderList.Where(r => r.PositionClass == PositionClass.Pitcher).Count()
                    //    + OnlyDefenseList.Where(r => r.PositionClass == PositionClass.Pitcher).Count() != 1)
                    //{
                    //    ModelState.AddModelError(nameof(Models.Game) + "." + nameof(Models.Game.GameID), "投手は必ず一人指定してください。");

                    //    return Page();
                    //}

                    ////捕手チェック
                    //if (OrderList.Where(r => r.PositionClass == PositionClass.Catcher).Count()
                    //    + OnlyDefenseList.Where(r => r.PositionClass == PositionClass.Catcher).Count() != 1)
                    //{
                    //    ModelState.AddModelError(nameof(Models.Game) + "." + nameof(Models.Game.GameID), "捕手は必ず一人指定してください。");

                    //    return Page();

                    //}

                    //削除対象試合シーン
                    var deleteGameSceneList = await Context.GameScenes
                                            .Where(r => r.GameID == Game.GameID
                                                && r.Inning == GameScene.Inning
                                                && r.TopButtomClass == GameScene.TopButtomClass
                                                && r.InningIndex == GameScene.InningIndex).ToListAsync();

                    Context.GameScenes.RemoveRange(deleteGameSceneList);

                    if (deleteGameSceneList.Any())
                    {
                        //削除対象オーダー
                        var deleteGameSceneDetailList = await Context.GameSceneDetails
                                                    .Where(r => r.GameSceneID == deleteGameSceneList.FirstOrDefault().GameSceneID).ToListAsync();

                        Context.GameSceneDetails.RemoveRange(deleteGameSceneDetailList);

                        //削除対象オーダー
                        var deleteGameSceneRunnerList = await Context.GameSceneRunners
                                                .Where(r => r.GameSceneID == deleteGameSceneList.FirstOrDefault().GameSceneID).ToListAsync();

                        Context.GameSceneRunners.RemoveRange(deleteGameSceneRunnerList);

                        //削除対象オーダー
                        var deleteOrderList = await Context.Orders
                                                .Where(r => r.GameSceneID == deleteGameSceneList.FirstOrDefault().GameSceneID).ToListAsync();

                        Context.Orders.RemoveRange(deleteOrderList);
                    }

                    await Context.SaveChangesAsync();

                    //データ作成
                    var gameScene = new Models.GameScene();
                    gameScene.GameSceneDetails = new List<Models.GameSceneDetail>();
                    gameScene.GameSceneRunners = new List<Models.GameSceneRunner>();
                    gameScene.Orders = new List<Models.Order>();

                    //POST値セット
                    TryUpdateModel(gameScene);

                    //データ追加
                    Context.GameScenes.Add(gameScene);

                    //試合中
                    Game.StatusClass = StatusClass.DuringGame;

                    await Context.SaveChangesAsync();

                    //イニングスコア
                    var inningScore = Context.InningScores
                                        .Where(r => r.GameID == gameScene.GameID
                                            && r.Inning == gameScene.Inning
                                            && r.TopButtomClass == gameScene.TopButtomClass).FirstOrDefault();

                    var gameScenes = Context.GameScenes
                                        .Where(r => r.GameID == gameScene.GameID
                                            && r.Inning == gameScene.Inning
                                            && r.TopButtomClass == gameScene.TopButtomClass);

                    var score = gameScenes.DefaultIfEmpty().Sum(r => r.Run);

                    if (inningScore != null)
                    {
                        inningScore.Score = score;

                        //試合終了
                        if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterGameSet
                            || GameSceneSubmitClass == Enum.GameSceneSubmitClass.BeforeBatterGameSet)
                        {
                            inningScore.LastInningFLG = true;
                        }
                        else
                        {
                            inningScore.LastInningFLG = false;
                        }

                        base.SetUpdateInfo(inningScore);

                    }
                    else
                    {
                        inningScore = new InningScore();

                        inningScore.GameID = gameScene.GameID;
                        inningScore.TeamID = gameScene.TeamID;
                        inningScore.Inning = gameScene.Inning;
                        inningScore.TopButtomClass = gameScene.TopButtomClass;
                        inningScore.Score = score;

                        //試合終了
                        if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterGameSet
                            || GameSceneSubmitClass == Enum.GameSceneSubmitClass.BeforeBatterGameSet)
                        {
                            inningScore.LastInningFLG = true;
                        }
                        else
                        {
                            inningScore.LastInningFLG = false;
                        }

                        base.SetEntryInfo(inningScore);

                        Context.InningScores.Add(inningScore);
                    }

                    inningScore.GameScenes = gameScenes.ToList();

                    await Context.SaveChangesAsync();

                    await tran.CommitAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.NextBatter)
            {
                return RedirectToPage("./Edit", new { gameID = Game.GameID });
            }
            else
            {
                return RedirectToPage("./Edit", new { gameID = Game.GameID });
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="gameScene"></param>
        private void TryUpdateModel(Models.GameScene gameScene)
        {
            //試合シーン
            gameScene.GameID = Game.GameID;
            gameScene.TeamID = Game.TeamID;
            gameScene.Inning = GameScene.Inning;
            gameScene.TopButtomClass = GameScene.TopButtomClass;
            gameScene.InningIndex = GameScene.InningIndex;
            gameScene.BattingOrder = GameScene.BattingOrder;
            gameScene.OffenseDefenseClass = GameScene.OffenseDefenseClass;
            gameScene.PitcherMemberID = GameScene.PitcherMemberID;
            gameScene.BatterMemberID = GameScene.BatterMemberID;
            gameScene.HittingDirectionClass = GameScene.HittingDirectionClass;
            gameScene.HitBallClass = GameScene.HitBallClass;
            gameScene.ResultClass = GameScene.ResultClass;

            base.SetEntryInfo(gameScene);

            //試合詳細シーン
            foreach (var gameSceneDetail in BeforeGameSceneDetailList)
            {
                var newGameSceneDetail = new Models.GameSceneDetail();

                newGameSceneDetail.GameID = gameScene.GameID;
                newGameSceneDetail.TeamID = gameScene.TeamID;
                newGameSceneDetail.MemberID = gameSceneDetail.MemberID;
                newGameSceneDetail.SceneResultClass = SceneResultClass.SceneChange;
                newGameSceneDetail.DetailResultClass = gameSceneDetail.DetailResultClass;

                base.SetEntryInfo(newGameSceneDetail);

                gameScene.GameSceneDetails.Add(newGameSceneDetail);
            }

            //試合ランナーシーン
            foreach (var gameSceneRunner in BeforeGameSceneRunnerList)
            {
                var newGameSceneRunner = new Models.GameSceneRunner();

                newGameSceneRunner.GameID = gameScene.GameID;
                newGameSceneRunner.TeamID = gameScene.TeamID;
                newGameSceneRunner.MemberID = gameSceneRunner.MemberID;
                newGameSceneRunner.BattingOrder = gameSceneRunner.BattingOrder;
                newGameSceneRunner.SceneResultClass = SceneResultClass.SceneChange;
                newGameSceneRunner.RunnerClass = gameSceneRunner.RunnerClass;
                newGameSceneRunner.RunnerResultClass = gameSceneRunner.RunnerResultClass;

                base.SetEntryInfo(newGameSceneRunner);

                gameScene.GameSceneRunners.Add(newGameSceneRunner);
            }

            //試合シーンアウト＆ランナー
            gameScene.OutCount = GameScene.OutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange && r.RunnerResultClass == RunnerResultClass.Out).Count();
            gameScene.RunnerSceneClass = GetRunnerSceneClass(gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange));

            //試合詳細結果
            foreach (var gameSceneDetail in AfterGameSceneDetailList)
            {
                var newGameSceneDetail = new Models.GameSceneDetail();

                newGameSceneDetail.GameID = gameScene.GameID;
                newGameSceneDetail.TeamID = gameScene.TeamID;
                newGameSceneDetail.MemberID = gameSceneDetail.MemberID;
                newGameSceneDetail.SceneResultClass = SceneResultClass.Result;
                newGameSceneDetail.DetailResultClass = gameSceneDetail.DetailResultClass;

                base.SetEntryInfo(newGameSceneDetail);

                gameScene.GameSceneDetails.Add(newGameSceneDetail);
            }

            //試合ランナー結果
            foreach (var gameSceneRunner in AfterGameSceneRunnerList)
            {
                var newGameSceneRunner = new Models.GameSceneRunner();

                newGameSceneRunner.GameID = gameScene.GameID;
                newGameSceneRunner.TeamID = gameScene.TeamID;
                newGameSceneRunner.MemberID = gameSceneRunner.MemberID;
                newGameSceneRunner.BattingOrder = gameSceneRunner.BattingOrder;
                newGameSceneRunner.SceneResultClass = SceneResultClass.Result;
                newGameSceneRunner.RunnerClass = gameSceneRunner.RunnerClass;
                newGameSceneRunner.RunnerResultClass = gameSceneRunner.RunnerResultClass;

                base.SetEntryInfo(newGameSceneRunner);

                gameScene.GameSceneRunners.Add(newGameSceneRunner);
            }

            //試合シーン結果アウト＆ランナー
            gameScene.ResultOutCount = gameScene.OutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result && r.RunnerResultClass == RunnerResultClass.Out).Count();
            gameScene.ResultRunnerSceneClass = GetRunnerSceneClass(gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result));

            //得点
            gameScene.Run = gameScene.GameSceneRunners.Where(r => r.RunnerResultClass == RunnerResultClass.Run
                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptRBI
                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun
                                                        || r.RunnerResultClass == RunnerResultClass.RunExceptRBIEarnedRun).Count();
            //打点
            gameScene.RBI = gameScene.GameSceneRunners.Where(r => r.RunnerResultClass == RunnerResultClass.Run
                                                         || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun).Count();
            //自責点
            gameScene.EarnedRun = gameScene.GameSceneRunners.Where(r => r.RunnerResultClass == RunnerResultClass.Run
                                                         || r.RunnerResultClass == RunnerResultClass.RunExceptRBI).Count();

            //チェンジ
            if (GameSceneSubmitClass != Enum.GameSceneSubmitClass.NextBatter)
            {
                gameScene.ChangeFLG = true;
            }

            //オーダー
            var beforeOrders = Context.Orders
                                .Where(r => r.GameID == gameScene.GameID
                                    && r.GameSceneID == SystemConstant.TempGameSceneID).ToList();

            //仮オーダーなし
            if (!beforeOrders.Any())
            {
                var lastGameSceneID = GetLastGameSceneID(gameScene.GameID);

                beforeOrders = Context.Orders
                                .Where(r => r.GameID == gameScene.GameID
                                    && r.GameSceneID == lastGameSceneID).ToList();
            }

            //オーダー作成
            foreach (var order in beforeOrders)
            {
                var newOrder = new Models.Order();

                newOrder.GameID = order.GameID;
                newOrder.TeamID = order.TeamID;
                newOrder.MemberID = order.MemberID;
                newOrder.BattingOrder = order.BattingOrder;
                newOrder.ParticipationIndex = order.ParticipationIndex;
                newOrder.PositionClass = order.PositionClass;
                newOrder.ParticipationClass = order.ParticipationClass;

                //代打、代走
                if (gameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
                {
                    //シーン前ランナーID
                    int? runnerID = null;

                    var gameSceneRunner = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
                                                                    && r.BattingOrder == order.BattingOrder).FirstOrDefault();

                    if (gameSceneRunner != null)
                    {
                        runnerID = gameSceneRunner.MemberID;
                    }

                    //代走
                    if (runnerID != null
                        && newOrder.MemberID != runnerID)
                    {
                        newOrder.MemberID = runnerID;
                        newOrder.ParticipationIndex = order.ParticipationIndex + 1;
                        newOrder.ParticipationClass = ParticipationClass.PinchRunner;
                    }

                    //代打
                    if (gameScene.BattingOrder == order.BattingOrder
                        && gameScene.BatterMemberID != order.MemberID)
                    {
                        newOrder.MemberID = gameScene.BatterMemberID;
                        newOrder.ParticipationIndex = order.ParticipationIndex + 1;
                        newOrder.ParticipationClass = ParticipationClass.PinchHitter;
                    }

                    //シーン後ランナーID
                    runnerID = null;

                    gameSceneRunner = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
                                                                   && r.BattingOrder == order.BattingOrder).FirstOrDefault();

                    if (gameSceneRunner != null)
                    {
                        runnerID = gameSceneRunner.MemberID;
                    }

                    //代走
                    if (runnerID != null
                        && newOrder.MemberID != runnerID)
                    {
                        newOrder.MemberID = runnerID;
                        newOrder.ParticipationIndex = order.ParticipationIndex + 1;
                        newOrder.ParticipationClass = ParticipationClass.PinchRunner;
                    }
                }
                //投手交代(守備は仮オーダーで対応)
                else
                {
                    if (order.PositionClass == PositionClass.Pitcher
                        && order.MemberID != gameScene.PitcherMemberID)
                    {
                        newOrder.MemberID = gameScene.PitcherMemberID;
                        newOrder.ParticipationIndex = order.ParticipationIndex + 1;
                        newOrder.ParticipationClass = ParticipationClass.Defense;
                    }
                }

                base.SetEntryInfo(newOrder);

                gameScene.Orders.Add(newOrder);
            }


        }

        ///// <summary>
        ///// 初期処理
        ///// </summary>
        //private void Initialize()
        //{
        //    //チームID
        //    base.TeamID = Game.TeamID;
        //    //メンバーID空不要
        //    base.IsMemberIDNeedEmpty = false;
        //}

        /// <summary>
        /// 最新GameSceneID取得
        /// </summary>
        /// <param name="gameScenes"></param>
        /// <returns></returns>
        private int? GetLastGameSceneID(int gameID)
        {
            int? gameSceneID = null;

            var gameScenes = Context.GameScenes.Where(r => r.GameID == gameID);

            if (!gameScenes.Any())
            {
                return gameSceneID;
            }

            var inning = gameScenes.DefaultIfEmpty().Max(r => r.Inning);
            var topButtomClass = gameScenes.Where(r => r.Inning == inning).DefaultIfEmpty().Max(r => r.TopButtomClass);
            var inningIndex = gameScenes.Where(r => r.Inning == inning && r.TopButtomClass == topButtomClass).DefaultIfEmpty().Max(r => r.InningIndex);

            var gameScene = gameScenes.Where(r => r.Inning == inning && r.TopButtomClass == topButtomClass && r.InningIndex == inningIndex).FirstOrDefault();

            if (gameScene != null)
            {
                gameSceneID = gameScene.GameSceneID;
            }

            return gameSceneID;
        }

        /// <summary>
        /// ランナーシーン取得
        /// </summary>
        /// <param name="gameSceneRunner"></param>
        /// <returns></returns>
        private RunnerSceneClass GetRunnerSceneClass(IEnumerable<GameSceneRunner> gameSceneRunner)
        {
            bool isOnFirstBase = false;
            bool isOnSecondBase = false;
            bool isOnThirdBase = false;

            //一塁
            if (!gameSceneRunner.Any(r => r.RunnerResultClass == RunnerResultClass.OnFirstBase))
            {
                isOnFirstBase = true;
            }

            //二塁
            if (!gameSceneRunner.Any(r => r.RunnerResultClass == RunnerResultClass.OnSecondBase))
            {
                isOnSecondBase = true;
            }

            //三塁
            if (!gameSceneRunner.Any(r => r.RunnerResultClass == RunnerResultClass.OnThirdBase))
            {
                isOnThirdBase = true;
            }

            //満塁
            if (isOnFirstBase
                && isOnSecondBase
                && isOnThirdBase)
            {
                return RunnerSceneClass.FullBase;
            }
            //二三塁
            else if (!isOnFirstBase
               && isOnSecondBase
               && isOnThirdBase)
            {
                return RunnerSceneClass.SecondThird;
            }
            //一三塁
            else if (isOnFirstBase
             && !isOnSecondBase
             && isOnThirdBase)
            {
                return RunnerSceneClass.FirstThird;
            }
            //一二塁
            else if (isOnFirstBase
             && isOnSecondBase
             && !isOnThirdBase)
            {
                return RunnerSceneClass.FirstSecond;
            }
            //三塁
            else if (!isOnFirstBase
             && !isOnSecondBase
             && isOnThirdBase)
            {
                return RunnerSceneClass.Third;
            }
            //二塁
            else if (!isOnFirstBase
             && isOnSecondBase
             && !isOnThirdBase)
            {
                return RunnerSceneClass.Second;
            }
            //一塁
            else if (isOnFirstBase
             && !isOnSecondBase
             && !isOnThirdBase)
            {
                return RunnerSceneClass.First;
            }
            else
            {
                return RunnerSceneClass.None;
            }
        }
    }
}
