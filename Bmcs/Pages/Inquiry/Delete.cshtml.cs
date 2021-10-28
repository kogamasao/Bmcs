using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Inquiry
{
    public class DeleteModel : PageModelBase<DeleteModel>
    {
        public DeleteModel(ILogger<DeleteModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Inquiry Inquiry { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || !base.IsLogin() || !base.IsAdmin())
            {
                return NotFound();
            }

            Inquiry = await Context.Inquirys.FirstOrDefaultAsync(m => m.InquiryID == id);

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

            Inquiry = await Context.Inquirys.FindAsync(id);

            if (Inquiry != null)
            {
                Context.Inquirys.Remove(Inquiry);
                await Context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
