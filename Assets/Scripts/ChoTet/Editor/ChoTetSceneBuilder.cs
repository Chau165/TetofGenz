using System.Collections.Generic;
using ChoTet.Data;
using ChoTet.Level1;
using ChoTet.Level1.Editor;
using ChoTet.Level1.HometownArrival;
using ChoTet.Runtime;
using UnityEditor;

using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChoTet.Editor
{
    public static class ChoTetSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/ChoTet_VerticalSlice.unity";
        private const string SettingsRoot = "Assets/Settings/ChoTet";
        private const string ItemsRoot = SettingsRoot + "/Items";
        private const string VendorsRoot = SettingsRoot + "/Vendors";
        private const string MaterialsRoot = SettingsRoot + "/Materials";
        private const string ArtRoot = "Assets/Art/ChoTetEnvironment";
        private const string FestivalDecorRoot = ArtRoot + "/FestivalDecor";
        private const string SkyRoot = ArtRoot + "/Sky";
        private const string TerrainRoot = ArtRoot + "/Terrain/RockyTerrain02";
        private const string TerrainTexturesRoot = TerrainRoot + "/textures";
        private const string GrassPathTexturesRoot = ArtRoot + "/Terrain/GrassPath2/textures";
        private const string DirtTexturesRoot = ArtRoot + "/Terrain/Dirt2/textures";
        private const string StonyDirtTexturesRoot = ArtRoot + "/Terrain/StonyDirtPath/textures";
        private const string RedMudTexturesRoot = ArtRoot + "/Terrain/RedMudStones/textures";
        private const string UltimateNatureRoot = ArtRoot + "/UltimateStylizedNature";
        private const string UltimateNatureFbxRoot = UltimateNatureRoot + "/FBX";
        private const string LowPolyNaturePrefabsRoot = "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs";
        private const string LowPolyNatureMaterialsRoot = "Assets/lowpoly-nature/lowpoly-nature/material";
        private const string LowPolyUrbanPrefabsRoot = "Assets/Assets/AIRIDev/Low-Poly Urban Assets/Prefabs";
        private const string TempleCookingPotPath = "Assets/Art/Temple/Cooking Pot.glb";
        
        private const string ExplorerModelPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped.glb";
        private const string ExplorerWalkPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped_Animation_Walking_withSkin.glb";
        private const string ExplorerRunPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped_Animation_Running_withSkin.glb";
        private const string ExplorerAltRunPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped_Animation_Run_03_withSkin.glb";
        private const string ExplorerControllerPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped.controller";
        
        private const string ExplorerPaletteMaterialPath = "Assets/Settings/ChoTet/Materials/Explorer_Palette.mat";
        private const string ExplorerPaletteMeshPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped_Palette.asset";
        private static readonly Color32 ExplorerHairDark = new Color32(61, 49, 43, 255);
        private static readonly Color32 ExplorerHairLight = new Color32(82, 67, 59, 255);
        private static readonly Color32 ExplorerJacketDark = new Color32(166, 75, 68, 255);
        private static readonly Color32 ExplorerJacketLight = new Color32(196, 115, 106, 255);
        private static readonly Color32 ExplorerShirt = new Color32(222, 213, 197, 255);
        private static readonly Color32 ExplorerPantsDark = new Color32(78, 80, 49, 255);
        private static readonly Color32 ExplorerPantsLight = new Color32(107, 109, 70, 255);
        private static readonly Color32 ExplorerSkin = new Color32(226, 179, 163, 255);
        private static readonly Color32 ExplorerSkinShadow = new Color32(210, 152, 133, 255);
        private static readonly Color32 ExplorerSandals = new Color32(115, 86, 70, 255);
        private static readonly Color32 ExplorerEyes = new Color32(59, 88, 102, 255);
        private static readonly Color32 ExplorerEyeWhite = new Color32(242, 239, 234, 255);
        private static readonly Color32 ExplorerMouth = new Color32(182, 118, 114, 255);
        private const string ExplorerIdleClipPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped_IdlePose.anim";
        private const string MotherModelPath = "Assets/Characters/Mother/Meshy_AI_Portrait_of_a_Grandmo_0613065903_generate.glb";
        private const string MotherPrefabPath = "Assets/Prefabs/NPCs/MotherNPC.prefab";
        private const string MotherPaletteMaterialPath = "Assets/Settings/ChoTet/Materials/Mother_VertexPalette.mat";
        private const string MotherPaletteMeshPath = "Assets/Characters/Mother/MotherNPC_VertexPalette.asset";
        private static readonly Color32 MotherHairDark = new Color32(94, 91, 90, 255);
        private static readonly Color32 MotherHairLight = new Color32(168, 163, 161, 255);
        private static readonly Color32 MotherJacketDark = new Color32(148, 67, 54, 255);
        private static readonly Color32 MotherJacketLight = new Color32(188, 93, 75, 255);
        private static readonly Color32 MotherShirt = new Color32(214, 205, 184, 255);
        private static readonly Color32 MotherFloralDetail = new Color32(122, 94, 67, 255);
        private static readonly Color32 MotherPantsDark = new Color32(67, 77, 58, 255);
        private static readonly Color32 MotherPantsLight = new Color32(94, 108, 82, 255);
        private static readonly Color32 MotherSkin = new Color32(212, 167, 150, 255);
        private static readonly Color32 MotherSkinShadow = new Color32(181, 130, 115, 255);
        private static readonly Color32 MotherAccessory = new Color32(133, 110, 93, 255);
        private static readonly Color32 MotherSandals = new Color32(110, 82, 68, 255);
        private static readonly Color32 MotherEyes = new Color32(59, 88, 102, 255);
        private const string HondaPrefabsRoot = "Assets/Honda Motor Co., Ltd/Super Cub C125/Prefabs";
        private const string RacingCarNo1201PrefabPath = "Assets/Azerilo/Car Model No.1201 Asset/Prefab/Car_NO.1201.prefab";
        private const string ArcadeFreeRacingCarPrefabPath = "Assets/ARCADE - FREE Racing Car/Prefabs (With Colliders)/Free Racing Car.prefab";
        private const string FemalePeasantPrefabPath = "Assets/Polytope Studio/Lowpoly_Characters/Prefabs/Modular_NPC/Peasants_Citizens/PT_Female_Modular_Free_Pack.prefab";
        private const string LunarBlessingPolePrefabPath = FestivalDecorRoot + "/Meshy_AI_Lunar_New_Year_Blessi_0706154007_texture.glb";
        private const string BuffaloPrefabPath = FestivalDecorRoot + "/Meshy_AI_Low_Poly_Buffalo_0706163110_texture.glb";
        private const string FortuneTreePrefabPath = FestivalDecorRoot + "/Meshy_AI_Fortune_Tree_in_Bloom_0707025950_texture.glb";
        private const string CoHuongGrandmotherPrefabPath = FestivalDecorRoot + "/Meshy_AI_Grandmother_in_red_fl_0708034116_texture.glb";
        private const string DuaHauVendorPrefabPath = FestivalDecorRoot + "/banduahau.glb";
        private const string HoaMaiVendorPrefabPath = FestivalDecorRoot + "/banhoamai.glb";
        private const string ThitVendorPrefabPath = FestivalDecorRoot + "/banthit.glb";
        private const string QuaTetVendorPrefabPath = FestivalDecorRoot + "/banquatet.glb";

        public static void BuildVerticalSlice()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += BuildVerticalSlice;
                Debug.Log("Stopped Play Mode before rebuilding ChoTet_VerticalSlice.");
                return;
            }

            AssetDatabase.Refresh();
            EnsureFolder("Assets/Settings");
            EnsureFolder(SettingsRoot);
            EnsureFolder(ItemsRoot);
            EnsureFolder(VendorsRoot);
            EnsureFolder(MaterialsRoot);
            EnsureFolder("Assets/Characters");
            EnsureFolder("Assets/Characters/Mother");
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/NPCs");

            Dictionary<string, ItemDefinition> items = CreateItems();
            Dictionary<string, VendorProfile> vendors = CreateVendors(items);
            Dictionary<string, Material> materials = CreateMaterials();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ChoTet_VerticalSlice";

            GameObject worldRoot = new GameObject("ChoTetWorld");
            GameObject systemsRoot = new GameObject("Systems");

            ConfigureEnvironmentLighting();
            CreateLight(worldRoot.transform);
            CreateGroundAndDecor(worldRoot.transform, materials);

            Transform homeSpawnPoint = CreateMarker("HomeSpawnPoint", new Vector3(6f, 0.15f, 12f), worldRoot.transform);

            GameHudController hud = new GameObject("GameHud").AddComponent<GameHudController>();
            hud.transform.SetParent(systemsRoot.transform, false);

            DialogueManager dialogueManager = new GameObject("DialogueManager").AddComponent<DialogueManager>();
            dialogueManager.transform.SetParent(systemsRoot.transform, false);
            dialogueManager.hud = hud;

            PlayerTopDownController player = CreatePlayer(homeSpawnPoint.position);
            player.transform.SetParent(worldRoot.transform, true);

            TopDownCameraFollow cameraFollow = CreateCamera(player.transform);
            cameraFollow.transform.SetParent(worldRoot.transform, true);

            GameFlowController flow = new GameObject("GameFlowController").AddComponent<GameFlowController>();
            flow.transform.SetParent(systemsRoot.transform, false);
            flow.hud = hud;
            flow.dialogueManager = dialogueManager;
            flow.player = player;
            flow.homeSpawnPoint = homeSpawnPoint;
            flow.requiredItems = new[]
            {
                items["banh_mut"],
                items["dua_hau"],
                items["hoa_mai"],
                items["thit"],
                items["qua_bieu"]
            };

            TempleQuestController templeQuest = new GameObject("TempleQuestController").AddComponent<TempleQuestController>();
            templeQuest.transform.SetParent(systemsRoot.transform, false);
            templeQuest.flow = flow;
            templeQuest.hud = hud;

            if (systemsRoot.GetComponent<ChoTetOutdoorEnvironment>() == null)
            {
                systemsRoot.AddComponent<ChoTetOutdoorEnvironment>();
            }

            CreateHomeArea(worldRoot.transform, materials);
            CreateGardenArea(worldRoot.transform, materials);
            CreateRoadArea(worldRoot.transform, materials);
            CreateMarketArea(worldRoot.transform, vendors, materials);
            CreateTempleArea(worldRoot.transform, materials);
            CreateStoryCharacters(worldRoot.transform);
            CreateZoneTriggers(worldRoot.transform);
            CreateCrowdedRoadTraffic(worldRoot.transform, materials);
            CreateLunarRoadDecor(worldRoot.transform);
            CreateFortuneTreePots(worldRoot.transform);
            CreateBuffaloHerd(worldRoot.transform);

            string[] buildScenes = { ScenePath };
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = BuildEditorScenes(buildScenes);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Cho Tet vertical slice built at " + ScenePath);
        }

        private static Dictionary<string, ItemDefinition> CreateItems()
        {
            Dictionary<string, ItemDefinition> items = new Dictionary<string, ItemDefinition>();
            items["banh_mut"] = CreateOrUpdateAsset<ItemDefinition>(
                ItemsRoot + "/BanhMut.asset",
                asset =>
                {
                    asset.itemId = "banh_mut";
                    asset.displayName = "Khay mứt Tết";
                    asset.description = "Khay mứt Tết dùng để tiếp khách và trò chuyện cùng gia đình.";
                    asset.category = "snack";
                    asset.basePrice = 110000;
                    asset.capacityCost = 2;
                    asset.minQuality = 62;
                    asset.maxQuality = 88;
                    asset.presentationBonus = 12;
                    asset.traditionBonus = 20;
                    asset.socialBonus = 10;
                    asset.luckWeight = 0;
                    asset.qualityFocus = "cách bày trí";
                    asset.perishable = false;
                }
            );

            items["dua_hau"] = CreateOrUpdateAsset<ItemDefinition>(
                ItemsRoot + "/DuaHau.asset",
                asset =>
                {
                    asset.itemId = "dua_hau";
                    asset.displayName = "Dưa hấu";
                    asset.description = "Trái cây may mắn ngày Tết với vị ngọt lành và ngập tràn may mắn.";
                    asset.category = "fruit";
                    asset.basePrice = 135000;
                    asset.capacityCost = 3;
                    asset.minQuality = 55;
                    asset.maxQuality = 92;
                    asset.presentationBonus = 14;
                    asset.traditionBonus = 16;
                    asset.socialBonus = 4;
                    asset.luckWeight = 12;
                    asset.qualityFocus = "độ ngọt và độ đỏ may mắn";
                    asset.perishable = true;
                }
            );

            items["hoa_mai"] = CreateOrUpdateAsset<ItemDefinition>(
                ItemsRoot + "/HoaMai.asset",
                asset =>
                {
                    asset.itemId = "hoa_mai";
                    asset.displayName = "Hoa mai";
                    asset.description = "Nhành hoa mai vàng mang đậm không khí Tết xuân sang.";
                    asset.category = "flower";
                    asset.basePrice = 185000;
                    asset.capacityCost = 2;
                    asset.minQuality = 60;
                    asset.maxQuality = 95;
                    asset.presentationBonus = 22;
                    asset.traditionBonus = 22;
                    asset.socialBonus = 6;
                    asset.luckWeight = 8;
                    asset.qualityFocus = "độ tươi của hoa";
                    asset.perishable = true;
                }
            );

            items["thit"] = CreateOrUpdateAsset<ItemDefinition>(
                ItemsRoot + "/Thit.asset",
                asset =>
                {
                    asset.itemId = "thit";
                    asset.displayName = "Thịt lợn";
                    asset.description = "Nguyên liệu tươi ngon cho mâm cơm sum vầy gia đình.";
                    asset.category = "meat";
                    asset.basePrice = 155000;
                    asset.capacityCost = 2;
                    asset.minQuality = 58;
                    asset.maxQuality = 90;
                    asset.presentationBonus = 9;
                    asset.traditionBonus = 14;
                    asset.socialBonus = 3;
                    asset.luckWeight = 0;
                    asset.qualityFocus = "độ tươi ngon";
                    asset.perishable = true;
                }
            );

            items["qua_bieu"] = CreateOrUpdateAsset<ItemDefinition>(
                ItemsRoot + "/QuaBieu.asset",
                asset =>
                {
                    asset.itemId = "qua_bieu";
                    asset.displayName = "Giỏ quà Tết";
                    asset.description = "Giỏ quà Tết ấm cúng thể hiện sự trân trọng đến người thân.";
                    asset.category = "gift";
                    asset.basePrice = 165000;
                    asset.capacityCost = 1;
                    asset.minQuality = 65;
                    asset.maxQuality = 90;
                    asset.presentationBonus = 18;
                    asset.traditionBonus = 8;
                    asset.socialBonus = 20;
                    asset.luckWeight = 2;
                    asset.qualityFocus = "cách trình bày và ý nghĩa biểu tượng";
                    asset.perishable = false;
                }
            );

            return items;
        }

        private static Dictionary<string, VendorProfile> CreateVendors(Dictionary<string, ItemDefinition> items)
        {
            Dictionary<string, VendorProfile> vendors = new Dictionary<string, VendorProfile>();

            vendors["co_huong_banh"] = CreateOrUpdateAsset<VendorProfile>(
                VendorsRoot + "/CoHuongBanh.asset",
                asset =>
                {
                    asset.vendorId = "co_huong_banh";
                    asset.displayName = "Quầy bánh kẹo Cô Hương";
                    asset.greeting = "Mứt bánh tươi ngon đây con. Bàn trà ngày Tết thịnh soạn là điều quý giá nhất.";
                    asset.stockedItem = items["banh_mut"];
                    asset.patience = 64;
                    asset.greed = 0.32f;
                    asset.honesty = 0.88f;
                    asset.familiarity = 1;
                    asset.mood = "warm";
                    asset.favoriteApproach = "soft_bargain";
                    asset.zoneName = "Market";
                }
            );

            vendors["chu_ba_dua"] = CreateOrUpdateAsset<VendorProfile>(
                VendorsRoot + "/ChuBaDua.asset",
                asset =>
                {
                    asset.vendorId = "chu_ba_dua";
                    asset.displayName = "Quầy hoa quả Chú Ba";
                    asset.greeting = "Thích quả dưa hấu nào cứ gõ xem thử, nhưng dưa may mắn đầu năm thì giá cả đi đôi với chất lượng nhé cháu.";
                    asset.stockedItem = items["dua_hau"];
                    asset.patience = 58;
                    asset.greed = 0.27f;
                    asset.honesty = 0.78f;
                    asset.familiarity = 0;
                    asset.mood = "direct";
                    asset.favoriteApproach = "inspect_goods";
                    asset.zoneName = "Market";
                }
            );

            vendors["co_lan_hoa"] = CreateOrUpdateAsset<VendorProfile>(
                VendorsRoot + "/CoLanHoa.asset",
                asset =>
                {
                    asset.vendorId = "co_lan_hoa";
                    asset.displayName = "Quầy hoa mai Cô Lan";
                    asset.greeting = "Mua hoa là mua cái lộc xuân. Nhìn nhành mai nở rộ tươi tắn thế này là biết Tết ấm áp rồi.";
                    asset.stockedItem = items["hoa_mai"];
                    asset.patience = 55;
                    asset.greed = 0.38f;
                    asset.honesty = 0.82f;
                    asset.familiarity = 0;
                    asset.mood = "alert";
                    asset.favoriteApproach = "charm";
                    asset.zoneName = "Market";
                }
            );

            vendors["chu_tu_thit"] = CreateOrUpdateAsset<VendorProfile>(
                VendorsRoot + "/ChuTuThit.asset",
                asset =>
                {
                    asset.vendorId = "chu_tu_thit";
                    asset.displayName = "Quầy thịt Chú Tư";
                    asset.greeting = "Thịt tươi ngon nhất thì hết nhanh lắm đấy. Đến muộn là không còn miếng ngon đâu con.";
                    asset.stockedItem = items["thit"];
                    asset.patience = 61;
                    asset.greed = 0.3f;
                    asset.honesty = 0.84f;
                    asset.familiarity = 1;
                    asset.mood = "busy";
                    asset.favoriteApproach = "compare_price";
                    asset.zoneName = "Market";
                }
            );

            vendors["chi_sau_qua"] = CreateOrUpdateAsset<VendorProfile>(
                VendorsRoot + "/ChiSauQua.asset",
                asset =>
                {
                    asset.vendorId = "chi_sau_qua";
                    asset.displayName = "Quầy quà Tết Chị Sáu";
                    asset.greeting = "Quà tặng Tết cốt ở tấm lòng của gia đình gửi gắm, chứ đâu chỉ nằm ở cái túi tiền.";
                    asset.stockedItem = items["qua_bieu"];
                    asset.patience = 67;
                    asset.greed = 0.34f;
                    asset.honesty = 0.76f;
                    asset.familiarity = 0;
                    asset.mood = "talkative";
                    asset.favoriteApproach = "bulk_buy";
                    asset.zoneName = "Market";
                }
            );

            return vendors;
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            Dictionary<string, Material> materials = new Dictionary<string, Material>();
            materials["grass"] = CreateTexturedMaterial("Grass_Ground.mat", GrassPathTexturesRoot + "/grass_path_2_diff_2k.jpg", Hex("#93C874"), new Vector2(5f, 5f), 0.12f);
            materials["garden"] = CreateTexturedMaterial("Garden_Ground.mat", GrassPathTexturesRoot + "/grass_path_2_diff_2k.jpg", Hex("#88BE69"), new Vector2(4f, 4f), 0.12f);
            materials["home_dirt"] = CreateTexturedMaterial("Home_Dirt.mat", DirtTexturesRoot + "/dirt_diff_2k.jpg", Hex("#B28A64"), new Vector2(4.2f, 4.2f), 0.22f);
            materials["road"] = CreateTexturedMaterial("Road_Ground.mat", StonyDirtTexturesRoot + "/stony_dirt_path_diff_2k.jpg", Hex("#AA8C68"), new Vector2(4.2f, 11f), 0.26f);
            materials["market"] = CreateTexturedMaterial("Market_Ground.mat", RedMudTexturesRoot + "/red_mud_stones_diff_2k.jpg", Hex("#AE7254"), new Vector2(6.5f, 5f), 0.2f);
            materials["accent"] = CreateMaterial("Accent_Red.mat", Hex("#9E5338"));
            materials["dark"] = CreateTexturedMaterial("Dark_Brown.mat", TerrainTexturesRoot + "/rocky_terrain_02_diff_2k.jpg", Hex("#5F4C3F"), new Vector2(2f, 14f), 0.18f);
            materials["wall_light"] = CreateMaterial("Wall_Light.mat", Hex("#D1B499"));
            materials["wall_shadow"] = CreateMaterial("Wall_Shadow.mat", Hex("#A88E76"));
            materials["roof_light"] = CreateMaterial("Roof_Light.mat", Hex("#9E5338"));
            materials["roof_dark"] = CreateMaterial("Roof_Dark.mat", Hex("#7A3E28"));
            materials["wood"] = CreateMaterial("Wood_Warm.mat", Hex("#543928"));
            materials["wood_dark"] = CreateMaterial("Wood_Dark.mat", Hex("#614330"));
            materials["door_dark"] = CreateMaterial("Door_Dark.mat", Hex("#3D2B20"));
            materials["glass"] = CreateMaterial("Glass_Blue.mat", Hex("#5A8BA8"));
            materials["foundation"] = CreateMaterial("Foundation_Stone.mat", Hex("#5C6670"));
            materials["foliage"] = CreateMaterial("Foliage_Green.mat", Hex("#5C8C4A"));
            materials["flower"] = CreateMaterial("Flower_Yellow.mat", Hex("#F1C84E"));
            materials["lantern"] = CreateMaterial("Lantern_Gold.mat", Hex("#E3B64D"));
            materials["market_red"] = CreateMaterial("Market_Red.mat", new Color(0.78f, 0.24f, 0.2f));
            materials["market_green"] = CreateMaterial("Market_Green.mat", new Color(0.31f, 0.56f, 0.25f));
            materials["produce_red"] = CreateMaterial("Produce_Red.mat", new Color(0.76f, 0.22f, 0.22f));
            materials["produce_yellow"] = CreateMaterial("Produce_Yellow.mat", new Color(0.91f, 0.73f, 0.24f));
            materials["produce_orange"] = CreateMaterial("Produce_Orange.mat", new Color(0.88f, 0.5f, 0.18f));
            materials["produce_purple"] = CreateMaterial("Produce_Purple.mat", new Color(0.49f, 0.31f, 0.63f));
            materials["meat"] = CreateMaterial("Meat_Rose.mat", new Color(0.74f, 0.39f, 0.39f));
            materials["gift"] = CreateMaterial("Gift_Red.mat", new Color(0.7f, 0.16f, 0.16f));
            materials["coin"] = CreateMaterial("Coin_Gold.mat", new Color(0.91f, 0.74f, 0.24f));
            materials["rock"] = CreateTexturedMaterial("Rock_Edge.mat", TerrainTexturesRoot + "/rocky_terrain_02_diff_2k.jpg", Hex("#8C7A6A"), new Vector2(2.6f, 2.6f), 0.24f);
            materials["terrain_edge"] = CreateTexturedMaterial("Terrain_Edge.mat", DirtTexturesRoot + "/dirt_diff_2k.jpg", Hex("#8A765F"), new Vector2(3.4f, 3.4f), 0.2f);
            materials["urban_path"] = CreateTexturedMaterial("Urban_Path.mat", StonyDirtTexturesRoot + "/stony_dirt_path_diff_2k.jpg", Hex("#B7A58E"), new Vector2(3.4f, 7.5f), 0.18f);
            return materials;
        }

        private static void ConfigureEnvironmentLighting()
        {
            Material daySky = AssetDatabase.LoadAssetAtPath<Material>(ChoTetMarketEnvironment.HometownDaySkyboxAssetPath);
            if (daySky == null)
            {
                daySky = CreateSkyboxMaterial("ChoTet_SunsetSkybox.mat", SkyRoot + "/plains_sunset_2k.exr");
            }

            ChoTetMarketEnvironment.ApplyHometownArrivalRenderSettings(daySky, 38f, 105f);
            DynamicGI.UpdateEnvironment();
        }

        private static void CreateLight(Transform parent)
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(28f, -38f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.18f;
            light.color = new Color(1f, 0.78f, 0.61f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.78f;
        }

        private static void CreateGroundAndDecor(Transform parent, Dictionary<string, Material> materials)
        {
            CreateGround("MainGround", parent, new Vector3(0f, -0.55f, 46f), new Vector3(58f, 1f, 112f), materials["grass"]);
            CreateGround("HomeGround", parent, new Vector3(-8f, -0.5f, 8f), new Vector3(26f, 1f, 24f), materials["home_dirt"]);
            CreateGround("HomeGrassBlend", parent, new Vector3(-2f, -0.42f, 10.5f), new Vector3(12f, 0.12f, 8.5f), materials["garden"]);
            CreateGround("GardenGround", parent, new Vector3(0f, -0.49f, 28f), new Vector3(44f, 1f, 24f), materials["garden"]);
            // Placeholder only — DressTrafficRoad replaces this with the Level-1 endless asphalt deck.
            CreateGround("RoadGround", parent, new Vector3(0f, 0.06f, 54f), new Vector3(400f, 0.18f, 16f), materials["road"]);
            CreateGround("MarketGround", parent, new Vector3(0f, -0.47f, 94f), new Vector3(54f, 1f, 44f), materials["grass"]);
            // Stop the dirt path before the market so the market pad is fully grass.
            CreateGround("MainPath", parent, new Vector3(0f, -0.38f, 38f), new Vector3(10f, 0.15f, 68f), materials["road"]);
            CreateGround("SideWalkLeft", parent, new Vector3(-22f, -0.3f, 50f), new Vector3(2f, 0.18f, 96f), materials["terrain_edge"]);
            CreateGround("SideWalkRight", parent, new Vector3(22f, -0.3f, 50f), new Vector3(2f, 0.18f, 96f), materials["terrain_edge"]);
            CreateGround("MarketApron", parent, new Vector3(0f, -0.32f, 78f), new Vector3(18f, 0.12f, 9f), materials["grass"]);
            CreateTerrainBorders(parent, materials);
        }

        private static void CreateHomeArea(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("HomeArea");
            root.transform.SetParent(parent, false);

            CreateStylizedHome(root.transform, new Vector3(-16f, 0f, 2f), "HomeHouse", materials);

            GameObject bench = InstantiatePrefab(
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Bench.prefab",
                root.transform,
                new Vector3(-4f, 0f, 7f),
                Quaternion.Euler(0f, -20f, 0f),
                Vector3.one * 1.1f,
                "HomeBench"
            );
            ApplyMaterialRecursive(bench, materials["wood"]);

            for (int i = 0; i < 4; i++)
            {
                GameObject fence = InstantiatePrefab(
                    "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Fence.prefab",
                    root.transform,
                    new Vector3(-20f + (i * 4.2f), 0f, 15f),
                    Quaternion.identity,
                    Vector3.one * 1.2f,
                    "HomeFence_Front_" + i
                );
                ApplyMaterialRecursive(fence, materials["wood"]);
            }

            for (int i = 0; i < 3; i++)
            {
                GameObject sideFence = InstantiatePrefab(
                    "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Fence.prefab",
                    root.transform,
                    new Vector3(-22f, 0f, 2f + (i * 4.2f)),
                    Quaternion.Euler(0f, 90f, 0f),
                    Vector3.one * 1.15f,
                    "HomeFence_Left_" + i
                );
                ApplyMaterialRecursive(sideFence, materials["wood"]);
            }

            CreateGround("Porch", root.transform, new Vector3(-9f, 0.05f, 10f), new Vector3(12f, 0.15f, 5f), materials["accent"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower Pot.prefab", root.transform, new Vector3(1f, 0f, 12f), Quaternion.identity, Vector3.one * 1.35f, "HomeFlowerPot_1"), materials["flower"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower Pot.prefab", root.transform, new Vector3(-2.2f, 0f, 11.6f), Quaternion.identity, Vector3.one * 1.2f, "HomeFlowerPot_2"), materials["flower"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_6.prefab", root.transform, new Vector3(-20f, 0f, -2.5f), Quaternion.Euler(0f, 16f, 0f), Vector3.one * 1.55f, "HomeLargeTree"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_2.prefab", root.transform, new Vector3(-24f, 0f, 9f), Quaternion.Euler(0f, 12f, 0f), Vector3.one * 1.35f, "HomeTree_2"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Pine Tree_3.prefab", root.transform, new Vector3(8f, 0f, 2f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.28f, "HomeTree_3"), materials["foliage"]);
            InstantiatePrefab(HondaPrefabsRoot + "/C125 - MM.prefab", root.transform, new Vector3(4.5f, 0.02f, 9f), Quaternion.Euler(0f, -48f, 0f), Vector3.one * 1.1f, "HomeScooter");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/rock3.prefab", root.transform, new Vector3(-22.5f, 0f, 13f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 1.55f, "HomeRockEdge");
            SpawnScatter(root.transform, LowPolyNaturePrefabsRoot + "/bush2.prefab", "HomeBush", new[]
            {
                new Vector3(-8.5f,0f,14.5f), new Vector3(-4.8f,0f,15.2f), new Vector3(-0.8f,0f,15.1f), new Vector3(3.8f,0f,14.6f)
            }, 1.12f);
            SpawnScatter(root.transform, LowPolyNaturePrefabsRoot + "/grass2.prefab", "HomeGrass", new[]
            {
                new Vector3(-5f, 0f, 13f), new Vector3(-3f, 0f, 14f), new Vector3(-1f, 0f, 13.4f), new Vector3(1f, 0f, 14.4f),
                new Vector3(3.5f, 0f, 13.7f), new Vector3(-7.5f, 0f, 10.6f), new Vector3(6f, 0f, 11f), new Vector3(-9f, 0f, 12.6f),
                new Vector3(-10f,0f,15.2f), new Vector3(-6f,0f,16f), new Vector3(-2f,0f,16.4f), new Vector3(2.5f,0f,16.1f),
                new Vector3(6.5f,0f,15f), new Vector3(-11f,0f,8.2f), new Vector3(-6.2f,0f,8.6f), new Vector3(-0.5f,0f,9.3f)
            }, 1.18f);
        }

        private static void CreateGardenArea(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("GardenArea");
            root.transform.SetParent(parent, false);

            InstantiatePrefab(UltimateNatureFbxRoot + "/NormalTree_3.fbx", root.transform, new Vector3(-3f, 0f, 27f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.85f, "GardenHeroTree");
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", root.transform, new Vector3(8f, 0f, 26f), Quaternion.identity, Vector3.one * 1.1f, "GardenLantern"), materials["lantern"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower Pot.prefab", root.transform, new Vector3(-5f, 0f, 18f), Quaternion.identity, Vector3.one * 1.35f, "FlowerPot"), materials["flower"]);
            SpawnScatter(root.transform, UltimateNatureFbxRoot + "/Bush_Large.fbx", "GardenBushLarge", new[]
            {
                new Vector3(-12f, 0f, 20f), new Vector3(11f, 0f, 21f), new Vector3(-14f, 0f, 34f),
                new Vector3(13f, 0f, 31f), new Vector3(8f, 0f, 36f)
            }, 1.25f);
            SpawnScatter(root.transform, UltimateNatureFbxRoot + "/Flower_5_Clump.fbx", "GardenFlowers", new[]
            {
                new Vector3(7f, 0f, 30f), new Vector3(-6f, 0f, 34f), new Vector3(4f, 0f, 19f), new Vector3(-9f, 0f, 24f)
            }, 1.2f);
            SpawnScatter(root.transform, LowPolyNaturePrefabsRoot + "/rock2.prefab", "GardenRock", new[]
            {
                new Vector3(-16f, 0f, 28f), new Vector3(15f, 0f, 27f), new Vector3(-10f, 0f, 36f)
            }, 1.25f);
            SpawnScatter(root.transform, UltimateNatureFbxRoot + "/Grass_Small.fbx", "GardenGrassSmall", new[]
            {
                new Vector3(-11f,0f,18f), new Vector3(-8f,0f,19f), new Vector3(-4f,0f,20f), new Vector3(0f,0f,20f), new Vector3(4f,0f,19f),
                new Vector3(8f,0f,20f), new Vector3(12f,0f,21f), new Vector3(-13f,0f,24f), new Vector3(-9f,0f,24f), new Vector3(-5f,0f,24f),
                new Vector3(-1f,0f,24f), new Vector3(3f,0f,24f), new Vector3(7f,0f,24f), new Vector3(11f,0f,25f), new Vector3(-12f,0f,30f),
                new Vector3(-8f,0f,31f), new Vector3(-4f,0f,32f), new Vector3(1f,0f,32f), new Vector3(6f,0f,33f), new Vector3(11f,0f,34f)
            }, 1.18f);
        }

        public static void ApplyHometownGardenLookMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += ApplyHometownGardenLookMenu;
                Debug.Log("Stopped Play Mode before applying hometown garden look.");
                return;
            }

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("ChoTet_VerticalSlice.unity", System.StringComparison.OrdinalIgnoreCase))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Material daySky = AssetDatabase.LoadAssetAtPath<Material>(ChoTetMarketEnvironment.HometownDaySkyboxAssetPath);
            ChoTetMarketEnvironment.ApplyHometownArrivalRenderSettings(daySky, 38f, 105f);

            Transform worldRoot = FindSceneTransform("ChoTetWorld");
            if (worldRoot != null)
            {
                ApplyMarketGrassGround(worldRoot);
                DressHoaMaiStall(worldRoot);
                DressQuaBieuStall(worldRoot);
                DressTrafficRoad(worldRoot);
                CreateLunarRoadDecor(worldRoot);
                CreateFortuneTreePots(worldRoot);
                CreateBuffaloHerd(worldRoot);
                ChoTetWorldBackdropDressing.Apply(worldRoot);
                ChoTetFloralDressing.Apply(worldRoot);
                ChoTetMarketCrowdBuilder.Build(worldRoot);
                RefreshChoTetMotherVisual(worldRoot);
            }

            GameObject systems = GameObject.Find("Systems");
            if (systems != null)
            {
                ChoTetOutdoorEnvironment outdoor = systems.GetComponent<ChoTetOutdoorEnvironment>();
                if (outdoor == null)
                {
                    outdoor = systems.AddComponent<ChoTetOutdoorEnvironment>();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Hometown world backdrop applied to " + scene.path);
        }

        public static void ApplyLunarRoadPolesMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += ApplyLunarRoadPolesMenu;
                Debug.Log("Stopped Play Mode before applying lunar road poles.");
                return;
            }

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("ChoTet_VerticalSlice.unity", System.StringComparison.OrdinalIgnoreCase))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Transform worldRoot = FindSceneTransform("ChoTetWorld");
            if (worldRoot == null)
            {
                Debug.LogWarning("ChoTetWorld was not found in the active scene.");
                return;
            }

            CreateLunarRoadDecor(worldRoot);
            CreateBuffaloHerd(worldRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Applied lunar road poles to " + scene.path);
        }

        public static void ApplyBuffaloHerdMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += ApplyBuffaloHerdMenu;
                Debug.Log("Stopped Play Mode before applying buffalo herd.");
                return;
            }

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("ChoTet_VerticalSlice.unity", System.StringComparison.OrdinalIgnoreCase))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Transform worldRoot = FindSceneTransform("ChoTetWorld");
            if (worldRoot == null)
            {
                Debug.LogWarning("ChoTetWorld was not found in the active scene.");
                return;
            }

            CreateBuffaloHerd(worldRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Applied buffalo herd to " + scene.path);
        }

        public static void ApplyFortuneTreePotsMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += ApplyFortuneTreePotsMenu;
                Debug.Log("Stopped Play Mode before applying fortune tree pots.");
                return;
            }

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.path.EndsWith("ChoTet_VerticalSlice.unity", System.StringComparison.OrdinalIgnoreCase))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Transform worldRoot = FindSceneTransform("ChoTetWorld");
            if (worldRoot == null)
            {
                Debug.LogWarning("ChoTetWorld was not found in the active scene.");
                return;
            }

            CreateFortuneTreePots(worldRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Applied fortune tree pots to " + scene.path);
        }

        private static void DressHoaMaiStall(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            Transform stall = FindChildRecursive(worldRoot, "HoaMaiStall");
            if (stall == null)
            {
                return;
            }

            DestroyNamedDescendants(stall, "HoaDecor_1");
            DestroyNamedDescendants(stall, "HoaDecor_2");
            DestroyNamedDescendants(stall, "HoaMaiStall_Flowers");

            Transform oldVisual = stall.Find("HoaMaiStall_Visual");
            if (oldVisual != null)
            {
                Object.DestroyImmediate(oldVisual.gameObject);
            }

            GameObject visual = InstantiatePrefab(
                "Assets/Low-Poly Medieval Market/Prefabs/stand_01.prefab",
                stall,
                new Vector3(12f, 0f, 86f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * 1.35f,
                "HoaMaiStall_Visual");
            Material wood = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Wood_Warm.mat");
            if (visual != null && wood != null)
            {
                ApplyMaterialRecursive(visual, wood);
            }

            Material yellow = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Flower_Yellow.mat");
            if (yellow == null)
            {
                yellow = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Tet_FlowerYellow.mat");
            }

            if (yellow == null)
            {
                yellow = CreateMaterial("Flower_Yellow.mat", Hex("#F1C84E"));
            }

            GameObject flowers = new GameObject("HoaMaiStall_Flowers");
            flowers.transform.SetParent(stall, false);

            // Prefer yellow flower prefabs; fall back to other flower assets if a path is missing.
            string[] flowerPrefabs =
            {
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_1.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_2.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_3.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_4.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_5.prefab",
                "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/flower1-1.prefab",
                "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/flower2-1.prefab",
                "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/flower3-2.prefab",
                "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/flower4-1.prefab",
                "Assets/Emilulz_Assets/DEMOLowPolyFlowers/Prefabs/SM_Dandelion_Small.prefab",
                "Assets/Emilulz_Assets/DEMOLowPolyFlowers/Prefabs/SM_Daisy_Single.prefab",
            };

            Vector3[] counterSpots =
            {
                new Vector3(11.1f, 1.05f, 85.6f),
                new Vector3(12f, 1.05f, 85.5f),
                new Vector3(12.9f, 1.05f, 85.7f),
                new Vector3(11.5f, 1.08f, 86.2f),
                new Vector3(12.5f, 1.08f, 86.15f),
                new Vector3(11.8f, 1.1f, 85.9f),
                new Vector3(12.3f, 1.1f, 85.85f),
            };

            for (int i = 0; i < counterSpots.Length; i++)
            {
                SpawnYellowFlower(
                    flowers.transform,
                    PickExistingPrefab(flowerPrefabs, i),
                    counterSpots[i],
                    Vector3.one * (1.25f + (i % 3) * 0.1f),
                    yellow,
                    "HoaMai_Top_" + (i + 1));
            }

            SpawnYellowFlower(
                flowers.transform,
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower Pot.prefab",
                new Vector3(10.2f, 0f, 84.8f),
                Vector3.one * 1.15f,
                yellow,
                "HoaMai_Pot_L");
            SpawnYellowFlower(
                flowers.transform,
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower Pot.prefab",
                new Vector3(13.8f, 0f, 84.9f),
                Vector3.one * 1.15f,
                yellow,
                "HoaMai_Pot_R");

            Vector3[] groundSpots =
            {
                new Vector3(10.4f, 0f, 85.6f),
                new Vector3(13.6f, 0f, 85.5f),
                new Vector3(12f, 0f, 84.2f),
                new Vector3(10.8f, 0f, 84.4f),
                new Vector3(13.2f, 0f, 84.3f),
                new Vector3(11.2f, 0f, 86.8f),
                new Vector3(12.8f, 0f, 86.9f),
                new Vector3(14.2f, 0f, 85.8f),
                new Vector3(9.8f, 0f, 85.9f),
            };

            for (int i = 0; i < groundSpots.Length; i++)
            {
                SpawnYellowFlower(
                    flowers.transform,
                    PickExistingPrefab(flowerPrefabs, i + 3),
                    groundSpots[i],
                    Vector3.one * (1.35f + (i % 4) * 0.08f),
                    yellow,
                    "HoaMai_Ground_" + (i + 1));
            }
        }

        private static string PickExistingPrefab(string[] candidates, int startIndex)
        {
            if (candidates == null || candidates.Length == 0)
            {
                return null;
            }

            for (int offset = 0; offset < candidates.Length; offset++)
            {
                string path = candidates[(startIndex + offset) % candidates.Length];
                if (!string.IsNullOrEmpty(path) && AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                {
                    return path;
                }
            }

            return candidates[0];
        }

        private static void SpawnYellowFlower(
            Transform parent,
            string prefabPath,
            Vector3 position,
            Vector3 scale,
            Material yellow,
            string name)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                return;
            }

            GameObject instance = InstantiatePrefab(prefabPath, parent, position, Quaternion.identity, scale, name);
            if (instance != null && yellow != null)
            {
                ApplyMaterialRecursive(instance, yellow);
            }
        }

        private static void DressQuaBieuStall(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            Transform stall = FindChildRecursive(worldRoot, "QuaBieuStall");
            if (stall == null)
            {
                return;
            }

            DestroyNamedDescendants(stall, "QuaBieuStall_Contents");

            Material gold = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Lantern_Gold.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Coin_Gold.mat")
                ?? CreateMaterial("Coin_Gold.mat", Hex("#E8BC3D"));
            Material orange = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Produce_Orange.mat")
                ?? CreateMaterial("Produce_Orange.mat", Hex("#E0802E"));
            Material yellow = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Produce_Yellow.mat")
                ?? CreateMaterial("Produce_Yellow.mat", Hex("#E8BA3D"));
            Material red = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Gift_Red.mat")
                ?? CreateMaterial("Gift_Red.mat", Hex("#B32929"));
            Material wood = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Wood_Warm.mat");

            GameObject contents = new GameObject("QuaBieuStall_Contents");
            contents.transform.SetParent(stall, false);

            const string market = "Assets/Low-Poly Medieval Market/Prefabs/";
            const string fruits = market + "Fruits and vegetables/";

            // Fill the red crate (center 9.5, 97; rim ~y=1.0) with Tet gift goods.
            SpawnGiftProp(contents.transform, market + "candy_jar.prefab",
                new Vector3(9.05f, 0.42f, 96.7f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.35f, "GiftCandyJar_1", null);
            SpawnGiftProp(contents.transform, market + "candy_jar.prefab",
                new Vector3(9.95f, 0.42f, 96.75f), Quaternion.Euler(0f, -22f, 0f), Vector3.one * 1.3f, "GiftCandyJar_2", null);
            SpawnGiftProp(contents.transform, market + "wicker_bottle.prefab",
                new Vector3(9.2f, 0.4f, 97.25f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 1.4f, "GiftBottle_1", null);
            SpawnGiftProp(contents.transform, market + "wicker_bottle.prefab",
                new Vector3(9.85f, 0.4f, 97.3f), Quaternion.Euler(0f, -40f, 0f), Vector3.one * 1.35f, "GiftBottle_2", null);
            SpawnGiftProp(contents.transform, market + "wooden_jar.prefab",
                new Vector3(9.5f, 0.4f, 96.55f), Quaternion.identity, Vector3.one * 1.25f, "GiftJar_1", wood);
            SpawnGiftProp(contents.transform, market + "cookies.prefab",
                new Vector3(9.35f, 0.55f, 97.0f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 1.5f, "GiftCookies_1", null);
            SpawnGiftProp(contents.transform, market + "cake.prefab",
                new Vector3(9.7f, 0.52f, 96.9f), Quaternion.Euler(0f, -15f, 0f), Vector3.one * 1.2f, "GiftCake_1", null);
            SpawnGiftProp(contents.transform, market + "pie.prefab",
                new Vector3(9.15f, 0.5f, 96.95f), Quaternion.Euler(0f, 50f, 0f), Vector3.one * 1.15f, "GiftPie_1", null);
            SpawnGiftProp(contents.transform, market + "pastry.prefab",
                new Vector3(9.8f, 0.5f, 97.05f), Quaternion.Euler(0f, -30f, 0f), Vector3.one * 1.2f, "GiftPastry_1", null);
            SpawnGiftProp(contents.transform, fruits + "apple_red.prefab",
                new Vector3(9.3f, 0.7f, 97.15f), Quaternion.identity, Vector3.one * 1.4f, "GiftApple_1", null);
            SpawnGiftProp(contents.transform, fruits + "apple_yellow.prefab",
                new Vector3(9.65f, 0.72f, 97.2f), Quaternion.identity, Vector3.one * 1.35f, "GiftApple_2", null);
            SpawnGiftProp(contents.transform, fruits + "apple_red.prefab",
                new Vector3(9.5f, 0.68f, 96.8f), Quaternion.Euler(0f, 40f, 0f), Vector3.one * 1.3f, "GiftApple_3", null);
            SpawnGiftProp(contents.transform, market + "coins.prefab",
                new Vector3(9.45f, 0.78f, 97.05f), Quaternion.identity, Vector3.one * 1.1f, "GiftCoinsTop", gold);

            // Overflow gift parcels on top of the crate.
            CreateGiftParcel(contents.transform, new Vector3(9.1f, 0.95f, 97.35f), orange, gold, "GiftParcel_1");
            CreateGiftParcel(contents.transform, new Vector3(9.9f, 0.98f, 97.4f), red, gold, "GiftParcel_2");
            CreateGiftParcel(contents.transform, new Vector3(9.5f, 1.05f, 96.65f), yellow, gold, "GiftParcel_3");

            // Extra filled baskets beside the main crate so the stall reads as a gift shop.
            SpawnGiftProp(contents.transform, market + "basket_01.prefab",
                new Vector3(7.8f, 0f, 96.4f), Quaternion.Euler(0f, 20f, 0f), Vector3.one * 1.6f, "GiftBasket_L", wood);
            SpawnGiftProp(contents.transform, market + "candy_jar.prefab",
                new Vector3(7.8f, 0.35f, 96.4f), Quaternion.identity, Vector3.one * 1.1f, "GiftBasket_L_Candy", null);
            SpawnGiftProp(contents.transform, fruits + "apple_red.prefab",
                new Vector3(7.65f, 0.45f, 96.5f), Quaternion.identity, Vector3.one * 1.15f, "GiftBasket_L_Apple", null);
            SpawnGiftProp(contents.transform, market + "wicker_bottle.prefab",
                new Vector3(7.95f, 0.3f, 96.25f), Quaternion.Euler(0f, 15f, 0f), Vector3.one * 1.05f, "GiftBasket_L_Bottle", null);

            SpawnGiftProp(contents.transform, market + "basket_02.prefab",
                new Vector3(11.2f, 0f, 96.5f), Quaternion.Euler(0f, -25f, 0f), Vector3.one * 1.55f, "GiftBasket_R", wood);
            SpawnGiftProp(contents.transform, market + "cookies.prefab",
                new Vector3(11.2f, 0.35f, 96.5f), Quaternion.identity, Vector3.one * 1.2f, "GiftBasket_R_Cookies", null);
            SpawnGiftProp(contents.transform, fruits + "apple_yellow.prefab",
                new Vector3(11.05f, 0.45f, 96.6f), Quaternion.identity, Vector3.one * 1.15f, "GiftBasket_R_Apple", null);
            SpawnGiftProp(contents.transform, market + "wooden_jar.prefab",
                new Vector3(11.35f, 0.3f, 96.35f), Quaternion.identity, Vector3.one * 1.0f, "GiftBasket_R_Jar", wood);

            SpawnGiftProp(contents.transform, market + "round_bread_basket.prefab",
                new Vector3(8.2f, 0f, 98.2f), Quaternion.Euler(0f, 40f, 0f), Vector3.one * 1.4f, "GiftBasket_Back", wood);
            SpawnGiftProp(contents.transform, market + "cake.prefab",
                new Vector3(8.2f, 0.35f, 98.2f), Quaternion.identity, Vector3.one * 1.1f, "GiftBasket_Back_Cake", null);
            SpawnGiftProp(contents.transform, market + "pretzel_box.prefab",
                new Vector3(10.8f, 0f, 98.1f), Quaternion.Euler(0f, -15f, 0f), Vector3.one * 1.3f, "GiftBox_Side", null);
        }

        private static void SpawnGiftProp(
            Transform parent,
            string prefabPath,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string name,
            Material optionalTint)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                return;
            }

            GameObject instance = InstantiatePrefab(prefabPath, parent, position, rotation, scale, name);
            if (instance != null && optionalTint != null)
            {
                ApplyMaterialRecursive(instance, optionalTint);
            }
        }

        private static void CreateGiftParcel(Transform parent, Vector3 position, Material wrap, Material ribbon, string name)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Wrap";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.38f, 0.28f, 0.32f);
            ApplyMaterialAndStripCollider(body, wrap);

            GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = "Ribbon";
            band.transform.SetParent(root.transform, false);
            band.transform.localPosition = new Vector3(0f, 0.16f, 0f);
            band.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            band.transform.localScale = new Vector3(0.42f, 0.06f, 0.08f);
            ApplyMaterialAndStripCollider(band, ribbon);

            GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bow.name = "Bow";
            bow.transform.SetParent(root.transform, false);
            bow.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            bow.transform.localScale = Vector3.one * 0.14f;
            ApplyMaterialAndStripCollider(bow, ribbon);
        }

        private static void ApplyMaterialAndStripCollider(GameObject instance, Material material)
        {
            if (instance == null)
            {
                return;
            }

            Renderer renderer = instance.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        // East-west traffic road (Level-1 highway materials). Long enough to read as endless.
        private const float TrafficRoadCenterZ = 54f;
        private const float TrafficRoadHalfWidthZ = 8f;
        private const float TrafficRoadMedianWidthZ = 1.4f;
        private const float TrafficRoadHalfLengthX = 200f;
        private const float TrafficRoadSurfaceY = 0.06f;
        private const float TrafficRoadMarkY = 0.12f;
        private const float TrafficCarSpawnX = -190f;
        private const float TrafficCarSpawnY = 0.2f;
        // Pedestrian may only cross at this zebra corridor (world X).
        private const float TrafficCrosswalkCenterX = 0f;
        private const float TrafficCrosswalkHalfWidthX = 3.2f;
        private const float TrafficCrosswalkWalkY = 0.16f;
        // Two lanes each side of the median (south / north).
        private static readonly float[] TrafficLaneZs = { 49.6f, 51.8f, 56.2f, 58.4f };

        /// <summary>
        /// Level-1 style asphalt road, endless on both sides for continuous car flow.
        /// Stays on the home↔market band (z≈54) and never reaches the temple (z≈88+, x≈55).
        /// </summary>
        private static void DressTrafficRoad(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            DestroyNamedDescendants(worldRoot, "TrafficRoadSurface");

            Transform roadGround = FindChildRecursive(worldRoot, "RoadGround");
            if (roadGround != null)
            {
                Object.DestroyImmediate(roadGround.gameObject);
            }

            // Pedestrian path meets the south road edge at the same height (no step).
            Transform mainPath = FindChildRecursive(worldRoot, "MainPath");
            if (mainPath != null)
            {
                float pathTop = TrafficCrosswalkWalkY;
                float pathThickness = 0.14f;
                float pathStartZ = 13f;
                float pathEndZ = TrafficRoadCenterZ - TrafficRoadHalfWidthZ;
                float pathLengthZ = pathEndZ - pathStartZ;
                float pathCenterZ = (pathStartZ + pathEndZ) * 0.5f;
                mainPath.position = new Vector3(TrafficCrosswalkCenterX, pathTop - pathThickness * 0.5f, pathCenterZ);
                mainPath.localScale = new Vector3(TrafficCrosswalkHalfWidthX * 2f, pathThickness, pathLengthZ);
                Material pathMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Urban_Path.mat")
                    ?? AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Road_Ground.mat");
                if (pathMat != null)
                {
                    ApplyMaterialRecursive(mainPath.gameObject, pathMat);
                }
            }

            ClearAllAssetsOnTrafficRoad(worldRoot);

            Material asphalt = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Highway_Asphalt_Gray.mat")
                ?? CreateMaterial("Traffic_Asphalt.mat", Hex("#2F3034"));
            Material laneMark = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Highway_LaneMark_White.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Wall_Light.mat")
                ?? CreateMaterial("Traffic_LaneWhite.mat", Hex("#E8E8EC"));
            Material shoulder = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Highway_Shoulder_Concrete.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Foundation_Stone.mat")
                ?? CreateMaterial("Traffic_Shoulder.mat", Hex("#6A6A70"));
            Material edgeMark = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Lantern_Gold.mat")
                ?? CreateMaterial("Traffic_EdgeGold.mat", Hex("#E3B64D"));

            GameObject surface = new GameObject("TrafficRoadSurface");
            surface.transform.SetParent(worldRoot, false);

            float roadLength = TrafficRoadHalfLengthX * 2f;
            float roadWidth = TrafficRoadHalfWidthZ * 2f;
            float deckY = TrafficRoadSurfaceY;
            float markY = TrafficRoadMarkY;
            float zNorth = TrafficRoadCenterZ + TrafficRoadHalfWidthZ;
            float zSouth = TrafficRoadCenterZ - TrafficRoadHalfWidthZ;
            float medianHalf = TrafficRoadMedianWidthZ * 0.5f;

            // Two carriageways split by a raised median (narrower than before).
            float southLaneWidth = TrafficRoadHalfWidthZ - medianHalf;
            float southLaneCenterZ = zSouth + southLaneWidth * 0.5f;
            float northLaneWidth = TrafficRoadHalfWidthZ - medianHalf;
            float northLaneCenterZ = zNorth - northLaneWidth * 0.5f;

            CreateGround(
                "TrafficAsphaltSouth",
                surface.transform,
                new Vector3(0f, deckY, southLaneCenterZ),
                new Vector3(roadLength, 0.18f, southLaneWidth),
                asphalt);
            CreateGround(
                "TrafficAsphaltNorth",
                surface.transform,
                new Vector3(0f, deckY, northLaneCenterZ),
                new Vector3(roadLength, 0.18f, northLaneWidth),
                asphalt);

            // Raised median divider, split so the crosswalk can pass through.
            float medianSegmentLength = TrafficRoadHalfLengthX - TrafficCrosswalkHalfWidthX - 0.5f;
            float medianWestX = -TrafficCrosswalkHalfWidthX - 0.5f - medianSegmentLength * 0.5f;
            float medianEastX = TrafficCrosswalkHalfWidthX + 0.5f + medianSegmentLength * 0.5f;
            BuildMedianSegment(surface.transform, "West", medianWestX, medianSegmentLength, deckY, markY, medianHalf, shoulder, asphalt, edgeMark);
            BuildMedianSegment(surface.transform, "East", medianEastX, medianSegmentLength, deckY, markY, medianHalf, shoulder, asphalt, edgeMark);

            // Concrete shoulders / guards — split so the crosswalk stays open.
            float curbSegmentLength = TrafficRoadHalfLengthX - TrafficCrosswalkHalfWidthX - 0.4f;
            float curbWestX = -TrafficCrosswalkHalfWidthX - 0.4f - curbSegmentLength * 0.5f;
            float curbEastX = TrafficCrosswalkHalfWidthX + 0.4f + curbSegmentLength * 0.5f;
            BuildCurbSegment(surface.transform, "West", curbWestX, curbSegmentLength, deckY, markY, zNorth, zSouth, shoulder);
            BuildCurbSegment(surface.transform, "East", curbEastX, curbSegmentLength, deckY, markY, zNorth, zSouth, shoulder);

            // White edge lines — split so the zebra corridor has no raised lip.
            float edgeSegLen = TrafficRoadHalfLengthX - TrafficCrosswalkHalfWidthX - 0.5f;
            float edgeWestX = -TrafficCrosswalkHalfWidthX - 0.5f - edgeSegLen * 0.5f;
            float edgeEastX = TrafficCrosswalkHalfWidthX + 0.5f + edgeSegLen * 0.5f;
            StripCollider(CreateGround(
                "TrafficEdgeLineNorth_West",
                surface.transform,
                new Vector3(edgeWestX, markY, zNorth - 0.18f),
                new Vector3(edgeSegLen, 0.03f, 0.14f),
                laneMark));
            StripCollider(CreateGround(
                "TrafficEdgeLineNorth_East",
                surface.transform,
                new Vector3(edgeEastX, markY, zNorth - 0.18f),
                new Vector3(edgeSegLen, 0.03f, 0.14f),
                laneMark));
            StripCollider(CreateGround(
                "TrafficEdgeLineSouth_West",
                surface.transform,
                new Vector3(edgeWestX, markY, zSouth + 0.18f),
                new Vector3(edgeSegLen, 0.03f, 0.14f),
                laneMark));
            StripCollider(CreateGround(
                "TrafficEdgeLineSouth_East",
                surface.transform,
                new Vector3(edgeEastX, markY, zSouth + 0.18f),
                new Vector3(edgeSegLen, 0.03f, 0.14f),
                laneMark));

            // Dashed lane dividers within each carriageway (skip the crosswalk gap).
            float[] dividerZs = { 50.7f, 57.3f };
            int dashIndex = 0;
            for (int d = 0; d < dividerZs.Length; d++)
            {
                for (float x = -TrafficRoadHalfLengthX + 2f; x <= TrafficRoadHalfLengthX - 2f; x += 7f)
                {
                    if (Mathf.Abs(x - TrafficCrosswalkCenterX) < TrafficCrosswalkHalfWidthX + 1.5f)
                    {
                        continue;
                    }

                    StripCollider(CreateGround(
                        "TrafficLaneDash_" + dashIndex,
                        surface.transform,
                        new Vector3(x, markY, dividerZs[d]),
                        new Vector3(4f, 0.025f, 0.12f),
                        laneMark));
                    dashIndex++;
                }
            }

            BuildPedestrianCrosswalk(surface.transform, worldRoot, laneMark, asphalt, zSouth, zNorth, markY);
            ConfigureEndlessTraffic(worldRoot);
        }

        private static void BuildMedianSegment(
            Transform parent,
            string side,
            float centerX,
            float length,
            float deckY,
            float markY,
            float medianHalf,
            Material shoulder,
            Material asphalt,
            Material edgeMark)
        {
            CreateGround(
                "TrafficMedianBase_" + side,
                parent,
                new Vector3(centerX, deckY + 0.06f, TrafficRoadCenterZ),
                new Vector3(length, 0.22f, TrafficRoadMedianWidthZ),
                shoulder);
            CreateGround(
                "TrafficMedianTop_" + side,
                parent,
                new Vector3(centerX, markY + 0.08f, TrafficRoadCenterZ),
                new Vector3(length, 0.08f, TrafficRoadMedianWidthZ * 0.72f),
                asphalt);
            CreateGround(
                "TrafficMedianEdgeN_" + side,
                parent,
                new Vector3(centerX, markY + 0.1f, TrafficRoadCenterZ + medianHalf * 0.55f),
                new Vector3(length - 1f, 0.03f, 0.12f),
                edgeMark);
            CreateGround(
                "TrafficMedianEdgeS_" + side,
                parent,
                new Vector3(centerX, markY + 0.1f, TrafficRoadCenterZ - medianHalf * 0.55f),
                new Vector3(length - 1f, 0.03f, 0.12f),
                edgeMark);
        }

        private static void BuildCurbSegment(
            Transform parent,
            string side,
            float centerX,
            float length,
            float deckY,
            float markY,
            float zNorth,
            float zSouth,
            Material shoulder)
        {
            CreateGround(
                "TrafficShoulderNorth_" + side,
                parent,
                new Vector3(centerX, deckY - 0.02f, zNorth + 1.1f),
                new Vector3(length, 0.14f, 2.2f),
                shoulder);
            CreateGround(
                "TrafficShoulderSouth_" + side,
                parent,
                new Vector3(centerX, deckY - 0.02f, zSouth - 1.1f),
                new Vector3(length, 0.14f, 2.2f),
                shoulder);
            CreateGround(
                "TrafficGuardNorth_" + side,
                parent,
                new Vector3(centerX, markY + 0.04f, zNorth + 2.1f),
                new Vector3(length, 0.16f, 0.18f),
                shoulder);
            CreateGround(
                "TrafficGuardSouth_" + side,
                parent,
                new Vector3(centerX, markY + 0.04f, zSouth - 2.1f),
                new Vector3(length, 0.16f, 0.18f),
                shoulder);
        }

        private static void BuildPedestrianCrosswalk(
            Transform roadSurface,
            Transform worldRoot,
            Material laneMark,
            Material asphalt,
            float zSouth,
            float zNorth,
            float markY)
        {
            float roadWidth = TrafficRoadHalfWidthZ * 2f;
            float crossWidth = TrafficCrosswalkHalfWidthX * 2f;
            float walkTop = TrafficCrosswalkWalkY;
            float walkThickness = 0.12f;
            float walkCenterY = walkTop - walkThickness * 0.5f;

            Material pathMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Urban_Path.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Road_Ground.mat")
                ?? asphalt;

            // Walkable asphalt deck through the roadway (flush with MainPath, no step).
            CreateGround(
                "TrafficCrosswalkWalkway",
                roadSurface,
                new Vector3(TrafficCrosswalkCenterX, walkCenterY, TrafficRoadCenterZ),
                new Vector3(crossWidth, walkThickness, roadWidth + 0.6f),
                asphalt);

            // Continue north toward the market at the same height.
            CreateGround(
                "TrafficCrosswalkPathNorth",
                roadSurface,
                new Vector3(TrafficCrosswalkCenterX, walkCenterY, zNorth + 8f),
                new Vector3(crossWidth, walkThickness, 12f),
                pathMat);

            // QCVN 41-style zebra (vạch đi bộ qua đường):
            // white bars PARALLEL to traffic (along X), spaced along pedestrian direction (Z).
            // Bar width ≈ gap ≈ 0.4–0.6 m; bar length = pedestrian corridor width.
            // Paint only — no collision so the player can walk through.
            const float stripeThicknessZ = 0.5f;
            const float stripeGapZ = 0.5f;
            float stripeLengthX = crossWidth - 0.3f;
            float stripeY = walkTop + 0.012f;
            const float stripeHeight = 0.02f;
            int stripeIndex = 0;
            float stripeStartZ = zSouth + stripeThicknessZ * 0.5f + 0.15f;
            float stripeEndZ = zNorth - stripeThicknessZ * 0.5f - 0.15f;
            for (float z = stripeStartZ; z <= stripeEndZ + 0.01f; z += stripeThicknessZ + stripeGapZ)
            {
                StripCollider(CreateGround(
                    "TrafficCrosswalkStripe_" + stripeIndex,
                    roadSurface,
                    new Vector3(TrafficCrosswalkCenterX, stripeY, z),
                    new Vector3(stripeLengthX, stripeHeight, stripeThicknessZ),
                    laneMark));
                stripeIndex++;
            }

            // Invisible walls block asphalt everywhere except the zebra corridor.
            float gapHalf = TrafficCrosswalkHalfWidthX + 0.15f;
            float westLength = TrafficRoadHalfLengthX - gapHalf;
            float eastLength = TrafficRoadHalfLengthX - gapHalf;
            float westCenterX = -gapHalf - westLength * 0.5f;
            float eastCenterX = gapHalf + eastLength * 0.5f;
            float blockerHeight = 2.4f;
            float blockerY = blockerHeight * 0.5f;
            float blockerDepth = roadWidth + 0.4f;

            CreateRoadBlocker(
                roadSurface,
                "TrafficRoadBlockerWest",
                new Vector3(westCenterX, blockerY, TrafficRoadCenterZ),
                new Vector3(westLength, blockerHeight, blockerDepth));
            CreateRoadBlocker(
                roadSurface,
                "TrafficRoadBlockerEast",
                new Vector3(eastCenterX, blockerY, TrafficRoadCenterZ),
                new Vector3(eastLength, blockerHeight, blockerDepth));
        }

        private static void CreateRoadBlocker(Transform parent, string name, Vector3 position, Vector3 scale)
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

            Collider collider = blocker.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }
        }

        private static GameObject StripCollider(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return target;
        }

        private static void ClearAllAssetsOnTrafficRoad(Transform worldRoot)
        {
            Transform roadArea = FindChildRecursive(worldRoot, "RoadArea");
            if (roadArea != null)
            {
                for (int i = roadArea.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(roadArea.GetChild(i).gameObject);
                }
            }

            // Remove any world props sitting on the traffic band (bushes, grass, trees, rocks…).
            Transform[] all = worldRoot.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                Transform t = all[i];
                if (t == null || t == worldRoot)
                {
                    continue;
                }

                if (IsTrafficRoadInfrastructure(t))
                {
                    continue;
                }

                Vector3 p = t.position;
                if (!IsOnTrafficRoadBand(p))
                {
                    continue;
                }

                // Only clear decorative props, not gameplay systems / player / ground slabs.
                string n = t.name;
                bool isDecor =
                    n.StartsWith("Border", System.StringComparison.Ordinal)
                    || n.StartsWith("Road", System.StringComparison.Ordinal)
                    || n.StartsWith("MarketBush", System.StringComparison.Ordinal)
                    || n.StartsWith("MarketGrass", System.StringComparison.Ordinal)
                    || n.StartsWith("MarketTree", System.StringComparison.Ordinal)
                    || n.Contains("Bush")
                    || n.Contains("Grass")
                    || n.Contains("Tree")
                    || n.Contains("Rock")
                    || n.Contains("Cliff")
                    || n.Contains("Fence")
                    || n.Contains("Scooter")
                    || n.Contains("Pole")
                    || n.Contains("Bench")
                    || n.Contains("Dustbin")
                    || n.Contains("Lantern")
                    || n.Contains("Sign")
                    || n.Contains("Stop");

                if (!isDecor)
                {
                    continue;
                }

                // Destroy the prop root (prefer nearest named prop under world, not deep mesh children).
                Transform destroyTarget = t;
                while (destroyTarget.parent != null
                       && destroyTarget.parent != worldRoot
                       && destroyTarget.parent.name != "ChoTetWorldBackdrop"
                       && destroyTarget.parent.name != "RoadArea"
                       && destroyTarget.parent.name != "MarketArea"
                       && destroyTarget.parent.name != "HomeArea"
                       && destroyTarget.parent.name != "GardenArea")
                {
                    destroyTarget = destroyTarget.parent;
                }

                if (destroyTarget != null && destroyTarget != worldRoot)
                {
                    Object.DestroyImmediate(destroyTarget.gameObject);
                }
            }
        }

        private static bool IsTrafficRoadInfrastructure(Transform t)
        {
            Transform current = t;
            while (current != null)
            {
                string n = current.name;
                if (n == "TrafficRoadSurface"
                    || n == "CrowdedRoadTraffic"
                    || n == "Player"
                    || n == "Main Camera"
                    || n == "Systems"
                    || n == "ZoneTriggers"
                    || n == "MainGround"
                    || n == "MainPath"
                    || n == "MarketGround"
                    || n == "GardenGround"
                    || n == "HomeGround")
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool IsOnTrafficRoadBand(Vector3 position)
        {
            return position.z >= TrafficRoadCenterZ - TrafficRoadHalfWidthZ - 3f
                && position.z <= TrafficRoadCenterZ + TrafficRoadHalfWidthZ + 3f
                && Mathf.Abs(position.x) <= TrafficRoadHalfLengthX;
        }

        private static void ConfigureEndlessTraffic(Transform worldRoot)
        {
            TrafficSpawner spawner = Object.FindAnyObjectByType<TrafficSpawner>(FindObjectsInactive.Include);
            if (spawner == null)
            {
                return;
            }

            Transform spawnRoot = FindChildRecursive(worldRoot, "TrafficSpawnPoints");
            if (spawnRoot == null && spawner.transform.parent != null)
            {
                spawnRoot = FindChildRecursive(spawner.transform.parent, "TrafficSpawnPoints");
            }

            if (spawnRoot != null)
            {
                for (int i = spawnRoot.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(spawnRoot.GetChild(i).gameObject);
                }

                spawner.spawnPoints = BuildBidirectionalTrafficSpawnPoints(spawnRoot);
            }

            ApplyModerateAlwaysOnTrafficSettings(spawner);
            WireLevel1KenneyTrafficPrefabs(spawner);

            CrowdedRoadTrafficController controller = Object.FindAnyObjectByType<CrowdedRoadTrafficController>(FindObjectsInactive.Include);
            if (controller != null)
            {
                controller.trafficSpawner = spawner;
                controller.RefreshFromGameState();
            }

            spawner.gameObject.SetActive(true);
            spawner.enabled = true;
        }

        /// <summary>
        /// South lanes eastbound (from left), north lanes westbound (from right) — right-hand traffic.
        /// </summary>
        private static Transform[] BuildBidirectionalTrafficSpawnPoints(Transform spawnRoot)
        {
            Transform[] points = new Transform[TrafficLaneZs.Length];
            float eastSpawnX = TrafficCarSpawnX;
            float westSpawnX = -TrafficCarSpawnX;
            for (int i = 0; i < TrafficLaneZs.Length; i++)
            {
                bool eastbound = i < TrafficLaneZs.Length / 2;
                float spawnX = eastbound ? eastSpawnX : westSpawnX;
                float yaw = eastbound ? 0f : 180f;
                string side = eastbound ? "Eastbound" : "Westbound";
                points[i] = CreateTrafficSpawnPoint(
                    spawnRoot,
                    "Lane" + (char)('A' + i) + "_" + side,
                    new Vector3(spawnX, TrafficCarSpawnY, TrafficLaneZs[i]),
                    yaw);
            }

            return points;
        }

        private static void ApplyModerateAlwaysOnTrafficSettings(TrafficSpawner spawner)
        {
            if (spawner == null)
            {
                return;
            }

            // Always on, sparse enough to dodge at the zebra (3 lives on hit).
            spawner.roadBoundsCenter = new Vector3(0f, TrafficCarSpawnY, TrafficRoadCenterZ);
            spawner.roadBoundsSize = new Vector3(TrafficRoadHalfLengthX * 2f + 20f, 8f, TrafficRoadHalfWidthZ * 2f + 4f);
            spawner.spawnInterval = 3.4f;
            spawner.spawnJitter = 0.7f;
            spawner.spawnBatchMin = 1;
            spawner.spawnBatchMax = 1;
            spawner.maxActiveObstacles = 5;
            spawner.obstacleSpeed = 6.4f;
            spawner.obstacleScale = Vector3.one * 1.05f;
            spawner.damageAmount = 8;
        }

        private static void WireLevel1KenneyTrafficPrefabs(TrafficSpawner spawner)
        {
            if (spawner == null)
            {
                return;
            }

            Level1KenneyCarKitTrafficBuilder.EnsureTrafficPrefabs(forceRebuild: false);

            // Everyday Level-1 cars only (no tractors / race / kart).
            string[] paths =
            {
                Level1KenneyCarKitPaths.SedanPrefab,
                Level1KenneyCarKitPaths.SedanSportsPrefab,
                Level1KenneyCarKitPaths.HatchbackSportsPrefab,
                Level1KenneyCarKitPaths.SuvPrefab,
                Level1KenneyCarKitPaths.TaxiPrefab,
                Level1KenneyCarKitPaths.VanPrefab,
                Level1KenneyCarKitPaths.DeliveryPrefab,
                Level1KenneyCarKitPaths.PolicePrefab
            };

            Transform prefabHost = spawner.transform;
            for (int i = prefabHost.childCount - 1; i >= 0; i--)
            {
                Transform child = prefabHost.GetChild(i);
                if (child.name.Contains("Traffic") || child.name.Contains("Car") || child.name.Contains("Kenney"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            List<GameObject> wired = new List<GameObject>(paths.Length);
            for (int i = 0; i < paths.Length; i++)
            {
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (source == null)
                {
                    continue;
                }

                GameObject instance = Object.Instantiate(source, prefabHost);
                instance.name = source.name + "_TrafficPrefab";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.SetActive(false);
                DisableNestedPhysicsComponents(instance);
                wired.Add(instance);
            }

            if (wired.Count > 0)
            {
                spawner.obstaclePrefabs = wired.ToArray();
                spawner.obstaclePrefab = wired[0];
            }
        }

        private static void DestroyNamedDescendants(Transform root, string targetName)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                if (all[i] != null && all[i].name == targetName)
                {
                    Object.DestroyImmediate(all[i].gameObject);
                }
            }
        }

        private static void ApplyMarketGrassGround(Transform worldRoot)
        {
            Material grass = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Grass_Ground.mat");
            if (grass == null)
            {
                grass = AssetDatabase.LoadAssetAtPath<Material>(MaterialsRoot + "/Garden_Ground.mat");
            }

            if (grass == null || worldRoot == null)
            {
                return;
            }

            string[] groundNames = { "MarketGround", "MarketApron" };
            for (int i = 0; i < groundNames.Length; i++)
            {
                Transform ground = FindChildRecursive(worldRoot, groundNames[i]);
                if (ground == null)
                {
                    continue;
                }

                Renderer renderer = ground.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = grass;
                }
            }

            // Pull the dirt path back so it no longer covers the market pad.
            Transform mainPath = FindChildRecursive(worldRoot, "MainPath");
            if (mainPath != null)
            {
                mainPath.position = new Vector3(0f, -0.38f, 38f);
                mainPath.localScale = new Vector3(10f, 0.15f, 68f);
            }

            // Stretch market grass north so it meets the backdrop without a dirt lip seam.
            Transform marketGround = FindChildRecursive(worldRoot, "MarketGround");
            if (marketGround != null)
            {
                marketGround.position = new Vector3(0f, -0.47f, 94f);
                marketGround.localScale = new Vector3(54f, 1f, 44f);
            }

            GameObject lipNorth = GameObject.Find("TerrainLipNorth");
            if (lipNorth != null)
            {
                lipNorth.SetActive(false);
            }
        }

        private static Transform FindSceneTransform(string objectName)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < all.Length; j++)
                {
                    if (all[j].name == objectName)
                    {
                        return all[j];
                    }
                }
            }

            return null;
        }

        private static void CreateRoadArea(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("RoadArea");
            root.transform.SetParent(parent, false);

            for (int i = 0; i < 9; i++)
            {
                GameObject roadPiece = InstantiatePrefab(
                    "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Path Piece_5.prefab",
                    root.transform,
                    new Vector3(0f, 0.02f, 14f + (i * 9f)),
                    Quaternion.identity,
                    Vector3.one * 1.45f,
                    "RoadPiece_" + i
                );
                ApplyMaterialRecursive(roadPiece, materials["road"]);
            }

            for (int i = 0; i < 5; i++)
            {
                ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Fence.prefab", root.transform, new Vector3(-8.5f, 0f, 20f + (i * 12f)), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.2f, "RoadFence_L" + i), materials["wood"]);
                ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Fence.prefab", root.transform, new Vector3(8.5f, 0f, 26f + (i * 12f)), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.2f, "RoadFence_R" + i), materials["wood"]);
            }

            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_6.prefab", root.transform, new Vector3(-14f, 0f, 48f), Quaternion.identity, Vector3.one * 1.2f, "RoadTree_1"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_8.prefab", root.transform, new Vector3(14f, 0f, 62f), Quaternion.identity, Vector3.one * 1.25f, "RoadTree_2"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", root.transform, new Vector3(-10f, 0f, 70f), Quaternion.identity, Vector3.one * 1.05f, "RoadLantern"), materials["lantern"]);
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/electric pole.prefab", root.transform, new Vector3(-11.5f, 0f, 42f), Quaternion.identity, Vector3.one * 1.35f, "RoadPole_1");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/electric pole.prefab", root.transform, new Vector3(11.5f, 0f, 61f), Quaternion.identity, Vector3.one * 1.35f, "RoadPole_2");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/sign board.prefab", root.transform, new Vector3(10.5f, 0f, 72f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.15f, "RoadSign");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/bus stop.prefab", root.transform, new Vector3(-13.2f, 0f, 58f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 0.85f, "RoadStop");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/BENCH.prefab", root.transform, new Vector3(12f, 0f, 50f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.05f, "RoadBenchUrban");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/dustbin.prefab", root.transform, new Vector3(10.2f, 0f, 58.5f), Quaternion.identity, Vector3.one * 1.1f, "RoadDustbin");
            InstantiatePrefab(HondaPrefabsRoot + "/C125 - PG.prefab", root.transform, new Vector3(-11.2f, 0.02f, 66.5f), Quaternion.Euler(0f, 92f, 0f), Vector3.one * 1.02f, "RoadScooter");
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_5.prefab", root.transform, new Vector3(-17f, 0f, 40f), Quaternion.identity, Vector3.one * 1.18f, "RoadTree_3"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_7.prefab", root.transform, new Vector3(17f, 0f, 52f), Quaternion.identity, Vector3.one * 1.2f, "RoadTree_4"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Pine Tree_4.prefab", root.transform, new Vector3(-17f, 0f, 63f), Quaternion.identity, Vector3.one * 1.14f, "RoadTree_5"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Pine Tree_5.prefab", root.transform, new Vector3(17f, 0f, 76f), Quaternion.identity, Vector3.one * 1.1f, "RoadTree_6"), materials["foliage"]);
            InstantiatePrefab(UltimateNatureFbxRoot + "/Rock_1.fbx", root.transform, new Vector3(-16.5f, 0f, 47f), Quaternion.Euler(0f, 10f, 0f), Vector3.one * 1.45f, "RoadHeroRock_1");
            InstantiatePrefab(UltimateNatureFbxRoot + "/Rock_4.fbx", root.transform, new Vector3(16.5f, 0f, 65f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.25f, "RoadHeroRock_2");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/cliff2.prefab", root.transform, new Vector3(-21.5f, 0.35f, 54f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.8f, "RoadCliffLeft");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/cliff3.prefab", root.transform, new Vector3(21.5f, 0.35f, 66f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.7f, "RoadCliffRight");
            SpawnScatter(root.transform, LowPolyNaturePrefabsRoot + "/bush1.prefab", "RoadBush", new[]
            {
                new Vector3(-16f,0f,40f), new Vector3(16f,0f,44f), new Vector3(-17f,0f,56f),
                new Vector3(17f,0f,60f), new Vector3(-15f,0f,73f), new Vector3(15f,0f,75f),
                new Vector3(-16f,0f,48f), new Vector3(16f,0f,52f), new Vector3(-16f,0f,67f),
                new Vector3(16f,0f,70f), new Vector3(-16f,0f,81f)
            }, 1.22f);
            SpawnScatter(root.transform, LowPolyNaturePrefabsRoot + "/grass1.prefab", "RoadGrass", new[]
            {
                new Vector3(-13f,0f,38f), new Vector3(13f,0f,41f), new Vector3(-12f,0f,46f), new Vector3(12f,0f,49f),
                new Vector3(-13f,0f,54f), new Vector3(13f,0f,58f), new Vector3(-12f,0f,63f), new Vector3(12f,0f,68f),
                new Vector3(-13f,0f,72f), new Vector3(13f,0f,76f), new Vector3(-11f,0f,80f), new Vector3(11f,0f,82f),
                new Vector3(-14f,0f,43f), new Vector3(14f,0f,46f), new Vector3(-14f,0f,59f), new Vector3(14f,0f,62f),
                new Vector3(-14f,0f,74f), new Vector3(14f,0f,79f), new Vector3(-14f,0f,84f), new Vector3(14f,0f,87f)
            }, 1.16f);

        }

        private static void CreateLunarRoadDecor(Transform worldRoot)
        {
            GameObject lunarPolePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LunarBlessingPolePrefabPath);
            if (lunarPolePrefab == null)
            {
                Debug.LogWarning("Lunar road decor skipped because the GLB is not imported yet: " + LunarBlessingPolePrefabPath);
                return;
            }

            Transform roadArea = FindChildRecursive(worldRoot, "RoadArea");
            Transform parent = roadArea != null ? roadArea : worldRoot;

            DestroyNamedDescendants(parent, "LunarRoadDecor");

            GameObject root = new GameObject("LunarRoadDecor");
            root.transform.SetParent(parent, false);

            const float leftX = -5.35f;
            const float rightX = 5.35f;
            const float startZ = 18.2f;
            const float endZ = 51f;
            const float spacing = 3.25f;
            const float y = 0f;
            const int maxPoleCount = 18;
            Vector3 baseScale = new Vector3(1.35f, 3.6f, 1.35f);

            int index = 0;
            int placedCount = 0;
            for (float z = startZ; z <= endZ + 0.01f; z += spacing)
            {
                float currentZ = Mathf.Min(z + ((index % 2 == 0) ? 0f : 0.45f), endZ);

                if (placedCount < maxPoleCount)
                {
                    InstantiatePrefab(
                        LunarBlessingPolePrefabPath,
                        root.transform,
                        new Vector3(leftX, y, currentZ),
                        Quaternion.Euler(0f, 90f, 0f),
                        baseScale,
                        "LunarPole_L_" + index);
                    placedCount++;
                }

                if (placedCount < maxPoleCount)
                {
                    InstantiatePrefab(
                        LunarBlessingPolePrefabPath,
                        root.transform,
                        new Vector3(rightX, y, currentZ),
                        Quaternion.Euler(0f, -90f, 0f),
                        baseScale,
                        "LunarPole_R_" + index);
                    placedCount++;
                }

                index++;
            }

            Debug.Log("Lunar road decor placed " + placedCount + " poles.");
        }

        private static void CreateFortuneTreePots(Transform worldRoot)
        {
            GameObject fortuneTreePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FortuneTreePrefabPath);
            if (fortuneTreePrefab == null)
            {
                Debug.LogWarning("Fortune tree pots skipped because the GLB is not imported yet: " + FortuneTreePrefabPath);
                return;
            }

            Transform roadArea = FindChildRecursive(worldRoot, "RoadArea");
            Transform parent = roadArea != null ? roadArea : worldRoot;

            DestroyNamedDescendants(parent, "FortuneTreePots");

            GameObject root = new GameObject("FortuneTreePots");
            root.transform.SetParent(parent, false);

            Vector3 scale = Vector3.one * 1.45f;
            InstantiatePrefab(
                FortuneTreePrefabPath,
                root.transform,
                new Vector3(-5.0f, 1.45f, 13.0f),
                Quaternion.Euler(0f, 25f, 0f),
                scale,
                "FortuneTreePot_Left");

            InstantiatePrefab(
                FortuneTreePrefabPath,
                root.transform,
                new Vector3(4.1f, 1.45f, 13.7f),
                Quaternion.Euler(0f, -25f, 0f),
                scale,
                "FortuneTreePot_Right");

            Debug.Log("Fortune tree pots placed 2 pots.");
        }

        private static void CreateBuffaloHerd(Transform worldRoot)
        {
            GameObject buffaloPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BuffaloPrefabPath);
            if (buffaloPrefab == null)
            {
                Debug.LogWarning("Buffalo herd skipped because the GLB is not imported yet: " + BuffaloPrefabPath);
                return;
            }

            Transform roadArea = FindChildRecursive(worldRoot, "RoadArea");
            Transform parent = roadArea != null ? roadArea : worldRoot;

            DestroyNamedDescendants(parent, "BuffaloHerd");

            GameObject root = new GameObject("BuffaloHerd");
            root.transform.SetParent(parent, false);

            Vector3[] positions =
            {
                new Vector3(7.5f, 0.7f, 18.9f),
                new Vector3(9.1f, 0.7f, 20.0f),
                new Vector3(11.3f, 0.7f, 18.6f),
                new Vector3(8.3f, 0.7f, 21.9f),
                new Vector3(10.6f, 0.7f, 22.1f),
                new Vector3(11.5f, 0.7f, 20.9f),
                new Vector3(7.7f, 0.7f, 24.4f),
                new Vector3(9.7f, 0.7f, 25.0f),
                new Vector3(11.2f, 0.7f, 23.7f),
            };

            float[] yaws = { 25f, 165f, -70f, 245f, 95f, -145f, 320f, -15f, 205f };
            Vector3 herdScale = Vector3.one * 1.35f;

            for (int i = 0; i < positions.Length; i++)
            {
                InstantiatePrefab(
                    BuffaloPrefabPath,
                    root.transform,
                    positions[i],
                    Quaternion.Euler(0f, yaws[i], 0f),
                    herdScale,
                    "Buffalo_" + i);
            }

            Debug.Log("Buffalo herd placed " + positions.Length + " buffalo.");
        }

        private static void CreateMarketArea(Transform parent, Dictionary<string, VendorProfile> vendors, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("MarketArea");
            root.transform.SetParent(parent, false);

            VendorStall banhMutStall = CreateVendorStall(
                root.transform,
                "BanhMutStall",
                "Assets/Low-Poly Medieval Market/Prefabs/Bakery_market_with_food.prefab",
                new Vector3(-12f, 0f, 86f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * 1.2f,
                vendors["co_huong_banh"],
                new Vector3(-12f, 1f, 80.5f),
                new Vector3(5f, 2.4f, 4.4f),
                "bakery",
                materials
            );
            CreateVendorNpc(
                root.transform,
                banhMutStall,
                CoHuongGrandmotherPrefabPath,
                new Vector3(-14.8f, 1.15f, 80.75f),
                Quaternion.Euler(0f, 208f, 0f),
                Vector3.one * 1.15f,
                "CoHuong_BanhMut_NPC");

            VendorStall duaHauStall = CreateVendorStall(
                root.transform,
                "DuaHauStall",
                "Assets/Low-Poly Medieval Market/Prefabs/Vegetable_market_with_objects.prefab",
                new Vector3(0f, 0f, 86f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * 1.18f,
                vendors["chu_ba_dua"],
                new Vector3(0f, 1f, 80.5f),
                new Vector3(5f, 2.4f, 4.4f),
                "produce",
                materials
            );
            CreateVendorNpc(
                root.transform,
                duaHauStall,
                DuaHauVendorPrefabPath,
                new Vector3(-2.15f, 1.05f, 80.75f),
                Quaternion.Euler(0f, 192f, 0f),
                Vector3.one * 1.08f,
                "ChuBa_DuaHau_NPC");
            TintMarketObject(InstantiatePrefab("Assets/Low-Poly Medieval Market/Prefabs/watermelon_box.prefab", root.transform, new Vector3(4.2f, 0f, 82f), Quaternion.identity, Vector3.one * 1.2f, "WatermelonBox"), "produce", materials);

            VendorStall hoaMaiStall = CreateVendorStall(
                root.transform,
                "HoaMaiStall",
                "Assets/Low-Poly Medieval Market/Prefabs/stand_01.prefab",
                new Vector3(12f, 0f, 86f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * 1.35f,
                vendors["co_lan_hoa"],
                new Vector3(12f, 1f, 81f),
                new Vector3(4f, 2.6f, 4f),
                "produce",
                materials
            );
            CreateVendorNpc(
                root.transform,
                hoaMaiStall,
                HoaMaiVendorPrefabPath,
                new Vector3(9.9f, 1.05f, 80.95f),
                Quaternion.Euler(0f, 172f, 0f),
                Vector3.one * 1.08f,
                "CoLan_HoaMai_NPC");
            DressHoaMaiStall(root.transform);

            VendorStall thitStall = CreateVendorStall(
                root.transform,
                "ThitStall",
                "Assets/Low-Poly Medieval Market/Prefabs/Meat_market_with_objects.prefab",
                new Vector3(-8f, 0f, 97f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * 1.15f,
                vendors["chu_tu_thit"],
                new Vector3(-8f, 1f, 92f),
                new Vector3(5f, 2.4f, 4f),
                "meat",
                materials
            );
            CreateVendorNpc(
                root.transform,
                thitStall,
                ThitVendorPrefabPath,
                new Vector3(-10.4f, 1.05f, 92.35f),
                Quaternion.Euler(0f, 202f, 0f),
                Vector3.one * 1.08f,
                "ChuTu_Thit_NPC");

            VendorStall quaBieuStall = CreateVendorStall(
                root.transform,
                "QuaBieuStall",
                "Assets/Low-Poly Medieval Market/Prefabs/wooden_box.prefab",
                new Vector3(9.5f, 0f, 97f),
                Quaternion.identity,
                Vector3.one * 2.8f,
                vendors["chi_sau_qua"],
                new Vector3(9.5f, 1f, 92f),
                new Vector3(4.2f, 2.4f, 4f),
                "gift",
                materials
            );
            CreateVendorNpc(
                root.transform,
                quaBieuStall,
                QuaTetVendorPrefabPath,
                new Vector3(7.3f, 1.05f, 92.35f),
                Quaternion.Euler(0f, 165f, 0f),
                Vector3.one * 1.08f,
                "ChiSau_QuaTet_NPC");
            DressQuaBieuStall(root.transform);
            TintMarketObject(InstantiatePrefab("Assets/Low-Poly Medieval Market/Prefabs/coins.prefab", root.transform, new Vector3(8.4f, 0.6f, 97.2f), Quaternion.identity, Vector3.one * 1.8f, "GiftCoins"), "gift", materials);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", root.transform, new Vector3(11.9f, 0f, 97.4f), Quaternion.identity, Vector3.one * 1.15f, "GiftLantern"), materials["lantern"]);
            InstantiatePrefab(HondaPrefabsRoot + "/C125 - PB.prefab", root.transform, new Vector3(18.8f, 0.02f, 82.8f), Quaternion.Euler(0f, -122f, 0f), Vector3.one * 1.05f, "MarketScooter");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/parking cone.prefab", root.transform, new Vector3(17.4f, 0f, 84f), Quaternion.identity, Vector3.one * 1.15f, "MarketCone_1");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/parking cone.prefab", root.transform, new Vector3(20.4f, 0f, 84.5f), Quaternion.identity, Vector3.one * 1.15f, "MarketCone_2");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/electric pole.prefab", root.transform, new Vector3(-19f, 0f, 94f), Quaternion.identity, Vector3.one * 1.25f, "MarketPole");
            InstantiatePrefab(LowPolyUrbanPrefabsRoot + "/sign board.prefab", root.transform, new Vector3(18.2f, 0f, 94.5f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.1f, "MarketSign");
            InstantiatePrefab(UltimateNatureFbxRoot + "/Bush_Large.fbx", root.transform, new Vector3(-20f, 0f, 84f), Quaternion.Euler(0f, 24f, 0f), Vector3.one * 1.2f, "MarketBushEdge_1");
            InstantiatePrefab(UltimateNatureFbxRoot + "/Rock_2.fbx", root.transform, new Vector3(20.5f, 0f, 100f), Quaternion.Euler(0f, 75f, 0f), Vector3.one * 1.2f, "MarketRockEdge_1");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/grass4.prefab", root.transform, new Vector3(-16.8f, 0f, 80.8f), Quaternion.Euler(0f, 38f, 0f), Vector3.one * 1.5f, "MarketGrassEdge_1");
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_3.prefab", root.transform, new Vector3(-21f, 0f, 79f), Quaternion.identity, Vector3.one * 1.18f, "MarketTree_1"), materials["foliage"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Tree_4.prefab", root.transform, new Vector3(21f, 0f, 101f), Quaternion.identity, Vector3.one * 1.14f, "MarketTree_2"), materials["foliage"]);
            SpawnScatter(root.transform, LowPolyNaturePrefabsRoot + "/bush2.prefab", "MarketBush", new[]
            {
                new Vector3(-18f,0f,80f), new Vector3(-18f,0f,87f), new Vector3(-18f,0f,94f),
                new Vector3(18f,0f,82f), new Vector3(18f,0f,90f), new Vector3(18f,0f,98f)
            }, 1.08f);
            SpawnScatter(root.transform, LowPolyNaturePrefabsRoot + "/grass4.prefab", "MarketGrass", new[]
            {
                new Vector3(-16f,0f,79f), new Vector3(-15f,0f,84f), new Vector3(-15f,0f,89f), new Vector3(-15f,0f,95f),
                new Vector3(-15f,0f,100f), new Vector3(15f,0f,80f), new Vector3(15f,0f,86f), new Vector3(15f,0f,92f),
                new Vector3(15f,0f,97f), new Vector3(15f,0f,102f), new Vector3(0f,0f,101f)
            }, 1.12f);
            TintMarketObject(InstantiatePrefab("Assets/Low-Poly Medieval Market/Prefabs/table_02.prefab", root.transform, new Vector3(-18f, 0f, 90f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.3f, "MarketTableSide"), "bakery", materials);
            TintMarketObject(InstantiatePrefab("Assets/Low-Poly Medieval Market/Prefabs/wooden_box.prefab", root.transform, new Vector3(-15.4f, 0f, 95.5f), Quaternion.identity, Vector3.one * 2f, "MarketWoodBox_1"), "gift", materials);
            TintMarketObject(InstantiatePrefab("Assets/Low-Poly Medieval Market/Prefabs/stand_01.prefab", root.transform, new Vector3(16.8f, 0f, 88f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.4f, "MarketStand_1"), "produce", materials);
            CreateStoragePoint(root.transform, materials);
        }
        private static void CreateTempleArea(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("TempleArea");
            root.transform.SetParent(parent, false);

            CreateGround("TempleCourtyardGround", root.transform, new Vector3(55f, -0.46f, 131f), new Vector3(60f, 1f, 70f), materials["garden"]);
            CreateGround("TempleEntryPath", root.transform, new Vector3(55f, -0.25f, 88f), new Vector3(60f, 0.12f, 16f), materials["urban_path"]);
            CreateGround("TempleAltarPath", root.transform, new Vector3(55f, -0.24f, 115f), new Vector3(14f, 0.12f, 28f), materials["urban_path"]);
            CreateGround("TempleMainPad", root.transform, new Vector3(55f, -0.28f, 140f), new Vector3(32f, 0.14f, 32f), materials["market"]);
            CreateGround("TempleHallBase", root.transform, new Vector3(55f, 0.02f, 150f), new Vector3(32f, 0.2f, 28f), materials["home_dirt"]);

            CreateTempleGate(root.transform, materials);
            CreateTempleHall(root.transform, materials);
            CreatePerimeterWall(root.transform, materials);
            CreateLotusPond(root.transform, materials);
            CreateStoneStele(root.transform, new Vector3(40f, 0f, 105f), materials);
            CreateStoneStele(root.transform, new Vector3(70f, 0f, 105f), materials);
            CreateTempleBellTower(root.transform, new Vector3(35f, 0f, 120f), materials);
            CreateLargeIncenseBurner(root.transform, new Vector3(55f, 0f, 135f), materials);
            CreateOfferingStalls(root.transform, materials);
            CreateTempleCourtyardDecor(root.transform, materials);
            CreateTempleMonks(root.transform, materials);
            CreateTempleInteractables(root.transform, materials);
        }

        private static void CreateTempleGate(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject gateRoot = new GameObject("TempleGate");
            gateRoot.transform.SetParent(parent, false);
            gateRoot.transform.position = new Vector3(55f, 0f, 96.2f);

            CreateCubePart("GatePillarLeftMain", gateRoot.transform, new Vector3(-6f, 3f, 0f), new Vector3(1.8f, 6f, 1.8f), materials["wall_light"]);
            CreateCubePart("GatePillarRightMain", gateRoot.transform, new Vector3(6f, 3f, 0f), new Vector3(1.8f, 6f, 1.8f), materials["wall_light"]);
            CreateCubePart("GateBeamMain", gateRoot.transform, new Vector3(0f, 5.7f, 0f), new Vector3(13.8f, 0.6f, 0.9f), materials["accent"]);
            CreateCubePart("GateSignMain", gateRoot.transform, new Vector3(0f, 6.6f, 0f), new Vector3(7.2f, 1.2f, 0.6f), materials["roof_dark"]);
            CreateCubePart("GateRoofMainLeft", gateRoot.transform, new Vector3(-2.7f, 7.8f, 0f), new Vector3(8.2f, 1.2f, 3.8f), materials["roof_light"], Quaternion.Euler(0f, 0f, 15f));
            CreateCubePart("GateRoofMainRight", gateRoot.transform, new Vector3(2.7f, 7.8f, 0f), new Vector3(8.2f, 1.2f, 3.8f), materials["roof_light"], Quaternion.Euler(0f, 0f, -15f));
            CreateCubePart("GateRidgeMain", gateRoot.transform, new Vector3(0f, 8.5f, 0f), new Vector3(9f, 0.6f, 0.9f), materials["roof_dark"]);
            CreateCubePart("GatePillarFarLeft", gateRoot.transform, new Vector3(-12f, 2.2f, 0f), new Vector3(1.3f, 4.5f, 1.3f), materials["wall_light"]);
            CreateCubePart("GateBeamLeft", gateRoot.transform, new Vector3(-9f, 4.2f, 0f), new Vector3(5.5f, 0.4f, 0.7f), materials["accent"]);
            CreateCubePart("GateRoofSideLeft1", gateRoot.transform, new Vector3(-10.5f, 5.7f, 0f), new Vector3(4.8f, 0.9f, 3f), materials["roof_light"], Quaternion.Euler(0f, 0f, 15f));
            CreateCubePart("GateRoofSideLeft2", gateRoot.transform, new Vector3(-7.5f, 5.7f, 0f), new Vector3(4.8f, 0.9f, 3f), materials["roof_light"], Quaternion.Euler(0f, 0f, -15f));
            CreateCubePart("GateRidgeLeft", gateRoot.transform, new Vector3(-9f, 6.3f, 0f), new Vector3(5.2f, 0.4f, 0.7f), materials["roof_dark"]);
            CreateCubePart("GatePillarFarRight", gateRoot.transform, new Vector3(12f, 2.2f, 0f), new Vector3(1.3f, 4.5f, 1.3f), materials["wall_light"]);
            CreateCubePart("GateBeamRight", gateRoot.transform, new Vector3(9f, 4.2f, 0f), new Vector3(5.5f, 0.4f, 0.7f), materials["accent"]);
            CreateCubePart("GateRoofSideRight1", gateRoot.transform, new Vector3(7.5f, 5.7f, 0f), new Vector3(4.8f, 0.9f, 3f), materials["roof_light"], Quaternion.Euler(0f, 0f, 15f));
            CreateCubePart("GateRoofSideRight2", gateRoot.transform, new Vector3(10.5f, 5.7f, 0f), new Vector3(4.8f, 0.9f, 3f), materials["roof_light"], Quaternion.Euler(0f, 0f, -15f));
            CreateCubePart("GateRidgeRight", gateRoot.transform, new Vector3(9f, 6.3f, 0f), new Vector3(5.2f, 0.4f, 0.7f), materials["roof_dark"]);
            CreateRoofDragonAccent(gateRoot.transform, new Vector3(0f, 9.2f, 0f), 1.2f, materials);
            CreateDauDao(gateRoot.transform, new Vector3(-4.5f, 8.4f, 0f), Quaternion.Euler(0f, 0f, -45f), 1.5f, materials);
            CreateDauDao(gateRoot.transform, new Vector3(4.5f, 8.4f, 0f), Quaternion.Euler(0f, 0f, 45f), 1.5f, materials);
        }

        private static void CreateTempleHall(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject hall = new GameObject("TempleHall");
            hall.transform.SetParent(parent, false);
            hall.transform.position = new Vector3(55f, 0f, 150f);
            hall.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            CreateCubePart("Foundation", hall.transform, new Vector3(0f, 0.8f, 0f), new Vector3(32f, 1.6f, 28f), materials["foundation"]);
            CreateCubePart("HallWallLeft", hall.transform, new Vector3(-14f, 4.6f, 0f), new Vector3(1.2f, 6.3f, 22.4f), materials["wall_light"]);
            CreateCubePart("HallWallRight", hall.transform, new Vector3(14f, 4.6f, 0f), new Vector3(1.2f, 6.3f, 22.4f), materials["wall_light"]);
            CreateCubePart("HallWallBack", hall.transform, new Vector3(0f, 4.6f, -10.6f), new Vector3(29.2f, 6.3f, 1.2f), materials["wall_light"]);
            CreateCubePart("HallWallFrontLeft", hall.transform, new Vector3(-9.2f, 4.6f, 10.6f), new Vector3(9.2f, 6.3f, 1.2f), materials["wall_light"]);
            CreateCubePart("HallWallFrontRight", hall.transform, new Vector3(9.2f, 4.6f, 10.6f), new Vector3(9.2f, 6.3f, 1.2f), materials["wall_light"]);
            CreateCubePart("HallWallFrontTop", hall.transform, new Vector3(0f, 6.7f, 10.6f), new Vector3(9.2f, 2.1f, 1.2f), materials["wall_light"]);

            GameObject roofGroup = new GameObject("HallRoofGroup");
            roofGroup.transform.SetParent(hall.transform, false);
            CreateCubePart("HallShadow", roofGroup.transform, new Vector3(0f, 7.8f, 0f), new Vector3(26.8f, 0.6f, 20.8f), materials["wall_shadow"]);
            CreateCubePart("HallRoofLeft", roofGroup.transform, new Vector3(-3.6f, 9.3f, 0f), new Vector3(17f, 1.3f, 27.2f), materials["roof_light"], Quaternion.Euler(0f, 0f, 32f));
            CreateCubePart("HallRoofRight", roofGroup.transform, new Vector3(3.6f, 9.3f, 0f), new Vector3(17f, 1.3f, 27.2f), materials["roof_dark"], Quaternion.Euler(0f, 0f, -32f));
            CreateCubePart("HallRidge", roofGroup.transform, new Vector3(0f, 10.2f, 0f), new Vector3(28f, 0.6f, 1f), materials["roof_dark"]);
            CreateCubePart("HallDoorFrameLeft", hall.transform, new Vector3(-4.6f, 3.9f, 10.4f), new Vector3(0.6f, 4.5f, 0.6f), materials["wood_dark"]);
            CreateCubePart("HallDoorFrameRight", hall.transform, new Vector3(4.6f, 3.9f, 10.4f), new Vector3(0.6f, 4.5f, 0.6f), materials["wood_dark"]);
            CreateCubePart("HallDoorFrameTop", hall.transform, new Vector3(0f, 6.1f, 10.4f), new Vector3(10f, 0.6f, 0.6f), materials["wood_dark"]);
            CreateCubePart("HallNamePlaque", hall.transform, new Vector3(0f, 5.7f, 11f), new Vector3(7.6f, 0.7f, 0.25f), materials["lantern"]);
            CreateCubePart("HallWindowLeft", hall.transform, new Vector3(-9.6f, 3.7f, 11f), new Vector3(3.6f, 2.2f, 0.24f), materials["glass"]);
            CreateCubePart("HallWindowRight", hall.transform, new Vector3(9.6f, 3.7f, 11f), new Vector3(3.6f, 2.2f, 0.24f), materials["glass"]);
            CreateCubePart("HallPorch", hall.transform, new Vector3(0f, 0.3f, 13.6f), new Vector3(21.6f, 0.3f, 4.4f), materials["accent"]);
            for (int i = 0; i < 6; i++)
            {
                float stepHeight = 0.266f * (i + 1);
                float stepZ = 16.5f - (i * 0.65f);
                CreateCubePart("HallEntranceStep_" + i, hall.transform, new Vector3(0f, stepHeight / 2f, stepZ), new Vector3(10f, stepHeight, 0.8f), materials["foundation"]);
            }

            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", hall.transform, new Vector3(-9.6f, 1.8f, 12.8f), Quaternion.identity, Vector3.one * 1.5f, "HallLantern_L"), materials["lantern"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", hall.transform, new Vector3(9.6f, 1.8f, 12.8f), Quaternion.identity, Vector3.one * 1.5f, "HallLantern_R"), materials["lantern"]);
            CreateBuddhaStatue(hall.transform, materials);
            CreateRoofDragonAccent(roofGroup.transform, new Vector3(0f, 11.2f, 0f), 2.5f, materials);
            CreateDauDao(roofGroup.transform, new Vector3(-8.5f, 10f, 13f), Quaternion.Euler(-30f, 0f, -45f), 2f, materials);
            CreateDauDao(roofGroup.transform, new Vector3(8.5f, 10f, 13f), Quaternion.Euler(-30f, 0f, 45f), 2f, materials);
            CreateDauDao(roofGroup.transform, new Vector3(-8.5f, 10f, -13f), Quaternion.Euler(30f, 0f, -45f), 2f, materials);
            CreateDauDao(roofGroup.transform, new Vector3(8.5f, 10f, -13f), Quaternion.Euler(30f, 0f, 45f), 2f, materials);

            GameObject interiorTrigger = new GameObject("TempleInteriorTrigger");
            interiorTrigger.transform.SetParent(hall.transform, false);
            interiorTrigger.transform.localPosition = new Vector3(0f, 3f, 0f);
            BoxCollider interiorBox = interiorTrigger.AddComponent<BoxCollider>();
            interiorBox.isTrigger = true;
            interiorBox.size = new Vector3(26f, 6f, 20f);
            TempleInteriorTrigger interior = interiorTrigger.AddComponent<TempleInteriorTrigger>();
            interior.roofGroup = roofGroup;
        }

        private static void CreateBuddhaStatue(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject buddhaRoot = new GameObject("BuddhaStatue");
            buddhaRoot.transform.SetParent(parent, false);
            buddhaRoot.transform.localPosition = new Vector3(0f, 1.6f, -5f);
            CreateCubePart("PedestalBase", buddhaRoot.transform, new Vector3(0f, 0.5f, 0f), new Vector3(10f, 1f, 8f), materials["wood_dark"]);
            CreateCubePart("PedestalTier2", buddhaRoot.transform, new Vector3(0f, 1.4f, 0f), new Vector3(8f, 0.8f, 6.6f), materials["accent"]);
            CreateCubePart("LotusBase", buddhaRoot.transform, new Vector3(0f, 2f, 0f), new Vector3(7f, 0.4f, 5.6f), materials["market_red"]);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(radians) * 3.3f, 2.1f, Mathf.Cos(radians) * 2.6f);
                CreateCubePart("LotusPetal_" + i, buddhaRoot.transform, pos, new Vector3(1.6f, 0.6f, 0.4f), materials["market_red"], Quaternion.Euler(15f, angle, 0f));
            }
            CreateCubePart("LeftLegCrossed", buddhaRoot.transform, new Vector3(-1.8f, 2.5f, 0.8f), new Vector3(3.8f, 0.7f, 1.2f), materials["coin"], Quaternion.Euler(0f, 25f, -10f));
            CreateCubePart("RightLegCrossed", buddhaRoot.transform, new Vector3(1.8f, 2.5f, 0.8f), new Vector3(3.8f, 0.7f, 1.2f), materials["coin"], Quaternion.Euler(0f, -25f, 10f));
            CreateCubePart("BuddhaTorso", buddhaRoot.transform, new Vector3(0f, 4.2f, 0f), new Vector3(3.6f, 2.8f, 2.2f), materials["coin"]);
            CreateCubePart("RobeSash", buddhaRoot.transform, new Vector3(-0.4f, 4.3f, 1.15f), new Vector3(1.2f, 3f, 0.2f), materials["market_red"], Quaternion.Euler(0f, 0f, -15f));
            CreateCubePart("LeftArmUpper", buddhaRoot.transform, new Vector3(-2.1f, 4.2f, 0.2f), new Vector3(0.7f, 1.8f, 0.7f), materials["coin"], Quaternion.Euler(20f, 0f, 20f));
            CreateCubePart("RightArmUpper", buddhaRoot.transform, new Vector3(2.1f, 4.2f, 0.2f), new Vector3(0.7f, 1.8f, 0.7f), materials["coin"], Quaternion.Euler(20f, 0f, -20f));
            CreateCubePart("LeftForearm", buddhaRoot.transform, new Vector3(-1.2f, 3.1f, 1f), new Vector3(0.6f, 0.6f, 1.8f), materials["coin"], Quaternion.Euler(0f, 30f, 0f));
            CreateCubePart("RightForearm", buddhaRoot.transform, new Vector3(1.2f, 3.1f, 1f), new Vector3(0.6f, 0.6f, 1.8f), materials["coin"], Quaternion.Euler(0f, -30f, 0f));
            CreateCubePart("HandsMudra", buddhaRoot.transform, new Vector3(0f, 2.9f, 1.8f), new Vector3(1.2f, 0.4f, 0.8f), materials["coin"]);
            CreateCubePart("BuddhaNeck", buddhaRoot.transform, new Vector3(0f, 5.8f, 0f), new Vector3(0.8f, 0.6f, 0.8f), materials["coin"]);
            CreateCubePart("BuddhaHead", buddhaRoot.transform, new Vector3(0f, 6.7f, 0.2f), new Vector3(1.8f, 1.8f, 1.8f), materials["coin"]);
            CreateCubePart("BuddhaUshnisha", buddhaRoot.transform, new Vector3(0f, 7.8f, 0.2f), new Vector3(0.8f, 0.6f, 0.8f), materials["wood_dark"]);
            CreateCubePart("BuddhaHalo", buddhaRoot.transform, new Vector3(0f, 6.2f, -1.2f), new Vector3(6.5f, 6.5f, 0.2f), materials["lantern"]);
            CreateCubePart("BuddhaHaloOuter", buddhaRoot.transform, new Vector3(0f, 6.2f, -1.25f), new Vector3(8f, 8f, 0.1f), materials["coin"]);
        }

        private static void CreateTempleBellTower(Transform parent, Vector3 position, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("BellTower");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            CreateCubePart("TowerBase", root.transform, new Vector3(0f, 0.3f, 0f), new Vector3(6f, 0.6f, 6f), materials["foundation"]);
            CreateCubePart("Pillar1", root.transform, new Vector3(-2.5f, 2.5f, -2.5f), new Vector3(0.5f, 4f, 0.5f), materials["wood_dark"]);
            CreateCubePart("Pillar2", root.transform, new Vector3(2.5f, 2.5f, -2.5f), new Vector3(0.5f, 4f, 0.5f), materials["wood_dark"]);
            CreateCubePart("Pillar3", root.transform, new Vector3(-2.5f, 2.5f, 2.5f), new Vector3(0.5f, 4f, 0.5f), materials["wood_dark"]);
            CreateCubePart("Pillar4", root.transform, new Vector3(2.5f, 2.5f, 2.5f), new Vector3(0.5f, 4f, 0.5f), materials["wood_dark"]);
            CreateCubePart("RoofLower", root.transform, new Vector3(0f, 5f, 0f), new Vector3(7f, 0.8f, 7f), materials["roof_dark"]);
            CreateCubePart("RoofUpper", root.transform, new Vector3(0f, 6.2f, 0f), new Vector3(5f, 0.8f, 5f), materials["roof_light"]);
            CreateCubePart("RoofTop", root.transform, new Vector3(0f, 6.8f, 0f), new Vector3(2f, 0.6f, 2f), materials["lantern"]);
            GameObject bell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bell.name = "BronzeBell";
            bell.transform.SetParent(root.transform, false);
            bell.transform.localPosition = new Vector3(0f, 3.2f, 0f);
            bell.transform.localScale = new Vector3(1.8f, 1.2f, 1.8f);
            ApplyMaterialRecursive(bell, materials["roof_dark"]);
            Object.DestroyImmediate(bell.GetComponent<Collider>());
        }

        private static void CreateLargeIncenseBurner(Transform parent, Vector3 position, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("LargeIncenseBurner");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            CreateCubePart("Leg1", root.transform, new Vector3(0f, 0.4f, 1.2f), new Vector3(0.4f, 0.8f, 0.4f), materials["roof_dark"]);
            CreateCubePart("Leg2", root.transform, new Vector3(-1f, 0.4f, -0.6f), new Vector3(0.4f, 0.8f, 0.4f), materials["roof_dark"]);
            CreateCubePart("Leg3", root.transform, new Vector3(1f, 0.4f, -0.6f), new Vector3(0.4f, 0.8f, 0.4f), materials["roof_dark"]);

            GameObject cookingPotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TempleCookingPotPath);
            if (cookingPotPrefab != null)
            {
                GameObject pot = InstantiatePrefab(TempleCookingPotPath, root.transform, new Vector3(0f, 1.05f, 0f), Quaternion.identity, Vector3.one * 3f, "TempleCookingPot");
                DisableNestedPhysicsComponents(pot);
            }
            else
            {
                GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bowl.name = "BurnerBowl";
                bowl.transform.SetParent(root.transform, false);
                bowl.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                bowl.transform.localScale = new Vector3(3f, 0.6f, 3f);
                ApplyMaterialRecursive(bowl, materials["roof_dark"]);
                Object.DestroyImmediate(bowl.GetComponent<Collider>());
            }

            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "BurnerRim";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            rim.transform.localScale = new Vector3(3.2f, 0.1f, 3.2f);
            ApplyMaterialRecursive(rim, materials["accent"]);
            Object.DestroyImmediate(rim.GetComponent<Collider>());
            CreateTempleIncenseSmoke(root.transform, new Vector3(0f, 2.2f, 0f), 3f);
        }

        private static void CreateOfferingStalls(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("OfferingStalls");
            root.transform.SetParent(parent, false);
            CreateSingleStall(root.transform, new Vector3(45f, 0f, 105f), materials);
            CreateSingleStall(root.transform, new Vector3(65f, 0f, 105f), materials);
        }

        private static void CreateSingleStall(Transform parent, Vector3 position, Dictionary<string, Material> materials)
        {
            GameObject stall = new GameObject("Stall");
            stall.transform.SetParent(parent, false);
            stall.transform.position = position;
            CreateCubePart("TableLegs", stall.transform, new Vector3(0f, 0.4f, 0f), new Vector3(2.6f, 0.8f, 1.4f), materials["wood_dark"]);
            CreateCubePart("TableTop", stall.transform, new Vector3(0f, 0.8f, 0f), new Vector3(3f, 0.1f, 1.6f), materials["wood"]);
            CreateCubePart("RedCloth", stall.transform, new Vector3(0f, 0.86f, 0f), new Vector3(2.8f, 0.05f, 1.5f), materials["market_red"]);
            CreateCubePart("IncenseBundle1", stall.transform, new Vector3(-0.8f, 1f, -0.2f), new Vector3(0.3f, 0.1f, 0.8f), materials["roof_dark"]);
            CreateCubePart("IncenseBundle2", stall.transform, new Vector3(-0.4f, 1f, -0.2f), new Vector3(0.3f, 0.1f, 0.8f), materials["roof_dark"]);

            GameObject fruit1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruit1.transform.SetParent(stall.transform, false);
            fruit1.transform.localPosition = new Vector3(0.5f, 1f, 0.2f);
            fruit1.transform.localScale = Vector3.one * 0.3f;
            ApplyMaterialRecursive(fruit1, materials["lantern"]);
            Object.DestroyImmediate(fruit1.GetComponent<Collider>());

            GameObject fruit2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruit2.transform.SetParent(stall.transform, false);
            fruit2.transform.localPosition = new Vector3(0.8f, 1f, 0.1f);
            fruit2.transform.localScale = Vector3.one * 0.3f;
            ApplyMaterialRecursive(fruit2, materials["lantern"]);
            Object.DestroyImmediate(fruit2.GetComponent<Collider>());
        }

        private static void CreatePerimeterWall(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("PerimeterWall");
            root.transform.SetParent(parent, false);
            CreateCubePart("LeftWall", root.transform, new Vector3(25f, 1.2f, 131f), new Vector3(0.6f, 2.4f, 70f), materials["wall_light"]);
            CreateCubePart("LeftWallRoof", root.transform, new Vector3(25f, 2.5f, 131f), new Vector3(1.2f, 0.4f, 70.2f), materials["roof_dark"]);
            CreateCubePart("RightWall", root.transform, new Vector3(85f, 1.2f, 131f), new Vector3(0.6f, 2.4f, 70f), materials["wall_light"]);
            CreateCubePart("RightWallRoof", root.transform, new Vector3(85f, 2.5f, 131f), new Vector3(1.2f, 0.4f, 70.2f), materials["roof_dark"]);
            CreateCubePart("BackWall", root.transform, new Vector3(55f, 1.2f, 166f), new Vector3(60.6f, 2.4f, 0.6f), materials["wall_light"]);
            CreateCubePart("BackWallRoof", root.transform, new Vector3(55f, 2.5f, 166f), new Vector3(60.6f, 0.4f, 1.2f), materials["roof_dark"]);
            CreateCubePart("FrontWallLeft", root.transform, new Vector3(34f, 1.2f, 96f), new Vector3(18f, 2.4f, 0.6f), materials["wall_light"]);
            CreateCubePart("FrontWallLeftRoof", root.transform, new Vector3(34f, 2.5f, 96f), new Vector3(18f, 0.4f, 1.2f), materials["roof_dark"]);
            CreateCubePart("FrontWallRight", root.transform, new Vector3(76f, 1.2f, 96f), new Vector3(18f, 2.4f, 0.6f), materials["wall_light"]);
            CreateCubePart("FrontWallRightRoof", root.transform, new Vector3(76f, 2.5f, 96f), new Vector3(18f, 0.4f, 1.2f), materials["roof_dark"]);
        }

        private static void CreateLotusPond(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("LotusPond");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(55f, 0f, 112f);

            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "PondRim";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            rim.transform.localScale = new Vector3(16.4f, 0.12f, 16.4f);
            ApplyMaterialRecursive(rim, materials["foundation"]);
            Object.DestroyImmediate(rim.GetComponent<Collider>());

            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "PondWater";
            water.transform.SetParent(root.transform, false);
            water.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            water.transform.localScale = new Vector3(16f, 0.05f, 16f);
            ApplyMaterialRecursive(water, materials["glass"]);
            Object.DestroyImmediate(water.GetComponent<Collider>());

            CreateCubePart("BridgeBase1", root.transform, new Vector3(0f, 0.2f, -8f), new Vector3(4f, 0.4f, 1f), materials["foundation"]);
            CreateCubePart("BridgeBase2", root.transform, new Vector3(0f, 0.2f, 8f), new Vector3(4f, 0.4f, 1f), materials["foundation"]);
            CreateCubePart("BridgeArch1", root.transform, new Vector3(0f, 0.6f, -5.5f), new Vector3(4f, 0.4f, 4.5f), materials["foundation"], Quaternion.Euler(15f, 0f, 0f));
            CreateCubePart("BridgeArch2", root.transform, new Vector3(0f, 0.6f, 5.5f), new Vector3(4f, 0.4f, 4.5f), materials["foundation"], Quaternion.Euler(-15f, 0f, 0f));
            CreateCubePart("BridgeMid1", root.transform, new Vector3(0f, 1.1f, -2.2f), new Vector3(4f, 0.4f, 2.5f), materials["foundation"], Quaternion.Euler(5f, 0f, 0f));
            CreateCubePart("BridgeMid2", root.transform, new Vector3(0f, 1.1f, 2.2f), new Vector3(4f, 0.4f, 2.5f), materials["foundation"], Quaternion.Euler(-5f, 0f, 0f));
            CreateCubePart("BridgeTop", root.transform, new Vector3(0f, 1.25f, 0f), new Vector3(4f, 0.4f, 2f), materials["foundation"]);

            for (float zOffset = -8f; zOffset <= 8f; zOffset += 2f)
            {
                float yValue = 0.2f;
                if (Mathf.Abs(zOffset) < 1f) yValue = 1.25f;
                else if (Mathf.Abs(zOffset) < 3f) yValue = 1.1f;
                else if (Mathf.Abs(zOffset) < 6f) yValue = 0.6f;

                CreateCubePart("RailingPostL_" + zOffset, root.transform, new Vector3(-2.1f, yValue + 0.6f, zOffset), new Vector3(0.2f, 1f, 0.2f), materials["foundation"]);
                CreateCubePart("RailingPostR_" + zOffset, root.transform, new Vector3(2.1f, yValue + 0.6f, zOffset), new Vector3(0.2f, 1f, 0.2f), materials["foundation"]);
            }

            CreateCubePart("HandrailL1", root.transform, new Vector3(-2.1f, 1.2f, -5.5f), new Vector3(0.15f, 0.15f, 5f), materials["accent"], Quaternion.Euler(15f, 0f, 0f));
            CreateCubePart("HandrailL2", root.transform, new Vector3(-2.1f, 1.7f, -2.2f), new Vector3(0.15f, 0.15f, 2.5f), materials["accent"], Quaternion.Euler(5f, 0f, 0f));
            CreateCubePart("HandrailL3", root.transform, new Vector3(-2.1f, 1.85f, 0f), new Vector3(0.15f, 0.15f, 2f), materials["accent"]);
            CreateCubePart("HandrailL4", root.transform, new Vector3(-2.1f, 1.7f, 2.2f), new Vector3(0.15f, 0.15f, 2.5f), materials["accent"], Quaternion.Euler(-5f, 0f, 0f));
            CreateCubePart("HandrailL5", root.transform, new Vector3(-2.1f, 1.2f, 5.5f), new Vector3(0.15f, 0.15f, 5f), materials["accent"], Quaternion.Euler(-15f, 0f, 0f));
            CreateCubePart("HandrailR1", root.transform, new Vector3(2.1f, 1.2f, -5.5f), new Vector3(0.15f, 0.15f, 5f), materials["accent"], Quaternion.Euler(15f, 0f, 0f));
            CreateCubePart("HandrailR2", root.transform, new Vector3(2.1f, 1.7f, -2.2f), new Vector3(0.15f, 0.15f, 2.5f), materials["accent"], Quaternion.Euler(5f, 0f, 0f));
            CreateCubePart("HandrailR3", root.transform, new Vector3(2.1f, 1.85f, 0f), new Vector3(0.15f, 0.15f, 2f), materials["accent"]);
            CreateCubePart("HandrailR4", root.transform, new Vector3(2.1f, 1.7f, 2.2f), new Vector3(0.15f, 0.15f, 2.5f), materials["accent"], Quaternion.Euler(-5f, 0f, 0f));
            CreateCubePart("HandrailR5", root.transform, new Vector3(2.1f, 1.2f, 5.5f), new Vector3(0.15f, 0.15f, 5f), materials["accent"], Quaternion.Euler(-15f, 0f, 0f));

            for (int i = 0; i < 8; i++)
            {
                float angle = i * (360f / 8f) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * 6f;
                float z = Mathf.Sin(angle) * 6f;
                if (Mathf.Abs(x) < 2.5f)
                {
                    continue;
                }

                CreateCubePart("LotusLeaf_" + i, root.transform, new Vector3(x, 0.12f, z), new Vector3(0.8f, 0.02f, 0.8f), materials["foliage"]);
                GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = "LotusFlower_" + i;
                flower.transform.SetParent(root.transform, false);
                flower.transform.localPosition = new Vector3(x, 0.3f, z);
                flower.transform.localScale = Vector3.one * 0.4f;
                ApplyMaterialRecursive(flower, materials["market_red"]);
                Object.DestroyImmediate(flower.GetComponent<Collider>());
            }
        }

        private static void CreateStoneStele(Transform parent, Vector3 position, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("StoneStele");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            CreateCubePart("TurtleBody", root.transform, new Vector3(0f, 0.3f, 0f), new Vector3(1.6f, 0.6f, 2.2f), materials["urban_path"]);
            CreateCubePart("TurtleHead", root.transform, new Vector3(0f, 0.5f, -1.2f), new Vector3(0.6f, 0.4f, 0.6f), materials["urban_path"]);
            CreateCubePart("SteleBody", root.transform, new Vector3(0f, 2f, 0f), new Vector3(1.2f, 3f, 0.4f), materials["foundation"]);
            CreateCubePart("SteleTop", root.transform, new Vector3(0f, 3.6f, 0f), new Vector3(1.4f, 0.4f, 0.5f), materials["roof_dark"]);
        }

        private static void CreateRoofDragonAccent(Transform parent, Vector3 position, float scale, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("RoofDragonAccent");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localScale = new Vector3(scale, scale, scale);
            GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moon.name = "Moon";
            moon.transform.SetParent(root.transform, false);
            moon.transform.localScale = Vector3.one;
            ApplyMaterialRecursive(moon, materials["lantern"]);
            Object.DestroyImmediate(moon.GetComponent<Collider>());
            CreateCubePart("DragonLeft1", root.transform, new Vector3(-1f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 0.2f), materials["roof_dark"], Quaternion.Euler(0f, 0f, 30f));
            CreateCubePart("DragonLeft2", root.transform, new Vector3(-2f, -0.2f, 0f), new Vector3(1.2f, 0.4f, 0.2f), materials["roof_dark"], Quaternion.Euler(0f, 0f, -30f));
            CreateCubePart("DragonLeft3", root.transform, new Vector3(-3f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 0.2f), materials["roof_dark"], Quaternion.Euler(0f, 0f, 30f));
            CreateCubePart("DragonRight1", root.transform, new Vector3(1f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 0.2f), materials["roof_dark"], Quaternion.Euler(0f, 0f, -30f));
            CreateCubePart("DragonRight2", root.transform, new Vector3(2f, -0.2f, 0f), new Vector3(1.2f, 0.4f, 0.2f), materials["roof_dark"], Quaternion.Euler(0f, 0f, 30f));
            CreateCubePart("DragonRight3", root.transform, new Vector3(3f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 0.2f), materials["roof_dark"], Quaternion.Euler(0f, 0f, -30f));
        }

        private static void CreateDauDao(Transform parent, Vector3 localPosition, Quaternion rotation, float scale, Dictionary<string, Material> materials)
        {
            CreateCubePart("DauDao", parent, localPosition, new Vector3(1f * scale, 0.3f * scale, 1f * scale), materials["roof_dark"], rotation);
        }

        private static void CreateBanyanTree(Transform parent, Vector3 position, Dictionary<string, Material> materials)
        {
            GameObject treeRoot = new GameObject("BanyanTree");
            treeRoot.transform.SetParent(parent, false);
            treeRoot.transform.position = position;
            CreateCubePart("TrunkBase", treeRoot.transform, new Vector3(0f, 1.5f, 0f), new Vector3(1.8f, 3f, 1.6f), materials["wood_dark"]);
            CreateCubePart("TrunkMid", treeRoot.transform, new Vector3(0.2f, 3.5f, -0.2f), new Vector3(1.4f, 2f, 1.4f), materials["wood_dark"]);
            CreateCubePart("Branch1", treeRoot.transform, new Vector3(1.5f, 4.5f, 0.5f), new Vector3(2.5f, 0.8f, 0.8f), materials["wood_dark"], Quaternion.Euler(0f, 30f, 20f));
            CreateCubePart("Branch2", treeRoot.transform, new Vector3(-1.2f, 4.2f, -1f), new Vector3(2f, 0.8f, 0.8f), materials["wood_dark"], Quaternion.Euler(0f, -45f, -15f));
            CreateCubePart("Branch3", treeRoot.transform, new Vector3(0f, 5f, 1.5f), new Vector3(0.8f, 2.5f, 0.8f), materials["wood_dark"], Quaternion.Euler(20f, 0f, 0f));
            CreateCubePart("Root1", treeRoot.transform, new Vector3(2.2f, 2.2f, 0.8f), new Vector3(0.15f, 4.4f, 0.15f), materials["wood_dark"]);
            CreateCubePart("Root2", treeRoot.transform, new Vector3(-1.8f, 2f, -1.2f), new Vector3(0.15f, 4f, 0.15f), materials["wood_dark"]);
            CreateCubePart("Root3", treeRoot.transform, new Vector3(0.8f, 1.2f, 1.1f), new Vector3(0.2f, 2.4f, 0.2f), materials["wood_dark"]);
            CreateCubePart("CanopyMain", treeRoot.transform, new Vector3(0f, 6.5f, 0f), new Vector3(6.5f, 2.5f, 6f), materials["foliage"]);
            CreateCubePart("CanopyRight", treeRoot.transform, new Vector3(2.8f, 5.5f, 1f), new Vector3(4f, 2f, 4f), materials["foliage"]);
            CreateCubePart("CanopyLeft", treeRoot.transform, new Vector3(-2.5f, 5.2f, -1.5f), new Vector3(3.5f, 1.8f, 3.5f), materials["foliage"]);
            CreateCubePart("CanopyTop", treeRoot.transform, new Vector3(0.5f, 8f, 0.5f), new Vector3(4.5f, 2f, 4f), materials["foliage"]);
            CreateCubePart("CanopyFront", treeRoot.transform, new Vector3(0f, 6f, 3f), new Vector3(3f, 1.8f, 3f), materials["foliage"]);
        }

        private static void CreateTempleCourtyardDecor(Transform parent, Dictionary<string, Material> materials)
        {
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Bench.prefab", parent, new Vector3(45f, 0f, 125f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.2f, "TempleBench_L"), materials["wood"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Bench.prefab", parent, new Vector3(65f, 0f, 125f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.2f, "TempleBench_R"), materials["wood"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower Pot.prefab", parent, new Vector3(48f, 0f, 102f), Quaternion.identity, Vector3.one * 1.5f, "TempleFlower_1"), materials["flower"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower Pot.prefab", parent, new Vector3(62f, 0f, 102f), Quaternion.identity, Vector3.one * 1.5f, "TempleFlower_2"), materials["flower"]);
            CreateBanyanTree(parent, new Vector3(35f, 0f, 140f), materials);
            CreateBanyanTree(parent, new Vector3(75f, 0f, 140f), materials);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", parent, new Vector3(48f, 0f, 130f), Quaternion.identity, Vector3.one * 1.2f, "TempleLantern_1"), materials["lantern"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", parent, new Vector3(62f, 0f, 130f), Quaternion.identity, Vector3.one * 1.2f, "TempleLantern_2"), materials["lantern"]);
            SpawnScatter(parent, LowPolyNaturePrefabsRoot + "/grass2.prefab", "TempleGrass", new[]
            {
                new Vector3(40f, 0f, 100f), new Vector3(70f, 0f, 100f),
                new Vector3(38f, 0f, 125f), new Vector3(72f, 0f, 125f)
            }, 1.5f);
        }

        private static void CreateTempleMonks(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("TempleMonks");
            root.transform.SetParent(parent, false);
            CreateSweepingNovice(root.transform, new Vector3(48f, 0f, 128f), 15f, "Novice_1", materials);
            CreateSweepingNovice(root.transform, new Vector3(62f, 0f, 129f), -10f, "Novice_2", materials);
            CreateSweepingNovice(root.transform, new Vector3(53f, 0f, 126f), 0f, "Novice_3", materials);

            GameObject monkRoot = new GameObject("TempleMonk");
            monkRoot.transform.SetParent(root.transform, false);
            monkRoot.transform.position = new Vector3(51f, 0.05f, 135f);
            monkRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            GameObject monkVisual = InstantiatePrefab(FemalePeasantPrefabPath, monkRoot.transform, Vector3.zero, Quaternion.identity, Vector3.one * 0.95f, "TempleMonkVisual");
            DisableNestedPhysicsComponents(monkVisual);
            ApplyMotherPalette(monkVisual);
            IdleBreathingAnimation breathing = monkRoot.AddComponent<IdleBreathingAnimation>();
            breathing.visual = monkVisual.transform;
        }

        private static void CreateTempleInteractables(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("TempleGuidance");
            root.transform.SetParent(parent, false);

            CreateZoneTrigger(root.transform, "TempleZoneTrigger", new Vector3(55f, 1f, 110f), new Vector3(42f, 8f, 38f), "Temple", "", "", "", 0, false, false, "", 0);

            GameObject gateTrigger = new GameObject("TempleGateTrigger");
            gateTrigger.transform.SetParent(root.transform, false);
            gateTrigger.transform.position = new Vector3(55f, 1f, 95.2f);
            BoxCollider gateBox = gateTrigger.AddComponent<BoxCollider>();
            gateBox.isTrigger = true;
            gateBox.size = new Vector3(30f, 6f, 12f);
            gateTrigger.AddComponent<TempleGateTrigger>();

            // Hall floor y≈1.65. Blender props are spawned at play by TempleQuestLayout;
            // create named interactable anchors here so the layout can find them.
            const float hallFloorY = 1.65f;
            Vector3 offeringPos = new Vector3(55f, hallFloorY, 150.2f);
            Vector3 incensePos = new Vector3(55f, hallFloorY, 153.4f);
            Vector3 fortunePos = new Vector3(63f, hallFloorY, 150.5f);

            GameObject altarVisual = new GameObject("TempleOfferingAltar");
            altarVisual.transform.SetParent(root.transform, false);
            altarVisual.transform.position = offeringPos;

            GameObject offeringPoint = new GameObject("TempleOfferingPoint");
            offeringPoint.transform.SetParent(root.transform, false);
            offeringPoint.transform.position = offeringPos + Vector3.up;
            BoxCollider offeringBox = offeringPoint.AddComponent<BoxCollider>();
            offeringBox.isTrigger = true;
            offeringBox.size = new Vector3(5.5f, 3f, 4f);
            TempleInteractionPoint offering = offeringPoint.AddComponent<TempleInteractionPoint>();
            offering.interactionKind = TempleInteractionPoint.TempleInteractionKind.Offering;
            offering.SetPrompt("Nhấn E để dâng lễ phẩm Tết");

            GameObject incenseRoot = new GameObject("TempleIncenseSpot");
            incenseRoot.transform.SetParent(root.transform, false);
            incenseRoot.transform.position = incensePos;
            BoxCollider incenseBox = incenseRoot.AddComponent<BoxCollider>();
            incenseBox.isTrigger = true;
            incenseBox.center = new Vector3(0f, 1.05f, 0f);
            incenseBox.size = new Vector3(4.5f, 3f, 4f);
            TempleInteractionPoint incense = incenseRoot.AddComponent<TempleInteractionPoint>();
            incense.interactionKind = TempleInteractionPoint.TempleInteractionKind.Incense;
            incense.SetPrompt("Nhấn E để thắp hương");

            GameObject fortuneVisual = new GameObject("TempleFortuneStand");
            fortuneVisual.transform.SetParent(root.transform, false);
            fortuneVisual.transform.position = fortunePos;

            GameObject fortunePoint = new GameObject("TempleFortunePoint");
            fortunePoint.transform.SetParent(root.transform, false);
            fortunePoint.transform.position = fortunePos + Vector3.up;
            BoxCollider fortuneBox = fortunePoint.AddComponent<BoxCollider>();
            fortuneBox.isTrigger = true;
            fortuneBox.size = new Vector3(4.5f, 3f, 4.5f);
            TempleFortunePoint fortune = fortunePoint.AddComponent<TempleFortunePoint>();
            fortune.SetPrompt("Nhấn E để xin quẻ");
        }

        private static void CreateTempleIncenseSmoke(Transform parent, Vector3 localPos, float scaleMultiplier)
        {
            GameObject smoke = new GameObject("TempleIncenseSmoke");
            smoke.transform.SetParent(parent, false);
            smoke.transform.localPosition = localPos;
            ParticleSystem particles = smoke.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f * scaleMultiplier, 2.8f * scaleMultiplier);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.16f * scaleMultiplier, 0.32f * scaleMultiplier);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f * scaleMultiplier, 0.18f * scaleMultiplier);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.9f, 0.86f, 0.78f, 0.44f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 7f * scaleMultiplier;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.22f * scaleMultiplier;
            ParticleSystemRenderer renderer = smoke.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
            }
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static void CreateSweepingNovice(Transform parent, Vector3 position, float yaw, string name, Dictionary<string, Material> materials)
        {
            GameObject noviceRoot = new GameObject(name);
            noviceRoot.transform.SetParent(parent, false);
            noviceRoot.transform.position = position;
            noviceRoot.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            noviceRoot.transform.localScale = Vector3.one * 0.92f;
            GameObject noviceVisual = InstantiatePrefab(FemalePeasantPrefabPath, noviceRoot.transform, Vector3.zero, Quaternion.identity, Vector3.one, name + "_Visual");
            DisableNestedPhysicsComponents(noviceVisual);
            ApplyMotherPalette(noviceVisual);
            noviceVisual.transform.localPosition = new Vector3(-0.08f, 0f, 0f);
            noviceVisual.transform.localRotation = Quaternion.Euler(0f, -12f, 7f);

            GameObject broom = new GameObject(name + "_Broom");
            broom.transform.SetParent(noviceRoot.transform, false);
            broom.transform.localPosition = new Vector3(0.3f, 0.95f, 0.25f);
            broom.transform.localRotation = Quaternion.Euler(0f, 20f, 35f);

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(broom.transform, false);
            handle.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            handle.transform.localScale = new Vector3(0.08f, 0.55f, 0.08f);
            ApplyMaterialRecursive(handle, materials["wood_dark"]);
            Object.DestroyImmediate(handle.GetComponent<Collider>());

            GameObject bristles = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bristles.name = "Bristles";
            bristles.transform.SetParent(broom.transform, false);
            bristles.transform.localPosition = new Vector3(0f, -0.12f, 0.24f);
            bristles.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            bristles.transform.localScale = new Vector3(0.2f, 0.16f, 0.5f);
            ApplyMaterialRecursive(bristles, materials["wood"]);
            Object.DestroyImmediate(bristles.GetComponent<Collider>());

            CreateSweepMarks(noviceRoot.transform, materials);
            SweepingAnimation anim = noviceRoot.AddComponent<SweepingAnimation>();
            anim.broom = broom.transform;
            anim.visual = noviceVisual.transform;
        }

        private static void CreateSweepMarks(Transform parent, Dictionary<string, Material> materials)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "SweepMark_" + i;
                line.transform.SetParent(parent, false);
                line.transform.localPosition = new Vector3(0.45f + (i * 0.08f), 0.035f, 0.72f + (i * 0.12f));
                line.transform.localRotation = Quaternion.Euler(0f, -18f + (i * 9f), 0f);
                line.transform.localScale = new Vector3(0.08f, 0.018f, 0.95f);
                ApplyMaterialRecursive(line, materials["home_dirt"]);
                Object.DestroyImmediate(line.GetComponent<Collider>());
            }

            for (int i = 0; i < 5; i++)
            {
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leaf.name = "CourtyardLeaf_" + i;
                leaf.transform.SetParent(parent, false);
                leaf.transform.localPosition = new Vector3(-0.35f + (i * 0.18f), 0.045f, 0.95f + ((i % 2) * 0.18f));
                leaf.transform.localRotation = Quaternion.Euler(0f, i * 31f, 0f);
                leaf.transform.localScale = new Vector3(0.12f, 0.02f, 0.22f);
                ApplyMaterialRecursive(leaf, materials["foliage"]);
                Object.DestroyImmediate(leaf.GetComponent<Collider>());
            }
        }


        /// <summary>Match ChoTet mother to Hometown Arrival Creative Character mother.</summary>
        private static void RefreshChoTetMotherVisual(Transform worldRoot)
        {
            Transform mother = FindChildRecursive(worldRoot, "Mother");
            if (mother == null)
            {
                return;
            }

            CreativeCharactersParentSetup.SetupParentCharacters(showDialog: false);
            GameObject motherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.MotherVisualPrefab);
            if (motherPrefab == null)
            {
                return;
            }

            for (int i = mother.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(mother.GetChild(i).gameObject);
            }

            mother.localScale = Vector3.one;
            GameObject motherVisual = Object.Instantiate(motherPrefab, mother);
            motherVisual.name = "MotherVisual";
            motherVisual.transform.localPosition = Vector3.zero;
            motherVisual.transform.localRotation = Quaternion.Euler(
                0f,
                PlayerCharacterVisualPaths.CreativeCharacterModelYaw,
                0f);
            motherVisual.transform.localScale = Vector3.one * 0.94f;
            CreativeCharacterAssembly.ApplyMotherAppearanceMaterials(motherVisual);
            DisableNestedPhysicsComponents(motherVisual);

            GameObject prefabSource = Object.Instantiate(mother.gameObject);
            prefabSource.name = "MotherNPC";
            PrefabUtility.SaveAsPrefabAsset(prefabSource, MotherPrefabPath);
            Object.DestroyImmediate(prefabSource);
        }

        private static void CreateStoryCharacters(Transform parent)
        {
            GameObject root = new GameObject("StoryActors");
            root.transform.SetParent(parent, false);

            GameObject mom = new GameObject("Mother");
            mom.transform.SetParent(root.transform, false);
            mom.transform.position = new Vector3(0f, 0.05f, 10f);
            mom.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            mom.transform.localScale = Vector3.one * 1.0f;

            // Same Creative Character mother as Hometown Arrival (Level 1).
            CreativeCharactersParentSetup.SetupParentCharacters(showDialog: false);
            GameObject motherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.MotherVisualPrefab);
            GameObject motherVisual;
            if (motherPrefab != null)
            {
                motherVisual = Object.Instantiate(motherPrefab, mom.transform);
                motherVisual.name = "MotherVisual";
                motherVisual.transform.localPosition = Vector3.zero;
                motherVisual.transform.localRotation = Quaternion.Euler(
                    0f,
                    PlayerCharacterVisualPaths.CreativeCharacterModelYaw,
                    0f);
                motherVisual.transform.localScale = Vector3.one * 0.94f;
                CreativeCharacterAssembly.ApplyMotherAppearanceMaterials(motherVisual);
                DisableNestedPhysicsComponents(motherVisual);
            }
            else
            {
                string motherVisualPath = AssetDatabase.LoadAssetAtPath<GameObject>(MotherModelPath) != null
                    ? MotherModelPath
                    : FemalePeasantPrefabPath;
                motherVisual = InstantiatePrefab(
                    motherVisualPath,
                    mom.transform,
                    Vector3.zero,
                    Quaternion.identity,
                    Vector3.one,
                    "MotherVisual");
                DisableNestedPhysicsComponents(motherVisual);
                ApplyMotherVertexPalette(motherVisual);
                motherVisual.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            }

            BoxCollider box = mom.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 0.95f, 0f);
            box.size = new Vector3(0.95f, 2f, 0.95f);

            MotherQuestGiver motherQuest = mom.AddComponent<MotherQuestGiver>();
            motherQuest.npcName = "Mẹ";
            motherQuest.SetPrompt("Nhấn E để trò chuyện");

            GameObject prefabSource = Object.Instantiate(mom);
            prefabSource.name = "MotherNPC";
            PrefabUtility.SaveAsPrefabAsset(prefabSource, MotherPrefabPath);
            Object.DestroyImmediate(prefabSource);
        }

        private static void CreateZoneTriggers(Transform parent)
        {
            GameObject root = new GameObject("ZoneTriggers");
            root.transform.SetParent(parent, false);

            CreateZoneTrigger(root.transform, "HomeZone", new Vector3(-7f, 1f, 8f), new Vector3(28f, 4f, 24f), "Home", "", "", "", 0, true, false, "", 0);
            CreateZoneTrigger(root.transform, "GardenZone", new Vector3(0f, 1f, 28f), new Vector3(34f, 4f, 20f), "Garden", "", "Lối đi sân vườn", "Khu vực yên tĩnh này là nơi thích hợp để bạn chuẩn bị và lên kế hoạch.", 0, true, false, "", 0);
            CreateZoneTrigger(root.transform, "RoadCrowdTrigger", new Vector3(0f, 1f, 52f), new Vector3(14f, 4f, 12f), "Road", "crowd", "", "", 12, true, false, "", 0);
            CreateZoneTrigger(root.transform, "RoadZoneTail", new Vector3(0f, 1f, 70f), new Vector3(14f, 4f, 18f), "Road", "", "", "", 0, false, false, "", 0);
            CreateZoneTrigger(root.transform, "MarketEntryTrigger", new Vector3(0f, 1f, 80f), new Vector3(36f, 4f, 10f), "Market", "friend_discount", "", "", 0, true, false, "co_lan_hoa", 2);
        }

        private static void CreateStoragePoint(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("CourierPoint");
            root.transform.SetParent(parent, false);

            GameObject crate = InstantiatePrefab(
                "Assets/Low-Poly Medieval Market/Prefabs/wooden_box.prefab",
                root.transform,
                new Vector3(18f, 0f, 88f),
                Quaternion.identity,
                Vector3.one * 2.2f,
                "CourierCrate"
            );
            ApplyMaterialRecursive(crate, materials["wood"]);
            ApplyMaterialRecursive(InstantiatePrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab", root.transform, new Vector3(19.5f, 0f, 88f), Quaternion.identity, Vector3.one * 1.15f, "CourierLantern"), materials["lantern"]);

            GameObject triggerObject = new GameObject("CourierTrigger");
            triggerObject.transform.SetParent(root.transform, false);
            triggerObject.transform.position = new Vector3(18f, 1f, 84f);
            BoxCollider box = triggerObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(4.2f, 2.5f, 4f);

            StorageInteractable storage = triggerObject.AddComponent<StorageInteractable>();
            storage.storageId = "courier_drop";
            storage.titleText = "Xe ôm chuyển đồ";
            storage.bodyText = "Gửi các túi đồ đã sắm về nhà trước để có chỗ sắm tiếp.";
            storage.SetPrompt("Nhấn E để gửi đồ về nhà");
        }

        private static void CreateCrowdedRoadTraffic(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("CrowdedRoadTraffic");
            root.transform.SetParent(parent, false);

            CrowdedRoadTrafficController trafficController = root.AddComponent<CrowdedRoadTrafficController>();

            GameObject spawnerObject = new GameObject("TrafficSpawner");
            spawnerObject.transform.SetParent(root.transform, false);
            TrafficSpawner spawner = spawnerObject.AddComponent<TrafficSpawner>();
            ApplyModerateAlwaysOnTrafficSettings(spawner);

            GameObject spawnRoot = new GameObject("TrafficSpawnPoints");
            spawnRoot.transform.SetParent(root.transform, false);
            spawner.spawnPoints = BuildBidirectionalTrafficSpawnPoints(spawnRoot.transform);
            WireLevel1KenneyTrafficPrefabs(spawner);

            trafficController.trafficSpawner = spawner;
            spawnerObject.SetActive(true);
            spawner.enabled = true;

            DressTrafficRoad(parent);
        }

        private static Transform CreateTrafficSpawnPoint(Transform parent, string name, Vector3 position, float yaw)
        {
            GameObject point = new GameObject(name);
            point.transform.SetParent(parent, false);
            point.transform.position = position;
            point.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            return point.transform;
        }

        private static GameObject CreateTrafficPlaceholder(Transform parent, Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject("TrafficBikeProxyPrefab");
            root.transform.SetParent(parent, false);
            root.SetActive(false);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            body.transform.localScale = new Vector3(0.58f, 0.48f, 1.45f);
            ApplyMaterialRecursive(body, materials["market_red"]);

            GameObject frontFairing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontFairing.name = "FrontFairing";
            frontFairing.transform.SetParent(root.transform, false);
            frontFairing.transform.localPosition = new Vector3(0f, 0.86f, 0.68f);
            frontFairing.transform.localScale = new Vector3(0.52f, 0.38f, 0.38f);
            ApplyMaterialRecursive(frontFairing, materials["wall_light"]);

            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.SetParent(root.transform, false);
            seat.transform.localPosition = new Vector3(0f, 0.98f, -0.25f);
            seat.transform.localScale = new Vector3(0.5f, 0.12f, 0.62f);
            ApplyMaterialRecursive(seat, materials["door_dark"]);

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handlebar";
            handle.transform.SetParent(root.transform, false);
            handle.transform.localPosition = new Vector3(0f, 1.05f, 0.65f);
            handle.transform.localScale = new Vector3(1.15f, 0.08f, 0.12f);
            ApplyMaterialRecursive(handle, materials["wood_dark"]);

            GameObject frontFork = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontFork.name = "FrontFork";
            frontFork.transform.SetParent(root.transform, false);
            frontFork.transform.localPosition = new Vector3(0f, 0.58f, 0.72f);
            frontFork.transform.localScale = new Vector3(0.1f, 0.48f, 0.1f);
            ApplyMaterialRecursive(frontFork, materials["wood_dark"]);

            GameObject frontWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frontWheel.name = "FrontWheel";
            frontWheel.transform.SetParent(root.transform, false);
            frontWheel.transform.localPosition = new Vector3(0f, 0.32f, 0.78f);
            frontWheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            frontWheel.transform.localScale = new Vector3(0.28f, 0.12f, 0.28f);
            ApplyMaterialRecursive(frontWheel, materials["wood_dark"]);

            GameObject rearWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rearWheel.name = "RearWheel";
            rearWheel.transform.SetParent(root.transform, false);
            rearWheel.transform.localPosition = new Vector3(0f, 0.32f, -0.78f);
            rearWheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rearWheel.transform.localScale = new Vector3(0.28f, 0.12f, 0.28f);
            ApplyMaterialRecursive(rearWheel, materials["wood_dark"]);

            GameObject rearRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearRack.name = "RearRack";
            rearRack.transform.SetParent(root.transform, false);
            rearRack.transform.localPosition = new Vector3(0f, 0.92f, -0.86f);
            rearRack.transform.localScale = new Vector3(0.48f, 0.07f, 0.28f);
            ApplyMaterialRecursive(rearRack, materials["wood_dark"]);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0.65f, 0f);
            collider.size = new Vector3(1.2f, 1.3f, 2.25f);
            return root;
        }

        private static void DisableNestedPhysicsComponents(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].gameObject != root)
                {
                    colliders[i].enabled = false;
                }
            }

            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] != null && bodies[i].gameObject != root)
                {
                    bodies[i].isKinematic = true;
                    bodies[i].useGravity = false;
                }
            }
        }

        private static void ApplyMotherPaletteIfNeeded(GameObject root)
        {
            if (NeedsMotherPalette(root))
            {
                ApplyMotherPalette(root);
            }
        }

        private static bool NeedsMotherPalette(GameObject root)
        {
            if (root == null)
            {
                return true;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] mats = renderers[i].sharedMaterials;
                if (mats == null || mats.Length == 0)
                {
                    return true;
                }

                for (int j = 0; j < mats.Length; j++)
                {
                    Material mat = mats[j];
                    if (mat == null || mat.shader == null)
                    {
                        return true;
                    }

                    string shaderName = mat.shader.name.ToLowerInvariant();
                    string matName = mat.name.ToLowerInvariant();
                    Color materialColor = Color.white;
                    if (mat.HasProperty("_BaseColor"))
                    {
                        materialColor = mat.GetColor("_BaseColor");
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        materialColor = mat.GetColor("_Color");
                    }

                    if (shaderName.Contains("error") || matName.Contains("default") || matName.Contains("magenta") || IsMagenta(materialColor))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsMagenta(Color color)
        {
            return color.r > 0.85f && color.b > 0.85f && color.g < 0.25f;
        }

        private static void ApplyMotherVertexPalette(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            MeshRenderer renderer = root.GetComponentInChildren<MeshRenderer>(true);
            MeshFilter filter = renderer != null ? renderer.GetComponent<MeshFilter>() : root.GetComponentInChildren<MeshFilter>(true);
            if (renderer == null || filter == null || filter.sharedMesh == null)
            {
                ApplyMotherPalette(root);
                return;
            }

            Mesh paletteMesh = CreateMotherPaletteMesh(filter.sharedMesh);
            if (paletteMesh != null)
            {
                filter.sharedMesh = paletteMesh;
            }

            Material material = EnsureMotherVertexPaletteMaterial();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Mesh CreateMotherPaletteMesh(Mesh sourceMesh)
        {
            if (sourceMesh == null)
            {
                return null;
            }

            AssetDatabase.DeleteAsset(MotherPaletteMeshPath);
            Mesh paletteMesh = Object.Instantiate(sourceMesh);
            paletteMesh.name = "MotherNPC_VertexPalette";
            paletteMesh.colors32 = BuildMotherVertexColors(paletteMesh);
            AssetDatabase.CreateAsset(paletteMesh, MotherPaletteMeshPath);
            EditorUtility.SetDirty(paletteMesh);
            return paletteMesh;
        }

        private static Material EnsureMotherVertexPaletteMaterial()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Scripts/ChoTet/Shaders/VertexColorLit.shader");
            if (shader == null)
            {
                shader = Shader.Find("ChoTet/VertexColorLit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MotherPaletteMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MotherPaletteMaterialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            if (material.HasProperty("_AmbientStrength"))
            {
                material.SetFloat("_AmbientStrength", 0.72f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Color32[] BuildMotherVertexColors(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Color32[] colors = new Color32[vertices.Length];
            Bounds bounds = mesh.bounds;

            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = ToLinearColor32(PickMotherColor(vertices[i], bounds));
            }

            return colors;
        }

        private static Color32 PickMotherColor(Vector3 vertex, Bounds bounds)
        {
            float y = Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertex.y);
            float x = Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertex.x);
            float z = Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertex.z);
            float centerDistance = Mathf.Abs(x - 0.5f);
            bool front = z > 0.58f;

            bool leftEyeZone = x > 0.35f && x < 0.45f && y > 0.70f && y < 0.78f && front;
            bool rightEyeZone = x > 0.55f && x < 0.65f && y > 0.70f && y < 0.78f && front;
            bool browZone = y > 0.78f && y < 0.84f && front && ((x > 0.31f && x < 0.45f) || (x > 0.55f && x < 0.69f));
            bool noseOrMouthZone = centerDistance < 0.08f && y > 0.61f && y < 0.72f && front;

            if (leftEyeZone || rightEyeZone)
            {
                return MotherEyes;
            }

            if (browZone)
            {
                return MotherHairDark;
            }

            if (noseOrMouthZone)
            {
                return MotherSkinShadow;
            }

            if (y > 0.84f)
            {
                return centerDistance < 0.20f ? MotherHairLight : MotherHairDark;
            }

            if (y > 0.68f)
            {
                if (!front || centerDistance > 0.28f)
                {
                    return MotherHairDark;
                }

                return MotherSkin;
            }

            if (y > 0.46f)
            {
                if (centerDistance < 0.13f && front)
                {
                    return MotherShirt;
                }

                if (centerDistance > 0.38f)
                {
                    return MotherSkin;
                }

                return y > 0.58f ? MotherJacketLight : MotherJacketDark;
            }

            if (y > 0.37f && centerDistance < 0.08f && front)
            {
                return MotherFloralDetail;
            }

            if (y > 0.17f)
            {
                if (centerDistance > 0.36f && y > 0.32f)
                {
                    return MotherAccessory;
                }

                return y > 0.30f ? MotherPantsLight : MotherPantsDark;
            }

            if (y > 0.09f && centerDistance > 0.22f)
            {
                return MotherSkin;
            }

            return MotherSandals;
        }

        private static void ApplyMotherPalette(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Material hairDark = CreateMaterial("Mother_HairDark.mat", Hex("#5E5B5A"));
            Material hairLight = CreateMaterial("Mother_HairLight.mat", Hex("#A8A3A1"));
            Material jacketDark = CreateMaterial("Mother_JacketDark.mat", Hex("#944336"));
            Material jacketLight = CreateMaterial("Mother_JacketLight.mat", Hex("#BC5D4B"));
            Material shirt = CreateMaterial("Mother_Shirt.mat", Hex("#D6CDB8"));
            Material pantsDark = CreateMaterial("Mother_PantsDark.mat", Hex("#434D3A"));
            Material pantsLight = CreateMaterial("Mother_PantsLight.mat", Hex("#5E6C52"));
            Material skin = CreateMaterial("Mother_Skin.mat", Hex("#D4A796"));
            Material skinShadow = CreateMaterial("Mother_SkinShadow.mat", Hex("#B58273"));
            Material accessories = CreateMaterial("Mother_Accessories.mat", Hex("#856E5D"));
            Material sandals = CreateMaterial("Mother_Sandals.mat", Hex("#6E5244"));
            Material eyes = CreateMaterial("Mother_Eyes.mat", Hex("#3B5866"));
            Material eyeWhite = CreateMaterial("Mother_EyeWhite.mat", Hex("#F2EFEA"));

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                string lower = renderers[i].gameObject.name.ToLowerInvariant();
                Material target = jacketDark;
                bool matched = false;

                if (lower.Contains("hair") || lower.Contains("bun") || lower.Contains("pony"))
                {
                    target = lower.Contains("light") ? hairLight : hairDark;
                    matched = true;
                }
                else if (lower.Contains("shirt"))
                {
                    target = shirt;
                    matched = true;
                }
                else if (lower.Contains("head") || lower.Contains("skin") || lower.Contains("face") || lower.Contains("hand") || lower.Contains("arm") || lower.Contains("leg"))
                {
                    target = skin;
                    matched = true;
                }
                else if (lower.Contains("jacket") || lower.Contains("coat") || lower.Contains("upper") || lower.Contains("shirt") || lower.Contains("cloth") || lower.Contains("sleeve"))
                {
                    target = lower.Contains("light") ? jacketLight : jacketDark;
                    matched = true;
                }
                else if (lower.Contains("pants") || lower.Contains("lower") || lower.Contains("skirt") || lower.Contains("trouser"))
                {
                    target = lower.Contains("light") ? pantsLight : pantsDark;
                    matched = true;
                }
                else if (lower.Contains("rope") || lower.Contains("belt") || lower.Contains("wood") || lower.Contains("bag") || lower.Contains("accessory"))
                {
                    target = accessories;
                    matched = true;
                }
                else if (lower.Contains("shoe") || lower.Contains("sandal") || lower.Contains("foot"))
                {
                    target = sandals;
                    matched = true;
                }
                else if (lower.Contains("eyewhite") || lower.Contains("sclera") || lower.Contains("white"))
                {
                    target = eyeWhite;
                    matched = true;
                }
                else if (lower.Contains("eye") || lower.Contains("iris") || lower.Contains("pupil"))
                {
                    target = eyes;
                    matched = true;
                }
                else if (lower.Contains("mouth") || lower.Contains("nose") || lower.Contains("lip"))
                {
                    target = skinShadow;
                    matched = true;
                }

                if (!matched)
                {
                    switch (i % 6)
                    {
                        case 0:
                            target = hairDark;
                            break;
                        case 1:
                            target = jacketDark;
                            break;
                        case 2:
                            target = shirt;
                            break;
                        case 3:
                            target = pantsDark;
                            break;
                        case 4:
                            target = skin;
                            break;
                        default:
                            target = sandals;
                            break;
                    }
                }
                Material[] mats = renderers[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = target;
                }

                renderers[i].sharedMaterials = mats;
            }
        }

        private static void ApplyTrafficPalette(GameObject root, bool altPalette)
        {
            if (root == null)
            {
                return;
            }

            Material bodyPrimary = CreateMaterial("Traffic_BodyRed.mat", Hex("#A64B44"));
            Material bodySecondary = CreateMaterial("Traffic_BodyBlue.mat", Hex("#5A8BA8"));
            Material accent = CreateMaterial("Traffic_Accent.mat", Hex("#DED5C5"));
            Material dark = CreateMaterial("Traffic_Dark.mat", Hex("#3D2B20"));
            Material glass = CreateMaterial("Traffic_Glass.mat", Hex("#5C6670"));
            Material wheel = CreateMaterial("Traffic_Wheel.mat", Hex("#614330"));
            Material body = altPalette ? bodySecondary : bodyPrimary;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                string lower = renderers[i].gameObject.name.ToLowerInvariant();
                Material target = body;
                bool matched = false;

                if (lower.Contains("wheel") || lower.Contains("tyre") || lower.Contains("tire") || lower.Contains("rim"))
                {
                    target = wheel;
                    matched = true;
                }
                else if (lower.Contains("glass") || lower.Contains("window") || lower.Contains("windshield") || lower.Contains("screen"))
                {
                    target = glass;
                    matched = true;
                }
                else if (lower.Contains("light") || lower.Contains("mirror") || lower.Contains("head"))
                {
                    target = accent;
                    matched = true;
                }
                else if (lower.Contains("seat") || lower.Contains("interior") || lower.Contains("dashboard") || lower.Contains("roof"))
                {
                    target = dark;
                    matched = true;
                }
                else if (lower.Contains("body") || lower.Contains("door") || lower.Contains("fender") || lower.Contains("panel") || lower.Contains("car") || lower.Contains("chassis"))
                {
                    target = body;
                    matched = true;
                }

                if (!matched)
                {
                    switch (i % 4)
                    {
                        case 0:
                            target = body;
                            break;
                        case 1:
                            target = accent;
                            break;
                        case 2:
                            target = glass;
                            break;
                        default:
                            target = wheel;
                            break;
                    }
                }

                Material[] mats = renderers[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = target;
                }

                renderers[i].sharedMaterials = mats;
            }
        }

        private static PlayerTopDownController CreatePlayer(Vector3 spawnPosition)
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.position = spawnPosition;
            CharacterController characterController = playerObject.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.38f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            PlayerTopDownController controller = playerObject.AddComponent<PlayerTopDownController>();

            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(playerObject.transform, false);
            visualRoot.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            controller.visualRoot = visualRoot.transform;

            string playerVisualPath = CreativeCharactersPlayerSetup.ResolvePlayerVisualPrefabPath();
            RuntimeAnimatorController playerAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CreativeCharactersPlayerSetup.ResolvePlayerAnimatorControllerPath());
            controller.fallbackAnimatorController = playerAnimator;

            GameObject explorerModel = InstantiatePrefab(
                playerVisualPath,
                visualRoot.transform,
                Vector3.zero,
                Quaternion.identity,
                Vector3.one,
                CreativeCharactersPlayerSetup.ResolveVisualChildName()
            );

            if (playerVisualPath == PlayerCharacterVisualPaths.MeshyExplorerVisualPrefab)
            {
                ApplyExplorerPalette(explorerModel);
                AttachExplorerFacialFeatures(explorerModel);
            }

            Animator animator = explorerModel != null ? explorerModel.GetComponentInChildren<Animator>(true) : null;
            if (animator != null)
            {
                animator.runtimeAnimatorController = playerAnimator != null ? playerAnimator : EnsureExplorerAnimatorController();
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            GameObject origin = new GameObject("InteractionOrigin");
            origin.transform.SetParent(playerObject.transform, false);
            origin.transform.localPosition = new Vector3(0f, 1f, 0f);
            controller.interactionOrigin = origin.transform;

            return controller;
        }

        private static void ApplyExplorerPalette(GameObject explorerModel)
        {
            if (explorerModel == null)
            {
                return;
            }

            SkinnedMeshRenderer smr = explorerModel.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr == null || smr.sharedMesh == null)
            {
                return;
            }

            Mesh paletteMesh = CreateExplorerPaletteMesh(smr.sharedMesh, smr.bones);
            if (paletteMesh != null)
            {
                smr.sharedMesh = paletteMesh;
            }

            Material paletteMaterial = EnsureExplorerPaletteMaterial();
            if (paletteMaterial != null)
            {
                smr.sharedMaterial = paletteMaterial;
            }

            smr.updateWhenOffscreen = true;
            smr.quality = SkinQuality.Auto;
        }

        private static void AttachExplorerFacialFeatures(GameObject explorerModel)
        {
            if (explorerModel == null)
            {
                return;
            }

            Transform head = FindChildRecursive(explorerModel.transform, "Head");
            Transform headFront = FindChildRecursive(explorerModel.transform, "headfront");
            if (head == null)
            {
                return;
            }

            Transform existing = head.Find("FaceFeatures");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            Vector3 forward = Vector3.forward;
            if (headFront != null)
            {
                Vector3 delta = headFront.position - head.position;
                if (delta.sqrMagnitude > 0.00001f)
                {
                    forward = delta.normalized;
                }
            }

            GameObject featureRoot = new GameObject("FaceFeatures");
            featureRoot.transform.SetParent(head, false);
            featureRoot.transform.position = head.position + (forward * 0.085f) + (head.up * -0.005f);
            featureRoot.transform.rotation = Quaternion.LookRotation(forward, head.up);

            Material browMaterial = CreateUnlitColorMaterial("Explorer_Brow.mat", ExplorerHairDark);
            Material eyeWhiteMaterial = CreateUnlitColorMaterial("Explorer_EyeWhite.mat", ExplorerEyeWhite);
            Material irisMaterial = CreateUnlitColorMaterial("Explorer_Iris.mat", ExplorerEyes);
            Material noseMaterial = CreateUnlitColorMaterial("Explorer_Nose.mat", ExplorerSkinShadow);
            Material mouthMaterial = CreateUnlitColorMaterial("Explorer_Mouth.mat", ExplorerMouth);

            CreateFaceQuad("Brow_L", featureRoot.transform, new Vector3(-0.032f, 0.030f, 0f), new Vector3(0.030f, 0.007f, 0.004f), browMaterial, -12f);
            CreateFaceQuad("Brow_R", featureRoot.transform, new Vector3(0.032f, 0.030f, 0f), new Vector3(0.030f, 0.007f, 0.004f), browMaterial, 12f);

            CreateFaceQuad("EyeWhite_L", featureRoot.transform, new Vector3(-0.028f, 0.010f, 0.001f), new Vector3(0.024f, 0.012f, 0.003f), eyeWhiteMaterial, -6f);
            CreateFaceQuad("EyeWhite_R", featureRoot.transform, new Vector3(0.028f, 0.010f, 0.001f), new Vector3(0.024f, 0.012f, 0.003f), eyeWhiteMaterial, 6f);

            CreateFaceQuad("Iris_L", featureRoot.transform, new Vector3(-0.029f, 0.009f, -0.001f), new Vector3(0.009f, 0.010f, 0.004f), irisMaterial, 0f);
            CreateFaceQuad("Iris_R", featureRoot.transform, new Vector3(0.029f, 0.009f, -0.001f), new Vector3(0.009f, 0.010f, 0.004f), irisMaterial, 0f);

            CreateFaceQuad("Nose", featureRoot.transform, new Vector3(0f, -0.006f, 0.001f), new Vector3(0.007f, 0.014f, 0.003f), noseMaterial, 0f);
            CreateFaceQuad("Mouth", featureRoot.transform, new Vector3(0f, -0.030f, 0.001f), new Vector3(0.018f, 0.004f, 0.003f), mouthMaterial, 0f);
        }

        private static Mesh CreateExplorerPaletteMesh(Mesh sourceMesh, Transform[] bones)
        {
            if (sourceMesh == null)
            {
                return null;
            }

            AssetDatabase.DeleteAsset(ExplorerPaletteMeshPath);
            Mesh paletteMesh = Object.Instantiate(sourceMesh);
            paletteMesh.name = "Meshy_AI_Casual_Explorer_biped_Palette";
            paletteMesh.colors32 = BuildExplorerVertexColors(paletteMesh, bones);
            AssetDatabase.CreateAsset(paletteMesh, ExplorerPaletteMeshPath);
            EditorUtility.SetDirty(paletteMesh);
            return paletteMesh;
        }

        private static Material EnsureExplorerPaletteMaterial()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Scripts/ChoTet/Shaders/VertexColorLit.shader");
            if (shader == null)
            {
                shader = Shader.Find("ChoTet/VertexColorLit");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(ExplorerPaletteMaterialPath);
            if (material == null)
            {
                if (shader == null)
                {
                    return null;
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, ExplorerPaletteMaterialPath);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            if (material.HasProperty("_AmbientStrength"))
            {
                material.SetFloat("_AmbientStrength", 0.68f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Color32[] BuildExplorerVertexColors(Mesh mesh, Transform[] bones)
        {
            Vector3[] vertices = mesh.vertices;
            BoneWeight[] weights = mesh.boneWeights;
            Color32[] colors = new Color32[vertices.Length];
            Bounds bounds = mesh.bounds;

            for (int i = 0; i < vertices.Length; i++)
            {
                BoneWeight weight = i < weights.Length ? weights[i] : default(BoneWeight);
                colors[i] = ToLinearColor32(PickExplorerColor(vertices[i], bounds, bones, weight));
            }

            return colors;
        }

        private static Color32 PickExplorerColor(Vector3 vertex, Bounds bounds, Transform[] bones, BoneWeight weight)
        {
            string primaryBone = GetPrimaryBoneName(weight, bones);
            string bone = primaryBone != null ? primaryBone.ToLowerInvariant() : string.Empty;
            float y = Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertex.y);
            float x = Mathf.InverseLerp(bounds.min.x, bounds.max.x, vertex.x);
            float z = Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertex.z);
            float centerDistance = Mathf.Abs(x - 0.5f);

            bool leftEyeZone = x > 0.34f && x < 0.45f && y > 0.72f && y < 0.79f && z > 0.63f;
            bool rightEyeZone = x > 0.55f && x < 0.66f && y > 0.72f && y < 0.79f && z > 0.63f;
            bool browZone = y > 0.78f && y < 0.83f && z > 0.60f &&
                ((x > 0.32f && x < 0.44f) || (x > 0.56f && x < 0.68f));
            bool noseZone = centerDistance < 0.04f && y > 0.68f && y < 0.75f && z > 0.66f;
            bool mouthZone = centerDistance < 0.08f && y > 0.61f && y < 0.67f && z > 0.67f;

            if (bone.Contains("foot") || bone.Contains("toe"))
            {
                return ExplorerSandals;
            }

            if (bone.Contains("hand"))
            {
                return y > 0.48f ? ExplorerSkinShadow : ExplorerSkin;
            }

            if (bone.Contains("headfront") || bone.Contains("head") || bone.Contains("neck"))
            {
                if (browZone)
                {
                    return ExplorerHairDark;
                }

                if (leftEyeZone || rightEyeZone)
                {
                    return ExplorerEyes;
                }

                if (noseZone || mouthZone)
                {
                    return ExplorerSkinShadow;
                }

                if (y > 0.87f)
                {
                    return ExplorerHairDark;
                }

                if (y > 0.74f && (z < 0.58f || centerDistance > 0.18f))
                {
                    return ExplorerHairLight;
                }

                if (y > 0.70f && z < 0.48f)
                {
                    return ExplorerHairDark;
                }

                return y > 0.66f ? ExplorerSkinShadow : ExplorerSkin;
            }

            if (bone.Contains("leg") || bone.Contains("hip") || bone.Contains("upleg"))
            {
                if (y < 0.18f)
                {
                    return ExplorerSandals;
                }

                return y > 0.38f ? ExplorerPantsLight : ExplorerPantsDark;
            }

            if (bone.Contains("arm") || bone.Contains("shoulder") || bone.Contains("forearm"))
            {
                if (y > 0.58f && z > 0.53f && centerDistance < 0.20f)
                {
                    return ExplorerJacketLight;
                }

                if (y < 0.34f)
                {
                    return ExplorerSkin;
                }

                return ExplorerJacketDark;
            }

            if (bone.Contains("spine"))
            {
                if (y > 0.58f && z > 0.53f && centerDistance < 0.14f)
                {
                    return ExplorerShirt;
                }

                if (y > 0.48f)
                {
                    return ExplorerJacketDark;
                }

                return ExplorerPantsDark;
            }

            if (y > 0.84f)
            {
                return ExplorerHairDark;
            }

            if (y > 0.52f)
            {
                return ExplorerJacketDark;
            }

            if (y > 0.20f)
            {
                return ExplorerPantsDark;
            }

            return ExplorerSandals;
        }

        private static Transform FindChildRecursive(Transform root, string targetName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == targetName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), targetName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Material CreateUnlitColorMaterial(string fileName, Color color)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (material == null)
            {
                if (shader == null)
                {
                    return null;
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateFaceQuad(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, float zRotation)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = localPosition;
            quad.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
            quad.transform.localScale = localScale;

            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static string GetPrimaryBoneName(BoneWeight weight, Transform[] bones)
        {
            int index = weight.boneIndex0;
            float maxWeight = weight.weight0;

            if (weight.weight1 > maxWeight)
            {
                index = weight.boneIndex1;
                maxWeight = weight.weight1;
            }

            if (weight.weight2 > maxWeight)
            {
                index = weight.boneIndex2;
                maxWeight = weight.weight2;
            }

            if (weight.weight3 > maxWeight)
            {
                index = weight.boneIndex3;
            }

            if (bones == null || index < 0 || index >= bones.Length || bones[index] == null)
            {
                return string.Empty;
            }

            return bones[index].name;
        }

        private static Color32 ToLinearColor32(Color32 color)
        {
            Color linear = new Color(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f).linear;
            return (Color32)linear;
        }



        private static AnimatorController EnsureExplorerAnimatorController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ExplorerControllerPath);
            if (controller != null)
            {
                AssetDatabase.DeleteAsset(ExplorerControllerPath);
                controller = null;
            }

            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(ExplorerIdleClipPath) != null)
            {
                AssetDatabase.DeleteAsset(ExplorerIdleClipPath);
            }

            AnimationClip idleClip = new AnimationClip
            {
                name = "MeshyExplorerIdle"
            };
            AssetDatabase.CreateAsset(idleClip, ExplorerIdleClipPath);

            AnimationClip walkClip = LoadExplorerClip(ExplorerWalkPath, "walking");
            AnimationClip runClip = LoadExplorerClip(ExplorerRunPath, "running");
            if (walkClip == null)
            {
                walkClip = LoadExplorerClip(ExplorerAltRunPath, "run");
            }

            if (runClip == null)
            {
                runClip = LoadExplorerClip(ExplorerAltRunPath, "run");
            }

            if (walkClip == null)
            {
                walkClip = idleClip;
            }

            if (runClip == null)
            {
                runClip = walkClip;
            }

            controller = AnimatorController.CreateAnimatorControllerAtPath(ExplorerControllerPath);
            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine stateMachine = layer.stateMachine;

            AnimatorState idleState = stateMachine.AddState("Idle");
            AnimatorState walkState = stateMachine.AddState("Walk");
            AnimatorState runState = stateMachine.AddState("Run");

            idleState.motion = idleClip;
            walkState.motion = walkClip;
            runState.motion = runClip;

            stateMachine.defaultState = idleState;

            AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.08f;
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.05f, "MoveAmount");
            idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsRunning");

            AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.08f;
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.05f, "MoveAmount");
            idleToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsRunning");

            AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.08f;
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "MoveAmount");

            AnimatorStateTransition walkToRun = walkState.AddTransition(runState);
            walkToRun.hasExitTime = false;
            walkToRun.duration = 0.08f;
            walkToRun.AddCondition(AnimatorConditionMode.Greater, 0.05f, "MoveAmount");
            walkToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsRunning");

            AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.08f;
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "MoveAmount");

            AnimatorStateTransition runToWalk = runState.AddTransition(walkState);
            runToWalk.hasExitTime = false;
            runToWalk.duration = 0.08f;
            runToWalk.AddCondition(AnimatorConditionMode.Greater, 0.05f, "MoveAmount");
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsRunning");

            if (controller.parameters == null || controller.parameters.Length == 0)
            {
                controller.AddParameter("MoveAmount", AnimatorControllerParameterType.Float);
                controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimationClip LoadExplorerClip(string assetPath, string clipHint)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assets == null)
            {
                return null;
            }

            string hint = clipHint != null ? clipHint.ToLowerInvariant() : string.Empty;
            for (int i = 0; i < assets.Length; i++)
            {
                AnimationClip clip = assets[i] as AnimationClip;
                if (clip == null)
                {
                    continue;
                }

                string clipName = clip.name != null ? clip.name.ToLowerInvariant() : string.Empty;
                if (string.IsNullOrEmpty(hint) || clipName.Contains(hint))
                {
                    return clip;
                }
            }

            for (int i = 0; i < assets.Length; i++)
            {
                AnimationClip clip = assets[i] as AnimationClip;
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }


        private static TopDownCameraFollow CreateCamera(Transform target)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = target.position + Vector3.up * 1.62f;
            cameraObject.transform.rotation = Quaternion.identity;
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 72f;
            cameraObject.AddComponent<AudioListener>();

            FirstPersonCameraController firstPerson = cameraObject.AddComponent<FirstPersonCameraController>();
            firstPerson.Bind(target);
            firstPerson.EnsurePrimary();

            // Legacy follow kept disabled for tooling compatibility.
            TopDownCameraFollow follow = cameraObject.AddComponent<TopDownCameraFollow>();
            follow.enabled = false;
            follow.target = target;
            return follow;
        }

        private static GameObject CreateStylizedHome(Transform parent, Vector3 position, string name, Dictionary<string, Material> materials)
        {
            GameObject houseRoot = new GameObject(name);
            houseRoot.transform.SetParent(parent, false);
            houseRoot.transform.position = position;
            houseRoot.transform.rotation = Quaternion.Euler(0f, 46f, 0f);

            CreateCubePart("Foundation", houseRoot.transform, new Vector3(0f, 0.55f, 0f), new Vector3(8.6f, 1.1f, 7.4f), materials["foundation"]);
            CreateCubePart("Walls_Main", houseRoot.transform, new Vector3(0f, 2.15f, 0f), new Vector3(8f, 3.2f, 6.8f), materials["wall_light"]);
            CreateCubePart("Walls_Upper", houseRoot.transform, new Vector3(0f, 4.65f, -0.2f), new Vector3(6.9f, 1.8f, 5.9f), materials["wall_shadow"]);

            CreateCubePart("Roof_Left", houseRoot.transform, new Vector3(-1.6f, 5.65f, 0f), new Vector3(5.2f, 0.75f, 8f), materials["roof_light"], Quaternion.Euler(0f, 0f, 34f));
            CreateCubePart("Roof_Right", houseRoot.transform, new Vector3(1.6f, 5.65f, 0f), new Vector3(5.2f, 0.75f, 8f), materials["roof_dark"], Quaternion.Euler(0f, 0f, -34f));
            CreateCubePart("Chimney", houseRoot.transform, new Vector3(2.2f, 6.3f, -1.5f), new Vector3(1f, 2f, 1f), materials["roof_dark"]);

            CreateCubePart("Door", houseRoot.transform, new Vector3(0f, 1.55f, 3.1f), new Vector3(1.4f, 2.4f, 0.35f), materials["door_dark"]);
            CreateCubePart("DoorFrame", houseRoot.transform, new Vector3(0f, 2.35f, 2.96f), new Vector3(2.0f, 0.25f, 0.2f), materials["wood_dark"]);
            CreateCubePart("Frame_Left", houseRoot.transform, new Vector3(-0.92f, 1.55f, 2.96f), new Vector3(0.2f, 2.4f, 0.2f), materials["wood_dark"]);
            CreateCubePart("Frame_Right", houseRoot.transform, new Vector3(0.92f, 1.55f, 2.96f), new Vector3(0.2f, 2.4f, 0.2f), materials["wood_dark"]);

            CreateWindow(houseRoot.transform, "Window_Left", new Vector3(-2.3f, 2.5f, 3.02f), materials);
            CreateWindow(houseRoot.transform, "Window_Right", new Vector3(2.3f, 2.5f, 3.02f), materials);

            CreateCubePart("BalconyFloor", houseRoot.transform, new Vector3(0f, 3.75f, 3.7f), new Vector3(4.8f, 0.25f, 1.7f), materials["wood"]);
            CreateCubePart("BalconyRailFront", houseRoot.transform, new Vector3(0f, 4.35f, 4.42f), new Vector3(4.8f, 0.25f, 0.18f), materials["wood"]);
            CreateCubePart("BalconyRailLeft", houseRoot.transform, new Vector3(-2.25f, 4.35f, 3.72f), new Vector3(0.18f, 0.9f, 1.5f), materials["wood"]);
            CreateCubePart("BalconyRailRight", houseRoot.transform, new Vector3(2.25f, 4.35f, 3.72f), new Vector3(0.18f, 0.9f, 1.5f), materials["wood"]);
            CreateCubePart("PorchPostLeft", houseRoot.transform, new Vector3(-1.8f, 1.6f, 3.65f), new Vector3(0.28f, 3.2f, 0.28f), materials["wood_dark"]);
            CreateCubePart("PorchPostRight", houseRoot.transform, new Vector3(1.8f, 1.6f, 3.65f), new Vector3(0.28f, 3.2f, 0.28f), materials["wood_dark"]);

            return houseRoot;
        }

        private static void CreateWindow(Transform parent, string name, Vector3 position, Dictionary<string, Material> materials)
        {
            GameObject frame = CreateCubePart(name + "_Frame", parent, position, new Vector3(1.8f, 1.6f, 0.18f), materials["wood_dark"]);
            CreateCubePart(name + "_Glass", frame.transform, new Vector3(0f, 0f, -0.02f), new Vector3(1.42f, 1.22f, 0.05f), materials["glass"]);
            CreateCubePart(name + "_MullionV", frame.transform, new Vector3(0f, 0f, -0.03f), new Vector3(0.14f, 1.22f, 0.06f), materials["wood_dark"]);
            CreateCubePart(name + "_MullionH", frame.transform, new Vector3(0f, 0f, -0.03f), new Vector3(1.42f, 0.14f, 0.06f), materials["wood_dark"]);
        }

        private static GameObject CreateCubePart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            return CreateCubePart(name, parent, localPosition, localScale, material, Quaternion.identity);
        }

        private static GameObject CreateCubePart(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Quaternion localRotation)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return part;
        }

        private static VendorStall CreateVendorStall(
            Transform parent,
            string name,
            string visualPrefabPath,
            Vector3 visualPosition,
            Quaternion visualRotation,
            Vector3 visualScale,
            VendorProfile profile,
            Vector3 triggerPosition,
            Vector3 triggerSize,
            string visualTheme,
            Dictionary<string, Material> materials)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);

            GameObject visual = InstantiatePrefab(visualPrefabPath, root.transform, visualPosition, visualRotation, visualScale, name + "_Visual");
            TintMarketObject(visual, visualTheme, materials);

            GameObject triggerObject = new GameObject(name + "_Trigger");
            triggerObject.transform.SetParent(root.transform, false);
            triggerObject.transform.position = triggerPosition;
            BoxCollider box = triggerObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = triggerSize;

            VendorStall stall = triggerObject.AddComponent<VendorStall>();
            stall.vendorProfile = profile;
            stall.availablePrompt = "Nhấn E để xem " + profile.stockedItem.displayName.ToLower();
            stall.soldPrompt = "Đã mua: " + profile.stockedItem.displayName.ToLower();
            stall.SetPrompt(stall.availablePrompt);
            return stall;
        }

        private static void CreateVendorNpc(
            Transform parent,
            VendorStall stall,
            string prefabPath,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string name)
        {
            if (stall == null)
            {
                return;
            }

            GameObject npc = InstantiatePrefab(
                prefabPath,
                parent,
                position,
                rotation,
                scale,
                name);

            CoHuongBargainMinigame minigame = stall.gameObject.AddComponent<CoHuongBargainMinigame>();
            minigame.reactionRoot = npc != null ? npc.transform : stall.transform;
            minigame.arrowSpeed = 0.62f;
            minigame.cooldownSecondsMin = 30f;
            minigame.cooldownSecondsMax = 60f;
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

        private static void TintMarketObject(GameObject root, string theme, Dictionary<string, Material> materials)
        {
            if (root == null || materials == null)
            {
                return;
            }

            if (theme == "flower")
            {
                ApplyMaterialRecursive(root, materials["flower"]);
                return;
            }

            if (theme == "gift")
            {
                ApplyMaterialRecursive(root, materials["gift"]);
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                string lower = renderers[i].gameObject.name.ToLowerInvariant();
                Material targetMaterial = materials["wood"];

                if (theme == "bakery")
                {
                    if (lower.Contains("cookie") || lower.Contains("cake") || lower.Contains("pastry") || lower.Contains("pie") || lower.Contains("bread") || lower.Contains("pretzel"))
                    {
                        targetMaterial = materials["produce_yellow"];
                    }
                    else if (lower.Contains("egg"))
                    {
                        targetMaterial = materials["wall_light"];
                    }
                    else if (lower.Contains("jar") || lower.Contains("board") || lower.Contains("bakerymarket"))
                    {
                        targetMaterial = materials["market_red"];
                    }
                }
                else if (theme == "produce")
                {
                    if (lower.Contains("watermelon") || lower.Contains("cucumber") || lower.Contains("apple_green"))
                    {
                        targetMaterial = materials["market_green"];
                    }
                    else if (lower.Contains("tomato") || lower.Contains("apple_red") || lower.Contains("beet"))
                    {
                        targetMaterial = materials["produce_red"];
                    }
                    else if (lower.Contains("banana") || lower.Contains("onion") || lower.Contains("potato") || lower.Contains("apple_yellow"))
                    {
                        targetMaterial = materials["produce_yellow"];
                    }
                    else if (lower.Contains("carrot"))
                    {
                        targetMaterial = materials["produce_orange"];
                    }
                    else if (lower.Contains("eggplant"))
                    {
                        targetMaterial = materials["produce_purple"];
                    }
                    else if (lower.Contains("coin"))
                    {
                        targetMaterial = materials["coin"];
                    }
                }
                else if (theme == "meat")
                {
                    if (lower.Contains("meat") || lower.Contains("sausage"))
                    {
                        targetMaterial = materials["meat"];
                    }
                    else if (lower.Contains("market") || lower.Contains("table") || lower.Contains("hanger"))
                    {
                        targetMaterial = materials["market_red"];
                    }
                }
                else if (theme == "gift")
                {
                    if (lower.Contains("coin"))
                    {
                        targetMaterial = materials["coin"];
                    }
                    else if (lower.Contains("lantern"))
                    {
                        targetMaterial = materials["lantern"];
                    }
                    else if (lower.Contains("wood"))
                    {
                        targetMaterial = materials["wood"];
                    }
                    else
                    {
                        targetMaterial = materials["gift"];
                    }
                }

                Material[] mats = renderers[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    mats[j] = targetMaterial;
                }

                renderers[i].sharedMaterials = mats;
            }
        }

        private static void CreateZoneTrigger(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 size,
            string zoneId,
            string eventId,
            string eventTitle,
            string eventBody,
            int timePenalty,
            bool triggerOnce,
            bool applyRain,
            string vendorBonusId,
            int familiarityBonus)
        {
            GameObject triggerObject = new GameObject(name);
            triggerObject.transform.SetParent(parent, false);
            triggerObject.transform.position = position;
            BoxCollider box = triggerObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;

            ZoneTrigger trigger = triggerObject.AddComponent<ZoneTrigger>();
            trigger.zoneId = zoneId;
            trigger.eventId = eventId;
            trigger.eventTitle = eventTitle;
            trigger.eventBody = eventBody;
            trigger.timePenaltyMinutes = timePenalty;
            trigger.triggerOnce = triggerOnce;
            trigger.applyRain = applyRain;
            trigger.vendorBonusId = vendorBonusId;
            trigger.familiarityBonus = familiarityBonus;
        }

        private static GameObject CreateGround(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = name;
            ground.transform.SetParent(parent, false);
            ground.transform.position = position;
            ground.transform.localScale = scale;
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return ground;
        }

        private static void CreateTerrainBorders(Transform parent, Dictionary<string, Material> materials)
        {
            CreateGround("TerrainLipNorth", parent, new Vector3(0f, -0.22f, 102f), new Vector3(58f, 0.34f, 6f), materials["terrain_edge"]);
            CreateGround("TerrainLipSouth", parent, new Vector3(0f, -0.22f, -8f), new Vector3(58f, 0.34f, 8f), materials["terrain_edge"]);
            CreateGround("TerrainLipWest", parent, new Vector3(-28f, -0.22f, 46f), new Vector3(6f, 0.34f, 112f), materials["terrain_edge"]);
            CreateGround("TerrainLipEast", parent, new Vector3(28f, -0.22f, 46f), new Vector3(6f, 0.34f, 112f), materials["terrain_edge"]);

            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/cliff1.prefab", parent, new Vector3(-24.5f, 0.25f, 20f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 2.2f, "CliffWest_1");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/cliff4.prefab", parent, new Vector3(-24.5f, 0.25f, 56f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 2.35f, "CliffWest_2");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/cliff2.prefab", parent, new Vector3(24.5f, 0.25f, 28f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 2.15f, "CliffEast_1");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/cliff3.prefab", parent, new Vector3(24.5f, 0.25f, 74f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 2.3f, "CliffEast_2");
            InstantiatePrefab(UltimateNatureFbxRoot + "/Rock_1.fbx", parent, new Vector3(-23.5f, 0.1f, 6f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 1.8f, "BorderRock_1");
            InstantiatePrefab(UltimateNatureFbxRoot + "/Rock_4.fbx", parent, new Vector3(22.8f, 0.1f, 90f), Quaternion.Euler(0f, -12f, 0f), Vector3.one * 1.55f, "BorderRock_2");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/bush3.prefab", parent, new Vector3(-20f, 0f, 41f), Quaternion.identity, Vector3.one * 1.65f, "BorderBush_1");
            InstantiatePrefab(LowPolyNaturePrefabsRoot + "/bush2.prefab", parent, new Vector3(19.6f, 0f, 60f), Quaternion.identity, Vector3.one * 1.45f, "BorderBush_2");
            SpawnScatter(parent, LowPolyNaturePrefabsRoot + "/tree3_1_prefab.prefab", "BorderTree", new[]
            {
                new Vector3(-21f,0f,12f), new Vector3(20f,0f,20f), new Vector3(-20f,0f,34f), new Vector3(20f,0f,44f),
                new Vector3(-19f,0f,58f), new Vector3(19f,0f,72f), new Vector3(-21f,0f,86f), new Vector3(19f,0f,96f)
            }, 1.28f);
            SpawnScatter(parent, LowPolyNaturePrefabsRoot + "/bush1.prefab", "BorderBushScatter", new[]
            {
                new Vector3(-17f,0f,8f), new Vector3(-15f,0f,14f), new Vector3(16f,0f,18f), new Vector3(14f,0f,25f),
                new Vector3(-16f,0f,30f), new Vector3(17f,0f,38f), new Vector3(-15f,0f,46f), new Vector3(16f,0f,53f),
                new Vector3(-16f,0f,66f), new Vector3(16f,0f,70f), new Vector3(-15f,0f,82f), new Vector3(15f,0f,88f),
                new Vector3(-16f,0f,98f), new Vector3(15f,0f,102f)
            }, 1.18f);
            SpawnScatter(parent, LowPolyNaturePrefabsRoot + "/grass3.prefab", "BorderGrassScatter", new[]
            {
                new Vector3(-18f,0f,6f), new Vector3(-12f,0f,8f), new Vector3(12f,0f,10f), new Vector3(18f,0f,12f),
                new Vector3(-18f,0f,18f), new Vector3(-12f,0f,22f), new Vector3(12f,0f,24f), new Vector3(18f,0f,28f),
                new Vector3(-18f,0f,34f), new Vector3(-12f,0f,38f), new Vector3(12f,0f,40f), new Vector3(18f,0f,44f),
                new Vector3(-18f,0f,50f), new Vector3(-12f,0f,54f), new Vector3(12f,0f,56f), new Vector3(18f,0f,60f),
                new Vector3(-18f,0f,66f), new Vector3(-12f,0f,70f), new Vector3(12f,0f,72f), new Vector3(18f,0f,76f),
                new Vector3(-18f,0f,82f), new Vector3(-12f,0f,86f), new Vector3(12f,0f,88f), new Vector3(18f,0f,92f),
                new Vector3(-18f,0f,98f), new Vector3(-12f,0f,100f), new Vector3(12f,0f,102f), new Vector3(18f,0f,104f)
            }, 1.12f);
        }

        private static void SpawnScatter(Transform parent, string prefabPath, string prefix, Vector3[] positions, float scale)
        {
            if (positions == null)
            {
                return;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                float scaleFactor = scale * (1f + (((i % 3) - 1) * 0.08f));
                InstantiatePrefab(
                    prefabPath,
                    parent,
                    positions[i],
                    Quaternion.Euler(0f, (i * 41f) % 360f, 0f),
                    Vector3.one * scaleFactor,
                    prefix + "_" + i
                );
            }
        }

        private static Transform CreateMarker(string name, Vector3 position, Transform parent)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.position = position;
            return marker.transform;
        }

        private static GameObject InstantiatePrefab(string path, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance;
            if (prefab != null)
            {
                instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            else
            {
                instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            if (instance == null)
            {
                instance = new GameObject(name);
            }

            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;
            return instance;
        }

        private static Material CreateMaterial(string fileName, Color color)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateMaterialFromSource(string fileName, string sourcePath, Color tint, Vector2 tiling, float smoothness)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Material source = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
                if (source != null)
                {
                    material = new Material(source);
                }
                else
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    material = new Material(shader);
                }

                AssetDatabase.CreateAsset(material, path);
            }

            ApplyMaterialLook(material, tint, tiling, smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateTexturedMaterial(string fileName, string texturePath, Color tint, Vector2 tiling, float smoothness)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            ApplyMaterialLook(material, tint, tiling, smoothness);
            Texture baseTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
            if (baseTexture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", baseTexture);
                    material.SetTextureScale("_BaseMap", tiling);
                }

                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", baseTexture);
                    material.SetTextureScale("_MainTex", tiling);
                }
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateSkyboxMaterial(string fileName, string texturePath)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Skybox/Panoramic");
                if (shader == null)
                {
                    return null;
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            Texture skyTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
            if (skyTexture != null)
            {
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", skyTexture);
                }

                if (material.HasProperty("_Rotation"))
                {
                    material.SetFloat("_Rotation", 212f);
                }

                if (material.HasProperty("_Exposure"))
                {
                    material.SetFloat("_Exposure", 0.78f);
                }

                if (material.HasProperty("_Tint"))
                {
                    material.SetColor("_Tint", new Color(1f, 0.96f, 0.92f, 1f));
                }
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyMaterialLook(Material material, Color tint, Vector2 tiling, float smoothness)
        {
            if (material == null)
            {
                return;
            }

            material.color = tint;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTextureScale("_BaseMap", tiling);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTextureScale("_MainTex", tiling);
            }
        }

        private static Color Hex(string hex)
        {
            Color color;
            if (!ColorUtility.TryParseHtmlString(hex, out color))
            {
                return Color.white;
            }

            return color;
        }

        private static T CreateOrUpdateAsset<T>(string path, System.Action<T> configure) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            configure(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slashIndex = path.LastIndexOf('/');
            string parent = path.Substring(0, slashIndex);
            string name = path.Substring(slashIndex + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static EditorBuildSettingsScene[] BuildEditorScenes(string[] scenePaths)
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }

            return scenes;
        }
    }
}
