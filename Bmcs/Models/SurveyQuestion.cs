using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bmcs.Enum;

namespace Bmcs.Models
{
    /// <summary>
    /// アンケート設問
    /// </summary>
    public class SurveyQuestion : DataModelBase
    {
        [Key]
        [Display(Name = "設問ID")]
        public int SurveyQuestionID { get; set; }

        [Display(Name = "アンケートID")]
        public int SurveyID { get; set; }

        [Display(Name = "表示順")]
        public int DisplayOrder { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(200, ErrorMessage = "{0}は200桁以内で入力してください。")]
        [Display(Name = "設問")]
        public string QuestionText { get; set; }

        [StringLength(200)]
        [Display(Name = "補足説明")]
        public string QuestionNote { get; set; }

        [Display(Name = "回答形式")]
        public SurveyAnswerTypeClass AnswerTypeClass { get; set; }

        [Display(Name = "必須フラグ")]
        public bool RequiredFLG { get; set; }

        /// <summary>
        /// 複数選択時に選択できる上限数（未設定時は制限なし）
        /// </summary>
        [Display(Name = "選択上限数")]
        public int? MaxSelectCount { get; set; }

        public Survey Survey { get; set; }

        public ICollection<SurveyChoice> SurveyChoices { get; set; }
    }
}
