using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;
using Bmcs.Models;
using Bmcs.Enum;
using Bmcs.Function;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Bmcs.Constans;

namespace Bmcs.Pages.InningScore
{
    public class IndexModel : PageModelBase<IndexModel>
    {
        public IndexModel(ILogger<IndexModel> logger, BmcsContext context) : base(logger, context)
        {

        }

        public Models.Game Game { get; set; }

        public IList<Models.InningScore> InningScoreList { get;set; }

        public IList<Models.GameScene> GameSceneList { get;set; }

        public int? SelectInning { get; set; }

        public TopButtomClass? SelectTopButtomClass { get; set; }


        public async Task<IActionResult> OnGetAsync(int? gameID, int? inning, TopButtomClass? topButtomClass)
        {
            if (gameID == null)
            {
                return NotFound();
            }

            Game = await Context.Games
                .Include(m => m.Team).FirstOrDefaultAsync(m => m.GameID == gameID);

            if (Game == null)
            {
                return NotFound();
            }

            IsMyTeam = false;

            if (Game.TeamID == null
                || Game.TeamID == HttpContext.Session.GetString(SessionConstant.TeamID))
            {
                IsMyTeam = true;
            }

            if (!base.IsAdmin()
                && (Game.Team.DeleteFLG == true
                    || (Game.Team.PublicFLG == false && Game.TeamID != HttpContext.Session.GetString(SessionConstant.TeamID))
                    || Game.DeleteFLG == true
                    )
                )
            {
                return NotFound();
            }

            //イニング
            SelectInning = inning;

            //表裏
            SelectTopButtomClass = topButtomClass;

            //試合シーン
            InningScoreList = await Context.InningScores
                        .Where(r => r.GameID == Game.GameID)
                        .OrderBy(r => r.Inning)
                        .ThenBy(r => r.TopButtomClass)
                        .ToListAsync();

            //試合シーン(全て)
            var allGameSceneList = await Context.GameScenes
                        .Where(r => r.GameID == Game.GameID)
                        .ToListAsync();

            //試合シーン
            GameSceneList = await Context.GameScenes
                        .Include(r => r.BatterMember)
                        .Include(r => r.PitcherMember)
                        .Where(r => r.GameID == Game.GameID
                            && ((r.Inning == inning) || (inning == null))
                            && ((r.TopButtomClass == topButtomClass) || (topButtomClass == null))
                            )
                        .OrderBy(r => r.Inning)
                        .ThenBy(r => r.TopButtomClass)
                        .ThenBy(r => r.InningIndex)
                        .ToListAsync();

            //先攻後攻⇒表裏
            var myTeamOffenceTopButtomClass = Game.BatFirstBatSecondClass == BatFirstBatSecondClass.First ? TopButtomClass.Top : TopButtomClass.Buttom;

            //前回OUTカウント
            int? beforeOutCount = 0;
            //前回ランナー
            RunnerSceneClass? beforeRunnerSceneClass = RunnerSceneClass.None;

            foreach (var gameScene in GameSceneList)
            {
                //OUTカウント
                gameScene.InningScoreListOutCount = beforeOutCount.NullToEmpty() + "アウト";
                //ランナー
                gameScene.InningScoreListRunner = beforeRunnerSceneClass.GetEnumName();
                //打順
                gameScene.InningScoreListBattingOrder = gameScene.BattingOrder.DisplayDecimal();

                //試合詳細
                var gameSceneDetails = await Context.GameSceneDetails
                                        .Include(r => r.Member)
                                        .Where(r => r.GameID == Game.GameID)
                                        .ToListAsync();

                //結果
                foreach(var beforeGameSceneDetail in gameSceneDetails.Where(r => r.GameSceneID == gameScene.GameSceneID && r.SceneResultClass == SceneResultClass.SceneChange))
                {
                    if(!string.IsNullOrEmpty(beforeGameSceneDetail.MemberID.NullToEmpty()))
                    {
                        gameScene.InningScoreListDetail += beforeGameSceneDetail.Member.MemberName + "が" + beforeGameSceneDetail.DetailResultClass.GetEnumName(); 
                    }
                    else
                    {
                        gameScene.InningScoreListDetail += beforeGameSceneDetail.DetailResultClass.GetEnumName();
                    }

                    gameScene.InningScoreListDetail += "<br/>";
                }

                if(beforeRunnerSceneClass != gameScene.RunnerSceneClass
                    && gameScene.InningIndex > 1)
                {
                    gameScene.InningScoreListDetail += "ランナー" + gameScene.RunnerSceneClass.GetEnumName() + "に";
                    gameScene.InningScoreListDetail += "<br/>";
                }

                gameScene.InningScoreListDetail += "投手：" + gameScene.PitcherMember.MemberName + " 打者：" + gameScene.BatterMember.MemberName + " ";

                gameScene.InningScoreListDetail += GetBatterResultDetail(gameScene);

                foreach (var afterGameSceneDetail in gameSceneDetails.Where(r => r.GameSceneID == gameScene.GameSceneID && r.SceneResultClass == SceneResultClass.Result))
                {
                    gameScene.InningScoreListDetail += "<br/>";

                    if (!string.IsNullOrEmpty(afterGameSceneDetail.MemberID.NullToEmpty()))
                    {
                        gameScene.InningScoreListDetail += afterGameSceneDetail.Member.MemberName + "が" + afterGameSceneDetail.DetailResultClass.GetEnumName();
                    }
                    else
                    {
                        gameScene.InningScoreListDetail += afterGameSceneDetail.DetailResultClass.GetEnumName();
                    }
                }

                //スコア
                var myScoreList = allGameSceneList.Where(r => (r.Inning < gameScene.Inning && r.TopButtomClass == myTeamOffenceTopButtomClass)
                                                        || (r.Inning == gameScene.Inning && r.TopButtomClass == myTeamOffenceTopButtomClass
                                                            && ((myTeamOffenceTopButtomClass == TopButtomClass.Top && ((gameScene.TopButtomClass == TopButtomClass.Top && r.InningIndex <= gameScene.InningIndex) || (gameScene.TopButtomClass == TopButtomClass.Buttom)))
                                                                || (myTeamOffenceTopButtomClass == TopButtomClass.Buttom && ((gameScene.TopButtomClass == TopButtomClass.Top && r.InningIndex == 0) || (gameScene.TopButtomClass == TopButtomClass.Buttom && r.InningIndex <= gameScene.InningIndex)))
                                                                )
                                                            )
                                                        );
                var opponentScoreList = allGameSceneList.Where(r => (r.Inning < gameScene.Inning && r.TopButtomClass != myTeamOffenceTopButtomClass)
                                                        || (r.Inning == gameScene.Inning && r.TopButtomClass != myTeamOffenceTopButtomClass
                                                            && ((myTeamOffenceTopButtomClass == TopButtomClass.Top && ((gameScene.TopButtomClass == TopButtomClass.Top && r.InningIndex == 0) || (gameScene.TopButtomClass == TopButtomClass.Buttom && r.InningIndex <= gameScene.InningIndex)))
                                                                || (myTeamOffenceTopButtomClass == TopButtomClass.Buttom && ((gameScene.TopButtomClass == TopButtomClass.Top && r.InningIndex <= gameScene.InningIndex) || (gameScene.TopButtomClass == TopButtomClass.Buttom)))
                                                                )
                                                            )
                                                        );
                var myScore = 0;
                var opponentScore = 0;

                if(myScoreList != null && myScoreList.Any())
                {
                    myScore = myScoreList.DefaultIfEmpty().Sum(r => r.Run).NullToZero();
                }

                if (opponentScoreList != null && opponentScoreList.Any())
                {
                    opponentScore = opponentScoreList.DefaultIfEmpty().Sum(r => r.Run).NullToZero();
                }

                gameScene.InningScoreListGameScore = myScore.ToString() + "-" +  opponentScore.ToString();
            
                //チェンジ
                if(gameScene.ChangeFLG)
                {
                    //OUTカウント
                    beforeOutCount = 0;
                    //ランナー
                    beforeRunnerSceneClass = RunnerSceneClass.None;
                }
                else
                {
                    //OUTカウント
                    beforeOutCount = gameScene.ResultOutCount;
                    //ランナー
                    beforeRunnerSceneClass = gameScene.ResultRunnerSceneClass;
                }
            }

            return Page();

        }
    }
}
