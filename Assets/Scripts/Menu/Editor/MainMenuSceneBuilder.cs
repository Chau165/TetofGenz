#if UNITY_EDITOR
using System.IO;
using ChoTet.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Menu.Editor
{
    public static class MainMenuSceneBuilder
    {
        internal const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string LibraryAssetPath = "Assets/Resources/Menu/KenneyRpgUiLibrary.asset";

        public static void BuildMainMenu()
        {
            KenneyRpgUiSetup.SetupKenneyAssets();
            MainMenuTableDecorSetup.SetupTableDecorLibrary();
            MainMenuBannerDecorationSetup.SetupBannerDecorationLibrary();

            KenneyRpgUiLibrary library = AssetDatabase.LoadAssetAtPath<KenneyRpgUiLibrary>(LibraryAssetPath);
            if (library == null)
            {
                Debug.LogError("[MainMenuSceneBuilder] Kenney UI library was not created.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            GameObject bootstrap = new GameObject("MainMenuBootstrap");
            KenneyRpgMenuAssembler assembler = bootstrap.AddComponent<KenneyRpgMenuAssembler>();
            SerializedObject so = new SerializedObject(assembler);
            so.FindProperty("library").objectReferenceValue = library;
            so.FindProperty("buildOnAwake").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            assembler.Build();

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("[MainMenuSceneBuilder] Built Tet market main menu at " + ScenePath);
        }

        private static void UpdateBuildSettings()
        {
            ChoTetBuildSettings.ApplyMenuFlowScenes();
        }
    }
}
#endif
