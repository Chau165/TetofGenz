using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Resolved pickup path points beside the sidewalk curb.</summary>
    public struct Level1PickupPathPoints
    {
        public Vector3 RoadLaneStart;
        public Vector3 RoadLaneDrive;
        public Vector3 CurbEdgePoint;
        public Vector3 CurbStopPoint;
        public Vector3 RoadDirection;
        public Vector3 CurbDirection;
        public float RoadYaw;
    }

    /// <summary>Computes curb-aligned pickup stops at the fixed bus shelter — not the player's alley position.</summary>
    public static class Level1PickupStopLayout
    {
        public const float PickupOffsetX = 0.85f;
        public const float LaneDriveLeadX = 3.5f;
        public const float ApproachTriggerLeadX = 4.5f;
        public const float DefaultCurbOffset = 0.8f;
        public const float VehicleBoundsHalfWidth = 0.92f;
        public const float VehicleBoundsHalfLength = 2.05f;

        /// <summary>Where the player waits beside the route sign at the Vietnamese bus shelter.</summary>
        public static Vector3 BusStopWaitPosition => Level1BusStopRoadSettings.BusStopSignStandPosition;

        public static Level1PickupPathPoints Resolve(VehicleTrafficDirection direction, float curbOffset)
        {
            return Resolve(direction, BusStopWaitPosition, curbOffset);
        }

        public static Level1PickupPathPoints Resolve(VehicleTrafficDirection direction)
        {
            return Resolve(direction, DefaultCurbOffset);
        }

        public static Level1PickupPathPoints Resolve(VehicleTrafficDirection direction, Vector3 playerPosition, float curbOffset)
        {
            float roadY = Level1BusStopRoadSettings.BusRootY;
            float laneZ = Level1BusStopRoadSettings.LaneCenterZForDirection(direction);

            Vector3 roadDir = direction == VehicleTrafficDirection.LeftToRight
                ? Vector3.right
                : Vector3.left;
            Vector3 curbDir = ResolveCurbDirection(direction);
            float roadYaw = direction == VehicleTrafficDirection.LeftToRight ? 90f : -90f;

            Vector3 curbStop = Level1BusStopRoadSettings.PickupCurbStopPosition(curbOffset);
            curbStop.z = ClampStopZ(curbStop.z, direction);
            curbStop.x = Mathf.Max(curbStop.x, Level1BusStopRoadSettings.PickupParkX);

            Vector3 curbEdge = new Vector3(curbStop.x, roadY, Level1BusStopRoadSettings.CurbEdgeZForDirection(direction));

            Vector3 laneAtStopX = new Vector3(curbStop.x, roadY, laneZ);
            Vector3 roadLaneDrive = laneAtStopX - roadDir * LaneDriveLeadX;

            float approachStartX = roadLaneDrive.x - roadDir.x * ApproachTriggerLeadX;
            Vector3 roadLaneStart = new Vector3(approachStartX, roadY, laneZ);

            return new Level1PickupPathPoints
            {
                RoadLaneStart = roadLaneStart,
                RoadLaneDrive = roadLaneDrive,
                CurbEdgePoint = curbEdge,
                CurbStopPoint = curbStop,
                RoadDirection = roadDir,
                CurbDirection = curbDir,
                RoadYaw = roadYaw
            };
        }

        public static Vector3 ResolvePlayerPosition()
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                return player.transform.position;
            }

            return BusStopWaitPosition;
        }

        public static Vector3 ResolveCurbDirection(VehicleTrafficDirection direction)
        {
            return direction == VehicleTrafficDirection.LeftToRight
                ? Vector3.forward
                : Vector3.back;
        }

        private static float ClampStopZ(float z, VehicleTrafficDirection direction)
        {
            if (direction == VehicleTrafficDirection.LeftToRight)
            {
                float minZ = Level1BusStopRoadSettings.LaneSouthEdgeZ + 0.22f;
                float maxZ = Level1BusStopRoadSettings.SidewalkNearZ - 0.08f;
                return Mathf.Clamp(z, minZ, maxZ);
            }

            float northMin = Level1BusStopRoadSettings.LaneNorthEdgeZ - Level1BusStopRoadSettings.ShoulderDepth + 0.22f;
            float northMax = Level1BusStopRoadSettings.LaneNorthEdgeZ - 0.08f;
            return Mathf.Clamp(z, northMin, northMax);
        }
    }
}
