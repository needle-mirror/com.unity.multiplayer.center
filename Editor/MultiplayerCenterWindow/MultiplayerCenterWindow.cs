using System;
using Unity.Multiplayer.Center.UI.RecommendationView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    internal class MultiplayerCenterWindow : EditorWindow
    {
        const string k_PathInPackage = "Packages/com.unity.multiplayer.center/Editor/MultiplayerCenterWindow";

        Vector2 m_WindowSize = new(500, 400);

        public int CurrentTab => m_TabGroup.CurrentTab;

        // Testing purposes only. We don't want to set CurrentTab from window
        internal int CurrentTabTest
        {
            get => m_TabGroup.CurrentTab;
            set => m_TabGroup.SetSelected(value);
        }

        [SerializeField]
        bool m_RequestGettingStartedTabAfterDomainReload = false;

        [SerializeField]
        TabGroup m_TabGroup;

        [MenuItem("Window/Multiplayer Center")]
        public static void OpenWindow()
        {
            bool showUtility = false; // TODO: figure out if it would be a good idea to have a utility window (always on top, cannot be tabbed)
            GetWindow<MultiplayerCenterWindow>(showUtility, "Multiplayer Center", true);
        }

        void OnEnable()
        {
            // Adjust window size based on dpi scaling
            var dpiScale = EditorGUIUtility.pixelsPerPoint;
            minSize = new Vector2(m_WindowSize.x * dpiScale, m_WindowSize.y * dpiScale);
        }

        /// <summary>
        /// Changes Tab from Recommendation to the Getting Started 
        /// </summary>
        public void RequestShowGettingStartedTabAfterDomainReload()
        {
            m_RequestGettingStartedTabAfterDomainReload = true;

            // If no domain reload is necessary, this will be called. 
            // If domain reload is necessary, the delay call will be forgotten, but CreateGUI will be called like after any domain reload
            EditorApplication.delayCall += CreateGUI;
        }

        void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.name = "root";
            var theme = EditorGUIUtility.isProSkin ? "dark" : "light";
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"{k_PathInPackage}/UI/{theme}.uss"));
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"{k_PathInPackage}/UI/MultiplayerCenterWindow.uss"));

            if (m_TabGroup == null || m_TabGroup.ViewCount < 1)
                m_TabGroup = new TabGroup(new ITabView[] {new RecommendationTabView(), new GettingStartedTabView()});

            m_TabGroup.CreateTabs();
            rootVisualElement.Add(m_TabGroup.Root);
            rootVisualElement.Add(RecommendationInfoPopupSingleton.recommendationInfoPopup);

            var shouldEnable = m_RequestGettingStartedTabAfterDomainReload;

            if (shouldEnable)
            {
                m_RequestGettingStartedTabAfterDomainReload = false;
                m_TabGroup.SetSelected(1, force: true);
            }
            else
            {
                m_TabGroup.SetSelected(m_TabGroup.CurrentTab, force: true);
            }
            
            SetRootElementEnabled(shouldEnable);
        }

        void SetRootElementEnabled(bool shouldEnable)
        {
            var isInstallationFinished = PackageManagement.IsInstallationFinished();
            rootVisualElement.SetEnabled(isInstallationFinished || shouldEnable);
        }

        void OnDestroy()
        {
            m_TabGroup?.Clear();
        }
    }
}
