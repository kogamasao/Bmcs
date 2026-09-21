using System.Collections.Generic;

namespace Bmcs.PageHelper
{
    /// <summary>
    /// ページ送りの表示情報
    /// ※Tailwind版のページ送り部品（_PaginationTailwind.cshtml）へ渡す。
    ///   一覧画面ごとに同じHTMLを書くと、片方だけ直す事故が起きるため部品化する。
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// 現在のページ番号
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 総ページ数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// ページ送り先のページ名（例：/Member/Index）
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// ページ番号以外に引き継ぐルート値（チームIDなど）
        /// </summary>
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();

        public bool HasPreviousPage
        {
            get { return PageIndex > 1; }
        }

        public bool HasNextPage
        {
            get { return PageIndex < TotalPages; }
        }

        /// <summary>
        /// PaginatedList から表示情報を作成する
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="totalPages"></param>
        /// <param name="pageName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static PaginationInfo Create(int pageIndex, int totalPages, string pageName, Dictionary<string, string> routeValues = null)
        {
            return new PaginationInfo
            {
                PageIndex = pageIndex,
                TotalPages = totalPages,
                PageName = pageName,
                RouteValues = routeValues ?? new Dictionary<string, string>(),
            };
        }
    }
}
