using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    public enum MenuTextStyle
    {
        Title,
        Subtitle,
        Button,
        DialogTitle,
        DialogBody,
        IconButton
    }

    /// <summary>
    /// Fantasy menu typography — Almendra with outline and soft shadow.
    /// </summary>
    public static class MainMenuTypography
    {
        public const int TitleSize = 78;
        public const int ButtonSize = 34;
        public const int SubtitleSize = 21;
        public const int DialogTitleSize = 44;
        public const int DialogBodySize = 24;
        public const int IconButtonSize = 30;

        private const string BoldFontResource = "Menu/Fonts/Almendra-Bold";
        private const string RegularFontResource = "Menu/Fonts/Almendra-Regular";

        private static Font titleFont;
        private static Font bodyFont;

        public static Font TitleFont => titleFont ??= LoadFont(BoldFontResource);
        public static Font BodyFont => bodyFont ??= LoadFont(RegularFontResource);

        public static Text CreateStyledText(string name, Transform parent, MenuTextStyle style, TextAnchor alignment,
            string content)
        {
            GetStyle(style, out Font font, out int size, out FontStyle fontStyle, out Color color,
                out float outlineDistance, out Vector2 shadowDistance, out Color outlineColor, out Color shadowColor);

            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);

            Text label = labelObject.GetComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.fontStyle = fontStyle;
            label.alignment = alignment;
            label.color = color;
            label.text = content;
            label.supportRichText = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.lineSpacing = style == MenuTextStyle.Title ? 0.92f : 1f;

            ApplyOutline(label, outlineColor, outlineDistance);
            ApplyShadow(label, shadowColor, shadowDistance);
            return label;
        }

        public static void GetStyle(MenuTextStyle style, out Font font, out int size, out FontStyle fontStyle,
            out Color color, out float outlineDistance, out Vector2 shadowDistance, out Color outlineColor,
            out Color shadowColor)
        {
            font = BodyFont;
            fontStyle = FontStyle.Normal;
            outlineDistance = 2.5f;
            shadowDistance = new Vector2(2f, -3f);
            outlineColor = new Color(0.1f, 0.06f, 0.03f, 0.95f);
            shadowColor = new Color(0.05f, 0.03f, 0.02f, 0.65f);

            switch (style)
            {
                case MenuTextStyle.Title:
                    font = TitleFont;
                    size = TitleSize;
                    fontStyle = FontStyle.Bold;
                    color = KenneyRpgUiWidgets.TitleGold;
                    outlineDistance = 3.5f;
                    shadowDistance = new Vector2(4f, -5f);
                    break;
                case MenuTextStyle.Subtitle:
                    font = BodyFont;
                    size = SubtitleSize;
                    fontStyle = FontStyle.Italic;
                    color = KenneyRpgUiWidgets.SubtitleText;
                    outlineDistance = 1.5f;
                    shadowDistance = new Vector2(1.5f, -2f);
                    outlineColor = new Color(0.1f, 0.06f, 0.03f, 0.55f);
                    shadowColor = new Color(0.05f, 0.03f, 0.02f, 0.4f);
                    break;
                case MenuTextStyle.Button:
                    font = TitleFont;
                    size = ButtonSize;
                    fontStyle = FontStyle.Bold;
                    color = KenneyRpgUiWidgets.ButtonText;
                    outlineDistance = 2.75f;
                    shadowDistance = new Vector2(2f, -3f);
                    break;
                case MenuTextStyle.DialogTitle:
                    font = TitleFont;
                    size = DialogTitleSize;
                    fontStyle = FontStyle.Bold;
                    color = KenneyRpgUiWidgets.TitleGold;
                    outlineDistance = 3f;
                    shadowDistance = new Vector2(3f, -4f);
                    break;
                case MenuTextStyle.DialogBody:
                    font = BodyFont;
                    size = DialogBodySize;
                    fontStyle = FontStyle.Normal;
                    color = KenneyRpgUiWidgets.BodyText;
                    outlineDistance = 1.75f;
                    shadowDistance = new Vector2(2f, -2.5f);
                    break;
                case MenuTextStyle.IconButton:
                    font = TitleFont;
                    size = IconButtonSize;
                    fontStyle = FontStyle.Bold;
                    color = KenneyRpgUiWidgets.ButtonText;
                    outlineDistance = 2f;
                    shadowDistance = new Vector2(1.5f, -2f);
                    break;
                default:
                    size = ButtonSize;
                    color = KenneyRpgUiWidgets.ButtonText;
                    break;
            }
        }

        private static Font LoadFont(string resourcePath)
        {
            Font font = Resources.Load<Font>(resourcePath);
            if (font != null)
            {
                return font;
            }

            Debug.LogWarning("[MainMenuTypography] Missing font at Resources/" + resourcePath +
                             ". Reimport menu fonts or run Tools > Cho Tet > Setup Kenney RPG Menu Assets.");
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void ApplyOutline(Text label, Color color, float distance)
        {
            Outline outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        private static void ApplyShadow(Text label, Color color, Vector2 distance)
        {
            Shadow shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }
    }
}
