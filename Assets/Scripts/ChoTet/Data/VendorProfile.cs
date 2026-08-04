using UnityEngine;

namespace ChoTet.Data
{
    [CreateAssetMenu(menuName = "ChoTet/Vendor Profile", fileName = "VendorProfile")]
    public class VendorProfile : ScriptableObject
    {
        public string vendorId = "vendor";
        public string displayName = "Vendor";
        [TextArea(2, 4)] public string greeting = "Moi con xem hang.";
        public ItemDefinition stockedItem;
        [Range(10, 100)] public int patience = 60;
        [Range(0f, 1f)] public float greed = 0.35f;
        [Range(0f, 1f)] public float honesty = 0.75f;
        [Range(0, 3)] public int familiarity = 0;
        public string mood = "binh thuong";
        public string favoriteApproach = "soft_bargain";
        public string zoneName = "Market";
    }
}
