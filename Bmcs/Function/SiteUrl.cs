using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Bmcs.Function
{
    /// <summary>
    /// サイトの絶対URLの基点
    /// ※canonical・OGP・サイトマップ・robots.txt で同じURLを使うために1か所にまとめる。
    /// 　検索エンジンに旧URL（bmcs.azurewebsites.net）や、書き換えられた Host ヘッダのURLを伝えないよう、設定値を優先する
    /// </summary>
    public static class SiteUrl
    {
        /// <summary>
        /// 基点のURL（末尾の「/」なし。例：https://bmcs.app）
        /// 優先順：Site:CanonicalHost → EmailSettings:BaseUrl → リクエストのホスト
        /// </summary>
        public static string GetBaseUrl(IConfiguration configuration, HttpRequest request)
        {
            var canonicalHost = configuration?["Site:CanonicalHost"];

            if (!string.IsNullOrWhiteSpace(canonicalHost))
            {
                return "https://" + canonicalHost.Trim().TrimEnd('/');
            }

            var baseUrl = configuration?["EmailSettings:BaseUrl"];

            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                return baseUrl.Trim().TrimEnd('/');
            }

            return request.Scheme + "://" + request.Host;
        }

        /// <summary>
        /// 絶対URL
        /// </summary>
        /// <param name="relativeUrl">「/」から始まるURL（クエリ文字列を含んでよい）</param>
        public static string ToAbsolute(IConfiguration configuration, HttpRequest request, string relativeUrl)
        {
            return GetBaseUrl(configuration, request) + "/" + (relativeUrl ?? string.Empty).TrimStart('/');
        }
    }
}
