using System.Collections.Generic;
using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Spawns baked NSK2 countryside props — trees/bushes keep prefab materials.</summary>
    internal static class HometownArrivalCountrysidePropFactory
    {
        private const string BakedRoot = "Assets/Prefabs/Highway/NatureBaked";
        private const string OccaRoot = "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs";

        private enum DecorKind
        {
            Baked,
            Fence
        }

        private enum TreeLayer
        {
            FenceLine,
            Near,
            Mid,
            Far
        }

        private static Material woodDarkMaterial;
        private static Material woodWarmMaterial;

        private static List<GameObject> bakedTreePrefabs;
        private static List<GameObject> bakedBushPrefabs;
        private static GameObject fencePrefab;

        public static void EnsureCatalogLoaded()
        {
            woodDarkMaterial = HometownArrivalResourceCache.LoadMaterial("Wood_Dark.mat");
            woodWarmMaterial = HometownArrivalResourceCache.LoadMaterial("Wood_Warm.mat");

            HighwayNaturePrefabLibrary library = HometownArrivalResourceCache.LoadNatureLibrary();
            bakedTreePrefabs = CollectPrefabs(
                library != null ? library.trees : null,
                BakedRoot + "/tree01_Baked.prefab",
                BakedRoot + "/tree02_Baked.prefab",
                BakedRoot + "/tree03_Baked.prefab",
                BakedRoot + "/tree04_Baked.prefab");

            bakedBushPrefabs = CollectPrefabs(
                library != null ? library.bushes : null,
                BakedRoot + "/bush01_Baked.prefab",
                BakedRoot + "/bush02_Baked.prefab",
                BakedRoot + "/bush03_Baked.prefab",
                BakedRoot + "/bush04_Baked.prefab",
                BakedRoot + "/bush05_Baked.prefab",
                BakedRoot + "/bush06_Baked.prefab");

            if (fencePrefab == null)
            {
                fencePrefab = HometownArrivalResourceCache.LoadPrefab(OccaRoot + "/Fence.prefab");
            }
        }

        public static void SpawnPathAccentBush(Transform parent, Vector3 worldPosition, float yawDegrees, float scale)
        {
            SpawnWorldBush(parent, worldPosition, yawDegrees, scale);
        }

        public static void SpawnWorldFence(Transform parent, Vector3 worldPosition, float yawDegrees, float scale)
        {
            EnsureCatalogLoaded();
            if (fencePrefab == null || parent == null)
            {
                return;
            }

            GameObject instance = Object.Instantiate(fencePrefab, parent);
            instance.name = "Fence";
            instance.transform.position = worldPosition;
            instance.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            instance.transform.localScale = Vector3.one * scale;
            StripColliders(instance);
            ApplyDecorMaterials(instance, DecorKind.Fence);
        }

        public static void SpawnWorldBush(Transform parent, Vector3 worldPosition, float yawDegrees, float scale)
        {
            EnsureCatalogLoaded();
            if (bakedBushPrefabs == null || bakedBushPrefabs.Count == 0 || parent == null)
            {
                return;
            }

            GameObject prefab = bakedBushPrefabs[Mathf.Abs((int)(worldPosition.z * 10f)) % bakedBushPrefabs.Count];
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = "BranchAccentBush";
            instance.transform.position = worldPosition;
            instance.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            instance.transform.localScale = Vector3.one * scale;
            StripColliders(instance);
        }

        public static void SpawnWorldTree(Transform parent, Vector3 worldPosition, float yawDegrees, float scale)
        {
            EnsureCatalogLoaded();
            if (bakedTreePrefabs == null || bakedTreePrefabs.Count == 0 || parent == null)
            {
                return;
            }

            GameObject prefab = bakedTreePrefabs[Mathf.Abs((int)(worldPosition.x * 7f)) % bakedTreePrefabs.Count];
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = "BranchAccentTree";
            instance.transform.position = worldPosition;
            instance.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            instance.transform.localScale = Vector3.one * scale;
            StripColliders(instance);
        }

        public static void SpawnFenceRow(Transform parent, float sideSign, float localZStart, float localZEnd, float spacing)
        {
            EnsureCatalogLoaded();
            if (fencePrefab == null)
            {
                return;
            }

            float x = sideSign * HometownArrivalCountrysideLayout.FenceAbsX;
            float yaw = sideSign < 0f ? 90f : -90f;
            int count = Mathf.CeilToInt((localZEnd - localZStart) / spacing);
            for (int i = 0; i <= count; i++)
            {
                float localZ = localZStart + i * spacing;
                if (localZ > localZEnd + 0.01f)
                {
                    break;
                }

                if (IsBranchBeltSide(sideSign) && ShouldSuppressBranchBelt(parent, x, localZ, fence: true))
                {
                    continue;
                }

                Spawn(parent, fencePrefab, new Vector3(x, 0f, localZ), yaw, 1.05f, DecorKind.Fence);
            }
        }

        public static void SpawnBushesForSegment(Transform parent, float sideSign, float segmentLength, int layoutSeed)
        {
            EnsureCatalogLoaded();
            if (bakedBushPrefabs == null || bakedBushPrefabs.Count == 0)
            {
                return;
            }

            System.Random random = new System.Random(layoutSeed + 113);
            int bushCount = 18 + (layoutSeed % 4);
            List<Vector3> placed = new List<Vector3>(bushCount);
            int attempts = 0;
            while (placed.Count < bushCount && attempts < bushCount * 10)
            {
                attempts++;
                float localZ = NextRange(random, 0.4f, segmentLength - 0.4f);
                float x = sideSign * NextRange(random,
                    HometownArrivalCountrysideLayout.BushMinAbsX,
                    HometownArrivalCountrysideLayout.BushMaxAbsX);
                Vector3 candidate = new Vector3(x, 0f, localZ);
                if (IsTooClose(candidate, placed, 1.15f))
                {
                    continue;
                }

                if (IsBranchBeltSide(sideSign) && ShouldSuppressBranchBelt(parent, x, localZ, fence: false))
                {
                    continue;
                }

                GameObject prefab = bakedBushPrefabs[random.Next(bakedBushPrefabs.Count)];
                Spawn(parent, prefab, candidate, NextRange(random, 0f, 360f),
                    NextRange(random, 0.82f, 1.18f), DecorKind.Baked);
                placed.Add(candidate);
            }
        }

        public static void SpawnForestForSegment(Transform parent, float sideSign, float segmentLength, int layoutSeed)
        {
            EnsureCatalogLoaded();
            if (bakedTreePrefabs == null || bakedTreePrefabs.Count == 0)
            {
                return;
            }

            List<Vector3> placed = new List<Vector3>(96);
            SpawnTreeLayer(parent, sideSign, segmentLength, layoutSeed + 7, TreeLayer.FenceLine, placed);
            SpawnTreeLayer(parent, sideSign, segmentLength, layoutSeed + 11, TreeLayer.Near, placed);
            SpawnTreeLayer(parent, sideSign, segmentLength, layoutSeed + 29, TreeLayer.Mid, placed);
            SpawnTreeLayer(parent, sideSign, segmentLength, layoutSeed + 53, TreeLayer.Far, placed);
        }

        private static void SpawnTreeLayer(
            Transform parent,
            float sideSign,
            float segmentLength,
            int layerSeed,
            TreeLayer layer,
            List<Vector3> placed)
        {
            System.Random random = new System.Random(layerSeed);
            GetLayerConfig(layer, out float minAbsX, out float maxAbsX, out int targetCount,
                out float minScale, out float maxScale, out float minSpacing);

            int attempts = 0;
            int maxAttempts = targetCount * 12;
            int startCount = placed.Count;
            while (placed.Count < startCount + targetCount && attempts < maxAttempts)
            {
                attempts++;
                float localZ = NextRange(random, 0.3f, segmentLength - 0.3f);
                localZ += NextRange(random, -0.25f, 0.25f);
                localZ = Mathf.Clamp(localZ, 0.15f, segmentLength - 0.15f);

                float x = sideSign * NextRange(random, minAbsX, maxAbsX);
                Vector3 candidate = new Vector3(x, 0f, localZ);
                if (IsTooClose(candidate, placed, minSpacing))
                {
                    continue;
                }

                if (IsBranchBeltSide(sideSign) && ShouldSuppressBranchBelt(parent, x, localZ, fence: false))
                {
                    continue;
                }

                GameObject prefab = bakedTreePrefabs[random.Next(bakedTreePrefabs.Count)];
                float scale = NextRange(random, minScale, maxScale);
                Spawn(parent, prefab, candidate, NextRange(random, 0f, 360f), scale, DecorKind.Baked);
                placed.Add(candidate);
            }
        }

        private static void GetLayerConfig(
            TreeLayer layer,
            out float minAbsX,
            out float maxAbsX,
            out int targetCount,
            out float minScale,
            out float maxScale,
            out float minSpacing)
        {
            switch (layer)
            {
                case TreeLayer.FenceLine:
                    minAbsX = HometownArrivalCountrysideLayout.TreeLineMinAbsX;
                    maxAbsX = HometownArrivalCountrysideLayout.TreeLineMaxAbsX;
                    targetCount = 16;
                    minScale = 0.72f;
                    maxScale = 0.98f;
                    minSpacing = 1.35f;
                    break;
                case TreeLayer.Near:
                    minAbsX = HometownArrivalCountrysideLayout.TreeNearMinAbsX;
                    maxAbsX = HometownArrivalCountrysideLayout.TreeNearMaxAbsX;
                    targetCount = 14;
                    minScale = 0.85f;
                    maxScale = HometownArrivalCountrysideLayout.NearTreeMaxScale;
                    minSpacing = 1.55f;
                    break;
                case TreeLayer.Mid:
                    minAbsX = HometownArrivalCountrysideLayout.TreeMidMinAbsX;
                    maxAbsX = HometownArrivalCountrysideLayout.TreeMidMaxAbsX;
                    targetCount = 12;
                    minScale = 0.95f;
                    maxScale = HometownArrivalCountrysideLayout.MidTreeMaxScale;
                    minSpacing = 1.85f;
                    break;
                default:
                    minAbsX = HometownArrivalCountrysideLayout.TreeFarMinAbsX;
                    maxAbsX = HometownArrivalCountrysideLayout.TreeFarMaxAbsX;
                    targetCount = 9;
                    minScale = 1.05f;
                    maxScale = HometownArrivalCountrysideLayout.FarTreeMaxScale;
                    minSpacing = 2.2f;
                    break;
            }
        }

        public static void BuildGrassSlab(Transform parent, float sideSign, float segmentLength, Material grass)
        {
            if (grass == null)
            {
                return;
            }

            float width = HometownArrivalCountrysideLayout.GrassOuterAbsX
                - HometownArrivalCountrysideLayout.GrassInnerAbsX;
            float centerX = sideSign * (HometownArrivalCountrysideLayout.GrassInnerAbsX + width * 0.5f);
            float slabLength = segmentLength + 0.25f;

            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "GrassSlab";
            slab.transform.SetParent(parent, false);
            slab.transform.localPosition = new Vector3(centerX, -0.12f, segmentLength * 0.5f);
            slab.transform.localScale = new Vector3(width, 0.08f, slabLength);

            Collider collider = slab.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            MeshRenderer renderer = slab.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = grass;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static GameObject Spawn(
            Transform parent,
            GameObject prefab,
            Vector3 localPosition,
            float yawDegrees,
            float uniformScale,
            DecorKind kind)
        {
            if (prefab == null || parent == null)
            {
                return null;
            }

            if (kind == DecorKind.Baked && IsOnRoadCorridor(localPosition))
            {
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            instance.transform.localScale = Vector3.one * uniformScale;
            StripColliders(instance);

            if (kind != DecorKind.Baked)
            {
                ApplyDecorMaterials(instance, kind);
            }

            return instance;
        }

        private static void ApplyDecorMaterials(GameObject instance, DecorKind kind)
        {
            MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material chosen = kind switch
                {
                    DecorKind.Fence => woodDarkMaterial ?? woodWarmMaterial,
                    _ => null
                };

                if (chosen != null)
                {
                    renderer.sharedMaterial = chosen;
                }
            }
        }

        private static bool IsOnRoadCorridor(Vector3 localPosition)
        {
            return Mathf.Abs(localPosition.x) < HometownArrivalCountrysideLayout.RoadHalfClear + 1.2f;
        }

        private static bool IsBranchBeltSide(float sideSign)
        {
            return Mathf.Sign(sideSign) == Mathf.Sign(HometownArrivalBranchRoadJunction.CountrysideBeltSideSign);
        }

        private static bool ShouldSuppressBranchBelt(Transform beltParent, float localX, float localZ, bool fence)
        {
            if (beltParent == null || beltParent.parent == null)
            {
                return false;
            }

            Vector3 world = beltParent.parent.TransformPoint(new Vector3(localX, 0f, localZ));
            return fence
                ? HometownArrivalBranchRoadJunction.ShouldSuppressBranchFence(world)
                : HometownArrivalBranchRoadJunction.ShouldSuppressBranchVegetation(world);
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(colliders[i]);
                    continue;
                }
#endif
                Object.Destroy(colliders[i]);
            }
        }

        private static bool IsTooClose(Vector3 candidate, List<Vector3> placed, float minDistance)
        {
            float minDistanceSq = minDistance * minDistance;
            for (int i = 0; i < placed.Count; i++)
            {
                if ((placed[i] - candidate).sqrMagnitude < minDistanceSq)
                {
                    return true;
                }
            }

            return false;
        }

        private static float NextRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static List<GameObject> CollectPrefabs(GameObject[] fromLibrary, params string[] fallbackPaths)
        {
            List<GameObject> loaded = new List<GameObject>();
            if (fromLibrary != null)
            {
                for (int i = 0; i < fromLibrary.Length; i++)
                {
                    if (fromLibrary[i] != null)
                    {
                        loaded.Add(fromLibrary[i]);
                    }
                }
            }

            if (loaded.Count > 0)
            {
                return loaded;
            }

            return LoadAny(fallbackPaths);
        }

        private static List<GameObject> LoadAny(params string[] paths)
        {
            List<GameObject> loaded = new List<GameObject>(paths.Length);
            for (int i = 0; i < paths.Length; i++)
            {
                GameObject prefab = HometownArrivalResourceCache.LoadPrefab(paths[i]);
                if (prefab != null)
                {
                    loaded.Add(prefab);
                }
            }

            return loaded;
        }
    }
}
