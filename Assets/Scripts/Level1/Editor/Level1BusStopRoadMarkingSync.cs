#if UNITY_EDITOR
using ChoTet.Level1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Stretches bus-stop lane markings to match AsphaltRoadSurface length.</summary>
    public static class Level1BusStopRoadMarkingSync
    {
        private const string AsphaltName = "AsphaltRoadSurface";
        private const string MarkingsRootName = "LaneMarkings";
        private const string CenterDividerName = "CenterLaneDivider";
        private const float DashSpacing = 3.6f;
        private const float DashLengthX = 1.15f;
        private const float DashThicknessY = 0.02f;
        private const float DashWidthZ = 0.1f;

        public static void SyncOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1BusStopRoadMarkingSync] Exit Play mode before syncing lane markings.");
                return;
            }

            Transform asphalt = FindDeep(Level1OutdoorSceneLookup.ResolveOutdoorRoot(), AsphaltName);
            if (asphalt == null)
            {
                Debug.LogError("[Level1BusStopRoadMarkingSync] AsphaltRoadSurface not found.");
                return;
            }

            float roadLength = asphalt.localScale.x;
            float roadCenterX = asphalt.localPosition.x;
            float half = roadLength * 0.5f;
            float startX = roadCenterX - half + 0.15f;
            float endX = roadCenterX + half - 0.15f;
            float markY = Level1BusStopRoadSettings.AsphaltSurfaceY + 0.012f;
            float edgeY = Level1BusStopRoadSettings.AsphaltSurfaceY + 0.011f;

            Transform roadLayout = asphalt.parent;
            if (roadLayout == null)
            {
                Debug.LogError("[Level1BusStopRoadMarkingSync] BusStopRoadLayout parent not found.");
                return;
            }

            Material laneMark = AssetDatabase.LoadAssetAtPath<Material>(
                ChoTet.Runtime.ChoTetMarketEnvironment.MaterialsRoot + "/Wall_Light.mat");
            if (laneMark == null)
            {
                Debug.LogError("[Level1BusStopRoadMarkingSync] Wall_Light.mat not found.");
                return;
            }

            SyncSlab(roadLayout, CenterDividerName, roadCenterX, edgeY + 0.002f,
                Level1BusStopRoadSettings.RoadCenterZ,
                new Vector3(roadLength + 0.15f, 0.022f, Level1BusStopRoadSettings.CenterDividerWidth));

            Transform markings = roadLayout.Find(MarkingsRootName);
            if (markings == null)
            {
                GameObject marksObject = new GameObject(MarkingsRootName);
                markings = marksObject.transform;
                markings.SetParent(roadLayout, false);
            }

            for (int i = markings.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(markings.GetChild(i).gameObject);
            }

            float[] laneCenters =
            {
                Level1BusStopRoadSettings.LaneNorthCenterZ,
                Level1BusStopRoadSettings.LaneSouthCenterZ
            };

            int dashCount = Mathf.Max(4, Mathf.FloorToInt(roadLength / DashSpacing) + 1);
            for (int lane = 0; lane < laneCenters.Length; lane++)
            {
                for (int i = 0; i < dashCount; i++)
                {
                    float t = dashCount <= 1 ? 0.5f : i / (float)(dashCount - 1);
                    float x = Mathf.Lerp(startX, endX, t);
                    CreateMarkSlab(markings, "LaneDash_" + lane + "_" + i, laneMark,
                        new Vector3(x, markY, laneCenters[lane]),
                        new Vector3(DashLengthX, DashThicknessY, DashWidthZ));
                }
            }

            CreateMarkSlab(markings, "LaneEdgeN", laneMark,
                new Vector3(roadCenterX, edgeY, Level1BusStopRoadSettings.LaneNorthEdgeZ + 0.06f),
                new Vector3(roadLength + 0.2f, 0.018f, 0.06f));
            CreateMarkSlab(markings, "LaneEdgeS", laneMark,
                new Vector3(roadCenterX, edgeY, Level1BusStopRoadSettings.LaneSouthEdgeZ - 0.06f),
                new Vector3(roadLength + 0.2f, 0.018f, 0.06f));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(
                $"[Level1BusStopRoadMarkingSync] Synced lane markings to asphalt length {roadLength:F1}m ({dashCount} dashes per lane).");
        }

        private static void SyncSlab(Transform parent, string name, float x, float y, float z, Vector3 scale)
        {
            Transform slab = parent.Find(name);
            if (slab == null)
            {
                return;
            }

            slab.localPosition = new Vector3(x, y, z);
            slab.localScale = scale;
        }

        private static void CreateMarkSlab(Transform parent, string name, Material material, Vector3 position,
            Vector3 scale)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, false);
            slab.transform.position = position;
            slab.transform.localScale = scale;
            Object.DestroyImmediate(slab.GetComponent<Collider>());

            Renderer renderer = slab.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
