#if UNITY_EDITOR
using System.Collections.Generic;
using ChoTet.Level1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Rebuilds the Level 1 commercial alley ground (sidewalks, curbs, crowned asphalt road, drains)
    /// without moving storefront buildings.
    /// </summary>
    public static class Level1VietnameseAlleyGroundBuilder
    {
        private const string AlleyGroundRootName = "Level1_VietnameseAlleyGround";
        private const string SidewalkMaterialPath = Level1VietnameseAlleyMaterialBuilder.SidewalkMaterialPath;
        private const string AsphaltMaterialPath = Level1VietnameseAlleyMaterialBuilder.AsphaltMaterialPath;
        private const string DrainPrefabPath = "Assets/Prefabs/Highway/QuaterniusDowntown/Modules/Prop_Drain.prefab";

        private const float AlleyStartZ = -4.25f;
        private const float AlleyEndZ = -25.8f;
        private const float RoadHalfWidth = 2.38f;
        private const float SidewalkWidth = 1.35f;
        private const float CurbWidth = 0.16f;
        private const float CurbHeight = 0.12f;
        private const float Crossfall = 0.015f;
        private const float SidewalkThickness = 0.08f;
        private const float ApronThickness = 0.06f;
        private const float UvMetersPerTile = Level1VietnameseAlleyMaterialBuilder.SidewalkSheetMeters;
        private const float RoadUvMetersPerTile = Level1VietnameseAlleyMaterialBuilder.AsphaltSheetMeters;
        private const float DrainSpacing = 4.2f;

        private static readonly string[] StorefrontRoots =
        {
            Level1ConvenienceStoreFacadeBuilder.RootName,
            Level1VietnameseCarWashBuilder.RootName,
            Level1VietnamesePawnShopBuilder.RootName,
            Level1TheGioiMobileStoreBuilder.RootName,
            Level1VietnameseComTamShopBuilder.RootName,
            Level1VietnamesePharmacyBuilder.RootName
        };

        public static void RebuildOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseAlleyGroundBuilder] Exit Play mode before rebuilding alley ground.");
                return;
            }

            Transform outdoor = GameObject.Find("Level1_OutdoorArea")?.transform;
            if (outdoor == null)
            {
                Debug.LogError("[Level1VietnameseAlleyGroundBuilder] Level1_OutdoorArea not found.");
                return;
            }

            Rebuild(outdoor);
            AlignStorefronts(outdoor);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnameseAlleyGroundBuilder] Vietnamese alley ground rebuilt.");
        }

        public static void Rebuild(Transform outdoorRoot)
        {
            Level1VietnameseAlleyMaterialBuilder.BakeOrUpdateMaterials();
            Material sidewalkMat = AssetDatabase.LoadAssetAtPath<Material>(SidewalkMaterialPath);
            Material asphaltMat = AssetDatabase.LoadAssetAtPath<Material>(AsphaltMaterialPath);
            if (sidewalkMat == null || asphaltMat == null)
            {
                Debug.LogError("[Level1VietnameseAlleyGroundBuilder] Missing sidewalk or asphalt material.");
                return;
            }

            DisableLegacyAlleySurfaces(outdoorRoot);
            RemovePerShopSidewalkCubes(outdoorRoot);
            RemoveExistingAlleyGround(outdoorRoot);
            RebuildCurbs(outdoorRoot, sidewalkMat);

            float spanDepth = AlleyStartZ - AlleyEndZ;
            float spanCenterZ = (AlleyStartZ + AlleyEndZ) * 0.5f;
            float sidewalkSurfaceY = Level1OutdoorSettings.WalkSurfaceY;
            float roadEdgeY = sidewalkSurfaceY - CurbHeight;
            float crownRise = RoadHalfWidth * Crossfall;
            float westCurbCenterX = -(RoadHalfWidth + CurbWidth * 0.5f);
            float eastCurbCenterX = -westCurbCenterX;
            float westSidewalkCenterX = westCurbCenterX - CurbWidth * 0.5f - SidewalkWidth * 0.5f;
            float eastSidewalkCenterX = -westSidewalkCenterX;

            GameObject root = new GameObject(AlleyGroundRootName);
            root.transform.SetParent(outdoorRoot, false);

            BuildRoadMesh(root.transform, asphaltMat, spanCenterZ, spanDepth, roadEdgeY, crownRise);
            BuildSidewalkMesh(root.transform, "AlleySidewalk_L", sidewalkMat, westSidewalkCenterX, spanCenterZ, spanDepth,
                SidewalkWidth, sidewalkSurfaceY);
            BuildSidewalkMesh(root.transform, "AlleySidewalk_R", sidewalkMat, eastSidewalkCenterX, spanCenterZ, spanDepth,
                SidewalkWidth, sidewalkSurfaceY);
            BuildBuildingAprons(root.transform, sidewalkMat, westSidewalkCenterX, eastSidewalkCenterX, spanCenterZ,
                spanDepth, sidewalkSurfaceY);
            BuildStormDrains(root.transform, westCurbCenterX, eastCurbCenterX, sidewalkSurfaceY);
            MarkStatic(root);
        }

        private static void DisableLegacyAlleySurfaces(Transform outdoorRoot)
        {
            SetRendererEnabled(outdoorRoot, "UrbanPedestrianPavement/PedestrianPavementSlab", false);
            SetRendererEnabled(outdoorRoot, "TravelRoad/TravelRoadSurface", false);
            SetRendererEnabled(outdoorRoot, "TravelRoad/TravelRoadShoulderL", false);
            SetRendererEnabled(outdoorRoot, "TravelRoad/TravelRoadShoulderR", false);
        }

        private static void SetRendererEnabled(Transform root, string path, bool enabled)
        {
            Transform target = root.Find(path);
            if (target == null)
            {
                return;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }

        private static void RemovePerShopSidewalkCubes(Transform outdoorRoot)
        {
            for (int i = 0; i < StorefrontRoots.Length; i++)
            {
                Transform shop = outdoorRoot.Find(StorefrontRoots[i]);
                if (shop == null)
                {
                    continue;
                }

                Transform envSidewalk = shop.Find("Mobile_Environment/Env_Sidewalk")
                    ?? shop.Find("Pawn_Environment/Env_Sidewalk")
                    ?? shop.Find("ComTam_Environment/Env_Sidewalk")
                    ?? shop.Find("Pharmacy_Environment/Env_Sidewalk")
                    ?? FindChildRecursive(shop, "Env_Sidewalk");
                if (envSidewalk != null)
                {
                    Object.DestroyImmediate(envSidewalk.gameObject);
                }
            }
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent.name == childName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void RemoveExistingAlleyGround(Transform outdoorRoot)
        {
            Transform existing = outdoorRoot.Find(AlleyGroundRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void RebuildCurbs(Transform outdoorRoot, Material curbMaterial)
        {
            Transform streetscape = outdoorRoot.Find("Level1_OutdoorQuaterniusStreetscape");
            if (streetscape == null)
            {
                GameObject streetscapeRoot = new GameObject("Level1_OutdoorQuaterniusStreetscape");
                streetscapeRoot.transform.SetParent(outdoorRoot, false);
                streetscape = streetscapeRoot.transform;
            }

            DestroyIfExists(streetscape, "PathCurb_L");
            DestroyIfExists(streetscape, "PathCurb_R");

            float spanDepth = AlleyStartZ - AlleyEndZ;
            float spanCenterZ = (AlleyStartZ + AlleyEndZ) * 0.5f;
            float sidewalkSurfaceY = Level1OutdoorSettings.WalkSurfaceY;
            float westCurbCenterX = -(RoadHalfWidth + CurbWidth * 0.5f);
            float eastCurbCenterX = -westCurbCenterX;

            CreateCurb(streetscape, "PathCurb_L", westCurbCenterX, sidewalkSurfaceY, spanCenterZ, spanDepth, curbMaterial);
            CreateCurb(streetscape, "PathCurb_R", eastCurbCenterX, sidewalkSurfaceY, spanCenterZ, spanDepth, curbMaterial);
        }

        private static void DestroyIfExists(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void CreateCurb(Transform parent, string name, float centerX, float sidewalkY, float centerZ,
            float depth, Material material)
        {
            GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = name;
            curb.transform.SetParent(parent, false);
            curb.transform.position = new Vector3(centerX, sidewalkY - CurbHeight * 0.5f, centerZ);
            curb.transform.localScale = new Vector3(CurbWidth, CurbHeight, depth + 0.04f);
            Object.DestroyImmediate(curb.GetComponent<Collider>());

            Renderer renderer = curb.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static void BuildRoadMesh(Transform parent, Material asphalt, float centerZ, float depth,
            float roadEdgeY, float crownRise)
        {
            const int xSegments = 10;
            const int zSegments = 28;
            float halfDepth = depth * 0.5f;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (int z = 0; z <= zSegments; z++)
            {
                float vz = Mathf.Lerp(-halfDepth, halfDepth, z / (float)zSegments);
                for (int x = 0; x <= xSegments; x++)
                {
                    float t = x / (float)xSegments;
                    float vx = Mathf.Lerp(-RoadHalfWidth, RoadHalfWidth, t);
                    float normalized = Mathf.Abs(vx) / RoadHalfWidth;
                    float vy = roadEdgeY + (1f - normalized) * crownRise;
                    vertices.Add(new Vector3(vx, vy, vz));
                    uvs.Add(new Vector2(vx / RoadUvMetersPerTile, (centerZ + vz) / RoadUvMetersPerTile));
                }
            }

            int row = xSegments + 1;
            for (int z = 0; z < zSegments; z++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    int i0 = z * row + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + row;
                    int i3 = i2 + 1;
                    triangles.Add(i0);
                    triangles.Add(i2);
                    triangles.Add(i1);
                    triangles.Add(i1);
                    triangles.Add(i2);
                    triangles.Add(i3);
                }
            }

            GameObject road = new GameObject("AlleyRoad_Asphalt");
            road.transform.SetParent(parent, false);
            road.transform.position = new Vector3(0f, 0f, centerZ);

            Mesh mesh = new Mesh { name = "AlleyRoad_Asphalt_Mesh" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshFilter filter = road.AddComponent<MeshFilter>();
            MeshRenderer renderer = road.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterial = CreateTiledMaterial(asphalt, RoadHalfWidth * 2f, depth, RoadUvMetersPerTile);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void BuildSidewalkMesh(Transform parent, string name, Material material, float centerX,
            float centerZ, float depth, float width, float surfaceY)
        {
            GameObject side = new GameObject(name);
            side.transform.SetParent(parent, false);
            side.transform.position = new Vector3(centerX, surfaceY - SidewalkThickness * 0.5f, centerZ);

            MeshFilter filter = side.AddComponent<MeshFilter>();
            MeshRenderer renderer = side.AddComponent<MeshRenderer>();
            filter.sharedMesh = BuildFlatSlabMesh(width, depth, centerX, centerZ);
            renderer.sharedMaterial = CreateTiledMaterial(material, width, depth, UvMetersPerTile);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void BuildBuildingAprons(Transform parent, Material material, float westSidewalkCenterX,
            float eastSidewalkCenterX, float centerZ, float depth, float surfaceY)
        {
            float westInnerX = westSidewalkCenterX - SidewalkWidth * 0.5f;
            float eastInnerX = eastSidewalkCenterX + SidewalkWidth * 0.5f;
            float westOuterTargetX = Level1OutdoorMovementSettings.PlayMinX + 0.4f;
            float eastOuterTargetX = Level1OutdoorMovementSettings.PlayMaxX - 0.4f;
            float westApronWidth = westInnerX - westOuterTargetX;
            float eastApronWidth = eastOuterTargetX - eastInnerX;
            if (westApronWidth > 0.2f)
            {
                float westApronCenterX = (westInnerX + westOuterTargetX) * 0.5f;
                BuildSidewalkMesh(parent, "AlleyApron_L", material, westApronCenterX, centerZ, depth, westApronWidth,
                    surfaceY);
            }

            if (eastApronWidth > 0.2f)
            {
                float eastApronCenterX = (eastInnerX + eastOuterTargetX) * 0.5f;
                BuildSidewalkMesh(parent, "AlleyApron_R", material, eastApronCenterX, centerZ, depth, eastApronWidth,
                    surfaceY);
            }
        }

        private static void BuildStormDrains(Transform parent, float westCurbX, float eastCurbX, float sidewalkY)
        {
            GameObject drainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DrainPrefabPath);
            if (drainPrefab == null)
            {
                return;
            }

            GameObject drainRoot = new GameObject("AlleyStormDrains");
            drainRoot.transform.SetParent(parent, false);

            int count = Mathf.FloorToInt((AlleyStartZ - AlleyEndZ) / DrainSpacing);
            for (int i = 0; i <= count; i++)
            {
                float z = AlleyStartZ - i * DrainSpacing;
                if (z < AlleyEndZ + 0.5f)
                {
                    break;
                }

                SpawnDrain(drainRoot.transform, drainPrefab, westCurbX + CurbWidth * 0.65f, sidewalkY, z, -90f);
                SpawnDrain(drainRoot.transform, drainPrefab, eastCurbX - CurbWidth * 0.65f, sidewalkY, z, 90f);
            }
        }

        private static void SpawnDrain(Transform parent, GameObject prefab, float x, float y, float z, float yaw)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            if (instance == null)
            {
                return;
            }

            instance.name = "StormDrain";
            instance.transform.position = new Vector3(x, y, z);
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = Vector3.one * 0.9f;
            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static Mesh BuildFlatSlabMesh(float width, float length, float worldCenterX, float segmentWorldEndZ)
        {
            float halfWidth = width * 0.5f;
            float halfLength = length * 0.5f;
            float slabCenterWorldZ = segmentWorldEndZ;

            var vertices = new Vector3[]
            {
                new Vector3(-halfWidth, 0f, -halfLength),
                new Vector3(halfWidth, 0f, -halfLength),
                new Vector3(-halfWidth, 0f, halfLength),
                new Vector3(halfWidth, 0f, halfLength)
            };

            var uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                float worldX = worldCenterX + vertices[i].x;
                float worldZ = slabCenterWorldZ + vertices[i].z;
                uvs[i] = new Vector2(worldX / UvMetersPerTile, worldZ / UvMetersPerTile);
            }

            var mesh = new Mesh { name = "AlleySidewalkSlab" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Material CreateTiledMaterial(Material source, float worldWidth, float worldDepth,
            float metersPerTile)
        {
            if (source == null)
            {
                return null;
            }

            Material instance = new Material(source);
            float tileU = Mathf.Max(worldWidth / metersPerTile, 0.25f);
            float tileV = Mathf.Max(worldDepth / metersPerTile, 0.25f);
            Vector2 scale = new Vector2(tileU, tileV);
            if (instance.HasProperty("_BaseMap"))
            {
                instance.SetTextureScale("_BaseMap", scale);
            }

            if (instance.HasProperty("_MainTex"))
            {
                instance.SetTextureScale("_MainTex", scale);
            }

            if (instance.HasProperty("_BumpMap"))
            {
                instance.SetTextureScale("_BumpMap", scale);
            }

            return instance;
        }

        private static void AlignStorefronts(Transform outdoorRoot)
        {
            float targetY = Level1OutdoorSettings.WalkSurfaceY + 0.12f;

            for (int i = 0; i < StorefrontRoots.Length; i++)
            {
                Transform shop = outdoorRoot.Find(StorefrontRoots[i]);
                if (shop == null)
                {
                    continue;
                }

                Vector3 pos = shop.position;
                if (Mathf.Abs(pos.y - targetY) > 0.04f)
                {
                    pos.y = targetY;
                    shop.position = pos;
                }
            }
        }

        private static void MarkStatic(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(transforms[i].gameObject, StaticEditorFlags.BatchingStatic);
            }

            Transform streetscape = root.transform.parent != null
                ? root.transform.parent.Find("Level1_OutdoorQuaterniusStreetscape")
                : null;
            if (streetscape == null)
            {
                return;
            }

            Transform curbL = streetscape.Find("PathCurb_L");
            Transform curbR = streetscape.Find("PathCurb_R");
            if (curbL != null)
            {
                GameObjectUtility.SetStaticEditorFlags(curbL.gameObject, StaticEditorFlags.BatchingStatic);
            }

            if (curbR != null)
            {
                GameObjectUtility.SetStaticEditorFlags(curbR.gameObject, StaticEditorFlags.BatchingStatic);
            }
        }
    }
}
#endif
