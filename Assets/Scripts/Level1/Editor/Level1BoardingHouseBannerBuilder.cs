#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Mounts the Kim Lien boarding-house brand banner on the outdoor facade.</summary>
    public static class Level1BoardingHouseBannerBuilder
    {
        private const string BannerRootName = "BoardingHouseBanner";
        private const string BannerTexturePath = "Assets/Image/Brand/nha_tro_kim_lien.png";
        private const string MaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Brand_NhaTroKimLien.mat";

        private const float BannerAspect = 1774f / 887f;
        private const float BannerHeight = 1.32f;
        private const float FramePad = 0.055f;
        private const float FrameDepth = 0.04f;

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1BoardingHouseBannerBuilder] Exit Play mode before placing the banner.");
                return;
            }

            if (!TryFindHouseExterior(out Transform houseExterior))
            {
                Debug.LogError("[Level1BoardingHouseBannerBuilder] Level1_HouseExterior not found.");
                return;
            }

            Build(houseExterior);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1BoardingHouseBannerBuilder] Kim Lien banner placed on boarding-house facade.");
        }

        public static void Build(Transform houseExterior)
        {
            if (houseExterior == null)
            {
                return;
            }

            RemoveExisting(houseExterior);

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
            if (texture == null)
            {
                Debug.LogError("[Level1BoardingHouseBannerBuilder] Missing texture: " + BannerTexturePath);
                return;
            }

            Material wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/ChoTet/Materials/Wood_Warm.mat");
            Material bannerMat = EnsureBannerMaterial(texture);

            const float backWallZ = -4.28f;
            const float wallHeight = 3.15f;
            const float roofCenterZ = -2.05f;
            const float roofHalfDepth = 6.1f * 0.5f;
            const float overhangFrontZ = backWallZ - 0.22f - 0.275f;
            float roofStreetFrontZ = roofCenterZ - roofHalfDepth;
            float facadeFrontZ = Mathf.Min(overhangFrontZ, roofStreetFrontZ);
            float bannerCenterY = 2.88f;
            float bannerZ = facadeFrontZ - 0.16f;
            float bannerWidth = BannerHeight * BannerAspect;

            GameObject root = new GameObject(BannerRootName);
            root.transform.SetParent(houseExterior, false);
            root.transform.position = new Vector3(0f, bannerCenterY, bannerZ);
            root.transform.SetAsLastSibling();

            CreateFrameCube("Banner_FrameTop", root.transform, new Vector3(0f, BannerHeight * 0.5f + FramePad, 0f),
                new Vector3(bannerWidth + FramePad * 2f, FramePad, FrameDepth), wood);
            CreateFrameCube("Banner_FrameBottom", root.transform, new Vector3(0f, -BannerHeight * 0.5f - FramePad, 0f),
                new Vector3(bannerWidth + FramePad * 2f, FramePad, FrameDepth), wood);
            CreateFrameCube("Banner_FrameLeft", root.transform,
                new Vector3(-bannerWidth * 0.5f - FramePad, 0f, 0f),
                new Vector3(FramePad, BannerHeight + FramePad * 2f, FrameDepth), wood);
            CreateFrameCube("Banner_FrameRight", root.transform,
                new Vector3(bannerWidth * 0.5f + FramePad, 0f, 0f),
                new Vector3(FramePad, BannerHeight + FramePad * 2f, FrameDepth), wood);

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Banner_Sign";
            quad.transform.SetParent(root.transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = new Vector3(bannerWidth, BannerHeight, 1f);

            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = quad.GetComponent<Renderer>();
            Material bannerInstance = new Material(bannerMat);
            bannerInstance.SetTextureScale("_BaseMap", new Vector2(-1f, 1f));
            bannerInstance.SetTextureOffset("_BaseMap", new Vector2(1f, 0f));
            bannerInstance.SetTextureScale("_MainTex", new Vector2(-1f, 1f));
            bannerInstance.SetTextureOffset("_MainTex", new Vector2(1f, 0f));
            bannerInstance.renderQueue = 3102;
            renderer.sharedMaterial = bannerInstance;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            MarkStatic(root);
        }

        private static bool TryFindHouseExterior(out Transform houseExterior)
        {
            Transform world = GameObject.Find("Level1World")?.transform;
            if (world != null)
            {
                houseExterior = world.Find("Level1_OutdoorArea/Level1_HouseExterior");
                if (houseExterior == null)
                {
                    houseExterior = world.Find("Level1_HouseExterior");
                }

                if (houseExterior != null)
                {
                    return true;
                }
            }

            GameObject found = GameObject.Find("Level1_HouseExterior");
            houseExterior = found != null ? found.transform : null;
            return houseExterior != null;
        }

        private static void RemoveExisting(Transform houseExterior)
        {
            Transform existing = houseExterior.Find(BannerRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static GameObject CreateFrameCube(string name, Transform parent, Vector3 localPos, Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return cube;
        }

        private static Material EnsureBannerMaterial(Texture2D texture)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
                mat = new Material(shader);
                mat.name = "Brand_NhaTroKimLien";
                AssetDatabase.CreateAsset(mat, MaterialPath);
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
            MarkStaticRecursive(root, flags);
        }

        private static void MarkStaticRecursive(GameObject go, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(go, flags);
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                MarkStaticRecursive(t.GetChild(i).gameObject, flags);
            }
        }
    }
}
#endif
