using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Playable 3-lane centers derived from road width — never uses divider positions as lanes.</summary>
    public static class HighwayPlayerLaneLayout
    {
        public const int LaneCount = 3;
        public const int MiddleLaneIndex = 1;
        public const float RoadCenterX = 0f;

        public static float PlayableRoadHalfWidth => HighwayLaneSettings.PlayerRoadHalfWidth;
        public static float LaneWidth => HighwayLaneSettings.LaneWidth;
        public static float PlayableLeftEdgeX => RoadCenterX - PlayableRoadHalfWidth;
        public static float PlayableRightEdgeX => RoadCenterX + PlayableRoadHalfWidth;

        public static float LaneCenterX(int laneIndex)
        {
            laneIndex = HighwayLaneSettings.ClampPlayerLane(laneIndex);
            return PlayableLeftEdgeX + LaneWidth * (laneIndex + 0.5f);
        }

        public static float LaneDividerX(int dividerIndex)
        {
            dividerIndex = Mathf.Clamp(dividerIndex, 0, LaneCount - 2);
            return (LaneCenterX(dividerIndex) + LaneCenterX(dividerIndex + 1)) * 0.5f;
        }

        public static readonly float[] LaneCenters = BuildLaneCenters();

        public static readonly float[] LaneDividers = BuildLaneDividers();

        private static float[] BuildLaneCenters()
        {
            var centers = new float[LaneCount];
            for (int i = 0; i < LaneCount; i++)
            {
                centers[i] = LaneCenterX(i);
            }

            return centers;
        }

        private static float[] BuildLaneDividers()
        {
            var dividers = new float[LaneCount - 1];
            for (int i = 0; i < dividers.Length; i++)
            {
                dividers[i] = LaneDividerX(i);
            }

            return dividers;
        }
    }
}
