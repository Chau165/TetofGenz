using System.Collections;
using ChoTet.Level1;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// Level 1 ending: flashback → bus to hometown entrance path → level completes.
    /// No walk-home gameplay — the scene implies the player can walk the rest of the way on their own.
    /// </summary>
    public class HometownArrivalController : MonoBehaviour
    {
        private enum ArrivalPhase
        {
            Flashback,
            VehicleApproach,
            FamilyReunion,
            DropOffComplete
        }

        [Header("References")]
        [SerializeField] private PlayerTopDownController player;
        [SerializeField] private Level1ScreenFade screenFade;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform pickupBus;
        [SerializeField] private TopDownCameraFollow cameraFollow;

        [Header("Beat 1 — flashback")]
        [SerializeField] private float flashbackDuration = 2.2f;
        [SerializeField] private string flashbackLine = "Đường về quê dài lắm...";

        [Header("Beat 2 — vehicle approach (highway lane grid)")]
        [SerializeField] private float busLaneX = HometownArrivalLayout.BusLaneX;
        [SerializeField] private float busStartZ = HometownArrivalLayout.BusStartZ;
        [SerializeField] private float busStopZ = HometownArrivalLayout.BusStopZ;
        [SerializeField] private float busGroundY = HometownArrivalLayout.BusGroundY;
        [SerializeField] private float parkedRoadsideX = HometownArrivalLayout.ParkedRoadsideX;
        [SerializeField] private float pullOverStartNormalized = HometownArrivalLayout.PullOverStartNormalized;
        [SerializeField] private float busApproachDuration = 5.4f;
        [SerializeField] private float busArrivalPause = 0.85f;
        [SerializeField] private float arrivalFadeDuration = 0.75f;
        [SerializeField] private float disembarkSafeRadius = 3.2f;
        [SerializeField] private float disembarkSafetyDuration = 2.2f;

        [Header("Camera — straight on approach, diagonal after disembark")]
        [SerializeField] private float cameraBlendDuration = 1.1f;

        [Header("Level complete")]
        [SerializeField] private string completionTitle = "Về tới quê rồi — nhà ngay phía trước.";
        [SerializeField] private float completionHoldSeconds = 2.6f;
        [SerializeField] private string levelCompleteSceneName = "LevelSelectMenu";
        [SerializeField] private bool loadSceneAfterDropOff = true;

        private ArrivalPhase phase = ArrivalPhase.Flashback;
        private Canvas hudCanvas;
        private TextMeshProUGUI completionText;
        private CanvasGroup flashbackGroup;
        private TextMeshProUGUI flashbackText;

        private void Awake()
        {
            ApplyForcedLayoutFromCode();
            ResolveReferences();
        }

        private void Start()
        {
            ApplyForcedLayoutFromCode();
            StartCoroutine(RunArrivalSequence());
        }

        public void ConfigureFromBootstrap(
            PlayerTopDownController playerRef,
            Level1ScreenFade fade,
            Transform spawn,
            Transform bus,
            TopDownCameraFollow camera)
        {
            player = playerRef;
            screenFade = fade;
            spawnPoint = spawn;
            pickupBus = bus;
            cameraFollow = camera;
        }

        public void ConfigureVehicleApproach(
            float laneX,
            float startZ,
            float stopZ,
            float groundY,
            float roadsideParkX = HometownArrivalLayout.ParkedRoadsideX)
        {
            busLaneX = laneX;
            busStartZ = startZ;
            busStopZ = stopZ;
            busGroundY = groundY;
            parkedRoadsideX = roadsideParkX;
        }

        private void ApplyForcedLayoutFromCode()
        {
            busLaneX = HometownArrivalLayout.BusLaneX;
            busStartZ = HometownArrivalLayout.BusStartZ;
            busStopZ = HometownArrivalLayout.BusStopZ;
            busGroundY = HometownArrivalLayout.BusGroundY;
            parkedRoadsideX = HometownArrivalLayout.ParkedRoadsideX;
            pullOverStartNormalized = HometownArrivalLayout.PullOverStartNormalized;
            SyncSpawnPointToParkedStop();
        }

        private void SyncSpawnPointToParkedStop()
        {
            if (spawnPoint == null)
            {
                return;
            }

            spawnPoint.position = new Vector3(
                parkedRoadsideX + 0.35f,
                busGroundY + 0.15f,
                busStopZ - 0.2f);
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<PlayerTopDownController>();
            }

            if (screenFade == null)
            {
                screenFade = GetComponent<Level1ScreenFade>();
            }

            if (spawnPoint == null)
            {
                GameObject spawnObject = GameObject.Find("ArrivalSpawnPoint")
                    ?? GameObject.Find("HomeSpawnPoint");
                if (spawnObject != null)
                {
                    spawnPoint = spawnObject.transform;
                }
            }

            if (pickupBus == null)
            {
                GameObject busObject = GameObject.Find("PickupBus");
                if (busObject != null)
                {
                    pickupBus = busObject.transform;
                }
            }

            if (cameraFollow == null)
            {
                cameraFollow = FindAnyObjectByType<TopDownCameraFollow>();
            }
        }

        private IEnumerator RunArrivalSequence()
        {
            PrepareOpeningState();
            EnsureFlashbackOverlay();
            EnsureCompletionHud();

            yield return RunFlashbackBeat();
            yield return RunVehicleApproachBeat();
            yield return DisembarkPlayer();
            yield return RunFamilyReunionBeat();
            yield return CompleteDropOffRoutine();
        }

        private IEnumerator RunFamilyReunionBeat()
        {
            phase = ArrivalPhase.FamilyReunion;
            yield return HometownArrivalFamilyReunion.Run(player, cameraFollow);
        }

        private void PrepareOpeningState()
        {
            if (pickupBus != null)
            {
                pickupBus.SetPositionAndRotation(
                    new Vector3(busLaneX, busGroundY, busStartZ),
                    Quaternion.Euler(0f, 180f, 0f));
            }

            if (player != null)
            {
                player.MovementLocked = true;
                player.gameObject.SetActive(false);
            }

            if (cameraFollow != null)
            {
                HometownArrivalCamera.ApplyApproachDefaults(cameraFollow);
                if (pickupBus != null)
                {
                    cameraFollow.target = pickupBus;
                    cameraFollow.SnapToTarget();
                }
            }
        }

        private IEnumerator RunFlashbackBeat()
        {
            phase = ArrivalPhase.Flashback;

            if (flashbackGroup != null)
            {
                flashbackGroup.gameObject.SetActive(true);
                flashbackGroup.alpha = 0f;
            }

            if (flashbackText != null)
            {
                flashbackText.text = flashbackLine;
            }

            float elapsed = 0f;
            float fadeIn = Mathf.Min(0.45f, flashbackDuration * 0.35f);
            float hold = Mathf.Max(0.4f, flashbackDuration - fadeIn - 0.35f);
            float fadeOut = flashbackDuration - fadeIn - hold;

            while (elapsed < fadeIn)
            {
                elapsed += Time.unscaledDeltaTime;
                if (flashbackGroup != null)
                {
                    flashbackGroup.alpha = Mathf.Clamp01(elapsed / fadeIn);
                }

                yield return null;
            }

            if (flashbackGroup != null)
            {
                flashbackGroup.alpha = 1f;
            }

            yield return new WaitForSecondsRealtime(hold);

            elapsed = 0f;
            while (elapsed < fadeOut)
            {
                elapsed += Time.unscaledDeltaTime;
                if (flashbackGroup != null)
                {
                    flashbackGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOut);
                }

                yield return null;
            }

            if (flashbackGroup != null)
            {
                flashbackGroup.alpha = 0f;
                flashbackGroup.gameObject.SetActive(false);
            }
        }

        private IEnumerator RunVehicleApproachBeat()
        {
            phase = ArrivalPhase.VehicleApproach;
            ApplyForcedLayoutFromCode();

            if (pickupBus == null)
            {
                yield break;
            }

            Vector3 end = new Vector3(parkedRoadsideX, busGroundY, busStopZ);

            float elapsed = 0f;
            while (elapsed < busApproachDuration)
            {
                elapsed += Time.deltaTime;
                float t = busApproachDuration > 0f ? Mathf.Clamp01(elapsed / busApproachDuration) : 1f;
                float eased = 1f - Mathf.Pow(1f - t, 2.4f);
                float pullT = pullOverStartNormalized >= 1f
                    ? 1f
                    : Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(pullOverStartNormalized, 1f, t));

                float x = Mathf.Lerp(busLaneX, parkedRoadsideX, pullT);
                float z = Mathf.Lerp(busStartZ, busStopZ, eased);
                float junctionZ = HometownArrivalLayout.JunctionWorldZ;
                z = Mathf.Max(z, junctionZ + 0.45f);
                pickupBus.position = new Vector3(x, busGroundY, z);
                yield return null;
            }

            pickupBus.position = end;
            yield return new WaitForSeconds(busArrivalPause);
        }

        private IEnumerator DisembarkPlayer()
        {
            if (player == null)
            {
                yield break;
            }

            Transform exitPoint = HometownArrivalExitSpawn.EnsureExitPoint(pickupBus, pickupBus != null ? pickupBus.parent : null);
            if (pickupBus != null)
            {
                HometownArrivalExitSpawn.RefreshExitPoint(exitPoint, pickupBus);
            }

            Vector3 exitPosition = exitPoint != null
                ? exitPoint.position
                : HometownArrivalExitSpawn.SnapToGround(
                    pickupBus != null
                        ? HometownArrivalExitSpawn.ComputeBesideBus(pickupBus)
                        : spawnPoint != null ? spawnPoint.position : Vector3.zero,
                    busGroundY);

            Quaternion exitRotation = exitPoint != null
                ? exitPoint.rotation
                : spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            player.gameObject.SetActive(true);
            player.ForceHideCharacterVisual = true;
            player.TeleportTo(exitPosition, exitRotation);
            player.MovementLocked = true;
            StartCoroutine(GuardDisembarkPosition(exitPosition, exitRotation));

            if (cameraFollow != null)
            {
                cameraFollow.target = player.transform;
                FirstPersonCameraController fp = FirstPersonCameraController.Active
                    ?? FindAnyObjectByType<FirstPersonCameraController>();
                if (fp != null)
                {
                    fp.Bind(player.transform);
                    fp.SetYawPitch(player.transform.eulerAngles.y, 0f);
                }
                else
                {
                    yield return BlendCameraFraming(
                        HometownArrivalCamera.ApproachOffset,
                        HometownArrivalCamera.ReunionOffset,
                        HometownArrivalCamera.ApproachLookOffset,
                        HometownArrivalCamera.ReunionLookOffset,
                        cameraBlendDuration);
                    HometownArrivalCamera.ApplyReunionDefaults(cameraFollow);
                }
            }
            else
            {
                yield return null;
            }
        }

        private IEnumerator GuardDisembarkPosition(Vector3 safePosition, Quaternion safeRotation)
        {
            float until = Time.time + Mathf.Max(0.5f, disembarkSafetyDuration);
            while (Time.time < until)
            {
                if (player == null)
                {
                    yield break;
                }

                Vector3 current = player.transform.position;
                bool fellBelowGround = current.y < busGroundY - 0.9f;
                bool driftedFar = Vector2.Distance(
                    new Vector2(current.x, current.z),
                    new Vector2(safePosition.x, safePosition.z)) > disembarkSafeRadius;

                if (fellBelowGround || driftedFar)
                {
                    player.TeleportTo(safePosition, safeRotation);
                }

                yield return null;
            }
        }

        private IEnumerator CompleteDropOffRoutine()
        {
            phase = ArrivalPhase.DropOffComplete;

            HometownArrivalEnvironment.SuppressLegacyWorld();

            if (completionText != null)
            {
                completionText.text = completionTitle;
                completionText.transform.parent.gameObject.SetActive(true);
            }

            yield return new WaitForSecondsRealtime(completionHoldSeconds);

            if (loadSceneAfterDropOff && !string.IsNullOrWhiteSpace(levelCompleteSceneName))
            {
                if (screenFade != null)
                {
                    bool fadeDone = false;
                    screenFade.RunFadeOut(() => fadeDone = true, arrivalFadeDuration);
                    while (!fadeDone)
                    {
                        yield return null;
                    }
                }

                SceneManager.LoadScene(levelCompleteSceneName);
            }
        }

        private IEnumerator BlendCameraFraming(
            Vector3 fromOffset,
            Vector3 toOffset,
            Vector3 fromLook,
            Vector3 toLook,
            float duration)
        {
            if (cameraFollow == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                cameraFollow.offset = Vector3.Lerp(fromOffset, toOffset, eased);
                cameraFollow.lookOffset = Vector3.Lerp(fromLook, toLook, eased);
                yield return null;
            }

            cameraFollow.offset = toOffset;
            cameraFollow.lookOffset = toLook;
        }

        private void EnsureFlashbackOverlay()
        {
            if (flashbackGroup != null)
            {
                return;
            }

            GameObject root = new GameObject("HometownFlashbackOverlay");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 480;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panel.transform.SetParent(root.transform, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.42f, 0.28f, 0.16f, 0.72f);
            image.raycastTarget = false;

            flashbackGroup = panel.GetComponent<CanvasGroup>();
            flashbackGroup.alpha = 0f;
            flashbackGroup.blocksRaycasts = false;

            flashbackText = HighwayHudTypography.CreateLabel("FlashbackLine", panel.transform,
                HighwayHudTypography.SemiBoldFont, HighwayHudTypography.BodySize,
                HighwayHudTypography.WarmOffWhite, FontStyles.Italic);
            flashbackText.alignment = TextAlignmentOptions.Center;
            flashbackText.text = flashbackLine;

            RectTransform textRect = flashbackText.rectTransform;
            textRect.anchorMin = new Vector2(0.12f, 0.38f);
            textRect.anchorMax = new Vector2(0.88f, 0.58f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            panel.SetActive(false);
        }

        private void EnsureCompletionHud()
        {
            if (hudCanvas != null)
            {
                return;
            }

            GameObject root = new GameObject("HometownArrivalHUD");
            hudCanvas = root.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 120;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image completionPanel = HighwayHudTypography.CreatePanelBackground(root.transform);
            completionText = HighwayHudTypography.CreateLabel("Completion", completionPanel.transform,
                HighwayHudTypography.SemiBoldFont, HighwayHudTypography.TitleSize,
                HighwayHudTypography.WarmOffWhite, FontStyles.Bold);
            completionText.text = completionTitle;
            completionPanel.gameObject.SetActive(false);
        }
    }
}
