using ChoTet.Level1.Highway;
using TMPro;
using UnityEngine;

namespace ChoTet.Level1.UI
{
    /// <summary>
    /// Reusable student-style note paper panel. Wire text fields in the prefab or call <see cref="SetContent"/>.
    /// </summary>
    public class VehicleBookingNoteView : MonoBehaviour
    {
        private static readonly Color InkBlack = new Color(0.06f, 0.05f, 0.05f, 1f);
        private static readonly Color InkSoft = new Color(0.22f, 0.19f, 0.17f, 1f);

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI licensePlateLabelText;
        [SerializeField] private TextMeshProUGUI licensePlateValueText;
        [SerializeField] private TextMeshProUGUI pickupTimeLabelText;
        [SerializeField] private TextMeshProUGUI pickupTimeValueText;
        [SerializeField] private TextMeshProUGUI carColorLabelText;
        [SerializeField] private TextMeshProUGUI carColorValueText;
        [SerializeField] private TextMeshProUGUI footerText;

        private void OnEnable()
        {
            ApplyInkTypography();
        }

        public void SetContent(
            string title,
            string licensePlate,
            string pickupTime,
            string carColor,
            string footer)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (licensePlateValueText != null)
            {
                licensePlateValueText.text = licensePlate;
            }

            if (pickupTimeValueText != null)
            {
                pickupTimeValueText.text = pickupTime;
            }

            if (carColorValueText != null)
            {
                carColorValueText.text = carColor;
            }

            if (footerText != null)
            {
                footerText.text = footer;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ApplyDefaultSampleContent()
        {
            if (licensePlateLabelText != null)
            {
                licensePlateLabelText.text = "Biển số xe:";
            }

            if (pickupTimeLabelText != null)
            {
                pickupTimeLabelText.text = "Thời gian đón:";
            }

            if (carColorLabelText != null)
            {
                carColorLabelText.text = "Màu xe:";
            }

            string colorName = BusStopChallengeSession.TryGetHomeRideSlot(out BusStopCandidateSlot slot)
                ? Level1TrafficVisualUtility.GetBodyColorName(slot.BodyColorIndex)
                : "Đang cập nhật";

            SetContent(
                "THÔNG TIN XE ĐÃ ĐẶT",
                VehicleBookingSession.LicensePlate,
                VehicleBookingSession.PickupTime,
                colorName,
                "Chúc chuyến đi thuận lợi! :)");
        }

        private void ApplyInkTypography()
        {
            ApplyInkTypography(titleText, HighwayHudTypography.SemiBoldFont, FontStyles.Bold, 28);
            ApplyInkTypography(licensePlateLabelText, HighwayHudTypography.RegularFont, FontStyles.Normal, 18, InkSoft);
            ApplyInkTypography(licensePlateValueText, HighwayHudTypography.SemiBoldFont, FontStyles.Bold, 19);
            ApplyInkTypography(pickupTimeLabelText, HighwayHudTypography.RegularFont, FontStyles.Normal, 18, InkSoft);
            ApplyInkTypography(pickupTimeValueText, HighwayHudTypography.SemiBoldFont, FontStyles.Bold, 19);
            ApplyInkTypography(carColorLabelText, HighwayHudTypography.RegularFont, FontStyles.Normal, 18, InkSoft);
            ApplyInkTypography(carColorValueText, HighwayHudTypography.SemiBoldFont, FontStyles.Bold, 19);
            ApplyInkTypography(footerText, HighwayHudTypography.RegularFont, FontStyles.Italic, 19, InkSoft);
        }

        private static void ApplyInkTypography(
            TextMeshProUGUI label,
            TMP_FontAsset preferredFont,
            FontStyles style,
            int fontSize,
            Color? color = null)
        {
            if (label == null)
            {
                return;
            }

            if (preferredFont != null)
            {
                label.font = preferredFont;
            }

            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color ?? InkBlack;
            label.enableVertexGradient = false;
            label.outlineWidth = 0f;
            label.fontSharedMaterial = label.font != null ? label.font.material : label.fontSharedMaterial;
        }

#if UNITY_EDITOR
        public void BindEditorReferences(
            TextMeshProUGUI title,
            TextMeshProUGUI licensePlateLabel,
            TextMeshProUGUI licensePlateValue,
            TextMeshProUGUI pickupTimeLabel,
            TextMeshProUGUI pickupTimeValue,
            TextMeshProUGUI carColorLabel,
            TextMeshProUGUI carColorValue,
            TextMeshProUGUI footer)
        {
            titleText = title;
            licensePlateLabelText = licensePlateLabel;
            licensePlateValueText = licensePlateValue;
            pickupTimeLabelText = pickupTimeLabel;
            pickupTimeValueText = pickupTimeValue;
            carColorLabelText = carColorLabel;
            carColorValueText = carColorValue;
            footerText = footer;
        }
#endif
    }
}
