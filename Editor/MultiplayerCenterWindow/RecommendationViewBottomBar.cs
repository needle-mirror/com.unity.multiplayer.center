using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Analytics;
using Unity.Multiplayer.Center.Onboarding;
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
        List<PackageDetails> m_PackagesToInstall = new ();
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
                string.Join("\n", m_PackagesToInstall.ConvertAll(p => p.Name));
            return EditorUtility.DisplayDialog("Install Packages", warningMessage, "OK", "Cancel");
        }

        void InstallSelectedPackagesAndExtension()
        {
            m_Window.SetSpinnerIconRotating();
            m_Window.rootVisualElement.SetEnabled(false);

            var toInstall = m_PackagesToInstall.ConvertAll(p => p.Id);
            toInstall.Add(QuickstartIsMissingView.PackageId);
            PackageManagement.InstallPackages(toInstall, onAllInstalled: OnInstallationFinished);
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
            m_PackagesToInstall = RecommendationUtils.GetPackagesWithAdditionalPackages(packages.Select(p => p.PackageId).ToList(), out var toolTip);
            m_PackageCount.tooltip = toolTip;

            m_PackageCount.text = $"Packages to install: {m_PackagesToInstall.Count}";
            //if the list is empty, disable the button
            m_InstallPackageButton.SetEnabled(m_PackagesToInstall.Count > 0);
        }
    }
}
