using System;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window.UI
{
    /// <summary>
    /// Creates visual elements that represent a question / the overview of the questionnaire
    /// </summary>
    internal static class QuestionViewFactory
    {
        public static VisualElement CreateOverview(QuestionnaireData questionnaire, int currentQuestionIndex)
        {
            if (currentQuestionIndex < 0 || currentQuestionIndex >= questionnaire.Questions.Length)
                throw new ArgumentOutOfRangeException(nameof(currentQuestionIndex));

            var label = new Label($"Question {currentQuestionIndex + 1} of {questionnaire.Questions.Length}");
            label.AddToClassList("questionnaireOverview");
            return label;
        }

        public static VisualElement StandardQuestionHeader(Question question, out VisualElement inputContainer)
        {
            var root = new Foldout();
            root.AddToClassList(StyleClasses.QuestionView);
            if (question.IsMandatory)
            {
                root.AddToClassList(StyleClasses.MandatoryQuestion);
            } 
    
            root.text = question.Title  + (question.IsMandatory ? " (required)" : "");
            var questionText = new Label(question.Description);
            questionText.AddToClassList(StyleClasses.QuestionText);
            root.Add(questionText);
            inputContainer = root;
            return root;
        }
        
        public static VisualElement HorizontalQuestionHeader(Question question, out VisualElement inputContainer)
        {
            var root = new VisualElement();
            root.AddToClassList(StyleClasses.WelcomeScreenQuestionView);
            var textContainer = new VisualElement();
            textContainer.AddToClassList(StyleClasses.WelcomeScreenQuestionViewHeader);
            var title = new Label(question.Title);
            title.AddToClassList(StyleClasses.WelcomeScreenQuestionViewTitle);
            var description = new Label(question.Description);
            textContainer.Add(title);
            textContainer.Add(description);
            description.AddToClassList(StyleClasses.QuestionText);
            root.Add(textContainer);
            inputContainer = new VisualElement();
            inputContainer.AddToClassList(StyleClasses.WelcomeScreenQuestionViewContainer);
            root.Add(inputContainer);
            return root;
        }

        public static VisualElement CreateQuestionViewInput(Question question, AnsweredQuestion answer, Action<AnsweredQuestion> onAnswerChanged)
        {
            return question.ViewType switch
            {
                ViewType.Toggle => CreateToggle(question, answer, onAnswerChanged),
                ViewType.Radio => CreateRadio(question, answer, onAnswerChanged),
                ViewType.Checkboxes => CreateCheckboxes(question, answer, onAnswerChanged),
                ViewType.StepSlider => CreateStepSlider(question, answer, onAnswerChanged),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static VisualElement CreateStepSlider(Question question, AnsweredQuestion answeredQuestion, Action<AnsweredQuestion> onAnswerChanged = null)
        {
            var availableChoices = question.Choices.Length;
            var slider = new SliderInt(0, availableChoices - 1, pageSize: 1);
            if (answeredQuestion.Answers?.Count > 0)
            {
                var sliderValue = Array.FindIndex(question.Choices, choice => choice.Id == answeredQuestion.Answers[0]);
                slider.SetValueWithoutNotify(sliderValue);
            }
            else
            {
                // If there is not answer selected, we select the first answer as default
                slider.SetValueWithoutNotify(0);
                answeredQuestion.Answers = new List<string>(1) {question.Choices[0].Id};
                onAnswerChanged?.Invoke(answeredQuestion);
            }

            slider.AddToClassList("step-slider");
            for (var i = 0; i < question.Choices.Length; i++)
            {
                var questionChoice = question.Choices[i];
                var label = new Label(questionChoice.Title);
                var line = new VisualElement();
                label.AddToClassList("step-label");

                // calculate the offset for each label
                var leftInPercent = (i * 100f / (question.Choices.Length - 1));
                label.style.left = line.style.left = new StyleLength(new Length(leftInPercent, LengthUnit.Percent));

                // Adjust for offset, because the handle is not going all the way from beginning to end of the track
                // push labels left of the center to right and vice versa
                var marginLeftOffset = 0;
                if (i / (question.Choices.Length - 1f) > 0.5f)
                    marginLeftOffset = -2;
                else
                    marginLeftOffset = 2;

                // if there is an non odd number of choices, the middle does not get an offset
                if (question.Choices.Length % 2 != 0)
                {
                    if (i == question.Choices.Length / 2)
                        marginLeftOffset = 0;
                }

                label.style.marginLeft = line.style.marginLeft = marginLeftOffset;

                // Move first and last line inwards to be on the edge of the slider track
                if (i == 0)
                {
                    line.style.marginLeft = 4;

                    // Move the first text on the slider inwards to be on the edge of the slider track
                    label.style.marginLeft = 5;
                }

                if (i == question.Choices.Length - 1)
                    line.style.marginLeft = -4;

                line.AddToClassList("step-label-line");

                // Add the label and line to the track to be under the handle
                var tracker = slider.Q("unity-drag-container");

                tracker.Insert(0, line);
                tracker.Add(label);
            }

            slider.RegisterValueChangedCallback(evt =>
            {
                answeredQuestion.Answers = new List<string>(1) {question.Choices[evt.newValue].Id};
                onAnswerChanged?.Invoke(answeredQuestion);
            });

            return slider;
        }

        public static VisualElement CreateCheckboxes(Question question, AnsweredQuestion answeredQuestion, Action<AnsweredQuestion> onAnswerChanged = null)
        {
            var root = new VisualElement();

            foreach (var possibleAnswer in question.Choices)
            {
                var toggle = new Toggle(possibleAnswer.Title);
                var answerCopy = possibleAnswer;
                toggle.RegisterValueChangedCallback(evt => UpdateAnswersWithCheckBoxes(answeredQuestion, answerCopy.Id, evt.newValue, onAnswerChanged));
                root.Add(toggle);
            }

            return root;
        }

        static void UpdateAnswersWithCheckBoxes(AnsweredQuestion question, string id, bool newValue, Action<AnsweredQuestion> onAnswerChanged)
        {
            if (newValue && !question.Answers.Contains(id))
            {
                question.Answers.Add(id);
                onAnswerChanged?.Invoke(question);
            }
            else if (!newValue && question.Answers.Contains(id))
            {
                question.Answers.Remove(id);
                onAnswerChanged?.Invoke(question);
            }
        }

        public static RadioButtonGroup CreateRadio(Question question, AnsweredQuestion answeredQuestion, Action<AnsweredQuestion> onAnswerChanged = null)
        {
            var group = new RadioButtonGroup();

            foreach (var q in question.Choices)
            {
                var radioButton = new RadioButton(q.Title);

                // Todo: just checking for the first question for now.
                if (answeredQuestion.Answers != null && q.Id == answeredQuestion.Answers[0])
                    radioButton.SetValueWithoutNotify(true);
                group.Add(radioButton);
            }

            // this was not working, so I had to use the foreach loop above

            // var answerIndex = question.Choices.ToList().FindIndex(c => c.Id == answeredQuestion.Answers[0]);
            // group.SetValueWithoutNotify(answerIndex);

            group.RegisterValueChangedCallback(evt =>
            {
                answeredQuestion.Answers = new List<string>(1) {question.Choices[evt.newValue].Id};
                onAnswerChanged?.Invoke(answeredQuestion);
            });

            return group;
        }

        public static VisualElement CreateToggle(Question question, AnsweredQuestion answeredQuestion, Action<AnsweredQuestion> onAnswerChanged)
        {
            return CreateRadio(question, answeredQuestion, onAnswerChanged);
        }
    }
}
