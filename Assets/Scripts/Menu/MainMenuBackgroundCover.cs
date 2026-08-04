using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Scales a RawImage UV rect so the texture covers the rect without stretching (CSS object-fit: cover).
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class MainMenuBackgroundCover : MonoBehaviour
    {
        private RawImage rawImage;
        private RectTransform rectTransform;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            ApplyCover();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyCover();
        }

        public void ApplyCover()
        {
            if (rawImage == null)
            {
                rawImage = GetComponent<RawImage>();
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            Texture texture = rawImage != null ? rawImage.texture : null;
            if (texture == null || rectTransform == null)
            {
                return;
            }

            float textureWidth = texture.width;
            float textureHeight = texture.height;
            if (textureWidth <= 0f || textureHeight <= 0f)
            {
                return;
            }

            Rect rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float textureAspect = textureWidth / textureHeight;
            float rectAspect = rect.width / rect.height;

            if (textureAspect > rectAspect)
            {
                float visibleWidth = rectAspect / textureAspect;
                rawImage.uvRect = new Rect((1f - visibleWidth) * 0.5f, 0f, visibleWidth, 1f);
            }
            else
            {
                float visibleHeight = textureAspect / rectAspect;
                rawImage.uvRect = new Rect(0f, (1f - visibleHeight) * 0.5f, 1f, visibleHeight);
            }
        }
    }
}
