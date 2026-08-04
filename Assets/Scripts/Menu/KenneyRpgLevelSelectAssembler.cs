using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ChoTet.Menu
{
    [DefaultExecutionOrder(-200)]
    public class KenneyRpgLevelSelectAssembler : MonoBehaviour
    {
        [SerializeField] private KenneyRpgUiLibrary library;
        [SerializeField] private bool buildOnAwake = true;

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

        [ContextMenu("Rebuild Level Select Menu")]
        public void Build()
        {
            if (!EnsureLibrary())
            {
                return;
            }

            ClearPreviousBuild();

            MainMenuCameraSetup.EnsureMainCamera(transform);

            LevelSelectController controller = GetComponent<LevelSelectController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<LevelSelectController>();
            }

            GameObject canvas = KenneyRpgUiWidgets.CreateRootCanvas("LevelSelectCanvas");
            canvas.transform.SetParent(transform, false);

            KenneyRpgUiWidgets.CreateMenuPhotoBackground(canvas.transform, library);
            KenneyRpgUiWidgets.CreateVignetteOverlay(canvas.transform);
            BuildScreenUi(canvas.transform, controller, library);
            EnsureEventSystem();
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

            Debug.LogError("[KenneyRpgLevelSelectAssembler] UI library is not assigned.");
            return false;
        }

        private void BuildScreenUi(Transform canvas, LevelSelectController controller, KenneyRpgUiLibrary library)
        {
            GameObject titleBanner = KenneyRpgUiWidgets.CreateSpritePanel("LevelSelectBanner", canvas,
                library.panelBrown, KenneyRpgUiWidgets.TitleBoardTint);
            KenneyRpgUiWidgets.Layout(titleBanner.GetComponent<RectTransform>(),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -72f), new Vector2(720f, 108f));

            GameObject titleInset = KenneyRpgUiWidgets.CreateSpritePanel("BannerInset", titleBanner.transform,
                library.panelInsetBeigeLight, new Color(0.9f, 0.82f, 0.68f, 1f));
            KenneyRpgUiWidgets.Stretch(titleInset.GetComponent<RectTransform>(), 10f);

            Text bannerText = MainMenuTypography.CreateStyledText("BannerTitle", titleBanner.transform,
                MenuTextStyle.Title, TextAnchor.MiddleCenter, LevelSelectCopy.ScreenTitle);
            bannerText.fontSize = 48;
            KenneyRpgUiWidgets.Stretch(bannerText.rectTransform, 16f);

            LevelSelectLevelCard[] cards = BuildLevelCardGrid(canvas, library);
            BuildChromeButtons(canvas, controller, library, out Button backButton, out Button settingsButton,
                out Button infoButton, out Button characterButton);

            GameObject settingsPanel = BuildDialogPanel("SettingsOverlay", canvas, library,
                LevelSelectCopy.SettingsTitle, LevelSelectCopy.SettingsBody, controller.CloseSettings);
            GameObject infoPanel = BuildDialogPanel("InfoOverlay", canvas, library,
                LevelSelectCopy.InfoTitle, LevelSelectCopy.InfoBody, controller.CloseInfo);
            GameObject lockedPopup = BuildDialogPanel("LockedOverlay", canvas, library,
                LevelSelectCopy.LockedPopupTitle, LevelSelectCopy.LockedPopupBody, controller.CloseLockedPopup);

            Button lockedClose = lockedPopup.transform.Find("Dialog/CloseButton")?.GetComponent<Button>();

            controller.ConfigureLevelCards(cards);
            controller.ConfigurePopups(settingsPanel, infoPanel, lockedPopup);
            controller.ConfigureButtons(backButton, settingsButton, infoButton, characterButton, lockedClose);
        }

        private static LevelSelectLevelCard[] BuildLevelCardGrid(Transform canvas, KenneyRpgUiLibrary library)
        {
            const float cardWidth = 740f;
            const float cardHeight = 420f;
            const float gapX = 36f;
            float halfStepX = cardWidth * 0.5f + gapX * 0.5f;

            Vector2[] positions =
            {
                new Vector2(-halfStepX, 0f),
                new Vector2(halfStepX, 0f)
            };

            LevelSelectLevelCard[] cards = new LevelSelectLevelCard[LevelSelectCopy.Levels.Length];
            for (int i = 0; i < LevelSelectCopy.Levels.Length; i++)
            {
                LevelSelectLevelDefinition def = LevelSelectCopy.Levels[i];
                cards[i] = CreateLevelCard("LevelCard" + def.Index, canvas, library, def,
                    positions[i], new Vector2(cardWidth, cardHeight));
            }

            return cards;
        }

        private static LevelSelectLevelCard CreateLevelCard(string name, Transform canvas, KenneyRpgUiLibrary library,
            LevelSelectLevelDefinition definition, Vector2 position, Vector2 size)
        {
            Color cardTint = definition.IsUnlocked
                ? new Color(1.14f, 0.96f, 0.74f, 1f)
                : new Color(0.78f, 0.68f, 0.58f, 0.94f);

            GameObject cardObject = KenneyRpgUiWidgets.CreateSpritePanel(name, canvas, library.panelBrown, cardTint);
            KenneyRpgUiWidgets.Layout(cardObject.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

            Button button = cardObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = cardObject.GetComponent<Image>();
            MainMenuButtonFx buttonFx = cardObject.AddComponent<MainMenuButtonFx>();

            GameObject inset = KenneyRpgUiWidgets.CreateSpritePanel("Inset", cardObject.transform,
                library.panelInsetBeigeLight, new Color(0.92f, 0.84f, 0.7f, 1f));
            KenneyRpgUiWidgets.Stretch(inset.GetComponent<RectTransform>(), 10f);

            GameObject thumbArea = new GameObject("ThumbnailArea", typeof(RectTransform), typeof(RectMask2D));
            thumbArea.transform.SetParent(cardObject.transform, false);
            KenneyRpgUiWidgets.Layout(thumbArea.GetComponent<RectTransform>(),
                new Vector2(0f, 0.19f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(12f, 8f), new Vector2(-12f, -10f));

            LevelSelectThumbnailBuilder.Build(thumbArea.transform, definition, library);

            Text badge = MainMenuTypography.CreateStyledText("Badge", cardObject.transform, MenuTextStyle.Button,
                TextAnchor.MiddleCenter, definition.Title);
            LevelSelectCardVisualHelper.StyleTitleBadge(badge);
            KenneyRpgUiWidgets.Layout(badge.rectTransform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -18f), new Vector2(140f, 40f));

            Text title = MainMenuTypography.CreateStyledText("Subtitle", cardObject.transform, MenuTextStyle.Subtitle,
                TextAnchor.MiddleLeft, definition.Subtitle);
            LevelSelectCardVisualHelper.StyleSubtitle(title);
            KenneyRpgUiWidgets.Layout(title.rectTransform,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f),
                new Vector2(20f, 44f), new Vector2(-20f, 34f));

            string statusText = definition.IsUnlocked
                ? LevelSelectCopy.UnlockedStatus
                : LevelSelectCopy.LockedStatus;
            Text status = MainMenuTypography.CreateStyledText("Status", cardObject.transform, MenuTextStyle.IconButton,
                TextAnchor.MiddleLeft, statusText);
            LevelSelectCardVisualHelper.StyleStatus(status, definition.IsUnlocked);
            KenneyRpgUiWidgets.Layout(status.rectTransform,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f),
                new Vector2(20f, 16f), new Vector2(-20f, 28f));

            GameObject lockedOverlay = KenneyRpgUiWidgets.CreateSpritePanel("LockedOverlay", cardObject.transform,
                null, new Color(0.08f, 0.05f, 0.03f, definition.IsUnlocked ? 0f : 0.42f));
            lockedOverlay.GetComponent<Image>().raycastTarget = false;
            KenneyRpgUiWidgets.Stretch(lockedOverlay.GetComponent<RectTransform>());
            lockedOverlay.SetActive(!definition.IsUnlocked);

            if (!definition.IsUnlocked)
            {
                LevelSelectCardVisualHelper.BuildLockedOverlay(library, lockedOverlay);
            }

            if (definition.IsUnlocked)
            {
                LevelSelectCardVisualHelper.AddPlayableEmphasis(cardObject.transform, library, buttonFx);
            }

            LevelSelectLevelCard card = cardObject.AddComponent<LevelSelectLevelCard>();
            card.Configure(definition.Index, definition.IsUnlocked, button, lockedOverlay, badge, title, status, null);
            return card;
        }

        private static void BuildChromeButtons(Transform canvas, LevelSelectController controller,
            KenneyRpgUiLibrary library, out Button backButton, out Button settingsButton, out Button infoButton,
            out Button characterButton)
        {
            const float size = 56f;
            const float padding = 24f;

            backButton = KenneyRpgUiWidgets.CreateMenuButton("BackButton", canvas, library,
                LevelSelectCopy.BackButton, () => { });
            backButton.onClick.RemoveAllListeners();

            KenneyRpgUiWidgets.Layout(backButton.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(padding + 72f, -padding - 24f), new Vector2(180f, 52f));

            settingsButton = KenneyRpgUiWidgets.CreateIconButton("SettingsButton", canvas, library,
                LevelSelectCopy.SettingsButtonSymbol, () => { });
            settingsButton.onClick.RemoveAllListeners();

            KenneyRpgUiWidgets.Layout(settingsButton.GetComponent<RectTransform>(),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(-padding - size * 0.5f, -padding - size * 0.5f), new Vector2(size, size));

            infoButton = KenneyRpgUiWidgets.CreateIconButton("InfoButton", canvas, library,
                LevelSelectCopy.InfoButtonSymbol, () => { });
            infoButton.onClick.RemoveAllListeners();

            KenneyRpgUiWidgets.Layout(infoButton.GetComponent<RectTransform>(),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(-padding - size * 1.5f - 12f, -padding - size * 0.5f), new Vector2(size, size));

            characterButton = KenneyRpgUiWidgets.CreateMenuButton("CharacterButton", canvas, library,
                LevelSelectCopy.CharacterButtonLabel, () => { });
            characterButton.onClick.RemoveAllListeners();

            KenneyRpgUiWidgets.Layout(characterButton.GetComponent<RectTransform>(),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(padding + 92f, padding + 26f), new Vector2(200f, 52f));

            if (library.vineLeaf != null)
            {
                GameObject avatar = KenneyRpgUiWidgets.CreateSpritePanel("CharacterAvatar", characterButton.transform,
                    library.vineLeaf, KenneyRpgUiWidgets.TitleGold);
                avatar.GetComponent<Image>().preserveAspect = true;
                avatar.GetComponent<Image>().raycastTarget = false;
                KenneyRpgUiWidgets.Layout(avatar.GetComponent<RectTransform>(),
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(18f, 0f), new Vector2(28f, 28f));
            }
        }

        private static GameObject BuildDialogPanel(string overlayName, Transform canvas, KenneyRpgUiLibrary library,
            string titleText, string bodyText, UnityEngine.Events.UnityAction closeAction)
        {
            GameObject overlay = KenneyRpgUiWidgets.CreateSpritePanel(overlayName, canvas, null,
                KenneyRpgUiWidgets.DialogOverlay);
            KenneyRpgUiWidgets.Stretch(overlay.GetComponent<RectTransform>());
            overlay.GetComponent<Image>().raycastTarget = true;

            GameObject dialog = KenneyRpgUiWidgets.CreateSpritePanel("Dialog", overlay.transform,
                library.panelBrown, KenneyRpgUiWidgets.TitleBoardTint);
            KenneyRpgUiWidgets.Layout(dialog.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(760f, 460f));

            GameObject inset = KenneyRpgUiWidgets.CreateSpritePanel("DialogInset", dialog.transform,
                library.panelInsetBrown, new Color(0.88f, 0.78f, 0.62f, 1f));
            KenneyRpgUiWidgets.Stretch(inset.GetComponent<RectTransform>(), 12f);

            Text title = MainMenuTypography.CreateStyledText("Title", dialog.transform, MenuTextStyle.DialogTitle,
                TextAnchor.UpperLeft, titleText);
            KenneyRpgUiWidgets.Layout(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, -28f), new Vector2(680f, 52f));

            Text body = MainMenuTypography.CreateStyledText("Body", dialog.transform, MenuTextStyle.DialogBody,
                TextAnchor.UpperLeft, bodyText);
            KenneyRpgUiWidgets.Layout(body.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, -88f), new Vector2(680f, 250f));

            Button close = KenneyRpgUiWidgets.CreateMenuButton("CloseButton", dialog.transform, library,
                LevelSelectCopy.CloseButton, closeAction);
            KenneyRpgUiWidgets.Layout(close.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 42f), new Vector2(280f, 56f));

            overlay.SetActive(false);
            return overlay;
        }

        private void ClearPreviousBuild()
        {
            DestroyChild("MenuTableDecor");
            DestroyChild("LevelSelectCanvas");
            DestroyChild("Main Camera");
            DestroyChild("Sunset Light");
        }

        private void DestroyChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void DestroyObjectSafe(Object target)
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
                DestroyObjectSafe(legacyModule);
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

    public class LevelSelectLevelCard : MonoBehaviour
    {
        [SerializeField] private int levelIndex;
        [SerializeField] private bool isUnlocked;
        [SerializeField] private Button cardButton;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Text badgeLabel;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private RawImage thumbnailImage;

        public int LevelIndex => levelIndex;
        public bool IsUnlocked => isUnlocked;

        public void Configure(int index, bool unlocked, Button button, GameObject overlay, Text badge, Text title,
            Text status, RawImage thumbnail)
        {
            levelIndex = index;
            isUnlocked = unlocked;
            cardButton = button;
            lockedOverlay = overlay;
            badgeLabel = badge;
            titleLabel = title;
            statusLabel = status;
            thumbnailImage = thumbnail;
        }

        public void Bind(LevelSelectController controller)
        {
            if (cardButton == null || controller == null)
            {
                return;
            }

            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => controller.OnLevelCardClicked(this));
        }
    }

    public class LevelSelectController : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [Tooltip("TODO: Replace with the dedicated Level 2 scene name when the teammate scene is finalized in the repo.")]
        [SerializeField] private string level1SceneName = "Level1_PackingRoom";
        [SerializeField] private string level2SceneName = "ChoTet_VerticalSlice";

        [Header("Level Cards")]
        [SerializeField] private LevelSelectLevelCard[] levelCards;

        [Header("Popups")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private GameObject lockedPopupPanel;

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button infoButton;
        [SerializeField] private Button characterButton;
        [SerializeField] private Button lockedPopupCloseButton;

        public void ConfigureLevelCards(LevelSelectLevelCard[] cards)
        {
            levelCards = cards;
            if (levelCards == null)
            {
                return;
            }

            for (int i = 0; i < levelCards.Length; i++)
            {
                if (levelCards[i] != null)
                {
                    levelCards[i].Bind(this);
                }
            }
        }

        public void ConfigurePopups(GameObject settings, GameObject info, GameObject lockedPopup)
        {
            settingsPanel = settings;
            infoPanel = info;
            lockedPopupPanel = lockedPopup;
        }

        public void ConfigureButtons(Button back, Button settings, Button info, Button character, Button lockedClose)
        {
            backButton = back;
            settingsButton = settings;
            infoButton = info;
            characterButton = character;
            lockedPopupCloseButton = lockedClose;
        }

        private void Start()
        {
            MainMenuAudioSettings.LoadSavedVolume();
            SceneBackgroundMusic.EnsurePlaying();
            HideAllPopups();
            WireButtons();
        }

        private void WireButtons()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OpenSettings);
            }

            if (infoButton != null)
            {
                infoButton.onClick.AddListener(OpenInfo);
            }

            if (characterButton != null)
            {
                characterButton.onClick.AddListener(OpenCharacterSelect);
            }
        }

        public void OnLevelCardClicked(LevelSelectLevelCard card)
        {
            if (card == null)
            {
                return;
            }

            if (!card.IsUnlocked)
            {
                ShowLockedPopup();
                return;
            }

            if (card.LevelIndex == 1)
            {
                LoadLevel1Scene();
                return;
            }

            if (card.LevelIndex == 2)
            {
                LoadLevel2Scene();
            }
        }

        public void ReturnToMainMenu()
        {
            LoadSceneSafely(mainMenuSceneName, "Main Menu");
        }

        public void OpenSettings()
        {
            CloseInfo();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void OpenInfo()
        {
            CloseSettings();
            if (infoPanel != null)
            {
                infoPanel.SetActive(true);
            }
        }

        public void CloseInfo()
        {
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        public void OpenCharacterSelect()
        {
            Debug.Log("[LevelSelectController] Open character select (placeholder).");
        }

        public void CloseLockedPopup()
        {
            if (lockedPopupPanel != null)
            {
                lockedPopupPanel.SetActive(false);
            }
        }

        private void ShowLockedPopup()
        {
            if (lockedPopupPanel != null)
            {
                lockedPopupPanel.SetActive(true);
            }
        }

        private void HideAllPopups()
        {
            CloseSettings();
            CloseInfo();
            CloseLockedPopup();
        }

        private void LoadLevel1Scene()
        {
            LoadSceneSafely(level1SceneName, "Level 1");
        }

        private void LoadLevel2Scene()
        {
            LoadSceneSafely(level2SceneName, "Level 2");
        }

        private static void LoadSceneSafely(string sceneName, string label)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[LevelSelectController] Missing scene name for " + label + ".");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("[LevelSelectController] Scene '" + sceneName + "' for " + label +
                               " is missing or not enabled in Build Settings.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
