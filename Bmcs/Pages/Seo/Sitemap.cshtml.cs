using Bmcs.Constans;
using Bmcs.Data;
using Bmcs.Enum;
using Bmcs.Function;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bmcs.Pages.Seo
{
    /// <summary>
    /// サイトマップ（/sitemap.xml）
    /// ※検索エンジンに公開ページの URL を伝える。Google Search Console に登録する。
    /// 　載せるのは、各画面で SetIndex している（noindex でない）ページの正規URLだけにする。
    /// 　非公開・削除済みのチームや、サンプルデータのチームの URL を載せると、非公開にしたチームの存在が漏れるため、
    /// 　PageModelBase.IsSearchTargetTeam・IsSearchTargetGame と同じ条件で絞り込む
    /// ※セッション・ログイン状態に依存しないため、PageModelBase を継承しない
    /// </summary>
    public class SitemapModel : PageModel
    {
        /// <summary>
        /// 1ファイルに載せられる URL の上限（サイトマップの仕様）
        /// ※超える場合は、サイトマップを分割（サイトマップインデックス）する必要がある。現状の件数では届かないため、新しい試合を優先して切り捨てる
        /// </summary>
        private const int MaxUrlCount = 50000;

        private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

        private readonly BmcsContext _context;

        private readonly IConfiguration _configuration;

        public SitemapModel(BmcsContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var urlList = new List<XElement>();

            //サイト全体のページ
            urlList.Add(CreateUrl("/Index", null, null));
            urlList.Add(CreateUrl("/Team/Index", null, null));

            foreach (var scorePageClass in ScorePageClassList)
            {
                urlList.Add(CreateUrl("/Score/Index", new { scorePageClass, isPublic = "true" }, null));
            }

            urlList.Add(CreateUrl("/Message/Index", new { messagePageClass = MessagePageClass.Public }, null));
            urlList.Add(CreateUrl("/UserAccount/Create", null, null));
            urlList.Add(CreateUrl("/Updates/Index", null, UpdateHistoryList.All.Max(r => (DateTime?)r.Date)));
            urlList.Add(CreateUrl("/Privacy", null, null));
            urlList.Add(CreateUrl("/Term", null, null));
            urlList.Add(CreateUrl("/Inquiry/Create", null, null));

            //公開チーム
            var teamList = await _context.Teams
                                         .Where(r => r.PublicFLG
                                                  && !r.DeleteFLG
                                                  && !r.SystemDataFLG
                                                  && !SystemConstant.SampleDataTeamIDList.Contains(r.TeamID))
                                         .Select(r => new { r.TeamID, r.UpdateDatetime })
                                         .AsNoTracking()
                                         .ToListAsync();

            var teamIDList = teamList.Select(r => r.TeamID).ToList();

            //公開チームの確定済みの試合
            var gameList = await _context.Games
                                         .Where(r => teamIDList.Contains(r.TeamID)
                                                  && !r.DeleteFLG
                                                  && (r.StatusClass == StatusClass.EndGame || r.StatusClass == StatusClass.EndGameLock))
                                         .OrderByDescending(r => r.GameDate)
                                         .ThenByDescending(r => r.GameID)
                                         .Select(r => new { r.GameID, r.TeamID, r.GameInputTypeClass, r.UpdateDatetime })
                                         .AsNoTracking()
                                         .ToListAsync();

            foreach (var team in teamList)
            {
                //チームの最終更新は、チーム情報と試合のうち新しいほう（試合を確定すると成績が変わるため）
                var lastModified = gameList.Where(r => string.Equals(r.TeamID, team.TeamID, StringComparison.OrdinalIgnoreCase))
                                           .Select(r => r.UpdateDatetime)
                                           .Append(team.UpdateDatetime)
                                           .Max();

                urlList.Add(CreateUrl("/Team/Details", new { id = team.TeamID }, lastModified));
                urlList.Add(CreateUrl("/Game/Index", new { teamID = team.TeamID }, lastModified));

                foreach (var scorePageClass in ScorePageClassList)
                {
                    urlList.Add(CreateUrl("/Score/Index", new { scorePageClass, teamID = team.TeamID, isPublic = "true" }, lastModified));
                }
            }

            foreach (var game in gameList)
            {
                urlList.Add(CreateUrl("/GameScore/Details", new { gameID = game.GameID }, game.UpdateDatetime));

                //イニング詳細は「プレー毎」に入力した試合だけにある（試合結果の画面もその場合だけリンクを出す）
                if (game.GameInputTypeClass == GameInputTypeClass.ByPlay)
                {
                    urlList.Add(CreateUrl("/InningScore/Index", new { gameID = game.GameID }, game.UpdateDatetime));
                }
            }

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(SitemapNamespace + "urlset", urlList.Take(MaxUrlCount)));

            return Content(document.Declaration + Environment.NewLine + document.Root, "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// 成績画面の種類（TOP・チーム・投手・野手）
        /// </summary>
        private static readonly ScorePageClass[] ScorePageClassList =
        {
            ScorePageClass.Index,
            ScorePageClass.Team,
            ScorePageClass.Pitcher,
            ScorePageClass.Fielder,
        };

        private XElement CreateUrl(string pageName, object routeValues, DateTime? lastModified)
        {
            var element = new XElement(SitemapNamespace + "url",
                              new XElement(SitemapNamespace + "loc", SiteUrl.ToAbsolute(_configuration, Request, Url.Page(pageName, routeValues))));

            if (lastModified != null)
            {
                element.Add(new XElement(SitemapNamespace + "lastmod", lastModified.Value.ToString("yyyy-MM-dd")));
            }

            return element;
        }
    }
}
