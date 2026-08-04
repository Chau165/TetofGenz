using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Distant backdrop filler at the outer zone — blocks horizon gaps only.</summary>
    public class HighwayHorizonLayer : MonoBehaviour
    {
        [SerializeField] private int backdropCount = 16;
        [SerializeField] private float horizonZ = 88f;
        [SerializeField] private float spacing = 14f;
        [SerializeField] private HighwayPrefabSet prefabSet = new HighwayPrefabSet();

        private readonly List<Transform> elements = new List<Transform>();
        private float scrollOffset;

        public void Initialize(HighwayMeshFactory.Palette materials, HighwayPrefabSet prefabs = null)
        {
            if (prefabs != null && prefabs.HasAnyContent)
            {
                prefabSet = prefabs;
            }

            BuildBackdrop();
        }

        public void SetScrollOffset(float distance)
        {
            scrollOffset = distance;
            float parallax = distance * 0.05f;

            for (int i = 0; i < elements.Count; i++)
            {
                Transform el = elements[i];
                if (el == null)
                {
                    continue;
                }

                HorizonElement marker = el.GetComponent<HorizonElement>();
                Vector3 basePos = marker != null ? marker.BasePosition : el.localPosition;
                el.localPosition = new Vector3(basePos.x, basePos.y, horizonZ + basePos.z - parallax);
            }
        }

        private void BuildBackdrop()
        {
            ClearElements();

            for (int i = 0; i < backdropCount; i++)
            {
                bool left = i % 2 == 0;
                float xSign = left ? -1f : 1f;
                float x = xSign * (HighwayLaneSettings.ZoneBackdropX + (i % 4) * 4f);
                float z = i * spacing - backdropCount * spacing * 0.5f;

                GameObject go = new GameObject("Backdrop_" + i);
                go.transform.SetParent(transform, false);

                GameObject prefab = HighwayPropUtility.PickPrefab(prefabSet.trees, i);
                if (prefab != null)
                {
                    HighwayPropUtility.SpawnPrefab(prefab, go.transform, left, i);
                }

                go.transform.localPosition = new Vector3(x, 0f, horizonZ + z);
                go.transform.localScale = Vector3.one * 1.15f;
                var marker = go.AddComponent<HorizonElement>();
                marker.BasePosition = new Vector3(x, 0f, z);
                elements.Add(go.transform);
            }
        }

        private void ClearElements()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] != null)
                {
                    SafeDestroy(elements[i].gameObject);
                }
            }

            elements.Clear();
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(obj);
                return;
            }
#endif
            Destroy(obj);
        }

        private class HorizonElement : MonoBehaviour
        {
            public Vector3 BasePosition;
        }
    }
}
