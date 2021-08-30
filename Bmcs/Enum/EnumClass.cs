using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bmcs.Enum
{
    /// <summary>
    /// 試合区分
    /// </summary>
    public enum GameClass
    {
        [Description("公式戦")]
        Official = 1,
        [Description("練習試合")]
        Practice = 2,
        [Description("その他")]
        Other = 9,
    }

    /// <summary>
    /// チーム分類区分
    /// </summary>
    public enum TeamCategoryClass
    {
        [Description("小学生")]
        ElementarySchool = 1,
        [Description("中学生")]
        JuniorHighSchool = 2,
        [Description("高校生")]
        HighSchool = 3,
        [Description("大学生")]
        University = 4,
        [Description("社会人")]
        Adult = 5,
        [Description("独立リーグ")]
        Independent = 6,
        [Description("プロ")]
        Proffessional = 7,
        [Description("その他")]
        Other = 9,
    }

    /// <summary>
    /// 使用球区分
    /// </summary>
    public enum UseBallClass
    {
        [Description("硬式")]
        Hard = 1,
        [Description("準硬式")]
        SemiRigid = 2,
        [Description("軟式")] 
        Rubber = 3,
        [Description("ソフトボール")] 
        SoftBall = 4,
        [Description("その他")] 
        Other = 5,
    }

    /// <summary>
    /// メンバー区分
    /// </summary>
    public enum MemberClass
    {
        [Description("監督")]
        Manager = 1,
        [Description("選手兼監督")]
        PlayingManager = 2,
        [Description("コーチ")]
        Coach = 3,
        [Description("選手")]
        Player = 4,
        [Description("マネージャー")]
        Assistant = 5,
    }

    /// <summary>
    /// 投区分
    /// </summary>
    public enum ThrowClass
    {
        [Description("両")]
        Switch = 1,
        [Description("右")]
        Right = 2,
        [Description("左")]
        Left = 3,
    }

    /// <summary>
    /// 打席区分
    /// </summary>
    public enum BatClass
    {
        [Description("両")]
        Switch = 1,
        [Description("右")]
        Right = 2,
        [Description("左")]
        Left = 3,
    }

    /// <summary>
    /// 投球フォーム区分
    /// </summary>
    public enum ThrowFormClass
    {
        [Description("オーバー")]
        Over = 1,
        [Description("スリークォーター")]
        ThreeQuarter = 2,
        [Description("サイド")]
        Side = 3,
        [Description("アンダー")]
        Under = 4,
    }

    /// <summary>
    /// ポジショングループ
    /// </summary>
    public enum PositionGroupClass
    {
        [Description("投手")]
        Pitcher = 1,
        [Description("捕手")]
        Catcher = 2,
        [Description("内野手")]
        Infielder = 3,
        [Description("外野手")]
        Outfielder = 4,
    }

    /// <summary>
    /// ポジション
    /// </summary>
    public enum PositionClass
    {
        [Description("投")]
        Pitcher = 1,
        [Description("捕")]
        Catcher = 2,
        [Description("一")]
        First = 3,
        [Description("二")]
        Second = 4,
        [Description("三")]
        Third = 5,
        [Description("遊")]
        ShortStop = 6,
        [Description("左")]
        Left = 7,
        [Description("中")]
        Center = 8,
        [Description("右")]
        Right = 9,
        [Description("指")]
        DH = 10,
    }

    /// <summary>
    /// 天候
    /// </summary>
    public enum WeatherClass
    {
        [Description("晴")]
        Sunny = 1,
        [Description("曇")]
        Cloudy = 2,
        [Description("雨")]
        Rain = 3,
        [Description("雷")]
        Thunder = 4,
        [Description("雪")]
        Snow = 5,
    }

    /// <summary>
    /// 勝敗
    /// </summary>
    public enum WinLoseClass
    {
        [Description("〇")]
        Win = 1,
        [Description("●")]
        Lose = 2,
        [Description("△")]
        Draw = 3,
    }

    /// <summary>
    /// 試合入力タイプ
    /// </summary>
    public enum GameInputTypeClass
    {
        [Description("試合結果のみ")]
        OnlyGame = 1,
        [Description("打者毎")]
        ByBatter = 2,
    }

    /// <summary>
    /// ステータス
    /// </summary>
    public enum StatusClass
    {
        [Description("試合前")]
        BeforeGame = 1,
        [Description("試合中")]
        DuringGame = 2,
        [Description("試合終了")]
        EndGame = 9,
    }

    /// <summary>
    /// 攻守区分
    /// </summary>
    public enum OffenseDefenseClass
    {
        [Description("攻")]
        Offense = 1,
        [Description("守")]
        Defense = 2,
    }

    /// <summary>
    /// 先攻後攻区分
    /// </summary>
    public enum BatFirstBatSecondClass
    {
        [Description("先攻")]
        First = 1,
        [Description("後攻")]
        Second = 2,
    }

    /// <summary>
    /// 表裏区分
    /// </summary>
    public enum TopButtomClass
    {
        [Description("表")]
        Top = 1,
        [Description("裏")]
        Buttom = 2,
    }

    /// <summary>
    /// 出場区分
    /// </summary>
    public enum ParticipationClass
    {
        [Description("スタメン")]
        Start = 1,
        [Description("代打")]
        PinchHitter = 2,
        [Description("代走")]
        PinchRunner = 3,
        [Description("守備")]
        Defense = 4,
    }

    /// <summary>
    /// 打球方向区分
    /// </summary>
    public enum HittingDirectionClass
    {
        [Description("無")]
        None = 1,
        [Description("投")]
        Pitcher = 2,
        [Description("捕")]
        Catcher = 3,
        [Description("一")]
        First = 4,
        [Description("二")]
        Second = 5,
        [Description("三")]
        Third = 6,
        [Description("遊")]
        ShortStop = 7,
        [Description("左")]
        Left = 8,
        [Description("左中間")]
        LeftCenter = 9,
        [Description("中")]
        Center = 10,
        [Description("右中間")]
        RightCenter = 11,
        [Description("右")]
        Right = 12,
    }

    /// <summary>
    /// 打球区分
    /// </summary>
    public enum HitBallClass
    {
        [Description("無")]
        NoHit = 1,
        [Description("ゴロ")]
        Ground = 2,
        [Description("ライナー")]
        Liner = 3,
        [Description("フライ")]
        Fly = 4,
    }

    /// <summary>
    /// 結果区分
    /// </summary>
    public enum ResultClass
    {
        [Description("空三振")]
        Strikeout = 1,
        [Description("見逃三振")]
        MissedStrikeout = 2,
        [Description("振り逃げ")]
        UncaughtThirdStrike = 3,
        [Description("アウト")]
        Out = 4,
        [Description("併殺")]
        DoublePlay = 5,
        [Description("エラー")]
        Error = 6,
        [Description("野選")]
        FieldersChoice = 7,
        [Description("四球")]
        FourBalls = 11,
        [Description("死球")]
        DeadBall = 12,
        [Description("犠打")]
        Sacrifice = 13,
        [Description("犠牲")]
        SacrificeFly = 14,
        [Description("打撃妨害")]
        BatterInterference = 15,
        [Description("安打")]
        SingleHit = 21,
        [Description("二塁打")]
        DoubleHit = 22,
        [Description("三塁打")]
        TripleHit = 23,
        [Description("本塁打")]
        HomeRun = 24,
    }

    /// <summary>
    /// ランナー区分
    /// </summary>
    public enum RunnerClass
    {
        [Description("無")]
        None = 1,
        [Description("一塁")]
        First = 2,
        [Description("二塁")]
        Second = 3,
        [Description("三塁")]
        Third = 4,
        [Description("一二塁")]
        FirstSecond = 5,
        [Description("一三塁")]
        FirstThird = 6,
        [Description("二三塁")]
        SecondThird = 7,
        [Description("満塁")]
        FullBase = 8,
    }

    /// <summary>
    /// シーン結果区分
    /// </summary>
    public enum SceneResultClass
    {
        [Description("打席前")]
        SceneChange = 1,
        [Description("打席後")]
        Result = 2,
    }

    /// <summary>
    /// 詳細結果区分
    /// </summary>
    public enum DetailResultClass
    {
        [Description("牽制死")]
        PickOffBallOut = 1,
        [Description("盗塁死")]
        StolenBaseOut = 2,
        [Description("盗塁")]
        StolenBaseSccess = 3,
        [Description("ボーク")]
        Balk = 4,
        [Description("WP")]
        WildPitch = 5,
        [Description("PB")]
        PassBall = 6,
        [Description("エラー")]
        Error = 7,
        [Description("補殺")]
        AssistOut = 8,
        [Description("その他")]
        Other = 99,
    }

    /// <summary>
    /// ランナー結果区分
    /// </summary>
    public enum RunnerResultClass
    {
        [Description("アウト")]
        Out = 1,
        [Description("一塁")]
        OnFirstBase = 2,
        [Description("二塁")]
        OnSecondBase = 3,
        [Description("三塁")]
        OnThirdBase = 4,
        [Description("得点")]
        Run = 5,
        [Description("残塁")]
        LeftOnBase = 6,
    }

    /// <summary>
    /// メッセージ区分
    /// </summary>
    public enum MessageClass
    {
        [Description("投稿")]
        Post = 1,
        [Description("返信")]
        Reply = 2,
    }

    /// <summary>
    /// Enum関連処理
    /// </summary>
    public static class EnumClass
    {
        /// <summary>
        /// SelectList取得
        /// </summary>
        /// <returns></returns>
        public static SelectList GetSelectList<T>(bool isEmptyItem = true) where T : struct
        {
            var selectList = new List<SelectListItem>();
            
            if(isEmptyItem)
            { 
                selectList.Add(new SelectListItem(string.Empty, string.Empty));
            }

            foreach (var value in System.Enum.GetValues(typeof(T)))
            {
                selectList.Add(new SelectListItem(value.GetEnumName(), ((int)value).ToString()));
            }

            return new SelectList(selectList, "Value", "Text");

        }

        /// <summary>
        /// Enum名取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumName<T>(this T enumValue)
        {
            if(enumValue == null)
            {
                return string.Empty;
            }

            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            var desciptionString = attributes.Select(n => n.Description).FirstOrDefault();

            if (desciptionString != null)
            {
                return desciptionString;
            }

            return enumValue.ToString();
        }
    }

}
