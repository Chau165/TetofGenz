using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>URP materials using Nature Starter Kit 2 textures for highway countryside.</summary>
    public static class HighwayNatureMaterialDefaults
    {
        public const string GrassNearFile = "Highway_Grass_NSK2.mat";
        public const string GrassMidFile = "Highway_Ground_NSK2_Mid.mat";
        public const string GrassFarFile = "Highway_Ground_NSK2_Far.mat";
        public const string FoliageFile = "Highway_Foliage_NSK2.mat";
        public const string BarkFile = "Highway_Bark_NSK2.mat";
        public const string ShoulderEarthFile = "Highway_Shoulder_Earth.mat";
        public const string GrassBlade01File = "Highway_GrassBlade01_NSK2.mat";
        public const string GrassBlade02File = "Highway_GrassBlade02_NSK2.mat";
        public const string GroundEarthFile = "Highway_GroundEarth_NSK2.mat";

#if UNITY_EDITOR
        public static Material Load(string fileName)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
        }

        public static void ApplyGrassIfMissing(
            ref Material near,
            ref Material mid,
            ref Material far)
        {
            if (near == null)
            {
                near = Load(GrassNearFile);
            }

            if (mid == null)
            {
                mid = Load(GrassMidFile);
            }

            if (far == null)
            {
                far = Load(GrassFarFile);
            }
        }
#endif
    }
}
