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

        public Models.UserAccount UserAccount { get; set; }

        public Models.Team Team { get; set; }

        [BindProperty]
        public Models.Message Message { get; set; }

        [BindProperty]
        public MessagePageClass MessagePageClass { get; set; }

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

            //メッセージリスト
            MessageList = new List<Models.Message>();

            if(messageID == null)
            { 
                var tempMessageList = await Context.Messages
                                        .Include(r => r.UserAccount)
                                        .Include(r => r.Team)
                                        .Where(r => ((messagePageClass == MessagePageClass.Public) && (r.PublicFLG))
                                                || ((messagePageClass == MessagePageClass.PublicTeam) && (r.PublicFLG) && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID)) && (r.MessageClass == MessageClass.Post))
                                                || ((messagePageClass == MessagePageClass.RelatedTeam) && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID) || r.PrivateTeamID == HttpContext.Session.GetString(SessionConstant.TeamID)))
                                                || ((messagePageClass == MessagePageClass.Private) && (!r.PublicFLG) && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID) || r.PrivateTeamID == HttpContext.Session.GetString(SessionConstant.TeamID)))
                                        )
                                        .ToListAsync();

                if(messagePageClass == MessagePageClass.PublicTeam)
                {
                    MessageList = tempMessageList;
                }
                else
                {
                    var messageIDList = tempMessageList.Select(r => new { MessageID = r.ParentMessageID == null ? r.MessageID : r.ParentMessageID.NullToZero() }).GroupBy(r => r.MessageID).Select(r => MessageID = r.Key);

                    MessageList = await Context.Messages
                                            .Include(r => r.UserAccount)
                                            .Include(r => r.Team)
                                            .Where(r => messageIDList.Contains(r.MessageID)).ToListAsync();
                }
            }
            else
            {
                MessageList = await Context.Messages
                                    .Include(r => r.UserAccount)
                                    .Include(r => r.Team)
                                    .Where(r => r.MessageID == messageID || r.ParentMessageID == messageID)
                                    .ToListAsync();

            }

            if (base.IsLogin())
            { 
                //ユーザアカウント
                UserAccount = await Context.UserAccounts.FindAsync(HttpContext.Session.GetString(SessionConstant.UserAccountID));

                //メッセージ
                Message = new Models.Message()
                {
                    TeamID = teamID,
                    UserAccountID = UserAccount.UserAccountID,
                };
            }

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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                Team = await Context.Teams.FindAsync(Message.TeamID);

                UserAccount = await Context.UserAccounts.FindAsync(Message.UserAccountID);

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                //データ作成
                var message = new Models.Message();

                //POST値セット
                this.TryUpdateModel(message);
                //エントリ情報セット
                base.SetEntryInfo(message);

                Context.Messages.Add(message);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("/Message/Index", new { messagePageClass = MessagePageClass });

        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="message"></param>
        private void TryUpdateModel(Models.Message message)
        {
            message.TeamID = Message.TeamID;
            message.UserAccountID = Message.UserAccountID;
            message.PrivateTeamID = Message.PrivateTeamID;
            message.ParentMessageID = MessageID;
            message.MessageClass = MessageID == null ? MessageClass.Post : MessageClass.Reply;
            message.MessageDetail = Message.MessageDetail;
            message.PublicFLG = Message.PrivateTeamID == null ? true : false;
            message.DeleteFLG = false;
        }
    }
}
