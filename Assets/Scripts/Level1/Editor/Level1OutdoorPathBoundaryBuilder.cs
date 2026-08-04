#if UNITY_EDITOR
using ChoTet.Level1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Rebuilds invisible alley movement walls and syncs outdoor camera bounds — no storefront rebuild.</summary>
    public static class Level1OutdoorPathBoundaryBuilder
    {
        private const string BoundariesRootName = "Level1_OutdoorPathBoundaries";
        private const float PathWallHeight = 2.6f;

        private static Transform ResolveOutdoorRoot()
        {
            GameObject found = GameObject.Find("Level1_OutdoorArea");
            if (found != null)
            {
                return found.transform;
            }

            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform deep = FindDeep(roots[i].transform, "Level1_OutdoorArea");
                if (deep != null)
                {
                    return deep;
                }
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
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

        public static void RebuildOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorPathBoundaryBuilder] Exit Play mode before rebuilding movement bounds.");
                return;
            }

            Transform outdoor = ResolveOutdoorRoot();
            if (outdoor == null)
            {
                Debug.LogError("[Level1OutdoorPathBoundaryBuilder] Level1_OutdoorArea not found.");
                return;
            }

            Rebuild(outdoor);
            SyncPhaseControllerBounds();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Level1OutdoorPathBoundaryBuilder] Alley movement bounds + walk colliders rebuilt.");
        }

        public static void Rebuild(Transform outdoorRoot)
        {
            Level1OutdoorPathBoundaryBuilder.RebuildPathWalls(outdoorRoot);
            Level1OutdoorWalkCollisionBuilder.Rebuild(outdoorRoot);
        }

        internal static void RebuildPathWalls(Transform outdoorRoot)
        {
            Transform existing = outdoorRoot.Find(BoundariesRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject boundsRoot = new GameObject(BoundariesRootName);
            boundsRoot.transform.SetParent(outdoorRoot, false);

            float centerY = PathWallHeight * 0.5f;
            float spanCenterZ = Level1OutdoorMovementSettings.PlaySpanCenterZ;
            float spanDepth = Level1OutdoorMovementSettings.PlaySpanDepth;
            float halfWidth = Level1OutdoorMovementSettings.PlayHalfWidth;
            float gapMinZ = Level1OutdoorMovementSettings.BusStopBoundaryGapMinZ;
            float gapMaxZ = Level1OutdoorMovementSettings.BusStopBoundaryGapMaxZ;

            CreateInvisibleWall("PlayBoundaryLeft", boundsRoot.transform,
                new Vector3(Level1OutdoorMovementSettings.PlayMinX, centerY, spanCenterZ),
                new Vector3(0.28f, PathWallHeight, spanDepth));

            float northRightCenterZ = (Level1OutdoorMovementSettings.PlayStartZ + gapMaxZ) * 0.5f;
            float northRightDepth = Level1OutdoorMovementSettings.PlayStartZ - gapMaxZ + 0.35f;
            if (northRightDepth > 0.25f)
            {
                CreateInvisibleWall("PlayBoundaryRight_North", boundsRoot.transform,
                    new Vector3(Level1OutdoorMovementSettings.PlayMaxX, centerY, northRightCenterZ),
                    new Vector3(0.28f, PathWallHeight, northRightDepth));
            }

            float southRightCenterZ = (gapMinZ + Level1OutdoorMovementSettings.PlayEndZ) * 0.5f;
            float southRightDepth = gapMinZ - Level1OutdoorMovementSettings.PlayEndZ + 0.35f;
            if (southRightDepth > 0.25f)
            {
                CreateInvisibleWall("PlayBoundaryRight_South", boundsRoot.transform,
                    new Vector3(Level1OutdoorMovementSettings.PlayMaxX, centerY, southRightCenterZ),
                    new Vector3(0.28f, PathWallHeight, southRightDepth));
            }

            CreateInvisibleWall("PathEndWall", boundsRoot.transform,
                new Vector3(0f, centerY, Level1OutdoorMovementSettings.PlayEndZ),
                new Vector3(halfWidth * 2f, PathWallHeight, 0.35f));

            const float indoorOutdoorBoundaryZ = -3.95f;
            CreateInvisibleWall("HouseExitBlocker", boundsRoot.transform,
                new Vector3(0f, centerY, indoorOutdoorBoundaryZ + 0.28f),
                new Vector3(1.35f, PathWallHeight, 0.32f));
        }

        private static void SyncPhaseControllerBounds()
        {
            Level1PhaseController phase = Object.FindAnyObjectByType<Level1PhaseController>();
            if (phase == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(phase);
            serialized.FindProperty("outdoorMinX").floatValue = Level1OutdoorMovementSettings.PlayMinX;
            serialized.FindProperty("outdoorMaxX").floatValue = Level1OutdoorMovementSettings.OutdoorCameraMaxX;
            serialized.FindProperty("outdoorMinZ").floatValue = Level1OutdoorMovementSettings.OutdoorCameraMinZ;
            serialized.FindProperty("outdoorMaxZ").floatValue = Level1OutdoorMovementSettings.PlayStartZ + 1.2f;
            serialized.FindProperty("outdoorClampMargin").floatValue = Level1OutdoorMovementSettings.CameraClampMargin;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(phase);
        }

        private static void CreateInvisibleWall(string name, Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            Object.DestroyImmediate(wall.GetComponent<Renderer>());
        }
    }
}
#endif
