using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Bmcs.Models
{
    public class DataModelBase
    {
        [StringLength(50)]
        [Display(Name = "登録ユーザID")]
        public string EntryUserID { get; set; }

        [Display(Name = "登録日時")]
        public DateTime? EntryDatetime { get; set; }

        [StringLength(50)]
        [Display(Name = "更新ユーザID")]
        public string UpdateUserID { get; set; }

        [Display(Name = "更新日時")]
        public DateTime? UpdateDatetime { get; set; }
    }
}
