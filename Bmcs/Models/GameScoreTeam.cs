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
    public class GameScoreTeam
    {
        [Display(Name = "順位")]
        public int? Rank { get; set; }

        [Display(Name = "チーム")]
        public string TeamID { get; set; }

        [Display(Name = "年")]

        public string Year { get; set; }

        [Display(Name = "試合数")]
        public int? GameCount { get; set; }

        [Display(Name = "勝")]
        public int? Win { get; set; }

        [Display(Name = "負")]
        public int? Lose { get; set; }

        [Display(Name = "分")]
        public int? Draw { get; set; }

        [Display(Name = "勝率")]
        public decimal? WinRate { get; set; }

        [Display(Name = "勝率")]
        public string WinRateFormat
        {
            get
            {
                return WinRate == null ? "-" : WinRate.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "打率")]
        public decimal? BattingAverage { get; set; }

        [Display(Name = "打率")]
        public string BattingAverageFormat
        {
            get
            {
                return BattingAverage == null ? "-" : BattingAverage.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "安打")]
        public int? Hit { get; set; }

        [Display(Name = "二塁打")]
        public int? DoubleHit { get; set; }

        [Display(Name = "三塁打")]
        public int? TripleHit { get; set; }

        [Display(Name = "本塁打")]
        public int? HomeRun { get; set; }

        [Display(Name = "打点")]
        public int? RBI { get; set; }

        [Display(Name = "得点")]
        public int? Run { get; set; }

        [Display(Name = "盗塁成功率")]
        public decimal? StolenBaseSuccessRate { get; set; }

        [Display(Name = "盗塁成功率")]
        public string StolenBaseSuccessRateFormat
        {
            get
            {
                return StolenBaseSuccessRate == null ? "-" : StolenBaseSuccessRate.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "盗塁")]
        public int? StolenBase { get; set; }

        [Display(Name = "四球")]
        public int? FourBall { get; set; }

        [Display(Name = "死球")]
        public int? DeadBall { get; set; }

        [Display(Name = "犠打")]
        public int? Sacrifice { get; set; }

        [Display(Name = "犠牲")]
        public int? SacrificeFly { get; set; }

        [Display(Name = "残塁")]
        public int? LeftOnBase { get; set; }

        [Display(Name = "出塁率")]
        public decimal? OnBasePercentage { get; set; }

        [Display(Name = "出塁率")]
        public string OnBasePercentageFormat
        {
            get
            {
                return OnBasePercentage == null ? "-" : OnBasePercentage.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "長打率")]
        public decimal? SluggingPercentage { get; set; }

        [Display(Name = "長打率")]
        public string SluggingPercentageFormat
        {
            get
            {
                return SluggingPercentage == null ? "-" : SluggingPercentage.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "OPS")]
        public decimal? Ops { get; set; }

        [Display(Name = "OPS")]
        public string OpsFormat
        {
            get
            {
                return Ops == null ? "-" : Ops.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "得点圏打率")]
        public decimal? ScoringPositionBattingAverage { get; set; }

        [Display(Name = "得点圏打率")]
        public string ScoringPositionBattingAverageFormat
        {
            get
            {
                return ScoringPositionBattingAverage == null ? "-" : ScoringPositionBattingAverage.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "三振")]
        public int? StrikeOut { get; set; }

        [Display(Name = "盗塁阻止率")]
        public decimal? StopStolenBaseRate { get; set; }

        [Display(Name = "盗塁阻止率")]
        public string StopStolenBaseRateFormat
        {
            get
            {
                return StopStolenBaseRate == null ? "-" : StopStolenBaseRate.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "盗塁阻止数")]
        public int? StopStolenBase { get; set; }

        [Display(Name = "失策")]
        public int? OwnError { get; set; }


        [Display(Name = "防御率")]
        public decimal? EarnedRunAverage { get; set; }

        [Display(Name = "防御率")]
        public string EarnedRunAverageFormat
        {
            get
            {
                return EarnedRunAverage == null ? "-" : EarnedRunAverage.NullToZero().ToString("#0.00");
            }
        }

        [Display(Name = "QS率(%)")]
        public decimal? QualityStartRate { get; set; }

        [Display(Name = "QS率(%)")]
        public string QualityStartRateFormat
        {
            get
            {
                return QualityStartRate == null ? "-" : QualityStartRate.NullToZero().ToString("0.#");
            }
        }

        [Display(Name = "失点")]
        public int? PitcherRun { get; set; }

        [Display(Name = "自責点")]
        public int? PitcherEarnedRun { get; set; }

        [Display(Name = "与四球")]
        public int? PitcherFourBall { get; set; }

        [Display(Name = "与死球")]
        public int? PitcherDeadBall { get; set; }

        [Display(Name = "被安打")]
        public int? PitcherHit { get; set; }

        [Display(Name = "被本塁打")]
        public int? PitcherHomeRun { get; set; }

        [Display(Name = "被打率")]
        public decimal? PitcherBattingAverage { get; set; }

        [Display(Name = "被打率")]
        public string PitcherBattingAverageFormat
        {
            get
            {
                return PitcherBattingAverage == null ? "-" : PitcherBattingAverage.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "得点圏被打率")]
        public decimal? PitcherScoringPositionBattingAverage { get; set; }

        [Display(Name = "得点圏被打率")]
        public string PitcherScoringPositionBattingAverageFormat
        {
            get
            {
                return PitcherScoringPositionBattingAverage == null ? "-" : PitcherScoringPositionBattingAverage.NullToZero().ToString("#.000");
            }
        }

        [Display(Name = "奪三振率")]
        public decimal? PitcherStrikeOutRate { get; set; }

        [Display(Name = "奪三振率")]
        public string PitcherStrikeOutRateFormat
        {
            get
            {
                return PitcherStrikeOutRate == null ? "-" : PitcherStrikeOutRate.NullToZero().ToString("#0.00");
            }
        }

        [Display(Name = "奪三振")]
        public int? PitcherStrikeOut { get; set; }

        [Display(Name = "K/BB")]
        public decimal? PitcherStrikeOutBaseOnBallsRate { get; set; }

        [Display(Name = "K/BB")]
        public string PitcherStrikeOutBaseOnBallsRateFormat
        {
            get
            {
                return PitcherStrikeOutBaseOnBallsRate == null ? "-" : PitcherStrikeOutBaseOnBallsRate.NullToZero().ToString("#0.00");
            }
        }

        [Display(Name = "WHIP")]
        public decimal? Whip { get; set; }

        [Display(Name = "WHIP")]
        public string WhipFormat
        {
            get
            {
                return Whip == null ? "-" : Whip.NullToZero().ToString("#0.00");
            }
        }

        public decimal OrderValue { get; set; }

        public Team Team { get; set; }
    }
}
