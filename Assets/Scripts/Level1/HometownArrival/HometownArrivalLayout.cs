namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Shared arrival scene layout — junction, stop, and pull-over positions.</summary>
    public static class HometownArrivalLayout
    {
        public const float BusLaneX = 0f;
        public const float BusStartZ = 86f;
        public const float BusGroundY = 0f;

        /// <summary>T-junction ahead on the road (lower world Z — not yet reached when the car stops).</summary>
        public const float JunctionWorldZ = 26.5f;

        /// <summary>Car stops here before the junction (higher world Z, travel direction is -Z).</summary>
        public const float BusStopZ = 35.8f;

        /// <summary>Playable road half-width (3-lane highway grid).</summary>
        private const float PlayableRoadHalfWidth = 7.8f;

        /// <summary>Inset from road edge when pulling onto the branch-side shoulder.</summary>
        private const float ShoulderParkInsetFromEdge = 1.35f;

        /// <summary>Pull-over tight on the left shoulder — not in the driving lane.</summary>
        public const float ParkedRoadsideX = -(PlayableRoadHalfWidth - ShoulderParkInsetFromEdge);

        public const float PullOverStartNormalized = 0.48f;

        /// <summary>Distance from stop to junction — stop is this many meters before the junction.</summary>
        public static float StopBeforeJunctionMeters => BusStopZ - JunctionWorldZ;

        /// <summary>Parents' sedan — fixed pose on the branch shoulder (never tied to the boarded ride).</summary>
        public const float ParentsCarWorldX = -16.04f;
        public const float ParentsCarWorldZ = 22.59f;
        public const float ParentsCarWorldY = 0f;
        public const float ParentsCarYaw = 90f;
        public const float ParentsCarVisualScale = 1.38f;
        public const int ParentsCarBodyColorIndex = 7;
    }
}
