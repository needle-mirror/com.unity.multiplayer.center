using System;
using System.Reflection;
using NUnit.Framework;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using UnityEditor.PackageManager;
using Object = UnityEngine.Object;
namespace Unity.MultiplayerCenterTests
{
    [TestFixture]
    class HandlePreReleaseTests
    {
        const string k_NgoPackageId = "com.unity.netcode.gameobjects";

        DistributedAuthorityPreReleaseHandling m_DaHandling = new();
        
        [SetUp]
        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(RecommenderSystemDataObject.instance); // force reload from disk if accessed
        }

        [Test]
        public void TestDistributedAuthoritySupported()
        {
            Assert.False(m_DaHandling.IsDistributedAuthoritySupportedFor("1.9.1", No_NGO2()), "No NGO2 should not support Distributed Authority");
            Assert.True(m_DaHandling.IsDistributedAuthoritySupportedFor("1.9.1", NGO2_Experimental()), "NGO2 Experimental should support Distributed Authority");
            Assert.True(m_DaHandling.IsDistributedAuthoritySupportedFor("1.9.1", NGO2_PreRelease()), "NGO2 PreRelease should support Distributed Authority");
            Assert.True(m_DaHandling.IsDistributedAuthoritySupportedFor("1.9.1", NGO2_Released()), "NGO2 Released should support Distributed Authority");
        }

        [Test]
        public void TestGetPreReleaseVersion()
        {
            Assert.AreEqual(null, m_DaHandling.GetPreReleaseVersion("1.9.1", No_NGO2()));
            Assert.AreEqual("2.0.2-exp.5", m_DaHandling.GetPreReleaseVersion("1.9.1", NGO2_Experimental()));
            Assert.AreEqual("2.0.2-pre.2", m_DaHandling.GetPreReleaseVersion("1.9.1", NGO2_PreRelease()));
            
            // Will install 2.0.0, no need for a pre-release
            Assert.AreEqual(null, m_DaHandling.GetPreReleaseVersion("2.0.0", NGO2_Released()));
        }

        [Test]
        public void TestPatchRecommenderSystemData_WhenDistributedAuthorityIsNotSupported_DABecomesIncompatibleWithNGO()
        {
            var before = RecommenderSystemDataObject.instance.RecommenderSystemData.IsHostingModelCompatibleWithNetcode(PossibleSolution.NGO, PossibleSolution.DA, out _);
            Assert.True(before, "Wrong test setup: NGO2 should support DA");
            
            m_DaHandling = new ("1.9.1", No_NGO2());
            Assert.True(m_DaHandling.IsReady);
            Assert.False(m_DaHandling.IsDistributedAuthoritySupported);

            m_DaHandling.PatchRecommenderSystemData();
            var after = RecommenderSystemDataObject.instance.RecommenderSystemData.IsHostingModelCompatibleWithNetcode(PossibleSolution.NGO, PossibleSolution.DA, out _);
            Assert.False(after);
        }

        [Test]
        public void TestPatchPackages_WhenDAIsSelectedAndCompatible_WillInstallPreRelease()
        {
            var recommendation = UtilsForRecommendationTests.ComputeRecommendationForPreset(Preset.Adventure);
            UtilsForRecommendationTests.SimulateSelectionChange(PossibleSolution.DA, recommendation.ServerArchitectureOptions);
            var ngoBefore = GetNgoSolution(recommendation);
            Assert.True(string.IsNullOrEmpty(ngoBefore.MainPackage.PreReleaseVersion));
            
            m_DaHandling = new ("1.9.1", NGO2_PreRelease());
            Assert.True(m_DaHandling.IsReady);
            Assert.True(m_DaHandling.IsDistributedAuthoritySupported);
            
            m_DaHandling.PatchPackages(recommendation);
            var ngoAfter  = GetNgoSolution(recommendation);
            Assert.AreEqual("2.0.2-pre.2", ngoAfter.MainPackage.PreReleaseVersion);
        }
        
        [Test]
        public void TestPatchPackages_WhenDAIsNOTSelectedAndCompatible_WillNOTInstallPreRelease()
        {
            var recommendation = UtilsForRecommendationTests.ComputeRecommendationForPreset(Preset.Adventure);
            UtilsForRecommendationTests.SimulateSelectionChange(PossibleSolution.LS, recommendation.ServerArchitectureOptions);
            var ngoBefore = GetNgoSolution(recommendation);
            Assert.True(string.IsNullOrEmpty(ngoBefore.MainPackage.PreReleaseVersion));
            
            m_DaHandling = new ("1.9.1", NGO2_PreRelease());
            Assert.True(m_DaHandling.IsReady);
            Assert.True(m_DaHandling.IsDistributedAuthoritySupported);
            
            m_DaHandling.PatchPackages(recommendation);
            var ngoAfter = GetNgoSolution(recommendation);
            
            // Should not install pre-release: DA is not selected.
            Assert.True(string.IsNullOrEmpty(ngoAfter.MainPackage.PreReleaseVersion));
        }
        
        static RecommendedSolutionViewData GetNgoSolution(RecommendationViewData recommendation)
        {
            var ngo = Array.Find(recommendation.NetcodeOptions, p => p.Solution == PossibleSolution.NGO);
            Assert.NotNull(ngo, "Cannot find NGO solution in recommendation");
            return ngo;
        }

        static VersionsInfo No_NGO2() =>
            CreateInstance(new object[]
            {
                new [] { "1.8.1", "1.9.1"},
                new [] { "1.8.1", "1.9.1"},
                "1.8.1",
                Array.Empty<string>()
            });

        static VersionsInfo NGO2_Experimental() =>
            CreateInstance(new object[]
            {
                new [] { "1.8.1", "1.9.1", "2.0.2-exp.5"},
                new [] { "1.8.1", "1.9.1", "2.0.2-exp.5"},
                "1.8.1",
                Array.Empty<string>()
            });

        static VersionsInfo NGO2_PreRelease() =>
            CreateInstance(new object[]
            {
                new [] { "1.8.1", "1.9.1", "2.0.2-exp.5", "2.0.2-pre.2",},
                new [] { "1.8.1", "1.9.1", "2.0.2-exp.5", "2.0.2-pre.2",},
                "1.8.1",
                Array.Empty<string>()
            });

        static VersionsInfo NGO2_Released() =>
            CreateInstance(new object[]
            {
                new [] { "1.8.1", "1.9.1", "2.0.2-exp.5", "2.0.2-pre.2", "2.0.0" },
                new [] { "1.8.1", "1.9.1", "2.0.2-exp.5", "2.0.2-pre.2", "2.0.0" },
                "2.0.0",
                Array.Empty<string>()
            });

        static VersionsInfo CreateInstance(object[] args)
        {
            var type = typeof (VersionsInfo);
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (VersionsInfo) instance;
        }       
    }
}
