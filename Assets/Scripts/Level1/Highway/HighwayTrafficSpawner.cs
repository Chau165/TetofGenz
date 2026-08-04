using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Spawns pooled traffic ahead of the player with difficulty scaling.</summary>
    public class HighwayTrafficSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] sedanPrefabs;
        [SerializeField] private GameObject[] truckPrefabs;
        [SerializeField] private GameObject[] busPrefabs;
        [SerializeField] private GameObject[] vanPrefabs;
        [SerializeField] private int maxActiveVehicles = 24;
        [SerializeField] private float spawnAheadMin = 55f;
        [SerializeField] private float spawnAheadMax = 130f;
        [SerializeField] private float despawnBehind = 25f;
        [SerializeField] private float baseSpawnInterval = 2.4f;
        [SerializeField] private float minSpawnInterval = 0.85f;
        [SerializeField] private float minBumperGap = 8f;
        [SerializeField] private int spawnPlacementAttempts = 10;
        [SerializeField] private float overlapSeparationSmooth = 14f;
        [SerializeField] private float overlapSpeedDamp = 22f;
        [SerializeField] private float overlapHardPenetrationRatio = 0.35f;

        private readonly List<HighwayTrafficVehicle> active = new List<HighwayTrafficVehicle>();
        private readonly Queue<HighwayTrafficVehicle> hostPool = new Queue<HighwayTrafficVehicle>();
        private HighwayVisualPool visualPool;
        private HighwayGameController game;
        private float spawnTimer;
        private int nextSpawnLane = 2;
        private bool victoryMode;

        public void Initialize(
            HighwayGameController controller,
            HighwayVisualPool pool,
            GameObject[] sedans,
            GameObject[] trucks,
            GameObject[] buses,
            GameObject[] vans)
        {
            game = controller;
            visualPool = pool;
            sedanPrefabs = sedans;
            truckPrefabs = trucks;
            busPrefabs = buses;
            vanPrefabs = vans;
            PrewarmHosts(8);
        }

        public void EnterVictoryMode()
        {
            victoryMode = true;
            spawnTimer = float.MaxValue;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                Despawn(active[i]);
            }
        }

        public void Tick(float deltaTime, float playerForwardSpeed, float difficulty01)
        {
            if (!victoryMode)
            {
                spawnTimer -= deltaTime;
                float interval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, difficulty01);

                if (spawnTimer <= 0f && active.Count < maxActiveVehicles)
                {
                    TrySpawn(playerForwardSpeed, difficulty01);
                    spawnTimer = interval * Random.Range(0.72f, 1.28f);
                }
            }

            Vector3 playerPos = game.PlayerPosition;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                HighwayTrafficVehicle vehicle = active[i];
                vehicle.Tick(deltaTime);

                if (vehicle.transform.position.z < playerPos.z - despawnBehind
                    || vehicle.transform.position.z > playerPos.z + spawnAheadMax + 40f)
                {
                    Despawn(vehicle);
                }
            }

            ResolveLaneOverlaps(playerPos.z, deltaTime);
        }

        public void CheckCollisions(Vector3 playerPos, float playerLength)
        {
            if (victoryMode)
            {
                return;
            }

            for (int i = 0; i < active.Count; i++)
            {
                HighwayTrafficVehicle vehicle = active[i];
                if (vehicle.OverlapsPlayer(playerPos, playerLength))
                {
                    game.RegisterCollision(vehicle.transform.position);
                    Despawn(vehicle);
                    return;
                }
            }
        }

        private void TrySpawn(float playerForwardSpeed, float difficulty01)
        {
            HighwayTrafficVehicle.TrafficKind kind = PickKind();
            GameObject prefab = PickPrefab(kind);
            if (prefab == null)
            {
                return;
            }

            float scale = ScaleForKind(kind);
            float length = LengthForKind(kind) * scale;
            float speedZ = SpeedForLane(playerForwardSpeed, difficulty01, kind);
            float playerZ = game.PlayerPosition.z;

            for (int attempt = 0; attempt < spawnPlacementAttempts; attempt++)
            {
                float spawnZ = playerZ + Random.Range(spawnAheadMin, spawnAheadMax);
                int lane = PickSpawnLaneAt(spawnZ, length);
                if (lane < 0)
                {
                    continue;
                }

                Vector3 spawnPos = game.PlayerPosition;
                spawnPos.z = spawnZ;
                spawnPos.x = HighwayLaneSettings.LaneCenterX(lane, spawnPos.z - playerZ);
                spawnPos.y = HighwayLaneSettings.RoadSurfaceY;

                HighwayTrafficVehicle host = AcquireHost();
                host.transform.position = spawnPos;
                host.Activate(prefab, lane, speedZ, scale, Random.Range(0, 8), kind, length);
                active.Add(host);
                nextSpawnLane = (lane + 1) % HighwayLaneSettings.LaneCount;
                return;
            }
        }

        private int PickSpawnLaneAt(float spawnZ, float length)
        {
            int laneCount = HighwayLaneSettings.LaneCount;
            for (int attempt = 0; attempt < laneCount * 2; attempt++)
            {
                int lane = (nextSpawnLane + attempt) % laneCount;
                if (IsSpawnPositionClear(lane, spawnZ, length))
                {
                    return lane;
                }
            }

            return -1;
        }

        private bool IsSpawnPositionClear(int lane, float spawnZ, float length)
        {
            if (!IsClearOfPlayer(lane, spawnZ, length))
            {
                return false;
            }

            for (int i = 0; i < active.Count; i++)
            {
                HighwayTrafficVehicle vehicle = active[i];
                if (vehicle.LaneIndex != lane)
                {
                    continue;
                }

                if (!HasLongitudinalGap(spawnZ, length, vehicle.transform.position.z, vehicle.Length))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsClearOfPlayer(int lane, float spawnZ, float length)
        {
            if (game.PlayerLaneIndex != lane)
            {
                return true;
            }

            Vector3 playerPos = game.PlayerPosition;
            float dz = spawnZ - playerPos.z;
            if (dz <= 0f)
            {
                return true;
            }

            float required = (length + game.PlayerVehicleLength) * 0.5f + minBumperGap;
            return dz >= required;
        }

        private bool HasLongitudinalGap(float zA, float lengthA, float zB, float lengthB)
        {
            float required = (lengthA + lengthB) * 0.5f + minBumperGap * 1.08f;
            return Mathf.Abs(zA - zB) >= required;
        }

        private void ResolveLaneOverlaps(float playerZ, float deltaTime)
        {
            for (int lane = 0; lane < HighwayLaneSettings.LaneCount; lane++)
            {
                List<HighwayTrafficVehicle> laneVehicles = null;
                for (int i = 0; i < active.Count; i++)
                {
                    HighwayTrafficVehicle vehicle = active[i];
                    if (vehicle.LaneIndex != lane)
                    {
                        continue;
                    }

                    laneVehicles ??= new List<HighwayTrafficVehicle>(4);
                    laneVehicles.Add(vehicle);
                }

                if (laneVehicles == null || laneVehicles.Count < 2)
                {
                    continue;
                }

                laneVehicles.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

                for (int i = 1; i < laneVehicles.Count; i++)
                {
                    HighwayTrafficVehicle rear = laneVehicles[i - 1];
                    HighwayTrafficVehicle front = laneVehicles[i];
                    float gap = front.transform.position.z - rear.transform.position.z
                                - (rear.Length + front.Length) * 0.5f;

                    if (gap >= minBumperGap * 0.92f)
                    {
                        continue;
                    }

                    float penetration = Mathf.Clamp01((minBumperGap - gap) / minBumperGap);
                    float desiredSpeed = Mathf.Lerp(front.SpeedZ, front.SpeedZ * 0.5f, penetration);
                    rear.SetSpeedZ(Mathf.MoveTowards(rear.SpeedZ, desiredSpeed, overlapSpeedDamp * deltaTime));

                    if (gap >= minBumperGap * overlapHardPenetrationRatio)
                    {
                        continue;
                    }

                    float targetZ = front.transform.position.z
                                    - (rear.Length + front.Length) * 0.5f
                                    - minBumperGap;

                    if (targetZ < playerZ - despawnBehind)
                    {
                        Despawn(rear);
                        laneVehicles.RemoveAt(i - 1);
                        i--;
                        continue;
                    }

                    rear.SmoothNudgeWorldZ(targetZ, deltaTime, overlapSeparationSmooth);
                }
            }
        }

        private static float SpeedForLane(
            float playerForwardSpeed,
            float difficulty01,
            HighwayTrafficVehicle.TrafficKind kind)
        {
            float fastBoost = Mathf.Lerp(0f, 6f, difficulty01);
            float slowPenalty = Mathf.Lerp(0f, 4f, difficulty01);

            float sameDir = Random.value < 0.45f
                ? playerForwardSpeed - Random.Range(4f, 10f) - slowPenalty
                : playerForwardSpeed + Random.Range(2f, 8f) + fastBoost * 0.35f;

            if (kind == HighwayTrafficVehicle.TrafficKind.Truck
                || kind == HighwayTrafficVehicle.TrafficKind.Bus)
            {
                sameDir -= 3f;
            }

            return Mathf.Max(8f, sameDir);
        }

        private static HighwayTrafficVehicle.TrafficKind PickKind()
        {
            float roll = Random.value;
            if (roll < 0.42f)
            {
                return HighwayTrafficVehicle.TrafficKind.Sedan;
            }

            if (roll < 0.68f)
            {
                return HighwayTrafficVehicle.TrafficKind.Van;
            }

            if (roll < 0.86f)
            {
                return HighwayTrafficVehicle.TrafficKind.Truck;
            }

            return HighwayTrafficVehicle.TrafficKind.Bus;
        }

        private GameObject PickPrefab(HighwayTrafficVehicle.TrafficKind kind)
        {
            GameObject[] list = kind switch
            {
                HighwayTrafficVehicle.TrafficKind.Truck => truckPrefabs,
                HighwayTrafficVehicle.TrafficKind.Bus => busPrefabs,
                HighwayTrafficVehicle.TrafficKind.Van => vanPrefabs,
                _ => sedanPrefabs
            };

            if (list == null || list.Length == 0)
            {
                return null;
            }

            return list[Random.Range(0, list.Length)];
        }

        private const float PlayerComparableScale = 1.55f;

        private static float ScaleForKind(HighwayTrafficVehicle.TrafficKind kind)
        {
            return kind switch
            {
                HighwayTrafficVehicle.TrafficKind.Truck => PlayerComparableScale * 1.06f,
                HighwayTrafficVehicle.TrafficKind.Bus => PlayerComparableScale * 1.1f,
                HighwayTrafficVehicle.TrafficKind.Van => PlayerComparableScale * 1.02f,
                _ => PlayerComparableScale
            };
        }

        private static float LengthForKind(HighwayTrafficVehicle.TrafficKind kind)
        {
            return kind switch
            {
                HighwayTrafficVehicle.TrafficKind.Truck => 5.6f,
                HighwayTrafficVehicle.TrafficKind.Bus => 6.4f,
                HighwayTrafficVehicle.TrafficKind.Van => 4.8f,
                _ => 4.2f
            };
        }

        private void PrewarmHosts(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ReturnHost(CreateHost());
            }
        }

        private HighwayTrafficVehicle AcquireHost()
        {
            if (hostPool.Count > 0)
            {
                return hostPool.Dequeue();
            }

            return CreateHost();
        }

        private void Despawn(HighwayTrafficVehicle vehicle)
        {
            vehicle.Deactivate();
            active.Remove(vehicle);
            ReturnHost(vehicle);
        }

        private void ReturnHost(HighwayTrafficVehicle host)
        {
            host.transform.SetParent(transform, false);
            hostPool.Enqueue(host);
        }

        private HighwayTrafficVehicle CreateHost()
        {
            GameObject go = new GameObject("HighwayTrafficHost");
            go.transform.SetParent(transform, false);
            HighwayTrafficVehicle vehicle = go.AddComponent<HighwayTrafficVehicle>();
            vehicle.BindPool(visualPool);
            go.SetActive(false);
            return vehicle;
        }
    }
}
