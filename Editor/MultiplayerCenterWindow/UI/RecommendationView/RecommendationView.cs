using System;
using System.Linq;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.UI.RecommendationView;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window.UI
{
    internal class RecommendationView
    {
        RecommendationViewData m_Recommendation;

        public RecommendationViewData CurrentRecommendation => m_Recommendation;
        public ScrollView Root { get; } = new();

        NetcodeSection m_NetcodeSection = new();
        ToolingSection m_ToolingSection = new();
        InfrastructureSection m_InfrastructureSection = new();
        VisualElement m_NoRecommendationsView;

        VisualElement m_Content;

        //Todo: for now check on the view but actually should be on the model
        public Action OnPackageSelectionChanged;

        public RecommendationView()
        {
            Root.AddToClassList("recommendation-view");
            Root.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            Root.Add(m_Content = new VisualElement() {name = "recommendation-view-section-container"});
            m_Content.Add(m_NetcodeSection);
            m_NetcodeSection.OnUserChangedNetcode += UpdateView;

            m_Content.Add(m_ToolingSection);
            m_ToolingSection.OnUserChangedTooling += UpdateView;
            m_Content.Add(m_InfrastructureSection);
            m_InfrastructureSection.OnUserChangedInfrastructure += UpdateView;

            m_NoRecommendationsView = EmptyView();

            Root.Add(m_NoRecommendationsView);

            UpdateView();
        }

        public void UpdateRecommendation(RecommendationViewData recommendation)
        {
            m_Recommendation = recommendation;
            UpdateView();
        }

        static void UpdateUserInputObject(RecommendationViewData recommendation)
        {
            var currentSelectedNetcode = UserChoicesObject.instance.SelectedSolutions.SelectedNetcodeSolution;
            foreach (var netcodeOption in recommendation.NetcodeOptions)
            {
                if (netcodeOption.Selected)
                    currentSelectedNetcode = Logic.ConvertNetcodeSolution(netcodeOption);
            }
            
            var selectedHostingModel = UserChoicesObject.instance.SelectedSolutions.SelectedHostingModel;
            foreach (var serverArchitectureOption in recommendation.ServerArchitectureOptions)
            {
                if (serverArchitectureOption.Selected)
                    selectedHostingModel = Logic.ConvertInfrastructure(serverArchitectureOption);
            }
            
            UserChoicesObject.instance.SetUserSelection(selectedHostingModel, currentSelectedNetcode);
            UserChoicesObject.instance.Save();
        }

        public void UpdateView()
        {
            var flex = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            var none = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            var hideRecommendation = m_Recommendation == null;
            m_Content.style.display = hideRecommendation ? none : flex;
            m_NoRecommendationsView.style.display = hideRecommendation ? flex : none;

            // To show the EmptyView in the center, we have to change the behavior of the scroll view container.
            Root.Q<VisualElement>("unity-content-container").style.flexGrow = hideRecommendation ? 1 : 0;

            if (hideRecommendation)
            {
                OnPackageSelectionChanged?.Invoke();
                return;
            }

            var selectedNetcode = m_Recommendation.NetcodeOptions.First(sol => sol.Selected);
           
            m_NetcodeSection.UpdateData(m_Recommendation.NetcodeOptions);
            m_ToolingSection.UpdateData(selectedNetcode);
            m_InfrastructureSection.UpdateData(m_Recommendation.ServerArchitectureOptions, selectedNetcode);
            OnPackageSelectionChanged?.Invoke();
            
            UpdateUserInputObject(m_Recommendation);
        }

        public void Clear()
        {
            m_NetcodeSection.OnUserChangedNetcode -= UpdateView;
            m_ToolingSection.OnUserChangedTooling -= UpdateView;
            m_InfrastructureSection.OnUserChangedInfrastructure -= UpdateView;
            Root.Clear();
        }

        static VisualElement EmptyView()
        {
            var emptyView = new VisualElement();
            emptyView.name = "empty-view";

            var emptyViewContentContainer = new VisualElement();
            emptyViewContentContainer.name = "empty-view-content";
            var emptyViewMessage = new Label
            {
                text = "Answer the questionnaire so that we can recommend the right multiplayer setup for you.",
                name = "empty-view-message"
            };
            var emptyViewIcon = new VisualElement();
            emptyViewIcon.name = "empty-view-icon";
            emptyViewContentContainer.Add(emptyViewIcon);
            emptyViewContentContainer.Add(emptyViewMessage);
            emptyView.Add(emptyViewContentContainer);
            return emptyView;
        }
    }
}
