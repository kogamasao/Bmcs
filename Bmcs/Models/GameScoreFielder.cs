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
    public class GameScoreFielder : DataModelBase
    {
        [Key]
        [Display(Name = "試合スコア野手ID")]
        public int GameScoreFielderID { get; set; }
        
        [Display(Name = "試合ID")]
        public int? GameID { get; set; }

        [StringLength(50)]
        [Display(Name = "チーム")]
        public string TeamID { get; set; }

        [Display(Name = "スコア表示順")]
        public int? ScoreIndex { get; set; }

        [Column(TypeName = "decimal(4, 2)")]
        [Display(Name = "打順")]
        public decimal? BattingOrder { get; set; }

        [NotMapped]
        [Display(Name = "打順")]
        public string DisplayBattingOrder
        {
            get
            {
                return System.Convert.ToDecimal(BattingOrder).ToString("0.##");
            }
        }

        [Display(Name = "野手名")]
        public int? MemberID { get; set; }

        [StringLength(200)]
        [Display(Name = "守備詳細")]
        public string PositionDetail { get; set; }

        [StringLength(200)]
        [Display(Name = "打撃詳細")]
        public string BattingDetail { get; set; }

        [Display(Name = "打席")]
        public int? PlateAppearance { get; set; }

        [Display(Name = "打数")]
        public int? AtBat { get; set; }

        [Display(Name = "安打")]
        public int? Hit { get; set; }

        [Display(Name = "二塁打")]
        public int? DoubleHit { get; set; }

        [Display(Name = "三塁打")]
        public int? TripleHit { get; set; }

        [Display(Name = "本塁打")]
        public int? HomeRun { get; set; }

        [Display(Name = "塁打")]
        public int? TotalBase { get; set; }

        [Display(Name = "打点")]
        public int? RBI { get; set; }

        [Display(Name = "得点")]
        public int? Run { get; set; }

        [Display(Name = "盗塁企画数")]
        public int? StolenBasePlan { get; set; }

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

        [Display(Name = "得点圏打席")]
        public int? ScoringPositionPlateAppearance { get; set; }

        [Display(Name = "得点圏打数")]
        public int? ScoringPositionAtBat { get; set; }

        [Display(Name = "得点圏安打")]
        public int? ScoringPositionHit { get; set; }

        [Display(Name = "三振")]
        public int? StrikeOut { get; set; }

        [Display(Name = "併殺打")]
        public int? DoublePlay { get; set; }

        [Display(Name = "敵失策")]
        public int? Error { get; set; }

        [Display(Name = "被盗塁企画数")]
        public int? StolenBasePlaned { get; set; }

        [Display(Name = "盗塁阻止数")]
        public int? StopStolenBase { get; set; }

        [Display(Name = "補殺")]
        public int? Assist { get; set; }

        [Display(Name = "失策")]
        public int? OwnError { get; set; }

        [Display(Name = "PB")]
        public int? PassBall { get; set; }

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
        [Display(Name = "打率")]
        public decimal? BattingAverage { get; set; }

        [NotMapped]
        [Display(Name = "打率")]
        public string BattingAverageFormat
        {
            get
            {
                return BattingAverage == null ? "-" : BattingAverage.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "得点圏打率")]
        public decimal? ScoringPositionBattingAverage { get; set; }

        [NotMapped]
        [Display(Name = "得点圏打率")]
        public string ScoringPositionBattingAverageFormat
        {
            get
            {
                return ScoringPositionBattingAverage == null ? "-" : ScoringPositionBattingAverage.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "出塁率")]
        public decimal? OnBasePercentage { get; set; }

        [NotMapped]
        [Display(Name = "出塁率")]
        public string OnBasePercentageFormat
        {
            get
            {
                return OnBasePercentage == null ? "-" : OnBasePercentage.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "長打率")]
        public decimal? SluggingPercentage { get; set; }

        [NotMapped]
        [Display(Name = "長打率")]
        public string SluggingPercentageFormat
        {
            get
            {
                return SluggingPercentage == null ? "-" : SluggingPercentage.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "OPS")]
        public decimal? Ops { get; set; }

        [NotMapped]
        [Display(Name = "OPS")]
        public string OpsFormat
        {
            get
            {
                return Ops == null ? "-" : Ops.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "盗塁成功率")]
        public decimal? StolenBaseSuccessRate { get; set; }

        [NotMapped]
        [Display(Name = "盗塁成功率")]
        public string StolenBaseSuccessRateFormat
        {
            get
            {
                return StolenBaseSuccessRate == null ? "-" : StolenBaseSuccessRate.NullToZero().ToString("#.000");
            }
        }

        [NotMapped]
        [Display(Name = "盗塁阻止率")]
        public decimal? StopStolenBaseRate { get; set; }

        [NotMapped]
        [Display(Name = "盗塁阻止率")]
        public string StopStolenBaseRateFormat
        {
            get
            {
                return StopStolenBaseRate == null ? "-" : StopStolenBaseRate.NullToZero().ToString("#.000");
            }
        }

        public Game Game { get; set; }

        public Team Team { get; set; }

        public Member Member { get; set; }
    }
}
