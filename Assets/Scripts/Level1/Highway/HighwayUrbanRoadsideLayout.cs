using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>
    /// Urban roadside bands (road → curb → wide pedestrian sidewalk → buildings).
    /// </summary>
    public static class HighwayUrbanRoadsideLayout
    {
        public const int ContentVersion = 10;

        public const float CurbWidth = 0.18f;

        public const float BuildingFrontSetback = 0.08f;

        public static float ShoulderOuterX => HighwayRoadLayout.ShoulderOuterX;

        public static float PlayableRoadHalfWidth => HighwayPlayerLaneLayout.PlayableRoadHalfWidth;

        public static float TrafficRoadHalfWidth => HighwayPlayerLaneLayout.PlayableRoadHalfWidth;

        /// <summary>Curb sits on the playable road edge — no wide sand shoulder in front of buildings.</summary>
        public static float CurbInnerX => PlayableRoadHalfWidth;

        public static float CurbOuterX => PlayableRoadHalfWidth + CurbWidth;

        public static float SidewalkInnerX => CurbOuterX;

        public static float SidewalkWidth => HighwayLaneSettings.SidewalkWidth;

        public static float SidewalkSurfaceY => HighwayLaneSettings.SidewalkSurfaceY;

        public static float SidewalkCenterX => SidewalkInnerX + SidewalkWidth * 0.5f;

        public static float SidewalkOuterX => SidewalkInnerX + SidewalkWidth;

        public static float UtilityCorridorInnerX => SidewalkInnerX;

        public static float UtilityCorridorOuterX => SidewalkOuterX;

        public static float BuildingBlockInnerX => SidewalkOuterX + BuildingFrontSetback;

        public static float BuildingRowX => SidewalkOuterX + 0.12f;

        public static float BuildingRowSecondaryX => BuildingRowX + 2.8f;

        public static float BuildingRowTertiaryX => BuildingRowSecondaryX + 2.8f;

        public static float UtilityPoleRowX => SidewalkInnerX + 0.9f;

        public static float StreetLightRowX => SidewalkInnerX + 2.1f;

        public static float SignRowX => SidewalkInnerX + 3.4f;

        public static float TreeRowX => SidewalkOuterX - 0.65f;

        public static float DecorationRowX => SidewalkOuterX - 0.2f;

        /// <summary>Trash bins sit just inside the curb — visible from the road.</summary>
        public static float TrashBinRowX => SidewalkInnerX + 1.05f;

        /// <summary>Bus shelters anchor slightly inboard of bins, still near the curb.</summary>
        public static float BusStopRowX => SidewalkInnerX + 1.85f;

        /// <summary>Clear pedestrian path between street furniture and building trees.</summary>
        public static float WalkPathInnerX => SidewalkInnerX + 2.55f;

        public static float WalkPathOuterX => SidewalkOuterX - 1.35f;

        public const float MinClearanceFromRoadEdge = 0.9f;

        public const float MinBuildingSpacingZ = 0.55f;

        public const float BuildingGridSnap = 2.2f;

        public const float BuildingLotStepZ = 1.05f;

        public const float OccupancyRadius = 1.6f;

        public const float BuildingOccupancyRadius = 1.35f;

        public const float SignOccupancyRadius = 2.4f;

        public const float SignScaleBoost = 1.38f;

        public static float SignedSidewalkCenterX(bool leftSide) =>
            (leftSide ? -1f : 1f) * SidewalkCenterX;

        public static float SignedRowX(bool leftSide, float outwardRowX) =>
            (leftSide ? -1f : 1f) * outwardRowX;

        public static bool IsOnDrivingSurface(float absX) =>
            absX < TrafficRoadHalfWidth + MinClearanceFromRoadEdge;

        public static bool IsInsideSidewalk(float absX) =>
            absX >= SidewalkInnerX - 0.05f && absX <= SidewalkOuterX + 0.15f;

        public static bool IsInsideUtilityCorridor(float absX) => IsInsideSidewalk(absX);

        public static bool IsBuildingRowSafe(float absX) =>
            absX >= BuildingBlockInnerX - 0.1f;

        public static bool BuildingBoundsInsideBlock(Bounds bounds, bool leftSide)
        {
            float inner = BuildingBlockInnerX;
            if (leftSide)
            {
                return bounds.max.x <= -inner + 0.15f;
            }

            return bounds.min.x >= inner - 0.15f;
        }
    }
}
