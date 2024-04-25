using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Onboarding;
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
        readonly VisualElement m_SpinningIcon;

        MultiplayerCenterWindow m_Window = EditorWindow.GetWindow<MultiplayerCenterWindow>();
        List<PackageDetails> m_PackagesToInstall = new ();

        public RecommendationViewBottomBar()
        {
            name = "bottom-bar";
            m_PackageCount = new Label {name = "package-count"};

            // Setup Install Button
            m_InstallPackageButton = new Button(OnInstallButtonClicked) {text = "Install Packages"};
            m_InstallPackageButton.AddToClassList(StyleClasses.NextStepButton);

            // Setup Spinning Icon
            m_SpinningIcon = new VisualElement();
            m_SpinningIcon.AddToClassList("icon");

            // if we are current already processing an installation, show the spinning icon
            if (!PackageManagement.IsInstallationFinished())
            {
                m_SpinningIcon.AddToClassList("processing");
            };
            
            m_SpinningIcon.AddToClassList(StyleClasses.NextStepButton);

            // Put the button and the spinning icon in a container
            var installPackageContainer = new VisualElement() {name = "install-package-container"};
            installPackageContainer.Add(m_SpinningIcon);
            installPackageContainer.Add(m_InstallPackageButton);

            Add(m_PackageCount);
            Add(installPackageContainer);
        }

        void OnInstallButtonClicked()
        {
            if (!PackageManagement.IsAnyMultiplayerPackageInstalled() || WarnDialogForPackageInstallation())
            {
                InstallSelectedPackagesAndExtension();
            }
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
            m_Window.rootVisualElement.SetEnabled(false);
            m_SpinningIcon.AddToClassList("processing");
            var toInstall = m_PackagesToInstall.ConvertAll(p => p.Id);
            toInstall.Add(QuickstartIsMissingView.PackageId);
            PackageManagement.InstallPackages(toInstall, onAllInstalled: OnInstallationFinished);
        }

        void OnInstallationFinished(bool success)
        {
            m_Window.RequestShowGettingStartedTabAfterDomainReload();
            m_SpinningIcon.RemoveFromClassList("processing");
        }

        public void UpdatePackagesToInstall(List<RecommendedPackageViewData> packages)
        {
            m_PackagesToInstall = RecommendationUtils.GetPackagesWithAdditionalPackages(packages.Select(p => p.PackageId).ToList(), out var toolTip);
            m_PackageCount.tooltip = toolTip;

            m_PackageCount.text = $"Packages to install: {m_PackagesToInstall.Count}";
            //if the list is empty, disable the button
            m_InstallPackageButton.SetEnabled(m_PackagesToInstall.Count > 0);
        }
    }
}
