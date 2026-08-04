#if UNITY_EDITOR
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ChoTet.Level1.Editor
{
    /// <summary>Bakes HCMC alley boundary wall and fence materials.</summary>
    public static class Level1VietnameseAlleyBoundaryMaterialBuilder
    {
        public const string TextureFolder = "Assets/Art/Level1/Outdoor/Textures/AlleyBoundary";
        public const string PlasterAlbedoPath = TextureFolder + "/Alley_PlasterWall_Albedo.png";
        public const string PlasterNormalPath = TextureFolder + "/Alley_PlasterWall_Normal.png";
        public const string FenceAlbedoPath = TextureFolder + "/Alley_CorrugatedFence_Albedo.png";
        public const string FenceNormalPath = TextureFolder + "/Alley_CorrugatedFence_Normal.png";
        public const string PlasterMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_Boundary_Plaster.mat";
        public const string FenceMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_Boundary_CorrugatedFence.mat";
        public const string PostMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_Boundary_FencePost.mat";
        public const string PipeMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/Alley_Boundary_Pipe.mat";

        public const float PlasterSheetMeters = 2.0f;
        public const float FenceSheetMeters = 1.2f;

        public static void BakeOnMenu()
        {
            BakeOrUpdateMaterials();
            AssetDatabase.SaveAssets();
            Debug.Log("[Level1VietnameseAlleyBoundaryMaterialBuilder] Alley boundary materials baked.");
        }

        public static void BakeOrUpdateMaterials()
        {
            RegenerateTextures();
            ConfigureTextureImporter(PlasterAlbedoPath, false, 1024);
            ConfigureTextureImporter(PlasterNormalPath, true, 1024);
            ConfigureTextureImporter(FenceAlbedoPath, false, 1024);
            ConfigureTextureImporter(FenceNormalPath, true, 1024);
            AssetDatabase.Refresh();

            BakePlasterMaterial();
            BakeFenceMaterial();
            BakePostMaterial();
            BakePipeMaterial();
        }

        private static void RegenerateTextures()
        {
            string folder = Application.dataPath + "/Art/Level1/Outdoor/Textures/AlleyBoundary";
            RunPython(folder + "/GenerateAlleyPlasterWall.py");
            RunPython(folder + "/GenerateAlleyCorrugatedFence.py");
            AssetDatabase.Refresh();
        }

        private static void RunPython(string scriptPath)
        {
            if (!System.IO.File.Exists(scriptPath))
            {
                Debug.LogWarning("[Level1VietnameseAlleyBoundaryMaterialBuilder] Missing: " + scriptPath);
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
            process?.WaitForExit();
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

        private static void BakePlasterMaterial()
        {
            Material mat = LoadOrCreate(PlasterMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            mat.shader = shader;
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(PlasterAlbedoPath));
            mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(PlasterNormalPath));
            mat.SetFloat("_BumpScale", 0.55f);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0.18f);
            EditorUtility.SetDirty(mat);
        }

        private static void BakeFenceMaterial()
        {
            Material mat = LoadOrCreate(FenceMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            mat.shader = shader;
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(FenceAlbedoPath));
            mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(FenceNormalPath));
            mat.SetFloat("_BumpScale", 0.75f);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Metallic", 0.35f);
            mat.SetFloat("_Smoothness", 0.22f);
            EditorUtility.SetDirty(mat);
        }

        private static void BakePostMaterial()
        {
            Material mat = LoadOrCreate(PostMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            mat.shader = shader;
            mat.SetColor("_BaseColor", new Color(0.62f, 0.64f, 0.66f, 1f));
            mat.SetFloat("_Metallic", 0.78f);
            mat.SetFloat("_Smoothness", 0.28f);
            EditorUtility.SetDirty(mat);
        }

        private static void BakePipeMaterial()
        {
            Material mat = LoadOrCreate(PipeMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            mat.shader = shader;
            mat.SetColor("_BaseColor", new Color(0.72f, 0.74f, 0.76f, 1f));
            mat.SetFloat("_Metallic", 0.55f);
            mat.SetFloat("_Smoothness", 0.32f);
            EditorUtility.SetDirty(mat);
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
    }
}
#endif
