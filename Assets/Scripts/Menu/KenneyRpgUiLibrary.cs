using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Sprite references for Kenney RPG UI widgets. Populated by the editor setup tool.
    /// </summary>
    [CreateAssetMenu(fileName = "KenneyRpgUiLibrary", menuName = "Cho Tet/Menu/Kenney RPG UI Library")]
    public class KenneyRpgUiLibrary : ScriptableObject
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/KenneyRpgUiLibrary.asset";

        [Header("Buttons")]
        public Sprite buttonLongBrown;
        public Sprite buttonLongBrownPressed;
        public Sprite buttonLongBeige;
        public Sprite buttonLongBeigePressed;
        public Sprite buttonRoundBrown;
        public Sprite buttonSquareBrown;
        public Sprite buttonSquareBrownPressed;

        [Header("Background")]
        public Texture2D woodPlankBackground;
        public Texture2D menuVillageBackground;

        [Header("Panels")]
        public Sprite panelBrown;
        public Sprite panelBeige;
        public Sprite panelBeigeLight;
        public Sprite panelInsetBeige;
        public Sprite panelInsetBeigeLight;
        public Sprite panelInsetBrown;

        [Header("Accents")]
        public Sprite barYellowMid;
        public Sprite barYellowVertical;
        public Sprite barRedHorizontal;
        public Sprite iconCrossBrown;

        [Header("Vine Border")]
        public Sprite barGreenHorizontalLeft;
        public Sprite barGreenHorizontalMid;
        public Sprite barGreenHorizontalRight;
        public Sprite barGreenVerticalTop;
        public Sprite barGreenVerticalMid;
        public Sprite barGreenVerticalBottom;
        public Sprite vineLeaf;

        public static KenneyRpgUiLibrary LoadDefault()
        {
            KenneyRpgUiLibrary library = Resources.Load<KenneyRpgUiLibrary>(KenneyRpgUiPaths.LibraryResource);
#if UNITY_EDITOR
            if (library == null)
            {
                library = AssetDatabase.LoadAssetAtPath<KenneyRpgUiLibrary>(LibraryAssetPath);
            }
#endif
            if (library == null)
            {
                Debug.LogError("[KenneyRpgUiLibrary] Missing Resources asset at Menu/KenneyRpgUiLibrary.");
            }

            return library;
        }
    }
}
