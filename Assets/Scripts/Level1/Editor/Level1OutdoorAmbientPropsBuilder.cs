#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Small flower corners, plant pots, and a compact street stall using imported low-poly packs.</summary>
    public static class Level1OutdoorAmbientPropsBuilder
    {
        public const string RootName = "Ambient_Street_Props";
        public const string FlowerCornerName = "Flower_Corner";
        public const string FlowerDisplayName = "Flower_Display";
        public const string StreetStallName = "Street_Stall";
        public const string NpcPositionsName = "NPC_Ambient_Positions";

        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials";
        private const string BlenderFlowerPotPath =
            "Assets/Art/Level1/Outdoor/Prefabs/OutdoorFlowerPot.prefab";
        private const string BlenderFlowerPotFbxPath =
            "Assets/Art/Level1/Outdoor/OutdoorFlowerPot.fbx";
        private const string FlowersRoot = "Assets/Emilulz_Assets/DEMOLowPolyFlowers/Prefabs";
        private const string StallRoot = "Assets/PolyRonin/Medieval Market Stalls/Prefabs";
        private const string MarketProduceRoot = "Assets/Low-Poly Medieval Market/Prefabs/Fruits and vegetables";
        private const string MarketPropsRoot = "Assets/Low-Poly Medieval Market/Prefabs";
        private const string StallTexturePath = "Assets/PolyRonin/Medieval Market Stalls/Textures/market-stalls-colors.png";

        private const float GroundY = Level1OutdoorSettings.WalkSurfaceY;
        private const float StallYaw = 90f;
        private const int FlowerTrellisCount = 16;
        private const int FlowerGridColumns = 4;
        private const int FlowerGridRows = 4;

        // Left sidewalk — keep the stall and vendor off the building facade.
        private const float LeftSidewalkCenterX = -(3.38f + 2.28f * 0.5f);
        private const float RightSidewalkInnerX = 3.38f;
        private const float StallPathOffset = 0.82f;
        private const float SellerBehindStallOffset = 0.72f;
        public static readonly Vector3 StallCenter = new Vector3(LeftSidewalkCenterX + StallPathOffset, GroundY, -12.45f);
        // Sit on the inner half of the right sidewalk — clear of neighbor facades (~6m).
        public static readonly Vector3 FlowerCornerCenter = new Vector3(
            RightSidewalkInnerX + 0.52f,
            GroundY,
            -14.65f);

        /// <summary>Behind the stall counter with comfortable clearance from the counter edge.</summary>
        public static readonly Vector3 StallSellerSpot = new Vector3(
            StallCenter.x - SellerBehindStallOffset,
            GroundY + 0.02f,
            StallCenter.z);
        public static readonly Vector3 FlowerViewerSpot = new Vector3(1.95f, GroundY + 0.02f, FlowerCornerCenter.z);
        public static readonly Vector3 TalkSpotA = new Vector3(-1.62f, GroundY + 0.02f, -18.35f);
        public static readonly Vector3 TalkSpotB = new Vector3(-0.98f, GroundY + 0.02f, -18.55f);
        public static readonly Vector3 CasualAloneSpot = new Vector3(1.72f, GroundY + 0.02f, -20.45f);
        public static readonly Vector3 CasualSecondSpot = new Vector3(-2.05f, GroundY + 0.02f, -22.15f);

        private static readonly string[] ProducePrefabPaths =
        {
            MarketProduceRoot + "/tomato.prefab",
            MarketProduceRoot + "/carrot.prefab",
            MarketPropsRoot + "/potato.prefab",
            MarketProduceRoot + "/onion.prefab",
            MarketProduceRoot + "/apple_red.prefab",
            MarketProduceRoot + "/apple_green.prefab",
            MarketProduceRoot + "/apple_yellow.prefab",
            MarketProduceRoot + "/banana.prefab",
            MarketProduceRoot + "/watermelon.prefab",
            MarketProduceRoot + "/cucumber.prefab",
            MarketProduceRoot + "/beet.prefab",
            MarketProduceRoot + "/eggplant.prefab",
        };

        private static readonly string[] FlowerBloomPrefabPaths =
        {
            FlowersRoot + "/SM_Rose_Red_Open.prefab",
            FlowersRoot + "/SM_Daisy_Single.prefab",
            FlowersRoot + "/SM_Hyacinth_PastellBlue_Big.prefab",
            FlowersRoot + "/SM_Dandelion_Small.prefab",
        };

        // Blender pot rim height at scale 1 (see CreateOutdoorFlowerPot.py).
        private const float BlenderPotRimHeight = 0.31f;

        private static Material stallMaterial;
        private static Material flowerPotMaterial;
        private static Material produceMaterial;

        public static void Build(Transform outdoorRoot)
        {
            if (outdoorRoot == null)
            {
                return;
            }

            EnsureMaterials();

            Transform existing = outdoorRoot.Find(RootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject(RootName);
            root.transform.SetParent(outdoorRoot, false);

            BuildFlowerCorner(root.transform);
            BuildStreetStall(root.transform);
            BuildNpcPositionMarkers(root.transform);
        }

        private static void BuildFlowerCorner(Transform parent)
        {
            GameObject corner = new GameObject(FlowerCornerName);
            corner.transform.SetParent(parent, false);
            corner.transform.position = FlowerCornerCenter;

            GameObject display = new GameObject(FlowerDisplayName);
            display.transform.SetParent(corner.transform, false);
            display.transform.localPosition = Vector3.zero;

            BuildFlowerGrid(display.transform);
        }

        private static void BuildFlowerGrid(Transform parent)
        {
            const float spacingX = 0.36f;
            const float spacingZ = 0.36f;
            float startX = -((FlowerGridColumns - 1) * spacingX) * 0.5f;
            float startZ = -((FlowerGridRows - 1) * spacingZ) * 0.5f;

            int placed = 0;
            for (int row = 0; row < FlowerGridRows && placed < FlowerTrellisCount; row++)
            {
                for (int col = 0; col < FlowerGridColumns && placed < FlowerTrellisCount; col++)
                {
                    float hashA = Hash01(placed * 17 + 3);
                    float hashB = Hash01(placed * 29 + 11);
                    float hashC = Hash01(placed * 41 + 7);
                    float hashD = Hash01(placed * 53 + 19);

                    float potScale = Mathf.Lerp(0.40f, 0.48f, hashC);
                    float yaw = hashD * 360f;
                    Vector3 localOffset = new Vector3(
                        startX + col * spacingX + (hashA - 0.5f) * 0.05f,
                        0f,
                        startZ + row * spacingZ + (hashB - 0.5f) * 0.05f);

                    PlacePottedFlower(
                        parent,
                        localOffset,
                        yaw,
                        potScale,
                        placed,
                        "FlowerPot_" + (placed + 1).ToString("000"));
                    placed++;
                }
            }
        }

        private static void PlacePottedFlower(
            Transform parent,
            Vector3 localOffset,
            float yaw,
            float potScale,
            int bloomIndex,
            string name)
        {
            GameObject mount = new GameObject(name);
            mount.transform.SetParent(parent, false);
            mount.transform.localPosition = localOffset;
            mount.transform.localRotation = Quaternion.identity;

            string potPath = ResolveFlowerPotPrefabPath();
            GameObject pot = PlaceProp(
                mount.transform,
                potPath,
                Vector3.zero,
                yaw,
                Vector3.one * potScale,
                "Pot",
                snapToGround: false);
            if (pot != null && flowerPotMaterial != null)
            {
                ApplyMaterial(pot, flowerPotMaterial);
            }

            string bloomPath = FlowerBloomPrefabPaths[Mathf.Abs(bloomIndex) % FlowerBloomPrefabPaths.Length];
            float bloomScale = Mathf.Lerp(0.38f, 0.48f, Hash01(bloomIndex * 13 + 5));
            float bloomYaw = yaw + (Hash01(bloomIndex * 7 + 2) * 30f - 15f);
            float bloomY = BlenderPotRimHeight * potScale;
            PlaceProp(
                mount.transform,
                bloomPath,
                new Vector3(0f, bloomY, 0f),
                bloomYaw,
                Vector3.one * bloomScale,
                "Bloom",
                snapToGround: false);

            SnapToGround(mount, parent.position.y);
            StripColliders(mount);
        }

        private static string ResolveFlowerPotPrefabPath()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BlenderFlowerPotPath) != null)
            {
                return BlenderFlowerPotPath;
            }

            return BlenderFlowerPotFbxPath;
        }

        private static float Hash01(int seed)
        {
            unchecked
            {
                uint value = (uint)seed * 747796405u + 2891336453u;
                value = ((value >> 16) ^ value) * 0x45d9f3bu;
                value = ((value >> 16) ^ value) * 0x45d9f3bu;
                value = (value >> 16) ^ value;
                return (value & 0xFFFF) / 65535f;
            }
        }

        private static void BuildStreetStall(Transform parent)
        {
            GameObject stallZone = new GameObject(StreetStallName);
            stallZone.transform.SetParent(parent, false);
            stallZone.transform.position = StallCenter;
            stallZone.transform.rotation = Quaternion.Euler(0f, StallYaw, 0f);

            PlaceStallProp(stallZone.transform, StallRoot + "/Stall2.prefab", Vector3.zero, 0f, Vector3.one * 0.42f, "Stall_Main");
            PlaceStallProp(stallZone.transform, StallRoot + "/Basket1.prefab", new Vector3(0.62f, 0f, 0.28f), -12f, Vector3.one * 0.3f, "Stall_Basket_A");
            PlaceStallProp(stallZone.transform, StallRoot + "/Basket3.prefab", new Vector3(0.52f, 0f, -0.36f), 18f, Vector3.one * 0.28f, "Stall_Basket_B");
            PlaceStallProp(stallZone.transform, StallRoot + "/Crate1.prefab", new Vector3(-0.58f, 0f, 0.18f), 4f, Vector3.one * 0.32f, "Stall_Crate");
            PlaceStallProp(stallZone.transform, StallRoot + "/Box.prefab", new Vector3(-0.46f, 0f, -0.42f), -14f, Vector3.one * 0.29f, "Stall_Box");
            PlaceStallProp(stallZone.transform, StallRoot + "/Jar2.prefab", new Vector3(0.28f, 0f, 0.52f), 0f, Vector3.one * 0.24f, "Stall_Jar");
            PlaceStallProp(stallZone.transform, StallRoot + "/Barrel.prefab", new Vector3(-0.72f, 0f, -0.12f), 22f, Vector3.one * 0.26f, "Stall_Barrel");
            BuildStallProduce(stallZone.transform);
        }

        private static void BuildStallProduce(Transform stallZone)
        {
            Transform stallMain = stallZone.Find("Stall_Main");
            Transform produceParent = stallMain != null ? stallMain : stallZone;

            GameObject produceRoot = new GameObject("Stall_Produce");
            produceRoot.transform.SetParent(produceParent, false);
            produceRoot.transform.localPosition = Vector3.zero;
            produceRoot.transform.localRotation = Quaternion.identity;
            produceRoot.transform.localScale = Vector3.one;

            // Customer-facing side is +Z in Stall2 local space (stall zone yaw 90°).
            FillStallShelf(produceRoot.transform, "Shelf_Lower", shelfY: 0.94f, frontZ: 0.46f, xMin: -0.8f, xMax: 0.8f, columns: 12, rows: 5, itemScale: 0.5f);
            FillStallShelf(produceRoot.transform, "Shelf_Upper", shelfY: 1.56f, frontZ: 0.48f, xMin: -0.76f, xMax: 0.76f, columns: 10, rows: 4, itemScale: 0.48f);

            PlaceProduce(produceRoot.transform, MarketPropsRoot + "/wooden_bowl_01.prefab", new Vector3(0.24f, 0.97f, 0.44f), 0f, 0.58f, "Produce_Bowl_A", useProduceMaterial: false);
            PlaceProduce(produceRoot.transform, MarketPropsRoot + "/wooden_bowl_02.prefab", new Vector3(-0.24f, 0.97f, 0.44f), 10f, 0.55f, "Produce_Bowl_B", useProduceMaterial: false);
            PlaceProduce(produceRoot.transform, MarketProduceRoot + "/watermelon.prefab", new Vector3(0f, 1.6f, 0.46f), 12f, 0.72f, "Produce_Watermelon_Hero");
            PlaceProduce(produceRoot.transform, MarketProduceRoot + "/bananas.prefab", new Vector3(0.36f, 1.58f, 0.45f), -18f, 0.56f, "Produce_Bananas");
        }

        private static void FillStallShelf(
            Transform parent,
            string shelfName,
            float shelfY,
            float frontZ,
            float xMin,
            float xMax,
            int columns,
            int rows,
            float itemScale)
        {
            GameObject shelfRoot = new GameObject(shelfName);
            shelfRoot.transform.SetParent(parent, false);
            shelfRoot.transform.localPosition = Vector3.zero;
            shelfRoot.transform.localRotation = Quaternion.identity;

            float stepX = columns > 1 ? (xMax - xMin) / (columns - 1) : 0f;
            float rowStepZ = rows > 1 ? 0.09f : 0f;
            int index = 0;

            for (int row = 0; row < rows; row++)
            {
                float rowZ = frontZ - row * rowStepZ;
                for (int col = 0; col < columns; col++)
                {
                    float hashA = Hash01(index * 13 + 5);
                    float hashB = Hash01(index * 27 + 9);
                    float hashC = Hash01(index * 31 + 17);
                    float x = xMin + col * stepX + (hashA - 0.5f) * 0.04f;
                    float scale = itemScale * Mathf.Lerp(0.9f, 1.1f, hashB);
                    float yaw = hashC * 360f;
                    string prefabPath = ProducePrefabPaths[index % ProducePrefabPaths.Length];
                    PlaceProduce(
                        shelfRoot.transform,
                        prefabPath,
                        new Vector3(x, shelfY, rowZ),
                        yaw,
                        scale,
                        $"{shelfName}_{index + 1:00}");
                    index++;
                }
            }
        }

        private static void BuildNpcPositionMarkers(Transform parent)
        {
            GameObject markers = new GameObject(NpcPositionsName);
            markers.transform.SetParent(parent, false);

            CreateMarker(markers.transform, "Marker_StallSeller", StallSellerSpot);
            CreateMarker(markers.transform, "Marker_FlowerViewer", FlowerViewerSpot);
            CreateMarker(markers.transform, "Marker_TalkA", TalkSpotA);
            CreateMarker(markers.transform, "Marker_TalkB", TalkSpotB);
            CreateMarker(markers.transform, "Marker_CasualAlone", CasualAloneSpot);
            CreateMarker(markers.transform, "Marker_CasualSecond", CasualSecondSpot);
            CreateMarker(markers.transform, "Marker_FlowerCorner", FlowerCornerCenter + new Vector3(0f, 0.02f, 0f));
            CreateMarker(markers.transform, "Marker_StreetStall", StallCenter + new Vector3(0f, 0.02f, 0f));
        }

        private static void CreateMarker(Transform parent, string name, Vector3 worldPosition)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.position = worldPosition;
        }

        private static void PlaceProduce(
            Transform parent,
            string prefabPath,
            Vector3 localOffset,
            float localYaw,
            float uniformScale,
            string name,
            bool useProduceMaterial = true)
        {
            GameObject instance = PlaceProp(parent, prefabPath, localOffset, localYaw, Vector3.one * uniformScale, name, snapToGround: false);
            if (instance != null && useProduceMaterial && produceMaterial != null)
            {
                ApplyMaterial(instance, produceMaterial);
            }
        }

        private static void PlaceStallProp(
            Transform parent,
            string prefabPath,
            Vector3 localOffset,
            float localYaw,
            Vector3 scale,
            string name)
        {
            GameObject instance = PlaceProp(parent, prefabPath, localOffset, localYaw, scale, name);
            if (instance != null)
            {
                ApplyMaterial(instance, stallMaterial);
            }
        }

        private static GameObject PlaceProp(
            Transform parent,
            string prefabPath,
            Vector3 localOffset,
            float localYaw,
            Vector3 scale,
            string name,
            bool snapToGround = true)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[Level1OutdoorAmbientPropsBuilder] Missing prefab: " + prefabPath);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.localRotation = Quaternion.Euler(0f, localYaw, 0f);
            instance.transform.localScale = scale;
            instance.transform.localPosition = localOffset;
            if (snapToGround)
            {
                SnapToGround(instance, parent.position.y);
            }

            StripColliders(instance);
            return instance;
        }

        private static void EnsureMaterials()
        {
            stallMaterial = EnsureUrpLitMaterial(
                MaterialsRoot + "/Outdoor_Ambient_Stall_URP.mat",
                AssetDatabase.LoadAssetAtPath<Texture2D>(StallTexturePath),
                alphaClip: false);

            flowerPotMaterial = EnsureUrpLitMaterial(
                MaterialsRoot + "/Outdoor_FlowerPot_URP.mat",
                null,
                alphaClip: false);
            flowerPotMaterial.SetColor("_BaseColor", new Color(0.78f, 0.44f, 0.28f));

            produceMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Low-Poly Medieval Market/Materials/Vegetables market.mat");
        }

        private static Material EnsureUrpLitMaterial(string assetPath, Texture2D albedo, bool alphaClip)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogWarning("[Level1OutdoorAmbientPropsBuilder] URP Lit shader not found.");
                return existing;
            }

            if (existing != null && existing.shader == urpLit)
            {
                return existing;
            }

            Material material = existing != null ? existing : new Material(urpLit);
            material.shader = urpLit;
            if (albedo != null)
            {
                material.SetTexture("_BaseMap", albedo);
                material.SetTexture("_MainTex", albedo);
            }

            material.SetColor("_BaseColor", Color.white);
            if (alphaClip)
            {
                material.SetFloat("_AlphaClip", 1f);
                material.SetFloat("_Cutoff", 0.45f);
                material.EnableKeyword("_ALPHATEST_ON");
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.renderQueue = (int)RenderQueue.AlphaTest;
                material.doubleSidedGI = true;
                material.SetFloat("_Cull", (float)CullMode.Off);
            }
            else
            {
                material.DisableKeyword("_ALPHATEST_ON");
                material.SetFloat("_AlphaClip", 0f);
                material.renderQueue = -1;
            }

            if (existing == null)
            {
                EnsureFolder(Path.GetDirectoryName(assetPath)?.Replace('\\', '/'));
                AssetDatabase.CreateAsset(material, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void ApplyMaterial(GameObject root, Material material)
        {
            if (root == null || material == null)
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

                Material[] mats = renderer.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                {
                    mats[m] = material;
                }

                renderer.sharedMaterials = mats;
            }
        }

        private static void SnapToGround(GameObject instance, float targetGroundY)
        {
            if (instance == null)
            {
                return;
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                Vector3 position = instance.transform.position;
                position.y = targetGroundY;
                instance.transform.position = position;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float lift = targetGroundY - bounds.min.y;
            if (Mathf.Abs(lift) > 0.0001f)
            {
                instance.transform.position += new Vector3(0f, lift, 0f);
            }
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    Object.DestroyImmediate(colliders[i]);
                }
            }
        }
    }
}
#endif
