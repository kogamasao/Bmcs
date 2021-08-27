using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Game
{
    public class CreateModel : PageModel
    {
        private readonly Bmcs.Data.BmcsContext _context;

        public CreateModel(Bmcs.Data.BmcsContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["TeamID"] = new SelectList(_context.Teams, "TeamID", "TeamID");
            return Page();
        }

        [BindProperty]
        public Models.Game Game { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Games.Add(Game);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
