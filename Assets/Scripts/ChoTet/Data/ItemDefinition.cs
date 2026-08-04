using UnityEngine;

namespace ChoTet.Data
{
    [CreateAssetMenu(menuName = "ChoTet/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId = "item";
        public string displayName = "Item";
        [TextArea(2, 4)] public string description = "Tet item.";
        public string category = "market";
        public int basePrice = 100000;
        public int capacityCost = 2;
        public int minQuality = 55;
        public int maxQuality = 85;
        public int presentationBonus = 5;
        public int traditionBonus = 8;
        public int socialBonus = 5;
        public int luckWeight = 0;
        public string qualityFocus = "freshness";
        public bool perishable = false;
    }
}
