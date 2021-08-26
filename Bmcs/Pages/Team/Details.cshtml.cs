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
    public class DetailsModel : PageModelBase
    {
        public DetailsModel(BmcsContext context) : base(context)
        {

        }

        public Models.Team Team { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == id);

            if (Team == null || !Team.PublicFLG)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
