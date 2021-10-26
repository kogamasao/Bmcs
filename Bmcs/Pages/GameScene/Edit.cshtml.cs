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
        public List<Models.GameSceneDetail> BeforeGameSceneDetailList { get; set; }

        [BindProperty]
        public List<Models.GameSceneDetail> AfterGameSceneDetailList { get; set; }

        [BindProperty]
        public List<Models.GameSceneRunner> BeforeGameSceneRunnerList { get; set; }

        [BindProperty]
        public List<Models.GameSceneRunner> AfterGameSceneRunnerList { get; set; }

        [BindProperty]
        public List<Models.InningScore> InningScoreList { get; set; }

        [BindProperty]
        public Models.Order Order { get; set; }

        [BindProperty]
        public Models.Game Game { get; set; }

        [BindProperty]
        public GameSceneSubmitClass? GameSceneSubmitClass { get; set; }

        [BindProperty]
        public int? LastGameSceneID { get; set; }

        [BindProperty]
        public int? NextGameSceneID { get; set; }

        [BindProperty]
        public int? SkipCount { get; set; }

        [BindProperty]
        public decimal? RegularBattingOrder { get; set; }

        [BindProperty]
        public decimal? InterruptBattingOrder { get; set; }

        [BindProperty]
        public bool IsTieBreak { get; set; }

        public List<Models.Order> GameOrderList { get; set; }

        /// <summary>
        /// タイブレーク打順リスト
        /// </summary>
        public SelectList TieBreakBattingOrderList
        {
            get
            {
                return AddFirstItem(new SelectList(Context.Orders.Where(r => r.GameID == Game.GameID && r.OrderDataClass == OrderDataClass.Temp)
                                                                .OrderBy(r => r.BattingOrder)
                                                                , nameof(Order.BattingOrder), nameof(Order.DisplayBattingOrder), string.Empty)
                    , new SelectListItem("前の回から継続打順", null));
            }
        }

        public async Task<IActionResult> OnGetAsync(int? gameID, int? gameSceneID, bool isOrderChange = false, bool isInitialize = false, int skipCount = 0)
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

            GameOrderList = await Context.Orders
                .Where(r => r.GameID == gameID)
                .ToListAsync();

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
            LastGameSceneID = GetLastGameSceneID(Game.GameID, gameSceneID);
            //次回試合シーンID(修正時のみ)
            NextGameSceneID = GetNextGameSceneID(Game.GameID, gameSceneID);

            //仮オーダー削除
            await DeleteTempOrders(Game.GameID, OrderDataClass.Temp);

            //チェンジオーダー削除
            if (!isOrderChange)
            {
                await DeleteTempOrders(Game.GameID, OrderDataClass.Change);
            }

            //シーン指定なし、再読み込み
            if (gameSceneID == null || isInitialize)
            {
                //プレイボール
                if (LastGameSceneID == null)
                {
                    //イニングスコア
                    InningScoreList = new List<Models.InningScore>()
                    {
                        new Models.InningScore()
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
                        GameSceneID = gameSceneID.NullToZero(),
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        Inning = 1,
                        TopButtomClass = TopButtomClass.Top,
                        InningIndex = 1,
                        OutCount = 0,
                        RunnerSceneClass = RunnerSceneClass.None,
                    };

                    //タイブレーク判断
                    IsTieBreak = Game.TieBreakStartInning != null && Game.TieBreakStartInning <= GameScene.Inning;

                    //タイブレーク中
                    if(IsTieBreak)
                    {
                        GameScene.OutCount = Game.TieBreakStartOutCount;
                        GameScene.RunnerSceneClass = Game.TieBreakStartRunnerSceneClass;
                    }

                    //先攻
                    if (Game.BatFirstBatSecondClass == BatFirstBatSecondClass.First)
                    {
                        var orderCount = GameOrderList
                            .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.OrderDataClass == OrderDataClass.Normal && r.BattingOrder != null)
                            .Count();

                        if (skipCount >= orderCount)
                        {
                            skipCount -= orderCount;
                        }

                        Order = GameOrderList
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.OrderDataClass == OrderDataClass.Normal && r.BattingOrder != null)
                                .OrderBy(r => r.BattingOrder)
                                .Skip(skipCount)
                                .FirstOrDefault();

                        GameScene.OffenseDefenseClass = OffenseDefenseClass.Offense;
                        GameScene.PitcherMemberID = System.Convert.ToInt32(base.OpponentPitcherMemberIDList.FirstOrDefault().Value);
                        GameScene.BatterMemberID = Order.MemberID;
                        GameScene.BattingOrder = Order.BattingOrder;
                        RegularBattingOrder = GameScene.BattingOrder;
                        InterruptBattingOrder = GetInterruptBattingOrder(Game.GameID, null, GameScene.BattingOrder);

                        //結果ランナー(打者を初期表示)
                        AfterGameSceneRunnerList = InitializeAfterGameSceneRunner(GameScene);

                        //タイブレーク中
                        if (IsTieBreak)
                        {
                            var runnerOrder = GameOrderList
                                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.OrderDataClass == OrderDataClass.Normal && r.BattingOrder != null)
                                                .OrderByDescending(r => r.BattingOrder)
                                                .ToList();

                            //タイブレークランナー
                            AfterGameSceneRunnerList.AddRange(GetTieBreakRunner(Game, runnerOrder));
                        }
                    }
                    //後攻
                    else
                    {
                        if (skipCount >= 9)
                        {
                            skipCount -= 9;
                        }

                        Order = GameOrderList
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.OrderDataClass == OrderDataClass.Normal && r.PositionClass == PositionClass.Pitcher)
                                .FirstOrDefault();

                        GameScene.OffenseDefenseClass = OffenseDefenseClass.Defense;
                        GameScene.PitcherMemberID = Order.MemberID;
                        GameScene.BatterMemberID = System.Convert.ToInt32(base.OpponentFielderMemberIDList.FirstOrDefault().Value);
                        GameScene.BattingOrder = 1 + skipCount;

                        //結果ランナー(打者を初期表示)
                        AfterGameSceneRunnerList = InitializeAfterGameSceneRunner(GameScene);

                        //タイブレーク中
                        if (IsTieBreak)
                        {
                            var runnerOrder = GetOpponentOrder(Game, GameScene.BatterMemberID).OrderByDescending(r => r.BattingOrder).ToList();

                            //タイブレークランナー
                            AfterGameSceneRunnerList.AddRange(GetTieBreakRunner(Game, runnerOrder));
                        }
                    }

                    //タイブレーク中
                    if (IsTieBreak)
                    {
                        //シーンランナー
                        BeforeGameSceneRunnerList = CopyBeforeGameSceneRunnerListFromAfter(AfterGameSceneRunnerList);
                    }
                }
                else
                {
                    //最新試合シーン
                    var lastGameScene = await Context.GameScenes
                                .FindAsync(LastGameSceneID);

                    //イニングスコア
                    InningScoreList = await Context.InningScores
                                .Where(r => r.GameID == Game.GameID
                                        && ((lastGameScene.TopButtomClass == TopButtomClass.Buttom && r.Inning <= lastGameScene.Inning)
                                            ||
                                            (lastGameScene.TopButtomClass == TopButtomClass.Top && (r.Inning < lastGameScene.Inning || (r.Inning == lastGameScene.Inning && r.TopButtomClass == TopButtomClass.Top))))
                                       )
                                .OrderBy(r => r.Inning)
                                .ThenBy(r => r.TopButtomClass)
                                .ToListAsync();

                    //試合シーン
                    GameScene = new Models.GameScene()
                    {
                        GameSceneID = gameSceneID.NullToZero(),
                        GameID = lastGameScene.GameID,
                        TeamID = lastGameScene.TeamID,
                    };

                    //チェンジ後
                    if (lastGameScene.ChangeFLG)
                    {
                        GameScene.InningIndex = 1;

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

                        //タイブレーク判断
                        IsTieBreak = Game.TieBreakStartInning != null && Game.TieBreakStartInning <= GameScene.Inning;

                        //タイブレーク中
                        if (IsTieBreak)
                        {
                            GameScene.OutCount = Game.TieBreakStartOutCount;
                            GameScene.RunnerSceneClass = Game.TieBreakStartRunnerSceneClass;
                        }
                        else
                        {
                            GameScene.OutCount = 0;
                            GameScene.RunnerSceneClass = RunnerSceneClass.None;
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

                        //タイブレーク判断
                        IsTieBreak = Game.TieBreakStartInning != null && Game.TieBreakStartInning <= GameScene.Inning;
                    }

                    var lastBattingOrder = lastGameScene.BattingOrder;
                    var lastResultClass = lastGameScene.ResultClass;

                    //攻撃
                    if (GameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
                    {
                        var orderCount = GameOrderList
                                            .Where(r => r.GameID == Game.GameID && r.GameSceneID == LastGameSceneID && r.BattingOrder != null && r.OrderDataClass == OrderDataClass.Normal)
                                            .Count();

                        if (skipCount >= orderCount)
                        {
                            skipCount -= orderCount;
                        }

                        var orderList = GameOrderList
                                .Where(r => r.GameID == Game.GameID && r.GameSceneID == LastGameSceneID && r.BattingOrder != null && r.OrderDataClass == OrderDataClass.Normal)
                                .ToList();

                        var lastPitcherMemberID = lastGameScene.PitcherMemberID;

                        //チェンジ後
                        if (lastGameScene.ChangeFLG)
                        {
                            var lastOffenceGameScene = await Context.GameScenes.Where(r => r.GameID == lastGameScene.GameID
                                   && r.OffenseDefenseClass == OffenseDefenseClass.Offense
                                   && r.Inning == GameScene.Inning - 1)
                               .OrderByDescending(r => r.InningIndex)
                               .FirstOrDefaultAsync();

                            //タイブレーク開始イニングで打順指定あり
                            if (IsTieBreak
                                && Game.TieBreakStartInning == GameScene.Inning
                                && Game.TieBreakStartBattingOrder != null)
                            {
                                //前回攻撃
                                if (lastOffenceGameScene != null)
                                {
                                    var lastBatter = orderList.Where(r => r.BattingOrder < Game.TieBreakStartBattingOrder).OrderByDescending(r => r.BattingOrder).FirstOrDefault();

                                    if(lastBatter != null)
                                    { 
                                        lastBattingOrder = lastBatter.BattingOrder;
                                    }
                                    else
                                    {
                                        lastBattingOrder = orderList.OrderByDescending(r => r.BattingOrder).FirstOrDefault().BattingOrder;
                                    }

                                    lastResultClass = null;
                                    lastPitcherMemberID = lastOffenceGameScene.PitcherMemberID;
                                }
                                //初回
                                else
                                {
                                    lastBattingOrder = null;
                                    lastResultClass = null;
                                    //相手デフォルトピッチャー
                                    lastPitcherMemberID = System.Convert.ToInt32(base.OpponentPitcherMemberIDList.FirstOrDefault().Value);
                                }
                            }
                            else
                            { 

                                //前回攻撃
                                if (lastOffenceGameScene != null)
                                {
                                    lastBattingOrder = lastOffenceGameScene.BattingOrder;
                                    lastResultClass = lastOffenceGameScene.ResultClass;
                                    lastPitcherMemberID = lastOffenceGameScene.PitcherMemberID;
                                }
                                //初回
                                else
                                {
                                    lastBattingOrder = null;
                                    lastResultClass = null;
                                    //相手デフォルトピッチャー
                                    lastPitcherMemberID = System.Convert.ToInt32(base.OpponentPitcherMemberIDList.FirstOrDefault().Value);
                                }
                            }
                        }

                        if (lastBattingOrder != null)
                        {
                            //継続(盗塁死など)の場合、前回打者から
                            if (lastResultClass == ResultClass.Change)
                            {
                                var followingOrderCount = orderList.Where(r => r.BattingOrder >= lastBattingOrder).Count();

                                if (followingOrderCount > skipCount)
                                {
                                    Order = orderList.Where(r => r.BattingOrder >= lastBattingOrder).OrderBy(r => r.BattingOrder).Skip(skipCount).FirstOrDefault();
                                }
                                else
                                {
                                    Order = orderList.OrderBy(r => r.BattingOrder).Skip(skipCount - followingOrderCount).FirstOrDefault();
                                }
                            }
                            else
                            {
                                //前回が最終打者
                                if (orderList.DefaultIfEmpty().Max(r => r.BattingOrder) == lastBattingOrder)
                                {
                                    Order = orderList.OrderBy(r => r.BattingOrder).Skip(skipCount).FirstOrDefault();
                                }
                                else
                                {
                                    var followingOrderCount = orderList.Where(r => r.BattingOrder > lastBattingOrder).Count();

                                    if (followingOrderCount > skipCount)
                                    {
                                        Order = orderList.Where(r => r.BattingOrder > lastBattingOrder).OrderBy(r => r.BattingOrder).Skip(skipCount).FirstOrDefault();
                                    }
                                    else
                                    {
                                        Order = orderList.OrderBy(r => r.BattingOrder).Skip(skipCount - followingOrderCount).FirstOrDefault();
                                    }
                                }
                            }
                        }
                        else
                        {
                            //先頭バッター
                            Order = orderList.OrderBy(r => r.BattingOrder).Skip(skipCount).FirstOrDefault();
                        }

                        GameScene.BattingOrder = Order.BattingOrder;
                        GameScene.PitcherMemberID = lastPitcherMemberID;
                        GameScene.BatterMemberID = Order.MemberID;
                        RegularBattingOrder = GameScene.BattingOrder;
                        InterruptBattingOrder = GetInterruptBattingOrder(Game.GameID, LastGameSceneID, GameScene.BattingOrder);

                        //結果ランナー(打者を初期表示)
                        AfterGameSceneRunnerList = InitializeAfterGameSceneRunner(GameScene);

                        //チェンジ後、タイブレーク
                        if (lastGameScene.ChangeFLG && IsTieBreak)
                        {
                            var runnerOrder = SortOrderByBaseBattingOrder(orderList, lastBattingOrder.NullToZero());

                            //タイブレークランナー
                            AfterGameSceneRunnerList.AddRange(GetTieBreakRunner(Game, runnerOrder));

                            //シーンランナー
                            BeforeGameSceneRunnerList = CopyBeforeGameSceneRunnerListFromAfter(AfterGameSceneRunnerList);
                        }
                    }
                    //守備
                    else
                    {
                        if (skipCount >= 9)
                        {
                            skipCount -= 9;
                        }

                        Order = GameOrderList
                              .Where(r => r.GameID == Game.GameID
                                  && r.OrderDataClass == OrderDataClass.Change
                                  && r.PositionClass == PositionClass.Pitcher)
                              .FirstOrDefault();

                        if (Order == null)
                        {
                            Order = GameOrderList
                                    .Where(r => r.GameID == Game.GameID
                                        && r.OrderDataClass == OrderDataClass.Normal
                                        && r.GameSceneID == LastGameSceneID
                                        && r.PositionClass == PositionClass.Pitcher)
                                    .FirstOrDefault();
                        }

                        var lastBatterMemberID = lastGameScene.BatterMemberID;

                        //チェンジ後
                        if (lastGameScene.ChangeFLG)
                        {
                            //相手チームはタイブレーク指定打順関係なし
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
                                lastBatterMemberID = lastOffenceGameScene.BatterMemberID;
                            }
                            //初回
                            else
                            {
                                lastBattingOrder = null;
                                lastResultClass = null;
                                //相手デフォルトバッター
                                lastBatterMemberID = System.Convert.ToInt32(base.OpponentFielderMemberIDList.FirstOrDefault().Value);
                            }
                        }

                        if (lastBattingOrder != null)
                        {
                            //継続(盗塁死など)の場合、前回打者から
                            if (lastResultClass == ResultClass.Change)
                            {
                                var followingOrderCount = 9 - (lastBattingOrder - 1);

                                if (followingOrderCount > skipCount)
                                {
                                    GameScene.BattingOrder = lastBattingOrder + skipCount;
                                }
                                else
                                {
                                    GameScene.BattingOrder = 1 + (skipCount - followingOrderCount);
                                }
                            }
                            else
                            {
                                //相手チームは9人想定
                                if (lastBattingOrder == 9)
                                {
                                    GameScene.BattingOrder = 1 + skipCount;
                                }
                                else
                                {
                                    var followingOrderCount = 9 - lastBattingOrder;

                                    if (followingOrderCount > skipCount)
                                    {
                                        GameScene.BattingOrder = lastBattingOrder + 1 + skipCount;
                                    }
                                    else
                                    {
                                        GameScene.BattingOrder = 1 + (skipCount - followingOrderCount);
                                    }
                                }
                            }
                        }
                        else
                        {
                            GameScene.BattingOrder = 1 + skipCount;
                        }

                        GameScene.PitcherMemberID = Order.MemberID;
                        GameScene.BatterMemberID = lastBatterMemberID;

                        //結果ランナー(打者を初期表示)
                        AfterGameSceneRunnerList = InitializeAfterGameSceneRunner(GameScene);

                        //チェンジ後、タイブレーク
                        if (lastGameScene.ChangeFLG && IsTieBreak)
                        {
                            var runnerOrder = SortOrderByBaseBattingOrder(GetOpponentOrder(Game, GameScene.BatterMemberID).OrderByDescending(r => r.BattingOrder).ToList(), lastBattingOrder.NullToZero());

                            //タイブレークランナー
                            AfterGameSceneRunnerList.AddRange(GetTieBreakRunner(Game, runnerOrder));

                            //シーンランナー
                            BeforeGameSceneRunnerList = CopyBeforeGameSceneRunnerListFromAfter(AfterGameSceneRunnerList);
                        }
                    }

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
                                BeforeRunnerClass = runnerClass,
                                RunnerClass = runnerClass,
                                SceneResultClass = SceneResultClass.Result,
                                RunnerResultClass = lastGameSceneRunner.RunnerResultClass,
                            });
                        }

                        //シーンランナー
                        BeforeGameSceneRunnerList = CopyBeforeGameSceneRunnerListFromAfter(AfterGameSceneRunnerList);
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
                await CreateTempOrders(Game.GameID, LastGameSceneID);

            }
            //修正
            else
            {
                //最新試合シーン
                var lastGameScene = await Context.GameScenes
                            .FindAsync(LastGameSceneID);

                if (lastGameScene != null)
                {
                    //イニングスコア
                    InningScoreList = await Context.InningScores
                                .Where(r => r.GameID == Game.GameID
                                        && ((lastGameScene.TopButtomClass == TopButtomClass.Buttom && r.Inning <= lastGameScene.Inning)
                                            ||
                                            (lastGameScene.TopButtomClass == TopButtomClass.Top && (r.Inning < lastGameScene.Inning || (r.Inning == lastGameScene.Inning && r.TopButtomClass == TopButtomClass.Top))))
                                        )
                                .OrderBy(r => r.Inning)
                                .ThenBy(r => r.TopButtomClass)
                                .ToListAsync();
                }
                else
                {
                    //イニングスコア
                    InningScoreList = new List<Models.InningScore>()
                    {
                        new Models.InningScore()
                        {
                            GameID = Game.GameID,
                            TeamID = Game.TeamID,
                            Inning = 1,
                            TopButtomClass = TopButtomClass.Top,
                            Score = null
                        }
                    };
                }

                //試合シーン
                GameScene = await Context.GameScenes
                            .FindAsync(gameSceneID);

                //タイブレーク判断
                IsTieBreak = Game.TieBreakStartInning != null && Game.TieBreakStartInning <= GameScene.Inning;

                if (GameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
                {
                    if (GameScene.InterruptFLG)
                    {
                        var regularBattingOrder = GameOrderList
                                        .Where(r => r.GameID == Game.GameID
                                                && r.GameSceneID == LastGameSceneID
                                                && r.BattingOrder != null
                                                && r.BattingOrder > GameScene.BattingOrder
                                                && r.OrderDataClass == OrderDataClass.Normal)
                                        .FirstOrDefault();

                        if (regularBattingOrder != null)
                        {
                            RegularBattingOrder = regularBattingOrder.BattingOrder;
                            InterruptBattingOrder = GameScene.BattingOrder;
                        }
                    }
                    else
                    {
                        RegularBattingOrder = GameScene.BattingOrder;
                        InterruptBattingOrder = GetInterruptBattingOrder(Game.GameID, LastGameSceneID, GameScene.BattingOrder);
                    }
                }

                //シーン詳細
                BeforeGameSceneDetailList = await Context.GameSceneDetails
                                                .Where(r => r.GameSceneID == gameSceneID && r.SceneResultClass == SceneResultClass.SceneChange)
                                                .ToListAsync();

                if (BeforeGameSceneDetailList == null || !BeforeGameSceneDetailList.Any())
                {
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
                }

                //シーンランナー
                BeforeGameSceneRunnerList = await Context.GameSceneRunners
                                                .Where(r => r.GameSceneID == gameSceneID && r.SceneResultClass == SceneResultClass.SceneChange)
                                                .ToListAsync();

                //結果詳細
                AfterGameSceneDetailList = await Context.GameSceneDetails
                                                .Where(r => r.GameSceneID == gameSceneID && r.SceneResultClass == SceneResultClass.Result)
                                                .ToListAsync();

                if (AfterGameSceneDetailList == null || !AfterGameSceneDetailList.Any())
                {
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
                }

                //結果ランナー
                AfterGameSceneRunnerList = await Context.GameSceneRunners
                                                .Where(r => r.GameSceneID == gameSceneID && r.SceneResultClass == SceneResultClass.Result)
                                                .ToListAsync();

                //仮オーダー作成
                await CreateTempOrders(Game.GameID, GameScene.GameSceneID);

            }

            //スキップカウント
            SkipCount = skipCount;

            //タイブレークデフォルトルール
            if(!IsTieBreak)
            {
                Game.TieBreakStartBattingOrder = null;
                Game.TieBreakStartOutCount = 0;
                Game.TieBreakStartRunnerSceneClass = RunnerSceneClass.FirstSecond;
            }

            //タイトル
            ViewData[ViewDataConstant.Title] = GameScene.Inning.ToString() + "回"
                + GameScene.TopButtomClass.GetEnumName()
                + " "
                + GameScene.OutCount.ToString() + "アウト";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                using var tran = await Context.Database.BeginTransactionAsync();
                var tieBreakStartBattingOrder = Game.TieBreakStartBattingOrder;
                var tieBreakStartOutCount = Game.TieBreakStartOutCount;
                var tieBreakStartRunnerSceneClass = Game.TieBreakStartRunnerSceneClass;

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
                var gameScene = new Models.GameScene
                {
                    GameSceneDetails = new List<Models.GameSceneDetail>(),
                    GameSceneRunners = new List<Models.GameSceneRunner>(),
                    Orders = new List<Models.Order>()
                };

                //POST値セット
                TryUpdateModel(gameScene);

                //今回データ更新対象
                if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.NextBatter
                    || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterChange
                    || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterGameSet
                    || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterTieBreak)
                {
                    //データ追加
                    Context.GameScenes.Add(gameScene);

                    //試合中
                    if (Game.StatusClass == StatusClass.BeforeGame)
                    {
                        Game.StatusClass = StatusClass.DuringGame;
                    }

                    //タイブレーク
                    if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterTieBreak)
                    {
                        Game.TieBreakStartInning = gameScene.Inning + 1;
                        Game.TieBreakStartBattingOrder = tieBreakStartBattingOrder;
                        Game.TieBreakStartOutCount = tieBreakStartOutCount;
                        Game.TieBreakStartRunnerSceneClass = tieBreakStartRunnerSceneClass;
                    }
                    //チェンジ、試合終了
                    else if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterChange
                    || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterGameSet)
                    {
                        //タイブレークでない、裏終了、次イニングがタイブレーク(一度入力した)
                        if (!IsTieBreak
                            && gameScene.TopButtomClass == TopButtomClass.Buttom
                            && gameScene.Inning + 1 == Game.TieBreakStartInning)
                        {
                            Game.TieBreakStartInning = null;
                            Game.TieBreakStartBattingOrder = null;
                            Game.TieBreakStartOutCount = null;
                            Game.TieBreakStartRunnerSceneClass = null;
                        }
                    }

                    await Context.SaveChangesAsync();
                }

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
                    inningScore = new Models.InningScore
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        Inning = GameScene.Inning,
                        TopButtomClass = GameScene.TopButtomClass,
                        Score = score
                    };

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

                await Context.SaveChangesAsync();

                //仮オーダー削除
                await DeleteTempOrders(Game.GameID, OrderDataClass.Temp);
                await DeleteTempOrders(Game.GameID, OrderDataClass.Change);

                //修正
                if (GameScene.GameSceneID.ZeroToNull() != null)
                {
                    //次回試合シーンID
                    if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.BeforeBatterChange
                        || GameSceneSubmitClass == Enum.GameSceneSubmitClass.BeforeBatterGameSet)
                    {
                        NextGameSceneID = GetNextGameSceneID(Game.GameID, GameScene.GameSceneID);
                    }
                    else
                    {
                        NextGameSceneID = GetNextGameSceneID(Game.GameID, gameScene.GameSceneID);

                        var nextGameScene = await Context.GameScenes
                                                        .FindAsync(NextGameSceneID);

                        if (nextGameScene != null)
                        {
                            //同イニングでない次データあり
                            if ((nextGameScene.Inning != gameScene.Inning
                                || nextGameScene.TopButtomClass != gameScene.TopButtomClass)
                                && !gameScene.ChangeFLG)
                            {
                                //仮データ作成
                                var tempChangeGameScene = new Models.GameScene
                                {
                                    GameSceneDetails = new List<Models.GameSceneDetail>(),
                                    GameSceneRunners = new List<Models.GameSceneRunner>(),
                                    Orders = new List<Models.Order>()
                                };

                                CreateTempChangeData(tempChangeGameScene, gameScene);

                                //データ追加
                                Context.GameScenes.Add(tempChangeGameScene);

                                await Context.SaveChangesAsync();

                                //仮データIDをセット
                                NextGameSceneID = tempChangeGameScene.GameSceneID;
                            }
                        }
                    }
                }


                await tran.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (GameSceneSubmitClass == Enum.GameSceneSubmitClass.NextBatter
                || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterChange
                || GameSceneSubmitClass == Enum.GameSceneSubmitClass.ThisBatterTieBreak
                || GameSceneSubmitClass == Enum.GameSceneSubmitClass.BeforeBatterChange)
            {
                if (NextGameSceneID != null)
                {
                    return RedirectToPage("./Edit", new { gameID = Game.GameID, gameSceneID = NextGameSceneID });
                }
                else
                {
                    return RedirectToPage("./Edit", new { gameID = Game.GameID });
                }
            }
            else
            {
                //試合終了処理
                await base.GameSet(Game.GameID);

                //試合結果へ
                return RedirectToPage("/GameScore/Edit", new { gameID = Game.GameID });
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
            gameScene.InterruptFLG = GameScene.InterruptFLG;
            gameScene.Note = GameScene.Note;

            base.SetEntryInfo(gameScene);

            //試合詳細シーン
            foreach (var gameSceneDetail in BeforeGameSceneDetailList.Where(r => r.DetailResultClass != null))
            {
                var newGameSceneDetail = new Models.GameSceneDetail
                {
                    GameID = gameScene.GameID,
                    TeamID = gameScene.TeamID,
                    MemberID = gameSceneDetail.MemberID,
                    SceneResultClass = SceneResultClass.SceneChange,
                    DetailResultClass = gameSceneDetail.DetailResultClass
                };

                base.SetEntryInfo(newGameSceneDetail);

                gameScene.GameSceneDetails.Add(newGameSceneDetail);
            }

            //試合ランナーシーン
            foreach (var gameSceneRunner in BeforeGameSceneRunnerList)
            {
                var newGameSceneRunner = new Models.GameSceneRunner
                {
                    GameID = gameScene.GameID,
                    TeamID = gameScene.TeamID,
                    MemberID = gameSceneRunner.MemberID,
                    BattingOrder = gameSceneRunner.BattingOrder,
                    SceneResultClass = SceneResultClass.SceneChange,
                    BeforeRunnerClass = gameSceneRunner.BeforeRunnerClass,
                    RunnerClass = gameSceneRunner.RunnerClass,
                    RunnerResultClass = gameSceneRunner.RunnerResultClass
                };

                base.SetEntryInfo(newGameSceneRunner);

                gameScene.GameSceneRunners.Add(newGameSceneRunner);
            }

            //試合シーンアウト＆ランナー
            gameScene.OutCount = GameScene.OutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange && r.RunnerResultClass == RunnerResultClass.Out).Count();
            gameScene.RunnerSceneClass = GetRunnerSceneClass(gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange));

            //試合詳細結果
            foreach (var gameSceneDetail in AfterGameSceneDetailList.Where(r => r.DetailResultClass != null))
            {
                var newGameSceneDetail = new Models.GameSceneDetail
                {
                    GameID = gameScene.GameID,
                    TeamID = gameScene.TeamID,
                    MemberID = gameSceneDetail.MemberID,
                    SceneResultClass = SceneResultClass.Result,
                    DetailResultClass = gameSceneDetail.DetailResultClass
                };

                base.SetEntryInfo(newGameSceneDetail);

                gameScene.GameSceneDetails.Add(newGameSceneDetail);
            }

            //試合ランナー結果
            foreach (var gameSceneRunner in AfterGameSceneRunnerList)
            {
                var newGameSceneRunner = new Models.GameSceneRunner
                {
                    GameID = gameScene.GameID,
                    TeamID = gameScene.TeamID,
                    MemberID = gameSceneRunner.MemberID,
                    BattingOrder = gameSceneRunner.BattingOrder,
                    SceneResultClass = SceneResultClass.Result,
                    BeforeRunnerClass = gameSceneRunner.BeforeRunnerClass,
                    RunnerClass = gameSceneRunner.RunnerClass,
                    RunnerResultClass = gameSceneRunner.RunnerResultClass
                };

                base.SetEntryInfo(newGameSceneRunner);

                gameScene.GameSceneRunners.Add(newGameSceneRunner);
            }

            //試合シーン結果アウト＆ランナー
            gameScene.ResultOutCount = GameScene.OutCount + gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result && r.RunnerResultClass == RunnerResultClass.Out).Count();
            gameScene.ResultRunnerSceneClass = GetRunnerSceneClass(gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result));

            if (gameScene.ResultClass == ResultClass.Change)
            {
                //得点
                gameScene.Run = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
                                                                && (r.RunnerResultClass == RunnerResultClass.Run
                                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptRBI
                                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun
                                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptRBIEarnedRun)).Count();
                //打点
                gameScene.RBI = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
                                                            && (r.RunnerResultClass == RunnerResultClass.Run
                                                             || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun)).Count();
                //自責点
                gameScene.EarnedRun = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.SceneChange
                                                                && (r.RunnerResultClass == RunnerResultClass.Run
                                                                || r.RunnerResultClass == RunnerResultClass.RunExceptRBI)).Count();
            }
            else
            {
                //得点
                gameScene.Run = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
                                                                && (r.RunnerResultClass == RunnerResultClass.Run
                                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptRBI
                                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun
                                                                    || r.RunnerResultClass == RunnerResultClass.RunExceptRBIEarnedRun)).Count();
                //打点
                gameScene.RBI = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
                                                            && (r.RunnerResultClass == RunnerResultClass.Run
                                                             || r.RunnerResultClass == RunnerResultClass.RunExceptEarnedRun)).Count();
                //自責点
                gameScene.EarnedRun = gameScene.GameSceneRunners.Where(r => r.SceneResultClass == SceneResultClass.Result
                                                                && (r.RunnerResultClass == RunnerResultClass.Run
                                                                || r.RunnerResultClass == RunnerResultClass.RunExceptRBI)).Count();
            }

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
                beforeOrders = Context.Orders
                                .Where(r => r.GameID == gameScene.GameID
                                    && r.GameSceneID == LastGameSceneID
                                    && r.OrderDataClass == OrderDataClass.Normal).ToList();
            }

            //オーダー作成
            foreach (var order in beforeOrders)
            {
                var newOrder = new Models.Order
                {
                    GameID = order.GameID,
                    TeamID = order.TeamID,
                    MemberID = order.MemberID,
                    BattingOrder = order.BattingOrder,
                    ParticipationIndex = order.ParticipationIndex,
                    PositionClass = order.PositionClass,
                    ParticipationClass = order.ParticipationClass,
                    OrderDataClass = OrderDataClass.Normal
                };

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

            if (gameScene.InterruptFLG)
            {
                var newOrder = new Models.Order()
                {
                    GameID = gameScene.GameID,
                    TeamID = gameScene.TeamID,
                    MemberID = gameScene.BatterMemberID,
                    BattingOrder = gameScene.BattingOrder,
                    ParticipationIndex = 1,
                    PositionClass = PositionClass.DH,
                    ParticipationClass = ParticipationClass.PinchHitter,
                    OrderDataClass = OrderDataClass.Normal,
                };

                base.SetEntryInfo(newOrder);

                gameScene.Orders.Add(newOrder);
            }
        }

        /// <summary>
        /// 仮チェンジデータ作成(三振チェンジデータ)
        /// </summary>
        /// <param name="gameScene"></param>
        /// <param name="beforeGameScene"></param>
        private void CreateTempChangeData(Models.GameScene gameScene, Models.GameScene beforeGameScene)
        {
            //試合シーン
            gameScene.GameID = beforeGameScene.GameID;
            gameScene.TeamID = beforeGameScene.TeamID;
            gameScene.Inning = beforeGameScene.Inning;
            gameScene.TopButtomClass = beforeGameScene.TopButtomClass;
            gameScene.InningIndex = beforeGameScene.InningIndex + 1;
            gameScene.OffenseDefenseClass = beforeGameScene.OffenseDefenseClass;
            gameScene.PitcherMemberID = beforeGameScene.PitcherMemberID;
            gameScene.HittingDirectionClass = HittingDirectionClass.None;
            gameScene.HitBallClass = HitBallClass.NoHit;
            gameScene.ResultClass = ResultClass.Strikeout;

            var lastBattingOrder = beforeGameScene.BattingOrder;

            //攻撃
            if (beforeGameScene.OffenseDefenseClass == OffenseDefenseClass.Offense)
            {
                var orderList = Context.Orders
                                .Where(r => r.GameID == beforeGameScene.GameID && r.GameSceneID == beforeGameScene.GameSceneID && r.BattingOrder != null && r.OrderDataClass == OrderDataClass.Normal)
                                .ToList();

                if (lastBattingOrder != null)
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
                else
                {
                    //先頭バッター
                    Order = orderList.OrderBy(r => r.BattingOrder).FirstOrDefault();
                }

                gameScene.BattingOrder = Order.BattingOrder;
                gameScene.BatterMemberID = Order.MemberID;
            }
            //守備
            else
            {
                if (lastBattingOrder != null)
                {
                    //相手チームは9人想定
                    if (lastBattingOrder == 9)
                    {
                        gameScene.BattingOrder = 1;
                    }
                    else
                    {
                        gameScene.BattingOrder = lastBattingOrder + 1;
                    }
                }
                else
                {
                    gameScene.BattingOrder = 1;
                }

                gameScene.BatterMemberID = beforeGameScene.BatterMemberID;
            }

            base.SetEntryInfo(gameScene);

            //ランナー
            gameScene.GameSceneRunners = new List<Models.GameSceneRunner>()
            {
                new GameSceneRunner()
                {
                    GameID = beforeGameScene.GameID,
                    TeamID = beforeGameScene.TeamID,
                    MemberID = gameScene.BatterMemberID,
                    BattingOrder = gameScene.BattingOrder,
                    BeforeRunnerClass = RunnerClass.Batter,
                    RunnerClass = RunnerClass.Batter,
                    SceneResultClass = SceneResultClass.Result,
                    RunnerResultClass = RunnerResultClass.Out,
                }
            };

            var beforeGameSceneRunnerList = Context.GameSceneRunners.Where(r => r.GameSceneID == beforeGameScene.GameSceneID
                                            && r.SceneResultClass == SceneResultClass.Result
                                            && (r.RunnerResultClass == RunnerResultClass.OnFirstBase
                                                || r.RunnerResultClass == RunnerResultClass.OnSecondBase
                                                || r.RunnerResultClass == RunnerResultClass.OnThirdBase)
                                            )
                                            .OrderBy(r => r.RunnerResultClass)
                                            .ToList();

            foreach (var beforeGameSceneRunner in beforeGameSceneRunnerList)
            {
                RunnerClass runnerClass;

                if (beforeGameSceneRunner.RunnerResultClass == RunnerResultClass.OnFirstBase)
                {
                    runnerClass = RunnerClass.OnFirstBase;
                }
                else if (beforeGameSceneRunner.RunnerResultClass == RunnerResultClass.OnSecondBase)
                {
                    runnerClass = RunnerClass.OnSecondBase;
                }
                else
                {
                    runnerClass = RunnerClass.OnThirdBase;
                }

                gameScene.GameSceneRunners.Add(new GameSceneRunner()
                {
                    GameID = beforeGameScene.GameID,
                    TeamID = beforeGameScene.TeamID,
                    MemberID = beforeGameSceneRunner.MemberID,
                    BattingOrder = beforeGameSceneRunner.BattingOrder,
                    BeforeRunnerClass = runnerClass,
                    RunnerClass = runnerClass,
                    SceneResultClass = SceneResultClass.SceneChange,
                    RunnerResultClass = beforeGameSceneRunner.RunnerResultClass,
                });

                gameScene.GameSceneRunners.Add(new GameSceneRunner()
                {
                    GameID = beforeGameScene.GameID,
                    TeamID = beforeGameScene.TeamID,
                    MemberID = beforeGameSceneRunner.MemberID,
                    BattingOrder = beforeGameSceneRunner.BattingOrder,
                    BeforeRunnerClass = runnerClass,
                    RunnerClass = runnerClass,
                    SceneResultClass = SceneResultClass.Result,
                    RunnerResultClass = beforeGameSceneRunner.RunnerResultClass,
                });
            }

            foreach (var gameSceneRunner in gameScene.GameSceneRunners)
            {
                base.SetEntryInfo(gameSceneRunner);
            }

            //試合シーンアウト＆ランナー
            gameScene.OutCount = beforeGameScene.ResultOutCount;
            gameScene.RunnerSceneClass = beforeGameScene.ResultRunnerSceneClass;

            //試合シーン結果アウト＆ランナー
            gameScene.ResultOutCount = beforeGameScene.OutCount + 1;
            gameScene.ResultRunnerSceneClass = beforeGameScene.ResultRunnerSceneClass;

            //得点
            gameScene.Run = 0;
            //打点
            gameScene.RBI = 0;
            //自責点
            gameScene.EarnedRun = 0;
            //チェンジ
            gameScene.ChangeFLG = true;

            //オーダー
            var beforeOrders = Context.Orders
                                .Where(r => r.GameID == beforeGameScene.GameID
                                    && r.GameSceneID == beforeGameScene.GameSceneID
                                    && r.OrderDataClass == OrderDataClass.Normal).ToList();

            //オーダー作成
            foreach (var order in beforeOrders)
            {
                var newOrder = new Models.Order
                {
                    GameID = order.GameID,
                    TeamID = order.TeamID,
                    MemberID = order.MemberID,
                    BattingOrder = order.BattingOrder,
                    ParticipationIndex = order.ParticipationIndex,
                    PositionClass = order.PositionClass,
                    ParticipationClass = order.ParticipationClass,
                    OrderDataClass = OrderDataClass.Normal
                };

                base.SetEntryInfo(newOrder);

                gameScene.Orders.Add(newOrder);
            }
        }

        /// <summary>
        /// 最新GameSceneID取得
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="selectGameSceneID"></param>
        /// <returns></returns>
        private int? GetLastGameSceneID(int gameID, int? selectGameSceneID = null)
        {
            int? gameSceneID = null;

            var gameScenes = Context.GameScenes.Where(r => r.GameID == gameID);

            if (!gameScenes.Any())
            {
                return gameSceneID;
            }

            int inning;
            TopButtomClass? topButtomClass;
            int? inningIndex;

            if (selectGameSceneID == null)
            {
                inning = gameScenes.DefaultIfEmpty().Max(r => r.Inning);
                topButtomClass = gameScenes.Where(r => r.Inning == inning).DefaultIfEmpty().Max(r => r.TopButtomClass);
                inningIndex = gameScenes.Where(r => r.Inning == inning && r.TopButtomClass == topButtomClass).DefaultIfEmpty().Max(r => r.InningIndex);
            }
            else
            {
                var selectGameScene = Context.GameScenes.Find(selectGameSceneID);

                if (selectGameScene == null)
                {
                    return gameSceneID;
                }

                //同イニング
                if (selectGameScene.InningIndex > 1)
                {
                    inning = selectGameScene.Inning;
                    topButtomClass = selectGameScene.TopButtomClass;
                    inningIndex = gameScenes.Where(r => r.Inning == inning && r.TopButtomClass == topButtomClass && r.InningIndex < selectGameScene.InningIndex).DefaultIfEmpty().Max(r => r.InningIndex);
                }
                //裏⇒表
                else if (selectGameScene.TopButtomClass == TopButtomClass.Buttom)
                {
                    inning = selectGameScene.Inning;
                    topButtomClass = TopButtomClass.Top;
                    inningIndex = gameScenes.Where(r => r.Inning == inning && r.TopButtomClass == topButtomClass).DefaultIfEmpty().Max(r => r.InningIndex);
                }
                //表⇒前回の裏
                else
                {
                    inning = selectGameScene.Inning - 1;
                    topButtomClass = TopButtomClass.Buttom;
                    inningIndex = gameScenes.Where(r => r.Inning == inning && r.TopButtomClass == topButtomClass).DefaultIfEmpty().Max(r => r.InningIndex);
                }
            }

            var gameScene = gameScenes.Where(r => r.Inning == inning && r.TopButtomClass == topButtomClass && r.InningIndex == inningIndex).FirstOrDefault();

            if (gameScene != null)
            {
                gameSceneID = gameScene.GameSceneID;
            }

            return gameSceneID;
        }

        /// <summary>
        /// 次GameSceneID取得
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="selectGameSceneID"></param>
        /// <returns></returns>
        private int? GetNextGameSceneID(int gameID, int? selectGameSceneID = null)
        {
            int? gameSceneID = null;

            var gameScenes = Context.GameScenes.Where(r => r.GameID == gameID);

            if (!gameScenes.Any())
            {
                return gameSceneID;
            }

            if (selectGameSceneID == null)
            {
                return gameSceneID;
            }

            var selectGameScene = Context.GameScenes.Find(selectGameSceneID);

            if (selectGameScene == null)
            {
                return gameSceneID;
            }

            //同イニング
            var gameScene = gameScenes.Where(r => r.Inning == selectGameScene.Inning && r.TopButtomClass == selectGameScene.TopButtomClass && r.InningIndex == selectGameScene.InningIndex + 1).FirstOrDefault();

            if (gameScene != null)
            {
                gameSceneID = gameScene.GameSceneID;

                return gameSceneID;
            }

            //表⇒裏
            if (selectGameScene.TopButtomClass == TopButtomClass.Top)
            {
                gameScene = gameScenes.Where(r => r.Inning == selectGameScene.Inning && r.TopButtomClass == TopButtomClass.Buttom && r.InningIndex == 1).FirstOrDefault();
            }
            else
            {
                gameScene = gameScenes.Where(r => r.Inning == selectGameScene.Inning + 1 && r.TopButtomClass == TopButtomClass.Top && r.InningIndex == 1).FirstOrDefault();
            }

            if (gameScene != null)
            {
                gameSceneID = gameScene.GameSceneID;

                return gameSceneID;
            }

            return gameSceneID;
        }

        /// <summary>
        /// 結果ランナー初期化(打者初期表示)
        /// </summary>
        /// <param name="gameScene"></param>
        /// <returns></returns>
        private List<Models.GameSceneRunner> InitializeAfterGameSceneRunner(Models.GameScene gameScene)
        {
            //シーンランナー
            return new List<Models.GameSceneRunner>()
            {
                new GameSceneRunner()
                {
                    GameID = gameScene.GameID,
                    TeamID = gameScene.TeamID,
                    MemberID = gameScene.BatterMemberID,
                    BattingOrder = gameScene.BattingOrder,
                    BeforeRunnerClass = RunnerClass.Batter,
                    RunnerClass = RunnerClass.Batter,
                    SceneResultClass = SceneResultClass.Result,
                    RunnerResultClass = RunnerResultClass.Out,
                }
            };
        }

        /// <summary>
        /// 打席前ランナーを結果ランナーからコピー
        /// </summary>
        /// <param name="afterGameSceneRunners"></param>
        /// <returns></returns>
        private List<Models.GameSceneRunner> CopyBeforeGameSceneRunnerListFromAfter(List<Models.GameSceneRunner> afterGameSceneRunners)
        {
            //シーンランナー
            var result = new List<Models.GameSceneRunner>();

            foreach (var afterGameSceneRunner in afterGameSceneRunners.Where(r => r.RunnerClass != RunnerClass.Batter))
            {
                result.Add(new GameSceneRunner()
                {
                    GameID = Game.GameID,
                    TeamID = Game.TeamID,
                    MemberID = afterGameSceneRunner.MemberID,
                    BattingOrder = afterGameSceneRunner.BattingOrder,
                    BeforeRunnerClass = afterGameSceneRunner.RunnerClass,
                    RunnerClass = afterGameSceneRunner.RunnerClass,
                    SceneResultClass = SceneResultClass.SceneChange,
                    RunnerResultClass = afterGameSceneRunner.RunnerResultClass,
                });
            }

            return result;
        }

        /// <summary>
        /// 基準打順(ラスト打順)よりオーダーを並び変え
        /// </summary>
        /// <param name="game"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private List<Models.Order> SortOrderByBaseBattingOrder(List<Models.Order> orders, decimal baseBattingOrder)
        {
            var result = new List<Models.Order>();
            var orderCount = orders.Count();

            foreach (var order in orders)
            {
                result.Add(new Models.Order()
                {
                    GameID = order.GameID,
                    TeamID = order.TeamID,
                    GameSceneID = order.GameSceneID,
                    MemberID = order.MemberID,
                    BattingOrder = order.BattingOrder,
                    ParticipationIndex = order.ParticipationIndex,
                    PositionClass = order.PositionClass,
                    ParticipationClass = order.ParticipationClass,
                    TempBattingOrder = order.BattingOrder > baseBattingOrder ? order.BattingOrder - orderCount : order.BattingOrder,
                });
            }

            //降順
            result = result.OrderByDescending(r => r.TempBattingOrder).ToList();

            return result;
        }

        /// <summary>
        /// タイブレークランナー取得
        /// </summary>
        /// <param name="game"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private List<Models.GameSceneRunner> GetTieBreakRunner(Models.Game game, List<Models.Order> orders)
        {
            var result = new List<Models.GameSceneRunner>();
            var isFirst = false;
            var isSecond = false;
            var isThird = false;

            foreach (var order in orders)
            { 
                //一塁対象
                if((game.TieBreakStartRunnerSceneClass == RunnerSceneClass.First
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.FirstSecond
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.FirstThird
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.FullBase)
                    && !isFirst)
                {
                    result.Add(new GameSceneRunner()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        MemberID = order.MemberID,
                        BattingOrder = order.BattingOrder,
                        BeforeRunnerClass = RunnerClass.OnFirstBase,
                        RunnerClass = RunnerClass.OnFirstBase,
                        SceneResultClass = SceneResultClass.SceneChange,
                        RunnerResultClass = RunnerResultClass.OnFirstBase,
                    });

                    isFirst = true;

                    continue;
                }
                else
                {
                    isFirst = true;
                }

                //二塁対象
                if ((game.TieBreakStartRunnerSceneClass == RunnerSceneClass.Second
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.FirstSecond
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.SecondThird
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.FullBase)
                    && !isSecond)
                {
                    result.Add(new GameSceneRunner()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        MemberID = order.MemberID,
                        BattingOrder = order.BattingOrder,
                        BeforeRunnerClass = RunnerClass.OnSecondBase,
                        RunnerClass = RunnerClass.OnSecondBase,
                        SceneResultClass = SceneResultClass.SceneChange,
                        RunnerResultClass = RunnerResultClass.OnSecondBase,
                    });

                    isSecond = true;

                    continue;
                }
                else
                {
                    isSecond = true;
                }

                //三塁対象
                if ((game.TieBreakStartRunnerSceneClass == RunnerSceneClass.Third
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.FirstThird
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.SecondThird
                    || game.TieBreakStartRunnerSceneClass == RunnerSceneClass.FullBase)
                    && !isThird)
                {
                    result.Add(new GameSceneRunner()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        MemberID = order.MemberID,
                        BattingOrder = order.BattingOrder,
                        BeforeRunnerClass = RunnerClass.OnThirdBase,
                        RunnerClass = RunnerClass.OnThirdBase,
                        SceneResultClass = SceneResultClass.SceneChange,
                        RunnerResultClass = RunnerResultClass.OnThirdBase,
                    });

                    isThird = true;

                    continue;
                }
                else
                {
                    isThird = true;
                }

                //全て処理終了
                if(isFirst
                    && isSecond
                    && isThird)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 割込打順取得
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="gameSceneID"></param>
        /// <param name="battingOrder"></param>
        /// <returns></returns>
        private decimal? GetInterruptBattingOrder(int? gameID, int? gameSceneID, decimal? battingOrder)
        {
            decimal? result = null;

            //オーダー
            var interruptBattingOrder = GameOrderList
                                    .Where(r => r.GameID == gameID
                                        && r.GameSceneID == gameSceneID
                                        && r.BattingOrder != null
                                        && r.BattingOrder < battingOrder
                                        && r.OrderDataClass == OrderDataClass.Normal)
                                    .OrderBy(r => r.BattingOrder)
                                    .FirstOrDefault();

            //先頭打者
            if (interruptBattingOrder == null)
            {
                //割り込み可能
                if ((decimal)0.1 != battingOrder)
                {
                    //打順間を算出
                    result = (Math.Floor((battingOrder.NullToZero()) / 2 * 10) / 10);
                }
            }
            else
            {
                //割り込み可能
                if (interruptBattingOrder.BattingOrder + (decimal)0.1 != battingOrder)
                {
                    //打順間を算出
                    result = interruptBattingOrder.BattingOrder + (Math.Floor((battingOrder.NullToZero() - interruptBattingOrder.BattingOrder.NullToZero()) / 2 * 10) / 10);
                }
            }

            return result;
        }

        /// <summary>
        /// 相手チームオーダー取得
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>
        private List<Models.Order> GetOpponentOrder(Models.Game game, int? memberID)
        {
            var result = new List<Models.Order>();

            for(int i = 1; i <= 9; i++)
            {
                result.Add(new Models.Order()
                {
                    GameID = game.GameID,
                    TeamID = null,
                    GameSceneID = null,
                    MemberID = memberID,
                    BattingOrder = i,
                    ParticipationIndex = 1,
                    PositionClass = null,
                    ParticipationClass = ParticipationClass.Start,
                });
            }

            return result;
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
            if (gameSceneRunner.Any(r => r.RunnerResultClass == RunnerResultClass.OnFirstBase))
            {
                isOnFirstBase = true;
            }

            //二塁
            if (gameSceneRunner.Any(r => r.RunnerResultClass == RunnerResultClass.OnSecondBase))
            {
                isOnSecondBase = true;
            }

            //三塁
            if (gameSceneRunner.Any(r => r.RunnerResultClass == RunnerResultClass.OnThirdBase))
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
                var newOrder = new Models.Order
                {
                    GameID = order.GameID,
                    GameSceneID = null,
                    TeamID = order.TeamID,
                    MemberID = order.MemberID,
                    BattingOrder = order.BattingOrder,
                    ParticipationIndex = order.ParticipationIndex,
                    PositionClass = order.PositionClass,
                    ParticipationClass = order.ParticipationClass,
                    OrderDataClass = OrderDataClass.Temp
                };

                base.SetEntryInfo(newOrder);

                Context.Orders.Add(newOrder);
            }

            await Context.SaveChangesAsync();
        }

    }

}
