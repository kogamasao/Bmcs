using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Bmcs.Function
{
    /// <summary>
    /// メール本文の組み立て補助
    /// </summary>
    public static class MailBody
    {
        /// <summary>
        /// メール本文へ埋め込む文字列を無害化する
        /// ユーザ入力値をHTMLメールに埋め込む際のタグ挿入を防ぐ。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            //HTMLエスケープ後に改行を<br />へ変換する（順序を逆にするとタグが挿入できてしまう）
            return WebUtility.HtmlEncode(value)
                             .Replace("\r\n", "<br />")
                             .Replace("\n", "<br />")
                             .Replace("\r", "<br />");
        }

        /// <summary>
        /// メールの件名へ埋め込む文字列を無害化する（改行によるヘッダ挿入を防ぐ）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EscapeSubject(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\r", " ").Replace("\n", " ");
        }

        /// <summary>
        /// HTML本文からプレーンテキスト本文を生成する
        /// </summary>
        /// <param name="htmlBody"></param>
        /// <returns></returns>
        public static string ToPlainText(string htmlBody)
        {
            if (string.IsNullOrEmpty(htmlBody))
            {
                return string.Empty;
            }

            var text = Regex.Replace(htmlBody, @"<br\s*/?>", "\r\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<[^>]+>", string.Empty);

            return WebUtility.HtmlDecode(text);
        }
    }
}
