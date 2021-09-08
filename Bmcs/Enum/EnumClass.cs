﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "公式戦")]
        Official = 1,
        [Display(Name = "練習試合")]
        Practice = 2,
        [Display(Name = "その他")]
        Other = 9,
    }

    /// <summary>
    /// チーム分類区分
    /// </summary>
    public enum TeamCategoryClass
    {
        [Display(Name = "小学生")]
        ElementarySchool = 1,
        [Display(Name = "中学生")]
        JuniorHighSchool = 2,
        [Display(Name = "高校生")]
        HighSchool = 3,
        [Display(Name = "大学生")]
        University = 4,
        [Display(Name = "社会人")]
        Adult = 5,
        [Display(Name = "独立リーグ")]
        Independent = 6,
        [Display(Name = "プロ")]
        Proffessional = 7,
        [Display(Name = "その他")]
        Other = 9,
    }

    /// <summary>
    /// 使用球区分
    /// </summary>
    public enum UseBallClass
    {
        [Display(Name = "硬式")]
        Hard = 1,
        [Display(Name = "準硬式")]
        SemiRigid = 2,
        [Display(Name = "軟式")] 
        Rubber = 3,
        [Display(Name = "ソフトボール")] 
        SoftBall = 4,
        [Display(Name = "その他")] 
        Other = 5,
    }

    /// <summary>
    /// メンバー区分
    /// </summary>
    public enum MemberClass
    {
        [Display(Name = "監督")]
        Manager = 1,
        [Display(Name = "選手兼監督")]
        PlayingManager = 2,
        [Display(Name = "コーチ")]
        Coach = 3,
        [Display(Name = "選手")]
        Player = 4,
        [Display(Name = "マネージャー")]
        Assistant = 5,
    }

    /// <summary>
    /// 投区分
    /// </summary>
    public enum ThrowClass
    {
        [Display(Name = "両")]
        Switch = 1,
        [Display(Name = "右")]
        Right = 2,
        [Display(Name = "左")]
        Left = 3,
    }

    /// <summary>
    /// 打席区分
    /// </summary>
    public enum BatClass
    {
        [Display(Name = "両")]
        Switch = 1,
        [Display(Name = "右")]
        Right = 2,
        [Display(Name = "左")]
        Left = 3,
    }

    /// <summary>
    /// 投球フォーム区分
    /// </summary>
    public enum ThrowFormClass
    {
        [Display(Name = "オーバー")]
        Over = 1,
        [Display(Name = "スリークォーター")]
        ThreeQuarter = 2,
        [Display(Name = "サイド")]
        Side = 3,
        [Display(Name = "アンダー")]
        Under = 4,
    }

    /// <summary>
    /// ポジショングループ
    /// </summary>
    public enum PositionGroupClass
    {
        [Display(Name = "投手")]
        Pitcher = 1,
        [Display(Name = "捕手")]
        Catcher = 2,
        [Display(Name = "内野手")]
        Infielder = 3,
        [Display(Name = "外野手")]
        Outfielder = 4,
    }

    /// <summary>
    /// ポジション
    /// </summary>
    public enum PositionClass
    {
        [Display(Name = "投")]
        Pitcher = 1,
        [Display(Name = "捕")]
        Catcher = 2,
        [Display(Name = "一")]
        First = 3,
        [Display(Name = "二")]
        Second = 4,
        [Display(Name = "三")]
        Third = 5,
        [Display(Name = "遊")]
        ShortStop = 6,
        [Display(Name = "左")]
        Left = 7,
        [Display(Name = "中")]
        Center = 8,
        [Display(Name = "右")]
        Right = 9,
        [Display(Name = "指")]
        DH = 10,
    }

    /// <summary>
    /// 天候
    /// </summary>
    public enum WeatherClass
    {
        [Display(Name = "晴")]
        Sunny = 1,
        [Display(Name = "曇")]
        Cloudy = 2,
        [Display(Name = "雨")]
        Rain = 3,
        [Display(Name = "雷")]
        Thunder = 4,
        [Display(Name = "雪")]
        Snow = 5,
    }

    /// <summary>
    /// 勝敗
    /// </summary>
    public enum WinLoseClass
    {
        [Display(Name = "〇")]
        Win = 1,
        [Display(Name = "●")]
        Lose = 2,
        [Display(Name = "△")]
        Draw = 3,
    }

    /// <summary>
    /// 試合入力タイプ
    /// </summary>
    public enum GameInputTypeClass
    {
        [Display(Name = "試合結果のみ")]
        OnlyGame = 1,
        [Display(Name = "打者毎")]
        ByBatter = 2,
    }

    /// <summary>
    /// ステータス
    /// </summary>
    public enum StatusClass
    {
        [Display(Name = "試合前")]
        BeforeGame = 1,
        [Display(Name = "試合中")]
        DuringGame = 2,
        [Display(Name = "試合終了")]
        EndGame = 9,
    }

    /// <summary>
    /// 攻守区分
    /// </summary>
    public enum OffenseDefenseClass
    {
        [Display(Name = "攻")]
        Offense = 1,
        [Display(Name = "守")]
        Defense = 2,
    }

    /// <summary>
    /// 先攻後攻区分
    /// </summary>
    public enum BatFirstBatSecondClass
    {
        [Display(Name = "先攻")]
        First = 1,
        [Display(Name = "後攻")]
        Second = 2,
    }

    /// <summary>
    /// 表裏区分
    /// </summary>
    public enum TopButtomClass
    {
        [Display(Name = "表")]
        Top = 1,
        [Display(Name = "裏")]
        Buttom = 2,
    }

    /// <summary>
    /// 出場区分
    /// </summary>
    public enum ParticipationClass
    {
        [Display(Name = "スタメン")]
        Start = 1,
        [Display(Name = "代打")]
        PinchHitter = 2,
        [Display(Name = "代走")]
        PinchRunner = 3,
        [Display(Name = "守備")]
        Defense = 4,
    }

    /// <summary>
    /// 打球方向区分
    /// </summary>
    public enum HittingDirectionClass
    {
        [Display(Name = "無")]
        None = 1,
        [Display(Name = "投")]
        Pitcher = 2,
        [Display(Name = "捕")]
        Catcher = 3,
        [Display(Name = "一")]
        First = 4,
        [Display(Name = "二")]
        Second = 5,
        [Display(Name = "三")]
        Third = 6,
        [Display(Name = "遊")]
        ShortStop = 7,
        [Display(Name = "左")]
        Left = 8,
        [Display(Name = "左中間")]
        LeftCenter = 9,
        [Display(Name = "中")]
        Center = 10,
        [Display(Name = "右中間")]
        RightCenter = 11,
        [Display(Name = "右")]
        Right = 12,
    }

    /// <summary>
    /// 打球区分
    /// </summary>
    public enum HitBallClass
    {
        [Display(Name = "無")]
        NoHit = 1,
        [Display(Name = "ゴロ")]
        Ground = 2,
        [Display(Name = "ライナー")]
        Liner = 3,
        [Display(Name = "フライ")]
        Fly = 4,
    }

    /// <summary>
    /// 結果区分
    /// </summary>
    public enum ResultClass
    {
        [Display(Name = "空三振")]
        Strikeout = 1,
        [Display(Name = "見逃三振")]
        MissedStrikeout = 2,
        [Display(Name = "振り逃げ")]
        UncaughtThirdStrike = 3,
        [Display(Name = "アウト")]
        Out = 4,
        [Display(Name = "併殺")]
        DoublePlay = 5,
        [Display(Name = "エラー")]
        Error = 6,
        [Display(Name = "野選")]
        FieldersChoice = 7,
        [Display(Name = "四球")]
        FourBalls = 11,
        [Display(Name = "死球")]
        DeadBall = 12,
        [Display(Name = "犠打")]
        Sacrifice = 13,
        [Display(Name = "犠牲")]
        SacrificeFly = 14,
        [Display(Name = "打撃妨害")]
        BatterInterference = 15,
        [Display(Name = "安打")]
        SingleHit = 21,
        [Display(Name = "二塁打")]
        DoubleHit = 22,
        [Display(Name = "三塁打")]
        TripleHit = 23,
        [Display(Name = "本塁打")]
        HomeRun = 24,
        [Display(Name = "継続")]
        Continue = 25,
    }

    /// <summary>
    /// ランナーシーン区分
    /// </summary>
    public enum RunnerSceneClass
    {
        [Display(Name = "無")]
        None = 1,
        [Display(Name = "一塁")]
        First = 2,
        [Display(Name = "二塁")]
        Second = 3,
        [Display(Name = "三塁")]
        Third = 4,
        [Display(Name = "一二塁")]
        FirstSecond = 5,
        [Display(Name = "一三塁")]
        FirstThird = 6,
        [Display(Name = "二三塁")]
        SecondThird = 7,
        [Display(Name = "満塁")]
        FullBase = 8,
    }

    /// <summary>
    /// シーン結果区分
    /// </summary>
    public enum SceneResultClass
    {
        [Display(Name = "打席前")]
        SceneChange = 1,
        [Display(Name = "打席後")]
        Result = 2,
    }

    /// <summary>
    /// 詳細結果区分
    /// </summary>
    public enum DetailResultClass
    {
        [Display(Name = "牽制死")]
        PickOffBallOut = 1,
        [Display(Name = "盗塁死")]
        StolenBaseOut = 2,
        [Display(Name = "盗塁")]
        StolenBaseSccess = 3,
        [Display(Name = "ボーク")]
        Balk = 4,
        [Display(Name = "WP")]
        WildPitch = 5,
        [Display(Name = "PB")]
        PassBall = 6,
        [Display(Name = "エラー")]
        Error = 7,
        [Display(Name = "補殺")]
        AssistOut = 8,
        [Display(Name = "その他")]
        Other = 99,
    }

    /// <summary>
    /// ランナー区分
    /// </summary>
    public enum RunnerClass
    {
        [Display(Name = "打者")]
        Batter = 1,
        [Display(Name = "一塁")]
        OnFirstBase = 2,
        [Display(Name = "二塁")]
        OnSecondBase = 3,
        [Display(Name = "三塁")]
        OnThirdBase = 4,
    }

    /// <summary>
    /// ランナー結果区分
    /// </summary>
    public enum RunnerResultClass
    {
        [Display(Name = "アウト")]
        Out = 1,
        [Display(Name = "一塁")]
        OnFirstBase = 2,
        [Display(Name = "二塁")]
        OnSecondBase = 3,
        [Display(Name = "三塁")]
        OnThirdBase = 4,
        [Display(Name = "得点")]
        Run = 5,
        [Display(Name = "得点(打点無)")]
        RunExceptRBI = 6,
        [Display(Name = "得点(自責無)")]
        RunExceptEarnedRun = 7,
        [Display(Name = "得点(打点自責無)")]
        RunExceptRBIEarnedRun = 8,
        [Display(Name = "残塁")]
        LeftOnBase = 9,
    }

    /// <summary>
    /// メッセージ区分
    /// </summary>
    public enum MessageClass
    {
        [Display(Name = "投稿")]
        Post = 1,
        [Display(Name = "返信")]
        Reply = 2,
    }

    /// <summary>
    /// 試合シーンSubmit区分
    /// </summary>
    public enum GameSceneSubmitClass
    {
        [Display(Name = "次打者")]
        NextBatter = 1,
        [Display(Name = "この打者でチェンジ")]
        ThisBatterChange = 2,
        [Display(Name = "前回打者でチェンジ")]
        BeforeBatterChange = 3,
        [Display(Name = "この打者で試合終了")]
        ThisBatterGameSet = 4,
        [Display(Name = "前回打者で試合終了")]
        BeforeBatterGameSet = 5,
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

            var attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);

            var desciptionString = attributes.Select(n => n.Name).FirstOrDefault();

            if (desciptionString != null)
            {
                return desciptionString;
            }

            return enumValue.ToString();
        }
    }

}
