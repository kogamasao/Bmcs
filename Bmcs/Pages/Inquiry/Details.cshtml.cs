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
    public class DetailsModel : PageModelBase<DetailsModel>
    {
        public DetailsModel(ILogger<DetailsModel> logger, BmcsContext context) : base(logger, context)
        {

        }

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
    }
}
