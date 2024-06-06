using System;
using Unity.Multiplayer.Center.Analytics;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using Unity.Multiplayer.Center.Window.UI;
using Unity.Multiplayer.Center.Window.UI.RecommendationView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    internal class RecommendationTabView : ITabView
    {
        QuestionnaireView m_QuestionnaireView;
        RecommendationView m_RecommendationView;
        
        RecommendationViewBottomBar m_BottomBarView;
        
        bool m_ShouldRefresh = true;


        [field: SerializeField] // Marked as redundant by Rider, but it is not.
        public string Name { get; private set; }

        public VisualElement RootVisualElement { get; set; }

        public IMultiplayerCenterAnalytics MultiplayerCenterAnalytics { get; set; }
        
        // Access to QuestionnaireView for testing purposes
        internal QuestionnaireView QuestionnaireView => m_QuestionnaireView;

        public RecommendationTabView(string name = "Recommendation")
        {
            Name = name;
        }

        public void Clear()
        {
            m_RecommendationView?.Clear();
            m_QuestionnaireView?.Clear();
            RootVisualElement?.Clear();
        }

        public void Refresh()
        {
            Debug.Assert(MultiplayerCenterAnalytics != null, "MultiplayerCenterAnalytics != null");
            if (!m_ShouldRefresh && RootVisualElement.childCount > 0) return;
            CreateStandardView();
            m_ShouldRefresh = false;
        }

       
        void CreateStandardView()
        {
            RootVisualElement.Clear();
            MigrateUserChoices();
            // We need this because Bottom bar is a part of the Recommendations Tab and it should always stay
            // at the bottom of the view. So we need to make sure that the root tab element is always 100% height.
            RootVisualElement.style.height = Length.Percent(100);

            var horizontalContainer = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            horizontalContainer.name = "spit-view";

            // This is used to make sure the left side does not grow to 100% as this is what would happen by default.
            // It feels not 100% correct. But it seems to be the only way to match the height of the 2 sides with how
            // our views are build currently.
            horizontalContainer.contentContainer.style.position = Position.Relative;

            m_QuestionnaireView = new QuestionnaireView(QuestionnaireObject.instance.Questionnaire);
            m_QuestionnaireView.OnQuestionnaireDataChanged += HandleQuestionnaireDataChanged;
            m_QuestionnaireView.OnPresetSelected += OnPresetSelected;
            horizontalContainer.Add(m_QuestionnaireView.Root);

            m_RecommendationView = new RecommendationView();
            horizontalContainer.Add(m_RecommendationView.Root);
            RootVisualElement.Add(horizontalContainer);

            m_BottomBarView = new RecommendationViewBottomBar(MultiplayerCenterAnalytics);
            m_RecommendationView.OnPackageSelectionChanged += () => m_BottomBarView.UpdatePackagesToInstall(m_RecommendationView.CurrentRecommendation, m_RecommendationView.AllPackages);
            RootVisualElement.Add(m_BottomBarView);
            UpdateRecommendation(keepSelection: true);
        }

        void HandleQuestionnaireDataChanged()
        {
            UpdateRecommendation(keepSelection: false);
        }
        
        void MigrateUserChoices()
        {
            var questionnaire = QuestionnaireObject.instance.Questionnaire;
            var userChoices = UserChoicesObject.instance;
            
            // make sure the version of the questionnaire is the same as the one in the user choices.
            if (questionnaire.Version != userChoices.QuestionnaireVersion && userChoices.UserAnswers.Answers.Count > 0) 
            {
                Logic.MigrateUserChoices(questionnaire, userChoices);
            }
        }
        
        void UpdateRecommendation(bool keepSelection)
        {
            var questionnaire = QuestionnaireObject.instance.Questionnaire;
            var userChoices = UserChoicesObject.instance;

            var errors = Logic.ValidateAnswers(questionnaire, userChoices.UserAnswers);
            foreach (var error in errors)
            {
                Debug.LogError(error);
            }

            var recommendation = RecommenderSystem.GetRecommendation(questionnaire, userChoices.UserAnswers);
            if(keepSelection)
            {
                RecommendationUtils.ApplyPreviousSelection(recommendation, userChoices.SelectedSolutions);
            }
            else if (recommendation != null) // we only send the event if there is a recommendation and it is a new one
            {
                MultiplayerCenterAnalytics.SendRecommendationEvent(userChoices.UserAnswers, userChoices.Preset);
            }
            
            m_RecommendationView.UpdateRecommendation(recommendation);
            m_BottomBarView.UpdatePackagesToInstall(recommendation, m_RecommendationView.AllPackages);
        }

        void OnPresetSelected(Preset preset)
        {
            var (resultAnswerData, recommendation) = Logic.ApplyPresetToAnswerData(
                UserChoicesObject.instance.UserAnswers, preset, QuestionnaireObject.instance.Questionnaire);

            UserChoicesObject.instance.UserAnswers = resultAnswerData;
            UserChoicesObject.instance.Save();

            if(recommendation != null)
                MultiplayerCenterAnalytics.SendRecommendationEvent(resultAnswerData, preset);

            m_QuestionnaireView.Refresh();
            m_RecommendationView.UpdateRecommendation(recommendation);
        }
    }
}
