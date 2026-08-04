using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Glow, pulse, and enhanced hover for the playable level card.
    /// </summary>
    public class LevelSelectPlayableCardFx : MonoBehaviour
    {
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float pulseSpeed = 1.8f;
        [SerializeField] private float glowMinAlpha = 0.28f;
        [SerializeField] private float glowMaxAlpha = 0.62f;

        private RectTransform rectTransform;
        private Image glowImage;
        private Image shadowImage;
        private MainMenuButtonFx buttonFx;
        private Vector3 baseScale = Vector3.one;

        public void Initialize(Image glow, Image shadow, MainMenuButtonFx buttonFeedback)
        {
            glowImage = glow;
            shadowImage = shadow;
            buttonFx = buttonFeedback;
            rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                baseScale = rectTransform.localScale;
            }

            if (buttonFx != null)
            {
                buttonFx.SetHoverScale(hoverScale);
            }
        }

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
                baseScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
            }
        }

        private void Update()
        {
            if (glowImage != null)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
                float alpha = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, pulse);
                Color color = glowImage.color;
                color.a = alpha;
                glowImage.color = color;
            }

            if (shadowImage != null)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
                Color color = shadowImage.color;
                color.a = 0.22f + pulse * 0.12f;
                shadowImage.color = color;
            }
        }
    }
}
