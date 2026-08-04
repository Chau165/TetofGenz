using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Place on each packable object in the bedroom. Press E nearby to collect.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectableItem : Interactable
    {
        [Header("Item data")]
        [SerializeField] private InventoryItemData itemData;
        [SerializeField] private string itemId = "phone";
        [SerializeField] private string itemName = "Phone";
        [SerializeField] private Sprite itemIcon;
        [SerializeField] private bool isRequired = true;

        [Header("Scene object")]
        [SerializeField] private GameObject visualRoot;

        private bool collected;
        private Level1CollectableHighlight collectHighlight;

        public string ItemId => itemData != null ? itemData.ItemId : itemId;
        public string ItemName => itemData != null ? itemData.ItemName : itemName;
        public Sprite ItemIcon
        {
            get
            {
                if (itemData != null)
                {
                    return itemData.ResolvedIcon;
                }

                return KenneyGameIconLoader.ResolveItemIcon(itemIcon, ItemId);
            }
        }
        public bool IsRequired => itemData != null ? itemData.IsRequired : isRequired;
        public bool IsCollected => collected;

        private void Start()
        {
            SetPrompt(BuildCollectPrompt());
            EnsureIcon();
            EnsureCollectHighlight();
        }

        private void Reset()
        {
            Collider targetCollider = GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetCollider.isTrigger = true;
            }

            SetPrompt("Nhấn E để thu thập " + ItemName);
        }

        public override bool CanInteract()
        {
            if (collected)
            {
                return false;
            }

            InventoryManager inventory = InventoryManager.Instance;
            return inventory != null && !inventory.HasItem(ItemId);
        }

        public override void Interact(PlayerTopDownController player)
        {
            if (collected)
            {
                return;
            }

            InventoryManager inventory = InventoryManager.Instance;
            inventory?.AddItem(this);
        }

        public void MarkCollected()
        {
            collected = true;
            enabled = false;
            collectHighlight?.SetVisible(false);

            if (visualRoot != null)
            {
                visualRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void ApplyDefinition(InventoryItemData definition)
        {
            if (definition == null)
            {
                return;
            }

            itemData = definition;
            SetPrompt(BuildCollectPrompt());
            EnsureIcon();
        }

        /// <summary>Used by the Level 1 scene builder.</summary>
        public void Configure(string id, string displayName, GameObject visual, Sprite icon = null)
        {
            itemId = id;
            itemName = displayName;
            visualRoot = visual;
            itemIcon = icon;
            SetPrompt(BuildCollectPrompt());
            EnsureIcon();
            EnsureCollectHighlight();
        }

        /// <summary>Bridge from existing enum-based builder code.</summary>
        public void Configure(Level1PackingItemId id, GameObject visual, Sprite icon = null)
        {
            Configure(Level1ItemCatalog.ToItemId(id), Level1ItemCatalog.GetDisplayName(id), visual, icon);
        }

        public void EnsureIcon(Texture2D iconSheet = null)
        {
            if (ItemIcon != null)
            {
                return;
            }

            itemIcon = KenneyGameIconLoader.GetIconForItem(ItemId, iconSheet);
        }

        private string BuildCollectPrompt()
        {
            return "Nhấn E để thu thập " + ItemName;
        }

        private void EnsureCollectHighlight()
        {
            if (collected)
            {
                return;
            }

            Transform highlightTarget = visualRoot != null ? visualRoot.transform : transform;
            collectHighlight = GetComponent<Level1CollectableHighlight>();
            if (collectHighlight == null)
            {
                collectHighlight = gameObject.AddComponent<Level1CollectableHighlight>();
            }

            collectHighlight.Bind(highlightTarget);
            collectHighlight.SetVisible(true);
        }
    }
}
