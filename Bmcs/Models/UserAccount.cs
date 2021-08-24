using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Bmcs.Models
{
    public class UserAccount : DataModelBase, IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(50)]
        [Display(Name = "ユーザID")]
        public string UserAccountID { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(50)]
        [Display(Name = "ユーザ名")]
        public string UserAccountName { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "確認用パスワード")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "メールアドレス")]
        public string EmailAddress { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        public Team Team { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult(
                    "パスワードが一致していません。",
                    new[] { nameof(Password), nameof(ConfirmPassword) });
            }
        }
    }
}
