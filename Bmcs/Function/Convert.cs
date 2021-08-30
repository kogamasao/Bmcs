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
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NullToEmpty<T>(this T? value) where T : struct
        {
            return value == null ? string.Empty : value.ToString();
        }

        /// <summary>
        /// NULL値を空文字に変換する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NullToEmpty(this string value)
        {
            return value == null ? string.Empty : value.ToString();
        }
    }
}
