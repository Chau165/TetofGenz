using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    internal static class HighwayNatureMaterialFactory
    {
        private const string MaterialsRoot = ChoTetMarketEnvironment.MaterialsRoot;
        private const string GrassSource = MaterialsRoot + "/Highway_Grass_NSK2.mat";

        public static Material LoadOrCreate(string fileName, string textureAssetPath, Color tint, Vector2 tileScale)
        {
            string path = MaterialsRoot + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            if (!AssetDatabase.CopyAsset(GrassSource, path))
            {
                Debug.LogError("[HighwayNatureMaterialFactory] Failed to copy base material to " + path);
                return null;
            }

            AssetDatabase.SaveAssets();
            material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                return null;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            if (texture != null)
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTexture("_MainTex", texture);
                material.SetTextureScale("_BaseMap", tileScale);
            }

            material.SetColor("_BaseColor", tint);
            material.SetColor("_Color", tint);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        public static Material LoadOrCreateGrassBlade(string fileName, string textureAssetPath, Color tint)
        {
            Material material = LoadOrCreate(fileName, textureAssetPath, tint, Vector2.one);
            if (material == null)
            {
                return null;
            }

            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.38f);
            material.SetFloat("_Surface", 0f);
            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            material.SetColor("_BaseColor", tint);
            material.SetColor("_Color", tint);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            material.doubleSidedGI = true;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
