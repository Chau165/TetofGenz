#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChoTet.Menu.Editor
{
    public static class MainMenuBannerVineSetup
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/MainMenuBannerVineLibrary.asset";

        public static void SetupBannerVineLibrary()
        {
            EnsureFolder("Assets/Resources/Menu");

            MainMenuBannerVineLibrary library =
                AssetDatabase.LoadAssetAtPath<MainMenuBannerVineLibrary>(LibraryAssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<MainMenuBannerVineLibrary>();
                AssetDatabase.CreateAsset(library, LibraryAssetPath);
            }

            library.stemA = LoadPrefab(MainMenuBannerVinePaths.StemA);
            library.stemB = LoadPrefab(MainMenuBannerVinePaths.StemB);
            library.stemC = LoadPrefab(MainMenuBannerVinePaths.StemC);
            library.leafTuftA = LoadPrefab(MainMenuBannerVinePaths.LeafTuftA);
            library.leafTuftB = LoadPrefab(MainMenuBannerVinePaths.LeafTuftB);
            library.leafTuftC = LoadPrefab(MainMenuBannerVinePaths.LeafTuftC);
            library.edgeTuft = LoadPrefab(MainMenuBannerVinePaths.EdgeTuft);

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("[MainMenuBannerVineSetup] Banner vine library ready.");
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("[MainMenuBannerVineSetup] Missing prefab: " + path);
            }

            return prefab;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

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
#endif
