using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window.UI
{
    internal class QuestionnaireView
    {
        public VisualElement Root { get; private set; }
        readonly QuestionnaireData m_Questions;

        bool AllQuestionsAnswered => m_Questions.Questions.All(x => UserChoicesObject.instance.UserAnswers.Answers.Any(y => y.QuestionId == x.Id));

        public event Action OnQuestionnaireDataChanged;
        public event Action<Preset> OnPresetSelected;

        public QuestionnaireView(QuestionnaireData questions)
        {
            m_Questions = questions;
            Root = new VisualElement();
            Root.name = "questionnaire-view";
            Refresh();
        }

        public void Refresh()
        {
            RefreshData();
            ReCreateVisualElements();
        }

        void ReCreateVisualElements()
        {
            Root.Clear();

            var existingAnswers = UserChoicesObject.instance.UserAnswers.Answers;
            var questions = m_Questions.Questions;
            var gameSpecs = new QuestionSection(questions, existingAnswers, "Game Specs", true);
            gameSpecs.AddPresetView();
            gameSpecs.OnPresetSelected += RaisePresetSelected;
            gameSpecs.QuestionUpdated += QuestionUpdated;
            Root.Add(gameSpecs);

            Root.Add(new VisualElement {name = "questionnaire-spacer"});

            var advanced = new QuestionSection(questions, existingAnswers, "Advanced", false);
            advanced.QuestionUpdated += QuestionUpdated;
            Root.Add(advanced);
        }

        public void Clear()
        {
            OnQuestionnaireDataChanged = null;
            Root.Clear();
        }

        static void RefreshData()
        {
            UserChoicesObject.instance.UserAnswers.Answers ??= new List<AnsweredQuestion>();
        }
        
        void QuestionUpdated(AnsweredQuestion answeredQuestion)
        {
            Logic.Update(UserChoicesObject.instance.UserAnswers, answeredQuestion);
            UserChoicesObject.instance.Save();
            if (AllQuestionsAnswered)
            {
                OnQuestionnaireDataChanged?.Invoke();
            }
        }
        
        void RaisePresetSelected(Preset preset)
        {
            OnPresetSelected?.Invoke(preset);
        }
    }
}
