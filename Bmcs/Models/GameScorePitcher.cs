using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Enum;

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

        [Required]
        [Display(Name = "メンバーID")]
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

        [Display(Name = "完投")]
        public int? CompleteGame { get; set; }

        [Display(Name = "イニング")]
        public decimal? Inning { get; set; }

        [Display(Name = "打席")]
        public int? Plateappearance { get; set; }

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
        public int? ScoringPositionPlateappearance { get; set; }

        [Display(Name = "得点圏打数")]
        public int? ScoringPositionAtBat { get; set; }

        [Display(Name = "得点圏被安打")]
        public int? ScoringPositionHit { get; set; }

        [Display(Name = "奪三振")]
        public int? StrikeOut { get; set; }

        [Display(Name = "WP")]
        public int? WildPitch { get; set; }

        public Game Game { get; set; }

        public Team Team { get; set; }

        public Member Member { get; set; }
    }
}
