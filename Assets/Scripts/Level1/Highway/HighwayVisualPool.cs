using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Recycles Kenney traffic visual prefabs for highway vehicles.</summary>
    public class HighwayVisualPool : MonoBehaviour
    {
        private readonly Dictionary<int, Queue<Transform>> pools = new Dictionary<int, Queue<Transform>>();
        private Transform inactiveRoot;

        private void Awake()
        {
            inactiveRoot = new GameObject("HighwayVisualPool_Inactive").transform;
            inactiveRoot.SetParent(transform, false);
            inactiveRoot.gameObject.SetActive(false);
        }

        public Transform Acquire(GameObject prefab, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }

            int key = prefab.GetInstanceID();
            if (pools.TryGetValue(key, out Queue<Transform> queue) && queue.Count > 0)
            {
                Transform visual = queue.Dequeue();
                visual.SetParent(parent, false);
                visual.localPosition = Vector3.zero;
                visual.localRotation = Quaternion.identity;
                visual.localScale = Vector3.one;
                visual.gameObject.SetActive(true);
                return visual;
            }

            GameObject instance = Instantiate(prefab, parent);
            instance.name = "HighwayTrafficVisual";
            return instance.transform;
        }

        public void Release(Transform visual, GameObject prefabSource)
        {
            if (visual == null || prefabSource == null)
            {
                return;
            }

            visual.gameObject.SetActive(false);
            visual.SetParent(inactiveRoot, false);

            int key = prefabSource.GetInstanceID();
            if (!pools.TryGetValue(key, out Queue<Transform> queue))
            {
                queue = new Queue<Transform>();
                pools[key] = queue;
            }

            queue.Enqueue(visual);
        }
    }
}
