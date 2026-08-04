using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1
{
    /// <summary>
    /// Uniform light-blue sky for the rental-room interior (no procedural skybox gradient).
    /// </summary>
    public static class Level1IndoorSky
    {
        public static readonly Color SkyColor = new Color(0.82f, 0.92f, 0.98f, 1f);

        public static void Apply(Camera camera = null)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.88f, 0.86f, 0.82f, 1f);
            RenderSettings.ambientIntensity = 1.1f;
            RenderSettings.fog = false;
            RenderSettings.sun = null;

            Camera target = camera != null ? camera : Camera.main;
            if (target == null)
            {
                return;
            }

            target.clearFlags = CameraClearFlags.SolidColor;
            target.backgroundColor = SkyColor;
            target.farClipPlane = Mathf.Max(target.farClipPlane, 48f);
        }
    }
}
