using ChoTet.Level1.Highway;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    public static class RoadsideTownPaletteFactory
    {
        private const float MatteSmoothness = 0.14f;

        public static void BuildPalette()
        {
            EnsureFolder(RoadsideTownPalette.Folder);

            CreateMat("Wall_Cream", new Color(0.91f, 0.86f, 0.78f));
            CreateMat("Wall_WarmWhite", new Color(0.93f, 0.91f, 0.87f));
            CreateMat("Wall_Beige", new Color(0.84f, 0.78f, 0.68f));
            CreateMat("Wall_LightYellow", new Color(0.90f, 0.87f, 0.74f));
            CreateMat("Wall_LightGray", new Color(0.78f, 0.77f, 0.75f));
            CreateMat("Wall_Sand", new Color(0.86f, 0.80f, 0.70f));
            CreateMat("Wall_PalePeach", new Color(0.90f, 0.82f, 0.76f));
            CreateMat("Wall_SoftBlue", new Color(0.72f, 0.78f, 0.84f));
            CreateMat("Wall_MutedGreen", new Color(0.72f, 0.78f, 0.70f));
            CreateMat("Wall_LightTerracotta", new Color(0.82f, 0.70f, 0.62f));

            CreateMat("Roof_DarkGray", new Color(0.42f, 0.41f, 0.40f));
            CreateMat("Roof_Brown", new Color(0.48f, 0.36f, 0.28f));
            CreateMat("Roof_Terracotta", new Color(0.58f, 0.40f, 0.32f));
            CreateMat("Roof_RedBrown", new Color(0.52f, 0.32f, 0.28f));

            CreateMat("Window_BlueGray", new Color(0.55f, 0.62f, 0.70f));
            CreateMat("Window_Slate", new Color(0.50f, 0.58f, 0.66f));
            CreateMat("Window_Mist", new Color(0.62f, 0.68f, 0.74f));

            CreateMat("Door_DarkBrown", new Color(0.38f, 0.30f, 0.24f));
            CreateMat("Door_MutedGreen", new Color(0.32f, 0.38f, 0.30f));
            CreateMat("Door_RedBrown", new Color(0.45f, 0.28f, 0.24f));

            CreateMat("Trim_WoodBrown", new Color(0.50f, 0.42f, 0.34f));
            CreateMat("Trim_WarmTan", new Color(0.56f, 0.48f, 0.40f));
            CreateMat("Trim_DustyClay", new Color(0.46f, 0.40f, 0.36f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RoadsideTownPaletteFactory] Built palette at " + RoadsideTownPalette.Folder);
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "RoadsideTownPalette");
            }
        }

        private static void CreateMat(string fileName, Color color)
        {
            string path = RoadsideTownPalette.Folder + "/" + fileName + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = existing != null ? existing : new Material(shader);

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", MatteSmoothness);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            if (existing == null)
            {
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                EditorUtility.SetDirty(mat);
            }
        }
    }
}
