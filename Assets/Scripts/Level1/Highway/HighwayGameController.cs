using System.Collections;
using ChoTet.Level1.HometownArrival;
using ChoTet.Menu;
using ChoTet.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChoTet.Level1.Highway
{
    /// <summary>Highway mini-game loop — scroll, traffic, lives, destination, scene transitions.</summary>
    [DefaultExecutionOrder(-100)]
    public class HighwayGameController : MonoBehaviour
    {
        [Header("Speed & distance")]
        [SerializeField] private float forwardSpeed = 28f;
        [SerializeField] private float destinationDistance = 2200f;
        [SerializeField] private float maxDifficultyTime = 140f;

        [Header("Lives")]
        [SerializeField] private bool useLives = true;
        [SerializeField] private int startingLives = 3;
        [SerializeField] private float invulnerabilityAfterHit = 1.6f;

        [Header("Scene flow")]
        [SerializeField] private string nextSceneName = "Level1_HometownArrival";
        [SerializeField] private float fadeDuration = 0.55f;
        [SerializeField] private float victoryDriveExtraDistance = 150f;
        [SerializeField] private float victoryDriveSpeedMultiplier = 1.2f;

        [Header("References")]
        [SerializeField] private HighwayPlayerVehicle playerVehicle;
        [SerializeField] private HighwayFollowCamera followCamera;
        [SerializeField] private HighwayRoadSegmentPool roadPool;
        [SerializeField] private HighwaySideGroundPool sideGroundPool;
        [SerializeField] private HighwayNatureRoadsideSpawner natureRoadsideSpawner;
        [SerializeField] private HighwayStaticUrbanNeighborhood staticUrbanNeighborhood;
        [SerializeField] private HighwayQuaterniusTownBlockPool quaterniusTownBlockPool;
        [SerializeField] private HighwayQuaterniusSidewalkPool quaterniusSidewalkPool;
        [SerializeField] private HighwayEnvironmentSpawner environmentSpawner;
        [SerializeField] private HighwayHorizonLayer horizonLayer;
        [SerializeField] private HighwayCountrysideSceneryPool countrysideSceneryPool;
        [SerializeField] private HighwayCountrysidePropsSpawner countrysidePropsSpawner;
        [SerializeField] private HighwayTrafficSpawner trafficSpawner;
        [SerializeField] private HighwayVisualPool visualPool;
        [SerializeField] private HighwayCollisionFeedback collisionFeedback;
        [SerializeField] private Material sunsetSkybox;

        [Header("Traffic prefab groups")]
        [SerializeField] private GameObject[] sedanPrefabs;
        [SerializeField] private GameObject[] truckPrefabs;
        [SerializeField] private GameObject[] busPrefabs;
        [SerializeField] private GameObject[] vanPrefabs;
        [SerializeField] private GameObject playerVanPrefab;

        [Header("Traffic visuals")]
        [SerializeField] private Material[] trafficBodyColors;
        [SerializeField] private Material pickupBodyMaterial;
        [SerializeField] private Material wheelMaterial;
        [SerializeField] private Material glassMaterial;
        [SerializeField] private Material kenneyColormapMaterial;

        private float distanceTraveled;
        private float elapsedTime;
        private int lives;
        private float invulnTimer;
        private bool isRunning;
        private bool isEnding;
        private bool isVictoryDrive;
        private float victoryTargetDistance;
        private Canvas hudCanvas;
        private TextMeshProUGUI hudTitleText;
        private TextMeshProUGUI hudLivesText;
        private TextMeshProUGUI hudDistanceText;
        private TextMeshProUGUI hudHintText;

        public Vector3 PlayerPosition => playerVehicle != null ? playerVehicle.transform.position : Vector3.zero;
        public int PlayerLaneIndex => playerVehicle != null ? playerVehicle.CurrentLane : HighwayPlayerLaneLayout.MiddleLaneIndex;
        public float PlayerVehicleLength => playerVehicle != null ? playerVehicle.VehicleLength : 4.8f;

        private void Awake()
        {
            Time.timeScale = 1f;
            ClearStuckFadeOverlay();
            MainMenuAudioSettings.LoadSavedVolume();
            SceneBackgroundMusic.EnsurePlaying();
            ResolveSceneReferences();

            if (playerVehicle == null)
            {
                playerVehicle = FindAnyObjectByType<HighwayPlayerVehicle>();
            }

            if (followCamera == null)
            {
                followCamera = FindAnyObjectByType<HighwayFollowCamera>();
            }

            if (visualPool == null)
            {
                visualPool = GetComponentInChildren<HighwayVisualPool>();
                if (visualPool == null)
                {
                    visualPool = FindAnyObjectByType<HighwayVisualPool>();
                }
            }

            if (roadPool == null)
            {
                roadPool = FindAnyObjectByType<HighwayRoadSegmentPool>();
            }

            if (sideGroundPool == null)
            {
                sideGroundPool = FindAnyObjectByType<HighwaySideGroundPool>(FindObjectsInactive.Include);
            }

            if (natureRoadsideSpawner == null)
            {
                natureRoadsideSpawner = FindAnyObjectByType<HighwayNatureRoadsideSpawner>(FindObjectsInactive.Include);
            }

            if (quaterniusTownBlockPool == null)
            {
                quaterniusTownBlockPool = FindAnyObjectByType<HighwayQuaterniusTownBlockPool>(FindObjectsInactive.Include);
            }

            if (quaterniusSidewalkPool == null)
            {
                quaterniusSidewalkPool = FindAnyObjectByType<HighwayQuaterniusSidewalkPool>(FindObjectsInactive.Include);
            }

            if (staticUrbanNeighborhood == null)
            {
                staticUrbanNeighborhood = FindAnyObjectByType<HighwayStaticUrbanNeighborhood>(FindObjectsInactive.Include);
            }

            if (horizonLayer == null)
            {
                horizonLayer = FindAnyObjectByType<HighwayHorizonLayer>(FindObjectsInactive.Include);
            }

            if (countrysideSceneryPool == null)
            {
                countrysideSceneryPool = FindAnyObjectByType<HighwayCountrysideSceneryPool>(FindObjectsInactive.Include);
            }

            if (countrysidePropsSpawner == null)
            {
                countrysidePropsSpawner = FindAnyObjectByType<HighwayCountrysidePropsSpawner>(FindObjectsInactive.Include);
            }

            if (trafficSpawner == null)
            {
                trafficSpawner = FindAnyObjectByType<HighwayTrafficSpawner>();
            }

            DisableUrbanPoolsOnly();
            ApplyEnvironment();
            ApplyWarmAfternoonLighting();
            ConfigureTrafficVisuals();
            EnsureCollisionFeedback();
            BootstrapPlayerVan();
            StripConflictingFirstPersonCameras();
            EnsureHighwayCameraMode();
            ApplyCountrysideRoadsideMode();
            roadPool?.SetScrollOffset(0f);
            sideGroundPool?.SetScrollOffset(0f);
            natureRoadsideSpawner?.SetScrollOffset(0f);
            countrysideSceneryPool?.SetScrollOffset(0f);
            countrysidePropsSpawner?.SetScrollOffset(0f);
            horizonLayer?.SetScrollOffset(0f);
            followCamera?.Snap();
        }

        private static void ClearStuckFadeOverlay()
        {
            GameObject fade = GameObject.Find("HighwayFadeOverlay");
            if (fade != null)
            {
                Destroy(fade);
            }
        }

        private void ResolveSceneReferences()
        {
            // Runtime bootstrap may create objects after this Awake; references are re-resolved in Start.
        }

        private void Start()
        {
            if (natureRoadsideSpawner == null)
            {
                natureRoadsideSpawner = FindAnyObjectByType<HighwayNatureRoadsideSpawner>();
            }

            if (countrysidePropsSpawner == null)
            {
                countrysidePropsSpawner = FindAnyObjectByType<HighwayCountrysidePropsSpawner>();
            }

            if (sideGroundPool == null)
            {
                sideGroundPool = FindAnyObjectByType<HighwaySideGroundPool>();
            }

            ApplyCountrysideRoadsideMode();
            EnsureHighwayCameraMode();
            InitializeSystems();
            EnsureHud();
            lives = Mathf.Max(1, startingLives);
            playerVehicle?.SetLaneInputLocked(false);
            StartCoroutine(BeginWithFadeIn());
        }

        private static void StripConflictingFirstPersonCameras()
        {
            FirstPersonCameraController[] controllers =
                FindObjectsByType<FirstPersonCameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                FirstPersonCameraController controller = controllers[i];
                if (controller == null)
                {
                    continue;
                }

                controller.enabled = false;
                Destroy(controller);
            }
        }

        private void EnsureHighwayCameraMode()
        {
            if (playerVehicle == null)
            {
                return;
            }

            Camera main = Camera.main;
            if (main != null)
            {
                ChoTet.Runtime.FirstPersonCameraController firstPerson =
                    main.GetComponent<ChoTet.Runtime.FirstPersonCameraController>();
                if (firstPerson != null)
                {
                    firstPerson.enabled = false;
                    Destroy(firstPerson);
                }
            }

            if (followCamera == null)
            {
                followCamera = FindAnyObjectByType<HighwayFollowCamera>();
            }

            if (followCamera != null)
            {
                followCamera.enabled = true;
                followCamera.Bind(playerVehicle.transform);
            }
        }

        private void Update()
        {
            bool allowDriveInput = isRunning && !isEnding && !isVictoryDrive;
            playerVehicle?.SetHighwayInputEnabled(allowDriveInput);

            if (!isRunning || isEnding)
            {
                return;
            }

            float dt = Time.deltaTime;
            float driveSpeed = isVictoryDrive
                ? forwardSpeed * victoryDriveSpeedMultiplier
                : forwardSpeed;

            if (!isVictoryDrive)
            {
                elapsedTime += dt;
                invulnTimer = Mathf.Max(0f, invulnTimer - dt);
            }

            distanceTraveled += driveSpeed * dt;

            playerVehicle.SetForwardSpeed(driveSpeed);
            playerVehicle.Tick(dt);

            Vector3 playerPos = playerVehicle.transform.position;
            playerPos.z = distanceTraveled;
            playerVehicle.transform.position = playerPos;

            ApplyWorldScroll(distanceTraveled);

            if (!isVictoryDrive)
            {
                float difficulty = Mathf.Clamp01(elapsedTime / maxDifficultyTime);
                trafficSpawner?.Tick(dt, forwardSpeed, difficulty);

                if (invulnTimer <= 0f)
                {
                    trafficSpawner?.CheckCollisions(playerPos, playerVehicle.VehicleLength);
                }
            }
            else
            {
                trafficSpawner?.Tick(dt, driveSpeed, 0f);
            }

            UpdateHud();

            if (!isVictoryDrive && distanceTraveled >= destinationDistance)
            {
                BeginVictoryDrive();
            }
            else if (isVictoryDrive && distanceTraveled >= victoryTargetDistance && !isEnding)
            {
                StartCoroutine(CompleteHighwayRoutine());
            }
        }

        private void ApplyWorldScroll(float scrollOffset)
        {
            roadPool?.SetScrollOffset(scrollOffset);

            if (quaterniusTownBlockPool != null && quaterniusTownBlockPool.gameObject.activeInHierarchy)
            {
                quaterniusTownBlockPool.SetScrollOffset(scrollOffset);
            }

            if (quaterniusSidewalkPool != null && quaterniusSidewalkPool.gameObject.activeInHierarchy)
            {
                quaterniusSidewalkPool.SetScrollOffset(scrollOffset);
            }

            if (sideGroundPool != null && sideGroundPool.gameObject.activeInHierarchy)
            {
                sideGroundPool.SetScrollOffset(scrollOffset);
            }

            if (natureRoadsideSpawner != null && natureRoadsideSpawner.gameObject.activeInHierarchy)
            {
                natureRoadsideSpawner.SetScrollOffset(scrollOffset);
            }

            if (countrysideSceneryPool != null && countrysideSceneryPool.gameObject.activeInHierarchy)
            {
                countrysideSceneryPool.SetScrollOffset(scrollOffset);
            }

            if (countrysidePropsSpawner != null && countrysidePropsSpawner.gameObject.activeInHierarchy)
            {
                countrysidePropsSpawner.SetScrollOffset(scrollOffset);
            }

            horizonLayer?.SetScrollOffset(scrollOffset);
        }

        private void BeginVictoryDrive()
        {
            isVictoryDrive = true;
            victoryTargetDistance = destinationDistance + victoryDriveExtraDistance;
            invulnTimer = 999f;
            playerVehicle?.SetLaneInputLocked(true);
            playerVehicle?.TryChangeLane(HighwayPlayerLaneLayout.MiddleLaneIndex);
            trafficSpawner?.EnterVictoryMode();
        }

        public void RegisterCollision(Vector3 _)
        {
            if (invulnTimer > 0f || isEnding)
            {
                return;
            }

            if (useLives)
            {
                lives--;
                invulnTimer = invulnerabilityAfterHit;
                collisionFeedback?.PlayHit();
                UpdateHud();

                if (lives <= 0)
                {
                    StartCoroutine(FailHighwayRoutine());
                }
            }
            else
            {
                StartCoroutine(FailHighwayRoutine());
            }
        }

        private void EnsureCollisionFeedback()
        {
            if (collisionFeedback == null)
            {
                collisionFeedback = GetComponent<HighwayCollisionFeedback>();
            }

            if (collisionFeedback == null)
            {
                collisionFeedback = gameObject.AddComponent<HighwayCollisionFeedback>();
            }
        }

        private IEnumerator BeginWithFadeIn()
        {
            EnsureFadeOverlay(out CanvasGroup group);
            group.alpha = 0f;
            isRunning = true;
            playerVehicle?.SetLaneInputLocked(false);
            playerVehicle?.SetHighwayInputEnabled(true);
            yield return null;
        }

        private IEnumerator CompleteHighwayRoutine()
        {
            if (isEnding)
            {
                yield break;
            }

            isEnding = true;
            isRunning = false;
            yield return RunFade(0f, 1f, fadeDuration);

            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                HometownArrivalContext.TransferFromHighway();
                SceneManager.LoadScene(nextSceneName);
            }
        }

        private IEnumerator FailHighwayRoutine()
        {
            if (isEnding)
            {
                yield break;
            }

            isEnding = true;
            isRunning = false;
            yield return RunFade(0f, 1f, fadeDuration * 0.85f);
            ClearStuckFadeOverlay();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private IEnumerator RunFade(float from, float to, float duration)
        {
            EnsureFadeOverlay(out CanvasGroup group);
            group.alpha = from;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
        }

        private void BootstrapPlayerVan()
        {
            float scale = HighwayBoardingContext.HasBoardedVehicle
                ? HighwayBoardingContext.VehicleScale
                : defaultVisualScaleFallback();
            int color = HighwayBoardingContext.BodyColorIndex;
            GameObject van = HighwayBoardingContext.VehiclePrefab != null
                ? HighwayBoardingContext.VehiclePrefab
                : playerVanPrefab;

            playerVehicle.Initialize(
                visualPool,
                van,
                scale,
                color,
                HighwayBoardingContext.ModelRotationOffsetY);
            playerVehicle.PlaceAtSpawn(HighwayPlayerLaneLayout.MiddleLaneIndex, HighwayLaneSettings.PlayerSpawnZ);
            followCamera?.Bind(playerVehicle.transform);
        }

        private static float defaultVisualScaleFallback() => 1.55f;

        private void InitializeSystems()
        {
            if (trafficSpawner != null)
            {
                if (!trafficSpawner.gameObject.activeSelf)
                {
                    trafficSpawner.gameObject.SetActive(true);
                }

                trafficSpawner.Initialize(this, visualPool, sedanPrefabs, truckPrefabs, busPrefabs, vanPrefabs);
            }
        }

        private void ConfigureTrafficVisuals()
        {
            Level1TrafficVisualUtility.Configure(
                trafficBodyColors,
                pickupBodyMaterial,
                wheelMaterial,
                glassMaterial,
                kenneyColormapMaterial);
        }

        private void ApplyEnvironment()
        {
            Material skybox = null;
#if UNITY_EDITOR
            skybox = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Settings/ChoTet/Materials/Level1/Level1_StudyMorningSky.mat");
#endif
            ChoTetMarketEnvironment.ApplyHighwayRenderSettings(
                fogStart: 95f,
                fogEnd: 290f,
                skyboxOverride: skybox);

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
            }
        }

        private static void ApplyWarmAfternoonLighting()
        {
            Light sun = FindSunLight();
            if (sun == null)
            {
                return;
            }

            sun.color = new Color(1f, 0.93f, 0.78f);
            sun.intensity = 1.35f;
            sun.shadowStrength = 0.72f;
            sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
            RenderSettings.sun = sun;
        }

        private static Light FindSunLight()
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    return lights[i];
                }
            }

            return null;
        }

        private void DisableUrbanPoolsOnly()
        {
            if (quaterniusTownBlockPool != null)
            {
                quaterniusTownBlockPool.gameObject.SetActive(false);
            }

            if (quaterniusSidewalkPool != null)
            {
                quaterniusSidewalkPool.gameObject.SetActive(false);
            }

            if (staticUrbanNeighborhood != null)
            {
                staticUrbanNeighborhood.gameObject.SetActive(false);
            }

            if (environmentSpawner != null)
            {
                environmentSpawner.gameObject.SetActive(false);
            }
        }

        private void ApplyCountrysideRoadsideMode()
        {
            if (quaterniusTownBlockPool != null)
            {
                quaterniusTownBlockPool.gameObject.SetActive(false);
            }

            if (quaterniusSidewalkPool != null)
            {
                quaterniusSidewalkPool.gameObject.SetActive(false);
            }

            if (staticUrbanNeighborhood != null)
            {
                staticUrbanNeighborhood.gameObject.SetActive(false);
            }

            if (environmentSpawner != null)
            {
                environmentSpawner.gameObject.SetActive(false);
            }

            if (countrysideSceneryPool != null)
            {
                countrysideSceneryPool.gameObject.SetActive(true);
                if (natureRoadsideSpawner != null)
                {
                    natureRoadsideSpawner.gameObject.SetActive(false);
                }

                if (sideGroundPool != null)
                {
                    sideGroundPool.gameObject.SetActive(false);
                }
            }
            else
            {
                if (sideGroundPool != null)
                {
                    sideGroundPool.gameObject.SetActive(true);
                }

                if (natureRoadsideSpawner != null)
                {
                    natureRoadsideSpawner.gameObject.SetActive(true);
                    if (sideGroundPool != null)
                    {
                        natureRoadsideSpawner.InitializeFromSideGround(sideGroundPool);
                    }
                }
            }

            if (countrysidePropsSpawner != null)
            {
                countrysidePropsSpawner.gameObject.SetActive(true);
            }

            if (horizonLayer != null)
            {
                horizonLayer.gameObject.SetActive(true);
            }

            roadPool?.SetShouldersVisible(true);
        }

        private void EnsureHud()
        {
            if (hudCanvas != null)
            {
                return;
            }

            GameObject root = new GameObject("HighwayHUD");
            hudCanvas = root.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 120;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image panel = HighwayHudTypography.CreatePanelBackground(root.transform);
            Transform stack = panel.transform;

            hudTitleText = HighwayHudTypography.CreateLabel("Title", stack, HighwayHudTypography.SemiBoldFont,
                HighwayHudTypography.TitleSize, HighwayHudTypography.WarmOffWhite, FontStyles.Bold);
            hudTitleText.text = "Đường về quê ăn Tết";

            hudLivesText = HighwayHudTypography.CreateLabel("Lives", stack, HighwayHudTypography.RegularFont,
                HighwayHudTypography.BodySize, HighwayHudTypography.WarmOffWhite);
            hudLivesText.text = "Số mạng: 3";

            hudDistanceText = HighwayHudTypography.CreateLabel("Distance", stack, HighwayHudTypography.RegularFont,
                HighwayHudTypography.BodySize, HighwayHudTypography.WarmOffWhite);
            hudDistanceText.text = "Quãng đường: 0.0 / 0.0 km";

            hudHintText = HighwayHudTypography.CreateLabel("Hint", stack, HighwayHudTypography.RegularFont,
                HighwayHudTypography.HintSize, HighwayHudTypography.HintText);
            HighwayHudTypography.ApplyReadability(hudHintText, 0.14f);
            hudHintText.text = "A/D hoặc phím Mũi tên — chuyển làn";
        }

        private void UpdateHud()
        {
            if (hudTitleText == null)
            {
                return;
            }

            float km = distanceTraveled / 1000f;
            float goalKm = destinationDistance / 1000f;

            hudTitleText.text = isVictoryDrive ? "Đã gần đến nhà..." : "Đường về quê ăn Tết";
            hudDistanceText.text = "Quãng đường: " + km.ToString("0.0") + " / " + goalKm.ToString("0.0") + " km";
            hudHintText.text = isVictoryDrive
                ? "Tiếp tục đi — quê nhà ở ngay phía trước"
                : "A/D hoặc phím Mũi tên — chuyển làn";

            if (useLives)
            {
                hudLivesText.gameObject.SetActive(true);
                hudLivesText.text = "Số mạng: " + lives;
            }
            else
            {
                hudLivesText.gameObject.SetActive(false);
            }
        }

        private static void EnsureFadeOverlay(out CanvasGroup group)
        {
            const string fadeName = "HighwayFadeOverlay";
            GameObject existing = GameObject.Find(fadeName);
            if (existing == null)
            {
                existing = new GameObject(fadeName);
                Canvas canvas = existing.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 600;

                GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                panel.transform.SetParent(existing.transform, false);
                RectTransform rect = panel.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                panel.GetComponent<Image>().color = Color.black;
                group = panel.GetComponent<CanvasGroup>();
            }
            else
            {
                group = existing.GetComponentInChildren<CanvasGroup>();
            }
        }
    }
}
