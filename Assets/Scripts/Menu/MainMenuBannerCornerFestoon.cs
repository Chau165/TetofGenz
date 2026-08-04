using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Festive corner accents: leaf clusters, gold studs, and small blossom dots.
    /// </summary>
    public static class MainMenuBannerCornerFestoon
    {
        private static readonly Color BlossomYellow = new Color(1f, 0.9f, 0.35f, 0.92f);
        private static readonly Color BlossomRed = new Color(0.92f, 0.28f, 0.22f, 0.88f);
        private static readonly Color GoldStud = new Color(0.98f, 0.84f, 0.42f, 0.9f);

        public static void Build(Transform board, KenneyRpgUiLibrary library)
        {
            if (board == null || library == null)
            {
                return;
            }

            Sprite leaf = library.vineLeaf != null ? library.vineLeaf : library.barGreenHorizontalLeft;
            Sprite accent = library.barYellowMid != null ? library.barYellowMid : library.barRedHorizontal;
            if (leaf == null)
            {
                return;
            }

            GameObject root = new GameObject("BannerCornerFestoon", typeof(RectTransform));
            root.transform.SetParent(board, false);
            Stretch(root.GetComponent<RectTransform>());

            BuildCorner(root.transform, leaf, accent, new Vector2(-280f, 96f), CornerStyle.TopLeft);
            BuildCorner(root.transform, leaf, accent, new Vector2(280f, 94f), CornerStyle.TopRight);
            BuildCorner(root.transform, leaf, accent, new Vector2(-278f, -94f), CornerStyle.BottomLeft);
            BuildCorner(root.transform, leaf, accent, new Vector2(278f, -92f), CornerStyle.BottomRight);
        }

        private enum CornerStyle { TopLeft, TopRight, BottomLeft, BottomRight }

        private static void BuildCorner(Transform parent, Sprite leaf, Sprite accent, Vector2 anchor,
            CornerStyle style)
        {
            System.Random rng = new System.Random((int)style * 7919);

            float leafRotBase = style switch
            {
                CornerStyle.TopLeft => 35f,
                CornerStyle.TopRight => -35f,
                CornerStyle.BottomLeft => 145f,
                _ => -145f
            };

            for (int i = 0; i < 7; i++)
            {
                float angle = leafRotBase + i * 22f + Next(rng, -10f, 10f);
                float dist = Next(rng, 12f, 34f);
                Vector2 pos = anchor + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * dist;
                CreateLeaf(parent, leaf, pos, Next(rng, 16f, 26f), angle + Next(rng, -20f, 20f),
                    i % 3 == 0 ? BlossomYellow : new Color(0.78f, 0.98f, 0.62f, 0.95f), rng);
            }
        }

        private static Vector2 Offset(CornerStyle style, float x, float y)
        {
            return style switch
            {
                CornerStyle.TopRight or CornerStyle.BottomRight => new Vector2(-x, y),
                _ => new Vector2(x, y)
            };
        }

        private static void CreateLeaf(Transform parent, Sprite sprite, Vector2 pos, float size, float angle,
            Color color, System.Random rng)
        {
            GameObject obj = new GameObject("FestoonLeaf", typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(size, size * 0.82f);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            AmbientSway sway = obj.AddComponent<AmbientSway>();
            sway.Configure(Vector3.forward, Next(rng, 0.1f, 0.15f), Next(rng, 0.9f, 1.6f), Next(rng, 0f, 3f));
        }

        private static void CreateStud(Transform parent, Sprite sprite, Vector2 pos, Color color)
        {
            if (sprite == null)
            {
                return;
            }

            GameObject obj = new GameObject("FestoonStud", typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(14f, 14f);
        }

        private static float Next(System.Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
