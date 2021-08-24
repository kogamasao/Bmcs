using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Member
{
    public class EditModel : PageModel
    {
        private readonly Bmcs.Data.BmcsContext _context;

        public EditModel(Bmcs.Data.BmcsContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Member Member { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Member = await _context.Members
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.MemberID == id);

            if (Member == null)
            {
                return NotFound();
            }
            ViewData["TeamID"] = new SelectList(_context.Teams, "TeamID", "TeamID");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Member).State = EntityState.Modified;

            try
            {
                var memberToUpdate = await _context.Members.FindAsync(Member.MemberID);

                if (memberToUpdate == null)
                {
                    return NotFound();
                }

                if (await TryUpdateModelAsync<Models.Member>(
                    memberToUpdate
                  , "member"
                  , s => s.TeamID
                  , s => s.MemberName
                  , s => s.MemberClass
                  , s => s.BatClass
                  , s => s.ThrowClass
                  , s => s.PositionGroupClass
                  , s => s.UniformNumber
                  , s => s.MessageDetail
                  , s => s.DeleteFLG
                  , s => s.UpdateUserID
                  , s => s.UpdateDatetime))
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(Member.MemberID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberID == id);
        }
    }
}
