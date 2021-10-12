using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Enum;
using Bmcs.Function;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using System.Reflection;

namespace Bmcs.Pages.Message
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Team Team { get; set; }

        [BindProperty]
        public MessagePageClass MessagePageClass { get; set; }

        [BindProperty]
        public string SelectTeamID { get; set; }

        [BindProperty]
        public int? MessageID { get; set; }

        public List<Models.Message> MessageList { get; set; }

        public async Task<IActionResult> OnGetAsync(MessagePageClass messagePageClass, string teamID, int? messageID)
        {
            if (string.IsNullOrEmpty(teamID))
            {
                teamID = HttpContext.Session.GetString(SessionConstant.TeamID);
            }

            if (!string.IsNullOrEmpty(teamID))
            {
                Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

                if (Team == null)
                {
                    return NotFound();
                }

                if (!base.IsAdmin()
                    && (Team.DeleteFLG == true
                        || (Team.PublicFLG == false && Team.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID))
                        )
                    )
                {
                    return NotFound();
                }
            }

            //メッセージ
            MessageList = new List<Models.Message>();

            MessageList = await Context.Messages.ToListAsync();

            //タイトル
            ViewData[ViewDataConstant.Title] = "メッセージ";

            if (messagePageClass == MessagePageClass.Public)
            {
                ViewData[ViewDataConstant.Title] += "(公開)";
            }
            else if (messagePageClass == MessagePageClass.PublicTeam)
            {
                ViewData[ViewDataConstant.Title] += "(投稿)";
            }
            else if (messagePageClass == MessagePageClass.RelatedTeam)
            {
                ViewData[ViewDataConstant.Title] += "(関連)";
            }
            else if (messagePageClass == MessagePageClass.Private)
            {
                ViewData[ViewDataConstant.Title] += "(非公開)";
            }

            //引数セット
            MessagePageClass = messagePageClass;
            SelectTeamID = teamID;
            MessageID = messageID;

            return Page();

        }

    }
}
