using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Spawns parents once at the fixed bus-stop sidewalk — they do not follow the shuttle.</summary>
    [DefaultExecutionOrder(120)]
    public sealed class HometownArrivalParentsWaitingHost : MonoBehaviour
    {
        private static bool _spawned;

        private void Start()
        {
            if (!HometownArrivalBootstrap.IsArrivalScene(gameObject.scene.name))
            {
                return;
            }

            SpawnAtBusStopSidewalk();
        }

        public static void SpawnAtBusStopSidewalk()
        {
            if (_spawned)
            {
                return;
            }

            Transform worldRoot = GameObject.Find("ChoTetWorld")?.transform;
            HometownArrivalParentsWaiting.EnsureWaiting(worldRoot);
            _spawned = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSpawnState()
        {
            _spawned = false;
        }
    }
}
