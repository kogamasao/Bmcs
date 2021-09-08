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
    public class GameScene : DataModelBase
    {
        [Key]
        [Display(Name = "試合シーンID")]
        public int GameSceneID { get; set; }
        
        [Display(Name = "試合ID")]
        public int GameID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Display(Name = "イニングスコアID")]
        public int? InningScoreID { get; set; }

        [Display(Name = "イニング")]
        public int Inning { get; set; }

        [Display(Name = "表裏")]
        public TopButtomClass? TopButtomClass { get; set; }

        [Display(Name = "イニングインデックス")]
        public int? InningIndex { get; set; }

        [Column(TypeName = "decimal(4, 2)")]
        [Display(Name = "打順")]
        public decimal? BattingOrder { get; set; }

        [Required]
        [Display(Name = "攻撃守備区分")]
        public OffenseDefenseClass? OffenseDefenseClass { get; set; }

        [Required]
        [Display(Name = "OUTカウント")]
        public int? SceneOutCount { get; set; }

        [Required]
        [Display(Name = "ランナー")]
        public RunnerSceneClass? RunnerSceneClass { get; set; }

        [ForeignKey(nameof(PitcherMember)), Column(Order = 0)]

        [Display(Name = "投手")]
        public int? PitcherMemberID { get; set; }

        [ForeignKey(nameof(BatterMember)), Column(Order = 1)]
        [Display(Name = "打者")]
        public int? BatterMemberID { get; set; }

        [Required]
        [Display(Name = "打球方向")]
        public HittingDirectionClass? HittingDirectionClass { get; set; }

        [Required]
        [Display(Name = "打球")]
        public HitBallClass? HitBallClass { get; set; }

        [Required]
        [Display(Name = "結果")]
        public ResultClass? ResultClass { get; set; }

        [Display(Name = "得点失点")]
        public int? Run { get; set; }

        [Display(Name = "打点")]
        public int? RBI { get; set; }

        [Display(Name = "自責点")]
        public int? EarnedRun { get; set; }

        [Display(Name = "結果Outカウント")]
        public int? ResultOutCount { get; set; }

        [Display(Name = "結果ランナー")]
        public RunnerSceneClass? ResultRunnerSceneClass { get; set; }

        [DefaultValue(false)]
        [Display(Name = "チェンジフラグ")]
        public bool ChangeFLG { get; set; }

        public Game Game { get; set; }

        public InningScore InningScore { get; set; }

        public Team Team { get; set; }

        [ForeignKey("PitcherMemberID")]
        public virtual Member PitcherMember { get; set; }

        [ForeignKey("BatterMemberID")]
        public virtual Member BatterMember { get; set; }

        public ICollection<Order> Orders { get; set; }

        public ICollection<GameSceneDetail> GameSceneDetails { get; set; }

        public ICollection<GameSceneRunner> GameSceneRunners { get; set; }
    }
}
