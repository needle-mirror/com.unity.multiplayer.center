using System;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Recommendations;
using Unity.Multiplayer.Center.Window;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.UI.RecommendationView
{
    /// <summary>
    /// Section to show the available Netcode Options
    /// </summary>
    internal class NetcodeSection : FeatureSection
    {
        protected override List<RecommendationItemView> packageViews => this.Query<RecommendationItemView>().ToList();
        RecommendedSolutionViewData[] m_AvailableSolutions;

        /// <summary>
        /// Gets fired when the user changed the netcode option.
        /// </summary>
        public event Action OnUserChangedNetcode;

        public NetcodeSection()
        {
            text = "Netcode Solution";
        }

        public void UpdateData(RecommendedSolutionViewData[] availableNetcodeSolutions)
        {
            m_AvailableSolutions = availableNetcodeSolutions;

            for (var i = 0; i < m_AvailableSolutions.Length; i++)
            {
                //Todo: implement a pool for the views that also handles if the list 
                //gets shorter.
                if (packageViews.Count <= i)
                    Add(new RecommendationItemView(isRadio: true));

                var view = packageViews[i];
                SetRecommendationItemData(view, availableNetcodeSolutions[i]);
                view.OnUserChangedSelection -= NetcodeOptionChanged;
                view.OnUserChangedSelection += NetcodeOptionChanged;
            }
        }

        void NetcodeOptionChanged(RecommendationItemView view, bool selected)
        {
            if (selected == false)
                return;

            foreach (var solution in m_AvailableSolutions)
                solution.Selected = view.FeatureId == solution.Title;

            OnUserChangedNetcode?.Invoke();
        }
    }
}
