#if UNITY_EDITOR
using System.IO;
using ChoTet.Level1.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1.Editor
{
    public static class VehicleBookingNotePrefabBuilder
    {
        public const string PrefabPath = "Assets/Prefabs/UI/VehicleBookingNote.prefab";
        public const string ArtFolder = "Assets/Art/UI/VehicleBookingNote";

        private static readonly Color PaperCream = new Color(0.99f, 0.97f, 0.93f, 1f);
        private static readonly Color InkBlack = new Color(0.06f, 0.05f, 0.05f, 1f);
        private static readonly Color InkSoft = new Color(0.22f, 0.19f, 0.17f, 1f);
        private static readonly Color TitleUnderline = new Color(0.06f, 0.05f, 0.05f, 0.2f);
        private static readonly Color IconCircleTint = new Color(0.96f, 0.92f, 0.84f, 0.88f);
        private static readonly Color HighlightYellow = new Color(1f, 0.96f, 0.74f, 0.9f);
        private static readonly Color HighlightMint = new Color(0.82f, 0.96f, 0.88f, 0.9f);
        private static readonly Color HighlightSky = new Color(0.86f, 0.93f, 1f, 0.9f);
        private static readonly Color DoodleRed = new Color(0.88f, 0.36f, 0.4f, 0.65f);

        public static void BuildPrefabMenu()
        {
            GameObject prefab = BuildOrUpdatePrefab();
            if (prefab != null)
            {
                Selection.activeObject = prefab;
                Debug.Log("[VehicleBookingNotePrefabBuilder] Saved note prefab at " + PrefabPath);
            }
        }

        public static GameObject BuildOrUpdatePrefab()
        {
            HighwayHudFontSetup.SetupHighwayHudFonts();
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/UI");
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/UI");
            EnsureFolder(ArtFolder);

            NoteSprites sprites = GenerateSprites();

            GameObject root = new GameObject("VehicleBookingNote", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(VehicleBookingNoteView));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(520f, 660f);
            rootRect.anchoredPosition = Vector2.zero;

            // Soft drop shadow behind paper.
            Image dropShadow = CreateImage("DropShadow", root.transform, sprites.Shadow, new Color(0f, 0f, 0f, 0.22f));
            RectTransform shadowRect = dropShadow.rectTransform;
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(-10f, -14f);
            shadowRect.offsetMax = new Vector2(12f, 8f);

            GameObject paperRoot = new GameObject("Paper", typeof(RectTransform));
            paperRoot.transform.SetParent(root.transform, false);
            Stretch(paperRoot.GetComponent<RectTransform>());

            Image paper = CreateImage("PaperBackground", paperRoot.transform, sprites.Paper, Color.white);
            Stretch(paper.rectTransform);
            Shadow paperShadow = paper.gameObject.AddComponent<Shadow>();
            paperShadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
            paperShadow.effectDistance = new Vector2(3f, -4f);
            paperShadow.useGraphicAlpha = true;

            Image tape = CreateImage("TapeStrip", paperRoot.transform, sprites.Tape,
                new Color(0.96f, 0.9f, 0.66f, 0.72f));
            LayoutRect(tape.rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 22f), new Vector2(152f, 38f));

            Image cornerCurl = CreateImage("CornerCurl", paperRoot.transform, sprites.CornerCurl,
                new Color(0.88f, 0.82f, 0.72f, 0.55f));
            LayoutRect(cornerCurl.rectTransform,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-4f, 4f), new Vector2(64f, 52f));

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(paperRoot.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            Stretch(contentRect, new Vector2(82f, 44f), new Vector2(-38f, -38f));

            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = 0f;
            contentLayout.padding = new RectOffset(10, 14, 40, 12);

            TMP_FontAsset regularFont = LoadFont("Assets/Resources/Level1/Fonts/Nunito-Regular SDF.asset");
            TMP_FontAsset semiBoldFont = LoadFont("Assets/Resources/Level1/Fonts/Nunito-SemiBold SDF.asset");

            GameObject titleBlock = new GameObject("TitleBlock", typeof(RectTransform));
            titleBlock.transform.SetParent(content.transform, false);
            LayoutElement titleBlockLayout = titleBlock.AddComponent<LayoutElement>();
            titleBlockLayout.minHeight = 92f;
            titleBlockLayout.preferredHeight = 92f;

            TextMeshProUGUI titleText = CreateLabel("Title", titleBlock.transform, semiBoldFont, 28, InkBlack,
                FontStyles.Bold);
            titleText.text = "THÔNG TIN XE ĐÃ ĐẶT";
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.characterSpacing = 0.8f;
            Stretch(titleText.rectTransform, new Vector2(6f, 8f), new Vector2(-6f, -26f));

            Image titleUnderline = CreateImage("TitleUnderline", titleBlock.transform, sprites.Highlight,
                TitleUnderline);
            LayoutRect(titleUnderline.rectTransform,
                new Vector2(0.12f, 0.06f), new Vector2(0.88f, 0.06f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(0f, 2f));

            CreateSparkle(titleBlock.transform, sprites.Sparkle, new Vector2(0.78f, 0.9f), 11f, 10f);

            GameObject titleSpacer = new GameObject("TitleSpacer", typeof(RectTransform), typeof(LayoutElement));
            titleSpacer.transform.SetParent(content.transform, false);
            LayoutElement titleSpacerLayout = titleSpacer.GetComponent<LayoutElement>();
            titleSpacerLayout.minHeight = 10f;
            titleSpacerLayout.preferredHeight = 10f;

            GameObject rows = new GameObject("InfoRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rows.transform.SetParent(content.transform, false);
            VerticalLayoutGroup rowsLayout = rows.GetComponent<VerticalLayoutGroup>();
            rowsLayout.childAlignment = TextAnchor.UpperLeft;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = true;
            rowsLayout.childForceExpandWidth = true;
            rowsLayout.childForceExpandHeight = false;
            rowsLayout.spacing = 14f;
            rowsLayout.padding = new RectOffset(4, 10, 4, 0);
            LayoutElement rowsLayoutElement = rows.AddComponent<LayoutElement>();
            rowsLayoutElement.flexibleHeight = 1f;
            rowsLayoutElement.minHeight = 200f;

            TextMeshProUGUI licensePlateLabel;
            TextMeshProUGUI licensePlateValue;
            TextMeshProUGUI pickupTimeLabel;
            TextMeshProUGUI pickupTimeValue;
            TextMeshProUGUI carColorLabel;
            TextMeshProUGUI carColorValue;

            CreateInfoRow(rows.transform, sprites.CarIcon, sprites.Highlight, HighlightYellow,
                "Biển số xe:", "51H-247.68", regularFont, semiBoldFont,
                out licensePlateLabel, out licensePlateValue);
            CreateInfoRow(rows.transform, sprites.ClockIcon, sprites.Highlight, HighlightMint,
                "Thời gian đón:", "07:30 AM", regularFont, semiBoldFont,
                out pickupTimeLabel, out pickupTimeValue);
            CreateInfoRow(rows.transform, sprites.PaletteIcon, sprites.Highlight, HighlightSky,
                "Màu xe:", "Trắng", regularFont, semiBoldFont,
                out carColorLabel, out carColorValue);

            Image flowerDoodle = CreateImage("FlowerDoodle", paperRoot.transform, sprites.Flower, DoodleRed);
            LayoutRect(flowerDoodle.rectTransform,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-52f, 78f), new Vector2(24f, 24f));

            GameObject footerBlock = new GameObject("FooterBlock", typeof(RectTransform));
            footerBlock.transform.SetParent(content.transform, false);
            LayoutElement footerBlockLayout = footerBlock.AddComponent<LayoutElement>();
            footerBlockLayout.minHeight = 52f;
            footerBlockLayout.preferredHeight = 52f;

            TextMeshProUGUI footerText = CreateLabel("Footer", footerBlock.transform, regularFont, 19, InkSoft,
                FontStyles.Italic);
            footerText.text = "Chúc chuyến đi thuận lợi! :)";
            footerText.alignment = TextAlignmentOptions.BottomLeft;
            footerText.characterSpacing = 0.4f;
            Stretch(footerText.rectTransform, new Vector2(8f, 0f), new Vector2(-14f, 0f));

            VehicleBookingNoteView view = root.GetComponent<VehicleBookingNoteView>();
            view.BindEditorReferences(
                titleText,
                licensePlateLabel,
                licensePlateValue,
                pickupTimeLabel,
                pickupTimeValue,
                carColorLabel,
                carColorValue,
                footerText);

            GameObject prefab = SavePrefab(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return prefab;
        }

        private static void CreateInfoRow(
            Transform parent,
            Sprite iconSprite,
            Sprite highlightSprite,
            Color highlightColor,
            string label,
            string value,
            TMP_FontAsset regularFont,
            TMP_FontAsset semiBoldFont,
            out TextMeshProUGUI labelText,
            out TextMeshProUGUI valueText)
        {
            GameObject row = new GameObject("Row_" + label.TrimEnd(':'), typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;
            layout.padding = new RectOffset(6, 12, 0, 0);

            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.minHeight = 54f;
            rowLayout.preferredHeight = 54f;

            GameObject iconFrame = new GameObject("IconFrame", typeof(RectTransform));
            iconFrame.transform.SetParent(row.transform, false);
            LayoutElement iconFrameLayout = iconFrame.AddComponent<LayoutElement>();
            iconFrameLayout.minWidth = 36f;
            iconFrameLayout.preferredWidth = 36f;
            iconFrameLayout.minHeight = 36f;
            iconFrameLayout.preferredHeight = 36f;
            iconFrameLayout.flexibleWidth = 0f;

            Image iconCircle = CreateImage("IconCircle", iconFrame.transform, highlightSprite, IconCircleTint);
            Stretch(iconCircle.rectTransform);
            iconCircle.type = Image.Type.Sliced;

            Image icon = CreateImage("Icon", iconFrame.transform, iconSprite, InkBlack);
            Stretch(icon.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -8f));

            labelText = CreateLabel("Label", row.transform, regularFont, 18, InkSoft, FontStyles.Normal);
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
            labelLayout.minWidth = 96f;
            labelLayout.preferredWidth = 108f;
            labelLayout.flexibleWidth = 1f;

            GameObject valuePill = new GameObject("ValuePill", typeof(RectTransform), typeof(HorizontalLayoutGroup),
                typeof(LayoutElement));
            valuePill.transform.SetParent(row.transform, false);

            HorizontalLayoutGroup pillLayout = valuePill.GetComponent<HorizontalLayoutGroup>();
            pillLayout.childAlignment = TextAnchor.MiddleCenter;
            pillLayout.childControlWidth = true;
            pillLayout.childControlHeight = true;
            pillLayout.childForceExpandWidth = false;
            pillLayout.childForceExpandHeight = false;
            pillLayout.padding = new RectOffset(10, 10, 6, 6);

            LayoutElement pillLayoutElement = valuePill.GetComponent<LayoutElement>();
            pillLayoutElement.flexibleWidth = 0f;
            pillLayoutElement.minWidth = 104f;
            pillLayoutElement.preferredWidth = 104f;
            pillLayoutElement.minHeight = 34f;
            pillLayoutElement.preferredHeight = 34f;

            Image highlight = CreateImage("Highlight", valuePill.transform, highlightSprite, highlightColor);
            Stretch(highlight.rectTransform);
            highlight.type = Image.Type.Sliced;

            valueText = CreateLabel("Value", valuePill.transform, semiBoldFont, 19, InkBlack, FontStyles.Bold);
            valueText.text = value;
            valueText.alignment = TextAlignmentOptions.Center;
            LayoutElement valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
            valueLayout.minWidth = 84f;
            valueLayout.preferredWidth = 84f;
            valueLayout.flexibleWidth = 0f;
        }

        private static void CreateSparkle(Transform parent, Sprite sparkleSprite, Vector2 anchor, float size, float rotation)
        {
            Image sparkle = CreateImage("Sparkle", parent, sparkleSprite, InkBlack);
            RectTransform rect = sparkle.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
            rect.localEulerAngles = new Vector3(0f, 0f, rotation);
            sparkle.color = new Color(InkBlack.r, InkBlack.g, InkBlack.b, 0.32f);
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, TMP_FontAsset font, int size,
            Color color, FontStyles style)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font != null ? font : TMP_Settings.defaultFontAsset;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.richText = false;
            label.raycastTarget = false;
            label.margin = Vector4.zero;
            return label;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void LayoutRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static TMP_FontAsset LoadFont(string path)
        {
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }

            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private struct NoteSprites
        {
            public Sprite Paper;
            public Sprite Tape;
            public Sprite Shadow;
            public Sprite Highlight;
            public Sprite CornerCurl;
            public Sprite CarIcon;
            public Sprite ClockIcon;
            public Sprite PaletteIcon;
            public Sprite Heart;
            public Sprite Flower;
            public Sprite Sparkle;
        }

        private static NoteSprites GenerateSprites()
        {
            NoteSprites sprites = new NoteSprites
            {
                Paper = SaveSprite("note_paper.png", CreatePaperTexture(480, 620)),
                Tape = SaveSprite("note_tape.png", CreateTapeTexture(256, 64)),
                Shadow = SaveSprite("note_shadow.png", CreateRoundedRectTexture(480, 620, 24, new Color(0f, 0f, 0f, 1f))),
                Highlight = SaveSprite("note_highlight.png", CreateRoundedRectTexture(128, 48, 18, Color.white), 18),
                CornerCurl = SaveSprite("note_corner_curl.png", CreateCornerCurlTexture(96, 72)),
                CarIcon = SaveSprite("icon_car.png", CreateCarIconTexture(64, 64)),
                ClockIcon = SaveSprite("icon_clock.png", CreateClockIconTexture(64, 64)),
                PaletteIcon = SaveSprite("icon_palette.png", CreatePaletteIconTexture(64, 64)),
                Heart = SaveSprite("doodle_heart.png", CreateHeartTexture(48, 48)),
                Flower = SaveSprite("doodle_flower.png", CreateFlowerTexture(48, 48)),
                Sparkle = SaveSprite("doodle_sparkle.png", CreateSparkleTexture(32, 32))
            };
            return sprites;
        }

        private static Sprite SaveSprite(string fileName, Texture2D texture, int border = 0)
        {
            string path = ArtFolder + "/" + fileName;
            byte[] png = texture.EncodeToPNG();
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.spritePixelsPerUnit = 100f;
                if (border > 0)
                {
                    importer.spriteBorder = new Vector4(border, border, border, border);
                }

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Texture2D CreatePaperTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color baseColor = PaperCream;
            Color lineColor = new Color(0.64f, 0.74f, 0.84f, 0.12f);
            Color marginColor = new Color(0.84f, 0.36f, 0.36f, 0.34f);
            int marginX = 68;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.03f;
                    Color pixel = baseColor;
                    pixel.r += noise;
                    pixel.g += noise * 0.8f;
                    pixel.b += noise * 0.5f;

                    if (x >= marginX - 1 && x <= marginX + 1)
                    {
                        pixel = Color.Lerp(pixel, marginColor, marginColor.a);
                    }

                    if (y % 36 == 10)
                    {
                        pixel = Color.Lerp(pixel, lineColor, lineColor.a);
                    }

                    pixel.a = GetPaperAlpha(x, y, width, height);
                    texture.SetPixel(x, y, pixel);
                }
            }

            DrawNotebookHoles(texture, height);
            texture.Apply();
            return texture;
        }

        private static float GetPaperAlpha(int x, int y, int width, int height)
        {
            float alpha = 1f;

            // Torn left edge with soft scallops.
            if (x < 54)
            {
                float wave = Mathf.Sin(y * 0.1f) * 4f;
                float edge = 48f + wave;
                if (x < edge - 5f)
                {
                    alpha = 0f;
                }
                else if (x < edge)
                {
                    alpha = Mathf.InverseLerp(edge - 5f, edge, x);
                }
            }

            // Gentle softness on other edges.
            float rightNoise = Mathf.PerlinNoise(y * 0.06f, 99f) * 3f;
            if (x > width - 6f - rightNoise)
            {
                alpha *= Mathf.InverseLerp(width, width - 6f - rightNoise, x);
            }

            float topNoise = Mathf.PerlinNoise(x * 0.06f, 12f) * 2f;
            if (y > height - 8f - topNoise)
            {
                alpha *= Mathf.InverseLerp(height, height - 8f - topNoise, y);
            }

            float bottomNoise = Mathf.PerlinNoise(x * 0.05f, 44f) * 3f;
            if (y < 6f + bottomNoise)
            {
                alpha *= Mathf.InverseLerp(0f, 6f + bottomNoise, y);
            }

            return Mathf.Clamp01(alpha);
        }

        private static void DrawNotebookHoles(Texture2D texture, int height)
        {
            int[] holeYs = { 92, 188, 284, 380, 476, 572 };
            for (int i = 0; i < holeYs.Length; i++)
            {
                int cy = holeYs[i];
                for (int y = cy - 10; y <= cy + 10; y++)
                {
                    if (y < 0 || y >= height)
                    {
                        continue;
                    }

                    for (int x = 0; x < 34; x++)
                    {
                        float dx = x - 18f;
                        float dy = y - cy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist <= 9.5f)
                        {
                            Color c = texture.GetPixel(x, y);
                            c = Color.Lerp(c, new Color(0.55f, 0.48f, 0.38f, 1f), 0.35f);
                            if (dist <= 7.5f)
                            {
                                c.a = 0f;
                            }

                            texture.SetPixel(x, y, c);
                        }
                    }
                }
            }
        }

        private static Texture2D CreateTapeTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float edgeX = Mathf.Min(x, width - 1 - x) / 8f;
                    float edgeY = Mathf.Min(y, height - 1 - y) / 4f;
                    float alpha = Mathf.Clamp01(Mathf.Min(edgeX, edgeY));
                    float fiber = Mathf.PerlinNoise(x * 0.2f, y * 0.35f) * 0.08f;
                    Color color = new Color(0.95f + fiber, 0.88f + fiber, 0.62f + fiber, alpha * 0.95f);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D CreateRoundedRectTexture(int width, int height, int radius, Color fill)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float r = radius;
            Vector2 size = new Vector2(width - 1, height - 1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = RoundedRectAlpha(new Vector2(x, y), size, r);
                    Color pixel = fill;
                    pixel.a *= alpha;
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            return texture;
        }

        private static float RoundedRectAlpha(Vector2 p, Vector2 size, float radius)
        {
            Vector2 q = new Vector2(Mathf.Abs(p.x - size.x * 0.5f), Mathf.Abs(p.y - size.y * 0.5f));
            Vector2 b = size * 0.5f - new Vector2(radius, radius);
            float dx = Mathf.Max(q.x - b.x, 0f);
            float dy = Mathf.Max(q.y - b.y, 0f);
            float dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;
            return Mathf.Clamp01(0.75f - dist);
        }

        private static Texture2D CreateCornerCurlTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = x / (float)width;
                    float ny = y / (float)height;
                    float curl = nx * 0.55f + (1f - ny) * 0.45f;
                    float alpha = Mathf.SmoothStep(0.35f, 0.95f, curl);
                    Color color = Color.Lerp(new Color(0.9f, 0.84f, 0.72f, 1f), new Color(0.72f, 0.66f, 0.55f, 1f), curl);
                    color.a = alpha;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D CreateCarIconTexture(int width, int height)
        {
            Texture2D texture = BlankIconTexture(width, height);
            DrawStrokeRect(texture, 12, 30, 52, 18, 2);
            DrawStrokeRect(texture, 18, 22, 40, 12, 2);
            DrawFilledCircle(texture, 20, 48, 5);
            DrawFilledCircle(texture, 44, 48, 5);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateClockIconTexture(int width, int height)
        {
            Texture2D texture = BlankIconTexture(width, height);
            DrawStrokeCircle(texture, 32, 32, 20, 2);
            DrawStrokeLine(texture, 32, 32, 32, 18, 2);
            DrawStrokeLine(texture, 32, 32, 42, 32, 2);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreatePaletteIconTexture(int width, int height)
        {
            Texture2D texture = BlankIconTexture(width, height);
            DrawStrokeCircle(texture, 30, 34, 18, 2);
            DrawFilledCircle(texture, 22, 42, 3);
            DrawFilledCircle(texture, 30, 46, 3);
            DrawFilledCircle(texture, 38, 42, 3);
            DrawFilledCircle(texture, 40, 34, 3);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateHeartTexture(int width, int height)
        {
            Texture2D texture = BlankIconTexture(width, height);
            Color color = Color.white;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - width * 0.5f) / (width * 0.42f);
                    float ny = (y - height * 0.42f) / (height * 0.42f);
                    float v = nx * nx + ny * ny - 1f;
                    float heart = v * v * v - nx * nx * ny * ny * ny;
                    if (heart <= 0f && heart > -0.2f)
                    {
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 0.9f));
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D CreateFlowerTexture(int width, int height)
        {
            Texture2D texture = BlankIconTexture(width, height);
            DrawStrokeCircle(texture, 24, 24, 6, 2);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * Mathf.PI * 2f / 5f;
                int cx = 24 + Mathf.RoundToInt(Mathf.Cos(angle) * 12f);
                int cy = 24 + Mathf.RoundToInt(Mathf.Sin(angle) * 12f);
                DrawStrokeCircle(texture, cx, cy, 6, 2);
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D CreateSparkleTexture(int width, int height)
        {
            Texture2D texture = BlankIconTexture(width, height);
            DrawStrokeLine(texture, 16, 4, 16, 28, 2);
            DrawStrokeLine(texture, 4, 16, 28, 16, 2);
            DrawStrokeLine(texture, 8, 8, 24, 24, 2);
            DrawStrokeLine(texture, 24, 8, 8, 24, 2);
            texture.Apply();
            return texture;
        }

        private static Texture2D BlankIconTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            return texture;
        }

        private static void DrawStrokeRect(Texture2D texture, int x, int y, int w, int h, int thickness)
        {
            DrawStrokeLine(texture, x, y, x + w, y, thickness);
            DrawStrokeLine(texture, x + w, y, x + w, y + h, thickness);
            DrawStrokeLine(texture, x + w, y + h, x, y + h, thickness);
            DrawStrokeLine(texture, x, y + h, x, y, thickness);
        }

        private static void DrawStrokeLine(Texture2D texture, int x0, int y0, int x1, int y1, int thickness)
        {
            float length = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            int steps = Mathf.Max(1, Mathf.RoundToInt(length * 2f));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
                DrawFilledCircle(texture, x, y, thickness);
            }
        }

        private static void DrawStrokeCircle(Texture2D texture, int cx, int cy, int radius, int thickness)
        {
            int segments = 48;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f;
                float a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                int x0 = cx + Mathf.RoundToInt(Mathf.Cos(a0) * radius);
                int y0 = cy + Mathf.RoundToInt(Mathf.Sin(a0) * radius);
                int x1 = cx + Mathf.RoundToInt(Mathf.Cos(a1) * radius);
                int y1 = cy + Mathf.RoundToInt(Mathf.Sin(a1) * radius);
                DrawStrokeLine(texture, x0, y0, x1, y1, thickness);
            }
        }

        private static void DrawFilledCircle(Texture2D texture, int cx, int cy, int radius)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
            }
        }
    }
}
#endif
