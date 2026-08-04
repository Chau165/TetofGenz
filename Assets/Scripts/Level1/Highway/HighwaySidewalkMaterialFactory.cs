using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Bakes the single highway sidewalk material (URP Lit + Quaternius concrete albedo).</summary>
    public static class HighwaySidewalkMaterialFactory
    {
        public const string AlbedoTexturePath =
            "Assets/Exports/glTF (Godot)/T_MetalConcrete_BaseColor.png";

        public const string NormalTexturePath =
            "Assets/Exports/glTF (Godot)/T_MetalConcrete_Normal.png";

        /// <summary>Meters per texture repeat — large value keeps the surface reading as one slab from top-down.</summary>
        public const float WorldUvMetersPerRepeat = 36f;

#if UNITY_EDITOR
        public static Material BakeOrUpdateMaterialAsset()
        {
            Material asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                QuaterniusDowntownAssets.SidewalkPavementMaterialPath);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                return asset;
            }

            if (asset == null)
            {
                asset = new Material(shader);
                UnityEditor.AssetDatabase.CreateAsset(asset, QuaterniusDowntownAssets.SidewalkPavementMaterialPath);
            }
            else
            {
                asset.shader = shader;
            }

            Texture2D albedo = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoTexturePath);
            Texture2D normal = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(NormalTexturePath);
            if (albedo != null)
            {
                asset.SetTexture("_BaseMap", albedo);
                asset.SetTextureScale("_BaseMap", Vector2.one);
                asset.SetTextureOffset("_BaseMap", Vector2.zero);
            }

            if (normal != null)
            {
                asset.SetTexture("_BumpMap", normal);
                asset.SetFloat("_BumpScale", 0.35f);
            }

            asset.SetColor("_BaseColor", new Color(0.78f, 0.78f, 0.8f, 1f));
            asset.SetFloat("_Smoothness", 0.16f);
            asset.SetFloat("_Metallic", 0.04f);
            UnityEditor.EditorUtility.SetDirty(asset);
            return asset;
        }
#endif
    }
}
