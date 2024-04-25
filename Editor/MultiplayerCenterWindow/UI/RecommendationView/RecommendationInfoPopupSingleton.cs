using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.UI.RecommendationView
{
    internal static class RecommendationInfoPopupSingleton
    {
        static Unity.Multiplayer.Center.Window.MultiplayerCenterWindow MultiplayerCenterWindow => EditorWindow.GetWindow<Unity.Multiplayer.Center.Window.MultiplayerCenterWindow>();
        static RecommendationInfoPopup s_RecommendationInfoPopup;
        static VisualElement s_CurrentCaller;

        public static RecommendationInfoPopup recommendationInfoPopup =>
            s_RecommendationInfoPopup ??= new RecommendationInfoPopup();

        /// <summary>
        /// Show the RecommendationInfoPopup and sets the styles to the correct position
        /// </summary>
        /// <param name="caller"> Recommendation Badge that calls the InfoPopup</param> 
        /// <param name="info"> Info text displayed on the InfoPopup</param> 
        public static void ShowInfoPopup(VisualElement caller, string info)
        {
            // clicked on the same badge so hide the InfoPopup
            if (caller == s_CurrentCaller)
            {
                HideInfoPopup();
                s_CurrentCaller = null;
                return;
            }

            s_CurrentCaller = caller;

            recommendationInfoPopup.UpdateInfoPopup(info);
            recommendationInfoPopup.style.left = caller.worldBound.xMin;
            recommendationInfoPopup.style.top = caller.worldBound.yMin;

            // The layout width need to be calculated before we can get the adjusted position
            // We hide the layout first to avoid flickering
            recommendationInfoPopup.style.opacity = 0;

            // We need to wait for the layout to be calculated to get the adjusted position
            recommendationInfoPopup.schedule.Execute(() =>
            {
                var adjustedPosition = recommendationInfoPopup.GetAdjustedPosition(caller);
                recommendationInfoPopup.style.left = adjustedPosition.Left;
                recommendationInfoPopup.style.top = adjustedPosition.Top;
                recommendationInfoPopup.style.bottom = adjustedPosition.Bottom;

                // Here the calculated position is correct, we can show the InfoPopup
                recommendationInfoPopup.style.opacity = 1;
            }).ExecuteLater(50);

            RemoveCallbacks();
            AddCallbacks();
        }

        static void AddCallbacks()
        {
            MultiplayerCenterWindow.rootVisualElement.RegisterCallback<GeometryChangedEvent>(evt => HideInfoPopup());
            MultiplayerCenterWindow.rootVisualElement.RegisterCallback<ClickEvent>(OnWindowClicked);
        }

        static void RemoveCallbacks()
        {
            MultiplayerCenterWindow.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(evt => HideInfoPopup());
            MultiplayerCenterWindow.rootVisualElement.UnregisterCallback<ClickEvent>(OnWindowClicked);
        }

        static void HideInfoPopup()
        {
            recommendationInfoPopup.IsInfoPopUpVisible = false;
            RemoveCallbacks();
            s_CurrentCaller = null;
        }

        static void OnWindowClicked(ClickEvent evt)
        {
            HideInfoPopup();
        }
    }
}
