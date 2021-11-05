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

        [NotMapped]
        [Display(Name = "登録日時")]
        public string EntryDatetimeFormat
        {
            get
            {
                return EntryDatetime != null ? ((DateTime)EntryDatetime).ToString("yyyy/MM/dd HH:mm:ss") : "";
            }
        }

        [StringLength(50)]
        [Display(Name = "更新ユーザID")]
        public string UpdateUserID { get; set; }

        [Display(Name = "更新日時")]
        public DateTime? UpdateDatetime { get; set; }

        [NotMapped]
        [Display(Name = "更新日時")]
        public string UpdateDatetimeFormat
        {
            get
            {
                return UpdateDatetime != null ? ((DateTime)UpdateDatetime).ToString("yyyy/MM/dd HH:mm:ss") : "";
            }
        }

        [Timestamp]
        [Display(Name = "タイムスタンプ")]
        public byte[] TimeStamp { get; set; }
    }
}
