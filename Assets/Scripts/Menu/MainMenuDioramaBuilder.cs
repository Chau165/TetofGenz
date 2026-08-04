using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Builds a cozy Tet market/village backdrop using existing project prefabs and PBR textures.
    /// </summary>
    public static class MainMenuDioramaBuilder
    {
        public static void Build(Transform root)
        {
            ConfigureEnvironment();
            CreateCamera(root);
            CreateLighting(root);

            Transform world = new GameObject("MenuWorld").transform;
            world.SetParent(root, false);

            Material grass = CreateGroundMaterial(MainMenuAssetPaths.GrassTexture, new Color(0.72f, 0.82f, 0.58f), 6f);
            Material dirt = CreateGroundMaterial(MainMenuAssetPaths.DirtTexture, new Color(0.82f, 0.72f, 0.58f), 4f);
            Material road = CreateGroundMaterial(MainMenuAssetPaths.RoadTexture, new Color(0.9f, 0.82f, 0.7f), 5f);

            CreateGroundCube(world, "GroundGrass", new Vector3(0f, -0.55f, 6f), new Vector3(36f, 1f, 28f), grass);
            CreateGroundCube(world, "GroundPath", new Vector3(0f, -0.42f, 5f), new Vector3(8f, 0.12f, 22f), road);
            CreateGroundCube(world, "GroundMarket", new Vector3(0f, -0.48f, 9f), new Vector3(18f, 1f, 12f), dirt);

            BuildMarketStalls(world);
            BuildVillageProps(world);
            BuildNatureScatter(world);
            PlaceAdventurer(world);
        }

        private static void ConfigureEnvironment()
        {
            Texture skyTexture = LoadAsset<Texture>(MainMenuAssetPaths.SkyHdr);
            if (skyTexture != null)
            {
                Shader skyShader = Shader.Find("Skybox/Panoramic");
                if (skyShader != null)
                {
                    Material skybox = new Material(skyShader);
                    skybox.SetTexture("_MainTex", skyTexture);
                    skybox.SetFloat("_Exposure", 1.05f);
                    skybox.SetFloat("_Rotation", 18f);
                    RenderSettings.skybox = skybox;
                }
            }

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.reflectionIntensity = 0.75f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.92f, 0.74f, 0.56f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 42f;
        }

        private static void CreateCamera(Transform root)
        {
            Camera existing = Object.FindAnyObjectByType<Camera>();
            if (existing != null)
            {
                ConfigureMenuCamera(existing);
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root, false);
            cameraObject.transform.position = new Vector3(1.8f, 3.6f, -10.5f);
            cameraObject.transform.rotation = Quaternion.Euler(14f, 6f, 0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            ConfigureMenuCamera(camera);
            cameraObject.AddComponent<AudioListener>();
        }

        private static void ConfigureMenuCamera(Camera camera)
        {
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 120f;
        }

        private static void CreateLighting(Transform root)
        {
            GameObject lightObject = new GameObject("Sunset Light");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.rotation = Quaternion.Euler(32f, -42f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.76f, 0.55f);
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.65f;
        }

        private static void BuildMarketStalls(Transform world)
        {
            SpawnPrefab(MainMenuAssetPaths.MarketPrefabs + "/Vegetable_market_with_objects.prefab",
                world, new Vector3(-4.5f, 0f, 8.5f), Quaternion.Euler(0f, 28f, 0f), Vector3.one * 1.15f, "VegStall");
            SpawnPrefab(MainMenuAssetPaths.MarketPrefabs + "/Bakery_market_with_food.prefab",
                world, new Vector3(2.2f, 0f, 9.8f), Quaternion.Euler(0f, -18f, 0f), Vector3.one * 1.1f, "BakeryStall");
            SpawnPrefab(MainMenuAssetPaths.MarketPrefabs + "/Meat_market_with_objects.prefab",
                world, new Vector3(6.8f, 0f, 7.6f), Quaternion.Euler(0f, -52f, 0f), Vector3.one * 1.08f, "MeatStall");

            SpawnPrefab(MainMenuAssetPaths.MarketPrefabs + "/watermelon_box.prefab",
                world, new Vector3(-2.2f, 0f, 7.2f), Quaternion.identity, Vector3.one * 1.25f, "WatermelonBox");
            SpawnPrefab(MainMenuAssetPaths.MarketPrefabs + "/wooden_box.prefab",
                world, new Vector3(4.8f, 0f, 6.8f), Quaternion.Euler(0f, 22f, 0f), Vector3.one * 1.6f, "WoodBox");
            SpawnPrefab(MainMenuAssetPaths.MarketPrefabs + "/basket_01.prefab",
                world, new Vector3(-6.2f, 0f, 6.4f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 1.35f, "Basket");
            SpawnPrefab(MainMenuAssetPaths.MarketPrefabs + "/stand_01.prefab",
                world, new Vector3(8.5f, 0f, 9.2f), Quaternion.Euler(0f, -70f, 0f), Vector3.one * 1.2f, "SideStand");
        }

        private static void BuildVillageProps(Transform world)
        {
            for (int i = 0; i < 5; i++)
            {
                SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Fence.prefab",
                    world, new Vector3(-10f + i * 2.4f, 0f, 12.5f), Quaternion.identity, Vector3.one * 1.1f, "Fence_" + i);
            }

            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Lantern.prefab",
                world, new Vector3(-1.2f, 0f, 11.8f), Quaternion.identity, Vector3.one * 1.2f, "LanternCenter");
            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Lantern.prefab",
                world, new Vector3(5.5f, 0f, 11.5f), Quaternion.identity, Vector3.one * 1.05f, "LanternRight");

            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Path Piece_5.prefab",
                world, new Vector3(0f, 0f, 4.5f), Quaternion.identity, Vector3.one * 1.15f, "PathA");
            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Path Piece_8.prefab",
                world, new Vector3(0f, 0f, 6.8f), Quaternion.identity, Vector3.one * 1.15f, "PathB");

            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Flower Pot.prefab",
                world, new Vector3(-7.5f, 0f, 8.8f), Quaternion.identity, Vector3.one * 1.25f, "FlowerPotL");
            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Flower_1.prefab",
                world, new Vector3(9.2f, 0f, 8.4f), Quaternion.identity, Vector3.one * 2.2f, "FlowerAccent");
            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Bench.prefab",
                world, new Vector3(-8.8f, 0f, 5.5f), Quaternion.Euler(0f, 55f, 0f), Vector3.one * 1.05f, "Bench");
        }

        private static void BuildNatureScatter(Transform world)
        {
            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Tree_6.prefab",
                world, new Vector3(-12f, 0f, 4f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.35f, "TreeLeft");
            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Tree_4.prefab",
                world, new Vector3(13f, 0f, 5f), Quaternion.Euler(0f, -24f, 0f), Vector3.one * 1.28f, "TreeRight");
            SpawnPrefab(MainMenuAssetPaths.OccaPrefabs + "/Pine Tree_3.prefab",
                world, new Vector3(-11f, 0f, 11f), Quaternion.Euler(0f, 10f, 0f), Vector3.one * 1.15f, "PineBack");

            Vector3[] grassSpots =
            {
                new(-9f, 0f, 6.5f), new(-7f, 0f, 5.2f), new(10f, 0f, 6f), new(8.5f, 0f, 10f),
                new(-5f, 0f, 12f), new(3f, 0f, 12.4f), new(-3f, 0f, 4.8f)
            };
            for (int i = 0; i < grassSpots.Length; i++)
            {
                SpawnPrefab(MainMenuAssetPaths.NaturePrefabs + "/grass2.prefab",
                    world, grassSpots[i], Quaternion.Euler(0f, i * 37f, 0f), Vector3.one * 1.1f, "Grass_" + i);
            }

            Vector3[] flowerSpots =
            {
                new(-6.5f, 0f, 7.8f), new(7.2f, 0f, 6.2f), new(1.5f, 0f, 12.8f)
            };
            for (int i = 0; i < flowerSpots.Length; i++)
            {
                SpawnPrefab(MainMenuAssetPaths.NaturePrefabs + "/flower1-2.prefab",
                    world, flowerSpots[i], Quaternion.identity, Vector3.one * 1.15f, "WildFlower_" + i);
            }

            SpawnPrefab(MainMenuAssetPaths.NaturePrefabs + "/rock3.prefab",
                world, new Vector3(11.5f, 0f, 11.8f), Quaternion.Euler(0f, -30f, 0f), Vector3.one * 1.4f, "Rock");
        }

        private static void PlaceAdventurer(Transform world)
        {
            GameObject adventurer = SpawnPrefab(MainMenuAssetPaths.AdventurerGlb,
                world, new Vector3(3.4f, 0f, 7.1f), Quaternion.Euler(0f, -135f, 0f), Vector3.one * 0.95f, "LittleAdventurer");
            if (adventurer == null)
            {
                return;
            }

            foreach (Collider collider in adventurer.GetComponentsInChildren<Collider>())
            {
                Object.Destroy(collider);
            }
        }

        private static GameObject CreateGroundCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
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

        private static Material CreateGroundMaterial(string texturePath, Color tint, float tiling)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

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

        private static GameObject SpawnPrefab(string path, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale, string name)
        {
            GameObject prefab = LoadAsset<GameObject>(path);
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;
            return instance;
        }

        private static T LoadAsset<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }
    }
}
