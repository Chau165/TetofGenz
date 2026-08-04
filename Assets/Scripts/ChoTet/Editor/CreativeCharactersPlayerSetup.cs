#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChoTet.Runtime;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.Character;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.FaceEditor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.MaterialManagement;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Editor
{
    /// <summary>
    /// Builds the player visual from ITHappy Creative Characters FREE (template + colors + animations).
    /// Import the free pack first, then run Tools/ChoTet/Setup Creative Character Player.
    /// </summary>
    public static class CreativeCharactersPlayerSetup
    {
        private const string PackMarkerClip = "Idle_Breathing";
        private const string WalkClipName = "Walk_Forward";
        private const string RunClipName = "Run_Forward";
        private const string DefaultPackRoot = "Assets/ithappy/Creative_Characters_FREE";
        private const string DefaultAnimationsFolder =
            DefaultPackRoot + "/Animations/Other_Animations";
        private const string PackSavedCharactersFolder = DefaultPackRoot + "/Saved_Characters";

        [MenuItem("Tools/ChoTet/Setup Creative Character Player")]
        public static void SetupCreativeCharacterPlayer()
        {
            if (!TryFindPackRoot(out string packRoot))
            {
                EditorUtility.DisplayDialog(
                    "Creative Characters FREE",
                    "Pack chưa import.\n\n"
                    + "1. Unity Asset Store → Creative Characters FREE (ITHappy)\n"
                    + "2. Import bản URP cho Unity 6\n"
                    + "3. Chạy lại menu này",
                    "OK");
                return;
            }

            EnsureFolder(PlayerCharacterVisualPaths.CreativeCharacterRoot);
            EnsureFolder(PlayerCharacterVisualPaths.CreativeCharacterMaterialsRoot);

            if (!TryLoadAnimationClips(packRoot, out AnimationClip idle, out AnimationClip walk, out AnimationClip run))
            {
                EditorUtility.DisplayDialog(
                    "Creative Characters FREE",
                    "Không tìm thấy clip Idle_Breathing / Walk_Forward / Run_Forward trong pack.",
                    "OK");
                return;
            }

            RuntimeAnimatorController topDownController = BuildTopDownAnimatorController(idle, walk, run);
            if (topDownController == null)
            {
                return;
            }

            GameObject instance = BuildDressedCharacterFromPack();
            if (instance == null)
            {
                EditorUtility.DisplayDialog(
                    "Creative Characters FREE",
                    "Không tạo được nhân vật có đồ/màu từ pack.\n\n"
                    + "Thử mở Tools/ithappy/Creative_Characters_FREE/Character Customization, "
                    + "chọn outfit + phụ kiện, Save Prefab, rồi chạy lại menu này.",
                    "OK");
                return;
            }

            instance.name = "CreativeCharacter_PlayerVisual";
            CreativeCharacterAssembly.ApplyPlayerAppearanceMaterials(instance);
            PrepareCharacterInstance(instance, topDownController);

            string prefabPath = PlayerCharacterVisualPaths.CreativeCharacterVisualPrefab;
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CreativeCharacters] Player visual saved: " + prefabPath);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.FatherVisualPrefab) == null
                || AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.MotherVisualPrefab) == null)
            {
                CreativeCharactersParentSetup.SetupParentCharacters(false);
            }

            EditorUtility.DisplayDialog(
                "Creative Characters FREE",
                "Đã tạo player prefab (áo xanh ngọc + quần tối, tóc ngắn):\n" + prefabPath
                + "\n\nĐã tạo luôn prefab bố/mẹ (nếu pack sẵn sàng)."
                + "\n\nMuốn đổi outfit: Tools/ithappy/.../Character Customization → Save Prefab "
                + "vào Saved_Characters của pack, rồi chạy lại menu này."
                + "\n\nChạy Tools/ChoTet/Apply Creative Character To Open Scene để cập nhật scene.",
                "OK");
        }

        [MenuItem("Tools/ChoTet/Apply Creative Character To Open Scene")]
        public static void ApplyCreativeCharacterToOpenScene()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                PlayerCharacterVisualPaths.CreativeCharacterVisualPrefab);
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                PlayerCharacterVisualPaths.CreativeCharacterTopDownController);

            if (prefab == null || controller == null)
            {
                EditorUtility.DisplayDialog(
                    "Creative Characters FREE",
                    "Chưa có prefab player. Chạy Setup Creative Character Player trước.",
                    "OK");
                return;
            }

            PlayerTopDownController[] players = Object.FindObjectsByType<PlayerTopDownController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (players.Length == 0)
            {
                EditorUtility.DisplayDialog("Creative Characters FREE", "Không có Player trong scene.", "OK");
                return;
            }

            for (int i = 0; i < players.Length; i++)
            {
                ApplyVisualToPlayer(players[i], prefab, controller);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[CreativeCharacters] Updated " + players.Length + " player(s) in open scene.");
        }

        public static void ApplyVisualToPlayer(
            PlayerTopDownController player,
            GameObject visualPrefab,
            RuntimeAnimatorController animatorController)
        {
            if (player == null || visualPrefab == null)
            {
                return;
            }

            Transform visualRoot = player.visualRoot;
            if (visualRoot == null)
            {
                GameObject root = new GameObject("VisualRoot");
                root.transform.SetParent(player.transform, false);
                visualRoot = root.transform;
                player.visualRoot = visualRoot;
            }

            for (int c = visualRoot.childCount - 1; c >= 0; c--)
            {
                Object.DestroyImmediate(visualRoot.GetChild(c).gameObject);
            }

            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, visualRoot);
            string visualPrefabPath = AssetDatabase.GetAssetPath(visualPrefab);
            visual.name = visualPrefabPath == PlayerCharacterVisualPaths.MeshyExplorerVisualPrefab
                ? PlayerCharacterVisualPaths.MeshyExplorerVisualChildName
                : visualPrefabPath == PlayerCharacterVisualPaths.LegacyVisualPrefab
                    ? PlayerCharacterVisualPaths.LegacyVisualChildName
                    : PlayerCharacterVisualPaths.VisualChildName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, PlayerCharacterVisualPaths.CreativeCharacterModelYaw, 0f);
            visual.transform.localScale = Vector3.one;

            player.fallbackAnimatorController = animatorController;
            player.ForceRealignVisual();
            EditorUtility.SetDirty(player);
        }

        private static bool TryFindPackRoot(out string packRoot)
        {
            packRoot = null;
            if (AssetDatabase.IsValidFolder(DefaultPackRoot))
            {
                packRoot = DefaultPackRoot;
                return true;
            }

            string[] guids = AssetDatabase.FindAssets(PackMarkerClip + " t:AnimationClip");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!IsCreativeCharactersPackPath(path))
                {
                    continue;
                }

                packRoot = ResolvePackRootFromAssetPath(path);
                if (!string.IsNullOrEmpty(packRoot))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCreativeCharactersPackPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string normalized = assetPath.Replace('\\', '/');
            return normalized.Contains("Creative_Characters_FREE")
                || normalized.Contains("Creative Characters FREE")
                || normalized.Contains("/ithappy/");
        }

        private static string ResolvePackRootFromAssetPath(string assetPath)
        {
            string normalized = assetPath.Replace('\\', '/');
            const string marker = "/Creative_Characters_FREE/";
            int markerIndex = normalized.IndexOf(marker, System.StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                return normalized.Substring(0, markerIndex + marker.Length - 1);
            }

            markerIndex = normalized.IndexOf("/Creative Characters FREE/", System.StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                return normalized.Substring(0, markerIndex + "/Creative Characters FREE".Length);
            }

            return null;
        }

        private static bool TryLoadAnimationClips(
            string packRoot,
            out AnimationClip idle,
            out AnimationClip walk,
            out AnimationClip run)
        {
            idle = LoadClip(packRoot, PackMarkerClip);
            walk = LoadClip(packRoot, WalkClipName);
            run = LoadClip(packRoot, RunClipName);
            return idle != null && walk != null && run != null;
        }

        private static AnimationClip LoadClip(string packRoot, string clipName)
        {
            string directPath = DefaultAnimationsFolder + "/" + clipName + ".anim";
            AnimationClip direct = AssetDatabase.LoadAssetAtPath<AnimationClip>(directPath);
            if (ClipMatchesName(direct, clipName, directPath))
            {
                return direct;
            }

            string[] searchFolders = BuildClipSearchFolders(packRoot);
            for (int f = 0; f < searchFolders.Length; f++)
            {
                string folder = searchFolders[f];
                if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
                {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets(clipName + " t:AnimationClip", new[] { folder });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (ClipMatchesName(clip, clipName, path))
                    {
                        return clip;
                    }
                }

                guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folder });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (ClipMatchesName(clip, clipName, path))
                    {
                        return clip;
                    }
                }
            }

            return null;
        }

        private static string[] BuildClipSearchFolders(string packRoot)
        {
            return new[]
            {
                DefaultAnimationsFolder,
                packRoot + "/Animations",
                packRoot
            };
        }

        private static bool ClipMatchesName(AnimationClip clip, string clipName, string assetPath)
        {
            if (clip == null)
            {
                return false;
            }

            if (clip.name == clipName)
            {
                return true;
            }

            return Path.GetFileNameWithoutExtension(assetPath) == clipName;
        }

        private static RuntimeAnimatorController BuildTopDownAnimatorController(
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run)
        {
            EnsureLooping(walk);
            EnsureLooping(run);

            string path = PlayerCharacterVisualPaths.CreativeCharacterTopDownController;
            AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveAmount", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine root = controller.layers[0].stateMachine;
            AnimatorState idleState = root.AddState("Idle", new Vector3(240f, 120f, 0f));
            idleState.motion = idle;
            AnimatorState walkState = root.AddState("Walk", new Vector3(480f, 80f, 0f));
            walkState.motion = walk;
            AnimatorState runState = root.AddState("Run", new Vector3(480f, 200f, 0f));
            runState.motion = run;
            root.defaultState = idleState;

            AddTransition(idleState, walkState, AnimatorConditionMode.If, 0f, "IsMoving");
            AddTransition(walkState, idleState, AnimatorConditionMode.IfNot, 0f, "IsMoving");
            AddTransition(walkState, runState, AnimatorConditionMode.If, 0f, "IsRunning");
            AddTransition(runState, walkState, AnimatorConditionMode.IfNot, 0f, "IsRunning");
            AddTransition(runState, idleState, AnimatorConditionMode.IfNot, 0f, "IsMoving");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void EnsureLooping(AnimationClip clip)
        {
            if (clip == null)
            {
                return;
            }

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }

        private static void AddTransition(
            AnimatorState from,
            AnimatorState to,
            AnimatorConditionMode mode,
            float threshold,
            string parameter)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.AddCondition(mode, threshold, parameter);
        }

        private static GameObject BuildDressedCharacterFromPack()
        {
            GameObject savedCharacter = TryLoadUserSavedPackCharacter();
            if (savedCharacter != null)
            {
                GameObject fromSaved = Object.Instantiate(savedCharacter);
                fromSaved.name = "CreativeCharacter_PlayerVisual";
                Debug.Log("[CreativeCharacters] Using saved pack character: " + savedCharacter.name);
                return fromSaved;
            }

            return CreativeCharacterAssembly.BuildDressedCharacter(
                CreativeCharacterAssembly.ApplyPlayerLook,
                "CreativeCharacter_PlayerVisual");
        }

        internal static bool TryFindPackRootForParents(out string packRoot) => TryFindPackRoot(out packRoot);

        private static GameObject TryLoadUserSavedPackCharacter()
        {
            if (!AssetDatabase.IsValidFolder(PackSavedCharactersFolder))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PackSavedCharactersFolder });
            GameObject best = null;
            int bestSkinned = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                int skinned = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
                if (skinned > bestSkinned)
                {
                    bestSkinned = skinned;
                    best = prefab;
                }
            }

            return best;
        }

        private static void PrepareCharacterInstance(GameObject root, RuntimeAnimatorController controller)
        {
            CreativeCharacterAssembly.PrepareNpcInstance(root, controller);
        }

        private static void StripGameplayComponents(GameObject root)
        {
            // Kept for any legacy callers — logic lives in CreativeCharacterAssembly.
            CreativeCharacterAssembly.PrepareNpcInstance(root, null);
        }

        public static string ResolvePlayerVisualPrefabPath()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.MeshyExplorerVisualPrefab) != null)
            {
                return PlayerCharacterVisualPaths.MeshyExplorerVisualPrefab;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.LegacyVisualPrefab) != null
                ? PlayerCharacterVisualPaths.LegacyVisualPrefab
                : PlayerCharacterVisualPaths.CreativeCharacterVisualPrefab;
        }

        public static string ResolvePlayerAnimatorControllerPath()
        {
            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    PlayerCharacterVisualPaths.MeshyExplorerAnimatorController) != null)
            {
                return PlayerCharacterVisualPaths.MeshyExplorerAnimatorController;
            }

            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                PlayerCharacterVisualPaths.LegacyAnimatorController) != null
                ? PlayerCharacterVisualPaths.LegacyAnimatorController
                : PlayerCharacterVisualPaths.CreativeCharacterTopDownController;
        }

        public static string ResolveVisualChildName()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.MeshyExplorerVisualPrefab) != null)
            {
                return PlayerCharacterVisualPaths.MeshyExplorerVisualChildName;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterVisualPaths.LegacyVisualPrefab) != null
                ? PlayerCharacterVisualPaths.LegacyVisualChildName
                : PlayerCharacterVisualPaths.VisualChildName;
        }

        internal static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }

    /// <summary>Auto-swap player visual in Editor Play Mode to the project-selected player model.</summary>
    internal static class PlayerCreativeCharacterBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TryApplyCreativeCharacterVisual()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CreativeCharactersPlayerSetup.ResolvePlayerVisualPrefabPath());
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                CreativeCharactersPlayerSetup.ResolvePlayerAnimatorControllerPath());
            if (prefab == null || controller == null)
            {
                return;
            }

            PlayerTopDownController[] players = Object.FindObjectsByType<PlayerTopDownController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                PlayerTopDownController player = players[i];
                if (player == null || player.visualRoot == null)
                {
                    continue;
                }

                if (player.visualRoot.Find(CreativeCharactersPlayerSetup.ResolveVisualChildName()) != null)
                {
                    continue;
                }

                CreativeCharactersPlayerSetup.ApplyVisualToPlayer(player, prefab, controller);
            }
        }
    }
}
#endif
