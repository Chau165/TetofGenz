using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Perspective helpers — road and props converge toward the horizon.</summary>
    public static class HighwayPerspective
    {
        public static float HorizonDistance => HighwayLaneSettings.HorizonDistance;

        public static float PerspectiveT(float zAhead)
        {
            return Mathf.Clamp01(zAhead / HorizonDistance);
        }

        public static float RoadHalfWidthAt(float zAhead)
        {
            float t = PerspectiveT(zAhead);
            return Mathf.Lerp(HighwayLaneSettings.RoadHalfWidth, HighwayLaneSettings.RoadHalfWidthFar, t);
        }

        public static float SideOffsetAt(float baseOffsetX, float zAhead)
        {
            float t = PerspectiveT(zAhead);
            float pull = Mathf.Lerp(1f, 0.58f, t);
            return baseOffsetX * pull;
        }

        public static float ScaleAt(float zAhead)
        {
            float t = PerspectiveT(zAhead);
            return Mathf.Lerp(1f, 0.42f, t * t);
        }

        public static float PoleHeightScale(float zAhead)
        {
            float t = PerspectiveT(zAhead);
            return Mathf.Lerp(1f, 0.55f, t);
        }
    }
}
