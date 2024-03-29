﻿using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        /// <summary>
        /// 0をNULLに変換する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T? ZeroToNull<T>(this T value) where T : struct
        {
            if (decimal.TryParse(value.ToString(), out decimal decimalValue))
            {
                if (decimalValue == 0)
                {
                    return null;
                }
            }

            return value;
        }

        /// <summary>
        /// NULLを0に変換する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T NullToZero<T>(this T? value) where T : struct
        {
            if (value == null)
            {
                return (T)(dynamic)(0);
            }

            return (T)(dynamic)value;
        }

        /// <summary>
        /// 小数値表示
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DisplayDecimal(this decimal? value)
        {
            return System.Convert.ToDecimal(value).ToString("#.##");
        }

        /// <summary>
        /// 改行コード変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ReplaceNewLineForHtml(this string value)
        {
            return value.NullToEmpty().Replace(Environment.NewLine, "<br />");
        }

        /// <summary>
        /// ハッシュ値変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ChangeHashValue(this string value)
        {
            byte[] salt = new byte[128 / 8];

            //using (var rngCsp = new RNGCryptoServiceProvider())
            //{
            //    rngCsp.GetNonZeroBytes(salt);
            //}

            string hashed = System.Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: value,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hashed;
        }
    }
}
