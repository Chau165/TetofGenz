using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Builds the main menu: warm wood backdrop, Kenney wooden UI overlay, and top-right audio controls.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class KenneyRpgMenuAssembler : MonoBehaviour
    {
        [SerializeField] private KenneyRpgUiLibrary library;
        [SerializeField] private bool buildOnAwake = true;
        [SerializeField] private bool buildDioramaBackdrop;

        private bool assembled;

        private void Awake()
        {
            if (!buildOnAwake || assembled)
            {
                return;
            }

            if (!EnsureLibrary())
            {
                return;
            }

            Build();
            assembled = true;
        }

        private bool EnsureLibrary()
        {
            if (library == null)
            {
                library = KenneyRpgUiLibrary.LoadDefault();
            }

            if (library != null)
            {
                return true;
            }

            Debug.LogError("[KenneyRpgMenuAssembler] UI library is not assigned. " +
                           "Run Tools → Cho Tet → Setup Kenney RPG Menu Assets, then rebuild the scene.");
            return false;
        }

        [ContextMenu("Rebuild Main Menu")]
        public void Build()
        {
            if (!EnsureLibrary())
            {
                return;
            }

            ClearPreviousBuild();

            MainMenuCameraSetup.EnsureMainCamera(transform);

            if (buildDioramaBackdrop)
            {
                MainMenuDioramaBuilder.Build(transform);
            }

            MainMenuController controller = GetComponent<MainMenuController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<MainMenuController>();
            }

            GameObject canvas = KenneyRpgUiWidgets.CreateRootCanvas("MenuCanvas");
            canvas.transform.SetParent(transform, false);

            KenneyRpgUiWidgets.CreateMenuPhotoBackground(canvas.transform, library);
            KenneyRpgUiWidgets.CreateVignetteOverlay(canvas.transform);

            BuildMainMenuUi(canvas.transform, controller, library);
            BuildTopRightControls(canvas.transform, controller, library);

            GameObject howToPlay = BuildHowToPlay(canvas.transform, controller, library);
            controller.ConfigureHowToPlayPanel(howToPlay);

            GameObject settings = BuildSettingsPanel(canvas.transform, controller, library);
            controller.ConfigureSettingsPanel(settings);

            EnsureEventSystem();
        }

        private void BuildMainMenuUi(Transform canvas, MainMenuController controller, KenneyRpgUiLibrary library)
        {
            GameObject titleBoard = KenneyRpgUiWidgets.CreateTetTitleBannerCluster(canvas, library, transform);

            Text titleLine1 = MainMenuTypography.CreateStyledText("TitleLine1", titleBoard.transform,
                MenuTextStyle.Title, TextAnchor.UpperCenter, KenneyRpgMenuCopy.GameTitleLine1);
            KenneyRpgUiWidgets.Layout(titleLine1.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(660f, 92f));

            Text titleLine2 = MainMenuTypography.CreateStyledText("TitleLine2", titleBoard.transform,
                MenuTextStyle.Title, TextAnchor.UpperCenter, KenneyRpgMenuCopy.GameTitleLine2);
            KenneyRpgUiWidgets.Layout(titleLine2.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -128f), new Vector2(660f, 92f));

            Text subtitle = MainMenuTypography.CreateStyledText("Subtitle", titleBoard.transform,
                MenuTextStyle.Subtitle, TextAnchor.UpperCenter, KenneyRpgMenuCopy.GameSubtitle);
            KenneyRpgUiWidgets.Layout(subtitle.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -218f), new Vector2(600f, 48f));

            const float buttonWidth = 390f;
            const float buttonHeight = 68f;
            const float buttonGap = 28f;
            const float buttonSpacing = buttonHeight + buttonGap;
            float buttonY = -40f;

            CreateMenuButton(canvas, library, KenneyRpgMenuCopy.PlayButton,
                new Vector2(0f, buttonY), new Vector2(buttonWidth, buttonHeight), controller.StartJourney);
            buttonY -= buttonSpacing;
            CreateMenuButton(canvas, library, KenneyRpgMenuCopy.HowToPlayButton,
                new Vector2(0f, buttonY), new Vector2(buttonWidth, buttonHeight), controller.OpenHowToPlay);
            buttonY -= buttonSpacing;
            CreateMenuButton(canvas, library, KenneyRpgMenuCopy.QuitButton,
                new Vector2(0f, buttonY), new Vector2(buttonWidth, buttonHeight), controller.QuitGame);
        }

        private static void BuildTopRightControls(Transform canvas, MainMenuController controller,
            KenneyRpgUiLibrary library)
        {
            const float size = 56f;
            const float padding = 24f;
            const float spacing = 12f;

            float x = -padding - size * 0.5f;
            const float y = -padding - size * 0.5f;

            CreateIconButton(canvas, library, "SettingsButton", KenneyRpgMenuCopy.SettingsButtonSymbol,
                new Vector2(x, y), new Vector2(size, size), controller.OpenSettings);
            x -= size + spacing;

            KenneyRpgUiWidgets.CreateVolumeControl(canvas, library, new Vector2(x, y), size);
        }

        private static void CreateMenuButton(Transform canvas, KenneyRpgUiLibrary library, string label,
            Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            Button button = KenneyRpgUiWidgets.CreateMenuButton(label.Replace(" ", string.Empty) + "Button",
                canvas, library, label, action);
            KenneyRpgUiWidgets.Layout(button.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        }

        private static void CreateIconButton(Transform canvas, KenneyRpgUiLibrary library, string name, string symbol,
            Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            Button button = KenneyRpgUiWidgets.CreateIconButton(name, canvas, library, symbol, action);
            KenneyRpgUiWidgets.Layout(button.GetComponent<RectTransform>(),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), position, size);
        }

        private static GameObject BuildHowToPlay(Transform canvas, MainMenuController controller,
            KenneyRpgUiLibrary library)
        {
            return BuildDialogPanel("HowToPlayOverlay", canvas, controller, library,
                KenneyRpgMenuCopy.HowToPlayTitle, KenneyRpgMenuCopy.HowToPlayBody, controller.CloseHowToPlay);
        }

        private static GameObject BuildSettingsPanel(Transform canvas, MainMenuController controller,
            KenneyRpgUiLibrary library)
        {
            return BuildDialogPanel("SettingsOverlay", canvas, controller, library,
                KenneyRpgMenuCopy.SettingsTitle, KenneyRpgMenuCopy.SettingsBody, controller.CloseSettings);
        }

        private static GameObject BuildDialogPanel(string overlayName, Transform canvas, MainMenuController controller,
            KenneyRpgUiLibrary library, string titleText, string bodyText, UnityEngine.Events.UnityAction closeAction)
        {
            GameObject overlay = KenneyRpgUiWidgets.CreateSpritePanel(overlayName, canvas, null,
                KenneyRpgUiWidgets.DialogOverlay);
            KenneyRpgUiWidgets.Stretch(overlay.GetComponent<RectTransform>());
            overlay.GetComponent<Image>().raycastTarget = true;

            GameObject dialog = KenneyRpgUiWidgets.CreateSpritePanel("Dialog", overlay.transform,
                library.panelBrown, KenneyRpgUiWidgets.TitleBoardTint);
            KenneyRpgUiWidgets.Layout(dialog.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(860f, 520f));

            GameObject inset = KenneyRpgUiWidgets.CreateSpritePanel("DialogInset", dialog.transform,
                library.panelInsetBrown, new Color(0.88f, 0.78f, 0.62f, 1f));
            KenneyRpgUiWidgets.Stretch(inset.GetComponent<RectTransform>(), 12f);

            Text title = MainMenuTypography.CreateStyledText("Title", dialog.transform, MenuTextStyle.DialogTitle,
                TextAnchor.UpperLeft, titleText);
            KenneyRpgUiWidgets.Layout(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, -28f), new Vector2(780f, 52f));

            Text body = MainMenuTypography.CreateStyledText("Body", dialog.transform, MenuTextStyle.DialogBody,
                TextAnchor.UpperLeft, bodyText);
            KenneyRpgUiWidgets.Layout(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, -88f), new Vector2(780f, 300f));

            Button close = KenneyRpgUiWidgets.CreateMenuButton("CloseButton", dialog.transform, library,
                KenneyRpgMenuCopy.CloseButton, closeAction);
            KenneyRpgUiWidgets.Layout(close.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 42f), new Vector2(320f, 58f));

            overlay.SetActive(false);
            return overlay;
        }

        private void ClearPreviousBuild()
        {
            Transform menuWorld = transform.Find("MenuWorld");
            if (menuWorld != null)
            {
                DestroyImmediateSafe(menuWorld.gameObject);
            }

            Transform menuTableDecor = transform.Find("MenuTableDecor");
            if (menuTableDecor != null)
            {
                DestroyImmediateSafe(menuTableDecor.gameObject);
            }

            Transform bannerDecor = transform.Find("BannerDecorationRoot");
            if (bannerDecor != null)
            {
                DestroyImmediateSafe(bannerDecor.gameObject);
            }

            Transform menuBannerVineDecor = transform.Find("MenuBannerVineDecor");
            if (menuBannerVineDecor != null)
            {
                DestroyImmediateSafe(menuBannerVineDecor.gameObject);
            }

            Transform menuTitleBannerDecor = transform.Find("MenuTitleBannerDecor");
            if (menuTitleBannerDecor != null)
            {
                DestroyImmediateSafe(menuTitleBannerDecor.gameObject);
            }

            Transform menuCanvas = transform.Find("MenuCanvas");
            if (menuCanvas != null)
            {
                DestroyImmediateSafe(menuCanvas.gameObject);
            }

            Transform mainCamera = transform.Find("Main Camera");
            if (mainCamera != null)
            {
                DestroyImmediateSafe(mainCamera.gameObject);
            }

            Transform sunsetLight = transform.Find("Sunset Light");
            if (sunsetLight != null)
            {
                DestroyImmediateSafe(sunsetLight.gameObject);
            }
        }

        private static void DestroyImmediateSafe(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }

        private static void EnsureEventSystem()
        {
            EventSystem existing = Object.FindAnyObjectByType<EventSystem>();
            if (existing != null)
            {
                AttachInputModule(existing.gameObject);
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            AttachInputModule(eventSystem);
        }

        private static void AttachInputModule(GameObject eventSystemObject)
        {
#if ENABLE_INPUT_SYSTEM
            if (eventSystemObject.GetComponent<InputSystemUIInputModule>() != null)
            {
                return;
            }

            StandaloneInputModule legacyModule = eventSystemObject.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                DestroyImmediateSafe(legacyModule);
            }

            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}
