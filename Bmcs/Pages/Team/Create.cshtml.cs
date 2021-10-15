using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Bmcs.Function;
using Microsoft.Extensions.Logging;

namespace Bmcs.Pages.Team
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Models.Team Team { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                //チームIDチェック
                var dbTeam = Context.Teams.FirstOrDefault(r => r.TeamID == Team.TeamID);

                if (dbTeam != null || (dbTeam != null && dbTeam.TeamID == Team.TeamID))
                {
                    ModelState.AddModelError(nameof(Models.Team) + "." + nameof(Models.Team.TeamID), "入力したチームIDは既に使用されています。");

                    return Page();
                }

                //データ作成
                var team = new Models.Team();

                //POST値セット
                this.TryUpdateModel(team);
                //エントリ情報セット
                base.SetEntryInfo(team);

                Context.Teams.Add(team);

                if (!base.IsAdmin())
                {
                    //ユーザ情報取得
                    var userAccount = Context.UserAccounts.FirstOrDefault(r => r.UserAccountID == HttpContext.Session.GetString(SessionConstant.UserAccountID).NullToEmpty());

                    if (userAccount != null)
                    {
                        userAccount.TeamID = team.TeamID.NullToEmpty();
                        //更新情報セット
                        base.SetUpdateInfo(userAccount);
                    }

                    //ログイン情報セット
                    HttpContext.Session.SetString(SessionConstant.TeamID, team.TeamID.NullToEmpty());
                }

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (base.IsAdmin())
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
            team.TeamID = Team.TeamID;
            team.TeamName = Team.TeamName;
            team.TeamPassword = Team.TeamPassword;
            team.TeamAbbreviation = Team.TeamAbbreviation;
            team.RepresentativeName = Team.RepresentativeName;
            team.TeamCategoryClass = Team.TeamCategoryClass;
            team.UseBallClass = Team.UseBallClass;
            team.ActivityBase = Team.ActivityBase;
            team.TeamNumber = Team.TeamNumber;
            team.TeamEmailAddress = Team.TeamEmailAddress;
            team.MessageDetail = Team.MessageDetail;
            team.PublicFLG = Team.PublicFLG;
            team.DeleteFLG = false;
        }
    }
}
