#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds a reusable low-poly Vietnamese neighborhood pawn shop storefront prefab.</summary>
    /// <remarks>Street amenity row: Circle K, car wash, pawn shop, mobile store.</remarks>
    public static class Level1VietnamesePawnShopBuilder
    {
        public const string RootName = "PF_PawnShop";
        public const string PrefabPath = "Assets/Art/Level1/Outdoor/Prefabs/PF_PawnShop.prefab";
        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials/Level1";
        private const string MotorbikePrefabPath = "Assets/Honda Motor Co., Ltd/Super Cub C125/Prefabs/C125 - MM.prefab";
        private const string SidewalkMaterialPath = "Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat";
        private const string BannerTexturePath = "Assets/Art/Level1/Outdoor/Textures/PawnShop_PhucTin_Banner.png";
        private const string BannerMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_PawnShop_PhucTin.mat";

        private const float FacadeWidth = 4.6f;
        private const float FacadeDepth = 2.05f;
        private const float WallHeight = 3.05f;
        private const float FoundationHeight = 0.12f;
        private const float RoofThickness = 0.1f;
        private const float CanopyDepth = 0.72f;
        private const float CanopyHeight = 0.06f;
        private const float FasciaHeight = 0.62f;
        private const float DoorWidth = 1.55f;
        private const float DoorHeight = 2.28f;
        private const float FrameThickness = 0.06f;
        private const float GlassStartY = FoundationHeight + 0.04f;
        private const float RoadCenterX = 0f;
        private const float RoadHalfWidth = 2.38f;
        private const float SidewalkSetback = 1.28f;
        private const float BannerWidth = FacadeWidth - 0.08f;
        private const float BannerHeight = 1.15f;

        private static readonly Color GreenDark = new Color(0.08f, 0.34f, 0.2f);
        private static readonly Color GreenTrim = new Color(0.12f, 0.42f, 0.26f);
        private static readonly Color YellowAccent = new Color(0.96f, 0.78f, 0.1f);
        private static readonly Color WhiteTrim = new Color(0.96f, 0.97f, 0.98f);

        public static void BuildPrefabAsset()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamesePawnShopBuilder] Exit Play mode before building the prefab.");
                return;
            }

            GameObject root = BuildAt(Vector3.zero, Quaternion.identity, saveAsPrefab: true);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log("[Level1VietnamesePawnShopBuilder] Prefab saved: " + PrefabPath);
        }

        public static void AttachBannerOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamesePawnShopBuilder] Exit Play mode before attaching the banner.");
                return;
            }

            GameObject pawn = GameObject.Find(RootName);
            if (pawn == null)
            {
                Debug.LogError("[Level1VietnamesePawnShopBuilder] PF_PawnShop not found in scene.");
                return;
            }

            ApplyBannerToPawnShop(pawn.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnamesePawnShopBuilder] Pawn shop banner attached.");
        }

        public static void PlaceOppositeCircleKOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamesePawnShopBuilder] Exit Play mode before placing the pawn shop.");
                return;
            }

            GameObject store = GameObject.Find(Level1ConvenienceStoreFacadeBuilder.RootName);
            if (store == null)
            {
                Debug.LogError("[Level1VietnamesePawnShopBuilder] ConvenienceStoreFacade not found.");
                return;
            }

            RemoveExistingInScene();
            Transform parent = FindOutdoorParent();
            if (!TryResolvePlacementOppositeStore(store.transform, out Vector3 position, out Quaternion rotation))
            {
                Debug.LogError("[Level1VietnamesePawnShopBuilder] Could not resolve placement opposite Circle K.");
                return;
            }

            GameObject root = BuildAt(position, rotation, saveAsPrefab: false);
            if (root == null)
            {
                return;
            }

            if (parent != null)
            {
                root.transform.SetParent(parent, true);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnamesePawnShopBuilder] Pawn shop placed opposite Circle K at " + position);
        }

        public static bool TryResolvePlacementOppositeStore(Transform store, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.Euler(0f, -90f, 0f);

            float halfDepth = FacadeDepth * 0.5f;
            float mirroredX = RoadCenterX - (store.position.x - RoadCenterX);
            float westLimitX = -(RoadHalfWidth + SidewalkSetback + halfDepth);
            float targetX = Mathf.Min(mirroredX, westLimitX);
            position = new Vector3(targetX, Level1OutdoorSettings.WalkSurfaceY, store.position.z);
            rotation = Quaternion.Euler(0f, -90f, 0f);
            return true;
        }

        public static GameObject BuildAt(Vector3 worldPosition, Quaternion rotation, bool saveAsPrefab)
        {
            Material wallGreen = EnsureMaterial("PawnShop_Wall_Green.mat", GreenDark, 0f, 0.2f);
            Material trimYellow = EnsureMaterial("PawnShop_Trim_Yellow.mat", YellowAccent, 0f, 0.42f);
            Material trimWhite = EnsureMaterial("PawnShop_Trim_White.mat", WhiteTrim, 0f, 0.24f);
            Material roof = EnsureMaterial("PawnShop_Roof_Flat.mat", new Color(0.72f, 0.74f, 0.76f), 0.08f, 0.22f);
            Material frame = EnsureMaterial("PawnShop_Frame_Aluminum.mat", new Color(0.58f, 0.6f, 0.63f), 0.35f, 0.52f);
            Material glass = EnsureTintedGlassMaterial();
            Material shutter = EnsureMaterial("PawnShop_Shutter_Gray.mat", new Color(0.42f, 0.44f, 0.46f), 0.2f, 0.35f);
            Material interiorWall = EnsureMaterial("PawnShop_Interior_Wall.mat", new Color(0.9f, 0.92f, 0.9f), 0f, 0.16f);
            Material interiorFloor = EnsureMaterial("PawnShop_Interior_Floor.mat", new Color(0.74f, 0.76f, 0.78f), 0f, 0.28f);
            Material wood = EnsureMaterial("PawnShop_Wood.mat", new Color(0.52f, 0.38f, 0.24f), 0f, 0.3f);
            Material plastic = EnsureMaterial("PawnShop_Plastic_Red.mat", new Color(0.86f, 0.2f, 0.18f), 0f, 0.4f);
            Material decalGreen = EnsureUnlitMaterial("PawnShop_Decal_Green.mat", new Color(0.1f, 0.55f, 0.28f, 0.95f));
            Material decalYellow = EnsureUnlitMaterial("PawnShop_Decal_Yellow.mat", new Color(0.98f, 0.82f, 0.12f, 0.95f));
            Material sidewalk = AssetDatabase.LoadAssetAtPath<Material>(SidewalkMaterialPath)
                ?? EnsureMaterial("PawnShop_Sidewalk.mat", new Color(0.62f, 0.64f, 0.66f), 0f, 0.22f);

            float halfWidth = FacadeWidth * 0.5f;
            float halfDepth = FacadeDepth * 0.5f;
            float frontZ = -halfDepth;
            float backZ = halfDepth;
            float fasciaY = FoundationHeight + WallHeight - FasciaHeight * 0.5f - 0.03f;
            float glassTopY = fasciaY - FasciaHeight * 0.5f - 0.05f;
            float glassHeight = glassTopY - GlassStartY;
            float glassCenterY = GlassStartY + glassHeight * 0.5f;
            float doorCenterY = GlassStartY + DoorHeight * 0.5f;
            float sideGlassWidth = (FacadeWidth - DoorWidth - FrameThickness * 4f) * 0.5f;
            float leftGlassX = -(DoorWidth * 0.5f + FrameThickness + sideGlassWidth * 0.5f);
            float rightGlassX = -leftGlassX;

            GameObject root = new GameObject(RootName);
            root.transform.position = worldPosition;
            root.transform.rotation = rotation;

            GameObject structure = new GameObject("Pawn_Structure");
            structure.transform.SetParent(root.transform, false);
            BuildStructure(structure.transform, wallGreen, trimYellow, trimWhite, roof, halfWidth, halfDepth, frontZ, backZ,
                fasciaY);

            GameObject storefront = new GameObject("Pawn_Storefront");
            storefront.transform.SetParent(root.transform, false);
            BuildStorefront(storefront.transform, frame, glass, shutter, frontZ, glassCenterY, glassHeight, glassTopY,
                doorCenterY, sideGlassWidth, leftGlassX, rightGlassX, decalGreen, decalYellow);

            GameObject sign = new GameObject("Pawn_Sign");
            sign.transform.SetParent(root.transform, false);
            BuildMainSign(sign.transform, fasciaY, frontZ);

            GameObject interior = new GameObject("Pawn_Interior");
            interior.transform.SetParent(root.transform, false);
            BuildInterior(interior.transform, interiorWall, interiorFloor, wood, plastic, trimYellow, frontZ, backZ,
                glassCenterY, glassHeight, glassTopY, halfWidth);

            GameObject environment = new GameObject("Pawn_Environment");
            environment.transform.SetParent(root.transform, false);
            BuildEnvironment(environment.transform, sidewalk, trimWhite, trimYellow, plastic, frontZ, halfWidth);

            GameObject props = new GameObject("Pawn_ExteriorProps");
            props.transform.SetParent(root.transform, false);
            BuildExteriorProps(props.transform, trimYellow, trimWhite, plastic, frame, frontZ, halfWidth, fasciaY);

            MarkStatic(root);

            if (saveAsPrefab)
            {
                EnsureFolder("Assets/Art/Level1/Outdoor/Prefabs");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
            }

            return root;
        }

        private static void BuildStructure(Transform parent, Material wall, Material yellow, Material white, Material roof,
            float halfWidth, float halfDepth, float frontZ, float backZ, float fasciaY)
        {
            CreatePartCube("Pawn_Foundation", parent, new Vector3(0f, FoundationHeight * 0.5f, 0f),
                new Vector3(FacadeWidth + 0.06f, FoundationHeight, FacadeDepth + 0.06f), white);
            CreatePartCube("Pawn_Wall_Left", parent, new Vector3(-halfWidth + 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);
            CreatePartCube("Pawn_Wall_Right", parent, new Vector3(halfWidth - 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);
            CreatePartCube("Pawn_Wall_Back", parent, new Vector3(0f, FoundationHeight + WallHeight * 0.5f, backZ - 0.05f),
                new Vector3(FacadeWidth - 0.14f, WallHeight, 0.1f), wall);
            CreatePartCube("Pawn_Roof", parent,
                new Vector3(0f, FoundationHeight + WallHeight + RoofThickness * 0.5f, 0f),
                new Vector3(FacadeWidth + 0.1f, RoofThickness, FacadeDepth + 0.1f), roof);
            CreatePartCube("Pawn_Fascia", parent, new Vector3(0f, fasciaY, frontZ - 0.04f),
                new Vector3(FacadeWidth - 0.18f, FasciaHeight, 0.12f), yellow);
            CreatePartCube("Pawn_Canopy", parent, new Vector3(0f, FoundationHeight + WallHeight - 0.08f, frontZ - CanopyDepth * 0.5f - 0.02f),
                new Vector3(FacadeWidth - 0.08f, CanopyHeight, CanopyDepth), white);
            CreatePartCube("Pawn_Canopy_Lip", parent,
                new Vector3(0f, FoundationHeight + WallHeight - 0.04f, frontZ - CanopyDepth - 0.02f),
                new Vector3(FacadeWidth - 0.04f, 0.04f, 0.06f), yellow);
        }

        private static void BuildStorefront(Transform parent, Material frame, Material glass, Material shutter,
            float frontZ, float glassCenterY, float glassHeight, float glassTopY, float doorCenterY,
            float sideGlassWidth, float leftGlassX, float rightGlassX, Material decalGreen, Material decalYellow)
        {
            float frameZ = frontZ - 0.03f;
            CreatePartCube("Pawn_Glass_Left", parent, new Vector3(leftGlassX, glassCenterY, frontZ - 0.02f),
                new Vector3(sideGlassWidth, glassHeight, 0.04f), glass);
            CreatePartCube("Pawn_Glass_Right", parent, new Vector3(rightGlassX, glassCenterY, frontZ - 0.02f),
                new Vector3(sideGlassWidth, glassHeight, 0.04f), glass);
            CreatePartCube("Pawn_Frame_Header", parent, new Vector3(0f, glassTopY + FrameThickness * 0.5f, frameZ),
                new Vector3(FacadeWidth - 0.2f, FrameThickness, 0.06f), frame);
            CreatePartCube("Pawn_Frame_Sill", parent, new Vector3(0f, GlassStartY - FrameThickness * 0.5f, frameZ),
                new Vector3(FacadeWidth - 0.2f, FrameThickness, 0.06f), frame);
            CreatePartCube("Pawn_Door_Left", parent, new Vector3(-DoorWidth * 0.25f, doorCenterY, frontZ - 0.025f),
                new Vector3(DoorWidth * 0.5f - 0.04f, DoorHeight - 0.06f, 0.035f), glass);
            CreatePartCube("Pawn_Door_Right", parent, new Vector3(DoorWidth * 0.25f, doorCenterY, frontZ - 0.025f),
                new Vector3(DoorWidth * 0.5f - 0.04f, DoorHeight - 0.06f, 0.035f), glass);
            CreatePartCube("Pawn_Shutter", parent, new Vector3(0f, glassCenterY + 0.08f, frontZ + 0.06f),
                new Vector3(FacadeWidth - 0.34f, glassHeight - 0.12f, 0.05f), shutter);

            CreateWindowDecal(parent, "Decal_FastCash", "GIẢI NGÂN NHANH", leftGlassX, glassCenterY + 0.35f,
                frontZ - 0.035f, 0.42f, 0.18f, decalYellow);
            CreateWindowDecal(parent, "Decal_FairRate", "LÃI SUẤT HỢP LÝ", rightGlassX, glassCenterY + 0.1f,
                frontZ - 0.035f, 0.42f, 0.18f, decalGreen);
            CreateWindowDecal(parent, "Decal_Support24", "HỖ TRỢ 24/7", rightGlassX, glassCenterY - 0.28f,
                frontZ - 0.035f, 0.38f, 0.16f, decalYellow);
        }

        private static void CreateWindowDecal(Transform parent, string name, string text, float x, float y, float z,
            float width, float height, Material backing)
        {
            GameObject decalRoot = new GameObject(name);
            decalRoot.transform.SetParent(parent, false);
            decalRoot.transform.localPosition = new Vector3(x, y, z);
            CreatePartCube("Backing", decalRoot.transform, Vector3.zero, new Vector3(width, height, 0.012f), backing);
            CreateSignLine(decalRoot.transform, "Text", text, 0f, height * 0.34f, 0.11f, Color.white, FontStyles.Bold);
        }

        public static void ApplyBannerToPawnShop(Transform pawnRoot)
        {
            Transform sign = pawnRoot.Find("Pawn_Sign");
            if (sign == null)
            {
                Debug.LogError("[Level1VietnamesePawnShopBuilder] Pawn_Sign not found.");
                return;
            }

            Object.DestroyImmediate(sign.gameObject);

            Transform structure = pawnRoot.Find("Pawn_Structure");
            Transform fascia = structure != null ? structure.Find("Pawn_Fascia") : null;
            if (fascia != null)
            {
                fascia.gameObject.SetActive(false);
            }

            float frontZ = -FacadeDepth * 0.5f;
            float fasciaY = FoundationHeight + WallHeight - FasciaHeight * 0.5f - 0.03f;

            GameObject signRoot = new GameObject("Pawn_Sign");
            signRoot.transform.SetParent(pawnRoot, false);
            BuildMainSign(signRoot.transform, fasciaY, frontZ);
        }

        private static void BuildMainSign(Transform parent, float fasciaY, float frontZ)
        {
            Material bannerMat = EnsureBannerMaterial();
            if (bannerMat == null)
            {
                return;
            }

            float signY = fasciaY + FasciaHeight * 0.5f + BannerHeight * 0.5f + 0.06f;
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "PawnShop_Banner";
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = new Vector3(0f, signY, frontZ - 0.05f);
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = new Vector3(BannerWidth, BannerHeight, 1f);
            Object.DestroyImmediate(quad.GetComponent<Collider>());

            Material instance = new Material(bannerMat);
            instance.SetInt("_Cull", 0);
            instance.renderQueue = (int)RenderQueue.Geometry + 10;
            ApplyRenderer(quad.GetComponent<Renderer>(), instance);
        }

        private static void BuildInterior(Transform parent, Material wall, Material floor, Material wood, Material plastic,
            Material yellow, float frontZ, float backZ, float glassCenterY, float glassHeight, float glassTopY,
            float halfWidth)
        {
            float interiorFrontZ = frontZ + 0.12f;
            float interiorBackZ = backZ - 0.14f;
            float interiorCenterZ = (interiorFrontZ + interiorBackZ) * 0.5f;
            float interiorDepth = interiorBackZ - interiorFrontZ;

            CreatePartCube("Interior_Floor", parent, new Vector3(0f, FoundationHeight + 0.02f, interiorCenterZ),
                new Vector3(FacadeWidth - 0.28f, 0.04f, interiorDepth), floor);
            CreatePartCube("Interior_Ceiling", parent, new Vector3(0f, glassTopY - 0.03f, interiorCenterZ),
                new Vector3(FacadeWidth - 0.28f, 0.05f, interiorDepth), wall);
            CreatePartCube("Interior_Counter", parent, new Vector3(0f, FoundationHeight + 0.92f, interiorFrontZ + 0.55f),
                new Vector3(2.1f, 0.9f, 0.42f), yellow);
            CreatePartCube("Interior_Monitor", parent, new Vector3(0.05f, FoundationHeight + 1.42f, interiorFrontZ + 0.48f),
                new Vector3(0.42f, 0.28f, 0.05f), plastic);
            CreatePartCube("Interior_Chair_A", parent, new Vector3(-0.55f, FoundationHeight + 0.55f, interiorFrontZ + 0.95f),
                new Vector3(0.34f, 0.04f, 0.34f), plastic);
            CreatePartCube("Interior_Chair_B", parent, new Vector3(0.62f, FoundationHeight + 0.55f, interiorFrontZ + 0.95f),
                new Vector3(0.34f, 0.04f, 0.34f), plastic);
            CreatePartCube("Interior_WaitChair", parent, new Vector3(-halfWidth + 0.55f, FoundationHeight + 0.42f, interiorFrontZ + 1.35f),
                new Vector3(0.28f, 0.04f, 0.28f), wood);
            CreatePartCube("Interior_WaitChair_2", parent, new Vector3(-halfWidth + 0.9f, FoundationHeight + 0.42f, interiorFrontZ + 1.35f),
                new Vector3(0.28f, 0.04f, 0.28f), wood);

            for (int i = 0; i < 4; i++)
            {
                float shelfX = -halfWidth + 0.42f + i * 0.42f;
                CreatePartCube("Interior_Shelf_" + i, parent,
                    new Vector3(shelfX, FoundationHeight + 1.15f, interiorBackZ - 0.08f),
                    new Vector3(0.34f, 1.45f, 0.22f), wall);
                CreateSignLine(parent, "ShelfLabel_" + i, (i + 1).ToString("00"),
                    FoundationHeight + 1.55f, interiorBackZ - 0.16f, 0.12f, GreenDark, FontStyles.Bold);
            }
        }

        private static void BuildEnvironment(Transform parent, Material sidewalk, Material white, Material yellow,
            Material plastic, float frontZ, float halfWidth)
        {
            CreatePartCube("Env_Sidewalk", parent, new Vector3(0f, 0.04f, frontZ - 1.05f),
                new Vector3(FacadeWidth + 1.35f, 0.08f, 1.45f), sidewalk);
            CreatePartCube("Env_FlowerPot", parent, new Vector3(halfWidth - 0.35f, 0.16f, frontZ - 0.95f),
                new Vector3(0.22f, 0.24f, 0.22f), plastic);
            CreatePartCube("Env_Flower", parent, new Vector3(halfWidth - 0.35f, 0.34f, frontZ - 0.95f),
                new Vector3(0.18f, 0.12f, 0.18f), EnsureUnlitMaterial("PawnShop_Flower.mat", new Color(0.2f, 0.72f, 0.28f)));
            CreatePartCube("Env_UtilityMeter", parent, new Vector3(-halfWidth + 0.18f, FoundationHeight + 1.05f, frontZ + 0.55f),
                new Vector3(0.18f, 0.28f, 0.1f), white);
            CreatePartCube("Env_MeterPipe", parent, new Vector3(-halfWidth + 0.18f, FoundationHeight + 0.55f, frontZ + 0.58f),
                new Vector3(0.05f, 0.7f, 0.05f), yellow);

            GameObject address = new GameObject("Env_AddressPlaque");
            address.transform.SetParent(parent, false);
            address.transform.localPosition = new Vector3(-halfWidth + 0.42f, FoundationHeight + 1.45f, frontZ - 0.08f);
            CreateSignLine(address.transform, "Line1", "126/45 Đường Bùi Đình Túy", 0.08f, -0.03f, 0.08f, Color.white,
                FontStyles.Normal);
            CreateSignLine(address.transform, "Line2", "Phường 24, Quận Bình Thạnh, TP. Hồ Chí Minh", -0.08f, -0.03f,
                0.06f, Color.white, FontStyles.Normal);
        }

        private static void BuildExteriorProps(Transform parent, Material yellow, Material white, Material plastic,
            Material frame, float frontZ, float halfWidth, float fasciaY)
        {
            InstantiateScooter(parent, new Vector3(halfWidth + 0.55f, 0.02f, frontZ - 0.35f), 205f);
            CreatePartCube("Prop_ABoard", parent, new Vector3(-halfWidth - 0.42f, 0.62f, frontZ - 0.55f),
                new Vector3(0.55f, 0.78f, 0.05f), yellow);
            CreateSignLine(parent, "ABoard_Text", "GIẢI NGÂN\nNHANH 24/7", 0.62f, frontZ - 0.58f, 0.1f, GreenDark,
                FontStyles.Bold);
            CreatePartCube("Prop_ATMSign", parent, new Vector3(halfWidth - 0.35f, FoundationHeight + 1.75f, frontZ - 0.12f),
                new Vector3(0.42f, 0.55f, 0.05f), white);
            CreateSignLine(parent, "ATM_Text", "ATM", FoundationHeight + 1.78f, frontZ - 0.15f, 0.2f, GreenDark,
                FontStyles.Bold);
            CreatePartCube("Prop_CCTV", parent, new Vector3(0.15f, FoundationHeight + WallHeight - 0.12f, frontZ - 0.1f),
                new Vector3(0.12f, 0.08f, 0.1f), frame);
            CreatePartCube("Prop_CCTV_Lens", parent, new Vector3(0.15f, FoundationHeight + WallHeight - 0.12f, frontZ - 0.16f),
                new Vector3(0.05f, 0.05f, 0.03f), plastic);
            CreatePartCube("Prop_SecurityLight", parent, new Vector3(-0.45f, FoundationHeight + WallHeight - 0.08f, frontZ - 0.08f),
                new Vector3(0.14f, 0.08f, 0.1f), white);
            CreatePartCube("Prop_LightBulb", parent, new Vector3(-0.45f, FoundationHeight + WallHeight - 0.08f, frontZ - 0.14f),
                new Vector3(0.08f, 0.08f, 0.04f), yellow);
        }

        private static void InstantiateScooter(Transform parent, Vector3 localPos, float yaw)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MotorbikePrefabPath);
            if (prefab == null)
            {
                CreatePartCube("Scooter_Placeholder", parent, localPos + new Vector3(0f, 0.35f, 0f),
                    new Vector3(0.45f, 0.7f, 1.4f), EnsureMaterial("PawnShop_Scooter_Placeholder.mat", new Color(0.2f, 0.22f, 0.25f), 0.2f, 0.4f));
                return;
            }

            GameObject bike = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            bike.name = "Pawn_Scooter";
            bike.transform.localPosition = localPos;
            bike.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            bike.transform.localScale = Vector3.one * 0.88f;
            foreach (Collider collider in bike.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(collider);
            }

            foreach (Rigidbody rb in bike.GetComponentsInChildren<Rigidbody>(true))
            {
                Object.DestroyImmediate(rb);
            }
        }

        private static void CreateSignLine(Transform parent, string name, string text, float localY, float localZ,
            float fontSize, Color color, FontStyles style)
        {
            GameObject line = new GameObject(name);
            line.transform.SetParent(parent, false);
            line.transform.localPosition = new Vector3(0f, localY, localZ);
            line.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TextMeshPro tmp = line.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = style;
            tmp.rectTransform.sizeDelta = new Vector2(3.8f, 0.5f);
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        private static Transform FindOutdoorParent()
        {
            Transform world = GameObject.Find("Level1World")?.transform;
            return world != null ? world.Find("Level1_OutdoorArea") : null;
        }

        private static void RemoveExistingInScene()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static GameObject CreatePartCube(string name, Transform parent, Vector3 localPos, Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            ApplyRenderer(cube.GetComponent<Renderer>(), material);
            return cube;
        }

        private static void ApplyRenderer(Renderer renderer, Material material)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material EnsureMaterial(string fileName, Color baseColor, float metallic, float smoothness)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = Path.GetFileNameWithoutExtension(fileName);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureUnlitMaterial(string fileName, Color baseColor)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                mat = new Material(shader);
                mat.name = Path.GetFileNameWithoutExtension(fileName);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureBannerMaterial()
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
            if (texture == null)
            {
                Debug.LogError("[Level1VietnamesePawnShopBuilder] Missing banner texture: " + BannerTexturePath);
                return null;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(BannerMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_PawnShop_PhucTin";
                AssetDatabase.CreateAsset(mat, BannerMaterialPath);
            }

            mat.SetTexture("_BaseMap", texture);
            mat.SetTexture("_MainTex", texture);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_Color", Color.white);
            mat.doubleSidedGI = true;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureTintedGlassMaterial()
        {
            string path = MaterialsRoot + "/PawnShop_Glass_Tinted.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = "PawnShop_Glass_Tinted";
                AssetDatabase.CreateAsset(mat, path);
            }

            Color tint = new Color(0.18f, 0.22f, 0.28f, 0.72f);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_Color", tint);
            mat.SetFloat("_Metallic", 0.15f);
            mat.SetFloat("_Smoothness", 0.82f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void MarkStatic(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(transforms[i].gameObject, StaticEditorFlags.BatchingStatic);
            }
        }
    }
}
#endif
