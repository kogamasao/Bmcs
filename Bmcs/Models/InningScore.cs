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
    public class InningScore : DataModelBase
    {
        [Key]
        [Display(Name = "イニングスコアID")]
        public int InningScoreID { get; set; }

        [Display(Name = "試合ID")]
        public int GameID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Required]
        [Display(Name = "イニング")]
        public int Inning { get; set; }

        [Required]
        [Display(Name = "表裏")]
        public TopButtomClass? TopButtomClass { get; set; }

        [Display(Name = "得点")]
        public int? Score { get; set; }

        public Game Game { get; set; }

        public Team Team { get; set; }

        public ICollection<GameScene> GameScenes { get; set; }

        public ICollection<GameSceneDetail> GameSceneDetails { get; set; }

        public ICollection<GameSceneRunner> GameSceneRunners { get; set; }
    }
}
