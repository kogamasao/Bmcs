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
                teamList = Context.Teams.Where(r => r.PublicFLG == true && r.DeleteFLG == false)
                                        .OrderBy(r => r.TeamID);
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TeamIndex);
            //インデックス
            IsIndex = true;

            Team = await PaginatedList<Models.Team>.CreateAsync(
                teamList.AsNoTracking(), pageIndex ?? 1, 20);

            return Page();
        }
    }
}
