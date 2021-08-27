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

namespace Bmcs.Pages.Top
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public Models.SystemAdmin SystemAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FirstOrDefaultAsync();
     
            return Page();
        }

    }
}
