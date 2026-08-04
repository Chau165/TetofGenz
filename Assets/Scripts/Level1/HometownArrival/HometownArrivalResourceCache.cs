using System.Collections.Generic;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    internal static class HometownArrivalResourceCache
    {
        private const string NatureLibraryPath = "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset";

        public static Material LoadMaterial(string fileName)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                ChoTetMarketEnvironment.MaterialsRoot + "/" + fileName);
#else
            return LoadOrCreateRuntimeMaterial(fileName);
#endif
        }

        public static HighwayNaturePrefabLibrary LoadNatureLibrary()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(NatureLibraryPath);
#else
            return null;
#endif
        }

        public static GameObject LoadPrefab(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
#else
            GameObject serializedPrefab = HometownArrivalBootstrap.LoadRuntimePrefab(assetPath);
            if (serializedPrefab != null)
            {
                return serializedPrefab;
            }

            string resourcesPath = ToResourcesPath(assetPath);
            return string.IsNullOrEmpty(resourcesPath)
                ? null
                : Resources.Load<GameObject>(resourcesPath);
#endif
        }

        public static Material LoadMaterialAtPath(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPath);
#else
            return LoadOrCreateRuntimeMaterial(GetFileName(assetPath));
#endif
        }

        public static Mesh LoadMeshAtPath(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
#else
            return null;
#endif
        }

#if !UNITY_EDITOR
        private static readonly Dictionary<string, Material> RuntimeMaterials =
            new Dictionary<string, Material>(System.StringComparer.OrdinalIgnoreCase);

        private static Material LoadOrCreateRuntimeMaterial(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            if (RuntimeMaterials.TryGetValue(fileName, out Material cached) && cached != null)
            {
                return cached;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader)
            {
                name = "HometownRuntime_" + fileName,
                enableInstancing = true
            };

            Color color = ResolveRuntimeColor(fileName);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", ResolveSmoothness(fileName));
            }

            RuntimeMaterials[fileName] = material;
            return material;
        }

        private static Color ResolveRuntimeColor(string fileName)
        {
            string name = fileName.ToLowerInvariant();
            if (name.Contains("asphalt") || name.Contains("roadsurface"))
            {
                return new Color(0.18f, 0.19f, 0.21f);
            }

            if (name.Contains("lanemark") || name.Contains("wall_light") || name.Contains("white"))
            {
                return new Color(0.94f, 0.92f, 0.82f);
            }

            if (name.Contains("edgemark") || name.Contains("gold") || name.Contains("orange"))
            {
                return new Color(0.95f, 0.72f, 0.2f);
            }

            if (name.Contains("grass") || name.Contains("garden") || name.Contains("foliage"))
            {
                return new Color(0.28f, 0.48f, 0.22f);
            }

            if (name.Contains("earth") || name.Contains("dirt") || name.Contains("wood_dark"))
            {
                return new Color(0.34f, 0.24f, 0.16f);
            }

            if (name.Contains("shoulder") || name.Contains("concrete") || name.Contains("stone")
                || name.Contains("metal") || name.Contains("guard"))
            {
                return new Color(0.58f, 0.57f, 0.54f);
            }

            if (name.Contains("glass"))
            {
                return new Color(0.32f, 0.58f, 0.72f);
            }

            if (name.Contains("red"))
            {
                return new Color(0.68f, 0.16f, 0.12f);
            }

            if (name.Contains("wood"))
            {
                return new Color(0.48f, 0.29f, 0.16f);
            }

            return new Color(0.72f, 0.68f, 0.58f);
        }

        private static float ResolveSmoothness(string fileName)
        {
            string name = fileName.ToLowerInvariant();
            return name.Contains("glass") || name.Contains("metal") || name.Contains("guard")
                ? 0.55f
                : 0.18f;
        }

        private static string GetFileName(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            int slash = assetPath.LastIndexOf('/');
            return slash >= 0 ? assetPath.Substring(slash + 1) : assetPath;
        }

        private static string ToResourcesPath(string assetPath)
        {
            const string resourcesRoot = "Assets/Resources/";
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(resourcesRoot, System.StringComparison.Ordinal))
            {
                return null;
            }

            string relative = assetPath.Substring(resourcesRoot.Length);
            int extension = relative.LastIndexOf('.');
            return extension > 0 ? relative.Substring(0, extension) : relative;
        }
#endif

        public static RuntimeAnimatorController LoadAnimatorController(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
#else
            return null;
#endif
        }
    }
}
