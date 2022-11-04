using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;

namespace Bmcs.Pages.Member
{
    public class DetailsModel : PageModelBase<DetailsModel>
    {
        public DetailsModel(ILogger<DetailsModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public Models.Member Member { get; set; }

        public List<Models.GameScorePitcher> GameScorePitcherList { get; set; }

        public List<Models.GameScoreFielder> GameScoreFielderList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Member = await Context.Members
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.MemberID == id);

            if (Member == null)
            {
                return NotFound();
            }

            if (!base.IsAdmin()
                && (Member.Team.DeleteFLG == true
                    || (Member.Team.PublicFLG == false && Member.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID))
                    || Member.DeleteFLG == true
                    )
                )
            {
                return NotFound();
            }

            //集計項目
            var totalingItem = new TotalingItem()
            {
                Year = 0,
                GameClass = GameClass.All,
                TeamCategoryClass = TeamCategoryClass.All,
                UseBallClass = UseBallClass.All,
            };

            //投手スコア
            GameScorePitcherList = new List<GameScorePitcher>();

            //投手スコアデータ
            var gameScorePitcherList = await Context.GameScorePitchers
                      .Include(r => r.Game)
                      .Include(r => r.Member)
                      .Include(r => r.Team)
                      .Where(r => r.MemberID == id && (r.Game.StatusClass == StatusClass.EndGame || r.Game.StatusClass == StatusClass.EndGameLock) && r.Game.DeleteFLG == false)
                      .ToListAsync();

            if(gameScorePitcherList != null && gameScorePitcherList.Any())
            { 
                //通算集計処理
                GameScorePitcherList.AddRange(base.TotalingGameScorePitcher(gameScorePitcherList, totalingItem));

                foreach(var year in gameScorePitcherList.GroupBy(r => r.Game.GameDate.Year).Select(r => r.Key))
                {
                    //対象年
                    totalingItem.Year = year;
                    //対象年集計
                    GameScorePitcherList.AddRange(base.TotalingGameScorePitcher(gameScorePitcherList, totalingItem));
                }
            }

            //集計項目
            totalingItem = new TotalingItem()
            {
                Year = 0,
                GameClass = GameClass.All,
                TeamCategoryClass = TeamCategoryClass.All,
                UseBallClass = UseBallClass.All,
            };

            //野手スコア
            GameScoreFielderList = new List<GameScoreFielder>();

            //野手スコアデータ
            var gameScoreFielderList = await Context.GameScoreFielders
                      .Include(r => r.Game)
                      .Include(r => r.Member)
                      .Include(r => r.Team)
                      .Where(r => r.MemberID == id && (r.Game.StatusClass == StatusClass.EndGame || r.Game.StatusClass == StatusClass.EndGameLock) && r.Game.DeleteFLG == false)
                      .ToListAsync();

            if (gameScoreFielderList != null && gameScoreFielderList.Any())
            {
                //通算集計処理
                GameScoreFielderList.AddRange(base.TotalingGameScoreFielder(gameScoreFielderList, totalingItem));

                foreach (var year in gameScoreFielderList.GroupBy(r => r.Game.GameDate.Year).Select(r => r.Key))
                {
                    //対象年
                    totalingItem.Year = year;
                    //対象年集計
                    GameScoreFielderList.AddRange(base.TotalingGameScoreFielder(gameScoreFielderList, totalingItem));
                }
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberDetails);
            //インデックス
            IsIndex = true;

            return Page();
        }
    }
}
