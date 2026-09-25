using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Function;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;
using Bmcs.PageHelper;

namespace Bmcs.Pages.Game
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public PaginatedList<Models.Game> Game { get; set; }

        public Models.Team Team { get; set; }

        /// <summary>
        /// 確定前の試合の件数（全ページ分）
        /// </summary>
        public int BeforeFixGameCount { get; set; }

        public async Task<IActionResult> OnGetAsync(string teamID, int? pageIndex)
        {
            IsMyTeam = false;
            
            if (teamID == null
                || teamID == HttpContext.Session.GetString(SessionConstant.TeamID))
            {
                IsMyTeam = true;
                teamID = HttpContext.Session.GetString(SessionConstant.TeamID);
            }

            if (string.IsNullOrEmpty(teamID))
            {
                return NotFound();
            }

            List<Models.Game> gameList;

            if (!base.IsAdmin())
            {
                gameList = await Context.Games
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID
                        && r.Team.DeleteFLG == false
                        && ((r.Team.PublicFLG == true && !IsMyTeam) || IsMyTeam)
                        && r.DeleteFLG == false)
                    .OrderByDescending(r => r.GameDate)
                    .ThenBy(r => r.GameID)
                    .ToListAsync();
            }
            else
            {
                gameList = await Context.Games
                    .Include(m => m.Team)
                    .Where(r => r.TeamID == teamID
                        && r.DeleteFLG == false)
                    .OrderByDescending(r => r.GameDate)
                    .ThenBy(r => r.GameID)
                    .ToListAsync();
            }

            Game = PaginatedList<Models.Game>.Create(
                   gameList.AsQueryable().AsNoTracking(), pageIndex ?? 1, 20);

            //存在しないページを指定された場合は1ページ目へ戻す（Member/Index と同じ）
            //※そのまま表示すると「まだ試合が登録されていません」と誤表示され、ページ送りも描画されない
            if (Game.TotalPages > 0 && Game.PageIndex > Game.TotalPages)
            {
                return RedirectToPage("./Index", new { teamID });
            }

            //確定前の試合の件数（全ページ分）
            //※以前はビューで表示中の1ページ分だけを数えており、2ページ目以降の確定前の試合が案内されなかった
            BeforeFixGameCount = gameList.Count(r => r.StatusClass == StatusClass.BeforeFix);

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            //非公開・削除済みの他チームは、チーム名も表示しない（URL直接指定での確認を防ぐ）
            if (Team == null
                || (!base.IsAdmin() && !IsMyTeam && (Team.DeleteFLG || !Team.PublicFLG)))
            {
                return NotFound();
            }

            //システム管理データ
            if (IsMyTeam)
            {
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MyTeamGameIndex);
            }
            else
            {
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.PublicGameIndex);
            }

            //パンくず
            AddTeamBreadcrumb(Team);
            BreadcrumbList.Add(PageHelper.NavigationLink.Create("/Game/Index", "試合一覧", new Dictionary<string, string> { { "teamID", Team.TeamID } }));

            //検索エンジンに登録する（公開チームのみ）
            if (IsSearchTargetTeam(Team))
            {
                SetIndex("/Game/Index", new { teamID = Team.TeamID, pageIndex = Game.PageIndex > 1 ? Game.PageIndex : (int?)null });
            }

            MetaTitle = Team.TeamName + "の試合一覧" + (Game.PageIndex > 1 ? "（" + Game.PageIndex + "ページ目）" : string.Empty);
            MetaDescription = SeoContent.Truncate(SeoContent.TeamTitle(Team) + "の試合一覧。試合ごとのスコアボードと投手・打撃成績を見ることができます。");

            return Page();
        }
    }
}
