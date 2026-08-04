using ChoTet.Level1.HometownArrival;
using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// HometownArrival-style backdrop for ChoTet: grass fill outside the play area plus baked tree belts.
    /// </summary>
    public static class ChoTetWorldBackdropDressing
    {
        private static Material grassMaterial;

        public static void Apply(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            DestroyChild(worldRoot, "GardenForestBackdrop");
            DestroyChild(worldRoot, "GardenHometownDressing");
            DestroyChild(worldRoot, "ChoTetWorldBackdrop");

            Transform gardenArea = worldRoot.Find("GardenArea");
            if (gardenArea != null)
            {
                DestroyChild(gardenArea, "GardenForestBackdrop");
                DestroyChild(gardenArea, "GardenHometownDressing");
            }

            RestoreLegacyTerrainPieces();
            ResolveMaterials();

            GameObject root = new GameObject("ChoTetWorldBackdrop");
            root.transform.SetParent(worldRoot, false);
            root.transform.localPosition = Vector3.zero;

            HideSeamTerrainLips();
            SpawnGrassBackplates(root.transform);
            SpawnMarketNorthSeamlessGrass(root.transform);
            SpawnRoadAndMarketTreeBelts(root.transform);
            SpawnTempleTreeBelts(root.transform);
            SpawnMarketNorthBoundary(root.transform);
            SpawnMarketWestBoundary(root.transform);
            SpawnMarketLeftBackdrop(root.transform);
            SpawnFlowerSellerBoundary(root.transform);
        }

        private static void ResolveMaterials()
        {
            grassMaterial = HometownArrivalResourceCache.LoadMaterial("Grass_Ground.mat")
                ?? HometownArrivalResourceCache.LoadMaterial("Highway_Grass_NSK2.mat")
                ?? HometownArrivalResourceCache.LoadMaterial("Garden_Ground.mat");
        }

        private static void RestoreLegacyTerrainPieces()
        {
            string[] names =
            {
                "TerrainLipEast", "TerrainLipWest", "TerrainLipNorth", "TerrainLipSouth",
                "CliffEast_1", "CliffEast_2", "CliffWest_1", "CliffWest_2",
                "BorderRock_1", "BorderRock_2", "BorderBush_1", "BorderBush_2"
            };

            for (int i = 0; i < names.Length; i++)
            {
                GameObject target = GameObject.Find(names[i]);
                if (target != null)
                {
                    target.SetActive(true);
                }
            }
        }

        private static void HideSeamTerrainLips()
        {
            // Dirt lips create a visible step/seam against the market grass pad.
            string[] hideNames = { "TerrainLipNorth" };
            for (int i = 0; i < hideNames.Length; i++)
            {
                GameObject target = GameObject.Find(hideNames[i]);
                if (target != null)
                {
                    target.SetActive(false);
                }
            }
        }

        private static void SpawnGrassBackplates(Transform parent)
        {
            if (grassMaterial == null)
            {
                return;
            }

            // Match MarketGround height (-0.47) so north fill does not read as a stepped seam.
            const float grassY = -0.47f;
            CreateGrassPanel(parent, "GrassBackplate_East", new Vector3(74f, grassY, 58f), new Vector3(92f, 0.12f, 134f), stripCollider: true);
            CreateGrassPanel(parent, "GrassBackplate_West", new Vector3(-74f, grassY, 58f), new Vector3(92f, 0.12f, 134f), stripCollider: true);
            // Stop before the temple courtyard (x≈25–85, z≈96+) to prevent grass flicker.
            CreateGrassPanel(parent, "GrassBackplate_NorthMarket", new Vector3(0f, grassY, 118f), new Vector3(48f, 0.12f, 36f), stripCollider: true);
            CreateGrassPanel(parent, "GrassBackplate_South", new Vector3(0f, grassY, -22f), new Vector3(120f, 0.12f, 42f), stripCollider: true);
            // Keep temple grass well below the courtyard pad to avoid z-fight flicker.
            const float templeGrassY = -0.72f;
            CreateGrassPanel(parent, "GrassBackplate_TempleEast", new Vector3(118f, templeGrassY, 138f), new Vector3(48f, 0.12f, 100f), stripCollider: true);
            CreateGrassPanel(parent, "GrassBackplate_TempleWest", new Vector3(-18f, templeGrassY, 138f), new Vector3(48f, 0.12f, 100f), stripCollider: true);
            CreateGrassPanel(parent, "GrassBackplate_TempleNorth", new Vector3(55f, templeGrassY, 198f), new Vector3(110f, 0.12f, 48f), stripCollider: true);
        }

        private static void SpawnMarketNorthSeamlessGrass(Transform parent)
        {
            if (grassMaterial == null)
            {
                return;
            }

            // Overlap the market pad so the north edge is one continuous grass surface.
            CreateGrassPanel(parent, "MarketNorthGrassBlend", new Vector3(0f, -0.46f, 112f), new Vector3(56f, 0.1f, 28f), stripCollider: true);
        }

        private static void CreateGrassPanel(Transform parent, string name, Vector3 position, Vector3 scale, bool stripCollider)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.SetParent(parent, false);
            panel.transform.position = position;
            panel.transform.localScale = scale;

            if (stripCollider)
            {
                Collider collider = panel.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyCollider(collider);
                }
            }

            Renderer renderer = panel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = grassMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }
        }

        private static void DestroyCollider(Collider collider)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(collider);
                return;
            }
