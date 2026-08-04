using ChoTet.Level1.Highway;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Editor
{
    public static class Level1HighwayRoadPatcher
    {
        public static void PatchHighwayRoad()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1HighwayRoadPatcher] Exit Play mode before patching.");
                return;
            }

            Scene scene = EditorSceneManager.GetSceneByPath(Level1HighwaySceneBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(Level1HighwaySceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            HighwayRoadSegmentPool pool = Object.FindFirstObjectByType<HighwayRoadSegmentPool>();
            if (pool == null)
            {
                Debug.LogError("[Level1HighwayRoadPatcher] HighwayRoadSegmentPool not found in scene.");
                return;
            }

            SerializedObject so = new SerializedObject(pool);
            so.FindProperty("runtimeRoadSurface").objectReferenceValue =
                HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.RoadSurfaceFile);
            so.FindProperty("runtimeLaneMark").objectReferenceValue =
                HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.LaneMarkFile);
            so.FindProperty("runtimeShoulder").objectReferenceValue =
                HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.ShoulderFile);
            so.FindProperty("runtimeEdgeMark").objectReferenceValue =
                HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.EdgeMarkFile);
            so.ApplyModifiedPropertiesWithoutUndo();

            pool.RebuildRoadContent();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Level1HighwayRoadPatcher] Highway road patched — 3-lane deck, dividers at ±2.6, lane centers at -5.2 / 0 / +5.2.");
        }
    }
}
