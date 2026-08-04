using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Helpers for spawning low-poly roadside props on the highway.</summary>
    public static class HighwayPropUtility
    {
        public static GameObject SpawnPrefab(GameObject prefab, Transform parent, bool leftSide, int seed)
        {
            if (prefab == null || parent == null)
            {
                return null;
            }

            ClearChildren(parent);

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(0f, leftSide ? 90f : -90f, 0f);

            float yawJitter = HighwayPropVariation.YawJitter(seed);
            instance.transform.localRotation *= Quaternion.Euler(0f, yawJitter, 0f);

            float scale = HighwayPropVariation.ScaleMultiplier(seed);
            instance.transform.localScale = Vector3.one * scale;

            StripColliders(instance);
            return instance;
        }

        public static GameObject PickPrefab(GameObject[] options, int seed)
        {
            if (options == null || options.Length == 0)
            {
                return null;
            }

            int index = Mathf.Abs(seed) % options.Length;
            for (int i = 0; i < options.Length; i++)
            {
                GameObject candidate = options[(index + i) % options.Length];
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        public static void StripColliders(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                SafeDestroy(colliders[i]);
            }
        }

        public static void SafeDestroy(Object obj)
        {
            if (obj == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(obj);
                return;
            }
#endif
            Object.Destroy(obj);
        }

        public static void ClearChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                // Immediate destroy so procedural rebuilds don't stack for a frame.
#if UNITY_EDITOR
                Object.DestroyImmediate(child.gameObject);
#else
                Object.Destroy(child.gameObject);
#endif
            }
        }
    }

    internal static class HighwayPropVariation
    {
        public static float YawJitter(int seed) => ((seed * 17) % 11) - 5f;

        public static float ScaleMultiplier(int seed)
        {
            float t = (seed * 13 % 100) / 100f;
            return Mathf.Lerp(0.82f, 1.18f, t);
        }
    }
}
