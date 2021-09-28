using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Bmcs.Constans;
using Bmcs.Data;
using Bmcs.Enum;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Models
{
    /// <summary>
    /// ページモデルベース
    /// </summary>
    public abstract class PageModelBase<T> : PageModel
    {
        public PageModelBase(ILogger<T> logger, BmcsContext context) 
        {
            Logger = logger;
            Context = context;
        }

        public PageModelBase(ILogger<T> logger)
        {
            Logger = logger;
        }

        public PageModelBase(BmcsContext context)
        {
            Context = context;
        }

        /// <summary>
        /// ログ
        /// </summary>
        public readonly ILogger<T> Logger;

        /// <summary>
        /// DBコンテキスト
        /// </summary>
        public readonly BmcsContext Context;

        /// <summary>
        /// マイチームフラグ
        /// </summary>
        public bool IsMyTeam { get; set; }

        /// <summary>
        /// チームID
        /// </summary>
        public string TeamID { get; set; }

        /// <summary>
        /// メンバーID空必要フラグ
        /// </summary>
        public bool IsMemberIDNeedEmpty { get; set; }

        /// <summary>
        /// チームIDリスト
        /// </summary>
        public SelectList TeamIDList
        { 
            get
            {
                return AddFirstItem(new SelectList(Context.Teams.Where(r => r.DeleteFLG == false), nameof(Team.TeamID), nameof(Team.TeamIDName), string.Empty)
                    , new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// メンバーIDリスト
        /// </summary>
        public SelectList MemberIDList
        {
            get
            {
                return AddFirstItem(new SelectList(Context.Members.Where(r => r.TeamID == this.TeamID && r.DeleteFLG == false).OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty)
                    , new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// メンバーIDリスト(空なし)
        /// </summary>
        public SelectList MemberIDNoEmptyList
        {
            get
            {
                return new SelectList(Context.Members.Where(r => r.TeamID == this.TeamID && r.DeleteFLG == false).OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);
            }
        }

        /// <summary>
        /// メンバーIDリスト(相手チーム含む)
        /// </summary>
        public SelectList MemberIDIncludeOpponentMemberList
        {
            get
            {
                var memberIDIncludeOpponentMemberList = new SelectList(Context.Members
                                                .Where(r => (r.TeamID == this.TeamID || r.SystemDataFLG) && r.DeleteFLG == false)
                                                .OrderBy(r => r.SystemDataFLG)
                                                .ThenBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);

                return AddFirstItem(memberIDIncludeOpponentMemberList, new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// 相手メンバーIDリスト
        /// </summary>
        public SelectList OpponentMemberIDList
        {
            get
            {
                return AddFirstItem(new SelectList(Context.Members
                                    .Where(r => r.SystemDataFLG)
                                        .OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty)
                    , new SelectListItem(string.Empty, string.Empty));
            }
        }

        /// <summary>
        /// 相手投手メンバーIDリスト
        /// </summary>
        public SelectList OpponentPitcherMemberIDList
        {
            get
            {
                return new SelectList(Context.Members
                                    .Where(r => r.SystemDataFLG && r.PositionGroupClass == PositionGroupClass.Pitcher)
                                        .OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);
            }
        }

        /// <summary>
        /// 相手野手メンバーIDリスト
        /// </summary>
        public SelectList OpponentFielderMemberIDList
        {
            get
            {
                return new SelectList(Context.Members
                                    .Where(r => r.SystemDataFLG && r.PositionGroupClass != PositionGroupClass.Pitcher)
                                        .OrderBy(r => r.UniformNumber), nameof(Member.MemberID), nameof(Member.UniformNumberMemberName), string.Empty);
            }
        }

        /// <summary>
        /// 試合区分リスト
        /// </summary>
        public SelectList GameClassList
        {
            get
            {
                return EnumClass.GetSelectList<GameClass>();
            }
        }

        /// <summary>
        /// チーム分類区分リスト
        /// </summary>
        public SelectList TeamCategoryClassList
        {
            get
            {
                return EnumClass.GetSelectList<TeamCategoryClass>();
            }
        }

        /// <summary>
        /// 使用球リスト
        /// </summary>
        public SelectList UseBallClassList
        {
            get
            {
                return EnumClass.GetSelectList<UseBallClass>();
            }
        }

        /// <summary>
        /// メンバー区分リスト
        /// </summary>
        public SelectList MemberClassList
        {
            get
            {
                return EnumClass.GetSelectList<MemberClass>();
            }
        }

        /// <summary>
        /// 打席区分リスト
        /// </summary>
        public SelectList BatClassList
        {
            get
            {
                return EnumClass.GetSelectList<BatClass>();
            }
        }

        /// <summary>
        /// 投区分リスト
        /// </summary>
        public SelectList ThrowClassList
        {
            get
            {
                return EnumClass.GetSelectList<ThrowClass>();
            }
        }

        /// <summary>
        /// 投球フォーム区分リスト
        /// </summary>
        public SelectList ThrowFormClassList
        {
            get
            {
                return EnumClass.GetSelectList<ThrowFormClass>();
            }
        }

        /// <summary>
        /// ポジショングループ区分リスト
        /// </summary>
        public SelectList PositionGroupClassList
        {
            get
            {
                return EnumClass.GetSelectList<PositionGroupClass>();
            }
        }

        /// <summary>
        /// ポジション区分リスト
        /// </summary>
        public SelectList PositionClassList
        {
            get
            {
                return EnumClass.GetSelectList<PositionClass>();
            }
        }

        /// <summary>
        /// 天候区分リスト
        /// </summary>
        public SelectList WeatherClassList
        {
            get
            {
                return EnumClass.GetSelectList<WeatherClass>();
            }
        }

        /// <summary>
        /// 勝敗区分リスト
        /// </summary>
        public SelectList WinLoseClassList
        {
            get
            {
                return EnumClass.GetSelectList<WinLoseClass>();
            }
        }

         /// <summary>
        /// 試合入力タイプ区分リスト
        /// </summary>
        public SelectList GameInputTypeClassList
        {
            get
            {
                return EnumClass.GetSelectList<GameInputTypeClass>(false);
            }
        }

        /// <summary>
        /// ステータス区分リスト
        /// </summary>
        public SelectList StatusClassList
        {
            get
            {
                return EnumClass.GetSelectList<StatusClass>();
            }
        }

        /// <summary>
        /// 攻守区分リスト
        /// </summary>
        public SelectList OffenseDefenseClassList
        {
            get
            {
                return EnumClass.GetSelectList<OffenseDefenseClass>();
            }
        }

        /// <summary>
        /// 攻守区分リスト
        /// </summary>
        public SelectList BatFirstBatSecondClassList
        {
            get
            {
                return EnumClass.GetSelectList<BatFirstBatSecondClass>(false);
            }
        }

        /// <summary>
        /// 表裏区分リスト
        /// </summary>
        public SelectList TopButtomClassList
        {
            get
            {
                return EnumClass.GetSelectList<TopButtomClass>(false);
            }
        }

        /// <summary>
        /// 出場区分リスト
        /// </summary>
        public SelectList ParticipationClassList
        {
            get
            {
                return EnumClass.GetSelectList<ParticipationClass>();
            }
        }

        /// <summary>
        /// 打球方向区分リスト
        /// </summary>
        public SelectList HittingDirectionClassList
        {
            get
            {
                return EnumClass.GetSelectList<HittingDirectionClass>(false);
            }

        }
        /// <summary>
        /// 打球区分リスト
        /// </summary>
        public SelectList HitBallClassList
        {
            get
            {
                return EnumClass.GetSelectList<HitBallClass>(false);
            }
        }

        /// <summary>
        /// 結果区分リスト
        /// </summary>
        public SelectList ResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<ResultClass>(false);
            }
        }

        /// <summary>
        /// ランナー区分リスト
        /// </summary>
        public SelectList RunnerSceneClassList
        {
            get
            {
                return EnumClass.GetSelectList<RunnerSceneClass>();
            }
        }

        /// <summary>
        /// シーン結果区分リスト
        /// </summary>
        public SelectList SceneResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<SceneResultClass>();
            }
        }

        /// <summary>
        /// シーン詳細結果区分リスト
        /// </summary>
        public SelectList BeforeDetailResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<DetailResultClass>();
            }
        }

        /// <summary>
        /// 結果詳細結果区分リスト
        /// </summary>
        public SelectList AfterDetailResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<DetailResultClass>(true, (int)DetailResultClass.Error);
            }

        }
        /// <summary>
        /// ランナー結果区分リスト
        /// </summary>
        public SelectList RunnerResultClassList
        {
            get
            {
                return EnumClass.GetSelectList<RunnerResultClass>(false);
            }
        }

        /// <summary>
        /// 試合スコア投手区分リスト
        /// </summary>
        public SelectList GameScorePicherClassList
        {
            get
            {
                return EnumClass.GetSelectList<GameScorePitcherClass>();
            }
        }

        /// <summary>
        /// メッセージ区分リスト
        /// </summary>
        public SelectList MessageClassList
        {
            get
            {
                return EnumClass.GetSelectList<MessageClass>();
            }
        }


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
        public void SetEntryInfo<TModel>(TModel dataModelBase) where TModel : DataModelBase
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
        public void SetUpdateInfo<TModel>(TModel dataModelBase) where TModel : DataModelBase
        {
            dataModelBase.UpdateDatetime = DateTime.Now;
            dataModelBase.UpdateUserID = HttpContext.Session.GetString(SessionConstant.UserAccountID);
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

        /// <summary>
        /// 仮オーダー削除
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public async Task DeleteTempOrders(int? gameID, OrderDataClass? orderDataClass)
        {
            //オーダー
            var tempOrders = await Context.Orders
                                .Where(r => r.GameID == gameID
                                    && r.OrderDataClass == orderDataClass).ToListAsync();

            Context.Orders.RemoveRange(tempOrders);

            await Context.SaveChangesAsync();
        }
    }
}
