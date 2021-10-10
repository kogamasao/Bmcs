using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Enum;
using Bmcs.Function;

namespace Bmcs.Models
{
    public class GameScorePitcher : DataModelBase
    {
        [Key]
        [Display(Name = "試合スコア投手ID")]
        public int GameScorePitcherID { get; set; }
        
        [Display(Name = "試合ID")]
        public int? GameID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Display(Name = "スコア表示順")]
        public int? ScoreIndex { get; set; }

        [Display(Name = "投手名")]
        public int? MemberID { get; set; }

        [StringLength(200)]
        [Display(Name = "詳細")]
        public string Detail { get; set; }

        [Display(Name = "勝")]
        public int? Win { get; set; }

        [Display(Name = "負")]
        public int? Lose { get; set; }

        [Display(Name = "ホールド")]
        public int? Hold { get; set; }

        [Display(Name = "セーブ")]
        public int? Save { get; set; }

        [Display(Name = "先発")]
        public int? Starter { get; set; }

        [Display(Name = "完投")]
        public int? CompleteGame { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "イニング")]
        public decimal? Inning { get; set; }

        [Display(Name = "打席")]
        public int? PlateAppearance { get; set; }

        [Display(Name = "打数")]
        public int? AtBat { get; set; }

        [Display(Name = "被安打")]
        public int? Hit { get; set; }

        [Display(Name = "被本塁打")]
        public int? HomeRun { get; set; }

        [Display(Name = "失点")]
        public int? Run { get; set; }

        [Display(Name = "自責点")]
        public int? EarnedRun { get; set; }

        [Display(Name = "与四球")]
        public int? FourBall { get; set; }

        [Display(Name = "与死球")]
        public int? DeadBall { get; set; }

        [Display(Name = "得点圏打席")]
        public int? ScoringPositionPlateAppearance { get; set; }

        [Display(Name = "得点圏打数")]
        public int? ScoringPositionAtBat { get; set; }

        [Display(Name = "得点圏被安打")]
        public int? ScoringPositionHit { get; set; }

        [Display(Name = "奪三振")]
        public int? StrikeOut { get; set; }

        [Display(Name = "牽制死")]
        public int? PickOffBallOut { get; set; }

        [Display(Name = "WP")]
        public int? WildPitch { get; set; }

        [Display(Name = "ボーク")]
        public int? Balk { get; set; }

        [NotMapped]
        [Display(Name = "勝敗HS")]
        public GameScorePitcherClass? GameScorePitcherClass { get; set; }

        [NotMapped]
        [Display(Name = "勝敗HS")]
        public string GameScorePitcherClassName
        {
            get
            {
                return GameScorePitcherClass.GetEnumName();
            }
        }

        [NotMapped]
        [Display(Name = "順位")]
        public int? Rank { get; set; }

        [NotMapped]
        public decimal OrderValue { get; set; }

        [NotMapped]
        [Display(Name = "年")]
        public string Year { get; set; }

        [NotMapped]
        [Display(Name = "試合数")]
        public int? GameCount { get; set; }

        [NotMapped]
        [Display(Name = "防御率")]
        public decimal? EarnedRunAverage { get; set; }

        [NotMapped]
        [Display(Name = "防御率")]
        public string EarnedRunAverageFormat
        {
            get
            {
                return EarnedRunAverage == null ? "-" : EarnedRunAverage.NullToZero().ToString("#0.00");
            }
        }

        [NotMapped]
        [Display(Name = "勝率")]
        public decimal? WinRate { get; set; }

        [NotMapped]
        [Display(Name = "勝率")]
        public string WinRateFormat
        {
            get
            {
                return WinRate == null ? "-" : WinRate.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "被打率")]
        public decimal? BattingAverage { get; set; }

        [NotMapped]
        [Display(Name = "被打率")]
        public string BattingAverageFormat
        {
            get
            {
                return BattingAverage == null ? "-" : BattingAverage.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "得点圏被打率")]
        public decimal? ScoringPositionBattingAverage { get; set; }

        [NotMapped]
        [Display(Name = "得点圏被打率")]
        public string ScoringPositionBattingAverageFormat
        {
            get
            {
                return ScoringPositionBattingAverage == null ? "-" : ScoringPositionBattingAverage.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "奪三振率")]
        public decimal? StrikeOutRate { get; set; }

        [NotMapped]
        [Display(Name = "奪三振率")]
        public string StrikeOutRateFormat
        {
            get
            {
                return StrikeOutRate == null ? "-" : StrikeOutRate.NullToZero().ToString("#0.00");
            }
        }

        [NotMapped]
        [Display(Name = "WHIP")]
        public decimal? Whip { get; set; }

        [NotMapped]
        [Display(Name = "WHIP")]
        public string WhipFormat
        {
            get
            {
                return Whip == null ? "-" : Whip.NullToZero().ToString("#0.00");
            }
        }

        public Game Game { get; set; }

        public Team Team { get; set; }

        public Member Member { get; set; }
    }
}
