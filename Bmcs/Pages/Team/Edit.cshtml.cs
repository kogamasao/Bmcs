﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Microsoft.Extensions.Logging;
using Bmcs.Enum;
using Bmcs.Function;

namespace Bmcs.Pages.Team
{
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Team Team { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            if (id == null)
            {
                id = HttpContext.Session.GetString(SessionConstant.TeamID);
            }
            else
            {
                if (!base.IsAdmin())
                {
                    return NotFound();
                }
            }

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == id);

            if (Team == null)
            {
                return NotFound();
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TeamEdit);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                //データ更新
                var team = await Context.Teams.FindAsync(Team.TeamID);

                if (team == null)
                {
                    return NotFound();
                }

                //POST値セット
                this.TryUpdateModel(team);
                //更新情報セット
                base.SetUpdateInfo(team);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if(base.IsAdmin())
            { 
                return RedirectToPage("./Index");
            }
            else
            {
                return RedirectToPage("/Top/Index");
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="team"></param>
        private void TryUpdateModel(Models.Team team)
        {
            team.TeamName = Team.TeamName;
            team.TeamAbbreviation = Team.TeamAbbreviation;

            //パスワード変更時のみ
            if(!string.IsNullOrEmpty(Team.TeamPassword))
            { 
                team.TeamPassword = Team.TeamPassword.ChangeHashValue();
            }
            
            team.RepresentativeName = Team.RepresentativeName;
            team.TeamCategoryClass = Team.TeamCategoryClass;
            team.UseBallClass = Team.UseBallClass;
            team.ActivityBase = Team.ActivityBase;
            team.TeamNumber = Team.TeamNumber;
            team.TeamEmailAddress = Team.TeamEmailAddress;
            team.MessageDetail = Team.MessageDetail;
            team.PublicFLG = Team.PublicFLG;
        }
    }
}
