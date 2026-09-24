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

        /// <summary>
        /// 送信直後か（送信したスレッドに「送信しました」を表示する）
        /// </summary>
        public bool IsPosted { get; set; }

        /// <summary>
        /// ダイレクトメッセージの送信先チーム名（チーム一覧・チーム情報の「メッセージを送る」から来た場合）
        /// </summary>
        public string PrivateTeamName { get; set; }

        /// <summary>
        /// チーム内にだけ送れるか（他チームへの送信・公開投稿・他チームのスレッドへの返信ができない）
        /// ※非公開チームと、体験用ユーザ（誰でもログインできる共有アカウントのため、実在のチームへの送信を禁止する）
        /// </summary>
        public bool IsTeamOnly
        {
            get { return MyTeam != null && (!MyTeam.PublicFLG || base.IsSampleUser()); }
        }

        public async Task<IActionResult> OnGetAsync(MessagePageClass messagePageClass, string teamID, int? messageID, string privateTeamID, int? pageIndex, bool posted = false)
        {
            IsPosted = posted;

            return await LoadAsync(messagePageClass, teamID, messageID, privateTeamID, pageIndex);
        }

        /// <summary>
        /// スレッド（親メッセージ）を閲覧できるか
        /// ※非公開メッセージ（ダイレクトメッセージ・チーム内）は、送信元チームと送信先チームのみ閲覧できる
        /// </summary>
        private bool CanViewThread(Models.Message rootMessage)
        {
            if (rootMessage == null || rootMessage.DeleteFLG)
            {
                return false;
            }

            if (base.IsAdmin())
            {
                return true;
            }

            var myTeamID = HttpContext.Session.GetString(SessionConstant.TeamID);

            if (rootMessage.PublicFLG)
            {
                //公開メッセージでも、送信元チームを非公開にした・削除した場合は、そのチーム以外には見せない
                //※以前は見せており、チームを非公開にしても過去の公開投稿（本文・ユーザ名・チーム名）が残っていた
                return (rootMessage.Team != null && rootMessage.Team.PublicFLG && !rootMessage.Team.DeleteFLG)
                    || (!string.IsNullOrEmpty(myTeamID) && rootMessage.TeamID == myTeamID);
            }

            return !string.IsNullOrEmpty(myTeamID)
                && (rootMessage.TeamID == myTeamID || rootMessage.PrivateTeamID == myTeamID);
        }

        /// <summary>
        /// 指定されたメッセージが属するスレッドの親メッセージを取得する
        /// ※返信のIDを指定された場合も、親メッセージで閲覧権限を判定するため
        /// </summary>
        private async Task<Models.Message> FindRootMessageAsync(int? messageID)
        {
            var message = await Context.Messages.FindAsync(messageID);

            if (message?.ParentMessageID != null)
            {
                message = await Context.Messages.FindAsync(message.ParentMessageID);
            }

            //閲覧権限の判定に送信元チームの公開状態を使うため、読み込んでおく
            if (message != null)
            {
                await Context.Entry(message).Reference(r => r.Team).LoadAsync();
            }

            return message;
        }

        /// <summary>
        /// 画面の表示に必要なデータを取得する（入力エラーでの再表示にも使用する）
        /// </summary>
        private async Task<IActionResult> LoadAsync(MessagePageClass messagePageClass, string teamID, int? messageID, string privateTeamID, int? pageIndex)
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

            //スレッド表示の場合は閲覧権限を確認する
            //※以前は確認しておらず、メッセージIDを指定すれば未ログインでも他チーム同士のダイレクトメッセージを読めた
            if (messageID != null)
            {
                var rootMessage = await FindRootMessageAsync(messageID);

                if (!CanViewThread(rootMessage))
                {
                    return NotFound();
                }

                messageID = rootMessage.MessageID;
            }

            if(messageID == null)
            { 
                var tempMessageList = await Context.Messages
                                        .Include(r => r.UserAccount)
                                        .Include(r => r.Team)
                                        .Include(r => r.PrivateTeam)
                                        .Where(r => ((messagePageClass == MessagePageClass.Public) && (r.PublicFLG) && (r.Team.PublicFLG) && (!r.Team.DeleteFLG))
                                                || ((messagePageClass == MessagePageClass.PublicTeam) && (r.PublicFLG) && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID)) && (r.MessageClass == MessageClass.Post))
                                                || ((messagePageClass == MessagePageClass.RelatedTeam) && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID) || r.PrivateTeamID == HttpContext.Session.GetString(SessionConstant.TeamID)))
                                                || ((messagePageClass == MessagePageClass.Private) && (!r.PublicFLG) && (r.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID) || r.PrivateTeamID == HttpContext.Session.GetString(SessionConstant.TeamID)))
                                        )
                                        .Where(r => !r.DeleteFLG)
                                        .ToListAsync();

                if(messagePageClass == MessagePageClass.PublicTeam)
                {
                    messageList = tempMessageList;
                }
                else
                {
                    var messageIDList = tempMessageList.Select(r => new { MessageID = r.ParentMessageID == null ? r.MessageID : r.ParentMessageID.NullToZero() }).GroupBy(r => r.MessageID).Select(r => MessageID = r.Key);

                    //※公開メッセージは、送信元チームが公開・未削除のもの（自チームは除く）に限る（CanViewThread と同じ判定）
                    var myTeamID = HttpContext.Session.GetString(SessionConstant.TeamID);
                    var isAdmin = base.IsAdmin();

                    messageList = await Context.Messages
                                            .Include(r => r.UserAccount)
                                            .Include(r => r.Team)
                                            .Include(r => r.PrivateTeam)
                                            .Where(r => messageIDList.Contains(r.MessageID) && !r.DeleteFLG
                                                     && (!r.PublicFLG || (r.Team.PublicFLG && !r.Team.DeleteFLG) || r.TeamID == myTeamID || isAdmin))
                                            .ToListAsync();
                }
            }
            else
            {
                messageList = await Context.Messages
                                    .Include(r => r.UserAccount)
                                    .Include(r => r.Team)
                                    .Include(r => r.PrivateTeam)
                                    .Where(r => (r.MessageID == messageID || r.ParentMessageID == messageID) && !r.DeleteFLG)
                                    .ToListAsync();

            }

            //チーム未所属（ユーザ登録直後など）は投稿できない
            if (base.IsLogin() && MyTeam != null)
            { 
                //ユーザアカウント
                UserAccount = await Context.UserAccounts.FindAsync(HttpContext.Session.GetString(SessionConstant.UserAccountID));

                //メッセージ
                Message = new Models.Message()
                {
                    //送信元は自チーム（管理者は URL で指定したチームでも投稿できる）
                    //※以前は URL の teamID をそのまま使っており、他チームを指定すると送信時に NotFound になっていた
                    TeamID = base.IsAdmin() ? teamID : MyTeam.TeamID,
                    UserAccountID = UserAccount.UserAccountID,
                    MessageTitle = messageID == null ? null : "返信",
                };

                //非公開チーム・体験用ユーザは、チーム内のメッセージのみ
                if(IsTeamOnly)
                {
                    Message.PrivateTeamID = MyTeam.TeamID;
                }
                //チーム指定あり（チーム一覧・チーム情報の「メッセージを送る」）
                else if(!string.IsNullOrEmpty(privateTeamID))
                {
                    var privateTeam = await Context.Teams.FindAsync(privateTeamID);

                    //送信できるのは公開チーム（と自チーム）のみ。POST 時にも同じ判定をしている
                    if (privateTeam != null && !privateTeam.DeleteFLG && (privateTeam.PublicFLG || privateTeam.TeamID == MyTeam.TeamID))
                    {
                        Message.PrivateTeamID = privateTeam.TeamID;
                        PrivateTeamName = privateTeam.TeamName;
                    }
                }

                //親データ取得
                var parentMessage = await Context.Messages.FindAsync(messageID);

                if (IsTeamOnly && parentMessage != null && (parentMessage.PublicFLG || parentMessage.TeamID != MyTeam.TeamID))
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
            //※以前は一覧でも「メッセージ(投稿)」と表示しており、一覧なのか投稿画面なのか分かりにくかった
            ViewData[ViewDataConstant.Title] = messageID == null ? "メッセージ" : "メッセージ(返信)";

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
            Models.Message sentMessage;

            try
            {
                //自チーム以外の名義でメッセージを投稿できない（管理者は除く）
                if (Message == null || !base.IsMyTeamData(Message.TeamID))
                {
                    return NotFound();
                }

                if (MyTeam == null)
                {
                    return NotFound();
                }

                //親データ取得
                Models.Message parentMessage = null;

                if (MessageID != null)
                {
                    //返信先は、閲覧できるスレッドに限る（返信への返信は親にぶら下げる）
                    parentMessage = await FindRootMessageAsync(MessageID);

                    if (!CanViewThread(parentMessage))
                    {
                        return NotFound();
                    }

                    //非公開チーム・体験用ユーザが返信できるのは、自チームの非公開スレッドのみ（表示時の IsEnablePostReply と同じ判定）
                    if (IsTeamOnly && (parentMessage.PublicFLG || parentMessage.TeamID != MyTeam.TeamID))
                    {
                        return NotFound();
                    }

                    MessageID = parentMessage.MessageID;
                }
                else if (IsTeamOnly)
                {
                    //非公開チームは、公開投稿や他チームへのメッセージを送れない（チーム内のみ）
                    //※画面では hidden で自チームを送っているが、POST 値を信用しない
                    Message.PrivateTeamID = MyTeam.TeamID;
                }
                else if (!string.IsNullOrEmpty(Message.PrivateTeamID))
                {
                    //送信先は、画面の選択肢と同じく公開チーム（と自チーム）に限る
                    var privateTeam = await Context.Teams.FindAsync(Message.PrivateTeamID);

                    if (privateTeam == null || privateTeam.DeleteFLG
                        || (!privateTeam.PublicFLG && privateTeam.TeamID != MyTeam.TeamID))
                    {
                        ModelState.AddModelError(nameof(Models.Message) + "." + nameof(Models.Message.PrivateTeamID), "送信先のチームが見つかりません。");
                    }
                }

                if (!ModelState.IsValid)
                {
                    //再表示に必要なデータ（一覧・タイトル・ヘルプ）を取り直す
                    //※以前は取り直しておらず、一覧が無いため500になっていた
                    var postedMessage = Message;
                    var result = await LoadAsync(MessagePageClass, Message.TeamID, MessageID, Message.PrivateTeamID, null);

                    if (Message != null)
                    {
                        Message.MessageTitle = postedMessage.MessageTitle;
                        Message.MessageDetail = postedMessage.MessageDetail;
                    }

                    return result;
                }

                //データ作成
                var message = new Models.Message();

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

                sentMessage = message;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            //送信したスレッドを表示する
            //※以前は一覧（送信前のタブ）に戻しており、非公開のメッセージは公開一覧に出ないため、送れたかどうかが分からなかった
            return RedirectToPage("/Message/Index", new
            {
                messagePageClass = sentMessage.PublicFLG ? MessagePageClass.Public : MessagePageClass.Private,
                messageID = sentMessage.ParentMessageID ?? sentMessage.MessageID,
                posted = true,
            });

        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="message"></param>
        private void TryUpdateModel(Models.Message message, Models.Message parentMessage)
        {
            message.TeamID = Message.TeamID;
            //投稿者はログインユーザから設定する（POST値を信用すると他人になりすませるため）
            message.UserAccountID = HttpContext.Session.GetString(SessionConstant.UserAccountID);
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
