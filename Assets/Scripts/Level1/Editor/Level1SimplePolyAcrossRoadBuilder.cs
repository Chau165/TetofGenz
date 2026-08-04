#if UNITY_EDITOR
using System.Collections.Generic;
using ChoTet.Level1;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Dense Simple Poly facades on the north side of the Level 1 bus-stop road — packed tight without mesh overlap.
    /// </summary>
    public static class Level1SimplePolyAcrossRoadBuilder
    {
        public const string RootName = "SimplePoly_AcrossRoad";

        private const string BuildingsRoot =
            "Assets/Low Poly Simple Urban City 3D Asset Pack/Prefabs/Buildings";

        private const float SpanStartX = -9.85f;
        private const float SpanEndX = 9.85f;
        private const float PlacementPadding = 0.14f;
        private const float XScanStep = 0.38f;
        private const float RowDepthStep = 3.55f;

        private const float NorthSidewalkDepth = 2.35f;
        private const float BuildingSetbackFromSidewalk = 1.05f;
        private const float BuildingFaceYaw = 0f;

        private static readonly string[] BuildingPrefabNames =
        {
            "Building_1.prefab",
            "Building_2.prefab",
            "Building_3.prefab",
            "Building_4.prefab",
            "Building_5.prefab",
            "Building_6.prefab",
            "Building_7.prefab",
            "Building_8.prefab",
            "Building_9.prefab",
        };

        public static void Build(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            GameObject root = new GameObject(RootName);
            root.transform.SetParent(parent, false);

            float northShoulderOuterZ =
                Level1BusStopRoadSettings.LaneNorthEdgeZ - Level1BusStopRoadSettings.ShoulderDepth;
            float sidewalkCenterZ = northShoulderOuterZ - NorthSidewalkDepth * 0.5f - 0.06f;
            float buildingRowZ = sidewalkCenterZ - NorthSidewalkDepth * 0.5f - BuildingSetbackFromSidewalk;
            float surfaceY = Level1OutdoorSettings.WalkSurfaceY;

            BuildNorthSidewalk(root.transform, sidewalkCenterZ);

            var placed = new List<Bounds>();
            int placedCount = 0;
            int lastPrefabIndex = -99;
            int rowIndex = 0;
            float rowZ = buildingRowZ + RowZOffset(rowIndex) * 0.42f;

            while (rowZ >= buildingRowZ - RowDepthStep * 3.05f)
            {
                float xCursor = SpanStartX + RowXOffset(rowIndex) * 0.35f;
                int placedInRow = 0;
                int failedScans = 0;

                while (xCursor < SpanEndX && failedScans < 28)
                {
                    if (TryPlaceBuilding(
                            root.transform,
                            xCursor,
                            surfaceY,
                            rowZ,
                            rowIndex * 41 + placedInRow * 19,
                            ref lastPrefabIndex,
                            placed,
                            ref placedCount))
                    {
                        placedInRow++;
                        failedScans = 0;
                        xCursor = placed[placed.Count - 1].max.x + PlacementPadding;
                    }
                    else
                    {
                        failedScans++;
                        xCursor += XScanStep;
                    }
                }

                rowIndex++;
                rowZ = buildingRowZ - rowIndex * RowDepthStep + RowZOffset(rowIndex) * 0.42f;
            }
        }

        private static float RowZOffset(int rowIndex)
        {
            return (rowIndex % 3) - 1;
        }

        private static float RowXOffset(int rowIndex)
        {
            return (rowIndex % 2) == 0 ? 0f : 1f;
        }

        private static bool IsPlacementBlocked(float x, float z)
        {
            if (Level1BusStopRoadSettings.OverlapsRoad(x, z, 1.2f, 1.05f))
            {
                return true;
            }

            if (Level1BusStopRoadSettings.OverlapsBusStopPad(x, z, 0.55f))
            {
                return true;
            }

            return false;
        }

        private static bool TryPlaceBuilding(
            Transform parent,
            float targetMinX,
            float surfaceY,
            float z,
            int seed,
            ref int lastPrefabIndex,
            List<Bounds> placed,
            ref int placedCount)
        {
            if (IsPlacementBlocked(targetMinX, z))
            {
                return false;
            }

            for (int attempt = 0; attempt < BuildingPrefabNames.Length; attempt++)
            {
                int prefabIndex = PickPrefabIndex(seed + attempt * 11, ref lastPrefabIndex);
                string prefabPath = BuildingsRoot + "/" + BuildingPrefabNames[prefabIndex];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    continue;
                }

                float scale = 0.34f + ((seed + attempt * 5) % 5) * 0.01f;
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                if (instance == null)
                {
                    continue;
                }

                instance.name = "AcrossRoad_" + BuildingPrefabNames[prefabIndex].Replace(".prefab", "") + "_" + placedCount;
                instance.transform.localScale = Vector3.one * scale;
                instance.transform.position = new Vector3(targetMinX + 1.6f, surfaceY, z);
                instance.transform.rotation = Quaternion.Euler(0f, BuildingFaceYaw + GetPrefabYawOffset(prefab.name), 0f);
                SnapBuildingToGround(instance, surfaceY);
                AlignWestEdge(instance, targetMinX);
                HighwayPropUtility.StripColliders(instance);

                Bounds bounds = QuaterniusCompleteBuildingValidator.ComputeWorldBounds(instance);
                if (bounds.size.sqrMagnitude < 0.01f
                    || bounds.min.x < SpanStartX - 0.08f
                    || bounds.max.x > SpanEndX + 0.08f
                    || OverlapsPlaced(bounds, placed)
                    || IsPlacementBlocked(bounds.center.x, bounds.center.z))
                {
                    Object.DestroyImmediate(instance);
                    continue;
                }

                placed.Add(bounds);
                placedCount++;
                if (instance.GetComponent<CameraViewOccluder>() == null)
                {
                    instance.AddComponent<CameraViewOccluder>();
                }

                return true;
            }

            return false;
        }

        private static void AlignWestEdge(GameObject instance, float targetMinX)
        {
            Bounds bounds = QuaterniusCompleteBuildingValidator.ComputeWorldBounds(instance);
            if (bounds.size.sqrMagnitude < 0.01f)
            {
                return;
            }

            float shiftX = targetMinX - bounds.min.x;
            if (Mathf.Abs(shiftX) > 0.0001f)
            {
                instance.transform.position += new Vector3(shiftX, 0f, 0f);
            }
        }

        private static float GetPrefabYawOffset(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return 0f;
            }

            switch (prefabName)
            {
                case "Building_3":
                case "Building_5":
                case "Building_8":
                    return 180f;
                default:
                    return 0f;
            }
        }

        private static int PickPrefabIndex(int seed, ref int lastPrefabIndex)
        {
            int index = Mathf.Abs(seed * 73856093) % BuildingPrefabNames.Length;
            if (index == lastPrefabIndex)
            {
                index = (index + 1) % BuildingPrefabNames.Length;
            }

            lastPrefabIndex = index;
            return index;
        }

        private static bool OverlapsPlaced(Bounds candidate, List<Bounds> placed)
        {
            Bounds padded = candidate;
            padded.Expand(PlacementPadding);
            for (int i = 0; i < placed.Count; i++)
            {
                Bounds other = placed[i];
                other.Expand(PlacementPadding);
                if (padded.Intersects(other))
                {
                    return true;
                }
            }

            return false;
        }

        private static void BuildNorthSidewalk(Transform parent, float centerZ)
        {
            Material pavement = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat");
            if (pavement == null)
            {
                pavement = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Settings/ChoTet/Materials/Foundation_Stone.mat");
            }

            if (pavement == null)
            {
                return;
            }

            const float slabThickness = 0.08f;
            const float lotHalfWidth = 10.2f;
            float y = Level1OutdoorSettings.WalkSurfaceY - slabThickness * 0.5f - 0.01f;

            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "AcrossRoad_Sidewalk";
            slab.transform.SetParent(parent, false);
            slab.transform.position = new Vector3(0f, y, centerZ);
            slab.transform.localScale = new Vector3(lotHalfWidth * 2f, slabThickness, NorthSidewalkDepth);
            Object.DestroyImmediate(slab.GetComponent<Collider>());

            Renderer renderer = slab.GetComponent<Renderer>();
            Material tiled = new Material(pavement);
            tiled.SetTextureScale("_BaseMap", new Vector2(
                Mathf.Max(1f, lotHalfWidth * 2f / 2.8f),
                Mathf.Max(1f, NorthSidewalkDepth / 2.8f)));
            renderer.sharedMaterial = tiled;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void SnapBuildingToGround(GameObject instance, float targetGroundY)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                Vector3 position = instance.transform.position;
                position.y = targetGroundY;
                instance.transform.position = position;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float lift = targetGroundY - bounds.min.y;
            if (Mathf.Abs(lift) > 0.0001f)
            {
                instance.transform.position += new Vector3(0f, lift, 0f);
            }
        }
    }
}
#endif
