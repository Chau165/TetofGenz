using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Keeps road traffic running for the whole ChoTet day so the player must time the crosswalk.
    /// </summary>
    public class CrowdedRoadTrafficController : MonoBehaviour
    {
        public TrafficSpawner trafficSpawner;

        private void Awake()
        {
            ResolveTrafficSpawner();
            EnableTraffic();
        }

        private void OnEnable()
        {
            EnableTraffic();
        }

        private void Start()
        {
            EnableTraffic();
        }

        public void RefreshFromGameState()
        {
            EnableTraffic();
        }

        private void EnableTraffic()
        {
            ResolveTrafficSpawner();
            if (trafficSpawner == null)
            {
                return;
            }

            if (!trafficSpawner.gameObject.activeSelf)
            {
                trafficSpawner.gameObject.SetActive(true);
            }

            trafficSpawner.enabled = true;
        }

        private TrafficSpawner ResolveTrafficSpawner()
        {
            if (trafficSpawner != null)
            {
                return trafficSpawner;
            }

            trafficSpawner = GetComponentInChildren<TrafficSpawner>(true);
            if (trafficSpawner != null)
            {
                return trafficSpawner;
            }

            TrafficSpawner[] allSpawners = Resources.FindObjectsOfTypeAll<TrafficSpawner>();
            for (int i = 0; i < allSpawners.Length; i++)
            {
                if (allSpawners[i] != null)
                {
                    trafficSpawner = allSpawners[i];
                    return trafficSpawner;
                }
            }

            return null;
        }
    }
}
