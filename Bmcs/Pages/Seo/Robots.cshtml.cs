using Bmcs.Function;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Bmcs.Pages.Seo
{
    /// <summary>
    /// robots.txt
    /// ※サイトマップの場所は絶対URLで書く必要があるため、静的ファイルではなく設定値（正規のホスト名）から組み立てる
    /// ※検索結果に出さない画面の除外は、各画面の noindex（_HeadMeta.cshtml）で行う。
    /// 　Disallow にするとクローラが noindex を読めず、URL だけが検索結果に残ることがあるため、
    /// 　Disallow は「ログインしないと開けない画面」（クローラが開いてもログイン画面へ戻されるだけの画面）に限る
    /// </summary>
    public class RobotsModel : PageModel
    {
        /// <summary>
        /// ログインしないと開けない画面（新規作成・編集・削除・スコア入力・管理者向け）
        /// </summary>
        private static readonly string[] DisallowPathList =
        {
            "/GameScene/",
            "/Order/",
            "/Survey/",
            "/Game/Create",
            "/Game/Edit",
            "/Game/Delete",
            "/GameScore/Edit",
            "/Member/Create",
            "/Member/Edit",
            "/Member/Delete",
            "/Team/Create",
            "/Team/Edit",
            "/Team/Delete",
            "/UserAccount/Index",
            "/UserAccount/Details",
            "/UserAccount/Edit",
            "/UserAccount/Delete",
            "/Inquiry/Index",
            "/Inquiry/Details",
            "/Inquiry/Delete",
        };

        private readonly IConfiguration _configuration;

        public RobotsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
            var builder = new StringBuilder();

            builder.Append("User-agent: *\n");

            foreach (var path in DisallowPathList)
            {
                builder.Append("Disallow: " + path + "\n");
            }

            builder.Append("\n");
            builder.Append("Sitemap: " + SiteUrl.ToAbsolute(_configuration, Request, "/sitemap.xml") + "\n");

            return Content(builder.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}
