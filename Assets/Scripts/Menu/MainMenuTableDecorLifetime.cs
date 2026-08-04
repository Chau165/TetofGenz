using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Releases the decoration render texture when the menu is rebuilt or destroyed.
    /// </summary>
    public class MainMenuTableDecorLifetime : MonoBehaviour
    {
        private RenderTexture renderTexture;

        public void Initialize(RenderTexture texture)
        {
            renderTexture = texture;
        }

        private void OnDestroy()
        {
            if (renderTexture == null)
            {
                return;
            }

            renderTexture.Release();
            if (Application.isPlaying)
            {
                Destroy(renderTexture);
            }
            else
            {
                DestroyImmediate(renderTexture);
            }
        }
    }
}
