#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ChoTet.Level1.Editor;
using ChoTet.Editor;

namespace ChoTet.Menu.Editor
{
    public static class ChoTetBuildSettings
    {
        public const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        public const string LevelSelectScenePath = "Assets/Scenes/LevelSelectMenu.unity";
        public const string Level2ScenePath = "Assets/Scenes/ChoTet_VerticalSlice.unity";
        public const string Level1ScenePath = "Assets/Scenes/Level1_PackingRoom.unity";
        public const string Level1HighwayScenePath = "Assets/Scenes/Level1_Highway.unity";
        public const string Level1HometownArrivalScenePath = "Assets/Scenes/Level1_HometownArrival.unity";

        public static void RebuildAllScenesAndPlay()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[ChoTet] Exit Play mode before rebuilding all scenes.");
                return;
            }

            Debug.Log("[ChoTet] Rebuilding ALL scenes...");

            // 1. Build Main Menu
            MainMenuSceneBuilder.BuildMainMenu();

            // 2. Build Level Select
            LevelSelectSceneBuilder.BuildLevelSelectMenu();

            // 3. Build Level 1 Packing Room
            Level1SceneBuilder.BuildLevel1PackingRoom();

            // 4. Build Level 1 Highway
            Level1HighwaySceneBuilder.BuildHighwayScene();

            // 5. Build Level 2 Vertical Slice
            ChoTetSceneBuilder.BuildVerticalSlice();

            // 6. Build Level 1 Hometown Arrival (by cloning vertical slice and baking)
            string projectRoot = System.IO.Directory.GetParent(Application.dataPath).FullName;
            string source = System.IO.Path.Combine(projectRoot, "Assets/Scenes/ChoTet_VerticalSlice.unity");
            string destination = System.IO.Path.Combine(projectRoot, "Assets/Scenes/Level1_HometownArrival.unity");
            if (System.IO.File.Exists(source))
            {
                System.IO.File.Copy(source, destination, true);
                AssetDatabase.Refresh();
            }
            HometownArrivalSceneBuilder.BakeArrivalFrame();

            // 7. Refresh Build Settings and open Main Menu
            ApplyMenuFlowScenesWithHighway(includeHighway: true);
            EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            // 8. Start Play Mode
            EditorApplication.isPlaying = true;

            Debug.Log("[ChoTet] ALL scenes rebuilt and registered successfully! Starting Play mode...");
        }

        [MenuItem("Tools/Tet Holiday/Game Flow/2. Play From Main Menu")]
        public static void PlayFromMainMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EnsureMenuScenesExist();
            ApplyMenuFlowScenesWithHighway(includeHighway: true);

            SceneAsset mainMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
            if (mainMenu == null)
            {
                Debug.LogError("[ChoTet] Main Menu scene asset not found: " + MainMenuScenePath);
                return;
            }

            // Clear override so Play always uses Build Settings index 0 (Main Menu).
            EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Tet Holiday/Game Flow/3. Play Level 1 From Packing Room")]
        public static void PlayLevel1FromPackingRoom()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            ApplyMenuFlowScenesWithHighway(includeHighway: true);

            SceneAsset packingRoom = AssetDatabase.LoadAssetAtPath<SceneAsset>(Level1ScenePath);
            if (packingRoom == null)
            {
                Debug.LogError("[ChoTet] Packing Room scene asset not found: " + Level1ScenePath);
                return;
            }

            EditorSceneManager.playModeStartScene = packingRoom;
            EditorSceneManager.OpenScene(Level1ScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        /// <summary>Rebuild Main Menu + Level Select if missing, then register the full menu flow.</summary>
        [MenuItem("Tools/Tet Holiday/Game Flow/1. Configure Build Scene Order")]
        public static void BuildMenuFlow()
        {
            EnsureMenuScenesExist();
            ApplyMenuFlowScenesWithHighway(includeHighway: true);
            EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Debug.Log("[ChoTet] Build Settings start at Main Menu. Scenes: MainMenu → LevelSelect → Level1… → ChoTet.");
        }

        public static void BuildMenuFlowFromLevelSelect()
        {
            EnsureMenuScenesExist();
            LevelSelectSceneBuilder.BuildLevelSelectMenu();
            ApplyMenuFlowScenesWithHighway(includeHighway: true);
            EditorSceneManager.playModeStartScene = null;
            EditorSceneManager.OpenScene(LevelSelectScenePath, OpenSceneMode.Single);
            Debug.Log("[ChoTet] Rebuilt Level Select. Build Settings: MainMenu → LevelSelect → Level1… → ChoTet.");
        }

        public static void PlayFromLevelSelect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EnsureMenuScenesExist();
            ApplyMenuFlowScenesWithHighway(includeHighway: true);

            SceneAsset levelSelect = AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelSelectScenePath);
            if (levelSelect == null)
            {
                Debug.LogError("[ChoTet] Level Select scene asset not found: " + LevelSelectScenePath);
                return;
            }

            EditorSceneManager.playModeStartScene = levelSelect;
            EditorSceneManager.OpenScene(LevelSelectScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static void EnsureMenuScenesExist()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath) == null)
            {
                MainMenuSceneBuilder.BuildMainMenu();
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelSelectScenePath) == null)
            {
                LevelSelectSceneBuilder.BuildLevelSelectMenu();
            }
        }

        public static void ApplyMenuFlowScenes()
        {
            ApplyMenuFlowScenesWithHighway(includeHighway: true);
        }

        public static void ApplyMenuFlowScenesWithHighway(bool includeHighway = true)
        {
            if (includeHighway)
            {
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(MainMenuScenePath, true),
                    new EditorBuildSettingsScene(LevelSelectScenePath, true),
                    new EditorBuildSettingsScene(Level1ScenePath, true),
                    new EditorBuildSettingsScene(Level1HighwayScenePath, true),
                    new EditorBuildSettingsScene(Level1HometownArrivalScenePath, true),
                    new EditorBuildSettingsScene(Level2ScenePath, true)
                };
            }
            else
            {
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(MainMenuScenePath, true),
                    new EditorBuildSettingsScene(LevelSelectScenePath, true),
                    new EditorBuildSettingsScene(Level1ScenePath, true),
                    new EditorBuildSettingsScene(Level2ScenePath, true)
                };
            }
        }
    }
}
#endif
