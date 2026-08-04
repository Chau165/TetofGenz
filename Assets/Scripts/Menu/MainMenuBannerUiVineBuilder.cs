using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Curved Kenney sprite vines on the banner — reliable UI-layer foliage at corners.
    /// </summary>
    public static class MainMenuBannerUiVineBuilder
    {
        private const float SafeHalfWidth = 330f;
        private const float SafeHalfHeight = 118f;

        private static readonly Color VineColor = new Color(0.62f, 0.9f, 0.48f, 1f);
        private static readonly Color VineBackColor = new Color(0.48f, 0.72f, 0.38f, 0.82f);
        private static readonly Color LeafColor = new Color(0.75f, 0.98f, 0.58f, 1f);

        public static void BuildBack(Transform board, KenneyRpgUiLibrary library)
        {
            BuildLayer(board, library, front: false);
        }

        public static void BuildFront(Transform board, KenneyRpgUiLibrary library)
        {
            BuildLayer(board, library, front: true);
        }

        private static void BuildLayer(Transform board, KenneyRpgUiLibrary library, bool front)
        {
            if (board == null || library == null)
            {
                return;
            }

            Sprite leaf = library.vineLeaf != null ? library.vineLeaf : library.barGreenHorizontalMid;
            if (leaf == null)
            {
                return;
            }

            string rootName = front ? "BannerUiVinesFront" : "BannerUiVinesBack";
            GameObject root = new GameObject(rootName, typeof(RectTransform));
            root.transform.SetParent(board, false);
            Stretch(root.GetComponent<RectTransform>());

            System.Random rng = new System.Random(front ? 20260602 : 20260601);
            foreach (Corner corner in System.Enum.GetValues(typeof(Corner)))
            {
                foreach (VinePath path in GetPaths(corner, rng))
                {
                    if (path.Front != front)
                    {
                        continue;
                    }

                    AddUiVinePath(root.transform, leaf, path, rng);
                }
            }
        }

        private enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

        private readonly struct VinePath
        {
            public readonly Vector2 P0;
            public readonly Vector2 P1;
            public readonly Vector2 P2;
            public readonly Vector2 P3;
            public readonly bool Cubic;
            public readonly bool Front;
            public readonly int Segments;

            public VinePath(Vector2 p0, Vector2 p1, Vector2 p2, bool front, int segments)
            {
                P0 = p0;
                P1 = p1;
                P2 = p2;
                P3 = Vector2.zero;
                Cubic = false;
                Front = front;
                Segments = segments;
            }

            public VinePath(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, bool front, int segments)
            {
                P0 = p0;
                P1 = p1;
                P2 = p2;
                P3 = p3;
                Cubic = true;
                Front = front;
                Segments = segments;
            }
        }

        private static IEnumerable<VinePath> GetPaths(Corner corner, System.Random rng)
        {
            float j(float v) => v + (float)(rng.NextDouble() * 8.0 - 4.0);

            switch (corner)
            {
                case Corner.TopLeft:
                    yield return new VinePath(new Vector2(-318f, 112f), new Vector2(-288f, j(128f)), new Vector2(-248f, j(102f)),
                        front: true, segments: 7);
                    yield return new VinePath(new Vector2(-322f, 106f), new Vector2(-332f, j(48f)), new Vector2(-302f, j(16f)),
                        front: false, segments: 6);
                    break;
                case Corner.TopRight:
                    yield return new VinePath(new Vector2(318f, 110f), new Vector2(288f, j(126f)), new Vector2(248f, j(100f)),
                        front: false, segments: 7);
                    yield return new VinePath(new Vector2(322f, 104f), new Vector2(332f, j(46f)), new Vector2(300f, j(14f)),
                        front: true, segments: 6);
                    break;
                case Corner.BottomLeft:
                    yield return new VinePath(new Vector2(-316f, -110f), new Vector2(-286f, j(-126f)), new Vector2(-246f, j(-98f)),
                        front: false, segments: 6);
                    yield return new VinePath(new Vector2(-320f, -104f), new Vector2(-330f, j(-42f)), new Vector2(-298f, j(2f)),
                        front: true, segments: 5);
                    break;
                case Corner.BottomRight:
                    yield return new VinePath(new Vector2(316f, -108f), new Vector2(286f, j(-124f)), new Vector2(246f, j(-96f)),
                        front: true, segments: 6);
                    yield return new VinePath(new Vector2(320f, -102f), new Vector2(330f, j(-40f)), new Vector2(296f, j(4f)),
                        front: false, segments: 5);
                    break;
            }
        }

        private static void AddUiVinePath(Transform parent, Sprite leafSprite, VinePath path, System.Random rng)
        {
            int count = Mathf.Max(path.Segments, 4);
            Vector2 prevPos = path.P0;
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                Vector2 pos;
                Vector2 tangent;
                if (path.Cubic)
                {
                    pos = SampleCubic(path.P0, path.P1, path.P2, path.P3, t);
                    tangent = SampleCubicDeriv(path.P0, path.P1, path.P2, path.P3, t);
                }
                else
                {
                    pos = SampleQuad(path.P0, path.P1, path.P2, t);
                    tangent = SampleQuadDeriv(path.P0, path.P1, path.P2, t);
                }

                pos = ClampToSafeZone(pos);

                if (tangent.sqrMagnitude < 0.001f)
                {
                    tangent = (pos - prevPos).normalized;
                }
                else
                {
                    tangent.Normalize();
                }

                prevPos = pos;
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg + Next(rng, -12f, 12f);
                float cornerBias = 1f - t;
                float stemLen = Next(rng, 18f, 28f) * (0.85f + cornerBias * 0.2f);

                CreateLeafSegment(parent, leafSprite, pos, stemLen, angle,
                    path.Front ? VineColor : VineBackColor, Next(rng, 0.88f, 1f), rng);

                if (i % 2 == 0)
                {
                    Vector2 normal = new Vector2(-tangent.y, tangent.x);
                    for (int l = 0; l < 2; l++)
                    {
                        float side = l == 0 ? 1f : -1f;
                        Vector2 leafPos = ClampToSafeZone(pos + normal * Next(rng, 8f, 16f) * side);
                        CreateLeafSegment(parent, leafSprite, leafPos, Next(rng, 14f, 22f),
                            angle + Next(rng, 62f, 118f) * side, LeafColor, Next(rng, 0.82f, 1f), rng);
                    }
                }
            }
        }

        private static Vector2 ClampToSafeZone(Vector2 pos)
        {
            return new Vector2(
                Mathf.Clamp(pos.x, -SafeHalfWidth, SafeHalfWidth),
                Mathf.Clamp(pos.y, -SafeHalfHeight, SafeHalfHeight));
        }

        private static void CreateLeafSegment(Transform parent, Sprite sprite, Vector2 pos, float size, float angle,
            Color color, float alphaScale, System.Random rng)
        {
            GameObject obj = new GameObject("VineBit", typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = color * new Color(1f, 1f, 1f, alphaScale);
            image.raycastTarget = false;

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(size, size * 0.75f);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            AmbientSway sway = obj.AddComponent<AmbientSway>();
            sway.Configure(Vector3.forward, Next(rng, 0.1f, 0.15f), Next(rng, 0.9f, 1.5f), Next(rng, 0f, 4f));
        }

        private static Vector2 SampleQuad(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        private static Vector2 SampleQuadDeriv(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        }

        private static Vector2 SampleCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
        }

        private static Vector2 SampleCubicDeriv(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return 3f * u * u * (p1 - p0) + 6f * u * t * (p2 - p1) + 3f * t * t * (p3 - p2);
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
