using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bmcs.Data;
using Bmcs.Models;

namespace Bmcs.Pages.Member
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
        public Models.Member Member { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //-----------------------------

            var memberToUpdate = new Models.Member();


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
                _context.Members.Add(memberToUpdate);

                await _context.SaveChangesAsync();
            }
            ////-----------------------------
            ///
            //_context.Members.Add(Member);
            //await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
