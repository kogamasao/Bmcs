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

namespace Bmcs.Models
{
    /// <summary>
    /// ページモデルベース
    /// </summary>
    public abstract class PageModelBase : PageModel
    {
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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionConstant.UserID)))
            {
                return false;
            }

            return true;
        }
    }
}
