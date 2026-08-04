using System.IO;
using ChoTet.Level1.Highway;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Editor
{
    /// <summary>NSK2 baked vegetation + unified grass ground; keeps urban clutter hidden.</summary>
    public static class Level1HighwaySceneCleanup
    {
        private const string LibraryPath = "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset";

        private static readonly string[] TemporaryHideNames =
        {
            "HighwayEnvironment",
            "HighwayHorizon",
            "HighwayNatureRoadside",
            "HighwaySideGround",
            "Environment"
        };

        public static void RepairCorruptHighwayScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1HighwaySceneCleanup] Exit Play mode before repairing the highway scene.");
                return;
            }

            Level1HighwaySceneBuilder.BuildHighwayScene();
            ApplyNatureRoadside();
            Debug.Log("[Level1HighwaySceneCleanup] Highway scene rebuilt from scratch.");
        }

        public static void ApplyNatureRoadside()
        {
            Level1HighwayCountrysideRebuild.RebuildCountrysideEnvironment();
        }

        public static void ApplyStaticUrbanNeighborhood()
        {
            Level1HighwayCountrysideRebuild.RebuildCountrysideEnvironment();
        }

        public static void CleanUpHighwayScene()
        {
            Level1HighwayCountrysideRebuild.RebuildCountrysideEnvironment();
        }

        private static void ApplyNatureRoadside_LegacyUrban()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1HighwaySceneCleanup] Exit Play mode before applying nature roadside.");
                return;
            }

            if (IsSceneFileCorrupt(Level1HighwaySceneBuilder.ScenePath))
            {
                Debug.LogError(
                    "[Level1HighwaySceneCleanup] Level1_Highway.unity is corrupt or truncated. "
                    + "Run Tools/Cho Tet/Repair Level 1 Highway Scene.");
                return;
            }

            Scene scene = EditorSceneManager.GetSceneByPath(Level1HighwaySceneBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(Level1HighwaySceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            HighwayNatureStarterKitBaker.BakeVegetation();
            HighwayUrbanPrefabLibrary urbanLibrary = HighwayUrbanPrefabLibraryFactory.BuildLibraryAsset();

            Material grassGround = HighwayNatureMaterialFactory.LoadOrCreate(
                HighwayNatureMaterialDefaults.GrassNearFile,
                "Assets/NatureStarterKit2/Textures/grass01.tga",
                new Color(0.42f, 0.58f, 0.34f),
                new Vector2(10f, 8f));
            Material grassBladeA = HighwayNatureMaterialFactory.LoadOrCreateGrassBlade(
                HighwayNatureMaterialDefaults.GrassBlade01File,
                "Assets/NatureStarterKit2/Textures/grass01.tga",
                Color.white);
            Material grassBladeB = HighwayNatureMaterialFactory.LoadOrCreateGrassBlade(
                HighwayNatureMaterialDefaults.GrassBlade02File,
                "Assets/NatureStarterKit2/Textures/grass02.tga",
                new Color(0.92f, 1f, 0.88f));
            Material shoulder = HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.ShoulderFile);
            Material asphalt = HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.RoadSurfaceFile);
            Material laneMark = HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.LaneMarkFile);
            Material edgeMark = HighwayRoadMaterialDefaults.Load(HighwayRoadMaterialDefaults.EdgeMarkFile);

            HighwayNaturePrefabLibrary library =
                AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(LibraryPath);

            HideTemporaryObjects();
            DisableGrassRoadside();
            RemoveLegacySidewalkStrips();
            EnsureWindZone();
            Material sidewalk = HighwaySidewalkMaterialUtility.Resolve(
                AssetDatabase.LoadAssetAtPath<QuaterniusDowntownPrefabLibrary>(QuaterniusDowntownAssets.LibraryPath));
            if (sidewalk == null)
            {
                sidewalk = AssetDatabase.LoadAssetAtPath<Material>(HighwayUrbanAssets.SidewalkMaterialPath);
            }
            RoadsideTownPaletteFactory.BuildPalette();
            QuaterniusDowntownPrefabBaker.BuildKit();
            RebuildSideGround(grassGround, grassBladeA, grassBladeB, library);
            RebuildRoad(asphalt, laneMark, shoulder, edgeMark);
            PurgeDuplicateRoadSegments();
            EnsureStaticUrbanNeighborhood(sidewalk, urbanLibrary);
            EnsureQuaterniusTownBlockPool(urbanLibrary);
            EnsureQuaterniusSidewalkPool();
            WireGameController();
            StripRuntimePoolChildren();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Level1HighwaySceneCleanup] Urban highway visuals applied — unified Quaternius concrete sidewalks.");
        }

        private static void HideTemporaryObjects()
        {
            for (int i = 0; i < TemporaryHideNames.Length; i++)
            {
                GameObject root = GameObject.Find(TemporaryHideNames[i]);
                if (root != null)
                {
                    root.SetActive(false);
                }
            }
        }

        private static void DisableGrassRoadside()
        {
            GameObject sideGround = GameObject.Find("HighwaySideGround");
            if (sideGround != null)
            {
                sideGround.SetActive(false);
            }

            HighwayNatureRoadsideSpawner natureSpawner =
                Object.FindFirstObjectByType<HighwayNatureRoadsideSpawner>();
            if (natureSpawner != null)
            {
                natureSpawner.gameObject.SetActive(false);
            }

            HighwayEnvironmentSpawner envSpawner = Object.FindFirstObjectByType<HighwayEnvironmentSpawner>();
            if (envSpawner != null)
            {
                envSpawner.gameObject.SetActive(false);
            }
        }

        private static void PurgeDuplicateRoadSegments()
        {
            HighwayRoadSegmentPool pool = Object.FindFirstObjectByType<HighwayRoadSegmentPool>();
            if (pool == null)
            {
                return;
            }

            pool.RebuildRoadContent();
        }

        private static void RemoveLegacySidewalkStrips()
        {
            GameObject sideGround = GameObject.Find("HighwaySideGround");
            if (sideGround == null)
            {
                return;
            }

            Transform[] all = sideGround.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t == null || t == sideGround.transform)
                {
                    continue;
                }

                if (t.name == "Sidewalk" || t.name == "Grass")
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
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

            WindZone wind = windGo.GetComponent<WindZone>();
            if (wind == null)
            {
                wind = windGo.AddComponent<WindZone>();
            }
            wind.mode = WindZoneMode.Directional;
            wind.windMain = 0.35f;
            wind.windTurbulence = 0.45f;
            wind.windPulseMagnitude = 0.15f;
            wind.windPulseFrequency = 0.25f;
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
                Debug.LogWarning("[Level1HighwaySceneCleanup] HighwaySideGroundPool not found.");
                return;
            }

            SerializedObject so = new SerializedObject(pool);
            so.FindProperty("runtimeGrassGround").objectReferenceValue = grassGround;
            so.FindProperty("runtimeGrassBladeA").objectReferenceValue = grassBladeA;
            so.FindProperty("runtimeGrassBladeB").objectReferenceValue = grassBladeB;
            so.FindProperty("prefabLibrary").objectReferenceValue = library;
            so.ApplyModifiedPropertiesWithoutUndo();
            pool.RebuildSideGroundContent();
        }

        private static void RebuildRoad(Material asphalt, Material laneMark, Material shoulder, Material edgeMark)
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
            so.ApplyModifiedPropertiesWithoutUndo();
            pool.RebuildRoadContent();
        }

        private static void EnsureStaticUrbanNeighborhood(Material sidewalk, HighwayUrbanPrefabLibrary library)
        {
            GameObject world = GameObject.Find("HighwayWorld");
            HighwayStaticUrbanNeighborhood neighborhood =
                Object.FindFirstObjectByType<HighwayStaticUrbanNeighborhood>();
            if (neighborhood == null)
            {
                GameObject urbanGo = new GameObject("HighwayStaticUrbanNeighborhood");
                if (world != null)
                {
                    urbanGo.transform.SetParent(world.transform, false);
                }

                neighborhood = urbanGo.AddComponent<HighwayStaticUrbanNeighborhood>();
            }

            neighborhood.gameObject.SetActive(true);
            SerializedObject so = new SerializedObject(neighborhood);
            so.FindProperty("sidewalkMaterial").objectReferenceValue = sidewalk;
            so.FindProperty("prefabLibrary").objectReferenceValue = library;
            so.ApplyModifiedPropertiesWithoutUndo();
            neighborhood.RebuildNeighborhood();
        }

        private static void EnsureQuaterniusTownBlockPool(HighwayUrbanPrefabLibrary urbanFallback)
        {
            GameObject world = GameObject.Find("HighwayWorld");
            HighwayQuaterniusTownBlockPool pool = Object.FindFirstObjectByType<HighwayQuaterniusTownBlockPool>();
            if (pool == null)
            {
                GameObject poolGo = new GameObject("HighwayQuaterniusTownBlockPool");
                if (world != null)
                {
                    poolGo.transform.SetParent(world.transform, false);
                }

                pool = poolGo.AddComponent<HighwayQuaterniusTownBlockPool>();
            }

            QuaterniusDowntownPrefabLibrary downtownLibrary =
                AssetDatabase.LoadAssetAtPath<QuaterniusDowntownPrefabLibrary>(QuaterniusDowntownAssets.LibraryPath);

            pool.gameObject.SetActive(true);
            pool.Initialize(downtownLibrary);
        }

        private static void EnsureQuaterniusSidewalkPool()
        {
            GameObject world = GameObject.Find("HighwayWorld");
            HighwayQuaterniusSidewalkPool pool = Object.FindFirstObjectByType<HighwayQuaterniusSidewalkPool>();
            if (pool == null)
            {
                GameObject poolGo = new GameObject("HighwayQuaterniusSidewalkPool");
                if (world != null)
                {
                    poolGo.transform.SetParent(world.transform, false);
                }

                pool = poolGo.AddComponent<HighwayQuaterniusSidewalkPool>();
            }

            QuaterniusDowntownPrefabLibrary downtownLibrary =
                AssetDatabase.LoadAssetAtPath<QuaterniusDowntownPrefabLibrary>(QuaterniusDowntownAssets.LibraryPath);

            pool.gameObject.SetActive(true);
            pool.Initialize(downtownLibrary);
        }

        private static void WireGameController()
        {
            HighwayGameController controller = Object.FindFirstObjectByType<HighwayGameController>();
            HighwayStaticUrbanNeighborhood neighborhood =
                Object.FindFirstObjectByType<HighwayStaticUrbanNeighborhood>();
            HighwayQuaterniusTownBlockPool quaterniusPool =
                Object.FindFirstObjectByType<HighwayQuaterniusTownBlockPool>();
            HighwayQuaterniusSidewalkPool sidewalkPool =
                Object.FindFirstObjectByType<HighwayQuaterniusSidewalkPool>();
            if (controller == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(controller);
            if (neighborhood != null)
            {
                so.FindProperty("staticUrbanNeighborhood").objectReferenceValue = neighborhood;
            }

            if (quaterniusPool != null)
            {
                so.FindProperty("quaterniusTownBlockPool").objectReferenceValue = quaterniusPool;
            }

            if (sidewalkPool != null)
            {
                so.FindProperty("quaterniusSidewalkPool").objectReferenceValue = sidewalkPool;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool IsSceneFileCorrupt(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                return true;
            }

            FileInfo info = new FileInfo(scenePath);
            if (info.Length == 8388608)
            {
                return true;
            }

            string[] lines = File.ReadAllLines(scenePath);
            if (lines.Length == 0)
            {
                return true;
            }

            string last = lines[lines.Length - 1].TrimEnd();
            if (last.StartsWith("m_Icon: {fileID:", System.StringComparison.Ordinal) && !last.EndsWith("}"))
            {
                return true;
            }

            bool hasSceneRoots = false;
            for (int i = lines.Length - 1; i >= Mathf.Max(0, lines.Length - 12); i--)
            {
                if (lines[i].StartsWith("SceneRoots:", System.StringComparison.Ordinal))
                {
                    hasSceneRoots = true;
                    break;
                }
            }

            return !hasSceneRoots;
        }

        private static void StripRuntimePoolChildren()
        {
            string[] sideRoots =
            {
                "LeftQuaterniusSidewalks",
                "RightQuaterniusSidewalks",
                "LeftQuaterniusBlocks",
                "RightQuaterniusBlocks"
            };

            for (int i = 0; i < sideRoots.Length; i++)
            {
                GameObject root = GameObject.Find(sideRoots[i]);
                if (root == null)
                {
                    continue;
                }

                Transform transform = root.transform;
                for (int c = transform.childCount - 1; c >= 0; c--)
                {
                    Object.DestroyImmediate(transform.GetChild(c).gameObject);
                }
            }
        }
    }
}
