using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Editor
{
    /// <summary>Demolishes urban highway visuals and rebuilds Vietnamese countryside expressway scenery.</summary>
    public static class Level1HighwayCountrysideRebuild
    {
        private const string LibraryPath = "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset";

        public static void RebuildCountrysideEnvironment()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1HighwayCountrysideRebuild] Exit Play mode before rebuilding.");
                return;
            }

            Scene scene = EditorSceneManager.GetSceneByPath(Level1HighwaySceneBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(Level1HighwaySceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            HighwayNatureStarterKitBaker.BakeVegetation();
            HighwaySceneMaterialFactory.EnsureFolders();

            Material asphalt = CreateHighwayAsphalt();
            Material laneMark = CreateHighwayLaneMark();
            Material shoulder = CreateHighwayShoulder();
            Material edgeMark = HighwaySceneMaterialFactory.CopySharedOrCreate(
                HighwaySceneAssetPaths.LaneMarkMat,
                HighwayRoadMaterialDefaults.EdgeMarkFile);
            Material guardRail = CreateGuardRail();
            Material tireWear = CreateTireWear();
            Material water = CreateWater();
            Material bridgeConcrete = CreateBridgeConcrete();
            Material bridgeUnderside = CreateBridgeUnderside();
            Material grassGround = CreateGrassGround();
            Material grassBladeA = CreateGrassBladeA();
            Material grassBladeB = CreateGrassBladeB();
            Material foliage = CreateFoliage();
            Material bark = CreateBark();
            Material signMat = CreateSignMaterial();
            Material poleMat = CreatePoleMaterial();
            Material studySky = HighwaySceneMaterialFactory.EnsureSkyboxCopy();
            HighwayNaturePrefabLibrary library =
                AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(LibraryPath);

            DestroyUrbanSystems();
            EnsureWindZone();
            RebuildRoad(asphalt, laneMark, shoulder, edgeMark, guardRail, tireWear, bridgeConcrete, bridgeUnderside);
            RebuildSideGround(grassGround, grassBladeA, grassBladeB, library);
            DisableNatureRoadsideSpawner();
            EnsureCountrysideScenery(
                water,
                grassGround,
                foliage,
                bark,
                grassBladeA,
                library,
                grassBladeB);
            EnsureCountrysideProps(signMat, poleMat);
            EnsureHorizonLayer();
            ApplySceneLighting(studySky);
            WireGameController(studySky);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Level1HighwayCountrysideRebuild] Vietnamese countryside expressway environment rebuilt.");
        }

        private static Material CreateHighwayAsphalt() =>
            HighwaySceneMaterialFactory.CopySharedOrCreate(
                HighwaySceneAssetPaths.AsphaltMat,
                HighwayRoadMaterialDefaults.RoadSurfaceFile,
                mat =>
                {
                    Color dark = new Color(0.18f, 0.18f, 0.2f);
                    mat.SetColor("_BaseColor", dark);
                    mat.SetColor("_Color", dark);
                    mat.SetFloat("_Smoothness", 0.32f);
                });

        private static Material CreateHighwayLaneMark() =>
            HighwaySceneMaterialFactory.CopySharedOrCreate(
                HighwaySceneAssetPaths.LaneMarkMat,
                HighwayRoadMaterialDefaults.LaneMarkFile);

        private static Material CreateHighwayShoulder() =>
            HighwaySceneMaterialFactory.CopySharedOrCreate(
                HighwaySceneAssetPaths.ShoulderMat,
                HighwayRoadMaterialDefaults.ShoulderFile,
                mat =>
                {
                    Color concrete = new Color(0.66f, 0.64f, 0.6f);
                    mat.SetColor("_BaseColor", concrete);
                    mat.SetColor("_Color", concrete);
                });

        private static Material CreateGuardRail() =>
            HighwaySceneMaterialFactory.EnsureLit(
                HighwaySceneAssetPaths.GuardRailMat,
                new Color(0.72f, 0.74f, 0.76f),
                smoothness: 0.62f,
                metallic: 0.55f);

        private static Material CreateTireWear() =>
            HighwaySceneMaterialFactory.EnsureLit(
                HighwaySceneAssetPaths.TireWearMat,
                new Color(0.14f, 0.14f, 0.15f, 0.5f),
                smoothness: 0.12f);

        private static Material CreateWater()
        {
            // URP asset on disk — Built-in LowPolyWaterMaterial is magenta in this project.
            return HighwayCountrysideRiverBuilder.EnsurePackWaterMaterialAsset()
                   ?? HighwaySceneMaterialFactory.EnsureLit(
                       HighwaySceneAssetPaths.WaterMat,
                       new Color(0.2f, 0.46f, 0.56f, 0.85f),
                       smoothness: 0.94f,
                       metallic: 0.06f,
                       transparent: true);
        }

        private static Material CreateBridgeConcrete() =>
            HighwaySceneMaterialFactory.EnsureLit(
                HighwaySceneAssetPaths.BridgeConcreteMat,
                new Color(0.58f, 0.56f, 0.52f),
                smoothness: 0.38f);

        private static Material CreateBridgeUnderside() =>
            HighwaySceneMaterialFactory.EnsureLit(
                HighwaySceneAssetPaths.BridgeUndersideMat,
                new Color(0.28f, 0.27f, 0.26f),
                smoothness: 0.18f);

        private static Material CreateGrassGround() =>
            HighwaySceneMaterialFactory.CopyTextureMaterial(
                HighwaySceneAssetPaths.GrassGroundMat,
                "Assets/NatureStarterKit2/Textures/grass01.tga",
                new Color(0.36f, 0.52f, 0.28f),
                new Vector2(10f, 8f));

        private static Material CreateGrassBladeA() =>
            HighwaySceneMaterialFactory.CopyTextureMaterial(
                HighwaySceneAssetPaths.GrassBladeAMat,
                "Assets/NatureStarterKit2/Textures/grass01.tga",
                Color.white,
                Vector2.one);

        private static Material CreateGrassBladeB() =>
            HighwaySceneMaterialFactory.CopyTextureMaterial(
                HighwaySceneAssetPaths.GrassBladeBMat,
                "Assets/NatureStarterKit2/Textures/grass02.tga",
                new Color(0.92f, 1f, 0.88f),
                Vector2.one);

        private static Material CreateFoliage() =>
            HighwaySceneMaterialFactory.CopySharedOrCreate(
                HighwaySceneAssetPaths.FoliageMat,
                HighwayNatureMaterialDefaults.FoliageFile);

        private static Material CreateBark() =>
            HighwaySceneMaterialFactory.CopySharedOrCreate(
                HighwaySceneAssetPaths.BarkMat,
                HighwayNatureMaterialDefaults.BarkFile);

        private static Material CreateSignMaterial() =>
            HighwaySceneMaterialFactory.EnsureLit(
                HighwaySceneAssetPaths.SignMat,
                new Color(0.2f, 0.38f, 0.72f),
                smoothness: 0.35f);

        private static Material CreatePoleMaterial() =>
            HighwaySceneMaterialFactory.EnsureLit(
                HighwaySceneAssetPaths.PoleMat,
                new Color(0.62f, 0.64f, 0.66f),
                smoothness: 0.55f,
                metallic: 0.45f);

        private static void DestroyUrbanSystems()
        {
            string[] destroyNames =
            {
                "HighwayStaticUrbanNeighborhood",
                "HighwayQuaterniusTownBlockPool",
                "HighwayQuaterniusSidewalkPool",
                "HighwayEnvironment",
                "LeftQuaterniusSidewalks",
                "RightQuaterniusSidewalks",
                "LeftQuaterniusBlocks",
                "RightQuaterniusBlocks"
            };

            for (int i = 0; i < destroyNames.Length; i++)
            {
                GameObject go = GameObject.Find(destroyNames[i]);
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }

            HighwayEnvironmentSpawner env = Object.FindFirstObjectByType<HighwayEnvironmentSpawner>();
            if (env != null)
            {
                Object.DestroyImmediate(env.gameObject);
            }
        }

        private static void RebuildRoad(
            Material asphalt,
            Material laneMark,
            Material shoulder,
            Material edgeMark,
            Material guardRail,
            Material tireWear,
            Material bridgeConcrete,
            Material bridgeUnderside)
        {
            HighwayRoadSegmentPool pool = Object.FindFirstObjectByType<HighwayRoadSegmentPool>();
            if (pool == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(pool);
            so.FindProperty("runtimeRoadSurface").objectReferenceValue = asphalt;
            so.FindProperty("runtimeLaneMark").objectReferenceValue = laneMark;
            so.FindProperty("runtimeShoulder").objectReferenceValue = shoulder;
            so.FindProperty("runtimeEdgeMark").objectReferenceValue = edgeMark;
            so.FindProperty("runtimeGuardRail").objectReferenceValue = guardRail;
            so.FindProperty("runtimeTireWear").objectReferenceValue = tireWear;
            so.FindProperty("runtimeBridgeConcrete").objectReferenceValue = bridgeConcrete;
            so.FindProperty("runtimeBridgeUnderside").objectReferenceValue = bridgeUnderside;
            so.ApplyModifiedPropertiesWithoutUndo();
            pool.RebuildRoadContent();
            pool.SetShouldersVisible(true);
        }

        private static void RebuildSideGround(
            Material grassGround,
            Material grassBladeA,
            Material grassBladeB,
            HighwayNaturePrefabLibrary library)
        {
            HighwaySideGroundPool pool = Object.FindFirstObjectByType<HighwaySideGroundPool>();
            if (pool == null)
            {
                return;
            }

            pool.gameObject.SetActive(true);
            SerializedObject so = new SerializedObject(pool);
            so.FindProperty("runtimeGrassGround").objectReferenceValue = grassGround;
            so.FindProperty("runtimeGrassBladeA").objectReferenceValue = grassBladeA;
            so.FindProperty("runtimeGrassBladeB").objectReferenceValue = grassBladeB;
            so.FindProperty("prefabLibrary").objectReferenceValue = library;
            so.ApplyModifiedPropertiesWithoutUndo();
            pool.RebuildSideGroundContent();
        }

        private static void DisableNatureRoadsideSpawner()
        {
            HighwayNatureRoadsideSpawner spawner = Object.FindFirstObjectByType<HighwayNatureRoadsideSpawner>();
            if (spawner != null)
            {
                spawner.gameObject.SetActive(false);
            }
        }

        private static void EnsureCountrysideScenery(
            Material water,
            Material bank,
            Material foliage,
            Material bark,
            Material grassBladeA,
            HighwayNaturePrefabLibrary library,
            Material grassBladeB)
        {
            GameObject world = GameObject.Find("HighwayWorld");
            HighwayCountrysideSceneryPool pool = Object.FindFirstObjectByType<HighwayCountrysideSceneryPool>();
            if (pool == null)
            {
                GameObject go = new GameObject("HighwayCountrysideScenery");
                if (world != null)
                {
                    go.transform.SetParent(world.transform, false);
                }

                pool = go.AddComponent<HighwayCountrysideSceneryPool>();
            }

            pool.gameObject.SetActive(true);
            pool.Initialize(
                water,
                bank,
                foliage,
                bark,
                grassBladeA,
                library,
                grassBladeB,
                HighwayCountrysideSceneryPool.CountrysideBiome.ForestRiverAlternating);
        }

        private static void EnsureCountrysideProps(Material sign, Material pole)
        {
            GameObject world = GameObject.Find("HighwayWorld");
            HighwayCountrysidePropsSpawner spawner = Object.FindFirstObjectByType<HighwayCountrysidePropsSpawner>();
            if (spawner == null)
            {
                GameObject go = new GameObject("HighwayCountrysideProps");
                if (world != null)
                {
                    go.transform.SetParent(world.transform, false);
                }

                spawner = go.AddComponent<HighwayCountrysidePropsSpawner>();
            }

            spawner.gameObject.SetActive(true);
            spawner.Initialize(sign, pole);
        }

        private static void EnsureHorizonLayer()
        {
            HighwayHorizonLayer horizon = Object.FindFirstObjectByType<HighwayHorizonLayer>();
            if (horizon == null)
            {
                GameObject world = GameObject.Find("HighwayWorld");
                GameObject go = new GameObject("HighwayHorizon");
                if (world != null)
                {
                    go.transform.SetParent(world.transform, false);
                }

                horizon = go.AddComponent<HighwayHorizonLayer>();
            }

            horizon.gameObject.SetActive(true);
        }

        private static void ApplySceneLighting(Material skybox)
        {
            ChoTetMarketEnvironment.ApplyHighwayRenderSettings(68f, 265f, null);

            Light sun = null;
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    sun = lights[i];
                    break;
                }
            }

            if (sun == null)
            {
                return;
            }

            sun.color = new Color(0.94f, 0.97f, 1f);
            sun.intensity = 1.12f;
            sun.shadowStrength = 0.68f;
            sun.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private static void WireGameController(Material skybox)
        {
            HighwayGameController controller = Object.FindFirstObjectByType<HighwayGameController>();
            if (controller == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("sideGroundPool").objectReferenceValue =
                Object.FindFirstObjectByType<HighwaySideGroundPool>();
            so.FindProperty("natureRoadsideSpawner").objectReferenceValue =
                Object.FindFirstObjectByType<HighwayNatureRoadsideSpawner>();
            so.FindProperty("countrysideSceneryPool").objectReferenceValue =
                Object.FindFirstObjectByType<HighwayCountrysideSceneryPool>();
            so.FindProperty("countrysidePropsSpawner").objectReferenceValue =
                Object.FindFirstObjectByType<HighwayCountrysidePropsSpawner>();
            so.FindProperty("horizonLayer").objectReferenceValue =
                Object.FindFirstObjectByType<HighwayHorizonLayer>();
            so.FindProperty("staticUrbanNeighborhood").objectReferenceValue = null;
            so.FindProperty("quaterniusTownBlockPool").objectReferenceValue = null;
            so.FindProperty("quaterniusSidewalkPool").objectReferenceValue = null;
            so.FindProperty("environmentSpawner").objectReferenceValue = null;
            so.FindProperty("sunsetSkybox").objectReferenceValue = skybox;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureWindZone()
        {
            GameObject world = GameObject.Find("HighwayWorld");
            Transform windTransform = world != null ? world.transform.Find("HighwayWind") : null;
            GameObject windGo = windTransform != null ? windTransform.gameObject : GameObject.Find("HighwayWind");
            if (windGo == null)
            {
                windGo = new GameObject("HighwayWind");
                if (world != null)
                {
                    windGo.transform.SetParent(world.transform, false);
                }
            }

            WindZone wind = windGo.GetComponent<WindZone>() ?? windGo.AddComponent<WindZone>();
            wind.mode = WindZoneMode.Directional;
            wind.windMain = 0.35f;
            wind.windTurbulence = 0.45f;
            wind.windPulseMagnitude = 0.15f;
            wind.windPulseFrequency = 0.25f;
        }
    }
}
