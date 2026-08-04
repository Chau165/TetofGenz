using ChoTet.Level1.HometownArrival;
using ChoTet.Level1.UI;
using ChoTet.Menu;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ChoTet.Level1
{
    /// <summary>
    /// Drives the imported inventory UI prefab (panels, slots, progress, popups).
    /// </summary>
    public class InventoryUIController : MonoBehaviour
    {
        [Header("Imported inventory UI")]
        [SerializeField] private GameObject inventoryModalRoot;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text selectedItemNameText;
        [SerializeField] private Text closeHintText;

        [Header("Slots (6 required items)")]
        [SerializeField] private InventorySlotUI[] slots = Array.Empty<InventorySlotUI>();

        [Header("Packing progress")]
        [SerializeField] private Text progressTitleText;
        [SerializeField] private Text progressCountText;

        [Header("Feedback")]
        [SerializeField] private GameObject pickupPopup;
        [SerializeField] private Text pickupPopupText;
        [SerializeField] private float pickupPopupDuration = 1.8f;

        [Header("Gameplay hints")]
        [SerializeField] private GameObject interactionPromptPanel;
        [SerializeField] private Text interactionPromptText;
        [SerializeField] private Button helpButton;
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private Text helpText;
        [SerializeField] private bool showMovementGuideOnStart = true;

        [Header("Vehicle booking note (embedded in inventory panel)")]
        [SerializeField] private VehicleBookingNoteView vehicleBookingNote;
        [SerializeField] private VehicleLicensePlateHud licensePlateHud;
        [SerializeField] private Level1PhoneController phoneController;

        private InventoryManager inventoryManager;
        private readonly Dictionary<string, InventoryItemData> itemLookup = new Dictionary<string, InventoryItemData>();
        private Coroutine popupRoutine;
        private bool inventoryOpen;
        private bool helpOpen;
        private string currentPrompt = string.Empty;
        private bool gameplayHudBuilt;

        public bool IsInventoryOpen => inventoryOpen;
        public bool IsNoteOpen => inventoryOpen && vehicleBookingNote != null && vehicleBookingNote.gameObject.activeSelf;
        public bool IsModalOpen =>
            inventoryOpen
            || RetroChocolateDialogueUI.IsOpen
            || (phoneController != null && phoneController.IsOpen)
            || (licensePlateHud != null && licensePlateHud.IsPanelOpen);

        private void Awake()
        {
            EnsureEventSystem();
            ResolveButtonReferences();
            WireButtons();
        }

        private void OnEnable()
        {
            ResolveButtonReferences();
            WireButtons();
        }

        private void Start()
        {
            WireButtons();
            ConfigureSlots();
            EnsureGameplayHudWidgets();

            if (showMovementGuideOnStart)
            {
                SetHelpOpen(true);
            }
        }

        public void Initialize(InventoryManager manager, IReadOnlyList<InventoryItemData> itemDefinitions)
        {
            inventoryManager = manager;
            itemLookup.Clear();

            for (int i = 0; i < itemDefinitions.Count; i++)
            {
                InventoryItemData data = itemDefinitions[i];
                if (data != null && !string.IsNullOrEmpty(data.ItemId))
                {
                    itemLookup[data.ItemId] = data;
                }
            }

            WireButtons();
            ConfigureSlots();
            EnsureGameplayHudWidgets();
            EnsureOpenButtonIcon();
            SetInventoryOpen(false, false);
            RefreshAllSlots();
            UpdateProgress(0, slots.Length > 0 ? slots.Length : Level1ItemCatalog.RequiredItemCount);

            if (titleText != null)
            {
                titleText.text = "Hành lý";
                titleText.raycastTarget = false;
            }

            if (progressTitleText != null)
            {
                progressTitleText.text = "Tiến độ dọn đồ";
                progressTitleText.raycastTarget = false;
            }

            if (progressCountText != null)
            {
                progressCountText.raycastTarget = false;
            }

            if (closeHintText != null)
            {
                closeHintText.text = "Nhấn I hoặc Esc để đóng";
                closeHintText.raycastTarget = false;
            }

            if (selectedItemNameText != null)
            {
                selectedItemNameText.text = "Chọn một vật phẩm";
                selectedItemNameText.raycastTarget = false;
            }

            if (pickupPopup != null)
            {
                pickupPopup.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.transform.SetAsLastSibling();
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (phoneController != null && phoneController.IsOpen)
                {
                    phoneController.Close();
                }
                else if (licensePlateHud != null && licensePlateHud.IsPanelOpen)
                {
                    licensePlateHud.DismissPanel();
                }
                else if (helpOpen)
                {
                    SetHelpOpen(false);
                }
                else if (inventoryOpen)
                {
                    SetInventoryOpen(false);
                }

                return;
            }

            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                SetHelpOpen(false);
                SetInventoryOpen(false);
                phoneController?.Toggle();
            }
        }

        public void SetPrompt(string prompt)
        {
            currentPrompt = prompt ?? string.Empty;
            RefreshInteractionPrompt();
        }

        public void ToggleHelpPanel()
        {
            SetHelpOpen(!helpOpen);
        }

        public void UpdateProgress(int collected, int required)
        {
            if (progressCountText != null)
            {
                progressCountText.text = collected + " / " + required;
            }
        }

        public void ShowPickupPopup(string message)
        {
            ShowPickupPopup(message, pickupPopupDuration);
        }

        public void ShowBusStopFailPopup(string message)
        {
            ShowPickupPopup("THẤT BẠI — " + message, 3.5f);
        }

        public void ShowPickupPopup(string message, float durationSeconds)
        {
            if (pickupPopup == null || pickupPopupText == null)
            {
                return;
            }

            pickupPopupText.text = message;
            pickupPopupText.raycastTarget = false;
            pickupPopup.SetActive(true);

            if (popupRoutine != null)
            {
                StopCoroutine(popupRoutine);
            }

            popupRoutine = StartCoroutine(HidePopupAfterDelay(durationSeconds));
        }

        public void RefreshAllSlots()
        {
            if (inventoryManager == null)
            {
                return;
            }

            IReadOnlyList<InventoryItemData> requiredItems = inventoryManager.GetRequiredItemDefinitions();
            for (int i = 0; i < slots.Length; i++)
            {
                InventorySlotUI slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                if (i < requiredItems.Count)
                {
                    InventoryItemData definition = requiredItems[i];
                    bool collected = inventoryManager.HasItem(definition.ItemId);
                    if (collected)
                    {
                        slot.BindCollected(definition);
                    }
                    else
                    {
                        slot.BindEmpty();
                    }
                }
                else
                {
                    slot.BindEmpty();
                }
            }
        }

        public InventoryItemData GetItemData(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            itemLookup.TryGetValue(itemId, out InventoryItemData data);
            return data;
        }

        private void ResolveButtonReferences()
        {
            Transform root = transform;
            if (openButton == null)
            {
                openButton = FindButton(root, "OpenButton", "InventoryButton", "BagButton", "Btn_Inventory");
            }

            if (closeButton == null)
            {
                closeButton = FindButton(root, "CloseButton", "Btn_Close", "Close", "ExitButton");
            }

            if (backdropButton == null)
            {
                backdropButton = FindButton(root, "Backdrop", "DimOverlay", "Overlay", "Background");
            }

            if (helpButton == null)
            {
                helpButton = FindButton(root, "HelpButton");
            }

            if (inventoryModalRoot == null)
            {
                inventoryModalRoot = FindFirst(root, "InventoryModal", "ModalRoot")?.gameObject;
            }
        }

        private static Transform FindFirst(Transform parent, params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Transform found = FindDeepChild(parent, names[i]);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Button FindButton(Transform parent, params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Transform found = FindDeepChild(parent, names[i]);
                if (found != null && found.TryGetComponent(out Button button))
                {
                    return button;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.name == childName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindDeepChild(parent.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void WireButtons()
        {
            ResolveButtonReferences();

            if (openButton != null)
            {
                openButton.onClick.RemoveListener(ToggleInventoryPanel);
                openButton.onClick.AddListener(ToggleInventoryPanel);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseInventoryPanel);
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveAllListeners();
                backdropButton.onClick.AddListener(CloseInventoryPanel);
            }

            if (helpButton != null)
            {
                helpButton.onClick.RemoveAllListeners();
                helpButton.onClick.AddListener(ToggleHelpPanel);
            }

        }

        private void ConfigureSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].ConfigureSelection(HandleSlotSelected);
                }
            }
        }

        private void HandleSlotSelected(InventorySlotUI slot)
        {
            if (selectedItemNameText == null || slot == null)
            {
                return;
            }

            InventoryItemData data = slot.ResolveItemData();
            selectedItemNameText.text = data != null ? data.ItemName : "Ô trống";
        }

        private void ToggleInventoryPanel()
        {
            SetInventoryOpen(!inventoryOpen);
        }

        public void CloseInventoryPanel()
        {
            SetInventoryOpen(false);
        }

        private void SetInventoryOpen(bool open, bool refreshSlots = true)
        {
            if (open && phoneController != null && phoneController.IsOpen)
            {
                phoneController.Close();
            }

            inventoryOpen = open;

            if (inventoryModalRoot != null)
            {
                inventoryModalRoot.SetActive(inventoryOpen);
            }
            else if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(inventoryOpen);
            }

            if (vehicleBookingNote != null)
            {
                if (inventoryOpen)
                {
                    vehicleBookingNote.ApplyDefaultSampleContent();
                    vehicleBookingNote.Show();
                }
                else
                {
                    vehicleBookingNote.Hide();
                }
            }

            if (inventoryOpen && refreshSlots)
            {
                RefreshAllSlots();
            }

            if (closeButton != null && inventoryOpen)
            {
                closeButton.transform.SetAsLastSibling();
            }

            RefreshInteractionPrompt();
        }

        private void SetHelpOpen(bool open)
        {
            helpOpen = open;
            if (helpPanel != null)
            {
                helpPanel.SetActive(helpOpen);
            }

            RefreshInteractionPrompt();
        }

        public void RefreshGameplayPrompts()
        {
            RefreshInteractionPrompt();
        }

        private void RefreshInteractionPrompt()
        {
            if (interactionPromptPanel == null || interactionPromptText == null)
            {
                return;
            }

            bool platePanelOpen = licensePlateHud != null && licensePlateHud.IsPanelOpen;
            bool phoneOpen = phoneController != null && phoneController.IsOpen;
            bool showPrompt = !inventoryOpen && !helpOpen && !phoneOpen && !platePanelOpen && !string.IsNullOrEmpty(currentPrompt);
            interactionPromptText.text = currentPrompt;
            interactionPromptPanel.SetActive(showPrompt);
        }

        private void EnsureGameplayHudWidgets()
        {
            if (gameplayHudBuilt)
            {
                return;
            }

            KenneyRpgUiLibrary library = KenneyRpgUiLibrary.LoadDefault();
            if (library == null)
            {
                return;
            }

            Transform root = transform;

            if (interactionPromptPanel == null || interactionPromptText == null)
            {
                GameObject promptPanel = KenneyRpgUiWidgets.CreateSpritePanel("InteractionPromptPanel", root,
                    library.panelInsetBrown, new Color(0.14f, 0.1f, 0.08f, 0.88f));
                RectTransform promptRect = promptPanel.GetComponent<RectTransform>();
                promptRect.anchorMin = new Vector2(0.5f, 0f);
                promptRect.anchorMax = new Vector2(0.5f, 0f);
                promptRect.pivot = new Vector2(0.5f, 0f);
                promptRect.anchoredPosition = new Vector2(0f, 28f);
                promptRect.sizeDelta = new Vector2(760f, 72f);
                promptPanel.GetComponent<Image>().type = Image.Type.Sliced;
                promptPanel.GetComponent<Image>().raycastTarget = false;

                interactionPromptText = MainMenuTypography.CreateStyledText("InteractionPromptText", promptPanel.transform,
                    MenuTextStyle.DialogBody, TextAnchor.MiddleCenter, string.Empty);
                interactionPromptText.raycastTarget = false;
                StretchRect(interactionPromptText.rectTransform, 12f);
                interactionPromptPanel = promptPanel;
                interactionPromptPanel.SetActive(false);
            }

            if (helpButton == null)
            {
                helpButton = KenneyRpgUiWidgets.CreateIconButton("HelpButton", root, library, "?", ToggleHelpPanel);
                RectTransform helpRect = helpButton.GetComponent<RectTransform>();
                helpRect.anchorMin = new Vector2(0f, 0f);
                helpRect.anchorMax = new Vector2(0f, 0f);
                helpRect.pivot = new Vector2(0f, 0f);
                helpRect.anchoredPosition = new Vector2(24f, 24f);
                helpRect.sizeDelta = new Vector2(72f, 72f);
                helpButton.GetComponent<Image>().type = Image.Type.Sliced;
            }

            if (helpPanel == null)
            {
                helpPanel = FindDeepChild(transform, "HelpPanel")?.gameObject;
            }

            if (helpText == null)
            {
                helpText = FindDeepChild(transform, "HelpText")?.GetComponent<Text>();
            }

            if (helpPanel == null || helpText == null)
            {
                GameObject panel = KenneyRpgUiWidgets.CreateSpritePanel("HelpPanel", root,
                    library.panelBrown, KenneyRpgUiWidgets.TitleBoardTint);
                RectTransform panelRect = panel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = new Vector2(0f, 40f);
                panelRect.sizeDelta = new Vector2(620f, 300f);
                panel.GetComponent<Image>().type = Image.Type.Sliced;

                Text title = MainMenuTypography.CreateStyledText("HelpTitle", panel.transform,
                    MenuTextStyle.DialogTitle, TextAnchor.UpperCenter, "Hướng Dẫn Chơi");
                title.raycastTarget = false;
                LayoutHelpRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -20f), new Vector2(0f, -88f));

                helpText = MainMenuTypography.CreateStyledText("HelpText", panel.transform,
                    MenuTextStyle.DialogBody, TextAnchor.UpperLeft,
                    "Phím WASD / Mũi tên — Di chuyển\nShift Trái — Chạy nhanh\nE — Nhặt vật phẩm\nI — Mở điện thoại\nNút túi — Mở hành lý\nEsc — Đóng giao diện\n\nNgoài đường: có 3 mạng.\nĐừng xuống lòng đường — xe tông mất 1 mạng.");
                helpText.raycastTarget = false;
                LayoutHelpRect(helpText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                    new Vector2(24f, 24f), new Vector2(-24f, -96f));

                helpPanel = panel;
                helpPanel.SetActive(false);
            }

            ApplyLocalizedGameplayHelp();

            if (licensePlateHud == null)
            {
                licensePlateHud = GetComponent<VehicleLicensePlateHud>();
                if (licensePlateHud == null)
                {
                    licensePlateHud = gameObject.AddComponent<VehicleLicensePlateHud>();
                }
            }

            licensePlateHud.EnsureBuilt(root);

            if (phoneController == null)
            {
                phoneController = GetComponent<Level1PhoneController>();
                if (phoneController == null)
                {
                    phoneController = gameObject.AddComponent<Level1PhoneController>();
                }
            }

            phoneController.EnsureBuilt(root, this);

            gameplayHudBuilt = true;
            WireButtons();
            BringGameplayButtonsToFront();
        }

        private void ApplyLocalizedGameplayHelp()
        {
            if (helpPanel == null)
            {
                return;
            }

            Text helpTitle = FindDeepChild(helpPanel.transform, "HelpTitle")?.GetComponent<Text>();
            if (helpTitle != null)
            {
                helpTitle.text = "Hướng dẫn chơi";
                helpTitle.raycastTarget = false;
            }

            if (helpText != null)
            {
                helpText.text = "Phím WASD / Mũi tên — Di chuyển\nShift trái — Chạy nhanh\nE — Nhặt vật phẩm\nI — Mở điện thoại\nNút túi — Mở hành lý\nEsc — Đóng giao diện\n\nNgoài đường: có 3 mạng.\nĐừng xuống lòng đường — xe tông mất 1 mạng.";
                helpText.raycastTarget = false;
            }
        }

        private void BringGameplayButtonsToFront()
        {
            if (openButton != null)
            {
                openButton.transform.SetAsLastSibling();
            }

            Transform plateButton = FindDeepChild(transform, "CheckLicensePlateButton");
            if (plateButton != null)
            {
                plateButton.SetAsLastSibling();
            }

            if (interactionPromptPanel != null)
            {
                interactionPromptPanel.transform.SetAsFirstSibling();
            }
        }

        private static void StretchRect(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static void LayoutHelpRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private IEnumerator HidePopupAfterDelay(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (pickupPopup != null)
            {
                pickupPopup.SetActive(false);
            }

            popupRoutine = null;
        }

        private void EnsureOpenButtonIcon()
        {
            EnsureKenneyIconOnButton(openButton, "basket", new Color(0.24f, 0.16f, 0.1f, 1f));
        }

        private static void EnsureKenneyIconOnButton(Button button, string iconKey, Color iconTint)
        {
            if (button == null)
            {
                return;
            }

            Transform iconTransform = button.transform.Find("Icon");
            if (iconTransform == null)
            {
                return;
            }

            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage == null || iconImage.sprite != null)
            {
                return;
            }

            Sprite icon = KenneyGameIconLoader.GetIcon(iconKey);
            if (icon == null)
            {
                return;
            }

            iconImage.sprite = icon;
            iconImage.color = iconTint;
            iconImage.preserveAspect = true;
        }

        private static void EnsureEventSystem()
        {
            EventSystem existing = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
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
                UnityEngine.Object.Destroy(legacyModule);
            }

            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

