using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Microsoft.Extensions.Logging;
using Bmcs.Constans;
using Microsoft.AspNetCore.Http;
using Bmcs.Enum;

namespace Bmcs.Pages.Game
{
    public class EditModel : PageModelBase<EditModel>
    {
        public EditModel(ILogger<EditModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        [BindProperty]
        public Models.Game Game { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            if (id == null)
            {
                return NotFound();
            }

            Game = await Context.Games
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == id);

            if (Game == null
                || (Game.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID)
                    && !base.IsAdmin())
                )
            {
                return NotFound();
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.GameEdit);

            return Page();
        }

        /// <summary>
        /// 画面の表示に必要なデータを取得する（入力エラーでの再表示にも使用する）
        /// </summary>
        /// <returns></returns>
        private async Task SetPageDataAsync()
        {
            if (Game != null && !string.IsNullOrEmpty(Game.TeamID))
            {
                Game.Team = await Context.Teams.FirstOrDefaultAsync(r => r.TeamID == Game.TeamID);
            }

            //システム管理データ
            SystemAdmin = await Context.SystemAdmins.FindAsync(SystemAdminClass.GameEdit);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Models.Game game;

            try
            {
                if (!ModelState.IsValid)
                {
                    //再表示に使うチームは、POST 値ではなく DB の TeamID から取得する
                    //※TeamID は画面から送っていないが、POST に追加されればそのまま結び付けられるため、
                    //  書き換えで他チーム（非公開を含む）のチーム名を表示できてしまう
                    var dbTeamID = await Context.Games
                                        .Where(r => r.GameID == Game.GameID)
                                        .Select(r => r.TeamID)
                                        .FirstOrDefaultAsync();

                    if (!base.IsMyTeamData(dbTeamID))
                    {
                        return NotFound();
                    }

                    Game.TeamID = dbTeamID;

                    //再表示に必要なデータを取り直す
                    //※取り直さないと、画面でチーム名を参照している箇所で例外となる
                    await SetPageDataAsync();

                    return Page();
                }

                //データ作成
                game = await Context.Games.FindAsync(Game.GameID);

                if (game == null)
                {
                    return NotFound();
                }


                //自チーム以外のデータは更新できない（管理者は除く）
                if (!base.IsMyTeamData(game.TeamID))
                {
                    return NotFound();
                }
                //POST値セット
                this.TryUpdateModel(game);
                //エントリ情報セット
                base.SetUpdateInfo(game);

                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            //遷移先は POST 値（hidden の StatusClass）ではなく DB の状態で決める
            //※以前は試合中（プレー毎）でも打順設定へ送っており、打順設定は試合前以外 NotFound のため404になっていた
            switch (game.StatusClass)
            {
                case StatusClass.BeforeGame:
                    //試合前は、続けて入力の準備へ進む
                    return game.GameInputTypeClass == GameInputTypeClass.OnlyGame
                           ? RedirectToPage("/GameScore/Edit", new { gameID = game.GameID })
                           : RedirectToPage("/Order/Edit", new { gameID = game.GameID });

                case StatusClass.DuringGame:
                    //試合中は、スコア入力に戻る
                    return RedirectToPage("/GameScene/Edit", new { gameID = game.GameID });

                default:
                    return RedirectToPage("/Game/Index");
            }
        }

        /// <summary>
        /// POST値をモデルにセット
        /// </summary>
        /// <param name="member"></param>
        private void TryUpdateModel(Models.Game game)
        {
            game.GameDate = Game.GameDate;
            game.GameClass = Game.GameClass;
            game.OpponentTeamName = Game.OpponentTeamName;
            game.OpponentTeamAbbreviation = Game.OpponentTeamAbbreviation;
            game.StadiumName = Game.StadiumName;
            game.WeatherClass = Game.WeatherClass;

            //先攻後攻・入力方式は試合前のみ変更できる
            //※画面では試合前以外は hidden で送っているが、POST 値をそのまま使うと入力開始後でも書き換えられ、
            //  記録済みのプレーと表裏・入力方式が食い違う。DB の状態で判定する
            if (game.StatusClass == StatusClass.BeforeGame)
            {
                game.BatFirstBatSecondClass = Game.BatFirstBatSecondClass;
                game.GameInputTypeClass = Game.GameInputTypeClass;
            }
        }
    }
}
