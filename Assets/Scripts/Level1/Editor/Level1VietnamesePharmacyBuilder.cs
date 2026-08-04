#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds a low-poly Vietnamese neighborhood pharmacy facing the main road.</summary>
    public static class Level1VietnamesePharmacyBuilder
    {
        public const string RootName = "PF_Pharmacy";
        public const string PrefabPath = "Assets/Art/Level1/Outdoor/Prefabs/PF_Pharmacy.prefab";
        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials/Level1";
        private const string SidewalkMaterialPath = "Assets/Materials/Highway/QuaterniusSidewalk_Concrete.mat";
        private const string BannerTexturePath = "Assets/Art/Level1/Outdoor/Textures/Pharmacy_MinhPhat_Banner.png";
        private const string BannerMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_Pharmacy_MinhPhat.mat";

        private const float FacadeWidth = 4.35f;
        private const float FacadeDepth = 2.55f;
        private const float BannerWidth = FacadeWidth - 0.08f;
        private const float BannerHeight = BannerWidth * 0.5f;
        private const float WallHeight = 3.05f;
        private const float FoundationHeight = 0.12f;
        private const float RoofThickness = 0.1f;
        private const float SignHeight = BannerHeight;
        private const float CanopyDepth = 0.55f;
        private const float CanopyThickness = 0.05f;
        private const float DoorWidth = 1.55f;
        private const float DoorHeight = 2.25f;
        private const float FrameThickness = 0.05f;
        private const float GlassStartY = FoundationHeight + 0.05f;
        private const float FoundationLift = 0.12f;
        private const float AlleyGap = 1.0f;
        private const float SidewalkDepth = 1.05f;

        private static readonly Color PharmaBlue = new Color(0.1f, 0.4f, 0.78f);
        private static readonly Color PharmaWhite = new Color(0.96f, 0.98f, 1f);
        private static readonly Color PharmaGreen = new Color(0.16f, 0.6f, 0.34f);

        public static void AttachBannerOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamesePharmacyBuilder] Exit Play mode before attaching the banner.");
                return;
            }

            GameObject pharmacy = GameObject.Find(RootName);
            if (pharmacy == null)
            {
                Debug.LogError("[Level1VietnamesePharmacyBuilder] PF_Pharmacy not found in scene.");
                return;
            }

            ApplyBannerToPharmacy(pharmacy.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnamesePharmacyBuilder] Pharmacy banner attached.");
        }

        public static void ApplyBannerToPharmacy(Transform pharmacyRoot)
        {
            Transform sign = pharmacyRoot.Find("Pharmacy_Sign");
            if (sign == null)
            {
                Debug.LogError("[Level1VietnamesePharmacyBuilder] Pharmacy_Sign not found.");
                return;
            }

            Object.DestroyImmediate(sign.gameObject);
            float frontZ = -FacadeDepth * 0.5f;
            float signBottomY = FoundationHeight + WallHeight - SignHeight - 0.04f;

            GameObject signRoot = new GameObject("Pharmacy_Sign");
            signRoot.transform.SetParent(pharmacyRoot, false);
            BuildBannerSign(signRoot.transform, signBottomY, frontZ);
        }

        public static void BuildPrefabAsset()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamesePharmacyBuilder] Exit Play mode before building the prefab.");
                return;
            }

            GameObject root = BuildAt(Vector3.zero, Quaternion.Euler(0f, -90f, 0f), saveAsPrefab: true);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log("[Level1VietnamesePharmacyBuilder] Prefab saved: " + PrefabPath);
        }

        public static void PlaceBesideMobileStoreOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamesePharmacyBuilder] Exit Play mode before placing the pharmacy.");
                return;
            }

            GameObject mobile = GameObject.Find(Level1TheGioiMobileStoreBuilder.RootName);
            if (mobile == null)
            {
                Debug.LogError("[Level1VietnamesePharmacyBuilder] PF_TheGioiMobile_Store not found.");
                return;
            }

            RemoveExistingInScene();
            Transform parent = FindOutdoorParent();
            if (!TryResolvePlacementBesideMobileStore(mobile.transform, out Vector3 position, out Quaternion rotation))
            {
                Debug.LogError("[Level1VietnamesePharmacyBuilder] Could not resolve placement beside mobile store.");
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
            Debug.Log("[Level1VietnamesePharmacyBuilder] Pharmacy placed beside mobile store at " + position);
        }

        public static void FixFacingRoadOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamesePharmacyBuilder] Exit Play mode before fixing pharmacy orientation.");
                return;
            }

            GameObject pharmacy = GameObject.Find(RootName);
            if (pharmacy == null)
            {
                Debug.LogError("[Level1VietnamesePharmacyBuilder] PF_Pharmacy not found in scene.");
                return;
            }

            GameObject mobile = GameObject.Find(Level1TheGioiMobileStoreBuilder.RootName);
            Quaternion targetRotation = mobile != null
                ? mobile.transform.rotation
                : Quaternion.Euler(0f, -90f, 0f);
            pharmacy.transform.rotation = targetRotation;
            AlignXWithNeighbor(pharmacy.transform, mobile != null ? mobile.transform : null);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnamesePharmacyBuilder] Pharmacy rotated to face the main road.");
        }

        public static bool TryResolvePlacementBesideMobileStore(Transform mobileStore, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = mobileStore.rotation;

            Bounds mobileBounds = ComputeWorldBounds(mobileStore.gameObject);
            if (mobileBounds.size.sqrMagnitude < 0.01f)
            {
                return false;
            }

            Vector3 rowAxis = ResolveStreetRowAxis(mobileStore);
            float halfAlongRow = FacadeWidth * 0.5f;
            float mobileHalfAlongRow = ProjectedHalfExtent(mobileBounds, rowAxis);
            Vector3 rowEdge = mobileBounds.center + rowAxis * mobileHalfAlongRow;
            Vector3 center = rowEdge + rowAxis * (AlleyGap + halfAlongRow);
            center.y = Level1OutdoorSettings.WalkSurfaceY + FoundationLift;
            center.x = mobileStore.position.x;
            position = center;
            rotation = mobileStore.rotation;
            return true;
        }

        private static Vector3 ResolveStreetRowAxis(Transform anchor)
        {
            Transform carWash = GameObject.Find(Level1VietnameseCarWashBuilder.RootName)?.transform;
            if (carWash != null)
            {
                return carWash.TransformDirection(Vector3.right).normalized;
            }

            return anchor.TransformDirection(Vector3.right).normalized;
        }

        private static void AlignXWithNeighbor(Transform pharmacy, Transform neighbor)
        {
            if (neighbor == null)
            {
                return;
            }

            Vector3 pos = pharmacy.position;
            pos.x = neighbor.position.x;
            pharmacy.position = pos;
        }

        public static GameObject BuildAt(Vector3 worldPosition, Quaternion rotation, bool saveAsPrefab)
        {
            Material wallWhite = EnsureMaterial("Pharmacy_Wall_White.mat", PharmaWhite, 0f, 0.22f);
            Material wallBlue = EnsureMaterial("Pharmacy_Wall_Blue.mat", PharmaBlue, 0f, 0.35f);
            Material green = EnsureMaterial("Pharmacy_Brand_Green.mat", PharmaGreen, 0f, 0.4f);
            Material roof = EnsureMaterial("Pharmacy_Roof.mat", new Color(0.88f, 0.9f, 0.92f), 0.08f, 0.28f);
            Material frame = EnsureMaterial("Pharmacy_Frame_Blue.mat", new Color(0.08f, 0.32f, 0.62f), 0.3f, 0.5f);
            Material clearGlass = EnsureClearGlassMaterial();
            Material interiorFloor = EnsureMaterial("Pharmacy_Interior_Floor.mat", new Color(0.9f, 0.92f, 0.94f), 0f, 0.32f);
            Material shelfWhite = EnsureMaterial("Pharmacy_Shelf_White.mat", PharmaWhite, 0f, 0.3f);
            Material productBox = EnsureMaterial("Pharmacy_Product_Box.mat", new Color(0.82f, 0.88f, 0.95f), 0f, 0.25f);
            Material sidewalk = AssetDatabase.LoadAssetAtPath<Material>(SidewalkMaterialPath)
                ?? EnsureMaterial("Pharmacy_Sidewalk.mat", new Color(0.62f, 0.64f, 0.66f), 0f, 0.22f);

            float halfWidth = FacadeWidth * 0.5f;
            float halfDepth = FacadeDepth * 0.5f;
            float frontZ = -halfDepth;
            float backZ = halfDepth;
            float signBottomY = FoundationHeight + WallHeight - SignHeight - 0.04f;
            float glassTopY = signBottomY - 0.06f;
            float glassHeight = glassTopY - GlassStartY;
            float glassCenterY = GlassStartY + glassHeight * 0.5f;
            float doorCenterY = GlassStartY + DoorHeight * 0.5f;
            float sideGlassWidth = (FacadeWidth - DoorWidth - FrameThickness * 4f) * 0.5f;
            float leftGlassX = -(DoorWidth * 0.5f + FrameThickness + sideGlassWidth * 0.5f);
            float rightGlassX = -leftGlassX;

            GameObject root = new GameObject(RootName);
            root.transform.position = worldPosition;
            root.transform.rotation = rotation;

            GameObject structure = new GameObject("Pharmacy_Structure");
            structure.transform.SetParent(root.transform, false);
            BuildStructure(structure.transform, wallWhite, wallBlue, roof, halfWidth, halfDepth, frontZ, backZ);

            GameObject sign = new GameObject("Pharmacy_Sign");
            sign.transform.SetParent(root.transform, false);
            BuildBannerSign(sign.transform, signBottomY, frontZ);

            GameObject storefront = new GameObject("Pharmacy_Storefront");
            storefront.transform.SetParent(root.transform, false);
            BuildStorefront(storefront.transform, frame, clearGlass, wallBlue, frontZ, glassCenterY, glassHeight,
                glassTopY, doorCenterY, sideGlassWidth, leftGlassX, rightGlassX, signBottomY);

            GameObject interior = new GameObject("Pharmacy_Interior");
            interior.transform.SetParent(root.transform, false);
            BuildInterior(interior.transform, shelfWhite, productBox, green, wallWhite, wallBlue, frontZ, backZ, halfWidth);

            GameObject props = new GameObject("Pharmacy_ExteriorProps");
            props.transform.SetParent(root.transform, false);
            BuildExteriorProps(props.transform, green, wallBlue, wallWhite, productBox, halfWidth, frontZ);

            GameObject environment = new GameObject("Pharmacy_Environment");
            environment.transform.SetParent(root.transform, false);
            BuildEnvironment(environment.transform, sidewalk, halfWidth, frontZ);

            MarkStatic(root);

            if (saveAsPrefab)
            {
                EnsureFolder("Assets/Art/Level1/Outdoor/Prefabs");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
            }

            return root;
        }

        private static void BuildStructure(Transform parent, Material wall, Material blue, Material roof,
            float halfWidth, float halfDepth, float frontZ, float backZ)
        {
            CreatePartCube("Pharmacy_Foundation", parent, new Vector3(0f, FoundationHeight * 0.5f, 0f),
                new Vector3(FacadeWidth + 0.06f, FoundationHeight, FacadeDepth + 0.06f), blue);
            CreatePartCube("Pharmacy_Wall_Left", parent,
                new Vector3(-halfWidth + 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);
            CreatePartCube("Pharmacy_Wall_Right", parent,
                new Vector3(halfWidth - 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);
            CreatePartCube("Pharmacy_Wall_Back", parent,
                new Vector3(0f, FoundationHeight + WallHeight * 0.5f, backZ - 0.05f),
                new Vector3(FacadeWidth - 0.14f, WallHeight, 0.1f), wall);
            CreatePartCube("Pharmacy_Roof", parent,
                new Vector3(0f, FoundationHeight + WallHeight + RoofThickness * 0.5f, 0f),
                new Vector3(FacadeWidth + 0.1f, RoofThickness, FacadeDepth + 0.1f), roof);
            CreatePartCube("Pharmacy_BaseBand", parent,
                new Vector3(0f, FoundationHeight + 0.42f, frontZ - 0.02f),
                new Vector3(FacadeWidth - 0.1f, 0.72f, 0.08f), blue);
        }

        private static void BuildBannerSign(Transform parent, float signBottomY, float frontZ)
        {
            Material bannerMat = EnsureBannerMaterial();
            if (bannerMat == null)
            {
                return;
            }

            float signCenterY = signBottomY + BannerHeight * 0.5f;
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Pharmacy_Banner";
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = new Vector3(0f, signCenterY, frontZ - 0.05f);
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = new Vector3(BannerWidth, BannerHeight, 1f);
            Object.DestroyImmediate(quad.GetComponent<Collider>());

            Material instance = new Material(bannerMat);
            instance.SetInt("_Cull", 0);
            instance.renderQueue = (int)RenderQueue.Geometry + 10;
            ApplyRenderer(quad.GetComponent<Renderer>(), instance);
        }

        private static Material EnsureBannerMaterial()
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
            if (texture == null)
            {
                Debug.LogError("[Level1VietnamesePharmacyBuilder] Missing banner texture: " + BannerTexturePath);
                return null;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(BannerMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_Pharmacy_MinhPhat";
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

        private static void BuildStorefront(Transform parent, Material frame, Material clearGlass, Material blue,
            float frontZ, float glassCenterY, float glassHeight, float glassTopY, float doorCenterY,
            float sideGlassWidth, float leftGlassX, float rightGlassX, float signBottomY)
        {
            float frameZ = frontZ - 0.03f;
            CreatePartCube("Pharmacy_Glass_Left", parent, new Vector3(leftGlassX, glassCenterY, frontZ - 0.015f),
                new Vector3(sideGlassWidth, glassHeight, 0.03f), clearGlass);
            CreatePartCube("Pharmacy_Glass_Right", parent, new Vector3(rightGlassX, glassCenterY, frontZ - 0.015f),
                new Vector3(sideGlassWidth, glassHeight, 0.03f), clearGlass);
            CreatePartCube("Pharmacy_Glass_Door_Left", parent,
                new Vector3(-DoorWidth * 0.25f, doorCenterY, frontZ - 0.012f),
                new Vector3(DoorWidth * 0.5f - 0.04f, DoorHeight - 0.06f, 0.028f), clearGlass);
            CreatePartCube("Pharmacy_Glass_Door_Right", parent,
                new Vector3(DoorWidth * 0.25f, doorCenterY, frontZ - 0.012f),
                new Vector3(DoorWidth * 0.5f - 0.04f, DoorHeight - 0.06f, 0.028f), clearGlass);
            CreatePartCube("Pharmacy_Frame_Header", parent, new Vector3(0f, glassTopY + FrameThickness * 0.5f, frameZ),
                new Vector3(FacadeWidth - 0.16f, FrameThickness, 0.06f), frame);
            CreatePartCube("Pharmacy_Frame_Sill", parent, new Vector3(0f, GlassStartY - FrameThickness * 0.5f, frameZ),
                new Vector3(FacadeWidth - 0.16f, FrameThickness, 0.06f), frame);

            float canopyY = signBottomY - 0.03f;
            float canopyZ = frontZ - CanopyDepth * 0.5f + 0.02f;
            CreatePartCube("Pharmacy_Canopy", parent, new Vector3(0f, canopyY, canopyZ),
                new Vector3(DoorWidth + 0.55f, CanopyThickness, CanopyDepth), blue);
            CreatePartCube("Pharmacy_Canopy_Lip", parent, new Vector3(0f, canopyY - CanopyThickness * 0.5f, frontZ - CanopyDepth + 0.02f),
                new Vector3(DoorWidth + 0.5f, 0.03f, 0.04f), frame);
        }

        private static void BuildInterior(Transform parent, Material shelf, Material product, Material green,
            Material counter, Material blue, float frontZ, float backZ, float halfWidth)
        {
            float innerFrontZ = frontZ + 0.22f;
            float innerBackZ = backZ - 0.14f;
            float floorTopY = FoundationHeight + 0.04f;
            float shelfDepth = 0.22f;
            float shelfWidth = 0.68f;
            float shelfHeight = 1.18f;
            float shelfCenterY = floorTopY + shelfHeight * 0.5f;

            CreatePartCube("Interior_Floor", parent, new Vector3(0f, floorTopY - 0.02f, (innerFrontZ + innerBackZ) * 0.5f),
                new Vector3(FacadeWidth - 0.28f, 0.04f, innerBackZ - innerFrontZ), counter);

            float[] shelfX = { -halfWidth + 0.62f, 0f, halfWidth - 0.62f };
            for (int i = 0; i < shelfX.Length; i++)
            {
                float sx = shelfX[i];
                CreatePartCube("Shelf_Unit_" + i, parent, new Vector3(sx, shelfCenterY, innerFrontZ + 0.28f),
                    new Vector3(shelfWidth, shelfHeight, shelfDepth), shelf);
                PlaceBoxOnSurface(parent, "Shelf_Product_A_" + i, sx - 0.12f, floorTopY + shelfHeight * 0.42f,
                    innerFrontZ + 0.3f, product);
                PlaceBoxOnSurface(parent, "Shelf_Product_B_" + i, sx + 0.1f, floorTopY + shelfHeight * 0.78f,
                    innerFrontZ + 0.3f, product);
                PlaceBoxOnSurface(parent, "Shelf_Product_C_" + i, sx, floorTopY + shelfHeight, innerFrontZ + 0.3f,
                    EnsureMaterial("Pharmacy_Product_Green.mat", PharmaGreen, 0f, 0.3f));
            }

            float counterHeight = 0.82f;
            float counterCenterY = floorTopY + counterHeight * 0.5f;
            CreatePartCube("Cashier_Counter", parent, new Vector3(0f, counterCenterY, innerBackZ - 0.18f),
                new Vector3(1.75f, counterHeight, 0.34f), counter);
            CreatePartCube("Cashier_Accent", parent, new Vector3(0f, floorTopY + counterHeight, innerBackZ - 0.18f),
                new Vector3(1.8f, 0.05f, 0.36f), green);
            Material monitorMat = EnsureMaterial("Pharmacy_Monitor.mat", PharmaBlue, 0.1f, 0.4f);
            PlaceBoxOnSurface(parent, "Cashier_Monitor", 0.12f, floorTopY + counterHeight, innerBackZ - 0.2f,
                monitorMat, 0.28f, 0.22f, 0.04f);

            CreatePartCube("Wall_Poster", parent, new Vector3(-halfWidth + 0.1f, floorTopY + 1.35f, innerBackZ - 0.08f),
                new Vector3(0.03f, 0.42f, 0.32f), green);
        }

        private static void PlaceBoxOnSurface(Transform parent, string name, float x, float surfaceY, float z,
            Material material, float width = 0.14f, float height = 0.1f, float depth = 0.1f)
        {
            CreatePartCube(name, parent, new Vector3(x, surfaceY + height * 0.5f, z),
                new Vector3(width, height, depth), material);
        }

        private static void BuildExteriorProps(Transform parent, Material green, Material blue, Material white,
            Material product, float halfWidth, float frontZ)
        {
            float sidewalkZ = frontZ - SidewalkDepth * 0.55f;
            float floorTopY = FoundationHeight + 0.04f;

            CreatePartCube("Prop_EntranceMat", parent, new Vector3(0f, floorTopY - 0.01f, frontZ + 0.08f),
                new Vector3(DoorWidth + 0.2f, 0.02f, 0.18f), white);
            CreatePartCube("Prop_PlantPot", parent, new Vector3(-halfWidth + 0.28f, floorTopY + 0.11f, sidewalkZ),
                new Vector3(0.2f, 0.2f, 0.2f), EnsureMaterial("Pharmacy_Pot.mat", new Color(0.55f, 0.34f, 0.22f), 0f, 0.25f));
            CreatePartCube("Prop_Plant", parent, new Vector3(-halfWidth + 0.28f, floorTopY + 0.28f, sidewalkZ),
                new Vector3(0.24f, 0.18f, 0.24f), green);
            CreatePartCube("Prop_Sanitizer", parent, new Vector3(halfWidth - 0.22f, floorTopY + 0.42f, frontZ + 0.06f),
                new Vector3(0.1f, 0.22f, 0.1f), white);
            CreatePartCube("Prop_PromoStand", parent, new Vector3(halfWidth - 0.35f, floorTopY + 0.48f, sidewalkZ + 0.05f),
                new Vector3(0.42f, 0.62f, 0.04f), blue);
            PlaceBoxOnSurface(parent, "Prop_PromoBox", halfWidth - 0.35f, floorTopY + 0.62f, sidewalkZ + 0.05f, product,
                0.18f, 0.12f, 0.08f);
        }

        private static void BuildEnvironment(Transform parent, Material sidewalk, float halfWidth, float frontZ)
        {
            float sidewalkCenterZ = frontZ - SidewalkDepth * 0.5f - 0.08f;
            CreatePartCube("Env_Sidewalk", parent, new Vector3(0f, 0.04f, sidewalkCenterZ),
                new Vector3(FacadeWidth + 0.95f, 0.08f, SidewalkDepth), sidewalk);
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
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureClearGlassMaterial()
        {
            string path = MaterialsRoot + "/Pharmacy_Glass_Clear.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = "Pharmacy_Glass_Clear";
                AssetDatabase.CreateAsset(mat, path);
            }

            Color tint = new Color(0.92f, 0.96f, 0.99f, 0.16f);
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
            mat.SetFloat("_Metallic", 0.04f);
            mat.SetFloat("_Smoothness", 0.92f);
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
