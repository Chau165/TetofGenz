#if UNITY_EDITOR
using ChoTet.Level1.Highway;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    public static class HighwayUrbanPrefabLibraryFactory
    {
        private const string LibraryPath = "Assets/Settings/ChoTet/HighwayUrbanPrefabLibrary.asset";

        public static HighwayUrbanPrefabLibrary BuildLibraryAsset()
        {
            HighwayPrefabSet set = HighwayPrefabLoader.LoadDefaultSet();
            HighwayUrbanPrefabLibrary library =
                AssetDatabase.LoadAssetAtPath<HighwayUrbanPrefabLibrary>(LibraryPath);
            if (library == null)
            {
                EnsureFolder("Assets/Settings/ChoTet");
                library = ScriptableObject.CreateInstance<HighwayUrbanPrefabLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            library.houses = set.houses;
            library.shops = set.shops;
            library.streetLights = set.streetLights;
            library.utilityPoles = set.utilityPoles;
            library.roadSigns = set.roadSigns;
            library.roadsideProps = set.roadsideProps;
            library.skylineBuildings = set.skylineBuildings;
            library.treeFillers = set.trees;
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HighwayUrbanPrefabLibraryFactory] Updated " + LibraryPath);
            return library;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
                string leaf = System.IO.Path.GetFileName(path);
                if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                {
                    EnsureFolder(parent);
                }

                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}
#endif
