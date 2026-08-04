using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Runtime
{
    public class TrafficSpawner : MonoBehaviour
    {
        public GameObject obstaclePrefab;
        public GameObject[] obstaclePrefabs;
        public Transform[] spawnPoints;
        public float spawnInterval = 3.4f;
        public float spawnJitter = 0.7f;
        public int spawnBatchMin = 1;
        public int spawnBatchMax = 1;
        public float obstacleSpeed = 6.4f;
        public int damageAmount = 8;
        public Vector3 obstacleScale = Vector3.one;
        public Vector3 roadBoundsCenter = new Vector3(0f, 0f, 56f);
        public Vector3 roadBoundsSize = new Vector3(24f, 8f, 48f);
        public int maxActiveObstacles = 5;

        private readonly List<GameObject> spawned = new List<GameObject>();
        private readonly Dictionary<GameObject, Queue<GameObject>> inactiveByPrefab = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();
        private float timer;
        private int spawnIndex;

        private void OnEnable()
        {
            timer = 0.5f;
        }

        private void OnDisable()
        {
            ClearSpawned();
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }

            int batchMin = Mathf.Max(1, spawnBatchMin);
            int batchMax = Mathf.Max(batchMin, spawnBatchMax);
            int batchCount = Random.Range(batchMin, batchMax + 1);
            for (int i = 0; i < batchCount; i++)
            {
                SpawnObstacle();
                if (spawned.Count >= maxActiveObstacles)
                {
                    break;
                }
            }

            timer = spawnInterval + Random.Range(-spawnJitter, spawnJitter);
            if (timer < 1.2f)
            {
                timer = 1.2f;
            }
        }

        public void ClearSpawned()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] != null)
                {
                    Destroy(spawned[i]);
                }
            }

            spawned.Clear();

            foreach (KeyValuePair<GameObject, Queue<GameObject>> entry in inactiveByPrefab)
            {
                Queue<GameObject> pool = entry.Value;
                while (pool.Count > 0)
                {
                    GameObject obstacle = pool.Dequeue();
                    if (obstacle != null)
                    {
                        Destroy(obstacle);
                    }
                }
            }

            inactiveByPrefab.Clear();
            prefabByInstance.Clear();
        }

        public void ReleaseObstacle(GameObject obstacle)
        {
            if (obstacle == null)
            {
                return;
            }

            spawned.Remove(obstacle);
            if (!prefabByInstance.TryGetValue(obstacle, out GameObject prefab) || prefab == null)
            {
                Destroy(obstacle);
                return;
            }

            obstacle.SetActive(false);
            GetPool(prefab).Enqueue(obstacle);
        }

        private void SpawnObstacle()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return;
            }

            CleanupMissingReferences();
            if (spawned.Count >= maxActiveObstacles)
            {
                return;
            }

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (point == null)
            {
                return;
            }

            GameObject prefab = ResolveObstaclePrefab();
            if (prefab == null)
            {
                return;
            }

            GameObject obstacle = GetOrCreateObstacle(prefab);
            obstacle.name = prefab.name + "_Traffic";
            obstacle.transform.SetParent(transform, false);
            obstacle.transform.SetPositionAndRotation(point.position, point.rotation);
            obstacle.transform.localScale = obstacleScale;
            obstacle.SetActive(true);

            TrafficObstacle trafficObstacle = obstacle.GetComponent<TrafficObstacle>();
            if (trafficObstacle == null)
            {
                trafficObstacle = obstacle.AddComponent<TrafficObstacle>();
            }

            // Spawn point forward is the travel direction (yaw 0 = +Z look, but we use forward for lane heading).
            Vector3 travel = point.forward.sqrMagnitude > 0.001f ? point.forward.normalized : Vector3.right;
            // Prefer right-axis lanes authored with yaw 0/+180 (east/west on X).
            if (Mathf.Abs(point.right.x) > Mathf.Abs(point.forward.x))
            {
                travel = point.right.normalized;
            }

            trafficObstacle.Configure(this, obstacleSpeed, damageAmount, roadBoundsCenter, roadBoundsSize, travel);
            spawned.Add(obstacle);
        }

        private GameObject GetOrCreateObstacle(GameObject prefab)
        {
            Queue<GameObject> pool = GetPool(prefab);
            while (pool.Count > 0)
            {
                GameObject obstacle = pool.Dequeue();
                if (obstacle != null)
                {
                    return obstacle;
                }
            }

            GameObject instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            prefabByInstance[instance] = prefab;
            DisableNestedPhysicsComponents(instance);
            EnsureObstacleCollider(instance);
            return instance;
        }

        private Queue<GameObject> GetPool(GameObject prefab)
        {
            if (!inactiveByPrefab.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                inactiveByPrefab[prefab] = pool;
            }

            return pool;
        }

        private GameObject ResolveObstaclePrefab()
        {
            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
            {
                for (int i = 0; i < obstaclePrefabs.Length; i++)
                {
                    int index = (spawnIndex + i) % obstaclePrefabs.Length;
                    GameObject candidate = obstaclePrefabs[index];
                    if (candidate != null)
                    {
                        spawnIndex = index + 1;
                        return candidate;
                    }
                }
            }

            return obstaclePrefab;
        }

        private static void DisableNestedPhysicsComponents(GameObject obstacle)
        {
            if (obstacle == null)
            {
                return;
            }

            Collider[] colliders = obstacle.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].gameObject != obstacle)
                {
                    colliders[i].enabled = false;
                }
            }

            Rigidbody[] bodies = obstacle.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] != null && bodies[i].gameObject != obstacle)
                {
                    bodies[i].isKinematic = true;
                    bodies[i].useGravity = false;
                }
            }
        }

        private void CleanupMissingReferences()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] == null)
                {
                    spawned.RemoveAt(i);
                }
            }
        }

        private static void EnsureObstacleCollider(GameObject obstacle)
        {
            Collider collider = obstacle.GetComponent<Collider>();
            if (collider == null)
            {
                BoxCollider box = obstacle.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 0.65f, 0f);
                box.size = new Vector3(1.15f, 1.3f, 2.2f);
                collider = box;
            }

            collider.isTrigger = true;
        }
    }
}