#if UNITY_EDITOR
        public void AutoWireFromHierarchy()
        {
            Transform root = transform;
            inventoryModalRoot = FindFirst(root, "InventoryModal", "ModalRoot")?.gameObject;
            inventoryPanel = FindFirst(root,
                "InventoryPanel", "Panel", "PanelFrame", "InventoryWindow", "Window", "ContentPanel")?.gameObject;
            backdropButton = FindButton(root, "Backdrop", "DimOverlay", "Overlay", "Background");
            openButton = FindButton(root, "OpenButton", "InventoryButton", "BagButton", "Btn_Inventory");
            closeButton = FindButton(root, "CloseButton", "Btn_Close", "Close", "ExitButton");
            vehicleBookingNote = root.GetComponentInChildren<VehicleBookingNoteView>(true);
            titleText = FindText(root, "Title", "InventoryTitle", "HeaderTitle");
            selectedItemNameText = FindText(root, "SelectedItemName", "ItemName", "SelectedName", "Description");
            closeHintText = FindText(root, "CloseHint", "HintText", "InventoryHint");
            progressTitleText = FindText(root, "ProgressTitle", "PackingProgressTitle");
            progressCountText = FindText(root, "ProgressCount", "PackingProgressCount", "ProgressValue");
            pickupPopup = FindFirst(root, "PickupPopup", "Popup", "Toast", "Notification")?.gameObject;
            pickupPopupText = FindText(root, "PopupText", "ToastText", "Message");
            interactionPromptPanel = FindFirst(root, "InteractionPromptPanel", "PromptPanel")?.gameObject;
            interactionPromptText = FindText(root, "InteractionPromptText", "PromptText");
            helpButton = FindButton(root, "HelpButton");
            helpPanel = FindFirst(root, "HelpPanel")?.gameObject;
            helpText = FindText(root, "HelpText");

            Transform slotsRoot = FindFirst(root, "Slots", "SlotGrid", "ItemGrid", "Grid", "InventorySlots");
            if (slotsRoot != null)
            {
                slots = slotsRoot.GetComponentsInChildren<InventorySlotUI>(true);
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        private static Text FindText(Transform parent, params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Transform found = FindDeepChild(parent, names[i]);
                if (found != null && found.TryGetComponent(out Text text))
                {
                    return text;
                }
            }

            return null;
        }
#endif
    }
}
