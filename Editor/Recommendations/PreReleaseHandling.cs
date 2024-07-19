using System;
using Unity.Multiplayer.Center.Questionnaire;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Unity.Multiplayer.Center.Recommendations
{
    [Serializable]
    internal class PreReleaseHandling
    {
        [SerializeReference]
        PreReleaseHandlingBase[] m_PreReleaseHandledPackages =
        {
            new SimplePreReleaseHandling("com.unity.multiplayer.playmode", "1.3"),
            new SimplePreReleaseHandling("com.unity.netcode", "1.3"),
            new SimplePreReleaseHandling("com.unity.dedicated-server", "1.3"),
            new DistributedAuthorityPreReleaseHandling(),
        };

        public event Action OnAllChecksFinished;

        public bool IsReady => m_PreReleaseHandledPackages != null && 
            Array.TrueForAll(m_PreReleaseHandledPackages, p => p is {IsReady: true});

        public void CheckForUpdates()
        {
            foreach (var package in m_PreReleaseHandledPackages)
            {
                package.OnCheckFinished += OnOnePackageVersionCheckFinished;
                package.CheckForUpdates();
            }
        }

        public void PatchPackages(RecommendationViewData toPatch)
        {
            foreach (var package in m_PreReleaseHandledPackages)
            {
                package.PatchPackages(toPatch);
            }
        }

        public void PatchRecommenderSystemData()
        {
            foreach (var package in m_PreReleaseHandledPackages)
            {
                package.PatchRecommenderSystemData();
            }
        }

        void OnOnePackageVersionCheckFinished()
        {
            var allVersionChecksDone = true;
            foreach (var package in m_PreReleaseHandledPackages)
            {
                allVersionChecksDone &= package.IsReady;
                if (package.IsReady)
                {
                    package.OnCheckFinished -= OnOnePackageVersionCheckFinished;
                }
            }

            if (allVersionChecksDone)
            {
                OnAllChecksFinished?.Invoke();
            }
        }
    }

    [Serializable]
    internal abstract class PreReleaseHandlingBase
    {
        /// <summary>
        /// Whether the versions data is ready to be used.
        /// </summary>
        public bool IsReady => m_VersionsInfo != null && !string.IsNullOrEmpty(m_VersionsInfo.latestCompatible);

        /// <summary>
        /// Triggered when this instance is ready
        /// </summary>
        public event Action OnCheckFinished;

        /// <summary>
        /// The package id e.g. com.unity.netcode
        /// </summary>
        public abstract string PackageId { get; }

        /// <summary>
        /// The minimum version that we target.
        /// </summary>
        public abstract string MinVersion { get; }

        /// <summary>
        /// The cached versions of the package.
        /// </summary>
        [SerializeField]
        protected VersionsInfo m_VersionsInfo;

        /// <summary>
        /// version which would be installed by package manager 
        /// </summary>
        [SerializeField]
        protected string m_DefaultVersion;

        PackageManagement.VersionChecker m_VersionChecker;

        /// <summary>
        /// Start (online) request to check for available versions.
        /// </summary>
        public void CheckForUpdates()
        {
            m_VersionChecker = new PackageManagement.VersionChecker(PackageId);
            m_VersionChecker.OnVersionFound += OnVersionFound;
        }

        internal string GetPreReleaseVersion(string version, VersionsInfo versionsInfo)
        {
            if (version != null && version.StartsWith(MinVersion))
                return null; // no need for a pre-release version
            return versionsInfo.latestCompatible.StartsWith(MinVersion) ? versionsInfo.latestCompatible : null;
        }

        /// <summary>
        /// Patch the recommendation view data directly.
        /// </summary>
        /// <param name="toPatch">The view data to patch</param>
        public abstract void PatchPackages(RecommendationViewData toPatch);

        /// <summary>
        /// Patch the recommender system data, which will be used for every use of the recommendation.
        /// </summary>
        public abstract void PatchRecommenderSystemData();

        protected virtual void BeforeRaisingCheckFinished() { }

        void OnVersionFound(PackageInfo packageInfo)
        {
            m_DefaultVersion = packageInfo.version;
            m_VersionsInfo = packageInfo.versions;
            if (m_VersionChecker != null) // null observed in tests 
                m_VersionChecker.OnVersionFound -= OnVersionFound;
            m_VersionChecker = null;
            BeforeRaisingCheckFinished();
            OnCheckFinished?.Invoke();
        }
    }

    /// <summary>
    /// Implementation that fetches unconditionally the version starting with a prefix for a given package, even if it
    /// is an experimental package
    /// </summary>
    [Serializable]
    internal class SimplePreReleaseHandling : PreReleaseHandlingBase
    {
        [SerializeField] string m_MinVersion;
        [SerializeField] string m_PackageId;

        public override string MinVersion => m_MinVersion;
        public override string PackageId => m_PackageId;

        public SimplePreReleaseHandling(string packageId, string minVersion)
        {
            m_PackageId = packageId;
            m_MinVersion = minVersion;
        }

        private SimplePreReleaseHandling() { }

        public override void PatchPackages(RecommendationViewData toPatch)
        {
            // Nothing to do, we only patch the package details in the recommender system data
        }

        public override void PatchRecommenderSystemData()
        {
            if (!IsReady) return;

            var allPackages = RecommenderSystemDataObject.instance.RecommenderSystemData.Packages;
            foreach (var package in allPackages)
            {
                if (package.Id == PackageId)
                {
                    package.PreReleaseVersion = GetPreReleaseVersion(m_DefaultVersion, m_VersionsInfo);
                }
            }
        }
    }

    /// <summary>
    /// Patches the RecommendationData to handle distributed authority.
    /// It checks that the min netcode package is available
    /// </summary>
    [Serializable]
    internal class DistributedAuthorityPreReleaseHandling : PreReleaseHandlingBase
    {
        public override string MinVersion => "2.0";
        public override string PackageId => "com.unity.netcode.gameobjects";

        [field: SerializeField]
        public bool IsDistributedAuthoritySupported { get; private set; }

        public DistributedAuthorityPreReleaseHandling() { }

        /// <summary>
        /// Updates the compatibility of NGO with Distributed Authority so that we know whether to show DA or not.
        /// </summary>
        public override void PatchRecommenderSystemData()
        {
            if (!IsReady) return;

            const string reasonIncompatibility = "Distributed Authority is not supported yet";
            RecommenderSystemDataObject.instance.RecommenderSystemData.UpdateIncompatibility(PossibleSolution.NGO,
                PossibleSolution.DA, IsDistributedAuthoritySupported, reasonIncompatibility);
        }

        /// <summary>
        /// Sets the version of NGO to use in the recommendation view data based on the context:
        ///  - if DA is selected, we need a 2.0 release
        ///  - if DA is not selected, we should use the default release
        /// </summary>
        /// <param name="toPatch">The recommendation view data to modify</param>
        public override void PatchPackages(RecommendationViewData toPatch)
        {
            if (!IsReady || toPatch == null) return;

            PatchInternal(toPatch, m_DefaultVersion, m_VersionsInfo);
        }

        protected override void BeforeRaisingCheckFinished()
        {
            IsDistributedAuthoritySupported = IsDistributedAuthoritySupportedFor(m_DefaultVersion, m_VersionsInfo);
        }

        internal void PatchInternal(RecommendationViewData toPatch, string version, VersionsInfo versionsInfo)
        {
            string newNgoVersion =
                RecommendationUtils.GetSelectedHostingModel(toPatch).Solution == PossibleSolution.DA
                    ? GetPreReleaseVersion(version, versionsInfo)
                    : null;

            foreach (var p in toPatch.NetcodeOptions)
            {
                if (p.Solution == PossibleSolution.NGO)
                {
                    p.MainPackage.PreReleaseVersion = newNgoVersion;
                }
            }

            foreach (var s in toPatch.ServerArchitectureOptions)
            {
                if (s.Solution == PossibleSolution.DA)
                    s.WarningString = IsDistributedAuthoritySupportedFor(version, versionsInfo)
                        ? $"Distributed authority is only supported in a pre-release version of Netcode for GameObjects. This will install Netcode for GameObjects version {newNgoVersion}"
                        : null;
            }
        }

        internal DistributedAuthorityPreReleaseHandling(string defaultVersion, VersionsInfo versionsInfo)
        {
            m_DefaultVersion = defaultVersion;
            m_VersionsInfo = versionsInfo;
            IsDistributedAuthoritySupported = IsDistributedAuthoritySupportedFor(defaultVersion, versionsInfo);
        }

        internal bool IsDistributedAuthoritySupportedFor(string version, VersionsInfo versionsInfo)
        {
            return version != null && version.StartsWith(MinVersion)
                || GetPreReleaseVersion(version, versionsInfo) != null;
        }
    }
}
