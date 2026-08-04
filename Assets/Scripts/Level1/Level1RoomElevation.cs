namespace ChoTet.Level1
{
    /// <summary>Shared indoor packing-room elevation so editor builders and runtime bootstrap stay in sync.</summary>
    public static class Level1RoomElevation
    {
        public const float IndoorY = 0f;
        public const float PlayerSpawnLocalY = 0.02f;
        public const float PlayerSpawnLocalZ = 0.15f;
        public const float FallRecoveryMinY = IndoorY - 4f;

        public static bool IsElevatedIndoor(float worldY)
        {
            return worldY >= FallRecoveryMinY;
        }
    }
}
