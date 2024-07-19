using System;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Analytics;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using Unity.Multiplayer.Center.Window.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    class RecommendationViewBottomBar : VisualElement
    {
        readonly Label m_PackageCount;
        readonly Button m_InstallPackageButton;
        
        IMultiplayerCenterAnalytics m_Analytics;

        MultiplayerCenterWindow m_Window = EditorWindow.GetWindow<MultiplayerCenterWindow>();
        List<string> m_PackagesToInstallIds = new ();
        List<string> m_PackagesToInstallNames = new ();
        RecommendationViewData m_RecommendationViewData;
        SolutionsToRecommendedPackageViewData m_SolutionToPackageData;

        public RecommendationViewBottomBar(IMultiplayerCenterAnalytics analytics)
        {
            m_Analytics = analytics;
            name = "bottom-bar";
            m_PackageCount = new Label {name = "package-count"};

            // Setup Install Button
            m_InstallPackageButton = new Button(OnInstallButtonClicked) {text = "Install Packages"};
            m_InstallPackageButton.AddToClassList(StyleClasses.NextStepButton);
            
            // Put the button in a container
            var installPackageContainer = new VisualElement() {name = "install-package-container"};
            installPackageContainer.Add(m_InstallPackageButton);

            Add(m_PackageCount);
            Add(installPackageContainer);
        }

        void OnInstallButtonClicked()
        {
            if (!PackageManagement.IsAnyMultiplayerPackageInstalled() || WarnDialogForPackageInstallation())
            {
                SendInstallationAnalyticsEvent();
                InstallSelectedPackagesAndExtension();
            }
        }

        void SendInstallationAnalyticsEvent()
        {
            var answerObject = UserChoicesObject.instance;
            m_Analytics.SendInstallationEvent(answerObject.UserAnswers, answerObject.Preset,
                AnalyticsUtils.GetPackagesWithAnalyticsFormat(m_RecommendationViewData, m_SolutionToPackageData));
        }

        bool WarnDialogForPackageInstallation()
        {
            var warningMessage =
                "Ensure compatibility with your current multiplayer packages before installing or upgrading the following:\n" +
                string.Join("\n", m_PackagesToInstallNames);
            return EditorUtility.DisplayDialog("Install Packages", warningMessage, "OK", "Cancel");
        }

        void InstallSelectedPackagesAndExtension()
        {
            m_Window.SetSpinnerIconRotating();
            m_Window.rootVisualElement.SetEnabled(false);
            PackageManagement.InstallPackages(m_PackagesToInstallIds, onAllInstalled: OnInstallationFinished);
        }

        void OnInstallationFinished(bool success)
        {
            m_Window.RequestShowGettingStartedTabAfterDomainReload();
            m_Window.RemoveSpinnerIconRotating();
        }

        public void UpdatePackagesToInstall(RecommendationViewData data, SolutionsToRecommendedPackageViewData packageViewData)
        {
            m_RecommendationViewData = data;
            m_SolutionToPackageData = packageViewData;
            var packages = RecommendationUtils.PackagesToInstall(data, packageViewData);
            RecommendationUtils.GetPackagesWithAdditionalPackages(packages, out m_PackagesToInstallIds, out m_PackagesToInstallNames, out var toolTip);
            m_PackageCount.tooltip = toolTip;

            // Note: quickstart is counted in the list of packages to install, but not the names
            m_PackageCount.text = $"Packages to install: {m_PackagesToInstallNames.Count}";
            // if the list is empty, disable the button
            m_InstallPackageButton.SetEnabled(m_PackagesToInstallNames.Count > 0);
        }
    }
}
