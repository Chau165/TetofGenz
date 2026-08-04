using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1
{
    /// <summary>
    /// Full-screen fade overlay for Level 1 phase transitions (indoor packing → outdoor street).
    /// </summary>
    public class Level1ScreenFade : MonoBehaviour
    {
        [SerializeField] private float defaultFadeDuration = 0.55f;

        private Canvas fadeCanvas;
        private CanvasGroup canvasGroup;
        private Image fadeImage;
        private Coroutine fadeRoutine;

        /// <summary>True while a fade coroutine is running.</summary>
        public bool IsFading { get; private set; }

        private void Awake()
        {
            EnsureOverlay();
        }

        /// <summary>Fades to black and invokes <paramref name="onComplete"/> without fading back in.</summary>
        public Coroutine RunFadeOut(System.Action onComplete, float fadeDuration = -1f)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            float duration = fadeDuration > 0f ? fadeDuration : defaultFadeDuration;
            fadeRoutine = StartCoroutine(FadeOutRoutine(onComplete, duration));
            return fadeRoutine;
        }

        /// <summary>Fades to black, invokes <paramref name="midpoint"/>, then fades back in.</summary>
        public Coroutine RunTransition(System.Action midpoint, float fadeDuration = -1f)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            float duration = fadeDuration > 0f ? fadeDuration : defaultFadeDuration;
            fadeRoutine = StartCoroutine(TransitionRoutine(midpoint, duration));
            return fadeRoutine;
        }

        private IEnumerator FadeOutRoutine(System.Action onComplete, float fadeDuration)
        {
            IsFading = true;
            try
            {
                EnsureOverlay();
                fadeCanvas.gameObject.SetActive(true);

                yield return FadeTo(1f, fadeDuration);

                onComplete?.Invoke();
            }
            finally
            {
                IsFading = false;
                fadeRoutine = null;
            }
        }

        private IEnumerator TransitionRoutine(System.Action midpoint, float fadeDuration)
        {
            IsFading = true;
            try
            {
                EnsureOverlay();
                fadeCanvas.gameObject.SetActive(true);

                yield return FadeTo(1f, fadeDuration);

                midpoint?.Invoke();

                // One frame so teleport / camera updates settle before revealing the scene.
                yield return null;

                yield return FadeTo(0f, fadeDuration);

                fadeCanvas.gameObject.SetActive(false);
            }
            finally
            {
                IsFading = false;
                fadeRoutine = null;
            }
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.05f, duration);

            while (elapsed < safeDuration)
            {
                float dt = Time.unscaledDeltaTime;
                // Editor/MCP stalls can report 0 dt for long stretches — keep fade moving.
                if (dt <= 0f)
                {
                    dt = 1f / 60f;
                }

                elapsed += dt;
                float t = Mathf.Clamp01(elapsed / safeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        private void EnsureOverlay()
        {
            if (fadeCanvas != null)
            {
                return;
            }

            GameObject root = new GameObject("Level1ScreenFadeOverlay");
            root.transform.SetParent(transform, false);

            fadeCanvas = root.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 500;
            root.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new GameObject("FadePanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panel.transform.SetParent(root.transform, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            canvasGroup = panel.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;

            fadeImage = panel.GetComponent<Image>();
            fadeImage.color = Color.black;
            fadeImage.raycastTarget = true;

            root.SetActive(false);
        }
    }
}
