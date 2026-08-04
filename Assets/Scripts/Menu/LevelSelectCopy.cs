namespace ChoTet.Menu
{
    public static class LevelSelectCopy
    {
        public const string ScreenTitle = "CHỌN CẤP ĐỘ";

        public const string BackButton = "Quay Lại";
        public const string SettingsButtonSymbol = "\u2699";
        public const string InfoButtonSymbol = "i";
        public const string CharacterButtonLabel = "Nhân Vật";

        public const string UnlockedStatus = "ĐÃ MỞ KHÓA";
        public const string LockedStatus = "ĐANG PHÁT TRIỂN";
        public const string ComingSoonLabel = "SẮP RA MẮT";
        public const string LockIconSymbol = "\u26D4";

        public const string LockedPopupTitle = "Thông Báo";
        public const string LockedPopupBody = "Cấp độ này đang được phát triển.";
        public const string CloseButton = "Đóng";

        public const string SettingsTitle = "Cài Đặt";
        public const string SettingsBody = "Các tùy chọn âm thanh, đồ họa và điều khiển sẽ xuất hiện ở đây.";

        public const string InfoTitle = "Thông Tin";
        public const string InfoBody =
            "Hành trình Tết gồm 2 cấp độ:\n" +
            "Cấp độ 1 — rời thành phố, về quê đón Tết cùng gia đình.\n" +
            "Cấp độ 2 — đi chợ Tết và viếng chùa đầu năm.";

        public static readonly LevelSelectLevelDefinition[] Levels =
        {
            new LevelSelectLevelDefinition(1, "Cấp độ 1", "Đường Về Quê", true,
                "Assets/Resources/Menu/LevelSelectThumbs/level1_home.png"),
            new LevelSelectLevelDefinition(2, "Cấp độ 2", "Đi Chợ Tết", true,
                "Assets/Resources/Menu/LevelSelectThumbs/level2_market.png")
        };
    }

    public readonly struct LevelSelectLevelDefinition
    {
        public readonly int Index;
        public readonly string Title;
        public readonly string Subtitle;
        public readonly bool IsUnlocked;
        public readonly string ThumbnailAssetPath;
        public readonly string[] ThumbnailOverlayPaths;

        public LevelSelectLevelDefinition(int index, string title, string subtitle, bool isUnlocked,
            string thumbnailAssetPath, params string[] thumbnailOverlayPaths)
        {
            Index = index;
            Title = title;
            Subtitle = subtitle;
            IsUnlocked = isUnlocked;
            ThumbnailAssetPath = thumbnailAssetPath;
            ThumbnailOverlayPaths = thumbnailOverlayPaths ?? System.Array.Empty<string>();
        }
    }
}
