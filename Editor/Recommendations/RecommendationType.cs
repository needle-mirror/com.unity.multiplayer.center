using System;

namespace Unity.Multiplayer.Center.Recommendations
{
    /// <summary>
    /// Types of recommendation that are both used in ViewData and in the ground truth data for the recommendation.
    /// Architecture choices must be made. Packages are typically optional.
    /// </summary>
    [Serializable]
    internal enum RecommendationType
    {
        /// <summary>
        /// Invalid value, indicates error.
        /// </summary>
        None,

        /// <summary>
        /// Featured option (e.g. NGO if NGO is the recommended architecture)
        /// Note that in the case of architecture choice, the user should select something.
        /// </summary>
        MainArchitectureChoice,

        /// <summary>
        /// Non-featured option (e.g. N4E if NGO is the recommended architecture)
        /// </summary>
        SecondArchitectureChoice,

        /// <summary>
        /// Optional and highlighted, such as Vivox
        /// </summary>
        OptionalFeatured,

        /// <summary>
        /// Optional but not highlighted
        /// </summary>
        OptionalStandard,

        /// <summary>
        /// Not recommended, but not incompatible with the user intent.
        /// </summary>
        NotRecommended,

        /// <summary>
        /// Incompatible with the user intent. Might even break something, we need to warn the user
        /// </summary>
        Incompatible,
    }
}
