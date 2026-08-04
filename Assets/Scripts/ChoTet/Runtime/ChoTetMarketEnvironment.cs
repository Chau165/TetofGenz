using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Outdoor environment settings shared with Level 2 market (ChoTet_VerticalSlice).
    /// Uses PBR ground materials and an HDR skybox material — not a flat PNG backdrop.
    /// </summary>
    public static class ChoTetMarketEnvironment
    {
        public const string MaterialsRoot = "Assets/Settings/ChoTet/Materials";
        public const string SunsetSkyboxAssetPath = MaterialsRoot + "/ChoTet_SunsetSkybox.mat";
        public const string HometownDaySkyboxAssetPath =
            "Assets/Low-Poly Medieval Market/Materials/Skybox_Mat.mat";

        /// <summary>Same RenderSettings stack as <c>ChoTetSceneBuilder.ConfigureEnvironmentLighting</c>.</summary>
        public static void ApplyOutdoorRenderSettings(Material skyboxMaterial, float fogStart = 65f,
            float fogEnd = 150f)
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
            }

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 0.95f;
            RenderSettings.reflectionIntensity = 0.85f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.86f, 0.72f, 0.57f, 1f);
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        /// <summary>Hometown arrival - blue daytime skybox + soft blue horizon fog.</summary>
        public static void ApplyHometownArrivalRenderSettings(Material skyboxMaterial, float fogStart = 48f,
            float fogEnd = 115f)
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
            }

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.reflectionIntensity = 0.75f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.58f, 0.76f, 0.94f, 1f);
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        /// <summary>Alternate hometown mood - sunset HDR sky with cooler evening fog.</summary>
        public static void ApplyHometownEveningRenderSettings(Material skyboxMaterial, float fogStart = 42f,
            float fogEnd = 95f)
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
            }

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 0.82f;
            RenderSettings.reflectionIntensity = 0.55f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.32f, 0.38f, 0.58f, 1f);
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        /// <summary>Highway scene — skybox with visible sun + soft haze (water is a separate mesh below).</summary>
        public static void ApplyHighwayRenderSettings(
            float fogStart = 75f,
            float fogEnd = 240f,
            Material skyboxOverride = null)
        {
            Color skyBlue = new Color(0.55f, 0.76f, 0.95f, 1f);
            Material skybox = skyboxOverride ?? LoadDefaultHighwaySkybox();
            if (skybox != null)
            {
                // Runtime instance so we can show a clear sky / sun without mutating the asset.
                Material skyInstance = new Material(skybox)
                {
                    name = skybox.name + "_HighwayRuntime",
                    hideFlags = HideFlags.DontSave
                };
                if (skyInstance.HasProperty("_SunSize"))
                {
                    skyInstance.SetFloat("_SunSize", 0.14f);
                }

                if (skyInstance.HasProperty("_SunSizeConvergence"))
                {
                    skyInstance.SetFloat("_SunSizeConvergence", 3.2f);
                }

                if (skyInstance.HasProperty("_Exposure"))
                {
                    skyInstance.SetFloat("_Exposure", 1.45f);
                }

                if (skyInstance.HasProperty("_AtmosphereThickness"))
                {
                    skyInstance.SetFloat("_AtmosphereThickness", 1.05f);
                }

                if (skyInstance.HasProperty("_GroundColor"))
                {
                    // Soft horizon wash — avoid a dark teal band; real water is the pack ocean mesh.
                    skyInstance.SetColor("_GroundColor", new Color(0.72f, 0.82f, 0.9f));
                }

                if (skyInstance.HasProperty("_SkyTint"))
                {
                    skyInstance.SetColor("_SkyTint", new Color(0.55f, 0.72f, 0.95f));
                }

                RenderSettings.skybox = skyInstance;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1.1f;
#if UNITY_EDITOR || UNITY_2022_1_OR_NEWER
                DynamicGI.UpdateEnvironment();
#endif
            }
            else
            {
                RenderSettings.skybox = null;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.62f, 0.76f, 0.92f);
                RenderSettings.ambientIntensity = 1.12f;
            }

            RenderSettings.reflectionIntensity = 0.55f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            // Soft haze — keep sky/sun readable above the water plane.
            RenderSettings.fogColor = Color.Lerp(skyBlue, new Color(0.82f, 0.88f, 0.94f), 0.4f);
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        private static Material LoadDefaultHighwaySkybox()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Settings/ChoTet/Materials/Level1/Level1_StudyMorningSky.mat");
#else
            return null;
#endif
        }
    }
}
