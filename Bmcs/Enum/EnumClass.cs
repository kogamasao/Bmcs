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
    /// ポジショングループ
    /// </summary>
    public enum PositionGroupClass
    {
        Pitcher = 1,
        Catcher = 2,
        Infielder = 3,
        Outfielder = 4,
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
        DH = 10,
    }

    /// <summary>
    /// 勝敗
    /// </summary>
    public enum WinLoseClass
    {
        Win = 1,
        Lose = 2,
        Draw = 3,
    }

    /// <summary>
    /// ステータス
    /// </summary>
    public enum StatusClass
    {
        Incomplete = 1,
        Editing = 2,
        Complete = 9,
    }

    /// <summary>
    /// 表裏区分
    /// </summary>
    public enum TopButtomClass
    {
        Top = 1,
        Buttom = 2,
    }
}
