namespace ChoTet.Level1.Highway
{
    /// <summary>Asset paths for the Quaternius Downtown City Mega Kit (standard free pack).</summary>
    public static class QuaterniusDowntownAssets
    {
        public const string GltfRoot = "Assets/Exports/glTF (Godot)";
        public const string ModulePrefabRoot = "Assets/Prefabs/Highway/QuaterniusDowntown/Modules";
        public const string BlockPrefabRoot = "Assets/Prefabs/Highway/QuaterniusDowntown/Blocks";
        public const string LibraryPath = "Assets/Settings/ChoTet/QuaterniusDowntownPrefabLibrary.asset";
        public const string SidewalkPavementMaterialPath =
            "Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat";

        /// <summary>Canonical Quaternius module for highway sidewalk surfaces (plain concrete, no stripes).</summary>
        public const string PrimarySidewalkModuleName = "Sidewalk_NoCurb_3m";
        public const string SidewalkDecorPrefabRoot = "Assets/Prefabs/Highway/QuaterniusDowntown/Decor";

        public const float ModuleDepthZ = 2f;
        public const float ModuleGapZ = 0.08f;
        public const int TownBlockContentVersion = 7;
        public const int SidewalkContentVersion = 9;
    }
}
