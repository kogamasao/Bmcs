using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bmcs.Enum;

namespace Bmcs.Models
{
    /// <summary>
    /// 再設定トークン
    /// ユーザパスワード、チームパスワードの再設定に使用する。
    /// </summary>
    public class ResetToken : DataModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(100)]
        [Display(Name = "トークン")]
        public string ResetTokenID { get; set; }

        [Display(Name = "トークン区分")]
        public ResetTokenClass ResetTokenClass { get; set; }

        /// <summary>
        /// 対象ID（ユーザパスワードの場合はユーザID、チームパスワードの場合はチームID）
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "対象ID")]
        public string TargetID { get; set; }

        [Display(Name = "有効期限")]
        public DateTime ExpireDatetime { get; set; }

        [Display(Name = "使用済フラグ")]
        public bool UsedFLG { get; set; }
    }
}
