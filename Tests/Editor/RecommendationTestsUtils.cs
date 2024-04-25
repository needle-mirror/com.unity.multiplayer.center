using System;
using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;

namespace Unity.MultiplayerCenterTests.Recommendations
{
    internal static class RecommendationTestsUtils
    {
        public static QuestionnaireData GetProjectQuestionnaire()
        {
            // TODO: maybe accessing from disk is a better idea than using the singleton? (side effects)
            var result = Clone(QuestionnaireObject.instance.Questionnaire);
            // sanity checks
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Questions);
            Assert.Greater(result.Questions.Length, 0);
            Assert.False(result.Questions.Any(x => x.Choices.Length == 0));
            return result;
        }

        public static AnswerData BuildAnswerMatching(QuestionnaireData questionnaireData)
        {
            var answerData = new AnswerData();
            foreach (var question in questionnaireData.Questions)
            {
                var middleChoice = question.Choices[question.Choices.Length / 2];
                var answeredQuestion = new AnsweredQuestion()
                {
                    QuestionId = question.Id,
                    Answers = new(){middleChoice.Id}
                };
                Logic.Update(answerData, answeredQuestion);
            }

            return answerData;
        }

        public static void AssertRecommendedSolutionNotNull(RecommendedSolutionViewData solution, bool checkMainPackage = true)
        {
            Assert.NotNull(solution);
            Assert.False(string.IsNullOrEmpty(solution.Title));
            Assert.NotNull(solution.AssociatedFeatures);
            Assert.That(solution.RecommendationType is RecommendationType.MainArchitectureChoice 
                or RecommendationType.SecondArchitectureChoice or RecommendationType.NotRecommended);
            if (checkMainPackage)
                Assert.NotNull(solution.MainPackage);
        }

        public static void AssertAllRecommendedPackageNotNull(RecommendedSolutionViewData solution)
        {
            var solutionName = solution.Title;
            foreach (var package in solution.AssociatedFeatures)
            {
                Assert.NotNull(package, solutionName);
                Assert.False(string.IsNullOrEmpty(package.Name), solutionName);
                Assert.NotNull(string.IsNullOrEmpty(package.PackageId), $"{solutionName} - {package.Name}");
                Assert.That(package.RecommendationType != RecommendationType.MainArchitectureChoice &&
                    package.RecommendationType != RecommendationType.SecondArchitectureChoice, $"{solutionName} - {package.Name}");
            }
        }
        
        /// <summary>
        /// All solutions should have the same number of associated packages.
        /// Even if they are not recommended or incompatible they are still always shown as to not visually disturb
        /// the view when switching between solutions.
        /// </summary>
        /// <param name="sol1">Solution 1</param>
        /// <param name="sol2">Solution 2</param>
        public static void AssertRecommendationsHaveSameNumberOfPackages(RecommendedSolutionViewData sol1, RecommendedSolutionViewData sol2)
        {
            Assert.AreEqual(sol1.AssociatedFeatures.Length, sol2.AssociatedFeatures.Length);
        }

        /// <summary>
        /// All solutions should mention all packages, however the actual recommendations (RecommendationType) should be different.
        /// This checks that all packages in solution 1 are also in solution 2, but that they don't all have the same recommendation.
        /// Note: this a current requirement, but this might evolve. Adapt if necessary.
        /// 2nd Note: With the introduction of the Multiplayer SDK this requirement changed for the server architecture options, but not for the netcode options.
        /// </summary>
        /// <param name="sol1">Solution 1</param>
        /// <param name="sol2">Solution 2</param>
        public static void AssertSamePackagesWithDifferentRecommendations(RecommendedSolutionViewData sol1, RecommendedSolutionViewData sol2)
        {
            AssertRecommendationsHaveSameNumberOfPackages(sol1, sol2);
            var allTheSame = true;
            foreach (var p in sol1.AssociatedFeatures)
            {
                var matching = sol2.AssociatedFeatures.FirstOrDefault(x => x.PackageId == p.PackageId);
                Assert.IsNotNull(matching);
                if(matching.RecommendationType != p.RecommendationType)
                    allTheSame = false;
            }
            Assert.False(allTheSame, $"The two solutions have exactly the same recommended packages! Solution 1: {sol1.Title}, Solution 2: {sol2.Title}"); 
        }

        public static T Clone<T>(T obj)
        {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));
        }
    }
}
