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
    public class Game : DataModelBase
    {
        [Key]
        [Display(Name = "試合ID")]
        public int GameID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "日付")]
        public DateTime GameDate { get; set; }

        [Display(Name = "種別")]
        public GameClass? GameClass { get; set; }

        [Display(Name = "相手チーム名")]
        [StringLength(50)]
        public string OpponentTeamName { get; set; }

        [Display(Name = "球場")]
        [StringLength(50)]
        public string StadiumName { get; set; }

        [Display(Name = "勝敗")]
        public WinLoseClass? WinLoseClass { get; set; }

        [Display(Name = "得点")]
        public int? Score { get; set; }

        [Display(Name = "失点")]
        public int? OpponentTeamScore { get; set; }

        [Required]
        [Display(Name = "先攻後攻")]
        public TopButtomClass? TopButtomClass { get; set; }

        [Display(Name = "ステータス")]
        public StatusClass? StatusClass { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        public Team Team { get; set; }

        public ICollection<GameScene> GameScenes { get; set; }

        public ICollection<InningScore> InningScores { get; set; }

        public ICollection<GameSceneDetail> GameSceneDetails { get; set; }

        public ICollection<GameSceneRunner> GameSceneRunners { get; set; }

        public ICollection<GameScorePitcher> GameScorePitchers { get; set; }

        public ICollection<GameScoreFielder> GameScoreFielders { get; set; }
    }
}
