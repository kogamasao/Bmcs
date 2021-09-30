using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Bmcs.Constans;
using Bmcs.Data;
using Bmcs.Enum;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Models
{
    /// <summary>
    /// ページモデルベース
    /// </summary>
    public abstract class PageModelBase<T> : PageModel
    {
        public PageModelBase(ILogger<T> logger, BmcsContext context) 
        {
            Logger = logger;
            Context = context;
        }

        public PageModelBase(ILogger<T> logger)
        {
            Logger = logger;
        }

        public PageModelBase(BmcsContext context)
        {
            Context = context;
        }

        /// <summary>
        /// ログ
        /// </summary>
        public readonly ILogger<T> Logger;

        /// <summary>
        /// DBコンテキスト
        /// </summary>
        public readonly BmcsContext Context;

        /// <summary>
        /// マイチームフラグ
        /// </summary>
        public bool IsMyTeam { get; set; }

        /// <summary>
        /// チームID
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// メンバーID空必要フラグ
        /// </summary>
        public bool IsMemberIDNeedEmpty { get; set; }

        /// <summary>
        /// チームIDリスト
        /// </summary>
        public SelectList TeamIDList
        { 
            get
            {
                return AddFirstItem(new SelectList(Context.Teams.Where(r => r.DeleteFLG == false), nameof(Team.TeamID), nameof(Team.TeamIDName), string.Empty)
                    , new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// メンバーIDリスト
        /// </summary>
        public SelectList MemberIDList
        {
            get
            {
                return AddFirstItem(new SelectList(Context.Members.Where(r => r.TeamID == this.TeamID && r.DeleteFLG == false).OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty)
                    , new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// メンバーIDリスト(空なし)
        /// </summary>
        public SelectList MemberIDNoEmptyList
        {
            get
            {
                return new SelectList(Context.Members.Where(r => r.TeamID == this.TeamID && r.DeleteFLG == false).OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);
            }
        }

        /// <summary>
        /// メンバーIDリスト(相手チーム含む)
        /// </summary>
        public SelectList MemberIDIncludeOpponentMemberList
        {
            get
            {
                var memberIDIncludeOpponentMemberList = new SelectList(Context.Members
                                                .Where(r => (r.TeamID == this.TeamID || r.SystemDataFLG) && r.DeleteFLG == false)
                                                .OrderBy(r => r.SystemDataFLG)
                                                .ThenBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);

                return AddFirstItem(memberIDIncludeOpponentMemberList, new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// 相手メンバーIDリスト
        /// </summary>
        public SelectList OpponentMemberIDList
        {
            get
            {
                return AddFirstItem(new SelectList(Context.Members
                                    .Where(r => r.SystemDataFLG)
                                        .OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty)
                    , new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// 相手投手メンバーIDリスト
        /// </summary>
        public SelectList OpponentPitcherMemberIDList
        {
            get
            {
                return new SelectList(Context.Members
                                    .Where(r => r.SystemDataFLG && r.PositionGroupClass == PositionGroupClass.Pitcher)
                                        .OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);
            }
        }

        /// <summary>
        /// 相手野手メンバーIDリスト
        /// </summary>
        public SelectList OpponentFielderMemberIDList
        {
            get
            {
                return new SelectList(Context.Members
                                    .Where(r => r.SystemDataFLG && r.PositionGroupClass != PositionGroupClass.Pitcher)
                                        .OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);
            }
        }

        /// <summary>
        /// 試合区分リスト
        /// </summary>
        public SelectList GameClassList
        {
            get
            {
                return EnumClass.GetSelectList<GameClass>();
            }
        }

        /// <summary>
        /// チーム分類区分リスト
        /// </summary>
        public SelectList TeamCategoryClassList
        {
            get
            {
                return EnumClass.GetSelectList<TeamCategoryClass>();
            }
        }

        /// <summary>
        /// 使用球リスト
        /// </summary>
        public SelectList UseBallClassList
        {
            get
            {
                return EnumClass.GetSelectList<UseBallClass>();
            }
        }

        /// <summary>
        /// メンバー区分リスト
        /// </summary>
        public SelectList MemberClassList
        {
            get
            {
                return EnumClass.GetSelectList<MemberClass>();
            }
        }

        /// <summary>
        /// 打席区分リスト
        /// </summary>
        public SelectList BatClassList
        {
            get
            {
                return EnumClass.GetSelectList<BatClass>();
            }
        }

        /// <summary>
        /// 投区分リスト
        /// </summary>
        public SelectList ThrowClassList
        {
            get
            {
                return EnumClass.GetSelectList<ThrowClass>();
            }
        }

        /// <summary>
        /// 投球フォーム区分リスト
        /// </summary>
        public SelectList ThrowFormClassList
        {
            get
            {
                return EnumClass.GetSelectList<ThrowFormClass>();
            }
        }

        /// <summary>
        /// ポジショングループ区分リスト
        /// </summary>
        public SelectList PositionGroupClassList
        {
            get
            {
                return EnumClass.GetSelectList<PositionGroupClass>();
            }
        }

        /// <summary>
        /// ポジション区分リスト
        /// </summary>
        public SelectList PositionClassList
        {
            get
            {
                return EnumClass.GetSelectList<PositionClass>();
            }
        }

        /// <summary>
        /// 天候区分リスト
        /// </summary>
        public SelectList WeatherClassList
        {
            get
            {
                return EnumClass.GetSelectList<WeatherClass>();
            }
        }

        /// <summary>
        /// 勝敗区分リスト
        /// </summary>
        public SelectList WinLoseClassList
        {
            get
            {
                return EnumClass.GetSelectList<WinLoseClass>();
            }
        }

         /// <summary>
        /// 試合入力タイプ区分リスト
        /// </summary>
        public SelectList GameInputTypeClassList
        {
            get
            {
                return EnumClass.GetSelectList<GameInputTypeClass>(false);
            }
        }

        /// <summary>
        /// ステータス区分リスト
        /// </summary>
        public SelectList StatusClassList
        {
            get
            {
                return EnumClass.GetSelectList<StatusClass>();
            }
        }

        /// <summary>
        /// 攻守区分リスト
        /// </summary>
        public SelectList OffenseDefenseClassList
        {
            get
            {
                return EnumClass.GetSelectList<OffenseDefenseClass>();
            }
        }

        /// <summary>
        /// 攻守区分リスト
        /// </summary>
        public SelectList BatFirstBatSecondClassList
        {
            get
            {
                return EnumClass.GetSelectList<BatFirstBatSecondClass>(false);
            }
        }

        /// <summary>
        /// 表裏区分リスト
        /// </summary>
        public SelectList TopButtomClassList
        {
            get
            {
                return EnumClass.GetSelectList<TopButtomClass>(false);
            }
        }

        /// <summary>
        /// 出場区分リスト
        /// </summary>
        public SelectList ParticipationClassList
        {
            get
            {
                return EnumClass.GetSelectList<ParticipationClass>();
            }
        }

        /// <summary>
        /// 打球方向区分リスト
        /// </summary>
        public SelectList HittingDirectionClassList
        {
            get
            {
                return EnumClass.GetSelectList<HittingDirectionClass>(false);
            }

        }
        /// <summary>
        /// 打球区分リスト
        /// </summary>
        public SelectList HitBallClassList
        {
            get
            {
                return EnumClass.GetSelectList<HitBallClass>(false);
            }
        }

        /// <summary>
        /// 結果区分リスト
        /// </summary>
        public SelectList ResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<ResultClass>(false);
            }
        }

        /// <summary>
        /// ランナー区分リスト
        /// </summary>
        public SelectList RunnerSceneClassList
        {
            get
            {
                return EnumClass.GetSelectList<RunnerSceneClass>();
            }
        }

        /// <summary>
        /// シーン結果区分リスト
        /// </summary>
        public SelectList SceneResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<SceneResultClass>();
            }
        }

        /// <summary>
        /// シーン詳細結果区分リスト
        /// </summary>
        public SelectList BeforeDetailResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<DetailResultClass>();
            }
        }

        /// <summary>
        /// 結果詳細結果区分リスト
        /// </summary>
        public SelectList AfterDetailResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<DetailResultClass>(true, (int)DetailResultClass.Error);
            }

        }
        /// <summary>
        /// ランナー結果区分リスト
        /// </summary>
        public SelectList RunnerResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<RunnerResultClass>(false);
            }
        }

        /// <summary>
        /// 試合スコア投手区分リスト
        /// </summary>
        public SelectList GameScorePicherClassList
        {
            get
            {
                return EnumClass.GetSelectList<GameScorePitcherClass>();
            }
        }

        /// <summary>
        /// メッセージ区分リスト
        /// </summary>
        public SelectList MessageClassList
        {
            get
            {
                return EnumClass.GetSelectList<MessageClass>();
            }
        }


        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);
        }

        public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            base.OnPageHandlerSelected(context);
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);
        }

        /// <summary>
        /// ログイン判定
        /// </summary>
        /// <returns></returns>
        public bool IsLogin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.UserAccountID)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 管理者判定
        /// </summary>
        /// <returns></returns>
        public bool IsAdmin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.AdminFLG)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// エントリ情報セット
        /// </summary>
        /// <returns></returns>
        public void SetEntryInfo<TModel>(TModel dataModelBase) where TModel : DataModelBase
        {
            dataModelBase.EntryDatetime = DateTime.Now;
            dataModelBase.EntryUserID = HttpContext.Session.GetString(SessionConstant.UserAccountID);
            dataModelBase.UpdateDatetime = dataModelBase.EntryDatetime;
            dataModelBase.UpdateUserID = dataModelBase.EntryUserID;
        }

        /// <summary>
        /// エントリ情報セット
        /// </summary>
        /// <returns></returns>
        public void SetUpdateInfo<TModel>(TModel dataModelBase) where TModel : DataModelBase
        {
            dataModelBase.UpdateDatetime = DateTime.Now;
            dataModelBase.UpdateUserID = HttpContext.Session.GetString(SessionConstant.UserAccountID);
        }

        /// <summary>
        /// 先頭行に要素を追加
        /// </summary>
        /// <param name="selectList"></param>
        /// <param name="firstItem"></param>
        /// <returns></returns>
        private SelectList AddFirstItem(SelectList selectList, SelectListItem firstItem)
        {
            List<SelectListItem> newList = selectList.ToList();
            newList.Insert(0, firstItem);

            var selectedItem = newList.FirstOrDefault(item => item.Selected);
            var selectedItemValue = string.Empty;

            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(newList, "Value", "Text", selectedItemValue);
        }

        /// <summary>
        /// 仮オーダー削除
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public async Task DeleteTempOrders(int? gameID, OrderDataClass? orderDataClass)
        {
            //オーダー
            var tempOrders = await Context.Orders
                                .Where(r => r.GameID == gameID
                                    && r.OrderDataClass == orderDataClass).ToListAsync();

            Context.Orders.RemoveRange(tempOrders);

            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// 試合終了
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public async Task GameSet(int? gameID)
        {
            //削除対象試合投手スコア
            var deleteGameScorePitchers = await Context.GameScorePitchers
                                            .Where(r => r.GameID == gameID).ToListAsync();

            Context.GameScorePitchers.RemoveRange(deleteGameScorePitchers);

            //削除対象試合野手スコア
            var deleteGameScoreFielders = await Context.GameScoreFielders
                                            .Where(r => r.GameID == gameID).ToListAsync();

            Context.GameScoreFielders.RemoveRange(deleteGameScoreFielders);

            //試合
            var game = await Context.Games.FirstOrDefaultAsync(r => r.GameID == gameID);

            //イニングスコア
            var inningScores = await Context.InningScores.Where(r => r.GameID == gameID).ToListAsync();

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

            //確定前
            game.StatusClass = StatusClass.BeforeFix;

            SetUpdateInfo(game);

            //全試合シーン
            var orders = await Context.Orders.Include(r => r.Member)
                                            .Include(r => r.GameScene)
                                            .Where(r => r.GameID == gameID).ToListAsync();

            //全試合シーン
            var gameScenes = await Context.GameScenes.Include(r => r.BatterMember)
                                                    .Include(r => r.PitcherMember)
                                                    .Where(r => r.GameID == gameID).ToListAsync();

            //全試合シーン詳細
            var gameSceneDetails = await Context.GameSceneDetails.Include(r => r.GameScene)
                                                   .Include(r => r.Member)
                                                   .Where(r => r.GameID == gameID).ToListAsync();

            //全試合シーンランナー
            var gameSceneRunners = await Context.GameSceneRunners.Include(r => r.GameScene)
                                                   .Include(r => r.Member)
                                                   .Where(r => r.GameID == gameID).ToListAsync();




            //投手オーダー
            var tempPitcherOrderList = orders
                       .Where(r => r.PositionClass == PositionClass.Pitcher && r.GameSceneID != null)
                       .Select(r => new { MemberID = r.MemberID, Inning = r.GameScene.Inning, InningIndex = r.GameScene.InningIndex })
                       .ToList();

            var pitcherOrderList = tempPitcherOrderList
                        .GroupBy(r => r.MemberID)
                        .Select(r => new { MemberID = r.Key, Inning = r.Min(s => s.Inning), InningIndex = r.Min(s => s.InningIndex) })
                        .ToList();

            //投手スコア
            var gameScorePitcherList = new List<Models.GameScorePitcher>();

            //投手成績集計
            foreach (var pitcherMemberID in gameScenes.Select(r => r.PitcherMemberID).Distinct())
            {
                //相手投手
                if (gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID).FirstOrDefault().PitcherMember.SystemDataFLG)
                {
                    continue;
                }

                var gameScorePitcher = new GameScorePitcher()
                {
                    GameID = gameID,
                    TeamID = game.TeamID,
                    ScoreIndex = 999,
                    MemberID = pitcherMemberID,
                    Win = 0,
                    Lose = 0,
                    Hold = 0,
                    Save = 0,
                    Starter = 0,
                    CompleteGame = 0,
                };

                //先発(初回の先頭で自分が投げている)
                if (gameScenes.Any(r => r.Inning == 1 && r.InningIndex == 1 && r.PitcherMemberID == pitcherMemberID))
                {
                    gameScorePitcher.Starter = 1;
                }

                //完投(自分以外の投手が投げていない)
                if (!gameScenes.Any(r => r.TopButtomClass != myTeamOffenceTopButtomClass && r.PitcherMemberID != pitcherMemberID))
                {
                    gameScorePitcher.Starter = 1;
                }

                //OUT
                var outCount = gameSceneRunners.Where(r => r.GameScene.PitcherMemberID == pitcherMemberID && r.SceneResultClass == SceneResultClass.Result && r.RunnerResultClass == RunnerResultClass.Out).Count();
                //イニング(3OUTで1イニング)
                gameScorePitcher.Inning = (Math.Floor(System.Convert.ToDecimal(outCount / 3) * 10)) / 10;
                //打席
                gameScorePitcher.PlateAppearance = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass != ResultClass.Change).Count();
                //打数
                gameScorePitcher.AtBat = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass <= ResultClass.FieldersChoice || r.ResultClass >= ResultClass.SingleHit)).Count();
                //被安打
                gameScorePitcher.Hit = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass >= ResultClass.SingleHit && r.ResultClass <= ResultClass.HomeRun)).Count();
                //被本塁打
                gameScorePitcher.HomeRun = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass == ResultClass.HomeRun).Count();
                //失点
                gameScorePitcher.Run = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID).DefaultIfEmpty().Sum(r => r.Run);
                //自責点
                gameScorePitcher.EarnedRun = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID).DefaultIfEmpty().Sum(r => r.EarnedRun);
                //与四球
                gameScorePitcher.FourBall = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass == ResultClass.FourBalls).Count();
                //与死球
                gameScorePitcher.DeadBall = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass == ResultClass.DeadBall).Count();
                //得点圏打席
                gameScorePitcher.ScoringPositionPlateAppearance = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass != ResultClass.Change && r.RunnerSceneClass >= RunnerSceneClass.Second).Count();
                //得点圏打数
                gameScorePitcher.ScoringPositionAtBat = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass <= ResultClass.FieldersChoice || r.ResultClass >= ResultClass.SingleHit) && r.RunnerSceneClass >= RunnerSceneClass.Second).Count();
                //得点圏被安打
                gameScorePitcher.ScoringPositionHit = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass >= ResultClass.SingleHit && r.ResultClass <= ResultClass.HomeRun) && r.RunnerSceneClass >= RunnerSceneClass.Second).Count();
                //奪三振
                gameScorePitcher.StrikeOut = gameScenes.Where(r => r.PitcherMemberID == pitcherMemberID && (r.ResultClass == ResultClass.Strikeout || r.ResultClass == ResultClass.MissedStrikeout || r.ResultClass == ResultClass.UncaughtThirdStrike)).Count();
                //牽制死
                gameScorePitcher.PickOffBallOut = gameSceneDetails.Where(r => r.GameScene.PitcherMemberID == pitcherMemberID && r.MemberID == pitcherMemberID && r.DetailResultClass == DetailResultClass.PickOffBallOut).Count();
                //WP
                gameScorePitcher.WildPitch = gameSceneDetails.Where(r => r.GameScene.PitcherMemberID == pitcherMemberID && r.MemberID == pitcherMemberID && r.DetailResultClass == DetailResultClass.WildPitch).Count();
                //ボーク
                gameScorePitcher.Balk = gameSceneDetails.Where(r => r.GameScene.PitcherMemberID == pitcherMemberID && r.MemberID == pitcherMemberID && r.DetailResultClass == DetailResultClass.Balk).Count();

                SetEntryInfo(gameScorePitcher);

                gameScorePitcherList.Add(gameScorePitcher);
            }

            //登板順
            foreach (var x in pitcherOrderList.OrderBy(r => r.Inning).ThenBy(r => r.InningIndex).Select((order, index) => new { order, index }))
            {
                var gameScorePitcher = gameScorePitcherList.Where(r => r.MemberID == x.order.MemberID).FirstOrDefault();

                if (gameScorePitcher != null)
                {
                    gameScorePitcher.ScoreIndex = x.index + 1;
                }
            }

            Context.GameScorePitchers.AddRange(gameScorePitcherList);

            //野手オーダー
            var fielderOrderList = orders
                        .Where(r => r.BattingOrder != null)
                        .GroupBy(r => r.MemberID)
                        .Select(r => new { MemberID = r.Key, BattingOrder = r.Min(s => s.BattingOrder), ParticipationIndex = r.Min(s => s.ParticipationIndex) })
                        .ToList();

            //野手スコア
            var gameScoreFielderList = new List<Models.GameScoreFielder>();

            //野手成績集計
            var orderMemberID = orders.Select(r => r.MemberID).Distinct();
            var batterMemberID = gameScenes.Select(r => r.BatterMemberID).Distinct();
            var detailMemberID = gameSceneDetails.Select(r => r.MemberID).Distinct();
            var runnerMemberID = gameSceneRunners.Select(r => r.MemberID).Distinct();

            foreach (var fielderMemberID in orderMemberID.Union(batterMemberID.Union(detailMemberID.Union(runnerMemberID))))
            {
                //相手野手
                if (Context.Members.Find(fielderMemberID).SystemDataFLG)
                {
                    continue;
                }

                var gameScoreFielder = new GameScoreFielder()
                {
                    GameID = gameID,
                    TeamID = game.TeamID,
                    ScoreIndex = 999,
                    MemberID = fielderMemberID,
                };


                var batterGameScenes = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass != ResultClass.Change);

                gameScoreFielder.Detail = string.Empty;

                //打撃詳細
                foreach (var batterGameScene in batterGameScenes.OrderBy(r => r.Inning).ThenBy(r => r.TopButtomClass).ThenBy(r => r.InningIndex))
                {
                    if (batterGameScene.HittingDirectionClass != HittingDirectionClass.None)
                    {
                        gameScoreFielder.Detail += batterGameScene.HittingDirectionClass.GetEnumName();
                    }

                    if (batterGameScene.HitBallClass != HitBallClass.NoHit)
                    {
                        gameScoreFielder.Detail += batterGameScene.HitBallClass.GetEnumName();
                    }

                    gameScoreFielder.Detail += batterGameScene.ResultClass.GetEnumName();

                    gameScoreFielder.Detail += "　";
                }

                gameScoreFielder.Detail = gameScoreFielder.Detail.Trim();

                //打席
                gameScoreFielder.PlateAppearance = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass != ResultClass.Change).Count();
                //打数
                gameScoreFielder.AtBat = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass <= ResultClass.FieldersChoice || r.ResultClass >= ResultClass.SingleHit)).Count();
                //安打
                gameScoreFielder.Hit = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass >= ResultClass.SingleHit && r.ResultClass <= ResultClass.HomeRun)).Count();
                //二塁打
                gameScoreFielder.DoubleHit = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.DoubleHit).Count();
                //三塁打
                gameScoreFielder.TripleHit = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.TripleHit).Count();
                //本塁打
                gameScoreFielder.HomeRun = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.HomeRun).Count();
                //塁打
                gameScoreFielder.TotalBase = (gameScoreFielder.HomeRun * 4) + (gameScoreFielder.TripleHit * 3) + (gameScoreFielder.DoubleHit * 2) + (gameScoreFielder.Hit - gameScoreFielder.DoubleHit - gameScoreFielder.TripleHit - gameScoreFielder.HomeRun);
                //打点
                if (gameScenes.Any(r => r.BatterMemberID == fielderMemberID))
                {
                    gameScoreFielder.RBI = gameScenes.Where(r => r.BatterMemberID == fielderMemberID).DefaultIfEmpty().Sum(r => r.RBI);
                }
                else
                {
                    gameScoreFielder.RBI = 0;
                }
                //得点
                gameScoreFielder.Run = gameSceneRunners.Where(r => r.MemberID == fielderMemberID && r.SceneResultClass == SceneResultClass.Result && r.RunnerResultClass >= RunnerResultClass.Run).Count();
                //盗塁企画数
                gameScoreFielder.StolenBasePlan = gameSceneDetails.Where(r => r.MemberID == fielderMemberID && (r.DetailResultClass == DetailResultClass.StolenBaseSccess || r.DetailResultClass == DetailResultClass.StolenBaseOut)).Count();
                //盗塁
                gameScoreFielder.StolenBase = gameSceneDetails.Where(r => r.MemberID == fielderMemberID && r.DetailResultClass == DetailResultClass.StolenBaseSccess).Count();
                //四球
                gameScoreFielder.FourBall = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.FourBalls).Count();
                //死球
                gameScoreFielder.DeadBall = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.DeadBall).Count();
                //犠打
                gameScoreFielder.Sacrifice = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.Sacrifice).Count();
                //犠牲
                gameScoreFielder.SacrificeFly = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.SacrificeFly).Count();
                //残塁
                gameScoreFielder.LeftOnBase = gameSceneRunners.Where(r => r.MemberID == fielderMemberID && r.GameScene.ChangeFLG && r.SceneResultClass == SceneResultClass.Result && (r.RunnerResultClass >= RunnerResultClass.OnFirstBase && r.RunnerResultClass <= RunnerResultClass.OnThirdBase)).Count();
                //得点圏打席
                gameScoreFielder.ScoringPositionPlateAppearance = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass != ResultClass.Change && r.RunnerSceneClass >= RunnerSceneClass.Second).Count();
                //得点圏打数
                gameScoreFielder.ScoringPositionAtBat = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass <= ResultClass.FieldersChoice || r.ResultClass >= ResultClass.SingleHit) && r.RunnerSceneClass >= RunnerSceneClass.Second).Count();
                //得点圏安打
                gameScoreFielder.ScoringPositionHit = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass != ResultClass.Change && (r.ResultClass >= ResultClass.SingleHit && r.ResultClass <= ResultClass.HomeRun) && r.RunnerSceneClass >= RunnerSceneClass.Second).Count();
                //三振
                gameScoreFielder.StrikeOut = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && (r.ResultClass == ResultClass.Strikeout || r.ResultClass == ResultClass.MissedStrikeout || r.ResultClass == ResultClass.UncaughtThirdStrike)).Count();
                //併殺
                gameScoreFielder.DoublePlay = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.DoublePlay).Count();
                //敵失策
                gameScoreFielder.Error = gameScenes.Where(r => r.BatterMemberID == fielderMemberID && r.ResultClass == ResultClass.Error).Count();

                //捕手
                var catcherGameSceneIDs = orders.Where(r => r.MemberID == fielderMemberID && r.PositionClass == PositionClass.Catcher).Select(r => r.GameSceneID).Distinct();

                //被盗塁企画数
                gameScoreFielder.StolenBasePlaned = 0;
                //盗塁阻止数
                gameScoreFielder.StopStolenBase = 0;

                foreach (var catcherGameSceneID in catcherGameSceneIDs)
                {
                    var stolenBaseSccessCount = gameSceneDetails.Where(r => r.GameSceneID == catcherGameSceneID && r.GameScene.TopButtomClass != myTeamOffenceTopButtomClass && r.DetailResultClass == DetailResultClass.StolenBaseSccess).Count();
                    var stolenBaseOutCount = gameSceneDetails.Where(r => r.GameSceneID == catcherGameSceneID && r.GameScene.TopButtomClass != myTeamOffenceTopButtomClass && r.DetailResultClass == DetailResultClass.StolenBaseOut).Count();

                    gameScoreFielder.StolenBasePlaned += stolenBaseSccessCount + stolenBaseOutCount;
                    gameScoreFielder.StopStolenBase += stolenBaseOutCount;
                }

                //補殺
                gameScoreFielder.Assist = gameSceneDetails.Where(r => r.MemberID == fielderMemberID && r.DetailResultClass == DetailResultClass.AssistOut).Count();
                //失策
                gameScoreFielder.OwnError = gameSceneDetails.Where(r => r.MemberID == fielderMemberID && r.DetailResultClass == DetailResultClass.Error).Count();
                //PB
                gameScoreFielder.PassBall = gameSceneDetails.Where(r => r.MemberID == fielderMemberID && r.DetailResultClass == DetailResultClass.PassBall).Count();

                SetEntryInfo(gameScoreFielder);

                gameScoreFielderList.Add(gameScoreFielder);
            }

            //打順順
            foreach (var x in fielderOrderList.OrderBy(r => r.BattingOrder).ThenBy(r => r.ParticipationIndex).Select((order, index) => new { order, index }))
            {
                var gameScoreFielder = gameScoreFielderList.Where(r => r.MemberID == x.order.MemberID).FirstOrDefault();

                if (gameScoreFielder != null)
                {
                    gameScoreFielder.ScoreIndex = x.index + 1;
                }
            }

            Context.GameScoreFielders.AddRange(gameScoreFielderList);

            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// 投手成績集計
        /// </summary>
        /// <param name="gameScorePitcherList"></param>
        /// <param name="totalingItem"></param>
        /// <returns></returns>
        public List<GameScorePitcher> TotalingGameScorePitcher(List<GameScorePitcher> gameScorePitcherList, TotalingItem totalingItem)
        {
            var result = new List<GameScorePitcher>();

            foreach(var gameScorePitcher in gameScorePitcherList)
            {
                result.Add(gameScorePitcher);
            }

            return result;
        }
    }
}
