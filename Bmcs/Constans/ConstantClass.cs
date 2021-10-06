using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bmcs.Constans
{
    /// <summary>
    /// セッション定数
    /// </summary>
    public static class SessionConstant
    {
        public static readonly string UserAccountID = "UserAccountID";

        public static readonly string TeamID = "TeamID";

        public static readonly string AdminFLG = "AdminFLG";
    }

    /// <summary>
    /// ViewData定数
    /// </summary>
    public static class ViewDataConstant
    {
        public static readonly string Title = "Title";
    }

    /// <summary>
    /// システム定数
    /// </summary>
    public static class SystemConstant
    {
        public static readonly string AdminUserAccountID = "ADMIN";
    }

    /// <summary>
    /// イニング端数
    /// </summary>
    public static class FractionConstant
    {
        public static readonly decimal OneThird = (decimal)0.33;

        public static readonly decimal TwoThird = (decimal)0.66;

        public static readonly decimal ThreeThird = (decimal)0.99;

    }

    /// <summary>
    /// 規定計算用定数
    /// </summary>
    public static class CalculateRegulationConstant
    {
        public static readonly decimal BaseInning = 9;

        public static readonly decimal BaseRegulationAtBatting = (decimal)3.1;
    }
}
