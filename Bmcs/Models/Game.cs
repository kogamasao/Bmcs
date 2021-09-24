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

        [Display(Name = "試合種別")]
        public GameClass? GameClass { get; set; }

        [NotMapped]
        [Display(Name = "試合種別")]
        public string GameClassName
        {
            get
            {
                return GameClass.GetEnumName();
            }
        }

        [Display(Name = "相手チーム名")]
        [StringLength(50)]
        public string OpponentTeamName { get; set; }

        [Display(Name = "相手チーム略名")]
        [StringLength(10)]
        public string OpponentTeamAbbreviation { get; set; }

        [Display(Name = "球場")]
        [StringLength(50)]
        public string StadiumName { get; set; }

        [Display(Name = "天候")]
        public WeatherClass? WeatherClass { get; set; }

        [NotMapped]
        [Display(Name = "天候")]
        public string WeatherClassName
        {
            get
            {
                return WeatherClass.GetEnumName();
            }
        }

        [Display(Name = "勝敗")]
        public WinLoseClass? WinLoseClass { get; set; }

        [NotMapped]
        [Display(Name = "勝敗")]
        public string WinLoseClassName
        {
            get
            {
                return WinLoseClass.GetEnumName();
            }
        }

        [Display(Name = "得点")]
        public int? Score { get; set; }

        [Display(Name = "失点")]
        public int? OpponentTeamScore { get; set; }

        [NotMapped]
        [Display(Name = "スコア")]
        public string GameScore
        {
            get
            {
                if (!string.IsNullOrEmpty(Score.NullToEmpty())
                    && !string.IsNullOrEmpty(OpponentTeamScore.NullToEmpty()))
                {
                    return Score.NullToEmpty() +"-"+ OpponentTeamScore.NullToEmpty();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [Display(Name = "先攻後攻")]
        public BatFirstBatSecondClass? BatFirstBatSecondClass { get; set; }

        [NotMapped]
        [Display(Name = "先攻後攻")]
        public string BatFirstBatSecondClassName
        {
            get
            {
                return BatFirstBatSecondClass.GetEnumName();
            }
        }

        [Display(Name = "試合入力タイプ")]
        public GameInputTypeClass? GameInputTypeClass { get; set; }

        [NotMapped]
        [Display(Name = "試合入力タイプ")]
        public string GameInputTypeClassName
        {
            get
            {
                return GameInputTypeClass.GetEnumName();
            }
        }

        [Display(Name = "ステータス")]
        public StatusClass? StatusClass { get; set; }

        [NotMapped]
        [Display(Name = "ステータス")]
        public string StatusClassName
        {
            get
            {
                return StatusClass.GetEnumName();
            }
        }

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
