#if UNITY_EDITOR
using System.IO;
using ChoTet.Level1;
using ChoTet.Level1.Highway;
using ChoTet.Level1.HometownArrival;
using ChoTet.Menu.Editor;
using ChoTet.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1.Editor
{
    public static class HometownArrivalSceneBuilder
    {
        private const string MarketWorldSourcePath = "Assets/Scenes/ChoTet_VerticalSlice.unity";
        private const string ArrivalScenePath = "Assets/Scenes/Level1_HometownArrival.unity";
        private const string NaturePrefabs = "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs";
        private const string OccaPrefabs = "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs";
        private const string HighwayNaturePrefabs = "Assets/Prefabs/Highway/NatureBaked";

        private const float DropOffZ = 30f;
        private static readonly float BusLaneX =
            HighwayPlayerLaneLayout.LaneCenterX(HighwayPlayerLaneLayout.MiddleLaneIndex);
        private const float BusGroundY = HighwayLaneSettings.RoadSurfaceY;
        private const float BusStartZ = 86f;

        private static void OpenScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(ArrivalScenePath);
            }
        }

        public static void BakeArrivalFrame()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[HometownArrival] Exit Play mode before baking.");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            Scene scene = EditorSceneManager.OpenScene(ArrivalScenePath, OpenSceneMode.Single);
            ApplyArrivalFrame();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[HometownArrival] Arrival frame baked — road, greenery, camera, markers.");
        }

        private static void CloneMarketWorldToHometownArrival()
        {
            if (!EditorUtility.DisplayDialog(
                    "Clone market world",
                    "Copy ChoTet_VerticalSlice.unity over Level1_HometownArrival.unity?\n\n" +
                    "This replaces the arrival scene with a fresh market-world clone (market disabled at runtime).",
                    "Clone",
                    "Cancel"))
            {
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string source = Path.Combine(projectRoot, MarketWorldSourcePath.Replace('/', Path.DirectorySeparatorChar));
            string destination = Path.Combine(projectRoot, ArrivalScenePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(source))
            {
                EditorUtility.DisplayDialog("Clone failed", "Source scene not found:\n" + MarketWorldSourcePath, "OK");
                return;
            }

            File.Copy(source, destination, true);
            AssetDatabase.Refresh();
            ChoTetBuildSettings.ApplyMenuFlowScenesWithHighway(includeHighway: true);
            EditorSceneManager.OpenScene(ArrivalScenePath, OpenSceneMode.Single);
            ApplyArrivalFrame();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[HometownArrival] Cloned market world and baked Level 1 arrival frame.");
        }

        private static void RefreshBuildSettings()
        {
            ChoTetBuildSettings.ApplyMenuFlowScenesWithHighway(includeHighway: true);
            Debug.Log("[ChoTet] Build settings refreshed with Level1_HometownArrival.");
        }

        private static void ApplyArrivalFrame()
        {
            Transform world = GameObject.Find("ChoTetWorld")?.transform;
            if (world == null)
            {
                Debug.LogError("[HometownArrival] ChoTetWorld not found.");
                return;
            }

            SetInactive("MarketArea");
            SetInactive("StoryActors");
            SetInactive("ZoneTriggers");
            SetInactive("TrafficRoot");
            SetInactive("MainGround");
            SetInactive("GardenGround");
            SetInactive("GardenArea");
            SetInactive("HomeArea");
            SetActive("HomeGround", true);

            EnsureMarker(world, "ArrivalSpawnPoint", new Vector3(
                HometownArrivalLayout.ParkedRoadsideX + 0.35f,
                BusGroundY + 0.15f,
                DropOffZ - 0.2f));
            EnsureMarker(world, "PickupBus", new Vector3(BusLaneX, BusGroundY, BusStartZ));
            ClearPickupBusVisual();
            SetInactive("HouseEntranceTrigger");
            SetInactive("SideWalkLeft");
            SetInactive("SideWalkRight");
            SetInactive("RoadArea");
            SetInactive("GardenGround");
            SetInactive("HomeGrassBlend");
            TrimMainPathInScene();
            RemoveArrivalDressingInScene(world);

            WireSystems();
            TuneCamera();
        }

        private static void TrimMainPathInScene()
        {
            Transform mainPath = GameObject.Find("MainPath")?.transform;
            if (mainPath == null)
            {
                return;
            }

            mainPath.localPosition = new Vector3(0f, -0.42f, 12f);
            mainPath.localScale = new Vector3(6f, 0.15f, 26f);
            mainPath.gameObject.SetActive(false);
        }

        private static void RemoveArrivalDressingInScene(Transform world)
        {
            Transform dressing = world.Find("ArrivalDressing");
            if (dressing != null)
            {
                Object.DestroyImmediate(dressing.gameObject);
            }
        }

        private static void WireSystems()
        {
            GameObject systems = GameObject.Find("Systems") ?? new GameObject("Systems");

            HometownArrivalBootstrap bootstrap = systems.GetComponent<HometownArrivalBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = systems.AddComponent<HometownArrivalBootstrap>();
            }

            SerializedObject bootstrapSerialized = new SerializedObject(bootstrap);
            SetPrefab(bootstrapSerialized, "runtimePlayerVisualPrefab",
                PlayerCharacterVisualPaths.CreativeCharacterVisualPrefab);
            SetPrefab(bootstrapSerialized, "runtimeFatherVisualPrefab",
                PlayerCharacterVisualPaths.FatherVisualPrefab);
            SetPrefab(bootstrapSerialized, "runtimeMotherVisualPrefab",
                PlayerCharacterVisualPaths.MotherVisualPrefab);
            SetPrefab(bootstrapSerialized, "runtimeParentsCarPrefab", Level1KenneyCarKitPaths.SedanPrefab);
            bootstrapSerialized.ApplyModifiedPropertiesWithoutUndo();

            HometownArrivalController controller = systems.GetComponent<HometownArrivalController>();
            if (controller == null)
            {
                controller = systems.AddComponent<HometownArrivalController>();
            }

            if (systems.GetComponent<Level1ScreenFade>() == null)
            {
                systems.AddComponent<Level1ScreenFade>();
            }

            HometownArrivalEnvironment environment = systems.GetComponent<HometownArrivalEnvironment>();
            if (environment == null)
            {
                environment = systems.AddComponent<HometownArrivalEnvironment>();
            }

            SerializedObject envSerialized = new SerializedObject(environment);
            SetPrefab(envSerialized, "daySkybox", ChoTetMarketEnvironment.HometownDaySkyboxAssetPath);
            SetPrefab(envSerialized, "eveningSkybox", ChoTetMarketEnvironment.SunsetSkyboxAssetPath);
            SetFloat(envSerialized, "fogStart", 55f);
            SetFloat(envSerialized, "fogEnd", 145f);
            envSerialized.ApplyModifiedPropertiesWithoutUndo();

            ApplySceneSkyAndFog();

            SerializedObject controllerSerialized = new SerializedObject(controller);
            SetFloat(controllerSerialized, "busLaneX", BusLaneX);
            SetFloat(controllerSerialized, "busStartZ", BusStartZ);
            SetFloat(controllerSerialized, "busStopZ", DropOffZ);
            SetFloat(controllerSerialized, "busGroundY", BusGroundY);
            SetBool(controllerSerialized, "loadSceneAfterDropOff", true);
            SetString(controllerSerialized, "completionTitle", "Về tới quê rồi — nhà ngay phía trước.");
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TuneCamera()
        {
            Camera camera = Camera.main ?? Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                return;
            }

            PlayerTopDownController player = Object.FindAnyObjectByType<PlayerTopDownController>();
            FirstPersonCameraController firstPerson = camera.GetComponent<FirstPersonCameraController>();
            if (firstPerson == null)
            {
                firstPerson = camera.gameObject.AddComponent<FirstPersonCameraController>();
            }

            if (player != null)
            {
                firstPerson.Bind(player.transform);
            }

            firstPerson.EnsurePrimary();

            TopDownCameraFollow follow = camera.GetComponent<TopDownCameraFollow>();
            if (follow != null)
            {
                follow.enabled = false;
            }

            camera.fieldOfView = 72f;
            camera.farClipPlane = 120f;
            camera.clearFlags = CameraClearFlags.Skybox;
        }

        private static void ApplySceneSkyAndFog()
        {
            Material daySky = AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.HometownDaySkyboxAssetPath);
            ChoTetMarketEnvironment.ApplyHometownArrivalRenderSettings(daySky, fogStart: 55f, fogEnd: 145f);
        }

        private static void ClearPickupBusVisual()
        {
            Transform bus = GameObject.Find("PickupBus")?.transform;
            if (bus == null)
            {
                return;
            }

            for (int i = bus.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(bus.GetChild(i).gameObject);
            }
        }

        private static void EnsureMarker(Transform parent, string name, Vector3 position)
        {
            Transform marker = GameObject.Find(name)?.transform;
            if (marker == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent, false);
                marker = go.transform;
            }

            marker.position = position;
        }

        private static void ScatterPrefab(Transform parent, string path, string prefix, Vector3[] positions,
            float scale, float yaw = 0f)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                instance.name = prefix + "_" + i;
                instance.transform.position = positions[i];
                instance.transform.rotation = Quaternion.Euler(0f, yaw + (i * 37f) % 360f, 0f);
                instance.transform.localScale = Vector3.one * (scale + (i % 3) * 0.04f);
                StripColliders(instance);
            }
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void SetInactive(string objectName)
        {
            GameObject target = GameObject.Find(objectName);
            if (target != null)
            {
                target.SetActive(false);
            }
        }

        private static void SetActive(string objectName, bool active)
        {
            GameObject target = GameObject.Find(objectName);
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void AssignPrefabArray(SerializedObject serialized, string propertyName, string[] paths)
        {
            SerializedProperty array = serialized.FindProperty(propertyName);
            if (array == null || !array.isArray)
            {
                return;
            }

            array.arraySize = paths.Length;
            for (int i = 0; i < paths.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
            }
        }

        private static void SetPrefab(SerializedObject serialized, string propertyName, string path)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetBool(SerializedObject serialized, string propertyName, bool value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetVector3(SerializedObject serialized, string propertyName, Vector3 value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }
    }
}
#endif
