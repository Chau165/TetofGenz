#if UNITY_EDITOR
using System.Collections.Generic;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Lines the Level 1 outdoor walk path with Quaternius sidewalks and complete buildings
    /// (replaces wooden fence dressing). Placement avoids overlap and z-fighting.
    /// </summary>
    public static class Level1OutdoorQuaterniusStreetscape
    {
        private const string LibraryPath = "Assets/Settings/ChoTet/QuaterniusDowntownPrefabLibrary.asset";

        private const float PathInnerX = 3.38f;
        private const float SidewalkWidth = 2.28f;
        private const float PavementThickness = 0.08f;
        private const float CurbHeight = 0.13f;
        private const float CurbWidth = 0.16f;
        private const float BuildingRowPrimaryX = 6.02f;
        private const float BuildingRowSecondaryX = 8.42f;
        private const float BuildingRowTertiaryX = 10.72f;
        private const float SpanStartZ = -5.9f;
        private const float SpanEndZ = -25.8f;
        private const float BuildingSpacingZ = 2.85f;
        private const float BuildingScaleMin = 0.36f;
        private const float BuildingScaleMax = 0.48f;
        private const float UvMetersPerTile = 2.5f;

        public static void Build(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            QuaterniusDowntownPrefabLibrary library =
                AssetDatabase.LoadAssetAtPath<QuaterniusDowntownPrefabLibrary>(LibraryPath);
            Material pavement = QuaterniusSidewalkAssembler.ResolvePavementMaterial(library);
            if (pavement == null)
            {
                pavement = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat");
            }

            GameObject[] catalog = QuaterniusCompleteBuildingCatalog.Resolve(library);
            if (pavement == null || catalog.Length == 0)
            {
                Debug.LogWarning("[Level1OutdoorQuaterniusStreetscape] Missing Quaternius library or buildings — run " +
                                 "Tools → Cho Tet → Build Quaternius Downtown Kit first.");
                return;
            }

            GameObject root = new GameObject("Level1_OutdoorQuaterniusStreetscape");
            root.transform.SetParent(parent, false);

            float spanDepth = SpanStartZ - SpanEndZ;
            float spanCenterZ = (SpanStartZ + SpanEndZ) * 0.5f;

            // Pavement comes from UrbanPedestrianPavement — only curbs + buildings here to avoid z-fighting.
            BuildCurb(root.transform, pavement, leftSide: true, spanCenterZ, spanDepth);
            BuildCurb(root.transform, pavement, leftSide: false, spanCenterZ, spanDepth);
            BuildBuildingsSide(root.transform, catalog, leftSide: true);
            BuildBuildingsSide(root.transform, catalog, leftSide: false);
        }

        private static void BuildSidewalkSide(
            Transform parent,
            Material pavement,
            bool leftSide,
            float centerZ,
            float depth)
        {
            float sign = leftSide ? -1f : 1f;
            float centerX = sign * (PathInnerX + SidewalkWidth * 0.5f);
            float surfaceY = Level1OutdoorSettings.WalkSurfaceY;

            GameObject sideRoot = new GameObject(leftSide ? "OutdoorSidewalk_L" : "OutdoorSidewalk_R");
            sideRoot.transform.SetParent(parent, false);
            sideRoot.transform.position = new Vector3(centerX, surfaceY + PavementThickness * 0.5f, centerZ);

            GameObject slab = new GameObject("PavementSlab");
            slab.transform.SetParent(sideRoot.transform, false);
            slab.transform.localPosition = Vector3.zero;
            slab.transform.localRotation = Quaternion.identity;
            slab.transform.localScale = Vector3.one;

            MeshFilter filter = slab.AddComponent<MeshFilter>();
            MeshRenderer renderer = slab.AddComponent<MeshRenderer>();
            filter.sharedMesh = BuildPavementMesh(SidewalkWidth, depth, centerX, SpanEndZ, SpanStartZ - SpanEndZ);
            renderer.sharedMaterial = pavement;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void BuildCurb(Transform parent, Material material, bool leftSide, float centerZ, float depth)
        {
            float sign = leftSide ? -1f : 1f;
            float centerX = sign * (PathInnerX + CurbWidth * 0.5f);
            float surfaceY = Level1OutdoorSettings.WalkSurfaceY;

            GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = leftSide ? "PathCurb_L" : "PathCurb_R";
            curb.transform.SetParent(parent, false);
            curb.transform.position = new Vector3(centerX, surfaceY + CurbHeight * 0.5f, centerZ);
            curb.transform.localScale = new Vector3(CurbWidth, CurbHeight, depth + 0.04f);
            Object.DestroyImmediate(curb.GetComponent<Collider>());

            Renderer renderer = curb.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static void BuildBuildingsSide(Transform parent, GameObject[] catalog, bool leftSide)
        {
            GameObject rowRoot = new GameObject(leftSide ? "OutdoorBuildings_L" : "OutdoorBuildings_R");
            rowRoot.transform.SetParent(parent, false);

            var placed = new List<Bounds>();
            float[] rows = { BuildingRowPrimaryX, BuildingRowSecondaryX, BuildingRowTertiaryX };
            float depth = SpanStartZ - SpanEndZ;
            int slotCount = Mathf.Max(4, Mathf.FloorToInt(depth / BuildingSpacingZ));
            float startZ = SpanStartZ - BuildingSpacingZ * 0.35f;
            int lastIndex = -99;

            for (int slot = 0; slot < slotCount; slot++)
            {
                float z = startZ - slot * BuildingSpacingZ;
                for (int row = 0; row < rows.Length; row++)
                {
                    float rowX = (leftSide ? -1f : 1f) * rows[row];
                    float rowZ = z + row * 0.22f;
                    if (IsPlacementBlocked(rowX, rowZ))
                    {
                        continue;
                    }

                    int seed = (leftSide ? 19 : 31) + slot * 47 + row * 13;
                    TryPlaceBuilding(rowRoot.transform, catalog, leftSide, rowX, rowZ, seed, ref lastIndex, placed);
                }
            }
        }

        private static bool TryPlaceBuilding(
            Transform parent,
            GameObject[] catalog,
            bool leftSide,
            float rowX,
            float z,
            int seed,
            ref int lastIndex,
            List<Bounds> placed)
        {
            for (int attempt = 0; attempt < catalog.Length; attempt++)
            {
                GameObject prefab = QuaterniusCompleteBuildingCatalog.Pick(catalog, seed + attempt * 11, ref lastIndex);
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(prefab, parent);
                }

                instance.name = prefab.name;
                float scale = Mathf.Lerp(BuildingScaleMin, BuildingScaleMax, ((seed + attempt * 3) % 100) / 100f);
                instance.transform.position = new Vector3(rowX, Level1OutdoorSettings.WalkSurfaceY, z);
                instance.transform.rotation = HighwayUrbanBuildingOrientation.FaceRoadRotation(prefab, leftSide);
                instance.transform.localScale = Vector3.one * scale;
                HighwayPropUtility.StripColliders(instance);
                RemoveFacadeAccents(instance.transform);
                RemoveObstructiveStreetProps(instance.transform);
                PreservePrefabMaterials(instance, prefab);

                if (!QuaterniusCompleteBuildingValidator.IsValidSpawnedInstance(instance))
                {
                    HighwayPropUtility.SafeDestroy(instance);
                    continue;
                }

                Bounds bounds = QuaterniusCompleteBuildingValidator.ComputeWorldBounds(instance);
                if (OverlapsPath(bounds) || OverlapsPlaced(bounds, placed) || IsPlacementBlocked(bounds.center.x, bounds.center.z))
                {
                    HighwayPropUtility.SafeDestroy(instance);
                    continue;
                }

                placed.Add(bounds);
                TagCameraOccluder(instance);
                return true;
            }

            return false;
        }

        private static void PreservePrefabMaterials(GameObject instance, GameObject prefabAsset)
        {
            if (instance == null || prefabAsset == null)
            {
                return;
            }

            Renderer[] instanceRenderers = instance.GetComponentsInChildren<Renderer>(true);
            Renderer[] prefabRenderers = prefabAsset.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < instanceRenderers.Length && i < prefabRenderers.Length; i++)
            {
                Renderer instanceRenderer = instanceRenderers[i];
                Renderer prefabRenderer = prefabRenderers[i];
                if (instanceRenderer == null || prefabRenderer == null)
                {
                    continue;
                }

                Material[] source = prefabRenderer.sharedMaterials;
                if (source != null && source.Length > 0)
                {
                    instanceRenderer.sharedMaterials = source;
                }
            }
        }

        private static void RemoveFacadeAccents(Transform root)
        {
            if (root == null)
            {
                return;
            }

            var toRemove = new List<GameObject>();
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.IndexOf("FacadeAccents", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    toRemove.Add(child.gameObject);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                HighwayPropUtility.SafeDestroy(toRemove[i]);
            }
        }

        private static void RemoveObstructiveStreetProps(Transform root)
        {
            if (root == null)
            {
                return;
            }

            var toRemove = new List<GameObject>();
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                string lower = child.name.ToLowerInvariant();
                if (lower.Contains("crate")
                    || lower.Contains("box")
                    || lower.Contains("trash")
                    || lower.Contains("bin")
                    || lower.Contains("barrel"))
                {
                    toRemove.Add(child.gameObject);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                HighwayPropUtility.SafeDestroy(toRemove[i]);
            }
        }

        private static bool OverlapsPath(Bounds bounds)
        {
            float pathLimit = PathInnerX + 0.28f;
            return bounds.min.x < pathLimit && bounds.max.x > -pathLimit;
        }

        private static bool OverlapsPlaced(Bounds candidate, List<Bounds> placed)
        {
            const float padding = 0.08f;
            Bounds padded = candidate;
            padded.Expand(padding);
            for (int i = 0; i < placed.Count; i++)
            {
                Bounds other = placed[i];
                other.Expand(padding);
                if (padded.Intersects(other))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPlacementBlocked(float x, float z)
        {
            if (Level1BusStopRoadSettings.OverlapsRoad(x, z, 1.15f, 1.05f))
            {
                return true;
            }

            return Mathf.Abs(x) <= 4.5f
                && z >= Level1BusStopRoadSettings.LaneNorthEdgeZ - 0.65f
                && z <= Level1BusStopRoadSettings.SidewalkFarZ + 0.75f;
        }

        private static Mesh BuildPavementMesh(
            float width,
            float length,
            float worldCenterX,
            float segmentWorldEndZ,
            float segmentLengthZ)
        {
            float halfWidth = width * 0.5f;
            float halfLength = length * 0.5f;
            float slabCenterWorldZ = segmentWorldEndZ + segmentLengthZ * 0.5f;

            var vertices = new Vector3[]
            {
                new Vector3(-halfWidth, 0f, -halfLength),
                new Vector3(halfWidth, 0f, -halfLength),
                new Vector3(-halfWidth, 0f, halfLength),
                new Vector3(halfWidth, 0f, halfLength)
            };

            var uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                float worldX = worldCenterX + vertices[i].x;
                float worldZ = slabCenterWorldZ + vertices[i].z;
                uvs[i] = new Vector2(worldX / UvMetersPerTile, worldZ / UvMetersPerTile);
            }

            var mesh = new Mesh { name = "OutdoorPavementSlab" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void TagCameraOccluder(GameObject root)
        {
            if (root == null || root.GetComponent<CameraViewOccluder>() != null)
            {
                return;
            }

            root.AddComponent<CameraViewOccluder>();
        }
    }
}
#endif
