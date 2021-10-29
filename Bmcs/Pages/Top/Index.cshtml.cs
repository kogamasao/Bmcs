using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Enum;
using Bmcs.Function;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;

namespace Bmcs.Pages.Top
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public Models.Message PublicMessage { get; set; }

        public Models.Message PrivateMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TopInformation);

            int? messageID = null;

            var tempMessageList = await Context.Messages
                                    .Where(r => r.PublicFLG)
                                    .ToListAsync();

            var messageIDList = tempMessageList.Select(r => new { MessageID = r.ParentMessageID == null ? r.MessageID : r.ParentMessageID.NullToZero() })
                                            .GroupBy(r => r.MessageID)
                                            .Select(r => messageID = r.Key );

            PublicMessage = await Context.Messages
                                    .Include(r => r.UserAccount)
                                    .Include(r => r.Team)
                                    .Where(r => messageIDList.Contains(r.MessageID))
                                    .OrderByDescending(r => r.UpdateDatetime)
                                    .FirstOrDefaultAsync();

            if(base.IsLogin())
            { 
                tempMessageList = await Context.Messages
                                        .Where(r => (!r.PublicFLG) && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID) || r.PrivateTeamID == HttpContext.Session.GetString(SessionConstant.TeamID)))
                                        .ToListAsync();

                messageIDList = tempMessageList.Select(r => new { MessageID = r.ParentMessageID == null ? r.MessageID : r.ParentMessageID.NullToZero() }).GroupBy(r => r.MessageID).Select(r => messageID = r.Key);

                PrivateMessage = await Context.Messages
                                        .Include(r => r.UserAccount)
                                        .Include(r => r.Team)
                                        .Where(r => messageIDList.Contains(r.MessageID))
                                        .OrderByDescending(r => r.UpdateDatetime)
                                        .FirstOrDefaultAsync();
            }

            return Page();
        }

    }
}
