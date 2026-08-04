using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    public static class LevelSelectCardVisualHelper
    {
        public static void StyleTitleBadge(Text badge)
        {
            badge.font = MainMenuTypography.TitleFont;
            badge.fontStyle = FontStyle.Bold;
            badge.fontSize = 26;
            badge.color = KenneyRpgUiWidgets.ButtonText;
            badge.resizeTextForBestFit = true;
            badge.resizeTextMinSize = 20;
            badge.resizeTextMaxSize = 26;
            StrengthenOutline(badge, 3.2f);
        }

        public static void StyleSubtitle(Text subtitle)
        {
            subtitle.font = MainMenuTypography.TitleFont;
            subtitle.fontStyle = FontStyle.Bold;
            subtitle.fontSize = 26;
            subtitle.color = new Color(0.98f, 0.9f, 0.72f, 1f);
            subtitle.resizeTextForBestFit = true;
            subtitle.resizeTextMinSize = 20;
            subtitle.resizeTextMaxSize = 26;
            StrengthenOutline(subtitle, 2.8f);
        }

        public static void StyleStatus(Text status, bool unlocked)
        {
            status.font = MainMenuTypography.TitleFont;
            status.fontStyle = FontStyle.Bold;
            status.fontSize = 22;
            status.color = unlocked
                ? new Color(1f, 0.9f, 0.45f, 1f)
                : new Color(0.92f, 0.72f, 0.48f, 1f);
            status.resizeTextForBestFit = true;
            status.resizeTextMinSize = 16;
            status.resizeTextMaxSize = 22;
            StrengthenOutline(status, 2.6f);
        }

        public static void AddPlayableEmphasis(Transform cardRoot, KenneyRpgUiLibrary library, MainMenuButtonFx buttonFx)
        {
            GameObject shadow = KenneyRpgUiWidgets.CreateSpritePanel("PlayableShadow", cardRoot.parent, null,
                new Color(0.04f, 0.02f, 0.01f, 0.38f));
            shadow.transform.SetSiblingIndex(cardRoot.GetSiblingIndex());
            RectTransform cardRect = cardRoot as RectTransform;
            RectTransform shadowRect = shadow.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                shadowRect.anchorMin = cardRect.anchorMin;
                shadowRect.anchorMax = cardRect.anchorMax;
                shadowRect.pivot = cardRect.pivot;
                shadowRect.sizeDelta = cardRect.sizeDelta;
                shadowRect.anchoredPosition = cardRect.anchoredPosition + new Vector2(6f, -8f);
            }

            Image shadowImage = shadow.GetComponent<Image>();
            shadowImage.raycastTarget = false;

            GameObject glow = KenneyRpgUiWidgets.CreateSpritePanel("PlayableGlow", cardRoot, library.barYellowMid,
                new Color(1f, 0.86f, 0.38f, 0.42f));
            glow.transform.SetAsFirstSibling();
            KenneyRpgUiWidgets.Stretch(glow.GetComponent<RectTransform>(), -10f);
            Image glowImage = glow.GetComponent<Image>();
            glowImage.raycastTarget = false;

            AddGoldBorder(cardRoot, library);

            LevelSelectPlayableCardFx fx = cardRoot.gameObject.AddComponent<LevelSelectPlayableCardFx>();
            fx.Initialize(glowImage, shadowImage, buttonFx);
        }

        public static void BuildLockedOverlay(KenneyRpgUiLibrary library, GameObject lockedOverlay)
        {
            lockedOverlay.GetComponent<Image>().color = new Color(0.05f, 0.03f, 0.02f, 0.42f);

            for (int i = 0; i < 3; i++)
            {
                bool yellow = i % 2 == 0;
                GameObject stripe = KenneyRpgUiWidgets.CreateSpritePanel("TapeStripe" + i, lockedOverlay.transform,
                    yellow ? library.barYellowMid : library.barRedHorizontal,
                    yellow ? new Color(0.95f, 0.78f, 0.22f, 0.55f) : new Color(0.82f, 0.18f, 0.12f, 0.5f));
                RectTransform stripeRect = stripe.GetComponent<RectTransform>();
                stripeRect.anchorMin = new Vector2(-0.15f, 0.18f + i * 0.2f);
                stripeRect.anchorMax = new Vector2(1.15f, 0.28f + i * 0.2f);
                stripeRect.offsetMin = Vector2.zero;
                stripeRect.offsetMax = Vector2.zero;
                stripeRect.localRotation = Quaternion.Euler(0f, 0f, -18f + i * 3f);
                stripe.GetComponent<Image>().raycastTarget = false;
            }

            GameObject blueprint = KenneyRpgUiWidgets.CreateSpritePanel("BlueprintGrid", lockedOverlay.transform,
                library.panelInsetBeigeLight, new Color(0.55f, 0.72f, 0.88f, 0.08f));
            KenneyRpgUiWidgets.Stretch(blueprint.GetComponent<RectTransform>(), 12f);
            blueprint.GetComponent<Image>().raycastTarget = false;

            Text lockIcon = MainMenuTypography.CreateStyledText("LockIcon", lockedOverlay.transform,
                MenuTextStyle.Title, TextAnchor.MiddleCenter, LevelSelectCopy.LockIconSymbol);
            lockIcon.fontSize = 58;
            KenneyRpgUiWidgets.Layout(lockIcon.rectTransform,
                new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(90f, 90f));

            Text comingSoon = MainMenuTypography.CreateStyledText("ComingSoon", lockedOverlay.transform,
                MenuTextStyle.Button, TextAnchor.MiddleCenter, LevelSelectCopy.ComingSoonLabel);
            comingSoon.fontSize = 24;
            comingSoon.color = new Color(0.98f, 0.88f, 0.58f, 0.95f);
            StrengthenOutline(comingSoon, 2.5f);
            KenneyRpgUiWidgets.Layout(comingSoon.rectTransform,
                new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(260f, 36f));
        }

        private static void AddGoldBorder(Transform cardRoot, KenneyRpgUiLibrary library)
        {
            CreateBorderBar("BorderTop", cardRoot, library.barYellowMid,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -5f), new Vector2(0f, 5f));
            CreateBorderBar("BorderBottom", cardRoot, library.barYellowMid,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, -5f), new Vector2(0f, 5f));
            CreateBorderBar("BorderLeft", cardRoot, library.barYellowVertical,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-5f, 0f), new Vector2(5f, 0f));
            CreateBorderBar("BorderRight", cardRoot, library.barYellowVertical,
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-5f, 0f), new Vector2(5f, 0f));
        }

        private static void CreateBorderBar(string name, Transform parent, Sprite sprite, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (sprite == null)
            {
                return;
            }

            GameObject bar = KenneyRpgUiWidgets.CreateSpritePanel(name, parent, sprite,
                new Color(1f, 0.88f, 0.42f, 0.95f));
            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            bar.GetComponent<Image>().raycastTarget = false;
        }

        private static void StrengthenOutline(Text label, float distance)
        {
            Outline outline = label.GetComponent<Outline>();
            if (outline == null)
            {
                outline = label.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.06f, 0.03f, 0.02f, 0.98f);
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }
    }
}
