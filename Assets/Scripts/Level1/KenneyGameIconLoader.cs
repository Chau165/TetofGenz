using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Loads Kenney Game Icons from the black 1x spritesheet (TexturePacker XML rects).
    /// Default sheet: Resources/KenneyGameIcons/sheet_black1x
    /// Individual icons: Assets/PNG/Black/1x/{name}.png (used on InventoryItemData assets).
    /// </summary>
    public static class KenneyGameIconLoader
    {
        public static readonly Color InventoryIconTint = new Color(0.22f, 0.15f, 0.1f, 1f);
        public const string BlackIconFolder = "Assets/PNG/Black/1x";
        private static readonly Dictionary<string, Rect> IconRects = new Dictionary<string, Rect>
        {
            // Rects from Assets/Spritesheet/sheet_white1x.xml (texture height = 500, Y flipped for Unity).
            { "phone", new Rect(200f, 150f, 50f, 50f) },
            { "cart", new Rect(450f, 400f, 50f, 50f) },
            { "export", new Rect(400f, 450f, 50f, 50f) },
            { "open", new Rect(200f, 50f, 50f, 50f) },
            { "menuList", new Rect(250f, 200f, 50f, 50f) },
            { "basket", new Rect(0f, 350f, 50f, 50f) },
            { "door", new Rect(400f, 100f, 50f, 50f) },
            { "locked", new Rect(300f, 450f, 50f, 50f) },
        };

        private static readonly Dictionary<string, string> ItemIconKeys = new Dictionary<string, string>
        {
            { "phone", "phone" },
            { "smartphone", "phone" },
            { "wallet", "cart" },
            { "bus_ticket", "export" },
            { "clothes", "export" },
            { "water_bottle", "open" },
            { "id_card", "menuList" },
            { "documents", "menuList" },
            { "tet_gift", "basket" },
            { "snacks", "basket" },
        };

        private static Texture2D cachedSheet;
        private static readonly Dictionary<string, Sprite> CachedSprites = new Dictionary<string, Sprite>();

        public static string GetIconKeyForItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return "basket";
            }

            return ItemIconKeys.TryGetValue(itemId, out string key) ? key : itemId;
        }

        public static Sprite GetIcon(string iconKey, Texture2D sheet = null)
        {
            if (string.IsNullOrEmpty(iconKey))
            {
                return null;
            }

            if (CachedSprites.TryGetValue(iconKey, out Sprite cached))
            {
                return cached;
            }

            Texture2D texture = sheet != null ? sheet : GetDefaultSheet();
            if (texture == null || !IconRects.TryGetValue(iconKey, out Rect rect))
            {
                return null;
            }

            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 50f);
            sprite.name = "KenneyIcon_" + iconKey;
            CachedSprites[iconKey] = sprite;
            return sprite;
        }

        public static Sprite GetIconForItem(string itemId, Texture2D sheet = null)
        {
            return GetIcon(GetIconKeyForItem(itemId), sheet);
        }

        /// <summary>Returns the assigned sprite or a Kenney fallback for UI / world use.</summary>
        public static Sprite ResolveItemIcon(Sprite assignedIcon, string itemId)
        {
            if (assignedIcon != null)
            {
                return assignedIcon;
            }

            return GetIconForItem(itemId);
        }

        public static void ClearCache()
        {
            foreach (KeyValuePair<string, Sprite> pair in CachedSprites)
            {
                if (pair.Value != null)
                {
                    Object.Destroy(pair.Value);
                }
            }

            CachedSprites.Clear();
            cachedSheet = null;
        }

        private static Texture2D GetDefaultSheet()
        {
            if (cachedSheet != null)
            {
                return cachedSheet;
            }

            cachedSheet = Resources.Load<Texture2D>("KenneyGameIcons/sheet_black1x");
            if (cachedSheet == null)
            {
                cachedSheet = Resources.Load<Texture2D>("KenneyGameIcons/sheet_white1x");
            }

            return cachedSheet;
        }
    }
}
