using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Multiplayer.Center.Common;
using Unity.Multiplayer.Center.Questionnaire;
using Unity.Multiplayer.Center.Recommendations;
using UnityEngine;

namespace Unity.Multiplayer.Center.Window.UI
{
    internal static class RecommendationUtils
    {
        public static List<RecommendedPackageViewData> PackagesToInstall(RecommendationViewData recommendation)
        {
            var packagesToInstall = new List<RecommendedPackageViewData>();

            // Can happen on first load
            if (recommendation?.NetcodeOptions == null)
                return packagesToInstall;

            var selectedNetcode = recommendation.NetcodeOptions.FirstOrDefault(sol => sol.Selected);
            if (selectedNetcode == null)
                return packagesToInstall;

            // add features based on netcode
            if (selectedNetcode.MainPackage != null)
                packagesToInstall.Add(selectedNetcode.MainPackage);
            var netCodeAdditionalFeatures = selectedNetcode.AssociatedFeatures.Where(pack => pack.Selected);
            packagesToInstall.AddRange(netCodeAdditionalFeatures);

            // add features based on server architecture
            var selectedServerArchitecture = recommendation.ServerArchitectureOptions.First(sol => sol.Selected);
            if (selectedServerArchitecture.MainPackage != null)
                packagesToInstall.Add(selectedServerArchitecture.MainPackage);

            var serverArchitectureFeatures = selectedServerArchitecture.AssociatedFeatures.Where(pack => pack.Selected);
            packagesToInstall.AddRange(serverArchitectureFeatures);

            // remove all incompatible packages (this could happen with the dedicated server package)
            packagesToInstall.RemoveAll(p => p.RecommendationType == RecommendationType.Incompatible);

            return packagesToInstall;
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
            if (data == null) 
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
    }
}
