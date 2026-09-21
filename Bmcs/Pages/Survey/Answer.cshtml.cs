using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bmcs.Constans;
using Bmcs.Data;
using Bmcs.Enum;
using Bmcs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Bmcs.Pages.Survey
{
    /// <summary>
    /// アンケート回答
    /// </summary>
    public class AnswerModel : PageModelBase<AnswerModel>
    {
        public AnswerModel(ILogger<AnswerModel> logger, BmcsContext context) : base(logger, context)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int? SurveyID { get; set; }

        /// <summary>
        /// 回答対象のアンケート
        /// </summary>
        public Models.Survey Survey { get; set; }

        /// <summary>
        /// 設問一覧（選択肢を含む）
        /// </summary>
        public List<SurveyQuestion> SurveyQuestionList { get; set; } = new List<SurveyQuestion>();

        /// <summary>
        /// 入力値
        /// </summary>
        [BindProperty]
        public List<AnswerInput> AnswerInputList { get; set; } = new List<AnswerInput>();

        /// <summary>
        /// 完了フラグ
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// 設問ごとの入力値
        /// </summary>
        public class AnswerInput
        {
            public int SurveyQuestionID { get; set; }

            /// <summary>
            /// 単一選択の選択肢ID
            /// </summary>
            public int? SelectedChoiceID { get; set; }

            /// <summary>
            /// 複数選択の選択肢ID
            /// </summary>
            public List<int> SelectedChoiceIDList { get; set; } = new List<int>();

            /// <summary>
            /// 自由記述、または「その他」選択時の入力内容
            /// </summary>
            public string FreeText { get; set; }
        }

        /// <summary>
        /// 入力エラーでの再表示時に、選択状態を復元するための判定
        /// ※設問IDで引き当てる（AnswerInputListの並び順に依存しないため）
        /// </summary>
        /// <param name="surveyQuestionID"></param>
        /// <param name="surveyChoiceID"></param>
        /// <returns></returns>
        public bool IsSelected(int surveyQuestionID, int surveyChoiceID)
        {
            var input = AnswerInputList.FirstOrDefault(r => r.SurveyQuestionID == surveyQuestionID);

            if (input == null)
            {
                return false;
            }

            return input.SelectedChoiceID == surveyChoiceID
                || (input.SelectedChoiceIDList != null && input.SelectedChoiceIDList.Contains(surveyChoiceID));
        }

        /// <summary>
        /// 入力エラーでの再表示時に、自由記述を復元する
        /// </summary>
        /// <param name="surveyQuestionID"></param>
        /// <returns></returns>
        public string GetFreeText(int surveyQuestionID)
        {
            return AnswerInputList.FirstOrDefault(r => r.SurveyQuestionID == surveyQuestionID)?.FreeText;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            if (!await SetSurveyAsync())
            {
                //回答できるアンケートが無い場合はトップへ
                return RedirectToPage("/Top/Index");
            }

            //入力欄の初期化
            AnswerInputList = SurveyQuestionList
                .Select(r => new AnswerInput { SurveyQuestionID = r.SurveyQuestionID })
                .ToList();

            return Page();
        }

        /// <summary>
        /// 回答登録
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!base.IsLogin())
            {
                return ReLogin();
            }

            if (!await SetSurveyAsync())
            {
                return RedirectToPage("/Top/Index");
            }

            //設問の並びに合わせて入力値を整える（添字の欠落・重複による描画エラーを防ぐ）
            NormalizeAnswerInputList();

            if (!ValidateAnswer())
            {
                return Page();
            }

            var userAccountID = HttpContext.Session.GetString(SessionConstant.UserAccountID);

            var surveyAnswer = new SurveyAnswer
            {
                SurveyID = Survey.SurveyID,
                UserAccountID = userAccountID,
                TeamID = HttpContext.Session.GetString(SessionConstant.TeamID),
                AnswerDatetime = DateTime.Now,
            };

            base.SetEntryInfo(surveyAnswer);

            Context.SurveyAnswers.Add(surveyAnswer);

            foreach (var question in SurveyQuestionList)
            {
                var input = AnswerInputList.FirstOrDefault(r => r.SurveyQuestionID == question.SurveyQuestionID);

                if (input == null)
                {
                    continue;
                }

                foreach (var detail in CreateAnswerDetailList(question, input))
                {
                    detail.SurveyAnswer = surveyAnswer;
                    base.SetEntryInfo(detail);

                    Context.SurveyAnswerDetails.Add(detail);
                }
            }

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                //二重送信で既に回答済みの場合のみ完了扱いとし、それ以外はエラーとする
                var sqlException = ex.InnerException as Microsoft.Data.SqlClient.SqlException;

                if (sqlException == null
                    || (sqlException.Number != 2601 && sqlException.Number != 2627))
                {
                    Logger.LogError(ex, $"アンケート回答の登録に失敗しました。SurveyID:{Survey.SurveyID} UserAccountID:{userAccountID}");

                    ModelState.AddModelError(string.Empty, "登録に失敗しました。お手数ですが、もう一度お試しください。");

                    return Page();
                }

                Logger.LogWarning(ex, $"アンケートに二重で回答されました。SurveyID:{Survey.SurveyID} UserAccountID:{userAccountID}");
            }

            IsComplete = true;

            return Page();
        }

        /// <summary>
        /// 「あとで回答する」
        /// ※このセッション中は再表示しない（次回ログイン時に再度表示される）
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostSkip()
        {
            HttpContext.Session.SetString(SessionConstant.SurveySkip, "1");

            return RedirectToPage("/Top/Index");
        }

        /// <summary>
        /// 回答対象のアンケートと設問を取得する
        /// </summary>
        /// <returns>回答できるアンケートがある場合true</returns>
        private async Task<bool> SetSurveyAsync()
        {
            var userAccountID = HttpContext.Session.GetString(SessionConstant.UserAccountID);

            var surveyList = await Context.Surveys
                .Where(r => r.DeleteFLG == false
                         && r.StatusClass == SurveyStatusClass.Open)
                .OrderBy(r => r.SurveyID)
                .ToListAsync();

            //回答受付中、かつ未回答のもの
            var answeredSurveyIDList = await Context.SurveyAnswers
                .Where(r => r.UserAccountID == userAccountID)
                .Select(r => r.SurveyID)
                .ToListAsync();

            Survey = surveyList
                .Where(r => r.IsOpen() && !answeredSurveyIDList.Contains(r.SurveyID))
                .Where(r => SurveyID == null || r.SurveyID == SurveyID)
                .FirstOrDefault();

            if (Survey == null)
            {
                return false;
            }

            SurveyQuestionList = await Context.SurveyQuestions
                .Include(r => r.SurveyChoices)
                .Where(r => r.SurveyID == Survey.SurveyID)
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();

            foreach (var question in SurveyQuestionList)
            {
                question.SurveyChoices = question.SurveyChoices.OrderBy(r => r.DisplayOrder).ToList();
            }

            return SurveyQuestionList.Any();
        }

        /// <summary>
        /// 入力値を設問の並びに合わせて整える
        /// ※送信された添字が欠けていたり飛んでいたりしても、画面の再表示で例外にならないようにする
        /// </summary>
        private void NormalizeAnswerInputList()
        {
            var inputList = AnswerInputList ?? new List<AnswerInput>();

            AnswerInputList = SurveyQuestionList
                .Select(question => inputList.FirstOrDefault(r => r.SurveyQuestionID == question.SurveyQuestionID)
                                    ?? new AnswerInput { SurveyQuestionID = question.SurveyQuestionID })
                .ToList();

            foreach (var input in AnswerInputList)
            {
                //同じ選択肢を複数回送信されても1件として扱う（集計の水増しを防ぐ）
                input.SelectedChoiceIDList = input.SelectedChoiceIDList?.Distinct().ToList() ?? new List<int>();
            }
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <returns></returns>
        private bool ValidateAnswer()
        {
            foreach (var question in SurveyQuestionList)
            {
                var input = AnswerInputList.FirstOrDefault(r => r.SurveyQuestionID == question.SurveyQuestionID)
                            ?? new AnswerInput();

                var key = $"Q{question.SurveyQuestionID}";

                if (question.AnswerTypeClass == SurveyAnswerTypeClass.SingleSelect)
                {
                    //設問に紐づかない選択肢は未選択として扱う
                    if (input.SelectedChoiceID != null
                        && !question.SurveyChoices.Any(r => r.SurveyChoiceID == input.SelectedChoiceID))
                    {
                        input.SelectedChoiceID = null;
                    }

                    if (question.RequiredFLG && input.SelectedChoiceID == null)
                    {
                        ModelState.AddModelError(key, "選択してください。");
                    }
                }
                else if (question.AnswerTypeClass == SurveyAnswerTypeClass.MultiSelect)
                {
                    //設問に紐づかない選択肢は除外する
                    input.SelectedChoiceIDList = input.SelectedChoiceIDList
                        .Where(r => question.SurveyChoices.Any(c => c.SurveyChoiceID == r))
                        .ToList();

                    if (question.RequiredFLG && !input.SelectedChoiceIDList.Any())
                    {
                        ModelState.AddModelError(key, "1つ以上選択してください。");
                    }

                    if (question.MaxSelectCount != null
                        && input.SelectedChoiceIDList.Count > question.MaxSelectCount)
                    {
                        ModelState.AddModelError(key, $"選択できるのは{question.MaxSelectCount}つまでです。");
                    }
                }
                else
                {
                    if (question.RequiredFLG && string.IsNullOrWhiteSpace(input.FreeText))
                    {
                        ModelState.AddModelError(key, "入力してください。");
                    }
                }

                if (input.FreeText != null && input.FreeText.Length > SystemConstant.SurveyFreeTextMaxLength)
                {
                    ModelState.AddModelError(key, $"入力できるのは{SystemConstant.SurveyFreeTextMaxLength}文字までです。");
                }
            }

            return ModelState.IsValid;
        }

        /// <summary>
        /// 設問の入力値から回答明細を作成する
        /// </summary>
        /// <param name="question"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private IEnumerable<SurveyAnswerDetail> CreateAnswerDetailList(SurveyQuestion question, AnswerInput input)
        {
            var choiceIDList = new List<int>();

            if (question.AnswerTypeClass == SurveyAnswerTypeClass.SingleSelect)
            {
                if (input.SelectedChoiceID != null)
                {
                    choiceIDList.Add((int)input.SelectedChoiceID);
                }
            }
            else if (question.AnswerTypeClass == SurveyAnswerTypeClass.MultiSelect)
            {
                choiceIDList.AddRange(input.SelectedChoiceIDList);
            }
            else
            {
                //自由記述
                if (!string.IsNullOrWhiteSpace(input.FreeText))
                {
                    yield return new SurveyAnswerDetail
                    {
                        SurveyQuestionID = question.SurveyQuestionID,
                        AnswerText = input.FreeText.Trim(),
                    };
                }

                yield break;
            }

            foreach (var choiceID in choiceIDList)
            {
                //改ざん対策として、設問に紐づく選択肢のみ登録する
                var choice = question.SurveyChoices.FirstOrDefault(r => r.SurveyChoiceID == choiceID);

                if (choice == null)
                {
                    continue;
                }

                yield return new SurveyAnswerDetail
                {
                    SurveyQuestionID = question.SurveyQuestionID,
                    SurveyChoiceID = choice.SurveyChoiceID,
                    //「その他」選択時のみ自由入力を保存する
                    AnswerText = choice.FreeTextFLG && !string.IsNullOrWhiteSpace(input.FreeText)
                                 ? input.FreeText.Trim()
                                 : null,
                };
            }
        }
    }
}
