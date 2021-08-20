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
        private readonly Bmcs.Data.BmcsContext _context;

        public IndexModel(Bmcs.Data.BmcsContext context)
        {
            _context = context;
        }

        public IList<Models.Team> Team { get;set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsLogin())
            {
                return Redirect("/Error");
            }

            Team = await _context.Teams.ToListAsync();

            return Page();
        }

        //public async Task OnGetAsync()
        //{
        //    if(!IsLogin())
        //    {
        //        Redirect("/Error");
        //    }

        //    Team = await _context.Teams.ToListAsync();
        //}
    }
}
