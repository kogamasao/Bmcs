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

namespace Bmcs.Pages.Team
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public IList<Models.Team> Team { get;set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (base.IsAdmin())
            { 
                //全チーム
                Team = await Context.Teams.ToListAsync();
            }
            else
            {
                //公開チームのみ
                Team = await Context.Teams.Where(r => r.PublicFLG == true && r.DeleteFLG == false).ToListAsync();
            }

            return Page();
        }
    }
}
