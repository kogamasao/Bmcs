using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Inquiry
{
    public class DeleteModel : PageModel
    {
        private readonly Bmcs.Data.BmcsContext _context;

        public DeleteModel(Bmcs.Data.BmcsContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Inquiry Inquiry { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Inquiry = await _context.Inquirys.FirstOrDefaultAsync(m => m.InquiryID == id);

            if (Inquiry == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Inquiry = await _context.Inquirys.FindAsync(id);

            if (Inquiry != null)
            {
                _context.Inquirys.Remove(Inquiry);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
