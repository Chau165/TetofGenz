using ChoTet.Level1.HometownArrival;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Applies the same blue-sky countryside look as Level1 HometownArrival to the ChoTet market slice.
    /// </summary>
    [DefaultExecutionOrder(-20)]
    public class ChoTetOutdoorEnvironment : MonoBehaviour
    {
        [SerializeField] private Material daySkybox;
        [SerializeField] private float fogStart = 38f;
        [SerializeField] private float fogEnd = 105f;
        [SerializeField] private bool dressWorldOnAwake = true;

        private void Awake()
        {
            ApplyRenderSettings();

            if (!dressWorldOnAwake)
            {
                return;
            }

            Transform world = FindChoTetWorld();
            if (world == null)
            {
                return;
            }

            if (world.Find("ChoTetWorldBackdrop") == null)
            {
                ChoTetWorldBackdropDressing.Apply(world);
            }

            if (world.Find("ChoTetFloralDressing") == null)
            {
                ChoTetFloralDressing.Apply(world);
            }
        }

        public void ApplyRenderSettings()
        {
            Material skybox = ResolveSkybox();
            ChoTetMarketEnvironment.ApplyHometownArrivalRenderSettings(skybox, fogStart, fogEnd);

            Camera main = Camera.main;
            if (main != null)
            {
                main.clearFlags = CameraClearFlags.Skybox;
            }
        }

        private Material ResolveSkybox()
        {
            if (daySkybox != null)
            {
                return daySkybox;
            }

#if UNITY_EDITOR
            daySkybox = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.HometownDaySkyboxAssetPath);
            if (daySkybox == null)
            {
                daySkybox = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                    ChoTetMarketEnvironment.SunsetSkyboxAssetPath);
            }
#endif
            return daySkybox ?? RenderSettings.skybox;
        }

        private static Transform FindChoTetWorld()
        {
            GameObject world = GameObject.Find("ChoTetWorld");
            return world != null ? world.transform : null;
        }
    }
}
