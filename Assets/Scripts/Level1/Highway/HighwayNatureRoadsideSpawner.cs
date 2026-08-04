using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Layered roadside vegetation (bushes/trees) for a continuous landscape look.</summary>
    public class HighwayNatureRoadsideSpawner : MonoBehaviour
    {
        private struct ClusterSlot
        {
            public Transform Root;
            public bool LeftSide;
            public float BaseZ;
            public HighwayNatureClusterBuilder.ClusterKind Kind;
            public int Seed;
        }

        private const string LibraryAssetPath = "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset";

        [SerializeField] private int clusterCount = 20;
        [SerializeField] private float clusterSpacing = 36f;
        [SerializeField] private bool treesOnlyPreset;
        [SerializeField] private float spawnClearDistance = 54f;
        [SerializeField] private Material foliageMaterial;
        [SerializeField] private Material barkMaterial;
        [SerializeField] private Material grassBladeA;
        [SerializeField] private Material grassBladeB;
        [SerializeField] private HighwayNaturePrefabLibrary prefabLibrary;

        private readonly List<ClusterSlot> clusters = new List<ClusterSlot>();
        private HighwayNatureClusterBuilder.Palette palette;
        private float scrollOffset;

        private static readonly HighwayNatureClusterBuilder.ClusterKind[] ClusterCycle =
        {
            HighwayNatureClusterBuilder.ClusterKind.BushGroup,
            HighwayNatureClusterBuilder.ClusterKind.TreeGroup,
            HighwayNatureClusterBuilder.ClusterKind.BushGroup,
            HighwayNatureClusterBuilder.ClusterKind.TreeGroup,
            HighwayNatureClusterBuilder.ClusterKind.Backdrop
        };

        private void Awake()
        {
            EnsureMaterials();
            EnsurePrefabLibrary();
            if (clusters.Count == 0)
            {
                BuildClusters();
            }
        }

        public void InitializeFromSideGround(HighwaySideGroundPool sideGround)
        {
            if (sideGround == null)
            {
                return;
            }

            grassBladeA = sideGround.SharedGrassBladeA;
            grassBladeB = sideGround.SharedGrassBladeB ?? grassBladeA;
            foliageMaterial = foliageMaterial ?? grassBladeA;
            barkMaterial = barkMaterial ?? grassBladeA;
            if (sideGround.SharedPrefabLibrary != null)
            {
                prefabLibrary = sideGround.SharedPrefabLibrary;
            }

            EnsureMaterials();
            EnsurePrefabLibrary();
            RebuildClusters();
        }

        public void Initialize(
            Material foliage,
            Material bark,
            Material grassBladeA,
            HighwayNaturePrefabLibrary library = null,
            Material grassBladeB = null)
        {
            foliageMaterial = foliage;
            barkMaterial = bark;
            this.grassBladeA = grassBladeA;
            grassBladeB = grassBladeB ?? grassBladeA;
            if (library != null)
            {
                prefabLibrary = library;
            }

            EnsureMaterials();
            EnsurePrefabLibrary();
            BuildClusters();
        }

        public void RebuildClusters()
        {
            EnsureMaterials();
            EnsurePrefabLibrary();
            ClearClusters();
            BuildClusters();
            RepositionClusters();
        }

        public void SetScrollOffset(float distance)
        {
            scrollOffset = distance;
            RepositionClusters();
        }

        /// <summary>Dense tree belts for hometown arrival — scrolls with the vehicle like the highway.</summary>
        public void ApplyArrivalForestPreset()
        {
            clusterCount = 52;
            clusterSpacing = 14f;
            spawnClearDistance = 14f;
            treesOnlyPreset = true;
            RebuildClusters();
        }

        private void EnsureMaterials()
        {
#if UNITY_EDITOR
            if (foliageMaterial == null)
            {
                foliageMaterial = HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.FoliageFile);
            }

            if (barkMaterial == null)
            {
                barkMaterial = HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.BarkFile);
            }

            if (grassBladeA == null)
            {
                grassBladeA = HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassBlade01File);
            }

            if (grassBladeB == null)
            {
                grassBladeB = HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassBlade02File);
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

        private void EnsurePrefabLibrary()
        {
#if UNITY_EDITOR
            if (prefabLibrary == null)
            {
                prefabLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(LibraryAssetPath);
            }
#endif
        }

        private void BuildClusters()
        {
            ClearClusters();
            float z = spawnClearDistance + clusterSpacing * 0.35f;

            for (int i = 0; i < clusterCount; i++)
            {
                bool left = i % 2 == 0;
                var kind = treesOnlyPreset
                    ? (i % 5 == 4
                        ? HighwayNatureClusterBuilder.ClusterKind.Backdrop
                        : HighwayNatureClusterBuilder.ClusterKind.TreeGroup)
                    : ClusterCycle[i % ClusterCycle.Length];
                int seed = i * 41 + (left ? 17 : 29);
                float spacingJitter = clusterSpacing * (0.82f + (seed % 7) * 0.03f);

                GameObject root = new GameObject("NatureCluster_" + (left ? "L" : "R") + "_" + i);
                root.transform.SetParent(transform, false);
                BuildClusterContent(root.transform, kind, left, seed, z);

                clusters.Add(new ClusterSlot
                {
                    Root = root.transform,
                    LeftSide = left,
                    BaseZ = z,
                    Kind = kind,
                    Seed = seed
                });

                z += spacingJitter;
            }

            RepositionClusters();
        }

        private void BuildClusterContent(
            Transform root,
            HighwayNatureClusterBuilder.ClusterKind kind,
            bool leftSide,
            int seed,
            float z)
        {
            HighwayNatureClusterBuilder.Build(root, kind, prefabLibrary, palette, seed);
            PlaceClusterOnRoadside(root, kind, leftSide, seed, z);
        }

        private static void PlaceClusterOnRoadside(
            Transform root,
            HighwayNatureClusterBuilder.ClusterKind kind,
            bool leftSide,
            int seed,
            float z)
        {
            float outward = ZoneOutward(kind);
            float signedX = HighwayRoadsideLayout.SignedWorldX(leftSide, outward);
            root.localPosition = new Vector3(signedX, HighwayLaneSettings.RoadSurfaceY, z);

            float zJitter = ((seed % 9) - 4) * 1.2f;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                Vector3 local = child.localPosition;
                float childOutward = 0.8f + ((seed + i * 5) % 4) * 0.45f;
                float childSignedX = HighwayRoadsideLayout.SignedWorldX(leftSide, outward + childOutward);
                child.localPosition = new Vector3(
                    childSignedX - signedX,
                    local.y,
                    local.z + zJitter);
            }
        }

        private static float ZoneOutward(HighwayNatureClusterBuilder.ClusterKind kind)
        {
            return kind switch
            {
                HighwayNatureClusterBuilder.ClusterKind.GrassPatch =>
                    HighwayRoadsideLayout.GrassClusterMidOutward,
                HighwayNatureClusterBuilder.ClusterKind.BushGroup =>
                    HighwayRoadsideLayout.BushZoneOutward,
                HighwayNatureClusterBuilder.ClusterKind.TreeGroup =>
                    HighwayRoadsideLayout.TreeZoneOutward,
                _ => HighwayRoadsideLayout.BackdropZoneOutward
            };
        }

        private void RepositionClusters()
        {
            float recycleBehind = clusterSpacing * 2f;
            float maxBaseZ = 0f;
            for (int i = 0; i < clusters.Count; i++)
            {
                maxBaseZ = Mathf.Max(maxBaseZ, clusters[i].BaseZ);
            }

            for (int i = 0; i < clusters.Count; i++)
            {
                ClusterSlot slot = clusters[i];
                float z = slot.BaseZ - scrollOffset;

                while (z < -recycleBehind)
                {
                    slot.BaseZ += maxBaseZ + clusterSpacing;
                    slot.Kind = treesOnlyPreset
                        ? (Mathf.Abs(Mathf.RoundToInt(slot.BaseZ)) % 5 == 4
                            ? HighwayNatureClusterBuilder.ClusterKind.Backdrop
                            : HighwayNatureClusterBuilder.ClusterKind.TreeGroup)
                        : ClusterCycle[Mathf.Abs(Mathf.RoundToInt(slot.BaseZ)) % ClusterCycle.Length];
                    slot.Seed = Mathf.RoundToInt(slot.BaseZ) * 53 + (slot.LeftSide ? 19 : 31);
                    BuildClusterContent(slot.Root, slot.Kind, slot.LeftSide, slot.Seed, slot.BaseZ - scrollOffset);
                    z = slot.BaseZ - scrollOffset;
                }

                PlaceClusterOnRoadside(slot.Root, slot.Kind, slot.LeftSide, slot.Seed, z);
                clusters[i] = slot;
            }
        }

        private void ClearClusters()
        {
            for (int i = 0; i < clusters.Count; i++)
            {
                if (clusters[i].Root != null)
                {
                    HighwayPropUtility.SafeDestroy(clusters[i].Root.gameObject);
                }
            }

            clusters.Clear();
        }
    }
}
