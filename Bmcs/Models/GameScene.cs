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

        [Required]
        [Display(Name = "攻撃守備区分")]
        public OffenseDefenseClass? OffenseDefenseClass { get; set; }

        [Required]
        [Display(Name = "OUTカウント")]
        public int? SceneOutCount { get; set; }

        [Required]
        [Display(Name = "ランナー")]
        public RunnerClass? SceneRunnerClass { get; set; }

        [Required]
        [Display(Name = "メンバーID")]
        public int MemberID { get; set; }

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

        [Display(Name = "打点自責点")]
        public int? RBIEarnedRun { get; set; }

        [Required]
        [Display(Name = "結果Outカウント")]
        public int? ResultOutCount { get; set; }

        [Required]
        [Display(Name = "結果ランナー")]
        public RunnerClass? ResultSceneRunnerClass { get; set; }

        public Game Game { get; set; }

        public InningScore InningScore { get; set; }

        public Team Team { get; set; }

        public Member Member { get; set; }

        public ICollection<Order> Orders { get; set; }

        public ICollection<GameSceneDetail> GameSceneDetails { get; set; }

        public ICollection<GameSceneRunner> GameSceneRunners { get; set; }
    }
}
