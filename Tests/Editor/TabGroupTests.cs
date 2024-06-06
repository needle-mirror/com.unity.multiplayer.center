using NUnit.Framework;
using Unity.Multiplayer.Center.Analytics;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Common.Analytics;
using UnityEngine;
using Unity.Multiplayer.Center.Window;
using UnityEditor;

namespace Unity.MultiplayerCenterTests
{
    [TestFixture]
    class TabGroupTests
    {
        TabGroup m_TabGroup;
        RecommendationTabView m_RecommendationTabView;
        GettingStartedTabView m_GettingStartedTabView;

        private class AnalyticsMock : IMultiplayerCenterAnalytics
        {
            public void SendInstallationEvent(AnswerData data, Preset preset, Package[] packages) {}
            
            public void SendRecommendationEvent(AnswerData data, Preset preset){}

            public void SendGettingStartedInteractionEvent(string targetPackageId, string sectionId, InteractionDataType type, string displayName) {}
        }
        
        [SetUp]
        public void SetUp()
        {
            m_RecommendationTabView = new RecommendationTabView();
            m_GettingStartedTabView = new GettingStartedTabView();
            
            m_TabGroup = new TabGroup(new AnalyticsMock(), new ITabView[] {m_RecommendationTabView, m_GettingStartedTabView});
        }

        [Test]
        public void TabGroup_CreateTabs_2TabViews()
        {
            m_TabGroup.CreateTabs();
            Assert.AreEqual(2, m_TabGroup.ViewCount);
        }

        [Test]
        public void TabGroup_CreateTabs_SelectsTabFromUserPreferences()
        {
            m_TabGroup.CreateTabs();
            var currentTabFromEditorPrefs = EditorPrefs.GetInt(PlayerSettings.productName + "_MultiplayerCenter_TabIndex", 0);
            Assert.AreEqual(currentTabFromEditorPrefs, m_TabGroup.CurrentTab);
        }

        [Test]
        public void TabGroup_SetTabToRecommendationTab_ChangesToSecondTab()
        {
            m_TabGroup.CreateTabs();
            m_TabGroup.SetSelected(1);
            Assert.AreEqual(1, m_TabGroup.CurrentTab);
        }
        
        [Test]
        public void TabGroup_AnalyticsIsPropagatedToAllViews()
        {
            Assert.NotNull(m_RecommendationTabView.MultiplayerCenterAnalytics);
            Assert.NotNull(m_GettingStartedTabView.MultiplayerCenterAnalytics);
            Assert.AreEqual(m_RecommendationTabView.MultiplayerCenterAnalytics, m_GettingStartedTabView.MultiplayerCenterAnalytics);
            Assert.AreEqual(m_RecommendationTabView.MultiplayerCenterAnalytics.GetType(), typeof(AnalyticsMock));
        }
        
        [TearDown]
        public void TearDown()
        {
            if (m_TabGroup != null)
            {
                m_TabGroup.Clear();
            }
        }
    }
}
