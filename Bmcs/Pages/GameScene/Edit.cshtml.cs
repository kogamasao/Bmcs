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

        public IList<Models.Member> MemberList { get; set; }

        public IList<Models.Member> OpponentMemberList { get; set; }




        [BindProperty]
        public Models.Order Order { get; set; }

        [BindProperty]
        public IList<Models.Order> OnlyDefenseList { get; set; }

        [BindProperty]
        public Models.Game Game { get; set; }

        public int? GameSceneID { get; set; }

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

            //試合シーンID
            GameSceneID = gameSceneID;
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

                //先攻
                if(Game.BatFirstBatSecondClass == BatFirstBatSecondClass.First)
                {
                    Order = await Context.Orders
                            .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.BattingOrder == 1)
                            .FirstOrDefaultAsync();

                    GameScene = new Models.GameScene()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        InningIndex = 1,
                        BattingOrder = 1,
                        OffenseDefenseClass = OffenseDefenseClass.Offense,
                        SceneOutCount = 0,
                        SceneRunnerClass = RunnerClass.None,
                        PitcherMemberID = System.Convert.ToInt32(base.OpponentPitcherMemberIDList.FirstOrDefault().Value),
                        BatterMemberID = Order.MemberID,
                    };
                }
                //後攻
                else
                {
                    Order = await Context.Orders
                            .Where(r => r.GameID == Game.GameID && r.GameSceneID == null && r.PositionClass == PositionClass.Pitcher)
                            .FirstOrDefaultAsync();

                    GameScene = new Models.GameScene()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        InningIndex = 1,
                        BattingOrder = 1,
                        OffenseDefenseClass = OffenseDefenseClass.Defense,
                        SceneOutCount = 0,
                        SceneRunnerClass = RunnerClass.None,
                        PitcherMemberID = Order.MemberID,
                        BatterMemberID = System.Convert.ToInt32(base.OpponentFielderMemberIDList.FirstOrDefault().Value),
                    };
                }

                //結果ランナー(打者を初期表示)
                AfterGameSceneRunnerList = new List<Models.GameSceneRunner>()
                {
                    new GameSceneRunner()
                    {
                        GameID = Game.GameID,
                        TeamID = Game.TeamID,
                        MemberID = GameScene.BatterMemberID,
                        SceneResultClass = SceneResultClass.Result,
                        RunnerResultClass = RunnerResultClass.Out,
                    }
                };

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
            GameSceneID = -1;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                ////再取得
                //Game = await Context.Games
                //    .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == Game.GameID);
                ////試合シーンID
                //GameSceneID = OrderList.FirstOrDefault().GameSceneID;
                ////チームID
                //base.TeamID = Game.TeamID;

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
                ////削除対象
                //var deleteOrderList = await Context.Orders
                //                        .Where(m => m.GameID == OrderList.FirstOrDefault().GameID
                //                            && m.GameSceneID == OrderList.FirstOrDefault().GameSceneID).ToListAsync();

                ////前回データ削除
                //Context.Orders.RemoveRange(deleteOrderList);

                ////データ作成
                //var orderList = new List<Models.Order>();

                ////POST値セット
                //this.TryUpdateModel(orderList);
                ////データ追加
                //Context.Orders.AddRange(orderList);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("./Index", new { teamID = Game.TeamID });

        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="member"></param>
        private void TryUpdateModel(List<Models.Order> orderList)
        {
            ////守備のみを追加
            //foreach (var order in OnlyDefenseList)
            //{
            //    OrderList.Add(order);
            //}

            //foreach (var order in OrderList)
            //{
            //    var newOrder = new Models.Order();

            //    newOrder.GameID = order.GameID;
            //    newOrder.TeamID = order.TeamID;
            //    newOrder.GameSceneID = order.GameSceneID;
            //    newOrder.MemberID = order.MemberID;
            //    newOrder.BattingOrder = order.BattingOrder;
            //    newOrder.ParticipationIndex = order.ParticipationIndex;
            //    newOrder.PositionClass = order.PositionClass;
            //    newOrder.ParticipationClass = order.ParticipationClass;

            //    base.SetEntryInfo(order);

            //    orderList.Add(order);
            //}
        }
    }
}
