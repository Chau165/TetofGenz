#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds a reusable open-front Vietnamese street car wash prefab with props, foam, and a parked vehicle.</summary>
    public static class Level1VietnameseCarWashBuilder
    {
        public const string RootName = "PF_Vietnamese_CarWash";
        public const string PrefabPath = "Assets/Art/Level1/Outdoor/Prefabs/PF_Vietnamese_CarWash.prefab";
        private const string MaterialsRoot = "Assets/Settings/ChoTet/Materials/Level1";
        private const string MotorbikePrefabPath = "Assets/Honda Motor Co., Ltd/Super Cub C125/Prefabs/C125 - MM.prefab";
        private const string MetalRoofMaterialPath = "Assets/Settings/ChoTet/Materials/Roof_Metal_Corrugated.mat";
        private const string BannerTexturePath = "Assets/Art/Level1/Outdoor/Textures/CarWash_MinHPhat_Banner.png";
        private const string BannerMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_CarWash_MinHPhat.mat";

        private const float BayWidth = 4.2f;
        private const float BayDepth = 6.5f;
        private const float WallHeight = 2.85f;
        private const float WallThickness = 0.12f;
        private const float RoofThickness = 0.1f;
        private const float FloorThickness = 0.08f;
        private const float AlleyGap = 1.25f;
        private const float FoundationLift = 0.12f;
        private const float BannerWidth = BayWidth - 0.24f;
        private const float BannerHeight = 1.05f;

        public static void BuildPrefabAsset()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseCarWashBuilder] Exit Play mode before building the prefab.");
                return;
            }

            GameObject root = BuildAt(Vector3.zero, Quaternion.identity, saveAsPrefab: true);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            Debug.Log("[Level1VietnameseCarWashBuilder] Prefab saved: " + PrefabPath);
        }

        public static void AttachBannerOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseCarWashBuilder] Exit Play mode before attaching the banner.");
                return;
            }

            GameObject wash = GameObject.Find(RootName);
            if (wash == null)
            {
                Debug.LogError("[Level1VietnameseCarWashBuilder] PF_Vietnamese_CarWash not found in scene.");
                return;
            }

            ApplyBannerToCarWash(wash.transform);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnameseCarWashBuilder] Car wash banner attached.");
        }

        public static void PlaceBesideCircleKOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseCarWashBuilder] Exit Play mode before placing the car wash.");
                return;
            }

            GameObject store = GameObject.Find(Level1ConvenienceStoreFacadeBuilder.RootName);
            if (store == null)
            {
                Debug.LogError("[Level1VietnameseCarWashBuilder] ConvenienceStoreFacade not found.");
                return;
            }

            RemoveExistingInScene();
            Transform parent = FindOutdoorParent();
            if (!TryResolvePlacementBesideStore(store.transform, out Vector3 position, out Quaternion rotation))
            {
                Debug.LogError("[Level1VietnameseCarWashBuilder] Could not resolve placement beside Circle K.");
                return;
            }

            GameObject root = BuildAt(position, rotation, saveAsPrefab: false);
            if (root == null)
            {
                return;
            }

            if (parent != null)
            {
                root.transform.SetParent(parent, true);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnameseCarWashBuilder] Car wash placed beside Circle K at " + position);
        }

        public static bool TryResolvePlacementBesideStore(Transform store, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = store.rotation;

            Bounds storeBounds = ComputeWorldBounds(store.gameObject);
            if (storeBounds.size.sqrMagnitude < 0.01f)
            {
                return false;
            }

            Vector3 rowAxis = store.TransformDirection(Vector3.right).normalized;
            float halfBayAlongRow = BayWidth * 0.5f;
            float storeHalfAlongRow = ProjectedHalfExtent(storeBounds, rowAxis);
            Vector3 rowEdge = storeBounds.center + rowAxis * storeHalfAlongRow;
            Vector3 center = rowEdge + rowAxis * (AlleyGap + halfBayAlongRow);
            center.y = Level1OutdoorSettings.WalkSurfaceY + FoundationLift;
            position = center;
            return true;
        }

        public static GameObject BuildAt(Vector3 worldPosition, Quaternion rotation, bool saveAsPrefab)
        {
            Material concrete = EnsureMaterial("CarWash_Concrete.mat", new Color(0.62f, 0.64f, 0.66f), 0f, 0.22f);
            Material concreteWet = EnsureMaterial("CarWash_Concrete_Wet.mat", new Color(0.5f, 0.56f, 0.6f), 0f, 0.55f);
            Material wall = EnsureMaterial("CarWash_Wall.mat", new Color(0.78f, 0.8f, 0.82f), 0f, 0.18f);
            Material metalRoof = AssetDatabase.LoadAssetAtPath<Material>(MetalRoofMaterialPath)
                ?? EnsureMaterial("CarWash_Roof_Metal.mat", new Color(0.42f, 0.46f, 0.5f), 0.35f, 0.38f);
            Material foam = EnsureUnlitMaterial("CarWash_Foam.mat", new Color(0.96f, 0.98f, 1f, 0.95f));
            Material hose = EnsureMaterial("CarWash_Hose.mat", new Color(0.08f, 0.08f, 0.09f), 0f, 0.35f);
            Material propPlastic = EnsureMaterial("CarWash_Prop_Plastic.mat", new Color(0.92f, 0.24f, 0.18f), 0f, 0.42f);
            Material propYellow = EnsureMaterial("CarWash_Prop_Yellow.mat", new Color(0.95f, 0.78f, 0.12f), 0f, 0.4f);
            Material propGreen = EnsureMaterial("CarWash_Prop_Green.mat", new Color(0.18f, 0.62f, 0.28f), 0f, 0.4f);
            Material drain = EnsureMaterial("CarWash_Drain.mat", new Color(0.28f, 0.3f, 0.32f), 0.45f, 0.5f);

            float halfWidth = BayWidth * 0.5f;
            float halfDepth = BayDepth * 0.5f;
            float frontZ = -halfDepth;
            float backZ = halfDepth;
            float roofY = WallHeight + RoofThickness * 0.5f;

            GameObject root = new GameObject(RootName);
            root.transform.position = worldPosition;
            root.transform.rotation = rotation;

            GameObject structure = new GameObject("Wash_Structure");
            structure.transform.SetParent(root.transform, false);

            CreatePartCube("Wash_Floor", structure.transform,
                new Vector3(0f, FloorThickness * 0.5f, 0f),
                new Vector3(BayWidth + 0.08f, FloorThickness, BayDepth + 0.06f), concrete);

            CreatePartCube("Wash_Wall_Left", structure.transform,
                new Vector3(-halfWidth + WallThickness * 0.5f, WallHeight * 0.5f, 0f),
                new Vector3(WallThickness, WallHeight, BayDepth), wall);
            CreatePartCube("Wash_Wall_Right", structure.transform,
                new Vector3(halfWidth - WallThickness * 0.5f, WallHeight * 0.5f, 0f),
                new Vector3(WallThickness, WallHeight, BayDepth), wall);
            CreatePartCube("Wash_Wall_Back", structure.transform,
                new Vector3(0f, WallHeight * 0.5f, backZ - WallThickness * 0.5f),
                new Vector3(BayWidth, WallHeight, WallThickness), wall);

            CreatePartCube("Wash_Roof", structure.transform,
                new Vector3(0f, roofY, 0f),
                new Vector3(BayWidth + 0.18f, RoofThickness, BayDepth + 0.22f), metalRoof);
            CreatePartCube("Wash_Roof_FrontBeam", structure.transform,
                new Vector3(0f, WallHeight + 0.04f, frontZ - 0.04f),
                new Vector3(BayWidth + 0.14f, 0.08f, 0.1f), metalRoof);

            BuildSign(structure.transform, frontZ);

            GameObject vehicleRoot = new GameObject("Wash_Vehicle");
            vehicleRoot.transform.SetParent(root.transform, false);
            InstantiateMotorbike(vehicleRoot.transform);

            GameObject foamRoot = new GameObject("Wash_Foam");
            foamRoot.transform.SetParent(root.transform, false);
            BuildFoam(foamRoot.transform, foam, concreteWet);

            GameObject propsRoot = new GameObject("Wash_Props");
            propsRoot.transform.SetParent(root.transform, false);
            BuildProps(propsRoot.transform, frontZ, backZ, halfWidth, hose, propPlastic, propYellow, propGreen, drain,
                concreteWet);

            MarkStatic(root);

            if (saveAsPrefab)
            {
                EnsureFolder("Assets/Art/Level1/Outdoor/Prefabs");
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
            }

            return root;
        }

        public static void ApplyBannerToCarWash(Transform washRoot)
        {
            Transform structure = washRoot.Find("Wash_Structure");
            if (structure == null)
            {
                Debug.LogError("[Level1VietnameseCarWashBuilder] Wash_Structure not found.");
                return;
            }

            Transform existing = structure.Find("Wash_Sign");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            float frontZ = -BayDepth * 0.5f;
            BuildSign(structure, frontZ);
        }

        private static void BuildSign(Transform parent, float frontZ)
        {
            Material bannerMat = EnsureBannerMaterial();
            if (bannerMat == null)
            {
                return;
            }

            GameObject signRoot = new GameObject("Wash_Sign");
            signRoot.transform.SetParent(parent, false);
            signRoot.transform.localPosition = new Vector3(0f, WallHeight - BannerHeight * 0.5f - 0.12f, frontZ - 0.04f);

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "CarWash_Banner";
            quad.transform.SetParent(signRoot.transform, false);
            quad.transform.localPosition = Vector3.zero;
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = new Vector3(BannerWidth, BannerHeight, 1f);
            Object.DestroyImmediate(quad.GetComponent<Collider>());

            Material instance = new Material(bannerMat);
            instance.SetInt("_Cull", 0);
            instance.renderQueue = (int)RenderQueue.Geometry + 10;
            ApplyRenderer(quad.GetComponent<Renderer>(), instance);
        }

        private static void InstantiateMotorbike(Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MotorbikePrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[Level1VietnameseCarWashBuilder] Motorbike prefab missing, using placeholder cube.");
                CreatePartCube("Vehicle_Placeholder", parent,
                    new Vector3(0f, 0.45f, -0.8f),
                    new Vector3(0.5f, 0.8f, 1.6f),
                    EnsureMaterial("CarWash_Placeholder.mat", new Color(0.2f, 0.22f, 0.25f), 0.2f, 0.4f));
                return;
            }

            GameObject bike = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            bike.name = "Wash_Motorbike";
            bike.transform.localPosition = new Vector3(0.05f, 0.02f, -0.65f);
            bike.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            bike.transform.localScale = Vector3.one * 0.92f;

            foreach (Collider collider in bike.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(collider);
            }

            foreach (Rigidbody rb in bike.GetComponentsInChildren<Rigidbody>(true))
            {
                Object.DestroyImmediate(rb);
            }
        }

        private static void BuildFoam(Transform parent, Material foamMat, Material wetMat)
        {
            CreateFoamBlob(parent, "Foam_Seat", new Vector3(0.02f, 0.72f, -0.55f), new Vector3(0.42f, 0.16f, 0.34f), foamMat);
            CreateFoamBlob(parent, "Foam_Handlebar", new Vector3(0.04f, 0.92f, -1.05f), new Vector3(0.28f, 0.12f, 0.22f), foamMat);
            CreateFoamBlob(parent, "Foam_Windshield", new Vector3(0.02f, 0.86f, -0.92f), new Vector3(0.34f, 0.2f, 0.16f), foamMat);
            CreateFoamBlob(parent, "Foam_Wheel_Front", new Vector3(0.03f, 0.22f, -1.18f), new Vector3(0.26f, 0.14f, 0.26f), foamMat);
            CreateFoamBlob(parent, "Foam_Wheel_Rear", new Vector3(0.02f, 0.22f, -0.18f), new Vector3(0.28f, 0.14f, 0.28f), foamMat);
            CreateFoamBlob(parent, "Foam_Tank", new Vector3(0.02f, 0.66f, -0.78f), new Vector3(0.22f, 0.1f, 0.24f), foamMat);

            CreatePartCube("Foam_Puddle_A", parent, new Vector3(-0.35f, 0.045f, -0.95f),
                new Vector3(0.55f, 0.012f, 0.42f), wetMat);
            CreatePartCube("Foam_Puddle_B", parent, new Vector3(0.42f, 0.045f, -0.35f),
                new Vector3(0.48f, 0.012f, 0.36f), wetMat);
            CreatePartCube("Foam_Puddle_C", parent, new Vector3(0.08f, 0.045f, -1.25f),
                new Vector3(0.62f, 0.012f, 0.28f), wetMat);
        }

        private static void CreateFoamBlob(Transform parent, string name, Vector3 localPos, Vector3 localScale,
            Material material)
        {
            GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blob.name = name;
            blob.transform.SetParent(parent, false);
            blob.transform.localPosition = localPos;
            blob.transform.localScale = localScale;
            Object.DestroyImmediate(blob.GetComponent<Collider>());
            ApplyRenderer(blob.GetComponent<Renderer>(), material);
        }

        private static void BuildProps(Transform parent, float frontZ, float backZ, float halfWidth, Material hoseMat,
            Material plastic, Material yellow, Material green, Material drainMat, Material wetMat)
        {
            float entranceZ = frontZ + 0.45f;

            CreatePartCube("Prop_PressureWasher_Body", parent,
                new Vector3(halfWidth - 0.42f, 0.42f, backZ - 0.55f),
                new Vector3(0.34f, 0.62f, 0.28f), plastic);
            CreatePartCube("Prop_PressureWasher_Tank", parent,
                new Vector3(halfWidth - 0.42f, 0.72f, backZ - 0.55f),
                new Vector3(0.22f, 0.28f, 0.22f), green);
            CreatePartCube("Prop_PressureWasher_Handle", parent,
                new Vector3(halfWidth - 0.18f, 0.78f, backZ - 0.72f),
                new Vector3(0.06f, 0.42f, 0.06f), yellow);

            BuildHose(parent, hoseMat, new Vector3(halfWidth - 0.28f, 0.06f, backZ - 0.62f),
                new Vector3(-0.15f, 0.05f, entranceZ + 0.2f));

            CreatePartCube("Prop_Bucket", parent, new Vector3(-halfWidth + 0.42f, 0.14f, backZ - 0.42f),
                new Vector3(0.26f, 0.24f, 0.26f), yellow);
            CreatePartCube("Prop_Chair_Seat", parent, new Vector3(-halfWidth + 0.38f, 0.24f, backZ - 0.95f),
                new Vector3(0.34f, 0.04f, 0.34f), plastic);
            CreatePartCube("Prop_Chair_Back", parent, new Vector3(-halfWidth + 0.38f, 0.48f, backZ - 1.08f),
                new Vector3(0.34f, 0.42f, 0.05f), plastic);

            CreatePartCube("Prop_SoapBottle_A", parent, new Vector3(-halfWidth + 0.55f, 0.1f, backZ - 0.38f),
                new Vector3(0.08f, 0.16f, 0.08f), green);
            CreatePartCube("Prop_SoapBottle_B", parent, new Vector3(-halfWidth + 0.66f, 0.1f, backZ - 0.4f),
                new Vector3(0.07f, 0.14f, 0.07f), plastic);

            CreatePartCube("Prop_CleaningCloth", parent, new Vector3(-halfWidth + 0.5f, 0.08f, backZ - 0.52f),
                new Vector3(0.22f, 0.03f, 0.16f), EnsureUnlitMaterial("CarWash_Cloth.mat", new Color(0.92f, 0.9f, 0.2f)));

            CreatePartCube("Prop_FloorDrain", parent, new Vector3(0.35f, 0.05f, entranceZ),
                new Vector3(0.42f, 0.03f, 0.18f), drainMat);
            CreatePartCube("Prop_DrainGutter", parent, new Vector3(-0.55f, 0.045f, entranceZ + 0.08f),
                new Vector3(1.1f, 0.02f, 0.12f), wetMat);
        }

        private static void BuildHose(Transform parent, Material hoseMat, Vector3 start, Vector3 end)
        {
            GameObject hoseRoot = new GameObject("Prop_Hose");
            hoseRoot.transform.SetParent(parent, false);

            const int segments = 7;
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1);
                float tNext = (i + 1) / (float)(segments - 1);
                Vector3 a = Vector3.Lerp(start, end, t);
                Vector3 b = Vector3.Lerp(start, end, tNext);
                a.y += Mathf.Sin(t * Mathf.PI) * 0.04f;
                b.y += Mathf.Sin(tNext * Mathf.PI) * 0.04f;
                Vector3 mid = (a + b) * 0.5f;
                Vector3 dir = b - a;
                float len = dir.magnitude;
                if (len < 0.001f)
                {
                    continue;
                }

                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                seg.name = "Hose_Seg_" + i;
                seg.transform.SetParent(hoseRoot.transform, false);
                seg.transform.localPosition = mid;
                seg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
                seg.transform.localScale = new Vector3(0.035f, len * 0.5f, 0.035f);
                Object.DestroyImmediate(seg.GetComponent<Collider>());
                ApplyRenderer(seg.GetComponent<Renderer>(), hoseMat);
            }
        }

        private static Transform FindOutdoorParent()
        {
            Transform world = GameObject.Find("Level1World")?.transform;
            return world != null ? world.Find("Level1_OutdoorArea") : null;
        }

        private static void RemoveExistingInScene()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static float ProjectedHalfExtent(Bounds bounds, Vector3 axis)
        {
            Vector3 extents = bounds.extents;
            Vector3 absAxis = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
            return Vector3.Dot(extents, absAxis);
        }

        private static Bounds ComputeWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static GameObject CreatePartCube(string name, Transform parent, Vector3 localPos, Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            ApplyRenderer(cube.GetComponent<Renderer>(), material);
            return cube;
        }

        private static void ApplyRenderer(Renderer renderer, Material material)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material EnsureMaterial(string fileName, Color baseColor, float metallic, float smoothness)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = Path.GetFileNameWithoutExtension(fileName);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureBannerMaterial()
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
            if (texture == null)
            {
                Debug.LogError("[Level1VietnameseCarWashBuilder] Missing banner texture: " + BannerTexturePath);
                return null;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(BannerMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_CarWash_MinHPhat";
                AssetDatabase.CreateAsset(mat, BannerMaterialPath);
            }

            mat.SetTexture("_BaseMap", texture);
            mat.SetTexture("_MainTex", texture);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_Color", Color.white);
            mat.doubleSidedGI = true;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureUnlitMaterial(string fileName, Color baseColor)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                mat = new Material(shader);
                mat.name = Path.GetFileNameWithoutExtension(fileName);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void MarkStatic(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(transforms[i].gameObject, StaticEditorFlags.BatchingStatic);
            }
        }
    }
}
#endif
