#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Menu.Editor
{
    public static class KenneyRpgUiSetup
    {
        private const string LibraryAssetPath = "Assets/Resources/Menu/KenneyRpgUiLibrary.asset";

        private static readonly Dictionary<string, Vector4> SpriteBorders = new Dictionary<string, Vector4>
        {
            { "buttonLong_brown.png", new Vector4(32f, 32f, 16f, 16f) },
            { "buttonLong_brown_pressed.png", new Vector4(32f, 32f, 14f, 14f) },
            { "buttonLong_beige.png", new Vector4(32f, 32f, 16f, 16f) },
            { "buttonLong_beige_pressed.png", new Vector4(32f, 32f, 14f, 14f) },
            { "buttonRound_brown.png", new Vector4(12f, 12f, 12f, 12f) },
            { "buttonSquare_brown.png", new Vector4(12f, 12f, 12f, 12f) },
            { "buttonSquare_brown_pressed.png", new Vector4(12f, 12f, 12f, 12f) },
            { "panel_brown.png", new Vector4(20f, 20f, 20f, 20f) },
            { "panel_beige.png", new Vector4(20f, 20f, 20f, 20f) },
            { "panel_beigeLight.png", new Vector4(20f, 20f, 20f, 20f) },
            { "panelInset_beige.png", new Vector4(16f, 16f, 16f, 16f) },
            { "panelInset_beigeLight.png", new Vector4(16f, 16f, 16f, 16f) },
            { "panelInset_brown.png", new Vector4(16f, 16f, 16f, 16f) },
            { "barYellow_horizontalMid.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barYellow_verticalMid.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barRed_horizontalMid.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barGreen_horizontalLeft.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barGreen_horizontalMid.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barGreen_horizontalRight.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barGreen_verticalTop.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barGreen_verticalMid.png", new Vector4(4f, 4f, 4f, 4f) },
            { "barGreen_verticalBottom.png", new Vector4(4f, 4f, 4f, 4f) },
            { "iconCircle_beige.png", new Vector4(0f, 0f, 0f, 0f) },
            { "iconCross_brown.png", new Vector4(0f, 0f, 0f, 0f) }
        };

        public static void SetupKenneyAssets()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Menu");

            ConfigureSprite(KenneyRpgUiPaths.ButtonLongBrown);
            ConfigureSprite(KenneyRpgUiPaths.ButtonLongBrownPressed);
            ConfigureSprite(KenneyRpgUiPaths.ButtonLongBeige);
            ConfigureSprite(KenneyRpgUiPaths.ButtonLongBeigePressed);
            ConfigureSprite(KenneyRpgUiPaths.ButtonRoundBrown);
            ConfigureSprite(KenneyRpgUiPaths.ButtonSquareBrown);
            ConfigureSprite(KenneyRpgUiPaths.ButtonSquareBrownPressed);
            ConfigureWoodBackground(KenneyRpgUiPaths.WoodPlankBackground);
            ConfigureWoodBackground(KenneyRpgUiPaths.MenuVillageBackground);
            ConfigureSprite(KenneyRpgUiPaths.PanelBrown);
            ConfigureSprite(KenneyRpgUiPaths.PanelBeige);
            ConfigureSprite(KenneyRpgUiPaths.PanelBeigeLight);
            ConfigureSprite(KenneyRpgUiPaths.PanelInsetBeige);
            ConfigureSprite(KenneyRpgUiPaths.PanelInsetBeigeLight);
            ConfigureSprite(KenneyRpgUiPaths.PanelInsetBrown);
            ConfigureSprite(KenneyRpgUiPaths.BarYellowMid);
            ConfigureSprite(KenneyRpgUiPaths.BarYellowVertical);
            ConfigureSprite(KenneyRpgUiPaths.BarRedHorizontal);
            ConfigureSprite(KenneyRpgUiPaths.BarGreenHorizontalLeft);
            ConfigureSprite(KenneyRpgUiPaths.BarGreenHorizontalMid);
            ConfigureSprite(KenneyRpgUiPaths.BarGreenHorizontalRight);
            ConfigureSprite(KenneyRpgUiPaths.BarGreenVerticalTop);
            ConfigureSprite(KenneyRpgUiPaths.BarGreenVerticalMid);
            ConfigureSprite(KenneyRpgUiPaths.BarGreenVerticalBottom);
            ConfigureSprite(KenneyRpgUiPaths.VineLeaf);
            ConfigureSprite(KenneyRpgUiPaths.IconCrossBrown);
            ConfigureMenuFonts();

            AssetDatabase.Refresh();

            KenneyRpgUiLibrary library = AssetDatabase.LoadAssetAtPath<KenneyRpgUiLibrary>(LibraryAssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<KenneyRpgUiLibrary>();
                AssetDatabase.CreateAsset(library, LibraryAssetPath);
            }

            library.buttonLongBrown = LoadSprite(KenneyRpgUiPaths.ButtonLongBrown);
            library.buttonLongBrownPressed = LoadSprite(KenneyRpgUiPaths.ButtonLongBrownPressed);
            library.buttonLongBeige = LoadSprite(KenneyRpgUiPaths.ButtonLongBeige);
            library.buttonLongBeigePressed = LoadSprite(KenneyRpgUiPaths.ButtonLongBeigePressed);
            library.buttonRoundBrown = LoadSprite(KenneyRpgUiPaths.ButtonRoundBrown);
            library.buttonSquareBrown = LoadSprite(KenneyRpgUiPaths.ButtonSquareBrown);
            library.buttonSquareBrownPressed = LoadSprite(KenneyRpgUiPaths.ButtonSquareBrownPressed);
            library.woodPlankBackground = LoadTexture(KenneyRpgUiPaths.WoodPlankBackground);
            library.menuVillageBackground = LoadTexture(KenneyRpgUiPaths.MenuVillageBackground);
            library.panelBrown = LoadSprite(KenneyRpgUiPaths.PanelBrown);
            library.panelBeige = LoadSprite(KenneyRpgUiPaths.PanelBeige);
            library.panelBeigeLight = LoadSprite(KenneyRpgUiPaths.PanelBeigeLight);
            library.panelInsetBeige = LoadSprite(KenneyRpgUiPaths.PanelInsetBeige);
            library.panelInsetBeigeLight = LoadSprite(KenneyRpgUiPaths.PanelInsetBeigeLight);
            library.panelInsetBrown = LoadSprite(KenneyRpgUiPaths.PanelInsetBrown);
            library.barYellowMid = LoadSprite(KenneyRpgUiPaths.BarYellowMid);
            library.barYellowVertical = LoadSprite(KenneyRpgUiPaths.BarYellowVertical);
            library.barRedHorizontal = LoadSprite(KenneyRpgUiPaths.BarRedHorizontal);
            library.barGreenHorizontalLeft = LoadSprite(KenneyRpgUiPaths.BarGreenHorizontalLeft);
            library.barGreenHorizontalMid = LoadSprite(KenneyRpgUiPaths.BarGreenHorizontalMid);
            library.barGreenHorizontalRight = LoadSprite(KenneyRpgUiPaths.BarGreenHorizontalRight);
            library.barGreenVerticalTop = LoadSprite(KenneyRpgUiPaths.BarGreenVerticalTop);
            library.barGreenVerticalMid = LoadSprite(KenneyRpgUiPaths.BarGreenVerticalMid);
            library.barGreenVerticalBottom = LoadSprite(KenneyRpgUiPaths.BarGreenVerticalBottom);
            library.vineLeaf = LoadSprite(KenneyRpgUiPaths.VineLeaf);
            library.iconCrossBrown = LoadSprite(KenneyRpgUiPaths.IconCrossBrown);

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("[KenneyRpgUiSetup] Kenney RPG UI library and menu fonts ready at " + LibraryAssetPath);
        }

        private static void ConfigureMenuFonts()
        {
            ConfigureFont("Assets/Resources/Menu/Fonts/Almendra-Bold.ttf");
            ConfigureFont("Assets/Resources/Menu/Fonts/Almendra-Regular.ttf");
        }

        private static void ConfigureFont(string assetPath)
        {
            TrueTypeFontImporter importer = AssetImporter.GetAtPath(assetPath) as TrueTypeFontImporter;
            if (importer == null)
            {
                Debug.LogWarning("[KenneyRpgUiSetup] Missing menu font: " + assetPath);
                return;
            }

            importer.fontSize = 64;
            importer.fontTextureCase = FontTextureCase.Dynamic;
            importer.fontRenderingMode = FontRenderingMode.Smooth;
            importer.characterPadding = 2;
            importer.includeFontData = true;
            importer.SaveAndReimport();
        }

        private static void ConfigureWoodBackground(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("[KenneyRpgUiSetup] Missing wood background texture: " + assetPath);
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static void ConfigureSprite(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("[KenneyRpgUiSetup] Missing texture: " + assetPath);
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            string fileName = Path.GetFileName(assetPath);
            if (SpriteBorders.TryGetValue(fileName, out Vector4 border))
            {
                importer.spriteBorder = border;
            }

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static Sprite LoadSprite(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Texture2D LoadTexture(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
