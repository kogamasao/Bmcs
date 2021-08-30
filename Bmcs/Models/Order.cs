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
    public class Order : DataModelBase
    {
        [Key]
        [Display(Name = "オーダーID")]
        public int OrderID { get; set; }

        [Display(Name = "試合ID")]
        public int? GameID { get; set; }

        [Display(Name = "試合シーンID")]
        public int? GameSceneID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Display(Name = "メンバーID")]
        public int? MemberID { get; set; }

        [Required]
        [Column(TypeName = "decimal(4, 2)")]
        [Display(Name = "打順")]
        public decimal? BattingOrder { get; set; }

        [Display(Name = "出場順")]
        public int? ParticipationIndex { get; set; }

        [Required]
        [Display(Name = "守備")]
        public PositionClass? PositionClass { get; set; }

        [Display(Name = "出場")]
        public ParticipationClass? ParticipationClass { get; set; }

        public Game Game { get; set; }

        public GameScene GameScene { get; set; }

        public Team Team { get; set; }

        public Member Member { get; set; }
    }
}
