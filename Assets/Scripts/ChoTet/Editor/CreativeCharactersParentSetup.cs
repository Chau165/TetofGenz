#if UNITY_EDITOR
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Editor
{
    public static class CreativeCharactersParentSetup
    {
        private static bool _isBuilding;
        private static double _lastBuildFinishedAt = -999d;
        private const double MinBuildIntervalSeconds = 1.5d;

        [MenuItem("Tools/ChoTet/Setup Parent Characters (Father + Mother)")]
        public static void SetupParentCharacters()
        {
            SetupParentCharacters(true);
        }

        public static void SetupParentCharacters(bool showDialog)
        {
            if (_isBuilding)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastBuildFinishedAt < MinBuildIntervalSeconds)
            {
                return;
            }

            _isBuilding = true;
            try
            {
                if (!RunParentBuild(showDialog))
                {
                    return;
                }

                AssetDatabase.SaveAssets();

                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        "Creative Characters FREE",
                        "Đã tạo prefab bố/mẹ:\n"
                        + PlayerCharacterVisualPaths.FatherVisualPrefab + "\n"
                        + PlayerCharacterVisualPaths.MotherVisualPrefab
                    + "\n\nBố: áo khoác + quần + râu. Mẹ: tóc dài + áo hồng + jean."
                    + "\n\nHọ đứng sẵn trên lề tại điểm dừng xe, không theo xe chạy.",
                        "OK");
                }
                else
                {
                    Debug.Log(
                        "[CreativeCharacters] Parent prefabs saved: "
                        + PlayerCharacterVisualPaths.FatherVisualPrefab
                        + ", "
                        + PlayerCharacterVisualPaths.MotherVisualPrefab);
                }
            }
            finally
            {
                _lastBuildFinishedAt = EditorApplication.timeSinceStartup;
                _isBuilding = false;
            }
        }

        private static bool RunParentBuild(bool showDialog)
        {
            if (!CreativeCharactersPlayerSetup.TryFindPackRootForParents(out _))
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        "Creative Characters FREE",
                        "Pack chưa import. Import Creative Characters FREE trước.",
                        "OK");
                }

                return false;
            }

            CreativeCharactersPlayerSetup.EnsureFolder(PlayerCharacterVisualPaths.CreativeCharacterRoot);
            CreativeCharactersPlayerSetup.EnsureFolder(PlayerCharacterVisualPaths.CreativeCharacterMaterialsRoot);

            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                PlayerCharacterVisualPaths.CreativeCharacterTopDownController);
            if (controller == null)
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        "Creative Characters FREE",
                        "Chưa có animator. Chạy Tools/ChoTet/Setup Creative Character Player trước.",
                        "OK");
                }

                return false;
            }

            if (!TrySaveParentPrefab(
                    PlayerCharacterVisualPaths.MotherVisualPrefab,
                    CreativeCharacterAssembly.ApplyMotherLook,
                    "CreativeCharacter_MotherVisual",
                    controller,
                    showDialog))
            {
                return false;
            }

            if (!TrySaveParentPrefab(
                    PlayerCharacterVisualPaths.FatherVisualPrefab,
                    CreativeCharacterAssembly.ApplyFatherLook,
                    "CreativeCharacter_FatherVisual",
                    controller,
                    showDialog))
            {
                return false;
            }

            return true;
        }

        private static bool TrySaveParentPrefab(
            string prefabPath,
            System.Action<ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.Character.CustomizableCharacter> applyLook,
            string instanceName,
            RuntimeAnimatorController controller,
            bool showDialog)
        {
            GameObject instance = CreativeCharacterAssembly.BuildDressedCharacter(applyLook, instanceName);
            if (instance == null)
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        "Creative Characters FREE",
                        "Không tạo được nhân vật từ pack.",
                        "OK");
                }
                else
                {
                    Debug.LogWarning("[CreativeCharacters] Không tạo được nhân vật từ pack: " + instanceName);
                }

                return false;
            }

            if (instanceName.Contains("Mother"))
            {
                CreativeCharacterAssembly.ApplyMotherAppearanceMaterials(instance);
            }

            CreativeCharacterAssembly.PrepareNpcInstance(instance, controller);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            return true;
        }
    }
}
#endif
