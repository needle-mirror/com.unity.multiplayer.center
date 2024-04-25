using NUnit.Framework;
using Unity.Multiplayer.Center.Window;
using UnityEditor;
using UnityEngine;

namespace MultiplayerCenterTests
{
    [TestFixture]
    class MultiplayerCenterWindowTests
    {
        [Test]
        public void MultiplayerCenterWindow_ShowWindow()
        {
            var window = EditorWindow.GetWindow<MultiplayerCenterWindow>();
            Assert.NotNull(window);
        }

        [Test]
        public void MultiplayerCenterWindow_ChangeTab_ChangesCurrentTab()
        {
            var window = EditorWindow.GetWindow<MultiplayerCenterWindow>();
            window.CurrentTabTest = 1;
            Assert.AreEqual(1, window.CurrentTab);
            window.CurrentTabTest = 0;
            Assert.AreEqual(0, window.CurrentTab);

            // Switch to getting started tab
            window.CurrentTabTest = 1;
            Assert.AreEqual(1, window.CurrentTab);
        }

        [Test]
        public void MultiplayerCenterWindow_ReopenWindow_PersistentTabSelection()
        {
            var window = EditorWindow.GetWindow<MultiplayerCenterWindow>();
            window.CurrentTabTest = 1;
            Assert.AreEqual(1, window.CurrentTab);

            // Simulate reopening multiplayer center window
            window.Close();
            window = EditorWindow.GetWindow<MultiplayerCenterWindow>();

            Assert.AreEqual(1, window.CurrentTab);
        }

        [TearDown]
        public void TearDown()
        {
            var window = EditorWindow.GetWindow<MultiplayerCenterWindow>();
            if (window != null)
            {
                window.Close();
                Object.DestroyImmediate(window);
            }
        }
    }
}
