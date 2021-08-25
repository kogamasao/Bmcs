using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Bmcs.Constans;
using Bmcs.Data;

namespace Bmcs.Models
{
    /// <summary>
    /// ページモデルベース
    /// </summary>
    public abstract class PageModelBase : PageModel
    {
        public PageModelBase(Bmcs.Data.BmcsContext context)
        {
            Context = context;
        }

        public readonly BmcsContext Context;

        public SelectList TeamIDList { get; set; }

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);
        }

        public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            base.OnPageHandlerSelected(context);
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);
        }

        /// <summary>
        /// ログイン判定
        /// </summary>
        /// <returns></returns>
        public bool IsLogin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.UserAccountID)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 管理者判定
        /// </summary>
        /// <returns></returns>
        public bool IsAdmin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.AdminFLG)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// エントリ情報セット
        /// </summary>
        /// <returns></returns>
        public void SetEntryInfo<T>(T dataModelBase) where T : DataModelBase
        {
            dataModelBase.EntryDatetime = DateTime.Now;
            dataModelBase.EntryUserID = HttpContext.Session.GetString(SessionConstant.UserAccountID);
            dataModelBase.UpdateDatetime = dataModelBase.EntryDatetime;
            dataModelBase.UpdateUserID = dataModelBase.EntryUserID;
        }

        /// <summary>
        /// エントリ情報セット
        /// </summary>
        /// <returns></returns>
        public void SetUpdateInfo<T>(T dataModelBase) where T : DataModelBase
        {
            dataModelBase.UpdateDatetime = DateTime.Now;
            dataModelBase.UpdateUserID = HttpContext.Session.GetString(SessionConstant.UserAccountID);
        }

        /// <summary>
        /// SelectList取得
        /// </summary>
        /// <returns></returns>
        public void GetSelectList()
        {
            //チームID
            TeamIDList = AddFirstItem(new SelectList(Context.Teams.Where(r => r.DeleteFLG == false), nameof(Team.TeamID), nameof(Team.TeamIDName), string.Empty)
                                    , new SelectListItem(string.Empty, string.Empty));
        }

        /// <summary>
        /// 先頭行に要素を追加
        /// </summary>
        /// <param name="selectList"></param>
        /// <param name="firstItem"></param>
        /// <returns></returns>
        private SelectList AddFirstItem(SelectList selectList, SelectListItem firstItem)
        {
            List<SelectListItem> newList = selectList.ToList();
            newList.Insert(0, firstItem);

            var selectedItem = newList.FirstOrDefault(item => item.Selected);
            var selectedItemValue = string.Empty;

            if (selectedItem != null)
            {
                selectedItemValue = selectedItem.Value;
            }

            return new SelectList(newList, "Value", "Text", selectedItemValue);
        }
    }
}
