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
using Bmcs.Constans;
using Bmcs.Enum;
using Bmcs.Function;
using Bmcs.PageHelper;

namespace Bmcs.Pages.Team
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public PaginatedList<Models.Team> Team { get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            IQueryable<Models.Team> teamList;

            if (base.IsAdmin())
            {
                //全チーム
                teamList = Context.Teams.OrderBy(r => r.TeamID);

            }
            else
            {
                //公開チームのみ
                //※サンプルデータのチームは除く。実在の球団名・選手名を使っており、誰でも書き換えられるため、
                //  検索エンジンに登録する一覧に載せない（体験は「サンプルチームで体験する」から行う）
                teamList = Context.Teams.Where(r => r.PublicFLG == true && r.DeleteFLG == false
                                                 && !SystemConstant.SampleDataTeamIDList.Contains(r.TeamID))
                                        .OrderBy(r => r.TeamID);
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TeamIndex);
            Team = await PaginatedList<Models.Team>.CreateAsync(
                teamList.AsNoTracking(), pageIndex ?? 1, 20);

            //存在しないページを指定された場合は1ページ目へ戻す（Game/Index・Member/Index と同じ）
            //※そのまま表示すると、中身の無いページが「◯ページ目」として検索エンジンに登録される
            if (Team.TotalPages > 0 && Team.PageIndex > Team.TotalPages)
            {
                return RedirectToPage("./Index");
            }

            //検索エンジンに登録する
            //※2ページ目以降も別の内容のため、それぞれを正規URLとする（1ページ目にまとめると、2ページ目以降のチームが見つからなくなる）
            SetIndex("/Team/Index", new { pageIndex = Team.PageIndex > 1 ? Team.PageIndex : (int?)null });
            MetaTitle = "公開チーム一覧" + (Team.PageIndex > 1 ? "（" + Team.PageIndex + "ページ目）" : string.Empty);
            MetaDescription = "Bmcs で成績を公開している草野球・ソフトボールのチームの一覧です。各チームの試合結果・チーム成績・個人成績を見ることができます。";

            return Page();
        }
    }
}
