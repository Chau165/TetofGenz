using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Defines one packable item for Level 1 (icon, id, display name).
    /// Create assets via Tools → Cho Tet → Create Level 1 Inventory Items.
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryItem", menuName = "Cho Tet/Level 1/Inventory Item")]
    public class InventoryItemData : ScriptableObject
    {
        [SerializeField] private string itemId = "phone";
        [SerializeField] private string itemName = "Phone";
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isRequired = true;

        public string ItemId => itemId;
        public string ItemName => itemName;
        public Sprite Icon => icon;
        public Sprite ResolvedIcon => KenneyGameIconLoader.ResolveItemIcon(icon, itemId);
        public bool IsRequired => isRequired;

#if UNITY_EDITOR
        public void SetEditorData(string id, string displayName, Sprite itemIcon, bool required)
        {
            itemId = id;
            itemName = displayName;
            icon = itemIcon;
            isRequired = required;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
