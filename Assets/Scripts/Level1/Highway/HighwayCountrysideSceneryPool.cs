using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Scrolling forest / river belts — two simple biomes only.</summary>
    public class HighwayCountrysideSceneryPool : MonoBehaviour
    {
        public enum CountrysideBiome
        {
            DenseForest,
            River,
            ForestRiverAlternating
        }

        private struct ScenerySlot
        {
            public Transform Root;
            public float BaseZ;
            public CountrysideBiome Biome;
            public int Seed;
        }

        private const string LibraryAssetPath = "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset";
        private const float SegmentLength = HighwayRoadLayout.SegmentLength;
        private const float ForestStretch = 180f;
        private const float RiverStretch = 140f;

        [SerializeField] private int segmentCount = 10;
        [SerializeField] private CountrysideBiome masterVariation = CountrysideBiome.ForestRiverAlternating;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Material bankMaterial;
        [SerializeField] private Material foliageMaterial;
        [SerializeField] private Material barkMaterial;
        [SerializeField] private Material grassBladeA;
        [SerializeField] private Material grassBladeB;
        [SerializeField] private HighwayNaturePrefabLibrary prefabLibrary;

        private readonly List<ScenerySlot> slots = new List<ScenerySlot>();
        private HighwayNatureClusterBuilder.Palette palette;
        private float scrollOffset;
        private Transform packOceanRoot;

        private static readonly float[] ForestRowOffsets =
        {
            // Gap between barrier and first canopy so water reads over the edge.
            16f, 26f, 38f, 52f
        };

        private void Awake()
        {
            EnsureMaterials();
            EnsurePrefabLibrary();
        }

        public void Initialize(
            Material water,
            Material bank,
            Material foliage,
            Material bark,
            Material grassA,
            HighwayNaturePrefabLibrary library = null,
            Material grassB = null,
            CountrysideBiome variation = CountrysideBiome.ForestRiverAlternating)
        {
            waterMaterial = water;
            bankMaterial = bank;
            foliageMaterial = foliage;
            barkMaterial = bark;
            grassBladeA = grassA;
            grassBladeB = grassB ?? grassA;
            masterVariation = variation;
            if (library != null)
            {
                prefabLibrary = library;
            }

            EnsureMaterials();
            EnsurePrefabLibrary();
            RebuildAll();
        }

        public void RebuildAll()
        {
            ClearSlots();

            HighwayPlayerVehicle player = Object.FindFirstObjectByType<HighwayPlayerVehicle>();
            if (player != null)
            {
                scrollOffset = player.transform.position.z;
            }
            else if (Camera.main != null)
            {
                scrollOffset = Camera.main.transform.position.z;
            }

            scrollOffset = Mathf.Max(0f, scrollOffset);

            EnsurePackOcean();
            BuildSlots();
            RepositionSlots();
        }

        public void SetScrollOffset(float distance)
        {
            if (slots.Count == 0)
            {
                scrollOffset = distance;
                EnsurePackOcean();
                BuildSlots();
            }

            scrollOffset = distance;
            RepositionSlots();
            FollowPackOcean();
        }

        private void EnsurePackOcean()
        {
            if (packOceanRoot != null)
            {
                FollowPackOcean();
                return;
            }

            Transform existing = transform.Find("PackLowPolyWater");
            if (existing != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(existing.gameObject);
#else
                HighwayPropUtility.SafeDestroy(existing.gameObject);
#endif
            }

            packOceanRoot = HighwayCountrysideRiverBuilder.EnsurePackOcean(transform, waterMaterial);
            FollowPackOcean();
        }

        private void FollowPackOcean()
        {
            if (packOceanRoot == null)
            {
                return;
            }

            // Keep ocean under the active stretch (scroll), not a stale far Z.
            float z = scrollOffset;
            Camera cam = Camera.main;
            if (cam != null)
            {
                z = cam.transform.position.z;
            }

            HighwayCountrysideRiverBuilder.FollowCamera(packOceanRoot, z);
        }

        private void EnsureMaterials()
        {
#if UNITY_EDITOR
            if (waterMaterial == null)
            {
                waterMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/LowPolyWater_Pack/Materials/LowPolyWaterMaterial.mat")
                    ?? HighwayRoadMaterialDefaults.LoadScene(HighwaySceneAssetPaths.WaterMat)
                    ?? HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.WaterFile);
            }

            if (bankMaterial == null)
            {
                bankMaterial = HighwayRoadMaterialDefaults.LoadScene(HighwaySceneAssetPaths.GrassGroundMat)
                    ?? HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassNearFile);
            }

            if (foliageMaterial == null)
            {
                foliageMaterial = HighwayRoadMaterialDefaults.LoadScene(HighwaySceneAssetPaths.FoliageMat)
                    ?? HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.FoliageFile);
            }

            if (barkMaterial == null)
            {
                barkMaterial = HighwayRoadMaterialDefaults.LoadScene(HighwaySceneAssetPaths.BarkMat)
                    ?? HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.BarkFile);
            }

            if (grassBladeA == null)
            {
                grassBladeA = HighwayRoadMaterialDefaults.LoadScene(HighwaySceneAssetPaths.GrassBladeAMat)
                    ?? HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassBlade01File);
            }

            if (grassBladeB == null)
            {
                grassBladeB = HighwayRoadMaterialDefaults.LoadScene(HighwaySceneAssetPaths.GrassBladeBMat)
                    ?? HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassBlade02File);
            }
