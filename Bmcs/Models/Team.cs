using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Bmcs.Models
{
    public class Team : DataModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Required]
        [Display(Name = "チーム名")]
        [StringLength(50)]
        public string TeamName { get; set; }

        [Display(Name = "代表者名")]
        public string RepresentativeName { get; set; }

        [Display(Name = "チーム人数")]
        public int? TeamNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string TeamPassword { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "メールアドレス")]
        public string TeamEmailAddress { get; set; }

        [Display(Name = "メッセージ")]
        public string Message { get; set; }

        [DefaultValue(true)]
        [Display(Name = "公開フラグ")]
        public bool PublicFLG { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        public ICollection<Member> Members { get; set; }
    }
}
