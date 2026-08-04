using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ChoTet.Level1.Editor
{
    public static class HighwayHudFontSetup
    {
        private const string RegularTtfPath = "Assets/Resources/Level1/Fonts/Nunito-Regular.ttf";
        private const string SemiBoldTtfPath = "Assets/Resources/Level1/Fonts/Nunito-SemiBold.ttf";
        private const string RegularSdfPath = "Assets/Resources/Level1/Fonts/Nunito-Regular SDF.asset";
        private const string SemiBoldSdfPath = "Assets/Resources/Level1/Fonts/Nunito-SemiBold SDF.asset";

        private const string UiCharset =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
            " .,:;!?()-_/\\'\"@#&+%*[]{}<>=|~`" +
            "ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýăđĂĐƠơƯư" +
            "ẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰựỲỳỴỵỶỷỸỹ";

        public static void SetupHighwayHudFonts()
        {
            EnsureTmpEssentialResources();
            BakeFontAsset(RegularTtfPath, RegularSdfPath);
            BakeFontAsset(SemiBoldTtfPath, SemiBoldSdfPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HighwayHudFontSetup] Highway HUD Nunito SDF assets are ready.");
        }

        private static void EnsureTmpEssentialResources()
        {
            if (File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
            {
                return;
            }

            TMP_PackageResourceImporter.ImportResources(true, false, false);
        }

        private static void BakeFontAsset(string ttfPath, string sdfPath)
        {
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (sourceFont == null)
            {
                Debug.LogError("[HighwayHudFontSetup] Missing font file at " + ttfPath);
                return;
            }

            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(sdfPath);
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                6,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);
            if (fontAsset == null)
            {
                Debug.LogError("[HighwayHudFontSetup] Failed to create font asset for " + ttfPath);
                return;
            }

            fontAsset.name = Path.GetFileNameWithoutExtension(sdfPath);

            SerializedObject serializedFont = new SerializedObject(fontAsset);
            SerializedProperty clearOnBuild = serializedFont.FindProperty("m_ClearDynamicDataOnBuild");
            if (clearOnBuild != null)
            {
                clearOnBuild.boolValue = false;
                serializedFont.ApplyModifiedPropertiesWithoutUndo();
            }

            if (!fontAsset.TryAddCharacters(UiCharset, out string missingCharacters))
            {
                Debug.LogWarning("[HighwayHudFontSetup] Missing characters in " + fontAsset.name + ": " +
                                 missingCharacters);
            }

            AssetDatabase.CreateAsset(fontAsset, sdfPath);

            if (fontAsset.material != null)
            {
                fontAsset.material.name = fontAsset.name + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    Texture2D atlas = fontAsset.atlasTextures[i];
                    if (atlas == null)
                    {
                        continue;
                    }

                    atlas.name = fontAsset.name + " Atlas " + i;
                    AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
        }
    }
}
