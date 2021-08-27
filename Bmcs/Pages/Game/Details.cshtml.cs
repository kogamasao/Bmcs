using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Game
{
    public class DetailsModel : PageModel
    {
        private readonly Bmcs.Data.BmcsContext _context;

        public DetailsModel(Bmcs.Data.BmcsContext context)
        {
            _context = context;
        }

        public Models.Game Game { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Game = await _context.Games
                .Include(g => g.Team).FirstOrDefaultAsync(m => m.GameID == id);

            if (Game == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
