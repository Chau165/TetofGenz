using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.HometownArrival
{
    public enum HometownArrivalSkyMood
    {
        DayBlue,
        Evening
    }

    /// <summary>
    /// Hides the cloned market world and keeps only the procedural highway approach visible.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class HometownArrivalEnvironment : MonoBehaviour
    {
        [Header("Sky & fog")]
        [SerializeField] private HometownArrivalSkyMood skyMood = HometownArrivalSkyMood.DayBlue;
        [SerializeField] private Material daySkybox;
        [SerializeField] private Material eveningSkybox;
        [SerializeField] private float fogStart = 55f;
        [SerializeField] private float fogEnd = 145f;

        private void Awake()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.name.Contains("HometownArrival"))
            {
                enabled = false;
                return;
            }

            EnsureBootstrapDriver();
            ApplyRenderSettings();
            SuppressLegacyChoTetWorld();
            TrimMainPathVillageTail();
            HideMainPathUntilDropOff();
            RemoveArrivalDressing();
        }

        private static void EnsureBootstrapDriver()
        {
            if (FindAnyObjectByType<HometownArrivalBootstrap>() != null)
            {
                return;
            }

            GameObject host = new GameObject("HometownArrivalBootstrap");
            host.AddComponent<HometownArrivalBootstrap>();
        }

        public static void SuppressLegacyWorld()
        {
            SuppressLegacyChoTetWorld();
            TrimMainPathVillageTail();
            HideMainPathUntilDropOff();
            RemoveArrivalDressing();
        }

        public static void PrepareScene()
        {
#if UNITY_EDITOR
            Material skybox = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.HometownDaySkyboxAssetPath);
            ChoTetMarketEnvironment.ApplyHometownArrivalRenderSettings(skybox, 55f, 145f);
            Camera main = Camera.main;
            if (main != null)
            {
                main.clearFlags = CameraClearFlags.Skybox;
            }
#endif
        }

        private void ApplyRenderSettings()
        {
            Material skybox = ResolveSkybox();
            if (skyMood == HometownArrivalSkyMood.Evening)
            {
                ChoTetMarketEnvironment.ApplyHometownEveningRenderSettings(skybox, fogStart, fogEnd);
            }
            else
            {
                ChoTetMarketEnvironment.ApplyHometownArrivalRenderSettings(skybox, fogStart, fogEnd);
            }

            Camera main = Camera.main;
            if (main != null)
            {
                main.clearFlags = CameraClearFlags.Skybox;
            }
        }

        private Material ResolveSkybox()
        {
            if (skyMood == HometownArrivalSkyMood.Evening)
            {
                if (eveningSkybox != null)
                {
                    return eveningSkybox;
                }

#if UNITY_EDITOR
                eveningSkybox = LoadSkybox(ChoTetMarketEnvironment.SunsetSkyboxAssetPath);
#endif
                return eveningSkybox ?? daySkybox ?? RenderSettings.skybox;
            }

            if (daySkybox != null)
            {
                return daySkybox;
            }

#if UNITY_EDITOR
            daySkybox = LoadSkybox(ChoTetMarketEnvironment.HometownDaySkyboxAssetPath);
            if (daySkybox == null)
            {
                daySkybox = LoadSkybox(ChoTetMarketEnvironment.SunsetSkyboxAssetPath);
            }
#endif
            return daySkybox ?? RenderSettings.skybox;
        }

#if UNITY_EDITOR
        private static Material LoadSkybox(string assetPath)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        }
#endif

        private static void SuppressLegacyChoTetWorld()
        {
            Transform world = GameObject.Find("ChoTetWorld")?.transform;
            if (world == null)
            {
                return;
            }

            for (int i = 0; i < world.childCount; i++)
            {
                Transform child = world.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                string name = child.name;
                if (IsProtectedArrivalNode(child))
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private static bool IsProtectedArrivalNode(Transform node)
        {
            if (node == null)
            {
                return false;
            }

            string name = node.name;
            if (name == "PickupBus" || name == "ParentsCar" || name == "ParentsWaiting"
                || name == "ArrivalSpawnPoint" || name == "Main Camera"
                || name == "Player" || name == "Systems" || name == "Directional Light"
                || name == "HometownArrivalBootstrap" || name == "HometownHighwayApproach")
            {
                return true;
            }

            for (int i = 0; i < node.childCount; i++)
            {
                if (IsProtectedArrivalNode(node.GetChild(i)))
                {
                    return true;
                }
            }

            return false;
        }

        private static void HideMainPathUntilDropOff()
        {
            DestroyLegacyPath("MainPath");
            DestroyLegacyPath("SideWalkLeft");
            DestroyLegacyPath("SideWalkRight");
        }

        private static void DestroyLegacyPath(string objectName)
        {
            GameObject path = GameObject.Find(objectName);
            if (path != null)
            {
                Object.Destroy(path);
            }
        }

        public static void RevealVillagePath()
        {
            // Arrival beat keeps only the procedural countryside road — no legacy village props.
        }

        private static void TrimMainPathVillageTail()
        {
            Transform mainPath = GameObject.Find("MainPath")?.transform;
            if (mainPath == null)
            {
                return;
            }

            mainPath.localPosition = new Vector3(0f, -0.42f, 12f);
            mainPath.localScale = new Vector3(6f, 0.15f, 26f);
        }

        private static void RemoveArrivalDressing()
        {
            Transform world = GameObject.Find("ChoTetWorld")?.transform;
            Transform dressing = world != null ? world.Find("ArrivalDressing") : null;
            if (dressing != null)
            {
                Object.Destroy(dressing.gameObject);
            }
        }
    }
}
