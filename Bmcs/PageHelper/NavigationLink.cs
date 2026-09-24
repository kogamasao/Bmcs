using System.Collections.Generic;

namespace Bmcs.PageHelper
{
    /// <summary>
    /// ヘッダメニューのリンク1件
    /// ※移行期間中、Bootstrap版とTailwind版の2つのレイアウトにリンクをそれぞれ書いて、
    ///   片方だけ写し漏れる事故が起きた。定義はコード側に1本化し、レイアウトは見た目だけを担当する
    ///   （現在はレイアウトは1つだが、この形のままにしている）。
    /// </summary>
    public class NavigationLink
    {
        /// <summary>
        /// 遷移先のページ名（例：/Game/Index）
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// 表示文字列
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// ルート値（成績画面の公開フラグなど）
        /// </summary>
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 主要な操作として強調するか（未ログイン時の「無料で登録」）
        /// </summary>
        public bool IsPrimary { get; set; }

        public static NavigationLink Create(string pageName, string text, Dictionary<string, string> routeValues = null, bool isPrimary = false)
        {
            return new NavigationLink
            {
                PageName = pageName,
                Text = text,
                RouteValues = routeValues ?? new Dictionary<string, string>(),
                IsPrimary = isPrimary,
            };
        }
    }
}
