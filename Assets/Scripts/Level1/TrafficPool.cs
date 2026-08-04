using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Object pool for traffic vehicle hosts and their visual meshes.</summary>
    public class TrafficPool : MonoBehaviour
    {
        [SerializeField] private int prewarmCount = 6;
        [SerializeField] private int maxActiveCount = 14;

        private readonly Queue<VehicleTraffic> availableHosts = new Queue<VehicleTraffic>();
        private readonly Dictionary<int, Queue<Transform>> visualPools = new Dictionary<int, Queue<Transform>>();
        private Transform inactiveRoot;
        private int leasedCount;

        public int MaxActiveCount => maxActiveCount;

        private void Awake()
        {
            inactiveRoot = new GameObject("TrafficPool_Inactive").transform;
            inactiveRoot.SetParent(transform, false);
            inactiveRoot.gameObject.SetActive(false);
        }

        /// <summary>Resets lease bookkeeping when hosts cannot be reparented during hierarchy deactivation.</summary>
        public void ForceResetLeases()
        {
            leasedCount = 0;
        }

        /// <summary>Resets pooled hosts after the traffic manager finishes enabling.</summary>
        public void ReconcileAfterManagerEnable()
        {
            availableHosts.Clear();
            leasedCount = 0;

            VehicleTraffic[] existing = GetComponentsInChildren<VehicleTraffic>(true);
            var seen = new HashSet<VehicleTraffic>();
            for (int i = 0; i < existing.Length; i++)
            {
                VehicleTraffic host = existing[i];
                if (host == null || !seen.Add(host))
                {
                    continue;
                }

                host.PrepareForPool();
                SetParentIfNeeded(host.transform, inactiveRoot);
                host.gameObject.SetActive(false);
                availableHosts.Enqueue(host);
            }

            int targetPrewarm = Mathf.Clamp(prewarmCount, 2, maxActiveCount);
            while (availableHosts.Count < targetPrewarm)
            {
                ReturnHost(CreateHost());
            }
        }

        public VehicleTraffic AcquireHost()
        {
            if (leasedCount >= maxActiveCount)
            {
                return null;
            }

            VehicleTraffic host;
            if (availableHosts.Count > 0)
            {
                host = availableHosts.Dequeue();
            }
            else
            {
                host = CreateHost();
            }

            host.gameObject.SetActive(true);
            leasedCount++;
            return host;
        }

        public void ReturnHost(VehicleTraffic host)
        {
            if (host == null)
            {
                return;
            }

            host.PrepareForPool();

            if (!IsHierarchyModificationAllowed())
            {
                return;
            }

            SetParentIfNeeded(host.transform, inactiveRoot);
            host.gameObject.SetActive(false);
            availableHosts.Enqueue(host);
            leasedCount = Mathf.Max(0, leasedCount - 1);
        }

        public Transform AcquireVisual(GameObject prefab, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }

            int key = prefab.GetInstanceID();
            if (visualPools.TryGetValue(key, out Queue<Transform> queue) && queue.Count > 0)
            {
                Transform visual = queue.Dequeue();
                SetParentIfNeeded(visual, parent);
                visual.localPosition = Vector3.zero;
                visual.localScale = Vector3.one;
                visual.gameObject.SetActive(true);
                return visual;
            }

            GameObject instance = Instantiate(prefab, parent);
            instance.name = "TrafficVisual";
            return instance.transform;
        }

        public void ReturnVisual(Transform visual, GameObject prefabSource)
        {
            if (visual == null || prefabSource == null)
            {
                if (visual != null)
                {
                    Destroy(visual.gameObject);
                }

                return;
            }

            int key = prefabSource.GetInstanceID();
            if (!visualPools.TryGetValue(key, out Queue<Transform> queue))
            {
                queue = new Queue<Transform>();
                visualPools[key] = queue;
            }

            if (!IsHierarchyModificationAllowed())
            {
                Destroy(visual.gameObject);
                return;
            }

            SetParentIfNeeded(visual, inactiveRoot);
            visual.gameObject.SetActive(false);
            queue.Enqueue(visual);
        }

        private VehicleTraffic CreateHost()
        {
            GameObject hostObject = new GameObject("TrafficVehicle");
            hostObject.transform.SetParent(inactiveRoot, false);

            VehicleAnimator animator = hostObject.AddComponent<VehicleAnimator>();

            SphereCollider pickupTrigger = hostObject.AddComponent<SphereCollider>();
            pickupTrigger.isTrigger = true;
            pickupTrigger.radius = 2.85f;
            pickupTrigger.center = new Vector3(0f, 1.25f, 0f);
            pickupTrigger.enabled = false;

            BusStopPickup pickup = hostObject.AddComponent<BusStopPickup>();
            VehicleTraffic traffic = hostObject.AddComponent<VehicleTraffic>();

            traffic.ConfigureComponents(animator, pickup, pickupTrigger);
            pickup.Bind(traffic);

            return traffic;
        }

        private void OnDestroy()
        {
            if (inactiveRoot != null)
            {
                Destroy(inactiveRoot.gameObject);
            }
        }

        private bool IsHierarchyModificationAllowed()
        {
            return isActiveAndEnabled && gameObject.activeInHierarchy;
        }

        private static void SetParentIfNeeded(Transform child, Transform parent)
        {
            if (child == null || parent == null || child.parent == parent)
            {
                return;
            }

            child.SetParent(parent, false);
        }
    }
}
