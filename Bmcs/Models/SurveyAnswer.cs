using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bmcs.Models
{
    /// <summary>
    /// アンケート回答
    /// </summary>
    public class SurveyAnswer : DataModelBase
    {
        [Key]
        [Display(Name = "回答ID")]
        public int SurveyAnswerID { get; set; }

        [Display(Name = "アンケートID")]
        public int SurveyID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "ユーザID")]
        public string UserAccountID { get; set; }

        /// <summary>
        /// 回答時点のチームID
        /// ※回答後にチームを移動しても集計がぶれないよう、回答時の値を保持する
        /// </summary>
        [StringLength(50)]
        [Display(Name = "チームID")]
        public string TeamID { get; set; }

        [Display(Name = "回答日時")]
        public DateTime AnswerDatetime { get; set; }

        public Survey Survey { get; set; }

        public ICollection<SurveyAnswerDetail> SurveyAnswerDetails { get; set; }
    }
}
