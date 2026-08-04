namespace ChoTet.Level1.HometownArrival
{
    internal static class HometownArrivalCountrysideLayout
    {
        public const float RoadHalfClear = 11.8f;
        public const float FenceAbsX = RoadHalfClear + 1.1f;

        // Bushes sit just outside the fence line.
        public const float BushMinAbsX = FenceAbsX + 0.6f;
        public const float BushMaxAbsX = FenceAbsX + 5.5f;

        // Dense tree line right behind the fence.
        public const float TreeLineMinAbsX = FenceAbsX + 0.8f;
        public const float TreeLineMaxAbsX = FenceAbsX + 4.5f;

        // Three additional depth belts.
        public const float TreeNearMinAbsX = FenceAbsX + 3.5f;
        public const float TreeNearMaxAbsX = FenceAbsX + 9.0f;
        public const float TreeMidMinAbsX = FenceAbsX + 7.5f;
        public const float TreeMidMaxAbsX = FenceAbsX + 18.0f;
        public const float TreeFarMinAbsX = FenceAbsX + 15.0f;
        public const float TreeFarMaxAbsX = FenceAbsX + 28.0f;

        public const float GrassInnerAbsX = RoadHalfClear + 1.2f;
        public const float GrassOuterAbsX = FenceAbsX + 30f;
        public const float FenceSpacing = 3.2f;

        public const float NearTreeMaxScale = 1.05f;
        public const float MidTreeMaxScale = 1.35f;
        public const float FarTreeMaxScale = 1.55f;
    }
}
