namespace ChoTet.Runtime
{
    /// <summary>Player visual prefab paths — Creative Characters FREE output + Meshy legacy fallback.</summary>
    public static class PlayerCharacterVisualPaths
    {
        public const string CreativeCharacterRoot = "Assets/Characters/Player/CreativeCharacter";
        public const string CreativeCharacterMaterialsRoot = CreativeCharacterRoot + "/Materials";
        public const string MotherPinkShirtMaterial = CreativeCharacterMaterialsRoot + "/Mother_Pink_Shirt.mat";
        public const string MotherJeanPantsMaterial = CreativeCharacterMaterialsRoot + "/Mother_Jean_Pants.mat";
        public const string PlayerTealShirtMaterial = CreativeCharacterMaterialsRoot + "/Player_Teal_Shirt.mat";
        public const string PlayerDarkPantsMaterial = CreativeCharacterMaterialsRoot + "/Player_Dark_Pants.mat";
        public const string CreativeCharacterVisualPrefab = CreativeCharacterRoot + "/CreativeCharacter_PlayerVisual.prefab";
        public const string FatherVisualPrefab = CreativeCharacterRoot + "/CreativeCharacter_FatherVisual.prefab";
        public const string MotherVisualPrefab = CreativeCharacterRoot + "/CreativeCharacter_MotherVisual.prefab";
        public const string CreativeCharacterTopDownController = CreativeCharacterRoot + "/CreativeCharacter_TopDown.controller";

        public const string MeshyExplorerVisualPrefab = "Assets/Characters/Meshy_AI_Casual_Explorer_biped.glb";
        public const string MeshyExplorerAnimatorController = "Assets/Characters/Meshy_AI_Casual_Explorer_biped.controller";
        public const string MeshyExplorerVisualChildName = "CasualExplorer";

        public const string LegacyVisualRoot = "Assets/Characters/Player/RiggedExplorer_FromOldProject";
        public const string LegacyVisualPrefab = LegacyVisualRoot + "/RiggedExplorer_PlayerVisual.prefab";
        public const string LegacyAnimatorController = LegacyVisualRoot + "/Meshy_AI_Casual_Explorer_biped.controller";

        public const string VisualChildName = "CreativeCharacter";
        public const string LegacyVisualChildName = "RiggedExplorer";

        /// <summary>Default model yaw when the pack mesh faces +Z (Walk_Forward).</summary>
        public const float CreativeCharacterModelYaw = 0f;

        public const float DefaultModelScale = 1f;
    }
}
