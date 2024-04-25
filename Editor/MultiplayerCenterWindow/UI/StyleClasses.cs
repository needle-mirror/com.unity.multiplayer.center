namespace Unity.Multiplayer.Center.Window.UI
{
    /// <summary>
    /// Style common to the multiplayer center, excepted for the getting started tab (which needs shared style)
    /// </summary>
    internal static class StyleClasses
    {
        /// <summary> Game spec standard question </summary>
        public const string QuestionView = "question-view";
        
        /// <summary> Question view for the welcome screen </summary>
        public const string WelcomeScreenQuestionView = "question-view__horizontal";
        
        /// <summary> Container for question view for the welcome screen </summary>
        public const string WelcomeScreenQuestionViewContainer = "question-view__horizontal-container";
        
        /// <summary> Header of question view for the welcome screen </summary>
        public const string WelcomeScreenQuestionViewHeader = "question-view__horizontal-header";
        
        /// <summary> Title of question view for the welcome screen </summary>
        public const string WelcomeScreenQuestionViewTitle = "question-view__horizontal-title";
        
        /// <summary> Game spec mandatory question, highlighted </summary>
        public const string MandatoryQuestion = "mandatory-question";
        
        /// <summary> Description of game spec section </summary>
        public const string QuestionText = "question-text";
        
        /// <summary> Part of game spec questionnaire that contains several questions </summary>
        public const string QuestionSection = "question-section";
        
        /// <summary> Part of game spec questionnaire that contains several questions </summary>
        public const string QuestionSectionNoScrollbar = "question-section__no-scrollbar";
        
        /// <summary> Thicker button to go to a next step; e.g. Install packages </summary>
        public const string NextStepButton = "next-step-button";
    }
}
