using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChoTet.Level1
{
    /// <summary>Spawns and manages looping two-lane traffic for Level 1 Phase 2.</summary>
    public class TrafficManager : MonoBehaviour
    {
        [Header("Vehicle Prefabs (Kenney Car Kit)")]
        [SerializeField] private GameObject[] trafficVehiclePrefabs;
        [SerializeField] private GameObject carPrefabA;
        [SerializeField] private GameObject carPrefabB;

        [Header("Per-Car Alignment")]
        [SerializeField] private float carAModelRotationOffsetY;
        [SerializeField] private float carBModelRotationOffsetY;
        [SerializeField] private float pickupModelRotationOffsetY;
        [FormerlySerializedAs("brokenVectorCarLocalEuler")]
        [SerializeField] private Vector3 carVisualLocalEuler = Vector3.zero;
        [SerializeField] private float carVisualScale = 1.05f;
        [SerializeField] private float carBVisualScale = 1.05f;
        [SerializeField] private float pickupCarVisualScale = 1.55f;
        [SerializeField] private float carScaleVariance = 0.03f;

        [Header("Car Materials (URP)")]
        [SerializeField] private Material[] trafficBodyColors;
        [SerializeField] private Material pickupCarBodyMaterial;
        [SerializeField] private Material wheelMaterial;
        [SerializeField] private Material glassMaterial;
        [SerializeField] private Material kenneyColormapMaterial;

        [Header("Spawn")]
        [SerializeField] private float spawnInterval = 2.35f;
        [SerializeField] private float candidateSpawnInterval = 1.1f;
        [SerializeField] private float candidateRespawnDelay = 0.35f;
        [SerializeField] private float candidateRejectRespawnDelay = 10f;
        [SerializeField] private float candidateSpawnRetryWhenBlocked = 2f;
        [SerializeField] private float encoreRespawnDelay = 0.12f;
        [SerializeField] private float spawnOffsetBeyondRoad = 4.5f;
        [SerializeField] private int initialVehicleCount = 1;
        [SerializeField] private float initialSpawnDelay = 0.15f;

        [Header("Speed Variety")]
        [SerializeField] private float minSpeed = 5.2f;
        [SerializeField] private float maxSpeed = 8.1f;
        [SerializeField] private float minAcceleration = 3.6f;
        [SerializeField] private float maxAcceleration = 5.8f;
        [SerializeField] private float minDeceleration = 5.2f;
        [SerializeField] private float maxDeceleration = 7.4f;

        [Header("Following")]
        [SerializeField] private float vehicleLength = 1.75f;
        [SerializeField] private float minFollowDistance = 6.2f;
        [SerializeField] private float safeFollowDistance = 10.5f;
        [SerializeField] private float followGapBuffer = 2.4f;
        [SerializeField] private float followSpeedFactor = 0.78f;

        [Header("Pickup")]
        [Range(0f, 1f)]
        [SerializeField] private float pickupVehicleChance = 0.38f;
        [SerializeField] private float pullOverDuration = 6f;
        [SerializeField] private float pullOverSpeed = 0.5f;
        [SerializeField] private float pickupNormalSpeed = 5.2f;
        [SerializeField] private float pickupCurbSpeed = 3f;
        [SerializeField] private float curbOffset = 0.8f;
        [SerializeField] private float stopDuration = 45f;

        [Header("Debug")]
        [SerializeField] private bool drawLaneGizmos = true;

        private readonly List<VehicleTraffic> activeVehicles = new List<VehicleTraffic>();
        private TrafficPool trafficPool;
        private VehicleTraffic pickupWaitingVehicle;
        private VehicleTraffic pickupSlotHolder;
        private float spawnTimer;
        private float despawnMaxX;
        private float despawnMinX;
        private float roadStartX;
        private float roadEndX;
        private float encoreRetryTimer;
        private Coroutine deferredSpawnRoutine;
        private Coroutine encoreSpawnRoutine;
        private Coroutine candidateRejectSpawnRoutine;
        private bool outdoorSpawnReady;
        private VehicleTraffic encoreDepartGateVehicle;
        private float encoreGateFallbackTimer;
        private float emptyRoadRetryTimer;
        private float outdoorReadyElapsed;
        private float candidateSpawnTimer;

        private const float EncoreDepartClearDistance = 3.5f;
        private const float EncoreGateFallbackSeconds = 2.75f;

        public float PullOverDuration => pullOverDuration;
        public float PullOverSpeed => pullOverSpeed;
        public float PickupNormalSpeed => pickupNormalSpeed;
        public float PickupCurbSpeed => pickupCurbSpeed;
        public float CurbOffset => curbOffset;
        public float StopDuration => stopDuration;
        public VehicleTraffic PickupWaitingVehicle => pickupWaitingVehicle;

        public int ActiveVehicleCount => activeVehicles.Count;

        public VehicleTraffic GetActiveVehicle(int index)
        {
            if (index < 0 || index >= activeVehicles.Count)
            {
                return null;
            }

            return activeVehicles[index];
        }

        public int GetBodyColorCount()
        {
            return trafficBodyColors != null ? trafficBodyColors.Length : 0;
        }

        public int GetBusStopCandidatePrefabCount()
        {
            return Mathf.Max(1, GetBusStopCandidatePrefabs().Length);
        }

        /// <summary>Called after the indoor→outdoor fade finishes so traffic does not spike the same frame.</summary>
        public void NotifyOutdoorPhaseReady()
        {
            outdoorSpawnReady = true;
            outdoorReadyElapsed = 0f;
            spawnTimer = Mathf.Min(spawnTimer, 0.15f);
            emptyRoadRetryTimer = 0.1f;
            RefreshRoadExtents();
            EnsureChallengeSessionForOutdoorPhase();
            SanitizeStuckPickupState();
            PruneStaleActiveVehicles();
            ReconcilePoolWhenIdle();

            if (activeVehicles.Count == 0)
            {
                ForceBootstrapTraffic();
            }
        }

        private void SanitizeStuckPickupState()
        {
            if (pickupWaitingVehicle != null
                && (!pickupWaitingVehicle.gameObject.activeInHierarchy
                    || !pickupWaitingVehicle.IsPickupVehicle))
            {
                pickupWaitingVehicle = null;
            }

            if (pickupSlotHolder != null
                && (!pickupSlotHolder.gameObject.activeInHierarchy
                    || !pickupSlotHolder.IsPickupVehicle))
            {
                pickupSlotHolder = null;
            }
        }

        private void PruneStaleActiveVehicles()
        {
            for (int i = activeVehicles.Count - 1; i >= 0; i--)
            {
                VehicleTraffic vehicle = activeVehicles[i];
                if (vehicle == null || !vehicle.gameObject.activeInHierarchy)
                {
                    activeVehicles.RemoveAt(i);
                }
            }
        }

        private void ReconcilePoolWhenIdle()
        {
            if (trafficPool == null || activeVehicles.Count > 0)
            {
                return;
            }

            trafficPool.ReconcileAfterManagerEnable();
        }

        private void ForceBootstrapTraffic()
        {
            if (!isActiveAndEnabled || trafficPool == null)
            {
                return;
            }

            float minGap = GetSpawnStaggerGap();
            SpawnVehicle(
                VehicleTrafficDirection.LeftToRight,
                fromLeft: true,
                allowPickup: false,
                spawnIndex: 0,
                minGap: minGap);

            SpawnVehicle(
                VehicleTrafficDirection.RightToLeft,
                fromLeft: false,
                allowPickup: false,
                spawnIndex: 1,
                minGap: minGap);

            TrySpawnNextBusStopCandidate();
        }

        private void TryAutoEnableOutdoorSpawn()
        {
            if (outdoorSpawnReady)
            {
                return;
            }

            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            if (phase != null
                && phase.CurrentPhase == Level1PhaseController.Level1Phase.OutdoorStreet
                && !phase.IsTransitioning)
            {
                NotifyOutdoorPhaseReady();
                return;
            }

            if (IsOutdoorHierarchyActive())
            {
                NotifyOutdoorPhaseReady();
            }
        }

        private static bool IsOutdoorHierarchyActive()
        {
            GameObject outdoor = GameObject.Find("Level1_OutdoorArea");
            return outdoor != null && outdoor.activeInHierarchy;
        }

        private void EnsureChallengeSessionForOutdoorPhase()
        {
            if (BusStopChallengeSession.IsActive)
            {
                return;
            }

            BusStopChallengeSession.BeginOutdoorChallenge(
                Mathf.Max(1, GetBodyColorCount()),
                GetBusStopCandidatePrefabCount());
        }

        private void Awake()
        {
            trafficPool = GetComponent<TrafficPool>();
            if (trafficPool == null)
            {
                trafficPool = gameObject.AddComponent<TrafficPool>();
            }

            ConfigureMaterials();
        }

        private void ConfigureMaterials()
        {
            Level1TrafficVisualUtility.Configure(
                trafficBodyColors,
                pickupCarBodyMaterial,
                wheelMaterial,
                glassMaterial,
                kenneyColormapMaterial);
        }

        private void RefreshRoadExtents()
        {
            Level1BusStopRoadExtents.RefreshFromScene();
            roadStartX = Level1BusStopRoadExtents.RoadStartX;
            roadEndX = Level1BusStopRoadExtents.RoadEndX;
            despawnMaxX = roadEndX + spawnOffsetBeyondRoad;
            despawnMinX = roadStartX - spawnOffsetBeyondRoad;
        }

        private void OnEnable()
        {
            ConfigureMaterials();
            Level1TrafficVisualBootstrap.EnsureConfigured();
            RefreshRoadExtents();
            spawnTimer = 0.5f;
            candidateSpawnTimer = candidateSpawnInterval;
            outdoorSpawnReady = false;
            outdoorReadyElapsed = 0f;
            emptyRoadRetryTimer = 0.1f;
            EnsureChallengeSessionForOutdoorPhase();

            if (trafficPool == null)
            {
                trafficPool = GetComponent<TrafficPool>();
            }

            if (deferredSpawnRoutine != null)
            {
                StopCoroutine(deferredSpawnRoutine);
            }

            deferredSpawnRoutine = StartCoroutine(SpawnInitialTrafficWhenReady());
        }

        private IEnumerator SpawnInitialTrafficWhenReady()
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            const float maxWaitSeconds = 12f;
            float waitDeadline = Time.unscaledTime + maxWaitSeconds;

            while (!outdoorSpawnReady)
            {
                TryAutoEnableOutdoorSpawn();

                Level1PhaseController phase = Level1PhaseController.Instance;
                if (phase == null)
                {
                    phase = FindAnyObjectByType<Level1PhaseController>();
                }

                if (phase != null
                    && phase.CurrentPhase == Level1PhaseController.Level1Phase.OutdoorStreet
                    && !phase.IsTransitioning)
                {
                    outdoorSpawnReady = true;
                    break;
                }

                if (Time.unscaledTime >= waitDeadline)
                {
                    outdoorSpawnReady = true;
                    break;
                }

                yield return null;
            }

            if (initialSpawnDelay > 0f)
            {
                yield return new WaitForSeconds(initialSpawnDelay);
            }

            PruneStaleActiveVehicles();
            ReconcilePoolWhenIdle();

            if (activeVehicles.Count == 0 && trafficPool != null && isActiveAndEnabled)
            {
                if (activeVehicles.Count == 0)
                {
                    ForceBootstrapTraffic();
                }
            }

            deferredSpawnRoutine = null;
        }

        private void OnDisable()
        {
            outdoorSpawnReady = false;

            if (deferredSpawnRoutine != null)
            {
                StopCoroutine(deferredSpawnRoutine);
                deferredSpawnRoutine = null;
            }

            if (encoreSpawnRoutine != null)
            {
                StopCoroutine(encoreSpawnRoutine);
                encoreSpawnRoutine = null;
            }

            if (candidateRejectSpawnRoutine != null)
            {
                StopCoroutine(candidateRejectSpawnRoutine);
                candidateRejectSpawnRoutine = null;
            }

            // Do not reparent pooled objects here — Unity forbids hierarchy changes while this object deactivates.
            for (int i = activeVehicles.Count - 1; i >= 0; i--)
            {
                VehicleTraffic vehicle = activeVehicles[i];
                if (vehicle != null)
                {
                    vehicle.PrepareForPool();
                }
            }

            activeVehicles.Clear();
            pickupWaitingVehicle = null;
            pickupSlotHolder = null;
            trafficPool?.ForceResetLeases();
        }

        private void Update()
        {
            TryAutoEnableOutdoorSpawn();

            if (!outdoorSpawnReady)
            {
                return;
            }

            PruneStaleActiveVehicles();

            outdoorReadyElapsed += Time.deltaTime;
            if (activeVehicles.Count == 0 && outdoorReadyElapsed > 0.35f)
            {
                emptyRoadRetryTimer -= Time.deltaTime;
                if (emptyRoadRetryTimer <= 0f)
                {
                    emptyRoadRetryTimer = 0.45f;
                    TrySpawnWhenRoadEmpty();
                }
            }

            if (BusStopChallengeSession.EncorePending
                && pickupWaitingVehicle == null
                && pickupSlotHolder == null)
            {
                TickEncoreDepartGateFallback();
                encoreRetryTimer -= Time.deltaTime;
                if (encoreRetryTimer <= 0f)
                {
                    encoreRetryTimer = 0.2f;
                    TrySpawnEncoreHomeVehicle();
                }
            }

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = GetAmbientSpawnInterval();
                SpawnAmbientVehicle();
            }

            TryCandidateSpawnWatchdog();
        }

        private float GetAmbientSpawnInterval()
        {
            return spawnInterval;
        }

        /// <summary>Ensures a waiting pickup eventually arrives when candidates remain.</summary>
        private void TryCandidateSpawnWatchdog()
        {
            if (candidateRejectSpawnRoutine != null
                || !BusStopChallengeSession.HasRemainingCandidates
                || pickupWaitingVehicle != null
                || pickupSlotHolder != null
                || HasActiveBusStopPickupApproach())
            {
                return;
            }

            candidateSpawnTimer -= Time.deltaTime;
            if (candidateSpawnTimer > 0f)
            {
                return;
            }

            candidateSpawnTimer = candidateSpawnInterval;
            if (!TrySpawnNextBusStopCandidate())
            {
                candidateSpawnTimer = candidateSpawnRetryWhenBlocked;
            }
        }

        private bool HasActiveBusStopPickupApproach()
        {
            for (int i = 0; i < activeVehicles.Count; i++)
            {
                VehicleTraffic vehicle = activeVehicles[i];
                if (vehicle != null
                    && vehicle.IsPickupVehicle
                    && vehicle.Direction == VehicleTrafficDirection.LeftToRight
                    && !vehicle.IsEncoreRide
                    && vehicle.State != VehicleTraffic.VehicleState.Driving)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TrySpawnNextBusStopCandidate()
        {
            ForceReleasePickupSlots();

            if (!BusStopChallengeSession.HasRemainingCandidates)
            {
                return false;
            }

            float tightGap = GetMinCenterGap() * 0.55f;
            return SpawnVehicle(
                VehicleTrafficDirection.LeftToRight,
                fromLeft: true,
                allowPickup: true,
                spawnIndex: 0,
                minGap: tightGap);
        }

        public void ForceReleasePickupSlots()
        {
            pickupWaitingVehicle = null;
            pickupSlotHolder = null;
        }

        /// <summary>Player rejected a parked pickup — next candidate arrives after a fixed delay.</summary>
        public void NotifyPickupRejected()
        {
            if (candidateRejectSpawnRoutine != null)
            {
                StopCoroutine(candidateRejectSpawnRoutine);
            }

            candidateSpawnTimer = candidateRejectRespawnDelay;
            candidateRejectSpawnRoutine = StartCoroutine(SpawnNextCandidateAfterRejectRoutine());
        }

        private IEnumerator SpawnNextCandidateAfterRejectRoutine()
        {
            yield return new WaitForSeconds(candidateRejectRespawnDelay);

            const float maxRetryWindow = 45f;
            float deadline = Time.time + maxRetryWindow;

            while (Time.time < deadline && BusStopChallengeSession.HasRemainingCandidates)
            {
                ForceReleasePickupSlots();

                if (pickupWaitingVehicle == null && pickupSlotHolder == null && TrySpawnNextBusStopCandidate())
                {
                    candidateRejectSpawnRoutine = null;
                    candidateSpawnTimer = candidateSpawnInterval;
                    yield break;
                }

                yield return new WaitForSeconds(candidateSpawnRetryWhenBlocked);
            }

            candidateRejectSpawnRoutine = null;
            candidateSpawnTimer = candidateSpawnRetryWhenBlocked;
        }

        private float GetSpawnInterval()
        {
            if (BusStopChallengeSession.EncorePending
                && pickupWaitingVehicle == null
                && pickupSlotHolder == null)
            {
                return encoreRespawnDelay;
            }

            if (BusStopChallengeSession.HasRemainingCandidates
                && pickupWaitingVehicle == null
                && pickupSlotHolder == null)
            {
                return candidateSpawnInterval;
            }

            return spawnInterval;
        }

        private void ScheduleNextCandidateSpawn()
        {
            if (candidateRejectSpawnRoutine != null)
            {
                return;
            }

            if (BusStopChallengeSession.EncorePending)
            {
                candidateSpawnTimer = Mathf.Min(candidateSpawnTimer, encoreRespawnDelay);
                return;
            }

            if (!BusStopChallengeSession.HasRemainingCandidates
                || pickupWaitingVehicle != null
                || pickupSlotHolder != null)
            {
                return;
            }

            candidateSpawnTimer = Mathf.Min(candidateSpawnTimer, candidateRespawnDelay);
        }

        public float GetConstrainedSpeed(VehicleTraffic vehicle, float desiredSpeed)
        {
            VehicleTraffic leader = FindLeader(vehicle);
            if (leader == null)
            {
                return desiredSpeed;
            }

            float gap = ComputeGap(vehicle, leader);
            float minGap = GetPairFollowGap(vehicle, leader);
            if (gap <= minGap)
            {
                return 0f;
            }

            if (gap <= safeFollowDistance)
            {
                float leaderSpeed = leader.CurrentSpeed;
                if (leader.State == VehicleTraffic.VehicleState.Stopped
                    || leader.State == VehicleTraffic.VehicleState.PullingOver)
                {
                    leaderSpeed = 0f;
                }

                return Mathf.Min(desiredSpeed, leaderSpeed * followSpeedFactor);
            }

            return desiredSpeed;
        }

        public float ClampProposedPositionX(VehicleTraffic vehicle, float proposedX)
        {
            VehicleTraffic leader = FindLeader(vehicle);
            if (leader == null)
            {
                return proposedX;
            }

            float minGap = GetPairFollowGap(vehicle, leader);
            if (vehicle.Direction == VehicleTrafficDirection.LeftToRight)
            {
                return Mathf.Min(proposedX, leader.PositionX - minGap);
            }

            return Mathf.Max(proposedX, leader.PositionX + minGap);
        }

        public float GetPairFollowGap(VehicleTraffic follower, VehicleTraffic leader)
        {
            float followerLen = vehicleLength * (follower != null ? follower.LengthMultiplier : 1f);
            float leaderLen = vehicleLength * (leader != null ? leader.LengthMultiplier : 1f);
            return Mathf.Max(minFollowDistance, followerLen * 0.5f + leaderLen * 0.5f + followGapBuffer);
        }

        private float GetSpawnStaggerGap()
        {
            return Mathf.Max(minFollowDistance, vehicleLength * 2.6f + followGapBuffer + 1.2f);
        }

        private float GetMinCenterGap()
        {
            return GetSpawnStaggerGap();
        }

        private VehicleTraffic FindLeader(VehicleTraffic vehicle)
        {
            VehicleTraffic leader = null;
            float bestGap = float.MaxValue;

            for (int i = 0; i < activeVehicles.Count; i++)
            {
                VehicleTraffic other = activeVehicles[i];
                if (other == null || other == vehicle || other.Direction != vehicle.Direction)
                {
                    continue;
                }

                if (other.State == VehicleTraffic.VehicleState.Stopped && other.IsPickupVehicle)
                {
                    continue;
                }

                if (!IsSameLane(vehicle, other))
                {
                    continue;
                }

                float gap = ComputeGap(vehicle, other);
                if (gap <= 0f || gap >= bestGap)
                {
                    continue;
                }

                bestGap = gap;
                leader = other;
            }

            return leader;
        }

        private static bool IsSameLane(VehicleTraffic self, VehicleTraffic other)
        {
            float laneZ = Level1BusStopRoadSettings.LaneCenterZForDirection(self.Direction);
            return Mathf.Abs(other.transform.position.z - laneZ) <= 1.15f;
        }

        private static float ComputeGap(VehicleTraffic self, VehicleTraffic other)
        {
            float selfX = self.PositionX;
            float otherX = other.PositionX;
            return self.Direction == VehicleTrafficDirection.LeftToRight
                ? otherX - selfX
                : selfX - otherX;
        }

        private void SpawnInitialTraffic()
        {
            float minGap = GetSpawnStaggerGap();

            SpawnVehicle(
                VehicleTrafficDirection.LeftToRight,
                fromLeft: true,
                allowPickup: false,
                spawnIndex: 0,
                minGap: minGap);

            SpawnVehicle(
                VehicleTrafficDirection.RightToLeft,
                fromLeft: false,
                allowPickup: false,
                spawnIndex: 1,
                minGap: minGap);

            int targetCount = Mathf.Max(2, initialVehicleCount);
            for (int i = 2; i < targetCount; i++)
            {
                bool fromLeft = i % 2 == 0;
                VehicleTrafficDirection direction = fromLeft
                    ? VehicleTrafficDirection.LeftToRight
                    : VehicleTrafficDirection.RightToLeft;
                SpawnVehicle(
                    direction,
                    fromLeft,
                    allowPickup: false,
                    spawnIndex: i,
                    minGap: minGap);
            }

            TrySpawnNextBusStopCandidate();
        }

        private void TrySpawnWhenRoadEmpty()
        {
            if (!outdoorSpawnReady || activeVehicles.Count > 0 || trafficPool == null || !isActiveAndEnabled)
            {
                return;
            }

            EnsureChallengeSessionForOutdoorPhase();
            if (!TrySpawnNextBusStopCandidate())
            {
                SpawnAmbientVehicle();
            }
        }

        private void SpawnAmbientVehicle()
        {
            if (BusStopChallengeSession.EncorePending)
            {
                TrySpawnEncoreHomeVehicle();
            }

            bool fromLeft = Random.value > 0.5f;
            if (TrySpawnAmbientInDirection(fromLeft))
            {
                return;
            }

            TrySpawnAmbientInDirection(!fromLeft);
        }

        private bool TrySpawnAmbientInDirection(bool fromLeft)
        {
            VehicleTrafficDirection direction = fromLeft
                ? VehicleTrafficDirection.LeftToRight
                : VehicleTrafficDirection.RightToLeft;
            return SpawnVehicle(direction, fromLeft, allowPickup: false);
        }

        private void SpawnRandomVehicle()
        {
            SpawnAmbientVehicle();
        }

        private bool SpawnVehicle(
            VehicleTrafficDirection direction,
            bool fromLeft,
            bool allowPickup,
            int spawnIndex = 0,
            float minGap = 0f,
            BusStopCandidateSlot forcedCandidate = default,
            bool encoreRide = false)
        {
            bool wantsPickup = false;
            BusStopCandidateSlot candidateSlot = default;
            if (forcedCandidate.Outcome != VehicleBoardingOutcome.None)
            {
                wantsPickup = true;
                candidateSlot = forcedCandidate;
            }
            else if (allowPickup
                     && direction == VehicleTrafficDirection.LeftToRight
                     && pickupWaitingVehicle == null
                     && pickupSlotHolder == null)
            {
                wantsPickup = BusStopChallengeSession.TryTakeNextCandidate(out candidateSlot);
            }

            Level1TrafficVehicleProfile profile = wantsPickup
                ? BuildBusStopCandidateProfile(candidateSlot)
                : PickRandomCarProfile();

            if (profile.prefab == null || trafficPool == null)
            {
                if (wantsPickup && !encoreRide)
                {
                    BusStopChallengeSession.ReturnCandidate(candidateSlot);
                }

                return false;
            }

            float spawnX = fromLeft
                ? roadStartX - spawnOffsetBeyondRoad - spawnIndex * minGap
                : roadEndX + spawnOffsetBeyondRoad + spawnIndex * minGap;
            float laneZ = Level1BusStopRoadSettings.LaneCenterZForDirection(direction);

            if (minGap <= 0f)
            {
                minGap = GetMinCenterGap();
            }

            if (!HasClearSpawnPoint(direction, spawnX, laneZ, minGap))
            {
                if (wantsPickup && !encoreRide)
                {
                    BusStopChallengeSession.ReturnCandidate(candidateSlot);
                }

                return false;
            }

            VehicleTraffic traffic = trafficPool.AcquireHost();
            if (traffic == null)
            {
                if (wantsPickup && !encoreRide)
                {
                    BusStopChallengeSession.ReturnCandidate(candidateSlot);
                }

                Debug.LogWarning("[TrafficManager] Pool full — could not spawn a traffic vehicle.");
                return false;
            }

            SetParentIfNeeded(traffic.transform, transform);
            traffic.name = wantsPickup
                ? encoreRide ? "EncoreHomeTrafficVehicle" : "PickupTrafficVehicle"
                : "TrafficVehicle";
            traffic.transform.position = new Vector3(spawnX, Level1BusStopRoadSettings.BusRootY, laneZ);

            float speed = Random.Range(minSpeed, maxSpeed);
            float accel = Random.Range(minAcceleration, maxAcceleration);
            float decel = Random.Range(minDeceleration, maxDeceleration);

            traffic.Initialize(
                this,
                trafficPool,
                direction,
                profile,
                wantsPickup,
                candidateSlot.Outcome,
                encoreRide,
                speed,
                accel,
                decel,
                despawnMaxX,
                candidateSlot.PrefabIndex,
                despawnMinX);

            activeVehicles.Add(traffic);

            if (wantsPickup && candidateSlot.Outcome == VehicleBoardingOutcome.HomeRide && !encoreRide)
            {
                BusStopChallengeSession.RememberHomeRideAppearance(candidateSlot);
            }

            return true;
        }

        private Level1TrafficVehicleProfile BuildCarProfile(
            GameObject prefab,
            Level1TrafficVehicleVisualKind kind,
            float rotationOffsetY,
            float scale,
            int bodyColorIndex = -1)
        {
            int colorIndex = bodyColorIndex;
            if (colorIndex < 0 && trafficBodyColors != null && trafficBodyColors.Length > 0)
            {
                colorIndex = Random.Range(0, trafficBodyColors.Length);
            }

            return new Level1TrafficVehicleProfile
            {
                prefab = prefab,
                modelRotationOffsetY = rotationOffsetY,
                visualLocalEulerOffset = carVisualLocalEuler,
                scale = scale,
                enableWheelSpin = true,
                wheelSpinAxis = ResolveWheelSpinAxis(carVisualLocalEuler),
                flattenVisualTilt = false,
                visualKind = kind,
                bodyColorIndex = colorIndex,
                lengthMultiplier = ResolveLengthMultiplier(prefab)
            };
        }

        private Level1TrafficVehicleProfile BuildBusStopCandidateProfile(BusStopCandidateSlot slot)
        {
            GameObject[] candidatePrefabs = GetBusStopCandidatePrefabs();
            GameObject prefab = candidatePrefabs.Length > 0
                ? candidatePrefabs[slot.PrefabIndex % candidatePrefabs.Length]
                : ResolvePickupPrefab();
            float scale = ResolveSpawnScale(prefab);
            float rotationOffset = ResolveRotationOffset(prefab);
            return BuildCarProfile(
                prefab,
                Level1TrafficVehicleVisualKind.PickupCar,
                rotationOffset,
                Mathf.Max(scale, pickupCarVisualScale * 0.92f),
                slot.BodyColorIndex);
        }

        private GameObject[] GetBusStopCandidatePrefabs()
        {
            if (trafficVehiclePrefabs == null || trafficVehiclePrefabs.Length == 0)
            {
                return System.Array.Empty<GameObject>();
            }

            var candidates = new List<GameObject>();
            for (int i = 0; i < trafficVehiclePrefabs.Length; i++)
            {
                GameObject prefab = trafficVehiclePrefabs[i];
                if (prefab != null && Level1KenneyCarKitPaths.IsBusStopCandidatePrefab(prefab.name))
                {
                    candidates.Add(prefab);
                }
            }

            return candidates.Count > 0 ? candidates.ToArray() : BuildDefaultBusStopCandidatePrefabs();
        }

        private GameObject[] BuildDefaultBusStopCandidatePrefabs()
        {
            var driveable = new List<GameObject>();
            if (trafficVehiclePrefabs != null)
            {
                for (int i = 0; i < trafficVehiclePrefabs.Length; i++)
                {
                    GameObject prefab = trafficVehiclePrefabs[i];
                    if (prefab != null && Level1KenneyCarKitPaths.IsBusStopCandidatePrefab(prefab.name))
                    {
                        driveable.Add(prefab);
                    }
                }
            }

            if (driveable.Count > 0)
            {
                return driveable.ToArray();
            }

            return CollectDriveablePrefabs();
        }

        private GameObject[] CollectDriveablePrefabs()
        {
            if (trafficVehiclePrefabs == null || trafficVehiclePrefabs.Length == 0)
            {
                return System.Array.Empty<GameObject>();
            }

            var driveable = new List<GameObject>();
            for (int i = 0; i < trafficVehiclePrefabs.Length; i++)
            {
                GameObject prefab = trafficVehiclePrefabs[i];
                if (prefab != null && !Level1KenneyCarKitPaths.IsAlienTrafficPrefab(prefab.name))
                {
                    driveable.Add(prefab);
                }
            }

            return driveable.ToArray();
        }

        private GameObject ResolvePickupPrefab()
        {
            if (trafficVehiclePrefabs != null)
            {
                for (int i = 0; i < trafficVehiclePrefabs.Length; i++)
                {
                    GameObject candidate = trafficVehiclePrefabs[i];
                    if (candidate == null)
                    {
                        continue;
                    }

                    string lower = candidate.name.ToLowerInvariant();
                    if (lower.Contains("van"))
                    {
                        return candidate;
                    }
                }

                for (int i = 0; i < trafficVehiclePrefabs.Length; i++)
                {
                    GameObject candidate = trafficVehiclePrefabs[i];
                    if (candidate == null)
                    {
                        continue;
                    }

                    string lower = candidate.name.ToLowerInvariant();
                    if (lower.Contains("delivery"))
                    {
                        return candidate;
                    }
                }
            }

            GameObject fromPool = PickRandomTrafficPrefab();
            if (fromPool != null)
            {
                return fromPool;
            }

            return carPrefabA != null ? carPrefabA : carPrefabB;
        }

        private Level1TrafficVehicleProfile PickRandomCarProfile()
        {
            GameObject prefab = PickRandomTrafficPrefab();
            if (prefab == null)
            {
                return default;
            }

            float scale = ResolveSpawnScale(prefab);
            float rotationOffset = ResolveRotationOffset(prefab);
            return BuildCarProfile(prefab, Level1TrafficVehicleVisualKind.CarA, rotationOffset, scale);
        }

        private GameObject PickRandomTrafficPrefab()
        {
            if (trafficVehiclePrefabs != null && trafficVehiclePrefabs.Length > 0)
            {
                int start = Random.Range(0, trafficVehiclePrefabs.Length);
                for (int i = 0; i < trafficVehiclePrefabs.Length; i++)
                {
                    GameObject prefab = trafficVehiclePrefabs[(start + i) % trafficVehiclePrefabs.Length];
                    if (prefab != null && !Level1KenneyCarKitPaths.IsAlienTrafficPrefab(prefab.name))
                    {
                        return prefab;
                    }
                }
            }

            if (carPrefabA != null && carPrefabB != null)
            {
                return Random.value > 0.5f ? carPrefabB : carPrefabA;
            }

            if (carPrefabA != null)
            {
                return carPrefabA;
            }

            return carPrefabB;
        }

        private float ResolveSpawnScale(GameObject prefab)
        {
            float baseScale = carVisualScale;
            string lower = prefab != null ? prefab.name.ToLowerInvariant() : string.Empty;
            if (lower.Contains("kart") || lower.Contains("race"))
            {
                baseScale *= 0.96f;
            }
            else if (lower.Contains("tractor") || lower.Contains("truck"))
            {
                baseScale *= 0.88f;
            }
            else if (lower.Contains("van") || lower.Contains("delivery"))
            {
                baseScale *= 0.92f;
            }
            else if (lower.Contains("suv"))
            {
                baseScale *= 0.94f;
            }

            if (carScaleVariance > 0.001f)
            {
                baseScale *= Random.Range(1f - carScaleVariance, 1f + carScaleVariance);
            }

            return baseScale;
        }

        private static float ResolveLengthMultiplier(GameObject prefab)
        {
            if (prefab == null)
            {
                return 1f;
            }

            string lower = prefab.name.ToLowerInvariant();
            if (lower.Contains("kart") || lower.Contains("race"))
            {
                return 0.72f;
            }

            if (lower.Contains("tractor") && lower.Contains("shovel"))
            {
                return 1.38f;
            }

            if (lower.Contains("tractor") || lower.Contains("truck"))
            {
                return 1.32f;
            }

            if (lower.Contains("van") || lower.Contains("delivery"))
            {
                return 1.12f;
            }

            if (lower.Contains("suv"))
            {
                return 1.06f;
            }

            return 1f;
        }

        private float ResolveRotationOffset(GameObject prefab)
        {
            if (prefab == null)
            {
                return carAModelRotationOffsetY;
            }

            string lower = prefab.name.ToLowerInvariant();
            if (lower.Contains("sports") || lower.Contains("hatchback"))
            {
                return carBModelRotationOffsetY;
            }

            return carAModelRotationOffsetY;
        }

        public bool TryReservePickupSlot(VehicleTraffic vehicle)
        {
            if (!vehicle.IsPickupVehicle || pickupSlotHolder != null || pickupWaitingVehicle != null)
            {
                return false;
            }

            pickupSlotHolder = vehicle;
            return true;
        }

        public void ForceReserveEncorePickupSlot(VehicleTraffic vehicle)
        {
            if (vehicle == null || !vehicle.IsEncoreRide)
            {
                return;
            }

            pickupSlotHolder = vehicle;
        }

        public void NotifyPickupVehicleWaiting(VehicleTraffic vehicle)
        {
            pickupWaitingVehicle = vehicle;
            pickupSlotHolder = vehicle;

            if (vehicle != null && vehicle.IsEncoreRide)
            {
                Level1PhaseController phase = Level1PhaseController.Instance;
                if (phase == null)
                {
                    phase = FindAnyObjectByType<Level1PhaseController>();
                }

                phase?.BeginEncoreDriverDialogue(vehicle);
            }
        }

        public void ReleasePickupSlot(VehicleTraffic vehicle)
        {
            if (pickupSlotHolder == vehicle)
            {
                pickupSlotHolder = null;
            }

            if (pickupWaitingVehicle == vehicle)
            {
                pickupWaitingVehicle = null;
            }

            ScheduleNextCandidateSpawn();
            if (BusStopChallengeSession.EncorePending)
            {
                BeginEncoreSpawnAttempts();
            }
        }

        /// <summary>Waits until the rejected 5th vehicle leaves before spawning the encore ride.</summary>
        public void RegisterEncoreDepartGate(VehicleTraffic departingVehicle)
        {
            if (departingVehicle != null && BusStopChallengeSession.EncorePending)
            {
                encoreDepartGateVehicle = departingVehicle;
                encoreGateFallbackTimer = EncoreGateFallbackSeconds;
            }
        }

        public void RequestEncoreHomeRide()
        {
            if (!BusStopChallengeSession.EncorePending)
            {
                return;
            }

            spawnTimer = 0f;
            encoreRetryTimer = 0f;
            BeginEncoreSpawnAttempts();
        }

        private void BeginEncoreSpawnAttempts()
        {
            if (encoreSpawnRoutine != null)
            {
                StopCoroutine(encoreSpawnRoutine);
            }

            encoreSpawnRoutine = StartCoroutine(SpawnEncoreWhenReady());
        }

        private IEnumerator SpawnEncoreWhenReady()
        {
            spawnTimer = 0f;
            encoreRetryTimer = 0f;
            while (BusStopChallengeSession.EncorePending)
            {
                if (pickupWaitingVehicle == null
                    && pickupSlotHolder == null
                    && TrySpawnEncoreHomeVehicle())
                {
                    encoreSpawnRoutine = null;
                    yield break;
                }

                yield return null;
            }

            encoreSpawnRoutine = null;
        }

        private bool TrySpawnEncoreHomeVehicle()
        {
            if (!BusStopChallengeSession.EncorePending
                || pickupWaitingVehicle != null
                || pickupSlotHolder != null
                || !IsEncoreDepartGateClear())
            {
                return false;
            }

            if (!BusStopChallengeSession.TryGetEncoreHomeSlot(out BusStopCandidateSlot slot))
            {
                return false;
            }

            return SpawnEncoreHomeAtCurb(slot);
        }

        private bool SpawnEncoreHomeAtCurb(BusStopCandidateSlot slot)
        {
            slot.Outcome = VehicleBoardingOutcome.HomeRide;
            Level1TrafficVehicleProfile profile = BuildBusStopCandidateProfile(slot);
            if (profile.prefab == null || trafficPool == null)
            {
                return false;
            }

            VehicleTraffic traffic = trafficPool.AcquireHost();
            if (traffic == null)
            {
                Debug.LogWarning("[TrafficManager] Pool full — could not spawn encore home vehicle.");
                return false;
            }

            Level1PickupPathPoints path =
                Level1PickupStopLayout.Resolve(VehicleTrafficDirection.LeftToRight, curbOffset);
            float laneZ = Level1BusStopRoadSettings.LaneCenterZForDirection(VehicleTrafficDirection.LeftToRight);
            float spawnX = path.RoadLaneStart.x - 1.8f;

            SetParentIfNeeded(traffic.transform, transform);
            traffic.name = "EncoreHomeTrafficVehicle";
            traffic.transform.position = new Vector3(spawnX, Level1BusStopRoadSettings.BusRootY, laneZ);

            float speed = Random.Range(minSpeed, maxSpeed);
            float accel = Random.Range(minAcceleration, maxAcceleration);
            float decel = Random.Range(minDeceleration, maxDeceleration);

            traffic.Initialize(
                this,
                trafficPool,
                VehicleTrafficDirection.LeftToRight,
                profile,
                isPickup: true,
                slot.Outcome,
                encoreRide: true,
                speed,
                accel,
                decel,
                despawnMaxX,
                slot.PrefabIndex,
                despawnMinX);

            activeVehicles.Add(traffic);
            traffic.BeginEncoreInstantPark();

            if (!traffic.IsWaitingForPickup)
            {
                activeVehicles.Remove(traffic);
                trafficPool.ReturnHost(traffic);
                return false;
            }

            encoreDepartGateVehicle = null;
            encoreGateFallbackTimer = 0f;
            return true;
        }

        private void TickEncoreDepartGateFallback()
        {
            if (encoreDepartGateVehicle == null || encoreDepartGateVehicle.IsWaitingForPickup)
            {
                return;
            }

            encoreGateFallbackTimer -= Time.deltaTime;
            if (encoreGateFallbackTimer <= 0f)
            {
                encoreDepartGateVehicle = null;
            }
        }

        private bool IsEncoreDepartGateClear()
        {
            if (encoreDepartGateVehicle == null)
            {
                return true;
            }

            VehicleTraffic gate = encoreDepartGateVehicle;
            if (gate == null || !gate.gameObject.activeInHierarchy)
            {
                encoreDepartGateVehicle = null;
                return true;
            }

            if (gate.IsEncoreRide)
            {
                encoreDepartGateVehicle = null;
                return true;
            }

            if (gate.IsWaitingForPickup)
            {
                return false;
            }

            Level1PickupPathPoints path = Level1PickupStopLayout.Resolve(gate.Direction, curbOffset);
            float curbX = path.CurbStopPoint.x;
            float vehicleX = gate.PositionX;
            float clearance = gate.Direction == VehicleTrafficDirection.LeftToRight
                ? vehicleX - curbX
                : curbX - vehicleX;

            if (clearance >= EncoreDepartClearDistance)
            {
                encoreDepartGateVehicle = null;
                return true;
            }

            if (!gate.IsPickupVehicle)
            {
                encoreDepartGateVehicle = null;
                return true;
            }

            if (encoreGateFallbackTimer <= 0f)
            {
                encoreDepartGateVehicle = null;
                return true;
            }

            return false;
        }

        public void ReleaseVehicle(VehicleTraffic vehicle)
        {
            if (vehicle == null)
            {
                return;
            }

            if (encoreDepartGateVehicle == vehicle)
            {
                encoreDepartGateVehicle = null;
            }

            ReleasePickupSlot(vehicle);
            activeVehicles.Remove(vehicle);
            trafficPool?.ReturnHost(vehicle);
        }

        public void HandleVehicleBoarded(VehicleTraffic vehicle)
        {
            if (vehicle != null)
            {
                vehicle.DepartAfterBoarding();
                ReleasePickupSlot(vehicle);
                pickupWaitingVehicle = null;
            }
        }

        private bool HasClearSpawnPoint(
            VehicleTrafficDirection direction,
            float spawnX,
            float laneZ,
            float minGap)
        {
            minGap = Mathf.Max(minGap, GetSpawnStaggerGap() * 1.15f);
            for (int i = 0; i < activeVehicles.Count; i++)
            {
                VehicleTraffic other = activeVehicles[i];
                if (other == null || other.Direction != direction)
                {
                    continue;
                }

                if (other.IsPickupVehicle
                    && (other.State == VehicleTraffic.VehicleState.Leaving
                        || other.State == VehicleTraffic.VehicleState.PullingOver))
                {
                    continue;
                }

                if (Mathf.Abs(other.transform.position.z - laneZ) > 1.15f)
                {
                    continue;
                }

                float separation = direction == VehicleTrafficDirection.LeftToRight
                    ? other.PositionX - spawnX
                    : spawnX - other.PositionX;
                if (separation < minGap)
                {
                    return false;
                }
            }

            return true;
        }

        private static void SetParentIfNeeded(Transform child, Transform parent)
        {
            if (child == null || parent == null || child.parent == parent)
            {
                return;
            }

            child.SetParent(parent, false);
        }

        private static Vector3 ResolveWheelSpinAxis(Vector3 visualLocalEuler)
        {
            return Vector3.right;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawLaneGizmos)
            {
                return;
            }

            float roadY = Level1BusStopRoadSettings.BusRootY + 0.08f;
            float northZ = Level1BusStopRoadSettings.LaneNorthCenterZ;
            float southZ = Level1BusStopRoadSettings.LaneSouthCenterZ;
            float startX = roadStartX != 0f || roadEndX != 0f ? roadStartX : Level1BusStopRoadSettings.RoadStartX;
            float endX = roadStartX != 0f || roadEndX != 0f ? roadEndX : Level1BusStopRoadSettings.RoadEndX;
            if (!Application.isPlaying)
            {
                Level1BusStopRoadExtents.RefreshFromScene();
                startX = Level1BusStopRoadExtents.RoadStartX;
                endX = Level1BusStopRoadExtents.RoadEndX;
            }

            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.85f);
            Gizmos.DrawLine(new Vector3(startX, roadY, northZ), new Vector3(endX, roadY, northZ));
            Gizmos.DrawLine(new Vector3(startX, roadY, southZ), new Vector3(endX, roadY, southZ));

            Gizmos.color = Color.yellow;
            Vector3 curbEdge = new Vector3(
                Level1BusStopRoadSettings.BusParkX,
                roadY,
                Level1BusStopRoadSettings.CurbEdgeZForDirection(VehicleTrafficDirection.LeftToRight));
            Vector3 stop = new Vector3(
                Level1BusStopRoadSettings.BusParkX,
                roadY,
                Level1BusStopRoadSettings.PickupStopZ(curbOffset));
            Gizmos.DrawWireSphere(curbEdge, 0.2f);
            Gizmos.DrawWireCube(stop, new Vector3(1.2f, 0.1f, 0.8f));

            Gizmos.color = Color.green;
            for (int i = 0; i < activeVehicles.Count; i++)
            {
                VehicleTraffic vehicle = activeVehicles[i];
                if (vehicle == null)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(vehicle.transform.position + Vector3.up * 0.35f, 0.25f);
            }
        }
    }
}
