using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Composites layered textures into level card thumbnail areas.
    /// </summary>
    public static class LevelSelectThumbnailBuilder
    {
        public static void Build(Transform thumbRoot, LevelSelectLevelDefinition definition, KenneyRpgUiLibrary library)
        {
            if (thumbRoot == null)
            {
                return;
            }

            BuildStaticImage(thumbRoot, definition, library);
        }

        public static void BuildStaticImage(Transform thumbRoot, LevelSelectLevelDefinition definition,
            KenneyRpgUiLibrary library)
        {
            Color wash = definition.IsUnlocked
                ? new Color(1f, 0.98f, 0.92f, 1f)
                : new Color(0.62f, 0.58f, 0.52f, 0.92f);

            GameObject washPanel = KenneyRpgUiWidgets.CreateSpritePanel("ThumbnailWash", thumbRoot, null, wash);
            KenneyRpgUiWidgets.Stretch(washPanel.GetComponent<RectTransform>());
            washPanel.GetComponent<Image>().raycastTarget = false;

            ApplyCoverLayer(thumbRoot, "ThumbnailBase", definition.ThumbnailAssetPath,
                definition.IsUnlocked ? 1f : 0.88f);

            if (definition.ThumbnailOverlayPaths != null && definition.ThumbnailOverlayPaths.Length > 0)
            {
                for (int i = 0; i < definition.ThumbnailOverlayPaths.Length; i++)
                {
                    float alpha = definition.IsUnlocked ? 0.92f - i * 0.08f : 0.65f - i * 0.06f;
                    ApplyLayer(thumbRoot, "ThumbnailOverlay" + i, definition.ThumbnailOverlayPaths[i],
                        new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f),
                        Vector2.zero, Vector2.zero, alpha);
                }
            }

            ApplyFrameOverlay(thumbRoot, definition, library);
        }

        public static void ApplyFrameOverlay(Transform thumbRoot, LevelSelectLevelDefinition definition,
            KenneyRpgUiLibrary library)
        {
            if (definition.Index == 2 && definition.IsUnlocked)
            {
                GameObject warmGlow = KenneyRpgUiWidgets.CreateSpritePanel("TetWarmGlow", thumbRoot,
                    library != null ? library.barRedHorizontal : null,
                    new Color(0.95f, 0.28f, 0.18f, 0.14f));
                KenneyRpgUiWidgets.Stretch(warmGlow.GetComponent<RectTransform>(), -6f);
                warmGlow.GetComponent<Image>().raycastTarget = false;

                GameObject lanternGlow = KenneyRpgUiWidgets.CreateSpritePanel("LanternGlow", thumbRoot,
                    library != null ? library.barYellowMid : null,
                    new Color(1f, 0.86f, 0.42f, 0.22f));
                RectTransform lanternRect = lanternGlow.GetComponent<RectTransform>();
                lanternRect.anchorMin = new Vector2(0.55f, 0.45f);
                lanternRect.anchorMax = new Vector2(1f, 1f);
                lanternRect.offsetMin = Vector2.zero;
                lanternRect.offsetMax = Vector2.zero;
                lanternGlow.GetComponent<Image>().raycastTarget = false;
            }

            GameObject frame = KenneyRpgUiWidgets.CreateSpritePanel("ThumbnailFrame", thumbRoot,
                library != null ? library.panelInsetBrown : null,
                new Color(0.45f, 0.32f, 0.2f, 0.55f));
            KenneyRpgUiWidgets.Stretch(frame.GetComponent<RectTransform>(), 0f);
            frame.GetComponent<Image>().type = Image.Type.Sliced;
            frame.GetComponent<Image>().raycastTarget = false;

            GameObject vignette = KenneyRpgUiWidgets.CreateSpritePanel("ThumbnailVignette", thumbRoot, null,
                new Color(0.06f, 0.03f, 0.02f, definition.IsUnlocked ? 0.08f : 0.22f));
            KenneyRpgUiWidgets.Stretch(vignette.GetComponent<RectTransform>());
            vignette.GetComponent<Image>().raycastTarget = false;
        }

        private static void ApplyCoverLayer(Transform parent, string name, string assetPath, float alpha)
        {
            Texture texture = LoadTexture(assetPath);
            if (texture == null)
            {
                return;
            }

            GameObject layerObject = new GameObject(name, typeof(RectTransform), typeof(RawImage),
                typeof(MainMenuBackgroundCover));
            layerObject.transform.SetParent(parent, false);
            RawImage rawImage = layerObject.GetComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.raycastTarget = false;
            rawImage.color = new Color(1f, 1f, 1f, alpha);

            KenneyRpgUiWidgets.Stretch(layerObject.GetComponent<RectTransform>());
            layerObject.GetComponent<MainMenuBackgroundCover>().ApplyCover();
        }

        private static void ApplyLayer(Transform parent, string name, string assetPath, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float alpha)
        {
            Texture texture = LoadTexture(assetPath);
            if (texture == null)
            {
                return;
            }

            GameObject layerObject = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            layerObject.transform.SetParent(parent, false);
            RawImage rawImage = layerObject.GetComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.raycastTarget = false;
            rawImage.color = new Color(1f, 1f, 1f, alpha);

            RectTransform rect = layerObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static Texture LoadTexture(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            string resourcePath = ToResourcesPath(assetPath);
            if (!string.IsNullOrEmpty(resourcePath))
            {
                Texture resourceTexture = Resources.Load<Texture>(resourcePath);
                if (resourceTexture != null)
                {
                    return resourceTexture;
                }
            }

#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
#else
            return null;
#endif
        }

        private static string ToResourcesPath(string assetPath)
        {
            const string resourcesRoot = "Assets/Resources/";
            if (!assetPath.StartsWith(resourcesRoot))
            {
                return null;
            }

            string relative = assetPath.Substring(resourcesRoot.Length);
            int dot = relative.LastIndexOf('.');
            return dot >= 0 ? relative.Substring(0, dot) : relative;
        }
    }
}
