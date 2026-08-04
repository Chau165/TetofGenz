#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Menu.Editor
{
    [InitializeOnLoad]
    public static class MainMenuTableDecorAutoSetup
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/MainMenuTableDecorLibrary.asset";

        static MainMenuTableDecorAutoSetup()
        {
            EditorApplication.delayCall += EnsureLibraryAssetExists;
        }

        private static void EnsureLibraryAssetExists()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<MainMenuTableDecorLibrary>(LibraryAssetPath) != null)
            {
                return;
            }

            MainMenuTableDecorSetup.SetupTableDecorLibrary();
        }
    }

    public static class MainMenuTableDecorSetup
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/MainMenuTableDecorLibrary.asset";

        public static void SetupTableDecorLibrary()
        {
            EnsureFolder("Assets/Resources/Menu");

            MainMenuTableDecorLibrary library =
                AssetDatabase.LoadAssetAtPath<MainMenuTableDecorLibrary>(LibraryAssetPath);
            if (library == null)
            {
                library = CreatePopulatedLibrary();
                AssetDatabase.CreateAsset(library, LibraryAssetPath);
            }
            else
            {
                PopulateLibrary(library);
            }

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MainMenuTableDecorSetup] Table decor library ready at " + LibraryAssetPath);
        }

        public static MainMenuTableDecorLibrary CreatePopulatedLibrary()
        {
            MainMenuTableDecorLibrary library = ScriptableObject.CreateInstance<MainMenuTableDecorLibrary>();
            PopulateLibrary(library);
            return library;
        }

        public static void PopulateLibrary(MainMenuTableDecorLibrary library)
        {
            library.fruitBasket = LoadPrefab(MainMenuMarketDecorPaths.LoafBasket);
            library.breadBasket = LoadPrefab(MainMenuMarketDecorPaths.BreadBasket);
            library.basketSmall = LoadPrefab(MainMenuMarketDecorPaths.Basket01);
            library.basket = LoadPrefab(MainMenuMarketDecorPaths.Basket02);
            library.eggsBasket = LoadPrefab(MainMenuMarketDecorPaths.EggsBasket);
            library.appleBox = LoadPrefab(MainMenuMarketDecorPaths.AppleBox);

            library.appleRed = LoadPrefab(MainMenuMarketDecorPaths.AppleRed);
            library.appleGreen = LoadPrefab(MainMenuMarketDecorPaths.AppleGreen);
            library.appleYellow = LoadPrefab(MainMenuMarketDecorPaths.AppleYellow);
            library.beet = LoadPrefab(MainMenuMarketDecorPaths.Beet);
            library.tomato = LoadPrefab(MainMenuMarketDecorPaths.Tomato);
            library.carrot = LoadPrefab(MainMenuMarketDecorPaths.Carrot);
            library.banana = LoadPrefab(MainMenuMarketDecorPaths.Banana);
            library.watermelon = LoadPrefab(MainMenuMarketDecorPaths.Watermelon);

            library.woodenSign = LoadPrefab(MainMenuMarketDecorPaths.WoodenSign);
            library.woodenSignAlt = LoadPrefab(MainMenuMarketDecorPaths.WoodenSignAlt);
            library.lantern = LoadPrefab(MainMenuMarketDecorPaths.Lantern);

            library.woodenCrate = LoadPrefab(MainMenuMarketDecorPaths.WoodenCrate);
            library.sack = LoadPrefab(MainMenuMarketDecorPaths.Sack);
            library.sackOpen = LoadPrefab(MainMenuMarketDecorPaths.SackOpen);
            library.woodenBowl = LoadPrefab(MainMenuMarketDecorPaths.WoodenBowl);
            library.potato = LoadPrefab(MainMenuMarketDecorPaths.Potato);

            library.roundBread = LoadPrefab(MainMenuMarketDecorPaths.RoundBread);
            library.cookie = LoadPrefab(MainMenuMarketDecorPaths.Cookie);
            library.pretzel = LoadPrefab(MainMenuMarketDecorPaths.Pretzel);
            library.roundCheese = LoadPrefab(MainMenuMarketDecorPaths.RoundCheese);
        }

        private static GameObject LoadPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("[MainMenuTableDecorSetup] Missing prefab: " + path);
            }

            return prefab;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
