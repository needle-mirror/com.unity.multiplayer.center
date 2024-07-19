using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.Multiplayer.Center.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.MultiplayerCenterTests
{
    internal static class UtilsForGettingStartedTabTests
    {
        public static void OpenGettingStartedTab()
        {
            var window = EditorWindow.GetWindow<MultiplayerCenterWindow>();
            window.CurrentTabTest = 1;
        }

        public static VisualElement GetSection(string sectionName)
        {
            return EditorWindow.GetWindow<MultiplayerCenterWindow>().rootVisualElement.Q(sectionName);
        }

        public static bool CheckErrorLogged(LogType type)
        {
            return type == LogType.Error || type == LogType.Exception;
        }

        public static void DeleteSetupDirectoryIfExists(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            var metaFilePath = $"{directoryPath}.meta";
            if (File.Exists(metaFilePath))
            {
                File.Delete(metaFilePath);
            }
        }

        public static void AssertGameObjectHasNoMissingScripts(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            for (var index = 0; index < components.Length; index++)
            {
                var component = components[index];
                Assert.IsNotNull(component, $"GameObject {gameObject.name} has missing script (component index: {index})");
            }
        }

        /// <summary>
        /// Returns true if a SettingsPage (right side of the SettingsWindow) has content.
        /// This is useful to test if calling SettingsService.OpenProjectSettings("Your setting")
        /// makes the window show content. In case your path does not open settings, the right side
        /// will be empty and false will be returned.
        /// Caution: This will only work for SettingsPages created with UI-Toolkit.
        /// </summary>
        /// <returns> True if settings are shown on the right side of the SettingsWindow, false if no settings window is
        /// shown, or the content on the right is empty.</returns>
        public static bool SettingsWindowRightSideHasContent()
        {
            var windows = Resources.FindObjectsOfTypeAll(typeof(EditorWindow)) as EditorWindow[];
            var projectSettingsWindow = windows.FirstOrDefault(window => window.GetType().FullName == "UnityEditor.ProjectSettingsWindow");
            if (projectSettingsWindow == null)
                return false;

            var settingsPanel = projectSettingsWindow.rootVisualElement.Q<VisualElement>(className: "settings-panel");
            if (settingsPanel == null)
                return false;
            return settingsPanel.Children().First().childCount > 0;
        }
    }
}
