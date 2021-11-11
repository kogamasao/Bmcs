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
using Bmcs.PageHelper;

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

        public PaginatedList<Models.Message> MessageList { get; set; }

        public bool IsEnablePostReply { get; set; }

        public async Task<IActionResult> OnGetAsync(MessagePageClass messagePageClass, string teamID, int? messageID, string privateTeamID, int? pageIndex)
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
            var messageList = new List<Models.Message>();

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
                    messageList = tempMessageList;
                }
                else
                {
                    var messageIDList = tempMessageList.Select(r => new { MessageID = r.ParentMessageID == null ? r.MessageID : r.ParentMessageID.NullToZero() }).GroupBy(r => r.MessageID).Select(r => MessageID = r.Key);

                    messageList = await Context.Messages
                                            .Include(r => r.UserAccount)
                                            .Include(r => r.Team)
                                            .Where(r => messageIDList.Contains(r.MessageID)).ToListAsync();
                }
            }
            else
            {
                messageList = await Context.Messages
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
                    MessageTitle = messageID == null ? null : "返信",
                };

                //非公開チーム
                if(!MyTeam.PublicFLG)
                {
                    Message.PrivateTeamID = MyTeam.TeamID;
                }
                //チーム指定あり
                else if(!string.IsNullOrEmpty(privateTeamID))
                {
                    Message.PrivateTeamID = privateTeamID;
                }

                //親データ取得
                var parentMessage = await Context.Messages.FindAsync(messageID);

                if (!MyTeam.PublicFLG && parentMessage != null && (parentMessage.PublicFLG || parentMessage.TeamID != MyTeam.TeamID))
                {
                    IsEnablePostReply = false;
                }
                else
                {
                    IsEnablePostReply = true;
                }
            }
            else
            {
                IsEnablePostReply = false;
            }

            if (messageID == null)
            {
                ViewData[ViewDataConstant.MessageMode] = "投稿";
                //システム管理データ
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.PostMessage);

                MessageList = PaginatedList<Models.Message>.Create(
                                messageList
                                .OrderByDescending(r => r.UpdateDatetime).AsQueryable().AsNoTracking(), pageIndex ?? 1, 20);

            }
            else
            {
                ViewData[ViewDataConstant.MessageMode] = "返信";
                //システム管理データ
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.ReplyMessage);

                MessageList = PaginatedList<Models.Message>.Create(
                                messageList
                                .OrderBy(r => r.EntryDatetime).AsQueryable().AsNoTracking(), pageIndex ?? 1, 20);
            }

            //タイトル
            ViewData[ViewDataConstant.Title] = "メッセージ";

            if (base.IsLogin())
            {
                ViewData[ViewDataConstant.Title] += "(" + ViewData[ViewDataConstant.MessageMode].ToString() + ")";
            }

            //引数セット
            MessagePageClass = messagePageClass;
            SelectTeamID = teamID;
            MessageID = messageID;

            //インデックス
            IsIndex = true;

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
                //親データ取得
                var parentMessage = await Context.Messages.FindAsync(MessageID);

                //POST値セット
                this.TryUpdateModel(message, parentMessage);
                //エントリ情報セット
                base.SetEntryInfo(message);

                Context.Messages.Add(message);

                //返信
                if(parentMessage != null)
                {
                    parentMessage.ReplyCount += 1;
                    base.SetUpdateInfo(parentMessage);
                }

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
        private void TryUpdateModel(Models.Message message, Models.Message parentMessage)
        {
            message.TeamID = Message.TeamID;
            message.UserAccountID = Message.UserAccountID;
            message.PrivateTeamID = MessageID == null ? Message.PrivateTeamID : null;
            message.ParentMessageID = MessageID;
            message.MessageClass = MessageID == null ? MessageClass.Post : MessageClass.Reply;
            message.MessageTitle = Message.MessageTitle;
            message.MessageDetail = Message.MessageDetail;
            message.ReplyCount = 0;

            if (parentMessage != null)
            {
                message.PublicFLG = parentMessage.PublicFLG;
            }
            else
            {
                message.PublicFLG = Message.PrivateTeamID == null;
            }

            message.DeleteFLG = false;
        }
    }
}
