using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Filters Quaternius complete building prefabs for roadside spawning.</summary>
    public static class QuaterniusCompleteBuildingValidator
    {
        public const float MinHeight = 2.5f;
        public const float MinDepth = 1.5f;
        public const float MinWidth = 1.5f;
        public const int MinRendererCount = 4;

        private static readonly string[] RejectedNameTokens =
        {
            "wall", "window", "door", "roof", "trim", "cornice", "facade", "prop_",
            "decal_", "sidewalk", "street_", "brick_", "metal_", "floor_",
            "entrance_", "stairs_", "awning", "column", "bollard", "planter",
            "manhole", "drain", "acunit", "frame_", "inset", "ornament", "interior"
        };

        public static bool IsValidPrefabAsset(GameObject prefab)
        {
            if (prefab == null)
            {
                return false;
            }

            if (IsRejectedName(prefab.name))
            {
                return false;
            }

            if (!IsWhitelistedCompleteName(prefab.name))
            {
                return false;
            }

            return TryMeasurePrefab(prefab, out Bounds bounds, out int rendererCount)
                && PassesBoundsRules(bounds, prefab.name, rendererCount);
        }

        public static bool IsValidSpawnedInstance(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            if (!IsQuaterniusCompleteBuildingName(instance.name) || HasFacadeAccentChildren(instance))
            {
                return false;
            }

            Bounds bounds = ComputeWorldBounds(instance);
            int rendererCount = instance.GetComponentsInChildren<Renderer>(true).Length;
            return PassesBoundsRules(bounds, instance.name, rendererCount);
        }

        public static bool TryMeasurePrefab(GameObject prefab, out Bounds bounds, out int rendererCount)
        {
            bounds = default;
            rendererCount = 0;
            if (prefab == null)
            {
                return false;
            }

#if UNITY_EDITOR
            GameObject probe = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (probe == null)
            {
                probe = Object.Instantiate(prefab);
            }

            probe.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            probe.transform.localScale = Vector3.one;
            bounds = ComputeWorldBounds(probe);
            rendererCount = probe.GetComponentsInChildren<Renderer>(true).Length;
            Object.DestroyImmediate(probe);
            return bounds.size.sqrMagnitude > 0.001f;
#else
            bounds = new Bounds(Vector3.zero, new Vector3(3f, 4f, 3f));
            rendererCount = 1;
            return true;
#endif
        }

        private static bool PassesBoundsRules(Bounds bounds, string name, int rendererCount)
        {
            Vector3 size = bounds.size;
            float height = size.y;
            float width = Mathf.Max(size.x, size.z);
            float depth = Mathf.Min(size.x, size.z);

            if (height < MinHeight || width < MinWidth || depth < MinDepth)
            {
                return false;
            }

            if (IsQuaterniusCompleteBuildingName(name))
            {
                return true;
            }

            return rendererCount > MinRendererCount;
        }

        public static bool IsQuaterniusCompleteBuildingName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.StartsWith("Building_", System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsWhitelistedCompleteName(string name) => IsQuaterniusCompleteBuildingName(name);

        public static bool IsRejectedName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            string lower = name.ToLowerInvariant();
            for (int i = 0; i < RejectedNameTokens.Length; i++)
            {
                if (lower.Contains(RejectedNameTokens[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasFacadeAccentChildren(GameObject root)
        {
            return root.transform.Find("FacadeAccents") != null;
        }

        public static Bounds ComputeWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name == "FacadeAccents")
                {
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
    }
}
