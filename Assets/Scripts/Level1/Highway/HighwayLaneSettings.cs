using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Layout constants for the 3-lane highway (all lanes drive +Z with the player).</summary>
    public static class HighwayLaneSettings
    {
        public const int LaneCount = 3;
        public const int OppositeLaneCount = 0;
        public const int PlayerLaneCount = 3;

        public const float LaneWidth = 5.2f;
        public const float PlayerRoadHalfWidth = LaneWidth * 1.5f;
        public const float RoadHalfWidth = LaneWidth * 2f;
        public const float RoadHalfWidthFar = RoadHalfWidth * 0.68f;
        public const float CenterDividerWidth = 0.48f;
        public const float ShoulderWidth = 6.5f;
        public const float VergeWidth = 4f;
        public const float RoadSurfaceY = 0f;

        /// <summary>Road deck sits above shoulders/sidewalk to avoid z-fighting.</summary>
        public const float RoadDeckY = 0.02f;

        public const float ShoulderSurfaceY = -0.06f;

        public const float SidewalkWidth = 5f;

        public const float SidewalkSurfaceY = 0.045f;

        public const float ZonePropsX = 22f;
        public const float ZoneBuildingsX = 35f;
        public const float ZoneTreesX = 50f;
        public const float ZoneBackdropX = 72f;

        public const float TerrainOuterX = 220f;
        public const float TerrainOuterXFar = 150f;

        public const float HorizonDistance = 130f;
        public const float PlayerSpawnZ = 0f;
        public const int PlayerStartLaneIndex = 1;

        public static int ClampPlayerLane(int laneIndex) =>
            Mathf.Clamp(laneIndex, 0, PlayerLaneCount - 1);

        public static float PlayerLaneCenterX(int laneIndex) =>
            HighwayRoadLayout.PlayerLaneCenterX(laneIndex);

        public static float LaneCenterX(int laneIndex) => LaneCenterX(laneIndex, 0f);

        public static float LaneCenterX(int laneIndex, float zAhead)
        {
            return HighwayRoadLayout.LaneCenterX(laneIndex);
        }

        public static float ShoulderOuterX(float zAhead)
        {
            return HighwayRoadLayout.ShoulderOuterX;
        }

        public static float VergeOuterX(float zAhead)
        {
            return ShoulderOuterX(zAhead) + VergeWidth;
        }

        public static float TerrainOuterAt(float zAhead)
        {
            float t = HighwayPerspective.PerspectiveT(zAhead);
            return Mathf.Lerp(TerrainOuterX, TerrainOuterXFar, t);
        }

        public static float ZoneOffsetX(bool leftSide, HighwayRoadsideZone zone, float zAhead)
        {
            float baseX = zone switch
            {
                HighwayRoadsideZone.Props => ZonePropsX,
                HighwayRoadsideZone.Buildings => ZoneBuildingsX,
                HighwayRoadsideZone.Trees => ZoneTreesX,
                _ => ZoneBackdropX
            };
            float x = HighwayPerspective.SideOffsetAt(baseX, zAhead);
            return leftSide ? -x : x;
        }

        public static bool IsOppositeLane(int laneIndex) => laneIndex < OppositeLaneCount;

        public static bool IsPlayerLane(int laneIndex) => laneIndex >= OppositeLaneCount;

        public static int ClampLane(int laneIndex) => Mathf.Clamp(laneIndex, 0, LaneCount - 1);

        /// <summary>X position between two adjacent lanes (boundaryIndex: 1 = lanes 0|1, 2 = lanes 2|3).</summary>
        public static float LanePairBoundaryX(int boundaryIndex, float zAhead)
        {
            return HighwayRoadLayout.LanePairBoundaryX(boundaryIndex);
        }
    }
}
