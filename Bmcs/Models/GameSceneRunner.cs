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
    public class GameSceneRunner : DataModelBase
    {
        [Key]
        [Display(Name = "試合シーンランナーID")]
        public int GameSceneRunnerID { get; set; }

        [Display(Name = "試合シーンID")]
        public int? GameSceneID { get; set; }
        
        [Display(Name = "試合ID")]
        public int? GameID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Display(Name = "メンバーID")]
        public int? MemberID { get; set; }

        [Column(TypeName = "decimal(4, 2)")]
        [Display(Name = "打順")]
        public decimal? BattingOrder { get; set; }

        [Display(Name = "打席中ランナー")]
        public RunnerClass? BeforeRunnerClass { get; set; }

        [Display(Name = "ランナー")]
        public RunnerClass? RunnerClass { get; set; }

        [Display(Name = "シーン結果区分")]
        public SceneResultClass? SceneResultClass { get; set; }

        [Display(Name = "結果")]
        public RunnerResultClass? RunnerResultClass { get; set; }

        public Game Game { get; set; }

        public GameScene GameScene { get; set; }

        public Team Team { get; set; }

        public Member Member { get; set; }
    }
}