#else
            if (waterMaterial == null)
            {
                waterMaterial = CreateFallbackMaterial(new Color(0.2f, 0.46f, 0.56f, 0.88f), 0.9f);
            }

            if (bankMaterial == null)
            {
                bankMaterial = CreateFallbackMaterial(new Color(0.36f, 0.52f, 0.28f), 0.2f);
            }

            if (foliageMaterial == null)
            {
                foliageMaterial = CreateFallbackMaterial(new Color(0.22f, 0.48f, 0.2f), 0.15f);
            }

            if (grassBladeA == null)
            {
                grassBladeA = foliageMaterial;
            }

            if (grassBladeB == null)
            {
                grassBladeB = grassBladeA;
            }
#endif

            palette = new HighwayNatureClusterBuilder.Palette
            {
                Foliage = foliageMaterial,
                Bark = barkMaterial,
                GrassBladeA = grassBladeA,
                GrassBladeB = grassBladeB ?? grassBladeA
            };
        }

        private static Material CreateFallbackMaterial(Color color, float smoothness)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        private void EnsurePrefabLibrary()
        {
#if UNITY_EDITOR
            if (prefabLibrary == null)
            {
                prefabLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(LibraryAssetPath);
            }
#endif
        }

        private void BuildSlots()
        {
            ClearSlotsKeepOcean();
            // Absolute world Z around the player (same model as the road pool).
            float z = scrollOffset - SegmentLength;

            for (int i = 0; i < segmentCount; i++)
            {
                CountrysideBiome biome = PickBiome(z);
                int seed = Mathf.RoundToInt(z) * 67 + 13;

                GameObject root = new GameObject("Countryside_" + i);
                root.transform.SetParent(transform, false);
                BuildSegmentContent(root.transform, biome, seed, SegmentLength);

                slots.Add(new ScenerySlot
                {
                    Root = root.transform,
                    BaseZ = z,
                    Biome = biome,
                    Seed = seed
                });

                z += SegmentLength;
            }

            RepositionSlots();
        }

        private void ClearSlotsKeepOcean()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Root != null)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(slots[i].Root.gameObject);
#else
                    HighwayPropUtility.SafeDestroy(slots[i].Root.gameObject);
#endif
                }
            }

            slots.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null && child.name.StartsWith("Countryside_"))
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(child.gameObject);
#else
                    HighwayPropUtility.SafeDestroy(child.gameObject);
