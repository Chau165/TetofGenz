#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Places the open packing suitcase at the centre of the rental room floor.
    /// </summary>
    internal static class Level1SuitcaseBuilder
    {
        private const string MaterialFolder = "Assets/Settings/ChoTet/Materials/Level1";
        private const string ZoneName = "PackingArea";
        private const string SuitcaseName = "OpenSuitcase";
        private const float SuitcaseYaw = 0f;
        private static readonly Vector3 SuitcaseScale = new(2f, 2f, 2f);
        private const float FloorLift = 0.002f;
        private const float SuitcaseClearance = 0.55f;
        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        private const float RoomEdgeMargin = 0.85f;

        private static readonly Vector3[] PlacementCandidates =
        {
            new(1.35f, 0f, -0.25f),
            new(1.65f, 0f, 0.05f),
            new(1.05f, 0f, -0.65f),
            new(1.85f, 0f, -0.45f),
            new(0.75f, 0f, -0.95f),
        };

        public static void Place(Transform worldRoot, Vector3 floorPosition)
        {
            if (worldRoot == null)
            {
                return;
            }

            Transform indoor = Level1ScenePlacement.FindInRoom(worldRoot, "Level1_IndoorArea")
                ?? worldRoot;
            Transform zone = indoor.Find(ZoneName);
            if (zone == null)
            {
                GameObject zoneGo = new GameObject(ZoneName);
                zoneGo.transform.SetParent(indoor, false);
                zone = zoneGo.transform;
            }

            DestroyExisting(zone);

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(Level1RoomAssetPaths.OpenSuitcaseFbx);
            if (source == null)
            {
                Debug.LogWarning("[Level1SuitcaseBuilder] Missing FBX at " + Level1RoomAssetPaths.OpenSuitcaseFbx
                    + ". Run Blender script CreateOpenSuitcase.py first.");
                return;
            }

            GameObject suitcase = (GameObject)PrefabUtility.InstantiatePrefab(source, zone);
            suitcase.name = SuitcaseName;
            suitcase.transform.localScale = SuitcaseScale;
            suitcase.transform.rotation = Quaternion.Euler(0f, SuitcaseYaw, 0f);

            ApplyMaterials(suitcase);
            StripColliders(suitcase);
            PlaceWithoutOverlap(worldRoot, suitcase, floorPosition);
            SnapBottomToFloor(suitcase, floorPosition.y);
            FitInteractionCollider(suitcase);

            Debug.Log("[Level1SuitcaseBuilder] Open suitcase placed at " + suitcase.transform.position
                + " scale=" + suitcase.transform.localScale);
        }

        private static void PlaceWithoutOverlap(Transform worldRoot, GameObject suitcase, Vector3 preferred)
        {
            Vector3[] candidates = BuildCandidates(preferred);
            float floorY = preferred.y;

            for (int i = 0; i < candidates.Length; i++)
            {
                Vector3 candidate = candidates[i];
                candidate.y = floorY;
                suitcase.transform.position = candidate;
                SnapBottomToFloor(suitcase, floorY);
                ResolveClearance(worldRoot, suitcase);
                Level1ScenePlacement.ClampInsideRoom(suitcase, RoomHalfWidth, RoomHalfDepth, RoomEdgeMargin);
                SnapBottomToFloor(suitcase, floorY);

                if (!HasBlockerOverlap(worldRoot, suitcase))
                {
                    return;
                }
            }
        }

        private static Vector3[] BuildCandidates(Vector3 preferred)
        {
            Vector3[] merged = new Vector3[PlacementCandidates.Length + 1];
            merged[0] = preferred;
            for (int i = 0; i < PlacementCandidates.Length; i++)
            {
                merged[i + 1] = PlacementCandidates[i];
            }

            return merged;
        }

        private static bool HasBlockerOverlap(Transform worldRoot, GameObject suitcase)
        {
            string[] blockers =
            {
                "StudyArea/DeskChair",
                "StudyArea/Desk",
                "SleepingArea/Bed",
                "RoomPersonality/CornerPlant"
            };

            for (int i = 0; i < blockers.Length; i++)
            {
                GameObject blocker = Level1ScenePlacement.FindInRoomObject(worldRoot, blockers[i]);
                if (blocker != null
                    && Level1ScenePlacement.OverlapsXZ(suitcase, blocker, SuitcaseClearance * 0.65f))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SnapBottomToFloor(GameObject suitcase, float floorY)
        {
            if (suitcase == null)
            {
                return;
            }

            Transform bottomShell = suitcase.transform.Find("Suitcase_BottomShell");
            if (bottomShell != null
                && bottomShell.TryGetComponent<Renderer>(out Renderer bottomRenderer))
            {
                float delta = floorY + FloorLift - bottomRenderer.bounds.min.y;
                if (Mathf.Abs(delta) > 0.0005f)
                {
                    suitcase.transform.position += new Vector3(0f, delta, 0f);
                }

                return;
            }

            Renderer[] baseParts = suitcase.GetComponentsInChildren<Renderer>(true);
            float minY = float.MaxValue;
            for (int i = 0; i < baseParts.Length; i++)
            {
                string n = baseParts[i].gameObject.name;
                if (!n.Contains("BottomShell") && !n.Contains("Wheel"))
                {
                    continue;
                }

                minY = Mathf.Min(minY, baseParts[i].bounds.min.y);
            }

            if (minY < float.MaxValue)
            {
                float delta = floorY + FloorLift - minY;
                if (Mathf.Abs(delta) > 0.0005f)
                {
                    suitcase.transform.position += new Vector3(0f, delta, 0f);
                }
            }
        }

        private static void ResolveClearance(Transform worldRoot, GameObject suitcase)
        {
            if (worldRoot == null || suitcase == null)
            {
                return;
            }

            string[] blockers =
            {
                "StudyArea/DeskChair",
                "StudyArea/Desk",
                "SleepingArea/Bed",
                "RoomPersonality/CornerPlant"
            };

            for (int pass = 0; pass < 6; pass++)
            {
                for (int i = 0; i < blockers.Length; i++)
                {
                    GameObject blocker = Level1ScenePlacement.FindInRoomObject(worldRoot, blockers[i]);
                    if (blocker != null)
                    {
                        Level1ScenePlacement.EnsureXZClearance(suitcase, blocker, SuitcaseClearance);
                    }
                }
            }
        }

        private static void DestroyExisting(Transform zone)
        {
            Transform existing = zone.Find(SuitcaseName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void ApplyMaterials(GameObject root)
        {
            Material body = EnsureMaterial("Suitcase_Body.mat", new Color(0.12f, 0.18f, 0.30f), 0.46f);
            Material interior = EnsureMaterial("Suitcase_Interior.mat", new Color(0.74f, 0.71f, 0.66f), 0.84f);
            Material accent = EnsureMaterial("Suitcase_Accent.mat", new Color(0.10f, 0.10f, 0.11f), 0.32f);
            Material shirtBlue = EnsureMaterial("Cloth_ShirtBlue.mat", new Color(0.55f, 0.68f, 0.82f), 0.15f);
            Material shirtCream = EnsureMaterial("Cloth_ShirtCream.mat", new Color(0.92f, 0.86f, 0.74f), 0.15f);
            Material pants = EnsureMaterial("Cloth_Pants.mat", new Color(0.35f, 0.42f, 0.52f), 0.14f);
            Material towel = EnsureMaterial("Cloth_Towel.mat", new Color(0.88f, 0.62f, 0.62f), 0.12f);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                string objectName = renderer.gameObject.name;
                Material target = body;

                if (objectName.Contains("Interior") || objectName.Contains("Liner"))
                {
                    target = interior;
                }
                else if (objectName.Contains("Wheel") || objectName.Contains("Handle") || objectName.Contains("Hinge"))
                {
                    target = accent;
                }
                else if (objectName.Contains("Cloth_Shirt_1"))
                {
                    target = shirtBlue;
                }
                else if (objectName.Contains("Cloth_Shirt_2"))
                {
                    target = shirtCream;
                }
                else if (objectName.Contains("Cloth_Pants"))
                {
                    target = pants;
                }
                else if (objectName.Contains("Cloth_Towel"))
                {
                    target = towel;
                }

                Material[] slots = renderer.sharedMaterials;
                for (int s = 0; s < slots.Length; s++)
                {
                    slots[s] = target;
                }

                renderer.sharedMaterials = slots;
            }
        }

        private static Material EnsureMaterial(string fileName, Color color, float smoothness)
        {
            string path = MaterialFolder + "/" + fileName;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                return mat;
            }

            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = fileName.Replace(".mat", string.Empty);
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", 0.05f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void FitInteractionCollider(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            BoxCollider box = root.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = root.AddComponent<BoxCollider>();
            }

            Vector3 localCenter = root.transform.InverseTransformPoint(bounds.center);
            Vector3 lossy = root.transform.lossyScale;
            box.center = localCenter;
            box.size = new Vector3(
                bounds.size.x / Mathf.Max(lossy.x, 0.001f),
                bounds.size.y / Mathf.Max(lossy.y, 0.001f),
                bounds.size.z / Mathf.Max(lossy.z, 0.001f));
        }
    }
}
#endif
