using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Microsoft.EntityFrameworkCore;
using Bmcs.Enum;

namespace Bmcs.Pages.Member
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Member Member { get; set; }

        public async Task<IActionResult> OnGetAsync(string teamID)
        {
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            //マイチーム以外を指定して管理者でない
            if (!string.IsNullOrEmpty(teamID)
                && teamID != HttpContext.Session.GetString(SessionConstant.TeamID)
                && !base.IsAdmin())
            {
                return NotFound();
            }

            if (teamID == null)
            {
                teamID = HttpContext.Session.GetString(SessionConstant.TeamID);
            }

            if (string.IsNullOrEmpty(teamID))
            {
                return NotFound();
            }

            Member = new Models.Member
            {
                Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID),
                //「名前だけで登録できます」と案内しているため、区分は空ではなく「選手」を既定にする。
                //※空のままだと、初回の打順自動割当（選手・選手兼監督が対象）から外れてしまう
                MemberClass = MemberClass.Player,
            };

            if (Member.Team == null)
            {
                return NotFound();
            }

            //チームID
            Member.TeamID = Member.Team.TeamID;

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberCreate);

            return Page();
        }

        /// <summary>
        /// 画面の表示に必要なデータを取得する（入力エラーでの再表示にも使用する）
        /// </summary>
        /// <returns></returns>
        private async Task SetPageDataAsync()
        {
            if (Member != null && !string.IsNullOrEmpty(Member.TeamID))
            {
                Member.Team = await Context.Teams.FirstOrDefaultAsync(r => r.TeamID == Member.TeamID);
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberCreate);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                //自チーム以外にはメンバーを登録できない（管理者は除く）
                //※入力エラーでの再表示より先に確認する。後だと、POST する TeamID を書き換えることで
                //  再表示の画面に他チーム（非公開を含む）のチーム名を表示できてしまう
                if (!base.IsMyTeamData(Member.TeamID))
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    //再表示に必要なデータを取り直す
                    //※取り直さないと、画面でチーム名を参照している箇所で例外となる
                    await SetPageDataAsync();

                    return Page();
                }

                //データ作成
                var member = new Models.Member();

                //POST値セット
                this.TryUpdateModel(member);
                //エントリ情報セット
                base.SetEntryInfo(member);

                Context.Members.Add(member);

                if (!base.IsAdmin())
                {
                    //ユーザ情報取得
                    var team = Context.Teams.FirstOrDefault(r => r.TeamID == Member.TeamID);

                    if (team != null)
                    {
                        //チーム人数更新
                        team.TeamNumber = Context.Members.Where(r => r.TeamID == Member.TeamID).Count() + 1;

                        //更新情報セット
                        base.SetUpdateInfo(team);
                    }
                }

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("./Index", new { teamID = Member.TeamID });

        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="member"></param>
        private void TryUpdateModel(Models.Member member)
        {
            member.TeamID = Member.TeamID;
            member.MemberName = Member.MemberName;
            member.MemberClass = Member.MemberClass;
            member.BatClass = Member.BatClass;
            member.ThrowClass = Member.ThrowClass;
            member.PositionGroupClass = Member.PositionGroupClass;
            member.UniformNumber = Member.UniformNumber;
            member.MessageDetail = Member.MessageDetail;
            member.DeleteFLG = false;
        }
    }
}
