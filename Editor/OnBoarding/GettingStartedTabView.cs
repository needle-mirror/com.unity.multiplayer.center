using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Multiplayer.Center.Analytics;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Onboarding;
using Unity.Multiplayer.Center.Questionnaire;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    /// <summary>
    /// This is the main view for the Quickstart tab.
    /// Note that in the code, the Quickstart tab is referred to as the Getting Started tab.
    /// </summary>
    [Serializable]
    internal class GettingStartedTabView : ITabView
    {
        const string k_SectionUssClass = "onboarding-section-category-container";
        const string k_SectionTitleUssClass = "onboarding-section-category-title";
        const string k_NonEmptyMessage = "Here are some helpful resources to get started with your installed packages.";
        const string k_EmptyMessage = "You have not installed any packages yet. To get started, go to the Recommendation tab.";
        
        [field: SerializeField]
        public string Name { get; private set; }

        public VisualElement RootVisualElement { get; set; }

        [SerializeReference]
        List<IOnboardingSection> m_Sections;

        /// <summary>
        /// To find out if new section appeared, we need to keep track of the last section types we found.
        /// </summary>
        [SerializeField]
        AvailableSectionTypes m_LastFoundSectionTypes;
        
        public IMultiplayerCenterAnalytics MultiplayerCenterAnalytics { get; set; }

        public GettingStartedTabView(string name = "Quickstart")
        {
            Name = name;
        }

        public void Refresh()
        {
            Debug.Assert(MultiplayerCenterAnalytics != null, "MultiplayerCenterAnalytics != null");
            UserChoicesObject.instance.OnSolutionSelectionChanged -= NotifyChoicesChanged;
            UserChoicesObject.instance.OnSolutionSelectionChanged += NotifyChoicesChanged;
            
            var currentSectionTypes = SectionsFinder.FindSectionTypes();

            if (m_Sections == null || m_Sections.Count == 0 || m_LastFoundSectionTypes.HaveTypesChanged(currentSectionTypes))
            {
                m_LastFoundSectionTypes = currentSectionTypes;
                ConstructSectionInstances();
                CreateViews();
            }
            else if(RootVisualElement == null || RootVisualElement.childCount == 0)
            {
                CreateViews(); 
            }
        }

        public void Clear()
        {
            RootVisualElement?.Clear();
            if (m_Sections == null)
                return;
            foreach (var section in m_Sections)
            {
                section?.Unload();
            }

            m_Sections.Clear();
        }

        void ConstructSectionInstances()
        {
            var allSections = new List<IOnboardingSection>();
            foreach (var categoryObject in Enum.GetValues(typeof(OnboardingSectionCategory)))
            {
                var category = (OnboardingSectionCategory) categoryObject;
                if (!m_LastFoundSectionTypes.TryGetValue(category, out var sections))
                    continue;

                foreach (var sectionType in sections)
                {
                    var newSection = SectionFromType(sectionType);
                    if (newSection == null) continue;

                    allSections.Add(newSection);
                }
            }

            m_Sections = allSections;
        }

        void CreateViews()
        {
            RootVisualElement ??= new VisualElement();
            RootVisualElement.Clear();
            var scrollView = new ScrollView(ScrollViewMode.Vertical) {horizontalScrollerVisibility = ScrollerVisibility.Hidden};
            scrollView.AddToClassList("onboarding-content");
            RootVisualElement.Add(scrollView);
            scrollView.Add(CreateIntroContent(m_Sections.Count));
            VisualElement currentContainer = null;
            OnboardingSectionCategory? currentCategory = null;
            foreach (var section in m_Sections)
            {
                try
                {
                    var thisSectionCategory = section.Category; // may throw
                    if (thisSectionCategory != currentCategory)
                    {
                        currentContainer = StartNewSection(scrollView, thisSectionCategory);
                        scrollView.Add(currentContainer);
                        currentCategory = thisSectionCategory;
                    }

                    if (section is ISectionWithAnalytics sectionWithAnalytics)
                    {   
                        var attribute = section.GetType().GetCustomAttribute<OnboardingSectionAttribute>();
                        sectionWithAnalytics.AnalyticsProvider = new OnboardingSectionAnalyticsProvider(MultiplayerCenterAnalytics,
                            targetPackageId:attribute.TargetPackageId, sectionId: attribute.Id);  
                    }
                    
                    section.Load();
                    section.Root.name = section.GetType().Name;
                    currentContainer.Add(section.Root);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not load onboarding section {section?.GetType()}: {e}");
                }
            }
            
            NotifyChoicesChanged();
        }

        void NotifyChoicesChanged()
        {
            if (m_Sections == null)
                return;
            
            foreach (var section in m_Sections)
            {
                if (section is not ISectionDependingOnUserChoices dependentSection) continue;

                try
                {
                    SetChoicesData(dependentSection);
                }
                catch (Exception e)
                {
                     Debug.LogWarning($"Could not set data for onboarding section {section?.GetType()}: {e}");
                }
            }
        }

        static void SetChoicesData(ISectionDependingOnUserChoices dependentSection)
        {
            dependentSection.HandleAnswerData(UserChoicesObject.instance.UserAnswers);
            dependentSection.HandlePreset(UserChoicesObject.instance.Preset);
            dependentSection.HandleUserSelectionData(UserChoicesObject.instance.SelectedSolutions);
        }
        
        VisualElement CreateIntroContent(int sectionCount)
        {
            // The combination of both the QuickstartMissingView and the label is weird, so we only show the 
            // QuickstartMissingView if it needs to be shown. 
            // If no Multiplayer package is installed, we do not show the warning, because it is expected that
            // the package is not installed yet.
            if(PackageManagement.IsAnyMultiplayerPackageInstalled() && QuickstartIsMissingView.ShouldShow)
               return new QuickstartIsMissingView().RootVisualElement;
            
            var label = new Label(sectionCount > 0 ? k_NonEmptyMessage : k_EmptyMessage);
            label.style.opacity = 0.8f;
            label.AddToClassList(k_SectionUssClass);
            return label;
        }

        static VisualElement StartNewSection(VisualElement parent, OnboardingSectionCategory category)
        {
            var title = new Label(SectionCategoryToString(category));
            title.AddToClassList(k_SectionTitleUssClass);
            var container = new VisualElement();

            if (category != OnboardingSectionCategory.Intro)
                container.Add(title);
            container.AddToClassList(k_SectionUssClass);
            parent.Add(container);
            return container;
        }

        static IOnboardingSection SectionFromType(System.Type type)
        {
            var constructed = type.GetConstructor(System.Type.EmptyTypes)?.Invoke(null);
            if (constructed is IOnboardingSection section) return section;

            Debug.LogWarning($"Could not create onboarding section {type}");
            return null;
        }

        static string SectionCategoryToString(OnboardingSectionCategory category)
        {
            return category switch
            {
                OnboardingSectionCategory.Intro => "Intro",
                OnboardingSectionCategory.Netcode => "Fundamentals - Netcode and Tools",
                OnboardingSectionCategory.ConnectingPlayers => "Connecting Players",
                OnboardingSectionCategory.ServerInfrastructure => "Server Infrastructure",
                OnboardingSectionCategory.Other => "Other",
                _ => "Unknown"
            };
        }
    }
}
