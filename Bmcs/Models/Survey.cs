using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bmcs.Enum;

namespace Bmcs.Models
{
    /// <summary>
    /// アンケート
    /// </summary>
    public class Survey : DataModelBase
    {
        [Key]
        [Display(Name = "アンケートID")]
        public int SurveyID { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(100, ErrorMessage = "{0}は100桁以内で入力してください。")]
        [Display(Name = "タイトル")]
        public string SurveyTitle { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "説明")]
        public string SurveyDetail { get; set; }

        [Display(Name = "開始日時")]
        public DateTime? StartDatetime { get; set; }

        [Display(Name = "終了日時")]
        public DateTime? EndDatetime { get; set; }

        [Display(Name = "状態")]
        public SurveyStatusClass StatusClass { get; set; }

        [NotMapped]
        [Display(Name = "状態")]
        public string StatusClassName
        {
            get
            {
                return StatusClass.GetEnumName();
            }
        }

        [DefaultValue(false)]
        [Display(Name = "削除フラグ")]
        public bool DeleteFLG { get; set; }

        public ICollection<SurveyQuestion> SurveyQuestions { get; set; }

        public ICollection<SurveyAnswer> SurveyAnswers { get; set; }

        /// <summary>
        /// 回答を受け付けている状態かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsOpen()
        {
            if (DeleteFLG || StatusClass != SurveyStatusClass.Open)
            {
                return false;
            }

            if (StartDatetime != null && StartDatetime > DateTime.Now)
            {
                return false;
            }

            if (EndDatetime != null && EndDatetime < DateTime.Now)
            {
                return false;
            }

            return true;
        }
    }
}
