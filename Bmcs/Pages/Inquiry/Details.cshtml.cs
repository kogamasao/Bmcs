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
    public class DetailsModel : PageModel
    {
        private readonly Bmcs.Data.BmcsContext _context;

        public DetailsModel(Bmcs.Data.BmcsContext context)
        {
            _context = context;
        }

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
    }
}
