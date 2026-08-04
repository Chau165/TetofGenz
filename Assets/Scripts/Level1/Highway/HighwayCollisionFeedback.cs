using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1.Highway
{
    /// <summary>Lightweight hit feedback — brief screen flicker and a short life-lost message.</summary>
    public class HighwayCollisionFeedback : MonoBehaviour
    {
        [SerializeField] private float flickerDuration = 0.32f;
        [SerializeField] private int flickerPulses = 3;
        [SerializeField] private float maxFlashAlpha = 0.38f;
        [SerializeField] private float messageDuration = 1.05f;

        private Canvas overlayCanvas;
        private Image flashImage;
        private Text hitMessageText;
        private Coroutine feedbackRoutine;

        public void PlayHit()
        {
            EnsureUi();
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(PlayHitRoutine());
        }

        private void EnsureUi()
        {
            if (overlayCanvas != null)
            {
                return;
            }

            GameObject root = new GameObject("HighwayHitFeedback");
            root.transform.SetParent(transform, false);
            overlayCanvas = root.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 550;
            root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject flashGo = new GameObject("Flash", typeof(RectTransform), typeof(Image));
            flashGo.transform.SetParent(root.transform, false);
            RectTransform flashRect = flashGo.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;
            flashImage = flashGo.GetComponent<Image>();
            flashImage.color = new Color(1f, 0.22f, 0.18f, 0f);
            flashImage.raycastTarget = false;

            GameObject messageGo = new GameObject("HitMessage", typeof(RectTransform), typeof(Text));
            messageGo.transform.SetParent(root.transform, false);
            RectTransform messageRect = messageGo.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.5f, 0.58f);
            messageRect.anchorMax = new Vector2(0.5f, 0.58f);
            messageRect.pivot = new Vector2(0.5f, 0.5f);
            messageRect.sizeDelta = new Vector2(520f, 72f);
            hitMessageText = messageGo.GetComponent<Text>();
            hitMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hitMessageText.fontSize = 34;
            hitMessageText.fontStyle = FontStyle.Bold;
            hitMessageText.alignment = TextAnchor.MiddleCenter;
            hitMessageText.color = new Color(1f, 0.88f, 0.78f, 0f);
            hitMessageText.raycastTarget = false;
            hitMessageText.text = "Mất 1 mạng!";
        }

        private IEnumerator PlayHitRoutine()
        {
            hitMessageText.text = "Mất 1 mạng!";
            float pulseDuration = flickerDuration / Mathf.Max(1, flickerPulses);

            for (int i = 0; i < flickerPulses; i++)
            {
                yield return FlickerPulse(pulseDuration, maxFlashAlpha * (1f - i * 0.12f));
            }

            flashImage.color = new Color(1f, 0.22f, 0.18f, 0f);

            float messageElapsed = 0f;
            while (messageElapsed < messageDuration)
            {
                messageElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(messageElapsed / messageDuration);
                float alpha = t < 0.12f
                    ? Mathf.Lerp(0f, 1f, t / 0.12f)
                    : Mathf.Lerp(1f, 0f, (t - 0.12f) / 0.88f);
                Color c = hitMessageText.color;
                c.a = alpha;
                hitMessageText.color = c;
                yield return null;
            }

            Color hidden = hitMessageText.color;
            hidden.a = 0f;
            hitMessageText.color = hidden;
            feedbackRoutine = null;
        }

        private IEnumerator FlickerPulse(float duration, float peakAlpha)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float alpha = Mathf.Sin(t * Mathf.PI) * peakAlpha;
                flashImage.color = new Color(1f, 0.22f, 0.18f, alpha);
                yield return null;
            }
        }
    }
}
