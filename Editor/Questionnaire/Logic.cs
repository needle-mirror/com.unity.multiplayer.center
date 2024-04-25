using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Recommendations;

namespace Unity.Multiplayer.Center.Questionnaire
{
    using AnswerMap = System.Collections.Generic.Dictionary<string, List<string>>;

    /// <summary>
    /// Question and answer logic manipulations.
    /// </summary>
    static internal class Logic
    {
        /// <summary>
        /// Checks if the answers make sense and correspond to the questions.
        /// </summary>
        /// <param name="questions">The reference questionnaire</param>
        /// <param name="currentAnswers">The validated answers.</param>
        /// <returns>A list of problems or an empty list.</returns>
        public static List<string> ValidateAnswers(QuestionnaireData questions, AnswerData currentAnswers)
        {
            if (questions == null || questions.Questions == null || questions.Questions.Length < 1)
                return new List<string> {"No questions found in questionnaire"};

            // TODO: check all question Id exist and are different
            // TODO: check the questions have at least two possible answers

            var errors = new List<string>();
            for (int i = 0; i < currentAnswers.Answers.Count; ++i)
            {
                var current = currentAnswers.Answers[i];
                if (string.IsNullOrEmpty(current.QuestionId))
                    errors.Add($"AnswerData at index {i}: Question id is empty");

                if (!TryGetQuestionByQuestionId(questions, current.QuestionId, out var question))
                    errors.Add($"AnswerData at index {i}: Question id {current.QuestionId} not found in questionnaire");

                if (current.Answers == null || current.Answers.Count < 1) // TODO: in the future we might have questions with nothing selected being a valid answer
                {
                    errors.Add($"AnswerData at index {i}: No answers found (question {current.QuestionId})");
                }
                else if (question != null)
                {
                    switch (question.ViewType)
                    {
                        case ViewType.Toggle when current.Answers.Count > 1:
                        case ViewType.Radio when current.Answers.Count > 1:
                        case ViewType.StepSlider when current.Answers.Count != 1:
                            errors.Add($"AnswerData at index {i}: Too many answers (question {current.QuestionId})");
                            break;
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Finds the first unanswered question index, aka the first question to display.
        /// If all questions have been answered, the index is the length of the questions array.
        /// </summary>
        /// <param name="questions">The questionnaire</param>
        /// <param name="currentAnswers">the answers so far</param>
        /// <returns>The index in the questions array or the size of the questions array</returns>
        public static int FindFirstUnansweredQuestion(QuestionnaireData questions, AnswerData currentAnswers)
        {
            if (currentAnswers.Answers.Count < 1)
                return 0;

            var dico = AnswersToDictionary(questions, currentAnswers); // Assumes validation has already taken place.
            for (int i = 0; i < questions.Questions.Length; i++)
            {
                var current = questions.Questions[i];

                bool isAnswered = dico.ContainsKey(current.Id); // Assumes validation has already taken place.
                if (!isAnswered)
                    return i;
            }

            // all answered
            return questions.Questions.Length;
        }

        /// <summary>
        /// Conversion to a dictionary for easier manipulation.
        /// </summary>
        /// <param name="questions">the questionnaire</param>
        /// <param name="currentAnswers">the answers so far</param>
        /// <returns>The dictionary containing (id, list of answer id) </returns>
        internal static AnswerMap AnswersToDictionary(QuestionnaireData questions, AnswerData currentAnswers)
        {
            var dico = new AnswerMap();
            foreach (var answer in currentAnswers.Answers)
            {
                dico.Add(answer.QuestionId, answer.Answers);
            }

            return dico;
        }

        /// <summary>
        /// Checks if the question id exists in the questionnaire and return it.
        /// </summary>
        /// <param name="questions">The questionnaire</param>
        /// <param name="questionId">The id to find</param>
        /// <param name="foundQuestion">The found question or null</param>
        /// <returns>True if found, else false</returns>
        internal static bool TryGetQuestionByQuestionId(QuestionnaireData questions, string questionId, out Question foundQuestion)
        {
            foreach (var q in questions.Questions)
            {
                if (q.Id == questionId)
                {
                    foundQuestion = q;
                    return true;
                }
            }

            foundQuestion = null;
            return false;
        }

        internal static bool TryGetAnswerByQuestionId(AnswerData answers, string questionId, out AnsweredQuestion foundAnswer)
        {
            foreach (var aq in answers.Answers)
            {
                if (aq.QuestionId == questionId)
                {
                    foundAnswer = aq;
                    return true;
                }
            }

            foundAnswer = null;
            return false;
        }

        internal static bool TryGetAnswerByAnswerId(Question question, string answerId, out Answer answer)
        {
            answer = null;

            foreach (var choice in question.Choices)
            {
                if (answerId == choice.Id)
                {
                    answer = choice;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Applies the preset to the current answers and returns the new answers and the recommendation.
        /// It skips all the mandatory questions, which have to be answered separately.
        /// </summary>
        /// <param name="currentAnswers">The answers the user selected so far</param>
        /// <param name="preset">The preset to apply</param>
        /// <param name="questionnaire">All the available questions</param>
        /// <returns>The new answer data and the associated recommendation(might be null).</returns>
        public static (AnswerData, RecommendationViewData) ApplyPresetToAnswerData(AnswerData currentAnswers, Preset preset,
            QuestionnaireData questionnaire)
        {
            if (preset == Preset.None) return (new AnswerData(), null);

            var presetData = questionnaire.PresetData;
            var index = Array.IndexOf(presetData.Presets, preset);
            var presetAnswers = presetData.Answers[index];
            var resultAnswerData = presetAnswers.Clone();
            foreach (var question in questionnaire.Questions)
            {
                if (!question.IsMandatory) continue;

                if (TryGetAnswerByQuestionId(currentAnswers, question.Id, out var currentAnswer))
                    Update(resultAnswerData, currentAnswer);
            }

            var recommendation = RecommenderSystem.GetRecommendation(questionnaire, resultAnswerData);
            return (resultAnswerData, recommendation);
        }
        
        public static void Update(AnswerData data, AnsweredQuestion a)
        {
            if (a.Answers == null || a.Answers.Count < 1)
            {
                // TODO: this might need to change in the future
                return;
            }
            
            if (data.Answers == null)
            {
                data.Answers = new List<AnsweredQuestion>() {a};
                return;
            }
            
            for (int i = 0; i < data.Answers.Count; i++)
            {
                if (data.Answers[i].QuestionId == a.QuestionId)
                {
                    data.Answers[i] = a;
                    return;
                }
            }
            
            data.Answers.Add(a);
        }
        public static bool AreMandatoryQuestionsFilled(QuestionnaireData questionnaire, AnswerData answers)
        {
            var mandatoryQuestions = questionnaire.Questions.Where(x => x.IsMandatory).Select(q => q.Id).ToArray();
            var foundAnswers = new bool[mandatoryQuestions.Length];
            foreach (var answer in answers.Answers)
            {
                for (int i = 0; i < mandatoryQuestions.Length; i++)
                {
                    if (answer.QuestionId == mandatoryQuestions[i] && answer.Answers.Count > 0)
                    {
                        foundAnswers[i] = true;
                        break;
                    }
                }
            }
            
            return foundAnswers.All(x => x); 
        }

        public static SelectedSolutionsData.HostingModel ConvertInfrastructure(RecommendedSolutionViewData serverArchitectureOption)
        {
            return serverArchitectureOption.Solution switch
            {
                PossibleSolution.LS => SelectedSolutionsData.HostingModel.ClientHosted,
                PossibleSolution.DS => SelectedSolutionsData.HostingModel.DedicatedServer,
                PossibleSolution.CloudCode => SelectedSolutionsData.HostingModel.CloudCode,
                _ => SelectedSolutionsData.HostingModel.None
            };
        }

        public static SelectedSolutionsData.NetcodeSolution ConvertNetcodeSolution(RecommendedSolutionViewData netcodeOption)
        {
            return netcodeOption.Solution switch
            {
                PossibleSolution.NGO => SelectedSolutionsData.NetcodeSolution.NGO,
                PossibleSolution.N4E => SelectedSolutionsData.NetcodeSolution.N4E,
                PossibleSolution.CustomNetcode => SelectedSolutionsData.NetcodeSolution.CustomNetcode,
                PossibleSolution.NoNetcode => SelectedSolutionsData.NetcodeSolution.NoNetcode,
                _ => SelectedSolutionsData.NetcodeSolution.None
            };
        }
    }
}
