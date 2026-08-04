#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Indoor potted plant tucked into a rental-room wall corner.
    /// </summary>
    internal static class Level1CornerPlantBuilder
    {
        private const string PlantRootName = "CornerPlant";

        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        private const float WallHalfThickness = 0.09f;
        private const float WallInset = 0.38f;
        private const float FloorTopY = 0f;
        private const float FloorPropLift = 0.002f;
        private const float PlantFootprintRadius = 0.24f;
        private const float CornerInset = 0.1f;
        private const float OverlapClearance = 0.12f;
        private const float PlantUniformScale = 1.15f;

        private static readonly string[] PlantPrefabPaths =
        {
            Level1RoomAssetPaths.LowPolyOfficeProps + "/mesh_plant_a_01.fbx",
            Level1RoomAssetPaths.LowPolyOfficeProps + "/mesh_plant_b_01.fbx",
            Level1RoomAssetPaths.KenneyFbx + "/pottedPlant.fbx",
            Level1RoomAssetPaths.KenneyFbx + "/plantSmall2.fbx",
            "Assets/Home Decor/Prefab/plant_2.prefab"
        };

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1CornerPlantBuilder] Exit Play mode before placing the corner plant.");
                return;
            }

            Transform personality = FindRoomPersonality();
            if (personality == null)
            {
                Debug.LogError("[Level1CornerPlantBuilder] RoomPersonality not found in the open scene.");
                return;
            }

            Build(personality);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1CornerPlantBuilder] Corner plant updated.");
        }

        public static void Build(Transform roomPersonality)
        {
            if (roomPersonality == null)
            {
                return;
            }

            Transform existing = roomPersonality.Find(PlantRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject plantPrefab = LoadPlantPrefab();
            if (plantPrefab == null)
            {
                Debug.LogWarning("[Level1CornerPlantBuilder] No indoor plant prefab found.");
                return;
            }

            GameObject plantRoot = (GameObject)PrefabUtility.InstantiatePrefab(plantPrefab, roomPersonality);
            plantRoot.name = PlantRootName;
            plantRoot.transform.localScale = Vector3.one * PlantUniformScale;
            StripColliders(plantRoot);
            NormalizePlantHeight(plantRoot, 0.62f);

            List<GameObject> blockers = CollectBlockers(roomPersonality.root);
            if (!TryPlaceInCorner(plantRoot, blockers))
            {
                Debug.LogWarning("[Level1CornerPlantBuilder] Could not place plant without overlap.");
            }

            Level1ScenePlacement.SnapPropBottomToFloor(plantRoot, FloorTopY, FloorPropLift);
        }

        private static Transform FindRoomPersonality()
        {
            GameObject world = GameObject.Find("Level1World");
            if (world != null)
            {
                Transform personality = world.transform.Find("Level1_IndoorArea/RoomPersonality");
                if (personality != null)
                {
                    return personality;
                }

                personality = world.transform.Find("RoomPersonality");
                if (personality != null)
                {
                    return personality;
                }
            }

            Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "RoomPersonality")
                {
                    return all[i];
                }
            }

            return null;
        }

        private static GameObject LoadPlantPrefab()
        {
            for (int i = 0; i < PlantPrefabPaths.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlantPrefabPaths[i]);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private static bool TryPlaceInCorner(GameObject plant, List<GameObject> blockers)
        {
            float innerMaxX = RoomHalfWidth - WallHalfThickness - WallInset;
            float innerMinX = -RoomHalfWidth + WallHalfThickness + WallInset;
            float innerMaxZ = RoomHalfDepth - WallHalfThickness - WallInset;
            float innerMinZ = -RoomHalfDepth + WallHalfThickness + WallInset;
            float offset = PlantFootprintRadius + CornerInset;

            Vector3[] corners =
            {
                new Vector3(innerMaxX - offset, FloorTopY, innerMaxZ - offset),
                new Vector3(innerMaxX - offset, FloorTopY, innerMinZ + offset),
                new Vector3(innerMinX + offset, FloorTopY, innerMaxZ - offset),
                new Vector3(innerMinX + offset, FloorTopY, innerMinZ + offset)
            };

            float[] yaws = { -135f, -45f, 135f, 45f };

            for (int i = 0; i < corners.Length; i++)
            {
                plant.transform.position = corners[i];
                plant.transform.rotation = Quaternion.Euler(0f, yaws[i], 0f);
                Level1ScenePlacement.SnapPropBottomToFloor(plant, FloorTopY, FloorPropLift);

                if (!HasOverlap(plant, blockers))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<GameObject> CollectBlockers(Transform worldRoot)
        {
            List<GameObject> blockers = new List<GameObject>();
            AddIfFound(blockers, worldRoot, "SleepingArea/Bed");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/SleepingArea/Bed");
            AddIfFound(blockers, worldRoot, "StudyArea/Desk");
            AddIfFound(blockers, worldRoot, "StudyArea/DeskChair");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/StudyArea/Desk");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/StudyArea/DeskChair");
            AddIfFound(blockers, worldRoot, "StorageArea/Wardrobe");
            AddIfFound(blockers, worldRoot, "StorageArea/Shelf");
            AddIfFound(blockers, worldRoot, "ExitArea/Suitcase");
            AddIfFound(blockers, worldRoot, "RentalRoomInterior/PortraitWall");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/RentalRoomInterior/PortraitWall");
            AddIfFound(blockers, worldRoot, "RentalRoomInterior/WallClock");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/RentalRoomInterior/WallClock");
            AddIfFound(blockers, worldRoot, "SleepingArea/FPT_WallHanger");
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

        private static bool HasOverlap(GameObject plant, List<GameObject> blockers)
        {
            if (!Level1ScenePlacement.TryGetWorldBounds(plant, out Bounds plantBounds))
            {
                return false;
            }

            for (int i = 0; i < blockers.Count; i++)
            {
                GameObject blocker = blockers[i];
                if (blocker == null || blocker == plant
                    || !Level1ScenePlacement.TryGetWorldBounds(blocker, out Bounds blockerBounds))
                {
                    continue;
                }

                if (OverlapsXZ(plantBounds, blockerBounds, OverlapClearance))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool OverlapsXZ(Bounds a, Bounds b, float clearance)
        {
            return a.min.x - clearance < b.max.x
                && a.max.x + clearance > b.min.x
                && a.min.z - clearance < b.max.z
                && a.max.z + clearance > b.min.z;
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void NormalizePlantHeight(GameObject plant, float targetHeight)
        {
            if (plant == null || !Level1ScenePlacement.TryGetWorldBounds(plant, out Bounds bounds))
            {
                return;
            }

            float height = Mathf.Max(0.01f, bounds.size.y);
            if (height < targetHeight * 0.75f || height > targetHeight * 1.35f)
            {
                float scaleUp = targetHeight / height;
                plant.transform.localScale *= scaleUp;
            }
        }
    }
}
#endif
