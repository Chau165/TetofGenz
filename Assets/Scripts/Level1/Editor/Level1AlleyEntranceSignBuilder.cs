#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using ChoTet.Level1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace ChoTet.Level1.Editor
{
    /// <summary>Builds the Hẻm 45 Bùi Đình Túy procedural enamel sign with sidewalk support posts.</summary>
    public static class Level1AlleyEntranceSignBuilder
    {
        public const string RootName = "Level1_AlleyEntranceSign";

        private const string TextureFolder = "Assets/Art/Level1/Outdoor/Textures";
        private const string AlbedoPath = TextureFolder + "/AlleyEntranceSign_Albedo.png";
        private const string MaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_EntranceSign_Enamel.mat";
        private const string SteelMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_EntranceSign_Bracket.mat";

        private const float RoadHalfWidth = 2.38f;
        private const float CurbWidth = 0.16f;
        private const float SidewalkWidth = 1.35f;
        private const float SignWidth = 5.5f;
        private const float SignHeight = 0.72f;
        private const float SignThickness = 0.025f;
        private const float PoleSize = 0.07f;
        private const float PoleBasePlateSize = 0.2f;
        private const float PoleBasePlateHeight = 0.03f;
        private const float DefaultSignCenterY = 3.42f;
        private const float DefaultSignZ = -4.3f;

        private static float SidewalkPoleX =>
            RoadHalfWidth + CurbWidth + SidewalkWidth * 0.32f;

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1AlleyEntranceSignBuilder] Exit Play mode before placing the sign.");
                return;
            }

            Transform outdoor = Level1OutdoorSceneLookup.ResolveOutdoorRoot();
            if (outdoor == null)
            {
                Debug.LogError("[Level1AlleyEntranceSignBuilder] Level1_OutdoorArea not found.");
                return;
            }

            Build(outdoor);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1AlleyEntranceSignBuilder] Procedural alley sign rebuilt (transform preserved).");
        }

        public static void Build(Transform outdoorRoot)
        {
            EnsureAlbedoTexture();
            BakeMaterials();

            Vector3 defaultPosition = new Vector3(SidewalkPoleX, DefaultSignCenterY, DefaultSignZ);
            Vector3 position = defaultPosition;
            Quaternion rotation = Quaternion.identity;
            Vector3 localScale = Vector3.one;

            Transform existing = outdoorRoot.Find(RootName);
            if (existing != null)
            {
                bool misplacedAtBusStop = Mathf.Abs(existing.position.z - Level1BusStopRoadSettings.BusShelterCenterZ) < 4f;
                position = misplacedAtBusStop ? defaultPosition : existing.position;
                rotation = existing.rotation;
                localScale = existing.localScale;
                Object.DestroyImmediate(existing.gameObject);
            }

            Material enamel = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Material steel = AssetDatabase.LoadAssetAtPath<Material>(SteelMaterialPath);
            if (enamel == null || steel == null)
            {
                Debug.LogError("[Level1AlleyEntranceSignBuilder] Missing sign materials.");
                return;
            }

            GameObject root = new GameObject(RootName);
            root.transform.SetParent(outdoorRoot, false);
            root.transform.position = position;
            root.transform.rotation = rotation;
            root.transform.localScale = localScale;
            root.transform.SetAsLastSibling();

            BuildSignPanel(root.transform, enamel);
            BuildSidewalkSupport(root.transform, steel, -SidewalkPoleX);
            BuildSidewalkSupport(root.transform, steel, SidewalkPoleX);

            MarkStatic(root);
        }

        private static void BuildSignPanel(Transform parent, Material enamel)
        {
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Sign_Back";
            back.transform.SetParent(parent, false);
            back.transform.localPosition = new Vector3(0f, 0f, -SignThickness * 0.5f);
            back.transform.localScale = new Vector3(SignWidth, SignHeight, SignThickness);
            Object.DestroyImmediate(back.GetComponent<Collider>());
            ApplyRenderer(back.GetComponent<Renderer>(), enamel);

            BuildSignFace(parent, enamel, "Sign_Face_Alley", 0.004f, 180f);
            BuildSignFace(parent, enamel, "Sign_Face_Street", -0.004f, 0f);
        }

        private static void BuildSignFace(Transform parent, Material enamel, string name, float localZ, float yawDegrees)
        {
            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
            face.name = name;
            face.transform.SetParent(parent, false);
            face.transform.localPosition = new Vector3(0f, 0f, localZ);
            face.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            face.transform.localScale = new Vector3(SignWidth, SignHeight, 1f);
            Object.DestroyImmediate(face.GetComponent<Collider>());
            ApplyRenderer(face.GetComponent<Renderer>(), enamel);
        }

        private static void BuildSidewalkSupport(Transform parent, Material steel, float poleX)
        {
            float side = Mathf.Sign(poleX);
            string suffix = side < 0f ? "L" : "R";
            float scaleY = Mathf.Max(parent.lossyScale.y, 0.01f);
            float signBottomLocalY = -SignHeight * 0.5f;
            float signBottomWorldY = parent.position.y + signBottomLocalY * scaleY;
            float poleHeightWorld = Mathf.Max(0.5f, signBottomWorldY - Level1OutdoorSettings.WalkSurfaceY);
            float poleCenterWorldY = Level1OutdoorSettings.WalkSurfaceY + poleHeightWorld * 0.5f;
            float poleCenterLocalY = (poleCenterWorldY - parent.position.y) / scaleY;
            float poleBaseLocalY = (Level1OutdoorSettings.WalkSurfaceY - parent.position.y) / scaleY +
                                   PoleBasePlateHeight * 0.5f;

            GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basePlate.name = "Support_BasePlate_" + suffix;
            basePlate.transform.SetParent(parent, false);
            basePlate.transform.localPosition = new Vector3(poleX, poleBaseLocalY, 0f);
            basePlate.transform.localScale = new Vector3(PoleBasePlateSize, PoleBasePlateHeight, PoleBasePlateSize);
            Object.DestroyImmediate(basePlate.GetComponent<Collider>());
            ApplyRenderer(basePlate.GetComponent<Renderer>(), steel);

            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Support_Post_" + suffix;
            post.transform.SetParent(parent, false);
            post.transform.localPosition = new Vector3(poleX, poleCenterLocalY, 0f);
            post.transform.localScale = new Vector3(PoleSize, poleHeightWorld / scaleY, PoleSize);
            Object.DestroyImmediate(post.GetComponent<Collider>());
            ApplyRenderer(post.GetComponent<Renderer>(), steel);

            float signEdgeX = side * SignWidth * 0.5f;
            float armLength = Mathf.Abs(signEdgeX - poleX);
            float armCenterX = (signEdgeX + poleX) * 0.5f;
            float armY = signBottomLocalY - 0.03f;

            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Support_Arm_" + suffix;
            arm.transform.SetParent(parent, false);
            arm.transform.localPosition = new Vector3(armCenterX, armY, 0f);
            arm.transform.localScale = new Vector3(armLength, 0.05f, 0.05f);
            Object.DestroyImmediate(arm.GetComponent<Collider>());
            ApplyRenderer(arm.GetComponent<Renderer>(), steel);

            GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "Support_Bracket_" + suffix;
            bracket.transform.SetParent(parent, false);
            bracket.transform.localPosition = new Vector3(signEdgeX - side * 0.03f, signBottomLocalY - 0.06f, 0f);
            bracket.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
            Object.DestroyImmediate(bracket.GetComponent<Collider>());
            ApplyRenderer(bracket.GetComponent<Renderer>(), steel);
        }

        private static void EnsureAlbedoTexture()
        {
            string script = Path.Combine(Application.dataPath, "Art/Level1/Outdoor/Textures/GenerateAlleyEntranceSign.py");
            if (!File.Exists(script))
            {
                Debug.LogWarning("[Level1AlleyEntranceSignBuilder] Missing generator: " + script);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "\"" + script + "\"",
                WorkingDirectory = Path.GetDirectoryName(script),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo);
            process?.WaitForExit();
            AssetDatabase.Refresh();
        }

        private static void BakeMaterials()
        {
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath);
            if (albedo != null)
            {
                TextureImporter importer = AssetImporter.GetAtPath(AlbedoPath) as TextureImporter;
                if (importer != null)
                {
                    importer.sRGBTexture = true;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.mipmapEnabled = true;
                    importer.maxTextureSize = 4096;
                    importer.SaveAndReimport();
                }
            }

            Material enamel = LoadOrCreate(MaterialPath);
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            enamel.shader = lit;
            if (albedo != null)
            {
                enamel.SetTexture("_BaseMap", albedo);
                enamel.SetTexture("_MainTex", albedo);
            }

            enamel.SetColor("_BaseColor", Color.white);
            enamel.SetFloat("_Metallic", 0.18f);
            enamel.SetFloat("_Smoothness", 0.58f);
            enamel.SetFloat("_Cull", (float)CullMode.Off);
            enamel.renderQueue = 3102;
            EditorUtility.SetDirty(enamel);

            Material steel = LoadOrCreate(SteelMaterialPath);
            steel.shader = lit;
            steel.SetColor("_BaseColor", new Color(0.74f, 0.76f, 0.78f, 1f));
            steel.SetFloat("_Metallic", 0.82f);
            steel.SetFloat("_Smoothness", 0.42f);
            EditorUtility.SetDirty(steel);
            AssetDatabase.SaveAssets();
        }

        private static Material LoadOrCreate(string path)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                return mat;
            }

            Material created = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static void ApplyRenderer(Renderer renderer, Material material)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
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
