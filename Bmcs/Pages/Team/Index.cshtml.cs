using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Team
{
    public class IndexModel : PageModelBase
    {
        public IndexModel(BmcsContext context) : base(context)
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
