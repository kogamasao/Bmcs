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

        /// <summary>
        /// 未回答のアンケート（無い場合はnull）
        /// ※ログイン時に「あとで回答する」を選んだ場合でも、ここから回答できるようにする
        /// </summary>
        public Models.Survey UnansweredSurvey { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TopInformation);

            int? messageID = null;

            //※送信元チームが非公開・削除済みの公開メッセージと、削除済みのメッセージは出さない（Message/Index と同じ判定）
            var tempMessageList = await Context.Messages
                                    .Where(r => r.PublicFLG && !r.DeleteFLG && r.Team.PublicFLG && !r.Team.DeleteFLG)
                                    .ToListAsync();

            var messageIDList = tempMessageList.Select(r => new { MessageID = r.ParentMessageID == null ? r.MessageID : r.ParentMessageID.NullToZero() })
                                            .GroupBy(r => r.MessageID)
                                            .Select(r => messageID = r.Key );

            PublicMessage = await Context.Messages
                                    .Include(r => r.UserAccount)
                                    .Include(r => r.Team)
                                    .Where(r => messageIDList.Contains(r.MessageID) && !r.DeleteFLG && r.Team.PublicFLG && !r.Team.DeleteFLG)
                                    .OrderByDescending(r => r.UpdateDatetime)
                                    .FirstOrDefaultAsync();

            if(base.IsLogin())
            { 
                tempMessageList = await Context.Messages
                                        .Where(r => (!r.PublicFLG) && !r.DeleteFLG && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID) || r.PrivateTeamID == HttpContext.Session.GetString(SessionConstant.TeamID)))
                                        .ToListAsync();

                messageIDList = tempMessageList.Select(r => new { MessageID = r.ParentMessageID == null ? r.MessageID : r.ParentMessageID.NullToZero() }).GroupBy(r => r.MessageID).Select(r => messageID = r.Key);

                PrivateMessage = await Context.Messages
                                        .Include(r => r.UserAccount)
                                        .Include(r => r.Team)
                                        .Where(r => messageIDList.Contains(r.MessageID))
                                        .OrderByDescending(r => r.UpdateDatetime)
                                        .FirstOrDefaultAsync();

                //未回答のアンケートがある場合は案内を表示する（管理者は対象外）
                if (!base.IsAdmin())
                {
                    UnansweredSurvey = await base.GetUnansweredSurveyAsync();
                }
            }

            //インデックス
            IsIndex = true;

            return Page();
        }

    }
}
