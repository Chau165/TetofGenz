using UnityEngine;

namespace ChoTet.Level1.UI
{
    /// <summary>Runtime rounded-rect sprites for license plate UI (white fill + black border).</summary>
    public static class VehicleLicensePlateSpriteUtility
    {
        private static Sprite cachedPlateSprite;
        private static Sprite cachedBorderSprite;

        public static Sprite GetPlateFillSprite(int radius = 8)
        {
            if (cachedPlateSprite == null)
            {
                cachedPlateSprite = CreateRoundedSprite(128, 64, radius, Color.white);
            }

            return cachedPlateSprite;
        }

        public static Sprite GetPlateBorderSprite(int radius = 8)
        {
            if (cachedBorderSprite == null)
            {
                cachedBorderSprite = CreateRoundedSprite(128, 64, radius + 2, Color.black);
            }

            return cachedBorderSprite;
        }

        private static Sprite CreateRoundedSprite(int width, int height, int radius, Color fill)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Vector2 size = new Vector2(width, height);
            float r = Mathf.Clamp(radius, 1f, Mathf.Min(width, height) * 0.5f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = RoundedRectAlpha(new Vector2(x + 0.5f, y + 0.5f), size, r);
                    texture.SetPixel(x, y, new Color(fill.r, fill.g, fill.b, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static float RoundedRectAlpha(Vector2 point, Vector2 size, float radius)
        {
            Vector2 half = size * 0.5f;
            Vector2 center = half;
            Vector2 q = new Vector2(Mathf.Abs(point.x - center.x), Mathf.Abs(point.y - center.y));
            Vector2 box = half - new Vector2(radius, radius);
            if (q.x <= box.x || q.y <= box.y)
            {
                return 1f;
            }

            Vector2 corner = q - box;
            return corner.sqrMagnitude <= radius * radius ? 1f : 0f;
        }
    }
}
