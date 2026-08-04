using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Builds uGUI widgets from Kenney RPG sprites with 9-slice and consistent wooden styling.
    /// </summary>
    public static class KenneyRpgUiWidgets
    {
        public static readonly Color TitleGold = new Color(0.96f, 0.86f, 0.62f, 1f);
        public static readonly Color ButtonText = new Color(0.95f, 0.9f, 0.78f, 1f);
        public static readonly Color SubtitleText = new Color(0.82f, 0.74f, 0.6f, 0.92f);
        public static readonly Color BodyText = new Color(0.9f, 0.84f, 0.74f, 1f);
        public static readonly Color DialogOverlay = new Color(0.04f, 0.03f, 0.02f, 0.55f);
        public static readonly Color TitleBoardTint = new Color(0.74f, 0.6f, 0.46f, 1f);
        public static readonly Color TitleBoardWarmTint = new Color(0.82f, 0.66f, 0.48f, 1f);
        public static readonly Color TetRedAccent = new Color(0.88f, 0.24f, 0.18f, 1f);
        public static readonly Color ButtonWarmOrangeBrown = new Color(1.04f, 0.82f, 0.6f, 1f);

        private static Texture2D vignetteTexture;

        public static GameObject CreateRootCanvas(string name)
        {
            GameObject canvasObject = new GameObject(name);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        public static GameObject CreateWoodBackground(Transform parent, KenneyRpgUiLibrary library)
        {
            GameObject panel = CreatePhotoBackgroundPanel(parent, ResolveWoodPlankBackground(library), "WoodBackground");
            if (panel != null)
            {
                return panel;
            }

            return CreateSolidPanelFallback(parent, library, "WoodBackground");
        }

        /// <summary>Full-screen Tet village artwork (object-fit cover).</summary>
        public static GameObject CreateMenuPhotoBackground(Transform parent, KenneyRpgUiLibrary library)
        {
            GameObject panel = CreatePhotoBackgroundPanel(parent, ResolveMenuVillageBackground(library),
                "MenuPhotoBackground");
            if (panel != null)
            {
                return panel;
            }

            return CreateWoodBackground(parent, library);
        }

        private static GameObject CreatePhotoBackgroundPanel(Transform parent, Texture2D backgroundTexture,
            string objectName)
        {
            if (backgroundTexture == null)
            {
                return null;
            }

            GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(RawImage),
                typeof(MainMenuBackgroundCover));
            panel.transform.SetParent(parent, false);

            RawImage rawImage = panel.GetComponent<RawImage>();
            rawImage.texture = backgroundTexture;
            rawImage.raycastTarget = false;
            rawImage.color = Color.white;

            Stretch(panel.GetComponent<RectTransform>());
            panel.transform.SetAsFirstSibling();
            panel.GetComponent<MainMenuBackgroundCover>().ApplyCover();
            return panel;
        }

        private static GameObject CreateSolidPanelFallback(Transform parent, KenneyRpgUiLibrary library, string name)
        {
            Sprite fallbackSprite = library != null ? library.panelBrown : null;
            GameObject fallback = CreateSpritePanel(name, parent, fallbackSprite,
                new Color(0.55f, 0.38f, 0.24f, 1f));
            Image image = fallback.GetComponent<Image>();
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            Stretch(fallback.GetComponent<RectTransform>());
            fallback.transform.SetAsFirstSibling();
            return fallback;
        }

        private static Texture2D ResolveMenuVillageBackground(KenneyRpgUiLibrary library)
        {
            if (library != null && library.menuVillageBackground != null)
            {
                return library.menuVillageBackground;
            }

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(KenneyRpgUiPaths.MenuVillageBackground);
#else
            return null;
#endif
        }

        private static Texture2D ResolveWoodPlankBackground(KenneyRpgUiLibrary library)
        {
            if (library != null && library.woodPlankBackground != null)
            {
                return library.woodPlankBackground;
            }

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(KenneyRpgUiPaths.WoodPlankBackground);
#else
            return null;
#endif
        }

        public static GameObject CreateTableDecorOverlay(Transform parent, RenderTexture renderTexture)
        {
            GameObject panel = new GameObject("TableDecorOverlay", typeof(RectTransform), typeof(RawImage));
            panel.transform.SetParent(parent, false);

            RawImage rawImage = panel.GetComponent<RawImage>();
            rawImage.texture = renderTexture;
            rawImage.raycastTarget = false;
            rawImage.color = Color.white;

            Stretch(panel.GetComponent<RectTransform>());
            return panel;
        }

        public static GameObject CreateVignetteOverlay(Transform parent)
        {
            GameObject panel = new GameObject("VignetteOverlay", typeof(RectTransform), typeof(RawImage));
            panel.transform.SetParent(parent, false);

            RawImage rawImage = panel.GetComponent<RawImage>();
            rawImage.texture = GetVignetteTexture();
            rawImage.raycastTarget = false;
            rawImage.color = Color.white;

            Stretch(panel.GetComponent<RectTransform>());
            return panel;
        }

        public static GameObject CreateTitleBoard(Transform parent, KenneyRpgUiLibrary library, Transform sceneRoot)
        {
            return CreateTetTitleBannerCluster(parent, library, sceneRoot);
        }

        /// <summary>
        /// Title banner: wooden frame with organic vine growth and title on top.
        /// </summary>
        public static GameObject CreateTetTitleBannerCluster(Transform parent, KenneyRpgUiLibrary library,
            Transform sceneRoot)
        {
            GameObject cluster = new GameObject("TitleBannerCluster", typeof(RectTransform));
            cluster.transform.SetParent(parent, false);
            Layout(cluster.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 220f), new Vector2(820f, 340f));

            MainMenuBannerUiFxBuilder.BuildGlow(cluster.transform, library);

            GameObject shadow = CreateSpritePanel("TitleFrameShadow", cluster.transform, library.panelInsetBrown,
                new Color(0.18f, 0.1f, 0.06f, 0.42f));
            Layout(shadow.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -10f), new Vector2(760f, 292f));
            shadow.GetComponent<Image>().raycastTarget = false;

            GameObject outerFrame = CreateSpritePanel("TitleOuterFrame", cluster.transform, library.panelBrown,
                new Color(0.62f, 0.48f, 0.34f, 1f));
            Layout(outerFrame.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(776f, 296f));
            outerFrame.GetComponent<Image>().raycastTarget = false;

            GameObject board = CreateSpritePanel("TitleBoard", cluster.transform, library.panelBrown, TitleBoardWarmTint);
            Layout(board.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(760f, 280f));

            // 3D decor RT overlays disabled — they leaked white edge pixels past the frame (maskable was off).
            MainMenuBannerUiVineBuilder.BuildBack(board.transform, library);

            CreateFrameBar("FrameTop", board.transform, library, new Vector2(0.5f, 1f), new Vector2(0f, -4f),
                library.barYellowMid, new Color(0.98f, 0.88f, 0.58f, 1f));
            CreateFrameBar("FrameBottom", board.transform, library, new Vector2(0.5f, 0f), new Vector2(0f, 4f),
                library.barYellowMid, new Color(0.96f, 0.82f, 0.52f, 1f));
            SetFrameBarSimple(board.transform.Find("FrameTop"));
            SetFrameBarSimple(board.transform.Find("FrameBottom"));
            CreateCornerCap("CornerTopLeft", board.transform, library, new Vector2(0f, 1f), new Vector2(16f, -16f));
            CreateCornerCap("CornerTopRight", board.transform, library, new Vector2(1f, 1f), new Vector2(-16f, -16f));
            CreateCornerCap("CornerBottomLeft", board.transform, library, new Vector2(0f, 0f), new Vector2(16f, 16f));
            CreateCornerCap("CornerBottomRight", board.transform, library, new Vector2(1f, 0f), new Vector2(-16f, 16f));

            MainMenuBannerUiVineBuilder.BuildFront(board.transform, library);
            MainMenuBannerCornerFestoon.Build(board.transform, library);

            GameObject inset = CreateSpritePanel("TitleInset", board.transform,
                library.panelInsetBeigeLight != null ? library.panelInsetBeigeLight : library.panelInsetBrown,
                new Color(0.94f, 0.84f, 0.68f, 1f));
            Stretch(inset.GetComponent<RectTransform>(), 22f);

            MainMenuBannerUiFxBuilder.BuildOverlays(cluster.transform, library);

            board.AddComponent<RectMask2D>();

            return board;
        }

        public static GameObject CreateBannerDecorOverlay(string name, Transform parent, RenderTexture renderTexture,
            bool bringToFront)
        {
            return CreateBannerVineOverlay(name, parent, renderTexture, bringToFront);
        }

        public static GameObject CreateBannerVineOverlay(string name, Transform parent, RenderTexture renderTexture,
            bool bringToFront)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            panel.transform.SetParent(parent, false);

            RawImage rawImage = panel.GetComponent<RawImage>();
            rawImage.texture = renderTexture;
            rawImage.raycastTarget = false;
            rawImage.color = new Color(1f, 1f, 1f, 0.72f);
            rawImage.maskable = false;

            Stretch(panel.GetComponent<RectTransform>());
            if (bringToFront)
            {
                panel.transform.SetAsLastSibling();
            }

            return panel;
        }

        public static GameObject CreateSpritePanel(string name, Transform parent, Sprite sprite, Color tint)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = tint;
            image.raycastTarget = sprite != null;
            return panel;
        }

        public static Button CreateMenuButton(string name, Transform parent, KenneyRpgUiLibrary library,
            string label, UnityAction onClick)
        {
            GameObject buttonObject = CreateSpritePanel(name, parent, library.buttonLongBrown, ButtonWarmOrangeBrown);
            Button button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = buttonObject.GetComponent<Image>();
            button.onClick.AddListener(onClick);
            buttonObject.AddComponent<MainMenuButtonFx>();

            Text text = MainMenuTypography.CreateStyledText("Label", buttonObject.transform, MenuTextStyle.Button,
                TextAnchor.MiddleCenter, label);
            Stretch(text.rectTransform, 8f);
            text.raycastTarget = false;
            return button;
        }

        public static Button CreateIconButton(string name, Transform parent, KenneyRpgUiLibrary library,
            string symbol, UnityAction onClick)
        {
            Sprite sprite = library.buttonSquareBrown != null ? library.buttonSquareBrown : library.buttonRoundBrown;
            GameObject buttonObject = CreateSpritePanel(name, parent, sprite, ButtonWarmOrangeBrown);
            Button button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = buttonObject.GetComponent<Image>();
            button.onClick.AddListener(onClick);
            buttonObject.AddComponent<MainMenuButtonFx>();

            Text text = MainMenuTypography.CreateStyledText("Symbol", buttonObject.transform, MenuTextStyle.IconButton,
                TextAnchor.MiddleCenter, symbol);
            Stretch(text.rectTransform, 4f);
            text.raycastTarget = false;
            return button;
        }

        public static MainMenuVolumeControl CreateVolumeControl(Transform parent, KenneyRpgUiLibrary library,
            Vector2 anchoredPosition, float buttonSize)
        {
            const float sliderWidth = 260f;
            const float sliderHeight = 48f;
            const float gap = 8f;

            GameObject root = new GameObject("VolumeControl", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Layout(root.GetComponent<RectTransform>(),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                anchoredPosition, Vector2.zero);

            Button toggleButton = CreateIconButton("VolumeButton", root.transform, library,
                KenneyRpgMenuCopy.VolumeButtonSymbol, () => { });
            toggleButton.onClick.RemoveAllListeners();
            Layout(toggleButton.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(buttonSize, buttonSize));

            GameObject sliderPanel = CreateSpritePanel("VolumeSliderPanel", root.transform, library.panelInsetBrown,
                TitleBoardTint);
            sliderPanel.GetComponent<Image>().raycastTarget = true;
            Layout(sliderPanel.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1f, 1f),
                new Vector2(buttonSize * 0.5f, -buttonSize * 0.5f - gap),
                new Vector2(sliderWidth, sliderHeight));

            Text percentLabel = MainMenuTypography.CreateStyledText("VolumePercent", sliderPanel.transform,
                MenuTextStyle.IconButton, TextAnchor.MiddleCenter, "70%");
            percentLabel.raycastTarget = false;
            Layout(percentLabel.rectTransform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-32f, 0f), new Vector2(52f, 32f));

            Slider slider = CreateStyledVolumeSlider(sliderPanel.transform, library);

            MainMenuVolumeControl control = root.AddComponent<MainMenuVolumeControl>();
            control.Initialize(toggleButton, sliderPanel, slider, percentLabel);
            return control;
        }

        private static Slider CreateStyledVolumeSlider(Transform parent, KenneyRpgUiLibrary library)
        {
            GameObject sliderObject = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(-16f, 0f);
            sliderRect.sizeDelta = new Vector2(-72f, 24f);

            GameObject background = CreateSpritePanel("Background", sliderObject.transform, library.panelInsetBrown,
                new Color(0.55f, 0.42f, 0.3f, 0.85f));
            background.GetComponent<Image>().raycastTarget = true;
            Stretch(background.GetComponent<RectTransform>());

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(6f, 0f);
            fillAreaRect.offsetMax = new Vector2(-6f, 0f);

            GameObject fill = CreateSpritePanel("Fill", fillArea.transform, library.barYellowMid, TitleGold);
            fill.GetComponent<Image>().raycastTarget = false;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject handleSlideArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleSlideArea.transform.SetParent(sliderObject.transform, false);
            Stretch(handleSlideArea.GetComponent<RectTransform>(), 4f);

            GameObject handle = CreateSpritePanel("Handle", handleSlideArea.transform, library.buttonRoundBrown,
                ButtonWarmOrangeBrown);
            Image handleImage = handle.GetComponent<Image>();
            handleImage.raycastTarget = true;
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        public static void Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        public static void Layout(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void CreateFrameBar(string name, Transform parent, KenneyRpgUiLibrary library,
            Vector2 anchorY, Vector2 offset, Sprite sprite, Color tint)
        {
            if (sprite == null)
            {
                return;
            }

            GameObject bar = CreateSpritePanel(name, parent, sprite, tint);
            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, anchorY.y);
            rect.anchorMax = new Vector2(1f, anchorY.y);
            rect.pivot = new Vector2(0.5f, anchorY.y);
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(-48f, 16f);
            bar.GetComponent<Image>().raycastTarget = false;
        }

        private static void SetFrameBarSimple(Transform barTransform)
        {
            if (barTransform == null)
            {
                return;
            }

            Image image = barTransform.GetComponent<Image>();
            if (image != null)
            {
                image.type = Image.Type.Simple;
            }
        }

        private static void CreateVerticalFrameBar(string name, Transform parent, KenneyRpgUiLibrary library, bool left)
        {
            Sprite sprite = library.barYellowMid != null ? library.barYellowMid : library.barYellowVertical;
            if (sprite == null)
            {
                return;
            }

            GameObject bar = CreateSpritePanel(name, parent, sprite, new Color(0.98f, 0.9f, 0.72f, 1f));
            Image image = bar.GetComponent<Image>();
            image.type = Image.Type.Simple;
            image.preserveAspect = false;

            RectTransform rect = bar.GetComponent<RectTransform>();
            float anchorX = left ? 0f : 1f;
            rect.anchorMin = new Vector2(anchorX, 0f);
            rect.anchorMax = new Vector2(anchorX, 1f);
            rect.pivot = new Vector2(anchorX, 0.5f);
            rect.anchoredPosition = new Vector2(left ? 8f : -8f, 0f);
            rect.sizeDelta = new Vector2(10f, -24f);
            bar.GetComponent<Image>().raycastTarget = false;
        }

        private static void CreateCornerCap(string name, Transform parent, KenneyRpgUiLibrary library,
            Vector2 anchor, Vector2 offset)
        {
            Sprite sprite = library.buttonSquareBrown != null ? library.buttonSquareBrown : library.buttonRoundBrown;
            if (sprite == null)
            {
                return;
            }

            GameObject cap = CreateSpritePanel(name, parent, sprite, new Color(0.92f, 0.72f, 0.48f, 1f));
            RectTransform rect = cap.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(28f, 28f);
            cap.GetComponent<Image>().raycastTarget = false;
        }

        private static void CreateCornerStud(Transform parent, KenneyRpgUiLibrary library, string name,
            Vector2 anchor, Vector2 offset)
        {
            Sprite sprite = library.barYellowMid;
            if (sprite == null)
            {
                return;
            }

            GameObject stud = CreateSpritePanel(name, parent, sprite, new Color(0.98f, 0.84f, 0.38f, 0.95f));
            Image image = stud.GetComponent<Image>();
            image.type = Image.Type.Simple;

            RectTransform rect = stud.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(12f, 12f);
            image.raycastTarget = false;
        }

        private static void CreateFrameBar(string name, Transform parent, KenneyRpgUiLibrary library,
            Vector2 anchorY, Vector2 offset)
        {
            CreateFrameBar(name, parent, library, anchorY, offset, library.barYellowMid, Color.white);
        }

        private static Texture2D GetVignetteTexture()
        {
            if (vignetteTexture != null)
            {
                return vignetteTexture;
            }

            const int size = 256;
            vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };

            Vector2 center = new Vector2(0.5f, 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x / (size - 1f) - center.x) * 2f;
                    float dy = (y / (size - 1f) - center.y) * 2f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((distance - 0.35f) / 0.65f) * 0.28f;
                    vignetteTexture.SetPixel(x, y, new Color(0.04f, 0.025f, 0.015f, alpha));
                }
            }

            vignetteTexture.Apply();
            return vignetteTexture;
        }
    }
}
