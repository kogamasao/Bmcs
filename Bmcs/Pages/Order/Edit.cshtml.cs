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
                || (gameSceneID == null
                    && Game.StatusClass != Enum.StatusClass.BeforeGame)
                )
            {
                return NotFound();
            }

            //既存データ取得
            OrderList = await Context.Orders
                .Include(o => o.Game)
                .Include(o => o.GameScene)
                .Include(o => o.Member)
                .Include(o => o.Team)
                .Where(m => m.GameID == gameID
                    && m.GameSceneID == gameSceneID)
                .OrderBy(r => r.BattingOrder)
                .ToListAsync();

            if(!OrderList.Any())
            {
                //前回の試合より
                OrderList = await Context.Orders
                    .Include(o => o.Game)
                    .Include(o => o.GameScene)
                    .Include(o => o.Member)
                    .Include(o => o.Team)
                    .Where(m => m.GameID <= gameID
                        && m.GameSceneID == null
                        && m.TeamID == Game.TeamID
                        )
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
            //チームID
            base.TeamID = Game.TeamID;

            //タイトル
            if(gameSceneID == null)
            {
                ViewData[ViewDataConstant.Title] = "スターティングオーダー";
            }
            else
            {
                ViewData[ViewDataConstant.Title] = "選手交代";
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

                //削除対象
                var deleteOrderList = await Context.Orders
                                        .Where(m => m.GameID == OrderList.FirstOrDefault().GameID
                                            && m.GameSceneID == OrderList.FirstOrDefault().GameSceneID).ToListAsync();

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

            return RedirectToPage("./Index", new { teamID = Game.TeamID });

        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="member"></param>
        private void TryUpdateModel(List<Models.Order> orderList)
        {
            foreach (var order in OrderList)
            {
                var newOrder = new Models.Order();

                newOrder.GameID = order.GameID;
                newOrder.TeamID = order.TeamID;
                newOrder.GameSceneID = order.GameSceneID;
                newOrder.MemberID = order.MemberID;
                newOrder.BattingOrder = order.BattingOrder;
                newOrder.ParticipationIndex = order.ParticipationIndex;
                newOrder.PositionClass = order.PositionClass;
                newOrder.ParticipationClass = order.ParticipationClass;

                base.SetEntryInfo(order);

                orderList.Add(order);
            }
        }
    }
}
