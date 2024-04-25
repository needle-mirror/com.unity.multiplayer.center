using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window.UI
{
    /// <summary>
    /// Section to show available tooling.
    /// </summary>
    internal class ToolingSection : FeatureSection
    {
        protected override List<RecommendationItemView> packageViews => this.Query<RecommendationItemView>().ToList();
        public event Action OnUserChangedTooling;
        RecommendedSolutionViewData m_Solution;

        public ToolingSection() 
        {
            text = "Tools";
        }
        
        public void UpdateData(RecommendedSolutionViewData solution)
        {
            m_Solution = solution;
            for (var i = 0; i < solution.AssociatedFeatures.Length; i++)
            {
                //Todo: implement a pool for the views that also handles if the list 
                //gets shorter.
                if (packageViews.Count <= i)
                    Add(new RecommendationItemView(isRadio: false));
                var view = packageViews[i];
                SetRecommendationItemData(view, solution.AssociatedFeatures[i]);
                view.OnUserChangedSelection -= OnToolSelectionChanged;
                view.OnUserChangedSelection += OnToolSelectionChanged;
            }
        }

        void OnToolSelectionChanged(RecommendationItemView view, bool isSelected)
        {
            var featureToSet = m_Solution.AssociatedFeatures.FirstOrDefault(sol => sol.PackageId == view.FeatureId);
            if (featureToSet == null)
            {
                Debug.LogError($"ToolingSection - Feature {view.FeatureId} not found");
                return;
            }
            featureToSet.Selected = isSelected;
            OnUserChangedTooling?.Invoke();
        }
    }
}
