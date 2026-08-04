#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Replaces the boarding-house flat roof with a low-budget Vietnamese corrugated metal roof (mái tôn).</summary>
    public static class Level1BoardingHouseRoofBuilder
    {
        private const string RoofRootName = "HouseMetalRoof";
        private const string MetalMaterialPath = "Assets/Settings/ChoTet/Materials/Roof_Metal_Corrugated.mat";
        private const string FasciaMaterialPath = "Assets/Settings/ChoTet/Materials/Roof_Metal_Fascia.mat";

        private const float WallHeight = 3.15f;
        private const float BackWallZ = -4.28f;
        private const float RoofWidth = 11.85f;
        private const float PitchDegrees = 10f;
        private const float OverhangMeters = 0.42f;
        private const float RoofBackZ = 0.12f;
        private const float BannerClearanceZ = -5.05f;

        public static void ReplaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1BoardingHouseRoofBuilder] Exit Play mode before replacing the roof.");
                return;
            }

            if (!TryFindHouseExterior(out Transform houseExterior))
            {
                Debug.LogError("[Level1BoardingHouseRoofBuilder] Level1_HouseExterior not found.");
                return;
            }

            Build(houseExterior);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1BoardingHouseRoofBuilder] Corrugated metal roof installed. Banner untouched.");
        }

        public static void Build(Transform shell)
        {
            if (shell == null)
            {
                return;
            }

            RemoveLegacyRoof(shell);

            Material metal = EnsureMetalMaterial();
            Material fasciaMat = EnsureFasciaMaterial();
            Material steel = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/ChoTet/Materials/Roof_Dark.mat");

            float frontWallFaceZ = BackWallZ - 0.17f;
            float eaveFrontZ = Mathf.Max(frontWallFaceZ - OverhangMeters, BannerClearanceZ + 0.18f);
            float runLength = RoofBackZ - eaveFrontZ;
            float pitchRad = PitchDegrees * Mathf.Deg2Rad;
            float rise = runLength * Mathf.Tan(pitchRad);
            float eaveY = WallHeight + 0.05f;
            float cosPitch = Mathf.Cos(pitchRad);

            GameObject roofRoot = new GameObject(RoofRootName);
            roofRoot.transform.SetParent(shell, false);
            roofRoot.transform.position = new Vector3(0f, eaveY, eaveFrontZ);
            roofRoot.transform.rotation = Quaternion.Euler(-PitchDegrees, 0f, 0f);

            float localDepth = runLength / cosPitch;
            BuildCorrugatedDeck(roofRoot.transform, localDepth, metal);
            BuildFascia(roofRoot.transform, fasciaMat);
            BuildOverhangSupports(shell, frontWallFaceZ, eaveFrontZ, eaveY, steel ?? metal);

            StripShadows(roofRoot);
        }

        private static void BuildCorrugatedDeck(Transform roofRoot, float localDepth, Material metal)
        {
            GameObject deckRoot = new GameObject("CorrugatedDeck");
            deckRoot.transform.SetParent(roofRoot, false);

            const float ridgeSpacing = 0.11f;
            const float ridgeWidth = 0.048f;
            const float troughWidth = 0.062f;
            const float sheetThickness = 0.016f;

            int count = Mathf.CeilToInt(RoofWidth / ridgeSpacing);
            float startX = -RoofWidth * 0.5f + ridgeSpacing * 0.5f;

            for (int i = 0; i < count; i++)
            {
                bool isRidge = i % 2 == 0;
                float w = isRidge ? ridgeWidth : troughWidth;
                float h = isRidge ? sheetThickness * 2.1f : sheetThickness;
                float x = startX + i * ridgeSpacing;

                CreateRoofCube(
                    isRidge ? "Ridge_" + i : "Trough_" + i,
                    deckRoot.transform,
                    new Vector3(x, h * 0.5f, localDepth * 0.5f),
                    new Vector3(w, h, localDepth),
                    metal);
            }
        }

        private static void BuildFascia(Transform roofRoot, Material fasciaMat)
        {
            const float fasciaHeight = 0.085f;
            const float fasciaDepth = 0.022f;

            CreateRoofCube("RoofFascia", roofRoot,
                new Vector3(0f, -fasciaHeight * 0.35f, -fasciaDepth * 0.35f),
                new Vector3(RoofWidth + 0.08f, fasciaHeight, fasciaDepth),
                fasciaMat);
        }

        private static void BuildOverhangSupports(Transform shell, float frontWallFaceZ, float eaveFrontZ,
            float eaveY, Material steel)
        {
            GameObject supports = new GameObject("RoofOverhangSupports");
            supports.transform.SetParent(shell, false);

            float supportTopY = eaveY - 0.04f;
            float supportBottomY = WallHeight - 0.02f;
            float supportHeight = supportTopY - supportBottomY;
            float supportZ = (frontWallFaceZ + eaveFrontZ) * 0.5f;
            float[] xs = { -4.35f, -1.55f, 1.55f, 4.35f };

            for (int i = 0; i < xs.Length; i++)
            {
                CreateRoofCube("RoofSupport_" + i, supports.transform,
                    new Vector3(xs[i], supportBottomY + supportHeight * 0.5f, supportZ),
                    new Vector3(0.055f, supportHeight, 0.055f),
                    steel);
            }

            CreateRoofCube("RoofGutterLip", supports.transform,
                new Vector3(0f, supportTopY - 0.02f, eaveFrontZ + 0.03f),
                new Vector3(RoofWidth + 0.04f, 0.028f, 0.05f),
                steel);

            StripShadows(supports);
        }

        private static void RemoveLegacyRoof(Transform shell)
        {
            string[] legacyNames =
            {
                "HouseRoof",
                "HouseRoofOverhang",
                "RoofOverhangSupports",
                RoofRootName
            };

            for (int i = 0; i < legacyNames.Length; i++)
            {
                Transform child;
                while ((child = shell.Find(legacyNames[i])) != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
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

        private static GameObject CreateRoofCube(string name, Transform parent, Vector3 localPos, Vector3 localScale,
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

        private static Material EnsureMetalMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MetalMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = "Roof_Metal_Corrugated";
                AssetDatabase.CreateAsset(mat, MetalMaterialPath);
            }

            Color weathered = new Color(0.34f, 0.36f, 0.39f);
            mat.SetColor("_BaseColor", weathered);
            mat.SetColor("_Color", weathered);
            mat.SetFloat("_Metallic", 0.62f);
            mat.SetFloat("_Smoothness", 0.28f);
            mat.SetFloat("_Glossiness", 0.28f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureFasciaMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(FasciaMaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = "Roof_Metal_Fascia";
                AssetDatabase.CreateAsset(mat, FasciaMaterialPath);
            }

            Color fascia = new Color(0.28f, 0.30f, 0.33f);
            mat.SetColor("_BaseColor", fascia);
            mat.SetColor("_Color", fascia);
            mat.SetFloat("_Metallic", 0.55f);
            mat.SetFloat("_Smoothness", 0.22f);
            mat.SetFloat("_Glossiness", 0.22f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void StripShadows(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
            }
        }
    }
}
#endif
