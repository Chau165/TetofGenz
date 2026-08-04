using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Zone-organized roadside clusters — props, buildings, trees, backdrop filler.</summary>
    public class HighwayEnvironmentSpawner : MonoBehaviour
    {
        private enum ClusterType
        {
            RoadsideUtilities,
            Residential,
            ShopRow,
            TreeGrove,
            BackdropFiller
        }

        private struct ClusterSlot
        {
            public Transform Root;
            public bool LeftSide;
            public float BaseZ;
            public ClusterType Type;
            public int Seed;
        }

        [SerializeField] private int clusterCount = 24;
        [SerializeField] private float clusterSpacing = 40f;
        [SerializeField] private float spawnClearDistance = 55f;
        [SerializeField] private HighwayPrefabSet prefabSet = new HighwayPrefabSet();

        private readonly List<ClusterSlot> clusters = new List<ClusterSlot>();
        private HighwayMeshFactory.Palette proceduralPalette;
        private float scrollOffset;
        private bool usePrefabs;

        private static readonly ClusterType[] ClusterCycle =
        {
            ClusterType.RoadsideUtilities,
            ClusterType.Residential,
            ClusterType.TreeGrove,
            ClusterType.ShopRow,
            ClusterType.RoadsideUtilities,
            ClusterType.Residential,
            ClusterType.BackdropFiller,
            ClusterType.TreeGrove
        };

        public void Initialize(HighwayMeshFactory.Palette materials, HighwayPrefabSet prefabs = null)
        {
            proceduralPalette = materials;
            if (prefabs != null && prefabs.HasAnyContent)
            {
                prefabSet = prefabs;
            }

            usePrefabs = prefabSet != null && prefabSet.HasAnyContent;
            BuildClusters();
            RepositionClusters();
        }

        public void SetScrollOffset(float distance)
        {
            scrollOffset = distance;
            RepositionClusters();
        }

        private void BuildClusters()
        {
            ClearClusters();
            float z = spawnClearDistance + clusterSpacing;

            for (int i = 0; i < clusterCount; i++)
            {
                bool left = i % 2 == 0;
                ClusterType type = ClusterCycle[i % ClusterCycle.Length];
                int seed = i * 41 + (left ? 13 : 29);

                GameObject root = new GameObject("Cluster_" + i);
                root.transform.SetParent(transform, false);
                BuildCluster(root.transform, type, left, seed);

                clusters.Add(new ClusterSlot
                {
                    Root = root.transform,
                    LeftSide = left,
                    BaseZ = z,
                    Type = type,
                    Seed = seed
                });

                z += clusterSpacing;
            }
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
                    slot.Type = ClusterCycle[Mathf.RoundToInt(slot.BaseZ / clusterSpacing) % ClusterCycle.Length];
                    slot.Seed = Mathf.RoundToInt(slot.BaseZ) * 53 + (slot.LeftSide ? 17 : 31);
                    BuildCluster(slot.Root, slot.Type, slot.LeftSide, slot.Seed);
                    z = slot.BaseZ - scrollOffset;
                }

                float zAhead = Mathf.Max(0f, z);
                float scale = HighwayPerspective.ScaleAt(zAhead);
                slot.Root.localPosition = new Vector3(0f, HighwayLaneSettings.RoadSurfaceY, z);
                slot.Root.localScale = Vector3.one * scale;
                UpdateClusterPerspective(slot.Root, slot.LeftSide, zAhead);
                clusters[i] = slot;
            }
        }

        private void BuildCluster(Transform root, ClusterType type, bool leftSide, int seed)
        {
            HighwayPropUtility.ClearChildren(root);

            if (!usePrefabs)
            {
                BuildProceduralCluster(root, type, leftSide);
                return;
            }

            switch (type)
            {
                case ClusterType.RoadsideUtilities:
                    PlaceInZone(root, prefabSet.streetLights, 1, HighwayRoadsideZone.Props, leftSide, seed, 0f);
                    if (seed % 2 == 0)
                    {
                        PlaceInZone(root, prefabSet.utilityPoles, 1, HighwayRoadsideZone.Props, leftSide, seed + 1, 6f);
                    }

                    break;
                case ClusterType.Residential:
                    PlaceInZone(root, prefabSet.houses, 2, HighwayRoadsideZone.Buildings, leftSide, seed, 7f);
                    if (seed % 3 == 0)
                    {
                        PlaceInZone(root, prefabSet.fences, 1, HighwayRoadsideZone.Props, leftSide, seed + 2, 4f);
                    }

                    break;
                case ClusterType.ShopRow:
                    PlaceInZone(root, prefabSet.shops, 2, HighwayRoadsideZone.Buildings, leftSide, seed, 6f);
                    PlaceInZone(root, prefabSet.roadsideProps, 1, HighwayRoadsideZone.Props, leftSide, seed + 4, 3f);
                    break;
                case ClusterType.TreeGrove:
                    PlaceInZone(root, prefabSet.trees, 3, HighwayRoadsideZone.Trees, leftSide, seed, 5f);
                    PlaceInZone(root, prefabSet.vegetation, 2, HighwayRoadsideZone.Trees, leftSide, seed + 3, 4f, 1.8f);
                    break;
                case ClusterType.BackdropFiller:
                    PlaceInZone(root, prefabSet.skylineBuildings, 2, HighwayRoadsideZone.Backdrop, leftSide, seed, 9f);
                    PlaceInZone(root, prefabSet.trees, 2, HighwayRoadsideZone.Backdrop, leftSide, seed + 5, 7f);
                    break;
            }
        }

        private void PlaceInZone(
            Transform clusterRoot,
            GameObject[] options,
            int count,
            HighwayRoadsideZone zone,
            bool leftSide,
            int seed,
            float zSpacing,
            float xJitter = 0f)
        {
            if (options == null || options.Length == 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = HighwayPropUtility.PickPrefab(options, seed + i * 7);
                if (prefab == null)
                {
                    continue;
                }

                GameObject slot = new GameObject(prefab.name);
                slot.transform.SetParent(clusterRoot, false);
                HighwayPropUtility.SpawnPrefab(prefab, slot.transform, leftSide, seed + i);

                float zOffset = (i - (count - 1) * 0.5f) * zSpacing;
                float jitter = xJitter > 0f ? ((seed + i) % 5 - 2) * xJitter * 0.15f : 0f;
                float zoneX = HighwayLaneSettings.ZoneOffsetX(leftSide, zone, 0f);
                float xSign = leftSide ? -1f : 1f;
                slot.transform.localPosition = new Vector3(zoneX + xSign * jitter, 0f, zOffset);
                slot.AddComponent<HighwayZoneMarker>().Zone = zone;
            }
        }

        private static void UpdateClusterPerspective(Transform clusterRoot, bool leftSide, float zAhead)
        {
            if (clusterRoot == null)
            {
                return;
            }

            for (int i = 0; i < clusterRoot.childCount; i++)
            {
                Transform child = clusterRoot.GetChild(i);
                HighwayZoneMarker marker = child.GetComponent<HighwayZoneMarker>();
                if (marker == null)
                {
                    continue;
                }

                Vector3 pos = child.localPosition;
                float zoneX = HighwayLaneSettings.ZoneOffsetX(leftSide, marker.Zone, zAhead);
                float jitter = pos.x - HighwayLaneSettings.ZoneOffsetX(leftSide, marker.Zone, 0f);
                child.localPosition = new Vector3(zoneX + jitter, pos.y, pos.z);
            }
        }

        private void BuildProceduralCluster(Transform root, ClusterType type, bool leftSide)
        {
            HighwayMeshFactory.PropKind kind = type switch
            {
                ClusterType.RoadsideUtilities => HighwayMeshFactory.PropKind.StreetLight,
                ClusterType.Residential => HighwayMeshFactory.PropKind.House,
                ClusterType.ShopRow => HighwayMeshFactory.PropKind.Shop,
                ClusterType.TreeGrove => HighwayMeshFactory.PropKind.Tree,
                _ => HighwayMeshFactory.PropKind.CityBlock
            };

            HighwayRoadsideZone zone = type switch
            {
                ClusterType.RoadsideUtilities => HighwayRoadsideZone.Props,
                ClusterType.Residential or ClusterType.ShopRow => HighwayRoadsideZone.Buildings,
                ClusterType.TreeGrove => HighwayRoadsideZone.Trees,
                _ => HighwayRoadsideZone.Backdrop
            };

            GameObject slot = new GameObject("Procedural");
            slot.transform.SetParent(root, false);
            HighwayMeshFactory.Build(slot.transform, kind, proceduralPalette, leftSide);
            float zoneX = HighwayLaneSettings.ZoneOffsetX(leftSide, zone, 0f);
            slot.transform.localPosition = new Vector3(
                HighwayLaneSettings.ZoneOffsetX(leftSide, zone, 0f), 0f, 0f);
            slot.AddComponent<HighwayZoneMarker>().Zone = zone;
        }

        private void ClearClusters()
        {
            for (int i = 0; i < clusters.Count; i++)
            {
                if (clusters[i].Root != null)
                {
                    SafeDestroy(clusters[i].Root.gameObject);
                }
            }

            clusters.Clear();
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(obj);
                return;
            }
#endif
            Destroy(obj);
        }
    }
}
