using System;
using System.Security.Cryptography;

namespace Bmcs.Function
{
    /// <summary>
    /// トークン生成クラス
    /// </summary>
    public static class TokenGenerator
    {
        /// <summary>
        /// 推測困難なトークンを生成する（URLに使用可能な文字のみ）
        /// </summary>
        /// <returns></returns>
        public static string CreateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            //Bmcs.Function.Convertクラスと区別するため、System.Convertを明示する
            return System.Convert.ToBase64String(bytes)
                          .Replace("+", "-")
                          .Replace("/", "_")
                          .Replace("=", string.Empty);
        }
    }
}
