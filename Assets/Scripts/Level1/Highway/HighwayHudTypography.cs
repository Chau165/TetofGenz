using ChoTet.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace ChoTet.Level1.Highway
{
    /// <summary>
    /// Cozy highway HUD typography — Nunito via TextMeshPro with warm off-white text and soft shadow.
    /// </summary>
    public static class HighwayHudTypography
    {
        public const int TitleSize = 28;
        public const int BodySize = 20;
        public const int HintSize = 17;

        public static readonly Color WarmOffWhite = new Color(1f, 0.96f, 0.91f, 0.98f);
        public static readonly Color HintText = new Color(1f, 0.94f, 0.86f, 0.78f);
        public static readonly Color PanelTint = new Color(0.12f, 0.09f, 0.07f, 0.46f);
        public static readonly Color Outline = new Color(0.08f, 0.05f, 0.04f, 0.72f);
        public static readonly Color Underlay = new Color(0.04f, 0.03f, 0.02f, 0.55f);

        private const string RegularFontResource = "Level1/Fonts/Nunito-Regular SDF";
        private const string SemiBoldFontResource = "Level1/Fonts/Nunito-SemiBold SDF";
        private const string RegularTtfResource = "Level1/Fonts/Nunito-Regular";
        private const string SemiBoldTtfResource = "Level1/Fonts/Nunito-SemiBold";

        private static TMP_FontAsset regularFont;
        private static TMP_FontAsset semiBoldFont;

        public static TMP_FontAsset RegularFont => regularFont ??= LoadFont(RegularFontResource, RegularTtfResource);
        public static TMP_FontAsset SemiBoldFont => semiBoldFont ??= LoadFont(SemiBoldFontResource, SemiBoldTtfResource);

        public static TextMeshProUGUI CreateLabel(string name, Transform parent, TMP_FontAsset font, int size,
            Color color, FontStyles style = FontStyles.Normal)
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
            label.alignment = TextAlignmentOptions.TopLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.richText = false;
            label.raycastTarget = false;
            label.margin = Vector4.zero;

            ApplyReadability(label, style == FontStyles.Bold ? 0.24f : 0.2f);
            return label;
        }

        public static void ApplyReadability(TextMeshProUGUI label, float outlineWidth)
        {
            label.outlineColor = Outline;
            label.outlineWidth = outlineWidth;

            Material material = label.fontMaterial;
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor(ShaderUtilities.ID_UnderlayColor, Underlay);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.45f);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.55f);
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.08f);
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.35f);
        }

        public static Image CreatePanelBackground(Transform parent)
        {
            GameObject panelObject = new GameObject("HudPanel", typeof(RectTransform), typeof(Image),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panelObject.transform.SetParent(parent, false);

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -22f);

            Image panel = panelObject.GetComponent<Image>();
            panel.raycastTarget = false;
            panel.color = PanelTint;

            Sprite panelSprite = KenneyRpgUiLibrary.LoadDefault()?.panelInsetBeigeLight;
            if (panelSprite != null)
            {
                panel.sprite = panelSprite;
                panel.type = Image.Type.Sliced;
            }

            VerticalLayoutGroup layout = panelObject.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 7f;
            layout.padding = new RectOffset(20, 22, 18, 16);

            ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return panel;
        }

        private static TMP_FontAsset LoadFont(string sdfResourcePath, string ttfResourcePath)
        {
            TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>(sdfResourcePath);
            if (fontAsset != null)
            {
                return fontAsset;
            }

            Font sourceFont = Resources.Load<Font>(ttfResourcePath);
            if (sourceFont == null)
            {
                Debug.LogWarning("[HighwayHudTypography] Missing font at Resources/" + ttfResourcePath +
                                 ". Run Tools > Cho Tet > Setup Highway HUD Fonts.");
                return null;
            }

            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont, 90, 6, GlyphRenderMode.SDFAA, 1024, 1024);
            fontAsset.name = sourceFont.name + " SDF (Runtime)";
            return fontAsset;
        }
    }
}