#endif
                }
            }
        }

        private void BuildSegmentContent(Transform root, CountrysideBiome biome, int seed, float length)
        {
            HighwayPropUtility.ClearChildren(root);

            switch (biome)
            {
                case CountrysideBiome.DenseForest:
                    BuildDenseForest(root, seed, length);
                    break;
                case CountrysideBiome.River:
                    BuildRiver(root, seed, length);
                    break;
                default:
                    if (seed % 2 == 0)
                    {
                        BuildDenseForest(root, seed, length);
                    }
                    else
                    {
                        BuildRiver(root, seed, length);
                    }

                    break;
            }
        }

        private void BuildDenseForest(Transform root, int seed, float length)
        {
            // Shared PackLowPolyWater provides the river; segments only add canopy.
            BuildForestSide(root, true, seed, length);
            BuildForestSide(root, false, seed + 3, length);
        }

        private void BuildForestSide(Transform root, bool leftSide, int seed, float length)
        {
            GameObject side = new GameObject(leftSide ? "Forest_L" : "Forest_R");
            side.transform.SetParent(root, false);

            for (int i = 0; i < ForestRowOffsets.Length; i++)
            {
                GameObject row = new GameObject("CanopyRow_" + i);
                row.transform.SetParent(side.transform, false);
                float signedX = HighwayRoadsideLayout.SignedWorldX(leftSide, ForestRowOffsets[i]);
                row.transform.localPosition = new Vector3(signedX, 0f, 0f);
                HighwayNatureClusterBuilder.BuildDenseCanopyStrip(
                    row.transform,
                    prefabLibrary,
                    palette,
                    seed + i * 29,
                    length);
            }
        }

        private void BuildRiver(Transform root, int seed, float length)
        {
            HighwayCountrysideRiverBuilder.BuildFullRiver(
                root,
                length,
                seed,
                waterMaterial,
                bankMaterial,
                prefabLibrary,
                palette);
        }

        private CountrysideBiome PickBiome(float worldZ)
        {
            if (masterVariation == CountrysideBiome.DenseForest)
            {
                return CountrysideBiome.DenseForest;
            }

            if (masterVariation == CountrysideBiome.River)
            {
                return CountrysideBiome.River;
            }

            float cycle = ForestStretch + RiverStretch;
            return worldZ % cycle < ForestStretch
                ? CountrysideBiome.DenseForest
                : CountrysideBiome.River;
        }

        private void RepositionSlots()
        {
            float recycleBehind = SegmentLength * 2f;
            float maxBaseZ = 0f;
            for (int i = 0; i < slots.Count; i++)
            {
                maxBaseZ = Mathf.Max(maxBaseZ, slots[i].BaseZ);
            }

            for (int i = 0; i < slots.Count; i++)
            {
                ScenerySlot slot = slots[i];

                // Place at absolute world Z (player moves forward in world space).
                while (slot.BaseZ < scrollOffset - recycleBehind)
                {
                    slot.BaseZ = maxBaseZ + SegmentLength;
                    maxBaseZ = slot.BaseZ;
                    slot.Biome = PickBiome(slot.BaseZ);
                    slot.Seed = Mathf.RoundToInt(slot.BaseZ) * 59 + 11;
                    BuildSegmentContent(slot.Root, slot.Biome, slot.Seed, SegmentLength);
                }

                slot.Root.localPosition = new Vector3(0f, 0f, slot.BaseZ);
                slots[i] = slot;
            }

            FollowPackOcean();
        }

        private void ClearSlots()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Root != null)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(slots[i].Root.gameObject);
#else
                    HighwayPropUtility.SafeDestroy(slots[i].Root.gameObject);
#endif
                }
            }

            slots.Clear();

            // Sweep any orphaned countryside roots left by deferred Destroy.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("Countryside_"))
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(child.gameObject);
#else
                    HighwayPropUtility.SafeDestroy(child.gameObject);
#endif
                }
            }

            // Recreate pack ocean on next EnsurePackOcean after full rebuild.
            if (packOceanRoot != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(packOceanRoot.gameObject);
#else
                HighwayPropUtility.SafeDestroy(packOceanRoot.gameObject);
#endif
                packOceanRoot = null;
            }
        }
    }
}
