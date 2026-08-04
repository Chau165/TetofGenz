#if UNITY_EDITOR
using ChoTet.Level1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Wide screening wall on the north side of the bus-stop road with a large eco-awareness banner.
    /// Does not rebuild or move existing SimplePoly across-road assets.
    /// </summary>
    public static class Level1AcrossRoadEcoWallBuilder
    {
        public const string RootName = "Level1_AcrossRoadEcoWall";

        private const string BannerTexturePath = "Assets/Image/Brand/vi_mot_hanh_tinh_xanh_sach_dep.png";
        private const string BannerMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_EcoPlanetBanner.mat";
        private const string WallMaterialPath = "Assets/Settings/ChoTet/Materials/Foundation_Stone.mat";

        private const float BaseScreenWidth = 20.4f;
        private const float WidthMultiplier = 10f;
        private const float WallHeight = 5.4f;
        private const float WallThickness = 0.38f;
        private const float BannerHeight = 4.65f;
        private const float BannerAspect = 1024f / 512f;
        private const float BannerFill = 0.94f;

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1AcrossRoadEcoWallBuilder] Exit Play mode before placing the eco wall.");
                return;
            }

            Transform busArea = GameObject.Find("Level1_OutdoorArea/Level1_BusPickupArea")?.transform;
            if (busArea == null)
            {
                Debug.LogError("[Level1AcrossRoadEcoWallBuilder] Level1_BusPickupArea not found.");
                return;
            }

            Build(busArea);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1AcrossRoadEcoWallBuilder] Across-road eco banner wall placed.");
        }

        public static void Build(Transform busPickupRoot)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
            Material wallMat = AssetDatabase.LoadAssetAtPath<Material>(WallMaterialPath);
            if (texture == null)
            {
                Debug.LogError("[Level1AcrossRoadEcoWallBuilder] Missing banner texture: " + BannerTexturePath);
                return;
            }

            if (wallMat == null)
            {
                Debug.LogError("[Level1AcrossRoadEcoWallBuilder] Missing wall material: " + WallMaterialPath);
                return;
            }

            Material bannerMat = EnsureBannerMaterial(texture);

            Vector3 position = ComputeDefaultPosition();
            Quaternion rotation = Quaternion.identity;
            Vector3 localScale = Vector3.one;

            Transform existing = busPickupRoot.Find(RootName);
            if (existing != null)
            {
                position = existing.position;
                rotation = existing.rotation;
                localScale = existing.localScale;
                Object.DestroyImmediate(existing.gameObject);
            }

            float wallSpanX = BaseScreenWidth * WidthMultiplier;
            float wallCenterZ = position.z;

            GameObject root = new GameObject(RootName);
            root.transform.SetParent(busPickupRoot, false);
            root.transform.position = position;
            root.transform.rotation = rotation;
            root.transform.localScale = localScale;
            root.transform.SetAsLastSibling();

            float surfaceY = Level1OutdoorSettings.WalkSurfaceY;
            float wallY = surfaceY + WallHeight * 0.5f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "EcoWall_Body";
            body.transform.SetParent(root.transform, false);
            body.transform.position = new Vector3(0f, wallY, wallCenterZ);
            body.transform.localScale = new Vector3(wallSpanX, WallHeight, WallThickness);
            ApplyWallRenderer(body, wallMat);

            float bannerWidth = wallSpanX * BannerFill;
            float bannerY = surfaceY + WallHeight * 0.52f;
            float bannerZ = wallCenterZ + WallThickness * 0.5f + 0.03f;

            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Quad);
            banner.name = "EcoWall_Banner";
            banner.transform.SetParent(root.transform, false);
            banner.transform.position = new Vector3(0f, bannerY, bannerZ);
            banner.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            banner.transform.localScale = new Vector3(bannerWidth, BannerHeight, 1f);
            ApplyBannerRenderer(banner, bannerMat);

            MarkStatic(root);
            Debug.Log($"[Level1AcrossRoadEcoWallBuilder] Wall span {wallSpanX:F1}m x {WallHeight:F1}m at Z={wallCenterZ:F2}, " +
                      $"banner {bannerWidth:F1}m x {BannerHeight:F1}m.");
        }

        private static Vector3 ComputeDefaultPosition()
        {
            float northShoulderOuterZ =
                Level1BusStopRoadSettings.LaneNorthEdgeZ - Level1BusStopRoadSettings.ShoulderDepth;
            float sidewalkCenterZ = northShoulderOuterZ - 2.35f * 0.5f - 0.06f;
            float buildingRowZ = sidewalkCenterZ - 2.35f * 0.5f - 1.05f;
            float wallCenterZ = buildingRowZ + 1.85f;
            return new Vector3(0f, 0f, wallCenterZ);
        }

        private static void ApplyWallRenderer(GameObject wall, Material wallMat)
        {
            Collider collider = wall.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = wall.GetComponent<Renderer>();
            Material tiled = new Material(wallMat);
            float spanX = wall.transform.localScale.x;
            float spanY = wall.transform.localScale.y;
            tiled.SetTextureScale("_BaseMap", new Vector2(Mathf.Max(1f, spanX / 2.4f), Mathf.Max(1f, spanY / 1.2f)));
            renderer.sharedMaterial = tiled;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private static void ApplyBannerRenderer(GameObject banner, Material bannerMat)
        {
            Collider collider = banner.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = banner.GetComponent<Renderer>();
            Material instance = new Material(bannerMat);
            instance.renderQueue = 3102;
            renderer.sharedMaterial = instance;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material EnsureBannerMaterial(Texture2D texture)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(BannerMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_EcoPlanetBanner";
                AssetDatabase.CreateAsset(mat, BannerMaterialPath);
            }

            mat.SetTexture("_BaseMap", texture);
            mat.SetTexture("_MainTex", texture);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void MarkStatic(GameObject root)
        {
            StaticEditorFlags flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(transforms[i].gameObject, flags);
            }
        }
    }
}
#endif
