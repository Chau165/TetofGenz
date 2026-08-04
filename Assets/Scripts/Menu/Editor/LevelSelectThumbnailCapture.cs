#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Menu.Editor
{
    public static class LevelSelectThumbnailCapture
    {
        private const string OutputFolder = "Assets/Resources/Menu/LevelSelectThumbs";
        private const int CaptureWidth = 960;
        private const int CaptureHeight = 448;

        public static void CaptureAll()
        {
            CaptureAllInternal();
        }

        /// <summary>Batchmode entry point for automated thumbnail regeneration.</summary>
        public static void CaptureAllBatch()
        {
            CaptureAllInternal();
        }

        private static void CaptureAllInternal()
        {
            Directory.CreateDirectory(OutputFolder);

            CaptureLevel(1, "level1_home.png", LevelSelectThumbnailDiorama.BuildHomeScene);
            CaptureLevel(2, "level2_market.png", LevelSelectThumbnailDiorama.BuildMarketScene);

            AssetDatabase.Refresh();
            Debug.Log("[LevelSelectThumbnailCapture] Saved Level 1 and Level 2 thumbnails to " + OutputFolder +
                      ". Rebuild the level select scene or enter Play mode to see updates.");
        }

        private static void CaptureLevel(int levelIndex, string fileName,
            System.Func<Transform, LevelSelectThumbnailDiorama.ThumbnailScene> buildScene)
        {
            GameObject root = new GameObject("LevelSelectThumbnailCaptureRoot");
            root.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                LevelSelectThumbnailDiorama.ThumbnailScene scene = buildScene(root.transform);
                if (scene.Camera == null)
                {
                    Debug.LogError("[LevelSelectThumbnailCapture] Missing camera for level " + levelIndex);
                    return;
                }

                Texture2D capture = RenderCamera(scene.Camera, CaptureWidth, CaptureHeight);
                capture = EnhanceContrast(capture, levelIndex == 2 ? 1.08f : 1.04f, levelIndex == 2 ? 1.06f : 1.03f);

                string outputPath = Path.Combine(OutputFolder, fileName);
                File.WriteAllBytes(outputPath, capture.EncodeToPNG());
                Object.DestroyImmediate(capture);

                Debug.Log("[LevelSelectThumbnailCapture] Level " + levelIndex + " -> " + outputPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Texture2D RenderCamera(Camera camera, int width, int height)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;

            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
            return texture;
        }

        private static Texture2D EnhanceContrast(Texture2D source, float contrast, float saturation)
        {
            Color[] pixels = source.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color color = pixels[i];
                color.r = ApplyContrastChannel(color.r, contrast);
                color.g = ApplyContrastChannel(color.g, contrast);
                color.b = ApplyContrastChannel(color.b, contrast);

                float gray = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
                color.r = Mathf.Lerp(gray, color.r, saturation);
                color.g = Mathf.Lerp(gray, color.g, saturation);
                color.b = Mathf.Lerp(gray, color.b, saturation);
                pixels[i] = color;
            }

            source.SetPixels(pixels);
            source.Apply();
            return source;
        }

        private static float ApplyContrastChannel(float channel, float contrast)
        {
            return Mathf.Clamp01((channel - 0.5f) * contrast + 0.5f);
        }
    }
}
#endif
