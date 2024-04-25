using System;
using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;

namespace Unity.MultiplayerCenterTests.Recommendations
{
    /// <summary>
    /// Basic test for the recommender system, without checking that the recommendation itself make sense.
    /// </summary>
    [TestFixture]
    class RecommenderSystemUnitTests
    {
        const string k_NoNetcodeTitle = "No Netcode";
        [Test]
        public void TestEmptyQuestionnaireAndAnswer_ThrowsArgumentException()
        {
            var questionnaireData = new QuestionnaireData();
            var answerData = new AnswerData();

            // Having an empty questionnaire is an invalid state and should be caught. 
            Assert.Throws<ArgumentException>(() => RecommenderSystem.GetRecommendation(questionnaireData, answerData));
        }

        [Test]
        public void TestEmptyQuestionnaire_ThrowsArgumentException()
        {
            var questionnaireData = new QuestionnaireData();
            var answerData = RecommendationTestsUtils.BuildAnswerMatching(RecommendationTestsUtils.GetProjectQuestionnaire());

            // Having an empty questionnaire is an invalid state and should be caught. 
            Assert.Throws<ArgumentException>(() => RecommenderSystem.GetRecommendation(questionnaireData, answerData));
        }

        [Test]
        public void TestEmptyAnswer_ReturnsNull()
        {
            var questionnaireData = RecommendationTestsUtils.GetProjectQuestionnaire();
            var answerData = new AnswerData();

            // Having empty answers is a normal state, where we don't have any recommendation to make.
            var recommendation = RecommenderSystem.GetRecommendation(questionnaireData, answerData);
            Assert.IsNull(recommendation);
        }

        [Test]
        public void TestGetRecommendationForMatchingAnswers_NothingNull()
        {
            var questionnaireData = RecommendationTestsUtils.GetProjectQuestionnaire();
            var answerData = RecommendationTestsUtils.BuildAnswerMatching(questionnaireData);
            var recommendation = RecommenderSystem.GetRecommendation(questionnaireData, answerData);
            Assert.NotNull(recommendation);

            foreach (var solution in recommendation.NetcodeOptions)
            {
                if (solution.Title == k_NoNetcodeTitle) continue;

                RecommendationTestsUtils.AssertRecommendedSolutionNotNull(solution);
                RecommendationTestsUtils.AssertAllRecommendedPackageNotNull(solution);
            }

            foreach (var solution in recommendation.ServerArchitectureOptions)
            {
                RecommendationTestsUtils.AssertRecommendedSolutionNotNull(solution, false);
                RecommendationTestsUtils.AssertAllRecommendedPackageNotNull(solution);
            }
        }

        [Test]
        public void TestGetRecommendationForMatchingAnswers_OnlyOneMainArchitecturePerCategory()
        {
            var questionnaireData = RecommendationTestsUtils.GetProjectQuestionnaire();
            var answerData = RecommendationTestsUtils.BuildAnswerMatching(questionnaireData);
            var recommendation = RecommenderSystem.GetRecommendation(questionnaireData, answerData);
            AssertNoNetcodeIsTheLastNetcodeRecommendation(recommendation);
            Assert.AreEqual(1, recommendation.NetcodeOptions.Count(x => x.RecommendationType == RecommendationType.MainArchitectureChoice));
            Assert.AreEqual(1, recommendation.NetcodeOptions.Count(x => x.Selected));
            Assert.AreEqual(1, recommendation.ServerArchitectureOptions.Count(x => x.RecommendationType == RecommendationType.MainArchitectureChoice));
            Assert.AreEqual(1, recommendation.ServerArchitectureOptions.Count(x => x.Selected));
        }

        [Test]
        public void TestGetRecommendationForMatchingAnswers_FeaturesAreNotIncoherent()
        {
            var questionnaireData = RecommendationTestsUtils.GetProjectQuestionnaire();
            var answerData = RecommendationTestsUtils.BuildAnswerMatching(questionnaireData);
            var recommendation = RecommenderSystem.GetRecommendation(questionnaireData, answerData);

            AssertNoNetcodeIsTheLastNetcodeRecommendation(recommendation);
            
            // Note: this check fits current requirements, but this might change
            for (var i = 1; i < recommendation.NetcodeOptions.Length; i++)
                RecommendationTestsUtils.AssertSamePackagesWithDifferentRecommendations(recommendation.NetcodeOptions[0],
                    recommendation.NetcodeOptions[i]);

            // Note: server architecture options are not required to have the same recommendations but they should have the same number of packages
            for (var i = 1; i < recommendation.ServerArchitectureOptions.Length; i++)
                RecommendationTestsUtils.AssertRecommendationsHaveSameNumberOfPackages(
                    recommendation.ServerArchitectureOptions[0], recommendation.ServerArchitectureOptions[i]);
        }

        static void AssertNoNetcodeIsTheLastNetcodeRecommendation(RecommendationViewData recommendation)
        {
            var netcodeSolutionCount = recommendation.NetcodeOptions.Length;
            Assert.AreEqual(4, netcodeSolutionCount);
            var lastSolution = recommendation.NetcodeOptions[netcodeSolutionCount - 1];
            Assert.AreEqual(k_NoNetcodeTitle, lastSolution.Title);
            Assert.False(lastSolution.Selected);
        }
    }
}
