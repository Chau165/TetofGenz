#if UNITY_EDITOR
using ChoTet.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Menu.Editor
{
    public static class LevelSelectSceneBuilder
    {
        internal const string ScenePath = ChoTetBuildSettings.LevelSelectScenePath;
        private const string LibraryAssetPath = "Assets/Resources/Menu/KenneyRpgUiLibrary.asset";

        public static void BuildLevelSelectMenu()
        {
            KenneyRpgUiSetup.SetupKenneyAssets();
            MainMenuTableDecorSetup.SetupTableDecorLibrary();

            KenneyRpgUiLibrary library = AssetDatabase.LoadAssetAtPath<KenneyRpgUiLibrary>(LibraryAssetPath);
            if (library == null)
            {
                Debug.LogError("[LevelSelectSceneBuilder] Kenney UI library was not created.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "LevelSelectMenu";

            GameObject bootstrap = new GameObject("LevelSelectBootstrap");
            KenneyRpgLevelSelectAssembler assembler = bootstrap.AddComponent<KenneyRpgLevelSelectAssembler>();
            SerializedObject so = new SerializedObject(assembler);
            so.FindProperty("library").objectReferenceValue = library;
            so.FindProperty("buildOnAwake").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Keep the saved scene as a minimal bootstrap. The complete level-select UI is
            // assembled by Awake in the player, avoiding a large editor-built UI hierarchy
            // that can produce an unreadable level1 scene in Windows builds.

            EditorSceneManager.SaveScene(scene, ScenePath);
            ChoTetBuildSettings.ApplyMenuFlowScenes();
            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("[LevelSelectSceneBuilder] Built level select menu at " + ScenePath);
        }
    }
}
#endif
