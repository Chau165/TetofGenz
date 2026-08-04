#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds a low-poly Circle K-inspired convenience store front facade as a modular prefab.</summary>
    public static class Level1ConvenienceStoreFacadeBuilder
    {
        public const string RootName = "ConvenienceStoreFacade";
        public const string PrefabPath = "Assets/Art/Level1/Outdoor/Prefabs/ConvenienceStoreFacade.prefab";

        private const string LogoTexturePath = "Assets/Image/Brand/circleK.png";
        private const string LogoMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_CircleK.mat";
        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials/Level1";

        private const float FacadeWidth = 6.75f;
        private const float FacadeDepth = 1.75f;
        private const float FoundationHeight = 0.1f;
        private const float WallHeight = 3.02f;
        private const float RoofThickness = 0.1f;
        private const float ParapetHeight = 0.16f;
        private const float FasciaHeight = 0.52f;
        private const float BannerHeightScale = 1.15f;
        private const float BannerWidthScale = 0.98f;
        private const float DoorWidth = 1.9f;
        private const float DoorHeight = 2.38f;
        private const float FrameThickness = 0.07f;
        private const float GlassStartY = FoundationHeight + 0.02f;
        private const float PlacementGap = 0.22f;
        private const float BoardingHouseSideClearance = 1.45f;

        private static readonly Vector3 FallbackScenePosition = new Vector3(8.52f, Level1OutdoorSettings.WalkSurfaceY, -2.35f);
        private static readonly Quaternion FallbackSceneRotation = Quaternion.Euler(0f, 90f, 0f);

        private const float HouseRightWallX = 5.55f;
        private const float HouseSideCenterZ = -2.35f;

        public static void UpgradeInteriorOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1ConvenienceStoreFacadeBuilder] Exit Play mode before upgrading the interior.");
                return;
            }

            GameObject store = GameObject.Find(RootName);
            if (store == null)
            {
                Debug.LogError("[Level1ConvenienceStoreFacadeBuilder] ConvenienceStoreFacade not found in scene.");
                return;
            }

            ApplyFakeInteriorVisuals(store.transform);
            ApplyCircleKBanner(store.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1ConvenienceStoreFacadeBuilder] Store interior backdrop and banner updated.");
        }

        public static void AttachBannerOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1ConvenienceStoreFacadeBuilder] Exit Play mode before attaching the banner.");
                return;
            }

            GameObject store = GameObject.Find(RootName);
            if (store == null)
            {
                Debug.LogError("[Level1ConvenienceStoreFacadeBuilder] ConvenienceStoreFacade not found in scene.");
                return;
            }

            ApplyCircleKBanner(store.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1ConvenienceStoreFacadeBuilder] Circle K banner attached.");
        }

        public static void ApplyCircleKBanner(Transform store)
        {
            Material logo = EnsureLogoMaterial();
            Transform fascia = store.Find("Facade_Fascia/FasciaBoard");
            if (fascia == null)
            {
                Debug.LogError("[Level1ConvenienceStoreFacadeBuilder] FasciaBoard not found.");
                return;
            }

            float fasciaInnerWidth = FacadeWidth - 0.42f;
            float logoHeight = FasciaHeight - 0.08f;
            float logoWidth = fasciaInnerWidth - 0.12f;

            Transform logoRoot = store.Find("Facade_Logo");
            if (logoRoot == null)
            {
                GameObject logoObj = new GameObject("Facade_Logo");
                logoRoot = logoObj.transform;
                logoRoot.SetParent(store, false);
            }

            logoRoot.localPosition = fascia.parent != null ? fascia.parent.localPosition : fascia.localPosition;
            logoRoot.localRotation = Quaternion.identity;

            Transform existing = fascia.Find("CircleKBanner");
            if (existing == null)
            {
                existing = logoRoot.Find("CircleKBanner");
            }

            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            AttachBannerToFascia(fascia, logoRoot, store, logo);
            MarkStatic(logoRoot.gameObject);
        }

        public static void BuildPrefabAsset()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1ConvenienceStoreFacadeBuilder] Exit Play mode before building the prefab.");
                return;
            }

            GameObject root = BuildFacadeAt(Vector3.zero, Quaternion.identity, saveAsPrefab: true);
            if (root == null)
            {
                return;
            }

            Object.DestroyImmediate(root);
            Debug.Log("[Level1ConvenienceStoreFacadeBuilder] Prefab saved: " + PrefabPath);
        }

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1ConvenienceStoreFacadeBuilder] Exit Play mode before placing the facade.");
                return;
            }

            RemoveExistingInScene();
            Transform parent = FindOutdoorParent();
            ResolvePlacementBesideBoardingHouse(out Vector3 placement, out Quaternion rotation);
            GameObject root = BuildFacadeAt(placement, rotation, saveAsPrefab: false);
            if (root == null)
            {
                return;
            }

            root.transform.SetParent(parent, true);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1ConvenienceStoreFacadeBuilder] Convenience store facade placed on right sidewalk.");
        }

        public static GameObject BuildFacadeAt(Vector3 worldPosition, Quaternion rotation, bool saveAsPrefab)
        {
            Material wall = EnsureMaterial("Store_Facade_White.mat", new Color(0.94f, 0.95f, 0.96f), 0f, 0.18f);
            Material roof = EnsureMaterial("Store_Roof_Flat.mat", new Color(0.78f, 0.8f, 0.82f), 0.05f, 0.22f);
            Material fascia = EnsureMaterial("Store_Fascia_Red.mat", new Color(0.86f, 0.12f, 0.16f), 0f, 0.24f);
            Material frame = EnsureMaterial("Store_Frame_Aluminum.mat", new Color(0.62f, 0.64f, 0.67f), 0.35f, 0.55f);
            Material glass = EnsureTintedGlassMaterial();
            Material logo = EnsureLogoMaterial();

            float halfWidth = FacadeWidth * 0.5f;
            float halfDepth = FacadeDepth * 0.5f;
            float frontZ = -halfDepth;
            float backZ = halfDepth;
            float roofY = FoundationHeight + WallHeight + RoofThickness * 0.5f;
            float fasciaCenterY = FoundationHeight + WallHeight - FasciaHeight * 0.5f - 0.03f;
            float glassTopY = fasciaCenterY - FasciaHeight * 0.5f - 0.04f;
            float glassHeight = glassTopY - GlassStartY;
            float glassCenterY = GlassStartY + glassHeight * 0.5f;
            float doorCenterY = GlassStartY + DoorHeight * 0.5f;
            float sideGlassWidth = (FacadeWidth - DoorWidth - FrameThickness * 4f) * 0.5f;
            float leftGlassX = -(DoorWidth * 0.5f + FrameThickness + sideGlassWidth * 0.5f);
            float rightGlassX = -leftGlassX;

            GameObject root = new GameObject(RootName);
            root.transform.position = worldPosition;
            root.transform.rotation = rotation;

            CreatePartCube("Facade_Foundation", root.transform,
                new Vector3(0f, FoundationHeight * 0.5f, 0f),
                new Vector3(FacadeWidth + 0.04f, FoundationHeight, FacadeDepth + 0.04f), frame);

            CreatePartCube("Facade_SideWall_Left", root.transform,
                new Vector3(-halfWidth + 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);

            CreatePartCube("Facade_SideWall_Right", root.transform,
                new Vector3(halfWidth - 0.06f, FoundationHeight + WallHeight * 0.5f, 0f),
                new Vector3(0.12f, WallHeight, FacadeDepth), wall);

            CreatePartCube("Facade_BackWall", root.transform,
                new Vector3(0f, FoundationHeight + WallHeight * 0.5f, backZ - 0.05f),
                new Vector3(FacadeWidth - 0.18f, WallHeight, 0.1f), wall);

            BuildFakeInteriorVisuals(root.transform, frontZ, backZ, glassCenterY, glassHeight, glassTopY,
                leftGlassX, rightGlassX, sideGlassWidth, doorCenterY);

            CreatePartCube("Facade_Roof", root.transform,
                new Vector3(0f, roofY, 0f),
                new Vector3(FacadeWidth + 0.08f, RoofThickness, FacadeDepth + 0.08f), roof);

            CreatePartCube("Facade_Parapet", root.transform,
                new Vector3(0f, roofY + RoofThickness * 0.5f + ParapetHeight * 0.5f, frontZ - 0.03f),
                new Vector3(FacadeWidth + 0.1f, ParapetHeight, 0.08f), frame);

            GameObject fasciaRoot = new GameObject("Facade_Fascia");
            fasciaRoot.transform.SetParent(root.transform, false);
            GameObject fasciaBoard = CreatePartCube("FasciaBoard", fasciaRoot.transform,
                new Vector3(0f, fasciaCenterY, frontZ - 0.04f),
                new Vector3(FacadeWidth - 0.28f, FasciaHeight, 0.12f), fascia);

            GameObject logoRoot = new GameObject("Facade_Logo");
            logoRoot.transform.SetParent(root.transform, false);
            logoRoot.transform.localPosition = fasciaRoot.transform.localPosition;
            AttachBannerToFascia(fasciaBoard.transform, logoRoot.transform, root.transform, logo);

            CreatePartCube("Facade_Glass_Left", root.transform,
                new Vector3(leftGlassX, glassCenterY, frontZ - 0.02f),
                new Vector3(sideGlassWidth, glassHeight, 0.04f), glass);

            CreatePartCube("Facade_Glass_Right", root.transform,
                new Vector3(rightGlassX, glassCenterY, frontZ - 0.02f),
                new Vector3(sideGlassWidth, glassHeight, 0.04f), glass);

            GameObject frameRoot = new GameObject("Facade_StorefrontFrame");
            frameRoot.transform.SetParent(root.transform, false);
            float frameFrontZ = frontZ - 0.03f;
            CreatePartCube("Frame_Header", frameRoot.transform,
                new Vector3(0f, glassTopY + FrameThickness * 0.5f, frameFrontZ),
                new Vector3(FacadeWidth - 0.24f, FrameThickness, 0.06f), frame);
            CreatePartCube("Frame_Sill", frameRoot.transform,
                new Vector3(0f, GlassStartY - FrameThickness * 0.5f, frameFrontZ),
                new Vector3(FacadeWidth - 0.24f, FrameThickness, 0.06f), frame);
            CreatePartCube("Frame_Mullion_Left", frameRoot.transform,
                new Vector3(-DoorWidth * 0.5f - FrameThickness * 0.5f, glassCenterY, frameFrontZ),
                new Vector3(FrameThickness, glassHeight, 0.06f), frame);
            CreatePartCube("Frame_Mullion_Right", frameRoot.transform,
                new Vector3(DoorWidth * 0.5f + FrameThickness * 0.5f, glassCenterY, frameFrontZ),
                new Vector3(FrameThickness, glassHeight, 0.06f), frame);

            GameObject doorRoot = new GameObject("Facade_SlidingDoor");
            doorRoot.transform.SetParent(root.transform, false);

            float panelWidth = DoorWidth * 0.5f - 0.03f;
            GameObject leftPanel = CreatePartCube("Door_Panel_Left", doorRoot.transform,
                new Vector3(-panelWidth * 0.5f, doorCenterY, frontZ - 0.025f),
                new Vector3(panelWidth, DoorHeight - 0.06f, 0.035f), glass);
            GameObject rightPanel = CreatePartCube("Door_Panel_Right", doorRoot.transform,
                new Vector3(panelWidth * 0.5f, doorCenterY, frontZ - 0.025f),
                new Vector3(panelWidth, DoorHeight - 0.06f, 0.035f), glass);

            GameObject triggerObj = new GameObject("Door_ProximityTrigger");
            triggerObj.transform.SetParent(doorRoot.transform, false);
            triggerObj.transform.localPosition = new Vector3(0f, doorCenterY, frontZ - 0.45f);
            BoxCollider trigger = triggerObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(DoorWidth + 0.35f, DoorHeight, 1.1f);
            trigger.center = Vector3.zero;

            SlidingGlassDoor door = doorRoot.AddComponent<SlidingGlassDoor>();
            door.Configure(leftPanel.transform, rightPanel.transform, trigger);

            MarkStatic(root);

            if (saveAsPrefab)
            {
                EnsureFolder("Assets/Art/Level1/Outdoor/Prefabs");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
            }

            return root;
        }

        public static void ApplyFakeInteriorVisuals(Transform store)
        {
            float halfWidth = FacadeWidth * 0.5f;
            float halfDepth = FacadeDepth * 0.5f;
            float frontZ = -halfDepth;
            float backZ = halfDepth;
            float fasciaCenterY = FoundationHeight + WallHeight - FasciaHeight * 0.5f - 0.03f;
            float glassTopY = fasciaCenterY - FasciaHeight * 0.5f - 0.04f;
            float glassHeight = glassTopY - GlassStartY;
            float glassCenterY = GlassStartY + glassHeight * 0.5f;
            float doorCenterY = GlassStartY + DoorHeight * 0.5f;
            float sideGlassWidth = (FacadeWidth - DoorWidth - FrameThickness * 4f) * 0.5f;
            float leftGlassX = -(DoorWidth * 0.5f + FrameThickness + sideGlassWidth * 0.5f);
            float rightGlassX = -leftGlassX;

            Transform legacy = store.Find("Facade_FakeInterior");
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy.gameObject);
            }

            Transform existing = store.Find("Facade_InteriorView");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            BuildFakeInteriorVisuals(store, frontZ, backZ, glassCenterY, glassHeight, glassTopY,
                leftGlassX, rightGlassX, sideGlassWidth, doorCenterY);
            MarkStatic(store.gameObject);
        }

        private static void BuildFakeInteriorVisuals(Transform root, float frontZ, float backZ, float glassCenterY,
            float glassHeight, float glassTopY, float leftGlassX, float rightGlassX, float sideGlassWidth,
            float doorCenterY)
        {
            Material interiorWall = EnsureMaterial("Store_Interior_Wall.mat", new Color(0.96f, 0.93f, 0.86f), 0f, 0.16f);
            Material interiorFloor = EnsureMaterial("Store_Interior_Floor.mat", new Color(0.78f, 0.8f, 0.82f), 0f, 0.24f);
            Material shelf = EnsureMaterial("Store_Interior_Shelf.mat", new Color(0.99f, 0.99f, 1f), 0f, 0.2f);
            Material productRed = EnsureUnlitMaterial("Store_Product_Red.mat", new Color(0.95f, 0.15f, 0.18f));
            Material productYellow = EnsureUnlitMaterial("Store_Product_Yellow.mat", new Color(1f, 0.84f, 0.1f));
            Material productGreen = EnsureUnlitMaterial("Store_Product_Green.mat", new Color(0.1f, 0.82f, 0.28f));
            Material windowGlow = EnsureUnlitMaterial("Store_Interior_WindowGlow.mat", new Color(0.98f, 0.94f, 0.82f));
            Material counter = EnsureMaterial("Store_Interior_Counter.mat", new Color(0.86f, 0.88f, 0.9f), 0.12f, 0.42f);

            float interiorFrontZ = frontZ + 0.1f;
            float interiorBackZ = backZ - 0.12f;
            float interiorCenterZ = (interiorFrontZ + interiorBackZ) * 0.5f;
            float interiorDepth = interiorBackZ - interiorFrontZ;
            float openingWidth = FacadeWidth - 0.34f;
            float nearZ = frontZ + 0.2f;
            float shelfDepth = 0.42f;
            float shelfCenterZ = interiorFrontZ + shelfDepth * 0.5f + 0.06f;

            GameObject interiorRoot = new GameObject("Facade_InteriorView");
            interiorRoot.transform.SetParent(root, false);

            CreatePartCube("Interior_Floor", interiorRoot.transform,
                new Vector3(0f, FoundationHeight + 0.02f, interiorCenterZ),
                new Vector3(openingWidth, 0.04f, interiorDepth), interiorFloor);

            CreatePartCube("Interior_Ceiling", interiorRoot.transform,
                new Vector3(0f, glassTopY - 0.03f, interiorCenterZ),
                new Vector3(openingWidth, 0.05f, interiorDepth), interiorWall);

            CreatePartCube("Interior_BackWall", interiorRoot.transform,
                new Vector3(0f, glassCenterY, interiorBackZ),
                new Vector3(openingWidth, glassHeight + 0.08f, 0.08f), interiorWall);

            CreatePartCube("Interior_Shelf_Left", interiorRoot.transform,
                new Vector3(leftGlassX, glassCenterY, shelfCenterZ),
                new Vector3(sideGlassWidth - 0.06f, glassHeight - 0.08f, shelfDepth), shelf);
            CreatePartCube("Interior_Shelf_Right", interiorRoot.transform,
                new Vector3(rightGlassX, glassCenterY, shelfCenterZ),
                new Vector3(sideGlassWidth - 0.06f, glassHeight - 0.08f, shelfDepth), shelf);

            CreatePartCube("Interior_Counter", interiorRoot.transform,
                new Vector3(0f, doorCenterY - 0.16f, interiorBackZ - 0.18f),
                new Vector3(DoorWidth + 0.5f, 0.95f, 0.38f), counter);

            CreatePartCube("Interior_CounterTop", interiorRoot.transform,
                new Vector3(0f, doorCenterY + 0.22f, interiorBackZ - 0.18f),
                new Vector3(DoorWidth + 0.56f, 0.07f, 0.4f), shelf);

            float[] shelfYs = { glassCenterY - 0.58f, glassCenterY + 0.02f, glassCenterY + 0.64f };
            Material[] productMats = { productRed, productYellow, productGreen };
            for (int side = -1; side <= 1; side += 2)
            {
                float shelfX = side < 0 ? leftGlassX : rightGlassX;
                CreateInteriorQuad($"Interior_WindowFill_{(side < 0 ? "Left" : "Right")}", interiorRoot.transform,
                    new Vector3(shelfX, glassCenterY, nearZ - 0.04f),
                    new Vector3(sideGlassWidth * 0.9f, glassHeight * 0.92f), windowGlow);

                for (int i = 0; i < shelfYs.Length; i++)
                {
                    CreatePartCube($"Interior_Product_{(side < 0 ? "L" : "R")}_{i}", interiorRoot.transform,
                        new Vector3(shelfX, shelfYs[i], nearZ),
                        new Vector3(sideGlassWidth * 0.78f, 0.22f, 0.12f), productMats[i]);
                }
            }

            CreateInteriorQuad("Interior_WindowFill_Door", interiorRoot.transform,
                new Vector3(0f, doorCenterY, nearZ - 0.02f),
                new Vector3(DoorWidth * 0.88f, DoorHeight * 0.92f), windowGlow);

            CreatePartCube("Interior_DoorBackdrop", interiorRoot.transform,
                new Vector3(0f, doorCenterY, interiorFrontZ + 0.12f),
                new Vector3(DoorWidth - 0.04f, DoorHeight - 0.06f, 0.06f), interiorWall);

            CreatePartCube("Interior_SideOccluder_Left", interiorRoot.transform,
                new Vector3(-openingWidth * 0.5f + 0.03f, glassCenterY, interiorCenterZ),
                new Vector3(0.06f, glassHeight, interiorDepth), interiorWall);
            CreatePartCube("Interior_SideOccluder_Right", interiorRoot.transform,
                new Vector3(openingWidth * 0.5f - 0.03f, glassCenterY, interiorCenterZ),
                new Vector3(0.06f, glassHeight, interiorDepth), interiorWall);
        }

        private static GameObject CreateInteriorQuad(string name, Transform parent, Vector3 localPos, Vector3 localScale,
            Material material)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = localPos;
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = localScale;

            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            ApplyRenderer(quad.GetComponent<Renderer>(), material);
            return quad;
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

        private static Transform FindOutdoorParent()
        {
            Transform world = GameObject.Find("Level1World")?.transform;
            if (world != null)
            {
                Transform outdoor = world.Find("Level1_OutdoorArea");
                if (outdoor != null)
                {
                    return outdoor;
                }
            }

            return null;
        }

        private static void ResolvePlacementBesideBoardingHouse(out Vector3 position, out Quaternion rotation)
        {
            rotation = FallbackSceneRotation;
            position = FallbackScenePosition;

            Transform outdoor = FindOutdoorParent();
            if (outdoor == null)
            {
                return;
            }

            Transform house = outdoor.Find("Level1_HouseExterior");
            if (house == null)
            {
                house = GameObject.Find("Level1_HouseExterior")?.transform;
            }

            if (house == null)
            {
                return;
            }

            Bounds houseBounds = ComputeWorldBounds(house.gameObject);
            float halfDepth = FacadeDepth * 0.5f;
            float storeX = houseBounds.max.x + halfDepth + PlacementGap + BoardingHouseSideClearance;
            float halfWidthAlongZ = FacadeWidth * 0.5f;
            float storeZ = Mathf.Clamp(HouseSideCenterZ, houseBounds.min.z + halfWidthAlongZ, houseBounds.max.z - halfWidthAlongZ);
            position = new Vector3(storeX, Level1OutdoorSettings.WalkSurfaceY, storeZ);
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

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            ApplyRenderer(cube.GetComponent<Renderer>(), material);
            return cube;
        }

        private static GameObject AttachBannerToFascia(Transform fascia, Transform logoRoot, Transform store, Material material)
        {
            Renderer fasciaRenderer = fascia.GetComponent<Renderer>();
            if (fasciaRenderer == null)
            {
                Debug.LogError("[Level1ConvenienceStoreFacadeBuilder] FasciaBoard has no Renderer.");
                return null;
            }

            Vector3 outward = store.TransformDirection(-Vector3.forward).normalized;
            Bounds bounds = fasciaRenderer.bounds;
            Vector3 absOut = new Vector3(Mathf.Abs(outward.x), Mathf.Abs(outward.y), Mathf.Abs(outward.z));
            float outwardExtent = Vector3.Dot(bounds.extents, absOut);
            Vector3 faceCenter = bounds.center + outward * outwardExtent;

            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, outward);
            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.Cross(Vector3.forward, outward);
            }

            right.Normalize();
            Vector3 absRight = new Vector3(Mathf.Abs(right.x), Mathf.Abs(right.y), Mathf.Abs(right.z));
            float width = Vector3.Dot(bounds.size, absRight) * BannerWidthScale;
            float height = bounds.size.y * BannerHeightScale;

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "CircleKBanner";
            quad.transform.position = faceCenter + outward * 0.12f;
            quad.transform.rotation = Quaternion.LookRotation(outward, up);
            quad.transform.SetParent(logoRoot, true);

            Vector3 parentScale = logoRoot.lossyScale;
            quad.transform.localScale = new Vector3(
                width / Mathf.Max(parentScale.x, 0.001f),
                height / Mathf.Max(parentScale.y, 0.001f),
                1f / Mathf.Max(parentScale.z, 0.001f));

            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = quad.GetComponent<Renderer>();
            Material instance = new Material(material);
            instance.SetTextureScale("_BaseMap", new Vector2(-1f, 1f));
            instance.SetTextureOffset("_BaseMap", new Vector2(1f, 0f));
            instance.SetTextureScale("_MainTex", new Vector2(-1f, 1f));
            instance.SetTextureOffset("_MainTex", new Vector2(1f, 0f));
            instance.SetInt("_Cull", 0);
            instance.renderQueue = (int)RenderQueue.Geometry + 10;
            ApplyRenderer(renderer, instance);

            fasciaRenderer.enabled = false;
            return quad;
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

        private static Material EnsureTintedGlassMaterial()
        {
            string path = MaterialsRoot + "/Store_Glass_TintedBlue.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = "Store_Glass_TintedBlue";
                AssetDatabase.CreateAsset(mat, path);
            }

            Color tint = new Color(0.62f, 0.8f, 0.92f, 0.68f);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_Color", tint);
            mat.SetFloat("_Metallic", 0.08f);
            mat.SetFloat("_Smoothness", 0.82f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureLogoMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(LogoMaterialPath);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoTexturePath);
            if (texture == null)
            {
                Debug.LogError("[Level1ConvenienceStoreFacadeBuilder] Missing banner texture: " + LogoTexturePath);
                return EnsureMaterial("Store_Fascia_Red.mat", Color.white, 0f, 0.2f);
            }

            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_CircleK";
                AssetDatabase.CreateAsset(mat, LogoMaterialPath);
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
