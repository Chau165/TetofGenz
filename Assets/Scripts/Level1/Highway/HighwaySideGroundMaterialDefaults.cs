using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Default roadside grass material (Nature Starter Kit 2 texture on URP Lit).</summary>
    public static class HighwaySideGroundMaterialDefaults
    {
        public const string GrassFile = "Highway_Grass_NSK2.mat";

#if UNITY_EDITOR
        public static Material LoadGrass()
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.MaterialsRoot + "/" + GrassFile);
        }

        public static void ApplyIfMissing(ref Material grass)
        {
            if (grass == null)
            {
                grass = LoadGrass();
            }
        }
#endif
    }
}
