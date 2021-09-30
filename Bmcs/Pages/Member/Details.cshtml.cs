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
            var totalingItem = new TotalingItem();

            //投手スコア
            GameScorePitcherList = new List<GameScorePitcher>();

            //投手スコアデータ
            var gameScorePitcherList = await Context.GameScorePitchers
                      .Include(r => r.Member)
                      .Include(r => r.Team)
                      .Where(r => r.MemberID == id)
                      .ToListAsync();

            if(gameScorePitcherList != null && gameScorePitcherList.Any())
            { 
                //集計処理
                GameScorePitcherList.AddRange(base.TotalingGameScorePitcher(gameScorePitcherList, totalingItem));
            }

            //野手スコア
            GameScoreFielderList = new List<GameScoreFielder>();

            //野手スコアデータ
            var gameScoreFielderList = await Context.GameScoreFielders
                      .Include(r => r.Member)
                      .Include(r => r.Team)
                      .Where(r => r.MemberID == id)
                      .ToListAsync();

            GameScoreFielderList.AddRange(gameScoreFielderList);

            return Page();
        }
    }
}
