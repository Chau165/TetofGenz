#if UNITY_EDITOR
using System.IO;
using ChoTet.Level1.UI;
using ChoTet.Menu;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Builds or locates the Level 1 inventory UI prefab using Kenney RPG UI sprites.
    /// If you imported a third-party inventory prefab, place it at the prefab path below
    /// or assign it on InventoryUIController in the scene.
    /// </summary>
    public static class Level1InventoryUiAssembler
    {
        public const string PrefabPath = "Assets/Art/UI/Level1InventoryUI.prefab";
        public const string ItemDataFolder = "Assets/Resources/Level1/InventoryItems";
        public const string BlenderIconFolder = "Assets/Art/Level1/InventoryIcons";

        public static void WireSelectedInventoryUi()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("[Level1InventoryUiAssembler] Select your imported inventory UI root in the Hierarchy.");
                return;
            }

            InventoryUIController controller = selected.GetComponent<InventoryUIController>();
            if (controller == null)
            {
                controller = selected.AddComponent<InventoryUIController>();
            }

            controller.AutoWireFromHierarchy();
            EditorUtility.SetDirty(controller);
            Debug.Log("[Level1InventoryUiAssembler] Wired InventoryUIController on " + selected.name +
                      ". Review slot references in the Inspector.");
        }

        public static void BuildPrefabMenu()
        {
            GameObject prefab = BuildOrUpdatePrefab();
            if (prefab != null)
            {
                Selection.activeObject = prefab;
                Debug.Log("[Level1InventoryUiAssembler] Saved inventory UI prefab at " + PrefabPath);
            }
        }

        public static void AssignBlenderIconsMenu()
        {
            InventoryItemData[] items = AssignBlenderIconsToItemDefinitions();
            Debug.Log("[Level1InventoryUiAssembler] Assigned Blender icons to " + items.Length + " items.");
        }

        public static void CreateItemDataMenu()
        {
            InventoryItemData[] items = CreateOrUpdateItemDefinitions();
            Debug.Log("[Level1InventoryUiAssembler] Created/updated " + items.Length + " inventory item assets.");
        }

        public static GameObject EnsurePrefabInstance(Transform parent)
        {
            CreateOrUpdateItemDefinitions();
            GameObject prefabAsset = FindExistingInventoryPrefab();
            if (prefabAsset == null)
            {
                prefabAsset = BuildOrUpdatePrefab();
            }

            if (prefabAsset == null)
            {
                Debug.LogError("[Level1InventoryUiAssembler] Could not create inventory UI prefab.");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, parent);
            instance.name = "Level1InventoryUI";
            return instance;
        }

        /// <summary>
        /// Looks for an existing inventory prefab (third-party or previously built).
        /// Prefabs with InventoryUIController are preferred.
        /// </summary>
        public static GameObject FindExistingInventoryPrefab()
        {
            GameObject preferred = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (preferred != null && preferred.GetComponent<InventoryUIController>() != null)
            {
                return preferred;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
                {
                    continue;
                }

                GameObject candidate = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (candidate == null || candidate.GetComponent<InventoryUIController>() == null)
                {
                    continue;
                }

                if (path == PrefabPath)
                {
                    return candidate;
                }

                Debug.Log("[Level1InventoryUiAssembler] Using inventory prefab at " + path);
                return candidate;
            }

            return preferred;
        }

        public static InventoryItemData[] CreateOrUpdateItemDefinitions(bool preferBlenderIcon = true)
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Level1");
            EnsureFolder(ItemDataFolder);

            if (preferBlenderIcon)
            {
                EnsureFolder("Assets/Art");
                EnsureFolder("Assets/Art/Level1");
                EnsureFolder(BlenderIconFolder);
            }

            Texture2D iconSheet = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Spritesheet/sheet_white1x.png");
            InventoryItemData[] results = new InventoryItemData[6];
            results[0] = SaveItem("phone", Level1ItemCatalog.GetDisplayName("phone"), iconSheet, preferBlenderIcon);
            results[1] = SaveItem("wallet", Level1ItemCatalog.GetDisplayName("wallet"), iconSheet, preferBlenderIcon);
            results[2] = SaveItem("bus_ticket", Level1ItemCatalog.GetDisplayName("bus_ticket"), iconSheet, preferBlenderIcon);
            results[3] = SaveItem("id_card", Level1ItemCatalog.GetDisplayName("id_card"), iconSheet, preferBlenderIcon);
            results[4] = SaveItem("water_bottle", Level1ItemCatalog.GetDisplayName("water_bottle"), iconSheet, preferBlenderIcon);
            results[5] = SaveItem("tet_gift", Level1ItemCatalog.GetDisplayName("tet_gift"), iconSheet, preferBlenderIcon);
            AssetDatabase.SaveAssets();
            return results;
        }

        public static InventoryItemData[] AssignBlenderIconsToItemDefinitions()
        {
            InventoryItemData[] results = CreateOrUpdateItemDefinitions(preferBlenderIcon: true);
            AssetDatabase.Refresh();
            return results;
        }

        public static GameObject BuildOrUpdatePrefab()
        {
            KenneyRpgUiLibrary library = KenneyRpgUiLibrary.LoadDefault();
            if (library == null)
            {
                return null;
            }

            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/UI");

            Texture2D iconSheet = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Spritesheet/sheet_white1x.png");

            GameObject root = KenneyRpgUiWidgets.CreateRootCanvas("Level1InventoryUI");
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.sortingOrder = 40;

            InventoryUIController controller = root.AddComponent<InventoryUIController>();

            // Progress — top-left inset panel
            GameObject progressGroup = KenneyRpgUiWidgets.CreateSpritePanel("ProgressGroup", root.transform,
                library.panelInsetBeige, KenneyRpgUiWidgets.TitleBoardTint);
            LayoutRect(progressGroup.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -24f), new Vector2(300f, 88f));

            Text progressTitle = MainMenuTypography.CreateStyledText("ProgressTitle", progressGroup.transform,
                MenuTextStyle.Subtitle, TextAnchor.UpperLeft, "Tiến độ dọn đồ");
            DisableRaycast(progressTitle);
            Stretch(progressTitle.rectTransform, 14f);

            Text progressCount = MainMenuTypography.CreateStyledText("ProgressCount", progressGroup.transform,
                MenuTextStyle.Button, TextAnchor.LowerLeft, "0 / 6");
            DisableRaycast(progressCount);
            LayoutRect(progressCount.rectTransform,
                new Vector2(0f, 0f), new Vector2(1f, 0.45f), new Vector2(0f, 0f),
                new Vector2(14f, 10f), new Vector2(-14f, -8f));

            // Open bag button — top-right with Kenney Game Icon (persistent sprite asset)
            Sprite bagIcon = LoadPersistentGameIconSprite("basket", KenneyGameIconLoader.BlackIconFolder);
            Button openButton = CreateKenneyIconButton("OpenButton", root.transform, library,
                library.buttonRoundBrown, bagIcon, new Vector2(1f, 1f), new Vector2(-28f, -28f), 88f,
                new Color(0.24f, 0.16f, 0.1f, 1f));

            GameObject inventoryModal = new GameObject("InventoryModal", typeof(RectTransform));
            inventoryModal.transform.SetParent(root.transform, false);
            Stretch(inventoryModal.GetComponent<RectTransform>(), 0f);
            inventoryModal.SetActive(false);

            GameObject backdrop = KenneyRpgUiWidgets.CreateSpritePanel("Backdrop", inventoryModal.transform,
                null, new Color(0.04f, 0.03f, 0.02f, 0.62f));
            Stretch(backdrop.GetComponent<RectTransform>(), 0f);
            Button backdropButton = backdrop.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdropButton.targetGraphic = backdrop.GetComponent<Image>();

            GameObject panelFrame = KenneyRpgUiWidgets.CreateSpritePanel("PanelFrame", inventoryModal.transform,
                library.panelBrown, KenneyRpgUiWidgets.TitleBoardTint);
            LayoutRect(panelFrame.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1140f, 600f));
            panelFrame.GetComponent<Image>().type = Image.Type.Sliced;

            GameObject panelInset = KenneyRpgUiWidgets.CreateSpritePanel("PanelInset", panelFrame.transform,
                library.panelInsetBeigeLight, new Color(0.88f, 0.78f, 0.62f, 0.95f));
            Stretch(panelInset.GetComponent<RectTransform>(), 14f);
            panelInset.GetComponent<Image>().raycastTarget = false;

            GameObject titleBarTop = KenneyRpgUiWidgets.CreateSpritePanel("TitleBarTop", panelFrame.transform,
                library.barYellowMid, new Color(0.96f, 0.86f, 0.62f, 1f));
            LayoutRect(titleBarTop.GetComponent<RectTransform>(),
                new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -8f), new Vector2(0f, -18f));
            titleBarTop.GetComponent<Image>().raycastTarget = false;

            Text title = MainMenuTypography.CreateStyledText("Title", panelFrame.transform,
                MenuTextStyle.DialogTitle, TextAnchor.UpperCenter, "Túi đồ");
            DisableRaycast(title);
            LayoutRect(title.rectTransform,
                new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -28f), new Vector2(0f, -88f));

            Button closeButton = CreateKenneyIconButton("CloseButton", panelFrame.transform, library,
                library.buttonSquareBrown, library.iconCrossBrown, new Vector2(0.5f, 1f), new Vector2(0f, -14f), 36f,
                Color.white);
            closeButton.transform.SetAsLastSibling();

            GameObject inventorySide = new GameObject("InventorySide", typeof(RectTransform));
            inventorySide.transform.SetParent(panelFrame.transform, false);
            StretchHalf(inventorySide.GetComponent<RectTransform>(), true, 18f, 18f);

            GameObject noteSide = new GameObject("NoteSide", typeof(RectTransform));
            noteSide.transform.SetParent(panelFrame.transform, false);
            StretchHalf(noteSide.GetComponent<RectTransform>(), false, 18f, 18f);

            GameObject splitDivider = KenneyRpgUiWidgets.CreateSpritePanel("SplitDivider", panelFrame.transform,
                null, new Color(0.45f, 0.34f, 0.24f, 0.35f));
            LayoutRect(splitDivider.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(2f, -104f));
            splitDivider.GetComponent<Image>().raycastTarget = false;

            GameObject slotsRoot = new GameObject("Slots", typeof(RectTransform));
            slotsRoot.transform.SetParent(inventorySide.transform, false);
            LayoutRect(slotsRoot.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(420f, 300f));

            InventorySlotUI[] slotComponents = new InventorySlotUI[6];
            const int columns = 3;
            const int rows = 2;
            float slotWidth = 132f;
            float slotHeight = 132f;
            float startX = -((columns - 1) * slotWidth) * 0.5f;
            float startY = ((rows - 1) * slotHeight) * 0.5f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    slotComponents[index] = CreateSlot(slotsRoot.transform, library, index,
                        new Vector2(startX + col * slotWidth, startY - row * slotHeight));
                }
            }

            Text selectedName = MainMenuTypography.CreateStyledText("SelectedItemName", inventorySide.transform,
                MenuTextStyle.DialogBody, TextAnchor.MiddleCenter, "Select an item");
            DisableRaycast(selectedName);
            LayoutRect(selectedName.rectTransform,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(16f, 52f), new Vector2(-16f, 96f));

            Text closeHint = MainMenuTypography.CreateStyledText("CloseHint", inventorySide.transform,
                MenuTextStyle.Subtitle, TextAnchor.LowerCenter, "Press I or Esc to close");
            DisableRaycast(closeHint);
            LayoutRect(closeHint.rectTransform,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(16f, 16f), new Vector2(-16f, 44f));

            VehicleBookingNoteView vehicleBookingNote = EnsureVehicleBookingNoteInstance(noteSide.transform);
            LayoutEmbeddedNote(vehicleBookingNote);

            // Pickup popup
            GameObject pickupPopup = KenneyRpgUiWidgets.CreateSpritePanel("PickupPopup", root.transform,
                library.panelInsetBrown, new Color(0.16f, 0.34f, 0.18f, 0.95f));
            LayoutRect(pickupPopup.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(500f, 96f));
            pickupPopup.SetActive(false);

            Text popupText = MainMenuTypography.CreateStyledText("PopupText", pickupPopup.transform,
                MenuTextStyle.DialogBody, TextAnchor.MiddleCenter, string.Empty);
            DisableRaycast(popupText);
            Stretch(popupText.rectTransform, 12f);

            // Bottom interaction prompt (shown when near collectables)
            GameObject interactionPromptPanel = KenneyRpgUiWidgets.CreateSpritePanel("InteractionPromptPanel", root.transform,
                library.panelInsetBrown, new Color(0.14f, 0.1f, 0.08f, 0.88f));
            LayoutRect(interactionPromptPanel.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 28f), new Vector2(760f, 72f));
            interactionPromptPanel.GetComponent<Image>().type = Image.Type.Sliced;
            interactionPromptPanel.SetActive(false);

            Text interactionPromptText = MainMenuTypography.CreateStyledText("InteractionPromptText",
                interactionPromptPanel.transform, MenuTextStyle.DialogBody, TextAnchor.MiddleCenter, string.Empty);
            DisableRaycast(interactionPromptText);
            Stretch(interactionPromptText.rectTransform, 12f);

            // Help / movement guide button
            Button helpButton = KenneyRpgUiWidgets.CreateIconButton("HelpButton", root.transform, library, "?",
                () => { });
            LayoutRect(helpButton.GetComponent<RectTransform>(),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(24f, 24f), new Vector2(72f, 72f));
            helpButton.GetComponent<Image>().type = Image.Type.Sliced;

            GameObject helpPanel = KenneyRpgUiWidgets.CreateSpritePanel("HelpPanel", root.transform,
                library.panelBrown, KenneyRpgUiWidgets.TitleBoardTint);
            LayoutRect(helpPanel.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(620f, 300f));
            helpPanel.GetComponent<Image>().type = Image.Type.Sliced;
            helpPanel.SetActive(false);

            Text helpTitle = MainMenuTypography.CreateStyledText("HelpTitle", helpPanel.transform,
                MenuTextStyle.DialogTitle, TextAnchor.UpperCenter, "Hướng Dẫn Chơi");
            DisableRaycast(helpTitle);
            LayoutRect(helpTitle.rectTransform,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(0f, -88f));

            Text helpText = MainMenuTypography.CreateStyledText("HelpText", helpPanel.transform,
                MenuTextStyle.DialogBody, TextAnchor.UpperLeft,
                "Phím WASD / Mũi tên — Di chuyển\nShift Trái — Chạy nhanh\nE — Nhặt vật phẩm\nI — Mở hành lý + Giấy tờ\nEsc — Đóng bảng hướng dẫn");
            DisableRaycast(helpText);
            LayoutRect(helpText.rectTransform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(24f, 24f), new Vector2(-24f, -96f));

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("inventoryModalRoot").objectReferenceValue = inventoryModal;
            serializedController.FindProperty("inventoryPanel").objectReferenceValue = panelFrame;
            serializedController.FindProperty("backdropButton").objectReferenceValue = backdropButton;
            serializedController.FindProperty("openButton").objectReferenceValue = openButton;
            serializedController.FindProperty("vehicleBookingNote").objectReferenceValue = vehicleBookingNote;
            serializedController.FindProperty("closeButton").objectReferenceValue = closeButton;
            serializedController.FindProperty("titleText").objectReferenceValue = title;
            serializedController.FindProperty("selectedItemNameText").objectReferenceValue = selectedName;
            serializedController.FindProperty("closeHintText").objectReferenceValue = closeHint;
            serializedController.FindProperty("progressTitleText").objectReferenceValue = progressTitle;
            serializedController.FindProperty("progressCountText").objectReferenceValue = progressCount;
            serializedController.FindProperty("pickupPopup").objectReferenceValue = pickupPopup;
            serializedController.FindProperty("pickupPopupText").objectReferenceValue = popupText;
            serializedController.FindProperty("interactionPromptPanel").objectReferenceValue = interactionPromptPanel;
            serializedController.FindProperty("interactionPromptText").objectReferenceValue = interactionPromptText;
            serializedController.FindProperty("helpButton").objectReferenceValue = helpButton;
            serializedController.FindProperty("helpPanel").objectReferenceValue = helpPanel;
            serializedController.FindProperty("helpText").objectReferenceValue = helpText;
            serializedController.FindProperty("showMovementGuideOnStart").boolValue = true;
            serializedController.FindProperty("slots").arraySize = slotComponents.Length;
            for (int i = 0; i < slotComponents.Length; i++)
            {
                serializedController.FindProperty("slots").GetArrayElementAtIndex(i).objectReferenceValue =
                    slotComponents[i];
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefabRoot = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return prefabRoot;
        }

        public static void WireNoteButtonIntoOpenSceneMenu()
        {
            InventoryUIController controller = Object.FindAnyObjectByType<InventoryUIController>();
            if (controller == null)
            {
                Debug.LogWarning("[Level1InventoryUiAssembler] No InventoryUIController found in open scenes.");
                return;
            }

            EnsureCombinedInventoryLayout(controller);
            EditorUtility.SetDirty(controller);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[Level1InventoryUiAssembler] Applied combined inventory + note layout on " + controller.name);
        }

        public static void EnsureCombinedInventoryLayout(InventoryUIController controller)
        {
            if (controller == null)
            {
                return;
            }

            Transform root = controller.transform;
            SerializedObject serializedController = new SerializedObject(controller);

            Transform panelFrame = FindDeepChild(root, "PanelFrame");
            if (panelFrame == null)
            {
                Debug.LogWarning("[Level1InventoryUiAssembler] PanelFrame not found. Rebuild the inventory prefab.");
                return;
            }

            LayoutRect(panelFrame.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1140f, 600f));

            Transform titleBar = FindDeepChild(panelFrame, "TitleBarTop");
            if (titleBar != null)
            {
                LayoutRect(titleBar.GetComponent<RectTransform>(),
                    new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -8f), new Vector2(0f, -18f));
            }

            Text title = panelFrame.GetComponentInChildren<Text>(true);
            foreach (Text candidate in panelFrame.GetComponentsInChildren<Text>(true))
            {
                if (candidate != null && candidate.name == "Title")
                {
                    title = candidate;
                    break;
                }
            }

            if (title != null)
            {
                LayoutRect(title.rectTransform,
                    new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -28f), new Vector2(0f, -88f));
            }

            Transform inventorySide = EnsureSplitSide(panelFrame, "InventorySide", true);
            Transform noteSide = EnsureSplitSide(panelFrame, "NoteSide", false);
            EnsureSplitDivider(panelFrame);

            ReparentIfFound(panelFrame, inventorySide, "Slots");
            ReparentIfFound(panelFrame, inventorySide, "SelectedItemName");
            ReparentIfFound(panelFrame, inventorySide, "CloseHint");

            Transform strayNoteButton = FindDeepChild(root, "OpenNoteButton");
            if (strayNoteButton != null)
            {
                Object.DestroyImmediate(strayNoteButton.gameObject);
            }

            VehicleBookingNoteView noteView = root.GetComponentInChildren<VehicleBookingNoteView>(true);
            if (noteView == null)
            {
                noteView = EnsureVehicleBookingNoteInstance(noteSide);
            }
            else
            {
                noteView.transform.SetParent(noteSide, false);
                noteView.gameObject.SetActive(false);
            }

            LayoutEmbeddedNote(noteView);

            Button closeButton = FindChildButton(panelFrame, "CloseButton");
            if (closeButton != null)
            {
                LayoutCloseButton(closeButton);
            }

            serializedController.FindProperty("vehicleBookingNote").objectReferenceValue = noteView;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void EnsureNoteUiOnController(InventoryUIController controller)
        {
            EnsureCombinedInventoryLayout(controller);
        }

        private static Transform EnsureSplitSide(Transform panelFrame, string sideName, bool leftHalf)
        {
            Transform side = panelFrame.Find(sideName);
            if (side == null)
            {
                GameObject sideObject = new GameObject(sideName, typeof(RectTransform));
                sideObject.transform.SetParent(panelFrame, false);
                side = sideObject.transform;
            }

            StretchHalf(side.GetComponent<RectTransform>(), leftHalf, 18f, 18f);
            return side;
        }

        private static void EnsureSplitDivider(Transform panelFrame)
        {
            Transform divider = panelFrame.Find("SplitDivider");
            if (divider == null)
            {
                GameObject dividerObject = KenneyRpgUiWidgets.CreateSpritePanel("SplitDivider", panelFrame,
                    null, new Color(0.45f, 0.34f, 0.24f, 0.35f));
                divider = dividerObject.transform;
            }

            LayoutRect(divider.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(2f, -104f));
            divider.GetComponent<Image>().raycastTarget = false;
        }

        private static void ReparentIfFound(Transform searchRoot, Transform newParent, string childName)
        {
            Transform child = FindDeepChild(searchRoot, childName);
            if (child != null && child.parent != newParent)
            {
                child.SetParent(newParent, false);
            }
        }

        private static void LayoutEmbeddedNote(VehicleBookingNoteView noteView)
        {
            if (noteView == null)
            {
                return;
            }

            RectTransform noteRect = noteView.GetComponent<RectTransform>();
            noteRect.anchorMin = new Vector2(0.5f, 0.5f);
            noteRect.anchorMax = new Vector2(0.5f, 0.5f);
            noteRect.pivot = new Vector2(0.5f, 0.5f);
            noteRect.anchoredPosition = new Vector2(0f, -18f);
            noteRect.sizeDelta = new Vector2(468f, 608f);
            noteRect.localScale = Vector3.one;
            noteView.gameObject.SetActive(false);
        }

        private static void LayoutCloseButton(Button closeButton)
        {
            if (closeButton == null)
            {
                return;
            }

            LayoutRect(closeButton.GetComponent<RectTransform>(),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -14f), new Vector2(36f, 36f));

            Transform icon = closeButton.transform.Find("Icon");
            if (icon != null)
            {
                Stretch(icon.GetComponent<RectTransform>(), 9f);
            }
        }

        private static void StretchHalf(RectTransform rect, bool leftHalf, float paddingX, float paddingY)
        {
            if (leftHalf)
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0.5f, 1f);
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
            }

            rect.offsetMin = new Vector2(paddingX, paddingY);
            rect.offsetMax = new Vector2(-paddingX, -paddingY);
        }

        private static VehicleBookingNoteView EnsureVehicleBookingNoteInstance(Transform parent)
        {
            Transform existing = parent.Find("VehicleBookingNote");
            if (existing != null && existing.TryGetComponent(out VehicleBookingNoteView existingView))
            {
                existing.gameObject.SetActive(false);
                return existingView;
            }

            GameObject notePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VehicleBookingNotePrefabBuilder.PrefabPath);
            if (notePrefab == null)
            {
                VehicleBookingNotePrefabBuilder.BuildOrUpdatePrefab();
                notePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VehicleBookingNotePrefabBuilder.PrefabPath);
            }

            if (notePrefab == null)
            {
                Debug.LogError("[Level1InventoryUiAssembler] Vehicle booking note prefab is missing.");
                return null;
            }

            GameObject noteInstance = (GameObject)PrefabUtility.InstantiatePrefab(notePrefab, parent);
            noteInstance.name = "VehicleBookingNote";
            noteInstance.SetActive(false);
            return noteInstance.GetComponent<VehicleBookingNoteView>();
        }

        private static Button FindChildButton(Transform parent, string childName)
        {
            Transform found = parent.Find(childName);
            if (found != null && found.TryGetComponent(out Button button))
            {
                return button;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform deep = FindDeepChild(parent.GetChild(i), childName);
                if (deep != null && deep.TryGetComponent(out Button deepButton))
                {
                    return deepButton;
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

        private static Button CreateKenneyIconButton(string name, Transform parent, KenneyRpgUiLibrary library,
            Sprite frameSprite, Sprite icon, Vector2 anchor, Vector2 anchoredPosition, float size, Color iconTint)
        {
            GameObject buttonObject = KenneyRpgUiWidgets.CreateSpritePanel(name, parent,
                frameSprite, KenneyRpgUiWidgets.ButtonWarmOrangeBrown);
            LayoutRect(buttonObject.GetComponent<RectTransform>(),
                anchor, anchor, anchor, anchoredPosition, new Vector2(size, size));
            Image frameImage = buttonObject.GetComponent<Image>();
            frameImage.type = Image.Type.Sliced;

            Button button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = frameImage;
            buttonObject.AddComponent<MainMenuButtonFx>();

            if (icon != null)
            {
                GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObject.transform.SetParent(buttonObject.transform, false);
                Image iconImage = iconObject.GetComponent<Image>();
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                iconImage.color = iconTint;
                Stretch(iconObject.GetComponent<RectTransform>(), 18f);
            }

            return button;
        }

        private static Sprite LoadPersistentGameIconSprite(string iconName, string folder)
        {
            string path = folder + "/" + iconName + ".png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void DisableRaycast(Text label)
        {
            if (label != null)
            {
                label.raycastTarget = false;
            }
        }

        private static InventorySlotUI CreateSlot(Transform parent, KenneyRpgUiLibrary library, int index,
            Vector2 anchoredPosition)
        {
            GameObject slotRoot = KenneyRpgUiWidgets.CreateSpritePanel("Slot_" + index, parent,
                library.panelInsetBrown, new Color(0.62f, 0.52f, 0.4f, 0.92f));
            LayoutRect(slotRoot.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                anchoredPosition, new Vector2(118f, 118f));
            slotRoot.GetComponent<Image>().type = Image.Type.Sliced;

            GameObject slotInner = KenneyRpgUiWidgets.CreateSpritePanel("SlotInner", slotRoot.transform,
                library.panelInsetBeigeLight, new Color(0.78f, 0.68f, 0.54f, 0.88f));
            Stretch(slotInner.GetComponent<RectTransform>(), 6f);
            slotInner.GetComponent<Image>().raycastTarget = false;

            Button slotButton = slotRoot.AddComponent<Button>();
            slotButton.targetGraphic = slotRoot.GetComponent<Image>();
            slotButton.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = slotButton.colors;
            colors.highlightedColor = new Color(1.05f, 0.95f, 0.78f, 1f);
            colors.pressedColor = new Color(0.85f, 0.72f, 0.55f, 1f);
            slotButton.colors = colors;

            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(slotRoot.transform, false);
            Image icon = iconObject.GetComponent<Image>();
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            icon.color = KenneyGameIconLoader.InventoryIconTint;
            LayoutRect(iconObject.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(72f, 72f));

            GameObject emptyState = KenneyRpgUiWidgets.CreateSpritePanel("EmptyState", slotRoot.transform,
                library.panelBrown, new Color(0.14f, 0.1f, 0.08f, 0.42f));
            Stretch(emptyState.GetComponent<RectTransform>(), 8f);
            emptyState.GetComponent<Image>().raycastTarget = false;

            InventorySlotUI slotUi = slotRoot.AddComponent<InventorySlotUI>();
            SerializedObject serializedSlot = new SerializedObject(slotUi);
            serializedSlot.FindProperty("iconImage").objectReferenceValue = icon;
            serializedSlot.FindProperty("emptyState").objectReferenceValue = emptyState;
            serializedSlot.FindProperty("slotButton").objectReferenceValue = slotButton;
            serializedSlot.ApplyModifiedPropertiesWithoutUndo();
            return slotUi;
        }

        private static InventoryItemData SaveItem(string id, string displayName, Texture2D iconSheet,
            bool preferBlenderIcon = false)
        {
            string path = ItemDataFolder + "/" + id + ".asset";
            InventoryItemData asset = AssetDatabase.LoadAssetAtPath<InventoryItemData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<InventoryItemData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            Sprite icon = null;
            if (preferBlenderIcon)
            {
                icon = LoadPersistentGameIconSprite(id, BlenderIconFolder);
            }

            if (icon == null)
            {
                string iconKey = KenneyGameIconLoader.GetIconKeyForItem(id);
                icon = LoadPersistentGameIconSprite(iconKey, KenneyGameIconLoader.BlackIconFolder);
            }

            if (icon == null)
            {
                icon = KenneyGameIconLoader.GetIconForItem(id, iconSheet);
            }

            asset.SetEditorData(id, displayName, icon, true);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void LayoutRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }
    }
}
#endif
