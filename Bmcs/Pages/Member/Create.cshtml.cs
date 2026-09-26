using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Function;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;
using Microsoft.EntityFrameworkCore;
using Bmcs.Enum;

namespace Bmcs.Pages.Member
{
    /// <summary>
    /// メンバー追加（複数人をまとめて登録する）
    /// ※以前は1人ずつ登録し、登録のたびにメンバー一覧へ戻っていた（14人で42回のページ遷移。issues.md D-1・D-2）
    /// </summary>
    public class CreateModel : PageModelBase<CreateModel>
    {
        /// <summary>
        /// 最初に表示する入力行の数
        /// </summary>
        public const int InitialRowCount = 5;

        /// <summary>
        /// チーム作成の直後に表示する入力行の数（チーム全員をまとめて登録することが多いため）
        /// </summary>
        public const int AfterTeamCreateRowCount = 10;

        /// <summary>
        /// 1回に登録できる人数の上限
        /// </summary>
        public const int MaxRowCount = 50;

        /// <summary>
        /// 1回に追加する入力行の数
        /// </summary>
        public const int AddRowCount = 5;

        public CreateModel(ILogger<CreateModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        /// <summary>
        /// 登録先のチームID
        /// </summary>
        [BindProperty]
        public string TeamID { get; set; }

        /// <summary>
        /// 入力行
        /// </summary>
        [BindProperty]
        public List<MemberRow> MemberRowList { get; set; } = new List<MemberRow>();

        /// <summary>
        /// 登録先のチーム
        /// </summary>
        public Models.Team Team { get; set; }

        /// <summary>
        /// チームを作成した直後か（「チームを作成しました」を表示する）
        /// </summary>
        public bool IsTeamCreated { get; set; }

        public async Task<IActionResult> OnGetAsync(string teamID, bool created = false)
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

            Team = await Context.Teams.FirstOrDefaultAsync(m => m.TeamID == teamID);

            if (Team == null)
            {
                return NotFound();
            }

            TeamID = Team.TeamID;
            IsTeamCreated = created;

            for (var i = 0; i < (created ? AfterTeamCreateRowCount : InitialRowCount); i++)
            {
                MemberRowList.Add(new MemberRow());
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberCreate);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //自チーム以外にはメンバーを登録できない（管理者は除く）
            //※入力エラーでの再表示より先に確認する。後だと、POST する TeamID を書き換えることで
            //  再表示の画面に他チーム（非公開を含む）のチーム名を表示できてしまう
            if (!base.IsMyTeamData(TeamID))
            {
                return NotFound();
            }

            //登録先のチーム（管理者が存在しないチームIDを送った場合も、ここで止める）
            Team = await Context.Teams.FirstOrDefaultAsync(r => r.TeamID == TeamID);

            if (Team == null)
            {
                return NotFound();
            }

            //行の番号が重複している場合は受け付けない（画面からは起きない。同じ行が二重に登録されるのを防ぐ）
            var rowIndexList = Request.Form["MemberRowList.Index"].ToList();

            if (rowIndexList.Count != rowIndexList.Distinct().Count())
            {
                return BadRequest();
            }

            MemberRowList ??= new List<MemberRow>();

            //入力チェックはここで行う（空の行は登録しないため、行ごとの必須チェックを属性では表せない）
            //※送信された行の番号（削除した行は欠番になる）と、再表示する行の番号（0からの連番）がずれるため、
            //  結び付けた値のエラー・入力値はいったん消し、エラーは再表示する行の番号で付け直す
            ModelState.Clear();

            foreach (var row in MemberRowList)
            {
                row.MemberName = row.MemberName?.Trim();
                row.UniformNumber = row.UniformNumber.ToHalfWidthDigits();
            }

            var inputRowList = MemberRowList.Where(r => !r.IsEmpty).ToList();

            for (var i = 0; i < MemberRowList.Count; i++)
            {
                var row = MemberRowList[i];

                if (row.IsEmpty)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(row.MemberName))
                {
                    ModelState.AddModelError($"{nameof(MemberRowList)}[{i}].{nameof(MemberRow.MemberName)}", "名前を入力してください。");
                }
                else if (row.MemberName.Length > 50)
                {
                    ModelState.AddModelError($"{nameof(MemberRowList)}[{i}].{nameof(MemberRow.MemberName)}", "名前は50文字以内で入力してください。");
                }

                if (!Models.Member.IsValidUniformNumber(row.UniformNumber))
                {
                    ModelState.AddModelError($"{nameof(MemberRowList)}[{i}].{nameof(MemberRow.UniformNumber)}", "数字3桁まで");
                }
            }

            if (inputRowList.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "登録するメンバーの名前を入力してください。");
            }
            else if (inputRowList.Count > MaxRowCount)
            {
                ModelState.AddModelError(string.Empty, $"1回に登録できるのは{MaxRowCount}人までです。");
            }

            if (!ModelState.IsValid)
            {
                //再表示に必要なデータを取り直す
                SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.MemberCreate);

                if (MemberRowList.Count == 0)
                {
                    MemberRowList.Add(new MemberRow());
                }

                return Page();
            }

            foreach (var row in inputRowList)
            {
                var member = new Models.Member
                {
                    TeamID = TeamID,
                    MemberName = row.MemberName,
                    UniformNumber = string.IsNullOrEmpty(row.UniformNumber) ? null : row.UniformNumber,
                    MemberClass = row.MemberClass,
                    ThrowClass = row.ThrowClass,
                    BatClass = row.BatClass,
                    PositionGroupClass = row.PositionGroupClass,
                    DeleteFLG = false,
                };

                //エントリ情報セット
                base.SetEntryInfo(member);

                Context.Members.Add(member);
            }

            if (!base.IsAdmin())
            {
                //チーム人数更新（以前の1人ずつの登録と同じ数え方）
                Team.TeamNumber = await Context.Members.CountAsync(r => r.TeamID == TeamID) + inputRowList.Count;

                //更新情報セット
                base.SetUpdateInfo(Team);
            }

            await Context.SaveChangesAsync();

            //登録後はメンバー一覧で、登録した人数と次にやること（試合の登録）を案内する
            return RedirectToPage("./Index", new { teamID = TeamID, registered = inputRowList.Count });
        }

        /// <summary>
        /// 入力行1行分
        /// </summary>
        public class MemberRow
        {
            public string UniformNumber { get; set; }

            public string MemberName { get; set; }

            //「名前だけで登録できます」と案内しているため、区分は空ではなく「選手」を既定にする。
            //※空のままだと、初回の打順自動割当（選手・選手兼監督が対象）から外れてしまう
            public MemberClass? MemberClass { get; set; } = Enum.MemberClass.Player;

            public ThrowClass? ThrowClass { get; set; }

            public BatClass? BatClass { get; set; }

            public PositionGroupClass? PositionGroupClass { get; set; }

            /// <summary>
            /// 何も入力されていない行か（背番号・名前が空。区分などの選択肢だけの行も空とみなす）
            /// </summary>
            public bool IsEmpty
            {
                get
                {
                    return string.IsNullOrWhiteSpace(UniformNumber) && string.IsNullOrWhiteSpace(MemberName);
                }
            }
        }
    }

    /// <summary>
    /// 入力行の表示用（_MemberCreateRow.cshtml のモデル）
    /// </summary>
    public class MemberCreateRowView
    {
        /// <summary>
        /// 行の番号（ひな形は「__index__」）
        /// </summary>
        public string Index { get; set; }

        public CreateModel.MemberRow Row { get; set; }

        public string UniformNumberError { get; set; }

        public string MemberNameError { get; set; }
    }
}
