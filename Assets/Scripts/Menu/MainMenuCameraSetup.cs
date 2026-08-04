using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Ensures a Main Camera exists so Unity does not show "No cameras rendering".
    /// </summary>
    public static class MainMenuCameraSetup
    {
        private static readonly Color WarmWoodClear = new Color(0.34f, 0.22f, 0.13f, 1f);

        public static Camera EnsureMainCamera(Transform parent)
        {
            Camera camera = null;
            if (parent != null)
            {
                Transform existing = parent.Find("Main Camera");
                if (existing != null)
                {
                    camera = existing.GetComponent<Camera>();
                }
            }

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                if (parent != null)
                {
                    cameraObject.transform.SetParent(parent, false);
                }

                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }
            else if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = WarmWoodClear;
            camera.cullingMask = 1 << 0;
            camera.orthographic = true;
            camera.depth = -100;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.targetDisplay = 0;
            camera.gameObject.tag = "MainCamera";
            camera.gameObject.SetActive(true);
            camera.enabled = true;
            return camera;
        }
    }
}
