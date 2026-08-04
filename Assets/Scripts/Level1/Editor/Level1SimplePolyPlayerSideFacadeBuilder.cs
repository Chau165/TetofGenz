#if UNITY_EDITOR
using System.Collections.Generic;
using ChoTet.Level1;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Dense Simple Poly facade on the player's side of the outdoor street — packed tight without mesh overlap.
    /// </summary>
    public static class Level1SimplePolyPlayerSideFacadeBuilder
    {
        public const string RootName = "SimplePoly_PlayerSideBlock";

        private const string BuildingsRoot =
            "Assets/Low Poly Simple Urban City 3D Asset Pack/Prefabs/Buildings";

        private const float PathInnerX = 3.38f;
        private const float PathEdgePadding = 0.32f;
        private const float PlacementPadding = 0.14f;
        private const float ZScanStep = 0.38f;

        private const float SpanStartZ = -5.9f;
        private const float SpanEndZ = -25.8f;
        private const float FarLeftLimitX = -22.4f;

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

            var placed = new List<Bounds>();
            SeedQuaterniusFacades(parent, placed);

            float allowedMaxX = ComputeAllowedMaxX(placed);
            float surfaceY = Level1OutdoorSettings.WalkSurfaceY;
            int placedCount = 0;
            int lastPrefabIndex = -99;
            float columnX = ComputeFirstColumnX(allowedMaxX);
            int columnIndex = 0;

            while (columnX > FarLeftLimitX)
            {
                float zCursor = SpanStartZ - 0.22f - ColumnZOffset(columnIndex) * 0.42f;
                int placedInColumn = 0;

                while (zCursor >= SpanEndZ)
                {
                    if (TryPlaceBuilding(
                            root.transform,
                            columnX,
                            surfaceY,
                            zCursor,
                            columnIndex * 53 + placedInColumn * 17,
                            ref lastPrefabIndex,
                            placed,
                            allowedMaxX,
                            ref placedCount))
                    {
                        placedInColumn++;
                        zCursor = placed[placed.Count - 1].min.z - PlacementPadding;
                    }
                    else
                    {
                        zCursor -= ZScanStep;
                    }
                }

                if (placedInColumn == 0)
                {
                    columnX -= 1.05f;
                }
                else
                {
                    columnX = ComputeNextColumnX(placed, columnX);
                }

                columnIndex++;
            }
        }

        private static float ComputeAllowedMaxX(List<Bounds> placed)
        {
            float westEdge = float.PositiveInfinity;
            for (int i = 0; i < placed.Count; i++)
            {
                westEdge = Mathf.Min(westEdge, placed[i].min.x);
            }

            if (float.IsPositiveInfinity(westEdge))
            {
                return -5.85f;
            }

            return westEdge - PlacementPadding;
        }

        private static float ComputeFirstColumnX(float allowedMaxX)
        {
            float halfDepth = EstimateHalfDepthX();
            return allowedMaxX - halfDepth;
        }

        private static float ComputeNextColumnX(List<Bounds> placed, float currentColumnX)
        {
            float westEdge = float.PositiveInfinity;
            for (int i = 0; i < placed.Count; i++)
            {
                Bounds bounds = placed[i];
                if (Mathf.Abs(bounds.center.x - currentColumnX) > 2.6f)
                {
                    continue;
                }

                westEdge = Mathf.Min(westEdge, bounds.min.x);
            }

            if (float.IsPositiveInfinity(westEdge))
            {
                return currentColumnX - 3.95f;
            }

            return westEdge - EstimateHalfDepthX() - PlacementPadding;
        }

        private static float EstimateHalfDepthX()
        {
            // Building_1 at max scale after facing the road — conservative for all variants.
            return 2.28f;
        }

        private static float ColumnZOffset(int columnIndex)
        {
            return (columnIndex % 3) - 1;
        }

        private static void SeedQuaterniusFacades(Transform outdoorRoot, List<Bounds> placed)
        {
            if (outdoorRoot == null)
            {
                return;
            }

            Transform quaterniusRoot = outdoorRoot.Find("Level1_OutdoorQuaterniusStreetscape");
            if (quaterniusRoot == null)
            {
                return;
            }

            Transform leftRow = quaterniusRoot.Find("OutdoorBuildings_L");
            if (leftRow == null)
            {
                return;
            }

            foreach (Transform child in leftRow)
            {
                Bounds bounds = QuaterniusCompleteBuildingValidator.ComputeWorldBounds(child.gameObject);
                if (bounds.size.sqrMagnitude > 0.01f)
                {
                    placed.Add(bounds);
                }
            }
        }

        private static bool IsPlacementBlocked(float x, float z)
        {
            if (x > -5.85f)
            {
                return true;
            }

            if (Level1BusStopRoadSettings.OverlapsRoad(x, z, 1.2f, 1.0f))
            {
                return true;
            }

            if (x > 2.2f && z > -16.2f && z < -13.1f)
            {
                return true;
            }

            return false;
        }

        private static bool TryPlaceBuilding(
            Transform parent,
            float x,
            float surfaceY,
            float z,
            int seed,
            ref int lastPrefabIndex,
            List<Bounds> placed,
            float allowedMaxX,
            ref int placedCount)
        {
            if (IsPlacementBlocked(x, z))
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

                instance.name = "PlayerSide_" + BuildingPrefabNames[prefabIndex].Replace(".prefab", "") + "_" + placedCount;
                instance.transform.localScale = Vector3.one * scale;
                instance.transform.position = new Vector3(x, surfaceY, z);
                instance.transform.rotation = HighwayUrbanBuildingOrientation.FaceRoadRotation(prefab, leftSide: true);
                SnapBuildingToGround(instance, surfaceY);
                HighwayPropUtility.StripColliders(instance);

                Bounds bounds = QuaterniusCompleteBuildingValidator.ComputeWorldBounds(instance);
                if (bounds.size.sqrMagnitude < 0.01f
                    || bounds.max.x > allowedMaxX
                    || OverlapsPath(bounds)
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

        private static bool OverlapsPath(Bounds bounds)
        {
            float pathLimit = PathInnerX + PathEdgePadding;
            return bounds.min.x < pathLimit && bounds.max.x > -pathLimit;
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
