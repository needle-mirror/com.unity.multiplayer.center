using System;
using System.Linq;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Window.UI;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    class WelcomeView
    {
        public VisualElement Root { get; private set; }

        public event Action NextButtonClicked;

        QuestionSection m_QuestionSection;

        Button m_NextButton;

        public static bool ShouldShowWelcomeScreen()
        {
            return UserChoicesObject.instance.Preset == Preset.None ||
                !Logic.AreMandatoryQuestionsFilled(QuestionnaireObject.instance.Questionnaire, UserChoicesObject.instance.UserAnswers);
        }

        public WelcomeView()
        {
            Root = new VisualElement(){name = "welcome-view"};
            Root.Add(new Label("Welcome to the Multiplayer Center!") {style = {fontSize = 16}});
            Root.Add(new Label("To get started, please fill out the following:") {style = { marginBottom = 10 }});
            m_QuestionSection = new QuestionSection(QuestionnaireObject.instance.Questionnaire.Questions,
                Enumerable.Empty<AnsweredQuestion>(), "Game Specs", true, horizontal:true);
            m_QuestionSection.AddPresetView();
            m_QuestionSection.OnPresetSelected += OnFirstPresetSelected;
            m_QuestionSection.QuestionUpdated += OnFirstPlayerSpecsUpdated;
            Root.Add(m_QuestionSection);
            m_NextButton = new Button(RaiseButtonEvent) {text = "Get recommendations"};
            m_NextButton.AddToClassList(StyleClasses.NextStepButton);
            m_NextButton.SetEnabled(false);
            m_QuestionSection.Add(m_NextButton);
        }

        void OnFirstPlayerSpecsUpdated(AnsweredQuestion obj)
        {
            Logic.Update(UserChoicesObject.instance.UserAnswers, obj);
            UserChoicesObject.instance.Save();

            if (!ShouldShowWelcomeScreen())
                m_NextButton.SetEnabled(true);
        }

        void OnFirstPresetSelected(Preset preset)
        {
            var (resultAnswerData, _) = Logic.ApplyPresetToAnswerData(
                UserChoicesObject.instance.UserAnswers, preset,
                QuestionnaireObject.instance.Questionnaire);

            UserChoicesObject.instance.UserAnswers = resultAnswerData;
            UserChoicesObject.instance.Save();

            m_NextButton.SetEnabled(!ShouldShowWelcomeScreen());
        }

        void RaiseButtonEvent()
        {
            NextButtonClicked?.Invoke();
        }

        ~WelcomeView()
        {
            m_QuestionSection.OnPresetSelected -= OnFirstPresetSelected;
            m_QuestionSection.QuestionUpdated -= OnFirstPlayerSpecsUpdated;
            NextButtonClicked = null;
        }
    }
}
