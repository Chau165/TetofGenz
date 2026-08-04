using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Prefab references for main-menu tabletop decorations.
    /// </summary>
    [CreateAssetMenu(fileName = "MainMenuTableDecorLibrary", menuName = "Cho Tet/Menu/Table Decor Library")]
    public class MainMenuTableDecorLibrary : ScriptableObject
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/MainMenuTableDecorLibrary.asset";

        [Header("Baskets")]
        public GameObject fruitBasket;
        public GameObject breadBasket;
        public GameObject basketSmall;
        public GameObject basket;
        public GameObject eggsBasket;
        public GameObject appleBox;

        [Header("Fruits")]
        public GameObject appleRed;
        public GameObject appleGreen;
        public GameObject appleYellow;
        public GameObject beet;
        public GameObject tomato;
        public GameObject carrot;
        public GameObject banana;
        public GameObject watermelon;

        [Header("Signs and Light")]
        public GameObject woodenSign;
        public GameObject woodenSignAlt;
        public GameObject lantern;

        [Header("Containers")]
        public GameObject woodenCrate;
        public GameObject sack;
        public GameObject sackOpen;
        public GameObject woodenBowl;
        public GameObject potato;

        [Header("Food")]
        public GameObject roundBread;
        public GameObject cookie;
        public GameObject pretzel;
        public GameObject roundCheese;

        public static MainMenuTableDecorLibrary LoadDefault()
        {
            MainMenuTableDecorLibrary library =
                Resources.Load<MainMenuTableDecorLibrary>(MainMenuMarketDecorPaths.LibraryResource);
            if (library != null)
            {
                return library;
            }

#if UNITY_EDITOR
            library = AssetDatabase.LoadAssetAtPath<MainMenuTableDecorLibrary>(LibraryAssetPath);
            if (library != null)
            {
                return library;
            }

            Debug.LogWarning("[MainMenuTableDecorLibrary] Missing library asset. Run Tools > Cho Tet > Setup Main Menu Table Decor.");
#else
            Debug.LogWarning("[MainMenuTableDecorLibrary] Missing Resources asset at Menu/MainMenuTableDecorLibrary.");
#endif
            return null;
        }
    }
}
