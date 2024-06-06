using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using UnityEngine;

namespace Unity.Multiplayer.Center.Recommendations
{
    internal static class RecommendationUtils
    {
        public static List<RecommendedPackageViewData> PackagesToInstall(RecommendationViewData recommendation,
            SolutionsToRecommendedPackageViewData solutionToPackageData)
        {
            var packagesToInstall = new List<RecommendedPackageViewData>();

            // Can happen on first load
            if (recommendation?.NetcodeOptions == null)
                return packagesToInstall;

            var selectedNetcode = GetSelectedNetcode(recommendation);
            if (selectedNetcode == null)
                return packagesToInstall;

            // add features based on netcode
            if (selectedNetcode.MainPackage != null)
                packagesToInstall.Add(selectedNetcode.MainPackage);

            var selectedServerArchitecture = GetSelectedHostingModel(recommendation);
            if (selectedServerArchitecture.MainPackage != null)
                packagesToInstall.Add(selectedServerArchitecture.MainPackage);

            var packages = solutionToPackageData.GetPackagesForSelection(selectedNetcode.Solution, selectedServerArchitecture.Solution)
                .Where(e => e.Selected);

            packagesToInstall.AddRange(packages);

            return packagesToInstall;
        }

        public static RecommendedSolutionViewData GetSelectedHostingModel(RecommendationViewData recommendation)
        {
            // TODO: remove linq usage
            return recommendation.ServerArchitectureOptions.FirstOrDefault(sol => sol.Selected);
        }

        public static RecommendedSolutionViewData GetSelectedNetcode(RecommendationViewData recommendation)
        {
            // TODO: remove linq usage
            return recommendation.NetcodeOptions.FirstOrDefault(sol => sol.Selected);
        }

        public static PackageDetails GetPackageDetailForPackageId(string packageId)
        {
            var idToPackageDetailDict = RecommenderSystemDataObject.instance.RecommenderSystemData.PackageDetailsById;

            if (idToPackageDetailDict.TryGetValue(packageId, out var packageDetail))
                return packageDetail;

            Debug.LogError("Trying to get package detail for package id that does not exist: " + packageId);
            return null;
        }

        /// <summary>
        /// Returns all the packages passed via packageIds and their informal dependencies (stored in AdditionalPackages)
        /// </summary>
        /// <returns>List of PackageDetails</returns>
        /// <param name="packageIds">List of package id</param>
        /// <param name="toolTip">tooltip text</param>
        public static List<PackageDetails> GetPackagesWithAdditionalPackages(List<string> packageIds, out string toolTip)
        {
            var packagesToInstall = new List<PackageDetails>();
            var toolTipBuilder = new StringBuilder();
            foreach (var packId in packageIds)
            {
                var packageDetail = GetPackageDetailForPackageId(packId);

                toolTipBuilder.Append(packageDetail.Name);
                packagesToInstall.Add(packageDetail);

                if (packageDetail.AdditionalPackages is {Length: > 0})
                {
                    toolTipBuilder.Append(" + ");
                    var additionalPackages = packageDetail.AdditionalPackages.Select(GetPackageDetailForPackageId);
                    packagesToInstall.AddRange(additionalPackages);
                    toolTipBuilder.Append(String.Join(", ", additionalPackages.Select(p => p.Name)));
                }

                toolTipBuilder.Append("\n");
            }

            // remove last newline
            toolTip = toolTipBuilder.ToString().TrimEnd('\n');
            return packagesToInstall;
        }

        /// <summary>
        /// Reapplies the previous selection on the view data
        /// </summary>
        /// <param name="recommendation">The recommendation view data to update</param>
        /// <param name="data">The previous selection data</param>
        public static void ApplyPreviousSelection(RecommendationViewData recommendation, SelectedSolutionsData data)
        {
            if (data == null || recommendation == null)
                return;

            if (data.SelectedNetcodeSolution != SelectedSolutionsData.NetcodeSolution.None)
            {
                foreach (var d in recommendation.NetcodeOptions)
                {
                    d.Selected = Logic.ConvertNetcodeSolution(d) == data.SelectedNetcodeSolution;
                }
            }

            if (data.SelectedHostingModel != SelectedSolutionsData.HostingModel.None)
            {
                foreach (var view in recommendation.ServerArchitectureOptions)
                {
                    view.Selected = Logic.ConvertInfrastructure(view) == data.SelectedHostingModel;
                }
            }
        }

        /// <summary>
        /// Returns the packages that are of the given recommendation type
        /// </summary>
        /// <param name="packages">All the package view data as returned by the recommender system</param>
        /// <param name="type">The target recommendation type</param>
        /// <returns>The filtered list</returns>
        public static List<RecommendedPackageViewData> FilterByType(List<RecommendedPackageViewData> packages, RecommendationType type)
        {
            return packages.Where(p => p.RecommendationType == type).ToList();
        }

        public static int IndexOfMaximumScore(RecommendedSolutionViewData[] array)
        {
            var maxIndex = -1;
            var maxScore = float.MinValue;
            for (var index = 0; index < array.Length; index++)
            {
                var solution = array[index];
                if (solution.RecommendationType == RecommendationType.Incompatible)
                    continue;
                
                if (solution.Score > maxScore)
                {
                    maxScore = solution.Score;
                    maxIndex = index;
                }
            }

            return maxIndex;
        }

        /// <summary>
        /// Looks through the hosting models and marks them incompatible if necessary (deselected and recommendation
        /// type incompatible).
        /// Note that this might leave the recommendation in an invalid state (no hosting model selected)
        /// </summary>
        /// <param name="recommendation">The recommendation data to modify.</param>
        public static void MarkIncompatibleHostingModels(RecommendationViewData recommendation)
        {
            var netcode = GetSelectedNetcode(recommendation);

            for (var index = 0; index < recommendation.ServerArchitectureOptions.Length; index++)
            {
                var hosting = recommendation.ServerArchitectureOptions[index];
                var isCompatible = RecommenderSystemDataObject.instance.RecommenderSystemData.IsHostingModelCompatibleWithNetcode(netcode.Solution, hosting.Solution, out _);
                if (!isCompatible)
                {
                    hosting.RecommendationType = RecommendationType.Incompatible;
                    hosting.Selected = false;
                }
            }
        }
    }
}
