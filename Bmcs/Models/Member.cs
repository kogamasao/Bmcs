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
    public class Member : DataModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(50)]
        [Display(Name = "メンバーID")]
        public string MemberID { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [StringLength(50)]
        [Display(Name = "名前")]
        public string MemberName { get; set; }

        [Display(Name = "メンバー区分")]
        public MemberClass? MemberClass { get; set; }

        [Display(Name = "打")]
        public BatClass? BatClass { get; set; }

        [Display(Name = "投")]
        public ThrowClass? ThrowClass { get; set; }

        [Display(Name = "ポジション")]
        public PositionGroupClass? PositionGroupClass { get; set; }

        [StringLength(3)]
        [Display(Name = "背番号")]
        public string UniformNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "メッセージ")]
        public string Message { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        public Team Team { get; set; }
    }
}
