#if UNITY_EDITOR
using System.IO;
using ChoTet.Menu.Editor;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Ensures the Level 1 packing room scene exists and is registered in Build Settings.
    /// </summary>
    [InitializeOnLoad]
    internal static class Level1SceneBootstrap
    {
        private const string SceneAssetPath = Level1SceneBuilder.SceneAssetPath;
        private static bool _pendingRebuildAfterPlay;

        static Level1SceneBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode || !_pendingRebuildAfterPlay)
            {
                return;
            }

            _pendingRebuildAfterPlay = false;
            EditorApplication.delayCall += EnsureSceneReady;
        }

        public static void EnsureSceneReady()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (NeedsInteriorRebuild())
                {
                    _pendingRebuildAfterPlay = true;
                    Debug.LogWarning("[Level1SceneBootstrap] Scene rebuild deferred — exit Play mode first.");
                }

                return;
            }

            if (!File.Exists(SceneAssetPath) || NeedsInteriorRebuild())
            {
                Debug.Log("[Level1SceneBootstrap] Level 1 scene missing or outdated. Building interior layout now...");
                Level1SceneBuilder.BuildLevel1PackingRoom();
                return;
            }

            if (!IsSceneInBuildSettings(SceneAssetPath))
            {
                Debug.Log("[Level1SceneBootstrap] Adding Level 1 scene to Build Settings.");
                ChoTetBuildSettings.ApplyMenuFlowScenes();
            }
        }

        private static bool NeedsInteriorRebuild()
        {
            if (!File.Exists(SceneAssetPath))
            {
                return true;
            }

            string text = File.ReadAllText(SceneAssetPath);
            if (!text.Contains(Level1SceneBuilder.EnvironmentVersionTag))
            {
                return true;
            }

            if (!text.Contains("IndoorHighAngleCamera"))
            {
                return true;
            }

            if (!text.Contains("Level1_OutdoorArea") || !text.Contains("Level1_OutdoorSpawn"))
            {
                return true;
            }

            if (!text.Contains("Level1_IndoorArea"))
            {
                return true;
            }

            if (!text.Contains("Level1_HouseExterior"))
            {
                return true;
            }

            if (!text.Contains("TopDownCameraOcclusion"))
            {
                return true;
            }

            if (!text.Contains("Level1PhaseController"))
            {
                return true;
            }

            if (text.Contains("InteriorOccluders") || text.Contains("WindowCountrysideView"))
            {
                return true;
            }

            return false;
        }

        private static bool IsSceneInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath && scenes[i].enabled)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
