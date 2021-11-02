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
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Enum;

namespace Bmcs.Pages.Member
{
    public class DeleteModel : PageModelBase<DeleteModel>
    {
        public DeleteModel(ILogger<DeleteModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Member Member { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!base.IsLogin())
            {
                return NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            Member = await Context.Members
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.MemberID == id);

            if (Member == null
                || (Member.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID)
                    && !base.IsAdmin())
                )
            {
                return NotFound();
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberDelete);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var member = await Context.Members.FindAsync(id);

                if (member != null)
                {
                    member.DeleteFLG = true;
                    await Context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("./Index", new { teamID = Member.TeamID });
        }
    }
}
