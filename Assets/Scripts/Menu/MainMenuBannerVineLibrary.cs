using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    [CreateAssetMenu(fileName = "MainMenuBannerVineLibrary", menuName = "Cho Tet/Menu/Banner Vine Library")]
    public class MainMenuBannerVineLibrary : ScriptableObject
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/MainMenuBannerVineLibrary.asset";

        public GameObject stemA;
        public GameObject stemB;
        public GameObject stemC;
        public GameObject leafTuftA;
        public GameObject leafTuftB;
        public GameObject leafTuftC;
        public GameObject edgeTuft;

        public static MainMenuBannerVineLibrary LoadDefault()
        {
            MainMenuBannerVineLibrary library =
                Resources.Load<MainMenuBannerVineLibrary>("Menu/MainMenuBannerVineLibrary");
#if UNITY_EDITOR
            if (library == null)
            {
                library = AssetDatabase.LoadAssetAtPath<MainMenuBannerVineLibrary>(LibraryAssetPath);
            }
#endif
            if (library == null)
            {
                Debug.LogWarning("[MainMenuBannerVineLibrary] Missing asset at Menu/MainMenuBannerVineLibrary.");
            }

            return library;
        }
    }
}
