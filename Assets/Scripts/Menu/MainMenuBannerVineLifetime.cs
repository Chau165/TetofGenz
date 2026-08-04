using System.Collections;
using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Keeps banner vine render textures updated with dual-pass rendering each frame.
    /// </summary>
    public class MainMenuBannerVineLifetime : MonoBehaviour
    {
        private RenderTexture backTexture;
        private RenderTexture frontTexture;
        private Camera backCamera;
        private Camera frontCamera;
        private Transform backProps;
        private Transform frontProps;

        public void Initialize(RenderTexture back, RenderTexture front, Camera backCam, Camera frontCam,
            Transform backRoot, Transform frontRoot)
        {
            backTexture = back;
            frontTexture = front;
            backCamera = backCam;
            frontCamera = frontCam;
            backProps = backRoot;
            frontProps = frontRoot;
            StartCoroutine(ForceInitialRender());
        }

        private IEnumerator ForceInitialRender()
        {
            yield return null;
            RenderBothPasses();
            yield return new WaitForEndOfFrame();
            RenderBothPasses();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RenderBothPasses();
        }

        private void RenderBothPasses()
        {
            MainMenuBannerVineBuilder.RenderPass(backCamera, backProps, frontProps);
            MainMenuBannerVineBuilder.RenderPass(frontCamera, frontProps, backProps);
        }

        private void OnDestroy()
        {
            Release(backTexture);
            Release(frontTexture);
        }

        private static void Release(RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
        }
    }
}
