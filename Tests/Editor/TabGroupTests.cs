using NUnit.Framework;
using UnityEngine;
using Unity.Multiplayer.Center.Window;
using UnityEditor;

namespace MultiplayerCenterTests
{
    [TestFixture]
    class TabGroupTests
    {
        TabGroup m_TabGroup;

        [SetUp]
        public void SetUp()
        {
            m_TabGroup = new TabGroup(new ITabView[] {new RecommendationTabView(), new GettingStartedTabView()});
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
