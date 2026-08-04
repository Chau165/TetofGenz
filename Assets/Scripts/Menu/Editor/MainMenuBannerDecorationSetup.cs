#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChoTet.Menu.Editor
{
    public static class MainMenuBannerDecorationSetup
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/MainMenuBannerDecorationLibrary.asset";

        public static void SetupBannerDecorationLibrary()
        {
            EnsureFolder("Assets/Resources/Menu");

            MainMenuBannerDecorationLibrary library =
                AssetDatabase.LoadAssetAtPath<MainMenuBannerDecorationLibrary>(LibraryAssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<MainMenuBannerDecorationLibrary>();
                AssetDatabase.CreateAsset(library, LibraryAssetPath);
            }

            library.stemA = LoadPrefab(MainMenuBannerDecorationPaths.StemA);
            library.stemB = LoadPrefab(MainMenuBannerDecorationPaths.StemB);
            library.stemC = LoadPrefab(MainMenuBannerDecorationPaths.StemC);
            library.leafA = LoadPrefab(MainMenuBannerDecorationPaths.LeafA);
            library.leafB = LoadPrefab(MainMenuBannerDecorationPaths.LeafB);
            library.leafC = LoadPrefab(MainMenuBannerDecorationPaths.LeafC);
            library.edgeTuft = LoadPrefab(MainMenuBannerDecorationPaths.EdgeTuft);
            library.blossomYellowA = LoadPrefab(MainMenuBannerDecorationPaths.BlossomYellowA);
            library.blossomYellowB = LoadPrefab(MainMenuBannerDecorationPaths.BlossomYellowB);
            library.blossomYellowC = LoadPrefab(MainMenuBannerDecorationPaths.BlossomYellowC);
            library.blossomRedA = LoadPrefab(MainMenuBannerDecorationPaths.BlossomRedA);
            library.blossomRedB = LoadPrefab(MainMenuBannerDecorationPaths.BlossomRedB);
            library.blossomPot = LoadPrefab(MainMenuBannerDecorationPaths.BlossomPot);
            library.lantern = LoadPrefab(MainMenuBannerDecorationPaths.Lantern);
            library.lanternMarket = LoadPrefab(MainMenuBannerDecorationPaths.LanternMarket);
            library.coin = LoadPrefab(MainMenuBannerDecorationPaths.Coin);
            library.coins = LoadPrefab(MainMenuBannerDecorationPaths.Coins);
            library.appleRed = LoadPrefab(MainMenuBannerDecorationPaths.AppleRed);
            library.appleYellow = LoadPrefab(MainMenuBannerDecorationPaths.AppleYellow);

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("[MainMenuBannerDecorationSetup] Banner decoration library ready.");
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("[MainMenuBannerDecorationSetup] Missing prefab: " + path);
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
