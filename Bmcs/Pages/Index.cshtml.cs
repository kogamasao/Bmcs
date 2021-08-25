using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Constans;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages
{
    public class IndexModel : PageModelBase
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(context)
        {
            _logger = logger;
        }

        [BindProperty]
        public Models.UserAccount UserAccount { get; set; }

        public void OnGet()
        {

            HttpContext.Session.SetString(SessionConstant.UserAccountID, "ADMIN");
            HttpContext.Session.SetString(SessionConstant.AdminFLG, "1");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                //ユーザIDチェック
                var dbUserAccount = await Context.UserAccounts.FirstOrDefaultAsync(r => r.UserAccountID == UserAccount.UserAccountID
                                                                                && r.Password == UserAccount.Password);

                if (dbUserAccount == null)
                {
                    ModelState.AddModelError(nameof(Models.UserAccount) + "." + nameof(Models.UserAccount.UserAccountID), "入力したユーザID、またはパスワードが間違っています。");

                    return Page();
                }
                else
                {
                    //ログイン情報セット
                    HttpContext.Session.SetString(SessionConstant.UserAccountID, dbUserAccount.UserAccountID);
                    HttpContext.Session.SetString(SessionConstant.TeamID, dbUserAccount.TeamID);

                    //管理者権限
                    if(dbUserAccount.UserAccountID== SystemConstant.AdminUserAccountID)
                    { 
                        HttpContext.Session.SetString(SessionConstant.AdminFLG, "1");
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToPage("./Team/Index");
        }
    }
}
