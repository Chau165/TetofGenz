using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>
    /// Single canonical sidewalk material for the entire highway urban corridor.
    /// Source: Quaternius Downtown <see cref="QuaterniusDowntownAssets.PrimarySidewalkModuleName"/>.
    /// </summary>
    public static class HighwaySidewalkMaterialUtility
    {
        public static Material Resolve(QuaterniusDowntownPrefabLibrary library = null)
        {
            if (library != null && library.sidewalkPavementMaterial != null)
            {
                return library.sidewalkPavementMaterial;
            }

#if UNITY_EDITOR
            Material baked = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                QuaterniusDowntownAssets.SidewalkPavementMaterialPath);
            if (baked != null)
            {
                return baked;
            }
#endif

            return null;
        }
    }
}
