using System;
using System.Collections.Generic;
using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Stores collected items, updates UI, and unlocks the door when packing is complete.
    /// </summary>
    public class InventoryManager : MonoBehaviour, IGameplayPromptSource
    {
        public static InventoryManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private InventoryUIController inventoryUi;
        [SerializeField] private DoorInteraction departureDoor;

        [Header("Required items")]
        [SerializeField] private InventoryItemData[] requiredItemDefinitions = Array.Empty<InventoryItemData>();

        private readonly HashSet<string> collectedItemIds = new HashSet<string>();
        private readonly List<CollectableItem> registeredCollectables = new List<CollectableItem>();

        public event Action OnInventoryChanged;

        public bool IsModalOpen => inventoryUi != null && inventoryUi.IsModalOpen;
        public bool IsReadyForDeparture { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (inventoryUi == null)
            {
                inventoryUi = FindAnyObjectByType<InventoryUIController>();
            }

            if (departureDoor == null)
            {
                departureDoor = FindAnyObjectByType<DoorInteraction>();
            }

            EnsureRequiredDefinitions();
            RegisterCollectablesInScene();
            inventoryUi?.Initialize(this, requiredItemDefinitions);
            RefreshUi();
            departureDoor?.SetLocked(true);
        }

        public void Configure(InventoryUIController ui, DoorInteraction door, InventoryItemData[] definitions)
        {
            inventoryUi = ui;
            departureDoor = door;
            if (definitions != null && definitions.Length > 0)
            {
                requiredItemDefinitions = definitions;
            }
        }

        public void SetPrompt(string value)
        {
            inventoryUi?.SetPrompt(value);
        }

        public bool AddItem(CollectableItem item)
        {
            if (item == null || item.IsCollected)
            {
                return false;
            }

            string itemId = item.ItemId;
            if (collectedItemIds.Contains(itemId))
            {
                return false;
            }

            collectedItemIds.Add(itemId);
            item.MarkCollected();

            string displayName = GetItemData(itemId)?.ItemName ?? item.ItemName;
            inventoryUi?.ShowPickupPopup("Đã thu thập: " + displayName);
            RefreshUi();
            OnInventoryChanged?.Invoke();

            if (HasAllRequiredItems())
            {
                HandlePackingComplete();
            }

            return true;
        }

        public bool HasItem(string itemId)
        {
            return collectedItemIds.Contains(itemId);
        }

        public bool HasAllRequiredItems()
        {
            return GetCollectedCount() >= GetRequiredCount();
        }

        public int GetCollectedCount()
        {
            return collectedItemIds.Count;
        }

        public int GetRequiredCount()
        {
            return requiredItemDefinitions.Length > 0
                ? requiredItemDefinitions.Length
                : Level1ItemCatalog.RequiredItemCount;
        }

        public IReadOnlyList<InventoryItemData> GetRequiredItemDefinitions()
        {
            return requiredItemDefinitions;
        }

        public InventoryItemData GetItemData(string itemId)
        {
            for (int i = 0; i < requiredItemDefinitions.Length; i++)
            {
                InventoryItemData data = requiredItemDefinitions[i];
                if (data != null && data.ItemId == itemId)
                {
                    return data;
                }
            }

            return null;
        }

        public IReadOnlyList<CollectableItem> GetRegisteredCollectables()
        {
            return registeredCollectables;
        }

        private void EnsureRequiredDefinitions()
        {
            if (requiredItemDefinitions != null && requiredItemDefinitions.Length >= Level1ItemCatalog.RequiredItemCount)
            {
                return;
            }

            InventoryItemData[] loaded = Resources.LoadAll<InventoryItemData>("Level1/InventoryItems");
            if (loaded != null && loaded.Length > 0)
            {
                Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.ItemId, b.ItemId));
                requiredItemDefinitions = loaded;
            }
        }

        private void RegisterCollectablesInScene()
        {
            registeredCollectables.Clear();
            CollectableItem[] items = FindObjectsByType<CollectableItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < items.Length; i++)
            {
                CollectableItem item = items[i];
                if (item == null || !item.IsRequired)
                {
                    continue;
                }

                item.ApplyDefinition(GetItemData(item.ItemId));
                if (!registeredCollectables.Exists(existing => existing.ItemId == item.ItemId))
                {
                    registeredCollectables.Add(item);
                }
            }

            registeredCollectables.Sort((a, b) => string.CompareOrdinal(a.ItemId, b.ItemId));
        }

        private void RefreshUi()
        {
            inventoryUi?.UpdateProgress(GetCollectedCount(), GetRequiredCount());
            inventoryUi?.RefreshAllSlots();
        }

        private void HandlePackingComplete()
        {
            SetPrompt(string.Empty);
            departureDoor?.SetLocked(false);
            IsReadyForDeparture = true;
            inventoryUi?.ShowPickupPopup("Đã đóng gói đủ! Hãy ra cửa để lên đường.");
        }
    }
}
