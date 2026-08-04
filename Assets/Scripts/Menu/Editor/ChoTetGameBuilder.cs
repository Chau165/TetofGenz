#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ChoTet.Menu.Editor
{
    /// <summary>Builds a Windows standalone player into the project's GameBuilder folder.</summary>
    public static class ChoTetGameBuilder
    {
        public const string OutputFolder = "GameBuilder";
        public const string ExeName = "TetHolidayAdventure.exe";

        [MenuItem("Tools/Tet Holiday/Build/Build Windows Test")]
        public static void BuildToGameBuilder()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[ChoTetGameBuilder] Exit Play mode before building.");
                return;
            }

            ChoTetBuildSettings.ApplyMenuFlowScenesWithHighway(includeHighway: true);

            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            string outputDir = Path.Combine(projectRoot, OutputFolder);
            Directory.CreateDirectory(outputDir);
            string exePath = Path.Combine(outputDir, ExeName);

            string[] scenes = GetEnabledBuildScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[ChoTetGameBuilder] No enabled scenes in Build Settings.");
                return;
            }

            Debug.Log("[ChoTetGameBuilder] Building " + scenes.Length + " scenes → " + exePath);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                // A standalone build must not reuse stale Player data after scene changes.
                options = BuildOptions.CleanBuildCache
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[ChoTetGameBuilder] SUCCESS → " + exePath + " (" + summary.totalSize + " bytes)");
                EditorUtility.RevealInFinder(exePath);
            }
            else
            {
                Debug.LogError("[ChoTetGameBuilder] FAILED: " + summary.result);
            }
        }

        private static string[] GetEnabledBuildScenes()
        {
            var list = new System.Collections.Generic.List<string>();
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < buildScenes.Length; i++)
            {
                if (buildScenes[i] != null && buildScenes[i].enabled && !string.IsNullOrEmpty(buildScenes[i].path))
                {
                    list.Add(buildScenes[i].path);
                }
            }

            return list.ToArray();
        }
    }
}
#endif
