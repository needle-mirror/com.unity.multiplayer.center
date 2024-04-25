using System;
using System.Collections.Generic;
using Unity.Multiplayer.Center.UI.RecommendationView;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    /// <summary>
    /// View to show one option to the user
    /// Option can be shown as a radio button or as a checkbox.
    /// </summary>
    internal class RecommendationItemView : VisualElement
    {
        string m_DocsUrl;

        BaseBoolField m_RadioButton;
        Label m_PackageNameLabel = new();
        Label m_ReasonText = new();
        RecommendationBadge m_RecommendedBadge;
        InstalledBadge m_InstalledBadge;
        Image m_HelpIcon;

        /// <summary>
        /// Feature Id stores a unique identifier that identifies the feature.
        /// Usually this is the title of the solution or the package id.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        /// Triggered when the user changes the selection.
        /// </summary>
        public Action<RecommendationItemView, bool> OnUserChangedSelection;

        public RecommendationItemView(bool isRadio = true)
        {
            AddToClassList("recommendation-item");

            var topContainer = new VisualElement();
            topContainer.name = "header";

            m_RadioButton = isRadio ? new RadioButton() : new Toggle();
            m_RadioButton.RegisterValueChangedCallback(evt =>
            {
                if (OnUserChangedSelection != null)
                    OnUserChangedSelection(this, evt.newValue);
            });

            var topContainerLeft = new VisualElement();
            m_RecommendedBadge = new RecommendationBadge();
            m_InstalledBadge = new InstalledBadge();
            topContainerLeft.Add(m_RadioButton);
            topContainerLeft.Add(m_PackageNameLabel);
            topContainerLeft.Add(m_RecommendedBadge);

            topContainerLeft.Add(m_InstalledBadge);
            topContainerLeft.AddToClassList("recommendation-item-left-container");
            topContainer.Add(topContainerLeft);

            var topContainerRight = new VisualElement();
            topContainerRight.AddToClassList("recommendation-item-right-container");

            m_HelpIcon = new Image();
            m_HelpIcon.name = "info-icon";
            m_HelpIcon.AddToClassList("icon");
            m_HelpIcon.AddToClassList("icon-questionmark");
            m_HelpIcon.RegisterCallback<ClickEvent>(OpenInBrowser);
            topContainerRight.Add(m_HelpIcon);

            topContainer.Add(topContainerRight);
            Add(topContainer);

            var bottomContainer = new VisualElement();
            bottomContainer.Add(m_ReasonText);
            bottomContainer.name = "sub-info-text";
            Add(bottomContainer);
        }

        public void UpdateData(string featureName, string featureId, RecommendedItemViewData item)
        {
            FeatureId = featureId;
            SetFeatureName(featureName);
            SetIsSelected(item.Selected);
            SetRecommendationType(item.RecommendationType);
            SetReasonText(item.Reason);
            SetDocUrl(item.DocsUrl);
            SetFeatureShortDescription(item.ShortDescription);
            SetupInstalledBadge(item, featureId);
        }

        void SetIsSelected(bool value)
        {
            m_RadioButton.SetValueWithoutNotify(value);
        }

        void SetupInstalledBadge(RecommendedItemViewData item, string featureId)
        {
            if (!item.IsInstalledAsProjectDependency)
            {
                m_InstalledBadge.style.display = DisplayStyle.None;
                return;
            }

            m_InstalledBadge.style.display = DisplayStyle.Flex;
            m_InstalledBadge.PackageName = featureId;
            m_InstalledBadge.SetTooltip($"Installed version: {item.InstalledVersion}\nClick to open Package Manager");
        }

        void SetFeatureName(string value)
        {
            m_PackageNameLabel.text = value;
        }

        void SetRecommendationType(RecommendationType value)
        {
            m_RecommendedBadge.SetRecommendationType(value);
            m_RadioButton.SetEnabled(true);
            style.opacity = 1f;
            if (value == RecommendationType.Incompatible)
            {
                style.opacity = 0.8f;
                m_RadioButton.SetEnabled(false);
                m_RadioButton.SetValueWithoutNotify(false);
            }
        }

        void SetReasonText(string value)
        {
            m_ReasonText.text = value;
            m_RecommendedBadge.InfoPopupText = value;

            // Deleted the reason text for now
            m_ReasonText.style.display = DisplayStyle.None;
            if (string.IsNullOrEmpty(value))
                m_ReasonText.style.display = DisplayStyle.None;
        }

        void SetFeatureShortDescription(string value)
        {
            m_HelpIcon.tooltip = value;
        }

        void SetDocUrl(string url)
        {
            m_DocsUrl = url;
            m_HelpIcon.SetEnabled(!string.IsNullOrEmpty(url));
        }

        void OpenInBrowser(ClickEvent evt)
        {
            // For a better solution look at PackageLinkButton.cs in PackageManagerUI, there seems to be a version with analytics etc.
            Application.OpenURL(m_DocsUrl);
        }
    }

    internal class InstalledBadge : VisualElement
    {
        Label m_InstalledLabel;

        public string PackageName { get; set; }

        public InstalledBadge()
        {
            m_InstalledLabel = new Label("Installed");
            m_InstalledLabel.AddToClassList("recommended-badge");
            m_InstalledLabel.AddToClassList("color-grey");

            Add(m_InstalledLabel);
            RegisterCallback<ClickEvent>(OnBadgeClick);
        }

        public void SetTooltip(string tooltip)
        {
            m_InstalledLabel.tooltip = tooltip;
        }

        void OnBadgeClick(ClickEvent evt)
        {
            // Open the package manager window using the property
            PackageManagement.OpenPackageManager(PackageName);
        }
    }

    internal class RecommendationBadge : VisualElement
    {
        Label m_RecommendedLabel;
        public string InfoPopupText { get; set; }

        List<string> m_PossibleLabelStyles = new List<string>()
        {
            "color-grey",
            "color-blue",
        };

        public void SetRecommendationType(RecommendationType value)
        {
            style.display = DisplayStyle.Flex;
            m_PossibleLabelStyles.ForEach(s => m_RecommendedLabel.RemoveFromClassList(s));
            switch (value)
            {
                case RecommendationType.MainArchitectureChoice or
                    RecommendationType.OptionalFeatured or
                    RecommendationType.OptionalStandard:
                    m_RecommendedLabel.AddToClassList("color-blue");
                    m_RecommendedLabel.text = "Recommended";
                    break;
                case RecommendationType.NotRecommended:
                    m_RecommendedLabel.AddToClassList("color-grey");
                    m_RecommendedLabel.text = "Not Recommended";
                    break;
                case RecommendationType.Incompatible:
                    m_RecommendedLabel.AddToClassList("color-grey");
                    m_RecommendedLabel.text = "Incompatible";
                    break;
                default:
                    style.display = DisplayStyle.None;
                    break;
            }
        }

        public RecommendationBadge()
        {
            m_RecommendedLabel = new Label("Recommended");
            m_RecommendedLabel.AddToClassList("recommended-badge");
            Add(m_RecommendedLabel);
            RegisterCallback<ClickEvent>(OnBadgeClick);
        }

        void OnBadgeClick(ClickEvent evt)
        {
            // Show or update the InfoPopup
            RecommendationInfoPopupSingleton.ShowInfoPopup(this, InfoPopupText);

            // Stop the event from propagating to the Window. Without this, since the badge is 
            // also on the window, OnWindowClick will be called and the InfoPopup will be hidden.
            evt.StopPropagation();
        }
    }
}
