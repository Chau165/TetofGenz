using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Orients urban prefabs so their street facade faces the highway.</summary>
    public static class HighwayUrbanBuildingOrientation
    {
        private static readonly Dictionary<string, float> PrefabYawOffsetByName = new Dictionary<string, float>
        {
            { "Building_1", 0f },
            { "Building_2", 0f },
            { "Building_3", 180f },
            { "Building_4", 0f },
            { "Building_5", 180f },
            { "Building_6", 0f },
            { "Building_7", 0f },
            { "Building_8", 180f },
            { "Building_9", 0f },
            { "Building_Small_1", 0f },
            { "Building_Medium_2_001", 0f },
            { "Building_Large_2", 0f },
            { "House_2", 0f },
            { "House_3", 180f },
            { "Hot_Dog", 0f },
            { "Newspaper_Stand", 0f },
            { "Tram_Stop", 0f },
            { "book stall", 0f },
            { "vending machine", 180f },
            { "Decor_BusStop", 0f },
            { "Decor_TrashBin", 0f }
        };

        public static Quaternion FaceRoadRotation(GameObject prefabAsset, bool leftSide)
        {
            float roadYaw = leftSide ? 90f : -90f;
            float offset = GetPrefabYawOffset(prefabAsset != null ? prefabAsset.name : string.Empty);
            return Quaternion.Euler(0f, roadYaw + offset, 0f);
        }

        public static void ApplyFacing(GameObject root, GameObject prefabAsset, bool leftSide)
        {
            if (root == null)
            {
                return;
            }

            Transform visual = GetVisualTransform(root);
            Quaternion best = FindBestFacingRotation(visual.gameObject, prefabAsset, leftSide);
            visual.localRotation = best;
        }

        public static bool IsFacingRoad(GameObject root, bool leftSide)
        {
            return ScoreFacing(root, leftSide) > 1.35f;
        }

        public static void CorrectFacing(GameObject root, GameObject prefabAsset, bool leftSide)
        {
            if (root == null || IsFacingRoad(root, leftSide))
            {
                return;
            }

            Transform visual = GetVisualTransform(root);
            visual.localRotation = FindBestFacingRotation(visual.gameObject, prefabAsset, leftSide);
        }

        private static Transform GetVisualTransform(GameObject root)
        {
            return root.transform.childCount > 0 ? root.transform.GetChild(0) : root.transform;
        }

        private static Quaternion FindBestFacingRotation(GameObject visualRoot, GameObject prefabAsset, bool leftSide)
        {
            Transform visual = visualRoot.transform;
            float roadYaw = leftSide ? 90f : -90f;
            string prefabName = prefabAsset != null ? prefabAsset.name : visualRoot.name;
            float baseOffset = GetPrefabYawOffset(prefabName);
            float[] offsets = { baseOffset, baseOffset + 90f, baseOffset + 180f, baseOffset + 270f };

            float bestScore = float.MinValue;
            Quaternion best = visual.localRotation;
            for (int i = 0; i < offsets.Length; i++)
            {
                visual.localRotation = Quaternion.Euler(0f, roadYaw + offsets[i], 0f);
                float score = ScoreFacing(visualRoot.transform.parent != null ? visualRoot.transform.parent.gameObject : visualRoot, leftSide);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = visual.localRotation;
                }
            }

            return best;
        }

        private static float ScoreFacing(GameObject root, bool leftSide)
        {
            Transform visual = GetVisualTransform(root);
            Vector3 towardRoad = leftSide ? Vector3.right : Vector3.left;

            float axisScore = Mathf.Max(
                Mathf.Abs(Vector3.Dot(visual.forward, towardRoad)),
                Mathf.Abs(Vector3.Dot(-visual.forward, towardRoad)),
                Mathf.Abs(Vector3.Dot(visual.right, towardRoad)),
                Mathf.Abs(Vector3.Dot(-visual.right, towardRoad)));

            Bounds bounds = ComputeBounds(root);
            float widthAlongStreet = bounds.size.z;
            float depthTowardRoad = bounds.size.x;
            float profileScore = widthAlongStreet / Mathf.Max(depthTowardRoad, 0.2f);
            return axisScore * 2.2f + Mathf.Clamp(profileScore, 0.4f, 2.4f);
        }

        private static Bounds ComputeBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one * 2f);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static float GetPrefabYawOffset(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return 0f;
            }

            if (PrefabYawOffsetByName.TryGetValue(prefabName, out float offset))
            {
                return offset;
            }

            string lower = prefabName.ToLowerInvariant();
            foreach (KeyValuePair<string, float> pair in PrefabYawOffsetByName)
            {
                if (lower.Contains(pair.Key.ToLowerInvariant()))
                {
                    return pair.Value;
                }
            }

            return 0f;
        }
    }
}
