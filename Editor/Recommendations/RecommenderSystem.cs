using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;

namespace Unity.Multiplayer.Center.Recommendations
{
    using AnswerWithQuestion = Tuple<Question, Answer>;

    /// <summary>
    /// Builds recommendation based on Questionnaire data and Answer data
    /// </summary>
    internal static class RecommenderSystem
    {
        /// <summary>
        /// Main entry point for the recommender system: computes the recommendation based on the questionnaire data and
        /// the answers.
        /// If no answer has been given or the questionnaire does not match the answers, this returns null.
        /// </summary>
        /// <param name="questionnaireData"></param>
        /// <param name="answerData"></param>
        /// <returns></returns>
        public static RecommendationViewData GetRecommendation(QuestionnaireData questionnaireData, AnswerData answerData)
        {
            var answers = CollectAnswers(questionnaireData, answerData);
            
            // Note: valid now only because we do not have multiple answers per question
            if (answers.Count < questionnaireData.Questions.Length) return null;

            var scoredSolutions = CalculateScore(answers);

            var data = RecommenderSystemDataObject.instance.RecommenderSystemData;
            
            return CreateRecommendation(data, scoredSolutions);
        }

        static List<AnswerWithQuestion> CollectAnswers(QuestionnaireData questionnaireData, AnswerData answerData)
        {
            if (questionnaireData?.Questions == null || questionnaireData.Questions.Length == 0)
                throw new ArgumentException("Questionnaire data is null or empty", nameof(questionnaireData));

            List<AnswerWithQuestion> givenAnswers = new();

            var answers = answerData.Answers;

            foreach (var answeredQuestion in answers)
            {
                // find question for the answer
                if (!Logic.TryGetQuestionByQuestionId(questionnaireData, answeredQuestion.QuestionId, out var question))
                    continue;

                // find answer object for the given answer id
                foreach (var answerId in answeredQuestion.Answers)
                {
                    if (!Logic.TryGetAnswerByAnswerId(question, answerId, out var choice))
                        continue;
                    givenAnswers.Add(Tuple.Create(question, choice));
                }
            }

            return givenAnswers;
        }

        static Dictionary<PossibleSolution, Scoring> CalculateScore(List<AnswerWithQuestion> answers)
        {
            var possibleSolutions = Enum.GetValues(typeof(PossibleSolution));
            Dictionary<PossibleSolution, Scoring> scores = new(possibleSolutions.Length);

            foreach (var solution in possibleSolutions)
            {
                scores.Add((PossibleSolution) solution, new Scoring());
            }

            foreach (var (question, answer) in answers)
            {
                foreach (var scoreImpact in answer.ScoreImpacts)
                {
                    scores[scoreImpact.Solution].AddScore(scoreImpact.Score * question.GlobalWeight, scoreImpact.Comment);
                }
            }

            return scores;
        }

        static RecommendationViewData CreateRecommendation(RecommenderSystemData data, IReadOnlyDictionary<PossibleSolution, Scoring> scoredSolutions)
        {
            RecommendationViewData recommendation = new();
            var installedPackageDictionary = PackageManagement.InstalledPackageDictionary();

            recommendation.NetcodeOptions = BuildRecommendedSolutions(data, new [] {
                (PossibleSolution.NGO, scoredSolutions[PossibleSolution.NGO]), 
                (PossibleSolution.N4E, scoredSolutions[PossibleSolution.N4E]),
                (PossibleSolution.CustomNetcode, scoredSolutions[PossibleSolution.CustomNetcode]), 
                (PossibleSolution.NoNetcode, scoredSolutions[PossibleSolution.NoNetcode]) },
                installedPackageDictionary);

            recommendation.ServerArchitectureOptions = BuildRecommendedSolutions(data, new [] {
                (PossibleSolution.LS, scoredSolutions[PossibleSolution.LS]),
                (PossibleSolution.DS, scoredSolutions[PossibleSolution.DS]),
                (PossibleSolution.CloudCode, scoredSolutions[PossibleSolution.CloudCode]),},
                installedPackageDictionary);
            
            return recommendation;
        }

        static RecommendedSolutionViewData[] BuildRecommendedSolutions(RecommenderSystemData data, (PossibleSolution, Scoring)[] scoredSolutions, Dictionary<string, string> installedPackageDictionary)
        {
            var maxScore = scoredSolutions.Max(x => x.Item2.TotalScore);
            var recommendedSolution = scoredSolutions.First(x => Math.Abs(x.Item2.TotalScore - maxScore) < 0.0001).Item1;

            var result = new RecommendedSolutionViewData[scoredSolutions.Length];
            
            for (var index = 0; index < scoredSolutions.Length; index++)
            {
                var scoredSolution = scoredSolutions[index];
                var recoType = scoredSolution.Item1 == recommendedSolution ? RecommendationType.MainArchitectureChoice : RecommendationType.SecondArchitectureChoice;
                var reco = new RecommendedSolutionViewData(data, data.SolutionsByType[scoredSolution.Item1], recoType, scoredSolution.Item2, installedPackageDictionary);
                result[index] = reco;
            }

            return result;
        }
    }
}
