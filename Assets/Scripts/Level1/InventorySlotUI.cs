using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1
{
    /// <summary>
    /// One inventory slot on the imported UI prefab.
    /// Attach to each slot root and wire Icon / EmptyState / Button in the Inspector.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Button slotButton;

        private string boundItemId;
        private Action<InventorySlotUI> onSelected;

        public string BoundItemId => boundItemId;

        private void Awake()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(HandleClick);
            }
        }

        public void ConfigureSelection(Action<InventorySlotUI> selectedCallback)
        {
            onSelected = selectedCallback;
        }

        public void BindCollected(InventoryItemData item)
        {
            boundItemId = item != null ? item.ItemId : string.Empty;
            if (iconImage != null)
            {
            Sprite sprite = item != null ? item.ResolvedIcon : null;
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
                iconImage.color = item != null && item.Icon != null
                    ? Color.white
                    : KenneyGameIconLoader.InventoryIconTint;
            }

            if (emptyState != null)
            {
                emptyState.SetActive(false);
            }

            if (slotButton != null)
            {
                slotButton.interactable = item != null;
            }
        }

        public void BindEmpty()
        {
            boundItemId = string.Empty;
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (emptyState != null)
            {
                emptyState.SetActive(true);
            }

            if (slotButton != null)
            {
                slotButton.interactable = false;
            }
        }

        public InventoryItemData ResolveItemData()
        {
            if (string.IsNullOrEmpty(boundItemId))
            {
                return null;
            }

            InventoryManager manager = InventoryManager.Instance;
            return manager != null ? manager.GetItemData(boundItemId) : null;
        }

        private void HandleClick()
        {
            onSelected?.Invoke(this);
        }
    }
}
