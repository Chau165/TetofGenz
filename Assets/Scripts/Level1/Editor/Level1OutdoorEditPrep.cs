#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Prepares the outdoor boarding-house scene for direct Scene View editing.</summary>
    internal static class Level1OutdoorEditPrep
    {
        private const float HouseExitFocusZ = -5.2f;

        public static void PrepareForEditing()
        {
            ShowOutdoorForEditing(focusBusStop: false);
        }

        public static void BuildOutdoorForUiEditing()
        {
            ShowOutdoorForEditing(focusBusStop: false);
        }

        public static void RebuildOutdoorFromScratch()
        {
            BuildAndShowForEditing(focusBusStop: false);
        }

        private static void ShowOutdoorForEditing(bool focusBusStop)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorEditPrep] Exit Play mode first, then run this again.");
                EditorApplication.isPlaying = false;
                return;
            }

            if (!EnsurePackingRoomScene())
            {
                return;
            }

            Level1OutdoorBusStopFocus.SwitchToOutdoorEditMode();

            if (focusBusStop)
            {
                Level1OutdoorBusStopFocus.FocusBusStopInEditor();
            }
            else
            {
                Level1OutdoorBusStopFocus.FrameOutdoorEditAngle();
            }

            Debug.Log("[Level1OutdoorEditPrep] Outdoor shown for editing — existing layout preserved. Use 'Rebuild Outdoor Area From Scratch' only when you want a fresh procedural build.");
        }

        private static void BuildAndShowForEditing(bool focusBusStop)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorEditPrep] Exit Play mode first, then run this again.");
                EditorApplication.isPlaying = false;
                return;
            }

            if (!EnsurePackingRoomScene())
            {
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.LogError("[Level1OutdoorEditPrep] Level1World not found.");
                return;
            }

            Transform outdoorSpawn = Level1OutdoorAreaBuilder.BuildOrUpdate(worldRoot.transform);
            Level1OutdoorAreaBuilder.WirePhaseController(outdoorSpawn);

            // Persist safe play defaults: indoor on, outdoor off.
            SetActiveByName("Level1_IndoorArea", true);
            SetActiveByName("Level1_OutdoorArea", false);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            // Editor session only — keep outdoor visible for layout / UI work.
            SetActiveByName("Level1_OutdoorArea", true);
            SetActiveByName("Level1_IndoorArea", false);

            if (focusBusStop)
            {
                Level1OutdoorBusStopFocus.FocusBusStopInEditor();
            }
            else
            {
                Level1OutdoorBusStopFocus.FrameOutdoorEditAngle();
            }

            Transform outdoor = worldRoot.transform.Find("Level1_OutdoorArea");
            if (outdoor != null)
            {
                Selection.activeGameObject = outdoor.gameObject;
            }

            Debug.Log("[Level1OutdoorEditPrep] Outdoor rebuilt and visible in Scene View. Play mode will auto-restore indoor.");
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayRestore()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            SetActiveByName("Level1_IndoorArea", true);
            SetActiveByName("Level1_OutdoorArea", false);
        }

        private static bool EnsurePackingRoomScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path.EndsWith("Level1_PackingRoom.unity"))
            {
                return true;
            }

            if (!System.IO.File.Exists("Assets/Scenes/Level1_PackingRoom.unity"))
            {
                Debug.LogError("[Level1OutdoorEditPrep] Level1_PackingRoom scene not found.");
                return false;
            }

            EditorSceneManager.OpenScene("Assets/Scenes/Level1_PackingRoom.unity");
            return true;
        }

        private static void SetActiveByName(string objectName, bool active)
        {
            Transform world = GameObject.Find("Level1World")?.transform;
            if (world == null)
            {
                return;
            }

            Transform target = world.Find(objectName);
            if (target != null)
            {
                target.gameObject.SetActive(active);
            }
        }

        private static void FrameHouseExit()
        {
            Vector3 focus = new Vector3(0f, Level1OutdoorSettings.WalkSurfaceY + 1.4f, HouseExitFocusZ);
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
            {
                sceneView = SceneView.sceneViews[0] as SceneView;
            }

            if (sceneView != null)
            {
                sceneView.in2DMode = false;
                sceneView.LookAt(focus, Quaternion.Euler(22f, 0f, 0f), 11f);
                sceneView.Repaint();
            }
        }
    }
}
#endif
