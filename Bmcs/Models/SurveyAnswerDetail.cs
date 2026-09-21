using System;
using System.ComponentModel.DataAnnotations;

namespace Bmcs.Models
{
    /// <summary>
    /// アンケート回答明細
    /// </summary>
    public class SurveyAnswerDetail : DataModelBase
    {
        [Key]
        [Display(Name = "回答明細ID")]
        public int SurveyAnswerDetailID { get; set; }

        [Display(Name = "回答ID")]
        public int SurveyAnswerID { get; set; }

        [Display(Name = "設問ID")]
        public int SurveyQuestionID { get; set; }

        /// <summary>
        /// 選択した選択肢（自由記述の場合はnull）
        /// </summary>
        [Display(Name = "選択肢ID")]
        public int? SurveyChoiceID { get; set; }

        /// <summary>
        /// 自由記述の内容（「その他」選択時の入力を含む）
        /// ※"FreeText" はSQL Serverの予約語のため、列名は AnswerText とする
        /// </summary>
        [Display(Name = "自由記述")]
        public string AnswerText { get; set; }

        public SurveyAnswer SurveyAnswer { get; set; }
    }
}
