using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Composes 3D diorama scenes for level-select thumbnail rendering and capture.
    /// </summary>
    public static class LevelSelectThumbnailDiorama
    {
        public const int ThumbnailLayer = 31;
        private const string OccaRoot = MainMenuAssetPaths.OccaPrefabs;
        private const string MarketRoot = MainMenuAssetPaths.MarketPrefabs;
        private const string NatureRoot = MainMenuAssetPaths.NaturePrefabs;

        public static ThumbnailScene BuildHomeScene(Transform root)
        {
            ConfigureEveningEnvironment(new Color(0.96f, 0.72f, 0.48f), 0.95f, 14f, 34f);

            Transform world = CreateWorldRoot(root, "Level1HomeWorld");
            Material grass = CreateGroundMaterial(MainMenuAssetPaths.GrassTexture, new Color(0.58f, 0.72f, 0.48f), 5f);
            Material path = CreateGroundMaterial(MainMenuAssetPaths.RoadTexture, new Color(0.82f, 0.74f, 0.62f), 4f);

            CreateGroundCube(world, "Ground", new Vector3(0f, -0.5f, 2f), new Vector3(24f, 1f, 20f), grass);
            CreateGroundCube(world, "Path", new Vector3(0f, -0.38f, 1.2f), new Vector3(3.2f, 0.08f, 14f), path);

            Spawn(OccaRoot + "/House_2.prefab", world, new Vector3(0f, 0f, 5.8f), Quaternion.Euler(0f, 180f, 0f),
                Vector3.one * 1.35f, "HomeFocus");
            Spawn(OccaRoot + "/House_1.prefab", world, new Vector3(-5.2f, 0f, 7.2f), Quaternion.Euler(0f, 145f, 0f),
                Vector3.one * 1.05f, "NeighborHouse");

            Spawn(OccaRoot + "/Path Piece_5.prefab", world, new Vector3(0f, 0f, 0.8f), Quaternion.identity,
                Vector3.one * 1.1f, "PathNear");
            Spawn(OccaRoot + "/Path Piece_8.prefab", world, new Vector3(0f, 0f, 3.2f), Quaternion.identity,
                Vector3.one * 1.1f, "PathMid");
            Spawn(OccaRoot + "/Path Piece_12.prefab", world, new Vector3(0f, 0f, 4.8f), Quaternion.identity,
                Vector3.one * 1.05f, "PathFar");

            Spawn(OccaRoot + "/Tree_6.prefab", world, new Vector3(-7.5f, 0f, 3.5f), Quaternion.Euler(0f, 24f, 0f),
                Vector3.one * 1.45f, "TreeLeft");
            Spawn(OccaRoot + "/Tree_4.prefab", world, new Vector3(7.8f, 0f, 4.2f), Quaternion.Euler(0f, -32f, 0f),
                Vector3.one * 1.35f, "TreeRight");
            Spawn(OccaRoot + "/Pine Tree_3.prefab", world, new Vector3(-8.5f, 0f, 8.5f), Quaternion.Euler(0f, 12f, 0f),
                Vector3.one * 1.2f, "PineBackLeft");
            Spawn(OccaRoot + "/Pine Tree_5.prefab", world, new Vector3(8.2f, 0f, 8.8f), Quaternion.Euler(0f, -18f, 0f),
                Vector3.one * 1.15f, "PineBackRight");

            Spawn(NatureRoot + "/tree3_2_prefab.prefab", world, new Vector3(-5.8f, 0f, 1.8f),
                Quaternion.Euler(0f, 40f, 0f), Vector3.one * 0.95f, "NatureTreeL");
            Spawn(NatureRoot + "/tree4_1_prefab.prefab", world, new Vector3(6.2f, 0f, 2.4f),
                Quaternion.Euler(0f, -55f, 0f), Vector3.one * 0.9f, "NatureTreeR");

            Spawn(OccaRoot + "/Lantern.prefab", world, new Vector3(-1.4f, 0f, 4.2f), Quaternion.identity,
                Vector3.one * 1.25f, "LanternLeft");
            Spawn(OccaRoot + "/Lantern.prefab", world, new Vector3(1.5f, 0f, 4.2f), Quaternion.identity,
                Vector3.one * 1.25f, "LanternRight");
            Spawn(OccaRoot + "/Lantern.prefab", world, new Vector3(0.2f, 0f, 6.8f), Quaternion.identity,
                Vector3.one * 1.05f, "LanternHouse");

            Spawn(OccaRoot + "/Flower Pot.prefab", world, new Vector3(-2.8f, 0f, 5.4f), Quaternion.identity,
                Vector3.one * 1.2f, "FlowerPotL");
            Spawn(OccaRoot + "/Flower Pot.prefab", world, new Vector3(2.9f, 0f, 5.5f), Quaternion.identity,
                Vector3.one * 1.15f, "FlowerPotR");
            Spawn(OccaRoot + "/Flower_1.prefab", world, new Vector3(-4.2f, 0f, 2.2f), Quaternion.identity,
                Vector3.one * 1.8f, "WildFlowersL");
            Spawn(OccaRoot + "/Flower_4.prefab", world, new Vector3(4.5f, 0f, 2.5f), Quaternion.identity,
                Vector3.one * 1.6f, "WildFlowersR");
            Spawn(OccaRoot + "/Bench.prefab", world, new Vector3(-3.4f, 0f, 3.6f), Quaternion.Euler(0f, 70f, 0f),
                Vector3.one * 1.05f, "Bench");
            Spawn(OccaRoot + "/House_3.prefab", world, new Vector3(4.8f, 0f, 6.8f), Quaternion.Euler(0f, -160f, 0f),
                Vector3.one * 0.95f, "VillageHouse");
            Spawn(NatureRoot + "/flower1-2.prefab", world, new Vector3(-2.2f, 0f, 2.4f),
                Quaternion.identity, Vector3.one * 1.2f, "NatureFlowers");
            Spawn(NatureRoot + "/bush2.prefab", world, new Vector3(3.8f, 0f, 2.8f),
                Quaternion.identity, Vector3.one * 1.1f, "BushAccent");

            for (int i = 0; i < 6; i++)
            {
                Spawn(NatureRoot + "/grass2.prefab", world,
                    new Vector3(-6f + i * 2.2f, 0f, 1.2f + (i % 2) * 0.4f),
                    Quaternion.Euler(0f, i * 29f, 0f), Vector3.one * 1.05f, "Grass_" + i);
            }

            for (int i = 0; i < 4; i++)
            {
                Spawn(OccaRoot + "/Fence.prefab", world, new Vector3(-8f + i * 2.1f, 0f, 9.8f),
                    Quaternion.identity, Vector3.one * 1f, "Fence_" + i);
            }

            CreateDirectionalLight(root, new Color(1f, 0.72f, 0.48f), 1.15f, new Vector3(24f, -38f, 0f));
            CreatePointLight(root, "WindowGlow", new Vector3(0.3f, 2.2f, 5.6f), new Color(1f, 0.82f, 0.52f), 2.8f, 7f);
            CreatePointLight(root, "LanternWarm", new Vector3(-1.4f, 1.6f, 4.2f), new Color(1f, 0.78f, 0.42f), 1.8f, 5f);
            CreatePointLight(root, "LanternWarm2", new Vector3(1.5f, 1.6f, 4.2f), new Color(1f, 0.78f, 0.42f), 1.8f, 5f);

            Camera camera = CreateCaptureCamera(root, new Vector3(0.2f, 2.4f, -6.8f), Quaternion.Euler(8f, 0f, 0f), 42f);
            return new ThumbnailScene(camera, world.gameObject);
        }

        public static ThumbnailScene BuildMarketScene(Transform root)
        {
            ConfigureEveningEnvironment(new Color(0.98f, 0.78f, 0.55f), 1.05f, 12f, 30f);

            Transform world = CreateWorldRoot(root, "Level2MarketWorld");
            Material dirt = CreateGroundMaterial(MainMenuAssetPaths.DirtTexture, new Color(0.86f, 0.74f, 0.58f), 4f);
            Material road = CreateGroundMaterial(MainMenuAssetPaths.RoadTexture, new Color(0.92f, 0.82f, 0.68f), 5f);

            CreateGroundCube(world, "MarketGround", new Vector3(0f, -0.48f, 3f), new Vector3(22f, 1f, 16f), dirt);
            CreateGroundCube(world, "MarketPath", new Vector3(0f, -0.36f, 1.5f), new Vector3(5f, 0.08f, 12f), road);

            Spawn(MarketRoot + "/Vegetable_market_with_objects.prefab", world, new Vector3(-4.2f, 0f, 4.8f),
                Quaternion.Euler(0f, 35f, 0f), Vector3.one * 1.18f, "VegStall");
            Spawn(MarketRoot + "/Bakery_market_with_food.prefab", world, new Vector3(1.2f, 0f, 5.6f),
                Quaternion.Euler(0f, -12f, 0f), Vector3.one * 1.12f, "BakeryStall");
            Spawn(MarketRoot + "/Meat_market_with_objects.prefab", world, new Vector3(6.2f, 0f, 4.4f),
                Quaternion.Euler(0f, -48f, 0f), Vector3.one * 1.08f, "MeatStall");
            Spawn(MarketRoot + "/Fish_market_with_sections.prefab", world, new Vector3(-7.4f, 0f, 3.2f),
                Quaternion.Euler(0f, 62f, 0f), Vector3.one * 0.95f, "FishStall");

            Spawn(MarketRoot + "/Fruits and vegetables/watermelon.prefab", world, new Vector3(-2.4f, 0f, 2.8f),
                Quaternion.identity, Vector3.one * 1.35f, "Watermelon");
            Spawn(MarketRoot + "/Fruits and vegetables/apple_red.prefab", world, new Vector3(-3.1f, 0.55f, 3.4f),
                Quaternion.identity, Vector3.one * 1.5f, "Apples");
            Spawn(MarketRoot + "/Fruits and vegetables/tomato.prefab", world, new Vector3(-5.2f, 0.45f, 3.8f),
                Quaternion.identity, Vector3.one * 1.4f, "Tomatoes");
            Spawn(MarketRoot + "/Fruits and vegetables/carrot.prefab", world, new Vector3(-4.8f, 0.42f, 2.6f),
                Quaternion.identity, Vector3.one * 1.35f, "Carrots");
            Spawn(MarketRoot + "/Fruits and vegetables/banana.prefab", world, new Vector3(0.2f, 0.5f, 3.2f),
                Quaternion.identity, Vector3.one * 1.3f, "Bananas");
            Spawn(MarketRoot + "/Fruits and vegetables/beet.prefab", world, new Vector3(-1.8f, 0.48f, 3.6f),
                Quaternion.identity, Vector3.one * 1.25f, "Beets");
            Spawn(MarketRoot + "/Fruits and vegetables/watermelon.prefab", world, new Vector3(3.2f, 0f, 3.4f),
                Quaternion.Euler(0f, 25f, 0f), Vector3.one * 1.2f, "Watermelon2");

            Spawn(MarketRoot + "/vegetable_section_02.prefab", world, new Vector3(-0.8f, 0f, 6.2f),
                Quaternion.Euler(0f, 8f, 0f), Vector3.one * 0.95f, "VegSection");

            Spawn(MarketRoot + "/wooden_box.prefab", world, new Vector3(3.8f, 0f, 2.6f),
                Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.55f, "CrateA");
            Spawn(MarketRoot + "/wooden_box.prefab", world, new Vector3(4.9f, 0f, 2.4f),
                Quaternion.Euler(0f, -24f, 0f), Vector3.one * 1.35f, "CrateB");
            Spawn(MarketRoot + "/basket_01.prefab", world, new Vector3(-1.2f, 0f, 2.2f),
                Quaternion.Euler(0f, 22f, 0f), Vector3.one * 1.35f, "BasketA");
            Spawn(MarketRoot + "/basket_02.prefab", world, new Vector3(2.6f, 0f, 2.1f),
                Quaternion.Euler(0f, -15f, 0f), Vector3.one * 1.25f, "BasketB");
            Spawn(MarketRoot + "/round_bread_basket.prefab", world, new Vector3(1.8f, 0.48f, 4.8f),
                Quaternion.identity, Vector3.one * 1.15f, "BreadBasket");
            Spawn(MarketRoot + "/apple_box.prefab", world, new Vector3(-6.4f, 0f, 2.4f),
                Quaternion.Euler(0f, 40f, 0f), Vector3.one * 1.2f, "AppleBox");
            Spawn(MarketRoot + "/bag_open.prefab", world, new Vector3(5.8f, 0f, 2.2f),
                Quaternion.Euler(0f, -30f, 0f), Vector3.one * 1.2f, "Sack");
            Spawn(MarketRoot + "/stand_01.prefab", world, new Vector3(7.8f, 0f, 5.8f),
                Quaternion.Euler(0f, -70f, 0f), Vector3.one * 1.1f, "SideStand");

            Spawn(MarketRoot + "/Environment/Latern.prefab", world, new Vector3(-2.2f, 0f, 5.2f),
                Quaternion.identity, Vector3.one * 1.35f, "LanternA");
            Spawn(MarketRoot + "/Environment/Latern.prefab", world, new Vector3(2.8f, 0f, 5.4f),
                Quaternion.identity, Vector3.one * 1.3f, "LanternB");
            Spawn(MarketRoot + "/Environment/Latern.prefab", world, new Vector3(6.8f, 0f, 4.8f),
                Quaternion.identity, Vector3.one * 1.2f, "LanternC");
            Spawn(MarketRoot + "/Environment/Latern.prefab", world, new Vector3(-6.2f, 0f, 4.6f),
                Quaternion.identity, Vector3.one * 1.15f, "LanternD");

            Spawn(NatureRoot + "/grass2.prefab", world, new Vector3(-8.2f, 0f, 1.8f),
                Quaternion.identity, Vector3.one * 1f, "GrassL");
            Spawn(NatureRoot + "/grass2.prefab", world, new Vector3(8.5f, 0f, 1.6f),
                Quaternion.identity, Vector3.one * 1f, "GrassR");

            CreateDirectionalLight(root, new Color(1f, 0.84f, 0.62f), 1.35f, new Vector3(28f, -52f, 0f));
            CreatePointLight(root, "StallGlowA", new Vector3(-4.2f, 2.4f, 4.8f), new Color(1f, 0.72f, 0.38f), 2.2f, 8f);
            CreatePointLight(root, "StallGlowB", new Vector3(1.2f, 2.4f, 5.6f), new Color(1f, 0.76f, 0.42f), 2f, 8f);
            CreatePointLight(root, "StallGlowC", new Vector3(6.2f, 2.2f, 4.4f), new Color(1f, 0.7f, 0.36f), 1.8f, 7f);

            Camera camera = CreateCaptureCamera(root, new Vector3(0.5f, 3.2f, -7.2f), Quaternion.Euler(14f, 0f, 0f), 46f);
            return new ThumbnailScene(camera, world.gameObject);
        }

        private static Transform CreateWorldRoot(Transform root, string name)
        {
            GameObject world = new GameObject(name);
            world.transform.SetParent(root, false);
            SetLayerRecursive(world, ThumbnailLayer);
            return world.transform;
        }

        private static void ConfigureEveningEnvironment(Color fogColor, float ambientIntensity, float fogStart, float fogEnd)
        {
            Texture skyTexture = LoadAsset<Texture>(MainMenuAssetPaths.SkyHdr);
            if (skyTexture != null)
            {
                Shader skyShader = Shader.Find("Skybox/Panoramic");
                if (skyShader != null)
                {
                    Material skybox = new Material(skyShader);
                    skybox.SetTexture("_MainTex", skyTexture);
                    skybox.SetFloat("_Exposure", 1.08f);
                    skybox.SetFloat("_Rotation", 24f);
                    RenderSettings.skybox = skybox;
                }
            }

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.reflectionIntensity = 0.8f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        private static Camera CreateCaptureCamera(Transform root, Vector3 position, Quaternion rotation, float fov)
        {
            GameObject cameraObject = new GameObject("ThumbnailCamera");
            cameraObject.transform.SetParent(root, false);
            cameraObject.transform.localPosition = position;
            cameraObject.transform.localRotation = rotation;
            cameraObject.layer = ThumbnailLayer;

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = fov;
            camera.nearClipPlane = 0.15f;
            camera.farClipPlane = 80f;
            camera.cullingMask = 1 << ThumbnailLayer;
            camera.depth = -100f;
            return camera;
        }

        private static void CreateDirectionalLight(Transform root, Color color, float intensity, Vector3 euler)
        {
            GameObject lightObject = new GameObject("SunsetLight");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localRotation = Quaternion.Euler(euler);
            lightObject.layer = ThumbnailLayer;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.shadowStrength = 0.72f;
            light.cullingMask = 1 << ThumbnailLayer;
        }

        private static void CreatePointLight(Transform root, string name, Vector3 localPosition, Color color,
            float intensity, float range)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = localPosition;
            lightObject.layer = ThumbnailLayer;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            light.cullingMask = 1 << ThumbnailLayer;
        }

        private static GameObject CreateGroundCube(Transform parent, string name, Vector3 position, Vector3 scale,
            Material material)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = name;
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = position;
            ground.transform.localScale = scale;
            ground.layer = ThumbnailLayer;

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return ground;
        }

        private static Material CreateGroundMaterial(string texturePath, Color tint, float tiling)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            Texture2D texture = LoadAsset<Texture2D>(texturePath);
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

        private static GameObject Spawn(string path, Transform parent, Vector3 position, Quaternion rotation,
            Vector3 scale, string name)
        {
            GameObject prefab = LoadAsset<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("[LevelSelectThumbnailDiorama] Missing prefab: " + path);
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = scale;
            SetLayerRecursive(instance, ThumbnailLayer);
            return instance;
        }

        private static void SetLayerRecursive(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private static T LoadAsset<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }

        public readonly struct ThumbnailScene
        {
            public readonly Camera Camera;
            public readonly GameObject WorldRoot;

            public ThumbnailScene(Camera camera, GameObject worldRoot)
            {
                Camera = camera;
                WorldRoot = worldRoot;
            }
        }
    }
}
