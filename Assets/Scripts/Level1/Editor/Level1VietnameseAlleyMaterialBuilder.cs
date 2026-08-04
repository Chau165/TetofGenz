#if UNITY_EDITOR
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ChoTet.Level1.Editor
{
    /// <summary>Bakes HCMC alley sidewalk + worn asphalt materials from generated tile textures.</summary>
    public static class Level1VietnameseAlleyMaterialBuilder
    {
        public const string TextureFolder = "Assets/Art/Level1/Outdoor/Textures/AlleyGround";
        public const string SidewalkAlbedoPath = TextureFolder + "/Alley_Sidewalk_Albedo.png";
        public const string SidewalkNormalPath = TextureFolder + "/Alley_Sidewalk_Normal.png";
        public const string AsphaltAlbedoPath = TextureFolder + "/Alley_Asphalt_Albedo.png";
        public const string AsphaltNormalPath = TextureFolder + "/Alley_Asphalt_Normal.png";
        public const string SidewalkMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_Sidewalk_Pavement.mat";
        public const string AsphaltMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_Road_Asphalt.mat";

        public const float SidewalkSheetMeters = 3.6f;
        public const float AsphaltSheetMeters = 4.0f;
        public const float SidewalkTileMeters = 0.45f;

        public static void BakeOnMenu()
        {
            BakeOrUpdateMaterials();
            AssetDatabase.SaveAssets();
            Debug.Log("[Level1VietnameseAlleyMaterialBuilder] Alley sidewalk and asphalt materials baked.");
        }

        public static void BakeOrUpdateMaterials()
        {
            RegenerateTextures();
            ConfigureTextureImporter(SidewalkAlbedoPath, isNormal: false, maxSize: 1024);
            ConfigureTextureImporter(SidewalkNormalPath, isNormal: true, maxSize: 1024);
            ConfigureTextureImporter(AsphaltAlbedoPath, isNormal: false, maxSize: 1024);
            ConfigureTextureImporter(AsphaltNormalPath, isNormal: true, maxSize: 1024);
            AssetDatabase.Refresh();

            BakeSidewalkMaterial();
            BakeAsphaltMaterial();
        }

        private static void RegenerateTextures()
        {
            string pythonFolder = Application.dataPath + "/Art/Level1/Outdoor/Textures/AlleyGround";
            RunPython(pythonFolder + "/GenerateAlleySidewalkTiles.py");
            RunPython(pythonFolder + "/GenerateAlleyAsphalt.py");
            AssetDatabase.Refresh();
        }

        private static void EnsureGeneratedTextures()
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(SidewalkAlbedoPath) != null
                && AssetDatabase.LoadAssetAtPath<Texture2D>(AsphaltAlbedoPath) != null)
            {
                return;
            }

            RegenerateTextures();
        }

        private static void RunPython(string scriptPath)
        {
            if (!System.IO.File.Exists(scriptPath))
            {
                Debug.LogWarning("[Level1VietnameseAlleyMaterialBuilder] Missing generator: " + scriptPath);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "\"" + scriptPath + "\"",
                WorkingDirectory = System.IO.Path.GetDirectoryName(scriptPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo);
            if (process == null)
            {
                return;
            }

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Debug.LogWarning("[Level1VietnameseAlleyMaterialBuilder] Python failed: " + scriptPath + "\n"
                    + process.StandardError.ReadToEnd());
            }
        }

        private static void ConfigureTextureImporter(string path, bool isNormal, int maxSize)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !isNormal;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.maxTextureSize = maxSize;
            importer.SaveAndReimport();
        }

        private static void BakeSidewalkMaterial()
        {
            Material material = LoadOrCreate(SidewalkMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                return;
            }

            material.shader = shader;
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(SidewalkAlbedoPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(SidewalkNormalPath);
            if (albedo != null)
            {
                material.SetTexture("_BaseMap", albedo);
                material.SetTextureScale("_BaseMap", Vector2.one);
            }

            if (normal != null)
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normal);
                material.SetTextureScale("_BumpMap", Vector2.one);
                material.SetFloat("_BumpScale", 0.48f);
            }

            material.SetColor("_BaseColor", new Color(0.94f, 0.95f, 0.96f, 1f));
            material.SetFloat("_Smoothness", 0.12f);
            material.SetFloat("_Metallic", 0.01f);
            EditorUtility.SetDirty(material);
        }

        private static void BakeAsphaltMaterial()
        {
            Material material = LoadOrCreate(AsphaltMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                return;
            }

            material.shader = shader;
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AsphaltAlbedoPath);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(AsphaltNormalPath);
            if (albedo != null)
            {
                material.SetTexture("_BaseMap", albedo);
                material.SetTextureScale("_BaseMap", Vector2.one);
            }

            if (normal != null)
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normal);
                material.SetTextureScale("_BumpMap", Vector2.one);
                material.SetFloat("_BumpScale", 0.55f);
            }

            material.SetColor("_BaseColor", new Color(0.78f, 0.79f, 0.80f, 1f));
            material.SetFloat("_Smoothness", 0.06f);
            material.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(material);
        }

        private static Material LoadOrCreate(string path)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif
