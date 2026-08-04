using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Hover/click scale and tint feedback for Kenney menu buttons.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MainMenuButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float pressScale = 0.96f;
        [SerializeField] private float tweenSpeed = 14f;
        [SerializeField] private Color hoverTint = new Color(1.12f, 1.08f, 1f, 1f);

        private RectTransform rectTransform;
        private Image image;
        private Vector3 baseScale = Vector3.one;
        private Color baseColor = Color.white;
        private float targetScale = 1f;
        private bool isPointerInside;
        private bool isPointerDown;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            baseScale = rectTransform.localScale;
            if (image != null)
            {
                baseColor = image.color;
            }
        }

        private void Update()
        {
            float desired = isPointerDown ? pressScale : isPointerInside ? hoverScale : 1f;
            targetScale = Mathf.Lerp(targetScale, desired, Time.unscaledDeltaTime * tweenSpeed);
            rectTransform.localScale = baseScale * targetScale;

            if (image != null)
            {
                Color goal = isPointerInside && !isPointerDown
                    ? new Color(baseColor.r * hoverTint.r, baseColor.g * hoverTint.g, baseColor.b * hoverTint.b, baseColor.a)
                    : baseColor;
                image.color = Color.Lerp(image.color, goal, Time.unscaledDeltaTime * tweenSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            isPointerDown = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
        }

        public void SetHoverScale(float scale)
        {
            hoverScale = scale;
        }
    }
}
