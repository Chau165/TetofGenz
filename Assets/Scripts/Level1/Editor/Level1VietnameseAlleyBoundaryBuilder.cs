#if UNITY_EDITOR
using System.Collections.Generic;
using ChoTet.Level1;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Continuous alley boundary walls placed behind amenity rows — blocks void views without overlapping gameplay props.
    /// </summary>
    public static class Level1VietnameseAlleyBoundaryBuilder
    {
        public const string RootName = "Level1_VietnameseAlleyBoundary";

        private const float AlleyStartZ = -4.25f;
        private const float AlleyEndZ = -25.8f;
        private const float WallSetback = 2.15f;
        private const float VoidScreenSetback = 0.75f;
        private const float WallThickness = 0.22f;
        private const float FencePanelThickness = 0.05f;
        private const float PostSize = 0.09f;
        private const float PostSpacing = 2.85f;
        private const float WallHeight = 3.35f;
        private const float GateWidth = 1.15f;
        private const float GateCenterZ = -13.85f;
        private const float SkylineSetback = 2.8f;
        private const int LayoutSeed = 20260710;

        private static readonly string[] WestSigns =
        {
            "CẤM ĐỔ RÁC",
            "CẤM TIỂU BẬY",
            "KHU PHỐ VĂN HÓA",
            "ĐIỆN LỰC TP.HCM"
        };

        private static readonly string[] GraffitiLines =
        {
            "KHOAN CẮT BÊ TÔNG",
            "HÚT HẦM CẦU",
            "SỬA ĐIỆN NƯỚC",
            "THÔNG CỐNG NGHẸT",
            "0903 812 456",
            "0937 44 22 18"
        };

        private static readonly string[] ExcludedRoots =
        {
            RootName,
            "PerimeterEnclosure",
            "Ground",
            "TravelRoad",
            "Level1_MainRoad",
            "Level1_BusPickupArea",
            "OrbitViewScreening",
            "OutdoorLighting",
            "OutdoorSun",
            "Level1_OutdoorPathBoundaries",
            "Level1_OutdoorWalkColliders",
            "OutdoorWalkCollider",
            "Level1_OutdoorCrowd",
            "Level1_OutdoorSpawn",
            "Level1_HouseExterior",
            "Level1_VietnameseAlleyGround",
            "Level1_AlleyEntranceSign"
        };

        public static void RebuildOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseAlleyBoundaryBuilder] Exit Play mode before rebuilding boundaries.");
                return;
            }

            Transform outdoor = GameObject.Find("Level1_OutdoorArea")?.transform;
            if (outdoor == null)
            {
                Debug.LogError("[Level1VietnameseAlleyBoundaryBuilder] Level1_OutdoorArea not found.");
                return;
            }

            Rebuild(outdoor);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnameseAlleyBoundaryBuilder] Vietnamese alley boundary walls rebuilt.");
        }

        public static void Rebuild(Transform outdoorRoot)
        {
            Level1VietnameseAlleyBoundaryMaterialBuilder.BakeOrUpdateMaterials();

            Material plaster = AssetDatabase.LoadAssetAtPath<Material>(
                Level1VietnameseAlleyBoundaryMaterialBuilder.PlasterMaterialPath);
            Material fence = AssetDatabase.LoadAssetAtPath<Material>(
                Level1VietnameseAlleyBoundaryMaterialBuilder.FenceMaterialPath);
            Material postMat = AssetDatabase.LoadAssetAtPath<Material>(
                Level1VietnameseAlleyBoundaryMaterialBuilder.PostMaterialPath);
            Material pipeMat = AssetDatabase.LoadAssetAtPath<Material>(
                Level1VietnameseAlleyBoundaryMaterialBuilder.PipeMaterialPath);

            if (plaster == null || fence == null || postMat == null)
            {
                Debug.LogError("[Level1VietnameseAlleyBoundaryBuilder] Missing boundary materials.");
                return;
            }

            RemoveExisting(outdoorRoot);
            ComputeAmenityExtents(outdoorRoot, out float westMinX, out float eastMaxX, out float amenityMinZ,
                out float amenityMaxZ);

            float wallStartZ = Mathf.Min(AlleyStartZ - 0.25f, amenityMaxZ + 0.35f);
            float wallEndZ = Mathf.Max(AlleyEndZ - 0.25f, amenityMinZ - 0.35f);
            float wallDepthZ = wallStartZ - wallEndZ;
            float wallCenterZ = (wallStartZ + wallEndZ) * 0.5f;

            float westWallX = westMinX - WallSetback;
            float eastFenceX = eastMaxX + WallSetback;

            GameObject root = new GameObject(RootName);
            root.transform.SetParent(outdoorRoot, false);

            GameObject westRoot = new GameObject("Boundary_WestPlaster");
            westRoot.transform.SetParent(root.transform, false);
            GameObject eastRoot = new GameObject("Boundary_EastFence");
            eastRoot.transform.SetParent(root.transform, false);
            GameObject voidRoot = new GameObject("Boundary_VoidScreen");
            voidRoot.transform.SetParent(root.transform, false);
            GameObject decalRoot = new GameObject("Boundary_Decals");
            decalRoot.transform.SetParent(root.transform, false);
            GameObject skylineRoot = new GameObject("Boundary_Skyline");
            skylineRoot.transform.SetParent(root.transform, false);

            System.Random rng = new System.Random(LayoutSeed);

            BuildContinuousWestWall(westRoot.transform, plaster, pipeMat, westWallX, wallCenterZ, wallDepthZ, rng);
            BuildContinuousEastFence(eastRoot.transform, fence, postMat, eastFenceX, wallStartZ, wallEndZ, rng);
            BuildVoidScreens(voidRoot.transform, plaster, fence, westMinX, eastMaxX, wallCenterZ, wallDepthZ);
            BuildSouthCap(root.transform, plaster, westWallX, eastFenceX, wallEndZ);
            BuildDecals(decalRoot.transform, westWallX, wallCenterZ, wallDepthZ, rng);
            BuildSkyline(skylineRoot.transform, westWallX, eastFenceX, wallCenterZ, wallDepthZ, rng);

            MarkStatic(root);
            Debug.Log($"[Level1VietnameseAlleyBoundaryBuilder] West wall X={westWallX:F2}, East fence X={eastFenceX:F2}, " +
                      $"Z span {wallEndZ:F2}..{wallStartZ:F2} (amenity X {westMinX:F2}..{eastMaxX:F2}).");
        }

        private static void ComputeAmenityExtents(Transform outdoorRoot, out float westMinX, out float eastMaxX,
            out float amenityMinZ, out float amenityMaxZ)
        {
            westMinX = -6.5f;
            eastMaxX = 6.5f;
            amenityMinZ = AlleyEndZ;
            amenityMaxZ = AlleyStartZ;

            float scanZMin = AlleyEndZ - 1.0f;
            float scanZMax = AlleyStartZ + 1.0f;
            bool found = false;

            Renderer[] renderers = outdoorRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (IsExcludedFromExtentScan(renderer.transform))
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;
                if (bounds.size.sqrMagnitude < 0.02f)
                {
                    continue;
                }

                if (bounds.max.z < scanZMin || bounds.min.z > scanZMax)
                {
                    continue;
                }

                if (bounds.max.x < -2.6f)
                {
                    westMinX = Mathf.Min(westMinX, bounds.min.x);
                    found = true;
                }

                if (bounds.min.x > 2.6f)
                {
                    eastMaxX = Mathf.Max(eastMaxX, bounds.max.x);
                    found = true;
                }

                if (bounds.max.x < -2.6f || bounds.min.x > 2.6f)
                {
                    amenityMinZ = Mathf.Min(amenityMinZ, bounds.min.z);
                    amenityMaxZ = Mathf.Max(amenityMaxZ, bounds.max.z);
                }
            }

            if (!found)
            {
                westMinX = -10.8f;
                eastMaxX = 10.8f;
            }
        }

        private static bool IsExcludedFromExtentScan(Transform t)
        {
            Transform current = t;
            while (current != null)
            {
                string name = current.name;
                for (int i = 0; i < ExcludedRoots.Length; i++)
                {
                    if (name == ExcludedRoots[i])
                    {
                        return true;
                    }
                }

                if (name.StartsWith("Backdrop") || name.StartsWith("EdgeCliff") || name.StartsWith("EdgeRock"))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static void BuildContinuousWestWall(Transform parent, Material plaster, Material pipeMat, float wallX,
            float centerZ, float depthZ, System.Random rng)
        {
            CreateWallSegment(parent, "WestPlaster_Wall", wallX, centerZ, WallThickness, WallHeight, depthZ, plaster,
                Level1VietnameseAlleyBoundaryMaterialBuilder.PlasterSheetMeters);

            int pipeCount = Mathf.Max(2, Mathf.FloorToInt(depthZ / 5.5f));
            for (int i = 0; i < pipeCount; i++)
            {
                float t = (i + 0.5f) / pipeCount;
                float z = centerZ + (0.5f - t) * depthZ;
                float y = 0.55f + (float)rng.NextDouble() * 1.6f;
                CreateCube("West_Pipe_" + i, parent,
                    new Vector3(wallX + WallThickness * 0.5f + 0.05f, Level1OutdoorSettings.WalkSurfaceY + y, z),
                    new Vector3(0.05f, 0.05f, 0.35f + (float)rng.NextDouble() * 0.35f), pipeMat);
            }

            for (int i = 0; i < 2; i++)
            {
                float z = centerZ + ((i == 0) ? -0.22f : 0.22f) * depthZ;
                CreateCube("West_Vent_" + i, parent,
                    new Vector3(wallX + WallThickness * 0.5f + 0.03f, Level1OutdoorSettings.WalkSurfaceY + 1.35f, z),
                    new Vector3(0.03f, 0.2f, 0.3f), pipeMat);
            }
        }

        private static void BuildContinuousEastFence(Transform parent, Material fence, Material postMat, float fenceX,
            float startZ, float endZ, System.Random rng)
        {
            float gateHalf = GateWidth * 0.5f;
            float gateFrontZ = GateCenterZ + gateHalf;
            float gateBackZ = GateCenterZ - gateHalf;

            BuildFenceRun(parent, fence, postMat, fenceX, startZ, gateFrontZ, "FenceRun_N", rng, 0);
            BuildGate(parent, fenceX, GateCenterZ, postMat, fence);
            BuildFenceRun(parent, fence, postMat, fenceX, gateBackZ, endZ, "FenceRun_S", rng, 1);
        }

        private static void BuildFenceRun(Transform parent, Material fence, Material postMat, float fenceX,
            float runStartZ, float runEndZ, string prefix, System.Random rng, int seedOffset)
        {
            if (runEndZ >= runStartZ - 0.2f)
            {
                return;
            }

            float depthZ = runStartZ - runEndZ;
            float centerZ = (runStartZ + runEndZ) * 0.5f;
            CreateWallSegment(parent, prefix + "_Panel", fenceX, centerZ, FencePanelThickness, WallHeight - 0.06f,
                depthZ, fence, Level1VietnameseAlleyBoundaryMaterialBuilder.FenceSheetMeters);

            int postCount = Mathf.Max(2, Mathf.CeilToInt(depthZ / PostSpacing) + 1);
            for (int i = 0; i < postCount; i++)
            {
                float t = i / (float)(postCount - 1);
                float z = Mathf.Lerp(runStartZ, runEndZ, t);
                float wobbleX = ((float)rng.NextDouble() - 0.5f) * 0.04f;
                CreateCube(prefix + "_Post_" + i, parent,
                    new Vector3(fenceX + wobbleX, Level1OutdoorSettings.WalkSurfaceY + WallHeight * 0.5f, z),
                    new Vector3(PostSize, WallHeight, PostSize), postMat);
            }
        }

        private static void BuildGate(Transform parent, float fenceX, float gateZ, Material postMat, Material fence)
        {
            GameObject gateRoot = new GameObject("FenceGate");
            gateRoot.transform.SetParent(parent, false);
            gateRoot.transform.position = new Vector3(fenceX, 0f, gateZ);

            float postHeight = WallHeight + 0.06f;
            CreateLocalCube("GatePost_L", gateRoot.transform,
                new Vector3(0f, Level1OutdoorSettings.WalkSurfaceY + postHeight * 0.5f, GateWidth * 0.5f),
                new Vector3(PostSize * 1.25f, postHeight, PostSize * 1.25f), postMat);
            CreateLocalCube("GatePost_R", gateRoot.transform,
                new Vector3(0f, Level1OutdoorSettings.WalkSurfaceY + postHeight * 0.5f, -GateWidth * 0.5f),
                new Vector3(PostSize * 1.25f, postHeight, PostSize * 1.25f), postMat);
            CreateLocalCube("GatePanel", gateRoot.transform,
                new Vector3(-0.04f, Level1OutdoorSettings.WalkSurfaceY + WallHeight * 0.48f, 0f),
                new Vector3(FencePanelThickness, WallHeight * 0.9f, GateWidth * 0.9f), fence);
            CreateLocalCube("GateLatch", gateRoot.transform,
                new Vector3(-0.07f, Level1OutdoorSettings.WalkSurfaceY + WallHeight * 0.45f, GateWidth * 0.36f),
                new Vector3(0.04f, 0.08f, 0.05f), postMat);
        }

        private static void BuildVoidScreens(Transform parent, Material plaster, Material fence, float westMinX,
            float eastMaxX, float centerZ, float depthZ)
        {
            float westScreenX = westMinX - VoidScreenSetback;
            float eastScreenX = eastMaxX + VoidScreenSetback;
            float screenHeight = WallHeight + 0.45f;
            CreateWallSegment(parent, "VoidScreen_West", westScreenX, centerZ, WallThickness * 0.85f, screenHeight,
                depthZ, plaster, Level1VietnameseAlleyBoundaryMaterialBuilder.PlasterSheetMeters);
            CreateWallSegment(parent, "VoidScreen_East", eastScreenX, centerZ, FencePanelThickness + 0.02f,
                screenHeight, depthZ, fence, Level1VietnameseAlleyBoundaryMaterialBuilder.FenceSheetMeters);
        }

        private static void BuildSouthCap(Transform parent, Material plaster, float westX, float eastX, float capZ)
        {
            float spanX = eastX - westX;
            float centerX = (eastX + westX) * 0.5f;
            CreateWallSegment(parent, "Boundary_SouthCap", centerX, capZ, spanX, WallHeight + 0.15f,
                WallThickness + 0.06f, plaster, Level1VietnameseAlleyBoundaryMaterialBuilder.PlasterSheetMeters,
                alongX: true);
        }

        private static void BuildDecals(Transform parent, float faceX, float centerZ, float depthZ, System.Random rng)
        {
            float face = faceX + WallThickness * 0.5f + 0.02f;
            for (int i = 0; i < WestSigns.Length; i++)
            {
                float t = (i + 0.55f) / (WestSigns.Length + 0.5f);
                float z = centerZ + (0.5f - t) * depthZ * 0.85f;
                float y = 1.35f + (float)rng.NextDouble() * 0.85f;
                CreateWallLabel(parent, "Sign_" + i, WestSigns[i], face, y, z,
                    0.11f + (float)rng.NextDouble() * 0.02f, new Color(0.82f, 0.18f, 0.14f, 0.92f), FontStyles.Bold,
                    rng, official: true);
            }

            for (int i = 0; i < 5; i++)
            {
                float t = (i + 1f) / 6f;
                float z = centerZ + (0.5f - t) * depthZ;
                float y = 0.8f + (float)rng.NextDouble() * 1.2f;
                string line = GraffitiLines[(i + rng.Next(0, GraffitiLines.Length)) % GraffitiLines.Length];
                float fade = 0.45f + (float)rng.NextDouble() * 0.3f;
                CreateWallLabel(parent, "Graffiti_" + i, line, face, y, z,
                    0.08f + (float)rng.NextDouble() * 0.04f,
                    new Color(0.12f, 0.12f, 0.12f, fade), FontStyles.Normal, rng, official: false);
            }
        }

        private static void BuildSkyline(Transform parent, float westWallX, float eastFenceX, float centerZ,
            float depthZ, System.Random rng)
        {
            Material roof = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/ChoTet/Materials/Dark_Brown.mat");
            Material tank = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/ChoTet/Materials/Wood_Warm.mat");
            if (roof == null || tank == null)
            {
                return;
            }

            float skylineWestX = westWallX - SkylineSetback;
            float skylineEastX = eastFenceX + SkylineSetback;
            float[] skylineZ = { -7.2f, -12.5f, -17.6f, -22.8f };

            for (int i = 0; i < skylineZ.Length; i++)
            {
                float z = skylineZ[i] + ((float)rng.NextDouble() - 0.5f) * 0.8f;
                if (z > centerZ + depthZ * 0.5f || z < centerZ - depthZ * 0.5f)
                {
                    continue;
                }

                SpawnSkylineFoliage(parent, "Skyline_BushW_" + i, skylineWestX, z, rng);
                if (i % 2 == 0)
                {
                    SpawnSkylineFoliage(parent, "Skyline_BushE_" + i, skylineEastX, z + 0.6f, rng);
                }

                if (rng.NextDouble() < 0.65f)
                {
                    CreateCube("Skyline_RoofW_" + i, parent,
                        new Vector3(skylineWestX, Level1OutdoorSettings.WalkSurfaceY + 3.55f, z),
                        new Vector3(2.4f, 0.14f, 1.5f), roof);
                }

                if (rng.NextDouble() < 0.5f)
                {
                    CreateCube("Skyline_Tank_" + i, parent,
                        new Vector3(skylineEastX, Level1OutdoorSettings.WalkSurfaceY + 3.65f, z - 0.3f),
                        new Vector3(0.75f, 0.55f, 0.75f), tank);
                }
            }
        }

        private static void SpawnSkylineFoliage(Transform parent, string name, float x, float z, System.Random rng)
        {
            const string bushPath = "Assets/Prefabs/Highway/NatureBaked/bush04_Baked.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bushPath);
            if (prefab == null)
            {
                return;
            }

            GameObject bush = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            bush.name = name;
            float scale = 0.55f + (float)rng.NextDouble() * 0.2f;
            bush.transform.position = new Vector3(x, Level1OutdoorSettings.WalkSurfaceY, z);
            bush.transform.localScale = Vector3.one * scale;
            bush.transform.rotation = Quaternion.Euler(0f, rng.Next(0, 360), 0f);
            StripColliders(bush);
        }

        private static void CreateWallSegment(Transform parent, string name, float centerX, float centerZ,
            float sizeX, float height, float sizeZ, Material material, float sheetMeters, bool alongX = false)
        {
            Vector3 scale = new Vector3(sizeX, height, sizeZ);
            Vector3 pos = new Vector3(centerX, Level1OutdoorSettings.WalkSurfaceY + height * 0.5f, centerZ);
            GameObject wall = CreateCube(name, parent, pos, scale, material);
            ApplyTiling(wall, scale, material, sheetMeters, alongX);
        }

        private static void ApplyTiling(GameObject wall, Vector3 scale, Material material, float sheetMeters,
            bool alongX)
        {
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Material instance = new Material(material);
            float u = (alongX ? scale.x : scale.z) / sheetMeters;
            float v = scale.y / sheetMeters;
            instance.SetTextureScale("_BaseMap", new Vector2(u, v));
            instance.SetTextureScale("_BumpMap", new Vector2(u, v));
            renderer.sharedMaterial = instance;
        }

        private static void CreateWallLabel(Transform parent, string name, string text, float faceX, float y, float z,
            float fontSize, Color color, FontStyles style, System.Random rng, bool official)
        {
            GameObject label = new GameObject(name);
            label.transform.SetParent(parent, false);
            label.transform.position = new Vector3(faceX, Level1OutdoorSettings.WalkSurfaceY + y, z);
            label.transform.rotation = Quaternion.Euler(
                official ? 0f : rng.Next(-3, 4),
                -90f,
                official ? 0f : rng.Next(-5, 6));

            TextMeshPro tmp = label.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize * 10f;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = style;
            tmp.rectTransform.sizeDelta = new Vector2(2.8f, 0.5f);
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            Renderer renderer = label.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 worldPos, Vector3 scale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.position = worldPos;
            cube.transform.localScale = scale;
            return FinishCube(cube, material);
        }

        private static GameObject CreateLocalCube(string name, Transform parent, Vector3 localPos, Vector3 scale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = scale;
            return FinishCube(cube, material);
        }

        private static GameObject FinishCube(GameObject cube, Material material)
        {
            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return cube;
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void RemoveExisting(Transform outdoorRoot)
        {
            Transform existing = outdoorRoot.Find(RootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void MarkStatic(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(transforms[i].gameObject,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            }
        }
    }
}
#endif
