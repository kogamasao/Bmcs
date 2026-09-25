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

        public static readonly string UrlAfterLogin = "UrlAfterLogin";

        /// <summary>
        /// アンケートを「あとで回答する」で保留したかどうか
        /// </summary>
        public static readonly string SurveySkip = "SurveySkip";
    }

    /// <summary>
    /// ViewData定数
    /// </summary>
    public static class ViewDataConstant
    {
        public static readonly string Title = "Title";
        public static readonly string MessageMode = "MessageMode";

        /// <summary>
        /// ページごとの説明文（meta description・OGPに使用する）
        /// ※設定しない場合はサービス全体の説明文を使用する
        /// </summary>
        public static readonly string Description = "Description";
    }

    /// <summary>
    /// システム定数
    /// </summary>
    public static class SystemConstant
    {
        public static readonly string AdminUserAccountID = "ADMIN";

        /// <summary>
        /// サンプルチーム体験用のユーザID（トップの「サンプルチームで体験する」でログインする）
        /// ※wwwroot/js/index.js・DbInitializer と同じ値
        /// </summary>
        public static readonly string SampleUserAccountID = "YGUser";

        /// <summary>
        /// サンプルチームのチームID（体験用ユーザの所属チーム）
        /// ※DbInitializer と同じ値
        /// </summary>
        public static readonly string SampleTeamID = "YG";

        /// <summary>
        /// サンプルデータのチームID（体験用のサンプルチームと、その対戦相手のチーム）
        /// ※実在の球団名を使っており、誰でも書き換えられるため、検索エンジンに登録しない（サイトマップ・noindex）
        /// ※DbInitializer と同じ値
        /// </summary>
        public static readonly string[] SampleDataTeamIDList = { "YG", "HT" };

        /// <summary>
        /// 再設定トークンの有効時間（時間）
        /// </summary>
        public static readonly int ResetTokenExpireHour = 1;

        /// <summary>
        /// アカウント復旧機能の試行回数上限
        /// </summary>
        public static readonly int RecoverLimitCount = 10;

        /// <summary>
        /// アカウント復旧機能の試行回数を数える期間（分）
        /// </summary>
        public static readonly int RecoverLimitMinute = 10;

        /// <summary>
        /// メール送信を伴う受け付けの回数上限（同一対象への大量送信を防ぐ）
        /// </summary>
        public static readonly int SendMailLimitCount = 3;

        /// <summary>
        /// メール送信を伴う受け付けの回数を数える期間（分）
        /// </summary>
        public static readonly int SendMailLimitMinute = 60;

        /// <summary>
        /// アンケートの自由記述の文字数上限
        /// </summary>
        public static readonly int SurveyFreeTextMaxLength = 1000;

        /// <summary>
        /// 問い合わせの試行回数上限
        /// </summary>
        public static readonly int InquiryLimitCount = 5;

        /// <summary>
        /// 問い合わせの試行回数を数える期間（分）
        /// </summary>
        public static readonly int InquiryLimitMinute = 60;
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
