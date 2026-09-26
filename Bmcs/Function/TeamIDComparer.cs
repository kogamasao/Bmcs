using System.Collections.Generic;
using System.Globalization;

namespace Bmcs.Function
{
    /// <summary>
    /// チームIDの比較（DB の照合順序と同じく、大文字小文字・全角半角・ひらがなカタカナを区別しない）
    /// ※DB では「JB」「jb」「ＪＢ」が同じチームIDとして扱われる（外部キーも通る）ため、アプリで比べるときも同じ基準にする
    /// </summary>
    public sealed class TeamIDComparer : IEqualityComparer<string>
    {
        public static readonly TeamIDComparer Instance = new TeamIDComparer();

        private const CompareOptions Options = CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType;

        private static readonly CompareInfo CompareInfo = CultureInfo.InvariantCulture.CompareInfo;

        public bool Equals(string x, string y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }

            return CompareInfo.Compare(x.TrimEnd(), y.TrimEnd(), Options) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj == null ? 0 : CompareInfo.GetHashCode(obj.TrimEnd(), Options);
        }
    }
}
