using System;
using ChoTet.Level1.Highway;
using ChoTet.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1.UI
{
    /// <summary>Popup panel showing a vehicle license plate — white fill, thick black border, radius 8.</summary>
    public class VehicleLicensePlatePanelView : MonoBehaviour
    {
        private const float BorderThickness = 5f;
        private const float CardWidth = 440f;
        private const float CardHeight = 232f;

        [SerializeField] private GameObject root;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button goButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private TextMeshProUGUI plateText;
        [SerializeField] private TextMeshProUGUI hintText;

        public event Action OnGoClicked;
        public event Action OnRejectClicked;

        public bool IsOpen => root != null && root.activeSelf;

        public void EnsureBuilt(Transform parent)
        {
            if (root != null && goButton != null)
            {
                WireButtons();
                return;
            }

            if (root != null)
            {
                Destroy(root);
                root = null;
                backdropButton = null;
                closeButton = null;
                goButton = null;
                rejectButton = null;
                plateText = null;
                hintText = null;
            }

            GameObject panelRoot = new GameObject("VehicleLicensePlatePanel", typeof(RectTransform));
            panelRoot.transform.SetParent(parent, false);
            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image dim = CreateImage("Backdrop", panelRoot.transform, new Color(0f, 0f, 0f, 0.42f));
            Stretch(dim.rectTransform);
            backdropButton = dim.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;

            GameObject card = new GameObject("Card", typeof(RectTransform));
            card.transform.SetParent(panelRoot.transform, false);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(CardWidth, CardHeight);

            Sprite borderSprite = VehicleLicensePlateSpriteUtility.GetPlateBorderSprite();
            Sprite fillSprite = VehicleLicensePlateSpriteUtility.GetPlateFillSprite();

            Image border = CreateImage("Border", card.transform, Color.black);
            border.sprite = borderSprite;
            border.type = Image.Type.Sliced;
            Stretch(border.rectTransform);

            Image fill = CreateImage("Fill", card.transform, Color.white);
            fill.sprite = fillSprite;
            fill.type = Image.Type.Sliced;
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(BorderThickness, BorderThickness);
            fillRect.offsetMax = new Vector2(-BorderThickness, -BorderThickness);

            TextMeshProUGUI title = CreateLabel("Title", card.transform, 18, FontStyles.Bold, "BIỂN SỐ XE");
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -16f);
            titleRect.sizeDelta = new Vector2(-24f, 28f);

            plateText = CreateLabel("PlateValue", card.transform, 36, FontStyles.Bold, "51H-247.68");
            RectTransform plateRect = plateText.rectTransform;
            plateRect.anchorMin = new Vector2(0f, 0.56f);
            plateRect.anchorMax = new Vector2(1f, 0.56f);
            plateRect.pivot = new Vector2(0.5f, 0.5f);
            plateRect.anchoredPosition = Vector2.zero;
            plateRect.sizeDelta = new Vector2(-32f, 56f);
            plateText.alignment = TextAlignmentOptions.Center;

            hintText = CreateLabel("Hint", card.transform, 15, FontStyles.Italic,
                "Đối chiếu với giấy đặt xe trong túi (phím I)");
            RectTransform hintRect = hintText.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(1f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.anchoredPosition = new Vector2(0f, 62f);
            hintRect.sizeDelta = new Vector2(-24f, 34f);
            hintText.color = new Color(0.18f, 0.18f, 0.18f, 0.88f);
            hintText.alignment = TextAlignmentOptions.Center;

            KenneyRpgUiLibrary library = KenneyRpgUiLibrary.LoadDefault();
            goButton = CreateActionButton("GoButton", card.transform, library, "Đi",
                new Color(0.2f, 0.55f, 0.28f, 1f), new Vector2(-108f, 16f));
            rejectButton = CreateActionButton("RejectButton", card.transform, library, "Từ chối",
                new Color(0.55f, 0.22f, 0.18f, 1f), new Vector2(108f, 16f));

            GameObject closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(card.transform, false);
            RectTransform closeRect = closeGo.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-8f, -8f);
            closeRect.sizeDelta = new Vector2(32f, 32f);
            Image closeImage = closeGo.GetComponent<Image>();
            closeImage.color = new Color(0.12f, 0.12f, 0.12f, 0.12f);
            closeButton = closeGo.GetComponent<Button>();
            TextMeshProUGUI closeLabel = CreateLabel("X", closeGo.transform, 20, FontStyles.Bold, "×");
            Stretch(closeLabel.rectTransform);

            root = panelRoot;
            root.SetActive(false);
            WireButtons();
        }

        public void Show(string licensePlate)
        {
            if (root == null)
            {
                return;
            }

            WireButtons();

            if (plateText != null)
            {
                plateText.text = licensePlate ?? string.Empty;
            }

            root.SetActive(true);
            transform.SetAsLastSibling();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void WireButtons()
        {
            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(Hide);
                backdropButton.onClick.AddListener(Hide);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            if (goButton != null)
            {
                goButton.onClick.RemoveListener(HandleGoClicked);
                goButton.onClick.AddListener(HandleGoClicked);
            }

            if (rejectButton != null)
            {
                rejectButton.onClick.RemoveListener(HandleRejectClicked);
                rejectButton.onClick.AddListener(HandleRejectClicked);
            }
        }

        private void HandleGoClicked()
        {
            OnGoClicked?.Invoke();
        }

        private void HandleRejectClicked()
        {
            OnRejectClicked?.Invoke();
        }

        private static Button CreateActionButton(string name, Transform parent, KenneyRpgUiLibrary library,
            string label, Color tint, Vector2 anchoredPosition)
        {
            Sprite sprite = library != null && library.buttonLongBrown != null
                ? library.buttonLongBrown
                : null;
            GameObject buttonObject = KenneyRpgUiWidgets.CreateSpritePanel(name, parent, sprite, tint);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(188f, 44f);
            buttonObject.GetComponent<Image>().type = Image.Type.Sliced;

            Button button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = buttonObject.GetComponent<Image>();

            TextMeshProUGUI text = CreateLabel("Label", buttonObject.transform, 20, FontStyles.Bold, label);
            Stretch(text.rectTransform);
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return button;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, int size, FontStyles style, string text)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.font = HighwayHudTypography.SemiBoldFont;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = Color.black;
            label.text = text;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            return label;
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
