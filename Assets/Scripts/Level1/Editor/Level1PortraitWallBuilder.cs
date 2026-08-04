#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Large frameless posters on the empty front wall (+Z): no door, window, or bed headboard.
    /// </summary>
    internal static class Level1PortraitWallBuilder
    {
        private const string PortraitFolderPrimary = "Assets/Image/Portrait";
        private const string PortraitFolderAlt = "Assets/Images/Portrait";

        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        private const float WallHalfThickness = 0.09f;

        private const float WallInset = 0.025f;
        private const float PortraitRowCenterHeight = 0.95f;
        private const float SchedulePosterCenterHeight = 1.85f;
        private const int Columns = 4;
        private const int Rows = 1;
        private const float PosterCellW = 0.75f;
        private const float PosterCellH = 1.05f;
        private const float PosterGapX = 0.12f;
        private const float PosterGapY = 0.12f;
        private const float SchedulePosterWidth = 1.45f;
        private const float SchedulePosterMaxHeight = 1.45f;

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1PortraitWallBuilder] Exit Play mode before placing portraits.");
                return;
            }

            Transform interior = FindRentalRoomInterior();
            if (interior == null)
            {
                Debug.LogError("[Level1PortraitWallBuilder] RentalRoomInterior not found in the open scene.");
                return;
            }

            Build(interior, RoomHalfWidth, RoomHalfDepth, WallHalfThickness);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1PortraitWallBuilder] Poster wall updated on the open scene.");
        }

        private static Transform FindRentalRoomInterior()
        {
            GameObject world = GameObject.Find("Level1World");
            if (world != null)
            {
                Transform indoor = world.transform.Find("Level1_IndoorArea/RentalRoomInterior");
                if (indoor != null)
                {
                    return indoor;
                }

                indoor = world.transform.Find("RentalRoomInterior");
                if (indoor != null)
                {
                    return indoor;
                }
            }

            return FindInteriorByName();
        }

        private static Transform FindInteriorByName()
        {
            Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "RentalRoomInterior")
                {
                    return all[i];
                }
            }

            return null;
        }

        public static void Build(Transform rentalRoomInterior, float roomHalfWidth, float roomHalfDepth,
            float wallHalfThickness, float anchorLocalZ = 0.05f)
        {
            if (rentalRoomInterior == null)
            {
                return;
            }

            Transform existing = rentalRoomInterior.Find("PortraitWall");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            List<Texture2D> portraits = LoadPortraitTextures();
            List<Texture2D> schedules = LoadScheduleTextures();
            if (portraits.Count == 0 && schedules.Count == 0)
            {
                Debug.LogWarning("[Level1PortraitWallBuilder] No portraits found under "
                    + PortraitFolderPrimary + " or " + PortraitFolderAlt + ".");
                return;
            }

            // Front wall (+Z) — the only empty wall (door back, window left, bed headboard right).
            float frontWallZ = roomHalfDepth - wallHalfThickness - WallInset;
            GameObject root = new GameObject("PortraitWall");
            root.transform.SetParent(rentalRoomInterior, false);

            int placed = 0;
            if (portraits.Count > 0)
            {
                placed += PlacePortraitGrid(root.transform, portraits, frontWallZ);
            }

            for (int i = 0; i < schedules.Count; i++)
            {
                Texture2D schedule = schedules[i];
                float aspect = schedule.width / (float)Mathf.Max(1, schedule.height);
                float width = SchedulePosterWidth;
                float height = width / aspect;
                if (height > SchedulePosterMaxHeight)
                {
                    height = SchedulePosterMaxHeight;
                    width = height * aspect;
                }

                string name = "Poster_" + schedule.name;
                PlacePosterOnFrontWall(root.transform, schedule, name, 0f, SchedulePosterCenterHeight,
                    frontWallZ, width, height);
                placed++;
            }

            Debug.Log("[Level1PortraitWallBuilder] Placed " + placed + " posters on front wall (z="
                + frontWallZ.ToString("F2") + ").");
        }

        private static int PlacePortraitGrid(Transform parent, List<Texture2D> portraits, float frontWallZ)
        {
            float gridW = Columns * PosterCellW + (Columns - 1) * PosterGapX;
            float startX = -gridW * 0.5f + PosterCellW * 0.5f;
            float startY = PortraitRowCenterHeight;

            int placed = 0;
            int max = Mathf.Min(portraits.Count, Columns * Rows);
            for (int row = 0; row < Rows && placed < max; row++)
            {
                for (int col = 0; col < Columns && placed < max; col++)
                {
                    Texture2D texture = portraits[placed];
                    float aspect = texture.width / (float)Mathf.Max(1, texture.height);
                    float width = PosterCellW;
                    float height = width / aspect;
                    if (height > PosterCellH)
                    {
                        height = PosterCellH;
                        width = height * aspect;
                    }

                    float x = startX + col * (PosterCellW + PosterGapX);
                    float y = startY;
                    string name = "Portrait_" + texture.name;
                    PlacePosterOnFrontWall(parent, texture, name, x, y, frontWallZ, width, height);
                    placed++;
                }
            }

            return placed;
        }

        private static List<Texture2D> LoadPortraitTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            HashSet<string> seen = new HashSet<string>();

            CollectFromFolder(PortraitFolderPrimary, textures, seen, includeSchedules: false);
            CollectFromFolder(PortraitFolderAlt, textures, seen, includeSchedules: false);
            textures.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
            return textures;
        }

        private static List<Texture2D> LoadScheduleTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            HashSet<string> seen = new HashSet<string>();

            CollectFromFolder(PortraitFolderPrimary, textures, seen, includeSchedules: true);
            CollectFromFolder(PortraitFolderAlt, textures, seen, includeSchedules: true);
            textures.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
            return textures;
        }

        private static bool ShouldSkipPortrait(string textureName)
        {
            return textureName.IndexOf("FPT", System.StringComparison.OrdinalIgnoreCase) >= 0
                || IsSchedulePoster(textureName);
        }

        private static bool IsSchedulePoster(string textureName)
        {
            return textureName.IndexOf("WorldCup", System.StringComparison.OrdinalIgnoreCase) >= 0
                || textureName.IndexOf("Schedule", System.StringComparison.OrdinalIgnoreCase) >= 0
                || textureName.IndexOf("Lich", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void CollectFromFolder(string folder, List<Texture2D> textures, HashSet<string> seen,
            bool includeSchedules)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (seen.Contains(path))
                {
                    continue;
                }

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null)
                {
                    continue;
                }

                bool isSchedule = IsSchedulePoster(texture.name);
                if (includeSchedules != isSchedule)
                {
                    continue;
                }

                if (!includeSchedules && ShouldSkipPortrait(texture.name))
                {
                    continue;
                }

                seen.Add(path);
                textures.Add(texture);
            }
        }

        private static void PlacePosterOnFrontWall(Transform parent, Texture2D texture, string name, float centerX,
            float centerY, float wallZ, float width, float height)
        {
            Material mat = EnsureUnlitPortraitMaterial(texture);
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = new Vector3(centerX, centerY, wallZ - 0.01f);
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            quad.transform.localScale = new Vector3(width, height, 1f);

            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = quad.GetComponent<Renderer>();
            Material instance = new Material(mat);
            instance.SetTextureScale("_BaseMap", new Vector2(-1f, 1f));
            instance.SetTextureOffset("_BaseMap", new Vector2(1f, 0f));
            instance.SetTextureScale("_MainTex", new Vector2(-1f, 1f));
            instance.SetTextureOffset("_MainTex", new Vector2(1f, 0f));
            renderer.sharedMaterial = instance;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material EnsureUnlitPortraitMaterial(Texture2D texture)
        {
            string safeName = texture.name.Replace(" ", "_");
            string path = "Assets/Settings/ChoTet/Materials/Level1/Portrait_" + safeName + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Texture");
                }

                mat = new Material(shader);
                mat.name = "Portrait_" + safeName;
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetTexture("_BaseMap", texture);
            mat.SetTexture("_MainTex", texture);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
#endif
