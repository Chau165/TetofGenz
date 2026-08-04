using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ChoTet.Runtime
{
    public class GameHudController : MonoBehaviour
    {
        public bool IsModalOpen
        {
            get
            {
                EnsureUiBuilt();
                return dialogPanel.activeSelf || bargainPanel.activeSelf || choicePanel.activeSelf || evaluationPanel.activeSelf;
            }
        }

        private Canvas rootCanvas;
        private GameObject hudPanel;
        private GameObject promptPanel;
        private GameObject dialogPanel;
        private GameObject dialogPortraitPanel;
        private GameObject bargainPanel;
        private GameObject choicePanel;
        private GameObject evaluationPanel;
        private Text statusText;
        private Text shoppingListText;
        private Text promptText;
        private Text dialogTitleText;
        private Text dialogBodyText;
        private Text dialogPortraitText;
        private Text dialogContinueText;
        private Text bargainTitleText;
        private Text bargainBodyText;
        private Text bargainOptionsText;
        private Text bargainQuoteText;
        private Text bargainFeedbackText;
        private Text choiceTitleText;
        private Text choiceBodyText;
        private Text choiceOptionsText;
        private Text choiceHintText;
        private Text dialogHintText;
        private Text evaluationTitleText;
        private Text evaluationBodyText;
        private Text evaluationHintText;
        private GameObject hudDivider;
        private Image dialogPortraitImage;
        private Button dialogContinueButton;

        // Retro Chocolate dialogue palette (matches Hometown Arrival VN box).
        private static readonly Color ChocolatePanel = new Color(0.4f, 0.25f, 0.17f, 0.98f);
        private static readonly Color ChocolateOuter = new Color(0.22f, 0.13f, 0.08f, 1f);
        private static readonly Color ChocolateInner = new Color(0.46f, 0.3f, 0.2f, 1f);
        private static readonly Color ChocolateCream = new Color(0.93f, 0.85f, 0.72f, 1f);
        private static readonly Color ChocolateNamePlate = new Color(0.74f, 0.48f, 0.32f, 1f);
        private static readonly Color ChocolateBodyText = new Color(1f, 0.99f, 0.96f, 1f);
        private static readonly Color ChocolateHintText = new Color(0.93f, 0.86f, 0.74f, 0.92f);

        private Action pendingDialogContinue;
        private Action pendingNpcDialogueComplete;
        private Action<string> pendingBargainAction;
        private Action pendingBargainClose;
        private Action<string> pendingChoiceAction;
        private Action pendingChoiceClose;
        private Action pendingEvaluationContinue;

        private bool npcDialogueMode;
        private string npcDialogueSpeaker = string.Empty;
        private string[] npcDialogueLines = Array.Empty<string>();
        private int npcDialogueIndex;
        private const string MotherPortraitResourcePath = "ChoTet/Portraits/MotherDialoguePortrait";
        private Sprite cachedMotherPortraitSprite;

        private readonly string[] bargainActionLabels = new string[]
        {
            "1. Xin bớt lấy lộc đầu năm",
            "2. Khảo giá hàng khác",
            "3. Mua nhiều bớt giá",
            "4. Giả vờ bỏ đi",
            "5. Khen đồ đẹp/ngon",
            "6. Xem kỹ hàng",
            "7. Mua luôn",
            "Esc. Đóng"
        };

        private void Awake()
        {
            EnsureUiBuilt();
        }

        private void Update()
        {
            if (dialogPanel.activeSelf)
            {
                if (npcDialogueMode)
                {
                    if (WasConfirmPressed())
                    {
                        AdvanceNpcDialogue();
                    }

                    return;
                }

                if (WasConfirmPressed())
                {
                    dialogPanel.SetActive(false);
                    VoiceNarrator.Instance.Stop();
                    Action callback = pendingDialogContinue;
                    pendingDialogContinue = null;
                    if (callback != null)
                    {
                        callback();
                    }
                }

                return;
            }

            if (bargainPanel.activeSelf)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                    {
                        pendingBargainAction?.Invoke("soft_bargain");
                    }
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                    {
                        pendingBargainAction?.Invoke("compare_price");
                    }
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
                    {
                        pendingBargainAction?.Invoke("bulk_buy");
                    }
                    else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
                    {
                        pendingBargainAction?.Invoke("walk_away");
                    }
                    else if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame)
                    {
                        pendingBargainAction?.Invoke("charm");
                    }
                    else if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame)
                    {
                        pendingBargainAction?.Invoke("inspect_goods");
                    }
                    else if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame)
                    {
                        pendingBargainAction?.Invoke("buy_now");
                    }
                    else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        CloseBargainInternal();
                    }
                }

                return;
            }

            if (choicePanel.activeSelf)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                    {
                        pendingChoiceAction?.Invoke("choice_1");
                    }
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                    {
                        pendingChoiceAction?.Invoke("choice_2");
                    }
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
                    {
                        pendingChoiceAction?.Invoke("choice_3");
                    }
                    else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
                    {
                        pendingChoiceAction?.Invoke("choice_4");
                    }
                    else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        CloseChoiceInternal();
                    }
                }

                return;
            }

            if (evaluationPanel.activeSelf && WasConfirmPressed())
            {
                evaluationPanel.SetActive(false);
                Action callback = pendingEvaluationContinue;
                pendingEvaluationContinue = null;
                if (callback != null)
                {
                    callback();
                }
            }
        }

        public void UpdateHud(string statusSummary, string shoppingListSummary)
        {
            EnsureUiBuilt();
            statusText.text = statusSummary;
            shoppingListText.text = shoppingListSummary;
            if (shoppingListText != null)
            {
                shoppingListText.gameObject.SetActive(!string.IsNullOrWhiteSpace(shoppingListSummary));
            }
            RefreshHudVisibility();
        }

        public void SetPrompt(string prompt)
        {
            EnsureUiBuilt();
            promptText.text = string.IsNullOrEmpty(prompt) ? string.Empty : prompt;
            promptText.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
            if (promptText.transform.parent != null)
            {
                promptPanel = promptText.transform.parent.gameObject;
                promptPanel.SetActive(!string.IsNullOrEmpty(prompt) && !IsModalOpen);
            }
        }

        public void ShowDialog(string title, string body, Action onContinue)
        {
            EnsureUiBuilt();
            ApplyRetroChocolateDialogLayout();
            npcDialogueMode = false;
            pendingNpcDialogueComplete = null;
            if (dialogTitleText != null)
            {
                dialogTitleText.text = title;
                dialogTitleText.gameObject.SetActive(false);
            }

            dialogBodyText.text = body;
            dialogBodyText.color = ChocolateBodyText;
            SetHintText(dialogHintText, "Enter / Space — tiếp tục »");
            if (dialogHintText != null)
            {
                dialogHintText.color = ChocolateHintText;
            }

            pendingDialogContinue = onContinue;
            dialogPanel.SetActive(true);
            VoiceNarrator.Instance.Speak(title, body);

            // Name plate always shows speaker (Retro Chocolate style — no big portrait slab).
            if (dialogPortraitPanel != null)
            {
                dialogPortraitPanel.SetActive(true);
                if (dialogPortraitImage != null)
                {
                    dialogPortraitImage.sprite = null;
                    dialogPortraitImage.color = ChocolateNamePlate;
                }

                if (dialogPortraitText != null)
                {
                    dialogPortraitText.gameObject.SetActive(true);
                    dialogPortraitText.text = string.IsNullOrWhiteSpace(title) ? "..." : title.ToUpperInvariant();
                    dialogPortraitText.color = ChocolateBodyText;
                }
            }

            if (dialogContinueButton != null)
            {
                dialogContinueButton.gameObject.SetActive(false);
            }

            bargainPanel.SetActive(false);
            choicePanel.SetActive(false);
            evaluationPanel.SetActive(false);
            RefreshHudVisibility();
        }

        public void ShowNpcDialogue(string npcName, Sprite portrait, string[] lines, Action onFinished)
        {
            EnsureUiBuilt();
            ApplyRetroChocolateDialogLayout();
            npcDialogueMode = true;
            pendingDialogContinue = null;
            pendingNpcDialogueComplete = onFinished;
            npcDialogueSpeaker = string.IsNullOrEmpty(npcName) ? "NPC" : npcName;
            npcDialogueLines = lines != null && lines.Length > 0 ? lines : new[] { "..." };
            npcDialogueIndex = 0;
            dialogPanel.SetActive(true);
            bargainPanel.SetActive(false);
            choicePanel.SetActive(false);
            evaluationPanel.SetActive(false);
            if (dialogPortraitPanel != null)
            {
                dialogPortraitPanel.SetActive(true);
                if (dialogPortraitImage != null)
                {
                    dialogPortraitImage.sprite = null;
                    dialogPortraitImage.color = ChocolateNamePlate;
                }

                if (dialogPortraitText != null)
                {
                    dialogPortraitText.gameObject.SetActive(true);
                    dialogPortraitText.text = npcDialogueSpeaker.ToUpperInvariant();
                    dialogPortraitText.color = ChocolateBodyText;
                }
            }

            UpdateNpcDialogueUi();
            RefreshHudVisibility();
        }

        public void ShowBargain(string title, string body, int quote, string feedback, Action<string> onAction, Action onClose)
        {
            EnsureUiBuilt();
            npcDialogueMode = false;
            bargainTitleText.text = title;
            bargainBodyText.text = body;
            bargainOptionsText.text = string.Join("\n", bargainActionLabels);
            bargainQuoteText.text = "Giá người bán đưa ra: " + FormatMoney(quote);
            bargainFeedbackText.text = feedback;
            pendingBargainAction = onAction;
            pendingBargainClose = onClose;
            bargainPanel.SetActive(true);
            dialogPanel.SetActive(false);
            choicePanel.SetActive(false);
            evaluationPanel.SetActive(false);
            RefreshHudVisibility();

            // Speak vendor's greeting line
            string greetingText = body;
            int hintIdx = body.IndexOf("\n\n");
            if (hintIdx >= 0)
            {
                greetingText = body.Substring(0, hintIdx);
            }
            VoiceNarrator.Instance.Speak(title, greetingText);
        }

        public void UpdateBargain(int quote, string feedback)
        {
            EnsureUiBuilt();
            bargainQuoteText.text = "Giá người bán đưa ra: " + FormatMoney(quote);
            bargainFeedbackText.text = feedback;
        }

        public void ShowChoice(string title, string body, string[] options, Action<string> onChoice, Action onClose)
        {
            EnsureUiBuilt();
            npcDialogueMode = false;
            choiceTitleText.text = title;
            choiceBodyText.text = body;
            string joinedOptions = options != null && options.Length > 0 ? string.Join("\n", options) : string.Empty;
            if (choiceOptionsText != null)
            {
                choiceOptionsText.text = joinedOptions;
                choiceOptionsText.gameObject.SetActive(!string.IsNullOrEmpty(joinedOptions));
            }
            SetHintText(choiceHintText, "Nhấn số 1–4 để chọn · Esc để đóng");
            pendingChoiceAction = onChoice;
            pendingChoiceClose = onClose;
            choicePanel.SetActive(true);
            dialogPanel.SetActive(false);
            bargainPanel.SetActive(false);
            evaluationPanel.SetActive(false);
            RefreshHudVisibility();
        }

        public void ShowEvaluation(string title, string body, Action onContinue)
        {
            EnsureUiBuilt();
            npcDialogueMode = false;
            evaluationTitleText.text = title;
            evaluationBodyText.text = body;
            SetHintText(evaluationHintText, "Nhấn Enter hoặc Space để bắt đầu lại.");
            pendingEvaluationContinue = onContinue;
            evaluationPanel.SetActive(true);
            dialogPanel.SetActive(false);
            bargainPanel.SetActive(false);
            choicePanel.SetActive(false);
            RefreshHudVisibility();
        }

        private void CloseBargainInternal()
        {
            bargainPanel.SetActive(false);
            VoiceNarrator.Instance.Stop();
            RefreshHudVisibility();
            Action callback = pendingBargainClose;
            pendingBargainClose = null;
            pendingBargainAction = null;
            if (callback != null)
            {
                callback();
            }
        }

        private void CloseChoiceInternal()
        {
            choicePanel.SetActive(false);
            RefreshHudVisibility();
            Action callback = pendingChoiceClose;
            pendingChoiceClose = null;
            pendingChoiceAction = null;
            if (callback != null)
            {
                callback();
            }
        }

        private bool WasConfirmPressed()
        {
            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        private void EnsureUiBuilt()
        {
            if (rootCanvas == null || promptText == null || dialogPanel == null)
            {
                BuildUi();
            }

            EnsureEventSystem();
            RefreshHudVisibility();
        }

        private void EnsureEventSystem()
        {
            UnityEngine.EventSystems.EventSystem existing = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existing != null)
            {
                AttachInputModule(existing.gameObject);
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            AttachInputModule(eventSystem);
        }

        private void AttachInputModule(GameObject eventSystemObject)
        {
#if ENABLE_INPUT_SYSTEM
            if (eventSystemObject.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() != null)
            {
                return;
            }

            UnityEngine.EventSystems.StandaloneInputModule legacyModule = eventSystemObject.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (legacyModule != null)
            {
                Destroy(legacyModule);
            }

            eventSystemObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            if (eventSystemObject.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
#endif
        }

        private void RefreshHudVisibility()
        {
            if (hudPanel == null)
            {
                return;
            }

            bool hasModal = (dialogPanel != null && dialogPanel.activeSelf)
                || (bargainPanel != null && bargainPanel.activeSelf)
                || (choicePanel != null && choicePanel.activeSelf)
                || (evaluationPanel != null && evaluationPanel.activeSelf);

            hudPanel.SetActive(!hasModal);
            if (promptPanel != null)
            {
                bool hasPromptText = promptText != null && !string.IsNullOrEmpty(promptText.text);
                promptPanel.SetActive(!hasModal && hasPromptText);
            }
        }

        private void BuildUi()
        {
            rootCanvas = GetComponentInChildren<Canvas>();
            if (rootCanvas != null)
            {
                statusText = FindText(rootCanvas.transform, "StatusText");
                shoppingListText = FindText(rootCanvas.transform, "ShoppingListText");
                promptText = FindText(rootCanvas.transform, "PromptText");
                dialogTitleText = FindText(rootCanvas.transform, "DialogTitle");
                dialogBodyText = FindText(rootCanvas.transform, "DialogBody");
                dialogPortraitText = FindText(rootCanvas.transform, "DialogPortraitText");
                dialogContinueText = FindText(rootCanvas.transform, "DialogContinueText");
                bargainTitleText = FindText(rootCanvas.transform, "BargainTitle");
                bargainBodyText = FindText(rootCanvas.transform, "BargainBody");
                bargainOptionsText = FindText(rootCanvas.transform, "BargainOptions");
                bargainQuoteText = FindText(rootCanvas.transform, "BargainQuote");
                bargainFeedbackText = FindText(rootCanvas.transform, "BargainFeedback");
                choiceTitleText = FindText(rootCanvas.transform, "ChoiceTitle");
                choiceBodyText = FindText(rootCanvas.transform, "ChoiceBody");
                choiceOptionsText = FindText(rootCanvas.transform, "ChoiceOptions");
                choiceHintText = FindText(rootCanvas.transform, "ChoiceHint");
                dialogHintText = FindText(rootCanvas.transform, "DialogHint");
                evaluationTitleText = FindText(rootCanvas.transform, "EvaluationTitle");
                evaluationBodyText = FindText(rootCanvas.transform, "EvaluationBody");
                evaluationHintText = FindText(rootCanvas.transform, "EvaluationHint");
                hudDivider = FindChild(rootCanvas.transform, "HudDivider");
                hudPanel = FindChild(rootCanvas.transform, "HudPanel");
                promptPanel = FindChild(rootCanvas.transform, "PromptPanel");
                dialogPanel = FindChild(rootCanvas.transform, "DialogPanel");
                dialogPortraitPanel = FindChild(rootCanvas.transform, "DialogPortraitPanel");
                bargainPanel = FindChild(rootCanvas.transform, "BargainPanel");
                choicePanel = FindChild(rootCanvas.transform, "ChoicePanel");
                evaluationPanel = FindChild(rootCanvas.transform, "EvaluationPanel");
                if (dialogPortraitPanel != null)
                {
                    dialogPortraitImage = dialogPortraitPanel.GetComponent<Image>();
                }
                dialogContinueButton = FindButton(rootCanvas.transform, "DialogContinueButton");

                bool existingUiIsComplete = statusText != null
                    && shoppingListText != null
                    && promptText != null
                    && promptPanel != null
                    && dialogPanel != null
                    && dialogPortraitPanel != null
                    && dialogContinueButton != null
                    && bargainPanel != null
                    && bargainOptionsText != null
                    && choicePanel != null
                    && evaluationPanel != null;

                if (existingUiIsComplete)
                {
                    ApplyDialogPortraitLayout();
                    ApplyUpgradedLayout();
                    ApplyRetroChocolateDialogLayout();
                    DestroyGuideArrowIfPresent();
                    return;
                }

                if (Application.isPlaying)
                {
                    rootCanvas.gameObject.name = "DestroyedCanvasPending";
                    Destroy(rootCanvas.gameObject);
                }
                else
                {
                    DestroyImmediate(rootCanvas.gameObject);
                }
                rootCanvas = null;
            }

            GameObject canvasObject = new GameObject("ChoTetCanvas");
            canvasObject.transform.SetParent(transform, false);
            rootCanvas = canvasObject.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            hudPanel = CreatePanel("HudPanel", canvasObject.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(420f, 0f), new Color(0.09f, 0.1f, 0.12f, 0.88f), new Vector2(0f, 1f));
            RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
            hudRect.sizeDelta = new Vector2(420f, 580f);

            statusText = CreateText("StatusText", hudPanel.transform, font, 20, TextAnchor.UpperLeft, new Vector2(16f, -16f), new Vector2(388f, 300f));
            hudDivider = CreateDivider("HudDivider", hudPanel.transform, new Vector2(16f, -324f), new Vector2(388f, 2f));
            shoppingListText = CreateText("ShoppingListText", hudPanel.transform, font, 18, TextAnchor.UpperLeft, new Vector2(16f, -340f), new Vector2(388f, 220f));

            promptPanel = CreatePanel("PromptPanel", canvasObject.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(900f, 80f), new Color(0.05f, 0.06f, 0.08f, 0.85f), new Vector2(0.5f, 0f));
            promptText = CreateText("PromptText", promptPanel.transform, font, 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 60f), new Vector2(0.5f, 0.5f));
            promptText.gameObject.SetActive(false);
            promptPanel.SetActive(false);

            // Retro Chocolate VN box — bottom-centered, chocolate frame.
            dialogPanel = CreatePanel(
                "DialogPanel",
                canvasObject.transform,
                new Vector2(0.09f, 0.028f),
                new Vector2(0.91f, 0.22f),
                Vector2.zero,
                Vector2.zero,
                ChocolatePanel,
                new Vector2(0.5f, 0f));
            Image dialogImage = dialogPanel.GetComponent<Image>();
            dialogImage.color = new Color(0f, 0f, 0f, 0.01f);

            dialogPortraitPanel = CreatePanel(
                "DialogPortraitPanel",
                dialogPanel.transform,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(12f, 0f),
                new Vector2(110f, 0f),
                ChocolateNamePlate,
                new Vector2(0f, 0.5f));
            dialogPortraitImage = dialogPortraitPanel.GetComponent<Image>();
            ApplyDialogPortraitLayout();
            dialogPortraitText = CreateText(
                "DialogPortraitText",
                dialogPortraitPanel.transform,
                font,
                22,
                TextAnchor.MiddleCenter,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Vector2(0.5f, 0.5f));
            dialogPortraitText.text = "MẸ";
            dialogPortraitText.color = ChocolateBodyText;

            dialogTitleText = CreateText(
                "DialogTitle",
                dialogPanel.transform,
                font,
                26,
                TextAnchor.MiddleLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(132f, -10f),
                new Vector2(280f, 36f),
                new Vector2(0f, 1f));
            dialogTitleText.color = ChocolateBodyText;

            dialogBodyText = CreateText(
                "DialogBody",
                dialogPanel.transform,
                font,
                22,
                TextAnchor.UpperLeft,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(132f, -52f),
                new Vector2(-24f, -100f),
                new Vector2(0f, 1f));
            dialogBodyText.color = ChocolateBodyText;
            dialogBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialogBodyText.verticalOverflow = VerticalWrapMode.Overflow;

            dialogHintText = CreateHintText("DialogHint", dialogPanel.transform, font, new Vector2(132f, 12f), new Vector2(520f, 28f));
            dialogHintText.color = ChocolateHintText;
            RectTransform hintRect = dialogHintText.rectTransform;
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(0f, 0f);
            hintRect.pivot = new Vector2(0f, 0f);

            dialogContinueButton = CreateButton(
                "DialogContinueButton",
                dialogPanel.transform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-16f, 10f),
                new Vector2(150f, 40f),
                ChocolateNamePlate);
            dialogContinueText = CreateText(
                "DialogContinueText",
                dialogContinueButton.transform,
                font,
                20,
                TextAnchor.MiddleCenter,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Vector2(0.5f, 0.5f));
            dialogContinueText.text = "Tiếp tục »";
            dialogContinueText.color = ChocolateBodyText;
            dialogContinueButton.onClick.AddListener(AdvanceNpcDialogue);
            dialogPanel.SetActive(false);
            dialogPortraitPanel.SetActive(false);
            dialogContinueButton.gameObject.SetActive(false);
            ApplyRetroChocolateDialogLayout();

            bargainPanel = CreatePanel("BargainPanel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(1080f, 760f), new Color(0.13f, 0.11f, 0.09f, 0.96f), new Vector2(0.5f, 0.5f));
            bargainTitleText = CreateText("BargainTitle", bargainPanel.transform, font, 32, TextAnchor.UpperLeft, new Vector2(28f, -20f), new Vector2(1024f, 40f));
            bargainBodyText = CreateText("BargainBody", bargainPanel.transform, font, 22, TextAnchor.UpperLeft, new Vector2(28f, -72f), new Vector2(1024f, 250f));
            bargainOptionsText = CreateText("BargainOptions", bargainPanel.transform, font, 22, TextAnchor.UpperLeft, new Vector2(28f, -340f), new Vector2(1024f, 240f));
            bargainQuoteText = CreateText("BargainQuote", bargainPanel.transform, font, 26, TextAnchor.UpperLeft, new Vector2(28f, -600f), new Vector2(1024f, 36f));
            bargainFeedbackText = CreateText("BargainFeedback", bargainPanel.transform, font, 20, TextAnchor.UpperLeft, new Vector2(28f, -648f), new Vector2(1024f, 80f));
            bargainPanel.SetActive(false);

            choicePanel = CreatePanel("ChoicePanel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(1040f, 620f), new Color(0.09f, 0.11f, 0.14f, 0.96f), new Vector2(0.5f, 0.5f));
            choiceTitleText = CreateText("ChoiceTitle", choicePanel.transform, font, 32, TextAnchor.UpperLeft, new Vector2(28f, -20f), new Vector2(984f, 40f));
            choiceBodyText = CreateText("ChoiceBody", choicePanel.transform, font, 22, TextAnchor.UpperLeft, new Vector2(28f, -72f), new Vector2(984f, 150f));
            choiceOptionsText = CreateText("ChoiceOptions", choicePanel.transform, font, 22, TextAnchor.UpperLeft, new Vector2(28f, -240f), new Vector2(984f, 300f));
            choiceHintText = CreateHintText("ChoiceHint", choicePanel.transform, font, new Vector2(28f, -560f), new Vector2(984f, 36f));
            choicePanel.SetActive(false);

            evaluationPanel = CreatePanel("EvaluationPanel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(1040f, 680f), new Color(0.1f, 0.1f, 0.11f, 0.97f), new Vector2(0.5f, 0.5f));
            evaluationTitleText = CreateText("EvaluationTitle", evaluationPanel.transform, font, 32, TextAnchor.UpperLeft, new Vector2(30f, -20f), new Vector2(980f, 40f));
            evaluationBodyText = CreateText("EvaluationBody", evaluationPanel.transform, font, 22, TextAnchor.UpperLeft, new Vector2(30f, -72f), new Vector2(980f, 520f));
            evaluationHintText = CreateHintText("EvaluationHint", evaluationPanel.transform, font, new Vector2(30f, -620f), new Vector2(980f, 36f));
            evaluationPanel.SetActive(false);

            ApplyHudTextStyle(statusText, 20);
            ApplyHudTextStyle(shoppingListText, 18);
            ApplyModalTextStyle(dialogBodyText, 24);
            ApplyModalTextStyle(choiceBodyText, 22);
            ApplyModalTextStyle(choiceOptionsText, 22);
            ApplyModalTextStyle(evaluationBodyText, 22);
            ApplyModalTextStyle(bargainBodyText, 22);
            ApplyModalTextStyle(bargainOptionsText, 22);
            ApplyModalTextStyle(bargainFeedbackText, 20);
            ApplyModalTextStyle(bargainQuoteText, 26);
        }

        private static GameObject FindChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.gameObject : null;
        }

        private static Text FindText(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.GetComponent<Text>() : null;
        }

        private static Button FindButton(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color, Vector2 pivot)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Text CreateText(string name, Transform parent, Font font, int size, TextAnchor alignment, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            return CreateText(name, parent, font, size, alignment, new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, sizeDelta, new Vector2(0f, 1f));
        }

        private Text CreateText(string name, Transform parent, Font font, int size, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.lineSpacing = 1.15f;
            text.supportRichText = false;
            text.resizeTextForBestFit = false;
            text.color = Color.white;
            text.text = string.Empty;
            return text;
        }

        private Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private void UpdateNpcDialogueUi()
        {
            if (!npcDialogueMode)
            {
                return;
            }

            if (dialogTitleText != null)
            {
                dialogTitleText.text = npcDialogueSpeaker;
                dialogTitleText.gameObject.SetActive(false);
            }

            string line = npcDialogueLines != null && npcDialogueIndex >= 0 && npcDialogueIndex < npcDialogueLines.Length
                ? npcDialogueLines[npcDialogueIndex]
                : "...";
            dialogBodyText.text = line;
            dialogBodyText.color = ChocolateBodyText;
            SetHintText(dialogHintText, "Enter / Space — tiếp tục »");
            if (dialogHintText != null)
            {
                dialogHintText.color = ChocolateHintText;
            }

            VoiceNarrator.Instance.Speak(npcDialogueSpeaker, line);

            if (dialogPortraitText != null)
            {
                dialogPortraitText.text = npcDialogueSpeaker.ToUpperInvariant();
                dialogPortraitText.color = ChocolateBodyText;
                dialogPortraitText.gameObject.SetActive(true);
            }

            if (dialogContinueText != null)
            {
                dialogContinueText.text = npcDialogueLines != null && npcDialogueIndex + 1 < npcDialogueLines.Length ? "Tiếp tục" : "Kết thúc";
            }

            if (dialogContinueButton != null)
            {
                dialogContinueButton.gameObject.SetActive(true);
            }
        }

        private void AdvanceNpcDialogue()
        {
            if (!npcDialogueMode)
            {
                return;
            }

            if (npcDialogueLines != null && npcDialogueIndex + 1 < npcDialogueLines.Length)
            {
                npcDialogueIndex += 1;
                UpdateNpcDialogueUi();
                return;
            }

            dialogPanel.SetActive(false);
            VoiceNarrator.Instance.Stop();
            if (dialogContinueButton != null)
            {
                dialogContinueButton.gameObject.SetActive(false);
            }

            if (dialogPortraitPanel != null)
            {
                dialogPortraitPanel.SetActive(false);
            }

            npcDialogueMode = false;
            Action callback = pendingNpcDialogueComplete;
            pendingNpcDialogueComplete = null;
            if (callback != null)
            {
                callback();
            }

            RefreshHudVisibility();
        }

        private void SetNpcDialoguePortrait(Sprite portrait)
        {
            if (dialogPortraitImage == null)
            {
                return;
            }

            dialogPortraitImage.sprite = portrait;
            dialogPortraitImage.color = portrait == null ? new Color(0f, 0f, 0f, 0f) : Color.white;
            dialogPortraitImage.preserveAspect = true;
            if (dialogPortraitText != null)
            {
                dialogPortraitText.gameObject.SetActive(portrait == null);
            }
        }

        private Sprite ResolveDialoguePortrait(string speaker, Sprite portrait)
        {
            if (portrait != null || !IsMotherSpeaker(speaker))
            {
                return portrait;
            }

            return GetMotherPortraitSprite();
        }

        private Sprite GetMotherPortraitSprite()
        {
            if (cachedMotherPortraitSprite != null)
            {
                return cachedMotherPortraitSprite;
            }

            cachedMotherPortraitSprite = Resources.Load<Sprite>(MotherPortraitResourcePath);
            if (cachedMotherPortraitSprite != null)
            {
                return cachedMotherPortraitSprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(MotherPortraitResourcePath);
            if (texture == null)
            {
                return null;
            }

            cachedMotherPortraitSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            return cachedMotherPortraitSprite;
        }

        private static bool IsMotherSpeaker(string speaker)
        {
            if (string.IsNullOrWhiteSpace(speaker))
            {
                return false;
            }

            string normalized = speaker.Trim().ToLowerInvariant();
            return normalized == "me"
                || normalized == "mẹ"
                || normalized.Contains("máº¸")
                || normalized.Contains("mã");
        }

        private void ApplyDialogPortraitLayout()
        {
            if (dialogPortraitImage == null)
            {
                return;
            }

            dialogPortraitImage.color = ChocolateNamePlate;
            dialogPortraitImage.preserveAspect = true;
            dialogPortraitImage.raycastTarget = false;
        }

        private Transform EnsureDialogChocolateShell(Transform dialogRoot)
        {
            // Hierarchy (back → front): OuterFrame (dark) → CreamBorder (thin ring) → MainPanel (chocolate fill)
            Transform outer = dialogRoot.Find("ChocolateOuter");
            if (outer == null)
            {
                outer = CreateStretchImage(dialogRoot, "ChocolateOuter", ChocolateOuter, Vector2.zero, Vector2.zero).transform;
                outer.SetAsFirstSibling();
            }
            else
            {
                outer.GetComponent<Image>().color = ChocolateOuter;
            }

            Transform cream = outer.Find("ChocolateCream");
            if (cream == null)
            {
                cream = CreateStretchImage(outer, "ChocolateCream", ChocolateCream, new Vector2(3f, 3f), new Vector2(-3f, -3f)).transform;
            }
            else
            {
                cream.GetComponent<Image>().color = ChocolateCream;
            }

            Transform main = cream.Find("ChocolateMain");
            if (main == null)
            {
                main = CreateStretchImage(cream, "ChocolateMain", ChocolatePanel, new Vector2(4f, 4f), new Vector2(-4f, -4f)).transform;
            }
            else
            {
                main.GetComponent<Image>().color = ChocolatePanel;
            }

            Transform inner = main.Find("ChocolateInner");
            if (inner == null)
            {
                inner = CreateStretchImage(main, "ChocolateInner", ChocolateInner, new Vector2(8f, 8f), new Vector2(-8f, -8f)).transform;
            }
            else
            {
                inner.GetComponent<Image>().color = ChocolateInner;
            }

            return inner;
        }

        private static Image CreateStretchImage(
            Transform parent,
            string name,
            Color color,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void ApplyRetroChocolateDialogLayout()
        {
            if (dialogPanel == null)
            {
                return;
            }

            // Park widgets on the root before rebuilding the chocolate shell.
            ReparentDialogWidget(dialogPortraitPanel != null ? dialogPortraitPanel.transform : null, dialogPanel.transform);
            ReparentDialogWidget(dialogTitleText != null ? dialogTitleText.transform : null, dialogPanel.transform);
            ReparentDialogWidget(dialogBodyText != null ? dialogBodyText.transform : null, dialogPanel.transform);
            ReparentDialogWidget(dialogHintText != null ? dialogHintText.transform : null, dialogPanel.transform);
            ReparentDialogWidget(dialogContinueButton != null ? dialogContinueButton.transform : null, dialogPanel.transform);

            // Remove any previous frame layers (including the broken full-cream overlay).
            DestroyChildIfExists(dialogPanel.transform, "ChocolateOuter");
            DestroyChildIfExists(dialogPanel.transform, "ChocolateCream");

            RectTransform dialogRect = dialogPanel.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.09f, 0.03f);
            dialogRect.anchorMax = new Vector2(0.91f, 0.21f);
            dialogRect.pivot = new Vector2(0.5f, 0f);
            dialogRect.anchoredPosition = Vector2.zero;
            dialogRect.offsetMin = Vector2.zero;
            dialogRect.offsetMax = Vector2.zero;

            // Root image is only a click catcher — real fill is ChocolateMain.
            Image dialogImage = dialogPanel.GetComponent<Image>();
            if (dialogImage != null)
            {
                dialogImage.color = new Color(0f, 0f, 0f, 0.01f);
                dialogImage.raycastTarget = true;
            }

            Transform contentRoot = EnsureDialogChocolateShell(dialogPanel.transform);

            // Move dialogue widgets onto the inner chocolate panel so they sit above the frame.
            ReparentDialogWidget(dialogPortraitPanel != null ? dialogPortraitPanel.transform : null, contentRoot);
            ReparentDialogWidget(dialogTitleText != null ? dialogTitleText.transform : null, contentRoot);
            ReparentDialogWidget(dialogBodyText != null ? dialogBodyText.transform : null, contentRoot);
            ReparentDialogWidget(dialogHintText != null ? dialogHintText.transform : null, contentRoot);
            ReparentDialogWidget(dialogContinueButton != null ? dialogContinueButton.transform : null, contentRoot);

            // Floating name plate (speaker), not a giant portrait slab.
            if (dialogPortraitPanel != null)
            {
                RectTransform portraitRect = dialogPortraitPanel.GetComponent<RectTransform>();
                portraitRect.anchorMin = new Vector2(0f, 1f);
                portraitRect.anchorMax = new Vector2(0f, 1f);
                portraitRect.pivot = new Vector2(0f, 0f);
                portraitRect.anchoredPosition = new Vector2(18f, 8f);
                portraitRect.sizeDelta = new Vector2(200f, 40f);
                Image portraitImage = dialogPortraitPanel.GetComponent<Image>();
                if (portraitImage != null)
                {
                    portraitImage.sprite = null;
                    portraitImage.color = ChocolateNamePlate;
                    portraitImage.raycastTarget = false;
                }

                if (dialogPortraitText != null)
                {
                    dialogPortraitText.fontSize = 20;
                    dialogPortraitText.color = ChocolateBodyText;
                    dialogPortraitText.fontStyle = FontStyle.Bold;
                    dialogPortraitText.alignment = TextAnchor.MiddleCenter;
                }
            }

            if (dialogTitleText != null)
            {
                // Title is shown on the name plate; hide duplicate title text.
                dialogTitleText.gameObject.SetActive(false);
            }

            if (dialogBodyText != null)
            {
                RectTransform bodyRect = dialogBodyText.rectTransform;
                bodyRect.anchorMin = new Vector2(0f, 0f);
                bodyRect.anchorMax = new Vector2(1f, 1f);
                bodyRect.pivot = new Vector2(0f, 1f);
                bodyRect.offsetMin = new Vector2(22f, 36f);
                bodyRect.offsetMax = new Vector2(-22f, -18f);
                dialogBodyText.color = ChocolateBodyText;
                dialogBodyText.fontSize = 23;
                dialogBodyText.fontStyle = FontStyle.Normal;
                dialogBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
                dialogBodyText.verticalOverflow = VerticalWrapMode.Overflow;
                dialogBodyText.alignment = TextAnchor.UpperLeft;
            }

            if (dialogHintText != null)
            {
                RectTransform hintRect = dialogHintText.rectTransform;
                hintRect.anchorMin = new Vector2(1f, 0f);
                hintRect.anchorMax = new Vector2(1f, 0f);
                hintRect.pivot = new Vector2(1f, 0f);
                hintRect.anchoredPosition = new Vector2(-18f, 8f);
                hintRect.sizeDelta = new Vector2(340f, 26f);
                dialogHintText.color = ChocolateHintText;
                dialogHintText.alignment = TextAnchor.MiddleRight;
                dialogHintText.text = "Enter / Space — tiếp tục »";
                dialogHintText.fontStyle = FontStyle.Italic;
            }

            if (dialogContinueButton != null)
            {
                dialogContinueButton.gameObject.SetActive(false);
            }
        }

        private static void ReparentDialogWidget(Transform widget, Transform contentRoot)
        {
            if (widget == null || contentRoot == null || widget.parent == contentRoot)
            {
                return;
            }

            widget.SetParent(contentRoot, false);
        }

        private static void DestroyChildIfExists(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                child.gameObject.name = childName + "_DestroyedPending";
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private void ApplyUpgradedLayout()
        {
            Font font = statusText != null ? statusText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (hudPanel != null)
            {
                RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
                hudRect.sizeDelta = new Vector2(420f, 580f);
                hudRect.anchoredPosition = new Vector2(20f, -20f);
            }

            if (statusText != null)
            {
                ApplyRect(statusText.rectTransform, new Vector2(16f, -16f), new Vector2(388f, 300f));
                ApplyHudTextStyle(statusText, 20);
            }

            if (shoppingListText != null)
            {
                ApplyRect(shoppingListText.rectTransform, new Vector2(16f, -340f), new Vector2(388f, 220f));
                ApplyHudTextStyle(shoppingListText, 18);
            }

            if (hudPanel != null && hudDivider == null)
            {
                hudDivider = CreateDivider("HudDivider", hudPanel.transform, new Vector2(16f, -324f), new Vector2(388f, 2f));
            }

            if (dialogPanel != null)
            {
                dialogPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080f, 560f);
            }

            if (dialogTitleText != null)
            {
                ApplyRect(dialogTitleText.rectTransform, new Vector2(268f, -20f), new Vector2(788f, 40f));
                ApplyModalTextStyle(dialogTitleText, 32);
            }

            if (dialogBodyText != null)
            {
                ApplyRect(dialogBodyText.rectTransform, new Vector2(268f, -72f), new Vector2(788f, 360f));
                ApplyModalTextStyle(dialogBodyText, 24);
            }

            if (dialogPanel != null && dialogHintText == null && font != null)
            {
                dialogHintText = CreateHintText("DialogHint", dialogPanel.transform, font, new Vector2(268f, -460f), new Vector2(788f, 36f));
            }
            else if (dialogHintText != null)
            {
                ApplyRect(dialogHintText.rectTransform, new Vector2(268f, -460f), new Vector2(788f, 36f));
            }

            if (bargainPanel != null)
            {
                bargainPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080f, 760f);
            }

            if (bargainTitleText != null)
            {
                ApplyRect(bargainTitleText.rectTransform, new Vector2(28f, -20f), new Vector2(1024f, 40f));
                ApplyModalTextStyle(bargainTitleText, 32);
            }

            if (bargainBodyText != null)
            {
                ApplyRect(bargainBodyText.rectTransform, new Vector2(28f, -72f), new Vector2(1024f, 250f));
                ApplyModalTextStyle(bargainBodyText, 22);
            }

            if (bargainOptionsText != null)
            {
                ApplyRect(bargainOptionsText.rectTransform, new Vector2(28f, -340f), new Vector2(1024f, 240f));
                ApplyModalTextStyle(bargainOptionsText, 22);
            }

            if (bargainQuoteText != null)
            {
                ApplyRect(bargainQuoteText.rectTransform, new Vector2(28f, -600f), new Vector2(1024f, 36f));
                ApplyModalTextStyle(bargainQuoteText, 26);
            }

            if (bargainFeedbackText != null)
            {
                ApplyRect(bargainFeedbackText.rectTransform, new Vector2(28f, -648f), new Vector2(1024f, 80f));
                ApplyModalTextStyle(bargainFeedbackText, 20);
            }

            if (choicePanel != null)
            {
                choicePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1040f, 620f);
            }

            if (choiceTitleText != null)
            {
                ApplyRect(choiceTitleText.rectTransform, new Vector2(28f, -20f), new Vector2(984f, 40f));
                ApplyModalTextStyle(choiceTitleText, 32);
            }

            if (choiceBodyText != null)
            {
                ApplyRect(choiceBodyText.rectTransform, new Vector2(28f, -72f), new Vector2(984f, 150f));
                ApplyModalTextStyle(choiceBodyText, 22);
            }

            if (choicePanel != null && choiceOptionsText == null && font != null)
            {
                choiceOptionsText = CreateText("ChoiceOptions", choicePanel.transform, font, 22, TextAnchor.UpperLeft, new Vector2(28f, -240f), new Vector2(984f, 300f));
                ApplyModalTextStyle(choiceOptionsText, 22);
            }
            else if (choiceOptionsText != null)
            {
                ApplyRect(choiceOptionsText.rectTransform, new Vector2(28f, -240f), new Vector2(984f, 300f));
                ApplyModalTextStyle(choiceOptionsText, 22);
            }

            if (choicePanel != null && choiceHintText == null && font != null)
            {
                choiceHintText = CreateHintText("ChoiceHint", choicePanel.transform, font, new Vector2(28f, -560f), new Vector2(984f, 36f));
            }
            else if (choiceHintText != null)
            {
                ApplyRect(choiceHintText.rectTransform, new Vector2(28f, -560f), new Vector2(984f, 36f));
            }

            if (evaluationPanel != null)
            {
                evaluationPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1040f, 680f);
            }

            if (evaluationTitleText != null)
            {
                ApplyRect(evaluationTitleText.rectTransform, new Vector2(30f, -20f), new Vector2(980f, 40f));
                ApplyModalTextStyle(evaluationTitleText, 32);
            }

            if (evaluationBodyText != null)
            {
                ApplyRect(evaluationBodyText.rectTransform, new Vector2(30f, -72f), new Vector2(980f, 520f));
                ApplyModalTextStyle(evaluationBodyText, 22);
            }

            if (evaluationPanel != null && evaluationHintText == null && font != null)
            {
                evaluationHintText = CreateHintText("EvaluationHint", evaluationPanel.transform, font, new Vector2(30f, -620f), new Vector2(980f, 36f));
            }
            else if (evaluationHintText != null)
            {
                ApplyRect(evaluationHintText.rectTransform, new Vector2(30f, -620f), new Vector2(980f, 36f));
            }
        }

        private static void ApplyRect(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void ApplyHudTextStyle(Text text, int fontSize)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.lineSpacing = 1.2f;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = new Color(0.95f, 0.96f, 0.98f, 1f);
        }

        private static void ApplyModalTextStyle(Text text, int fontSize)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.lineSpacing = 1.15f;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            // Keep text inside its rect so modal sections never paint over each other.
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = Color.white;
        }

        private static void SetHintText(Text hintText, string message)
        {
            if (hintText == null)
            {
                return;
            }

            hintText.text = message;
            hintText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        private GameObject CreateDivider(string name, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject divider = new GameObject(name, typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(parent, false);
            RectTransform rect = divider.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            divider.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);
            return divider;
        }

        private Text CreateHintText(string name, Transform parent, Font font, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            Text hint = CreateText(name, parent, font, 18, TextAnchor.MiddleLeft, anchoredPosition, sizeDelta);
            hint.fontStyle = FontStyle.Italic;
            hint.color = new Color(0.78f, 0.8f, 0.84f, 0.95f);
            hint.lineSpacing = 1.1f;
            return hint;
        }

        private string FormatMoney(int amount)
        {
            string raw = Mathf.Abs(amount).ToString();
            string result = string.Empty;
            while (raw.Length > 3)
            {
                result = "." + raw.Substring(raw.Length - 3) + result;
                raw = raw.Substring(0, raw.Length - 3);
            }

            result = raw + result + "d";
            if (amount < 0)
            {
                result = "-" + result;
            }

            return result;
        }

        private void DestroyGuideArrowIfPresent()
        {
            if (rootCanvas == null)
            {
                return;
            }

            Transform existing = rootCanvas.transform.Find("GuideArrowRoot");
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }
        }
    }
}
