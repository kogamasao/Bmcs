using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bmcs.Function
{
    /// <summary>
    /// 変換クラス
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// NULL値を空文字に変換する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string NullToEmpty(this string s)
        {
            return s ?? string.Empty;
        }
    }
}
