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
        public IList<Models.Order> OnlyDefenseList { get; set; }

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

            //新規
            if (gameSceneID == null)
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
                    SceneOutCount = 0,
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

                ////攻撃
                //if(GameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
                //{ 
                    //結果ランナー(打者を初期表示)
                    AfterGameSceneRunnerList = new List<Models.GameSceneRunner>()
                    {
                        new GameSceneRunner()
                        {
                            GameID = Game.GameID,
                            TeamID = Game.TeamID,
                            MemberID = GameScene.BatterMemberID,
                            RunnerClass = RunnerClass.Batter,
                            SceneResultClass = SceneResultClass.Result,
                            RunnerResultClass = RunnerResultClass.Out,
                        }
                    };
                //}
                ////守備
                //else
                //{
                //    //結果ランナー(打者を初期表示)
                //    AfterGameSceneRunnerList = new List<Models.GameSceneRunner>()
                //    {
                //        new GameSceneRunner()
                //        {
                //            GameID = Game.GameID,
                //            TeamID = Game.TeamID,
                //            MemberID = GameScene.BatterMemberID,
                //            SceneResultClass = SceneResultClass.Result,
                //            RunnerResultClass = RunnerResultClass.Out,
                //        }
                //    };
                //}


                //対象イニング
                var inningScore = InningScoreList.Where(r => r.Inning == InningScoreList.Max(r => r.Inning)).OrderByDescending(r => r.TopButtomClass).FirstOrDefault();

                //タイトル
                ViewData[ViewDataConstant.Title] = inningScore.Inning.ToString() + "回"
                    + inningScore.TopButtomClass.GetEnumName()
                    + " "
                    + GameScene.SceneOutCount.ToString() + "アウト";

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

                if (GameScene.GameSceneID == SystemConstant.TempGameSceneID)
                {
                    //削除対象試合シーン
                    var deleteGameSceneList = await Context.GameScenes
                                            .Where(r => r.GameID == Game.GameID
                                                && r.Inning == GameScene.Inning
                                                && r.TopButtomClass == GameScene.TopButtomClass
                                                && r.InningIndex == GameScene.InningIndex).ToListAsync();

                    //削除対象オーダー
                    var deleteGameSceneDetailList = await Context.GameSceneDetails
                                            .Where(r => r.GameSceneID == deleteGameSceneList.FirstOrDefault().GameSceneID).ToListAsync();

                    //削除対象オーダー
                    var deleteGameSceneRunnerList = await Context.GameSceneRunners
                                            .Where(r => r.GameSceneID == deleteGameSceneList.FirstOrDefault().GameSceneID).ToListAsync();

                    //削除対象オーダー
                    var deleteOrderList = await Context.Orders
                                            .Where(r => r.GameSceneID == deleteGameSceneList.FirstOrDefault().GameSceneID).ToListAsync();

                    //前回データ削除
                    Context.GameScenes.RemoveRange(deleteGameSceneList);
                    Context.Orders.RemoveRange(deleteOrderList);
                    Context.GameSceneDetails.RemoveRange(deleteGameSceneDetailList);
                    Context.GameSceneRunners.RemoveRange(deleteGameSceneRunnerList);

                    //データ作成
                    var gameScene = new Models.GameScene();
                    gameScene.GameSceneDetails = new List<Models.GameSceneDetail>();
                    gameScene.GameSceneRunners = new List<Models.GameSceneRunner>();
                    gameScene.Orders = new List<Models.Order>();

                    //POST値セット
                    TryUpdateModel(gameScene);
                    //TryUpdateModel(gameScene, gameSceneDetailList, gameSceneRunnerList, orderList);

                    //データ追加
                    Context.GameScenes.Add(gameScene);
                    //Context.GameSceneDetails.AddRange(gameSceneDetailList);
                    //Context.GameSceneRunners.AddRange(gameSceneRunnerList);
                    //Context.Orders.AddRange(orderList);

                    //イニングスコア更新


                    //試合中
                    Game.StatusClass = StatusClass.DuringGame;

                }
                else
                {

                }


             

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if(GameSceneSubmitClass == Enum.GameSceneSubmitClass.NextBatter)
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
        //private void TryUpdateModel(Models.GameScene gameScene, List<Models.GameSceneDetail> gameSceneDetailList, List<Models.GameSceneRunner> gameSceneRunnerList, List<Models.Order> orderList)
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
                newGameSceneRunner.SceneResultClass = SceneResultClass.SceneChange;
                newGameSceneRunner.RunnerClass = gameSceneRunner.RunnerClass;
                newGameSceneRunner.RunnerResultClass = gameSceneRunner.RunnerResultClass;

                base.SetEntryInfo(newGameSceneRunner);

                gameScene.GameSceneRunners.Add(newGameSceneRunner);
            }

            //試合シーンアウト＆ランナー
            gameScene.SceneOutCount = GameScene.SceneOutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange && r.RunnerResultClass == RunnerResultClass.Out).Count();
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
                newGameSceneRunner.SceneResultClass = SceneResultClass.Result;
                newGameSceneRunner.RunnerClass = gameSceneRunner.RunnerClass;
                newGameSceneRunner.RunnerResultClass = gameSceneRunner.RunnerResultClass;

                base.SetEntryInfo(newGameSceneRunner);

                gameScene.GameSceneRunners.Add(newGameSceneRunner);
            }

            //試合シーン結果アウト＆ランナー
            gameScene.ResultOutCount = gameScene.SceneOutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result && r.RunnerResultClass == RunnerResultClass.Out).Count();
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
            if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterChange
                || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterGameSet)
            {
                gameScene.ChangeFLG = true;
            }

            //オーダー
            //gameScene.Orders = Context.Orders
            //                    .Where(r => r.GameSceneID == deleteGameSceneList.FirstOrDefault().GameSceneID).ToList();
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
            if(isOnFirstBase
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
