namespace ChoTet.Level1
{
    /// <summary>Maps Level 1 enum ids to inventory string ids and display names.</summary>
    public static class Level1ItemCatalog
    {
        public const int RequiredItemCount = 6;

        public static string ToItemId(Level1PackingItemId itemId)
        {
            switch (itemId)
            {
                case Level1PackingItemId.Smartphone:
                    return "phone";
                case Level1PackingItemId.Wallet:
                    return "wallet";
                case Level1PackingItemId.Clothes:
                    return "bus_ticket";
                case Level1PackingItemId.WaterBottle:
                    return "water_bottle";
                case Level1PackingItemId.PersonalDocuments:
                    return "id_card";
                case Level1PackingItemId.Snacks:
                    return "tet_gift";
                default:
                    return itemId.ToString().ToLowerInvariant();
            }
        }

        public static string GetDisplayName(Level1PackingItemId itemId)
        {
            return GetDisplayName(ToItemId(itemId));
        }

        public static string GetDisplayName(string itemId)
        {
            switch (itemId)
            {
                case "phone":
                    return "Điện thoại";
                case "wallet":
                    return "Ví tiền";
                case "bus_ticket":
                    return "Vé xe";
                case "water_bottle":
                    return "Chai nước";
                case "id_card":
                    return "Căn cước công dân";
                case "tet_gift":
                    return "Quà Tết";
                default:
                    return itemId;
            }
        }
    }
}
