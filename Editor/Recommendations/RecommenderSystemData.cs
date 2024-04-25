using System;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Questionnaire;

namespace Unity.Multiplayer.Center.Recommendations
{
    /// <summary>
    /// The source of data for the recommender system. Based on scoring and this data, the recommender system will
    /// populate the recommendation view data. 
    /// </summary>
    [Serializable]
    internal class RecommenderSystemData
    {
        public string TargetUnityVersion;

        /// <summary>
        /// Stores all the recommended solutions.
        /// </summary>
        public RecommendedSolution[] RecommendedSolutions;

        /// <summary>
        /// Stores all the package details.
        /// This is serialized.
        /// </summary>
        public PackageDetails[] Packages;

        /// <summary>
        /// Provides convenient access to the package details by package id.
        /// </summary>
        public Dictionary<string, PackageDetails> PackageDetailsById
        {
            get
            {
                if (m_PackageDetailsById != null) return m_PackageDetailsById;
                m_PackageDetailsById = Utils.ToDictionary(Packages, p => p.Id);
                return m_PackageDetailsById;
            }
        }

        public Dictionary<PossibleSolution, RecommendedSolution> SolutionsByType
        {
            get
            {
                if (m_SolutionsByType != null) return m_SolutionsByType;
                m_SolutionsByType = Utils.ToDictionary(RecommendedSolutions, s => s.Type);
                return m_SolutionsByType;
            }
        }

        Dictionary<string, PackageDetails> m_PackageDetailsById;
        Dictionary<PossibleSolution, RecommendedSolution> m_SolutionsByType;
    }

    [Serializable]
    internal class RecommendedSolution
    {
        public PossibleSolution Type;
        public string Title;
        public string MainPackageId; // only id because scoring will impact the rest
        public string DocUrl;
        public string ShortDescription;
        public RecommendedPackage[] RecommendedPackages;
    }

    [Serializable]
    internal class RecommendedPackage
    {
        public string PackageId;
        public RecommendationType Type;
        public string Reason;

        public RecommendedPackage(string packageId, RecommendationType type, string reason)
        {
            PackageId = packageId;
            Type = type;
            Reason = reason;
        }
    }

    [Serializable]
    internal class PackageDetails
    {
        public string Id;
        public string Name;
        public string ShortDescription;
        public string DocsUrl;
        public string [] AdditionalPackages;

        /// <summary>
        /// Details about the package.
        /// </summary>
        /// <param name="id">Package ID</param>
        /// <param name="name">Package Name (for display)</param>
        /// <param name="shortDescription">Short description.</param>
        /// <param name="docsUrl">Link to Docs</param>
        /// <param name="additionalPackages">Ids of packages that should be installed along this one, but are not formal dependencies.</param>
        public PackageDetails(string id, string name, string shortDescription, string docsUrl, string[] additionalPackages = null)
        {
            Id = id;
            Name = name;
            ShortDescription = shortDescription;
            DocsUrl = docsUrl;
            AdditionalPackages = additionalPackages;
        }
    }

    static class Utils
    {
        public static Dictionary<TKey, T> ToDictionary<T, TKey>(T[] array, Func<T, TKey> keySelector)
        {
            if (array == null) return null;
            var result = new Dictionary<TKey, T>();
            foreach (var item in array)
            {
                result[keySelector(item)] = item;
            }

            return result;
        }
    }
}