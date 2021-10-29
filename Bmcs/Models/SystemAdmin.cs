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
    public class SystemAdmin : DataModelBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "システム管理区分")]
        public SystemAdminClass SystemAdminClass { get; set; }

        [Display(Name = "メッセージタイトル")]
        public string MessageTitle { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "メッセージ")]
        public string MessageDetail { get; set; }
    }
}
