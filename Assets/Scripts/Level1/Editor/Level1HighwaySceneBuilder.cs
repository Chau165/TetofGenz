#if UNITY_EDITOR
using ChoTet.Level1.Highway;
using ChoTet.Menu.Editor;
using ChoTet.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds the Level 1 highway driving scene with pooled road, traffic, and decorations.</summary>
    public static class Level1HighwaySceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Level1_Highway.unity";

        public static void BuildHighwaySceneMenu()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1HighwaySceneBuilder] Exit Play mode before building.");
                return;
            }

            BuildHighwayScene();
            Debug.Log("[Level1HighwaySceneBuilder] Highway scene built at " + ScenePath);
        }

        public static void BuildHighwayScene()
        {
            Level1KenneyCarKitTrafficBuilder.EnsureTrafficPrefabs();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level1_Highway";

            ConfigureRenderSettings();
            BuildLighting();
            BuildWorldRoot();
            EnsureSceneAsset(scene);
            AddToBuildSettings();
        }

        private static void BuildWorldRoot()
        {
            GameObject root = new GameObject("HighwayWorld");

            HighwayVisualPool visualPool = root.AddComponent<HighwayVisualPool>();

            GameObject roadGo = new GameObject("HighwayRoad");
            roadGo.transform.SetParent(root.transform, false);
            HighwayRoadSegmentPool roadPool = roadGo.AddComponent<HighwayRoadSegmentPool>();
            SerializedObject roadPoolSo = new SerializedObject(roadPool);
            roadPoolSo.FindProperty("runtimeRoadSurface").objectReferenceValue = LoadMat("Highway_Asphalt_Gray.mat");
            roadPoolSo.FindProperty("runtimeLaneMark").objectReferenceValue = LoadMat("Highway_LaneMark_White.mat");
            roadPoolSo.FindProperty("runtimeShoulder").objectReferenceValue = LoadMat("Highway_Shoulder_Concrete.mat");
            roadPoolSo.FindProperty("runtimeEdgeMark").objectReferenceValue = LoadMat("Lantern_Gold.mat");
            roadPoolSo.ApplyModifiedPropertiesWithoutUndo();

            HighwayNatureStarterKitBaker.BakeVegetation();
            HighwayNaturePrefabLibrary natureLibrary = AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(
                "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset");

            GameObject sideGroundGo = new GameObject("HighwaySideGround");
            sideGroundGo.transform.SetParent(root.transform, false);
            Material grassGround = LoadMat("Highway_Grass_NSK2.mat");
            Material grassBladeA = LoadMat("Highway_GrassBlade01_NSK2.mat");
            Material grassBladeB = LoadMat("Highway_GrassBlade02_NSK2.mat");
            HighwaySideGroundPool sideGround = sideGroundGo.AddComponent<HighwaySideGroundPool>();
            SerializedObject sideGroundSo = new SerializedObject(sideGround);
            sideGroundSo.FindProperty("runtimeGrassGround").objectReferenceValue = grassGround;
            sideGroundSo.FindProperty("runtimeGrassBladeA").objectReferenceValue = grassBladeA;
            sideGroundSo.FindProperty("runtimeGrassBladeB").objectReferenceValue = grassBladeB;
            sideGroundSo.FindProperty("prefabLibrary").objectReferenceValue = natureLibrary;
            sideGroundSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject natureGo = new GameObject("HighwayNatureRoadside");
            natureGo.transform.SetParent(root.transform, false);
            natureGo.SetActive(false);
            HighwayNatureRoadsideSpawner natureSpawner = natureGo.AddComponent<HighwayNatureRoadsideSpawner>();

            HighwayUrbanPrefabLibrary urbanLibrary = HighwayUrbanPrefabLibraryFactory.BuildLibraryAsset();
            GameObject urbanGo = new GameObject("HighwayStaticUrbanNeighborhood");
            urbanGo.transform.SetParent(root.transform, false);
            urbanGo.SetActive(false);
            HighwayStaticUrbanNeighborhood urbanNeighborhood = urbanGo.AddComponent<HighwayStaticUrbanNeighborhood>();
            Material sidewalkMat = AssetDatabase.LoadAssetAtPath<Material>(HighwayUrbanAssets.SidewalkMaterialPath);
            SerializedObject urbanSo = new SerializedObject(urbanNeighborhood);
            urbanSo.FindProperty("sidewalkMaterial").objectReferenceValue = sidewalkMat;
            urbanSo.FindProperty("prefabLibrary").objectReferenceValue = urbanLibrary;
            urbanSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject windGo = new GameObject("HighwayWind");
            windGo.transform.SetParent(root.transform, false);
            WindZone wind = windGo.AddComponent<WindZone>();
            wind.mode = WindZoneMode.Directional;
            wind.windMain = 0.35f;
            wind.windTurbulence = 0.45f;

            GameObject horizonGo = new GameObject("HighwayHorizon");
            horizonGo.transform.SetParent(root.transform, false);
            horizonGo.SetActive(false);
            HighwayHorizonLayer horizon = horizonGo.AddComponent<HighwayHorizonLayer>();

            GameObject envGo = new GameObject("HighwayEnvironment");
            envGo.transform.SetParent(root.transform, false);
            envGo.SetActive(false);
            HighwayEnvironmentSpawner environment = envGo.AddComponent<HighwayEnvironmentSpawner>();

            GameObject playerGo = new GameObject("HighwayPlayer");
            playerGo.transform.SetParent(root.transform, false);
            playerGo.transform.position = new Vector3(
                HighwayPlayerLaneLayout.LaneCenterX(HighwayPlayerLaneLayout.MiddleLaneIndex),
                HighwayLaneSettings.RoadSurfaceY,
                HighwayLaneSettings.PlayerSpawnZ);
            HighwayPlayerVehicle player = playerGo.AddComponent<HighwayPlayerVehicle>();
            playerGo.AddComponent<HighwayPlayerLaneDebugMarkers>();

            GameObject cameraGo = new GameObject("HighwayCamera");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(root.transform, false);
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = HighwayFollowCamera.HighwaySkyColor;
            camera.farClipPlane = 320f;
            camera.fieldOfView = 72f;
            cameraGo.AddComponent<AudioListener>();
            FirstPersonCameraController firstPerson = cameraGo.AddComponent<FirstPersonCameraController>();
            firstPerson.Bind(playerGo.transform);
            firstPerson.EnsurePrimary();
            HighwayFollowCamera followCam = cameraGo.AddComponent<HighwayFollowCamera>();
            followCam.enabled = false;

            GameObject trafficGo = new GameObject("HighwayTraffic");
            trafficGo.transform.SetParent(root.transform, false);
            HighwayTrafficSpawner traffic = trafficGo.AddComponent<HighwayTrafficSpawner>();

            HighwayGameController controller = root.AddComponent<HighwayGameController>();
            HighwayCollisionFeedback collisionFeedback = controller.gameObject.AddComponent<HighwayCollisionFeedback>();
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("playerVehicle").objectReferenceValue = player;
            so.FindProperty("followCamera").objectReferenceValue = followCam;
            so.FindProperty("roadPool").objectReferenceValue = roadPool;
            so.FindProperty("sideGroundPool").objectReferenceValue = sideGround;
            so.FindProperty("natureRoadsideSpawner").objectReferenceValue = natureSpawner;
            so.FindProperty("staticUrbanNeighborhood").objectReferenceValue = urbanNeighborhood;
            so.FindProperty("environmentSpawner").objectReferenceValue = environment;
            so.FindProperty("horizonLayer").objectReferenceValue = horizon;
            so.FindProperty("trafficSpawner").objectReferenceValue = traffic;
            so.FindProperty("visualPool").objectReferenceValue = visualPool;
            so.FindProperty("collisionFeedback").objectReferenceValue = collisionFeedback;
            so.FindProperty("sunsetSkybox").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>(ChoTetMarketEnvironment.SunsetSkyboxAssetPath);
            so.FindProperty("playerVanPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(Level1KenneyCarKitPaths.PickupTrafficPrefab);

            Material[] bodyColors = LoadTrafficBodyColors();
            so.FindProperty("trafficBodyColors").arraySize = bodyColors.Length;
            for (int i = 0; i < bodyColors.Length; i++)
            {
                so.FindProperty("trafficBodyColors").GetArrayElementAtIndex(i).objectReferenceValue = bodyColors[i];
            }

            so.FindProperty("pickupBodyMaterial").objectReferenceValue = LoadMat("Accent_Red.mat");
            so.FindProperty("wheelMaterial").objectReferenceValue = LoadMat("Dark_Brown.mat");
            so.FindProperty("glassMaterial").objectReferenceValue = LoadMat("Glass_Blue.mat");
            so.FindProperty("kenneyColormapMaterial").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Material>(Level1KenneyCarKitPaths.ColormapMaterial);

            AssignPrefabGroup(so.FindProperty("sedanPrefabs"), new[]
            {
                Level1KenneyCarKitPaths.SedanPrefab,
                Level1KenneyCarKitPaths.SedanSportsPrefab,
                Level1KenneyCarKitPaths.HatchbackSportsPrefab,
                Level1KenneyCarKitPaths.SuvPrefab,
                Level1KenneyCarKitPaths.TaxiPrefab
            });
            AssignPrefabGroup(so.FindProperty("truckPrefabs"), new[]
            {
                Level1KenneyCarKitPaths.TruckPrefab,
                Level1KenneyCarKitPaths.TruckFlatPrefab,
                Level1KenneyCarKitPaths.DeliveryPrefab,
                Level1KenneyCarKitPaths.DeliveryFlatPrefab
            });
            AssignPrefabGroup(so.FindProperty("busPrefabs"), new[]
            {
                Level1KenneyCarKitPaths.DeliveryPrefab,
                Level1KenneyCarKitPaths.VanPrefab
            });
            AssignPrefabGroup(so.FindProperty("vanPrefabs"), new[]
            {
                Level1KenneyCarKitPaths.VanPrefab,
                Level1KenneyCarKitPaths.DeliveryPrefab,
                Level1KenneyCarKitPaths.SuvPrefab
            });
            so.ApplyModifiedPropertiesWithoutUndo();

            followCam.Bind(player.transform);
        }

        private static void AssignPrefabGroup(SerializedProperty arrayProp, string[] paths)
        {
            arrayProp.arraySize = paths.Length;
            for (int i = 0; i < paths.Length; i++)
            {
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            }
        }

        private static void BuildLighting()
        {
            GameObject sun = new GameObject("Directional Light");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.78f, 0.55f);
            light.intensity = 1.35f;
            sun.transform.rotation = Quaternion.Euler(28f, -42f, 0f);

            GameObject fill = new GameObject("Sunset Fill Light");
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.95f, 0.62f, 0.45f);
            fillLight.intensity = 0.55f;
            fill.transform.rotation = Quaternion.Euler(8f, 118f, 0f);
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.58f, 0.74f, 0.9f);
            RenderSettings.ambientIntensity = 1.1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = HighwayFollowCamera.HighwaySkyColor;
            RenderSettings.fogStartDistance = 75f;
            RenderSettings.fogEndDistance = 240f;
        }

        private static void EnsureSceneAsset(Scene scene)
        {
            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AddToBuildSettings()
        {
            ChoTetBuildSettings.ApplyMenuFlowScenesWithHighway();
        }

        private static Material LoadMat(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
        }

        private static Material[] LoadTrafficBodyColors()
        {
            string[] names =
            {
                "Accent_Red.mat", "Accent_Orange.mat", "Wood_Warm.mat", "Wall_Light.mat",
                "Garden_Ground.mat", "Glass_Blue.mat", "Dark_Brown.mat", "Foundation_Stone.mat"
            };

            Material[] mats = new Material[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                mats[i] = LoadMat(names[i]);
            }

            return mats;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
                string folder = System.IO.Path.GetFileName(path);
                if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                {
                    EnsureFolder(parent);
                }

                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
#endif
