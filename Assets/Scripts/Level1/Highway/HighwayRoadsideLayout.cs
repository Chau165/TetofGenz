namespace ChoTet.Level1.Highway
{
    /// <summary>Roadside belts: shoulder -> continuous grass field -> vegetation layers.</summary>
    public static class HighwayRoadsideLayout
    {
        public const int ContentVersion = 8;

        /// <summary>Grass begins just outside the paved shoulder — no urban sidewalk belt.</summary>
        public const float ShoulderClearance = 0.35f;

        public static float GrassGroundInnerX => HighwayRoadLayout.ShoulderOuterX + ShoulderClearance;

        public static float GrassGroundOuterX => HighwayRoadLayout.GrassOuterX;

        public static float GrassBeltWidth => GrassGroundOuterX - GrassGroundInnerX;

        /// <summary>Clear strip between shoulder edge and first vegetation details.</summary>
        public const float VegetationMarginFromShoulder = 3.2f;

        public static float VegetationMinAbsX => GrassGroundInnerX + VegetationMarginFromShoulder;

        public static float GrassClusterNearOutward => VegetationMarginFromShoulder + 5f;

        public static float GrassClusterMidOutward => VegetationMarginFromShoulder + 16f;

        public static float GrassClusterFarOutward => VegetationMarginFromShoulder + 26f;

        public static float BushZoneOutward => VegetationMarginFromShoulder + 24f;

        public static float TreeZoneOutward => VegetationMarginFromShoulder + 40f;

        public static float BackdropZoneOutward => VegetationMarginFromShoulder + 54f;

        public static float ClampOutwardOffset(float outward) =>
            UnityEngine.Mathf.Clamp(outward, VegetationMarginFromShoulder, GrassBeltWidth - 2f);

        public static float SignedInnerEdgeX(bool leftSide) =>
            (leftSide ? -1f : 1f) * GrassGroundInnerX;

        public static float SignedWorldX(bool leftSide, float outwardOffset)
        {
            float clamped = ClampOutwardOffset(outwardOffset);
            return leftSide ? -(GrassGroundInnerX + clamped) : GrassGroundInnerX + clamped;
        }

        public static bool IsOnRoadOrShoulder(float absX) => absX < GrassGroundInnerX;

        public static bool IsValidVegetationAbsX(float absX) =>
            absX >= VegetationMinAbsX && absX <= GrassGroundOuterX - 2f;
    }
}
