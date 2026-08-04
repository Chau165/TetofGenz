using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Menu
{
    /// <summary>
    /// Renders a hidden 3D diorama into the level card thumbnail area.
    /// </summary>
    public class LevelSelectLiveThumbnail : MonoBehaviour
    {
        private const int RenderWidth = 960;
        private const int RenderHeight = 448;

        private LevelSelectLevelDefinition definition;
        private KenneyRpgUiLibrary library;
        private RawImage targetImage;

        public void Configure(LevelSelectLevelDefinition levelDefinition, KenneyRpgUiLibrary uiLibrary)
        {
            definition = levelDefinition;
            library = uiLibrary;
            StartCoroutine(RenderWhenReady());
        }

        private IEnumerator RenderWhenReady()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            RenderDiorama();
        }

        private void RenderDiorama()
        {
#if !UNITY_EDITOR
            LevelSelectThumbnailBuilder.BuildStaticImage(transform, definition, library);
            return;
#else
            Vector3 hiddenOffset = new Vector3(5000f + definition.Index * 120f, -800f, 5000f);
            GameObject root = new GameObject("LiveThumbnailRoot_" + definition.Index);
            root.transform.position = hiddenOffset;
            root.hideFlags = HideFlags.HideAndDontSave;

            Material previousSkybox = RenderSettings.skybox;
            bool previousFog = RenderSettings.fog;
            Color previousFogColor = RenderSettings.fogColor;

            try
            {
                LevelSelectThumbnailDiorama.ThumbnailScene scene = definition.Index == 2
                    ? LevelSelectThumbnailDiorama.BuildMarketScene(root.transform)
                    : LevelSelectThumbnailDiorama.BuildHomeScene(root.transform);

                if (scene.Camera == null)
                {
                    LevelSelectThumbnailBuilder.BuildStaticImage(transform, definition, library);
                    return;
                }

                RenderTexture renderTexture = new RenderTexture(RenderWidth, RenderHeight, 24, RenderTextureFormat.ARGB32);
                RenderTexture previousActive = RenderTexture.active;
                RenderTexture previousTarget = scene.Camera.targetTexture;

                scene.Camera.targetTexture = renderTexture;
                scene.Camera.Render();

                RenderTexture.active = renderTexture;
                Texture2D texture = new Texture2D(RenderWidth, RenderHeight, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, RenderWidth, RenderHeight), 0, 0);
                texture.Apply();

                scene.Camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                DestroyObjectSafe(renderTexture);

                targetImage = CreateTargetImage(texture);
                LevelSelectThumbnailBuilder.ApplyFrameOverlay(transform, definition, library);
            }
            finally
            {
                RenderSettings.skybox = previousSkybox;
                RenderSettings.fog = previousFog;
                RenderSettings.fogColor = previousFogColor;
                DestroyObjectSafe(root);
            }
#endif
        }

        private static void DestroyObjectSafe(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }

        private RawImage CreateTargetImage(Texture texture)
        {
            GameObject imageObject = new GameObject("LiveThumbnailImage", typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(transform, false);
            RawImage rawImage = imageObject.GetComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.raycastTarget = false;
            rawImage.color = definition.IsUnlocked
                ? Color.white
                : new Color(0.78f, 0.74f, 0.68f, 0.92f);

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rawImage;
        }

        private void OnDestroy()
        {
            if (targetImage != null && targetImage.texture != null)
            {
                DestroyObjectSafe(targetImage.texture);
            }
        }
    }
}
