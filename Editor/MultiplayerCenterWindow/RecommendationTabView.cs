using System;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using Unity.Multiplayer.Center.Window.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    internal class RecommendationTabView : ITabView
    {
        QuestionnaireView m_QuestionnaireView;
        RecommendationView m_RecommendationView;
        
        RecommendationViewBottomBar m_BottomBarView;
        
        WelcomeView m_WelcomeView;
        
        [field: SerializeField] // Marked as redundant by Rider, but it is not.
        public string Name { get; private set; }

        public VisualElement RootVisualElement { get; set; }

        bool m_ShouldRefresh = true;
        
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
            if (!m_ShouldRefresh && RootVisualElement.childCount > 0) return;

            if (WelcomeView.ShouldShowWelcomeScreen())
            {
                CreateWelcomeView();
            }
            else
            {
                m_WelcomeView = null; 
                CreateStandardView();
            }

            m_ShouldRefresh = false;
        }


        void CreateWelcomeView()
        {
            RootVisualElement.Clear();
            m_WelcomeView = new WelcomeView();
            m_WelcomeView.NextButtonClicked += () =>
            {
                m_ShouldRefresh = true;
                Refresh();
            };
            RootVisualElement.Add(m_WelcomeView.Root);
        }

        void CreateStandardView()
        {
            RootVisualElement.Clear();

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

            m_BottomBarView = new RecommendationViewBottomBar();
            m_RecommendationView.OnPackageSelectionChanged += () => m_BottomBarView.UpdatePackagesToInstall(RecommendationUtils.PackagesToInstall(m_RecommendationView.CurrentRecommendation));
            RootVisualElement.Add(m_BottomBarView);
            UpdateRecommendation(keepSelection: true);
        }

        void HandleQuestionnaireDataChanged()
        {
            UpdateRecommendation(keepSelection: false);
        }
        
        void UpdateRecommendation(bool keepSelection)
        {
            var questionnaire = QuestionnaireObject.instance.Questionnaire;
            var answers = UserChoicesObject.instance;
            var errors = Logic.ValidateAnswers(questionnaire, answers.UserAnswers);
            foreach (var error in errors)
            {
                Debug.LogError(error);
            }

            var recommendation = RecommenderSystem.GetRecommendation(questionnaire, answers.UserAnswers);
            if(keepSelection)
            {
                RecommendationUtils.ApplyPreviousSelection(recommendation, answers.SelectedSolutions);
            }
            m_RecommendationView.UpdateRecommendation(recommendation);
            m_BottomBarView.UpdatePackagesToInstall(RecommendationUtils.PackagesToInstall(recommendation));
        }

        void OnPresetSelected(Preset preset)
        {
            var (resultAnswerData, recommendation) = Logic.ApplyPresetToAnswerData(
                UserChoicesObject.instance.UserAnswers, preset,
                QuestionnaireObject.instance.Questionnaire);

            UserChoicesObject.instance.UserAnswers = resultAnswerData;
            UserChoicesObject.instance.Save();

            m_QuestionnaireView.Refresh();
            m_RecommendationView.UpdateRecommendation(recommendation);
        }
    }
}
