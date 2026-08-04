using ChoTet.Level1.HometownArrival;
using ChoTet.Level1.UI;
using ChoTet.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1
{
    /// <summary>
    /// Tracks Level 1 gameplay phases inside one scene:
    /// Phase 1 — indoor packing room, Phase 2 — outdoor street walk to bus pickup.
    /// </summary>
    public class Level1PhaseController : MonoBehaviour
    {
        public enum Level1Phase
        {
            IndoorPacking = 1,
            OutdoorStreet = 2
        }

        public static Level1PhaseController Instance { get; private set; }

        [Header("Phase 2 — outdoor street")]
        [SerializeField] private Transform outdoorSpawn;
        [SerializeField] private GameObject outdoorAreaRoot;
        [SerializeField] private GameObject rentalRoomInterior;
        [SerializeField] private Level1ScreenFade screenFade;
        [SerializeField] private IndoorHighAngleCamera gameplayCamera;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Material outdoorSkybox;

        [Header("Outdoor camera (same cozy high-angle style)")]
        [SerializeField] private float outdoorCameraHeight = 6.8f;
        [SerializeField] private float outdoorForwardOffset = 5.2f;
        [SerializeField] private float outdoorLookHeight = 0.55f;
        [SerializeField] private float outdoorFarClip = 58f;
        [SerializeField] private BoardingHouseStreetOrbitCamera streetOrbitCamera;
        [SerializeField] private StreetLookAheadCamera streetLookAheadCamera;

        [Header("Outdoor path bounds for camera clamp")]
        [SerializeField] private float outdoorMinX = Level1OutdoorMovementSettings.PlayMinX;
        [SerializeField] private float outdoorMaxX = Level1OutdoorMovementSettings.PlayMaxX;
        [SerializeField] private float outdoorMinZ = Level1OutdoorMovementSettings.PlayEndZ - 8f;
        [SerializeField] private float outdoorMaxZ = Level1OutdoorMovementSettings.PlayStartZ + 1.2f;
        [SerializeField] private float outdoorClampMargin = Level1OutdoorMovementSettings.CameraClampMargin;

        [Header("Boarding transition")]
        [SerializeField] private string nextLevelSceneName = "Level1_Highway";

        private TopDownCameraOcclusion cameraOcclusion;

        private Material previousSkybox;
        private AmbientMode previousAmbientMode;
        private Color previousAmbientLight;
        private bool previousFog;
        private float previousFarClip;
        private bool transitionStarted;

        public Level1Phase CurrentPhase { get; private set; } = Level1Phase.IndoorPacking;
        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveReferences();
        }

        private void Start()
        {
            // Outdoor geometry lives in the same scene but must not appear inside the rental room.
            if (CurrentPhase == Level1Phase.IndoorPacking)
            {
                SetIndoorAreaActive(true);
                SetOutdoorAreaActive(false);
                Level1IndoorSky.Apply(mainCamera);
            }
            else
            {
                SetOutdoorAreaActive(true);
                SetIndoorAreaActive(false);
            }

            EnsureFirstPersonCamera();
        }

        private void EnsureFirstPersonCamera()
        {
            ResolveReferences();
            if (mainCamera == null)
            {
                return;
            }

            Transform followTarget = FindAnyObjectByType<PlayerTopDownController>()?.transform;
            FirstPersonCameraController.EnsurePrimary(mainCamera, followTarget);
            FirstPersonCameraController fp = mainCamera.GetComponent<FirstPersonCameraController>();
            if (fp != null)
            {
                if (CurrentPhase == Level1Phase.IndoorPacking)
                {
                    fp.SyncEyeHeightForBoardingHouseIndoor();
                }
                else
                {
                    fp.SyncEyeHeightToCharacters();
                }
            }
            if (gameplayCamera != null)
            {
                gameplayCamera.enabled = false;
            }

            if (streetOrbitCamera != null)
            {
                streetOrbitCamera.enabled = false;
            }

            if (streetLookAheadCamera != null)
            {
                streetLookAheadCamera.enabled = false;
            }
        }

        /// <summary>Called by the departure door once packing is complete.</summary>
        public bool BeginOutdoorTransition(PlayerTopDownController player)
        {
            if (transitionStarted || CurrentPhase == Level1Phase.OutdoorStreet || IsTransitioning)
            {
                return false;
            }

            ResolveReferences();
            if (player == null || outdoorSpawn == null)
            {
                Debug.LogWarning("[Level1PhaseController] Missing player or Level1_OutdoorSpawn.");
                return false;
            }

            transitionStarted = true;
            StartCoroutine(BeginOutdoorTransitionRoutine(player));
            return true;
        }

        private IEnumerator BeginOutdoorTransitionRoutine(PlayerTopDownController player)
        {
            IsTransitioning = true;
            player.MovementLocked = true;

            TrafficManager traffic = FindTrafficManager();
            int bodyColors = traffic != null ? Mathf.Max(1, traffic.GetBodyColorCount()) : 5;
            int candidatePrefabs = traffic != null ? Mathf.Max(1, traffic.GetBusStopCandidatePrefabCount()) : 5;

            if (screenFade != null)
            {
                bool midpointReached = false;
                screenFade.RunTransition(() => midpointReached = true);

                while (!midpointReached)
                {
                    yield return null;
                }

                // Heavy outdoor activation runs while the screen is still black.
                BusStopChallengeSession.BeginOutdoorChallenge(bodyColors, candidatePrefabs);
                yield return ApplyOutdoorPhaseDeferred(player);

                while (screenFade.IsFading)
                {
                    yield return null;
                }
            }
            else
            {
                BusStopChallengeSession.BeginOutdoorChallenge(bodyColors, candidatePrefabs);
                yield return ApplyOutdoorPhaseDeferred(player);
            }

            traffic = FindTrafficManager();
            traffic?.NotifyOutdoorPhaseReady();

            InventoryUIController ui = FindAnyObjectByType<InventoryUIController>();
            RetroChocolateDialogueUI dialogue = RetroChocolateDialogueUI.Ensure();
            yield return dialogue.ShowLine(
                "Mình",
                "Mình sắp trễ giờ ra bến xe rồi, điện thoại mình đâu rồi, mình phải lấy ra để xem bản đồ và xe mình là xe số mấy.");
            ui?.ShowPickupPopup("Nhấn I để mở điện thoại, xem Map và Ticket.", 4f);

            player.MovementLocked = false;
            IsTransitioning = false;
        }

        /// <summary>Called when the player boards a stopped pickup vehicle at the bus stop.</summary>
        public void BeginBoardingTransition(PlayerTopDownController player, VehicleTraffic vehicle)
        {
            if (IsTransitioning || CurrentPhase != Level1Phase.OutdoorStreet || player == null || vehicle == null)
            {
                return;
            }

            switch (vehicle.BoardingOutcome)
            {
                case VehicleBoardingOutcome.HomeRide:
                    BeginHomeRideBoarding(player, vehicle);
                    break;
                case VehicleBoardingOutcome.Kidnap:
                    StartCoroutine(BoardingFailRoutine(player, vehicle, VehicleBoardingOutcome.Kidnap));
                    break;
                case VehicleBoardingOutcome.WrongVehicle:
                    StartCoroutine(BoardingFailRoutine(player, vehicle, VehicleBoardingOutcome.WrongVehicle));
                    break;
                default:
                    Debug.LogWarning(
                        "[Level1PhaseController] Boarding ignored — vehicle has no boarding outcome ("
                        + vehicle.name + ").");
                    break;
            }
        }

        public void BeginEncoreDriverDialogue(VehicleTraffic encoreVehicle)
        {
            if (IsTransitioning || encoreDialogueRoutine != null || BusStopChallengeSession.EncoreDialoguePlayed)
            {
                return;
            }

            if (encoreVehicle == null || !encoreVehicle.IsEncoreRide)
            {
                return;
            }

            encoreDialogueRoutine = StartCoroutine(EncoreDriverDialogueRoutine(encoreVehicle));
        }

        private Coroutine encoreDialogueRoutine;

        private IEnumerator EncoreDriverDialogueRoutine(VehicleTraffic encoreVehicle)
        {
            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            if (player == null || encoreVehicle == null)
            {
                encoreDialogueRoutine = null;
                yield break;
            }

            IsTransitioning = true;
            player.MovementLocked = true;
            player.StopScriptedLocomotion();

            InventoryUIController ui = FindAnyObjectByType<InventoryUIController>();
            ui?.SetPrompt(string.Empty);

            RetroChocolateDialogueUI dialogue = RetroChocolateDialogueUI.Ensure();
            yield return dialogue.ShowLine("Bác tài", "Ủa em, có coi kĩ biển xe không mà sao hồi nãy từ chối?");
            yield return dialogue.ShowLine("Em", "Lỗi em lỗi em");
            yield return dialogue.ShowLine("Em", "Dạ phiền bác tài nhiều ạ?");

            BusStopChallengeSession.MarkEncoreDialoguePlayed();
            encoreDialogueRoutine = null;

            // Dialogue held IsTransitioning; release before handing off to boarding fade.
            IsTransitioning = false;
            BeginBoardingTransition(player, encoreVehicle);

            if (!IsTransitioning)
            {
                player.MovementLocked = false;
            }
        }

        private IEnumerator BoardingFailRoutine(PlayerTopDownController player, VehicleTraffic vehicle, VehicleBoardingOutcome outcome)
        {
            IsTransitioning = true;
            player.MovementLocked = true;
            player.SetCharacterVisible(false);

            yield return BusStopBoardingFailOverlay.PlayMoment(outcome);

            TrafficManager traffic = FindAnyObjectByType<TrafficManager>();
            traffic?.HandleVehicleBoarded(vehicle);

            player.SetCharacterVisible(true);
            player.MovementLocked = false;
            IsTransitioning = false;
        }

        private void BeginHomeRideBoarding(PlayerTopDownController player, VehicleTraffic vehicle)
        {
            IsTransitioning = true;
            player.MovementLocked = true;
            ResolveStreetOrbitCamera()?.ExitZone();
            HometownArrivalPlayerVisualContext.CaptureFromPlayer(player);

            TrafficManager traffic = FindAnyObjectByType<TrafficManager>();
            traffic?.HandleVehicleBoarded(vehicle);

            BusStopChallengeSession.MarkPlayerSucceeded();
            CaptureBoardedVehicleForHighway(vehicle);

            // Cosmetic only — never gate the highway load on fade completion.
            screenFade?.RunFadeOut(null);

            if (string.IsNullOrWhiteSpace(nextLevelSceneName))
            {
                player.MovementLocked = false;
                IsTransitioning = false;
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(nextLevelSceneName))
            {
                Debug.LogError(
                    "[Level1PhaseController] Highway scene is not in Build Settings: " + nextLevelSceneName);
                player.MovementLocked = false;
                IsTransitioning = false;
                return;
            }

            SceneManager.LoadScene(nextLevelSceneName);
        }

        private IEnumerator ApplyOutdoorPhaseDeferred(PlayerTopDownController player)
        {
            CurrentPhase = Level1Phase.OutdoorStreet;

            SetOutdoorAreaActive(true);
            yield return null;
            yield return null;

            SetIndoorAreaActive(false);
            yield return null;

            Physics.SyncTransforms();

            Vector3 spawnPosition = outdoorSpawn.position;
            spawnPosition.y = Level1OutdoorSettings.WalkSurfaceY;
            Quaternion spawnRotation = outdoorSpawn.rotation;
            player.TeleportTo(spawnPosition, spawnRotation);

            ConfigureOutdoorEnvironment();
            yield return null;

            ConfigureOutdoorCamera(player.transform);
            yield return null;

            EnableOutdoorSceneLighting();
            yield return null;

            EnableOutdoorCameraOcclusion(player.transform);
            yield return null;

            HidePlayerThroughDriveSegments(player);
            Level1BusStopRoadsideBarrier.EnsureInScene();
            Level1OutdoorLivesController.Ensure();

            FindTrafficManager()?.NotifyOutdoorPhaseReady();
        }

        private void ApplyOutdoorPhase(PlayerTopDownController player)
        {
            CurrentPhase = Level1Phase.OutdoorStreet;

            SetOutdoorAreaActive(true);
            SetIndoorAreaActive(false);
            Physics.SyncTransforms();

            Vector3 spawnPosition = outdoorSpawn.position;
            spawnPosition.y = Level1OutdoorSettings.WalkSurfaceY;
            Quaternion spawnRotation = outdoorSpawn.rotation;
            player.TeleportTo(spawnPosition, spawnRotation);

            ConfigureOutdoorEnvironment();
            ConfigureOutdoorCamera(player.transform);
            EnableOutdoorSceneLighting();
            EnableOutdoorCameraOcclusion(player.transform);
            HidePlayerThroughDriveSegments(player);
            Level1BusStopRoadsideBarrier.EnsureInScene();
            Level1OutdoorLivesController.Ensure();
        }

        /// <summary>Invisible avatar from boarding-house street through highway; reunion reveals again.</summary>
        private static void HidePlayerThroughDriveSegments(PlayerTopDownController player)
        {
            if (player == null)
            {
                return;
            }

            player.ForceHideCharacterVisual = true;
        }

        private void EnableOutdoorCameraOcclusion(Transform followTarget)
        {
            if (cameraOcclusion == null && mainCamera != null)
            {
                cameraOcclusion = mainCamera.GetComponent<TopDownCameraOcclusion>();
            }

            if (cameraOcclusion == null)
            {
                return;
            }

            cameraOcclusion.SetTarget(followTarget);
            cameraOcclusion.SetActive(true);
        }

        private void SetOutdoorAreaActive(bool active)
        {
            ResolveOutdoorAreaRoot();
            if (outdoorAreaRoot != null)
            {
                outdoorAreaRoot.SetActive(active);
                if (active)
                {
                    EnsureOutdoorNpcDialogues();
                }
            }
        }

        private void EnsureOutdoorNpcDialogues()
        {
            // Workplace NPCs use NPCDialogue on each character — no bootstrap required.
        }

        /// <summary>Hides the packing-room interior so it does not bleed into the outdoor camera view.</summary>
        private void SetIndoorAreaActive(bool active)
        {
            ResolveRentalRoomInterior();
            if (rentalRoomInterior != null)
            {
                rentalRoomInterior.SetActive(active);
            }
        }

        private void ResolveRentalRoomInterior()
        {
            if (rentalRoomInterior != null)
            {
                return;
            }

            Transform worldRoot = GameObject.Find("Level1World")?.transform;
            if (worldRoot == null)
            {
                return;
            }

            Transform room = worldRoot.Find("Level1_IndoorArea");
            if (room == null)
            {
                room = worldRoot.Find("RentalRoomInterior");
            }
            if (room != null)
            {
                rentalRoomInterior = room.gameObject;
            }
        }

        private void ResolveOutdoorAreaRoot()
        {
            if (outdoorAreaRoot != null)
            {
                return;
            }

            GameObject found = GameObject.Find("Level1_OutdoorArea");
            if (found != null)
            {
                outdoorAreaRoot = found;
            }
        }

        private void EnableOutdoorSceneLighting()
        {
            ResolveOutdoorAreaRoot();
            if (outdoorAreaRoot == null)
            {
                return;
            }

            Transform outdoorLighting = outdoorAreaRoot.transform.Find("OutdoorLighting");
            if (outdoorLighting == null)
            {
                return;
            }

            for (int i = 0; i < outdoorLighting.childCount; i++)
            {
                outdoorLighting.GetChild(i).gameObject.SetActive(true);
            }
        }

        private void ConfigureOutdoorEnvironment()
        {
            previousSkybox = RenderSettings.skybox;
            previousAmbientMode = RenderSettings.ambientMode;
            previousAmbientLight = RenderSettings.ambientLight;
            previousFog = RenderSettings.fog;

            // Bright daytime street — blue sky + soft horizon haze (not sunset).
            ChoTetMarketEnvironment.ApplyHometownArrivalRenderSettings(outdoorSkybox, fogStart: 55f, fogEnd: 125f);
            RenderSettings.ambientIntensity = 1.15f;
        }

        private void ConfigureOutdoorCamera(Transform followTarget)
        {
            ResolveReferences();

            if (mainCamera != null)
            {
                previousFarClip = mainCamera.farClipPlane;
                mainCamera.farClipPlane = outdoorFarClip;
                mainCamera.clearFlags = CameraClearFlags.Skybox;
            }

            if (gameplayCamera == null)
            {
                return;
            }

            float cameraMaxX = Level1OutdoorMovementSettings.OutdoorCameraMaxX;
            float cameraMinZ = Level1OutdoorMovementSettings.OutdoorCameraMinZ;

            gameplayCamera.SetFollowTarget(followTarget);
            gameplayCamera.Configure(followTarget.position, outdoorCameraHeight, outdoorForwardOffset, 0f,
                outdoorLookHeight);
            gameplayCamera.SetRectBounds(outdoorMinX, cameraMaxX, cameraMinZ, outdoorMaxZ, outdoorClampMargin);
            gameplayCamera.SnapToFollowTarget();

            BoardingHouseStreetOrbitCamera orbit = ResolveStreetOrbitCamera();
            FirstPersonCameraController firstPerson = mainCamera != null
                ? mainCamera.GetComponent<FirstPersonCameraController>()
                : null;
            if (orbit != null && firstPerson == null)
            {
                orbit.ConfigureRig(outdoorForwardOffset, outdoorCameraHeight, outdoorLookHeight, 0f);
                orbit.ConfigureOutdoorBounds(outdoorMinX, cameraMaxX, cameraMinZ, outdoorMaxZ, outdoorClampMargin);
                orbit.EnterZone(followTarget);
            }
            else if (firstPerson != null)
            {
                firstPerson.Bind(followTarget);
                firstPerson.EnsurePrimary();
                // Outdoor street: eye level with NPCs (no boarding-house boost).
                firstPerson.SyncEyeHeightToCharacters();
                if (gameplayCamera != null)
                {
                    gameplayCamera.enabled = false;
                }

                if (orbit != null)
                {
                    orbit.enabled = false;
                }

                if (streetLookAheadCamera != null)
                {
                    streetLookAheadCamera.enabled = false;
                }
            }
            else if (gameplayCamera != null)
            {
                gameplayCamera.enabled = true;
            }

            StreetLookAheadCamera lookAhead = ResolveStreetLookAheadCamera();
            lookAhead?.ConfigureOutdoorRig(outdoorCameraHeight, outdoorForwardOffset, outdoorLookHeight);
        }

        private StreetLookAheadCamera ResolveStreetLookAheadCamera()
        {
            if (streetLookAheadCamera != null)
            {
                return streetLookAheadCamera;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return null;
            }

            streetLookAheadCamera = mainCamera.GetComponent<StreetLookAheadCamera>();
            if (streetLookAheadCamera == null)
            {
                streetLookAheadCamera = mainCamera.gameObject.AddComponent<StreetLookAheadCamera>();
            }

            return streetLookAheadCamera;
        }

        private BoardingHouseStreetOrbitCamera ResolveStreetOrbitCamera()
        {
            if (streetOrbitCamera != null)
            {
                return streetOrbitCamera;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return null;
            }

            streetOrbitCamera = mainCamera.GetComponent<BoardingHouseStreetOrbitCamera>();
            if (streetOrbitCamera == null)
            {
                streetOrbitCamera = mainCamera.gameObject.AddComponent<BoardingHouseStreetOrbitCamera>();
            }

            return streetOrbitCamera;
        }

        private static void CaptureBoardedVehicleForHighway(VehicleTraffic vehicle)
        {
            if (vehicle == null)
            {
                Highway.HighwayBoardingContext.SetFromPickup(null, 1.55f, 0);
                return;
            }

            Level1TrafficVehicleProfile profile = vehicle.CurrentProfile;
            Highway.HighwayBoardingContext.SetFromPickup(
                profile.prefab,
                profile.scale,
                profile.bodyColorIndex,
                modelRotationOffsetY: 0f);
            HometownArrival.HometownArrivalContext.PrimeFromBoarding(
                profile.prefab,
                profile.scale,
                profile.bodyColorIndex,
                modelRotationOffsetY: 0f);
        }

        private void ResolveReferences()
        {
            if (screenFade == null)
            {
                screenFade = GetComponent<Level1ScreenFade>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (gameplayCamera == null && mainCamera != null)
            {
                gameplayCamera = mainCamera.GetComponent<IndoorHighAngleCamera>();
            }

            if (streetOrbitCamera == null && mainCamera != null)
            {
                streetOrbitCamera = mainCamera.GetComponent<BoardingHouseStreetOrbitCamera>();
            }

            if (streetLookAheadCamera == null && mainCamera != null)
            {
                streetLookAheadCamera = mainCamera.GetComponent<StreetLookAheadCamera>();
            }

            ResolveOutdoorAreaRoot();
            if (outdoorSpawn == null && outdoorAreaRoot != null)
            {
                Transform spawn = outdoorAreaRoot.transform.Find("Level1_OutdoorSpawn");
                if (spawn != null)
                {
                    outdoorSpawn = spawn;
                }
            }

            if (outdoorSpawn == null)
            {
                Transform worldRoot = GameObject.Find("Level1World")?.transform;
                Transform outdoorArea = worldRoot != null ? worldRoot.Find("Level1_OutdoorArea") : null;
                if (outdoorArea != null)
                {
                    if (outdoorAreaRoot == null)
                    {
                        outdoorAreaRoot = outdoorArea.gameObject;
                    }

                    outdoorSpawn = outdoorArea.Find("Level1_OutdoorSpawn");
                }
            }
        }

        private TrafficManager FindTrafficManager()
        {
            if (outdoorAreaRoot != null)
            {
                TrafficManager onOutdoor = outdoorAreaRoot.GetComponentInChildren<TrafficManager>(true);
                if (onOutdoor != null)
                {
                    return onOutdoor;
                }
            }

            return FindAnyObjectByType<TrafficManager>();
        }
    }
}
