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

namespace Bmcs.Pages.Order
{
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public IList<Models.Order> OrderList { get; set; }

        [BindProperty]
        public IList<Models.Order> OnlyDefenseList { get; set; }

        [BindProperty]
        public Models.Game Game { get; set; }

        [BindProperty]
        public int? GameSceneID { get; set; }

        [BindProperty]
        public bool IsDuringGame { get; set; }

        [BindProperty]
        public decimal InterruptBattingOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(int? gameID, int? gameSceneID = null, bool isDuringGame = false)
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
                || (!isDuringGame
                    && Game.StatusClass != Enum.StatusClass.BeforeGame)
                )
            {
                return NotFound();
            }

            var maxOrderDataClass = await Context.Orders
                                        .Where(m => m.GameID == gameID)
                                        .DefaultIfEmpty()
                                        .MaxAsync(r => r.OrderDataClass);

            //既存データ取得
            OrderList = await Context.Orders
                .Include(o => o.Game)
                .Include(o => o.GameScene)
                .Include(o => o.Member)
                .Include(o => o.Team)
                .Where(m => m.GameID == gameID
                    && ((isDuringGame && m.OrderDataClass == maxOrderDataClass) || (!isDuringGame && m.GameSceneID == null && m.OrderDataClass == OrderDataClass.Normal))
                    )
                .OrderBy(r => r.BattingOrder)
                .ToListAsync();

            if(!OrderList.Any())
            {
                //前回試合ID
                var lastGameID = await Context.Orders
                                        .Where(m => m.GameID < gameID
                                            && m.GameSceneID == null
                                            && m.OrderDataClass == OrderDataClass.Normal
                                            && m.TeamID == Game.TeamID
                                            )
                                        .DefaultIfEmpty()
                                        .MaxAsync(r => r.GameID);

                //前回の試合より
                OrderList = await Context.Orders
                    .Include(o => o.Game)
                    .Include(o => o.Member)
                    .Include(o => o.Team)
                    .Where(m => m.GameID == lastGameID
                            && m.GameSceneID == null
                            && m.OrderDataClass == OrderDataClass.Normal
                            && m.TeamID == Game.TeamID)
                .OrderBy(r => r.BattingOrder)
                .ToListAsync();

                //初試合
                if (!OrderList.Any())
                {
                    for(var i = 1; i <= 9; i++)
                    {
                        var order = new Models.Order()
                        {
                            GameID = gameID,
                            TeamID = Game.TeamID,
                            GameSceneID = null,
                            MemberID = null,
                            BattingOrder = i,
                            ParticipationIndex = 1,
                            PositionClass = (PositionClass)System.Enum.ToObject(typeof(PositionClass), i),
                            ParticipationClass = ParticipationClass.Start,
                            OrderDataClass = OrderDataClass.Normal,
                        };

                        OrderList.Add(order);
                    }
                }
                else
                {
                    //前回オーダーを初期表示
                    foreach(var order in OrderList)
                    {
                        order.GameID = gameID;
                    }
                }
            }

            //試合シーンID
            GameSceneID = gameSceneID;
            //試合中フラグ
            IsDuringGame = isDuringGame;
            //チームID
            base.TeamID = Game.TeamID;

            //タイトル
            if(!isDuringGame)
            {
                ViewData[ViewDataConstant.Title] = "スターティングオーダー";
            }
            else
            {
                ViewData[ViewDataConstant.Title] = "選手交代";
            }

            //守備のみ
            OnlyDefenseList = new List<Models.Order>();

            foreach(var order in OrderList.Where(r => r.BattingOrder == null).ToList())
            {
                OnlyDefenseList.Add(order);
                OrderList.Remove(order);
            }

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

                //再取得
                Game = await Context.Games
                    .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == Game.GameID);
                //チームID
                base.TeamID = Game.TeamID;

                //未入力チェック
                if (OrderList.Any(r => r.MemberID == null || r.PositionClass == null)
                    || OnlyDefenseList.Any(r => r.MemberID == null || r.PositionClass == null))
                {
                    ModelState.AddModelError(nameof(Models.Game) + "." + nameof(Models.Game.GameID), "未指定の行があります。不要であれば行削除してください。");

                    return Page();
                }

                //投手チェック
                if (OrderList.Where(r => r.PositionClass == PositionClass.Pitcher).Count()
                    + OnlyDefenseList.Where(r => r.PositionClass == PositionClass.Pitcher).Count() != 1)
                {
                    ModelState.AddModelError(nameof(Models.Game) + "." + nameof(Models.Game.GameID), "投手は必ず一人指定してください。");

                    return Page();
                }

                //捕手チェック
                if (OrderList.Where(r => r.PositionClass == PositionClass.Catcher).Count()
                    + OnlyDefenseList.Where(r => r.PositionClass == PositionClass.Catcher).Count() != 1)
                {
                    ModelState.AddModelError(nameof(Models.Game) + "." + nameof(Models.Game.GameID), "捕手は必ず一人指定してください。");

                    return Page();

                }
                //削除対象
                var deleteOrderList = await Context.Orders
                                        .Where(m => m.GameID == OrderList.FirstOrDefault().GameID
                                            && m.GameSceneID == OrderList.FirstOrDefault().GameSceneID
                                            && m.OrderDataClass == OrderList.FirstOrDefault().OrderDataClass).ToListAsync();

                //前回データ削除).ToListAsync();

                //前回データ削除
                Context.Orders.RemoveRange(deleteOrderList);

                //データ作成
                var orderList = new List<Models.Order>();

                //POST値セット
                this.TryUpdateModel(orderList);
                //データ追加
                Context.Orders.AddRange(orderList);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if(IsDuringGame)
            {
                return RedirectToPage("/GameScene/Edit", new { gameID = Game.GameID, gameSceneID = GameSceneID, isOrderChange = true });
            }
            else
            {
                return RedirectToPage("/GameScene/Edit", new { gameID = Game.GameID, gameSceneID = GameSceneID });
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="member"></param>
        private void TryUpdateModel(List<Models.Order> orderList)
        {
            //守備のみを追加
            foreach (var order in OnlyDefenseList)
            {
                OrderList.Add(order);
            }

            foreach (var order in OrderList)
            {
                var newOrder = new Models.Order
                {
                    GameID = order.GameID,
                    TeamID = order.TeamID,
                    GameSceneID = order.GameSceneID,
                    MemberID = order.MemberID,
                    BattingOrder = order.BattingOrder,
                    ParticipationIndex = order.ParticipationIndex,
                    PositionClass = order.PositionClass,
                    ParticipationClass = order.ParticipationClass
                };

                //試合中
                if (IsDuringGame)
                { 
                    newOrder.OrderDataClass = OrderDataClass.Change;
                    newOrder.ParticipationIndex += 1;
                    newOrder.ParticipationClass = ParticipationClass.Defense;
                }
                else
                {
                    newOrder.OrderDataClass = order.OrderDataClass;
                }

                base.SetEntryInfo(newOrder);

                orderList.Add(newOrder);
            }
        }
    }
}
