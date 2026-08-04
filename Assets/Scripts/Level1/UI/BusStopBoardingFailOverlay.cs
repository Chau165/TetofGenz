using System.Collections;
using ChoTet.Level1.Highway;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1.UI
{
    /// <summary>Full-screen blackout moment when boarding the wrong bus-stop vehicle.</summary>
    public static class BusStopBoardingFailOverlay
    {
        private static readonly Color KidnapTextColor = new Color(0.95f, 0.18f, 0.16f, 1f);
        private static readonly Color WrongCarTextColor = new Color(1f, 0.84f, 0.29f, 1f);

        private static CanvasGroup rootGroup;
        private static TextMeshProUGUI messageText;
        private static bool built;

        public static IEnumerator PlayMoment(VehicleBoardingOutcome outcome)
        {
            EnsureBuilt();
            if (rootGroup == null || messageText == null)
            {
                yield break;
            }

            string message;
            Color textColor;
            switch (outcome)
            {
                case VehicleBoardingOutcome.Kidnap:
                    message = "Bạn đã đi lộn xe";
                    textColor = KidnapTextColor;
                    break;
                case VehicleBoardingOutcome.WrongVehicle:
                    message = "Bạn đã đi lộn xe";
                    textColor = WrongCarTextColor;
                    break;
                default:
                    yield break;
            }

            messageText.text = message;
            messageText.color = textColor;
            messageText.alpha = 0f;
            rootGroup.gameObject.SetActive(true);
            rootGroup.blocksRaycasts = true;

            yield return Fade(0f, 1f, 0.42f);
            yield return FadeText(0f, 1f, 0.18f);
            yield return new WaitForSecondsRealtime(2.2f);
            yield return FadeText(1f, 0f, 0.18f);
            yield return Fade(1f, 0f, 0.38f);

            rootGroup.blocksRaycasts = false;
            rootGroup.gameObject.SetActive(false);
        }

        private static IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                rootGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            rootGroup.alpha = to;
        }

        private static IEnumerator FadeText(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                messageText.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            messageText.alpha = to;
        }

        private static void EnsureBuilt()
        {
            if (built)
            {
                return;
            }

            GameObject host = new GameObject("BusStopBoardingFailOverlay", typeof(RectTransform));
            Object.DontDestroyOnLoad(host);

            GameObject root = new GameObject("Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(host.transform, false);

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 620;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            rootGroup = root.GetComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.blocksRaycasts = false;

            Image backdrop = CreateImage("Backdrop", root.transform, Color.black);
            Stretch(backdrop.rectTransform);

            GameObject labelObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(root.transform, false);
            messageText = labelObject.GetComponent<TextMeshProUGUI>();
            messageText.font = HighwayHudTypography.SemiBoldFont;
            messageText.fontSize = 58;
            messageText.fontStyle = FontStyles.Bold;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.textWrappingMode = TextWrappingModes.Normal;
            messageText.overflowMode = TextOverflowModes.Overflow;
            messageText.raycastTarget = false;
            messageText.outlineWidth = 0.2f;
            messageText.outlineColor = new Color(0f, 0f, 0f, 0.85f);
            Stretch(labelObject.GetComponent<RectTransform>());

            root.SetActive(false);
            built = true;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
