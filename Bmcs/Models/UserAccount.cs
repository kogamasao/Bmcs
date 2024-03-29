﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Data;

namespace Bmcs.Models
{
    public class UserAccount : DataModelBase, IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(50, ErrorMessage = "{0}は50桁以内で入力してください。")]
        [Display(Name = "ユーザID")]
        public string UserAccountID { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(50, ErrorMessage = "{0}は50桁以内で入力してください。")]
        [Display(Name = "ユーザ名")]
        public string UserAccountName { get; set; }

        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "確認用パスワード")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "メールアドレスの書式で入力してください。")]
        [Display(Name = "メールアドレス")]
        public string EmailAddress { get; set; }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "チームパスワード")]
        public string TeamPassword { get; set; }

        public Team Team { get; set; }

        public ICollection<Message> Messages { get; set; }

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
