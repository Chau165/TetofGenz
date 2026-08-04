#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Applies <c>Guide/CURSOR_RESTYLE_PHONG_TRO_VIETNAM.md</c> to the Level 1 packing room.
    /// Environment and prop layout only — gameplay systems unchanged.
    /// </summary>
    internal static class Level1VietnamRoomRestyleBuilder
    {
        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        private const float WallHalfThickness = 0.09f;

        public static void RestyleOpenScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnamRoomRestyleBuilder] Exit Play mode before restyling.");
                return;
            }

            GameObject worldRoot = GameObject.Find("Level1World");
            if (worldRoot == null)
            {
                Debug.Log("[Level1VietnamRoomRestyleBuilder] Level1World not found — running full build...");
                Level1SceneBuilder.BuildLevel1PackingRoom();
                return;
            }

            Apply(worldRoot.transform);
            Level1SceneBuilder.PolishPackingRoomLayout();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnamRoomRestyleBuilder] Vietnam rental-room guide applied.");
        }

        public static void Apply(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            Transform interior = Level1ScenePlacement.FindInRoom(worldRoot, "RentalRoomInterior")
                ?? worldRoot.Find("RentalRoomInterior");
            if (interior == null)
            {
                Debug.LogWarning("[Level1VietnamRoomRestyleBuilder] RentalRoomInterior not found.");
                return;
            }

            Material wallMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Settings/ChoTet/Materials/Wall_Light.mat");
            float deskZ = Level1SceneBuilder.RoomLayout.Desk.z;

            Level1FloorMaterialBuilder.ApplyTileFloor(worldRoot);
            EnsureRoomCeiling(interior, wallMaterial);
            ReplaceLeftWallAndWindow(interior, wallMaterial, deskZ);
            ReplaceWallArtWithPortraits(interior, deskZ);
            ReplaceSuitcase(worldRoot);
            PlaceBedProps(worldRoot);
            PlaceWallClock(interior);
            PlaceCornerPlant(worldRoot);
            PlaceChieuTaiPlaque(worldRoot);
            ConfigureWindowLighting(worldRoot, deskZ);
            Level1StudyWindowBuilder.SyncDetachedIndoorActors(worldRoot);
        }

        private static void ReplaceLeftWallAndWindow(Transform interior, Material wallMaterial, float deskZ)
        {
            Transform oldLeft = interior.Find("Wall_Left");
            if (oldLeft != null)
            {
                Object.DestroyImmediate(oldLeft.gameObject);
            }

            Transform oldWindow = interior.Find("StudyWindow");
            if (oldWindow != null)
            {
                Object.DestroyImmediate(oldWindow.gameObject);
            }

            if (wallMaterial != null)
            {
                Level1StudyWindowBuilder.BuildLeftWallWithOpening(interior, wallMaterial, deskZ);
            }

            Level1StudyWindowBuilder.DestroyLegacyWindowDecor(interior);
            Level1StudyWindowBuilder.Build(interior, deskZ);
            Level1StudyWindowBuilder.ClearExteriorWorld(interior.root);
        }

        private static void ReplaceWallArtWithPortraits(Transform interior, float deskZ)
        {
            Transform personality = interior.parent != null
                ? interior.parent.Find("RoomPersonality")
                : null;
            if (personality == null)
            {
                personality = interior.root.Find("RoomPersonality");
            }

            if (personality != null)
            {
                Transform gallery = personality.Find("WallArtGallery");
                if (gallery != null)
                {
                    Object.DestroyImmediate(gallery.gameObject);
                }
            }

            Level1PortraitWallBuilder.Build(interior, RoomHalfWidth, RoomHalfDepth, WallHalfThickness, deskZ);
        }

        private static void ReplaceSuitcase(Transform worldRoot)
        {
            Transform exitSuitcase = Level1ScenePlacement.FindInRoom(worldRoot, "ExitArea/Suitcase");
            if (exitSuitcase != null)
            {
                Object.DestroyImmediate(exitSuitcase.gameObject);
            }

            Vector3 center = new Vector3(1.35f, Level1SceneBuilder.FloorTopY, -0.25f);
            Level1SuitcaseBuilder.Place(worldRoot, center);
        }

        private static void PlaceBedProps(Transform worldRoot)
        {
            GameObject bed = Level1ScenePlacement.FindInRoomObject(worldRoot, "SleepingArea/Bed");
            if (bed != null)
            {
                Level1BedPropsBuilder.PlaceAll(bed);
            }
        }

        private static void PlaceWallClock(Transform interior)
        {
            if (interior == null)
            {
                return;
            }

            Level1WallClockBuilder.Build(interior, RoomHalfWidth, RoomHalfDepth, WallHalfThickness);
        }

        private static void PlaceCornerPlant(Transform worldRoot)
        {
            Transform personality = Level1ScenePlacement.FindInRoom(worldRoot, "RoomPersonality")
                ?? worldRoot.Find("Level1_IndoorArea/RoomPersonality")
                ?? worldRoot.Find("RoomPersonality");
            if (personality != null)
            {
                Level1CornerPlantBuilder.Build(personality);
            }
        }

        private static void PlaceChieuTaiPlaque(Transform worldRoot)
        {
            Transform personality = Level1ScenePlacement.FindInRoom(worldRoot, "RoomPersonality")
                ?? worldRoot.Find("Level1_IndoorArea/RoomPersonality")
                ?? worldRoot.Find("RoomPersonality");
            if (personality != null)
            {
                Level1ChieuTaiPlaqueBuilder.Build(personality);
            }
        }

        private static void EnsureRoomCeiling(Transform interior, Material wallMaterial)
        {
            if (wallMaterial != null)
            {
                Level1SceneBuilder.BuildRoomCeiling(interior, wallMaterial);
            }
        }

        private static void ConfigureWindowLighting(Transform worldRoot, float deskZ)
        {
            DestroyIfExists(worldRoot, "WindowSunlight");
            DestroyIfExists(worldRoot, "RoomWarmFill");
            DestroyIfExists(worldRoot, "BedsideWarm");

            Level1StudyWindowBuilder.ConfigureMorningEnvironment();
            Level1StudyWindowBuilder.ConfigureIndoorCameraForWindow();
            Level1StudyWindowBuilder.BuildMorningLighting(worldRoot, deskZ);
            Level1StudyWindowBuilder.BuildFloorSunPatch(worldRoot, deskZ);
        }

        private static void DestroyIfExists(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
#endif
