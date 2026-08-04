using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    [CreateAssetMenu(fileName = "MainMenuBannerDecorationLibrary",
        menuName = "Cho Tet/Menu/Banner Decoration Library")]
    public class MainMenuBannerDecorationLibrary : ScriptableObject
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/MainMenuBannerDecorationLibrary.asset";

        [Header("Vines")]
        public GameObject stemA;
        public GameObject stemB;
        public GameObject stemC;
        public GameObject leafA;
        public GameObject leafB;
        public GameObject leafC;
        public GameObject edgeTuft;

        [Header("Flowers")]
        public GameObject blossomYellowA;
        public GameObject blossomYellowB;
        public GameObject blossomYellowC;
        public GameObject blossomRedA;
        public GameObject blossomRedB;
        public GameObject blossomPot;

        [Header("Festive Props")]
        public GameObject lantern;
        public GameObject lanternMarket;
        public GameObject coin;
        public GameObject coins;
        public GameObject appleRed;
        public GameObject appleYellow;

        public static MainMenuBannerDecorationLibrary LoadDefault()
        {
            MainMenuBannerDecorationLibrary library =
                Resources.Load<MainMenuBannerDecorationLibrary>(MainMenuBannerDecorationPaths.LibraryResource);
#if UNITY_EDITOR
            if (library == null)
            {
                library = AssetDatabase.LoadAssetAtPath<MainMenuBannerDecorationLibrary>(LibraryAssetPath);
            }
#endif
            if (library == null)
            {
                Debug.LogWarning("[MainMenuBannerDecorationLibrary] Missing library asset.");
            }

            return library;
        }
    }
}
