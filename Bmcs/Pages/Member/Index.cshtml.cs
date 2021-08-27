using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Member
{
    public class IndexModel : PageModelBase
    {
        public IndexModel(BmcsContext context) : base(context)
        {

        }

        public IList<Models.Member> Member { get;set; }

        public async Task OnGetAsync()
        {
            Member = await Context.Members
                .Include(m => m.Team).ToListAsync();
        }
    }
}
