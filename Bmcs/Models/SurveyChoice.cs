using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bmcs.Models
{
    /// <summary>
    /// アンケート選択肢
    /// </summary>
    public class SurveyChoice : DataModelBase
    {
        [Key]
        [Display(Name = "選択肢ID")]
        public int SurveyChoiceID { get; set; }

        [Display(Name = "設問ID")]
        public int SurveyQuestionID { get; set; }

        [Display(Name = "表示順")]
        public int DisplayOrder { get; set; }

        [Required(ErrorMessage = "{0}は必須です。")]
        [StringLength(200, ErrorMessage = "{0}は200桁以内で入力してください。")]
        [Display(Name = "選択肢")]
        public string ChoiceText { get; set; }

        /// <summary>
        /// 選択時に自由入力欄を表示するか（「その他」用）
        /// </summary>
        [Display(Name = "自由入力フラグ")]
        public bool FreeTextFLG { get; set; }

        public SurveyQuestion SurveyQuestion { get; set; }
    }
}
