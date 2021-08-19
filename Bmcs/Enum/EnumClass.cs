using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bmcs.Enum
{
    /// <summary>
    /// メンバー区分
    /// </summary>
    public enum MemberClass
    {
        Manager = 1,
        Coach = 2,
        Player = 3,
        PlayingManager = 4,
    }

    /// <summary>
    /// 打席区分
    /// </summary>
    public enum BatClass
    {
        Switch = 1,
        Right = 2,
        Left = 3,
    }

    /// <summary>
    /// 投げ区分
    /// </summary>
    public enum ThrowClass
    {
        Switch = 1,
        Right = 2,
        Left = 3,
    }

    /// <summary>
    /// ポジション
    /// </summary>
    public enum PositionClass
    {
        Pitcher = 1,
        Catcher = 2,
        First = 3,
        Second = 4,
        Third = 5,
        ShortStop = 6,
        Left = 7,
        Center = 8,
        Right = 9,
    }
}
