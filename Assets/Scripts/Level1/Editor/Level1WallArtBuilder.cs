#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Hangs framed pictures on rental-room walls using 2D art from project asset packs.
    /// </summary>
    internal static class Level1WallArtBuilder
    {
        private const string ArtRoot = "Assets/Art";
        private const string NoteArt = ArtRoot + "/UI/VehicleBookingNote";
        private const string IconArt = ArtRoot + "/Level1/InventoryIcons";
        private const string PhotoFrameModel = Level1RoomAssetPaths.LowPolyOfficeProps + "/mesh_photo.fbx";

        private static readonly string[] GalleryTextures =
        {
            NoteArt + "/doodle_heart.png",
            NoteArt + "/doodle_flower.png",
            NoteArt + "/doodle_sparkle.png",
            NoteArt + "/note_paper.png",
            NoteArt + "/icon_car.png",
            IconArt + "/tet_gift.png",
            IconArt + "/phone.png",
            IconArt + "/id_card.png",
            IconArt + "/wallet.png",
            IconArt + "/water_bottle.png"
        };

        public static void BuildGallery(Transform parent, float roomHalfWidth, float roomHalfDepth,
            float wallHalfThickness, Material frameWood, Material matBacking)
        {
            if (parent == null)
            {
                return;
            }

            float leftWallX = -roomHalfWidth + wallHalfThickness + 0.04f;
            float rightWallX = roomHalfWidth - wallHalfThickness - 0.04f;
            float frontWallZ = roomHalfDepth - wallHalfThickness - 0.04f;
            float backWallZ = -roomHalfDepth + wallHalfThickness + 0.04f;

            GameObject galleryRoot = new GameObject("WallArtGallery");
            galleryRoot.transform.SetParent(parent, false);

            // Front wall — memories around the Tet banner.
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[0], "WallArt_FamilyHeart",
                new Vector3(-1.35f, 2.02f, frontWallZ), new Vector2(0.34f, 0.28f), 180f, frameWood, matBacking);
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[5], "WallArt_TetGift",
                new Vector3(-3.12f, 1.72f, frontWallZ), new Vector2(0.3f, 0.24f), 180f, frameWood, matBacking);
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[6], "WallArt_PhoneMemory",
                new Vector3(2.35f, 1.78f, frontWallZ), new Vector2(0.26f, 0.22f), 180f, frameWood, matBacking);
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[4], "WallArt_BusTicketMemory",
                new Vector3(3.55f, 1.95f, frontWallZ), new Vector2(0.3f, 0.24f), 180f, frameWood, matBacking);

            // Left wall — travel notes beside the calendar.
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[3], "WallArt_TravelNote",
                new Vector3(leftWallX, 1.42f, -1.75f), new Vector2(0.24f, 0.3f), -90f, frameWood, matBacking);
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[8], "WallArt_WalletMemento",
                new Vector3(leftWallX, 1.62f, 0.55f), new Vector2(0.22f, 0.18f), -90f, frameWood, matBacking);

            // Right wall — above the bed nook.
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[1], "WallArt_FlowerSketch",
                new Vector3(rightWallX, 2.08f, 2.35f), new Vector2(0.28f, 0.28f), 90f, frameWood, matBacking);
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[7], "WallArt_IdPhoto",
                new Vector3(rightWallX, 1.58f, 0.95f), new Vector2(0.2f, 0.16f), 90f, frameWood, matBacking);

            // Back wall — small accents near storage.
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[2], "WallArt_SparklePin",
                new Vector3(3.05f, 2.05f, backWallZ), new Vector2(0.16f, 0.16f), 0f, frameWood, matBacking);
            PlaceTexturedFrame(galleryRoot.transform, GalleryTextures[9], "WallArt_WaterBottleStill",
                new Vector3(-2.45f, 1.88f, backWallZ), new Vector2(0.18f, 0.22f), 0f, frameWood, matBacking);

            PlaceOfficePhotoFrames(galleryRoot.transform, frameWood, matBacking, leftWallX, rightWallX, frontWallZ);
        }

        private static void PlaceOfficePhotoFrames(Transform parent, Material frameWood, Material matBacking,
            float leftWallX, float rightWallX, float frontWallZ)
        {
            PlaceOfficePhotoFrame(parent, "WallArt_OfficeFrame_Left", PhotoFrameModel,
                new Vector3(leftWallX, 2.28f, 2.05f), Quaternion.Euler(0f, -90f, 0f), 0.85f,
                GalleryTextures[0], frameWood, matBacking);
            PlaceOfficePhotoFrame(parent, "WallArt_OfficeFrame_Right", PhotoFrameModel,
                new Vector3(rightWallX, 1.92f, -0.35f), Quaternion.Euler(0f, 90f, 0f), 0.78f,
                GalleryTextures[1], frameWood, matBacking);
            PlaceOfficePhotoFrame(parent, "WallArt_OfficeFrame_Front", PhotoFrameModel,
                new Vector3(0.95f, 2.12f, frontWallZ), Quaternion.Euler(0f, 180f, 0f), 0.72f,
                GalleryTextures[5], frameWood, matBacking);
        }

        private static void PlaceTexturedFrame(Transform parent, string texturePath, string name, Vector3 center,
            Vector2 pictureSize, float yawDegrees, Material frameWood, Material matBacking)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = center;
            root.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);

            float depth = 0.018f;
            float framePad = 0.035f;
            Vector3 pictureScale = new Vector3(pictureSize.x, pictureSize.y, depth);
            Vector3 frameScale = pictureScale + new Vector3(framePad * 2f, framePad * 2f, depth * 0.65f);

            CreateFramePart("Frame_Backing", root.transform, Vector3.zero, frameScale, matBacking);
            CreateFramePart("Frame_Outer", root.transform, new Vector3(0f, 0f, depth * 0.35f), frameScale, frameWood);

            Material pictureMat = CreatePictureMaterial(texturePath);
            float pictureZ = depth * 0.85f;
            CreateFramePart("Picture", root.transform, new Vector3(0f, 0f, pictureZ), pictureScale, pictureMat);
        }

        private static void PlaceOfficePhotoFrame(Transform parent, string name, string modelPath, Vector3 position,
            Quaternion rotation, float uniformScale, string texturePath, Material frameWood, Material matBacking)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (prefab == null)
            {
                PlaceTexturedFrame(parent, texturePath, name, position, new Vector2(0.26f, 0.2f),
                    rotation.eulerAngles.y, frameWood, matBacking);
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = Vector3.one * uniformScale;

            StripColliders(instance);
            ApplyFrameMaterials(instance, frameWood, matBacking, texturePath);
        }

        private static void ApplyFrameMaterials(GameObject root, Material frameWood, Material matBacking,
            string texturePath)
        {
            Material pictureMat = CreatePictureMaterial(texturePath);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                string lower = renderer.gameObject.name.ToLowerInvariant();
                if (lower.Contains("photo") || lower.Contains("mark") || lower.Contains("picture"))
                {
                    renderer.sharedMaterial = pictureMat;
                }
                else if (lower.Contains("frame") || lower.Contains("wood"))
                {
                    renderer.sharedMaterial = frameWood;
                }
                else
                {
                    renderer.sharedMaterial = matBacking;
                }
            }
        }

        private static void CreateFramePart(string partName, Transform parent, Vector3 localPosition, Vector3 scale,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            if (material != null)
            {
                part.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static Material CreatePictureMaterial(string texturePath)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Texture");
            }

            Material material = new Material(shader);
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }
                else
                {
                    material.mainTexture = texture;
                }
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            return material;
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }
    }
}
#endif
