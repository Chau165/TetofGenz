namespace ChoTet.Level1
{
    public enum Level1PackingItemId
    {
        Smartphone,
        Wallet,
        Clothes,
        WaterBottle,
        PersonalDocuments,
        Snacks
    }

    public static class Level1PackingItemLabels
    {
        public static string GetDisplayName(Level1PackingItemId itemId)
        {
            switch (itemId)
            {
                case Level1PackingItemId.Smartphone:
                    return "Smartphone";
                case Level1PackingItemId.Wallet:
                    return "Wallet";
                case Level1PackingItemId.Clothes:
                    return "Clothes";
                case Level1PackingItemId.WaterBottle:
                    return "Water Bottle";
                case Level1PackingItemId.PersonalDocuments:
                    return "Personal Documents";
                case Level1PackingItemId.Snacks:
                    return "Snacks";
                default:
                    return itemId.ToString();
            }
        }
    }
}
