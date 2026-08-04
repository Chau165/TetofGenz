#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Simple round wall clock for the Vietnamese boarding-room headboard wall (+X).
    /// Low-poly, flush-mounted, no pendulum or chain.
    /// </summary>
    internal static class Level1WallClockBuilder
    {
        private const string MaterialFolder = "Assets/Settings/ChoTet/Materials/Level1";
        private const string ClockRootName = "WallClock";

        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        private const float WallHalfThickness = 0.09f;
        private const float WallSurfaceInset = 0.01f;

        private const float ClockDiameter = 0.4f;
        private const float ClockFaceDepth = 0.012f;
        private const float ClockRimDepth = 0.016f;
        private const float RimThickness = 0.018f;
        private const float TargetCenterY = 2.0f;
        private const float OverlapClearance = 0.08f;

        private static readonly float[] RightWallCandidateZ =
        {
            3.55f, 1.45f, -0.35f, 3.95f, 0.75f, -1.55f
        };

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1WallClockBuilder] Exit Play mode before placing the wall clock.");
                return;
            }

            Transform interior = FindRentalRoomInterior();
            if (interior == null)
            {
                Debug.LogError("[Level1WallClockBuilder] RentalRoomInterior not found in the open scene.");
                return;
            }

            Build(interior, RoomHalfWidth, RoomHalfDepth, WallHalfThickness);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1WallClockBuilder] Wall clock placed on the headboard wall.");
        }

        public static void Build(Transform rentalRoomInterior, float roomHalfWidth, float roomHalfDepth,
            float wallHalfThickness)
        {
            if (rentalRoomInterior == null)
            {
                return;
            }

            Transform existing = rentalRoomInterior.Find(ClockRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            float rightWallInnerX = roomHalfWidth - wallHalfThickness;
            Material faceMat = EnsureMaterial("WallClock_Face_White", new Color(0.97f, 0.97f, 0.95f));
            Material rimMat = EnsureMaterial("WallClock_Rim_Dark", new Color(0.12f, 0.12f, 0.13f));
            Material handMat = EnsureMaterial("WallClock_Hands_Black", new Color(0.05f, 0.05f, 0.06f));

            GameObject clock = CreateClockMesh(faceMat, rimMat, handMat);
            clock.name = ClockRootName;
            clock.transform.SetParent(rentalRoomInterior, false);

            if (!TryPlaceOnRightWall(clock, rentalRoomInterior, rightWallInnerX, TargetCenterY))
            {
                Object.DestroyImmediate(clock);
                Debug.LogWarning("[Level1WallClockBuilder] Could not find a clear spot on the right wall.");
                return;
            }

            StripColliders(clock);
        }

        private static Transform FindRentalRoomInterior()
        {
            GameObject world = GameObject.Find("Level1World");
            if (world != null)
            {
                Transform indoor = world.transform.Find("Level1_IndoorArea/RentalRoomInterior");
                if (indoor != null)
                {
                    return indoor;
                }

                indoor = world.transform.Find("RentalRoomInterior");
                if (indoor != null)
                {
                    return indoor;
                }
            }

            Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "RentalRoomInterior")
                {
                    return all[i];
                }
            }

            return null;
        }

        private static GameObject CreateClockMesh(Material faceMat, Material rimMat, Material handMat)
        {
            GameObject root = new GameObject(ClockRootName);
            root.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            float radius = ClockDiameter * 0.5f;
            float faceRadius = radius - RimThickness * 0.55f;

            CreateDisc("Clock_Rim", root.transform, radius, ClockRimDepth, rimMat, 0f);
            CreateDisc("Clock_Face", root.transform, faceRadius, ClockFaceDepth, faceMat, ClockRimDepth * 0.35f);
            CreateTickMarks(root.transform, faceRadius * 0.9f, handMat, ClockFaceDepth * 0.65f);
            // 10:10 pose — both hands pivot at center, open slightly (no X crossing).
            CreateHand("HourHand", root.transform, faceRadius * 0.42f, 0.014f, handMat, -55f,
                ClockFaceDepth * 0.75f);
            CreateHand("MinuteHand", root.transform, faceRadius * 0.62f, 0.01f, handMat, -60f,
                ClockFaceDepth * 0.82f);
            CreateDisc("CenterPin", root.transform, 0.012f, 0.01f, handMat, ClockFaceDepth * 0.9f);

            return root;
        }

        private static void CreateDisc(string name, Transform parent, float radius, float depth, Material material,
            float localZ)
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            disc.transform.localPosition = new Vector3(0f, 0f, localZ);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.transform.localScale = new Vector3(radius * 2f, depth * 0.5f, radius * 2f);
            disc.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateTickMarks(Transform parent, float tickRadius, Material material, float localZ)
        {
            GameObject ticksRoot = new GameObject("TickMarks");
            ticksRoot.transform.SetParent(parent, false);
            ticksRoot.transform.localPosition = new Vector3(0f, 0f, localZ);

            for (int i = 0; i < 12; i++)
            {
                float angleDeg = i * 30f;
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f) * tickRadius;
                bool major = i % 3 == 0;
                float tickLen = major ? 0.022f : 0.014f;
                float tickThick = major ? 0.0045f : 0.003f;

                GameObject tick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tick.name = "Tick_" + i.ToString("00");
                tick.transform.SetParent(ticksRoot.transform, false);
                tick.transform.localPosition = offset;
                tick.transform.localRotation = Quaternion.Euler(0f, 0f, -angleDeg);
                tick.transform.localScale = new Vector3(tickThick, tickLen, tickThick * 0.8f);
                tick.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static void CreateHand(string name, Transform parent, float length, float thickness, Material material,
            float angleZ, float localZ)
        {
            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = new Vector3(0f, 0f, localZ);
            pivot.transform.localRotation = Quaternion.Euler(0f, 0f, angleZ);

            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hand.name = name + "_Mesh";
            hand.transform.SetParent(pivot.transform, false);
            hand.transform.localPosition = new Vector3(0f, length * 0.5f, 0f);
            hand.transform.localRotation = Quaternion.identity;
            hand.transform.localScale = new Vector3(thickness, length, thickness * 0.75f);
            hand.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static bool TryPlaceOnRightWall(GameObject clock, Transform interior, float rightWallInnerX,
            float targetY)
        {
            List<GameObject> blockers = CollectBlockers(interior);
            for (int i = 0; i < RightWallCandidateZ.Length; i++)
            {
                float candidateZ = RightWallCandidateZ[i];
                if (!TrySnapToWallSurface(clock, interior, rightWallInnerX, targetY, candidateZ))
                {
                    continue;
                }

                if (!HasOverlap(clock, blockers))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TrySnapToWallSurface(GameObject clock, Transform interior, float rightWallInnerX,
            float targetY, float candidateZ)
        {
            Vector3 castOrigin = new Vector3(rightWallInnerX - 0.75f, targetY, candidateZ);
            if (TryRaycastWall(castOrigin, Vector3.right, 1.5f, out RaycastHit hit))
            {
                clock.transform.position = hit.point;
                clock.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
                SnapClockBackToWall(clock, hit.point.x, WallSurfaceInset);
                return true;
            }

            clock.transform.position = new Vector3(rightWallInnerX - WallSurfaceInset, targetY, candidateZ);
            clock.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            SnapClockBackToWall(clock, rightWallInnerX, WallSurfaceInset);
            return true;
        }

        private static bool TryRaycastWall(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == null)
                {
                    continue;
                }

                string objectName = hits[i].collider.gameObject.name;
                if (objectName.IndexOf("Wall", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hit = hits[i];
                    return true;
                }
            }

            hit = default;
            return false;
        }

        private static void SnapClockBackToWall(GameObject clock, float wallInnerX, float inset)
        {
            if (!Level1ScenePlacement.TryGetWorldBounds(clock, out Bounds bounds))
            {
                return;
            }

            float deltaX = (wallInnerX - inset) - bounds.max.x;
            clock.transform.position += new Vector3(deltaX, 0f, 0f);
        }

        private static List<GameObject> CollectBlockers(Transform interior)
        {
            List<GameObject> blockers = new List<GameObject>();
            Transform worldRoot = interior.root;
            AddIfFound(blockers, worldRoot, "SleepingArea/Bed");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/SleepingArea/Bed");
            AddIfFound(blockers, interior, "PortraitWall");
            AddIfFound(blockers, interior, "StudyWindow");
            AddIfFound(blockers, worldRoot, "StudyArea/Desk");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/StudyArea/Desk");
            AddIfFound(blockers, worldRoot, "StorageArea/Wardrobe");
            AddIfFound(blockers, worldRoot, "StorageArea/Shelf");
            AddIfFound(blockers, worldRoot, "RoomPersonality/CornerPlant");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/RoomPersonality/CornerPlant");
            AddIfFound(blockers, worldRoot, "SleepingArea/FPT_WallHanger");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/SleepingArea/FPT_WallHanger");

            Transform sleeping = interior.Find("SleepingArea") ?? worldRoot.Find("Level1_IndoorArea/SleepingArea");
            if (sleeping != null)
            {
                for (int i = 0; i < sleeping.childCount; i++)
                {
                    GameObject child = sleeping.GetChild(i).gameObject;
                    if (child.name.IndexOf("Bed", System.StringComparison.OrdinalIgnoreCase) >= 0
                        || child.name.IndexOf("Hanger", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        blockers.Add(child);
                    }
                }
            }

            return blockers;
        }

        private static void AddIfFound(List<GameObject> list, Transform root, string path)
        {
            Transform found = Level1ScenePlacement.FindInRoom(root, path) ?? root.Find(path);
            if (found != null)
            {
                list.Add(found.gameObject);
            }
        }

        private static bool HasOverlap(GameObject clock, List<GameObject> blockers)
        {
            if (!Level1ScenePlacement.TryGetWorldBounds(clock, out Bounds clockBounds))
            {
                return false;
            }

            for (int i = 0; i < blockers.Count; i++)
            {
                GameObject blocker = blockers[i];
                if (blocker == null || blocker == clock
                    || !Level1ScenePlacement.TryGetWorldBounds(blocker, out Bounds blockerBounds))
                {
                    continue;
                }

                if (Overlaps3D(clockBounds, blockerBounds, OverlapClearance))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Overlaps3D(Bounds a, Bounds b, float clearance)
        {
            return a.min.x - clearance < b.max.x
                && a.max.x + clearance > b.min.x
                && a.min.y - clearance < b.max.y
                && a.max.y + clearance > b.min.y
                && a.min.z - clearance < b.max.z
                && a.max.z + clearance > b.min.z;
        }

        private static Material EnsureMaterial(string materialName, Color color)
        {
            string path = MaterialFolder + "/" + materialName + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                mat = new Material(shader);
                mat.name = materialName;
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            mat.SetFloat("_Smoothness", 0.2f);
            mat.SetFloat("_Glossiness", 0.2f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
            }
        }
    }
}
#endif
