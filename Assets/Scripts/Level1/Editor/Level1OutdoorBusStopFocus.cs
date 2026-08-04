#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Editor shortcuts to preview and play-test the outdoor bus pickup area.</summary>
    internal static class Level1OutdoorBusStopFocus
    {
        public static void SwitchToOutdoorEditMode()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorBusStopFocus] Exit Play mode before switching edit mode.");
                return;
            }

            if (!EnsurePackingRoomScene())
            {
                return;
            }

            ApplyEditMode(outdoorActive: true);
            Debug.Log("[Level1OutdoorBusStopFocus] Scene switched to outdoor edit mode.");
        }

        public static void SwitchToIndoorEditMode()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorBusStopFocus] Exit Play mode before switching edit mode.");
                return;
            }

            if (!EnsurePackingRoomScene())
            {
                return;
            }

            ApplyEditMode(outdoorActive: false);
            Debug.Log("[Level1OutdoorBusStopFocus] Scene switched to indoor edit mode.");
        }

        public static void RebuildBusStopCluster()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorBusStopFocus] Exit Play mode before rebuilding the bus stop cluster.");
                return;
            }

            if (!EnsurePackingRoomScene())
            {
                return;
            }

            Transform outdoor = Level1OutdoorSceneLookup.ResolveOutdoorRoot();
            Transform busArea = Level1OutdoorSceneLookup.ResolveBusPickupArea(outdoor);
            if (busArea == null)
            {
                Debug.LogError("[Level1OutdoorBusStopFocus] Level1_BusPickupArea not found.");
                return;
            }

            Level1VietnameseBusShelterBuilder.Build(busArea);
            if (outdoor != null)
            {
                Level1AlleyEntranceSignBuilder.Build(outdoor);
            }

            Level1OutdoorPathBoundaryBuilder.RebuildOnCurrentScene();
            Level1OutdoorWorkplaceNpcBuilder.RebuildAllOnCurrentScene();

            Vector3 curb = Level1BusStopRoadSettings.PickupCurbStopPosition(Level1PickupStopLayout.DefaultCurbOffset);
            Vector3 wait = Level1PickupStopLayout.BusStopWaitPosition;
            Debug.Log(
                $"[Level1OutdoorBusStopFocus] Bus stop cluster rebuilt. shelter=({Level1BusStopRoadSettings.BusShelterCenterX:F2}, {Level1BusStopRoadSettings.BusShelterCenterZ:F2}) " +
                $"wait=({wait.x:F2}, {wait.y:F2}, {wait.z:F2}) curb=({curb.x:F2}, {curb.y:F2}, {curb.z:F2})");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            FrameBusStop();
        }

        public static void FocusInEditor()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorBusStopFocus] Exit Play mode before focusing the bus stop.");
                return;
            }

            if (!EnsurePackingRoomScene())
            {
                return;
            }

            SetOutdoorAreaActive(true);
            FrameBusStop();
            Selection.activeGameObject = FindBusStopRoot();
            Debug.Log("[Level1OutdoorBusStopFocus] Scene view focused on outdoor bus stop.");
        }

        public static void FrameOutdoorEditAngle()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorBusStopFocus] Exit Play mode before framing the scene.");
                return;
            }

            SetOutdoorAreaActive(true);
            FrameEditAngle();
            Debug.Log("[Level1OutdoorBusStopFocus] Scene view set to outdoor edit angle.");
        }

        internal static void FocusBusStopInEditor()
        {
            SetOutdoorAreaActive(true);
            FrameBusStop();
            Selection.activeGameObject = FindBusStopRoot();
        }

        public static void PlayFromBusStop()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            if (!EnsurePackingRoomScene())
            {
                return;
            }

            EditorPrefs.SetBool(Level1OutdoorBusStopPlayBootstrap.PlayFromBusStopKey, true);
            EditorApplication.isPlaying = true;
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
                Debug.LogError("[Level1OutdoorBusStopFocus] Level1_PackingRoom scene not found.");
                return false;
            }

            EditorSceneManager.OpenScene("Assets/Scenes/Level1_PackingRoom.unity");
            return true;
        }

        private static void SetOutdoorAreaActive(bool active)
        {
            Transform outdoor = Level1OutdoorSceneLookup.ResolveOutdoorRoot();
            if (outdoor != null)
            {
                outdoor.gameObject.SetActive(active);
            }
        }

        private static void ApplyEditMode(bool outdoorActive)
        {
            Transform world = ResolveWorldRoot();
            if (world == null)
            {
                Debug.LogError("[Level1OutdoorBusStopFocus] Level1World not found.");
                return;
            }

            SetChildActive(world, "Level1_OutdoorArea", outdoorActive);
            SetChildActive(world, "Level1_IndoorArea", !outdoorActive);
            SetChildActive(world, "Level1RoomEnvironmentV64BrightSkyHaze", !outdoorActive);
            SetChildActive(world, "IndoorCameraRig", !outdoorActive);

            if (outdoorActive)
            {
                EnableOutdoorLighting();
                FrameEditAngle();
                Transform outdoor = Level1OutdoorSceneLookup.ResolveOutdoorRoot();
                if (outdoor != null)
                {
                    Selection.activeGameObject = outdoor.gameObject;
                }
            }
            else
            {
                Transform indoor = world.Find("Level1_IndoorArea");
                if (indoor != null)
                {
                    Selection.activeGameObject = indoor.gameObject;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static Transform ResolveWorldRoot()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == "Level1World")
                {
                    return roots[i].transform;
                }
            }

            return GameObject.Find("Level1World")?.transform;
        }

        private static void SetChildActive(Transform parent, string childName, bool active)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(active);
            }
        }

        private static void EnableOutdoorLighting()
        {
            Transform outdoor = Level1OutdoorSceneLookup.ResolveOutdoorRoot();
            if (outdoor == null)
            {
                return;
            }

            Transform outdoorLighting = outdoor.Find("OutdoorLighting");
            if (outdoorLighting == null)
            {
                return;
            }

            outdoorLighting.gameObject.SetActive(true);
            for (int i = 0; i < outdoorLighting.childCount; i++)
            {
                outdoorLighting.GetChild(i).gameObject.SetActive(true);
            }
        }

        private static GameObject FindBusStopRoot()
        {
            Transform outdoor = Level1OutdoorSceneLookup.ResolveOutdoorRoot();
            if (outdoor == null)
            {
                return null;
            }

            Transform busStop = outdoor.Find("BusStopRoadLayout")
                ?? outdoor.Find("Level1_MainRoad/BusStopRoadLayout");
            return busStop != null ? busStop.gameObject : outdoor.gameObject;
        }

        private static void FrameBusStop()
        {
            Vector3 focus = new Vector3(
                Level1BusStopRoadSettings.BusShelterCenterX,
                Level1OutdoorSettings.WalkSurfaceY + 1.1f,
                Level1BusStopRoadSettings.BusShelterCenterZ);

            ApplySceneView(focus, new Vector3(28f, 0f, 0f), 14f);
        }

        private static void FrameEditAngle()
        {
            Vector3 focus = new Vector3(
                Level1BusStopRoadSettings.ComTamShopCenterX + 2.5f,
                Level1OutdoorSettings.WalkSurfaceY + 1.35f,
                Level1BusStopRoadSettings.BusShelterCenterZ);

            // 3/4 view: Com Tam row, bus shelter, and highway approach.
            ApplySceneView(focus, new Vector3(34f, 42f, 0f), 22f);
        }

        private static void ApplySceneView(Vector3 focus, Vector3 rotationEuler, float distance)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
            {
                sceneView = SceneView.sceneViews[0] as SceneView;
            }

            if (sceneView == null)
            {
                return;
            }

            sceneView.in2DMode = false;
            sceneView.orthographic = false;
            sceneView.LookAt(focus, Quaternion.Euler(rotationEuler), distance);
            sceneView.Repaint();
        }
    }
}
#endif
