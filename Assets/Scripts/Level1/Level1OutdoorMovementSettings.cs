using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Playable pedestrian bounds for the Bùi Đình Túy alley — sidewalks, shop aprons, bus shelter; not the highway lanes.</summary>
    public static class Level1OutdoorMovementSettings
    {
        /// <summary>West sidewalk + shop aprons (inside boundary fence).</summary>
        public const float PlayMinX = -12.8f;

        /// <summary>East sidewalk + shop aprons (inside boundary fence).</summary>
        public const float PlayMaxX = 12.8f;

        /// <summary>Just south of the boarding-house exit.</summary>
        public const float PlayStartZ = -4.1f;

        /// <summary>North edge of continuous walk colliders (house exit apron).</summary>
        public const float FloorStartZ = -3.85f;

        /// <summary>South end of alley walk — before highway asphalt (road blockers handle the rest).</summary>
        public const float PlayEndZ = -27.4f;

        public const float CameraClampMargin = 0.72f;

        /// <summary>Alley layout — matches <see cref="Level1VietnameseAlleyGroundBuilder"/>.</summary>
        public const float AlleyRoadHalfWidth = 2.38f;
        public const float AlleyCurbWidth = 0.16f;
        public const float AlleyCurbHeight = 0.12f;
        public const float WalkFloorThickness = 0.55f;

        /// <summary>How far sidewalk collision overlaps the curb strip so CharacterController can step up from the road.</summary>
        public const float CurbTransitionOverlap = 0.12f;

        public static float RoadSurfaceY => Level1OutdoorSettings.WalkSurfaceY - AlleyCurbHeight;

        /// <summary>Inner curb edge beside the alley asphalt (west side).</summary>
        public static float WestWalkInnerX => -(AlleyRoadHalfWidth + AlleyCurbWidth);

        /// <summary>Inner curb edge beside the alley asphalt (east side).</summary>
        public static float EastWalkInnerX => AlleyRoadHalfWidth + AlleyCurbWidth;

        public static float FloorSpanCenterZ => (FloorStartZ + PlayEndZ) * 0.5f;
        public static float FloorSpanDepth => PlayEndZ - FloorStartZ + 0.8f;

        public static float PlaySpanCenterZ => (PlayStartZ + PlayEndZ) * 0.5f;
        public static float PlaySpanDepth => PlayEndZ - PlayStartZ + 1.2f;
        public static float PlayHalfWidth => (PlayMaxX - PlayMinX) * 0.5f;

        /// <summary>North edge of the west-curb opening that leads to the bus stop sidewalk.</summary>
        public static float BusStopCurbGapNorthZ => Level1BusStopRoadSettings.SidewalkFarZ - 1.1f;

        /// <summary>South edge of the west-curb opening that leads to the bus stop sidewalk.</summary>
        public static float BusStopCurbGapSouthZ => Level1BusStopRoadSettings.SidewalkNearZ + 1.1f;

        /// <summary>East-west walk span around the bus shelter cluster (beside Com Tam).</summary>
        public static float BusStopWalkMinX => Level1BusStopRoadSettings.BusShelterCenterX - 3.2f;

        /// <summary>Includes pickup vehicle nose on the highway shoulder east of the shelter.</summary>
        public static float BusStopWalkMaxX =>
            Level1BusStopRoadSettings.PickupParkX
            + Level1PickupStopLayout.VehicleBoundsHalfLength
            + 0.85f;

        /// <summary>Legacy alias — east sidewalk bus-stop pad reaches the sign beside Com Tam.</summary>
        public static float BusStopApproachMaxX => BusStopWalkMaxX;

        /// <summary>North apron around the shelter roof footprint.</summary>
        public static float BusStopWalkMaxZ =>
            Mathf.Max(Level1BusStopRoadSettings.SidewalkNearZ + 1.35f,
                Level1BusStopRoadSettings.BusShelterCenterZ + 0.65f);

        /// <summary>Allows stepping onto highway lanes (lives + car hits apply).</summary>
        public static float BusStopWalkMinZ => Level1BusStopRoadSettings.LaneNorthEdgeZ + 0.2f;

        /// <summary>Gap in the east alley boundary wall so pedestrians can reach the bus stop apron.</summary>
        public static float BusStopBoundaryGapMinZ => BusStopWalkMinZ - 0.45f;
        public static float BusStopBoundaryGapMaxZ => BusStopWalkMaxZ + 0.45f;

        /// <summary>Outdoor camera should follow the player through the full bus-stop apron.</summary>
        public static float OutdoorCameraMaxX => Mathf.Max(PlayMaxX, BusStopWalkMaxX);

        public static float OutdoorCameraMinZ => Mathf.Min(PlayEndZ - 8f, BusStopWalkMinZ - 1f);
    }
}
