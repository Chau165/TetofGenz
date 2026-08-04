using ChoTet.Menu;
using ChoTet.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChoTet.Level1
{
    /// <summary>
    /// Outdoor Level 1: 3 lives. Warns when stepping onto the highway; each car hit costs one life.
    /// </summary>
    public class Level1OutdoorLivesController : MonoBehaviour
    {
        public const int StartingLives = 3;
        private const float HitCooldownSeconds = 1.55f;
        private const float WarningRefreshSeconds = 0.2f;
        private const float HitRadius = 1.55f;

        public static Level1OutdoorLivesController Instance { get; private set; }

        [SerializeField] private int lives = StartingLives;

        private float hitCooldown;
        private float warningTimer;
        private bool onRoad;
        private bool isGameOver;
        private Text livesText;
        private Text warningText;
        private CanvasGroup warningGroup;
        private PlayerTopDownController player;
        private TrafficManager traffic;

        public int Lives => lives;
        public bool IsOnRoad => onRoad;

        public static Level1OutdoorLivesController Ensure()
        {
            Level1OutdoorLivesController existing = FindAnyObjectByType<Level1OutdoorLivesController>();
            if (existing != null)
            {
                existing.ResetForOutdoor();
                return existing;
            }

            GameObject host = new GameObject("Level1OutdoorLives");
            return host.AddComponent<Level1OutdoorLivesController>();
        }

        private void Awake()
        {
            Instance = this;
            EnsureHud();
            ResetForOutdoor();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ResetForOutdoor()
        {
            lives = StartingLives;
            hitCooldown = 0f;
            isGameOver = false;
            onRoad = false;
            player = FindAnyObjectByType<PlayerTopDownController>();
            traffic = FindAnyObjectByType<TrafficManager>();
            EnsureHud();
            RefreshLivesHud();
            SetWarningVisible(false);
        }

        private void Update()
        {
            if (isGameOver)
            {
                return;
            }

            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null || phase.CurrentPhase != Level1PhaseController.Level1Phase.OutdoorStreet)
            {
                SetWarningVisible(false);
                return;
            }

            if (player == null)
            {
                player = FindAnyObjectByType<PlayerTopDownController>();
            }

            if (player == null || !player.gameObject.activeInHierarchy)
            {
                return;
            }

            hitCooldown = Mathf.Max(0f, hitCooldown - Time.deltaTime);
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f)
            {
                warningTimer = WarningRefreshSeconds;
                UpdateRoadWarning(player.transform.position);
            }

            if (hitCooldown <= 0f)
            {
                TryDetectVehicleHit(player);
            }
        }

        private void UpdateRoadWarning(Vector3 playerPos)
        {
            onRoad = IsOnHighwayLanes(playerPos);
            if (onRoad)
            {
                SetWarningVisible(true);
                if (warningText != null)
                {
                    warningText.text = "Cẩn thận! Đừng xuống lòng đường — xe chạy sẽ tông mất mạng.";
                }
            }
            else
            {
                SetWarningVisible(false);
            }
        }

        public static bool IsOnHighwayLanes(Vector3 world)
        {
            float minZ = Level1BusStopRoadSettings.LaneNorthEdgeZ - 0.15f;
            float maxZ = Level1BusStopRoadSettings.SidewalkNearZ - 0.05f;
            float halfX = Level1BusStopRoadSettings.PlayableHalfX + 2.2f;
            return world.z >= minZ && world.z <= maxZ
                   && Mathf.Abs(world.x) <= halfX;
        }

        private void TryDetectVehicleHit(PlayerTopDownController target)
        {
            if (traffic == null)
            {
                traffic = FindAnyObjectByType<TrafficManager>();
            }

            if (traffic == null)
            {
                return;
            }

            Vector3 playerPos = target.transform.position;
            int count = traffic.ActiveVehicleCount;
            for (int i = 0; i < count; i++)
            {
                VehicleTraffic vehicle = traffic.GetActiveVehicle(i);
                if (vehicle == null || !vehicle.CanHitPedestrian)
                {
                    continue;
                }

                Vector3 delta = playerPos - vehicle.transform.position;
                delta.y = 0f;
                float hitRange = HitRadius * Mathf.Max(1f, vehicle.LengthMultiplier);
                if (delta.sqrMagnitude > hitRange * hitRange)
                {
                    continue;
                }

                RegisterCarHit(vehicle, target);
                return;
            }
        }

        public void RegisterCarHit(VehicleTraffic vehicle, PlayerTopDownController target)
        {
            if (isGameOver || hitCooldown > 0f || target == null)
            {
                return;
            }

            hitCooldown = HitCooldownSeconds;
            lives = Mathf.Max(0, lives - 1);
            RefreshLivesHud();

            Vector3 push = target.transform.position - (vehicle != null ? vehicle.transform.position : target.transform.position);
            // Prefer knockback back onto the sidewalk (+Z).
            if (Mathf.Abs(push.z) < 0.2f)
            {
                push.z = 1f;
            }

            if (push.z < 0f)
            {
                push.z = Mathf.Abs(push.z) + 0.5f;
            }

            target.ApplyTrafficKnockback(push, 1.35f);

            if (lives <= 0)
            {
                StartCoroutine(GameOverRoutine());
            }
            else
            {
                SetWarningVisible(true);
                if (warningText != null)
                {
                    warningText.text = "Xe tông! Còn " + lives + " mạng. Tránh lòng đường.";
                }
            }
        }

        private IEnumerator GameOverRoutine()
        {
            isGameOver = true;
            if (player != null)
            {
                player.MovementLocked = true;
            }

            SetWarningVisible(true);
            if (warningText != null)
            {
                warningText.text = "Hết mạng! Bắt đầu lại từ nhà trọ…";
            }

            yield return new WaitForSecondsRealtime(1.4f);

            string sceneName = gameObject.scene.name;
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = "Level1_PackingRoom";
            }

            SceneManager.LoadScene(sceneName);
        }

        private void EnsureHud()
        {
            if (livesText != null)
            {
                return;
            }

            GameObject root = new GameObject("Level1OutdoorLivesHUD");
            root.transform.SetParent(transform, false);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 95;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            livesText = MainMenuTypography.CreateStyledText("LivesText", root.transform,
                MenuTextStyle.DialogBody, TextAnchor.UpperLeft, "Mạng: 3 / 3");
            livesText.raycastTarget = false;
            RectTransform livesRect = livesText.rectTransform;
            livesRect.anchorMin = new Vector2(0f, 1f);
            livesRect.anchorMax = new Vector2(0f, 1f);
            livesRect.pivot = new Vector2(0f, 1f);
            livesRect.anchoredPosition = new Vector2(28f, -110f);
            livesRect.sizeDelta = new Vector2(280f, 40f);

            GameObject warnPanel = new GameObject("RoadWarning", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            warnPanel.transform.SetParent(root.transform, false);
            warningGroup = warnPanel.GetComponent<CanvasGroup>();
            warningGroup.blocksRaycasts = false;
            Image warnBg = warnPanel.GetComponent<Image>();
            warnBg.color = new Color(0.18f, 0.08f, 0.05f, 0.82f);
            warnBg.raycastTarget = false;
            RectTransform warnRect = warnPanel.GetComponent<RectTransform>();
            warnRect.anchorMin = new Vector2(0.5f, 0f);
            warnRect.anchorMax = new Vector2(0.5f, 0f);
            warnRect.pivot = new Vector2(0.5f, 0f);
            warnRect.anchoredPosition = new Vector2(0f, 110f);
            warnRect.sizeDelta = new Vector2(820f, 64f);

            warningText = MainMenuTypography.CreateStyledText("RoadWarningText", warnPanel.transform,
                MenuTextStyle.DialogBody, TextAnchor.MiddleCenter, string.Empty);
            warningText.raycastTarget = false;
            RectTransform warnTextRect = warningText.rectTransform;
            warnTextRect.anchorMin = Vector2.zero;
            warnTextRect.anchorMax = Vector2.one;
            warnTextRect.offsetMin = new Vector2(16f, 8f);
            warnTextRect.offsetMax = new Vector2(-16f, -8f);

            SetWarningVisible(false);
            RefreshLivesHud();
        }

        private void RefreshLivesHud()
        {
            if (livesText != null)
            {
                livesText.text = "Mạng: " + lives + " / " + StartingLives;
            }
        }

        private void SetWarningVisible(bool visible)
        {
            if (warningGroup != null)
            {
                warningGroup.alpha = visible ? 1f : 0f;
                warningGroup.gameObject.SetActive(visible);
            }
        }
    }
}
