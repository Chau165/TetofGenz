#if UNITY_EDITOR
using ChoTet.Level1;
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Builds Level 1 Phase 2 outdoor street inside the same Level 1 scene.
    /// Layered PBR ground + perimeter dressing — same visual language as Level 2 market.
    /// </summary>
    public static class Level1OutdoorAreaBuilder
    {
        private const string OccaPrefabs = "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs";
        private const string NaturePrefabs = "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs";
        private const string UrbanPrefabs = "Assets/Assets/AIRIDev/Low-Poly Urban Assets/Prefabs";
        private const string MarketPrefabs = "Assets/Low-Poly Medieval Market/Prefabs";

        private const float FloorY = 0f;
        private const float GroundTopY = -0.55f;
        private const float AlleyHalfWidth = 1.6f;
        private const float RoadHalfWidth = 3.2f;
        private const float BusHalfWidth = 4f;
        private const float OuterBoundaryHalfWidth = 6.2f;
        private const float PathCorridorHalfWidth = 6.5f;
        private const float BorderTreeHalfWidth = 14.5f;

        // Visual ground extends well beyond the walkable corridor so the camera never sees bare sky/fog.
        private const float VisualGroundHalfWidth = 26f;
        private const float VisualGroundNearZ = -3.2f;
        private const float VisualGroundFarZ = -52f;

        private const float SpawnZ = -6.85f;
        private const float IndoorOutdoorBoundaryZ = -5.55f;
        private const float AlleyStartZ = -7.5f;
        private const float AlleyEndZ = -15f;
        private const float RoadEndZ = -28f;
        private const float BusEndZ = -36f;
        private const float PathEndZ = -26.5f;

        private const float TravelRoadHalfWidth = 2.38f;
        private const float PedestrianCorridorHalfWidth = 3.38f;
        private const float PedestrianPlazaSpanStartZ = -4.25f;
        private const float PedestrianPlazaSpanEndZ = -25.8f;
        private const float PlayerSidePavementOuterX = -24f;
        private const float PedestrianPavementInnerX = 12f;
        private const float SidePropMinX = 4.35f;

        public const float OutdoorMinX = Level1OutdoorMovementSettings.PlayMinX;
        public const float OutdoorMaxX = Level1OutdoorMovementSettings.PlayMaxX;
        public const float OutdoorMinZ = Level1OutdoorMovementSettings.PlayEndZ - 8f;
        public const float OutdoorMaxZ = Level1OutdoorMovementSettings.PlayStartZ + 1.2f;

        /// <summary>World-space Y for the outdoor walk mesh + CharacterController feet.</summary>
        public const float WalkSurfaceY = Level1OutdoorSettings.WalkSurfaceY;

        private const float WalkColliderThickness = 0.75f;

        public static void FixOutdoorMaterialsMenu()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorAreaBuilder] Exit Play mode before fixing outdoor materials.");
                return;
            }

            Transform outdoorRoot = GameObject.Find("Level1_OutdoorArea")?.transform;
            if (outdoorRoot == null)
            {
                Debug.LogWarning("[Level1OutdoorAreaBuilder] Level1_OutdoorArea not found.");
                return;
            }

            int fixedCount = FinalizeOutdoorAreaMaterials(outdoorRoot, LoadOutdoorMaterialSet());
            FixTrafficCars(outdoorRoot);
            MarkSceneDirty();
            Debug.Log("[Level1OutdoorAreaBuilder] Applied Cho Tet materials to " + fixedCount +
                      " outdoor renderer(s). Save the scene if it looks correct.");
        }

        public static void BuildOutdoorAreaMenu()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorAreaBuilder] Exit Play mode before building the outdoor area.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.LogWarning("[Level1OutdoorAreaBuilder] Level1World not found. Run full Level 1 build first.");
                return;
            }

            Transform outdoorSpawn = BuildOrUpdate(worldRoot.transform);
            WirePhaseController(outdoorSpawn);
            MarkSceneDirty();
            Debug.Log("[Level1OutdoorAreaBuilder] Outdoor street rebuilt — layered ground + perimeter dressing.");
        }

        public static Transform BuildOrUpdate(Transform worldRoot)
        {
            Transform existing = worldRoot.Find("Level1_OutdoorArea");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            Material grass = LoadMaterial("Grass_Ground.mat");
            Material garden = LoadMaterial("Garden_Ground.mat");
            Material road = LoadMaterial("Road_Ground.mat");
            Material cliffEarth = LoadMaterial("Dark_Brown.mat");
            Material homeDirt = LoadMaterial("Home_Dirt.mat");
            Material terrainEdge = LoadMaterial("Terrain_Edge.mat");
            Material rockEdge = LoadMaterial("Rock_Edge.mat");
            Material woodWarm = LoadMaterial("Wood_Warm.mat");
            Material accentRed = LoadMaterial("Accent_Red.mat");
            Material foliageGreen = LoadMaterial("Foliage_Green.mat");
            Material lanternGold = LoadMaterial("Lantern_Gold.mat");
            Material houseCream = LoadMaterial("House_Cream.mat");
            Material roofDark = LoadMaterial("Roof_Dark.mat");
            Material doorDark = LoadMaterial("Door_Dark.mat");
            Material urbanPavement = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat");
            if (urbanPavement == null)
            {
                urbanPavement = LoadMaterial("Urban_Path.mat");
            }

            GameObject outdoorRoot = new GameObject("Level1_OutdoorArea");
            outdoorRoot.transform.SetParent(worldRoot, false);

            BuildLayeredGround(outdoorRoot.transform, urbanPavement, homeDirt, terrainEdge);
            BuildTravelRoad(outdoorRoot.transform, urbanPavement, homeDirt);
            BuildHouseExteriorShell(outdoorRoot.transform, houseCream, roofDark, woodWarm, doorDark, lanternGold);
            BuildUnifiedPedestrianPavement(outdoorRoot.transform, urbanPavement); // single slab — no z-fight
            Level1OutdoorQuaterniusStreetscape.Build(outdoorRoot.transform);
            Level1SimplePolyPlayerSideFacadeBuilder.Build(outdoorRoot.transform);
            BuildSmallAlley(outdoorRoot.transform, woodWarm);
            BuildMainRoad(outdoorRoot.transform);
            BuildBusPickupArea(outdoorRoot.transform, woodWarm, accentRed, foliageGreen, lanternGold);
            BuildOrbitViewScreening(outdoorRoot.transform, foliageGreen, woodWarm, accentRed);
            BuildPerimeterEnclosure(outdoorRoot.transform, foliageGreen, cliffEarth, woodWarm);
            BuildCountrysideDetails(outdoorRoot.transform, woodWarm, houseCream, roofDark);
            BuildPathBoundaries(outdoorRoot.transform);
            BuildOutdoorLighting(outdoorRoot.transform);
            RemoveOutdoorBoxes(outdoorRoot.transform);
            Level1OutdoorAmbientPropsBuilder.Build(outdoorRoot.transform);
            Level1OutdoorWorkplaceNpcBuilder.Build(outdoorRoot.transform);

            FinalizeOutdoorAreaMaterials(outdoorRoot.transform, new OutdoorMaterialSet
            {
                Grass = grass,
                Garden = garden,
                Road = road,
                EarthBrown = cliffEarth,
                RockEdge = rockEdge,
                WoodWarm = woodWarm,
                AccentRed = accentRed,
                FoliageGreen = foliageGreen,
                FlowerYellow = LoadMaterial("Flower_Yellow.mat"),
                LanternGold = lanternGold,
                HouseCream = houseCream,
                RoofDark = roofDark,
                PathStone = LoadMaterial("Urban_Path.mat")
            });
            FixTrafficCars(outdoorRoot.transform);

            outdoorRoot.SetActive(false);
            Transform outdoorSpawn = CreateSpawnPoint(outdoorRoot.transform);
            WirePhaseController(outdoorSpawn);
            return outdoorSpawn;
        }

        private static void RemoveOutdoorBoxes(Transform outdoorRoot)
        {
            if (outdoorRoot == null)
            {
                return;
            }

            var toRemove = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in outdoorRoot.GetComponentsInChildren<Transform>(true))
            {
                string lower = child.name.ToLowerInvariant();
                if (lower.Contains("box")
                    || lower.Contains("crate")
                    || lower.Contains("wooden_box")
                    || lower.Contains("woodenbox"))
                {
                    toRemove.Add(child.gameObject);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                Object.DestroyImmediate(toRemove[i]);
            }
        }

        public static void WirePhaseController(Transform outdoorSpawn, Transform systemsRoot = null)
        {
            if (systemsRoot == null)
            {
                GameObject systems = GameObject.Find("Systems");
                systemsRoot = systems != null ? systems.transform : null;
            }

            if (systemsRoot == null)
            {
                return;
            }

            Level1PhaseController phaseController = systemsRoot.GetComponentInChildren<Level1PhaseController>(true);
            if (phaseController == null)
            {
                GameObject phaseObject = new GameObject("Level1PhaseController");
                phaseObject.transform.SetParent(systemsRoot, false);
                phaseController = phaseObject.AddComponent<Level1PhaseController>();
                phaseObject.AddComponent<Level1ScreenFade>();
            }

            Level1ScreenFade fade = phaseController.GetComponent<Level1ScreenFade>();
            if (fade == null)
            {
                fade = phaseController.gameObject.AddComponent<Level1ScreenFade>();
            }

            Camera mainCamera = Camera.main ?? Object.FindAnyObjectByType<Camera>();
            IndoorHighAngleCamera gameplayCamera = mainCamera != null
                ? mainCamera.GetComponent<IndoorHighAngleCamera>()
                : null;
            Material skyMaterial = LoadMaterial("ChoTet_SunsetSkybox.mat");
            GameObject outdoorAreaObject = outdoorSpawn != null && outdoorSpawn.parent != null
                ? outdoorSpawn.parent.gameObject
                : null;

            SerializedObject serialized = new SerializedObject(phaseController);
            serialized.FindProperty("outdoorSpawn").objectReferenceValue = outdoorSpawn;
            serialized.FindProperty("outdoorAreaRoot").objectReferenceValue = outdoorAreaObject;
            serialized.FindProperty("screenFade").objectReferenceValue = fade;

            Transform worldRoot = outdoorSpawn != null ? outdoorSpawn.root : null;
            if (worldRoot == null)
            {
                worldRoot = GameObject.Find("Level1World")?.transform;
            }

            GameObject indoorAreaObject = worldRoot != null
                ? worldRoot.Find("Level1_IndoorArea")?.gameObject
                : null;

            serialized.FindProperty("rentalRoomInterior").objectReferenceValue = indoorAreaObject != null
                ? indoorAreaObject
                : worldRoot != null ? worldRoot.Find("RentalRoomInterior")?.gameObject : null;
            serialized.FindProperty("gameplayCamera").objectReferenceValue = gameplayCamera;
            serialized.FindProperty("mainCamera").objectReferenceValue = mainCamera;
            BoardingHouseStreetOrbitCamera streetOrbit = mainCamera != null
                ? mainCamera.GetComponent<BoardingHouseStreetOrbitCamera>()
                : null;
            if (streetOrbit == null && mainCamera != null)
            {
                streetOrbit = mainCamera.gameObject.AddComponent<BoardingHouseStreetOrbitCamera>();
            }

            serialized.FindProperty("streetOrbitCamera").objectReferenceValue = streetOrbit;

            StreetLookAheadCamera streetLookAhead = mainCamera != null
                ? mainCamera.GetComponent<StreetLookAheadCamera>()
                : null;
            if (streetLookAhead == null && mainCamera != null)
            {
                streetLookAhead = mainCamera.gameObject.AddComponent<StreetLookAheadCamera>();
            }

            serialized.FindProperty("streetLookAheadCamera").objectReferenceValue = streetLookAhead;
            serialized.FindProperty("outdoorSkybox").objectReferenceValue = skyMaterial;
            serialized.FindProperty("outdoorMinX").floatValue = OutdoorMinX;
            serialized.FindProperty("outdoorMaxX").floatValue = OutdoorMaxX;
            serialized.FindProperty("outdoorMinZ").floatValue = OutdoorMinZ;
            serialized.FindProperty("outdoorMaxZ").floatValue = OutdoorMaxZ;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Multi-layer ground: distant fill + urban lot pavement + terrain lips.
        /// Fills the camera frustum without exposing sky; no grass in the streetscape.
        /// </summary>
        private static void BuildLayeredGround(Transform parent, Material urbanPavement, Material homeDirt,
            Material terrainEdge)
        {
            GameObject groundRoot = new GameObject("Ground");
            groundRoot.transform.SetParent(parent, false);

            float centerZ = (VisualGroundNearZ + VisualGroundFarZ) * 0.5f;
            float depth = VisualGroundNearZ - VisualGroundFarZ;
            float width = VisualGroundHalfWidth * 2f;
            Material outerFill = LoadMaterial("Foundation_Stone.mat") ?? homeDirt;

            // Deepest layer — neutral stone fill beyond the walkable block.
            GameObject outerBase = CreateGroundSlab("OutdoorOuterBaseGround", groundRoot.transform,
                new Vector3(0f, GroundTopY - 0.08f, centerZ),
                new Vector3(width + 8f, 1f, depth + 10f), null);
            outerBase.GetComponent<Renderer>().sharedMaterial =
                CreateTiledMaterial(outerFill, width + 8f, depth + 10f, 12f);

            BuildTerrainEdgeLips(groundRoot.transform, terrainEdge, width, depth, centerZ);
        }

        /// <summary>Pedestrian connector toward the bus stop — same pavement as the plaza, not asphalt.</summary>
        private static void BuildTravelRoad(Transform parent, Material pavement, Material homeDirt)
        {
            GameObject roadRoot = new GameObject("TravelRoad");
            roadRoot.transform.SetParent(parent, false);

            float roadStartZ = IndoorOutdoorBoundaryZ - 0.85f;
            float roadEndZ = Level1BusStopRoadSettings.SidewalkFarZ - 0.35f;
            float depth = roadStartZ - roadEndZ;
            float centerZ = (roadStartZ + roadEndZ) * 0.5f;
            float roadWidth = TravelRoadHalfWidth * 2f;

            GameObject surface = CreateVisualCube("TravelRoadSurface", roadRoot.transform,
                new Vector3(0f, GroundTopY + 0.09f, centerZ),
                new Vector3(roadWidth, 0.14f, depth + 0.6f), null);
            surface.GetComponent<Renderer>().sharedMaterial =
                CreateTiledMaterial(pavement, roadWidth, depth + 0.6f, 2.35f);

            float shoulderWidth = 0.5f;
            Material shoulderMat = CreateTiledMaterial(
                LoadMaterial("Foundation_Stone.mat") ?? homeDirt, shoulderWidth, depth, 3.5f);
            GameObject shoulderL = CreateVisualCube("TravelRoadShoulderL", roadRoot.transform,
                new Vector3(-TravelRoadHalfWidth - shoulderWidth * 0.5f, GroundTopY + 0.07f, centerZ),
                new Vector3(shoulderWidth, 0.1f, depth), null);
            shoulderL.GetComponent<Renderer>().sharedMaterial = shoulderMat;
            GameObject shoulderR = CreateVisualCube("TravelRoadShoulderR", roadRoot.transform,
                new Vector3(TravelRoadHalfWidth + shoulderWidth * 0.5f, GroundTopY + 0.07f, centerZ),
                new Vector3(shoulderWidth, 0.1f, depth), null);
            shoulderR.GetComponent<Renderer>().sharedMaterial = shoulderMat;
        }

        private static void BuildTerrainEdgeLips(Transform parent, Material terrainEdge, float width, float depth,
            float centerZ)
        {
            float lipHeight = 0.34f;
            float lipY = GroundTopY + 0.12f;
            float lipThickness = 3.2f;

            CreateGroundSlab("TerrainLipNorth", parent,
                new Vector3(0f, lipY, VisualGroundNearZ + lipThickness * 0.45f),
                new Vector3(width + 2f, lipHeight, lipThickness), terrainEdge);
            CreateGroundSlab("TerrainLipSouth", parent,
                new Vector3(0f, lipY, VisualGroundFarZ - lipThickness * 0.45f),
                new Vector3(width + 2f, lipHeight, lipThickness), terrainEdge);
            CreateGroundSlab("TerrainLipWest", parent,
                new Vector3(-VisualGroundHalfWidth + 0.25f, lipY, centerZ),
                new Vector3(lipThickness, lipHeight, depth + 4f), terrainEdge);
            CreateGroundSlab("TerrainLipEast", parent,
                new Vector3(VisualGroundHalfWidth - 0.25f, lipY, centerZ),
                new Vector3(lipThickness, lipHeight, depth + 4f), terrainEdge);
        }

        private static Material CreateTiledMaterial(Material source, float worldWidth, float worldDepth,
            float metersPerTile)
        {
            Material mat = new Material(source);
            mat.SetTextureScale("_BaseMap", new Vector2(
                Mathf.Max(1f, worldWidth / metersPerTile),
                Mathf.Max(1f, worldDepth / metersPerTile)));
            return mat;
        }

        /// <summary>
        /// One continuous pedestrian pavement slab — avoids z-fighting from stacked plaza / fill / lot meshes.
        /// </summary>
        private static void BuildUnifiedPedestrianPavement(Transform parent, Material pavement)
        {
            if (pavement == null)
            {
                return;
            }

            GameObject pavementRoot = new GameObject("UrbanPedestrianPavement");
            pavementRoot.transform.SetParent(parent, false);

            float northZ = IndoorOutdoorBoundaryZ + 0.48f;
            float southZ = PedestrianPlazaSpanEndZ - 0.45f;
            float depth = northZ - southZ;
            float centerZ = (northZ + southZ) * 0.5f;
            float width = PedestrianPavementInnerX - PlayerSidePavementOuterX;
            float centerX = (PedestrianPavementInnerX + PlayerSidePavementOuterX) * 0.5f;
            const float slabThickness = 0.08f;
            float surfaceY = Level1OutdoorSettings.WalkSurfaceY;

            GameObject slab = CreateVisualCube(
                "PedestrianPavementSlab",
                pavementRoot.transform,
                new Vector3(centerX, surfaceY + slabThickness * 0.5f, centerZ),
                new Vector3(width, slabThickness, depth),
                null);
            Renderer renderer = slab.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateTiledMaterial(pavement, width, depth, 2.5f);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        /// <summary>Exterior shell + exit props — blocks the top-down camera from seeing indoor furniture.</summary>
        private static void BuildHouseExteriorShell(Transform parent, Material wall, Material roof, Material woodWarm,
            Material doorDark, Material lanternGold)
        {
            Material foliageGreen = LoadMaterial("Foliage_Green.mat");
            Material flowerYellow = LoadMaterial("Flower_Yellow.mat");
            Material pathStone = LoadMaterial("Urban_Path.mat");
            Material glass = LoadMaterial("Glass_Blue.mat");

            GameObject exitDecor = new GameObject("Level1_ExitDecor");
            exitDecor.transform.SetParent(parent, false);

            GameObject shell = new GameObject("Level1_HouseExterior");
            shell.transform.SetParent(parent, false);

            GameObject zone = new GameObject("Level1_HouseExitSpawn");
            zone.transform.SetParent(exitDecor.transform, false);

            const float wallHeight = 3.15f;
            const float backWallZ = -4.28f;
            const float doorCenterY = 1.12f;
            const float doorWidth = 1.34f;
            const float doorHeight = 2.12f;
            const float doorFrontZ = backWallZ - 0.18f;

            BuildBoardingHouseShell(shell.transform, wall, woodWarm, glass, backWallZ, wallHeight);
            BuildBoardingHouseDoorAssembly(shell.transform, woodWarm, doorDark, roof, pathStone, lanternGold,
                backWallZ, doorCenterY, doorWidth, doorHeight, doorFrontZ);
            if (shell.transform.Find("BoardingHouseBanner") == null)
            {
                Level1BoardingHouseBannerBuilder.Build(shell.transform);
            }

            BuildBoardingHouseExitDecor(zone.transform, pathStone, flowerYellow, foliageGreen, doorFrontZ);

            StripDecorShadows(exitDecor);
            StripDecorShadows(shell);
        }

        private static void BuildBoardingHouseShell(Transform shell, Material wall, Material woodWarm,
            Material glass, float backWallZ, float wallHeight)
        {
            CreateVisualCube("HouseBackWall", shell, new Vector3(0f, wallHeight * 0.5f, backWallZ),
                new Vector3(11.4f, wallHeight, 0.34f), wall);
            CreateVisualCube("HouseSideWallL", shell, new Vector3(-5.55f, wallHeight * 0.5f, -2.35f),
                new Vector3(0.3f, wallHeight, 4.35f), wall);
            CreateVisualCube("HouseSideWallR", shell, new Vector3(5.55f, wallHeight * 0.5f, -2.35f),
                new Vector3(0.3f, wallHeight, 4.35f), wall);

            Level1BoardingHouseRoofBuilder.Build(shell);

            CreateVisualCube("HouseFoundationBand", shell, new Vector3(0f, 0.16f, backWallZ - 0.04f),
                new Vector3(11.55f, 0.22f, 0.42f), woodWarm);
            CreateVisualCube("HouseCorniceBand", shell, new Vector3(0f, wallHeight - 0.08f, backWallZ - 0.06f),
                new Vector3(11.5f, 0.14f, 0.28f), woodWarm);

            BuildFacadeWindow(shell, "HouseWindowL", -2.95f, 1.72f, backWallZ - 0.08f, woodWarm, glass);
            BuildFacadeWindow(shell, "HouseWindowR", 2.95f, 1.72f, backWallZ - 0.08f, woodWarm, glass);
            BuildFacadeWindow(shell, "HouseSideWindowL", -5.48f, 1.62f, -3.35f, woodWarm, glass);
            BuildFacadeWindow(shell, "HouseSideWindowR", 5.48f, 1.62f, -3.35f, woodWarm, glass);

            CreateVisualCube("HouseDoorPilasterL", shell, new Vector3(-0.98f, 1.12f, backWallZ - 0.05f),
                new Vector3(0.16f, 2.35f, 0.28f), woodWarm);
            CreateVisualCube("HouseDoorPilasterR", shell, new Vector3(0.98f, 1.12f, backWallZ - 0.05f),
                new Vector3(0.16f, 2.35f, 0.28f), woodWarm);
        }

        private static void BuildFacadeWindow(Transform shell, string name, float x, float y, float z,
            Material frame, Material glass)
        {
            GameObject windowRoot = new GameObject(name);
            windowRoot.transform.SetParent(shell, false);
            windowRoot.transform.position = new Vector3(x, y, z);

            CreateVisualCube("FrameTop", windowRoot.transform, Vector3.zero,
                new Vector3(1.18f, 0.1f, 0.12f), frame);
            CreateVisualCube("FrameBottom", windowRoot.transform, new Vector3(0f, -0.62f, 0f),
                new Vector3(1.18f, 0.1f, 0.12f), frame);
            CreateVisualCube("FrameLeft", windowRoot.transform, new Vector3(-0.52f, -0.31f, 0f),
                new Vector3(0.1f, 1.14f, 0.12f), frame);
            CreateVisualCube("FrameRight", windowRoot.transform, new Vector3(0.52f, -0.31f, 0f),
                new Vector3(0.1f, 1.14f, 0.12f), frame);
            CreateVisualCube("Glass", windowRoot.transform, new Vector3(0f, -0.31f, -0.02f),
                new Vector3(0.86f, 0.9f, 0.05f), glass);
            CreateVisualCube("Sill", windowRoot.transform, new Vector3(0f, -0.7f, 0.04f),
                new Vector3(1.28f, 0.06f, 0.18f), frame);
        }

        private static void BuildBoardingHouseDoorAssembly(Transform shell, Material woodWarm, Material doorDark,
            Material roof, Material pathStone, Material lanternGold, float backWallZ, float doorCenterY,
            float doorWidth, float doorHeight, float doorFrontZ)
        {
            float lintelY = doorCenterY + doorHeight * 0.5f + 0.08f;

            CreateVisualCube("DoorRecessBacking", shell, new Vector3(0f, doorCenterY, backWallZ + 0.04f),
                new Vector3(doorWidth + 0.22f, doorHeight + 0.14f, 0.2f), doorDark);
            CreateVisualCube("DoorFrameLeft", shell, new Vector3(-doorWidth * 0.5f - 0.07f, doorCenterY, doorFrontZ),
                new Vector3(0.12f, doorHeight + 0.16f, 0.2f), woodWarm);
            CreateVisualCube("DoorFrameRight", shell, new Vector3(doorWidth * 0.5f + 0.07f, doorCenterY, doorFrontZ),
                new Vector3(0.12f, doorHeight + 0.16f, 0.2f), woodWarm);
            CreateVisualCube("DoorFrameTop", shell, new Vector3(0f, lintelY, doorFrontZ),
                new Vector3(doorWidth + 0.34f, 0.12f, 0.22f), woodWarm);

            CreateVisualCube("DoorPanel", shell, new Vector3(0f, doorCenterY - 0.02f, doorFrontZ + 0.06f),
                new Vector3(doorWidth - 0.1f, doorHeight - 0.12f, 0.12f), woodWarm);
            CreateVisualCube("DoorPanelUpper", shell, new Vector3(0f, doorCenterY + 0.52f, doorFrontZ + 0.12f),
                new Vector3(doorWidth - 0.34f, doorHeight * 0.34f, 0.05f), doorDark);
            CreateVisualCube("DoorPanelLower", shell, new Vector3(0f, doorCenterY - 0.42f, doorFrontZ + 0.12f),
                new Vector3(doorWidth - 0.34f, doorHeight * 0.34f, 0.05f), doorDark);
            CreateVisualCube("DoorHandle", shell, new Vector3(doorWidth * 0.32f, doorCenterY - 0.05f, doorFrontZ + 0.16f),
                new Vector3(0.08f, 0.08f, 0.06f), lanternGold);

            CreateVisualCube("DoorAwning", shell, new Vector3(0f, lintelY + 0.28f, doorFrontZ + 0.12f),
                new Vector3(doorWidth + 0.82f, 0.1f, 0.62f), roof);
            CreateVisualCube("DoorAwningSupportL", shell, new Vector3(-doorWidth * 0.5f - 0.18f, lintelY + 0.08f, doorFrontZ + 0.22f),
                new Vector3(0.08f, 0.32f, 0.08f), woodWarm);
            CreateVisualCube("DoorAwningSupportR", shell, new Vector3(doorWidth * 0.5f + 0.18f, lintelY + 0.08f, doorFrontZ + 0.22f),
                new Vector3(0.08f, 0.32f, 0.08f), woodWarm);

            CreateVisualCube("DoorThreshold", shell, new Vector3(0f, 0.1f, doorFrontZ + 0.2f),
                new Vector3(doorWidth + 0.55f, 0.14f, 0.34f), pathStone);
            CreateVisualCube("DoorStep", shell, new Vector3(0f, 0.04f, doorFrontZ + 0.42f),
                new Vector3(doorWidth + 0.72f, 0.08f, 0.28f), pathStone);

            GameObject lanternL = InstantiatePrefab(OccaPrefabs + "/Lantern.prefab", shell,
                new Vector3(-1.35f, 1.95f, doorFrontZ + 0.08f), Quaternion.Euler(0f, 12f, 0f),
                Vector3.one * 0.55f, "DoorLanternL", stripCollider: true);
            ApplyMaterialPersistent(lanternL, lanternGold);
            StripDecorShadows(lanternL);

            GameObject lanternR = InstantiatePrefab(OccaPrefabs + "/Lantern.prefab", shell,
                new Vector3(1.35f, 1.95f, doorFrontZ + 0.08f), Quaternion.Euler(0f, -12f, 0f),
                Vector3.one * 0.55f, "DoorLanternR", stripCollider: true);
            ApplyMaterialPersistent(lanternR, lanternGold);
            StripDecorShadows(lanternR);
        }

        private static void BuildBoardingHouseExitDecor(Transform zone, Material pathStone, Material flowerYellow,
            Material foliageGreen, float doorFrontZ)
        {
            float pathZ = doorFrontZ + 0.72f;

            GameObject exitPath = InstantiatePrefab(OccaPrefabs + "/Path Piece_5.prefab", zone,
                new Vector3(0f, FloorY + 0.03f, pathZ), Quaternion.identity, Vector3.one * 1.05f, "ExitPathStone");
            ApplyMaterialPersistent(exitPath, pathStone);
            StripDecorShadows(exitPath);

            GameObject exitFlowerL = InstantiatePrefab(NaturePrefabs + "/flower2-1.prefab", zone,
                new Vector3(1.15f, 0f, pathZ + 0.08f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.12f, "ExitFlowerL");
            ApplyMaterialPersistent(exitFlowerL, flowerYellow);
            StripDecorShadows(exitFlowerL);

            GameObject exitFlowerR = InstantiatePrefab(NaturePrefabs + "/flower3-2.prefab", zone,
                new Vector3(-1.15f, 0f, pathZ + 0.08f), Quaternion.Euler(0f, -22f, 0f), Vector3.one * 1.08f, "ExitFlowerR");
            ApplyMaterialPersistent(exitFlowerR, flowerYellow);
            StripDecorShadows(exitFlowerR);

            GameObject exitPotPlant = InstantiatePrefab(UrbanPrefabs + "/pot plant.prefab", zone,
                new Vector3(1.05f, 0f, pathZ + 0.38f), Quaternion.Euler(0f, -25f, 0f), Vector3.one * 0.72f, "ExitPotPlant");
            ApplyMaterialPersistent(exitPotPlant, foliageGreen);
            StripDecorShadows(exitPotPlant);
        }

        private static void BuildHouseExitFacade(Transform parent)
        {
            // Legacy entry point — merged into BuildHouseExteriorShell.
            BuildHouseExteriorShell(parent,
                LoadMaterial("House_Cream.mat"),
                LoadMaterial("Roof_Dark.mat"),
                LoadMaterial("Wood_Warm.mat"),
                LoadMaterial("Door_Dark.mat"),
                LoadMaterial("Lantern_Gold.mat"));
        }

        private static void BuildSmallAlley(Transform parent, Material woodWarm)
        {
            GameObject zone = new GameObject("Level1_SmallAlley");
            zone.transform.SetParent(parent, false);
        }

        private static void BuildMainRoad(Transform parent)
        {
            GameObject zone = new GameObject("Level1_MainRoad");
            zone.transform.SetParent(parent, false);
        }

        private static void BuildBusPickupArea(Transform parent, Material woodWarm, Material accentRed,
            Material foliageGreen, Material lanternGold)
        {
            GameObject zone = new GameObject("Level1_BusPickupArea");
            zone.transform.SetParent(parent, false);

            Level1BusStopRoadBuilder.Build(zone.transform);
            Level1SimplePolyAcrossRoadBuilder.Build(zone.transform);
            BuildTrafficSystem(zone.transform);

            float waitZ = Level1BusStopRoadSettings.SidewalkCenterZ;
            const float waitX = -3.15f;
            float padY = Level1BusStopRoadSettings.SidewalkSurfaceY;

            GameObject flower = InstantiatePrefab(NaturePrefabs + "/flower2-1.prefab", zone.transform,
                new Vector3(waitX + 0.45f, padY, waitZ - 0.35f),
                Quaternion.Euler(0f, -12f, 0f), Vector3.one * 0.88f, "BusStopFlower");
            ApplyMaterialPersistent(flower, LoadMaterial("Flower_Yellow.mat"));
            StripDecorShadows(flower);
        }

        /// <summary>Soft blockers on weak map edges visible from the limited street orbit camera.</summary>
        private static void BuildOrbitViewScreening(Transform parent, Material foliageGreen, Material woodWarm,
            Material accentRed)
        {
            GameObject root = new GameObject("OrbitViewScreening");
            root.transform.SetParent(parent, false);

            float padY = WalkSurfaceY;
            var eastScreenSpots = new (float x, float z, float yaw, float scale)[]
            {
                (8.2f, -8.5f, -18f, 1.05f),
                (9.4f, -13.8f, 24f, 0.95f),
                (7.6f, -19.5f, -8f, 1.1f),
                (8.8f, -24.6f, 16f, 1f),
                (6.9f, -30.2f, -22f, 0.92f),
                (10.1f, -6.8f, 32f, 0.88f),
                (9.2f, -11.2f, -28f, 1.02f)
            };

            for (int i = 0; i < eastScreenSpots.Length; i++)
            {
                (float x, float z, float yaw, float scale) spot = eastScreenSpots[i];
                GameObject bush = InstantiatePrefab(NaturePrefabs + "/bush1.prefab", root.transform,
                    new Vector3(spot.x, padY, spot.z),
                    Quaternion.Euler(0f, spot.yaw, 0f),
                    Vector3.one * spot.scale,
                    "OrbitScreenBushE_" + i,
                    stripCollider: true);
                ApplyMaterialPersistent(bush, foliageGreen);
                StripDecorShadows(bush);
            }

            GameObject sign = CreateVisualCube("OrbitScreenSign", root.transform,
                new Vector3(5.8f, padY + 1.35f, -16.8f),
                new Vector3(0.12f, 1.55f, 0.9f), woodWarm);
            CreateVisualCube("OrbitScreenSignBoard", sign.transform,
                new Vector3(0f, 0.35f, 0f),
                new Vector3(1.1f, 0.55f, 0.06f), accentRed);
        }

        private static void BuildTrafficSystem(Transform parent)
        {
            Level1KenneyCarKitTrafficBuilder.EnsureTrafficPrefabs();

            GameObject trafficRoot = new GameObject("Level1_TrafficManager");
            trafficRoot.transform.SetParent(parent, false);

            TrafficManager manager = trafficRoot.AddComponent<TrafficManager>();
            trafficRoot.AddComponent<TrafficPool>();

            GameObject[] trafficPrefabs = Level1KenneyCarKitTrafficBuilder.LoadAllTrafficPrefabs();
            GameObject sedanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Level1KenneyCarKitPaths.SedanPrefab);
            GameObject sportsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Level1KenneyCarKitPaths.SedanSportsPrefab);

            Material[] bodyColors = LoadTrafficBodyColors();
            Material pickupBody = LoadMaterial("Accent_Red.mat");
            Material wheel = LoadMaterial("Dark_Brown.mat");
            Material glass = LoadMaterial("Glass_Blue.mat");
            Material kenneyColormap = AssetDatabase.LoadAssetAtPath<Material>(Level1KenneyCarKitPaths.ColormapMaterial);

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty prefabArray = serializedManager.FindProperty("trafficVehiclePrefabs");
            prefabArray.arraySize = trafficPrefabs.Length;
            for (int i = 0; i < trafficPrefabs.Length; i++)
            {
                prefabArray.GetArrayElementAtIndex(i).objectReferenceValue = trafficPrefabs[i];
            }

            serializedManager.FindProperty("carPrefabA").objectReferenceValue = sedanPrefab;
            serializedManager.FindProperty("carPrefabB").objectReferenceValue = sportsPrefab;
            serializedManager.FindProperty("carAModelRotationOffsetY").floatValue = 0f;
            serializedManager.FindProperty("carBModelRotationOffsetY").floatValue = 0f;
            serializedManager.FindProperty("pickupModelRotationOffsetY").floatValue = 0f;
            serializedManager.FindProperty("carVisualLocalEuler").vector3Value = Vector3.zero;
            serializedManager.FindProperty("carVisualScale").floatValue = 1.05f;
            serializedManager.FindProperty("carBVisualScale").floatValue = 1.05f;
            serializedManager.FindProperty("pickupCarVisualScale").floatValue = 1.55f;
            serializedManager.FindProperty("carScaleVariance").floatValue = 0.03f;
            serializedManager.FindProperty("vehicleLength").floatValue = 1.75f;
            serializedManager.FindProperty("minFollowDistance").floatValue = 6.2f;
            serializedManager.FindProperty("safeFollowDistance").floatValue = 10.5f;
            serializedManager.FindProperty("followGapBuffer").floatValue = 2.4f;
            serializedManager.FindProperty("spawnInterval").floatValue = 2.35f;
            serializedManager.FindProperty("initialVehicleCount").intValue = 2;
            serializedManager.FindProperty("initialSpawnDelay").floatValue = 0.2f;
            serializedManager.FindProperty("pullOverDuration").floatValue = 6f;
            serializedManager.FindProperty("pullOverSpeed").floatValue = 0.35f;
            serializedManager.FindProperty("pickupNormalSpeed").floatValue = 4f;
            serializedManager.FindProperty("pickupCurbSpeed").floatValue = 2f;
            serializedManager.FindProperty("curbOffset").floatValue = 0.8f;
            serializedManager.FindProperty("drawLaneGizmos").boolValue = true;

            SerializedProperty colorsProp = serializedManager.FindProperty("trafficBodyColors");
            colorsProp.arraySize = bodyColors.Length;
            for (int i = 0; i < bodyColors.Length; i++)
            {
                colorsProp.GetArrayElementAtIndex(i).objectReferenceValue = bodyColors[i];
            }

            serializedManager.FindProperty("pickupCarBodyMaterial").objectReferenceValue = pickupBody;
            serializedManager.FindProperty("wheelMaterial").objectReferenceValue = wheel;
            serializedManager.FindProperty("glassMaterial").objectReferenceValue = glass;
            serializedManager.FindProperty("kenneyColormapMaterial").objectReferenceValue = kenneyColormap;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material[] LoadTrafficBodyColors()
        {
            return new[]
            {
                LoadMaterial("Accent_Red.mat"),
                LoadMaterial("Wood_Warm.mat"),
                LoadMaterial("Foliage_Green.mat"),
                LoadMaterial("Produce_Orange.mat"),
                LoadMaterial("Market_Red.mat"),
                LoadMaterial("Coin_Gold.mat"),
                LoadMaterial("Produce_Yellow.mat"),
                LoadMaterial("House_Cream.mat")
            };
        }

        private static void ConfigureTrafficVisualUtility()
        {
            Level1TrafficVisualUtility.Configure(
                LoadTrafficBodyColors(),
                LoadMaterial("Accent_Red.mat"),
                LoadMaterial("Dark_Brown.mat"),
                LoadMaterial("Glass_Blue.mat"),
                AssetDatabase.LoadAssetAtPath<Material>(Level1KenneyCarKitPaths.ColormapMaterial));
        }

        private static void ApplyParkedCarLook(GameObject carRoot, int bodyColorIndex)
        {
            if (carRoot == null)
            {
                return;
            }

            ConfigureTrafficVisualUtility();
            Level1TrafficVisualUtility.ApplyCarLook(carRoot, bodyColorIndex);
        }

        private static void FixTrafficCars(Transform outdoorRoot)
        {
            if (outdoorRoot == null)
            {
                return;
            }

            ConfigureTrafficVisualUtility();
            Material[] palette = LoadTrafficBodyColors();
            int paletteLength = palette.Length;
            if (paletteLength == 0)
            {
                return;
            }

            int carIndex = 0;
            Transform[] all = outdoorRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform node = all[i];
                if (node == null)
                {
                    continue;
                }

                string name = node.name;
                if (name != "ParkedCar" && name != "Car 1" && name != "Car 2"
                    && !name.StartsWith("Traffic"))
                {
                    continue;
                }

                Level1TrafficVisualUtility.ApplyCarLook(node.gameObject, carIndex % paletteLength);
                carIndex++;
            }
        }

        private static void BuildBusStopSign(Transform parent, float x, float z, Material wood, Material accent,
            Material gold)
        {
            GameObject signRoot = new GameObject("BusStopSign");
            signRoot.transform.SetParent(parent, false);
            signRoot.transform.position = new Vector3(x, Level1BusStopRoadSettings.SidewalkSurfaceY, z);

            CreateVisualCube("BusStopSignPost", signRoot.transform, new Vector3(0f, 0.95f, 0f),
                new Vector3(0.14f, 1.9f, 0.14f), wood);
            CreateVisualCube("BusStopSignBoard", signRoot.transform, new Vector3(0f, 1.95f, 0f),
                new Vector3(1.05f, 0.52f, 0.08f), accent);
            CreateVisualCube("BusStopSignHeader", signRoot.transform, new Vector3(0f, 2.28f, 0f),
                new Vector3(0.9f, 0.12f, 0.09f), gold);
            CreateVisualCube("BusStopSignLabelA", signRoot.transform, new Vector3(0f, 1.95f, 0.05f),
                new Vector3(0.72f, 0.1f, 0.04f), gold);
            CreateVisualCube("BusStopSignLabelB", signRoot.transform, new Vector3(0f, 1.78f, 0.05f),
                new Vector3(0.72f, 0.1f, 0.04f), gold);
        }

        private static void BuildBusPickupInteractPoint(Transform parent, Material markerGold, Material accentRed)
        {
            GameObject interact = new GameObject("BusPickupInteractPoint");
            interact.transform.SetParent(parent, false);
            interact.transform.position = new Vector3(
                Level1BusStopRoadSettings.CrosswalkCenterX,
                Level1BusStopRoadSettings.SidewalkSurfaceY + 0.55f,
                Level1BusStopRoadSettings.SidewalkCenterZ - 0.45f);

            SphereCollider trigger = interact.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.35f;

            BusPickupInteraction interaction = interact.AddComponent<BusPickupInteraction>();
            interaction.SetPrompt("Nhấn E để kiểm tra trạm xe buýt");

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "PickupMarker";
            marker.transform.SetParent(interact.transform, false);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = new Vector3(0.95f, 0.04f, 0.95f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            ApplyMaterialPersistent(marker, markerGold);
            marker.AddComponent<BusPickupMarker>();

            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "PickupBeacon";
            beacon.transform.SetParent(interact.transform, false);
            beacon.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            beacon.transform.localScale = new Vector3(0.12f, 0.55f, 0.12f);
            Object.DestroyImmediate(beacon.GetComponent<Collider>());
            ApplyMaterialPersistent(beacon, accentRed);
        }

        private static void BuildSideDressing(Transform parent, Material foliageGreen, Material woodWarm)
        {
            GameObject dressing = new GameObject("SideDressing");
            dressing.transform.SetParent(parent, false);

            // Corridor zone (|x| <= PathCorridorHalfWidth): only low props — never tall trees.
            float[] corridorZs = { -8f, -11f, -14f, -17f, -20f, -35f };
            for (int i = 0; i < corridorZs.Length; i++)
            {
                float z = corridorZs[i];
                float leftX = -PathCorridorHalfWidth + 0.6f;
                float rightX = PathCorridorHalfWidth - 0.6f;
                if (z <= -24f)
                {
                    leftX = -(SidePropMinX - 0.25f);
                    rightX = SidePropMinX - 0.25f;
                }

                if (IsHighwayClearanceZone(leftX, z) && IsHighwayClearanceZone(rightX, z))
                {
                    continue;
                }

                if (!IsHighwayClearanceZone(leftX, z))
                {
                    SpawnScatter(dressing.transform, NaturePrefabs + "/bush1.prefab", "CorridorBushL",
                        new[] { new Vector3(leftX, 0f, z) }, 0.72f + (i % 3) * 0.04f);
                }

                if (!IsHighwayClearanceZone(rightX, z - 0.4f))
                {
                    SpawnScatter(dressing.transform, NaturePrefabs + "/bush2.prefab", "CorridorBushR",
                        new[] { new Vector3(rightX, 0f, z - 0.4f) }, 0.7f + (i % 2) * 0.05f);
                }

                if (i % 3 == 0)
                {
                    Vector3 flowerL = new Vector3(leftX + 0.2f, 0f, z + 0.5f);
                    Vector3 flowerR = new Vector3(rightX - 0.2f, 0f, z - 0.5f);
                    if (!IsHighwayClearanceZone(flowerL.x, flowerL.z))
                    {
                        InstantiatePrefab(NaturePrefabs + "/flower1-2.prefab", dressing.transform,
                            flowerL, Quaternion.identity, Vector3.one * 0.88f,
                            "CorridorFlowerL_" + i, stripCollider: true);
                    }

                    if (!IsHighwayClearanceZone(flowerR.x, flowerR.z))
                    {
                        InstantiatePrefab(NaturePrefabs + "/flower2-1.prefab", dressing.transform,
                            flowerR, Quaternion.identity, Vector3.one * 0.86f,
                            "CorridorFlowerR_" + i, stripCollider: true);
                    }
                }

                if (i % 5 == 2 && z > -24f)
                {
                    Vector3 potL = new Vector3(leftX + 1.1f, 0f, z + 0.35f);
                    Vector3 potR = new Vector3(rightX - 1.1f, 0f, z - 0.15f);
                    if (!IsHighwayClearanceZone(potL.x, potL.z))
                    {
                        InstantiatePrefab(NaturePrefabs + "/flower3-2.prefab", dressing.transform,
                            potL, Quaternion.identity, Vector3.one * 0.82f,
                            "CorridorFlowerPotL_" + i, stripCollider: true);
                    }

                    if (!IsHighwayClearanceZone(potR.x, potR.z))
                    {
                        InstantiatePrefab(NaturePrefabs + "/flower1-1.prefab", dressing.transform,
                            potR, Quaternion.identity, Vector3.one * 0.8f,
                            "CorridorFlowerPotR_" + i, stripCollider: true);
                    }
                }
            }

            // Border trees — pushed to map edge only; tagged for camera occlusion fallback.
            string[] borderTreePrefabs =
            {
                OccaPrefabs + "/Pine Tree_3.prefab",
                OccaPrefabs + "/Tree_4.prefab",
                OccaPrefabs + "/Pine Tree_4.prefab",
                OccaPrefabs + "/Tree_2.prefab",
                OccaPrefabs + "/Tree_6.prefab"
            };

            for (int i = 0; i < corridorZs.Length; i++)
            {
                float z = corridorZs[i];
                if (z > -33f && z < -22f)
                {
                    continue;
                }

                float scale = 0.52f + (i % 3) * 0.04f;
                string prefab = borderTreePrefabs[i % borderTreePrefabs.Length];

                GameObject borderL = InstantiatePrefab(prefab, dressing.transform,
                    new Vector3(-BorderTreeHalfWidth, 0f, z - 0.5f), Quaternion.Euler(0f, i * 27f, 0f),
                    Vector3.one * scale, "BorderTreeL_" + i, stripCollider: true);
                ApplyMaterialPersistent(borderL, foliageGreen);
                TagCameraOccluder(borderL);

                GameObject borderR = InstantiatePrefab(borderTreePrefabs[(i + 2) % borderTreePrefabs.Length],
                    dressing.transform,
                    new Vector3(BorderTreeHalfWidth, 0f, z - 1f), Quaternion.Euler(0f, -i * 25f, 0f),
                    Vector3.one * scale, "BorderTreeR_" + i, stripCollider: true);
                ApplyMaterialPersistent(borderR, foliageGreen);
                TagCameraOccluder(borderR);
            }

            SpawnScatter(dressing.transform, NaturePrefabs + "/rock3.prefab", "MidRock", new[]
            {
                new Vector3(-9.5f, 0f, -12f), new Vector3(9.6f, 0f, -18f)
            }, 0.45f);
        }

        /// <summary>Outer ring of hills, rocks, and trees that block the map edge from every camera angle.</summary>
        private static void BuildOutdoorWalkCollider(Transform parent)
        {
            float centerZ = (SpawnZ + PathEndZ) * 0.5f;
            float depth = Mathf.Abs(SpawnZ - PathEndZ) + 3f;
            float centerY = WalkSurfaceY - WalkColliderThickness * 0.5f;
            CreateInvisibleWall("OutdoorWalkCollider", parent,
                new Vector3(0f, centerY, centerZ),
                new Vector3(Mathf.Abs(OutdoorMaxX) * 2f + 1.4f, WalkColliderThickness, depth));
        }

        private static void BuildPerimeterEnclosure(Transform parent, Material foliageGreen, Material earthBrown,
            Material woodWarm)
        {
            GameObject perimeter = new GameObject("PerimeterEnclosure");
            perimeter.transform.SetParent(parent, false);

            float backZ = VisualGroundFarZ + 1.5f;
            float frontZ = VisualGroundNearZ - 1.2f;

            // Backdrop hills — pushed far outside the highway frustum so meshes never clip lanes.
            ApplyMaterialPersistent(
                InstantiatePrefab(OccaPrefabs + "/Mountain_2.prefab", perimeter.transform,
                    new Vector3(-32f, -0.2f, backZ - 2.5f), Quaternion.Euler(0f, 38f, 0f), Vector3.one * 0.32f,
                    "BackdropHillL"),
                earthBrown);
            ApplyMaterialPersistent(
                InstantiatePrefab(OccaPrefabs + "/Mountain_3.prefab", perimeter.transform,
                    new Vector3(0f, -0.22f, backZ - 3.5f), Quaternion.Euler(0f, 0f, 0f), Vector3.one * 0.3f,
                    "BackdropHillC"),
                earthBrown);
            ApplyMaterialPersistent(
                InstantiatePrefab(OccaPrefabs + "/Mountain_1.prefab", perimeter.transform,
                    new Vector3(32f, -0.2f, backZ - 2.5f), Quaternion.Euler(0f, -32f, 0f), Vector3.one * 0.32f,
                    "BackdropHillR"),
                earthBrown);

            // Side + back cliff shelves — brown earth (cliff1–4 mix for silhouette variety).
            string[] cliffPrefabs =
            {
                NaturePrefabs + "/cliff1.prefab",
                NaturePrefabs + "/cliff2.prefab",
                NaturePrefabs + "/cliff3.prefab",
                NaturePrefabs + "/cliff4.prefab"
            };

            Vector3[] cliffSpots =
            {
                new Vector3(-VisualGroundHalfWidth + 0.8f, 0.1f, -12f),
                new Vector3(-VisualGroundHalfWidth + 0.6f, 0.1f, -28f),
                new Vector3(-VisualGroundHalfWidth + 0.7f, 0.1f, -40f),
                new Vector3(VisualGroundHalfWidth - 0.8f, 0.1f, -16f),
                new Vector3(VisualGroundHalfWidth - 0.6f, 0.1f, -32f),
                new Vector3(VisualGroundHalfWidth - 0.7f, 0.1f, -42f),
                new Vector3(-10f, 0.08f, backZ - 0.5f),
                new Vector3(10f, 0.08f, backZ - 0.8f),
                new Vector3(-6f, 0.08f, backZ - 1f),
                new Vector3(6f, 0.08f, backZ - 1.2f)
            };

            for (int i = 0; i < cliffSpots.Length; i++)
            {
                Vector3 spot = cliffSpots[i];
                if (spot.z > -36f && Mathf.Abs(spot.x) < 14f)
                {
                    continue;
                }

                if (Level1BusStopRoadSettings.OverlapsRoad(spot.x, spot.z))
                {
                    continue;
                }

                GameObject cliff = InstantiatePrefab(cliffPrefabs[i % cliffPrefabs.Length], perimeter.transform,
                    spot, Quaternion.Euler(0f, i * 41f, 0f), Vector3.one * (0.52f + (i % 3) * 0.06f),
                    "EdgeCliff_" + i);
                ApplyMaterialPersistent(cliff, earthBrown);
            }

            SpawnScatter(perimeter.transform, NaturePrefabs + "/rock1.prefab", "EdgeRock", new[]
            {
                new Vector3(-17.5f, 0f, -8f), new Vector3(17.2f, 0f, -10f), new Vector3(-17.8f, 0f, -20f),
                new Vector3(17.5f, 0f, -22f), new Vector3(-17.3f, 0f, -32f), new Vector3(17.6f, 0f, -34f),
                new Vector3(-16.8f, 0f, -42f), new Vector3(17f, 0f, -44f)
            }, 0.65f, earthBrown);

            ApplyMaterialPersistent(
                InstantiatePrefab(OccaPrefabs + "/Rock_3.prefab", perimeter.transform,
                    new Vector3(-15f, 0f, backZ + 0.8f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 0.85f,
                    "BackRockL"),
                earthBrown);
            ApplyMaterialPersistent(
                InstantiatePrefab(OccaPrefabs + "/Rock_4.prefab", perimeter.transform,
                    new Vector3(15f, 0f, backZ + 0.8f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 0.8f,
                    "BackRockR"),
                earthBrown);

        }

        /// <summary>Village travel props — cozy Tet countryside without entering the walkable path.</summary>
        private static void BuildCountrysideDetails(Transform parent, Material woodWarm, Material houseCream,
            Material roofDark)
        {
            GameObject details = new GameObject("CountrysideDetails");
            details.transform.SetParent(parent, false);

            // Distant farm silhouettes — pushed to the sides so they frame the path, not block it.
            GameObject houseL = InstantiatePrefab(OccaPrefabs + "/House_2.prefab", details.transform,
                new Vector3(-16.5f, 0f, -14f), Quaternion.Euler(0f, 72f, 0f), Vector3.one * 0.52f, "DistantHouseL");
            ApplyDistantHouseMaterials(houseL, houseCream, roofDark);
            TagCameraOccluder(houseL);
            GameObject houseR = InstantiatePrefab(OccaPrefabs + "/House_1.prefab", details.transform,
                new Vector3(16.5f, 0f, -14f), Quaternion.Euler(0f, -68f, 0f), Vector3.one * 0.5f, "DistantHouseR");
            ApplyDistantHouseMaterials(houseR, houseCream, roofDark);
            TagCameraOccluder(houseR);

        }

        private static void BuildNatureScatter(Transform parent, Material foliageGreen, Material flowerYellow,
            Material earthBrown)
        {
            GameObject scatter = new GameObject("NatureScatter");
            scatter.transform.SetParent(parent, false);

            SpawnScatter(scatter.transform, NaturePrefabs + "/rock2.prefab", "StreetRock", new[]
            {
                new Vector3(-SidePropMinX - 0.35f, 0f, -14f), new Vector3(SidePropMinX + 0.4f, 0f, -34f)
            }, 0.48f, earthBrown);

            SpawnScatter(scatter.transform, NaturePrefabs + "/flower1-2.prefab", "WildFlower", new[]
            {
                new Vector3(-3.5f, 0f, -11.5f), new Vector3(3.8f, 0f, -20f)
            }, 1.12f, flowerYellow);
        }

        private static void SpawnScatter(Transform parent, string prefabPath, string namePrefix, Vector3[] spots,
            float scale, Material material = null, bool stripCollider = true)
        {
            for (int i = 0; i < spots.Length; i++)
            {
                if (IsHighwayClearanceZone(spots[i].x, spots[i].z))
                {
                    continue;
                }

                GameObject instance = InstantiatePrefab(prefabPath, parent, spots[i],
                    Quaternion.Euler(0f, i * 37f, 0f), Vector3.one * scale, namePrefix + "_" + i, stripCollider);
                if (material != null)
                {
                    ApplyMaterialPersistent(instance, material);
                }
            }
        }

        /// <summary>True when a prop must not be placed (driving lanes + bus-stop pickup apron).</summary>
        private static bool IsHighwayClearanceZone(float x, float z)
        {
            if (Level1BusStopRoadSettings.OverlapsRoad(x, z, 1.1f, 1.0f))
            {
                return true;
            }

            return x >= -6.2f && x <= 4.5f
                && z >= Level1BusStopRoadSettings.LaneNorthEdgeZ - 0.55f
                && z <= Level1BusStopRoadSettings.SidewalkFarZ + 0.65f;
        }

        private static void BuildPathBoundaries(Transform parent)
        {
            Level1OutdoorPathBoundaryBuilder.Rebuild(parent);
        }

        private static void BuildOutdoorLighting(Transform parent)
        {
            GameObject lighting = new GameObject("OutdoorLighting");
            lighting.transform.SetParent(parent, false);

            GameObject sun = new GameObject("OutdoorSun");
            sun.transform.SetParent(lighting.transform, false);
            sun.transform.rotation = Quaternion.Euler(28f, -38f, 0f);
            Light directional = sun.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = 1.42f;
            directional.color = new Color(1f, 0.95f, 0.84f);
            directional.shadows = LightShadows.Soft;
            directional.shadowStrength = 0.36f;
            directional.shadowNearPlane = 0.75f;
            sun.SetActive(true);
        }

        private static Transform CreateSpawnPoint(Transform parent)
        {
            GameObject spawn = new GameObject("Level1_OutdoorSpawn");
            spawn.transform.SetParent(parent, false);
            spawn.transform.position = new Vector3(0f, WalkSurfaceY, SpawnZ);
            spawn.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            return spawn.transform;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale,
            Material material, bool keepCollider = true)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            if (material != null)
            {
                cube.GetComponent<Renderer>().sharedMaterial = material;
            }

            if (!keepCollider)
            {
                Collider collider = cube.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            return cube;
        }

        private static GameObject CreateVisualCube(string name, Transform parent, Vector3 position, Vector3 scale,
            Material material)
        {
            return CreateCube(name, parent, position, scale, material, keepCollider: false);
        }

        private static GameObject CreateGroundSlab(string name, Transform parent, Vector3 position, Vector3 scale,
            Material material)
        {
            return CreateVisualCube(name, parent, position, scale, material);
        }

        private static void CreateInvisibleWall(string name, Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = CreateCube(name, parent, position, scale, null);
            Object.DestroyImmediate(wall.GetComponent<Renderer>());
        }

        private static GameObject InstantiatePrefab(string path, Transform parent, Vector3 position,
            Quaternion rotation, Vector3 scale, string name, bool stripCollider = false)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("[Level1OutdoorAreaBuilder] Missing prefab: " + path);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;
            if (stripCollider)
            {
                StripColliders(instance);
            }

            return instance;
        }

        private static void StripColliders(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    Object.DestroyImmediate(colliders[i]);
                }
            }
        }

        private static GameObject InstantiateBrownFence(Transform parent, Vector3 position, Quaternion rotation,
            Vector3 scale, string name, Material woodWarm)
        {
            GameObject fence = InstantiatePrefab(OccaPrefabs + "/Fence.prefab", parent, position, rotation, scale,
                name);
            ApplyMaterialPersistent(fence, woodWarm);
            return fence;
        }

        private static void ApplyMaterialRecursive(GameObject root, Material material)
        {
            ApplyMaterialPersistent(root, material);
        }

        /// <summary>Assigns material instances so overrides persist on prefab instances in the saved scene.</summary>
        private static void ApplyMaterialPersistent(GameObject root, Material material)
        {
            if (root == null || material == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = material;
                }

                renderers[i].materials = mats;
            }
        }

        public struct OutdoorMaterialSet
        {
            public Material Grass;
            public Material Garden;
            public Material Road;
            public Material EarthBrown;
            public Material RockEdge;
            public Material WoodWarm;
            public Material AccentRed;
            public Material FoliageGreen;
            public Material FlowerYellow;
            public Material LanternGold;
            public Material HouseCream;
            public Material RoofDark;
            public Material PathStone;
        }

        private static OutdoorMaterialSet LoadOutdoorMaterialSet()
        {
            return new OutdoorMaterialSet
            {
                Grass = LoadMaterial("Grass_Ground.mat"),
                Garden = LoadMaterial("Garden_Ground.mat"),
                Road = LoadMaterial("Road_Ground.mat"),
                EarthBrown = LoadMaterial("Dark_Brown.mat"),
                RockEdge = LoadMaterial("Rock_Edge.mat"),
                WoodWarm = LoadMaterial("Wood_Warm.mat"),
                AccentRed = LoadMaterial("Accent_Red.mat"),
                FoliageGreen = LoadMaterial("Foliage_Green.mat"),
                FlowerYellow = LoadMaterial("Flower_Yellow.mat"),
                LanternGold = LoadMaterial("Lantern_Gold.mat"),
                HouseCream = LoadMaterial("House_Cream.mat"),
                RoofDark = LoadMaterial("Roof_Dark.mat"),
                PathStone = LoadMaterial("Urban_Path.mat")
            };
        }

        /// <summary>
        /// Ensures every outdoor prop uses Cho Tet URP materials instead of broken/default prefab shaders (white/pink).
        /// </summary>
        public static int FinalizeOutdoorAreaMaterials(Transform outdoorRoot, OutdoorMaterialSet palette)
        {
            if (outdoorRoot == null)
            {
                return 0;
            }

            int fixedCount = 0;
            System.Collections.Generic.HashSet<int> distantHousesDone =
                new System.Collections.Generic.HashSet<int>();
            Renderer[] renderers = outdoorRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || ShouldSkipOutdoorRenderer(renderer))
                {
                    continue;
                }

                Transform distantHouseRoot = FindAncestorNamedContains(renderer.transform, "DistantHouse");
                if (distantHouseRoot != null)
                {
                    int houseId = distantHouseRoot.GetInstanceID();
                    if (distantHousesDone.Add(houseId))
                    {
                        ApplyDistantHouseMaterials(distantHouseRoot.gameObject, palette.HouseCream, palette.RoofDark);
                        fixedCount++;
                    }

                    continue;
                }

                if (Level1TrafficVisualUtility.IsTrafficCarPart(renderer.gameObject.name))
                {
                    ConfigureTrafficVisualUtility();
                    int colorIndex = Mathf.Abs(renderer.transform.root.GetInstanceID()) % LoadTrafficBodyColors().Length;
                    Level1TrafficVisualUtility.ApplyCarLookToRenderer(renderer, colorIndex);
                    fixedCount++;
                    continue;
                }

                Material resolved = ResolveOutdoorMaterial(renderer.gameObject.name, palette);
                if (resolved == null)
                {
                    continue;
                }

                if (!RendererUsesProjectMaterial(renderer, resolved) || RendererUsesBrokenMaterials(renderer))
                {
                    ApplyMaterialPersistent(renderer.gameObject, resolved);
                    fixedCount++;
                }
            }

            return fixedCount;
        }

        private static Transform FindAncestorNamedContains(Transform transform, string nameFragment)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name.IndexOf(nameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return current;
                }

                if (current.parent == null || current.name == "Level1_OutdoorArea")
                {
                    break;
                }

                current = current.parent;
            }

            return null;
        }

        private static bool ShouldSkipOutdoorRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return true;
            }

            Transform current = renderer.transform;
            while (current != null)
            {
                string name = current.name;
                if (name == "Ground" || name == "TravelRoad" || name == "Level1_ParkedBus"
                    || name == "BusStopRoadLayout" || name == "UrbanPedestrianPavement"
                    || name == Level1SimplePolyAcrossRoadBuilder.RootName
                    || name == Level1SimplePolyPlayerSideFacadeBuilder.RootName)
                {
                    return true;
                }

                if (name == "Level1_HouseExterior")
                {
                    return true;
                }

                if (name == "Level1_OutdoorPathBoundaries" || name == "OutdoorLighting"
                    || name == "TravelRoad" || name == "Level1_ParkedBus")
                {
                    return true;
                }

                if (name == "Level1_OutdoorQuaterniusStreetscape")
                {
                    return true;
                }

                if (name == "Level1_OutdoorCrowd")
                {
                    return true;
                }

                if (name == "Ambient_Street_Props")
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static Material ResolveOutdoorMaterial(string objectName, OutdoorMaterialSet palette)
        {
            string lower = objectName.ToLowerInvariant();
            if (lower.Contains("busbody") || lower.Contains("busroof") || lower.Contains("buswindow")
                || lower.Contains("busdoor") || lower.Contains("busheadlight") || lower.Contains("buswheel")
                || lower.Contains("busbumper") || lower.Contains("pickupmarker") || lower.Contains("pickupbeacon")
                || lower.Contains("asphalt") || lower.Contains("crosswalk") || lower.Contains("lanedash")
                || lower.Contains("laneedge") || lower.Contains("busstopsign") || lower.Contains("busstopcurb")
                || lower.Contains("busstopsidewalk") || lower.Contains("roadgrass")
                || lower.Contains("roadshoulder") || lower.Contains("pathtoroad")
                || lower.Contains("pavement") || lower.Contains("sidewalk") || lower.Contains("pathcurb")
                || lower.Contains("outdoorbuildings") || lower.Contains("quaternius"))
            {
                return null;
            }

            if (lower.Contains("lantern"))
            {
                return palette.LanternGold;
            }

            if (lower.Contains("pathpiece") || lower.Contains("exitpathstone"))
            {
                return palette.PathStone;
            }

            if (lower.Contains("tree") || lower.Contains("pine"))
            {
                return palette.FoliageGreen;
            }

            if (lower.Contains("flower") && !lower.Contains("pot"))
            {
                return palette.FlowerYellow;
            }

            if (lower.Contains("bush") || lower.Contains("grass") || lower.Contains("meadow")
                || lower.Contains("corridor") || lower.Contains("pot") || lower.Contains("plant"))
            {
                return palette.FoliageGreen;
            }

            if (lower.Contains("rock") || lower.Contains("cliff") || lower.Contains("mountain")
                || lower.Contains("backdrop") || lower.Contains("hill") || lower.Contains("edge"))
            {
                return palette.EarthBrown;
            }

            if (lower.Contains("sign") || lower.Contains("cone") || lower.Contains("accent"))
            {
                return palette.AccentRed;
            }

            if (lower.Contains("fence") || lower.Contains("bench") || lower.Contains("crate") || lower.Contains("box")
                || lower.Contains("basket") || lower.Contains("wooden") || lower.Contains("mailbox")
                || lower.Contains("mail") || lower.Contains("dustbin") || lower.Contains("scooty")
                || lower.Contains("pole") || lower.Contains("bus") || lower.Contains("roadside")
                || lower.Contains("alley") || lower.Contains("crate") || lower.Contains("shelter"))
            {
                return palette.WoodWarm;
            }

            return palette.WoodWarm;
        }

        private static void ApplyDistantHouseMaterials(GameObject houseRoot, Material wallMaterial, Material roofMaterial)
        {
            if (houseRoot == null || wallMaterial == null || roofMaterial == null)
            {
                return;
            }

            Renderer[] renderers = houseRoot.GetComponentsInChildren<Renderer>(true);
            float roofSplitY = houseRoot.transform.position.y + 1.35f;
            for (int i = 0; i < renderers.Length; i++)
            {
                Material pick = renderers[i].bounds.center.y >= roofSplitY ? roofMaterial : wallMaterial;
                ApplyMaterialPersistent(renderers[i].gameObject, pick);
            }
        }

        private static bool RendererUsesProjectMaterial(Renderer renderer, Material expected)
        {
            if (renderer == null || expected == null)
            {
                return false;
            }

            Material[] mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != expected)
                {
                    return false;
                }
            }

            return mats.Length > 0;
        }

        private static bool RendererUsesBrokenMaterials(Renderer renderer)
        {
            if (renderer == null)
            {
                return true;
            }

            Material[] mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (UsesBrokenMaterial(mats[i]))
                {
                    return true;
                }
            }

            return mats.Length == 0;
        }

        private static bool UsesBrokenMaterial(Material material)
        {
            if (material == null)
            {
                return true;
            }

            if (material.shader == null)
            {
                return true;
            }

            string shaderName = material.shader.name;
            if (shaderName.Contains("Hidden/InternalErrorShader") || shaderName.Contains("Hidden/Universal"))
            {
                return true;
            }

            if (material.name.Contains("Default") || material.name == "Lit")
            {
                return true;
            }

            // Built-in Standard materials from third-party prefabs often render white in URP.
            if (shaderName.Contains("Standard") && !shaderName.Contains("Universal"))
            {
                return true;
            }

            return false;
        }

        private static Material LoadMaterial(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
        }

        private static void StripDecorShadows(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static void TagCameraOccluder(GameObject root)
        {
            if (root == null || root.GetComponent<CameraViewOccluder>() != null)
            {
                return;
            }

            root.AddComponent<CameraViewOccluder>();
        }

        private static void MarkSceneDirty()
        {
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}
#endif
