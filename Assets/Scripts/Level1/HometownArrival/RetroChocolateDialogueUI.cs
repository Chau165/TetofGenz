using System.Collections;
using ChoTet.Level1.Highway;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// Compact Retro Chocolate–style visual-novel dialogue box.
    /// Optional sprites: Resources/Level1/UI/RetroChocolate/DialogueBox, NamePlate
    /// </summary>
    public class RetroChocolateDialogueUI : MonoBehaviour
    {
        private const string DialogueBoxResource = "Level1/UI/RetroChocolate/DialogueBox";
        private const string NamePlateResource = "Level1/UI/RetroChocolate/NamePlate";

        // Layout — ~17% screen height, bottom-centered
        private const float PanelAnchorMinX = 0.09f;
        private const float PanelAnchorMaxX = 0.91f;
        private const float PanelAnchorMinY = 0.028f;
        private const float PanelAnchorMaxY = 0.198f;

        private static readonly Color BackdropTint = new Color(0.02f, 0.01f, 0.01f, 0.28f);
        private static readonly Color DropShadow = new Color(0.04f, 0.02f, 0.01f, 0.5f);
        private static readonly Color OuterFrame = new Color(0.22f, 0.13f, 0.08f, 1f);
        private static readonly Color ChocolatePanel = new Color(0.4f, 0.25f, 0.17f, 1f);
        private static readonly Color InnerPanel = new Color(0.46f, 0.3f, 0.2f, 1f);
        private static readonly Color CreamStroke = new Color(0.93f, 0.85f, 0.72f, 1f);
        private static readonly Color NamePlateFill = new Color(0.74f, 0.48f, 0.32f, 1f);
        private static readonly Color NamePlateShadow = new Color(0.18f, 0.1f, 0.06f, 1f);
        private static readonly Color NamePlateHighlight = new Color(1f, 0.92f, 0.8f, 0.55f);
        private static readonly Color BodyTextColor = new Color(1f, 0.99f, 0.96f, 1f);
        private static readonly Color NameTextColor = new Color(1f, 0.98f, 0.9f, 1f);
        private static readonly Color HintTextColor = new Color(0.93f, 0.86f, 0.74f, 0.92f);
        private static readonly Color HintArrowColor = new Color(1f, 0.84f, 0.48f, 1f);
        private static readonly Color FooterRuleTint = new Color(0.93f, 0.85f, 0.72f, 0.34f);
        private static readonly Color TextOutline = new Color(0.1f, 0.06f, 0.03f, 0.95f);

        private static RetroChocolateDialogueUI instance;

        private CanvasGroup rootGroup;
        private CanvasGroup panelGroup;
        private TextMeshProUGUI nameText;
        private TextMeshProUGUI bodyText;
        private TextMeshProUGUI hintText;
        private TextMeshProUGUI hintArrow;
        private bool advanceRequested;

        public static bool IsOpen => instance != null && instance.rootGroup != null && instance.rootGroup.alpha > 0.01f;

        public static RetroChocolateDialogueUI Ensure()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindAnyObjectByType<RetroChocolateDialogueUI>();
            if (instance != null)
            {
                return instance;
            }

            GameObject host = new GameObject("RetroChocolateDialogueUI");
            instance = host.AddComponent<RetroChocolateDialogueUI>();
            instance.BuildUi();
            return instance;
        }

        public IEnumerator ShowLine(string speakerName, string line)
        {
            EnsureUi();
            advanceRequested = false;
            nameText.text = string.IsNullOrWhiteSpace(speakerName) ? "..." : speakerName;
            bodyText.text = string.IsNullOrWhiteSpace(line) ? "..." : line;
            rootGroup.gameObject.SetActive(true);
            rootGroup.blocksRaycasts = true;
            panelGroup.alpha = 0f;
            rootGroup.alpha = 0f;

            // Start Vietnamese TTS Voice
            ChoTet.Runtime.VoiceNarrator.Instance.Speak(speakerName, line);

            float fadeElapsed = 0f;
            const float fadeDuration = 0.2f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                float t = fadeDuration > 0f ? Mathf.Clamp01(fadeElapsed / fadeDuration) : 1f;
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                rootGroup.alpha = eased;
                panelGroup.alpha = eased;
                yield return null;
            }

            rootGroup.alpha = 1f;
            panelGroup.alpha = 1f;

            float blinkTimer = 0f;
            while (!advanceRequested)
            {
                if (WasConfirmPressed())
                {
                    advanceRequested = true;
                }

                blinkTimer += Time.unscaledDeltaTime;
                float pulse = 0.4f + 0.6f * (0.5f + 0.5f * Mathf.Sin(blinkTimer * 6f));
                if (hintArrow != null)
                {
                    hintArrow.color = new Color(HintArrowColor.r, HintArrowColor.g, HintArrowColor.b, pulse);
                }

                yield return null;
            }

            // Stop voice narration
            ChoTet.Runtime.VoiceNarrator.Instance.Stop();

            fadeElapsed = 0f;
            const float fadeOut = 0.12f;
            while (fadeElapsed < fadeOut)
            {
                fadeElapsed += Time.unscaledDeltaTime;
                float t = fadeOut > 0f ? Mathf.Clamp01(fadeElapsed / fadeOut) : 1f;
                panelGroup.alpha = 1f - t;
                rootGroup.alpha = 1f - t;
                yield return null;
            }

            rootGroup.alpha = 0f;
            panelGroup.alpha = 0f;
            rootGroup.blocksRaycasts = false;
            rootGroup.gameObject.SetActive(false);
        }

        public IEnumerator ShowSequence(params (string speaker, string line)[] lines)
        {
            if (lines == null)
            {
                yield break;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                yield return ShowLine(lines[i].speaker, lines[i].line);
            }
        }

        private void EnsureUi()
        {
            if (rootGroup == null)
            {
                BuildUi();
            }
        }

        private void BuildUi()
        {
            GameObject root = new GameObject("RetroChocolateDialogue", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 520;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            rootGroup = root.GetComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.blocksRaycasts = false;

            Image backdrop = CreateImage("Backdrop", root.transform, BackdropTint);
            Stretch(backdrop.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject panelRoot = new GameObject("PanelRoot", typeof(RectTransform), typeof(CanvasGroup));
            panelRoot.transform.SetParent(root.transform, false);
            panelGroup = panelRoot.GetComponent<CanvasGroup>();
            RectTransform panelRootRect = panelRoot.GetComponent<RectTransform>();
            panelRootRect.anchorMin = new Vector2(PanelAnchorMinX, PanelAnchorMinY);
            panelRootRect.anchorMax = new Vector2(PanelAnchorMaxX, PanelAnchorMaxY);
            panelRootRect.offsetMin = Vector2.zero;
            panelRootRect.offsetMax = Vector2.zero;

            BuildDialogueBox(panelRoot.transform);
            BuildFloatingNameplate(panelRoot.transform);

            root.SetActive(false);
        }

        private void BuildDialogueBox(Transform panelRoot)
        {
            Image shadow = CreateImage("DropShadow", panelRoot, DropShadow);
            Stretch(shadow.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, -5f), new Vector2(8f, 4f));

            Image outerFrame = CreateImage("OuterFrame", panelRoot, OuterFrame);
            Stretch(outerFrame.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image creamBorder = CreateImage("CreamBorder", outerFrame.transform, CreamStroke);
            Stretch(creamBorder.rectTransform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));

            Image mainPanel = CreateImage("MainPanel", outerFrame.transform, ChocolatePanel);
            Stretch(mainPanel.rectTransform, Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f));

            Sprite boxSprite = Resources.Load<Sprite>(DialogueBoxResource);
            if (boxSprite != null)
            {
                mainPanel.sprite = boxSprite;
                mainPanel.type = Image.Type.Sliced;
                mainPanel.color = Color.white;
            }

            Image innerPanel = CreateImage("InnerPanel", mainPanel.transform, InnerPanel);
            Stretch(innerPanel.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(innerPanel.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            Stretch(contentRect, Vector2.zero, Vector2.one, new Vector2(20f, 14f), new Vector2(-20f, -14f));

            bodyText = CreateCrispLabel("Line", content.transform, HighwayHudTypography.RegularFont, 25,
                BodyTextColor, FontStyles.Normal);
            bodyText.alignment = TextAlignmentOptions.MidlineLeft;
            bodyText.textWrappingMode = TextWrappingModes.Normal;
            bodyText.lineSpacing = 0f;
            Stretch(bodyText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 40f), new Vector2(0f, 0f));

            BuildContinueFooter(content.transform);
        }

        private void BuildContinueFooter(Transform contentParent)
        {
            GameObject footer = new GameObject("ContinueFooter", typeof(RectTransform));
            footer.transform.SetParent(contentParent, false);
            RectTransform footerRect = footer.GetComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0f, 0f);
            footerRect.anchorMax = new Vector2(1f, 0f);
            footerRect.pivot = new Vector2(0.5f, 0f);
            footerRect.anchoredPosition = Vector2.zero;
            footerRect.sizeDelta = new Vector2(0f, 38f);

            Image footerRule = CreateImage("FooterRule", footer.transform, FooterRuleTint);
            RectTransform ruleRect = footerRule.rectTransform;
            ruleRect.anchorMin = new Vector2(0f, 1f);
            ruleRect.anchorMax = new Vector2(1f, 1f);
            ruleRect.pivot = new Vector2(0.5f, 1f);
            ruleRect.sizeDelta = new Vector2(0f, 1f);
            ruleRect.anchoredPosition = Vector2.zero;

            GameObject hintRow = new GameObject("HintRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            hintRow.transform.SetParent(footer.transform, false);
            RectTransform hintRowRect = hintRow.GetComponent<RectTransform>();
            Stretch(hintRowRect, Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(0f, -5f));

            HorizontalLayoutGroup hintLayout = hintRow.GetComponent<HorizontalLayoutGroup>();
            hintLayout.childAlignment = TextAnchor.MiddleRight;
            hintLayout.spacing = 5f;
            hintLayout.padding = new RectOffset(0, 0, 2, 0);
            hintLayout.childControlWidth = false;
            hintLayout.childControlHeight = true;
            hintLayout.childForceExpandWidth = false;
            hintLayout.childForceExpandHeight = false;

            hintText = CreateCrispLabel("Hint", hintRow.transform, HighwayHudTypography.RegularFont, 22,
                HintTextColor, FontStyles.Normal);
            hintText.text = "E / Space — tiếp tục";
            hintText.alignment = TextAlignmentOptions.MidlineRight;
            hintText.outlineWidth = 0.08f;
            hintText.rectTransform.sizeDelta = new Vector2(280f, 28f);

            hintArrow = CreateCrispLabel("HintArrow", hintRow.transform, HighwayHudTypography.SemiBoldFont, 18,
                HintArrowColor, FontStyles.Bold);
            hintArrow.text = "»";
            hintArrow.alignment = TextAlignmentOptions.Midline;
            hintArrow.outlineWidth = 0.1f;
            hintArrow.rectTransform.sizeDelta = new Vector2(16f, 28f);
        }

        private void BuildFloatingNameplate(Transform panelRoot)
        {
            GameObject nameplateRoot = new GameObject("NameplateFloat", typeof(RectTransform));
            nameplateRoot.transform.SetParent(panelRoot, false);
            RectTransform plateRootRect = nameplateRoot.GetComponent<RectTransform>();
            plateRootRect.anchorMin = new Vector2(0f, 1f);
            plateRootRect.anchorMax = new Vector2(0f, 1f);
            plateRootRect.pivot = new Vector2(0f, 0f);
            plateRootRect.anchoredPosition = new Vector2(28f, 10f);
            plateRootRect.sizeDelta = new Vector2(200f, 46f);

            Image plateShadow = CreateImage("PlateShadow", nameplateRoot.transform, NamePlateShadow);
            RectTransform shadowRect = plateShadow.rectTransform;
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(3f, -4f);
            shadowRect.offsetMax = new Vector2(7f, 0f);

            Image plateBack = CreateImage("PlateBack", nameplateRoot.transform, NamePlateFill);
            Stretch(plateBack.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Sprite nameSprite = Resources.Load<Sprite>(NamePlateResource);
            if (nameSprite != null)
            {
                plateBack.sprite = nameSprite;
                plateBack.type = Image.Type.Sliced;
                plateBack.color = Color.white;
            }

            Image plateHighlight = CreateImage("PlateHighlight", plateBack.transform, NamePlateHighlight);
            RectTransform hiRect = plateHighlight.rectTransform;
            hiRect.anchorMin = new Vector2(0f, 1f);
            hiRect.anchorMax = new Vector2(1f, 1f);
            hiRect.pivot = new Vector2(0.5f, 1f);
            hiRect.sizeDelta = new Vector2(0f, 5f);
            hiRect.anchoredPosition = Vector2.zero;

            Image plateTab = CreateImage("PlateTab", plateBack.transform, CreamStroke);
            RectTransform tabRect = plateTab.rectTransform;
            tabRect.anchorMin = new Vector2(0f, 0.15f);
            tabRect.anchorMax = new Vector2(0f, 0.85f);
            tabRect.pivot = new Vector2(0f, 0.5f);
            tabRect.sizeDelta = new Vector2(5f, 0f);
            tabRect.anchoredPosition = Vector2.zero;

            nameText = CreateCrispLabel("Speaker", plateBack.transform, HighwayHudTypography.SemiBoldFont, 24,
                NameTextColor, FontStyles.Bold);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            Stretch(nameText.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 0f), new Vector2(-12f, 0f));
        }

        private static TextMeshProUGUI CreateCrispLabel(string name, Transform parent, TMP_FontAsset font, int size,
            Color color, FontStyles style)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            if (font == null)
            {
                TMP_Settings.LoadDefaultSettings();
                font = TMP_Settings.defaultFontAsset;
            }

            label.font = font;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.richText = false;
            label.raycastTarget = false;
            label.margin = Vector4.zero;
            label.outlineColor = TextOutline;
            label.outlineWidth = 0.14f;
            return label;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static bool WasConfirmPressed()
        {
            if (Keyboard.current != null
                && (Keyboard.current.enterKey.wasPressedThisFrame
                    || Keyboard.current.spaceKey.wasPressedThisFrame
                    || Keyboard.current.eKey.wasPressedThisFrame))
            {
                return true;
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
