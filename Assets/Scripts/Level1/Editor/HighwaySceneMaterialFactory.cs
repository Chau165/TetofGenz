#if UNITY_EDITOR
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Creates highway-scene materials under Assets/Level1/HighWay without editing shared assets.</summary>
    public static class HighwaySceneMaterialFactory
    {
        public static void EnsureFolders()
        {
            EnsureFolder(HighwaySceneAssetPaths.Root);
            EnsureFolder(HighwaySceneAssetPaths.MaterialsFolder);
            EnsureFolder(HighwaySceneAssetPaths.PrefabsFolder);
            EnsureFolder(HighwaySceneAssetPaths.EnvironmentFolder);
        }

        public static Material EnsureLit(
            string fileName,
            Color baseColor,
            float smoothness = 0.45f,
            float metallic = 0f,
            bool transparent = false)
        {
            EnsureFolders();
            string path = HighwaySceneAssetPaths.MaterialPath(fileName);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.renderQueue = (int)RenderQueue.Transparent - 10;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CopySharedOrCreate(
            string highwayFileName,
            string sharedRelativePath,
            System.Action<Material> customize = null)
        {
            EnsureFolders();
            string highwayPath = HighwaySceneAssetPaths.MaterialPath(highwayFileName);
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(highwayPath);
            if (existing != null)
            {
                customize?.Invoke(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            string sharedPath = ChoTetMarketEnvironment.MaterialsRoot + "/" + sharedRelativePath;
            Material source = AssetDatabase.LoadAssetAtPath<Material>(sharedPath);
            Material mat = source != null ? new Material(source) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            customize?.Invoke(mat);
            AssetDatabase.CreateAsset(mat, highwayPath);
            return mat;
        }

        public static Material CopyTextureMaterial(
            string highwayFileName,
            string texturePath,
            Color tint,
            Vector2 tiling)
        {
            EnsureFolders();
            string path = HighwaySceneAssetPaths.MaterialPath(highwayFileName);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTexture("_MainTex", tex);
            }

            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_Color", tint);
            mat.SetTextureScale("_BaseMap", tiling);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material EnsureSkyboxCopy()
        {
            const string studySky = "Assets/Settings/ChoTet/Materials/Level1/Level1_StudyMorningSky.mat";
            Material source = AssetDatabase.LoadAssetAtPath<Material>(studySky);
            EnsureFolders();
            string path = HighwaySceneAssetPaths.MaterialPath(HighwaySceneAssetPaths.SkyMat);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = source != null
                    ? new Material(source)
                    : new Material(Shader.Find("Skybox/Procedural"));
                AssetDatabase.CreateAsset(mat, path);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string folder = path.Substring(slash + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
