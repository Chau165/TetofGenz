using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Shared validated complete-building list for both roadside sides.</summary>
    public static class QuaterniusCompleteBuildingCatalog
    {
        private static GameObject[] cached;

        public static GameObject[] Resolve(QuaterniusDowntownPrefabLibrary downtown)
        {
            if (cached != null && cached.Length > 0)
            {
                return cached;
            }

            var list = new List<GameObject>();
            AddValidated(list, downtown?.completeBuildings);

            cached = Deduplicate(list);
            return cached;
        }

        public static void ClearCache() => cached = null;

        public static GameObject Pick(GameObject[] catalog, int seed, ref int lastIndex)
        {
            if (catalog == null || catalog.Length == 0)
            {
                return null;
            }

            int start = Mathf.Abs(seed) % catalog.Length;
            for (int i = 0; i < catalog.Length; i++)
            {
                int index = (start + i) % catalog.Length;
                GameObject candidate = catalog[index];
                if (candidate == null || index == lastIndex)
                {
                    continue;
                }

                if (!QuaterniusCompleteBuildingValidator.IsValidPrefabAsset(candidate))
                {
                    continue;
                }

                lastIndex = index;
                return candidate;
            }

            lastIndex = start;
            return catalog[start];
        }

        private static void AddValidated(List<GameObject> list, GameObject[] source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Length; i++)
            {
                GameObject prefab = source[i];
                if (prefab == null || list.Contains(prefab))
                {
                    continue;
                }

                if (QuaterniusCompleteBuildingValidator.IsValidPrefabAsset(prefab))
                {
                    list.Add(prefab);
                }
            }
        }

        private static GameObject[] Deduplicate(List<GameObject> list) => list.ToArray();
    }
}
