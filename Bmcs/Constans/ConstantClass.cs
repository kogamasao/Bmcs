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

        public static readonly string TeamID = "TeamID";

        public static readonly string AdminFLG = "AdminFLG";

        public static readonly string OrderIndex = "OrderIndex";

        public static readonly string MemberIDList = "MemberIDList";

        public static readonly string PositionClassList = "PositionClassList";
    }

    /// <summary>
    /// システム定数
    /// </summary>
    public static class SystemConstant
    {
        public static readonly string AdminUserAccountID = "ADMIN";

        public static readonly string PageModePublic = "PageModePublic";

        public static readonly string PageModePrivate = "PageModePrivate";

        public static readonly int TempGameSceneID = -1;
    }
}
