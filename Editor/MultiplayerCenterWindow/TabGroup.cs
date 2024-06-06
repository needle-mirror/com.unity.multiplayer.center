using System;
using Unity.Multiplayer.Center.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    // Note: there is a TabView API in UI Toolkit, but only starting from 2023.2
    internal interface ITabView
    {
        /// <summary>
        /// The name as displayed in the tab button
        /// Should be serialized.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The root visual element of the tab view.
        /// The setter will only be used if the root visual element is null when the tab is created.
        /// </summary>
        VisualElement RootVisualElement { get; set; }

        /// <summary>
        /// Refreshes the UI Elements according to latest data.
        /// If the UI is not created yet, it does it.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Unregister all events and clear UI Elements
        /// </summary>
        void Clear();
        
        /// <summary>
        /// The Multiplayer Center Analytics provider.
        /// </summary>
        IMultiplayerCenterAnalytics MultiplayerCenterAnalytics { get; set; }
    }

    [Serializable]
    internal class TabGroup
    {
        const string k_TabZoneName = "tab-zone";

        [field: SerializeField]
        public int CurrentTab { get; private set; } = -1;

        public int ViewCount => m_TabViews?.Length ?? 0;

        VisualElement[] m_TabButtons;

        [SerializeReference]
        ITabView[] m_TabViews;

        public VisualElement Root { get; private set; }

        VisualElement m_MainContainer;

        IMultiplayerCenterAnalytics m_MultiplayerCenterAnalytics;
        
        internal IMultiplayerCenterAnalytics MultiplayerCenterAnalytics 
        {
            get => m_MultiplayerCenterAnalytics;
            set
            {
                m_MultiplayerCenterAnalytics = value;
                foreach (var tabView in m_TabViews)
                {
                    if(tabView != null)
                        tabView.MultiplayerCenterAnalytics = value;
                }
            }
        }

        public TabGroup(IMultiplayerCenterAnalytics analytics, ITabView[] tabViews, int defaultIndex = 0)
        {
            m_TabViews = tabViews;
            CurrentTab = defaultIndex;
            MultiplayerCenterAnalytics = analytics;
        }

        public void SetSelected(int index, bool force = false)
        {
            if (index == CurrentTab && !force)
                return;

            if (CurrentTab >= 0 && CurrentTab < m_TabViews.Length)
            {
                m_TabButtons[CurrentTab].RemoveFromClassList("selected");
                SetVisible(m_TabViews[CurrentTab].RootVisualElement, false);
            }

            EditorPrefs.SetInt(PlayerSettings.productName + "_MultiplayerCenter_TabIndex", index);
            CurrentTab = index;
            m_TabViews[CurrentTab].Refresh();
            m_TabButtons[CurrentTab].AddToClassList("selected");
            SetVisible(m_TabViews[CurrentTab].RootVisualElement, true);
        }

        /// <summary>
        /// Instantiates the visual elements for all the tabs.
        /// Use this to create the tabs for the first time the UI is shown or after a domain reload.
        /// </summary>
        public void CreateTabs()
        {
            Root ??= new VisualElement();
            m_MainContainer ??= new VisualElement();

            if (Root.Q(k_TabZoneName) != null)
                Root.Q(k_TabZoneName).RemoveFromHierarchy();

            var tabZone = new VisualElement() {name = k_TabZoneName};
            Root.Add(tabZone);
            m_TabButtons = new VisualElement[m_TabViews.Length];
            for (int i = 0; i < m_TabViews.Length; i++)
            {
                var index = i; // copy for closure
                var tabButton = new Button(() => SetSelected(index));
                tabButton.AddToClassList("tab-button");
                tabButton.text = m_TabViews[i].Name;
                tabZone.Add(tabButton);
                m_TabButtons[i] = tabButton;
                m_TabViews[i].RootVisualElement ??= new VisualElement();
                m_MainContainer.Add(m_TabViews[i].RootVisualElement);
            }

            m_MainContainer.AddToClassList("tab-container");
            Root.style.height = Length.Percent(100);
            Root.Add(m_MainContainer);
            CurrentTab = EditorPrefs.GetInt(PlayerSettings.productName + "_MultiplayerCenter_TabIndex", 0);
        }

        static void SetVisible(VisualElement e, bool visible)
        {
            e.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Clear()
        {
            foreach (var tabView in m_TabViews)
            {
                tabView?.Clear();
            }
        }
    }
}
