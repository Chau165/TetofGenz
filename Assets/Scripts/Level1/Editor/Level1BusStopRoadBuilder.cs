#if UNITY_EDITOR
using ChoTet.Level1;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Extended horizontal highway + localized bus-stop pad for Level 1 Phase 2.</summary>
    public static class Level1BusStopRoadBuilder
    {
        private const float AsphaltThickness = 0.12f;
        private const float SidewalkThickness = 0.1f;
        private const float CurbDepth = 0.22f;
        private const float CurbHeight = 0.16f;
        private const float DashSpacing = 3.6f;
        private const int MinDashCount = 4;
        private const int MaxDashCount = 50;

        public static float AsphaltSurfaceY => Level1BusStopRoadSettings.AsphaltSurfaceY;

        public static GameObject Build(Transform parent)
        {
            Material asphalt = LoadMaterial("Road_Ground.mat");
            Material sidewalk = LoadMaterial("Foundation_Stone.mat");
            Material curb = LoadMaterial("Wall_Shadow.mat");
            Material laneMark = LoadMaterial("Wall_Light.mat");
            Material grassShoulder = LoadMaterial("Foundation_Stone.mat");
            Material dirtShoulder = LoadMaterial("Home_Dirt.mat");

            GameObject root = new GameObject("BusStopRoadLayout");
            root.transform.SetParent(parent, false);

            float roadLength = Level1BusStopRoadSettings.RoadLength + 1.2f;
            float roadDepth = Level1BusStopRoadSettings.LaneSouthEdgeZ - Level1BusStopRoadSettings.LaneNorthEdgeZ + 0.25f;
            float roadCenterX = Level1BusStopRoadSettings.RoadCenterX;
            float roadCenterZ = Level1BusStopRoadSettings.RoadCenterZ;

            BuildTiledSlab(root.transform, "AsphaltRoadSurface", asphalt,
                new Vector3(roadCenterX, AsphaltSurfaceY - AsphaltThickness * 0.5f, roadCenterZ),
                new Vector3(roadLength, AsphaltThickness, roadDepth), 6f);

            BuildSlab(root.transform, "CenterLaneDivider", laneMark,
                new Vector3(roadCenterX, AsphaltSurfaceY + 0.013f, roadCenterZ),
                new Vector3(roadLength + 0.15f, 0.022f, Level1BusStopRoadSettings.CenterDividerWidth));

            BuildShoulders(root.transform, roadCenterX, roadLength, grassShoulder, dirtShoulder);
            BuildBusStopPad(root.transform, sidewalk, curb, roadCenterX);
            BuildLaneMarkings(root.transform, laneMark, roadCenterX, roadLength);
            BuildCrosswalk(root.transform, laneMark);
            BuildPlayerRoadBoundaries(root.transform);
            Level1BusStopRoadsideBarrier.EnsureUnder(root.transform);

            return root;
        }

        private static void BuildShoulders(Transform parent, float roadCenterX, float roadLength,
            Material grassShoulder, Material dirtShoulder)
        {
            float shoulderDepth = Level1BusStopRoadSettings.ShoulderDepth;
            float northZ = Level1BusStopRoadSettings.LaneNorthEdgeZ - shoulderDepth * 0.5f - 0.06f;
            BuildTiledSlab(parent, "RoadShoulderNorth", grassShoulder,
                new Vector3(roadCenterX, Level1BusStopRoadSettings.SidewalkSurfaceY - 0.015f, northZ),
                new Vector3(roadLength, 0.07f, shoulderDepth), 8f);

            float southZ = Level1BusStopRoadSettings.LaneSouthEdgeZ + shoulderDepth * 0.5f + 0.06f;
            BuildTiledSlab(parent, "RoadShoulderSouth", dirtShoulder,
                new Vector3(roadCenterX, Level1BusStopRoadSettings.SidewalkSurfaceY - 0.02f, southZ),
                new Vector3(roadLength, 0.06f, shoulderDepth), 8f);
        }

        private static void BuildBusStopPad(Transform parent, Material sidewalk, Material curb, float roadCenterX)
        {
            float padWidth = Level1BusStopRoadSettings.BusStopPadHalfX * 2f;
            float sidewalkDepth = Level1BusStopRoadSettings.SidewalkFarZ - Level1BusStopRoadSettings.SidewalkNearZ;
            float sidewalkCenterZ = Level1BusStopRoadSettings.SidewalkCenterZ;

            BuildSlab(parent, "BusStopSidewalk", sidewalk,
                new Vector3(roadCenterX, Level1BusStopRoadSettings.SidewalkSurfaceY - SidewalkThickness * 0.5f,
                    sidewalkCenterZ),
                new Vector3(padWidth + 0.35f, SidewalkThickness, sidewalkDepth + 0.35f));

            float curbZ = Level1BusStopRoadSettings.SidewalkNearZ - CurbDepth * 0.5f;
            BuildSlab(parent, "BusStopCurb", curb,
                new Vector3(roadCenterX, Level1BusStopRoadSettings.SidewalkSurfaceY + CurbHeight * 0.5f - 0.02f, curbZ),
                new Vector3(padWidth + 0.2f, CurbHeight, CurbDepth));
        }

        private static void BuildLaneMarkings(Transform parent, Material laneMark, float roadCenterX, float roadLength)
        {
            GameObject marks = new GameObject("LaneMarkings");
            marks.transform.SetParent(parent, false);

            int dashCount = Mathf.Clamp(Mathf.FloorToInt(roadLength / DashSpacing) + 1, MinDashCount, MaxDashCount);
            float half = roadLength * 0.5f;
            float startX = roadCenterX - half + 0.15f;
            float endX = roadCenterX + half - 0.15f;
            float[] laneCenters =
            {
                Level1BusStopRoadSettings.LaneNorthCenterZ,
                Level1BusStopRoadSettings.LaneSouthCenterZ
            };

            for (int lane = 0; lane < laneCenters.Length; lane++)
            {
                for (int i = 0; i < dashCount; i++)
                {
                    float t = dashCount <= 1 ? 0.5f : i / (float)(dashCount - 1);
                    float x = Mathf.Lerp(startX, endX, t);
                    BuildSlab(marks.transform, "LaneDash_" + lane + "_" + i, laneMark,
                        new Vector3(x, AsphaltSurfaceY + 0.012f, laneCenters[lane]),
                        new Vector3(1.15f, 0.02f, 0.1f));
                }
            }

            BuildSlab(marks.transform, "LaneEdgeN", laneMark,
                new Vector3(roadCenterX, AsphaltSurfaceY + 0.011f,
                    Level1BusStopRoadSettings.LaneNorthEdgeZ + 0.06f),
                new Vector3(roadLength + 0.2f, 0.018f, 0.06f));
            BuildSlab(marks.transform, "LaneEdgeS", laneMark,
                new Vector3(roadCenterX, AsphaltSurfaceY + 0.011f,
                    Level1BusStopRoadSettings.LaneSouthEdgeZ - 0.06f),
                new Vector3(roadLength + 0.2f, 0.018f, 0.06f));
        }

        private static void BuildCrosswalk(Transform parent, Material laneMark)
        {
            GameObject crosswalk = new GameObject("BusStopCrosswalk");
            crosswalk.transform.SetParent(parent, false);

            float startZ = Level1BusStopRoadSettings.SidewalkNearZ + 0.06f;
            float endZ = Level1BusStopRoadSettings.LaneSouthEdgeZ + 0.12f;
            int barCount = 4;
            for (int i = 0; i < barCount; i++)
            {
                float t = barCount <= 1 ? 0.5f : i / (float)(barCount - 1);
                float z = Mathf.Lerp(startZ, endZ, t);
                BuildSlab(crosswalk.transform, "CrosswalkBar_" + i, laneMark,
                    new Vector3(Level1BusStopRoadSettings.CrosswalkCenterX, AsphaltSurfaceY + 0.013f, z),
                    new Vector3(0.12f, 0.02f, 0.34f));
            }
        }

        private static void BuildPlayerRoadBoundaries(Transform parent)
        {
            GameObject bounds = new GameObject("BusStopRoadBoundaries");
            bounds.transform.SetParent(parent, false);

            float playableHalf = Level1BusStopRoadSettings.PlayableHalfX + 1.6f;
            float wallHeight = 2.4f;
            float northEdgeZ = Level1BusStopRoadSettings.LaneNorthEdgeZ - 0.12f;

            CreateInvisibleWall(bounds.transform, "RoadEdgeBlockerN",
                new Vector3(0f, wallHeight * 0.5f, northEdgeZ),
                new Vector3(playableHalf * 2f, wallHeight, 0.32f));

            // South edge left open — Level1OutdoorLivesController + road walk floor.
        }

        private static void CreateInvisibleWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            Object.DestroyImmediate(wall.GetComponent<Renderer>());
        }

        private static GameObject BuildTiledSlab(Transform parent, string name, Material source, Vector3 position,
            Vector3 scale, float metersPerTile)
        {
            GameObject slab = BuildSlab(parent, name, null, position, scale);
            Material tiled = new Material(source);
            tiled.SetTextureScale("_BaseMap", new Vector2(
                Mathf.Max(1f, scale.x / metersPerTile),
                Mathf.Max(1f, scale.z / metersPerTile)));
            Renderer renderer = slab.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = tiled;
            }

            return slab;
        }

        private static GameObject BuildSlab(Transform parent, string name, Material material, Vector3 position,
            Vector3 scale)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, false);
            slab.transform.position = position;
            slab.transform.localScale = scale;
            Object.DestroyImmediate(slab.GetComponent<Collider>());

            if (material != null)
            {
                Renderer renderer = slab.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                }
            }

            return slab;
        }

        private static Material LoadMaterial(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(
                ChoTet.Runtime.ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
        }
    }
}
#endif
