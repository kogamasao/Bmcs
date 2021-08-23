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
    /// 攻撃守備区分
    /// </summary>
    public enum OffenseDefenseClass
    {
        Offense = 1,
        Defense = 2,
    }

    /// <summary>
    /// 表裏区分
    /// </summary>
    public enum TopButtomClass
    {
        Top = 1,
        Buttom = 2,
    }

    /// <summary>
    /// 出場区分
    /// </summary>
    public enum ParticipationClass
    {
        Start = 1,
        PinchHitter = 2,
        PinchRunner = 3,
        Defense = 4,
    }

    /// <summary>
    /// 打球区分
    /// </summary>
    public enum HitBallClass
    {
        Ground = 1,
        Liner = 2,
        Fly = 3,
        NoHit = 4,
    }

    /// <summary>
    /// 結果区分
    /// </summary>
    public enum ResultClass
    {
        Strikeout = 1,
        MissedStrikeout = 2,
        Out = 3,
        DoublePlay = 4,
        Error = 5,
        FourBalls = 11,
        DeadBall = 12,
        Sacrifice = 13,
        SacrificeFly = 14,
        SingleHit = 21,
        DoubleHit = 22,
        TripleHit = 23,
        HomeRun = 24,
    }

    /// <summary>
    /// ランナー区分
    /// </summary>
    public enum RunnerClass
    {
        None = 1,
        First = 2,
        Second = 3,
        Third = 4,
        FirstSecond = 5,
        FirstThird = 6,
        SecondThird = 7,
        FullBase = 8,
    }

    /// <summary>
    /// シーン結果区分
    /// </summary>
    public enum SceneResultClass
    {
        SceneChange = 1,
        Result = 2,
    }

    /// <summary>
    /// 詳細結果区分
    /// </summary>
    public enum DetailResultClass
    {
        PickOffBallOut = 1,
        StolenBaseOut = 2,
        StolenBaseSccess = 3,
        Balk = 4,
        WildPitch = 5,
        PassBall = 6,
        Error = 7,
        AssistOut = 8,
        Other = 99,
    }

    /// <summary>
    /// ランナー結果区分
    /// </summary>
    public enum RunnerResultClass
    {
        Out = 1,
        OnFirstBase = 2,
        OnSecondBase = 3,
        OnThirdBase = 4,
        Run = 5,
    }

    /// <summary>
    /// メッセージ区分
    /// </summary>
    public enum MessageClass
    {
        Post = 1,
        Reply = 2,
    }
}
