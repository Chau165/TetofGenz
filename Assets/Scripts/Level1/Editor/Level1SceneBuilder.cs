#if UNITY_EDITOR
using ChoTet.Editor;
using ChoTet.Level1;
using ChoTet.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Editor
{
    public static class Level1SceneBuilder
    {
        private const string RiggedExplorerRoot = PlayerCharacterVisualPaths.LegacyVisualRoot;
        private const string RiggedExplorerModelPath = PlayerCharacterVisualPaths.LegacyVisualPrefab;
        private const string RiggedExplorerControllerPath = PlayerCharacterVisualPaths.LegacyAnimatorController;

        public const string SceneAssetPath = "Assets/Scenes/Level1_PackingRoom.unity";
        public const string EnvironmentVersionTag = "Level1RoomEnvironmentV65SkyFillOnly";

        private const float FloorPropLift = 0.003f;
        private const float RugFloorLift = 0.005f;

        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials";

        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        internal const float WallHeight = 3.2f;
        private const float WallHalfThickness = 0.09f;
        /// <summary>Minimum inset from inner wall face (0.2–0.5 m requested).</summary>
        private const float WallInset = 0.38f;
        internal const float FloorTopY = 0f;
        private const float DoorHalfWidth = 0.8f;
        private const float DoorOpeningHeight = 2.08f;
        private const float CharacterModelScale = 0.68f;
        private const float CharacterHeight = 1.18f;
        private const float CharacterRadius = 0.2f;
        private const float KenneyFurnitureScale = 0.28f;
        /// <summary>Adult-scale desk/chair set, including attached decorative props.</summary>
        private const float KenneyDeskChairScale = 0.26f;
        private const float KenneyBedScale = 0.34f;
        private const float KenneyDeskTopY = 0.70f;
        /// <summary>Low Poly Office Pack FBX import scale (globalScale 4 in importer).</summary>
        private const float LowPolyOfficePropScale = 1f;

        private static float InnerMinX => -RoomHalfWidth + WallHalfThickness + WallInset;
        private static float InnerMaxX => RoomHalfWidth - WallHalfThickness - WallInset;
        private static float InnerMinZ => -RoomHalfDepth + WallHalfThickness + WallInset;
        private static float InnerMaxZ => RoomHalfDepth - WallHalfThickness - WallInset;

        internal static class RoomLayout
        {
            // Sleeping — front-right zone with clear walk lane through room center.
            public static readonly Vector3 Bed = new(2.68f, FloorTopY, 2.78f);
            public const float BedYaw = 90f;
            public static readonly Vector3 ClothesPileCenter = new(1.72f, FloorTopY, 1.92f);
            public static readonly Vector3 BedRug = new(2.68f, FloorTopY, 2.78f);
            public static readonly Vector3 BedPhoneLocal = new(0.02f, 0f, 0.16f);
            public static readonly Vector3 BedPillowLocal = new(0f, 0f, 0.44f);

            // Study — desk on left wall; chair pulled into open floor facing desk.
            public static readonly Vector3 Desk = new(-4.22f, FloorTopY, 0.05f);
            public const float DeskYaw = 90f;
            public static readonly Vector3 Chair = new(-1.42f, FloorTopY, 1.62f);
            public const float ChairYaw = -118f;
            public static readonly Vector3 StudyChairBackpack = new(-1.08f, FloorTopY, 1.28f);
            public const float IdCardDeskU = -0.82f;
            public const float IdCardDeskV = 0.72f;
            public const float WalletDeskU = 0.76f;
            public const float WalletDeskV = -0.52f;
            public const float WaterBottleDeskU = -0.38f;
            public const float WaterBottleDeskV = -0.8f;

            // Storage — back wall row with breathing room from bed zone.
            public static readonly Vector3 Wardrobe = new(3.72f, FloorTopY, -2.58f);
            public const float WardrobeYaw = -90f;
            public static readonly Vector3 Shelf = new(2.18f, FloorTopY, -2.58f);
            public const float ShelfYaw = 0f;
            public static readonly Vector3 ShelfDocumentsLocal = new(0.04f, 0f, 0.02f);
            public static readonly Vector3 ShelfSnacksLocal = new(-0.02f, 0f, 0.04f);
            /// <summary>Open shelf tiers — bottom to top, one prop each.</summary>
            public const float ShelfTierBottom = 0.16f;
            public const float ShelfTierLowerMid = 0.40f;
            public const float ShelfTierUpperMid = 0.64f;
            public const float ShelfTierTop = 0.84f;
            public const float ShelfDocumentsTier = ShelfTierUpperMid;
            public const float ShelfSnacksTier = ShelfTierLowerMid;

            // Exit — back wall door side; keep spacing so props do not overlap.
            public static readonly Vector3 Suitcase = new(-0.55f, FloorTopY, -3.28f);
            public static readonly Vector3 SlippersLeft = new(-0.92f, FloorTopY, -3.58f);
            public static readonly Vector3 SlippersRight = new(-0.62f, FloorTopY, -3.58f);

            public static readonly Vector3 CornerPlant = new(4.69f, FloorTopY, 3.69f);
            public const float CornerPlantYaw = -135f;

            public static readonly Vector3 PlayerSpawn = new(0f, FloorTopY + 0.02f, 0.15f);
            public static readonly Vector3 CameraLookTarget = new(0f, FloorTopY, -0.05f);
        }

        public static void SnapDeskPropsToSurface()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1SceneBuilder] Exit Play mode before snapping desk props.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] Level1World not found.");
                return;
            }

            GameObject desk = Level1ScenePlacement.FindInRoomObject(worldRoot.transform, "StudyArea/Desk");
            if (desk == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] Desk not found.");
                return;
            }

            Transform packables = Level1ScenePlacement.FindInRoom(worldRoot.transform, "PackableItems");
            Level1ScenePlacement.SnapAllDeskSurfaceProps(desk, packables);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1SceneBuilder] Desk props snapped to tabletop surface.");
        }

        public static void FixStudyDeskProps()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1SceneBuilder] Exit Play mode before fixing desk props.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.Log("[Level1SceneBuilder] Level1World not found. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            Material woodDark = LoadMaterial("Wood_Dark.mat");
            Material woodWarm = LoadMaterial("Wood_Warm.mat");
            Material coinGold = LoadMaterial("Coin_Gold.mat");
            Material glassBlue = LoadMaterial("Glass_Blue.mat");

            Transform studyArea = Level1ScenePlacement.FindInRoom(worldRoot.transform, "StudyArea");
            if (studyArea == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] StudyArea missing. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            ClearStudyDeskProps(studyArea);
            GameObject desk = studyArea.Find("Desk")?.gameObject;
            if (desk == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] Desk missing. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            PopulateStudyDeskProps(desk, woodDark);

            Transform packables = Level1ScenePlacement.FindInRoom(worldRoot.transform, "PackableItems");
            if (packables != null)
            {
                DestroyIfExists(packables, "Wallet");
                DestroyIfExists(packables, "WaterBottle");
            }

            if (packables == null)
            {
                packables = new GameObject("PackableItems").transform;
                Transform indoor = worldRoot.transform.Find("Level1_IndoorArea");
                packables.SetParent(indoor != null ? indoor : worldRoot.transform, false);
            }

            Material giftRed = LoadMaterial("Gift_Red.mat");
            CreatePackableOnDesk(packables, Level1PackingItemId.Wallet, desk, RoomLayout.WalletDeskU,
                RoomLayout.WalletDeskV, new Vector3(0.22f, 0.04f, 0.16f), coinGold, "Wallet");
            CreatePackableOnDesk(packables, Level1PackingItemId.WaterBottle, desk, RoomLayout.WaterBottleDeskU,
                RoomLayout.WaterBottleDeskV, new Vector3(0.12f, 0.22f, 0.12f), glassBlue, "WaterBottle");
            CreatePackableOnDesk(packables, Level1PackingItemId.PersonalDocuments, desk,
                RoomLayout.IdCardDeskU, RoomLayout.IdCardDeskV,
                new Vector3(0.28f, 0.045f, 0.2f), giftRed, "Documents");

            SnapAllDeskProps(desk, packables);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1SceneBuilder] Study desk props refreshed (laptop + office pack).");
        }

        private static void DestroyIfExists(Transform parent, string childPath)
        {
            if (parent == null)
            {
                return;
            }

            Transform child = Level1ScenePlacement.FindInRoom(parent, childPath);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void ClearStudyDeskProps(Transform studyArea)
        {
            Transform desk = studyArea.Find("Desk");
            if (desk == null)
            {
                return;
            }

            string[] propNames =
            {
                "DeskLaptop", "DeskMouse", "DeskLamp", "DeskMug", "DeskPaperBox", "DeskPen", "DeskStapler",
                "DeskNotebook", "DeskPapers"
            };

            for (int i = 0; i < propNames.Length; i++)
            {
                Transform prop = desk.Find(propNames[i]);
                if (prop != null)
                {
                    Object.DestroyImmediate(prop.gameObject);
                }
            }
        }

        private static void PopulateStudyDeskProps(GameObject desk, Material woodDark)
        {
            if (!Level1ScenePlacement.TryGetDeskTopLayout(desk, out DeskTopLayout top))
            {
                Debug.LogWarning("[Level1SceneBuilder] Could not measure desk top — using fallback layout.");
                return;
            }

            // u/v in -1..1 span the full tabletop. Zones kept apart so packables do not stack under the laptop.
            GameObject lamp = PlaceKenneyPropOnDeskFraction(desk, top, 0.88f, -0.55f,
                "lampRoundTable.fbx", Quaternion.identity, 1.15f, "DeskLamp", woodDark);
            Level1ScenePlacement.FitPropFootprintOnDesk(desk, lamp, 0.14f);

            GameObject laptop = PlaceOfficePropOnDeskFraction(desk, top, 0.1f, -0.2f,
                "mesh_laptop_01.fbx", Quaternion.Euler(-14f, 180f, 0f), 2.5f, "DeskLaptop");
            Level1ScenePlacement.FitPropFootprintOnDesk(desk, laptop, 0.5f, 0.36f);

            GameObject mouse = PlaceOfficePropOnDeskFraction(desk, top, 0.72f, 0.26f,
                "mesh_mouse_01.fbx", Quaternion.Euler(0f, 24f, 0f), 1.8f, "DeskMouse");
            Level1ScenePlacement.FitPropFootprintOnDesk(desk, mouse, 0.12f);

            GameObject mug = PlaceOfficePropOnDeskFraction(desk, top, -0.84f, -0.58f,
                "mesh_mug.fbx", Quaternion.identity, 2f, "DeskMug");
            Level1ScenePlacement.FitPropFootprintOnDesk(desk, mug, 0.13f);

            GameObject paperBox = PlaceOfficePropOnDeskFraction(desk, top, -0.58f, 0.34f,
                "mesh_paperbox.fbx", Quaternion.Euler(0f, 18f, 0f), 1.8f, "DeskPaperBox");
            Level1ScenePlacement.FitPropFootprintOnDesk(desk, paperBox, 0.2f);

            GameObject pen = PlaceOfficePropOnDeskFraction(desk, top, 0.84f, 0.64f,
                "mesh_pen_01.fbx", Quaternion.Euler(0f, 40f, 90f), 1.8f, "DeskPen");
            Level1ScenePlacement.FitPropFootprintOnDesk(desk, pen, 0.08f);

            GameObject stapler = PlaceOfficePropOnDeskFraction(desk, top, 0.44f, -0.72f,
                "Mesh_Stapler.fbx", Quaternion.Euler(0f, -30f, 0f), 1.7f, "DeskStapler");
            Level1ScenePlacement.FitPropFootprintOnDesk(desk, stapler, 0.12f);

            SnapAllDeskProps(desk);
        }

        private static void SnapAllDeskProps(GameObject desk, Transform packablesRoot = null)
        {
            if (desk == null)
            {
                return;
            }

            if (packablesRoot == null)
            {
                GameObject worldRoot = GameObject.Find("Level1World");
                if (worldRoot != null)
                {
                    packablesRoot = Level1ScenePlacement.FindInRoom(worldRoot.transform, "PackableItems");
                }
            }

            Level1ScenePlacement.SnapAllDeskSurfaceProps(desk, packablesRoot);
        }

        private static GameObject PlaceKenneyPropOnDeskFraction(GameObject desk, DeskTopLayout top, float u, float v,
            string kenneyFile, Quaternion localRotation, float uniformScale, string name, Material tint)
        {
            Vector3 localOffset = top.LocalPoint(u, v);
            return PlaceKenneyPropOnSurface(desk, kenneyFile, localOffset, localRotation, uniformScale, name, tint);
        }

        private static GameObject PlaceOfficePropOnDeskFraction(GameObject desk, DeskTopLayout top, float u, float v,
            string fileName, Quaternion localRotation, float uniformScale, string name)
        {
            Vector3 localOffset = top.LocalPoint(u, v);
            return PlaceOfficePropOnSurface(desk, fileName, localOffset, localRotation, uniformScale, name);
        }

        private static void CreatePackableOnDesk(Transform parent, Level1PackingItemId itemId, GameObject desk,
            float u, float v, Vector3 visualScale, Material material, string objectName)
        {
            Vector3 position = GetDeskSurfaceWorldPoint(desk, u, v);
            GameObject item = CreatePackableItem(parent, itemId, position, visualScale, material, objectName);
            if (desk != null && item != null)
            {
                Level1ScenePlacement.SnapPropBottomToDeskTop(desk, item);
            }
        }

        private static Vector3 GetDeskSurfaceWorldPoint(GameObject desk, float u, float v)
        {
            if (desk != null && Level1ScenePlacement.TryGetDeskTopLayout(desk, out DeskTopLayout top))
            {
                Vector3 world = desk.transform.TransformPoint(top.LocalPoint(u, v));
                world.y = Level1ScenePlacement.TryGetDeskTopWorldY(desk, out float deskTop)
                    ? deskTop
                    : Level1ScenePlacement.GetSurfaceWorldTop(desk);
                return world;
            }

            return RoomLayout.PlayerSpawn + Vector3.up * 0.5f;
        }

        public static void FixExitAreaClutter()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1SceneBuilder] Exit Play mode before fixing exit area.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                BuildLevel1PackingRoom();
                return;
            }

            Material motherCloth = LoadMaterial("Mother_Cloth.mat");
            Material accentRed = LoadMaterial("Accent_Red.mat");
            Material woodWarm = LoadMaterial("Wood_Warm.mat");
            Material houseCream = LoadMaterial("House_Cream.mat");
            Material produceOrange = LoadMaterial("Produce_Orange.mat");

            Transform exitArea = worldRoot.transform.Find("ExitArea");
            if (exitArea != null)
            {
                Object.DestroyImmediate(exitArea.gameObject);
            }

            BuildExitArea(worldRoot.transform, houseCream, produceOrange, motherCloth, accentRed, woodWarm);
            RemoveLegacyClutter(worldRoot.transform);

            Transform door = worldRoot.transform.Find("DepartureDoor");
            if (door != null)
            {
                float doorZ = -RoomHalfDepth + WallHalfThickness + 0.12f;
                door.position = new Vector3(0f, 1f, doorZ);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1SceneBuilder] Exit area decluttered and door opening sealed.");
        }

        private static void RemoveLegacyClutter(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            DestroyIfExists(worldRoot, "ExitArea/TravelBag");
            DestroyIfExists(worldRoot, "ExitArea/SideTable");
            DestroyIfExists(worldRoot, "SleepingArea/ClothesPile");

            Transform packables = worldRoot.Find("PackableItems");
            GameObject bed = worldRoot.Find("SleepingArea/Bed")?.gameObject;
            if (packables != null && bed != null)
            {
                RepositionBedPackables(packables, bed);
            }

            Transform clothes = packables?.Find("Clothes");
            if (clothes != null)
            {
                RepositionPackableOnFloor(packables, "Clothes", RoomLayout.ClothesPileCenter,
                    new Vector3(0.28f, 0.22f, 0.14f));
            }
        }

        public static void FixWardrobeProps()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1SceneBuilder] Exit Play mode before fixing storage props.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.Log("[Level1SceneBuilder] Level1World not found. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            Material motherCloth = LoadMaterial("Mother_Cloth.mat");
            Material houseCream = LoadMaterial("House_Cream.mat");
            Material woodWarm = LoadMaterial("Wood_Warm.mat");
            Material darkBrown = LoadMaterial("Dark_Brown.mat");

            Transform storageArea = worldRoot.transform.Find("StorageArea");
            if (storageArea == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] StorageArea missing. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            ClearStorageShelfProps(storageArea);
            GameObject shelf = storageArea.Find("Shelf")?.gameObject;
            if (shelf == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] Shelf missing. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            PopulateOpenShelfTiers(shelf, motherCloth, houseCream, darkBrown);
            PopulateWardrobeTopAccent(storageArea.Find("Wardrobe")?.gameObject, houseCream);
            RepositionShelfPackables(worldRoot.transform, shelf);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1SceneBuilder] Storage shelf tiers refreshed (one item per level).");
        }

        private static void ClearStorageShelfProps(Transform storageArea)
        {
            ClearShelfTierProps(storageArea?.Find("Shelf"));
            ClearWardrobeTopProps(storageArea?.Find("Wardrobe"));
        }

        private static void ClearShelfTierProps(Transform shelf)
        {
            if (shelf == null)
            {
                return;
            }

            string[] propNames =
            {
                "ShelfTier_Box", "ShelfTier_Snacks", "ShelfTier_Books", "ShelfTier_Cloth",
                "ShelfDocumentsVisual", "ShelfSnacksVisual"
            };

            for (int i = 0; i < propNames.Length; i++)
            {
                Transform prop = shelf.Find(propNames[i]);
                if (prop != null)
                {
                    Object.DestroyImmediate(prop.gameObject);
                }
            }
        }

        private static void ClearWardrobeTopProps(Transform wardrobe)
        {
            if (wardrobe == null)
            {
                return;
            }

            string[] propNames =
            {
                "WardrobeTravelBag", "WardrobeStorageBox", "WardrobeFoldedCloth",
                "WardrobeBooks", "WardrobeHatBox", "WardrobeTopHatBox"
            };

            for (int i = 0; i < propNames.Length; i++)
            {
                Transform prop = wardrobe.Find(propNames[i]);
                if (prop != null)
                {
                    Object.DestroyImmediate(prop.gameObject);
                }
            }
        }

        private static void PopulateOpenShelfTiers(GameObject shelf, Material motherCloth, Material houseCream,
            Material darkBrown)
        {
            if (shelf == null)
            {
                return;
            }

            if (!Level1ScenePlacement.TryGetShelfBoardLocalPoint(shelf, "Shelf", out Vector3 boardLocal))
            {
                boardLocal = Vector3.zero;
            }

            GameObject box = PlacePropOnShelfTier(shelf, Level1RoomAssetPaths.Market + "/wooden_box.prefab",
                boardLocal, RoomLayout.ShelfTierBottom, Quaternion.Euler(0f, 14f, 0f),
                Vector3.one * 0.24f, "ShelfTier_Box");

            GameObject snacks = PlacePropOnShelfTier(shelf, Level1RoomAssetPaths.Market + "/cookies.prefab",
                boardLocal, RoomLayout.ShelfTierLowerMid, Quaternion.identity,
                Vector3.one * 0.26f, "ShelfTier_Snacks");

            GameObject books = PlacePropOnShelfTier(shelf, Level1RoomAssetPaths.KenneyFbx + "/books.fbx",
                boardLocal, RoomLayout.ShelfTierUpperMid,
                Quaternion.Euler(0f, RoomLayout.ShelfYaw + 6f, 0f), KenneyScaleOf(0.48f),
                "ShelfTier_Books", darkBrown);

            GameObject cloth = PlacePropOnShelfTier(shelf, Level1RoomAssetPaths.Market + "/rag.prefab",
                boardLocal, RoomLayout.ShelfTierTop, Quaternion.Euler(0f, -10f, 0f),
                Vector3.one * 0.22f, "ShelfTier_Cloth");
        }

        /// <summary>Closed wardrobe — single accent on top only; tiered items live on open shelf.</summary>
        private static void PopulateWardrobeTopAccent(GameObject wardrobe, Material houseCream)
        {
            if (wardrobe == null
                || !Level1ScenePlacement.TryGetFurnitureTopLayout(wardrobe, "Wardrobe", out DeskTopLayout top))
            {
                return;
            }

            GameObject hatBox = PlaceKenneyPropOnSurface(wardrobe, "cardboardBoxClosed.fbx",
                top.LocalPoint(0f, 0f), Quaternion.Euler(0f, 18f, 0f), 0.52f,
                "WardrobeTopHatBox", houseCream);
            if (hatBox != null)
            {
                Level1ScenePlacement.FitPropFootprintOnFurniture(wardrobe, "Wardrobe", hatBox, 0.2f);
                Level1ScenePlacement.SnapPropBottomToFurnitureTop(wardrobe, "Wardrobe", hatBox);
            }
        }

        public static void FixSleepingAreaBed()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1SceneBuilder] Exit Play mode before fixing the bed.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.Log("[Level1SceneBuilder] Level1World not found. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            Material motherCloth = LoadMaterial("Mother_Cloth.mat");
            Material houseCream = LoadMaterial("House_Cream.mat");
            Material woodDark = LoadMaterial("Wood_Dark.mat");
            Material woodWarm = LoadMaterial("Wood_Warm.mat");
            Material fabricSageSeat = LoadMaterial("Fabric_SageSeat.mat");
            Material fabricSageBack = LoadMaterial("Fabric_SageBack.mat");

            Transform sleepingArea = worldRoot.transform.Find("SleepingArea");
            if (sleepingArea == null)
            {
                sleepingArea = new GameObject("SleepingArea").transform;
                sleepingArea.SetParent(worldRoot.transform, false);
            }

            ClearSleepingAreaFurniture(sleepingArea);
            PopulateSleepingArea(sleepingArea, motherCloth, houseCream, woodDark, woodWarm, fabricSageSeat,
                fabricSageBack);

            GameObject bed = worldRoot.transform.Find("SleepingArea/Bed")?.gameObject;
            ResolveBedClearance(bed, worldRoot.transform);
            Level1ScenePlacement.RemovePropsOverlappingBed(bed, worldRoot.transform);
            RemoveFloatingSleepingDecor(worldRoot.transform);
            RepositionBedPackables(worldRoot.transform, bed);

            Transform light = worldRoot.transform.Find("BedsideWarm");
            if (light != null)
            {
                light.position = RoomLayout.Bed + new Vector3(0f, 1.75f, 0.35f);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1SceneBuilder] Bed restored at front-right (opposite chair) " + RoomLayout.Bed);
        }

        private static void RemoveFloatingSleepingDecor(Transform worldRoot)
        {
            DestroyIfExists(worldRoot, "SleepingArea/ClothesPile");
            RemoveOrphanOrangeNearClothes(worldRoot);
        }

        private static void RemoveOrphanOrangeNearClothes(Transform worldRoot)
        {
            Transform clothes = Level1ScenePlacement.FindInRoom(worldRoot, "PackableItems/Clothes");
            if (clothes == null)
            {
                return;
            }

            Vector3 anchor = clothes.position;
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Transform t = renderer.transform;
                if (t == clothes || t.IsChildOf(clothes))
                {
                    continue;
                }

                Transform root = t;
                while (root.parent != null && root.parent != worldRoot)
                {
                    root = root.parent;
                }

                string rootName = root.name;
                if (rootName is "BedsideRug" or "SlipperLeft" or "SlipperRight" or "DoorFloorMat" or "Clothes")
                {
                    continue;
                }

                Material mat = renderer.sharedMaterial;
                if (mat == null || (!mat.name.Contains("Produce_Orange") && !mat.name.Contains("Mother_Cloth")))
                {
                    continue;
                }

                Vector3 center = renderer.bounds.center;
                if (Vector3.Distance(new Vector3(center.x, 0f, center.z), new Vector3(anchor.x, 0f, anchor.z)) > 1.35f)
                {
                    continue;
                }

                if (renderer.bounds.min.y > 0.55f)
                {
                    continue;
                }

                Object.DestroyImmediate(root.gameObject);
            }
        }

        private static void RepositionBedPackables(Transform worldRoot, GameObject bed)
        {
            if (worldRoot == null)
            {
                return;
            }

            Transform packables = Level1ScenePlacement.FindInRoom(worldRoot, "PackableItems");
            if (packables == null)
            {
                return;
            }

            Material producePurple = LoadMaterial("Produce_Purple.mat");

            RepositionPackableOnBed(packables, "Smartphone", bed, new Vector3(0.18f, 0f, -0.22f),
                new Vector3(0.1f, 0.02f, 0.16f));
            RepositionPackableOnFloor(packables, "Clothes", RoomLayout.ClothesPileCenter,
                new Vector3(0.28f, 0.22f, 0.14f), producePurple);
        }

        private static void RepositionPackableOnBed(Transform packables, string objectName, GameObject bed,
            Vector3 worldOffset, Vector3 visualScale)
        {
            Transform item = packables.Find(objectName);
            if (item == null || bed == null)
            {
                return;
            }

            Vector3 position = GetBedPackableWorldPoint(bed, worldOffset);
            position.y += visualScale.y * 0.5f;
            item.position = position;

            Transform visual = item.Find("Visual");
            if (visual != null)
            {
                visual.localScale = visualScale;
            }

            Level1ScenePlacement.SnapPropBottomToBedTop(bed, item.gameObject);
        }

        private static void RepositionPackableOnFloor(Transform packables, string objectName, Vector3 floorCenter,
            Vector3 visualScale, Material material = null)
        {
            Transform item = packables.Find(objectName);
            if (item == null)
            {
                return;
            }

            Vector3 position = floorCenter + new Vector3(0f, visualScale.y * 0.5f, 0f);
            item.position = position;

            Transform visual = item.Find("Visual");
            if (visual != null)
            {
                visual.localScale = visualScale;
                if (material != null)
                {
                    Renderer renderer = visual.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = material;
                    }
                }
            }

            Level1ScenePlacement.SnapPropBottomToFloor(item.gameObject, FloorTopY);
        }

        private static void ResolveBedClearance(GameObject bed, Transform worldRoot)
        {
            if (bed == null || worldRoot == null)
            {
                return;
            }

            string[] blockerPaths =
            {
                "StudyArea/DeskChair",
                "StudyArea/Desk",
                "StudyArea/StudyChairBackpack",
                "StorageArea/Shelf",
                "StorageArea/Wardrobe"
            };

            for (int i = 0; i < blockerPaths.Length; i++)
            {
                GameObject blocker = Level1ScenePlacement.FindInRoomObject(worldRoot, blockerPaths[i]);
                if (blocker != null)
                {
                    Level1ScenePlacement.EnsureXZClearance(bed, blocker, 0.75f);
                }
            }

            Level1ScenePlacement.ClampInsideRoom(bed, RoomHalfWidth, RoomHalfDepth, WallInset + 0.15f);
        }

        private static void ClearSleepingAreaFurniture(Transform sleepingArea)
        {
            if (sleepingArea == null)
            {
                return;
            }

            for (int i = sleepingArea.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(sleepingArea.GetChild(i).gameObject);
            }
        }

        public static void PolishPackingRoomLayout()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1SceneBuilder] Exit Play mode before polishing the packing room.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.Log("[Level1SceneBuilder] Level1World not found. Running full room build...");
                BuildLevel1PackingRoom();
                return;
            }

            FinalizeRoomPresentation(worldRoot.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1SceneBuilder] Packing room layout polished — props snapped, overlaps resolved.");
        }

        public static void BuildLevel1PackingRoom()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1SceneBuilder] Exit Play mode before building the packing room.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level1_PackingRoom";

            Material woodWarm = LoadMaterial("Wood_Warm.mat");
            Material woodDark = LoadMaterial("Wood_Dark.mat");
            Material giftRed = LoadMaterial("Gift_Red.mat");
            Material coinGold = LoadMaterial("Coin_Gold.mat");
            Material glassBlue = LoadMaterial("Glass_Blue.mat");
            Material flowerYellow = LoadMaterial("Flower_Yellow.mat");
            Material produceOrange = LoadMaterial("Produce_Orange.mat");
            Material producePurple = LoadMaterial("Produce_Purple.mat");
            Material houseCream = LoadMaterial("House_Cream.mat");
            Material doorDark = LoadMaterial("Door_Dark.mat");
            Material motherCloth = LoadMaterial("Mother_Cloth.mat");
            Material fabricSageSeat = LoadMaterial("Fabric_SageSeat.mat");
            Material fabricSageBack = LoadMaterial("Fabric_SageBack.mat");
            Material darkBrown = LoadMaterial("Dark_Brown.mat");
            Material wallLight = LoadMaterial("Wall_Light.mat");
            Material accentRed = LoadMaterial("Accent_Red.mat");
            Material foliageGreen = LoadMaterial("Foliage_Green.mat");

            Material floorMaterial = Level1FloorMaterialBuilder.EnsureTileFloorMaterial();
            if (floorMaterial == null)
            {
                floorMaterial = LoadMaterial("Floor_WoodWarm.mat");
                if (floorMaterial == null)
                {
                    floorMaterial = EnsureFloorWoodWarmMaterial();
                }
            }

            Material wallMaterial = wallLight != null ? wallLight : woodWarm;
            Material wallAccent = woodDark;
            Material frostedGlass = CreateFrostedWindowGlass(glassBlue, houseCream);

            GameObject worldRoot = new GameObject("Level1World");
            new GameObject(EnvironmentVersionTag).transform.SetParent(worldRoot.transform, false);
            GameObject systemsRoot = new GameObject("Systems");
            systemsRoot.AddComponent<Level1PackingRoomIntro>();

            BuildRoomShell(worldRoot.transform, floorMaterial, wallMaterial, wallAccent, woodDark, woodWarm,
                motherCloth, doorDark, frostedGlass);
            BuildSleepingArea(worldRoot.transform, motherCloth, houseCream, woodDark, woodWarm, fabricSageSeat,
                fabricSageBack);
            BuildStudyArea(worldRoot.transform, woodWarm, woodDark, darkBrown, fabricSageSeat, fabricSageBack);
            BuildStorageArea(worldRoot.transform, woodWarm, woodDark, darkBrown, motherCloth, houseCream);
            BuildExitArea(worldRoot.transform, houseCream, produceOrange, motherCloth, accentRed, woodWarm);
            BuildRoomPersonality(worldRoot.transform, giftRed, flowerYellow, accentRed, houseCream, woodDark,
                motherCloth, foliageGreen);

            GameObject bed = worldRoot.transform.Find("SleepingArea/Bed")?.gameObject;
            ResolveBedClearance(bed, worldRoot.transform);
            Level1ScenePlacement.RemovePropsOverlappingBed(bed, worldRoot.transform);

            Vector3 spawnPosition = RoomLayout.PlayerSpawn;
            CreateMarker("SpawnPoint", spawnPosition, worldRoot.transform);

            GameObject inventoryUiRoot = Level1InventoryUiAssembler.EnsurePrefabInstance(systemsRoot.transform);
            InventoryUIController inventoryUi = inventoryUiRoot != null
                ? inventoryUiRoot.GetComponent<InventoryUIController>()
                : null;

            GameObject inventorySystem = new GameObject("InventorySystem");
            inventorySystem.transform.SetParent(systemsRoot.transform, false);
            InventoryManager inventoryManager = inventorySystem.AddComponent<InventoryManager>();

            InventoryItemData[] itemDefinitions = Level1InventoryUiAssembler.CreateOrUpdateItemDefinitions();
            DoorInteraction departureDoor = CreateDepartureDoor(worldRoot.transform);

            SerializedObject serializedManager = new SerializedObject(inventoryManager);
            serializedManager.FindProperty("inventoryUi").objectReferenceValue = inventoryUi;
            serializedManager.FindProperty("departureDoor").objectReferenceValue = departureDoor;
            serializedManager.FindProperty("requiredItemDefinitions").arraySize = itemDefinitions.Length;
            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                serializedManager.FindProperty("requiredItemDefinitions").GetArrayElementAtIndex(i).objectReferenceValue =
                    itemDefinitions[i];
            }

            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            CreatePackableItems(worldRoot.transform, coinGold, glassBlue, producePurple, produceOrange, giftRed,
                houseCream);
            ResolveDeskPropLayout(worldRoot.transform);
            RemoveLegacyClutter(worldRoot.transform);
            RepositionBedPackables(worldRoot.transform, bed);
            Level1VietnamRoomRestyleBuilder.Apply(worldRoot.transform);
            FinalizeRoomPresentation(worldRoot.transform);
            FinalizeIndoorAreaRoot(worldRoot.transform);

            PlayerTopDownController player = CreatePlayer(spawnPosition, Quaternion.Euler(0f, 90f, 0f));
            player.transform.SetParent(worldRoot.transform, true);
            player.interactionRadius = 1.35f;
            player.walkSpeed = 3.6f;
            player.runSpeed = 5.2f;

            CreateCamera(worldRoot.transform, player.transform);

            Transform outdoorSpawn = Level1OutdoorAreaBuilder.BuildOrUpdate(worldRoot.transform);
            Level1OutdoorAreaBuilder.WirePhaseController(outdoorSpawn, systemsRoot.transform);

            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            ChoTet.Menu.Editor.ChoTetBuildSettings.ApplyMenuFlowScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAssetPath);
            Debug.Log("[Level1SceneBuilder] Built cozy Level 1 rental room at " + SceneAssetPath);
        }

        /// <summary>Snaps props, resolves overlaps, and marks static geometry for smooth indoor play.</summary>
        private static void FinalizeRoomPresentation(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            RemoveLegacyClutter(worldRoot);
            RemoveFloatingSleepingDecor(worldRoot);

            string[] floorZones =
            {
                "SleepingArea", "StudyArea", "StorageArea", "ExitArea", "RoomPersonality"
            };

            for (int i = 0; i < floorZones.Length; i++)
            {
                Transform zone = Level1ScenePlacement.FindInRoom(worldRoot, floorZones[i]);
                if (zone != null)
                {
                    Level1ScenePlacement.SnapHierarchyFloorProps(zone, FloorTopY, RugFloorLift);
                }
            }

            SnapExitAreaFloorDecor(worldRoot);

            GameObject bed = Level1ScenePlacement.FindInRoomObject(worldRoot, "SleepingArea/Bed");
            if (bed != null)
            {
                Level1ScenePlacement.SnapPropBottomToFloor(bed, FloorTopY, FloorPropLift);
                SnapBedSurfaceProps(bed);
            }

            GameObject desk = Level1ScenePlacement.FindInRoomObject(worldRoot, "StudyArea/Desk");
            Transform packables = Level1ScenePlacement.FindInRoom(worldRoot, "PackableItems");
            if (desk != null)
            {
                SnapAllDeskProps(desk, packables);
            }

            GameObject shelf = Level1ScenePlacement.FindInRoomObject(worldRoot, "StorageArea/Shelf");
            if (shelf != null)
            {
                SnapAllShelfProps(shelf);
                RepositionShelfPackables(worldRoot, shelf);
            }

            GameObject wardrobe = Level1ScenePlacement.FindInRoomObject(worldRoot, "StorageArea/Wardrobe");
            if (wardrobe != null)
            {
                SnapWardrobeTopProps(wardrobe);
            }

            FinalizePackablePlacement(worldRoot, bed, desk, shelf);

            ResolveBedClearance(bed, worldRoot);
            Level1ScenePlacement.RemovePropsOverlappingBed(bed, worldRoot);
            Level1ScenePlacement.ResolveFurnitureOverlaps(worldRoot);
            Level1ScenePlacement.ClampRoomFurnitureInside(worldRoot, RoomHalfWidth, RoomHalfDepth, WallInset + 0.12f);

            ResnapRoomSurfaces(worldRoot, bed, desk, shelf);
            EnsureCornerPlantPresentation(worldRoot);
            MarkRoomStaticForBatching(worldRoot);
            OptimizeRoomLighting(worldRoot);
        }

        private static void SnapExitAreaFloorDecor(Transform worldRoot)
        {
            string[] exitDecorPaths =
            {
                "ExitArea/DoorFloorMat", "ExitArea/SlipperLeft", "ExitArea/SlipperRight", "ExitArea/Suitcase"
            };

            for (int i = 0; i < exitDecorPaths.Length; i++)
            {
                GameObject decor = Level1ScenePlacement.FindInRoomObject(worldRoot, exitDecorPaths[i]);
                if (decor == null)
                {
                    continue;
                }

                float lift = decor.name.IndexOf("rug", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || decor.name.IndexOf("mat", System.StringComparison.OrdinalIgnoreCase) >= 0
                    ? RugFloorLift
                    : FloorPropLift;
                Level1ScenePlacement.SnapPropBottomToFloor(decor, FloorTopY, lift);
            }
        }

        private static void SnapBedSurfaceProps(GameObject bed)
        {
            if (bed == null)
            {
                return;
            }

            for (int i = 0; i < bed.transform.childCount; i++)
            {
                GameObject child = bed.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Bed"))
                {
                    Level1ScenePlacement.SnapPropBottomToBedTop(bed, child);
                }
            }
        }

        private static void SnapAllShelfProps(GameObject shelf)
        {
            if (shelf == null)
            {
                return;
            }

            string[] tierNames =
            {
                "ShelfTier_Box", "ShelfTier_Snacks", "ShelfTier_Books", "ShelfTier_Cloth"
            };

            for (int i = 0; i < tierNames.Length; i++)
            {
                Transform tierProp = shelf.transform.Find(tierNames[i]);
                if (tierProp == null)
                {
                    continue;
                }

                float normalizedTier = tierNames[i] switch
                {
                    "ShelfTier_Box" => RoomLayout.ShelfTierBottom,
                    "ShelfTier_Snacks" => RoomLayout.ShelfTierLowerMid,
                    "ShelfTier_Books" => RoomLayout.ShelfTierUpperMid,
                    _ => RoomLayout.ShelfTierTop
                };
                Level1ScenePlacement.SnapPropToShelfTier(shelf, tierProp.gameObject, normalizedTier);
            }
        }

        private static void SnapWardrobeTopProps(GameObject wardrobe)
        {
            if (wardrobe == null)
            {
                return;
            }

            Transform topProp = wardrobe.transform.Find("WardrobeTopHatBox");
            if (topProp != null)
            {
                Level1ScenePlacement.SnapPropBottomToFurnitureTop(wardrobe, "Wardrobe", topProp.gameObject);
            }
        }

        private static void FinalizePackablePlacement(Transform worldRoot, GameObject bed, GameObject desk,
            GameObject shelf)
        {
            Transform packables = Level1ScenePlacement.FindInRoom(worldRoot, "PackableItems");
            if (packables == null)
            {
                packables = worldRoot.Find("PackableItems");
            }

            if (packables == null)
            {
                return;
            }

            RepositionBedPackables(worldRoot, bed);
            RepositionPackableOnFloor(packables, "Clothes", RoomLayout.ClothesPileCenter,
                new Vector3(0.28f, 0.22f, 0.14f));

            GameObject smartphone = packables.Find("Smartphone")?.gameObject;
            if (smartphone != null && bed != null)
            {
                Level1ScenePlacement.SnapPropBottomToBedTop(bed, smartphone);
            }

            if (desk != null)
            {
                RepositionDeskPackables(packables, desk, LoadMaterial("Gift_Red.mat"),
                    LoadMaterial("Coin_Gold.mat"), LoadMaterial("Glass_Blue.mat"));
            }

            if (shelf != null)
            {
                RepositionShelfPackables(worldRoot, shelf);
            }
        }

        private static void ResnapRoomSurfaces(Transform worldRoot, GameObject bed, GameObject desk, GameObject shelf)
        {
            string[] floorZones =
            {
                "SleepingArea", "StudyArea", "StorageArea", "ExitArea", "RoomPersonality"
            };

            for (int i = 0; i < floorZones.Length; i++)
            {
                Transform zone = Level1ScenePlacement.FindInRoom(worldRoot, floorZones[i]);
                if (zone != null)
                {
                    Level1ScenePlacement.SnapHierarchyFloorProps(zone, FloorTopY, RugFloorLift);
                }
            }

            SnapExitAreaFloorDecor(worldRoot);
            SnapBedSurfaceProps(bed);
            SnapAllDeskProps(desk, Level1ScenePlacement.FindInRoom(worldRoot, "PackableItems"));
            SnapAllShelfProps(shelf);

            GameObject wardrobe = Level1ScenePlacement.FindInRoomObject(worldRoot, "StorageArea/Wardrobe");
            SnapWardrobeTopProps(wardrobe);
            FinalizePackablePlacement(worldRoot, bed, desk, shelf);
        }

        private static void EnsureCornerPlantPresentation(Transform worldRoot)
        {
            Transform personality = Level1ScenePlacement.FindInRoom(worldRoot, "RoomPersonality");
            if (personality == null)
            {
                return;
            }

            Level1CornerPlantBuilder.Build(personality);
        }

        private static void MarkRoomStaticForBatching(Transform worldRoot)
        {
            StaticEditorFlags batchingFlags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic;

            string[] staticRoots =
            {
                "RentalRoomInterior", "SleepingArea", "StudyArea", "StorageArea", "ExitArea", "RoomPersonality"
            };

            for (int i = 0; i < staticRoots.Length; i++)
            {
                Transform root = Level1ScenePlacement.FindInRoom(worldRoot, staticRoots[i]);
                if (root != null)
                {
                    MarkStaticRecursive(root.gameObject, batchingFlags);
                }
            }
        }

        private static void MarkStaticRecursive(GameObject root, StaticEditorFlags flags)
        {
            if (root == null)
            {
                return;
            }

            GameObjectUtility.SetStaticEditorFlags(root, flags);

            Transform transform = root.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                MarkStaticRecursive(transform.GetChild(i).gameObject, flags);
            }
        }

        private static void OptimizeRoomLighting(Transform worldRoot)
        {
            Light[] lights = worldRoot.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light.type == LightType.Point)
                {
                    light.shadows = LightShadows.None;
                    light.renderMode = LightRenderMode.Auto;
                }
                else if (light.type == LightType.Directional)
                {
                    light.shadows = LightShadows.Soft;
                    light.shadowStrength = 0.22f;
                }
            }
        }

        /// <summary>Groups every indoor-only object so Phase 2 can hide the full room, not just the wall shell.</summary>
        private static void FinalizeIndoorAreaRoot(Transform worldRoot)
        {
            GameObject indoorArea = new GameObject("Level1_IndoorArea");
            indoorArea.transform.SetParent(worldRoot, false);

            string[] indoorChildNames =
            {
                "RentalRoomInterior", "SleepingArea", "StudyArea", "StorageArea", "ExitArea", "RoomPersonality",
                "PackableItems", "DepartureDoor", "PackingArea",
                "WindowSunFromAbove",
                "WindowMorningFill", "FloorSunPatch"
            };

            for (int i = 0; i < indoorChildNames.Length; i++)
            {
                Transform child = worldRoot.Find(indoorChildNames[i]);
                if (child != null)
                {
                    child.SetParent(indoorArea.transform, true);
                }
            }
        }

        private static void ConfigureInteriorEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.76f, 0.71f, 0.64f, 1f);
            RenderSettings.fog = false;
            RenderSettings.reflectionIntensity = 0.32f;
        }

        private static void ConfigureInteriorLighting(Transform parent)
        {
            GameObject windowLight = new GameObject("WindowSunlight");
            windowLight.transform.SetParent(parent, false);
            windowLight.transform.rotation = Quaternion.Euler(32f, 72f, 0f);
            Light directional = windowLight.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = 0.58f;
            directional.color = new Color(1f, 0.92f, 0.8f);
            directional.shadows = LightShadows.Soft;
            directional.shadowStrength = 0.22f;

            CreatePointLight(parent, "RoomWarmFill", new Vector3(0.15f, 2.45f, 0.05f),
                new Color(1f, 0.9f, 0.76f), 1.05f, 14f);
            CreatePointLight(parent, "BedsideWarm", RoomLayout.Bed + new Vector3(-0.15f, 1.65f, 0.25f),
                new Color(1f, 0.84f, 0.6f), 0.72f, 6.5f);
        }

        private static Vector3 GetStudyWindowCenter()
        {
            float leftWallX = -RoomHalfWidth + WallHalfThickness + 0.04f;
            return new Vector3(leftWallX + 0.18f, 1.55f, RoomLayout.Desk.z);
        }

        private static void BuildRoomShell(Transform root, Material floorMaterial, Material wallMaterial,
            Material wallAccent, Material woodDark, Material woodWarm, Material curtainMaterial, Material doorDark,
            Material frostedGlass)
        {
            GameObject room = new GameObject("RentalRoomInterior");
            room.transform.SetParent(root, false);

            LayWoodenFloor(room.transform, floorMaterial, wallAccent);
            BuildStructuralWalls(room.transform, wallMaterial, wallAccent);
            BuildDoorway(room.transform, woodDark, doorDark, woodWarm, wallMaterial);
            BuildStudyWindow(room.transform, woodDark, curtainMaterial, frostedGlass, wallMaterial);
            AddWallTrim(room.transform, wallAccent);
        }

        private static void LayWoodenFloor(Transform room, Material floorMaterial, Material plankMaterial)
        {
            GameObject floorRoot = new GameObject("WoodFloor");
            floorRoot.transform.SetParent(room, false);

            CreateCube("FloorSurface", floorRoot.transform, new Vector3(0f, FloorTopY - 0.015f, 0f),
                new Vector3(RoomHalfWidth * 2f - 0.12f, 0.03f, RoomHalfDepth * 2f - 0.12f), floorMaterial);

            int plankCount = 7;
            float plankSpacing = (RoomHalfDepth * 2f - 0.5f) / plankCount;
            float startZ = -RoomHalfDepth + 0.25f;
            for (int i = 0; i < plankCount; i++)
            {
                float z = startZ + i * plankSpacing;
                CreateCube("PlankSeam_" + i, floorRoot.transform, new Vector3(0f, FloorTopY + 0.002f, z),
                    new Vector3(RoomHalfWidth * 2f - 0.32f, 0.003f, 0.018f), plankMaterial);
            }

            GameObject collisionFloor = CreateCube("FloorCollider", room, new Vector3(0f, FloorTopY - 0.04f, 0f),
                new Vector3(RoomHalfWidth * 2f - 0.08f, 0.08f, RoomHalfDepth * 2f - 0.08f), floorMaterial);
            Object.DestroyImmediate(collisionFloor.GetComponent<MeshRenderer>());
        }

        private static void BuildStructuralWalls(Transform room, Material wallMaterial, Material wallAccent)
        {
            float wallThickness = 0.18f;

            CreateCube("Wall_Front", room, new Vector3(0f, WallHeight * 0.5f, RoomHalfDepth),
                new Vector3(RoomHalfWidth * 2f, WallHeight, wallThickness), wallMaterial);
            CreateCube("Wall_Left", room, new Vector3(-RoomHalfWidth, WallHeight * 0.5f, 0f),
                new Vector3(wallThickness, WallHeight, RoomHalfDepth * 2f), wallMaterial);
            CreateCube("Wall_Right", room, new Vector3(RoomHalfWidth, WallHeight * 0.5f, 0f),
                new Vector3(wallThickness, WallHeight, RoomHalfDepth * 2f), wallMaterial);

            float backSideWidth = RoomHalfWidth - DoorHalfWidth;
            float backSideCenterX = (RoomHalfWidth + DoorHalfWidth) * 0.5f;

            CreateCube("Wall_Back_Left", room, new Vector3(-backSideCenterX, WallHeight * 0.5f, -RoomHalfDepth),
                new Vector3(backSideWidth, WallHeight, wallThickness), wallMaterial);
            CreateCube("Wall_Back_Right", room, new Vector3(backSideCenterX, WallHeight * 0.5f, -RoomHalfDepth),
                new Vector3(backSideWidth, WallHeight, wallThickness), wallMaterial);
            CreateCube("Wall_Back_Top", room, new Vector3(0f, DoorOpeningHeight + (WallHeight - DoorOpeningHeight) * 0.5f,
                    -RoomHalfDepth),
                new Vector3(DoorHalfWidth * 2f + 0.15f, WallHeight - DoorOpeningHeight, wallThickness), wallMaterial);

            CreateCube("Wainscot_Back", room, new Vector3(0f, 0.28f, -RoomHalfDepth + 0.1f),
                new Vector3(RoomHalfWidth * 2f - 0.3f, 0.12f, 0.04f), wallAccent);
            CreateCube("Wainscot_Left", room, new Vector3(-RoomHalfWidth + 0.1f, 0.28f, 0f),
                new Vector3(0.04f, 0.12f, RoomHalfDepth * 2f - 0.3f), wallAccent);
            BuildRoomCeiling(room.transform, wallMaterial);
        }

        /// <summary>Flat ceiling slab — closes the room volume for first-person views.</summary>
        public static void BuildRoomCeiling(Transform room, Material ceilingMaterial)
        {
            if (room == null)
            {
                return;
            }

            Transform existing = room.Find("RoomCeiling");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            float inset = 0.08f;
            float thickness = 0.06f;
            float ceilingY = WallHeight - thickness * 0.5f - 0.01f;
            CreateCube("RoomCeiling", room, new Vector3(0f, ceilingY, 0f),
                new Vector3(RoomHalfWidth * 2f - inset, thickness, RoomHalfDepth * 2f - inset),
                ceilingMaterial);
        }

        private static void BuildDoorway(Transform room, Material woodDark, Material doorDark, Material woodWarm,
            Material wallMaterial)
        {
            float wallThickness = 0.18f;
            float doorZ = -RoomHalfDepth + WallHalfThickness + 0.04f;
            float frameHalfWidth = DoorHalfWidth + 0.12f;
            float frameCenterY = DoorOpeningHeight * 0.5f;
            float doorHeight = DoorOpeningHeight - 0.04f;

            CreateCube("DoorOpening_Fill", room,
                new Vector3(0f, DoorOpeningHeight * 0.5f, -RoomHalfDepth + wallThickness * 0.45f),
                new Vector3(DoorHalfWidth * 2f - 0.02f, DoorOpeningHeight, wallThickness * 0.9f), wallMaterial);

            CreateCube("DoorFrame_Left", room, new Vector3(-frameHalfWidth, frameCenterY, doorZ),
                new Vector3(0.1f, DoorOpeningHeight, 0.08f), woodDark);
            CreateCube("DoorFrame_Right", room, new Vector3(frameHalfWidth, frameCenterY, doorZ),
                new Vector3(0.1f, DoorOpeningHeight, 0.08f), woodDark);
            CreateCube("DoorFrame_Top", room, new Vector3(0f, DoorOpeningHeight + 0.05f, doorZ),
                new Vector3(frameHalfWidth * 2f + 0.04f, 0.1f, 0.08f), woodDark);

            CreateCube("RoomDoor", room, new Vector3(0f, doorHeight * 0.5f + 0.02f, doorZ + 0.02f),
                new Vector3(DoorHalfWidth * 2f - 0.08f, doorHeight, 0.06f), woodWarm);
            CreateCube("Door_Handle", room, new Vector3(DoorHalfWidth * 0.55f, doorHeight * 0.48f, doorZ + 0.05f),
                new Vector3(0.05f, 0.05f, 0.05f), woodDark);
            CreateCube("Door_Panel_Inset", room, new Vector3(0f, doorHeight * 0.52f, doorZ + 0.045f),
                new Vector3(DoorHalfWidth * 1.4f, doorHeight * 0.74f, 0.015f), doorDark);
        }

        private static void BuildStudyWindow(Transform room, Material woodDark, Material curtainMaterial,
            Material frostedGlass, Material wallMaterial)
        {
            float windowHalfWidth = 0.62f;
            float windowHalfHeight = 0.52f;
            float windowCenterZ = RoomLayout.Desk.z;
            float windowX = -RoomHalfWidth + WallHalfThickness + 0.04f;
            float glassInsetX = windowX + 0.045f;

            GameObject windowRoot = new GameObject("StudyWindow");
            windowRoot.transform.SetParent(room, false);

            CreateDecorCube("Window_Frame_Left", windowRoot.transform,
                new Vector3(windowX, 1.55f, windowCenterZ - windowHalfWidth),
                new Vector3(0.06f, windowHalfHeight * 2f + 0.08f, 0.07f), woodDark);
            CreateDecorCube("Window_Frame_Right", windowRoot.transform,
                new Vector3(windowX, 1.55f, windowCenterZ + windowHalfWidth),
                new Vector3(0.06f, windowHalfHeight * 2f + 0.08f, 0.07f), woodDark);
            CreateDecorCube("Window_Frame_Top", windowRoot.transform,
                new Vector3(windowX, 1.55f + windowHalfHeight + 0.04f, windowCenterZ),
                new Vector3(0.06f, 0.07f, windowHalfWidth * 2f + 0.06f), woodDark);
            CreateDecorCube("Window_Frame_Bottom", windowRoot.transform,
                new Vector3(windowX, 1.55f - windowHalfHeight - 0.04f, windowCenterZ),
                new Vector3(0.06f, 0.07f, windowHalfWidth * 2f + 0.06f), woodDark);

            CreateDecorCube("Window_FrostedGlass", windowRoot.transform,
                new Vector3(glassInsetX, 1.55f, windowCenterZ),
                new Vector3(0.025f, windowHalfHeight * 2f, windowHalfWidth * 2f - 0.04f), frostedGlass);

            CreateDecorCube("Window_Curtain_Left", windowRoot.transform,
                new Vector3(glassInsetX + 0.01f, 1.55f, windowCenterZ - windowHalfWidth * 0.58f),
                new Vector3(0.018f, windowHalfHeight * 1.75f, 0.22f), curtainMaterial);
            CreateDecorCube("Window_Curtain_Right", windowRoot.transform,
                new Vector3(glassInsetX + 0.01f, 1.55f, windowCenterZ + windowHalfWidth * 0.58f),
                new Vector3(0.018f, windowHalfHeight * 1.75f, 0.22f), curtainMaterial);
            CreateDecorCube("Window_Curtain_Rod", windowRoot.transform,
                new Vector3(glassInsetX + 0.02f, 1.55f + windowHalfHeight + 0.02f, windowCenterZ),
                new Vector3(0.03f, 0.03f, windowHalfWidth * 2f + 0.08f), woodDark);

            CreateDecorCube("Window_Sill", windowRoot.transform,
                new Vector3(glassInsetX + 0.05f, 0.82f, windowCenterZ),
                new Vector3(0.14f, 0.05f, windowHalfWidth * 2f + 0.06f), woodDark);
        }

        private static void AddWallTrim(Transform room, Material wallAccent)
        {
            CreateCube("Baseboard_Back", room, new Vector3(0f, 0.1f, -RoomHalfDepth + 0.1f),
                new Vector3(RoomHalfWidth * 2f - 0.35f, 0.06f, 0.04f), wallAccent);
            CreateCube("Baseboard_Left", room, new Vector3(-RoomHalfWidth + 0.1f, 0.1f, 0f),
                new Vector3(0.04f, 0.06f, RoomHalfDepth * 2f - 0.35f), wallAccent);
            CreateCube("Baseboard_Right", room, new Vector3(RoomHalfWidth - 0.1f, 0.1f, 0f),
                new Vector3(0.04f, 0.06f, RoomHalfDepth * 2f - 0.35f), wallAccent);
        }

        private static void BuildSleepingArea(Transform root, Material motherCloth, Material houseCream,
            Material woodDark, Material woodWarm, Material fabricSageSeat, Material fabricSageBack)
        {
            Transform existing = root.Find("SleepingArea");
            GameObject zone = existing != null ? existing.gameObject : new GameObject("SleepingArea");
            if (existing == null)
            {
                zone.transform.SetParent(root, false);
            }

            PopulateSleepingArea(zone.transform, motherCloth, houseCream, woodDark, woodWarm, fabricSageSeat,
                fabricSageBack);
        }

        private static void PopulateSleepingArea(Transform zone, Material motherCloth, Material houseCream,
            Material woodDark, Material woodWarm, Material fabricSageSeat, Material fabricSageBack)
        {
            ClearSleepingAreaFurniture(zone);

            GameObject bed = PlaceKenneyFurniture("bedSingle.fbx", zone, RoomLayout.Bed,
                Quaternion.Euler(0f, RoomLayout.BedYaw, 0f), 1f, "Bed", null, KenneyBedScale);
            if (bed == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] bedSingle.fbx missing — spawning fallback bed.");
                bed = CreateFallbackBed(zone, fabricSageSeat, woodWarm);
            }

            if (bed == null)
            {
                Debug.LogError("[Level1SceneBuilder] Could not create bed.");
                return;
            }

            ApplyKenneyUpholsteredMaterials(bed, fabricSageSeat, fabricSageBack, woodWarm, woodDark, houseCream);

            PlaceKenneyProp("rugRectangle.fbx", zone.transform, RoomLayout.BedRug,
                Quaternion.Euler(0f, RoomLayout.BedYaw, 0f), 0.88f, "BedsideRug", motherCloth);
            Level1ScenePlacement.SnapPropBottomToFloor(
                zone.transform.Find("BedsideRug")?.gameObject, FloorTopY, RugFloorLift);

            PlaceKenneyPropOnSurface(bed, "pillowBlueLong.fbx", RoomLayout.BedPillowLocal,
                Quaternion.identity, 1f, "BedPillow", motherCloth);

            GameObject lamp = PlaceKenneyProp("lampSquareTable.fbx", zone,
                RoomLayout.Bed + new Vector3(-0.48f, 0f, -0.38f), Quaternion.identity, 0.85f, "BedsideLamp", woodDark);
            Level1ScenePlacement.SnapPropBottomToFloor(lamp, FloorTopY);
        }

        private static GameObject CreateFallbackBed(Transform parent, Material fabric, Material frame)
        {
            GameObject bed = new GameObject("Bed");
            bed.transform.SetParent(parent, false);
            bed.transform.position = RoomLayout.Bed;
            bed.transform.rotation = Quaternion.Euler(0f, RoomLayout.BedYaw, 0f);

            CreateCube("Bed_Frame", bed.transform, new Vector3(0f, 0.12f, 0f),
                new Vector3(0.95f, 0.24f, 1.85f), frame != null ? frame : fabric);
            CreateCube("Bed_Mattress", bed.transform, new Vector3(0f, 0.28f, 0f),
                new Vector3(0.88f, 0.12f, 1.75f), fabric);
            FitFurnitureColliderFromRenderers(bed);
            Level1ScenePlacement.SnapPropBottomToFloor(bed, FloorTopY);
            return bed;
        }

        private static void BuildStudyArea(Transform root, Material woodWarm, Material woodDark, Material darkBrown,
            Material fabricSageSeat, Material fabricSageBack)
        {
            GameObject zone = new GameObject("StudyArea");
            zone.transform.SetParent(root, false);

            GameObject desk = PlaceKenneyFurniture("desk.fbx", zone.transform, RoomLayout.Desk,
                Quaternion.Euler(0f, RoomLayout.DeskYaw, 0f), 1f, "Desk", woodWarm, KenneyDeskChairScale);

            GameObject chair = PlaceKenneyFurniture("chairDesk.fbx", zone.transform, RoomLayout.Chair,
                Quaternion.Euler(0f, RoomLayout.ChairYaw, 0f), 1f, "DeskChair", null, KenneyDeskChairScale);
            ApplyKenneyUpholsteredMaterials(chair, fabricSageSeat, fabricSageBack, woodWarm, woodDark);

            PopulateStudyDeskProps(desk, woodDark);

            GameObject studyBackpack = PlaceMarketProp(Level1RoomAssetPaths.Market + "/bag.prefab", zone.transform,
                RoomLayout.StudyChairBackpack, Quaternion.Euler(0f, 18f, 6f), Vector3.one * 0.82f, "StudyChairBackpack");
            Level1ScenePlacement.SnapPropBottomToFloor(studyBackpack, FloorTopY);
        }

        private static void ResolveDeskPropLayout(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            GameObject desk = Level1ScenePlacement.FindInRoomObject(worldRoot, "StudyArea/Desk");
            if (desk == null)
            {
                return;
            }

            Transform packables = Level1ScenePlacement.FindInRoom(worldRoot, "PackableItems");
            Level1ScenePlacement.SnapAllDeskSurfaceProps(desk, packables);
        }

        private static void PlaceDecorOnDesk(GameObject desk, Vector3 localOffset, Vector3 scale, Material material,
            string name)
        {
            if (desk == null || material == null)
            {
                return;
            }

            GameObject decor = CreateDecorCube(name, desk.transform, Vector3.zero, scale, material);
            decor.transform.localPosition = new Vector3(localOffset.x, 0.2f, localOffset.z);
            decor.transform.localRotation = Quaternion.Euler(0f, RoomLayout.DeskYaw * 0.5f, 0f);
            Level1ScenePlacement.SnapPropBottomToSurface(desk, decor);
        }

        private static void BuildStorageArea(Transform root, Material woodWarm, Material woodDark, Material darkBrown,
            Material motherCloth, Material houseCream)
        {
            GameObject zone = new GameObject("StorageArea");
            zone.transform.SetParent(root, false);

            GameObject wardrobe = PlaceKenneyFurniture("bookcaseClosed.fbx", zone.transform, RoomLayout.Wardrobe,
                Quaternion.Euler(0f, RoomLayout.WardrobeYaw, 0f), 1f, "Wardrobe", woodDark);
            PopulateWardrobeTopAccent(wardrobe, houseCream);

            GameObject shelf = PlaceKenneyFurniture("bookcaseOpen.fbx", zone.transform, RoomLayout.Shelf,
                Quaternion.Euler(0f, RoomLayout.ShelfYaw, 0f), 0.95f, "Shelf", woodWarm);
            PopulateOpenShelfTiers(shelf, motherCloth, houseCream, darkBrown);
        }

        private static void BuildExitArea(Transform root, Material houseCream, Material produceOrange,
            Material motherCloth, Material accentRed, Material woodWarm)
        {
            GameObject zone = new GameObject("ExitArea");
            zone.transform.SetParent(root, false);

            PlaceKenneyProp("rugDoormat.fbx", zone.transform,
                new Vector3(0f, FloorTopY, InnerMinZ + 0.55f), Quaternion.identity, 1f, "DoorFloorMat", motherCloth);
            Level1ScenePlacement.SnapPropBottomToFloor(
                zone.transform.Find("DoorFloorMat")?.gameObject, FloorTopY);

            CreateDecorSlipper("SlipperLeft", zone.transform, RoomLayout.SlippersLeft, motherCloth);
            CreateDecorSlipper("SlipperRight", zone.transform, RoomLayout.SlippersRight, accentRed);

            GameObject suitcase = PlaceKenneyFurniture("cardboardBoxClosed.fbx", zone.transform, RoomLayout.Suitcase,
                Quaternion.Euler(0f, 8f, 0f), 1f, "Suitcase", houseCream);
            Level1ScenePlacement.SnapPropBottomToFloor(suitcase, FloorTopY);
        }

        private static void CreateDecorSlipper(string name, Transform parent, Vector3 center, Material material)
        {
            GameObject slipper = CreateDecorCube(name, parent, center, new Vector3(0.16f, 0.04f, 0.24f), material);
            Level1ScenePlacement.SnapPropBottomToFloor(slipper, FloorTopY);
        }

        private static GameObject CreateFloorPropCube(string name, Transform parent, Vector3 centerOnFloor,
            Vector3 scale, Material material)
        {
            Vector3 center = centerOnFloor + new Vector3(0f, scale.y * 0.5f, 0f);
            return CreateCube(name, parent, center, scale, material);
        }

        private static void BuildRoomPersonality(Transform root, Material giftRed, Material flowerYellow,
            Material accentRed, Material houseCream, Material woodDark, Material motherCloth, Material foliageGreen)
        {
            GameObject zone = new GameObject("RoomPersonality");
            zone.transform.SetParent(root, false);

            float leftWallX = -RoomHalfWidth + WallHalfThickness + 0.04f;
            float frontWallZ = RoomHalfDepth - WallHalfThickness - 0.04f;

            BuildTetTravelCalendar(zone.transform, leftWallX, giftRed, accentRed, flowerYellow, houseCream);
            PlaceIndoorPlant(zone.transform, foliageGreen, accentRed, woodDark);
            BuildCozyWallAccent(zone.transform, frontWallZ, giftRed, flowerYellow);
        }

        private static void BuildTetTravelCalendar(Transform parent, float wallX, Material giftRed, Material accentRed,
            Material flowerYellow, Material houseCream)
        {
            float calendarY = 2.18f;
            float calendarZ = RoomLayout.Desk.z + 0.05f;
            float insetX = wallX + 0.035f;

            GameObject calendar = new GameObject("TetTravelCalendar");
            calendar.transform.SetParent(parent, false);

            CreateDecorCube("Calendar_Frame", calendar.transform,
                new Vector3(insetX, calendarY, calendarZ), new Vector3(0.03f, 0.34f, 0.26f), accentRed);
            CreateDecorCube("Calendar_Paper", calendar.transform,
                new Vector3(insetX + 0.012f, calendarY, calendarZ), new Vector3(0.018f, 0.28f, 0.22f), houseCream);
            CreateDecorCube("Calendar_Header", calendar.transform,
                new Vector3(insetX + 0.014f, calendarY + 0.11f, calendarZ), new Vector3(0.014f, 0.05f, 0.18f),
                giftRed);
            CreateDecorCube("Calendar_GoldBand", calendar.transform,
                new Vector3(insetX + 0.014f, calendarY + 0.06f, calendarZ), new Vector3(0.014f, 0.025f, 0.18f),
                flowerYellow);

            for (int i = 0; i < 5; i++)
            {
                float dayX = insetX + 0.014f;
                float dayY = calendarY - 0.02f - i * 0.045f;
                Material dayMat = i == 0 ? giftRed : houseCream;
                CreateDecorCube("Calendar_Day_" + i, calendar.transform,
                    new Vector3(dayX, dayY, calendarZ - 0.04f + i * 0.02f), new Vector3(0.012f, 0.028f, 0.028f),
                    dayMat);
            }

            CreateDecorCube("Calendar_TravelNote", calendar.transform,
                new Vector3(insetX + 0.014f, calendarY - 0.13f, calendarZ + 0.06f),
                new Vector3(0.012f, 0.04f, 0.08f), accentRed);
        }

        private static void PlaceIndoorPlant(Transform parent, Material foliageGreen, Material potMaterial,
            Material woodDark)
        {
            GameObject plantRoot = new GameObject("CornerPlant");
            plantRoot.transform.SetParent(parent, false);
            plantRoot.transform.position = RoomLayout.CornerPlant;
            plantRoot.transform.rotation = Quaternion.Euler(0f, RoomLayout.CornerPlantYaw, 0f);

            Material pot = potMaterial != null ? potMaterial : woodDark;
            CreateDecorCube("PlantPot", plantRoot.transform, new Vector3(0f, 0.11f, 0f),
                new Vector3(0.34f, 0.22f, 0.34f), pot);

            GameObject foliage = PlaceKenneyProp("plantSmall1.fbx", plantRoot.transform,
                new Vector3(0f, 0.3f, 0f), Quaternion.identity, 1.15f, "PlantFoliage", foliageGreen);
            if (foliage == null)
            {
                foliage = PlaceKenneyProp("pottedPlant.fbx", plantRoot.transform, Vector3.zero,
                    Quaternion.identity, 0.9f, "PlantFoliage", null);
                if (foliage != null)
                {
                    TintIndoorPlant(plantRoot, foliageGreen, pot);
                }
            }

            if (foliage == null)
            {
                CreateDecorCube("PlantFoliage", plantRoot.transform, new Vector3(0f, 0.34f, 0f),
                    new Vector3(0.22f, 0.28f, 0.22f), foliageGreen);
            }

            Level1ScenePlacement.SnapPropBottomToFloor(plantRoot, FloorTopY, FloorPropLift);
        }

        private static void BuildCozyWallAccent(Transform parent, float wallZ, Material giftRed,
            Material flowerYellow)
        {
            float insetZ = wallZ > 0f ? -0.03f : 0.03f;
            CreateDecorCube("TetBanner", parent, new Vector3(0f, 2.48f, wallZ + insetZ),
                new Vector3(1.85f, 0.09f, 0.03f), giftRed);
            CreateDecorCube("TetBannerGold", parent, new Vector3(0f, 2.38f, wallZ + insetZ),
                new Vector3(1.55f, 0.035f, 0.03f), flowerYellow);
            CreateDecorCube("TetLanternHint", parent, new Vector3(0.55f, 2.22f, wallZ + insetZ),
                new Vector3(0.07f, 0.07f, 0.03f), giftRed);
        }

        private static void TintIndoorPlant(GameObject root, Material foliageGreen, Material potMaterial)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (int i = 0; i < renderers.Length; i++)
            {
                minY = Mathf.Min(minY, renderers[i].bounds.min.y);
                maxY = Mathf.Max(maxY, renderers[i].bounds.max.y);
            }

            float splitY = minY + (maxY - minY) * 0.34f;
            for (int i = 0; i < renderers.Length; i++)
            {
                string name = renderers[i].gameObject.name.ToLowerInvariant();
                bool isPot = IsPlantPotName(name);
                bool isFoliage = IsPlantFoliageName(name);
                bool isLower = renderers[i].bounds.center.y <= splitY;
                Material mat;
                if (isFoliage)
                {
                    mat = foliageGreen;
                }
                else if (isPot || isLower)
                {
                    mat = potMaterial;
                }
                else
                {
                    mat = foliageGreen;
                }

                if (mat != null)
                {
                    renderers[i].sharedMaterial = mat;
                }
            }
        }

        private static bool IsPlantPotName(string name)
        {
            return name.Contains("pot") || name.Contains("vase") || name.Contains("bucket")
                || name.Contains("base") || name.Contains("soil") || name.Contains("planter");
        }

        private static bool IsPlantFoliageName(string name)
        {
            return name.Contains("foliage") || name.Contains("leaf") || name.Contains("bush")
                || name.Contains("grass") || (name.Contains("plant") && !name.Contains("pot"));
        }

        private static GameObject CreateDecorCube(string name, Transform parent, Vector3 position, Vector3 scale,
            Material material)
        {
            GameObject cube = CreateCube(name, parent, position, scale, material);
            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return cube;
        }

        private static Material CreateFrostedWindowGlass(Material glassBlue, Material houseCream)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            Color frost = glassBlue != null
                ? glassBlue.GetColor("_BaseColor")
                : new Color(0.72f, 0.82f, 0.92f, 1f);
            frost = Color.Lerp(frost, houseCream != null ? houseCream.GetColor("_BaseColor") : Color.white, 0.72f);
            frost.r *= 0.82f;
            frost.g *= 0.86f;
            frost.b *= 0.9f;
            frost.a = 1f;
            material.SetColor("_BaseColor", frost);
            material.SetFloat("_Smoothness", 0.42f);
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.05f);
            }

            return material;
        }

        private static void CreatePackableItems(Transform root, Material coinGold, Material glassBlue,
            Material producePurple, Material produceOrange, Material giftRed, Material houseCream)
        {
            GameObject itemsRoot = new GameObject("PackableItems");
            itemsRoot.transform.SetParent(root, false);

            GameObject bed = root.Find("SleepingArea/Bed")?.gameObject;
            CreatePackableOnBed(itemsRoot.transform, Level1PackingItemId.Smartphone, bed,
                new Vector3(0.18f, 0f, -0.22f), new Vector3(0.1f, 0.02f, 0.16f), glassBlue, "Smartphone");

            GameObject desk = root.Find("StudyArea/Desk")?.gameObject;
            CreatePackableOnDesk(itemsRoot.transform, Level1PackingItemId.Wallet, desk, RoomLayout.WalletDeskU,
                RoomLayout.WalletDeskV, new Vector3(0.22f, 0.04f, 0.16f), coinGold, "Wallet");

            CreatePackableOnFloor(itemsRoot.transform, Level1PackingItemId.Clothes, RoomLayout.ClothesPileCenter,
                new Vector3(0.28f, 0.22f, 0.14f), producePurple, "Clothes");

            CreatePackableOnDesk(itemsRoot.transform, Level1PackingItemId.WaterBottle, desk, RoomLayout.WaterBottleDeskU,
                RoomLayout.WaterBottleDeskV, new Vector3(0.12f, 0.22f, 0.12f), glassBlue, "WaterBottle");

            CreatePackableOnDesk(itemsRoot.transform, Level1PackingItemId.PersonalDocuments, desk,
                RoomLayout.IdCardDeskU, RoomLayout.IdCardDeskV,
                new Vector3(0.28f, 0.045f, 0.2f), giftRed, "Documents");

            GameObject shelf = root.Find("StorageArea/Shelf")?.gameObject;
            CreatePackableOnShelfTier(itemsRoot.transform, Level1PackingItemId.Snacks, shelf,
                RoomLayout.ShelfSnacksTier,
                new Vector3(0.18f, 0.1f, 0.14f), produceOrange, "Snacks", -0.32f);
        }

        private static void CreatePackableOnSurface(Transform parent, Level1PackingItemId itemId, GameObject surface,
            Vector3 localOffset, Vector3 visualScale, Material material, string objectName)
        {
            Vector3 position = surface != null
                ? GetSurfaceWorldPoint(surface, localOffset)
                : RoomLayout.PlayerSpawn + Vector3.up * 0.5f;
            GameObject item = CreatePackableItem(parent, itemId, position, visualScale, material, objectName);
            if (surface != null && item != null)
            {
                if (surface.name == "Desk")
                {
                    Level1ScenePlacement.SnapPropBottomToDeskTop(surface, item);
                }
                else if (surface.name == "Bed")
                {
                    Level1ScenePlacement.SnapPropBottomToBedTop(surface, item);
                }
                else
                {
                    Level1ScenePlacement.SnapPropBottomToSurface(surface, item);
                }
            }
        }

        private static void CreatePackableOnBed(Transform parent, Level1PackingItemId itemId, GameObject bed,
            Vector3 worldOffset, Vector3 visualScale, Material material, string objectName)
        {
            Vector3 position = GetBedPackableWorldPoint(bed, worldOffset);
            position.y += visualScale.y * 0.5f;
            GameObject item = CreatePackableItem(parent, itemId, position, visualScale, material, objectName);
            if (bed != null && item != null)
            {
                Level1ScenePlacement.SnapPropBottomToBedTop(bed, item);
            }
        }

        private static Vector3 GetBedPackableWorldPoint(GameObject bed, Vector3 worldOffset)
        {
            if (bed != null && Level1ScenePlacement.TryGetWorldBounds(bed, out Bounds bounds))
            {
                return new Vector3(
                    bounds.center.x + worldOffset.x,
                    bounds.max.y,
                    bounds.center.z + worldOffset.z);
            }

            return GetSurfaceWorldPoint(bed, RoomLayout.BedPhoneLocal) + worldOffset;
        }

        private static void CreatePackableOnFloor(Transform parent, Level1PackingItemId itemId, Vector3 floorCenter,
            Vector3 visualScale, Material material, string objectName)
        {
            Vector3 position = floorCenter + new Vector3(0f, visualScale.y * 0.5f, 0f);
            GameObject item = CreatePackableItem(parent, itemId, position, visualScale, material, objectName);
            Level1ScenePlacement.SnapPropBottomToFloor(item, FloorTopY);
        }

        private static void CreatePackableOnShelfTier(Transform parent, Level1PackingItemId itemId, GameObject shelf,
            float normalizedTier, Vector3 visualScale, Material material, string objectName, float localXOffset = 0f)
        {
            Vector3 localPoint = Vector3.zero;
            if (shelf != null)
            {
                if (!Level1ScenePlacement.TryGetShelfBoardLocalPoint(shelf, "Shelf", out localPoint))
                {
                    localPoint = RoomLayout.ShelfSnacksLocal;
                }

                localPoint.x += localXOffset;
            }

            Vector3 position = shelf != null
                ? GetShelfTierWorldPoint(shelf, localPoint, normalizedTier)
                : RoomLayout.PlayerSpawn + Vector3.up * 0.5f;
            position.y += visualScale.y * 0.5f;

            GameObject item = CreatePackableItem(parent, itemId, position, visualScale, material, objectName);
            if (shelf != null && item != null)
            {
                Level1ScenePlacement.SnapPropBottomToWorldY(item,
                    Level1ScenePlacement.GetShelfTierWorldY(shelf, normalizedTier));
            }
        }

        private static void RepositionShelfPackables(Transform worldRoot, GameObject shelf)
        {
            if (worldRoot == null || shelf == null)
            {
                return;
            }

            Transform packables = Level1ScenePlacement.FindInRoom(worldRoot, "PackableItems");
            if (packables == null)
            {
                return;
            }

            if (!Level1ScenePlacement.TryGetShelfBoardLocalPoint(shelf, "Shelf", out Vector3 boardLocal))
            {
                boardLocal = RoomLayout.ShelfSnacksLocal;
            }

            RepositionPackableOnShelfTier(packables, "Snacks", shelf,
                boardLocal + new Vector3(-0.32f, 0f, 0f), RoomLayout.ShelfSnacksTier,
                new Vector3(0.18f, 0.1f, 0.14f));
        }

        private static void RepositionDeskPackables(Transform packables, GameObject desk, Material giftRed,
            Material coinGold, Material glassBlue)
        {
            RepositionPackableOnDesk(packables, desk, "Documents", RoomLayout.IdCardDeskU, RoomLayout.IdCardDeskV,
                new Vector3(0.28f, 0.045f, 0.2f), giftRed);
            RepositionPackableOnDesk(packables, desk, "Wallet", RoomLayout.WalletDeskU, RoomLayout.WalletDeskV,
                new Vector3(0.22f, 0.04f, 0.16f), coinGold);
            RepositionPackableOnDesk(packables, desk, "WaterBottle", RoomLayout.WaterBottleDeskU,
                RoomLayout.WaterBottleDeskV, new Vector3(0.12f, 0.22f, 0.12f), glassBlue);
        }

        private static void RepositionPackableOnDesk(Transform packables, GameObject desk, string objectName, float u,
            float v, Vector3 visualScale, Material material)
        {
            if (packables == null || desk == null)
            {
                return;
            }

            Transform item = packables.Find(objectName);
            if (item == null)
            {
                return;
            }

            Vector3 position = GetDeskSurfaceWorldPoint(desk, u, v);
            position.y += visualScale.y * 0.5f;
            item.position = position;

            Transform visual = item.Find("Visual");
            if (visual != null)
            {
                visual.localScale = visualScale;
                if (material != null)
                {
                    Renderer renderer = visual.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = material;
                    }
                }
            }

            Level1ScenePlacement.SnapPropBottomToDeskTop(desk, item.gameObject);
        }

        private static void RepositionDocumentsOnDesk(Transform packables, GameObject desk, Material giftRed)
        {
            RepositionPackableOnDesk(packables, desk, "Documents", RoomLayout.IdCardDeskU, RoomLayout.IdCardDeskV,
                new Vector3(0.28f, 0.045f, 0.2f), giftRed);
        }

        private static void RepositionPackableOnShelfTier(Transform packables, string objectName, GameObject shelf,
            Vector3 boardLocal, float normalizedTier, Vector3 visualScale)
        {
            Transform item = packables.Find(objectName);
            if (item == null)
            {
                return;
            }

            Vector3 position = GetShelfTierWorldPoint(shelf, boardLocal, normalizedTier);
            position.y += visualScale.y * 0.5f;
            item.position = position;

            Transform visual = item.Find("Visual");
            if (visual != null)
            {
                visual.localScale = visualScale;
            }

            Level1ScenePlacement.SnapPropBottomToWorldY(item.gameObject,
                Level1ScenePlacement.GetShelfTierWorldY(shelf, normalizedTier));
        }

        private static GameObject CreatePackableItem(Transform parent, Level1PackingItemId itemId, Vector3 position,
            Vector3 visualScale, Material material, string objectName)
        {
            GameObject itemRoot = new GameObject(objectName);
            itemRoot.transform.SetParent(parent, false);
            itemRoot.transform.position = position;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(itemRoot.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = visualScale;
            if (material != null)
            {
                visual.GetComponent<Renderer>().sharedMaterial = material;
            }

            Collider primitiveCollider = visual.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            SphereCollider collider = itemRoot.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.42f;
            collider.center = Vector3.zero;

            CollectableItem collectable = itemRoot.AddComponent<CollectableItem>();
            collectable.Configure(itemId, visual);
            return itemRoot;
        }

        private static DoorInteraction CreateDepartureDoor(Transform root)
        {
            float doorZ = -RoomHalfDepth + WallHalfThickness + 0.12f;

            GameObject doorTrigger = new GameObject("DepartureDoor");
            doorTrigger.transform.SetParent(root, false);
            doorTrigger.transform.position = new Vector3(0f, 1f, doorZ);

            BoxCollider collider = doorTrigger.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(DoorHalfWidth * 2f - 0.12f, 2f, 0.35f);

            DoorInteraction door = doorTrigger.AddComponent<DoorInteraction>();
            door.SetLocked(true);
            return door;
        }

        private static PlayerTopDownController CreatePlayer(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = spawnPosition;
            playerObject.transform.rotation = spawnRotation;

            CharacterController characterController = playerObject.AddComponent<CharacterController>();
            characterController.height = CharacterHeight;
            characterController.radius = CharacterRadius;
            characterController.center = new Vector3(0f, CharacterHeight * 0.5f, 0f);
            characterController.skinWidth = 0.04f;
            characterController.stepOffset = 0.15f;

            PlayerTopDownController controller = playerObject.AddComponent<PlayerTopDownController>();
            controller.fallbackAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CreativeCharactersPlayerSetup.ResolvePlayerAnimatorControllerPath());
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("characterHeight").floatValue = CharacterHeight;
            serializedController.FindProperty("characterRadius").floatValue = CharacterRadius;
            serializedController.FindProperty("modelScale").floatValue = CharacterModelScale;
            serializedController.FindProperty("feetGroundOffset").floatValue = 0.02f;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(playerObject.transform, false);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one * CharacterModelScale;
            controller.visualRoot = visualRoot.transform;

            InstantiatePrefab(
                CreativeCharactersPlayerSetup.ResolvePlayerVisualPrefabPath(),
                visualRoot.transform,
                Vector3.zero,
                Quaternion.Euler(0f, PlayerCharacterVisualPaths.CreativeCharacterModelYaw, 0f),
                Vector3.one,
                CreativeCharactersPlayerSetup.ResolveVisualChildName());

            GameObject origin = new GameObject("InteractionOrigin");
            origin.transform.SetParent(playerObject.transform, false);
            origin.transform.localPosition = new Vector3(0f, CharacterHeight * 0.58f, 0f);
            controller.interactionOrigin = origin.transform;

            return controller;
        }

        private static void CreateCamera(Transform parent, Transform followTarget)
        {
            GameObject cameraRig = new GameObject("IndoorCameraRig");
            cameraRig.transform.SetParent(parent, false);
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(cameraRig.transform, false);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Level1IndoorSky.SkyColor;
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 48f;
            cameraObject.AddComponent<AudioListener>();

            FirstPersonCameraController firstPerson = cameraObject.AddComponent<FirstPersonCameraController>();
            firstPerson.Bind(followTarget);
            firstPerson.EnsurePrimary();

            IndoorHighAngleCamera indoorCamera = cameraObject.AddComponent<IndoorHighAngleCamera>();
            indoorCamera.enabled = false;
            indoorCamera.Configure(RoomLayout.CameraLookTarget, 6.15f, 2.72f, 0f, 0.48f);
            indoorCamera.SetFollowTarget(followTarget);
            indoorCamera.SetRoomBounds(RoomHalfWidth, RoomHalfDepth, 0.45f);

            TopDownCameraOcclusion occlusion = cameraObject.AddComponent<TopDownCameraOcclusion>();
            occlusion.SetActive(false);
            BoardingHouseStreetOrbitCamera orbit = cameraObject.AddComponent<BoardingHouseStreetOrbitCamera>();
            orbit.enabled = false;
            StreetLookAheadCamera lookAhead = cameraObject.AddComponent<StreetLookAheadCamera>();
            lookAhead.enabled = false;
        }

        private static Transform CreateMarker(string name, Vector3 position, Transform parent)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.position = position;
            return marker.transform;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale,
            Material material)
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

            return cube;
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 position, Color color,
            float intensity, float range)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static Material LoadMaterial(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/" + fileName);
        }

        private static Material EnsureFloorWoodWarmMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(Level1RoomAssetPaths.FloorWoodWarmMaterial);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader);
            Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(Level1RoomAssetPaths.WoodFloorDiff);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(Level1RoomAssetPaths.WoodFloorNormal);
            if (diffuse != null)
            {
                material.SetTexture("_BaseMap", diffuse);
                material.SetTextureScale("_BaseMap", new Vector2(2.5f, 2.5f));
            }

            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.SetTextureScale("_BumpMap", new Vector2(2.5f, 2.5f));
            }

            material.SetColor("_BaseColor", new Color(0.82f, 0.68f, 0.52f));
            material.SetFloat("_Smoothness", 0.3f);
            material.SetFloat("_BumpScale", 0.35f);
            AssetDatabase.CreateAsset(material, Level1RoomAssetPaths.FloorWoodWarmMaterial);
            return material;
        }

        private static Material CreateTiledMaterial(string texturePath, Color tint, float tiling)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                    material.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
                }
                else
                {
                    material.mainTexture = texture;
                    material.mainTextureScale = new Vector2(tiling, tiling);
                }
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }
            else
            {
                material.color = tint;
            }

            return material;
        }

        private static GameObject PlaceKenneyFurniture(string fileName, Transform parent, Vector3 position,
            Quaternion rotation, float uniformScale, string name, Material tint,
            float baseKenneyScale = 0f)
        {
            GameObject instance = PlaceKenneyProp(fileName, parent, position, rotation, uniformScale, name, tint,
                baseKenneyScale);
            if (instance != null)
            {
                Level1ScenePlacement.SnapPropBottomToFloor(instance, FloorTopY);
                FitFurnitureColliderFromRenderers(instance);
            }

            return instance;
        }

        private static GameObject PlaceKenneyProp(string fileName, Transform parent, Vector3 position,
            Quaternion rotation, float uniformScale, string name, Material tint = null,
            float baseKenneyScale = 0f)
        {
            GameObject instance = InstantiateKenney(fileName, parent, position, rotation, uniformScale, name, tint,
                baseKenneyScale);
            if (instance != null)
            {
                StripColliders(instance);
            }

            return instance;
        }

        private static GameObject PlaceMarketProp(string path, Transform parent, Vector3 position, Quaternion rotation,
            Vector3 scale, string name, bool useBlockingCollider = false)
        {
            GameObject instance = InstantiatePrefab(path, parent, position, rotation, scale, name);
            if (instance == null)
            {
                return null;
            }

            StripColliders(instance);
            if (useBlockingCollider)
            {
                FitFurnitureColliderFromRenderers(instance);
            }

            return instance;
        }

        private static GameObject PlaceOfficePropOnSurface(GameObject surface, string fileName,
            Vector3 localSurfaceOffset, Quaternion localRotation, float uniformScale, string name)
        {
            return PlacePropOnSurface(surface, Level1RoomAssetPaths.LowPolyOfficeProps + "/" + fileName,
                localSurfaceOffset, localRotation, Vector3.one * (LowPolyOfficePropScale * uniformScale), name);
        }

        private static GameObject PlaceKenneyPropOnSurface(GameObject surface, string kenneyFile,
            Vector3 localSurfaceOffset, Quaternion localRotation, float uniformScale, string name, Material tint)
        {
            return PlacePropOnSurface(surface, Level1RoomAssetPaths.KenneyFbx + "/" + kenneyFile, localSurfaceOffset,
                localRotation, KenneyScaleOf(uniformScale), name, tint);
        }

        private static GameObject PlaceMarketPropOnSurface(GameObject surface, string assetPath,
            Vector3 localSurfaceOffset, Quaternion localRotation, Vector3 scale, string name)
        {
            return PlacePropOnSurface(surface, assetPath, localSurfaceOffset, localRotation, scale, name);
        }

        private static GameObject PlacePropOnShelfTier(GameObject shelf, string kenneyFile, Vector3 localOffset,
            float normalizedTier, Quaternion localRotation, float uniformScale, string name, Material tint)
        {
            return PlacePropOnShelfTier(shelf, Level1RoomAssetPaths.KenneyFbx + "/" + kenneyFile, localOffset,
                normalizedTier, localRotation, KenneyScaleOf(uniformScale), name, tint);
        }

        private static GameObject PlaceMarketPropOnShelfTier(GameObject shelf, string assetPath, Vector3 localOffset,
            float normalizedTier, Quaternion localRotation, Vector3 scale, string name)
        {
            return PlacePropOnShelfTier(shelf, assetPath, localOffset, normalizedTier, localRotation, scale, name);
        }

        private static GameObject PlacePropOnSurface(GameObject surface, string kenneyFile, Vector3 localSurfaceOffset,
            Quaternion localRotation, float uniformScale, string name, Material tint)
        {
            return PlacePropOnSurface(surface, Level1RoomAssetPaths.KenneyFbx + "/" + kenneyFile, localSurfaceOffset,
                localRotation, KenneyScaleOf(uniformScale), name, tint);
        }

        private static GameObject PlacePropOnSurface(GameObject surface, string assetPath, Vector3 localSurfaceOffset,
            Quaternion localRotation, Vector3 scale, string name, Material tint = null)
        {
            if (surface == null)
            {
                return null;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] Missing prefab: " + assetPath);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, surface.transform);
            instance.name = name;
            instance.transform.localPosition = new Vector3(localSurfaceOffset.x, 0f, localSurfaceOffset.z);
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = scale;

            StripColliders(instance);
            if (tint != null)
            {
                ApplyMaterialRecursive(instance, tint);
            }

            SnapPropToSurfaceTop(surface, instance);
            return instance;
        }

        private static void SnapPropToSurfaceTop(GameObject surface, GameObject prop)
        {
            if (surface == null || prop == null)
            {
                return;
            }

            if (surface.name == "Wardrobe")
            {
                Level1ScenePlacement.SnapPropBottomToFurnitureTop(surface, "Wardrobe", prop);
            }
            else if (surface.name == "Desk")
            {
                Level1ScenePlacement.SnapPropBottomToDeskTop(surface, prop);
            }
            else
            {
                Level1ScenePlacement.SnapPropBottomToSurface(surface, prop);
            }
        }

        private static GameObject PlacePropOnShelfTier(GameObject shelf, string assetPath, Vector3 localOffset,
            float normalizedTier, Quaternion localRotation, Vector3 scale, string name, Material tint = null)
        {
            if (shelf == null)
            {
                return null;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] Missing prefab: " + assetPath);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, shelf.transform);
            instance.name = name;
            instance.transform.localPosition = new Vector3(localOffset.x, 0f, localOffset.z);
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = scale;

            StripColliders(instance);
            if (tint != null)
            {
                ApplyMaterialRecursive(instance, tint);
            }

            Level1ScenePlacement.SnapPropToShelfTier(shelf, instance, normalizedTier);
            return instance;
        }

        private static Vector3 GetSurfaceWorldPoint(GameObject surface, Vector3 localSurfaceOffset)
        {
            Vector3 localPoint = new Vector3(localSurfaceOffset.x, 0f, localSurfaceOffset.z);
            Vector3 worldPoint = surface.transform.TransformPoint(localPoint);
            worldPoint.y = Level1ScenePlacement.TryGetDeskTopWorldY(surface, out float deskTop)
                ? deskTop
                : Level1ScenePlacement.GetSurfaceWorldTop(surface);
            return worldPoint;
        }

        private static Vector3 GetShelfTierWorldPoint(GameObject shelf, Vector3 localOffset, float normalizedTier)
        {
            Vector3 localPoint = new Vector3(localOffset.x, 0f, localOffset.z);
            Vector3 worldPoint = shelf.transform.TransformPoint(localPoint);
            worldPoint.y = Level1ScenePlacement.GetShelfTierWorldY(shelf, normalizedTier);
            return worldPoint;
        }

        private static float GetLocalBoundsTop(GameObject root)
        {
            if (!Level1ScenePlacement.TryGetWorldBounds(root, out Bounds worldBounds))
            {
                return KenneyDeskTopY;
            }

            Vector3 localTop = root.transform.InverseTransformPoint(
                new Vector3(worldBounds.center.x, worldBounds.max.y, worldBounds.center.z));
            return localTop.y;
        }

        private static void FitFurnitureColliderFromRenderers(GameObject root, float inflate = 0.05f)
        {
            if (root == null || !Level1ScenePlacement.TryGetWorldBounds(root, out Bounds worldBounds))
            {
                return;
            }

            StripColliders(root);

            Vector3 localCenter = root.transform.InverseTransformPoint(worldBounds.center);
            Vector3 localSize = root.transform.InverseTransformVector(worldBounds.size);
            localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
            localSize += Vector3.one * inflate;

            BoxCollider box = root.AddComponent<BoxCollider>();
            box.center = localCenter;
            box.size = localSize;
            box.isTrigger = false;
        }

        private static bool TryGetWorldBounds(GameObject root, out Bounds bounds)
        {
            return Level1ScenePlacement.TryGetWorldBounds(root, out bounds);
        }

        private static Vector3 KenneyScaleOf(float uniformMultiplier, float baseKenneyScale = 0f)
        {
            float baseScale = baseKenneyScale > 0f ? baseKenneyScale : KenneyFurnitureScale;
            return Vector3.one * (baseScale * uniformMultiplier);
        }

        private static GameObject InstantiateKenney(string fileName, Transform parent, Vector3 position,
            Quaternion rotation, float uniformScale, string name, Material tint = null,
            float baseKenneyScale = 0f)
        {
            GameObject instance = InstantiatePrefab(Level1RoomAssetPaths.KenneyFbx + "/" + fileName, parent, position,
                rotation, KenneyScaleOf(uniformScale, baseKenneyScale), name);
            if (instance != null && tint != null)
            {
                ApplyMaterialRecursive(instance, tint);
            }

            return instance;
        }

        private static GameObject InstantiatePrefab(string path, Transform parent, Vector3 position, Quaternion rotation,
            Vector3 scale, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("[Level1SceneBuilder] Missing prefab: " + path);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;
            return instance;
        }

        private static void ApplyMaterialRecursive(GameObject root, Material material)
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

                renderers[i].sharedMaterials = mats;
            }
        }

        /// <summary>
        /// Maps Kenney carpet/fabric slots to cozy sage upholstery and wood/metal slots to frame materials.
        /// Uses height split so backrest reads slightly darker than the seat cushion.
        /// </summary>
        private static void ApplyKenneyUpholsteredMaterials(GameObject root, Material fabricSeat, Material fabricBack,
            Material woodFrame, Material metalFrame, Material linenMaterial = null)
        {
            if (root == null || fabricSeat == null || fabricBack == null)
            {
                return;
            }

            Material frameMaterial = woodFrame != null ? woodFrame : metalFrame;
            Material metalMaterial = metalFrame != null ? metalFrame : woodFrame;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            float fabricSplitY = ComputeFabricBackrestSplitY(renderers);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material[] sourceMaterials = renderer.sharedMaterials;
                Material[] targetMaterials = new Material[sourceMaterials.Length];
                bool hasFabricSlot = false;
                string meshName = renderer.gameObject.name.ToLowerInvariant();

                for (int j = 0; j < sourceMaterials.Length; j++)
                {
                    string slotName = sourceMaterials[j] != null
                        ? sourceMaterials[j].name.ToLowerInvariant()
                        : string.Empty;

                    if (linenMaterial != null && IsKenneyLinenSlot(slotName, meshName))
                    {
                        targetMaterials[j] = linenMaterial;
                    }
                    else if (IsKenneyFabricSlot(slotName, meshName))
                    {
                        hasFabricSlot = true;
                        bool isBackrest = IsBackrestRenderer(renderer, fabricSplitY, meshName);
                        targetMaterials[j] = isBackrest ? fabricBack : fabricSeat;
                    }
                    else if (IsKenneyWoodSlot(slotName, meshName))
                    {
                        targetMaterials[j] = frameMaterial;
                    }
                    else if (IsKenneyMetalSlot(slotName, meshName))
                    {
                        targetMaterials[j] = metalMaterial;
                    }
                    else
                    {
                        targetMaterials[j] = fabricSeat;
                    }
                }

                if (!hasFabricSlot && IsLikelyFabricRenderer(renderer, meshName: renderer.gameObject.name))
                {
                    for (int j = 0; j < targetMaterials.Length; j++)
                    {
                        targetMaterials[j] = IsBackrestRenderer(renderer, fabricSplitY, meshName)
                            ? fabricBack
                            : fabricSeat;
                    }
                }

                renderer.sharedMaterials = targetMaterials;
            }
        }

        private static float ComputeFabricBackrestSplitY(Renderer[] renderers)
        {
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    string slotName = mats[j] != null ? mats[j].name.ToLowerInvariant() : string.Empty;
                    if (!IsKenneyFabricSlot(slotName, renderers[i].gameObject.name.ToLowerInvariant()))
                    {
                        continue;
                    }

                    minY = Mathf.Min(minY, renderers[i].bounds.min.y);
                    maxY = Mathf.Max(maxY, renderers[i].bounds.max.y);
                }
            }

            if (minY >= maxY)
            {
                return 0.45f;
            }

            return minY + (maxY - minY) * 0.52f;
        }

        private static bool IsBackrestRenderer(Renderer renderer, float splitY, string meshName)
        {
            if (meshName.Contains("back"))
            {
                return true;
            }

            if (meshName.Contains("seat") || meshName.Contains("cushion"))
            {
                return false;
            }

            return renderer.bounds.center.y >= splitY;
        }

        private static bool IsLikelyFabricRenderer(Renderer renderer, string meshName)
        {
            meshName = meshName.ToLowerInvariant();
            return meshName.Contains("chair") || meshName.Contains("sofa") || meshName.Contains("cushion")
                || meshName.Contains("cover") || meshName.Contains("bed");
        }

        private static bool IsKenneyFabricSlot(string slotName, string meshName)
        {
            if (IsKenneyLinenSlot(slotName, meshName))
            {
                return false;
            }

            return slotName.Contains("carpet") || slotName.Contains("fabric") || slotName.Contains("cloth")
                || meshName.Contains("cover");
        }

        private static bool IsKenneyLinenSlot(string slotName, string meshName)
        {
            return slotName.Contains("carpetwhite") || slotName.Contains("white")
                || meshName.Contains("pillow");
        }

        private static bool IsKenneyWoodSlot(string slotName, string meshName)
        {
            return slotName.Contains("wood") || meshName.Contains("wood");
        }

        private static bool IsKenneyMetalSlot(string slotName, string meshName)
        {
            return slotName.Contains("metal") || meshName.Contains("metal");
        }

        private static void StripColliders(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Collider[] colliders = target.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }
    }
}
#endif
