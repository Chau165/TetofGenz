using ChoTet.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChoTet.Editor
{
    public static class ApplyMeshyExplorerPlayerVisuals
    {
        private const string ExplorerPaletteMaterialPath = "Assets/Settings/ChoTet/Materials/Explorer_Palette.mat";
        private const string ExplorerPaletteMeshPath = "Assets/Characters/Meshy_AI_Casual_Explorer_biped_Palette.asset";

        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/ChoTet_VerticalSlice.unity",
            "Assets/Scenes/Level1_PackingRoom.unity",
            "Assets/Scenes/Level1_HometownArrival.unity",
            "Assets/Scenes/Level2_StreetPickup.unity"
        };

        public static void ApplyToAllPlayerScenes()
        {
            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                PlayerCharacterVisualPaths.MeshyExplorerVisualPrefab);
            RuntimeAnimatorController animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                PlayerCharacterVisualPaths.MeshyExplorerAnimatorController);

            if (visualPrefab == null)
            {
                Debug.LogError("[MeshyExplorerPlayer] Missing visual prefab: "
                    + PlayerCharacterVisualPaths.MeshyExplorerVisualPrefab);
                return;
            }

            int totalPlayers = 0;
            for (int i = 0; i < ScenePaths.Length; i++)
            {
                string scenePath = ScenePaths[i];
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset == null)
                {
                    continue;
                }

                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single);

                PlayerTopDownController[] players = Object.FindObjectsByType<PlayerTopDownController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                for (int p = 0; p < players.Length; p++)
                {
                    CreativeCharactersPlayerSetup.ApplyVisualToPlayer(players[p], visualPrefab, animatorController);
                    ApplyExplorerPalette(players[p]);
                    totalPlayers++;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MeshyExplorerPlayer] Applied Meshy Explorer visual to " + totalPlayers + " player(s).");
        }

        private static void ApplyExplorerPalette(PlayerTopDownController player)
        {
            if (player == null || player.visualRoot == null)
            {
                return;
            }

            Mesh paletteMesh = AssetDatabase.LoadAssetAtPath<Mesh>(ExplorerPaletteMeshPath);
            Material paletteMaterial = AssetDatabase.LoadAssetAtPath<Material>(ExplorerPaletteMaterialPath);
            if (paletteMesh == null || paletteMaterial == null)
            {
                Debug.LogWarning("[MeshyExplorerPlayer] Missing explorer palette mesh/material.");
                return;
            }

            SkinnedMeshRenderer renderer = player.visualRoot.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMesh = paletteMesh;
            renderer.sharedMaterial = paletteMaterial;
            renderer.updateWhenOffscreen = true;
            renderer.quality = SkinQuality.Auto;
            EditorUtility.SetDirty(renderer);
        }
    }
}
