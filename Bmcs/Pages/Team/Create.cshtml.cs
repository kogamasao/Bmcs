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
using Bmcs.Enum;

namespace Bmcs.Pages.Team
{
    public class CreateModel : PageModelBase<CreateModel>
    {
        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public async Task<IActionResult> OnGetAsync()
        {
            //未ログインで入力させると、送信時に全て失われるためログイン画面へ
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            //既にチームに所属している場合は作成させない
            //※作成すると所属が新しいチームへ移り、元のチームが所属ユーザ0になってしまう
            if (!base.IsAdmin()
                && !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.TeamID)))
            {
                return RedirectToPage("./Edit");
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TeamCreate);

            //このページ自身を正規URLとする（canonicalがトップを指すと個別ページとして扱われない）
            IsIndex = true;

            return Page();
        }

        [BindProperty]
        public Models.Team Team { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            //既にチームに所属している場合は作成させない（OnGet と同じ判定）
            //※作成直後のブラウザバックでフォームを再送信すると、2つ目のチームが作られて
            //  所属がそちらへ移り、元のチームが所属ユーザ0になってしまう
            if (!base.IsAdmin()
                && !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.TeamID)))
            {
                return RedirectToPage("./Edit");
            }

            //入力エラーでの再表示でもヘルプを表示できるようにする
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.TeamCreate);

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

                //パスワード必須チェック
                if (string.IsNullOrEmpty(Team.TeamPassword))
                {
                    ModelState.AddModelError(nameof(Models.Team) + "." + nameof(Models.Team.TeamPassword), "パスワードは必須です。");

                    return Page();
                }

                //確認パスワード必須チェック
                if (string.IsNullOrEmpty(Team.ConfirmTeamPassword))
                {
                    ModelState.AddModelError(nameof(Models.Team) + "." + nameof(Models.Team.ConfirmTeamPassword), "確認用パスワードは必須です。");

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
                //チーム作成の次にやることは選手の登録のため、そのまま登録画面へ進める。
                //※本番では、チームを作成したまま選手を1人も登録せずに離脱したチームが
                //  1日離脱118チームのうち43チームあった（doc/detail_design_ui.md 参照）
                return RedirectToPage("/Member/Create");
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
            team.TeamPassword = Team.TeamPassword.ChangeHashValue();
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
