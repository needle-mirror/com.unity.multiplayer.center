using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    /// <summary>
    /// Section to show the available Infrastructure Options
    /// </summary>
    internal class InfrastructureSection : FeatureSection
    {
        List<RecommendationItemView> m_ViewsArchitecture = new();
        RecommendedSolutionViewData[] m_ServerArchitectures;
        RecommendedSolutionViewData SelectedArchitecture => m_ServerArchitectures.First(sol => sol.Selected);
        VisualElement m_Divider;

        /// <summary>
        /// Gets fired when the user changes the server architecture
        /// or selected or deselects a feature.
        /// </summary>
        public event Action OnUserChangedInfrastructure;

        public InfrastructureSection()
        {
            text = "Infrastructure";
            m_Divider = new VisualElement();
            m_Divider.AddToClassList("divider");
        }

        void UpdateServerArchitectureSection()
        {
            for (var index = 0; index < m_ServerArchitectures.Length; index++)
            {
                //Todo: implement a pool for the views that also handles if the list 
                //gets shorter.
                if (m_ViewsArchitecture.Count <= index)
                {
                    m_ViewsArchitecture.Add(new RecommendationItemView(isRadio: true));
                    Add(m_ViewsArchitecture[index]);
                    Add(m_Divider);
                }

                var view = m_ViewsArchitecture[index];
                SetRecommendationItemData(view, m_ServerArchitectures[index]);

                // Todo: currently use the title as the id, because there are no packages related to server architectures.
                view.FeatureId = m_ServerArchitectures[index].Title;

                view.OnUserChangedSelection -= OnUserChangedServerArchitecture;
                view.OnUserChangedSelection += OnUserChangedServerArchitecture;
            }
        }

        void OnUserChangedServerArchitecture(RecommendationItemView view, bool newValue)
        {
            if (newValue == false)
                return;

            foreach (var solution in m_ServerArchitectures)
            {
                solution.Selected = view.FeatureId == solution.Title;
            }

            OnUserChangedInfrastructure?.Invoke();
        }

        public void UpdateData(RecommendedSolutionViewData[] serverArchitectures, RecommendedSolutionViewData netcodeSolution)
        {
            ConsiderNetcodeSolutionForDedicatedServerPackage(serverArchitectures, netcodeSolution);
            m_ServerArchitectures = serverArchitectures;
            UpdateServerArchitectureSection();

            text = "Infrastructure";
            for (var index = 0; index < SelectedArchitecture.AssociatedFeatures.Length; index++)
            {
                //Todo: implement a pool for the views that also handles if the list 
                //gets shorter.
                if (index > packageViews.Count - 1)
                {
                    var itemView = new RecommendationItemView(isRadio: false);
                    Add(itemView);
                    packageViews.Add(itemView);
                }

                SetRecommendationItemData(packageViews[index], SelectedArchitecture.AssociatedFeatures[index]);
                packageViews[index].OnUserChangedSelection -= OnUserChangedFeatureSelection;
                packageViews[index].OnUserChangedSelection += OnUserChangedFeatureSelection;
            }
        }
        
        // A bit of a hack to make the dedicated server package recommendation also dependent on the netcode choice.
        void ConsiderNetcodeSolutionForDedicatedServerPackage(RecommendedSolutionViewData[] serverArchitectures, RecommendedSolutionViewData netcodeSolution)
        {
            const string dedicatedServerId = "com.unity.dedicated-server";
            const string reasonRecommended = "The Dedicated Server package helps with the development for the dedicated server platform.";
            const string reasonNotRecommended = "The Dedicated Server package is only useful when you have a dedicated server architecture and use Netcode for Gameobjects";

            var selectedArchitecture = serverArchitectures.FirstOrDefault(sol => sol.Selected);
            foreach (var serverArchitecture in serverArchitectures)
            {
                var dedicatedServerData = Array.Find(serverArchitecture.AssociatedFeatures, p => p.PackageId == dedicatedServerId);
                if (dedicatedServerData == null)
                    continue;

                dedicatedServerData.Reason = reasonNotRecommended;
                dedicatedServerData.RecommendationType = RecommendationType.Incompatible;

                // only NGO and Dedicated Server is supported.
                if (netcodeSolution?.Solution == PossibleSolution.NGO && selectedArchitecture?.Solution == PossibleSolution.DS)
                {
                    dedicatedServerData.RecommendationType = RecommendationType.OptionalFeatured;
                    dedicatedServerData.Reason = reasonRecommended;
                }
            }
        } 

        void OnUserChangedFeatureSelection(RecommendationItemView view, bool newValue)
        {
            var selectedFeature = SelectedArchitecture.AssociatedFeatures.FirstOrDefault(feature => feature.PackageId == view.FeatureId);
            if (selectedFeature == null)
            {
                Debug.LogError("Could not find feature with id: " + view.FeatureId);
                return;
            }

            selectedFeature.Selected = newValue;
            OnUserChangedInfrastructure?.Invoke();
        }
    }
}
