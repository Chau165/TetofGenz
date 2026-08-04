#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Wall-mounted "Chiêu Tài Tiến Bảo" wooden plaque near the room entrance.
    /// Replaces the old floor shrine with a simple Vietnamese shop-style decoration.
    /// </summary>
    internal static class Level1ChieuTaiPlaqueBuilder
    {
        private const string PlaqueRootName = "ChieuTaiPlaque";
        private const string LegacyShrineRootName = "ThanTaiShrine";
        private const string MaterialFolder = "Assets/Settings/ChoTet/Materials/Level1";
        private const string CalligraphyTexturePath = "Assets/Art/Level1/RentalRoom/Textures/ChieuTai_ZhaoCaiJinBao.png";

        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        private const float WallHalfThickness = 0.09f;
        private const float WallSurfaceInset = 0.012f;

        private const float PlaqueWidth = 0.45f;
        private const float PlaqueHeight = 0.65f;
        private const float PlaqueDepth = 0.025f;
        private const float FramePad = 0.028f;
        private const float FrameDepth = 0.018f;
        private const float MountCenterY = 1.50f;
        private const float MountCenterX = 1.42f;
        private const float OverlapClearance = 0.10f;

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1ChieuTaiPlaqueBuilder] Exit Play mode before placing the plaque.");
                return;
            }

            Transform personality = FindRoomPersonality();
            if (personality == null)
            {
                Debug.LogError("[Level1ChieuTaiPlaqueBuilder] RoomPersonality not found in the open scene.");
                return;
            }

            Build(personality);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1ChieuTaiPlaqueBuilder] Chiêu Tài Tiến Bảo plaque placed near entrance.");
        }

        public static void Build(Transform roomPersonality)
        {
            if (roomPersonality == null)
            {
                return;
            }

            RemoveLegacyShrine(roomPersonality);
            RemoveExistingPlaque(roomPersonality);

            float backWallInnerZ = -RoomHalfDepth + WallHalfThickness;
            Material wood = EnsureMat("ChieuTai_Wood_Red", new Color(0.34f, 0.12f, 0.09f), 0.38f);
            Material frame = EnsureMat("ChieuTai_Frame_Dark", new Color(0.22f, 0.08f, 0.06f), 0.32f);
            Material gold = EnsureCalligraphyGoldMat("ChieuTai_Gold_Calligraphy");
            Material goldShadow = EnsureTintedGoldMat("ChieuTai_Gold_Shadow", new Color(0.45f, 0.34f, 0.10f), 0.72f, 0.35f);

            GameObject root = new GameObject(PlaqueRootName);
            root.transform.SetParent(roomPersonality, false);
            root.transform.rotation = Quaternion.identity;

            BuildPlaqueGeometry(root.transform, wood, frame, gold, goldShadow);

            if (!TryMountOnBackWall(root, backWallInnerZ))
            {
                Debug.LogWarning("[Level1ChieuTaiPlaqueBuilder] Could not find a clear entrance-wall spot.");
            }

            AddWarmSpotlight(root.transform, backWallInnerZ);
            StripColliders(root);
            MarkStatic(root);
        }

        private static void BuildPlaqueGeometry(Transform root, Material wood, Material frame, Material gold,
            Material goldShadow)
        {
            float backZ = PlaqueDepth * 0.5f;
            CreateCube("Plaque_Backing", root,
                new Vector3(0f, 0f, backZ),
                new Vector3(PlaqueWidth, PlaqueHeight, PlaqueDepth),
                wood);

            float frameW = PlaqueWidth + FramePad * 2f;
            float frameH = PlaqueHeight + FramePad * 2f;
            float frameZ = backZ + PlaqueDepth * 0.35f;

            GameObject frameRoot = new GameObject("Plaque_Frame");
            frameRoot.transform.SetParent(root, false);
            frameRoot.transform.localPosition = new Vector3(0f, 0f, frameZ);

            float trimT = 0.010f;
            CreateCube("Frame_Top", frameRoot.transform,
                new Vector3(0f, frameH * 0.5f - trimT * 0.5f, 0f),
                new Vector3(frameW, trimT, FrameDepth),
                frame);
            CreateCube("Frame_Bottom", frameRoot.transform,
                new Vector3(0f, -frameH * 0.5f + trimT * 0.5f, 0f),
                new Vector3(frameW, trimT, FrameDepth),
                frame);
            CreateCube("Frame_Left", frameRoot.transform,
                new Vector3(-frameW * 0.5f + trimT * 0.5f, 0f, 0f),
                new Vector3(trimT, frameH - trimT * 2f, FrameDepth),
                frame);
            CreateCube("Frame_Right", frameRoot.transform,
                new Vector3(frameW * 0.5f - trimT * 0.5f, 0f, 0f),
                new Vector3(trimT, frameH - trimT * 2f, FrameDepth),
                frame);

            float textW = PlaqueWidth * 0.82f;
            float textH = PlaqueHeight * 0.72f;
            float textZ = frameZ + FrameDepth * 0.55f;
            CreateTextQuad("Calligraphy_Shadow", root, new Vector3(0.004f, -0.004f, textZ - 0.002f),
                new Vector3(textW, textH), goldShadow);
            CreateTextQuad("Calligraphy", root, Vector3.zero, new Vector3(textW, textH), gold, textZ);
        }

        private static void CreateTextQuad(string name, Transform parent, Vector3 localOffset, Vector3 size,
            Material mat, float localZ = 0f)
        {
            Vector3 pos = new Vector3(localOffset.x, localOffset.y, localZ + localOffset.z);
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = pos;
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            Renderer renderer = quad.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static bool TryMountOnBackWall(GameObject plaque, float backWallInnerZ)
        {
            List<GameObject> blockers = CollectBlockers(plaque.transform.root);
            float[] candidateX = { MountCenterX, -MountCenterX, 1.05f, -1.05f };

            for (int i = 0; i < candidateX.Length; i++)
            {
                float x = candidateX[i];
                plaque.transform.position = new Vector3(x, MountCenterY, backWallInnerZ + WallSurfaceInset);
                plaque.transform.rotation = Quaternion.identity;
                SnapBackFaceToWall(plaque, backWallInnerZ);

                if (!HasOverlap(plaque, blockers))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SnapBackFaceToWall(GameObject plaque, float backWallInnerZ)
        {
            if (!Level1ScenePlacement.TryGetWorldBounds(plaque, out Bounds bounds))
            {
                return;
            }

            float deltaZ = (backWallInnerZ + WallSurfaceInset) - bounds.min.z;
            plaque.transform.position += new Vector3(0f, 0f, deltaZ);
        }

        private static void AddWarmSpotlight(Transform plaqueRoot, float backWallInnerZ)
        {
            Transform existing = plaqueRoot.Find("PlaqueSpotlight");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject lightGo = new GameObject("PlaqueSpotlight");
            lightGo.transform.SetParent(plaqueRoot, false);
            Vector3 plaquePos = plaqueRoot.position;
            lightGo.transform.position = new Vector3(plaquePos.x, plaquePos.y + 0.42f, plaquePos.z + 0.18f);
            lightGo.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1.0f, 0.86f, 0.70f);
            light.intensity = 0.95f;
            light.range = 2.1f;
            light.spotAngle = 34f;
            light.innerSpotAngle = 18f;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
        }

        private static void RemoveLegacyShrine(Transform roomPersonality)
        {
            Transform shrine = roomPersonality.Find(LegacyShrineRootName);
            if (shrine != null)
            {
                Object.DestroyImmediate(shrine.gameObject);
            }

            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == LegacyShrineRootName || all[i].name.StartsWith("Shrine_")
                    || all[i].name.StartsWith("Incense") || all[i].name == "ThanTai" || all[i].name == "OngDia")
                {
                    if (all[i].transform.IsChildOf(roomPersonality))
                    {
                        Object.DestroyImmediate(all[i]);
                    }
                }
            }
        }

        private static void RemoveExistingPlaque(Transform roomPersonality)
        {
            Transform existing = roomPersonality.Find(PlaqueRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static Transform FindRoomPersonality()
        {
            GameObject world = GameObject.Find("Level1World");
            if (world != null)
            {
                Transform personality = world.transform.Find("Level1_IndoorArea/RoomPersonality");
                if (personality != null)
                {
                    return personality;
                }

                personality = world.transform.Find("RoomPersonality");
                if (personality != null)
                {
                    return personality;
                }
            }

            Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "RoomPersonality")
                {
                    return all[i];
                }
            }

            return null;
        }

        private static List<GameObject> CollectBlockers(Transform worldRoot)
        {
            List<GameObject> blockers = new List<GameObject>();
            AddIfFound(blockers, worldRoot, "ExitArea/Suitcase");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/ExitArea/Suitcase");
            AddIfFound(blockers, worldRoot, "DepartureDoor");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/DepartureDoor");
            AddIfFound(blockers, worldRoot, "RentalRoomInterior/PortraitWall");
            AddIfFound(blockers, worldRoot, "Level1_IndoorArea/RentalRoomInterior/PortraitWall");
            return blockers;
        }

        private static void AddIfFound(List<GameObject> list, Transform root, string path)
        {
            Transform found = Level1ScenePlacement.FindInRoom(root, path) ?? root.Find(path);
            if (found != null)
            {
                list.Add(found.gameObject);
            }
        }

        private static bool HasOverlap(GameObject plaque, List<GameObject> blockers)
        {
            if (!Level1ScenePlacement.TryGetWorldBounds(plaque, out Bounds plaqueBounds))
            {
                return false;
            }

            for (int i = 0; i < blockers.Count; i++)
            {
                GameObject blocker = blockers[i];
                if (blocker == null || blocker == plaque
                    || !Level1ScenePlacement.TryGetWorldBounds(blocker, out Bounds blockerBounds))
                {
                    continue;
                }

                if (Overlaps3D(plaqueBounds, blockerBounds, OverlapClearance))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Overlaps3D(Bounds a, Bounds b, float clearance)
        {
            return a.min.x - clearance < b.max.x
                && a.max.x + clearance > b.min.x
                && a.min.y - clearance < b.max.y
                && a.max.y + clearance > b.min.y
                && a.min.z - clearance < b.max.z
                && a.max.z + clearance > b.min.z;
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static void MarkStatic(GameObject root)
        {
            StaticEditorFlags flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI;
            MarkStaticRecursive(root, flags);
        }

        private static void MarkStaticRecursive(GameObject go, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(go, flags);
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                MarkStaticRecursive(t.GetChild(i).gameObject, flags);
            }
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 localPos, Vector3 localScale,
            Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        private static Material EnsureMat(string name, Color color, float smoothness)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = name;
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", 0.05f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureTintedGoldMat(string name, Color tint, float metallic = 0.88f,
            float smoothness = 0.58f)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = name;
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", tint);
            mat.SetColor("_Color", tint);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureCalligraphyGoldMat(string name)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(CalligraphyTexturePath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.name = name;
                AssetDatabase.CreateAsset(mat, path);
            }

            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTexture("_MainTex", tex);
                mat.SetTexture("_BaseColorMap", tex);
            }

            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_Color", Color.white);
            mat.SetFloat("_Metallic", 0.92f);
            mat.SetFloat("_Smoothness", 0.62f);
            mat.SetFloat("_Glossiness", 0.62f);
            if (mat.HasProperty("_AlphaClip"))
            {
                mat.SetFloat("_AlphaClip", 1f);
            }

            if (mat.HasProperty("_Cutoff"))
            {
                mat.SetFloat("_Cutoff", 0.15f);
            }

            mat.EnableKeyword("_ALPHATEST_ON");
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 0f);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
#endif
