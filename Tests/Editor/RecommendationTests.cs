using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;

namespace Unity.MultiplayerCenterTests
{
    /// <summary>
    /// This is an integration test based on known data. It ensures that the recommendation matches the expected results.
    /// </summary>
    [TestFixture]
    class RecommendationTests
    {
        /// <summary>
        /// Ensures that the packages recommended for a given preset match the expected packages (4 players)
        /// Note that this does not handle hidden dependencies
        /// </summary>
        [TestCase(Preset.None)]
        [TestCase(Preset.Adventure,
            "com.unity.netcode.gameobjects",
            "com.unity.multiplayer.playmode",
            "com.unity.multiplayer.widgets",
            "com.unity.multiplayer.tools",
            "com.unity.dedicated-server",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.Shooter,
            "com.unity.netcode",
            "com.unity.entities.graphics",
            "com.unity.multiplayer.playmode",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.Racing,
            "com.unity.netcode",
            "com.unity.entities.graphics",
            "com.unity.multiplayer.playmode", 
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.TurnBased,
            "com.unity.services.cloudcode",
            "com.unity.services.deployment",
            "com.unity.multiplayer.playmode",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.Simulation,
            "com.unity.netcode.gameobjects",
            "com.unity.multiplayer.playmode",
            "com.unity.multiplayer.widgets",
            "com.unity.multiplayer.tools",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.Strategy,
            "com.unity.multiplayer.playmode",
            "com.unity.services.multiplayer",
            "com.unity.transport",
            "com.unity.services.vivox")]
        [TestCase(Preset.Sports,
            "com.unity.netcode",
            "com.unity.entities.graphics",
            "com.unity.multiplayer.playmode",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.RolePlaying,
            "com.unity.multiplayer.playmode",
            "com.unity.services.multiplayer",
            "com.unity.transport",
            "com.unity.services.vivox")]
        [TestCase(Preset.Async,
            "com.unity.services.cloudcode",
            "com.unity.services.deployment",
            "com.unity.services.vivox",
            "com.unity.services.multiplayer",
            "com.unity.multiplayer.playmode")]
        [TestCase(Preset.Fighting,
            "com.unity.multiplayer.playmode",
            "com.unity.services.multiplayer",
            "com.unity.transport",
            "com.unity.services.vivox")]
        [TestCase(Preset.Sandbox,
            "com.unity.netcode.gameobjects",
            "com.unity.multiplayer.playmode",
            "com.unity.multiplayer.widgets",
            "com.unity.multiplayer.tools",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        public void TestPreset_RecommendedPackagesMatchesExpected(Preset preset, params string[] expected)
        {
            var recommendation = ComputeRecommendationForPreset(preset);
            var allPackages = RecommenderSystem.GetSolutionsToRecommendedPackageViewData();
            if (expected == null || expected.Length == 0)
            {
                Assert.IsNull(recommendation);
                return;
            }

            var actualRecommendedPackages = RecommendationUtils.PackagesToInstall(recommendation, allPackages)
                .Select(e => e.PackageId).ToArray();

            // Use AreEqual instead of AreEquivalent to get a better error message
            Array.Sort(expected);
            Array.Sort(actualRecommendedPackages);
            CollectionAssert.AreEqual(expected, actualRecommendedPackages);
        }
        
        [TestCase(Preset.Adventure, PossibleSolution.NGO, PossibleSolution.DS)]
        [TestCase(Preset.Sandbox, PossibleSolution.NGO, PossibleSolution.LS)]
        [TestCase(Preset.Async, PossibleSolution.NoNetcode, PossibleSolution.CloudCode)]
        [TestCase(Preset.TurnBased, PossibleSolution.NoNetcode, PossibleSolution.CloudCode)]
        [TestCase(Preset.Fighting, PossibleSolution.CustomNetcode, PossibleSolution.LS)]
        [TestCase(Preset.Racing, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.RolePlaying, PossibleSolution.CustomNetcode, PossibleSolution.DS)]
        [TestCase(Preset.Shooter, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.Simulation, PossibleSolution.NGO, PossibleSolution.LS)]
        [TestCase(Preset.Strategy, PossibleSolution.CustomNetcode, PossibleSolution.LS)]
        [TestCase(Preset.Sports, PossibleSolution.N4E, PossibleSolution.DS)]
        public void TestPreset_4Players_RecommendedSolutionsAreValid(Preset preset, PossibleSolution expectedNetcode, PossibleSolution expectedHosting)
        {
            var recommendation = ComputeRecommendationForPreset(preset, playerCount: "4");
            Assert.NotNull(recommendation); 
            AssertTheRightSolutionsAreRecommended(expectedNetcode, expectedHosting, recommendation);
            AssertAllDynamicReasonsAreProperlyFormed(recommendation);
        }
        
        [TestCase(Preset.Adventure, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.Sandbox, PossibleSolution.N4E, PossibleSolution.DA)]
        [TestCase(Preset.Async, PossibleSolution.NoNetcode, PossibleSolution.CloudCode)]
        [TestCase(Preset.TurnBased, PossibleSolution.NoNetcode, PossibleSolution.CloudCode)]
        [TestCase(Preset.Fighting, PossibleSolution.CustomNetcode, PossibleSolution.LS)]
        [TestCase(Preset.Racing, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.RolePlaying, PossibleSolution.CustomNetcode, PossibleSolution.DS)]
        [TestCase(Preset.Shooter, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.Simulation, PossibleSolution.N4E, PossibleSolution.LS)]
        [TestCase(Preset.Strategy, PossibleSolution.CustomNetcode, PossibleSolution.LS)]
        [TestCase(Preset.Sports, PossibleSolution.N4E, PossibleSolution.DS)]
        public void TestPreset_16Players_RecommendedSolutionsAreValidAndNoMoreNGO(Preset preset, PossibleSolution expectedNetcode, PossibleSolution expectedHosting)
        {
            var recommendation = ComputeRecommendationForPreset(preset, playerCount: "16");
            Assert.NotNull(recommendation); 
            AssertTheRightSolutionsAreRecommended(expectedNetcode, expectedHosting, recommendation);
            AssertAllDynamicReasonsAreProperlyFormed(recommendation);
        }

        [TestCase(Preset.Adventure, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.Sandbox, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.Async, PossibleSolution.NoNetcode, PossibleSolution.CloudCode)]
        [TestCase(Preset.TurnBased, PossibleSolution.NoNetcode, PossibleSolution.CloudCode)]
        [TestCase(Preset.Fighting, PossibleSolution.CustomNetcode, PossibleSolution.LS)]
        [TestCase(Preset.Racing, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.RolePlaying, PossibleSolution.CustomNetcode, PossibleSolution.DS)]
        [TestCase(Preset.Shooter, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.Simulation, PossibleSolution.N4E, PossibleSolution.DS)]
        [TestCase(Preset.Strategy, PossibleSolution.CustomNetcode, PossibleSolution.LS)]
        [TestCase(Preset.Sports, PossibleSolution.N4E, PossibleSolution.DS)]
        public void TestPreset_128Players_RecommendedSolutionsAreValidAndMostlyDS(Preset preset, PossibleSolution expectedNetcode, PossibleSolution expectedHosting)
        {
            var recommendation = ComputeRecommendationForPreset(preset, playerCount: "128");
            Assert.NotNull(recommendation); 
            AssertTheRightSolutionsAreRecommended(expectedNetcode, expectedHosting, recommendation);
            AssertAllDynamicReasonsAreProperlyFormed(recommendation);
        }

        // First line
        [TestCase(PossibleSolution.LS, "NoCost", "Slow", "2", "4", "8" )]
        [TestCase(PossibleSolution.LS, "NoCost", "Fast", "2", "4", "8" )]
        [TestCase(PossibleSolution.LS, "BestMargin", "Slow", "2", "4", "8" )]
        [TestCase(PossibleSolution.DA, "BestMargin", "Fast", "2", "4", "8" )]
        [TestCase(PossibleSolution.DA, "BestExperience", "Slow", "2", "4", "8")]
        [TestCase(PossibleSolution.DS, "BestExperience", "Fast", "2", "4", "8")]
        
        // Second line
        [TestCase(PossibleSolution.LS, "NoCost", "Slow", "16", "64+" )]
        [TestCase(PossibleSolution.DA, "NoCost", "Fast", "16", "64+" )]
        [TestCase(PossibleSolution.DA, "BestMargin", "Slow", "16", "64+" )]
        [TestCase(PossibleSolution.DS, "BestMargin", "Fast", "16", "64+" )]
        [TestCase(PossibleSolution.DS, "BestExperience", "Slow", "16", "64+")]
        [TestCase(PossibleSolution.DS, "BestExperience", "Fast", "16", "64+")]
        
        // Third line
        [TestCase(PossibleSolution.DS, "NoCost", "Slow", "128", "256", "512" )]
        [TestCase(PossibleSolution.DS, "NoCost", "Fast", "128", "256", "512" )]
        [TestCase(PossibleSolution.DS, "BestMargin", "Slow", "128", "256", "512" )]
        [TestCase(PossibleSolution.DS, "BestMargin", "Fast", "128", "256", "512" )]
        [TestCase(PossibleSolution.DS, "BestExperience", "Slow", "128", "256", "512")]
        [TestCase(PossibleSolution.DS, "BestExperience", "Fast", "128", "256", "512")]
        public void TestGameSpecsForClientServerWithoutPreset_HostingModelMatchesMiroTable(PossibleSolution expectedHosting, string costSensitivity,
            string pace, params string[] playerCounts)
        {
            foreach (var playerCount in playerCounts)
            {
                ExpectHostingModelForClientServerAndCheatingNotImportant(costSensitivity, pace, playerCount, expectedHosting);
            }
        }

        static void ExpectHostingModelForClientServerAndCheatingNotImportant(string costSensitivity, string pace, 
            string playerCount, PossibleSolution expectedHosting)
        {
            var questionnaireData = RecommendationTestsUtils.GetProjectQuestionnaire();
            var answerData = new AnswerData(){ Answers = new List<AnsweredQuestion>
            {
                new () { QuestionId = "CostSensitivity", Answers = new() {costSensitivity}},
                new () { QuestionId = "Pace", Answers = new() {pace}},
                new () { QuestionId = "PlayerCount", Answers = new() {playerCount}},
                new () { QuestionId = "NetcodeArchitecture", Answers = new() {"ClientServer"}},
                new () { QuestionId = "Cheating", Answers = new() {"CheatingNotImportant"}}
            }};
            var recommendation = RecommenderSystem.GetRecommendation(questionnaireData, answerData);
            Assert.NotNull(recommendation);
            AssertHighestScoreSolution(expectedHosting, recommendation.ServerArchitectureOptions, $"{pace}, {costSensitivity}, {playerCount} players: ");
        }

        [Test]
        public void PackageLists_PackagesHaveNames()
        {
            var allpackages = RecommenderSystemDataObject.instance.RecommenderSystemData.PackageDetailsById;
            foreach (var (id, details) in allpackages)
            {
                Assert.False(string.IsNullOrEmpty(details.Name), $"Package {id} has no name");
            }
        }

        [Test]
        public void PackageLists_DependenciesAreAllValid()
        {
            var allpackages = RecommenderSystemDataObject.instance.RecommenderSystemData.PackageDetailsById;
            foreach (var (id, details) in allpackages)
            {
                Assert.NotNull(details, $"Package {id} has no details in RecommenderSystemData.PackageDetails");
                if(details.AdditionalPackages == null)
                    continue;
                
                foreach (var additionalPackageId in details.AdditionalPackages)
                {
                    var additionalPackage = RecommendationUtils.GetPackageDetailForPackageId(additionalPackageId);
                    Assert.NotNull(additionalPackage, $"Package {id} has an invalid dependency: {additionalPackageId}. It should be added to RecommenderSystemData.PackageDetails");
                }
            }
        }

        [TestCase(new []{"com.unity.netcode.gameobjects", "com.unity.netcode"},
            "com.unity.netcode.gameobjects", "com.unity.netcode")]
        
        [TestCase(new []{"com.unity.services.cloudcode", "com.unity.netcode.gameobjects"},
            "com.unity.services.cloudcode",
            "com.unity.netcode.gameobjects")]
        public void TestAdditionalPackagesAreAddedCorrectly(string[] mainPackageIds, params string[] expectedPackages)
        {
            var allPackages = RecommendationUtils.GetPackagesWithAdditionalPackages(mainPackageIds.ToList(), out var _).Select(p => p.Id).ToArray();
            Array.Sort(expectedPackages);
            Array.Sort(allPackages);
            CollectionAssert.AreEqual(expectedPackages, allPackages);
        }

        [TestCase(PossibleSolution.NGO, PossibleSolution.LS, true)]
        [TestCase(PossibleSolution.NGO, PossibleSolution.DA, false)]
        [TestCase(PossibleSolution.NGO, PossibleSolution.DS, true)]
        [TestCase(PossibleSolution.NGO, PossibleSolution.CloudCode, true)] // ?
        
        [TestCase(PossibleSolution.N4E, PossibleSolution.LS, true)]
        [TestCase(PossibleSolution.N4E, PossibleSolution.DA, false)]
        [TestCase(PossibleSolution.N4E, PossibleSolution.DS, true)]
        [TestCase(PossibleSolution.N4E, PossibleSolution.CloudCode, true)] // ?
        
        [TestCase(PossibleSolution.CustomNetcode, PossibleSolution.LS, true)]
        [TestCase(PossibleSolution.CustomNetcode, PossibleSolution.DA, false)]
        [TestCase(PossibleSolution.CustomNetcode, PossibleSolution.DS, true)]
        [TestCase(PossibleSolution.CustomNetcode, PossibleSolution.CloudCode, true)] // ?
        
        [TestCase(PossibleSolution.NoNetcode, PossibleSolution.LS, true)]
        [TestCase(PossibleSolution.NoNetcode, PossibleSolution.DA, false)] // ?
        [TestCase(PossibleSolution.NoNetcode, PossibleSolution.DS, true)]
        [TestCase(PossibleSolution.NoNetcode, PossibleSolution.CloudCode, true)]
        public void TestIncompatibilityWithSolution_MatchesExpected(PossibleSolution netcode, PossibleSolution hostingModel, bool expected)
        {
            var recommendationData = RecommenderSystemDataObject.instance.RecommenderSystemData;
            var actual = recommendationData.IsHostingModelCompatibleWithNetcode(netcode, hostingModel, out string reason);
            Assert.AreEqual(expected, actual, $"Hosting model {hostingModel} should be {(expected ? "compatible" : "incompatible")} with netcode {netcode}");
            Assert.AreEqual(expected, string.IsNullOrEmpty(reason), "reason is " + reason);
        }

        static RecommendationViewData ComputeRecommendationForPreset(Preset preset, string playerCount = "4")
        {
            var questionnaireData = RecommendationTestsUtils.GetProjectQuestionnaire();
            var answerData = GetAnswerDataForPreset(questionnaireData, preset, playerCount);
            var recommendation = RecommenderSystem.GetRecommendation(questionnaireData, answerData);
            return recommendation;
        }

        static AnswerData GetAnswerDataForPreset(QuestionnaireData questionnaireData, Preset preset, string playerCount)
        {
            if (preset == Preset.None)
                return new AnswerData();

            var indexOfPreset = Array.IndexOf(questionnaireData.PresetData.Presets, preset);
            var matchingAnswers = questionnaireData.PresetData.Answers[indexOfPreset].Clone();
            var playerCountAnswer = new AnsweredQuestion()
            {
                QuestionId = "PlayerCount",
                Answers = new() {playerCount}
            };
            Logic.Update(matchingAnswers, playerCountAnswer);
            return matchingAnswers;
        }

        static void AssertTheRightSolutionsAreRecommended(PossibleSolution expectedNetcode, PossibleSolution expectedHosting, RecommendationViewData recommendation)
        {
            AssertRightSolution(expectedNetcode, recommendation.NetcodeOptions);
            if(expectedNetcode != PossibleSolution.NGO && expectedHosting == PossibleSolution.DA)
                AssertHighestScoreSolution(expectedHosting, recommendation.ServerArchitectureOptions);
            else 
                AssertRightSolution(expectedHosting, recommendation.ServerArchitectureOptions);
        }

        static void AssertAllDynamicReasonsAreProperlyFormed(RecommendationViewData recommendation)
        {
            foreach (var hostingModelRecommendation in recommendation.ServerArchitectureOptions)
            {
                AssertHasProperDynamicReason(hostingModelRecommendation);
            }
            
            foreach (var netcodeRecommendation in recommendation.NetcodeOptions)
            {
                AssertHasProperDynamicReason(netcodeRecommendation);
            }
        }

        static void AssertHasProperDynamicReason(RecommendedSolutionViewData solution)
        {
            Assert.False(solution.Reason.Contains(Scoring.DynamicKeyword), $"Reason for {solution.Solution} contain dynamic keyword");
            Assert.True(solution.Reason.Length > 10, $"Reason for {solution.Solution} is too short ({solution.Reason})");
            Assert.False(solution.Reason.EndsWith(".."), $"Reason for {solution.Solution} ends with two dots ({solution.Reason})");
            Assert.True(solution.Reason.EndsWith("."), $"Reason for {solution.Solution} does not end with a dot ({solution.Reason})");
        }
        
        static void AssertRightSolution(PossibleSolution expectedNetcode, RecommendedSolutionViewData[] data, string msg="")
        {
            var selectedView = data.FirstOrDefault(e => e.Selected);
            Assert.NotNull(selectedView, $"{msg}No solution selected");
            Assert.AreEqual(expectedNetcode, selectedView.Solution,
                $"{msg}Expected {expectedNetcode} but got {string.Join(", ", data.Select(e => $"{e.Solution}: {e.Score}"))}");
        }

        static void AssertHighestScoreSolution(PossibleSolution expectedNetcode, RecommendedSolutionViewData[] data, string msg = "")
        {
            var maxScore = data.Max(e => e.Score);
            var solutionsWithMax = data.Where(e => Mathf.Approximately(e.Score, maxScore));
            Assert.AreEqual(1, solutionsWithMax.Count(), $"{msg}Multiple solutions with max score");
            var solutionWithMax = solutionsWithMax.First();
            Assert.True(solutionWithMax.RecommendationType is RecommendationType.MainArchitectureChoice or RecommendationType.Incompatible, $"The solution with the max score does not have the right recommendation type ({solutionWithMax.RecommendationType}");
            Assert.AreEqual(expectedNetcode, solutionWithMax.Solution, $"{msg}Expected {expectedNetcode} but got {string.Join(", ", data.Select(e => $"{e.Solution}: {e.Score}"))}");
        }
    }
}
