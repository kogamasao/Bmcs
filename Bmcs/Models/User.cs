using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Bmcs.Models
{
    public class User : DataModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(50)]
        [Display(Name = "ユーザID")]
        public string UserID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "ユーザ名")]
        public string UserName { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "メールアドレス")]
        public string EmailAddress { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        public Team Team { get; set; }
    }
}
