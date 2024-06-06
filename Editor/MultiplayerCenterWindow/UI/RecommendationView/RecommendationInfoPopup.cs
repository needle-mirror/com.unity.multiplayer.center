using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Window.UI.RecommendationView
{
    /// <summary>
    /// Represents a InfoPopUp specifically designed for recommendations.
    /// Note: This implementation assumes that the direct parent of this RecommendationInfoPopup is the
    /// Multiplayer Center window root visual element, which is crucial for position computations of the InfoPopUp.
    /// </summary>
    /// Todo: I think in the new design we can remove this.
    internal class RecommendationInfoPopup : VisualElement
    {
        Label m_InfoPopUpLabel;

        public struct InfoPopupPosition
        {
            internal StyleLength Top;
            internal StyleLength Bottom;
            internal StyleLength Left;
        }

        public bool IsInfoPopUpVisible
        {
            get => resolvedStyle.display == DisplayStyle.Flex;
            internal set => style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public RecommendationInfoPopup()
        {
            name = "infopopup-enabler";
            AddToClassList("infopopup-enabler");
            m_InfoPopUpLabel = new Label("");
            IsInfoPopUpVisible = false;
            Add(m_InfoPopUpLabel);
        }

        // The method to show InfoPopUp with updated info if needed
        public void UpdateInfoPopup(string info)
        {
            // Update the InfoPopUp label with new information
            m_InfoPopUpLabel.text = info;
            IsInfoPopUpVisible = true;
        }

        /// <summary>
        ///  Returns the adjusted position of the InfoPopUp based on the caller position
        /// </summary>
        /// <param name="caller">This is the Recommended badge on recommendations</param>
        /// <returns></returns>
        public InfoPopupPosition GetAdjustedPosition(VisualElement caller)
        {
            InfoPopupPosition adjustedPosition = new();

            // Right side margin from the window 
            const float marginRight = 10f;

            // Calculate the overflow amount from the right side of the window
            var overflow = style.left.value.value + resolvedStyle.width - parent.layout.width;

            // If the overflow is positive, we need to move the InfoPopUp to the left
            adjustedPosition.Left = overflow > 0
                ? style.left.value.value - overflow - marginRight
                : style.left.value.value + marginRight;

            // Check if the badge is on top or bottom of the window,
            // if it's on the bottom, we need to move the InfoPopUp to the top of the badge
            var showInfoPopUpTop = style.top.value.value > parent.layout.height / 2;
            if (showInfoPopUpTop)
            {
                adjustedPosition.Bottom =
                    parent.layout.height - caller.worldBound.yMin + caller.worldBound.height;
                adjustedPosition.Top = StyleKeyword.Auto;
            }
            else
            {
                adjustedPosition.Top = caller.worldBound.yMin;
                adjustedPosition.Bottom = StyleKeyword.Auto;
            }

            return adjustedPosition;
        }
    }
}
