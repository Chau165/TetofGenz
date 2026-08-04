using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Pickup maneuver: drive on lane, then a smooth Bezier merge to the curb with fixed road-aligned rotation.
    /// </summary>
    public class VehiclePickupController
    {
        public enum PickupPhase
        {
            Inactive,
            RoadLaneDrive,
            PullingOver,
            Parked,
            LeavingMerge,
            MergingOnRoad
        }

        private const float MergeTriggerDistance = 5.5f;
        private const float LeaveMergeLeadX = 5.5f;
        private const float BezierControlLead = 4f;
        private const float SpeedBlendRate = 4.5f;
        private const float DefaultPullOverSpeed = 0.35f;
        private const float DefaultNormalSpeed = 4f;
        private const float DefaultPickupSpeed = 2f;
        private const int BezierGizmoSegments = 24;

        private Transform host;
        private TrafficManager manager;
        private VehicleAnimator visual;
        private BusStopPickup pickupInteraction;

        private PickupPhase phase = PickupPhase.Inactive;
        private bool rotationLocked;

        private Level1PickupPathPoints path;
        private Vector3 roadDirection;
        private Quaternion parkedRotation;

        private Vector3 bezierP0;
        private Vector3 bezierP1;
        private Vector3 bezierP2;
        private Vector3 bezierP3;
        private float bezierT;

        private float pullOverSpeed;
        private float normalSpeed;
        private float pickupSpeed;

        private float curbOffset;

        private float laneCenterZ;
        private float cruiseSpeed;
        private float acceleration;
        private float deceleration;
        private float waitingElapsed;
        private bool slotReserved;

        private static readonly float RoadY = Level1BusStopRoadSettings.BusRootY;

        public PickupPhase Phase => phase;
        public bool IsActive => phase != PickupPhase.Inactive;
        public bool IsParked => phase == PickupPhase.Parked;
        public bool SlotReserved => slotReserved;
        public Level1PickupPathPoints Path => path;

        public VehicleTraffic.VehicleState MappedTrafficState
        {
            get
            {
                switch (phase)
                {
                    case PickupPhase.RoadLaneDrive:
                        return VehicleTraffic.VehicleState.SlowingDown;
                    case PickupPhase.PullingOver:
                        return VehicleTraffic.VehicleState.PullingOver;
                    case PickupPhase.Parked:
                        return VehicleTraffic.VehicleState.Stopped;
                    case PickupPhase.LeavingMerge:
                    case PickupPhase.MergingOnRoad:
                        return VehicleTraffic.VehicleState.Leaving;
                    default:
                        return VehicleTraffic.VehicleState.Driving;
                }
            }
        }

        public bool HasValidHost => host != null;

        public void Bind(Transform hostTransform, TrafficManager owner, VehicleAnimator vehicleVisual, BusStopPickup pickup)
        {
            host = hostTransform;
            manager = owner;
            visual = vehicleVisual;
            pickupInteraction = pickup;
        }

        public void EnsureHost(Transform hostTransform)
        {
            if (host == null && hostTransform != null)
            {
                host = hostTransform;
            }
        }

        public void Reset()
        {
            phase = PickupPhase.Inactive;
            rotationLocked = false;
            slotReserved = false;
            waitingElapsed = 0f;
            bezierT = 0f;
        }

        public void ConfigureLane(float laneZ, float travelYawDegrees, float speed, float accel, float decel, VehicleTrafficDirection direction)
        {
            laneCenterZ = laneZ;
            cruiseSpeed = speed;
            acceleration = accel;
            deceleration = decel;
            ApplyMovementSettingsFromManager();
            path = Level1PickupStopLayout.Resolve(direction, curbOffset);
            roadDirection = path.RoadDirection;
            parkedRotation = Quaternion.LookRotation(roadDirection, Vector3.up);
        }

        public void RefreshPath(VehicleTrafficDirection direction)
        {
            path = Level1PickupStopLayout.Resolve(direction, curbOffset);
            roadDirection = path.RoadDirection;
            parkedRotation = Quaternion.LookRotation(roadDirection, Vector3.up);
        }

        public bool TryTickDrivingApproach(float deltaTime, ref float currentSpeed, System.Func<float, float> clampX)
        {
            if (host == null)
            {
                return false;
            }

            if (phase != PickupPhase.Inactive)
            {
                return true;
            }

            float x = host.position.x;
            float triggerX = path.RoadLaneStart.x;
            bool pastTrigger = roadDirection.x > 0f ? x >= triggerX : x <= triggerX;
            if (!pastTrigger)
            {
                return false;
            }

            if (!slotReserved)
            {
                if (manager == null || !manager.TryReservePickupSlot(GetHostVehicle()))
                {
                    GetHostVehicle()?.AbandonPickupCandidate();
                    return false;
                }

                slotReserved = true;
            }

            RefreshPath(GetHostVehicle()?.Direction ?? VehicleTrafficDirection.LeftToRight);
            ApplyMovementSettingsFromManager();
            phase = PickupPhase.RoadLaneDrive;
            visual?.SetTurnSignal(VehicleTurnSignalSide.Left);
            return true;
        }

        public void Tick(float deltaTime, ref float currentSpeed, System.Func<float, float> clampX)
        {
            if (host == null)
            {
                return;
            }

            switch (phase)
            {
                case PickupPhase.RoadLaneDrive:
                    TickRoadLaneDrive(deltaTime, ref currentSpeed, clampX);
                    break;
                case PickupPhase.PullingOver:
                    TickPullingOver(deltaTime, ref currentSpeed);
                    break;
                case PickupPhase.Parked:
                    TickParked(deltaTime, ref currentSpeed);
                    break;
                case PickupPhase.LeavingMerge:
                    TickLeavingMerge(deltaTime, ref currentSpeed);
                    break;
                case PickupPhase.MergingOnRoad:
                    TickMergingOnRoad(deltaTime, ref currentSpeed, clampX);
                    break;
            }
        }

        public void BeginDeparture()
        {
            pickupInteraction?.SetWaiting(false);
            manager?.ReleasePickupSlot(GetHostVehicle());
            slotReserved = false;
            rotationLocked = false;
            waitingElapsed = 0f;
            BeginLeaveBezier();
            phase = PickupPhase.LeavingMerge;
            visual?.SetTurnSignal(VehicleTurnSignalSide.Right);
        }

        public void ForceDepartFromReview()
        {
            pickupInteraction?.SetWaiting(false);
            manager?.ReleasePickupSlot(GetHostVehicle());
            slotReserved = false;
            rotationLocked = false;
            waitingElapsed = 0f;

            if (phase == PickupPhase.Parked)
            {
                BeginLeaveBezier();
                phase = PickupPhase.LeavingMerge;
            }
            else
            {
                phase = PickupPhase.MergingOnRoad;
            }

            visual?.SetTurnSignal(VehicleTurnSignalSide.Right);
        }

        public void ForceEncorePark(ref float currentSpeed)
        {
            if (host == null)
            {
                return;
            }

            RefreshPath(GetHostVehicle()?.Direction ?? VehicleTrafficDirection.LeftToRight);
            ApplyMovementSettingsFromManager();

            VehicleTraffic hostVehicle = GetHostVehicle();
            if (!slotReserved && manager != null)
            {
                if (hostVehicle != null && hostVehicle.IsEncoreRide)
                {
                    manager.ForceReserveEncorePickupSlot(hostVehicle);
                    slotReserved = true;
                }
                else if (manager.TryReservePickupSlot(hostVehicle))
                {
                    slotReserved = true;
                }
            }

            EnterParked(ref currentSpeed);
        }

        public void CancelReservation()
        {
            if (phase == PickupPhase.Inactive && !slotReserved)
            {
                return;
            }

            if (slotReserved || phase == PickupPhase.Parked)
            {
                manager?.ReleasePickupSlot(GetHostVehicle());
            }

            slotReserved = false;
            Reset();
        }

        public static void DrawDebugGizmos(VehicleTrafficDirection direction, float curbOffsetDistance)
        {
            Level1PickupPathPoints points = Level1PickupStopLayout.Resolve(direction, curbOffsetDistance);
            float y = RoadY + 0.18f;

            Vector3 laneDrive = WithGizmoY(points.RoadLaneDrive, y);
            Vector3 curbEdge = WithGizmoY(points.CurbEdgePoint, y);
            Vector3 curbStop = WithGizmoY(points.CurbStopPoint, y);
            Vector3 laneStart = WithGizmoY(points.RoadLaneStart, y);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(laneDrive, 0.35f);
            Gizmos.DrawLine(laneStart, laneDrive);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(curbEdge, 0.22f);
            Gizmos.DrawLine(curbEdge, curbEdge + Vector3.up * 0.9f);

            Vector3 p0 = laneDrive;
            Vector3 p3 = curbStop;
            Vector3 p1 = p0 + points.RoadDirection * BezierControlLead;
            Vector3 p2 = p3 - points.RoadDirection * BezierControlLead;

            DrawBezierGizmo(p0, p1, p2, p3, Color.green);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(curbStop, 0.4f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(p1, 0.18f);
            Gizmos.DrawWireSphere(p2, 0.18f);

            DrawVehicleBoundsGizmo(curbStop, points.RoadDirection, y);

            Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.95f);
            Gizmos.DrawRay(curbStop + Vector3.up * 0.25f, points.RoadDirection * 2.4f);
        }

        private void TickRoadLaneDrive(float deltaTime, ref float currentSpeed, System.Func<float, float> clampX)
        {
            Vector3 pos = host.position;
            pos.y = RoadY;

            float alongRoad = roadDirection.x > 0f
                ? path.RoadLaneDrive.x - pos.x
                : pos.x - path.RoadLaneDrive.x;

            if (alongRoad <= MergeTriggerDistance)
            {
                BeginPullOverBezier();
                TickPullingOver(deltaTime, ref currentSpeed);
                return;
            }

            float decelRange = Mathf.Max(4f, Mathf.Abs(path.RoadLaneStart.x - path.RoadLaneDrive.x));
            float decelT = 1f - Mathf.Clamp01(alongRoad / decelRange);
            float easedDecel = Mathf.SmoothStep(0f, 1f, decelT);
            float desiredSpeed = Mathf.Lerp(normalSpeed, pickupSpeed, easedDecel * 0.35f);
            desiredSpeed = Mathf.Min(desiredSpeed, cruiseSpeed * 0.55f);
            SmoothSpeedToward(ref currentSpeed, desiredSpeed, deltaTime);

            float nextX = pos.x + roadDirection.x * currentSpeed * deltaTime;
            if (clampX != null)
            {
                nextX = clampX(nextX);
            }

            ApplyPose(nextX, laneCenterZ);
        }

        private void TickPullingOver(float deltaTime, ref float currentSpeed)
        {
            bezierT += deltaTime * pullOverSpeed;
            float t = Mathf.Clamp01(bezierT);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = EvaluateBezier(bezierP0, bezierP1, bezierP2, bezierP3, easedT);
            pos.y = RoadY;

            float targetSpeed = Mathf.Lerp(normalSpeed, pickupSpeed, easedT);
            SmoothSpeedToward(ref currentSpeed, targetSpeed, deltaTime);

            host.SetPositionAndRotation(pos, parkedRotation);

            if (t >= 1f)
            {
                EnterParked(ref currentSpeed);
            }
        }

        private void TickParked(float deltaTime, ref float currentSpeed)
        {
            SmoothSpeedToward(ref currentSpeed, 0f, deltaTime);

            waitingElapsed += deltaTime;
            if (manager != null && waitingElapsed >= manager.StopDuration)
            {
                GetHostVehicle()?.DepartAfterBoarding();
            }
        }

        private void TickLeavingMerge(float deltaTime, ref float currentSpeed)
        {
            bezierT += deltaTime * pullOverSpeed;
            float t = Mathf.Clamp01(bezierT);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = EvaluateBezier(bezierP0, bezierP1, bezierP2, bezierP3, easedT);
            pos.y = RoadY;

            float targetSpeed = Mathf.Lerp(pickupSpeed, normalSpeed, easedT);
            SmoothSpeedToward(ref currentSpeed, targetSpeed, deltaTime);

            host.SetPositionAndRotation(pos, parkedRotation);

            if (t >= 1f)
            {
                phase = PickupPhase.MergingOnRoad;
                visual?.SetTurnSignal(VehicleTurnSignalSide.None);
            }
        }

        private void TickMergingOnRoad(float deltaTime, ref float currentSpeed, System.Func<float, float> clampX)
        {
            float desiredSpeed = manager != null
                ? manager.GetConstrainedSpeed(GetHostVehicle(), cruiseSpeed)
                : cruiseSpeed;
            SmoothSpeedToward(ref currentSpeed, desiredSpeed, deltaTime);

            float nextX = host.position.x + roadDirection.x * currentSpeed * deltaTime;
            if (clampX != null)
            {
                nextX = clampX(nextX);
            }

            ApplyPose(nextX, laneCenterZ);
        }

        private void BeginPullOverBezier()
        {
            bezierP0 = host.position;
            bezierP0.y = RoadY;
            bezierP3 = path.CurbStopPoint;
            bezierP3.y = RoadY;
            bezierP1 = bezierP0 + roadDirection * BezierControlLead;
            bezierP2 = bezierP3 - roadDirection * BezierControlLead;
            bezierT = 0f;
            phase = PickupPhase.PullingOver;
        }

        private void BeginLeaveBezier()
        {
            bezierP0 = host.position;
            bezierP0.y = RoadY;
            bezierP3 = path.CurbStopPoint + roadDirection * LeaveMergeLeadX;
            bezierP3.y = RoadY;
            bezierP3.z = laneCenterZ;
            bezierP1 = bezierP0 + roadDirection * BezierControlLead;
            bezierP2 = bezierP3 - roadDirection * BezierControlLead;
            bezierT = 0f;
        }

        private void EnterParked(ref float currentSpeed)
        {
            phase = PickupPhase.Parked;
            rotationLocked = true;
            bezierT = 1f;

            Vector3 stopPoint = path.CurbStopPoint;
            stopPoint.y = RoadY;

            host.SetPositionAndRotation(stopPoint, parkedRotation);
            SmoothSpeedToward(ref currentSpeed, 0f, 0.05f);

            pickupInteraction?.SetWaiting(true);
            visual?.SetTurnSignal(VehicleTurnSignalSide.None);
            manager?.NotifyPickupVehicleWaiting(GetHostVehicle());

            if (GetHostVehicle()?.IsEncoreRide == true)
            {
                pickupInteraction?.SetWaiting(false);
                BusStopChallengeSession.MarkEncoreSpawned();
            }
        }

        private void ApplyMovementSettingsFromManager()
        {
            if (manager == null)
            {
                pullOverSpeed = DefaultPullOverSpeed;
                normalSpeed = DefaultNormalSpeed;
                pickupSpeed = DefaultPickupSpeed;
                curbOffset = Level1PickupStopLayout.DefaultCurbOffset;
                return;
            }

            pullOverSpeed = manager.PullOverSpeed;
            normalSpeed = manager.PickupNormalSpeed;
            pickupSpeed = manager.PickupCurbSpeed;
            curbOffset = manager.CurbOffset;
        }

        private void SmoothSpeedToward(ref float currentSpeed, float target, float deltaTime)
        {
            float rate = target > currentSpeed ? acceleration : deceleration;
            float blend = Mathf.Clamp01(deltaTime * SpeedBlendRate);
            float moveCap = rate * deltaTime;
            float lerped = Mathf.Lerp(currentSpeed, target, blend);
            currentSpeed = Mathf.MoveTowards(currentSpeed, lerped, moveCap);
        }

        private void ApplyPose(float x, float z)
        {
            if (rotationLocked)
            {
                host.position = new Vector3(x, RoadY, z);
                return;
            }

            host.SetPositionAndRotation(
                new Vector3(x, RoadY, z),
                parkedRotation);
        }

        private VehicleTraffic GetHostVehicle()
        {
            return host != null ? host.GetComponent<VehicleTraffic>() : null;
        }

        private static void DrawVehicleBoundsGizmo(Vector3 center, Vector3 roadDir, float y)
        {
            float halfW = Level1PickupStopLayout.VehicleBoundsHalfWidth;
            float halfL = Level1PickupStopLayout.VehicleBoundsHalfLength;
            Vector3 forward = roadDir.normalized;
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 c = center;
            c.y = y;
            Vector3 a = c - forward * halfL - side * halfW;
            Vector3 b = c + forward * halfL - side * halfW;
            Vector3 d = c + forward * halfL + side * halfW;
            Vector3 e = c - forward * halfL + side * halfW;

            Gizmos.color = new Color(0.2f, 0.75f, 1f, 0.9f);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, d);
            Gizmos.DrawLine(d, e);
            Gizmos.DrawLine(e, a);
        }

        private static void DrawBezierGizmo(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            Gizmos.color = color;
            Vector3 prev = p0;
            for (int i = 1; i <= BezierGizmoSegments; i++)
            {
                float t = i / (float)BezierGizmoSegments;
                float eased = Mathf.SmoothStep(0f, 1f, t);
                Vector3 point = EvaluateBezier(p0, p1, p2, p3, eased);
                Gizmos.DrawLine(prev, point);
                prev = point;
            }
        }

        private static Vector3 WithGizmoY(Vector3 point, float y)
        {
            point.y = y;
            return point;
        }

        private static Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0
                + 3f * u * u * t * p1
                + 3f * u * t * t * p2
                + t * t * t * p3;
        }
    }
}
