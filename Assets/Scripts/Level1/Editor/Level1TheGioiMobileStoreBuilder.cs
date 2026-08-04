#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds a reusable low-poly Vietnamese phone retail storefront (inspired layout, custom branding).</summary>
    public static class Level1TheGioiMobileStoreBuilder
    {
        public const string RootName = "PF_TheGioiMobile_Store";
        public const string PrefabPath = "Assets/Art/Level1/Outdoor/Prefabs/PF_TheGioiMobile_Store.prefab";
        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials/Level1";
        private const string SidewalkMaterialPath = "Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat";
        private const string BannerTexturePath = "Assets/Art/Level1/Outdoor/Textures/MobileStore_TheGioiMobile_Banner.png";
        private const string BannerMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_MobileStore_TheGioiMobile.mat";
        private const float RoadCenterX = 0f;
        private const float RoadHalfWidth = 2.38f;
        private const float SidewalkSetback = 1.28f;
        private const float BannerWidth = FacadeWidth - 0.08f;
        private const float BannerHeight = BannerWidth * 0.5f;

        private const float FacadeWidth = 5.1f;
        private const float FacadeDepth = 2.25f;
        private const float WallHeight = 3.2f;
        private const float FoundationHeight = 0.12f;
        private const float RoofThickness = 0.1f;
        private const float FasciaHeight = 0.68f;
        private const float DoorWidth = 1.65f;
        private const float DoorHeight = 2.35f;
        private const float FrameThickness = 0.06f;
        private const float GlassStartY = FoundationHeight + 0.04f;
        private const float FoundationLift = 0.12f;

        private static readonly Color YellowBrand = new Color(0.98f, 0.82f, 0.05f);
        private static readonly Color BlackBrand = new Color(0.08f, 0.08f, 0.09f);
        private static readonly Color WhiteTrim = new Color(0.97f, 0.97f, 0.98f);

        public static void BuildPrefabAsset()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1TheGioiMobileStoreBuilder] Exit Play mode before building the prefab.");
                return;
            }

            GameObject root = BuildAt(Vector3.zero, Quaternion.identity, saveAsPrefab: true);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log("[Level1TheGioiMobileStoreBuilder] Prefab saved: " + PrefabPath);
        }

        public static void AttachBannerOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1TheGioiMobileStoreBuilder] Exit Play mode before attaching the banner.");
                return;
            }

            GameObject mobile = GameObject.Find(RootName);
            if (mobile == null)
            {
                Debug.LogError("[Level1TheGioiMobileStoreBuilder] PF_TheGioiMobile_Store not found in scene.");
                return;
            }

            ApplyBannerToMobileStore(mobile.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1TheGioiMobileStoreBuilder] Mobile store banner attached.");
        }

        public static void PlaceOppositeCarWashOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1TheGioiMobileStoreBuilder] Exit Play mode before placing the store.");
                return;
            }

            GameObject wash = GameObject.Find(Level1VietnameseCarWashBuilder.RootName);
            if (wash == null)
            {
                Debug.LogError("[Level1TheGioiMobileStoreBuilder] PF_Vietnamese_CarWash not found.");
                return;
            }

            RemoveExistingInScene();
            Transform parent = FindOutdoorParent();
            if (!TryResolvePlacementOppositeCarWash(wash.transform, out Vector3 position, out Quaternion rotation))
            {
                Debug.LogError("[Level1TheGioiMobileStoreBuilder] Could not resolve placement opposite car wash.");
                return;
            }

            GameObject root = BuildAt(position, rotation, saveAsPrefab: false);
            if (root == null)
            {
                return;
            }

            ApplyBannerToMobileStore(root.transform);

            if (parent != null)
            {
                root.transform.SetParent(parent, true);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1TheGioiMobileStoreBuilder] Mobile store placed opposite car wash at " + position);
        }

        public static bool TryResolvePlacementOppositeCarWash(Transform carWash, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.Euler(0f, -90f, 0f);

            float halfDepth = FacadeDepth * 0.5f;
            float mirroredX = RoadCenterX - (carWash.position.x - RoadCenterX);
            float westLimitX = -(RoadHalfWidth + SidewalkSetback + halfDepth);
            float targetX = Mathf.Min(mirroredX, westLimitX);
            position = new Vector3(targetX, Level1OutdoorSettings.WalkSurfaceY + FoundationLift, carWash.position.z);
            rotation = Quaternion.Euler(0f, -90f, 0f);
            return true;
        }

        public static GameObject BuildAt(Vector3 worldPosition, Quaternion rotation, bool saveAsPrefab)
        {
            Material wallWhite = EnsureMaterial("Mobile_Wall_White.mat", WhiteTrim, 0f, 0.2f);
            Material yellow = EnsureMaterial("Mobile_Brand_Yellow.mat", YellowBrand, 0f, 0.42f);
            Material black = EnsureMaterial("Mobile_Brand_Black.mat", BlackBrand, 0.15f, 0.38f);
            Material roof = EnsureMaterial("Mobile_Roof_Flat.mat", new Color(0.2f, 0.2f, 0.22f), 0.1f, 0.25f);
            Material frame = EnsureMaterial("Mobile_Frame_Black.mat", BlackBrand, 0.35f, 0.5f);
            Material clearGlass = EnsureClearGlassMaterial();
            Material glassTop = EnsureMaterial("Mobile_GlassTop.mat", new Color(0.92f, 0.94f, 0.98f, 0.35f), 0.05f, 0.92f);
            Material interiorFloor = EnsureMaterial("Mobile_Interior_Floor.mat", new Color(0.82f, 0.84f, 0.86f), 0f, 0.3f);
            Material counterBody = EnsureMaterial("Mobile_Counter_White.mat", WhiteTrim, 0f, 0.35f);
            Material phoneBody = EnsureMaterial("Mobile_Phone_Black.mat", BlackBrand, 0.25f, 0.55f);
            Material boxBrown = EnsureMaterial("Mobile_Box_Brown.mat", new Color(0.55f, 0.42f, 0.3f), 0f, 0.28f);
            Material posterYellow = EnsureUnlitMaterial("Mobile_Poster_Yellow.mat", new Color(0.98f, 0.84f, 0.08f, 0.92f));
            Material posterBlack = EnsureUnlitMaterial("Mobile_Poster_Black.mat", new Color(0.06f, 0.06f, 0.07f, 0.9f));
            Material led = EnsureUnlitMaterial("Mobile_LED_White.mat", new Color(1f, 0.98f, 0.92f));
            Material sidewalk = AssetDatabase.LoadAssetAtPath<Material>(SidewalkMaterialPath)
                ?? EnsureMaterial("Mobile_Sidewalk.mat", new Color(0.62f, 0.64f, 0.66f), 0f, 0.22f);

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

            GameObject structure = new GameObject("Mobile_Structure");
            structure.transform.SetParent(root.transform, false);
            BuildStructure(structure.transform, wallWhite, yellow, black, roof, halfWidth, halfDepth, frontZ, backZ, fasciaY);

            GameObject sign = new GameObject("Mobile_Sign");
            sign.transform.SetParent(root.transform, false);
            BuildBannerSign(sign.transform, fasciaY, frontZ);

            GameObject storefront = new GameObject("Mobile_Storefront");
            storefront.transform.SetParent(root.transform, false);
            BuildStorefront(storefront.transform, frame, clearGlass, yellow, black, posterYellow, posterBlack, frontZ,
                glassCenterY, glassHeight, glassTopY, doorCenterY, sideGlassWidth, leftGlassX, rightGlassX);

            GameObject interior = new GameObject("Mobile_Interior");
            interior.transform.SetParent(root.transform, false);
            BuildInterior(interior.transform, wallWhite, interiorFloor, counterBody, glassTop, phoneBody, boxBrown,
                yellow, black, led, frontZ, backZ, glassTopY, halfWidth);

            GameObject environment = new GameObject("Mobile_Environment");
            environment.transform.SetParent(root.transform, false);
            BuildEnvironment(environment.transform, sidewalk, black, frontZ, halfWidth);

            GameObject props = new GameObject("Mobile_ExteriorProps");
            props.transform.SetParent(root.transform, false);
            BuildExteriorProps(props.transform, yellow, black, frame, frontZ, halfWidth, fasciaY);

            MarkStatic(root);

            if (saveAsPrefab)
            {
                EnsureFolder("Assets/Art/Level1/Outdoor/Prefabs");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
            }

            return root;
        }

        private static void BuildStructure(Transform parent, Material wall, Material yellow, Material black, Material roof,
            float halfWidth, float halfDepth, float frontZ, float backZ, float fasciaY)
        {
            CreatePartCube("Mobile_Foundation", parent, new Vector3(0f, FoundationHeight * 0.5f, 0f),
                new Vector3(FacadeWidth + 0.06f, FoundationHeight, FacadeDepth + 0.06f), black);
            CreatePartCube("Mobile_Wall_Left", parent, new Vector3(-halfWidth + 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);
            CreatePartCube("Mobile_Wall_Right", parent, new Vector3(halfWidth - 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);
            CreatePartCube("Mobile_Wall_Back", parent, new Vector3(0f, FoundationHeight + WallHeight * 0.5f, backZ - 0.05f),
                new Vector3(FacadeWidth - 0.14f, WallHeight, 0.1f), wall);
            CreatePartCube("Mobile_Roof", parent,
                new Vector3(0f, FoundationHeight + WallHeight + RoofThickness * 0.5f, 0f),
                new Vector3(FacadeWidth + 0.1f, RoofThickness, FacadeDepth + 0.1f), roof);
            CreatePartCube("Mobile_Fascia", parent, new Vector3(0f, fasciaY, frontZ - 0.04f),
                new Vector3(FacadeWidth - 0.16f, FasciaHeight, 0.12f), yellow);
            CreatePartCube("Mobile_Fascia_Band", parent, new Vector3(0f, fasciaY - FasciaHeight * 0.5f + 0.03f, frontZ - 0.045f),
                new Vector3(FacadeWidth - 0.12f, 0.06f, 0.13f), black);
        }

        private static void BuildBannerSign(Transform parent, float fasciaY, float frontZ)
        {
            Material bannerMat = EnsureBannerMaterial();
            if (bannerMat == null)
            {
                BuildFallbackSign(parent, fasciaY, frontZ);
                return;
            }

            float signY = fasciaY + FasciaHeight * 0.5f + BannerHeight * 0.5f + 0.06f;
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "MobileStore_Banner";
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

        public static void ApplyBannerToMobileStore(Transform mobileRoot)
        {
            Transform sign = mobileRoot.Find("Mobile_Sign");
            if (sign == null)
            {
                Debug.LogError("[Level1TheGioiMobileStoreBuilder] Mobile_Sign not found.");
                return;
            }

            Object.DestroyImmediate(sign.gameObject);

            Transform structure = mobileRoot.Find("Mobile_Structure");
            Transform fascia = structure != null ? structure.Find("Mobile_Fascia") : null;
            Transform fasciaBand = structure != null ? structure.Find("Mobile_Fascia_Band") : null;
            if (fascia != null)
            {
                fascia.gameObject.SetActive(false);
            }

            if (fasciaBand != null)
            {
                fasciaBand.gameObject.SetActive(false);
            }

            float frontZ = -FacadeDepth * 0.5f;
            float fasciaY = FoundationHeight + WallHeight - FasciaHeight * 0.5f - 0.03f;

            GameObject signRoot = new GameObject("Mobile_Sign");
            signRoot.transform.SetParent(mobileRoot, false);
            BuildBannerSign(signRoot.transform, fasciaY, frontZ);
        }

        private static void BuildFallbackSign(Transform parent, float fasciaY, float frontZ)
        {
            Material yellow = EnsureMaterial("Mobile_Brand_Yellow.mat", YellowBrand, 0f, 0.42f);
            Material black = EnsureMaterial("Mobile_Brand_Black.mat", BlackBrand, 0.15f, 0.38f);
            BuildProceduralSign(parent, yellow, black, fasciaY, frontZ);
        }

        private static void BuildProceduralSign(Transform parent, Material yellow, Material black, float fasciaY, float frontZ)
        {
            float signY = fasciaY + FasciaHeight * 0.5f + 0.34f;
            CreatePartCube("Sign_Board", parent, new Vector3(0f, signY, frontZ - 0.05f),
                new Vector3(FacadeWidth - 0.1f, 0.72f, 0.05f), yellow);
            CreatePartCube("Sign_Band", parent, new Vector3(0f, signY - 0.3f, frontZ - 0.052f),
                new Vector3(FacadeWidth - 0.08f, 0.08f, 0.055f), black);

            GameObject iconRoot = new GameObject("Sign_PhoneIcon");
            iconRoot.transform.SetParent(parent, false);
            iconRoot.transform.localPosition = new Vector3(-halfWidthFromFacade() + 0.55f, signY + 0.05f, frontZ - 0.06f);
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Icon_Disc";
            disc.transform.SetParent(iconRoot.transform, false);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.transform.localScale = new Vector3(0.42f, 0.02f, 0.42f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            ApplyRenderer(disc.GetComponent<Renderer>(), yellow);
            CreatePartCube("Icon_Phone", iconRoot.transform, new Vector3(0f, 0.03f, 0f),
                new Vector3(0.12f, 0.2f, 0.03f), black);

            CreateSignLine(parent, "Sign_Title", "THẾ GIỚI MOBILE", signY + 0.1f, frontZ - 0.07f, 0.36f, BlackBrand,
                FontStyles.Bold);
            CreateSignLine(parent, "Sign_Subtitle", "Điện thoại • Laptop • Phụ kiện", signY - 0.14f, frontZ - 0.07f,
                0.15f, BlackBrand, FontStyles.Normal);
        }

        private static float halfWidthFromFacade()
        {
            return FacadeWidth * 0.5f;
        }

        private static void BuildStorefront(Transform parent, Material frame, Material clearGlass, Material yellow,
            Material black, Material posterYellow, Material posterBlack, float frontZ, float glassCenterY,
            float glassHeight, float glassTopY, float doorCenterY, float sideGlassWidth, float leftGlassX,
            float rightGlassX)
        {
            float frameZ = frontZ - 0.03f;
            CreatePartCube("Mobile_Glass_Left", parent, new Vector3(leftGlassX, glassCenterY, frontZ - 0.015f),
                new Vector3(sideGlassWidth, glassHeight, 0.03f), clearGlass);
            CreatePartCube("Mobile_Glass_Right", parent, new Vector3(rightGlassX, glassCenterY, frontZ - 0.015f),
                new Vector3(sideGlassWidth, glassHeight, 0.03f), clearGlass);
            CreatePartCube("Mobile_Glass_Door_Left", parent, new Vector3(-DoorWidth * 0.25f, doorCenterY, frontZ - 0.012f),
                new Vector3(DoorWidth * 0.5f - 0.04f, DoorHeight - 0.06f, 0.028f), clearGlass);
            CreatePartCube("Mobile_Glass_Door_Right", parent, new Vector3(DoorWidth * 0.25f, doorCenterY, frontZ - 0.012f),
                new Vector3(DoorWidth * 0.5f - 0.04f, DoorHeight - 0.06f, 0.028f), clearGlass);
            CreatePartCube("Mobile_Frame_Header", parent, new Vector3(0f, glassTopY + FrameThickness * 0.5f, frameZ),
                new Vector3(FacadeWidth - 0.18f, FrameThickness, 0.06f), frame);
            CreatePartCube("Mobile_Frame_Sill", parent, new Vector3(0f, GlassStartY - FrameThickness * 0.5f, frameZ),
                new Vector3(FacadeWidth - 0.18f, FrameThickness, 0.06f), frame);

            CreatePoster(parent, "Poster_Installment", "Trả góp 0%", leftGlassX, glassCenterY + 0.42f, frontZ - 0.028f,
                0.5f, 0.22f, posterYellow, BlackBrand);
            CreatePoster(parent, "Poster_TradeIn", "Thu cũ đổi mới", rightGlassX, glassCenterY + 0.15f, frontZ - 0.028f,
                0.52f, 0.22f, posterBlack, YellowBrand);
            CreatePoster(parent, "Poster_Repair", "Sửa chữa nhanh", rightGlassX, glassCenterY - 0.35f, frontZ - 0.028f,
                0.5f, 0.2f, posterYellow, BlackBrand);
        }

        private static void CreatePoster(Transform parent, string name, string text, float x, float y, float z,
            float width, float height, Material backing, Color textColor)
        {
            GameObject poster = new GameObject(name);
            poster.transform.SetParent(parent, false);
            poster.transform.localPosition = new Vector3(x, y, z);
            CreatePartCube("Backing", poster.transform, Vector3.zero, new Vector3(width, height, 0.01f), backing);
            CreateSignLine(poster.transform, "Text", text, 0f, -0.012f, height * 0.38f, textColor, FontStyles.Bold);
        }

        private static void BuildInterior(Transform parent, Material wall, Material floor, Material counterBody,
            Material glassTop, Material phoneBody, Material boxBrown, Material yellow, Material black, Material led,
            float frontZ, float backZ, float glassTopY, float halfWidth)
        {
            float interiorFrontZ = frontZ + 0.18f;
            float interiorBackZ = backZ - 0.16f;
            float interiorCenterZ = (interiorFrontZ + interiorBackZ) * 0.5f;
            float interiorDepth = interiorBackZ - interiorFrontZ;

            CreatePartCube("Interior_Floor", parent, new Vector3(0f, FoundationHeight + 0.02f, interiorCenterZ),
                new Vector3(FacadeWidth - 0.24f, 0.04f, interiorDepth), floor);
            CreatePartCube("Interior_Ceiling", parent, new Vector3(0f, glassTopY - 0.04f, interiorCenterZ),
                new Vector3(FacadeWidth - 0.24f, 0.05f, interiorDepth), wall);

            for (int i = 0; i < 3; i++)
            {
                float x = -0.95f + i * 0.95f;
                float z = interiorFrontZ + 0.55f;
                CreatePartCube("DisplayCounter_" + i, parent, new Vector3(x, FoundationHeight + 0.48f, z),
                    new Vector3(0.72f, 0.82f, 0.42f), counterBody);
                CreatePartCube("DisplayCounter_Glass_" + i, parent, new Vector3(x, FoundationHeight + 0.92f, z),
                    new Vector3(0.68f, 0.02f, 0.38f), glassTop);
                CreatePartCube("DisplayPhone_" + i, parent, new Vector3(x, FoundationHeight + 0.96f, z - 0.02f),
                    new Vector3(0.08f, 0.16f, 0.03f), phoneBody);
                CreatePartCube("DisplayPhone_Screen_" + i, parent, new Vector3(x, FoundationHeight + 1.02f, z - 0.035f),
                    new Vector3(0.06f, 0.1f, 0.008f), led);
            }

            CreatePartCube("Cashier_Counter", parent, new Vector3(0f, FoundationHeight + 0.78f, interiorBackZ - 0.35f),
                new Vector3(2.2f, 0.9f, 0.48f), yellow);
            CreatePartCube("Cashier_Monitor", parent, new Vector3(0.1f, FoundationHeight + 1.32f, interiorBackZ - 0.42f),
                new Vector3(0.38f, 0.26f, 0.05f), black);

            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    float sx = -halfWidth + 0.55f + col * 0.42f;
                    float sz = interiorBackZ - 0.08f - row * 0.28f;
                    CreatePartCube("ShelfBox_" + row + "_" + col, parent, new Vector3(sx, FoundationHeight + 1.05f + row * 0.35f, sz),
                        new Vector3(0.22f, 0.14f, 0.16f), boxBrown);
                }
            }

            CreatePartCube("WallHeadphone_L", parent, new Vector3(-halfWidth + 0.28f, FoundationHeight + 1.65f, interiorBackZ - 0.55f),
                new Vector3(0.14f, 0.14f, 0.06f), black);
            CreatePartCube("WallCharger_R", parent, new Vector3(halfWidth - 0.32f, FoundationHeight + 1.35f, interiorBackZ - 0.62f),
                new Vector3(0.18f, 0.12f, 0.08f), yellow);
            CreatePartCube("WallAccessory_R", parent, new Vector3(halfWidth - 0.32f, FoundationHeight + 1.05f, interiorBackZ - 0.62f),
                new Vector3(0.16f, 0.1f, 0.07f), boxBrown);

            CreatePartCube("Customer_Stool_A", parent, new Vector3(-0.75f, FoundationHeight + 0.28f, interiorFrontZ + 1.05f),
                new Vector3(0.3f, 0.04f, 0.3f), black);
            CreatePartCube("Customer_Stool_B", parent, new Vector3(0.75f, FoundationHeight + 0.28f, interiorFrontZ + 1.05f),
                new Vector3(0.3f, 0.04f, 0.3f), black);
            CreatePartCube("Staff_Chair", parent, new Vector3(0.55f, FoundationHeight + 0.42f, interiorBackZ - 0.75f),
                new Vector3(0.34f, 0.04f, 0.34f), black);

            for (int i = 0; i < 4; i++)
            {
                float lx = -1.35f + i * 0.9f;
                CreatePartCube("Ceiling_LED_" + i, parent, new Vector3(lx, glassTopY - 0.12f, interiorCenterZ),
                    new Vector3(0.55f, 0.04f, 0.12f), led);
            }
        }

        private static void BuildEnvironment(Transform parent, Material sidewalk, Material black, float frontZ,
            float halfWidth)
        {
            CreatePartCube("Env_Sidewalk", parent, new Vector3(0f, 0.04f, frontZ - 1.05f),
                new Vector3(FacadeWidth + 1.35f, 0.08f, 1.45f), sidewalk);

            GameObject address = new GameObject("Env_AddressPlaque");
            address.transform.SetParent(parent, false);
            address.transform.localPosition = new Vector3(-halfWidth + 0.48f, FoundationHeight + 1.48f, frontZ - 0.08f);
            CreateSignLine(address.transform, "Line1", "127/45 Đường Bùi Đình Túy", 0.08f, -0.03f, 0.08f, BlackBrand,
                FontStyles.Normal);
            CreateSignLine(address.transform, "Line2", "Phường 24, Quận Bình Thạnh, TP. Hồ Chí Minh", -0.08f, -0.03f,
                0.06f, BlackBrand, FontStyles.Normal);
        }

        private static void BuildExteriorProps(Transform parent, Material yellow, Material black, Material frame,
            float frontZ, float halfWidth, float fasciaY)
        {
            CreatePartCube("Prop_ABoard", parent, new Vector3(-halfWidth - 0.45f, 0.68f, frontZ - 0.55f),
                new Vector3(0.58f, 0.82f, 0.05f), yellow);
            CreateSignLine(parent, "ABoard_Text", "THẾ GIỚI\nMOBILE", 0.68f, frontZ - 0.58f, 0.11f, BlackBrand,
                FontStyles.Bold);
            CreatePartCube("Prop_CCTV", parent, new Vector3(0.12f, FoundationHeight + WallHeight - 0.1f, frontZ - 0.1f),
                new Vector3(0.12f, 0.08f, 0.1f), frame);
            CreatePartCube("Prop_AC", parent, new Vector3(halfWidth - 0.35f, FoundationHeight + WallHeight - 0.22f, frontZ + 0.35f),
                new Vector3(0.62f, 0.28f, 0.22f), whiteMaterial());
            CreatePartCube("Prop_AC_Vent", parent, new Vector3(halfWidth - 0.35f, FoundationHeight + WallHeight - 0.22f, frontZ + 0.48f),
                new Vector3(0.5f, 0.18f, 0.04f), black);
        }

        private static Material whiteMaterial()
        {
            return EnsureMaterial("Mobile_AC_White.mat", WhiteTrim, 0f, 0.3f);
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
            tmp.rectTransform.sizeDelta = new Vector2(4.2f, 0.55f);
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
            if (baseColor.a < 0.99f)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

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

        private static Material EnsureClearGlassMaterial()
        {
            string path = MaterialsRoot + "/Mobile_Glass_Clear.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = "Mobile_Glass_Clear";
                AssetDatabase.CreateAsset(mat, path);
            }

            Color tint = new Color(0.94f, 0.96f, 0.98f, 0.14f);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_Color", tint);
            mat.SetFloat("_Metallic", 0.05f);
            mat.SetFloat("_Smoothness", 0.9f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureBannerMaterial()
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
            if (texture == null)
            {
                Debug.LogError("[Level1TheGioiMobileStoreBuilder] Missing banner texture: " + BannerTexturePath);
                return null;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(BannerMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_MobileStore_TheGioiMobile";
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
