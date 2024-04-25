using System.Collections.Generic;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window
{
    /// <summary>
    /// A RecommendationSection is a Foldout that contains a list of RecommendationItemViews.
    /// </summary>
    internal abstract class FeatureSection : Foldout
    {
        protected virtual List<RecommendationItemView> packageViews { get; } = new();

        protected FeatureSection()
        {
            AddToClassList("section-foldout");
        }

        protected void SetRecommendationItemData(RecommendationItemView view, RecommendedPackageViewData package)
        {
            view.UpdateData(package.Name, package.PackageId, package);
        }

        protected void SetRecommendationItemData(RecommendationItemView view, RecommendedSolutionViewData solution)
        {
            var featureName = solution.Title;
            var featureId = solution.Title;
            view.UpdateData(featureName, featureId, solution);
        }
    }
}
