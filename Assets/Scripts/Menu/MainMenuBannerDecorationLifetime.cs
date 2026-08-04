using System.Collections;
using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Keeps banner decoration render textures updated each frame.
    /// </summary>
    public class MainMenuBannerDecorationLifetime : MonoBehaviour
    {
        private Camera backCamera;
        private Camera frontCamera;
        private Transform backProps;
        private Transform frontProps;
        private RenderTexture backTexture;
        private RenderTexture frontTexture;

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
            MainMenuBannerDecorationBuilder.RenderPass(backCamera, backProps, frontProps);
            MainMenuBannerDecorationBuilder.RenderPass(frontCamera, frontProps, backProps);
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
