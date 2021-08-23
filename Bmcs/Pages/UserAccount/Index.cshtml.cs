using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.UserAccount
{
    public class IndexModel : PageModel
    {
        private readonly Bmcs.Data.BmcsContext _context;

        public IndexModel(Bmcs.Data.BmcsContext context)
        {
            _context = context;
        }

        public IList<Models.UserAccount> UserAccount { get;set; }

        public async Task OnGetAsync()
        {
            UserAccount = await _context.UserAccounts
                .Include(u => u.Team).ToListAsync();
        }
    }
}
