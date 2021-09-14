﻿using System;
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

        public async Task<IActionResult> OnGetAsync(int? gameID, int? gameSceneID, bool isOrderChange = false)
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

            //前回試合シーンID
            var lastGameSceneID = GetLastGameSceneID(Game.GameID, gameSceneID);

            //仮オーダー削除
            await DeleteTempOrders(Game.GameID, OrderDataClass.Temp);

            //チェンジオーダー削除
            if (!isOrderChange)
            {
                await DeleteTempOrders(Game.GameID, OrderDataClass.Change);
            }

            //シーン指定なし
            if (gameSceneID == null)
            {
                //プレイボール
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
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.OrderDataClass == OrderDataClass.Normal && r.BattingOrder == 1)
                                .FirstOrDefaultAsync();

                        GameScene.OffenseDefenseClass = OffenseDefenseClass.Offense;
                        GameScene.PitcherMemberID = System.Convert.ToInt32(base.OpponentPitcherMemberIDList.FirstOrDefault().Value);
                        GameScene.BatterMemberID = Order.MemberID;
                    }
                    //後攻
                    else
                    {
                        Order = await Context.Orders
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.OrderDataClass == OrderDataClass.Normal && r.PositionClass == PositionClass.Pitcher)
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

                    var lastBattingOrder = lastGameScene.BattingOrder;
                    var lastResultClass = lastGameScene.ResultClass;

                    //攻撃
                    if (GameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
                    {
                        var orderList = await Context.Orders
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == lastGameSceneID && r.BattingOrder != null && r.OrderDataClass == OrderDataClass.Normal)
                                .ToListAsync();

                        //チェンジ後
                        if (lastGameScene.ChangeFLG)
                        {
                            var lastOffenceGameScene = await Context.GameScenes.Where(r => r.GameID == lastGameScene.GameID
                                                               && r.OffenseDefenseClass == OffenseDefenseClass.Offense
                                                               && r.Inning == GameScene.Inning - 1)
                                                           .OrderByDescending(r => r.InningIndex)
                                                           .FirstOrDefaultAsync();

                            //前回攻撃
                            if (lastOffenceGameScene != null)
                            {
                                lastBattingOrder = lastOffenceGameScene.BattingOrder;
                                lastResultClass = lastOffenceGameScene.ResultClass;
                            }
                            //初回
                            else
                            {
                                lastBattingOrder = null;
                            }
                        }

                        if (lastBattingOrder != null)
                        {
                            //継続(盗塁死など)の場合、前回打者から
                            if (lastResultClass == ResultClass.Continue)
                            {
                                Order = orderList.Where(r => r.BattingOrder == lastBattingOrder).OrderBy(r => r.BattingOrder).FirstOrDefault();
                            }
                            else
                            {
                                //前回が最終打者
                                if (orderList.DefaultIfEmpty().Max(r => r.BattingOrder) == lastBattingOrder)
                                {
                                    Order = orderList.OrderBy(r => r.BattingOrder).FirstOrDefault();
                                }
                                else
                                {
                                    Order = orderList.Where(r => r.BattingOrder > lastBattingOrder).OrderBy(r => r.BattingOrder).FirstOrDefault();
                                }
                            }
                        }
                        else
                        {
                            //先頭バッター
                            Order = orderList.OrderBy(r => r.BattingOrder).FirstOrDefault();
                        }

                        GameScene.BattingOrder = Order.BattingOrder;
                        GameScene.PitcherMemberID = lastGameScene.PitcherMemberID;
                        GameScene.BatterMemberID = Order.MemberID;
                    }
                    //守備
                    else
                    {
                        Order = await Context.Orders
                              .Where(r => r.GameID == Game.GameID && r.OrderDataClass == OrderDataClass.Change && r.PositionClass == PositionClass.Pitcher)
                              .FirstOrDefaultAsync();

                        if (Order == null)
                        {
                            Order = await Context.Orders
                                    .Where(r => r.GameID == Game.GameID && r.OrderDataClass == OrderDataClass.Normal && r.GameSceneID == lastGameSceneID && r.PositionClass == PositionClass.Pitcher)
                                    .FirstOrDefaultAsync();
                        }

                        //チェンジ後
                        if (lastGameScene.ChangeFLG)
                        {
                            var lastOffenceGameScene = await Context.GameScenes.Where(r => r.GameID == lastGameScene.GameID
                                                                && r.OffenseDefenseClass == OffenseDefenseClass.Defense
                                                                && r.Inning == GameScene.Inning - 1)
                                                            .OrderByDescending(r => r.InningIndex)
                                                            .FirstOrDefaultAsync();

                            //初回以降
                            if (lastOffenceGameScene != null)
                            {
                                lastBattingOrder = lastOffenceGameScene.BattingOrder;
                                lastResultClass = lastOffenceGameScene.ResultClass;
                            }
                            //初回
                            else
                            {
                                lastBattingOrder = null;
                            }
                        }

                        if (lastBattingOrder != null)
                        {
                            //継続(盗塁死など)の場合、前回打者から
                            if (lastResultClass == ResultClass.Continue)
                            {
                                GameScene.BattingOrder = lastBattingOrder;
                            }
                            else
                            {
                                //相手チームは9人想定
                                if (lastBattingOrder == 9)
                                {
                                    GameScene.BattingOrder = 1;
                                }
                                else
                                {
                                    GameScene.BattingOrder = lastBattingOrder + 1;
                                }
                            }
                        }
                        else
                        {
                            GameScene.BattingOrder = 1;
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

                    //イニング途中
                    if (!lastGameScene.ChangeFLG)
                    {
                        var lastGameSceneRunnerList = await Context.GameSceneRunners.Where(r => r.GameSceneID == lastGameScene.GameSceneID
                                                        && r.SceneResultClass == SceneResultClass.Result
                                                        && (r.RunnerResultClass == RunnerResultClass.OnFirstBase
                                                            || r.RunnerResultClass == RunnerResultClass.OnSecondBase
                                                            || r.RunnerResultClass == RunnerResultClass.OnThirdBase)
                                                        )
                                                        .OrderBy(r => r.RunnerResultClass)
                                                        .ToListAsync();

                        foreach (var lastGameSceneRunner in lastGameSceneRunnerList)
                        {
                            RunnerClass runnerClass;

                            if (lastGameSceneRunner.RunnerResultClass == RunnerResultClass.OnFirstBase)
                            {
                                runnerClass = RunnerClass.OnFirstBase;
                            }
                            else if (lastGameSceneRunner.RunnerResultClass == RunnerResultClass.OnSecondBase)
                            {
                                runnerClass = RunnerClass.OnSecondBase;
                            }
                            else
                            {
                                runnerClass = RunnerClass.OnThirdBase;
                            }

                            AfterGameSceneRunnerList.Add(new GameSceneRunner()
                            {
                                GameID = Game.GameID,
                                TeamID = Game.TeamID,
                                MemberID = lastGameSceneRunner.MemberID,
                                BattingOrder = lastGameSceneRunner.BattingOrder,
                                RunnerClass = runnerClass,
                                SceneResultClass = SceneResultClass.Result,
                                RunnerResultClass = lastGameSceneRunner.RunnerResultClass,
                            });
                        }

                        //シーンランナー
                        BeforeGameSceneRunnerList = new List<Models.GameSceneRunner>();

                        foreach (var afterGameSceneRunner in AfterGameSceneRunnerList.Where(r => r.RunnerClass != RunnerClass.Batter))
                        {
                            BeforeGameSceneRunnerList.Add(new GameSceneRunner()
                            {
                                GameID = Game.GameID,
                                TeamID = Game.TeamID,
                                MemberID = afterGameSceneRunner.MemberID,
                                BattingOrder = afterGameSceneRunner.BattingOrder,
                                RunnerClass = afterGameSceneRunner.RunnerClass,
                                SceneResultClass = SceneResultClass.SceneChange,
                                RunnerResultClass = afterGameSceneRunner.RunnerResultClass,
                            });
                        }
                    }
                }

                //シーン詳細
                BeforeGameSceneDetailList = new List<Models.GameSceneDetail>()
                {
                    new GameSceneDetail()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        SceneResultClass = SceneResultClass.SceneChange,
                    }
                };

                //結果詳細
                AfterGameSceneDetailList = new List<Models.GameSceneDetail>()
                {
                    new GameSceneDetail()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        SceneResultClass = SceneResultClass.Result,
                    }
                };

                //仮オーダー作成
                await CreateTempOrders(Game.GameID, lastGameSceneID);

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

                //仮オーダー作成
                await CreateTempOrders(Game.GameID, GameScene.GameSceneID);

            }

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

                    var deleteGameSceneDetailList = new List<Models.GameSceneDetail>();
                    var deleteGameSceneRunnerList = new List<Models.GameSceneRunner>();
                    var deleteOrderList = new List<Models.Order>();

                    //試合シーン関連対象データ削除
                    DeleteRelatedGameScenes(deleteGameSceneList, deleteGameSceneDetailList, deleteGameSceneRunnerList, deleteOrderList);

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

                    //チェンジ、試合終了時は未来データを削除
                    if (GameSceneSubmitClass != Enum.GameSceneSubmitClass.NextBatter)
                    {
                        //試合シーン同イニング未来データ
                        deleteGameSceneList = await Context.GameScenes
                                            .Where(r => r.GameID == Game.GameID
                                                && r.Inning == GameScene.Inning
                                                && r.TopButtomClass == GameScene.TopButtomClass
                                                && r.InningIndex > GameScene.InningIndex).ToListAsync();

                        Context.GameScenes.RemoveRange(deleteGameSceneList);

                        //試合シーン関連対象データ削除
                        DeleteRelatedGameScenes(deleteGameSceneList, deleteGameSceneDetailList, deleteGameSceneRunnerList, deleteOrderList);

                        await Context.SaveChangesAsync();

                        //試合終了
                        if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.BeforeBatterGameSet
                            || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterGameSet)
                        {
                            //試合シーン未来イニング
                            deleteGameSceneList = await Context.GameScenes
                                                .Where(r => r.GameID == Game.GameID
                                                    && ((r.Inning > GameScene.Inning)
                                                     || (r.Inning == GameScene.Inning && r.TopButtomClass == TopButtomClass.Buttom && GameScene.TopButtomClass == TopButtomClass.Top))).ToListAsync();

                            Context.GameScenes.RemoveRange(deleteGameSceneList);

                            //試合シーン関連対象データ削除
                            DeleteRelatedGameScenes(deleteGameSceneList, deleteGameSceneDetailList, deleteGameSceneRunnerList, deleteOrderList);

                            //イニングスコア
                            var deleteInningScoreList = await Context.InningScores
                                                        .Where(r => r.GameID == Game.GameID
                                                                && ((r.Inning > GameScene.Inning)
                                                                 || (r.Inning == GameScene.Inning && r.TopButtomClass == TopButtomClass.Buttom && GameScene.TopButtomClass == TopButtomClass.Top))).ToListAsync();

                            Context.InningScores.RemoveRange(deleteInningScoreList);

                            await Context.SaveChangesAsync();
                        }
                    }

                    //イニングスコア
                    var inningScore = Context.InningScores
                                        .Where(r => r.GameID == Game.GameID
                                            && r.Inning == GameScene.Inning
                                            && r.TopButtomClass == GameScene.TopButtomClass).FirstOrDefault();

                    var gameScenes = Context.GameScenes
                                        .Where(r => r.GameID == Game.GameID
                                            && r.Inning == GameScene.Inning
                                            && r.TopButtomClass == GameScene.TopButtomClass);

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

                        inningScore.GameID = Game.GameID;
                        inningScore.TeamID = Game.TeamID;
                        inningScore.Inning = GameScene.Inning;
                        inningScore.TopButtomClass = GameScene.TopButtomClass;
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

                    //仮オーダー削除
                    await DeleteTempOrders(Game.GameID, OrderDataClass.Temp);
                    await DeleteTempOrders(Game.GameID, OrderDataClass.Change);

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
            foreach (var gameSceneDetail in BeforeGameSceneDetailList.Where(r => r.DetailResultClass != null))
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
            foreach (var gameSceneDetail in AfterGameSceneDetailList.Where(r => r.DetailResultClass != null))
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
                                    && r.OrderDataClass == OrderDataClass.Change).ToList();

            //変更オーダーなし
            if (!beforeOrders.Any())
            {
                var lastGameSceneID = GetLastGameSceneID(gameScene.GameID, GameScene.GameSceneID.ZeroToNull());

                beforeOrders = Context.Orders
                                .Where(r => r.GameID == gameScene.GameID
                                    && r.GameSceneID == lastGameSceneID
                                    && r.OrderDataClass == OrderDataClass.Normal).ToList();
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
                newOrder.OrderDataClass = OrderDataClass.Normal;

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
        /// <param name="gameID"></param>
        /// <param name="selectGameSceneID"></param>
        /// <returns></returns>
        private int? GetLastGameSceneID(int gameID, int? selectGameSceneID = null)
        {
            int? gameSceneID = null;

            if (selectGameSceneID == null)
            {
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
            }
            else
            {

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

        /// <summary>
        ///  試合シーン関連対象データ削除
        /// </summary>
        /// <param name="deleteGameSceneList"></param>
        /// <param name="deleteGameSceneDetailList"></param>
        /// <param name="deleteGameSceneRunnerList"></param>
        /// <param name="deleteOrderList"></param>
        private void DeleteRelatedGameScenes(List<Models.GameScene> deleteGameSceneList
                                            , List<Models.GameSceneDetail> deleteGameSceneDetailList
                                            , List<Models.GameSceneRunner> deleteGameSceneRunnerList
                                            , List<Models.Order> deleteOrderList)
        {
            foreach (var deleteGameScene in deleteGameSceneList)
            {
                //削除対象試合シーン詳細
                deleteGameSceneDetailList = Context.GameSceneDetails
                                            .Where(r => r.GameSceneID == deleteGameScene.GameSceneID).ToList();

                Context.GameSceneDetails.RemoveRange(deleteGameSceneDetailList);

                //削除対象試合シーンランナー
                deleteGameSceneRunnerList = Context.GameSceneRunners
                                        .Where(r => r.GameSceneID == deleteGameScene.GameSceneID).ToList();

                Context.GameSceneRunners.RemoveRange(deleteGameSceneRunnerList);

                //削除対象オーダー
                deleteOrderList = Context.Orders
                                        .Where(r => r.GameSceneID == deleteGameScene.GameSceneID).ToList();

                Context.Orders.RemoveRange(deleteOrderList);
            }
        }

        /// <summary>
        /// 仮オーダー作成
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="gameSceneID"></param>
        /// <returns></returns>
        private async Task CreateTempOrders(int? gameID, int? gameSceneID)
        {
            //オーダー
            var beforeOrders = await Context.Orders
                                    .Where(r => r.GameID == gameID
                                    && r.GameSceneID == gameSceneID
                                    && r.OrderDataClass == OrderDataClass.Normal).ToListAsync();

            //オーダー作成
            foreach (var order in beforeOrders)
            {
                var newOrder = new Models.Order();

                newOrder.GameID = order.GameID;
                newOrder.GameSceneID = null;
                newOrder.TeamID = order.TeamID;
                newOrder.MemberID = order.MemberID;
                newOrder.BattingOrder = order.BattingOrder;
                newOrder.ParticipationIndex = order.ParticipationIndex;
                newOrder.PositionClass = order.PositionClass;
                newOrder.ParticipationClass = order.ParticipationClass;
                newOrder.OrderDataClass = OrderDataClass.Temp;

                base.SetEntryInfo(newOrder);

                Context.Orders.Add(newOrder);
            }

            await Context.SaveChangesAsync();
        }
    }
}
