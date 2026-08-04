namespace ChoTet.Level1.Highway
{
    /// <summary>Scene-specific asset paths for Level1_Highway — never overwrites shared project assets.</summary>
    public static class HighwaySceneAssetPaths
    {
        public const string Root = "Assets/Level1/HighWay";
        public const string MaterialsFolder = Root + "/Materials";
        public const string PrefabsFolder = Root + "/Prefabs";
        public const string EnvironmentFolder = Root + "/Environment";

        public const string AsphaltMat = "Highway_Asphalt_Mat.mat";
        public const string LaneMarkMat = "Highway_LaneMark_Mat.mat";
        public const string ShoulderMat = "Highway_Shoulder_Mat.mat";
        public const string GuardRailMat = "Highway_Guardrail_Mat.mat";
        public const string TireWearMat = "Highway_TireWear_Mat.mat";
        public const string WaterMat = "Highway_River_Mat.mat";
        public const string PackWaterMat = "Highway_PackWater_URP.mat";
        public const string BridgeConcreteMat = "Highway_Bridge_Concrete_Mat.mat";
        public const string BridgeUndersideMat = "Highway_Bridge_Underside_Mat.mat";
        public const string RiceFieldMat = "Highway_RiceField_Mat.mat";
        public const string CanalMat = "Highway_Canal_Mat.mat";
        public const string SkyMat = "Highway_AfternoonSky_Mat.mat";
        public const string SignMat = "Highway_Sign_Mat.mat";
        public const string PoleMat = "Highway_Pole_Mat.mat";
        public const string GrassGroundMat = "Highway_GrassGround_Mat.mat";
        public const string FoliageMat = "Highway_Foliage_Mat.mat";
        public const string BarkMat = "Highway_Bark_Mat.mat";
        public const string GrassBladeAMat = "Highway_GrassBladeA_Mat.mat";
        public const string GrassBladeBMat = "Highway_GrassBladeB_Mat.mat";

        public static string MaterialPath(string fileName) => MaterialsFolder + "/" + fileName;
    }
}
