#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds a humble open-front Vietnamese cơm tấm street eatery prefab.</summary>
    public static class Level1VietnameseComTamShopBuilder
    {
        public const string RootName = "PF_ComTamShop";
        public const string PrefabPath = "Assets/Art/Level1/Outdoor/Prefabs/PF_ComTamShop.prefab";
        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials/Level1";
        private const string MetalRoofMaterialPath = "Assets/Settings/ChoTet/Materials/Roof_Metal_Corrugated.mat";
        private const string SidewalkMaterialPath = "Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat";
        private const string BannerTexturePath = "Assets/Art/Level1/Outdoor/Textures/ComTam_BaHung_Banner.png";
        private const string BannerMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_ComTam_BaHung.mat";

        // Wide along street frontage (local X), shallow depth into block (local Z).
        private const float ShopWidth = 5.85f;
        private const float ShopDepth = 2.75f;
        private const float WallHeight = 2.55f;
        private const float WallThickness = 0.1f;
        private const float TileHeight = 1.05f;
        private const float AwningDepth = 0.75f;
        private const float AwningHeight = 0.06f;
        private const float FloorThickness = 0.07f;
        private const float CeilingThickness = 0.06f;
        private const float RoofThickness = 0.09f;
        private const float FoundationLift = 0.12f;
        private const float AlleyGap = 1.25f;
        private const float SidewalkDepth = 1.05f;

        private static readonly Color CreamWall = new Color(0.94f, 0.9f, 0.82f);
        private static readonly Color GreenTile = new Color(0.2f, 0.46f, 0.34f);
        private static readonly Color WarmFluoro = new Color(1f, 0.95f, 0.8f);
        private static readonly Color MenuInk = new Color(0.12f, 0.1f, 0.08f);

        private const float BannerAspect = 682f / 1024f;
        private const float BannerPanelHeight = 1.62f;
        private const float BannerPanelWidth = BannerPanelHeight * BannerAspect;
        private const float TableTopThickness = 0.04f;
        private const float TableTopY = 0.52f;
        private static readonly float TableSurfaceY = TableTopY + TableTopThickness * 0.5f;

        public static void BuildPrefabAsset()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseComTamShopBuilder] Exit Play mode before building the prefab.");
                return;
            }

            GameObject root = BuildAt(Vector3.zero, Quaternion.identity, saveAsPrefab: true);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log("[Level1VietnameseComTamShopBuilder] Prefab saved: " + PrefabPath);
        }

        public static void PlaceBesideCarWashOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseComTamShopBuilder] Exit Play mode before placing the shop.");
                return;
            }

            GameObject wash = GameObject.Find(Level1VietnameseCarWashBuilder.RootName);
            if (wash == null)
            {
                Debug.LogError("[Level1VietnameseComTamShopBuilder] PF_Vietnamese_CarWash not found.");
                return;
            }

            RemoveExistingInScene();
            Transform parent = FindOutdoorParent();
            if (!TryResolvePlacementBesideCarWash(wash.transform, out Vector3 position, out Quaternion rotation))
            {
                Debug.LogError("[Level1VietnameseComTamShopBuilder] Could not resolve placement beside car wash.");
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
            Debug.Log("[Level1VietnameseComTamShopBuilder] Com tam shop placed beside car wash at " + position);
        }

        public static bool TryResolvePlacementBesideCarWash(Transform carWash, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = carWash.rotation;

            Bounds washBounds = ComputeWorldBounds(carWash.gameObject);
            if (washBounds.size.sqrMagnitude < 0.01f)
            {
                return false;
            }

            Vector3 rowAxis = carWash.TransformDirection(Vector3.right).normalized;
            float halfAlongRow = ShopWidth * 0.5f;
            float washHalfAlongRow = ProjectedHalfExtent(washBounds, rowAxis);
            Vector3 rowEdge = washBounds.center + rowAxis * washHalfAlongRow;
            Vector3 center = rowEdge + rowAxis * (AlleyGap + halfAlongRow);
            center.y = Level1OutdoorSettings.WalkSurfaceY + FoundationLift;
            position = center;
            return true;
        }

        public static GameObject BuildAt(Vector3 worldPosition, Quaternion rotation, bool saveAsPrefab)
        {
            Material cream = EnsureMaterial("ComTam_Wall_Cream.mat", CreamWall, 0f, 0.18f);
            Material tile = EnsureMaterial("ComTam_Tile_Green.mat", GreenTile, 0f, 0.42f);
            Material stainless = EnsureMaterial("ComTam_Stainless.mat", new Color(0.76f, 0.78f, 0.8f), 0.55f, 0.72f);
            Material concrete = EnsureMaterial("ComTam_Floor_Concrete.mat", new Color(0.68f, 0.7f, 0.72f), 0f, 0.24f);
            Material metalRoof = AssetDatabase.LoadAssetAtPath<Material>(MetalRoofMaterialPath)
                ?? EnsureMaterial("ComTam_Awning_Metal.mat", new Color(0.4f, 0.44f, 0.48f), 0.35f, 0.36f);
            Material wood = EnsureMaterial("ComTam_Wood_Dark.mat", new Color(0.42f, 0.3f, 0.2f), 0f, 0.28f);
            Material redPlastic = EnsureMaterial("ComTam_Stool_Red.mat", new Color(0.86f, 0.18f, 0.14f), 0f, 0.45f);
            Material bluePlastic = EnsureMaterial("ComTam_Stool_Blue.mat", new Color(0.14f, 0.38f, 0.78f), 0f, 0.45f);
            Material rice = EnsureMaterial("ComTam_Rice.mat", new Color(0.95f, 0.92f, 0.78f), 0f, 0.2f);
            Material pork = EnsureMaterial("ComTam_Pork.mat", new Color(0.62f, 0.36f, 0.22f), 0f, 0.3f);
            Material fluoro = EnsureUnlitMaterial("ComTam_Fluoro_Warm.mat", WarmFluoro);
            Material stain = EnsureUnlitMaterial("ComTam_OilStain.mat", new Color(0.18f, 0.16f, 0.12f, 0.35f));
            Material signFrame = EnsureMaterial("ComTam_SignFrame_Black.mat", new Color(0.06f, 0.06f, 0.07f), 0.4f, 0.45f);
            Material trashGreen = EnsureMaterial("ComTam_Trash_Green.mat", new Color(0.22f, 0.42f, 0.24f), 0f, 0.3f);
            Material plant = EnsureMaterial("ComTam_Plant.mat", new Color(0.2f, 0.5f, 0.22f), 0f, 0.25f);
            Material sidewalk = AssetDatabase.LoadAssetAtPath<Material>(SidewalkMaterialPath)
                ?? EnsureMaterial("ComTam_Sidewalk.mat", new Color(0.6f, 0.62f, 0.64f), 0f, 0.2f);

            float halfWidth = ShopWidth * 0.5f;
            float halfDepth = ShopDepth * 0.5f;
            float frontZ = -halfDepth;
            float backZ = halfDepth;

            GameObject root = new GameObject(RootName);
            root.transform.position = worldPosition;
            root.transform.rotation = rotation;

            GameObject structure = new GameObject("ComTam_Structure");
            structure.transform.SetParent(root.transform, false);
            BuildStructure(structure.transform, cream, tile, metalRoof, concrete, halfWidth, halfDepth, frontZ, backZ);

            GameObject interior = new GameObject("ComTam_Interior");
            interior.transform.SetParent(root.transform, false);
            BuildInterior(interior.transform, stainless, cream, wood, rice, pork, fluoro, halfWidth, frontZ, backZ);

            GameObject frontArea = new GameObject("ComTam_FrontArea");
            frontArea.transform.SetParent(root.transform, false);
            BuildFrontArea(frontArea.transform, stainless, redPlastic, bluePlastic, rice, pork, halfWidth, frontZ);

            GameObject wallProps = new GameObject("ComTam_WallProps");
            wallProps.transform.SetParent(root.transform, false);
            BuildWallProps(wallProps.transform, cream, wood, fluoro, halfWidth, frontZ, backZ);

            GameObject environment = new GameObject("ComTam_Environment");
            environment.transform.SetParent(root.transform, false);
            BuildEnvironment(environment.transform, sidewalk, stain, plant, trashGreen, halfWidth, frontZ);

            GameObject sign = new GameObject("ComTam_FreestandingSign");
            sign.transform.SetParent(root.transform, false);
            BuildFreestandingSign(sign.transform, signFrame, halfWidth, frontZ);

            MarkStatic(root);

            if (saveAsPrefab)
            {
                EnsureFolder("Assets/Art/Level1/Outdoor/Prefabs");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
            }

            return root;
        }

        private static void BuildStructure(Transform parent, Material cream, Material tile, Material awning,
            Material floor, float halfWidth, float halfDepth, float frontZ, float backZ)
        {
            CreatePartCube("ComTam_Floor", parent, new Vector3(0f, FloorThickness * 0.5f, 0f),
                new Vector3(ShopWidth + 0.04f, FloorThickness, ShopDepth), floor);
            CreatePartCube("ComTam_Wall_Left", parent,
                new Vector3(-halfWidth + WallThickness * 0.5f, WallHeight * 0.5f, 0f),
                new Vector3(WallThickness, WallHeight, ShopDepth), cream);
            CreatePartCube("ComTam_Wall_Right", parent,
                new Vector3(halfWidth - WallThickness * 0.5f, WallHeight * 0.5f, 0f),
                new Vector3(WallThickness, WallHeight, ShopDepth), cream);
            CreatePartCube("ComTam_Wall_Back", parent,
                new Vector3(0f, WallHeight * 0.5f, backZ - WallThickness * 0.5f),
                new Vector3(ShopWidth, WallHeight, WallThickness), cream);

            float tileCenterY = TileHeight * 0.5f + FloorThickness;
            CreatePartCube("ComTam_Tile_Left", parent,
                new Vector3(-halfWidth + WallThickness + 0.02f, tileCenterY, 0f),
                new Vector3(0.04f, TileHeight, ShopDepth - 0.08f), tile);
            CreatePartCube("ComTam_Tile_Right", parent,
                new Vector3(halfWidth - WallThickness - 0.02f, tileCenterY, 0f),
                new Vector3(0.04f, TileHeight, ShopDepth - 0.08f), tile);
            CreatePartCube("ComTam_Tile_Back", parent,
                new Vector3(0f, tileCenterY, backZ - WallThickness - 0.02f),
                new Vector3(ShopWidth - 0.12f, TileHeight, 0.04f), tile);

            // Interior ceiling slab (look-up surface) + exterior corrugated metal roof.
            float ceilingY = WallHeight - CeilingThickness * 0.5f;
            CreatePartCube("ComTam_Ceiling", parent,
                new Vector3(0f, ceilingY, 0f),
                new Vector3(ShopWidth - WallThickness * 2f + 0.02f, CeilingThickness, ShopDepth - WallThickness + 0.02f),
                cream);
            float roofY = WallHeight + RoofThickness * 0.5f;
            CreatePartCube("ComTam_Roof", parent,
                new Vector3(0f, roofY, 0.04f),
                new Vector3(ShopWidth + 0.16f, RoofThickness, ShopDepth + 0.18f), awning);

            float awningZ = frontZ - AwningDepth * 0.5f + 0.02f;
            CreatePartCube("ComTam_Awning", parent,
                new Vector3(0f, WallHeight - AwningHeight * 0.5f - 0.02f, awningZ),
                new Vector3(ShopWidth + 0.22f, AwningHeight, AwningDepth), awning);
            CreatePartCube("ComTam_Awning_Lip", parent,
                new Vector3(0f, WallHeight - AwningHeight - 0.05f, frontZ - AwningDepth + 0.02f),
                new Vector3(ShopWidth + 0.18f, 0.04f, 0.05f), awning);
        }

        public static void AttachBannerOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseComTamShopBuilder] Exit Play mode before attaching the banner.");
                return;
            }

            GameObject shop = GameObject.Find(RootName);
            if (shop == null)
            {
                Debug.LogError("[Level1VietnameseComTamShopBuilder] PF_ComTamShop not found in scene.");
                return;
            }

            ApplyBannerToFreestandingSign(shop.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnameseComTamShopBuilder] Com tam banner attached.");
        }

        public static void ApplyBannerToFreestandingSign(Transform shopRoot)
        {
            Transform sign = shopRoot.Find("ComTam_FreestandingSign");
            if (sign == null)
            {
                Debug.LogError("[Level1VietnameseComTamShopBuilder] ComTam_FreestandingSign not found.");
                return;
            }

            for (int i = sign.childCount - 1; i >= 0; i--)
            {
                Transform child = sign.GetChild(i);
                if (child.name.StartsWith("Sign_"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            Material frame = EnsureMaterial("ComTam_SignFrame_Black.mat", new Color(0.06f, 0.06f, 0.07f), 0.4f, 0.45f);
            float frontZ = -ShopDepth * 0.5f;
            BuildFreestandingSign(sign, frame, ShopWidth * 0.5f, frontZ);
        }

        private static void BuildInterior(Transform parent, Material stainless, Material cream, Material wood,
            Material rice, Material pork, Material fluoro, float halfWidth, float frontZ, float backZ)
        {
            float innerFrontZ = frontZ + 0.22f;
            float innerBackZ = backZ - 0.18f;
            float counterTopY = 0.86f;
            float displayTopY = 1.105f;

            CreatePartCube("Food_DisplayCabinet", parent,
                new Vector3(-halfWidth + 0.58f, 0.62f, innerFrontZ + 0.18f),
                new Vector3(1.05f, 0.95f, 0.38f), stainless);
            CreatePartCube("Food_DisplayGlass", parent,
                new Vector3(-halfWidth + 0.58f, 1.08f, innerFrontZ + 0.18f),
                new Vector3(0.98f, 0.04f, 0.34f), EnsureUnlitMaterial("ComTam_Glass_Tint.mat",
                    new Color(0.9f, 0.94f, 0.96f, 0.25f)));
            PlacePlateOnSurface(parent, "DisplayPlate_0", -halfWidth + 0.45f, displayTopY, innerFrontZ + 0.18f, rice, pork);
            PlacePlateOnSurface(parent, "DisplayPlate_1", -halfWidth + 0.72f, displayTopY, innerFrontZ + 0.18f, rice, pork);

            CreatePartCube("Rice_Cooker", parent, new Vector3(halfWidth - 0.5f, 0.42f, innerFrontZ + 0.2f),
                new Vector3(0.34f, 0.36f, 0.34f), stainless);
            CreatePartCube("Rice_Cooker_Lid", parent, new Vector3(halfWidth - 0.5f, 0.64f, innerFrontZ + 0.2f),
                new Vector3(0.3f, 0.05f, 0.3f), stainless);

            CreatePartCube("Soup_Pot", parent, new Vector3(halfWidth - 0.95f, 0.38f, innerFrontZ + 0.22f),
                new Vector3(0.36f, 0.28f, 0.36f), stainless);
            CreatePartCube("Prep_Counter", parent, new Vector3(0f, counterTopY - 0.36f, innerBackZ - 0.12f),
                new Vector3(ShopWidth - 0.55f, 0.72f, 0.34f), stainless);
            PlacePlateOnSurface(parent, "PrepPlate_0", -0.35f, counterTopY, innerBackZ - 0.12f, rice, pork);

            float fluoroY = WallHeight - CeilingThickness - 0.08f;
            for (int i = 0; i < 4; i++)
            {
                float lx = -1.35f + i * 0.9f;
                CreatePartCube("Ceiling_Fluoro_" + i, parent,
                    new Vector3(lx, fluoroY, (innerFrontZ + innerBackZ) * 0.5f),
                    new Vector3(0.62f, 0.04f, 0.1f), fluoro);
            }

            CreatePartCube("Shelf_Back", parent, new Vector3(0f, 1.02f, innerBackZ - 0.06f),
                new Vector3(ShopWidth - 0.7f, 0.06f, 0.16f), wood);
        }

        private static void PlacePlateOnSurface(Transform parent, string name, float x, float surfaceY, float z,
            Material rice, Material pork)
        {
            const float plateThickness = 0.025f;
            const float porkThickness = 0.018f;
            float plateY = surfaceY + plateThickness * 0.5f;
            CreatePartCube(name, parent, new Vector3(x, plateY, z),
                new Vector3(0.2f, plateThickness, 0.16f), rice);
            CreatePartCube(name + "_Pork", parent, new Vector3(x + 0.03f, plateY + plateThickness * 0.5f + porkThickness * 0.5f, z),
                new Vector3(0.09f, porkThickness, 0.06f), pork);
        }

        private static void BuildFrontArea(Transform parent, Material stainless, Material red, Material blue,
            Material rice, Material pork, float halfWidth, float frontZ)
        {
            float tableZ = frontZ + 0.42f;
            float[] tableX = { -1.45f, -0.35f, 0.85f, 1.95f };
            for (int i = 0; i < tableX.Length; i++)
            {
                float tx = tableX[i];
                if (Mathf.Abs(tx) > halfWidth - 0.35f)
                {
                    continue;
                }

                CreatePartCube("Table_" + i, parent, new Vector3(tx, TableTopY, tableZ),
                    new Vector3(0.58f, TableTopThickness, 0.38f), stainless);
                CreatePartCube("Table_Leg_L_" + i, parent, new Vector3(tx - 0.22f, 0.26f, tableZ - 0.12f),
                    new Vector3(0.04f, 0.48f, 0.04f), stainless);
                CreatePartCube("Table_Leg_R_" + i, parent, new Vector3(tx + 0.22f, 0.26f, tableZ - 0.12f),
                    new Vector3(0.04f, 0.48f, 0.04f), stainless);

                if (i < 2)
                {
                    PlacePlateOnSurface(parent, "TablePlate_" + i, tx, TableSurfaceY, tableZ, rice, pork);
                }

                Material stoolMat = i % 2 == 0 ? red : blue;
                float stoolY = FloorThickness + 0.015f;
                CreatePartCube("Stool_" + i + "_A", parent, new Vector3(tx - 0.16f, stoolY, tableZ + 0.34f),
                    new Vector3(0.26f, 0.03f, 0.26f), stoolMat);
                CreatePartCube("Stool_" + i + "_B", parent, new Vector3(tx + 0.16f, stoolY, tableZ + 0.34f),
                    new Vector3(0.26f, 0.03f, 0.26f), stoolMat == red ? blue : red);
            }

            float condimentY = TableSurfaceY + 0.07f;
            CreatePartCube("Chopstick_Holder", parent, new Vector3(-halfWidth + 0.42f, condimentY, tableZ - 0.02f),
                new Vector3(0.08f, 0.12f, 0.08f), stainless);
            CreatePartCube("FishSauce_A", parent, new Vector3(halfWidth - 0.42f, condimentY, tableZ),
                new Vector3(0.05f, 0.14f, 0.05f), EnsureMaterial("ComTam_FishSauce.mat",
                    new Color(0.72f, 0.48f, 0.12f), 0f, 0.5f));
            CreatePartCube("FishSauce_B", parent, new Vector3(halfWidth - 0.32f, condimentY, tableZ),
                new Vector3(0.05f, 0.14f, 0.05f), EnsureMaterial("ComTam_FishSauce_Dark.mat",
                    new Color(0.28f, 0.16f, 0.08f), 0f, 0.45f));
            CreatePartCube("Tissue_Box", parent, new Vector3(0.25f, TableSurfaceY + 0.03f, tableZ - 0.06f),
                new Vector3(0.14f, 0.06f, 0.1f), EnsureMaterial("ComTam_Tissue.mat", new Color(0.96f, 0.96f, 0.98f), 0f, 0.2f));
        }

        private static void BuildWallProps(Transform parent, Material cream, Material wood, Material fluoro,
            float halfWidth, float frontZ, float backZ)
        {
            GameObject menuBoard = new GameObject("Menu_Board");
            menuBoard.transform.SetParent(parent, false);
            menuBoard.transform.localPosition = new Vector3(-halfWidth + 0.12f, 1.55f, backZ - 0.18f);
            CreatePartCube("Board", menuBoard.transform, Vector3.zero, new Vector3(0.04f, 0.75f, 0.55f), wood);
            CreateSignLine(menuBoard.transform, "Menu_Title", "BẢNG GIÁ", 0.22f, -0.03f, 0.1f, MenuInk, FontStyles.Bold);
            CreateSignLine(menuBoard.transform, "Menu_Line1", "Cơm sườn  35k", 0.05f, -0.03f, 0.07f, MenuInk, FontStyles.Normal);
            CreateSignLine(menuBoard.transform, "Menu_Line2", "Cơm bì   30k", -0.08f, -0.03f, 0.07f, MenuInk, FontStyles.Normal);
            CreateSignLine(menuBoard.transform, "Menu_Line3", "Cơm chả   32k", -0.21f, -0.03f, 0.07f, MenuInk, FontStyles.Normal);

            CreatePartCube("Wall_Fan_Base", parent, new Vector3(halfWidth - 0.18f, 1.75f, frontZ + 0.65f),
                new Vector3(0.06f, 0.06f, 0.06f), cream);
            CreatePartCube("Wall_Fan_Head", parent, new Vector3(halfWidth - 0.18f, 1.75f, frontZ + 0.78f),
                new Vector3(0.28f, 0.28f, 0.04f), EnsureMaterial("ComTam_Fan_Grill.mat", new Color(0.3f, 0.32f, 0.34f), 0.2f, 0.4f));

            GameObject calendar = new GameObject("Wall_Calendar");
            calendar.transform.SetParent(parent, false);
            calendar.transform.localPosition = new Vector3(halfWidth - 0.14f, 1.35f, backZ - 0.16f);
            CreatePartCube("Cal_Backing", calendar.transform, Vector3.zero, new Vector3(0.03f, 0.22f, 0.18f),
                EnsureUnlitMaterial("ComTam_Calendar.mat", new Color(0.92f, 0.88f, 0.78f)));
            CreateSignLine(calendar.transform, "Cal_Month", "THÁNG 7", 0.05f, -0.02f, 0.05f, MenuInk, FontStyles.Bold);
        }

        private static void BuildEnvironment(Transform parent, Material sidewalk, Material stain, Material plant,
            Material trash, float halfWidth, float frontZ)
        {
            float sidewalkCenterZ = frontZ - SidewalkDepth * 0.5f - 0.08f;
            CreatePartCube("Env_Sidewalk", parent, new Vector3(0f, 0.04f, sidewalkCenterZ),
                new Vector3(ShopWidth + 1.1f, 0.08f, SidewalkDepth), sidewalk);
            CreatePartCube("Env_Stain_A", parent, new Vector3(-0.35f, 0.045f, sidewalkCenterZ + 0.12f),
                new Vector3(0.35f, 0.01f, 0.28f), stain);
            CreatePartCube("Env_Stain_B", parent, new Vector3(0.42f, 0.045f, sidewalkCenterZ - 0.18f),
                new Vector3(0.22f, 0.01f, 0.2f), stain);
            CreatePartCube("Env_WornPatch", parent, new Vector3(0f, 0.043f, sidewalkCenterZ),
                new Vector3(0.5f, 0.008f, 0.4f), EnsureMaterial("ComTam_WornConcrete.mat",
                    new Color(0.55f, 0.57f, 0.58f), 0f, 0.15f));

            CreatePartCube("Env_Plant_Pot", parent, new Vector3(-halfWidth - 0.22f, 0.18f, sidewalkCenterZ + 0.15f),
                new Vector3(0.22f, 0.22f, 0.22f), EnsureMaterial("ComTam_Pot_Terracotta.mat",
                    new Color(0.58f, 0.34f, 0.22f), 0f, 0.25f));
            CreatePartCube("Env_Plant", parent, new Vector3(-halfWidth - 0.22f, 0.38f, sidewalkCenterZ + 0.15f),
                new Vector3(0.28f, 0.22f, 0.28f), plant);

            CreatePartCube("Env_TrashBin", parent, new Vector3(halfWidth + 0.28f, 0.32f, sidewalkCenterZ - 0.1f),
                new Vector3(0.28f, 0.48f, 0.28f), trash);
        }

        private static void BuildFreestandingSign(Transform parent, Material frame, float halfWidth, float frontZ)
        {
            float signZ = frontZ - SidewalkDepth * 0.58f;
            parent.localPosition = new Vector3(-0.55f, 0f, signZ);

            float panelW = BannerPanelWidth;
            float panelH = BannerPanelHeight;
            float frameT = 0.04f;
            float legH = 1.02f;

            CreatePartCube("Sign_Frame_Top", parent, new Vector3(0f, legH + panelH * 0.5f + frameT * 0.5f, 0f),
                new Vector3(panelW + frameT * 2f, frameT, frameT), frame);
            CreatePartCube("Sign_Frame_Bottom", parent, new Vector3(0f, legH + frameT * 0.5f, 0f),
                new Vector3(panelW + frameT * 2f, frameT, frameT), frame);
            CreatePartCube("Sign_Frame_Left", parent, new Vector3(-panelW * 0.5f - frameT * 0.5f, legH + panelH * 0.5f, 0f),
                new Vector3(frameT, panelH, frameT), frame);
            CreatePartCube("Sign_Frame_Right", parent, new Vector3(panelW * 0.5f + frameT * 0.5f, legH + panelH * 0.5f, 0f),
                new Vector3(frameT, panelH, frameT), frame);

            Material bannerMat = EnsureBannerMaterial();
            if (bannerMat != null)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "ComTam_Banner";
                quad.transform.SetParent(parent, false);
                quad.transform.localPosition = new Vector3(0f, legH + panelH * 0.5f, -0.01f);
                quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                quad.transform.localScale = new Vector3(panelW - 0.05f, panelH - 0.06f, 1f);
                Object.DestroyImmediate(quad.GetComponent<Collider>());

                Material instance = new Material(bannerMat);
                instance.SetInt("_Cull", 0);
                instance.renderQueue = (int)RenderQueue.Geometry + 10;
                ApplyRenderer(quad.GetComponent<Renderer>(), instance);
            }

            CreatePartCube("Sign_Leg_L", parent, new Vector3(-panelW * 0.32f, legH * 0.5f, 0f),
                new Vector3(0.05f, legH, 0.05f), frame);
            CreatePartCube("Sign_Leg_R", parent, new Vector3(panelW * 0.32f, legH * 0.5f, 0f),
                new Vector3(0.05f, legH, 0.05f), frame);
            CreatePartCube("Sign_Base_L", parent, new Vector3(-panelW * 0.32f, 0.025f, 0f),
                new Vector3(0.2f, 0.05f, 0.12f), frame);
            CreatePartCube("Sign_Base_R", parent, new Vector3(panelW * 0.32f, 0.025f, 0f),
                new Vector3(0.2f, 0.05f, 0.12f), frame);
        }

        private static Material EnsureBannerMaterial()
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
            if (texture == null)
            {
                Debug.LogError("[Level1VietnameseComTamShopBuilder] Missing banner texture: " + BannerTexturePath);
                return null;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(BannerMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_ComTam_BaHung";
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
            tmp.rectTransform.sizeDelta = new Vector2(2.4f, 0.35f);
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

        private static float ProjectedHalfExtent(Bounds bounds, Vector3 axis)
        {
            Vector3 extents = bounds.extents;
            Vector3 absAxis = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
            return Vector3.Dot(extents, absAxis);
        }

        private static Bounds ComputeWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
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
            if (baseColor.a < 0.99f)
            {
                mat.SetFloat("_Surface", 1f);
                mat.renderQueue = 3000;
            }

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
