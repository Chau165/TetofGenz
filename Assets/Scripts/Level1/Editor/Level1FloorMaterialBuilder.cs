#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Vietnamese student-room ceramic floor tiles — material only, layout unchanged.
    /// </summary>
    internal static class Level1FloorMaterialBuilder
    {
        private const float TileSizeMeters = 0.40f;
        private const int TilesPerSheet = 8;
        private const float SheetMeters = TileSizeMeters * TilesPerSheet;
        private const float FloorWidthMeters = 10.88f;
        private const float FloorDepthMeters = 8.88f;

        private static readonly Vector2 FloorTiling = new(
            FloorWidthMeters / SheetMeters,
            FloorDepthMeters / SheetMeters);

        public static Material EnsureTileFloorMaterial()
        {
            ConfigureFloorTextures();
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(Level1RoomAssetPaths.FloorTileMaterial);
            if (existing != null)
            {
                ApplyTextureSettings(existing);
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader);
            ApplyTextureSettings(material);
            AssetDatabase.CreateAsset(material, Level1RoomAssetPaths.FloorTileMaterial);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static void ConfigureFloorTextures()
        {
            ConfigureTextureImporter(Level1RoomAssetPaths.FloorTileAlbedo, isNormal: false);
            ConfigureTextureImporter(Level1RoomAssetPaths.FloorTileNormal, isNormal: true);
        }

        private static void ConfigureTextureImporter(string path, bool isNormal)
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
            importer.maxTextureSize = 512;
            importer.SaveAndReimport();
        }

        private static void ApplyTextureSettings(Material material)
        {
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(Level1RoomAssetPaths.FloorTileAlbedo);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(Level1RoomAssetPaths.FloorTileNormal);

            if (albedo != null)
            {
                material.SetTexture("_BaseMap", albedo);
                material.SetTextureScale("_BaseMap", FloorTiling);
            }

            if (normal != null)
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normal);
                material.SetTextureScale("_BumpMap", FloorTiling);
            }

            material.SetColor("_BaseColor", new Color(0.93f, 0.90f, 0.86f));
            material.SetFloat("_Smoothness", 0.28f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_BumpScale", 0.22f);
        }

        public static void ApplyTileFloor(Transform worldRoot)
        {
            Material tileMaterial = EnsureTileFloorMaterial();
            if (tileMaterial == null || worldRoot == null)
            {
                return;
            }

            Transform woodFloor = Level1ScenePlacement.FindInRoom(worldRoot, "RentalRoomInterior/WoodFloor")
                ?? worldRoot.Find("RentalRoomInterior/WoodFloor");
            if (woodFloor != null)
            {
                RemovePlankSeams(woodFloor);
                ApplyMaterialToRenderers(woodFloor, tileMaterial);
            }

            Transform interior = Level1ScenePlacement.FindInRoom(worldRoot, "RentalRoomInterior")
                ?? worldRoot.Find("RentalRoomInterior");
            if (interior != null)
            {
                Transform collider = interior.Find("FloorCollider");
                if (collider != null)
                {
                    Renderer renderer = collider.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = tileMaterial;
                    }
                }
            }
        }

        public static void FixFloorMaterial()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1FloorMaterialBuilder] Exit Play mode before fixing the floor.");
                return;
            }

            Material tileMaterial = EnsureTileFloorMaterial();
            if (tileMaterial == null)
            {
                Debug.LogError("[Level1FloorMaterialBuilder] Could not create tile floor material.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.LogWarning("[Level1FloorMaterialBuilder] Level1World not found.");
                return;
            }

            Transform woodFloor = Level1ScenePlacement.FindInRoom(worldRoot.transform, "RentalRoomInterior/WoodFloor");
            if (woodFloor == null)
            {
                woodFloor = worldRoot.transform.Find("RentalRoomInterior/WoodFloor");
            }

            if (woodFloor != null)
            {
                RemovePlankSeams(woodFloor);
                ApplyMaterialToRenderers(woodFloor, tileMaterial);
            }

            Transform interior = Level1ScenePlacement.FindInRoom(worldRoot.transform, "RentalRoomInterior");
            if (interior != null)
            {
                Transform collider = interior.Find("FloorCollider");
                if (collider != null)
                {
                    Renderer renderer = collider.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = tileMaterial;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1FloorMaterialBuilder] Floor updated to VN ceramic tiles (tiling "
                + FloorTiling.x.ToString("F2") + " x " + FloorTiling.y.ToString("F2") + ").");
        }

        private static void RemovePlankSeams(Transform woodFloor)
        {
            for (int i = woodFloor.childCount - 1; i >= 0; i--)
            {
                Transform child = woodFloor.GetChild(i);
                if (child.name.StartsWith("PlankSeam_"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void ApplyMaterialToRenderers(Transform root, Material material)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
        }
    }
}
#endif
