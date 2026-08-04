using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Controlled matte palette for highway roadside buildings.</summary>
    public static class RoadsideTownPalette
    {
        public const string Folder = "Assets/Materials/RoadsideTownPalette";

        public static readonly Color[] WallColors =
        {
            new Color(0.91f, 0.86f, 0.78f),
            new Color(0.93f, 0.91f, 0.87f),
            new Color(0.84f, 0.78f, 0.68f),
            new Color(0.90f, 0.87f, 0.74f),
            new Color(0.78f, 0.77f, 0.75f),
            new Color(0.86f, 0.80f, 0.70f),
            new Color(0.90f, 0.82f, 0.76f),
            new Color(0.72f, 0.78f, 0.84f),
            new Color(0.72f, 0.78f, 0.70f),
            new Color(0.82f, 0.70f, 0.62f)
        };

        public static readonly Color[] RoofColors =
        {
            new Color(0.42f, 0.41f, 0.40f),
            new Color(0.48f, 0.36f, 0.28f),
            new Color(0.58f, 0.40f, 0.32f),
            new Color(0.52f, 0.32f, 0.28f)
        };

        public static readonly Color[] WindowColors =
        {
            new Color(0.55f, 0.62f, 0.70f),
            new Color(0.50f, 0.58f, 0.66f),
            new Color(0.62f, 0.68f, 0.74f)
        };

        public static readonly Color[] DoorColors =
        {
            new Color(0.38f, 0.30f, 0.24f),
            new Color(0.32f, 0.38f, 0.30f),
            new Color(0.45f, 0.28f, 0.24f)
        };

        public static readonly Color[] TrimColors =
        {
            new Color(0.50f, 0.42f, 0.34f),
            new Color(0.56f, 0.48f, 0.40f),
            new Color(0.46f, 0.40f, 0.36f)
        };

        public static readonly string[] WallMaterials =
        {
            Folder + "/Wall_Cream.mat",
            Folder + "/Wall_WarmWhite.mat",
            Folder + "/Wall_Beige.mat",
            Folder + "/Wall_LightYellow.mat",
            Folder + "/Wall_LightGray.mat",
            Folder + "/Wall_Sand.mat",
            Folder + "/Wall_PalePeach.mat",
            Folder + "/Wall_SoftBlue.mat",
            Folder + "/Wall_MutedGreen.mat",
            Folder + "/Wall_LightTerracotta.mat"
        };

        public static readonly string[] RoofMaterials =
        {
            Folder + "/Roof_DarkGray.mat",
            Folder + "/Roof_Brown.mat",
            Folder + "/Roof_Terracotta.mat",
            Folder + "/Roof_RedBrown.mat"
        };

        public static readonly string[] WindowMaterials =
        {
            Folder + "/Window_BlueGray.mat",
            Folder + "/Window_Slate.mat",
            Folder + "/Window_Mist.mat"
        };

        public static readonly string[] DoorMaterials =
        {
            Folder + "/Door_DarkBrown.mat",
            Folder + "/Door_MutedGreen.mat",
            Folder + "/Door_RedBrown.mat"
        };

        public static readonly string[] TrimMaterials =
        {
            Folder + "/Trim_WoodBrown.mat",
            Folder + "/Trim_WarmTan.mat",
            Folder + "/Trim_DustyClay.mat"
        };
    }
}
