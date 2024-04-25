using System;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Questionnaire;

namespace Unity.Multiplayer.Center.Recommendations
{
    /// <summary>
    /// Contains all the architectural options that we offer to users, with a score specific to their answers.
    /// This is the data that we show in the UI.
    /// </summary>
    [Serializable]
    internal class RecommendationViewData
    {
        /// <summary>
        /// NGO or N4E or Other
        /// </summary>
        public RecommendedSolutionViewData[] NetcodeOptions;

        /// <summary>
        /// Client hosted / dedicated server.
        /// It comes with UGS services that we might or might not recommend
        /// </summary>
        public RecommendedSolutionViewData[] ServerArchitectureOptions;
    }

    /// <summary>
    /// Base fields for things that we can recommend (typically a package or a solution).
    /// </summary>
    [Serializable]
    internal class RecommendedItemViewData
    {
        /// <summary>
        /// How much we want to recommend this item. With the score, this is what is modified by the recommender system.
        /// The architecture option with the best score will be marked as MainArchitectureChoice, the other as SecondArchitectureChoice,
        /// which enables us to highlight the recommended option.
        /// </summary>
        public RecommendationType RecommendationType;

        /// <summary>
        /// Item is part of the selection, because it was preselected or user selected it.
        /// </summary>
        public bool Selected;

        /// <summary>
        /// Optional: reason why this recommendation was made
        /// </summary>
        public string Reason;

        /// <summary>
        /// Url to feature documentation
        /// </summary>
        public string DocsUrl;

        /// <summary>
        /// A short description of the feature.
        /// </summary>
        public string ShortDescription = "Short description not added yet";
        
        /// <summary>
        /// Recommendation is installed and direct dependency
        /// </summary>
        public bool IsInstalledAsProjectDependency;

        /// <summary>
        /// Installed version number of a package
        /// </summary>
        public string InstalledVersion;
    }

    /// <summary>
    /// Architectural solution (netcode or server architecture) that we offer. It comes with an optional main package and
    /// associated recommended packages.
    /// </summary>
    [Serializable]
    internal class RecommendedSolutionViewData : RecommendedItemViewData
    {
        public string Title;
        
        public PossibleSolution Solution;

        /// <summary>
        /// How much of a match is this item. Computed by recommender system based on answers.
        /// </summary>
        public float Score;

        /// <summary>
        /// The main package to install for this solution (note that this might be null, e.g. for client hosted game)
        /// </summary>
        public RecommendedPackageViewData MainPackage;
        public RecommendedPackageViewData[] AssociatedFeatures;

        public RecommendedSolutionViewData(RecommenderSystemData data, RecommendedSolution solution,
            RecommendationType type, Scoring scoring, Dictionary<string, string> installedPackageDictionary)
        {
            if (!string.IsNullOrEmpty(solution.MainPackageId))
            {
                var mainPackageDetails = data.PackageDetailsById[solution.MainPackageId];
                ShortDescription = mainPackageDetails.ShortDescription;
                MainPackage = new RecommendedPackageViewData(mainPackageDetails, type, installedPackageDictionary);
                DocsUrl = string.IsNullOrEmpty(solution.DocUrl) ? mainPackageDetails.DocsUrl : solution.DocUrl;

                if (installedPackageDictionary.ContainsKey(solution.MainPackageId))
                {
                    InstalledVersion = installedPackageDictionary[solution.MainPackageId];
                    IsInstalledAsProjectDependency = PackageManagement.IsDirectDependency(solution.MainPackageId);
                }
            }
            else
            {
                ShortDescription = solution.ShortDescription;
                DocsUrl = solution.DocUrl;
            }

            RecommendationType = type;
            Title = solution.Title;
            var otherFeatures = new RecommendedPackageViewData[solution.RecommendedPackages.Length];
            for (var i = 0; i < solution.RecommendedPackages.Length; i++)
            {
                var packageRecommendation = solution.RecommendedPackages[i];
                var packageDetails = data.PackageDetailsById[packageRecommendation.PackageId];
                otherFeatures[i] = new RecommendedPackageViewData(packageDetails, packageRecommendation, installedPackageDictionary);
            }

            AssociatedFeatures = otherFeatures;
            Reason = scoring?.GetReasonString();
            Score = scoring?.TotalScore ?? 0f;
            Selected = RecommendationType == RecommendationType.MainArchitectureChoice;

            Solution = solution.Type;
        }
    }

    /// <summary>
    /// Single package that is part of a recommendation.
    /// </summary>
    [Serializable]
    internal class RecommendedPackageViewData : RecommendedItemViewData
    {
        public string PackageId;

        public string Name;

        public RecommendedPackageViewData(PackageDetails details, RecommendationType type, Dictionary<string, string> installedPackageDictionary, string reason = null)
        {
            RecommendationType = type;
            PackageId = details.Id;
            Name = details.Name;
            Selected = type is RecommendationType.OptionalStandard or RecommendationType.OptionalFeatured;
            ShortDescription = details.ShortDescription;
            Reason = reason;
            DocsUrl = details.DocsUrl;

            if (installedPackageDictionary.ContainsKey(PackageId))
            {
                InstalledVersion = installedPackageDictionary[PackageId];
                IsInstalledAsProjectDependency = PackageManagement.IsDirectDependency(PackageId);
            }
        }

        public RecommendedPackageViewData(PackageDetails details, RecommendedPackage recommendation, Dictionary<string, string> installedPackageDictionary)
        {
            RecommendationType = recommendation.Type;
            PackageId = details.Id;
            Name = details.Name;
            Selected = recommendation.Type is RecommendationType.OptionalStandard or RecommendationType.OptionalFeatured;
            Reason = recommendation.Reason;
            ShortDescription = details.ShortDescription;
            DocsUrl = details.DocsUrl;

            if (installedPackageDictionary.ContainsKey(PackageId))
            {
                InstalledVersion = installedPackageDictionary[PackageId];
                IsInstalledAsProjectDependency = PackageManagement.IsDirectDependency(PackageId);
            }
        }
    }
}
