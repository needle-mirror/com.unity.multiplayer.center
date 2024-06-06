using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window.UI
{
    /// <summary>
    /// Questions that go together in the Questionnaire view.
    /// Example: mandatory questions in "Game Specs" section, optional questions in "Advanced" section.
    /// </summary>
    internal class QuestionSection : VisualElement
    {
        public VisualElement ContentRoot { get; private set; }
        public event Action<AnsweredQuestion> QuestionUpdated;

        public event Action<Preset> OnPresetSelected;

        const string k_AdvancedFoldoutName = "advanced-questions";

        public void CreateAdvancedFoldout(Question[] questions, IEnumerable<AnsweredQuestion> existingAnswers, string headerTitle)
        {
            var foldout = new Foldout
            {
                text = headerTitle,
                name = k_AdvancedFoldoutName
            };
            foreach (var question in questions)
            {
                if (question.IsMandatory)
                    continue;

                foldout.Add(CreateSingleQuestionView(question, existingAnswers));
            }

            ContentRoot.Insert(1, foldout);
        }

        public QuestionSection(Question[] questions, IEnumerable<AnsweredQuestion> existingAnswers, string headerTitle,
            bool mandatoryQuestions)
        {
            var title = new VisualElement();
            title.Add(new Label() {text = headerTitle});
            title.AddToClassList("view-headline");
            Add(title);
            
            var withScrollView = !mandatoryQuestions;
            if (!withScrollView)
                AddToClassList(StyleClasses.QuestionSectionNoScrollbar);
            ContentRoot = CreateContentRoot(withScrollView);

            Add(ContentRoot);

            for (var index = 0; index < questions.Length; index++)
            {
                if ((!mandatoryQuestions && questions[index].IsMandatory) || (mandatoryQuestions && !questions[index].IsMandatory))
                    continue;

                ContentRoot.Add(CreateSingleQuestionView(questions[index], existingAnswers));
            }
        }

        public void AddPresetView()
        {
            ContentRoot.Insert(0, CreatePresetView());
        }

        static VisualElement CreateContentRoot(bool withScrollView)
        {
            if (!withScrollView)
                return new VisualElement();

            var root = new ScrollView();
            root.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            return root;
        }

        VisualElement CreateSingleQuestionView(Question question, IEnumerable<AnsweredQuestion> existingAnswers)
        {
            var existingAnswer = existingAnswers.FirstOrDefault(x => x.QuestionId == question.Id) ?? new AnsweredQuestion() {QuestionId = question.Id};
            var questionView = GetQuestionHeader(question, out var questionContainer);
            questionContainer.Add(QuestionViewFactory.CreateQuestionViewInput(question, existingAnswer, RaiseQuestionUpdated));
            return questionView;
        }

        static VisualElement GetQuestionHeader(Question question, out VisualElement inputContainer)
        {
            return QuestionViewFactory.StandardQuestionHeader(question, out inputContainer);
        }

        void RaiseQuestionUpdated(AnsweredQuestion question)
        {
            QuestionUpdated?.Invoke(question);
        }

        VisualElement CreatePresetView()
        {
            const string description = "Select the game genre that is the closest match to your project to generate common game specifications for this genre and recommended solutions. Recommendations are based solely on player count and game specifications.";
            var presetQuestion = new Question()
                {Title = "Genre of your Game", Description = description, IsMandatory = true};

            var questionView = GetQuestionHeader(presetQuestion, out var questionContainer);
            questionContainer.Add(PresetDropdown());
            return questionView;
        }

        EnumField PresetDropdown()
        {
            var presetDropdown = new EnumField(UserChoicesObject.instance.Preset);
            var serializedObject = new SerializedObject(UserChoicesObject.instance);
            var prop = serializedObject.FindProperty(nameof(UserChoicesObject.instance.Preset));
            presetDropdown.BindProperty(prop);
            presetDropdown.RegisterValueChangedCallback(RaiseValueChangedCallback);
            presetDropdown.name = "preset-dropdown";
            presetDropdown.tooltip = "Select your game type";
            return presetDropdown;
        }

        void RaiseValueChangedCallback(ChangeEvent<Enum> eventData)
        {
            if (!Equals(eventData.newValue, eventData.previousValue))
                OnPresetSelected?.Invoke((Preset) eventData.newValue);
        }
        
        public void SetAdvancedSectionVisible (bool isVisible){
            var advanced = ContentRoot.Q<Foldout>(k_AdvancedFoldoutName);
            advanced.style.display = isVisible ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
    }
}