#endif
            Object.Destroy(collider);
        }

        private static void SpawnRoadAndMarketTreeBelts(Transform parent)
        {
            HometownArrivalCountrysidePropFactory.EnsureCatalogLoaded();

            SpawnTreeColumns(parent, "RoadMarketEast", new[] { 40f, 54f, 72f }, 4f, 122f, 7.5f, 1.0f, 1.42f, 701);
            SpawnTreeColumns(parent, "RoadMarketEastFar", new[] { 92f, 108f }, 4f, 122f, 9f, 1.12f, 1.55f, 719);
            // West wall close to the market so the left edge is not an open slope.
            SpawnTreeColumns(parent, "MarketWestNear", new[] { -28f, -36f }, 74f, 110f, 5.5f, 1.05f, 1.48f, 733);
            SpawnTreeColumns(parent, "MarketWestFar", new[] { -48f, -60f }, 70f, 120f, 8f, 1.0f, 1.4f, 751);
            // Belt behind the market fence — lighter than before to cut temple-approach lag.
            SpawnTreeRows(parent, "MarketNorthNear", -28f, 28f, new[] { 112f, 120f }, 6f, 1.05f, 1.45f, 769);
            SpawnTreeRows(parent, "MarketNorthFar", -36f, 36f, new[] { 132f, 146f }, 8f, 1.1f, 1.55f, 787);
            SpawnTreeRows(parent, "SouthCorners", -48f, 48f, new[] { -10f }, 12f, 0.95f, 1.25f, 811);
        }

        private static void SpawnMarketNorthBoundary(Transform parent)
        {
            SpawnFenceLine(
                parent,
                "MarketNorthBoundary",
                "MarketNorthBlocker",
                minX: -26f,
                maxX: 26f,
                z: 107.5f,
                yaw: 0f,
                spacing: 3.6f,
                blockerOffsetZ: 0.35f);
        }

        private static void SpawnMarketWestBoundary(Transform parent)
        {
            SpawnFenceLineVertical(
                parent,
                "MarketWestBoundary",
                "MarketWestBlocker",
                x: -23.5f,
                minZ: 76f,
                maxZ: 108f,
                yaw: 90f,
                spacing: 3.6f,
                blockerOffsetX: -0.35f);
        }

        /// <summary>
        /// Close-in west wall (trees + fence) so the left market background matches the flower-seller side.
        /// </summary>
        private static void SpawnMarketLeftBackdrop(Transform parent)
        {
            HometownArrivalCountrysidePropFactory.EnsureCatalogLoaded();

            // Fence along the west edge of the market pad, same depth as the east flower-seller fence.
            SpawnFenceLineVertical(
                parent,
                "MarketLeftWestFence",
                "MarketLeftWestBlocker",
                x: -17.2f,
                minZ: 80f,
                maxZ: 106f,
                yaw: 90f,
                spacing: 3.2f,
                blockerOffsetX: -0.35f);

            // Horizontal fence across the north-west background (mirrors FlowerSellerNorthFence on the right).
            SpawnFenceLine(
                parent,
                "MarketLeftNorthFence",
                "MarketLeftNorthBlocker",
                minX: -17.2f,
                maxX: 2f,
                z: 104.5f,
                yaw: 0f,
                spacing: 3.2f,
                blockerOffsetZ: 0.35f);

            GameObject trees = CreateGroup(parent, "MarketLeftTrees");
            System.Random random = new System.Random(1303);
            Vector3[] spots =
            {
                new Vector3(-18.4f, 0f, 84f),
                new Vector3(-19.0f, 0f, 90f),
                new Vector3(-18.8f, 0f, 96f),
                new Vector3(-19.6f, 0f, 102f),
                new Vector3(-20.8f, 0f, 88f),
                new Vector3(-21.0f, 0f, 98f),
                new Vector3(-14f, 0f, 105.6f),
                new Vector3(-8f, 0f, 105.8f),
                new Vector3(-2f, 0f, 105.6f),
                new Vector3(-11f, 0f, 108f),
                new Vector3(-5f, 0f, 107.8f),
                new Vector3(-22.2f, 0f, 94f),
            };

            for (int i = 0; i < spots.Length; i++)
            {
                HometownArrivalCountrysidePropFactory.SpawnWorldTree(
                    trees.transform,
                    spots[i],
                    NextRange(random, 0f, 360f),
                    NextRange(random, 1.05f, 1.42f));

                if (i % 3 == 0)
                {
                    HometownArrivalCountrysidePropFactory.SpawnWorldBush(
                        trees.transform,
                        spots[i] + new Vector3(NextRange(random, -1.2f, 1.2f), 0f, NextRange(random, -1.2f, 1.2f)),
                        NextRange(random, 0f, 360f),
                        NextRange(random, 0.9f, 1.15f));
                }
            }
        }

        private static void SpawnFlowerSellerBoundary(Transform parent)
        {
            HometownArrivalCountrysidePropFactory.EnsureCatalogLoaded();

            // Fence frames the flower stall on the east and north, leaving the south approach open.
            SpawnFenceLineVertical(
                parent,
                "FlowerSellerEastFence",
                "FlowerSellerEastBlocker",
                x: 16.5f,
                minZ: 82f,
                maxZ: 92f,
                yaw: 90f,
                spacing: 3.4f,
                blockerOffsetX: 0.35f);

            SpawnFenceLine(
                parent,
                "FlowerSellerNorthFence",
                "FlowerSellerNorthBlocker",
                minX: 10f,
                maxX: 16.5f,
                z: 91.5f,
                yaw: 0f,
                spacing: 3.2f,
                blockerOffsetZ: 0.35f);

            // Tree row behind / beside the flower seller.
            GameObject trees = CreateGroup(parent, "FlowerSellerTrees");
            System.Random random = new System.Random(1201);
            Vector3[] spots =
            {
                new Vector3(17.5f, 0f, 84f),
                new Vector3(18.2f, 0f, 87f),
                new Vector3(17.8f, 0f, 90f),
                new Vector3(19.5f, 0f, 86f),
                new Vector3(19.2f, 0f, 89.5f),
                new Vector3(20.5f, 0f, 88f),
                new Vector3(15.5f, 0f, 92.5f),
                new Vector3(13f, 0f, 93f),
                new Vector3(11f, 0f, 92.8f),
                new Vector3(18.8f, 0f, 92.2f),
            };

            for (int i = 0; i < spots.Length; i++)
            {
                HometownArrivalCountrysidePropFactory.SpawnWorldTree(
                    trees.transform,
                    spots[i],
                    NextRange(random, 0f, 360f),
                    NextRange(random, 1.05f, 1.4f));
            }
        }

        private static void SpawnFenceLine(
            Transform parent,
            string groupName,
            string blockerName,
            float minX,
            float maxX,
            float z,
            float yaw,
            float spacing,
            float blockerOffsetZ)
        {
            HometownArrivalCountrysidePropFactory.EnsureCatalogLoaded();

            GameObject group = CreateGroup(parent, groupName);
            for (float x = minX; x <= maxX + 0.01f; x += spacing)
            {
                HometownArrivalCountrysidePropFactory.SpawnWorldFence(
                    group.transform,
                    new Vector3(x, 0f, z),
                    yaw,
                    1.2f);
            }

            CreateInvisibleBlocker(
                group.transform,
                blockerName,
                new Vector3((minX + maxX) * 0.5f, 1.2f, z + blockerOffsetZ),
                new Vector3(maxX - minX + 2f, 2.6f, 0.7f));
        }

        private static void SpawnFenceLineVertical(
            Transform parent,
            string groupName,
            string blockerName,
            float x,
            float minZ,
            float maxZ,
            float yaw,
            float spacing,
            float blockerOffsetX)
        {
            HometownArrivalCountrysidePropFactory.EnsureCatalogLoaded();

            GameObject group = CreateGroup(parent, groupName);
            for (float z = minZ; z <= maxZ + 0.01f; z += spacing)
            {
                HometownArrivalCountrysidePropFactory.SpawnWorldFence(
                    group.transform,
                    new Vector3(x, 0f, z),
                    yaw,
                    1.2f);
            }

            CreateInvisibleBlocker(
                group.transform,
                blockerName,
                new Vector3(x + blockerOffsetX, 1.2f, (minZ + maxZ) * 0.5f),
                new Vector3(0.7f, 2.6f, maxZ - minZ + 2f));
        }

        private static void CreateInvisibleBlocker(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = name;
            blocker.transform.SetParent(parent, false);
            blocker.transform.position = position;
            blocker.transform.localScale = scale;

            Renderer renderer = blocker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        private static void SpawnTempleTreeBelts(Transform parent)
        {
            HometownArrivalCountrysidePropFactory.EnsureCatalogLoaded();

            // Lighter temple belts — dense trees here were a major hitch source.
            SpawnTreeColumns(parent, "TempleEast", new[] { 100f, 118f }, 104f, 176f, 12f, 1.0f, 1.38f, 901);
            SpawnTreeColumns(parent, "TempleWest", new[] { 12f, -6f }, 104f, 176f, 12f, 1.0f, 1.38f, 947);
            SpawnTreeRows(parent, "TempleNorth", 12f, 98f, new[] { 182f, 198f }, 12f, 1.08f, 1.48f, 991);
            SpawnTreeRows(parent, "TempleSouthCorners", 4f, 106f, new[] { 82f }, 14f, 0.95f, 1.28f, 1037);
        }

        private static void SpawnTreeColumns(
            Transform parent,
            string groupName,
            float[] columns,
            float zStart,
            float zEnd,
            float spacing,
            float minScale,
            float maxScale,
            int seed)
        {
            GameObject group = CreateGroup(parent, groupName);
            System.Random random = new System.Random(seed);

            for (int c = 0; c < columns.Length; c++)
            {
                for (float z = zStart; z <= zEnd; z += spacing)
                {
                    Vector3 position = new Vector3(
                        columns[c] + NextRange(random, -1.4f, 1.4f),
                        0f,
                        z + NextRange(random, -1.6f, 1.6f));

                    if (IsInPlayableClearance(position))
                    {
                        continue;
                    }

                    HometownArrivalCountrysidePropFactory.SpawnWorldTree(
                        group.transform,
                        position,
                        NextRange(random, 0f, 360f),
                        NextRange(random, minScale, maxScale));

                    if ((Mathf.RoundToInt(z) + c) % 5 == 0)
                    {
                        HometownArrivalCountrysidePropFactory.SpawnWorldBush(
                            group.transform,
                            position + new Vector3(NextRange(random, -1.6f, 1.6f), 0f, NextRange(random, -1.6f, 1.6f)),
                            NextRange(random, 0f, 360f),
                            NextRange(random, 0.85f, 1.1f));
                    }
                }
            }
        }

        private static void SpawnTreeRows(
            Transform parent,
            string groupName,
            float xStart,
            float xEnd,
            float[] rows,
            float spacing,
            float minScale,
            float maxScale,
            int seed)
        {
            GameObject group = CreateGroup(parent, groupName);
            System.Random random = new System.Random(seed);
            float step = xStart <= xEnd ? spacing : -spacing;

            for (int r = 0; r < rows.Length; r++)
            {
                for (float x = xStart; step > 0f ? x <= xEnd : x >= xEnd; x += step)
                {
                    Vector3 position = new Vector3(
                        x + NextRange(random, -1.4f, 1.4f),
                        0f,
                        rows[r] + NextRange(random, -1.4f, 1.4f));

                    if (IsInPlayableClearance(position))
                    {
                        continue;
                    }

                    HometownArrivalCountrysidePropFactory.SpawnWorldTree(
                        group.transform,
                        position,
                        NextRange(random, 0f, 360f),
                        NextRange(random, minScale, maxScale));
                }
            }
        }

        private static GameObject CreateGroup(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            group.transform.localPosition = Vector3.zero;
            return group;
        }

        private static bool IsInPlayableClearance(Vector3 position)
        {
            // Keep the market walkway open, but free the west/east/north edges for fence + tree walls.
            bool inMainCorridor = Mathf.Abs(position.x) < 20f && position.z > -4f && position.z < 106f;
            bool inMarketCenter = Mathf.Abs(position.x) < 22f && position.z > 76f && position.z < 106f;
            bool inTemple = position.x > 30f && position.x < 87f && position.z > 84f && position.z < 168f;
            // Narrow traffic road band (home↔market) — never plant props on asphalt/median.
            bool onTrafficRoad = position.z > 44f && position.z < 64f;
            return inMainCorridor || inMarketCenter || inTemple || onTrafficRoad;
        }

        private static float NextRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static void DestroyChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(child.gameObject);
                return;
            }
#endif
            Object.Destroy(child.gameObject);
        }
    }
}
