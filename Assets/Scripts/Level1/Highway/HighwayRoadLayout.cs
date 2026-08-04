namespace ChoTet.Level1.Highway
{
    /// <summary>Grid constants for a straight, symmetrical highway cross-section and tiling.</summary>
    public static class HighwayRoadLayout
    {
        public const float TileLength = 7.5f;
        public const int TilesPerSegment = 6;
        public const float SegmentLength = TileLength * TilesPerSegment;

        public const float PlayerRoadHalfWidth = HighwayLaneSettings.PlayerRoadHalfWidth;
        public const float ShoulderOuterX = PlayerRoadHalfWidth + HighwayLaneSettings.ShoulderWidth;
        public const float PavedHalfWidth = ShoulderOuterX;
        public const float GrassInnerX = ShoulderOuterX;
        public const float GrassOuterX = HighwayLaneSettings.TerrainOuterX;

        public const float MarkingY = 0.06f;
        public const float MarkThickness = 0.012f;
        public const float DashLength = 4f;
        public const float DashGap = 3f;
        public const float EdgeLineWidth = 0.14f;
        public const float DividerLineWidth = 0.15f;
        public const float DashCycle = DashLength + DashGap;

        public const float SegmentOverlapZ = 0.15f;

        /// <summary>3 playable lane centers — computed from road mesh width, not divider lines.</summary>
        public static float PlayerLaneCenterX(int laneIndex) =>
            HighwayPlayerLaneLayout.LaneCenterX(laneIndex);

        public static readonly float[] PlayerLaneCenters = HighwayPlayerLaneLayout.LaneCenters;

        /// <summary>Dashed divider lines between playable lanes (not lane centers).</summary>
        public static readonly float[] PlayerLaneDividers = HighwayPlayerLaneLayout.LaneDividers;

        /// <summary>Traffic uses the same 3-lane grid as the player.</summary>
        public static readonly float[] TrafficLaneCenters = HighwayPlayerLaneLayout.LaneCenters;

        public static readonly float[] TrafficLaneDividers = HighwayPlayerLaneLayout.LaneDividers;

        public const float GuardEdgeWidth = 0.14f;

        public const float GuardEdgeHeight = 0.16f;

        public const int RoadContentVersion = 10;

        public const string RoadLayoutMarkerPrefix = "RoadLayout_v";

        public static float LaneCenterX(int laneIndex)
        {
            laneIndex = HighwayLaneSettings.ClampLane(laneIndex);
            return HighwayPlayerLaneLayout.LaneCenterX(laneIndex);
        }

        public static float LanePairBoundaryX(int boundaryIndex)
        {
            boundaryIndex = UnityEngine.Mathf.Clamp(boundaryIndex, 0, HighwayPlayerLaneLayout.LaneDividers.Length - 1);
            return HighwayPlayerLaneLayout.LaneDividers[boundaryIndex];
        }
    }
}
