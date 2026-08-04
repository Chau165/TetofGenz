using ChoTet.Level1;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// Boots the Level 1 hometown-arrival beat: disables market flow and wires bus / door markers.
    /// </summary>
    [DefaultExecutionOrder(-400)]
    public class HometownArrivalBootstrap : MonoBehaviour
    {
        private static bool sceneLoadedHooked;
        private static HometownArrivalBootstrap activeInstance;

        [Header("Runtime assets kept in the Windows build")]
        [SerializeField] private GameObject runtimePlayerVisualPrefab;
        [SerializeField] private GameObject runtimeFatherVisualPrefab;
        [SerializeField] private GameObject runtimeMotherVisualPrefab;
        [SerializeField] private GameObject runtimeParentsCarPrefab;

        [Header("Highway approach — same lane grid as obstacle highway")]
        [SerializeField] private float busLaneX = HometownArrivalLayout.BusLaneX;
        [SerializeField] private float busStartZ = HometownArrivalLayout.BusStartZ;
        [SerializeField] private float busStopZ = HometownArrivalLayout.BusStopZ;
        [SerializeField] private float busGroundY = HometownArrivalLayout.BusGroundY;
        [SerializeField] private float parkedRoadsideX = HometownArrivalLayout.ParkedRoadsideX;

        public static bool IsArrivalScene(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) &&
                   sceneName.IndexOf("HometownArrival", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InstallSceneLoadedHook()
        {
            if (sceneLoadedHooked)
            {
                return;
            }

            sceneLoadedHooked = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureBootstrapForScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            EnsureBootstrapForScene(scene);
        }

        private static void EnsureBootstrapForScene(Scene scene)
        {
            if (!IsArrivalScene(scene.name))
            {
                return;
            }

            if (FindAnyObjectByType<HometownArrivalBootstrap>() != null)
            {
                return;
            }

            GameObject host = new GameObject("HometownArrivalBootstrap");
            host.AddComponent<HometownArrivalBootstrap>();
        }

        private void Awake()
        {
            if (!IsArrivalScene(SceneManager.GetActiveScene().name))
            {
                Destroy(gameObject);
                return;
            }

            activeInstance = this;
            DisableMarketSystems();
            Level1TrafficVisualBootstrap.EnsureConfigured();
            HometownArrivalEnvironment.PrepareScene();
            EnsureArrivalStack();
        }

        private void OnDestroy()
        {
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        internal static GameObject LoadRuntimePrefab(string assetPath)
        {
            HometownArrivalBootstrap bootstrap = activeInstance != null
                ? activeInstance
                : FindAnyObjectByType<HometownArrivalBootstrap>();
            if (bootstrap == null)
            {
                return null;
            }

            if (string.Equals(assetPath, PlayerCharacterVisualPaths.CreativeCharacterVisualPrefab,
                    System.StringComparison.Ordinal))
            {
                return bootstrap.runtimePlayerVisualPrefab;
            }

            if (string.Equals(assetPath, PlayerCharacterVisualPaths.FatherVisualPrefab,
                    System.StringComparison.Ordinal))
            {
                return bootstrap.runtimeFatherVisualPrefab;
            }

            if (string.Equals(assetPath, PlayerCharacterVisualPaths.MotherVisualPrefab,
                    System.StringComparison.Ordinal))
            {
                return bootstrap.runtimeMotherVisualPrefab;
            }

            return string.Equals(assetPath, Level1KenneyCarKitPaths.SedanPrefab,
                    System.StringComparison.Ordinal)
                ? bootstrap.runtimeParentsCarPrefab
                : null;
        }

        private static void DisableMarketSystems()
        {
            GameFlowController flow = FindAnyObjectByType<GameFlowController>();
            if (flow != null)
            {
                flow.enabled = false;
            }

            GameHudController hud = FindAnyObjectByType<GameHudController>();
            if (hud != null)
            {
                hud.gameObject.SetActive(false);
            }

            GameObject traffic = GameObject.Find("TrafficRoot");
            if (traffic != null)
            {
                traffic.SetActive(false);
            }
        }

        private void EnsureArrivalStack()
        {
            Transform worldRoot = FindChoTetWorld();
            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            HometownArrivalPlayerVisualContext.ApplyToPlayer(player);
            HometownArrivalCharacterMaterialRepair.RepairAll();
            if (player != null)
            {
                // Stay invisible through approach/disembark; FamilyReunion reveals.
                player.ForceHideCharacterVisual = true;
            }

            TopDownCameraFollow cameraFollow = FindAnyObjectByType<TopDownCameraFollow>();

            Transform spawnPoint = EnsureSpawnPoint(worldRoot);
            Transform pickupBus = EnsurePickupBus(worldRoot);
            HometownArrivalExitSpawn.EnsureExitPoint(pickupBus, worldRoot);
            HometownArrivalController controller = EnsureController(player, spawnPoint, pickupBus, cameraFollow);
            ApplyArrivalCamera(cameraFollow, pickupBus);
            HometownArrivalHighwayApproach.EnsureBuilt(transform, HometownArrivalLayout.BusStopZ);
            HometownArrivalFamilyCar.EnsureParked(worldRoot);
            EnsureParentsWaitingHost();
            HometownArrivalShoulderBarrier.Remove();
            EnsureEnvironment();
            HometownArrivalEnvironment.SuppressLegacyWorld();
            HometownArrivalEnvironment.PrepareScene();
            EnsureInfiniteScroll(pickupBus);
            ApplyArrivalApproachSettings(controller, pickupBus);
        }

        private static void EnsureInfiniteScroll(Transform pickupBus)
        {
            HometownArrivalHighwayApproach approach = FindAnyObjectByType<HometownArrivalHighwayApproach>();
            if (approach == null)
            {
                return;
            }

            HometownArrivalInfiniteScroll scroll = FindAnyObjectByType<HometownArrivalInfiniteScroll>();
            if (scroll == null)
            {
                scroll = approach.gameObject.AddComponent<HometownArrivalInfiniteScroll>();
            }

            scroll.Bind(pickupBus, approach.transform);
        }

        private static Transform FindChoTetWorld()
        {
            GameObject world = GameObject.Find("ChoTetWorld");
            return world != null ? world.transform : null;
        }

        private Transform EnsureSpawnPoint(Transform worldRoot)
        {
            Transform existing = GameObject.Find("ArrivalSpawnPoint")?.transform;
            if (existing != null)
            {
                return existing;
            }

            Transform homeSpawn = GameObject.Find("HomeSpawnPoint")?.transform;
            if (homeSpawn != null)
            {
                GameObject marker = new GameObject("ArrivalSpawnPoint");
                marker.transform.SetParent(homeSpawn.parent != null ? homeSpawn.parent : worldRoot, false);
                marker.transform.SetPositionAndRotation(homeSpawn.position, homeSpawn.rotation);
                return marker.transform;
            }

            GameObject fallback = new GameObject("ArrivalSpawnPoint");
            if (worldRoot != null)
            {
                fallback.transform.SetParent(worldRoot, false);
            }

            fallback.transform.position = new Vector3(
                HometownArrivalLayout.ParkedRoadsideX + 0.35f,
                busGroundY + 0.15f,
                busStopZ - 0.2f);
            return fallback.transform;
        }

        private Transform EnsurePickupBus(Transform worldRoot)
        {
            Transform existing = GameObject.Find("PickupBus")?.transform;
            if (existing == null)
            {
                GameObject busRoot = new GameObject("PickupBus");
                if (worldRoot != null)
                {
                    busRoot.transform.SetParent(worldRoot, false);
                }

                existing = busRoot.transform;
            }

            existing.SetPositionAndRotation(
                new Vector3(busLaneX, busGroundY, busStartZ),
                Quaternion.Euler(0f, 180f, 0f));

            HometownArrivalVehicleVisual.Apply(existing, forceRebuild: true);
            return existing;
        }

        private static HometownArrivalController EnsureController(
            PlayerTopDownController player,
            Transform spawnPoint,
            Transform pickupBus,
            TopDownCameraFollow cameraFollow)
        {
            HometownArrivalController existing = FindAnyObjectByType<HometownArrivalController>();
            if (existing != null)
            {
                return existing;
            }

            GameObject systems = GameObject.Find("Systems") ?? new GameObject("Systems");
            HometownArrivalController controller = systems.GetComponent<HometownArrivalController>();
            if (controller == null)
            {
                controller = systems.AddComponent<HometownArrivalController>();
            }

            Level1ScreenFade fade = systems.GetComponent<Level1ScreenFade>();
            if (fade == null)
            {
                fade = systems.AddComponent<Level1ScreenFade>();
            }

            controller.ConfigureFromBootstrap(player, fade, spawnPoint, pickupBus, cameraFollow);
            return controller;
        }

        private static void EnsureEnvironment()
        {
            if (FindAnyObjectByType<HometownArrivalEnvironment>() != null)
            {
                return;
            }

            GameObject systems = GameObject.Find("Systems");
            if (systems == null)
            {
                systems = new GameObject("Systems");
            }

            if (systems.GetComponent<HometownArrivalEnvironment>() == null)
            {
                systems.AddComponent<HometownArrivalEnvironment>();
            }
        }

        private static void EnsureParentsWaitingHost()
        {
            if (FindAnyObjectByType<HometownArrivalParentsWaitingHost>() != null)
            {
                return;
            }

            GameObject systems = GameObject.Find("Systems");
            if (systems == null)
            {
                systems = new GameObject("Systems");
            }

            if (systems.GetComponent<HometownArrivalParentsWaitingHost>() == null)
            {
                systems.AddComponent<HometownArrivalParentsWaitingHost>();
            }
        }

        private static void ApplyArrivalCamera(TopDownCameraFollow cameraFollow, Transform pickupBus)
        {
            if (cameraFollow == null)
            {
                return;
            }

            HometownArrivalCamera.ApplyApproachDefaults(cameraFollow);
            if (pickupBus != null)
            {
                cameraFollow.target = pickupBus;
                cameraFollow.SnapToTarget();
            }
        }

        private void ApplyArrivalApproachSettings(HometownArrivalController controller, Transform pickupBus)
        {
            if (controller == null)
            {
                return;
            }

            controller.ConfigureVehicleApproach(
                HometownArrivalLayout.BusLaneX,
                HometownArrivalLayout.BusStartZ,
                HometownArrivalLayout.BusStopZ,
                HometownArrivalLayout.BusGroundY,
                HometownArrivalLayout.ParkedRoadsideX);
            if (pickupBus != null)
            {
                pickupBus.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
    }
}
