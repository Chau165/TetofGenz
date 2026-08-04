using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Default material paths for the procedural highway road deck.</summary>
    public static class HighwayRoadMaterialDefaults
    {
        public const string RoadSurfaceFile = "Highway_Asphalt_Gray.mat";
        public const string LaneMarkFile = "Highway_LaneMark_White.mat";
        public const string ShoulderFile = "Highway_Shoulder_Concrete.mat";
        public const string EdgeMarkFile = "Lantern_Gold.mat";
        public const string GuardEdgeFile = "Foundation_Stone.mat";
        public const string GuardRailFile = "Highway_Guardrail_Galvanized.mat";
        public const string WaterFile = "Highway_Countryside_Water.mat";
        public const string TireWearFile = "Highway_TireWear.mat";

        public static string SceneMaterialsRoot => HighwaySceneAssetPaths.MaterialsFolder;

        public static void ApplyIfMissing(
            ref Material roadSurface,
            ref Material laneMark,
            ref Material shoulder,
            ref Material edgeMark)
        {
#if UNITY_EDITOR
            if (roadSurface == null)
            {
                roadSurface = Load(RoadSurfaceFile);
            }

            if (laneMark == null)
            {
                laneMark = Load(LaneMarkFile);
            }

            if (shoulder == null)
            {
                shoulder = Load(ShoulderFile);
            }

            if (edgeMark == null)
            {
                edgeMark = Load(EdgeMarkFile);
            }
#endif
        }

#if UNITY_EDITOR
        public static Material Load(string fileName)
        {
            Material sceneMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                SceneMaterialsRoot + "/" + fileName);
            if (sceneMat != null)
            {
                return sceneMat;
            }

            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
        }

        public static Material LoadScene(string fileName) =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(SceneMaterialsRoot + "/" + fileName);
#endif
    }
}
