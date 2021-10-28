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
    public class Inquiry : DataModelBase
    {
        [Key]
        [Display(Name = "お問い合わせID")]
        public int InquiryID { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [DataType(DataType.EmailAddress, ErrorMessage = "メールアドレスの書式で入力してください。")]
        [Display(Name = "メールアドレス")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [Display(Name = "お問い合わせタイトル")]
        [StringLength(50, ErrorMessage = "{0}は50桁以内で入力してください。")]
        public string InquiryTitle { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "お問い合わせ内容")]
        public string InquiryDetail { get; set; }
    }
}
