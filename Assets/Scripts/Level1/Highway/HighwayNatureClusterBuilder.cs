using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>NSK2 mesh clusters for scrolling roadside vegetation.</summary>
    public static class HighwayNatureClusterBuilder
    {
        public struct Palette
        {
            public Material Foliage;
            public Material Bark;
            public Material GrassBladeA;
            public Material GrassBladeB;
        }

        public enum ClusterKind
        {
            GrassPatch,
            BushGroup,
            TreeGroup,
            Backdrop
        }

        /// <summary>
        /// Feet deep under water; tops sit near the deck so only canopy shows from the elevated cam.
        /// Kept short enough that the green canopy clears the waterline (not half-submerged).
        /// </summary>
        private const float CanopyBaseY = -28f;

        /// <summary>Dense tall trees — canopies rise toward the elevated road, trunks stay below water.</summary>
        public static void BuildDenseCanopyStrip(
            Transform root,
            HighwayNaturePrefabLibrary library,
            Palette palette,
            int seed,
            float segmentLength)
        {
            HighwayPropUtility.ClearChildren(root);

            if (library != null && library.HasContent)
            {
                int treeCount = 5 + seed % 3;
                float spacing = segmentLength / treeCount;
                for (int i = 0; i < treeCount; i++)
                {
                    int itemSeed = seed + i * 43;
                    float z = spacing * (i + 0.5f) + (((itemSeed * 7) % 100) - 50) * 0.035f;
                    float x = (((itemSeed * 11) % 100) - 50) * 0.055f;
                    SpawnTallTree(root, library, new Vector3(x, CanopyBaseY, z), itemSeed);
                }

                return;
            }

            BuildFallbackCanopy(root, palette, seed, segmentLength);
        }

        public static void Build(
            Transform root,
            ClusterKind kind,
            HighwayNaturePrefabLibrary library,
            Palette palette,
            int seed)
        {
            HighwayPropUtility.ClearChildren(root);

            if (library != null && library.HasContent)
            {
                BuildFromLibrary(root, kind, library, palette, seed);
                return;
            }

            BuildFallback(root, kind, palette, seed);
        }

        private static void BuildFromLibrary(
            Transform root,
            ClusterKind kind,
            HighwayNaturePrefabLibrary library,
            Palette palette,
            int seed)
        {
            switch (kind)
            {
                case ClusterKind.GrassPatch:
                    PlaceGrassPatch(root, library, palette, seed, 2, 4);
                    break;
                case ClusterKind.BushGroup:
                    PlaceBushRing(root, library, seed, 1, 2);
                    break;
                case ClusterKind.TreeGroup:
                    PlaceTrees(root, library, seed, 2, 4);
                    break;
                case ClusterKind.Backdrop:
                    PlaceTrees(root, library, seed, 3, 5);
                    break;
            }
        }

        private static void PlaceGrassPatch(
            Transform root,
            HighwayNaturePrefabLibrary library,
            Palette palette,
            int seed,
            int minCount,
            int maxCount)
        {
            int count = minCount + seed % (maxCount - minCount + 1);
            for (int i = 0; i < count; i++)
            {
                int itemSeed = seed + i * 29;
                Vector3 offset = JitterXZ(itemSeed, 1.8f, 2.5f);
                SpawnGrass(root, library, palette, offset, itemSeed);
            }
        }

        private static void PlaceBushRing(
            Transform root,
            HighwayNaturePrefabLibrary library,
            int seed,
            int minCount,
            int maxCount)
        {
            int count = minCount + seed % (maxCount - minCount + 1);
            for (int i = 0; i < count; i++)
            {
                int itemSeed = seed + i * 37;
                Vector3 offset = JitterXZ(itemSeed, 2f, 3f);
                SpawnBush(root, library, offset, itemSeed);
            }
        }

        private static void PlaceTrees(
            Transform root,
            HighwayNaturePrefabLibrary library,
            int seed,
            int minCount,
            int maxCount)
        {
            int count = minCount + seed % (maxCount - minCount + 1);
            for (int i = 0; i < count; i++)
            {
                int itemSeed = seed + i * 41;
                Vector3 offset = JitterXZ(itemSeed, 3f, 5f);
                SpawnTree(root, library, offset, itemSeed);
            }
        }

        private static void SpawnGrass(
            Transform root,
            HighwayNaturePrefabLibrary library,
            Palette palette,
            Vector3 offset,
            int seed)
        {
            Material grassA = palette.GrassBladeA ?? palette.Foliage;
            Material grassB = palette.GrassBladeB ?? grassA;
            if (grassA == null)
            {
                return;
            }

            float scale = Mathf.Lerp(0.75f, 1.1f, (seed % 100) / 100f);
            GameObject tuft = HighwayNatureGrassTuftMesh.CreateTuft(root, grassA, grassB, seed, scale);
            tuft.transform.localPosition = offset;
        }

        private static void SpawnBush(
            Transform root,
            HighwayNaturePrefabLibrary library,
            Vector3 offset,
            int seed)
        {
            GameObject prefab = library.PickBush(seed);
            if (prefab != null)
            {
                SpawnAt(root, prefab, offset, seed, 0.65f, 0.95f);
            }
        }

        private static void SpawnTree(
            Transform root,
            HighwayNaturePrefabLibrary library,
            Vector3 offset,
            int seed)
        {
            GameObject prefab = library.PickTree(seed);
            if (prefab != null)
            {
                SpawnAt(root, prefab, offset, seed, 0.9f, 1.2f);
            }
        }

        private static void SpawnTallTree(
            Transform root,
            HighwayNaturePrefabLibrary library,
            Vector3 offset,
            int seed)
        {
            GameObject prefab = library.PickTree(seed);
            if (prefab == null)
            {
                return;
            }

            // Scale kept moderate so the green canopy clears water (tall+low top = foliage in the river).
            GameObject instance = SpawnAt(root, prefab, offset, seed, 2.7f, 3.2f);
            AlignCanopyClearOfWater(
                instance,
                waterClearY: HighwayCountrysideRiverBuilder.WaterSurfaceY + 2.5f,
                preferredTopY: 1.8f,
                maxTopY: 3.2f);
        }

        /// <summary>
        /// Place canopy so tips sit near the deck while the foliage mass stays above the waterline.
        /// </summary>
        private static void AlignCanopyClearOfWater(
            GameObject instance,
            float waterClearY,
            float preferredTopY,
            float maxTopY)
        {
            if (instance == null)
            {
                return;
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            float height = Mathf.Max(0.5f, bounds.size.y);
            // Treat upper ~42% as foliage canopy (NSK2 trees).
            const float canopyFraction = 0.42f;
            float canopyHeight = height * canopyFraction;

            // Ideal: canopy bottom just above water, tip near deck.
            float targetTop = Mathf.Clamp(waterClearY + canopyHeight, preferredTopY, maxTopY);

            // If clamping to maxTop would still dunk foliage, nudge up past preferred deck tip.
            float canopyBottomAtMax = maxTopY - canopyHeight;
            if (canopyBottomAtMax < waterClearY)
            {
                targetTop = waterClearY + canopyHeight;
            }

            float deltaY = targetTop - bounds.max.y;
            instance.transform.position += new Vector3(0f, deltaY, 0f);
        }

        /// <summary>Shift tree so canopy tip sits at target Y.</summary>
        private static void AlignCanopyTop(GameObject instance, float targetTopY)
        {
            AlignCanopyClearOfWater(instance, targetTopY - 8f, targetTopY, targetTopY + 1f);
        }

        private static GameObject SpawnAt(
            Transform root,
            GameObject prefab,
            Vector3 offset,
            int seed,
            float minScale,
            float maxScale)
        {
            GameObject instance = Object.Instantiate(prefab, root);
            instance.name = prefab.name;
            instance.transform.localPosition = offset;
            instance.transform.localRotation = Quaternion.Euler(0f, (seed * 19) % 360, 0f);
            float t = (seed % 100) / 100f;
            instance.transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, t);
            HighwayPropUtility.StripColliders(instance);
            return instance;
        }

        private static Vector3 JitterXZ(int seed, float radiusX, float radiusZ)
        {
            float x = ((seed * 11) % 100 - 50) / 50f * radiusX;
            float z = ((seed * 17) % 100 - 50) / 50f * radiusZ;
            return new Vector3(x, 0f, z);
        }

        private static void BuildFallbackCanopy(Transform root, Palette palette, int seed, float segmentLength)
        {
            Material grassA = palette.GrassBladeA ?? palette.Foliage;
            if (grassA == null)
            {
                return;
            }

            Material grassB = palette.GrassBladeB ?? grassA;
            int count = 8 + seed % 4;
            for (int i = 0; i < count; i++)
            {
                int itemSeed = seed + i * 31;
                float z = segmentLength * (i + 0.5f) / count;
                float scale = Mathf.Lerp(3.2f, 4.6f, (itemSeed % 100) / 100f);
                GameObject tuft = HighwayNatureGrassTuftMesh.CreateTuft(root, grassA, grassB, itemSeed, scale);
                tuft.transform.localPosition = new Vector3(0f, CanopyBaseY + 8f, z);
            }
        }

        private static void BuildFallback(Transform root, ClusterKind kind, Palette palette, int seed)
        {
            Material grassA = palette.GrassBladeA ?? palette.Foliage;
            if (grassA == null)
            {
                return;
            }

            Material grassB = palette.GrassBladeB ?? grassA;
            int count = kind switch
            {
                ClusterKind.GrassPatch => 3 + seed % 2,
                ClusterKind.TreeGroup => 2 + seed % 2,
                _ => 2 + seed % 2
            };

            for (int i = 0; i < count; i++)
            {
                int itemSeed = seed + i * 31;
                Vector3 offset = JitterXZ(itemSeed, 2f, 3f);
                float scale = Mathf.Lerp(0.8f, 1.15f, (itemSeed % 100) / 100f);
                GameObject tuft = HighwayNatureGrassTuftMesh.CreateTuft(root, grassA, grassB, itemSeed, scale);
                tuft.transform.localPosition = offset;
            }
        }
    }
}
