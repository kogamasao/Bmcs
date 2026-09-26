using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Bmcs.Models;
using Bmcs.Data;

namespace Bmcs.Pages.Updates
{
    /// <summary>
    /// アップデート情報
    /// ※内容は Data/UpdateHistoryList.cs に書く（DB は使わない）
    /// </summary>
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        /// <summary>
        /// アップデート情報（新しい順）
        /// </summary>
        public IList<UpdateHistory> UpdateHistoryList { get; set; }

        public void OnGet()
        {
            UpdateHistoryList = Data.UpdateHistoryList.All.OrderByDescending(r => r.Date).ToList();

            //検索エンジンに登録する（サービスが継続して更新されていることが分かるように）
            SetIndex("/Updates/Index");
            MetaDescription = "Bmcs（草野球・ソフトボールのスコア管理）の新機能・改善・お知らせの一覧です。いつ、どんな機能が追加されたかをご確認いただけます。";
        }
    }
}
