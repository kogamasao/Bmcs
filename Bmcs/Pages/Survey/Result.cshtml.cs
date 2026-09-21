using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Bmcs.Data;
using Bmcs.Enum;
using Bmcs.Models;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.Survey
{
    /// <summary>
    /// アンケート集計（管理者用）
    /// </summary>
    public class ResultModel : PageModelBase<ResultModel>
    {
        public ResultModel(ILogger<ResultModel> logger, BmcsContext context) : base(logger, context)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int? SurveyID { get; set; }

        /// <summary>
        /// チームカテゴリでの絞り込み
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public TeamCategoryClass? TeamCategoryClass { get; set; }

        /// <summary>
        /// 回答内容での絞り込み（単一選択の設問の選択肢）
        /// ※「チームの代表が選んだ機能」のように、属性ごとの傾向を見るために使用する
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int? FilterChoiceID { get; set; }

        public Models.Survey Survey { get; set; }

        public SelectList SurveyIDList { get; set; }

        /// <summary>
        /// 回答内容での絞り込みの選択肢
        /// </summary>
        public SelectList FilterChoiceIDList { get; set; }

        /// <summary>
        /// 回答者数（絞り込み後）
        /// </summary>
        public int AnswerCount { get; set; }

        /// <summary>
        /// 回答者数（絞り込みなし）
        /// </summary>
        public int TotalAnswerCount { get; set; }

        /// <summary>
        /// 回答したチーム数（絞り込み後）
        /// </summary>
        public int AnswerTeamCount { get; set; }

        /// <summary>
        /// 対象ユーザ数（有効なユーザ全体）
        /// </summary>
        public int TargetUserCount { get; set; }

        /// <summary>
        /// 回答率（対象ユーザ数に対する割合）
        /// </summary>
        public decimal AnswerRate { get; set; }

        /// <summary>
        /// 設問ごとの集計結果
        /// </summary>
        public List<QuestionResult> QuestionResultList { get; set; } = new List<QuestionResult>();

        public class QuestionResult
        {
            public SurveyQuestion SurveyQuestion { get; set; }

            /// <summary>
            /// この設問に回答した人数（分母）
            /// </summary>
            public int AnsweredCount { get; set; }

            /// <summary>
            /// 無回答の人数
            /// </summary>
            public int NoAnswerCount { get; set; }

            public List<ChoiceResult> ChoiceResultList { get; set; } = new List<ChoiceResult>();

            /// <summary>
            /// 自由記述の内容（回答者の属性付き）
            /// </summary>
            public List<FreeTextResult> FreeTextList { get; set; } = new List<FreeTextResult>();
        }

        public class ChoiceResult
        {
            public string ChoiceText { get; set; }

            public int Count { get; set; }

            /// <summary>
            /// この設問に回答した人数に対する割合
            /// </summary>
            public decimal Rate { get; set; }

            /// <summary>
            /// 「その他」に入力された内容
            /// </summary>
            public List<FreeTextResult> FreeTextList { get; set; } = new List<FreeTextResult>();
        }

        public class FreeTextResult
        {
            public string AnswerText { get; set; }

            /// <summary>
            /// 回答者の属性（チーム名・カテゴリ・試合登録数）
            /// </summary>
            public string AnswererNote { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            if (!base.IsAdmin())
            {
                return NotFound();
            }

            var surveyList = await Context.Surveys
                .Where(r => r.DeleteFLG == false)
                .OrderByDescending(r => r.SurveyID)
                .ToListAsync();

            //同名のアンケートを区別できるよう、IDを併記する
            SurveyIDList = new SelectList(
                surveyList.Select(r => new { r.SurveyID, Title = $"[{r.SurveyID}] {r.SurveyTitle}" }),
                nameof(Models.Survey.SurveyID), "Title");

            Survey = SurveyID == null
                     ? surveyList.FirstOrDefault()
                     : surveyList.FirstOrDefault(r => r.SurveyID == SurveyID);

            if (Survey == null)
            {
                return Page();
            }

            SurveyID = Survey.SurveyID;

            await SetResultAsync();

            return Page();
        }

        /// <summary>
        /// 集計結果を作成する
        /// </summary>
        /// <returns></returns>
        private async Task SetResultAsync()
        {
            var questionList = await Context.SurveyQuestions
                .Include(r => r.SurveyChoices)
                .Where(r => r.SurveyID == Survey.SurveyID)
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();

            //回答内容での絞り込み用（単一選択かつ選択肢が10個以下の設問）
            SetFilterChoiceIDList(questionList);

            var answerList = await Context.SurveyAnswers
                .Where(r => r.SurveyID == Survey.SurveyID)
                .ToListAsync();

            TotalAnswerCount = answerList.Count;

            TargetUserCount = await Context.UserAccounts
                .CountAsync(r => r.DeleteFLG == false);

            //チームカテゴリでの絞り込み
            if (TeamCategoryClass != null)
            {
                var teamIDList = await Context.Teams
                    .Where(r => r.TeamCategoryClass == TeamCategoryClass)
                    .Select(r => r.TeamID)
                    .ToListAsync();

                answerList = answerList.Where(r => teamIDList.Contains(r.TeamID)).ToList();
            }

            var detailList = await Context.SurveyAnswerDetails
                .Where(r => r.SurveyAnswer.SurveyID == Survey.SurveyID)
                .ToListAsync();

            //回答内容での絞り込み（例：立場が「チームの代表」の回答だけを見る）
            if (FilterChoiceID != null)
            {
                var filteredAnswerIDList = detailList
                    .Where(r => r.SurveyChoiceID == FilterChoiceID)
                    .Select(r => r.SurveyAnswerID)
                    .ToList();

                answerList = answerList.Where(r => filteredAnswerIDList.Contains(r.SurveyAnswerID)).ToList();
            }

            var answerIDList = answerList.Select(r => r.SurveyAnswerID).ToList();

            AnswerCount = answerIDList.Count;
            AnswerTeamCount = answerList.Select(r => r.TeamID).Distinct().Count();
            AnswerRate = TargetUserCount == 0
                         ? 0
                         : Math.Round((decimal)TotalAnswerCount * 100 / TargetUserCount, 1);

            detailList = detailList.Where(r => answerIDList.Contains(r.SurveyAnswerID)).ToList();

            //自由記述に添える回答者の属性
            var answererNoteDictionary = await CreateAnswererNoteDictionaryAsync(answerList);

            foreach (var question in questionList)
            {
                var questionDetailList = detailList.Where(r => r.SurveyQuestionID == question.SurveyQuestionID).ToList();

                var questionResult = new QuestionResult
                {
                    SurveyQuestion = question,
                    //設問ごとの分母（任意設問は無回答がいるため、全回答者数とは一致しない）
                    AnsweredCount = questionDetailList.Select(r => r.SurveyAnswerID).Distinct().Count(),
                };

                questionResult.NoAnswerCount = AnswerCount - questionResult.AnsweredCount;

                if (question.AnswerTypeClass == SurveyAnswerTypeClass.FreeText)
                {
                    questionResult.FreeTextList = questionDetailList
                        .Where(r => !string.IsNullOrWhiteSpace(r.AnswerText))
                        .Select(r => new FreeTextResult
                        {
                            AnswerText = r.AnswerText,
                            AnswererNote = GetAnswererNote(answererNoteDictionary, r.SurveyAnswerID),
                        })
                        .ToList();
                }
                else
                {
                    foreach (var choice in question.SurveyChoices.OrderBy(r => r.DisplayOrder))
                    {
                        var choiceDetailList = questionDetailList
                            .Where(r => r.SurveyChoiceID == choice.SurveyChoiceID)
                            .ToList();

                        questionResult.ChoiceResultList.Add(new ChoiceResult
                        {
                            ChoiceText = choice.ChoiceText,
                            Count = choiceDetailList.Count,
                            //分母は「その設問に回答した人数」とする
                            Rate = questionResult.AnsweredCount == 0
                                   ? 0
                                   : Math.Round((decimal)choiceDetailList.Count * 100 / questionResult.AnsweredCount, 1),
                            FreeTextList = choiceDetailList
                                .Where(r => !string.IsNullOrWhiteSpace(r.AnswerText))
                                .Select(r => new FreeTextResult
                                {
                                    AnswerText = r.AnswerText,
                                    AnswererNote = GetAnswererNote(answererNoteDictionary, r.SurveyAnswerID),
                                })
                                .ToList(),
                        });
                    }
                }

                QuestionResultList.Add(questionResult);
            }
        }

        /// <summary>
        /// 回答内容での絞り込みの選択肢を作成する
        /// ※単一選択かつ選択肢が10個以下の設問を対象とする（属性を表す設問を想定）
        /// </summary>
        /// <param name="questionList"></param>
        private void SetFilterChoiceIDList(List<SurveyQuestion> questionList)
        {
            var itemList = new List<SelectListItem>();

            foreach (var question in questionList
                .Where(r => r.AnswerTypeClass == SurveyAnswerTypeClass.SingleSelect
                         && r.SurveyChoices.Count <= 10)
                .OrderBy(r => r.DisplayOrder))
            {
                var group = new SelectListGroup { Name = $"Q{question.DisplayOrder} {question.QuestionText}" };

                foreach (var choice in question.SurveyChoices.OrderBy(r => r.DisplayOrder))
                {
                    itemList.Add(new SelectListItem
                    {
                        Value = choice.SurveyChoiceID.ToString(),
                        Text = choice.ChoiceText,
                        Group = group,
                    });
                }
            }

            FilterChoiceIDList = new SelectList(itemList, nameof(SelectListItem.Value), nameof(SelectListItem.Text), null, nameof(SelectListItem.Group) + "." + nameof(SelectListGroup.Name));
        }

        /// <summary>
        /// 回答者の属性（チーム名・カテゴリ・試合登録数）を作成する
        /// </summary>
        /// <param name="answerList"></param>
        /// <returns></returns>
        private async Task<Dictionary<int, string>> CreateAnswererNoteDictionaryAsync(List<SurveyAnswer> answerList)
        {
            var teamIDList = answerList.Select(r => r.TeamID).Distinct().ToList();

            var teamList = await Context.Teams
                .Where(r => teamIDList.Contains(r.TeamID))
                .ToListAsync();

            //チームごとの試合登録数（使い込み度の目安）
            var gameCountList = await Context.Games
                .Where(r => teamIDList.Contains(r.TeamID) && r.DeleteFLG == false)
                .GroupBy(r => r.TeamID)
                .Select(r => new { TeamID = r.Key, Count = r.Count() })
                .ToListAsync();

            var dictionary = new Dictionary<int, string>();

            foreach (var answer in answerList)
            {
                var team = teamList.FirstOrDefault(r => r.TeamID == answer.TeamID);
                var gameCount = gameCountList.FirstOrDefault(r => r.TeamID == answer.TeamID)?.Count ?? 0;

                dictionary[answer.SurveyAnswerID] = team == null
                    ? "チーム未所属"
                    : $"{team.TeamName} / {team.TeamCategoryClassName} / 試合{gameCount}件";
            }

            return dictionary;
        }

        private static string GetAnswererNote(Dictionary<int, string> dictionary, int surveyAnswerID)
        {
            return dictionary.TryGetValue(surveyAnswerID, out var note) ? note : string.Empty;
        }
    }
}
