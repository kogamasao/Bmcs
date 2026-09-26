using System;
using System.Collections.Generic;
using Bmcs.Enum;

namespace Bmcs.Models
{
    /// <summary>
    /// アップデート情報の1件
    /// ※DB には保存しない。内容は Data/UpdateHistoryList.cs に書く（機能と同じコミットで追加するため）
    /// </summary>
    public class UpdateHistory
    {
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 日付を年月までで表示するか（日付がはっきりしない古い記録）
        /// </summary>
        public bool IsMonthOnly { get; set; }

        /// <summary>
        /// 区分（新機能・改善・お知らせ）
        /// </summary>
        public UpdateHistoryClass UpdateHistoryClass { get; set; }

        /// <summary>
        /// 見出し
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容（箇条書き）
        /// </summary>
        public List<string> DetailList { get; set; } = new List<string>();

        /// <summary>
        /// 表示用の日付
        /// </summary>
        public string DateFormat
        {
            get
            {
                return IsMonthOnly ? Date.ToString("yyyy年M月") : Date.ToString("yyyy年M月d日");
            }
        }

        /// <summary>
        /// 区分の表示名
        /// </summary>
        public string UpdateHistoryClassName
        {
            get
            {
                return UpdateHistoryClass.GetEnumName();
            }
        }
    }
}
