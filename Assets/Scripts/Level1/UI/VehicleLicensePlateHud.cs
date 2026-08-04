using ChoTet.Menu;
using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1.UI
{
    /// <summary>
    /// Shows a "check license plate" button when the player stands near a stopped pickup vehicle.
    /// </summary>
    public class VehicleLicensePlateHud : MonoBehaviour
    {
        [SerializeField] private float proximityRadius = 5.6f;
        [SerializeField] private Button checkPlateButton;
        [SerializeField] private VehicleLicensePlatePanelView platePanel;

        private VehicleTraffic trackedVehicle;
        private bool built;
        private bool wasPanelOpen;
        private InventoryUIController ownerUi;

        public bool IsPanelOpen => platePanel != null && platePanel.IsOpen;

        private void Update()
        {
            if (!built)
            {
                return;
            }

            VehicleTraffic nearby = FindNearbyStoppedPickup();
            if (nearby == null)
            {
                trackedVehicle = null;
                SetCheckButtonVisible(false);
                platePanel?.Hide();
                return;
            }

            trackedVehicle = nearby;
            SetCheckButtonVisible(!IsPanelOpen);

            bool panelOpen = IsPanelOpen;
            if (panelOpen != wasPanelOpen)
            {
                wasPanelOpen = panelOpen;
                ownerUi?.RefreshGameplayPrompts();
            }
        }

        public void EnsureBuilt(Transform uiRoot)
        {
            if (built || uiRoot == null)
            {
                return;
            }

            ownerUi = GetComponentInParent<InventoryUIController>();

            KenneyRpgUiLibrary library = KenneyRpgUiLibrary.LoadDefault();

            if (checkPlateButton == null)
            {
                checkPlateButton = KenneyRpgUiWidgets.CreateMenuButton("CheckLicensePlateButton", uiRoot, library,
                    "Xem biển số xe", OnCheckPlateClicked);
                RectTransform buttonRect = checkPlateButton.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0f);
                buttonRect.anchorMax = new Vector2(0.5f, 0f);
                buttonRect.pivot = new Vector2(0.5f, 0f);
                buttonRect.anchoredPosition = new Vector2(0f, 112f);
                buttonRect.sizeDelta = new Vector2(280f, 56f);
                checkPlateButton.GetComponent<Image>().type = Image.Type.Sliced;

                Text label = checkPlateButton.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.fontSize = 22;
                }
            }
            else
            {
                WireCheckPlateButton();
            }

            if (platePanel == null)
            {
                GameObject panelHost = new GameObject("VehicleLicensePlateHud", typeof(RectTransform), typeof(VehicleLicensePlatePanelView));
                panelHost.transform.SetParent(uiRoot, false);
                Stretch(panelHost.GetComponent<RectTransform>());
                platePanel = panelHost.GetComponent<VehicleLicensePlatePanelView>();
                platePanel.EnsureBuilt(panelHost.transform);
            }

            platePanel.OnGoClicked -= OnGoClicked;
            platePanel.OnRejectClicked -= OnRejectClicked;
            platePanel.OnGoClicked += OnGoClicked;
            platePanel.OnRejectClicked += OnRejectClicked;

            SetCheckButtonVisible(false);
            built = true;
            WireCheckPlateButton();
        }

        private void OnEnable()
        {
            if (built)
            {
                WireCheckPlateButton();
            }
        }

        private void WireCheckPlateButton()
        {
            if (checkPlateButton == null)
            {
                return;
            }

            checkPlateButton.onClick.RemoveListener(OnCheckPlateClicked);
            checkPlateButton.onClick.AddListener(OnCheckPlateClicked);
            checkPlateButton.interactable = true;
        }

        private void OnCheckPlateClicked()
        {
            if (trackedVehicle == null || platePanel == null)
            {
                return;
            }

            platePanel.Show(trackedVehicle.LicensePlate);
            SetCheckButtonVisible(false);
            ownerUi?.RefreshGameplayPrompts();
        }

        private void OnGoClicked()
        {
            if (trackedVehicle == null || platePanel == null)
            {
                return;
            }

            VehicleTraffic vehicleToBoard = trackedVehicle;
            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            platePanel.Hide();
            ownerUi?.RefreshGameplayPrompts();

            if (phase == null || player == null)
            {
                Debug.LogWarning("[VehicleLicensePlateHud] Cannot board — missing phase controller or player.");
                return;
            }

            phase.BeginBoardingTransition(player, vehicleToBoard);
        }

        private void OnRejectClicked()
        {
            if (trackedVehicle == null || platePanel == null)
            {
                return;
            }

            BusStopChallengeSession.RecordRejection();
            if (trackedVehicle.BoardingOutcome == VehicleBoardingOutcome.HomeRide)
            {
                BusStopChallengeSession.RememberHomeRideAppearance(new BusStopCandidateSlot
                {
                    Outcome = VehicleBoardingOutcome.HomeRide,
                    BodyColorIndex = trackedVehicle.CurrentProfile.bodyColorIndex,
                    PrefabIndex = trackedVehicle.CandidatePrefabIndex
                });
            }

            bool encoreQueued = BusStopChallengeSession.EncorePending;
            TrafficManager traffic = FindAnyObjectByType<TrafficManager>();
            trackedVehicle.DepartAfterReject();

            if (encoreQueued)
            {
                traffic?.RegisterEncoreDepartGate(trackedVehicle);
                traffic?.RequestEncoreHomeRide();
            }
            else
            {
                traffic?.NotifyPickupRejected();
            }

            platePanel.Hide();
            SetCheckButtonVisible(false);
            ownerUi?.RefreshGameplayPrompts();
        }

        public void DismissPanel()
        {
            if (platePanel != null)
            {
                platePanel.Hide();
                ownerUi?.RefreshGameplayPrompts();
            }
        }

        private VehicleTraffic FindNearbyStoppedPickup()
        {
            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            if (phase == null
                || phase.CurrentPhase != Level1PhaseController.Level1Phase.OutdoorStreet
                || phase.IsTransitioning)
            {
                return null;
            }

            TrafficManager traffic = FindAnyObjectByType<TrafficManager>();
            VehicleTraffic waiting = traffic != null ? traffic.PickupWaitingVehicle : null;
            if (waiting == null || !waiting.IsWaitingForPickup || waiting.IsEncoreRide)
            {
                return null;
            }

            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            if (player == null)
            {
                return null;
            }

            float distance = Vector3.Distance(player.transform.position, waiting.transform.position);
            return distance <= proximityRadius ? waiting : null;
        }

        private void SetCheckButtonVisible(bool visible)
        {
            if (checkPlateButton != null)
            {
                checkPlateButton.gameObject.SetActive(visible);
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
