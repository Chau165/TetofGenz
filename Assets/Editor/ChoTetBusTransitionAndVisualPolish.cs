#if UNITY_EDITOR
using System.IO;
using ChoTet.Level1;
using ChoTet.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ChoTetBusTransitionAndVisualPolish
{
    private const string FlagPath = "Assets/Editor/RunChoTetBusTransitionAndVisualPolish.flag";
    private const string Level1ScenePath = "Assets/Scenes/Level1_PackingRoom.unity";
    private const string ChoTetScenePath = "Assets/Scenes/ChoTet_VerticalSlice.unity";
    private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials";
    private const string PolishRootName = "TetStylePolish";

    static ChoTetBusTransitionAndVisualPolish()
    {
        EditorApplication.delayCall += RunIfFlagged;
    }

    public static void ApplyMenu()
    {
        ApplyAll();
    }

    private static void RunIfFlagged()
    {
        if (!File.Exists(FlagPath))
        {
            return;
        }

        ApplyAll();
        AssetDatabase.DeleteAsset(FlagPath);
        AssetDatabase.Refresh();
    }

    private static void ApplyAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[ChoTetPolish] Exit Play mode before applying scene polish.");
            return;
        }

        EnsureFolder("Assets/Settings");
        EnsureFolder("Assets/Settings/ChoTet");
        EnsureFolder(MaterialsRoot);

        EditorSceneManager.SaveOpenScenes();
        ApplyBuildSettings();

        EditorSceneManager.OpenScene(Level1ScenePath, OpenSceneMode.Single);
        EnsureLevel1BusTransition();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        EditorSceneManager.OpenScene(ChoTetScenePath, OpenSceneMode.Single);
        EnsureChoTetArrivalAndPolish();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ChoTetPolish] Bus transition, arrival spawn, Build Settings, and ChoTet visual polish applied.");
    }

    private static void ApplyBuildSettings()
    {
        ChoTet.Menu.Editor.ChoTetBuildSettings.ApplyMenuFlowScenesWithHighway(includeHighway: true);
    }

    private static void EnsureLevel1BusTransition()
    {
        BusPickupInteraction[] interactions = Object.FindObjectsByType<BusPickupInteraction>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (interactions.Length == 0)
        {
            Transform parent = GameObject.Find("Level1_BusPickupArea")?.transform
                ?? GameObject.Find("Level1_OutdoorArea")?.transform;
            GameObject trigger = new GameObject("BusPickupInteractPoint");
            if (parent != null)
            {
                trigger.transform.SetParent(parent, false);
            }

            trigger.transform.position = new Vector3(-2.85f, 0.7f, -31.7f);
            SphereCollider sphere = trigger.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 2.2f;
            interactions = new[] { trigger.AddComponent<BusPickupInteraction>() };
        }

        for (int i = 0; i < interactions.Length; i++)
        {
            BusPickupInteraction interaction = interactions[i];
            interaction.SetPrompt("Press E to go back home");
            SerializedObject serialized = new SerializedObject(interaction);
            SerializedProperty targetScene = serialized.FindProperty("targetSceneName");
            if (targetScene != null)
            {
                targetScene.stringValue = "ChoTet_VerticalSlice";
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            Collider collider = interaction.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            SphereCollider sphere = interaction.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                sphere.radius = Mathf.Max(sphere.radius, 2.2f);
            }
        }
    }

    private static void EnsureChoTetArrivalAndPolish()
    {
        MaterialSet materials = CreateMaterialSet();
        ApplyChoTetRenderSettings();
        TuneChoTetCameraAndLight();
        Transform world = GameObject.Find("ChoTetWorld")?.transform;
        if (world == null)
        {
            world = new GameObject("ChoTetWorld").transform;
        }

        Transform arrival = EnsureArrivalSpawn(world);
        WireGameFlowForArrival(arrival);
        RebuildPolishRoot(world, materials);
    }

    private static void ApplyChoTetRenderSettings()
    {
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.92f;
        RenderSettings.reflectionIntensity = 0.72f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.84f, 0.70f, 0.55f, 1f);
        RenderSettings.fogStartDistance = 42f;
        RenderSettings.fogEndDistance = 96f;
        DynamicGI.UpdateEnvironment();
    }

    private static void TuneChoTetCameraAndLight()
    {
        Camera camera = Camera.main ?? Object.FindAnyObjectByType<Camera>();
        PlayerTopDownController player = Object.FindAnyObjectByType<PlayerTopDownController>();
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 54f;
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 105f;
            TopDownCameraFollow follow = camera.GetComponent<TopDownCameraFollow>();
            if (follow != null)
            {
                follow.offset = new Vector3(0f, 12.4f, -8.4f);
                follow.lookOffset = new Vector3(0f, 1.18f, 0f);
                follow.smoothSpeed = 6.5f;
                if (player != null)
                {
                    follow.target = player.transform;
                    camera.transform.position = player.transform.position + follow.offset;
                    camera.transform.LookAt(player.transform.position + follow.lookOffset);
                }
            }
        }

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i].type != LightType.Directional)
            {
                continue;
            }

            lights[i].transform.rotation = Quaternion.Euler(30f, -42f, 0f);
            lights[i].intensity = 1.08f;
            lights[i].color = new Color(1f, 0.82f, 0.64f, 1f);
            lights[i].shadows = LightShadows.Soft;
            lights[i].shadowStrength = 0.68f;
        }
    }

    private static Transform EnsureArrivalSpawn(Transform world)
    {
        Transform arrival = GameObject.Find("CountrysideArrivalSpawn")?.transform;
        if (arrival == null)
        {
            GameObject marker = new GameObject("CountrysideArrivalSpawn");
            marker.transform.SetParent(world, false);
            arrival = marker.transform;
        }

        arrival.position = new Vector3(0f, 0.15f, 4.5f);
        arrival.rotation = Quaternion.Euler(0f, 0f, 0f);

        PlayerTopDownController player = Object.FindAnyObjectByType<PlayerTopDownController>();
        if (player != null)
        {
            player.transform.SetPositionAndRotation(arrival.position, arrival.rotation);
        }

        return arrival;
    }

    private static void WireGameFlowForArrival(Transform arrival)
    {
        GameFlowController flow = Object.FindAnyObjectByType<GameFlowController>();
        if (flow == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(flow);
        SerializedProperty spawn = serialized.FindProperty("homeSpawnPoint");
        if (spawn != null)
        {
            spawn.objectReferenceValue = arrival;
        }

        SerializedProperty showMission = serialized.FindProperty("showMissionOnStart");
        if (showMission != null)
        {
            showMission.boolValue = false;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();

        if (flow.dayState != null)
        {
            flow.dayState.Reset(flow.startMinutes, flow.closeMinutes, flow.startingBudget, flow.capacityLimit);
        }
    }

    private static void RebuildPolishRoot(Transform world, MaterialSet materials)
    {
        Transform existing = world.Find(PolishRootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject rootObject = new GameObject(PolishRootName);
        rootObject.transform.SetParent(world, false);
        Transform root = rootObject.transform;

        BuildHomeTetLayer(root, materials);
        BuildGardenTetLayer(root, materials);
        BuildRoadTetLayer(root, materials);
        BuildMarketTetLayer(root, materials);
        BuildInactiveTrafficLayer(root, materials);
    }

    private static void BuildHomeTetLayer(Transform root, MaterialSet materials)
    {
        Transform zone = CreateChild(root, "HomeArea_Polish");
        SpawnPrefab("Assets/Low-Poly Medieval Market/Prefabs/wooden_box.prefab", zone, new Vector3(4.6f, 0f, 12.2f),
            Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.25f, "HomeGiftCrate", true);
        CreateGiftBasket(zone, new Vector3(5.6f, 0.35f, 12.8f), materials, "HomeGiftBasket");
        CreateBanhChungStack(zone, new Vector3(-4.1f, 0.25f, 12.6f), materials, "HomeBanhChungStack", 3);
        CreateWatermelonPile(zone, new Vector3(-6.4f, 0.38f, 11.2f), materials, "HomeWatermelons", 3);
        CreateCouplet(zone, new Vector3(-2.35f, 2.05f, 13.45f), Quaternion.Euler(0f, 0f, 0f), materials, "HomeCoupletLeft");
        CreateCouplet(zone, new Vector3(2.35f, 2.05f, 13.45f), Quaternion.Euler(0f, 0f, 0f), materials, "HomeCoupletRight");
        CreateLanternPair(zone, new Vector3(-5.5f, 2.35f, 12.9f), new Vector3(5.5f, 2.35f, 12.9f), materials, "HomeLanterns");
        ScatterPrefab("Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/grass2.prefab", zone, "HomeExtraGrass",
            new[] { new Vector3(-9f, 0f, 6.5f), new Vector3(-6.5f, 0f, 7f), new Vector3(7.5f, 0f, 7.2f), new Vector3(9.5f, 0f, 10.5f), new Vector3(-11f, 0f, 12.4f) },
            1.05f);
    }

    private static void BuildGardenTetLayer(Transform root, MaterialSet materials)
    {
        Transform zone = CreateChild(root, "GardenArea_Polish");
        CreateHoaMaiTree(zone, new Vector3(-9.5f, 0f, 24.5f), 1.25f, materials, "HoaMaiTree_GardenLeft");
        CreateHoaMaiTree(zone, new Vector3(9.5f, 0f, 29.5f), 1.05f, materials, "HoaMaiTree_GardenRight");
        CreateLanternString(zone, new Vector3(-7f, 3.1f, 24f), new Vector3(7f, 3.1f, 28f), 7, materials, "GardenLanternString");
        ScatterPrefab("Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/bush2.prefab", zone, "GardenExtraBush",
            new[] { new Vector3(-14f, 0f, 22f), new Vector3(14f, 0f, 23f), new Vector3(-15f, 0f, 32f), new Vector3(14.5f, 0f, 36f), new Vector3(0f, 0f, 36.5f) },
            1.18f);
        ScatterPrefab("Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/grass4.prefab", zone, "GardenExtraGrass",
            new[] { new Vector3(-10f, 0f, 18f), new Vector3(-6f, 0f, 21f), new Vector3(-2f, 0f, 22f), new Vector3(4f, 0f, 22.5f), new Vector3(10f, 0f, 23f), new Vector3(-8f, 0f, 34f), new Vector3(6f, 0f, 35f) },
            1.12f);
    }

    private static void BuildRoadTetLayer(Transform root, MaterialSet materials)
    {
        Transform zone = CreateChild(root, "VillageRoad_Polish");
        for (int i = 0; i < 5; i++)
        {
            float z = 34f + (i * 9.5f);
            SpawnPrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Fence.prefab", zone, new Vector3(-10.8f, 0f, z),
                Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.05f, "RoadFenceExtra_L" + i, true);
            SpawnPrefab("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Fence.prefab", zone, new Vector3(10.8f, 0f, z + 3f),
                Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.05f, "RoadFenceExtra_R" + i, true);
        }

        CreateMarketSign(zone, new Vector3(-7.5f, 0.85f, 61f), Quaternion.Euler(0f, 24f, 0f), "CHO TET", materials, "RoadTetSign");
        CreateLanternString(zone, new Vector3(-9f, 2.7f, 53f), new Vector3(9f, 2.7f, 58f), 6, materials, "RoadLanternString");
        ScatterPrefab("Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/bush1.prefab", zone, "RoadExtraBush",
            new[] { new Vector3(-13f, 0f, 34f), new Vector3(13f, 0f, 38f), new Vector3(-13f, 0f, 52f), new Vector3(13f, 0f, 57f), new Vector3(-13f, 0f, 70f), new Vector3(13f, 0f, 76f) },
            1.14f);
        SpawnPrefab("Assets/Honda Motor Co., Ltd/Super Cub C125/Prefabs/C125 - PB.prefab", zone, new Vector3(-6.6f, 0.02f, 68f),
            Quaternion.Euler(0f, 58f, 0f), Vector3.one * 0.92f, "RoadParkedSuperCub", true);
    }

    private static void BuildMarketTetLayer(Transform root, MaterialSet materials)
    {
        Transform zone = CreateChild(root, "TetMarket_Polish");
        CreateLanternString(zone, new Vector3(-17f, 3.4f, 80f), new Vector3(17f, 3.4f, 80f), 9, materials, "MarketEntryLanterns");
        CreateLanternString(zone, new Vector3(-16f, 3.25f, 94f), new Vector3(16f, 3.25f, 94f), 8, materials, "MarketRearLanterns");

        CreateMarketSign(zone, new Vector3(-12f, 1.45f, 79.3f), Quaternion.Euler(0f, 180f, 0f), "BANH MUT", materials, "BakeryTetSign");
        CreateMarketSign(zone, new Vector3(0f, 1.45f, 79.3f), Quaternion.Euler(0f, 180f, 0f), "DUA HAU", materials, "WatermelonTetSign");
        CreateMarketSign(zone, new Vector3(12f, 1.45f, 79.3f), Quaternion.Euler(0f, 180f, 0f), "HOA MAI", materials, "FlowerTetSign");
        CreateMarketSign(zone, new Vector3(9.5f, 1.45f, 91f), Quaternion.Euler(0f, 180f, 0f), "QUA TET", materials, "GiftTetSign");

        CreateWatermelonPile(zone, new Vector3(2.8f, 0.38f, 82.6f), materials, "MarketWatermelonPileA", 5);
        CreateWatermelonPile(zone, new Vector3(-1.8f, 0.38f, 82.8f), materials, "MarketWatermelonPileB", 4);
        CreateBanhChungStack(zone, new Vector3(-14.5f, 0.26f, 83.2f), materials, "MarketBanhChungStackA", 4);
        CreateBanhChungStack(zone, new Vector3(-10.2f, 0.26f, 83.1f), materials, "MarketBanhChungStackB", 3);
        CreateGiftBasket(zone, new Vector3(7.4f, 0.36f, 94.4f), materials, "MarketGiftBasketA");
        CreateGiftBasket(zone, new Vector3(11.7f, 0.36f, 94.1f), materials, "MarketGiftBasketB");
        CreateHoaMaiTree(zone, new Vector3(14.8f, 0f, 84.6f), 0.75f, materials, "MarketHoaMaiPotA");
        CreateHoaMaiTree(zone, new Vector3(9.2f, 0f, 84.2f), 0.68f, materials, "MarketHoaMaiPotB");

        ScatterPrefab("Assets/Low-Poly Medieval Market/Prefabs/wooden_box.prefab", zone, "MarketExtraCrate",
            new[] { new Vector3(-18.5f, 0f, 86f), new Vector3(-17.8f, 0f, 96.8f), new Vector3(17f, 0f, 89.8f), new Vector3(16.8f, 0f, 98.5f) },
            1.25f);
        ScatterPrefab("Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs/grass4.prefab", zone, "MarketExtraGrass",
            new[] { new Vector3(-20f, 0f, 78.5f), new Vector3(-21f, 0f, 92f), new Vector3(20f, 0f, 82f), new Vector3(21f, 0f, 100f), new Vector3(0f, 0f, 104f) },
            1.1f);
    }

    private static void BuildInactiveTrafficLayer(Transform root, MaterialSet materials)
    {
        Transform zone = CreateChild(root, "TrafficObstacles_EnabledAfterShopping");
        zone.gameObject.SetActive(false);
        SpawnPrefab("Assets/Honda Motor Co., Ltd/Super Cub C125/Prefabs/C125 - MM.prefab", zone, new Vector3(-1.8f, 0.02f, 58f),
            Quaternion.Euler(0f, 90f, 0f), Vector3.one * 0.95f, "TrafficScooter_BlockerA", false);
        SpawnPrefab("Assets/Assets/AIRIDev/Low-Poly Urban Assets/Prefabs/parking cone.prefab", zone, new Vector3(1.5f, 0f, 57.2f),
            Quaternion.identity, Vector3.one * 1.15f, "TrafficCone_BlockerA", false);
        CreatePrimitive(PrimitiveType.Cube, "TrafficMarketCrate_Blocker", zone, new Vector3(0.4f, 0.45f, 60.2f),
            Quaternion.identity, new Vector3(1.6f, 0.9f, 1.2f), materials.Wood, true);
    }

    private static void CreateHoaMaiTree(Transform parent, Vector3 position, float scale, MaterialSet materials, string name)
    {
        Transform root = CreateChild(parent, name);
        root.position = position;
        CreatePrimitive(PrimitiveType.Cylinder, "Trunk", root, new Vector3(0f, 1.0f * scale, 0f), Quaternion.identity,
            new Vector3(0.18f * scale, 1.05f * scale, 0.18f * scale), materials.Wood, false);

        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f;
            Vector3 branchPosition = Quaternion.Euler(0f, angle, 0f) * new Vector3(0.45f * scale, 2.0f * scale, 0f);
            CreatePrimitive(PrimitiveType.Cube, "Branch_" + i, root, branchPosition, Quaternion.Euler(18f, angle, 18f),
                new Vector3(0.1f * scale, 0.08f * scale, 1.0f * scale), materials.Wood, false);
        }

        Vector3[] blossoms =
        {
            new Vector3(-0.7f, 2.35f, 0.15f), new Vector3(-0.2f, 2.65f, -0.45f), new Vector3(0.45f, 2.45f, 0.35f),
            new Vector3(0.75f, 2.1f, -0.15f), new Vector3(-0.45f, 2.05f, 0.55f), new Vector3(0.05f, 2.9f, 0.2f)
        };

        for (int i = 0; i < blossoms.Length; i++)
        {
            CreatePrimitive(PrimitiveType.Sphere, "YellowBlossom_" + i, root, blossoms[i] * scale, Quaternion.identity,
                Vector3.one * (0.32f * scale), materials.FlowerYellow, false);
        }
    }

    private static void CreateLanternPair(Transform parent, Vector3 a, Vector3 b, MaterialSet materials, string name)
    {
        Transform root = CreateChild(parent, name);
        CreateLantern(root, a, materials, "Lantern_A");
        CreateLantern(root, b, materials, "Lantern_B");
    }

    private static void CreateLanternString(Transform parent, Vector3 start, Vector3 end, int count, MaterialSet materials, string name)
    {
        Transform root = CreateChild(parent, name);
        Vector3 center = (start + end) * 0.5f;
        float length = Vector3.Distance(start, end);
        Quaternion rotation = Quaternion.LookRotation(end - start, Vector3.up);
        CreatePrimitive(PrimitiveType.Cube, "String", root, center, rotation, new Vector3(0.035f, 0.035f, length), materials.WoodDark, false);
        for (int i = 0; i < count; i++)
        {
            float t = count <= 1 ? 0.5f : i / (float)(count - 1);
            Vector3 position = Vector3.Lerp(start, end, t) + Vector3.down * (0.18f + Mathf.Sin(t * Mathf.PI) * 0.12f);
            CreateLantern(root, position, materials, "Lantern_" + i);
        }
    }

    private static void CreateLantern(Transform parent, Vector3 position, MaterialSet materials, string name)
    {
        Transform root = CreateChild(parent, name);
        root.position = position;
        CreatePrimitive(PrimitiveType.Sphere, "LanternBody", root, Vector3.zero, Quaternion.identity,
            new Vector3(0.42f, 0.58f, 0.42f), materials.Red, false);
        CreatePrimitive(PrimitiveType.Cylinder, "LanternCapTop", root, new Vector3(0f, 0.34f, 0f), Quaternion.identity,
            new Vector3(0.23f, 0.04f, 0.23f), materials.Gold, false);
        CreatePrimitive(PrimitiveType.Cylinder, "LanternCapBottom", root, new Vector3(0f, -0.34f, 0f), Quaternion.identity,
            new Vector3(0.23f, 0.04f, 0.23f), materials.Gold, false);
    }

    private static void CreateWatermelonPile(Transform parent, Vector3 position, MaterialSet materials, string name, int count)
    {
        Transform root = CreateChild(parent, name);
        root.position = position;
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3((i % 3) * 0.5f - 0.5f, (i / 3) * 0.28f, (i % 2) * 0.42f - 0.2f);
            Transform melon = CreatePrimitive(PrimitiveType.Sphere, "Watermelon_" + i, root, offset, Quaternion.Euler(0f, i * 23f, 0f),
                new Vector3(0.58f, 0.42f, 0.42f), materials.Watermelon, false).transform;
            CreatePrimitive(PrimitiveType.Cube, "StripeA", melon, new Vector3(0f, 0.01f, 0.01f), Quaternion.Euler(0f, 0f, 0f),
                new Vector3(0.065f, 1.02f, 1.04f), materials.WatermelonDark, false);
        }
    }

    private static void CreateBanhChungStack(Transform parent, Vector3 position, MaterialSet materials, string name, int count)
    {
        Transform root = CreateChild(parent, name);
        root.position = position;
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3((i % 2) * 0.55f - 0.25f, i * 0.18f, (i % 3) * 0.18f);
            Transform cake = CreatePrimitive(PrimitiveType.Cube, "BanhChung_" + i, root, offset, Quaternion.Euler(0f, i * 12f, 0f),
                new Vector3(0.55f, 0.16f, 0.55f), materials.BanhChung, false).transform;
            CreatePrimitive(PrimitiveType.Cube, "BambooStringA", cake, Vector3.zero, Quaternion.identity,
                new Vector3(0.06f, 1.06f, 1.08f), materials.BambooString, false);
            CreatePrimitive(PrimitiveType.Cube, "BambooStringB", cake, Vector3.zero, Quaternion.identity,
                new Vector3(1.08f, 1.06f, 0.06f), materials.BambooString, false);
        }
    }

    private static void CreateGiftBasket(Transform parent, Vector3 position, MaterialSet materials, string name)
    {
        Transform root = CreateChild(parent, name);
        root.position = position;
        CreatePrimitive(PrimitiveType.Cube, "BasketBase", root, Vector3.zero, Quaternion.identity,
            new Vector3(1.15f, 0.42f, 0.82f), materials.Wood, false);
        CreatePrimitive(PrimitiveType.Cube, "RedWrap", root, new Vector3(0f, 0.34f, 0f), Quaternion.identity,
            new Vector3(1.02f, 0.28f, 0.68f), materials.Red, false);
        CreatePrimitive(PrimitiveType.Sphere, "OrangeGift", root, new Vector3(-0.25f, 0.65f, 0.05f), Quaternion.identity,
            Vector3.one * 0.28f, materials.Orange, false);
        CreatePrimitive(PrimitiveType.Sphere, "GoldGift", root, new Vector3(0.25f, 0.67f, -0.06f), Quaternion.identity,
            Vector3.one * 0.24f, materials.Gold, false);
        CreatePrimitive(PrimitiveType.Cube, "Ribbon", root, new Vector3(0f, 0.74f, 0f), Quaternion.Euler(0f, 45f, 0f),
            new Vector3(0.9f, 0.07f, 0.12f), materials.Gold, false);
    }

    private static void CreateCouplet(Transform parent, Vector3 position, Quaternion rotation, MaterialSet materials, string name)
    {
        Transform root = CreateChild(parent, name);
        root.position = position;
        root.rotation = rotation;
        CreatePrimitive(PrimitiveType.Cube, "CoupletPanel", root, Vector3.zero, Quaternion.identity,
            new Vector3(0.42f, 1.8f, 0.045f), materials.Red, false);
        CreatePrimitive(PrimitiveType.Cube, "CoupletTrimTop", root, new Vector3(0f, 0.82f, -0.005f), Quaternion.identity,
            new Vector3(0.5f, 0.06f, 0.055f), materials.Gold, false);
        CreatePrimitive(PrimitiveType.Cube, "CoupletTrimBottom", root, new Vector3(0f, -0.82f, -0.005f), Quaternion.identity,
            new Vector3(0.5f, 0.06f, 0.055f), materials.Gold, false);
    }

    private static void CreateMarketSign(Transform parent, Vector3 position, Quaternion rotation, string label, MaterialSet materials, string name)
    {
        Transform root = CreateChild(parent, name);
        root.position = position;
        root.rotation = rotation;
        CreatePrimitive(PrimitiveType.Cube, "SignBoard", root, new Vector3(0f, 0.55f, 0f), Quaternion.identity,
            new Vector3(2.5f, 0.72f, 0.12f), materials.Red, false);
        CreatePrimitive(PrimitiveType.Cylinder, "PostL", root, new Vector3(-1.05f, 0f, 0f), Quaternion.identity,
            new Vector3(0.08f, 0.85f, 0.08f), materials.WoodDark, false);
        CreatePrimitive(PrimitiveType.Cylinder, "PostR", root, new Vector3(1.05f, 0f, 0f), Quaternion.identity,
            new Vector3(0.08f, 0.85f, 0.08f), materials.WoodDark, false);

        GameObject textObject = new GameObject("Label");
        textObject.transform.SetParent(root, false);
        textObject.transform.localPosition = new Vector3(0f, 0.55f, -0.07f);
        textObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        textObject.transform.localScale = Vector3.one * 0.16f;
        TextMesh text = textObject.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 1f;
        text.fontSize = 56;
        text.color = new Color(1f, 0.82f, 0.25f, 1f);
    }

    private static void ScatterPrefab(string path, Transform parent, string prefix, Vector3[] positions, float scale)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            SpawnPrefab(path, parent, positions[i], Quaternion.Euler(0f, (i * 47f) % 360f, 0f),
                Vector3.one * (scale + ((i % 3) * 0.04f)), prefix + "_" + i, true);
        }
    }

    private static GameObject SpawnPrefab(string path, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, string name, bool stripColliders)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        GameObject instance = null;
        if (prefab != null)
        {
            instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }

        if (instance == null)
        {
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        if (stripColliders)
        {
            StripColliders(instance);
        }

        return instance;
    }

    private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition,
        Quaternion localRotation, Vector3 localScale, Material material, bool keepCollider)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = localRotation;
        go.transform.localScale = localScale;
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        if (!keepCollider)
        {
            StripColliders(go);
        }

        return go;
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void StripColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    private static MaterialSet CreateMaterialSet()
    {
        return new MaterialSet
        {
            Red = EnsureMaterial("Tet_Red.mat", new Color(0.72f, 0.12f, 0.10f, 1f)),
            Gold = EnsureMaterial("Tet_Gold.mat", new Color(0.95f, 0.69f, 0.20f, 1f)),
            FlowerYellow = EnsureMaterial("Tet_FlowerYellow.mat", new Color(0.97f, 0.78f, 0.20f, 1f)),
            Wood = EnsureMaterial("Tet_Wood.mat", new Color(0.45f, 0.29f, 0.17f, 1f)),
            WoodDark = EnsureMaterial("Tet_WoodDark.mat", new Color(0.25f, 0.16f, 0.11f, 1f)),
            Watermelon = EnsureMaterial("Tet_Watermelon.mat", new Color(0.18f, 0.52f, 0.18f, 1f)),
            WatermelonDark = EnsureMaterial("Tet_WatermelonDark.mat", new Color(0.06f, 0.27f, 0.09f, 1f)),
            BanhChung = EnsureMaterial("Tet_BanhChung.mat", new Color(0.18f, 0.43f, 0.24f, 1f)),
            BambooString = EnsureMaterial("Tet_BambooString.mat", new Color(0.78f, 0.70f, 0.36f, 1f)),
            Orange = EnsureMaterial("Tet_Orange.mat", new Color(0.92f, 0.45f, 0.14f, 1f))
        };
    }

    private static Material EnsureMaterial(string fileName, Color color)
    {
        string path = MaterialsRoot + "/" + fileName;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string leaf = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, leaf);
    }

    private struct MaterialSet
    {
        public Material Red;
        public Material Gold;
        public Material FlowerYellow;
        public Material Wood;
        public Material WoodDark;
        public Material Watermelon;
        public Material WatermelonDark;
        public Material BanhChung;
        public Material BambooString;
        public Material Orange;
    }
}
#endif
