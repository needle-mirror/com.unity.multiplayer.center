using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using Unity.Multiplayer.Center.Window.UI;

namespace Unity.MultiplayerCenterTests.Recommendations
{
    /// <summary>
    /// This is an integration test that checks the actual recommended packages,
    /// the test does not cover for additional packages that will get installed
    /// as a dependency.
    /// </summary>
    [TestFixture]
    class RecommendationTests
    {
        [TestCase(Preset.None)]
        [TestCase(Preset.Adventure,
            "com.unity.netcode.gameobjects",
            "com.unity.multiplayer.playmode",
            "com.unity.multiplayer.widgets",
            "com.unity.multiplayer.tools",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.Shooter,
            "com.unity.netcode",
            "com.unity.multiplayer.playmode",
            "com.unity.dedicated-server",
            "com.unity.multiplayer.widgets",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.Racing,
            "com.unity.netcode",
            "com.unity.multiplayer.playmode", 
            "com.unity.dedicated-server",
            "com.unity.multiplayer.widgets",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.TurnBased,
            "com.unity.services.cloudcode",
            "com.unity.multiplayer.playmode",
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
            "com.unity.dedicated-server",
            "com.unity.multiplayer.widgets",
            "com.unity.services.multiplayer",
            "com.unity.transport",
            "com.unity.services.vivox")]
        [TestCase(Preset.Sports,
            "com.unity.netcode",
            "com.unity.multiplayer.playmode",
            "com.unity.dedicated-server",
            "com.unity.multiplayer.widgets",
            "com.unity.services.multiplayer",
            "com.unity.services.vivox")]
        [TestCase(Preset.RolePlaying,
            "com.unity.multiplayer.playmode",
            "com.unity.dedicated-server",
            "com.unity.multiplayer.widgets",
            "com.unity.services.multiplayer",
            "com.unity.transport",
            "com.unity.services.vivox")]
        [TestCase(Preset.Async,
            "com.unity.services.cloudcode",
            "com.unity.services.vivox",
        "com.unity.multiplayer.playmode")]
        [TestCase(Preset.Fighting,
            "com.unity.multiplayer.playmode",
            "com.unity.multiplayer.widgets",
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
        public void TestPreset_MatchesExpected(Preset preset, params string[] expected)
        {
            var recommendation = ComputeRecommendationForPreset(preset);
            if (expected == null || expected.Length == 0)
            {
                Assert.IsNull(recommendation);
                return;
            }

            var actualRecommendedPackages = RecommendationUtils.PackagesToInstall(recommendation)
                .Select(e => e.PackageId).ToArray();

            // Use AreEqual instead of AreEquivalent to get a better error message
            Array.Sort(expected);
            Array.Sort(actualRecommendedPackages);
            CollectionAssert.AreEqual(expected, actualRecommendedPackages);
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
                
                foreach (var additionalPackageId in details.AdditionalPackages)
                {
                    var additionalPackage = RecommendationUtils.GetPackageDetailForPackageId(additionalPackageId);
                    Assert.NotNull(additionalPackage, $"Package {id} has an invalid dependency: {additionalPackageId}. It should be added to RecommenderSystemData.PackageDetails");
                }
            }
        }
        
        [TestCase(new string[]{"com.unity.netcode.gameobjects", "com.unity.netcode"},
            "com.unity.netcode.gameobjects", "com.unity.netcode")]
        
        [TestCase(new string[]{"com.unity.services.cloudcode", "com.unity.netcode.gameobjects"},
            "com.unity.services.cloudcode",
            "com.unity.netcode.gameobjects",
            "com.unity.services.deployment")]
        public void TestAdditionalPackagesAreAddedCorrectly(string[] mainPackageIds, params string[] expectedPackages)
        {
            var allPackages = RecommendationUtils.GetPackagesWithAdditionalPackages(mainPackageIds.ToList(), out var _).Select(p => p.Id).ToArray();
            Array.Sort(expectedPackages);
            Array.Sort(allPackages);
            CollectionAssert.AreEqual(expectedPackages, allPackages);
        }
        
        static RecommendationViewData ComputeRecommendationForPreset(Preset preset)
        {
            var questionnaireData = RecommendationTestsUtils.GetProjectQuestionnaire();
            var answerData = GetAnswerDataForPreset(questionnaireData, preset);
            var recommendation = RecommenderSystem.GetRecommendation(questionnaireData, answerData);
            return recommendation;
        }

        static AnswerData GetAnswerDataForPreset(QuestionnaireData questionnaireData, Preset preset)
        {
            if (preset == Preset.None)
                return new AnswerData();

            var indexOfPreset = Array.IndexOf(questionnaireData.PresetData.Presets, preset);
            var matchingAnswers = questionnaireData.PresetData.Answers[indexOfPreset].Clone();
            var playerCountAnswer = new AnsweredQuestion()
            {
                QuestionId = "PlayerCount",
                Answers = new() {"4"} // why 4? because it does automatically select N4E
            };
            Logic.Update(matchingAnswers, playerCountAnswer);
            return matchingAnswers;
        }
    }
}
