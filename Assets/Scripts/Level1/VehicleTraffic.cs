using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Per-prefab visual alignment for Level 1 traffic vehicles.</summary>
    [System.Serializable]
    public struct Level1TrafficVehicleProfile
    {
        public GameObject prefab;
        [Tooltip("Extra Y rotation on the Visual wrapper to align model forward with lane travel.")]
        public float modelRotationOffsetY;
        [Tooltip("Local euler applied to the prefab root (Kenney Car Kit uses 0, 0, 0).")]
        public Vector3 visualLocalEulerOffset;
        public float scale;
        public bool enableWheelSpin;
        public Vector3 wheelSpinAxis;
        public bool flattenVisualTilt;
        public Level1TrafficVehicleVisualKind visualKind;
        public int bodyColorIndex;
        public float lengthMultiplier;
    }

    public enum Level1TrafficVehicleVisualKind
    {
        CarA,
        CarB,
        PickupCar
    }

    /// <summary>Transform-only traffic vehicle with lane movement and bus-stop pickup.</summary>
    public class VehicleTraffic : MonoBehaviour
    {
        public enum VehicleState
        {
            Driving,
            SlowingDown,
            PullingOver,
            Stopped,
            Leaving
        }

        [Header("Runtime (Inspector read-only)")]
        [SerializeField] private VehicleTrafficDirection direction;
        [SerializeField] private VehicleState state = VehicleState.Driving;
        [SerializeField] private bool isPickupVehicle;
        [SerializeField] private bool isEncoreRide;
        [SerializeField] private VehicleBoardingOutcome boardingOutcome;
        [SerializeField] private int candidateBodyColorIndex = -1;
        [SerializeField] private int candidatePrefabIndex = -1;
        [SerializeField] private string licensePlate;
        [SerializeField] private float currentSpeed;
        [SerializeField] private VehiclePickupController.PickupPhase pickupPhase;

        [Header("Debug")]
        [SerializeField] private bool drawPickupPathGizmos = true;

        private TrafficManager manager;
        private TrafficPool pool;
        private VehicleAnimator visual;
        private BusStopPickup pickupInteraction;
        private SphereCollider pickupTrigger;
        private readonly VehiclePickupController pickup = new VehiclePickupController();

        private float cruiseSpeed;
        private float acceleration;
        private float deceleration;
        private float despawnMinX;
        private float despawnMaxX;
        private float laneCenterZ;
        private float travelYaw;
        private float lengthMultiplier = 1f;
        private Level1TrafficVehicleProfile activeProfile;

        private static readonly float RoadY = Level1BusStopRoadSettings.BusRootY;

        private void Awake()
        {
            pickup.EnsureHost(transform);
        }

        private void OnEnable()
        {
            pickup.EnsureHost(transform);
        }

        public bool IsWaitingForPickup =>
            isPickupVehicle && pickup.Phase == VehiclePickupController.PickupPhase.Parked;
        public VehicleTrafficDirection Direction => direction;
        public bool IsPickupVehicle => isPickupVehicle;
        public bool IsEncoreRide => isEncoreRide;
        public VehicleBoardingOutcome BoardingOutcome => boardingOutcome;
        public float CurrentSpeed => currentSpeed;
        public float PositionX => transform.position.x;
        public float LengthMultiplier => lengthMultiplier;
        public VehicleState State => state;
        public Level1TrafficVehicleProfile CurrentProfile => activeProfile;
        public string LicensePlate => licensePlate;
        public int CandidatePrefabIndex => candidatePrefabIndex;

        /// <summary>Driving traffic can injure pedestrians who step onto the highway.</summary>
        public bool CanHitPedestrian =>
            isActiveAndEnabled
            && (state == VehicleState.Driving
                || state == VehicleState.Leaving
                || state == VehicleState.SlowingDown)
            && currentSpeed >= 1.25f
            && !(isPickupVehicle && state == VehicleState.Stopped);

        public void ConfigureComponents(VehicleAnimator vehicleVisual, BusStopPickup pickupComp, SphereCollider trigger)
        {
            visual = vehicleVisual;
            pickupInteraction = pickupComp;
            pickupTrigger = trigger;
            pickup.Bind(transform, manager, visual, pickupInteraction);
        }

        public void Initialize(
            TrafficManager owner,
            TrafficPool trafficPool,
            VehicleTrafficDirection travelDirection,
            Level1TrafficVehicleProfile profile,
            bool isPickup,
            VehicleBoardingOutcome outcome,
            bool encoreRide,
            float speed,
            float accel,
            float decel,
            float beyondRoadDespawnMaxX,
            int pickupPrefabIndex = -1,
            float beyondRoadDespawnMinX = float.NaN)
        {
            manager = owner;
            pool = trafficPool;
            direction = travelDirection;
            cruiseSpeed = speed;
            acceleration = accel;
            deceleration = decel;
            isPickupVehicle = isPickup;
            isEncoreRide = encoreRide;
            boardingOutcome = isPickup ? outcome : VehicleBoardingOutcome.None;
            despawnMaxX = beyondRoadDespawnMaxX;
            despawnMinX = float.IsNaN(beyondRoadDespawnMinX)
                ? -beyondRoadDespawnMaxX
                : beyondRoadDespawnMinX;
            state = VehicleState.Driving;
            currentSpeed = speed * 0.7f;
            licensePlate = boardingOutcome == VehicleBoardingOutcome.HomeRide
                ? VehicleBookingSession.LicensePlate
                : VehicleBookingSession.GenerateLicensePlate();
            candidateBodyColorIndex = isPickup ? profile.bodyColorIndex : -1;
            candidatePrefabIndex = isPickup ? pickupPrefabIndex : -1;

            laneCenterZ = Level1BusStopRoadSettings.LaneCenterZForDirection(direction);
            travelYaw = DirectionYaw(direction);
            lengthMultiplier = profile.lengthMultiplier > 0.01f ? profile.lengthMultiplier : 1f;
            activeProfile = profile;

            pickup.Bind(transform, manager, visual, pickupInteraction);
            pickup.Reset();
            pickup.ConfigureLane(laneCenterZ, travelYaw, cruiseSpeed, acceleration, deceleration, direction);

            DisablePhysicsOnRoot();

            if (visual != null)
            {
                visual.BindPool(pool);
                visual.AttachVisual(profile.prefab, profile.scale, profile);
            }

            pickupInteraction?.SetWaiting(false);
            visual?.SetTurnSignal(VehicleTurnSignalSide.None);
            if (pickupTrigger != null)
            {
                pickupTrigger.enabled = isPickupVehicle;
            }

            ApplyFlatPose(transform.position.x, laneCenterZ, travelYaw);
        }

        public void PrepareForPool()
        {
            CancelPickupReservation();
            pickup.Reset();
            state = VehicleState.Driving;
            pickupPhase = VehiclePickupController.PickupPhase.Inactive;
            isPickupVehicle = false;
            isEncoreRide = false;
            boardingOutcome = VehicleBoardingOutcome.None;
            currentSpeed = 0f;
            lengthMultiplier = 1f;
            candidatePrefabIndex = -1;
            licensePlate = string.Empty;

            if (pickupTrigger != null)
            {
                pickupTrigger.enabled = false;
            }

            pickupInteraction?.SetWaiting(false);
            visual?.ResetForPool();
        }

        private void LateUpdate()
        {
            if (!pickup.IsParked)
            {
                EnforceFlatTransform();
            }
            else
            {
                Vector3 position = transform.position;
                transform.position = new Vector3(position.x, RoadY, position.z);
            }

            visual?.SetWheelSpeed(currentSpeed);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (isPickupVehicle && direction == VehicleTrafficDirection.LeftToRight)
            {
                UpdatePickupVehicle(deltaTime);
                pickupPhase = pickup.Phase;
                return;
            }

            state = VehicleState.Driving;
            UpdateDriving(deltaTime);
        }

        private void UpdatePickupVehicle(float deltaTime)
        {
            pickup.EnsureHost(transform);
            if (!pickup.HasValidHost)
            {
                isPickupVehicle = false;
                UpdateDriving(deltaTime);
                return;
            }

            if (pickup.Phase == VehiclePickupController.PickupPhase.Inactive)
            {
                if (pickup.TryTickDrivingApproach(deltaTime, ref currentSpeed, ClampProposedX))
                {
                    state = pickup.MappedTrafficState;
                }
                else
                {
                    UpdateDriving(deltaTime);
                    return;
                }
            }

            if (pickup.Phase != VehiclePickupController.PickupPhase.Inactive)
            {
                pickup.Tick(deltaTime, ref currentSpeed, ClampProposedX);
                state = pickup.MappedTrafficState;

                if (pickup.Phase == VehiclePickupController.PickupPhase.MergingOnRoad)
                {
                    float nextX = transform.position.x;
                    if (HasPassedDespawnEdge(nextX))
                    {
                        manager?.ReleaseVehicle(this);
                    }
                }
            }
        }

        private void UpdateDriving(float deltaTime)
        {
            float desiredSpeed = manager != null
                ? manager.GetConstrainedSpeed(this, cruiseSpeed)
                : cruiseSpeed;
            SmoothSpeedToward(desiredSpeed, deltaTime);

            float moveDir = direction == VehicleTrafficDirection.LeftToRight ? 1f : -1f;
            float nextX = transform.position.x + moveDir * currentSpeed * deltaTime;
            nextX = ClampProposedX(nextX);

            ApplyFlatPose(nextX, laneCenterZ, travelYaw);

            if (HasPassedDespawnEdge(nextX))
            {
                manager?.ReleaseVehicle(this);
            }
        }

        public void DepartAfterBoarding()
        {
            if (!isPickupVehicle || pickup.Phase != VehiclePickupController.PickupPhase.Parked)
            {
                return;
            }

            pickup.BeginDeparture();
            state = VehicleState.Leaving;
            pickupPhase = pickup.Phase;
        }

        public void DepartAfterReject()
        {
            if (!isPickupVehicle)
            {
                return;
            }

            if (pickup.Phase == VehiclePickupController.PickupPhase.Parked)
            {
                DepartAfterBoarding();
                return;
            }

            pickup.ForceDepartFromReview();
            state = VehicleState.Leaving;
            pickupPhase = pickup.Phase;
        }

        public void AbandonPickupCandidate()
        {
            if (!isPickupVehicle || isEncoreRide)
            {
                return;
            }

            BusStopChallengeSession.ReturnCandidate(new BusStopCandidateSlot
            {
                Outcome = boardingOutcome,
                BodyColorIndex = candidateBodyColorIndex,
                PrefabIndex = candidatePrefabIndex
            });

            isPickupVehicle = false;
            boardingOutcome = VehicleBoardingOutcome.None;
            candidatePrefabIndex = -1;
            candidateBodyColorIndex = -1;
            pickup.CancelReservation();
            pickupInteraction?.SetWaiting(false);
            if (pickupTrigger != null)
            {
                pickupTrigger.enabled = false;
            }
        }

        public void BeginEncoreInstantPark()
        {
            if (!isPickupVehicle || !isEncoreRide)
            {
                return;
            }

            pickup.ForceEncorePark(ref currentSpeed);
            state = VehicleState.Stopped;
            pickupPhase = pickup.Phase;

            if (pickup.IsParked)
            {
                BusStopChallengeSession.MarkEncoreSpawned();
            }
        }

        public void CancelPickupReservation()
        {
            pickup.CancelReservation();
        }

        private float ClampProposedX(float nextX)
        {
            return manager != null ? manager.ClampProposedPositionX(this, nextX) : nextX;
        }

        private void SmoothSpeedToward(float target, float deltaTime)
        {
            float rate = target > currentSpeed ? acceleration : deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, target, rate * deltaTime);
        }

        private void ApplyFlatPose(float x, float z, float yaw)
        {
            transform.SetPositionAndRotation(
                new Vector3(x, RoadY, z),
                Quaternion.Euler(0f, yaw, 0f));
        }

        private void EnforceFlatTransform()
        {
            Vector3 position = transform.position;
            transform.position = new Vector3(position.x, RoadY, position.z);

            float yaw = transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void DisablePhysicsOnRoot()
        {
            Rigidbody body = GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = true;
                body.useGravity = false;
                body.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        private void OnDestroy()
        {
            CancelPickupReservation();
        }

        private bool HasPassedDespawnEdge(float x)
        {
            return direction == VehicleTrafficDirection.LeftToRight
                ? x > despawnMaxX
                : x < despawnMinX;
        }

        private static float DirectionYaw(VehicleTrafficDirection travelDirection)
        {
            return travelDirection == VehicleTrafficDirection.LeftToRight ? 90f : -90f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isPickupVehicle ? Color.yellow : Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.35f);

            Vector3 moveDir = direction == VehicleTrafficDirection.LeftToRight ? Vector3.right : Vector3.left;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.4f, moveDir * 2f);

            if (drawPickupPathGizmos && isPickupVehicle)
            {
                VehiclePickupController.DrawDebugGizmos(
                    direction,
                    Level1PickupStopLayout.DefaultCurbOffset);
            }
        }
    }
}
