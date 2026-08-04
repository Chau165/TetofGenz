#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Editor play shortcut: jump straight to the outdoor bus pickup wait spot.</summary>
    public static class Level1OutdoorBusStopPlayBootstrap
    {
        public const string PlayFromBusStopKey = "ChoTet_PlayFromOutdoorBusStop";

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TryBootstrapAfterSceneLoad()
        {
            if (!EditorPrefs.GetBool(PlayFromBusStopKey, false))
            {
                return;
            }

            EditorPrefs.DeleteKey(PlayFromBusStopKey);
            GameObject host = new GameObject("Level1_OutdoorBusStopBootstrap");
            host.AddComponent<Runner>();
        }
#endif

        private sealed class Runner : MonoBehaviour
        {
            private int attempts;

            private IEnumerator Start()
            {
                // Wait until Level1PhaseController.Start has finished hiding the outdoor area.
                yield return null;
                yield return null;

                while (attempts < 90)
                {
                    if (TryApply())
                    {
                        yield break;
                    }

                    attempts++;
                    yield return null;
                }

                Debug.LogWarning("[Level1OutdoorBusStopPlayBootstrap] Could not find player or phase controller.");
                Destroy(gameObject);
            }

            private bool TryApply()
            {
                Level1PhaseController phase = FindAnyObjectByType<Level1PhaseController>();
                PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
                if (phase == null || player == null)
                {
                    return false;
                }

                var applyOutdoor = typeof(Level1PhaseController).GetMethod(
                    "ApplyOutdoorPhase",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                applyOutdoor?.Invoke(phase, new object[] { player });

                Vector3 waitPosition = Level1PickupStopLayout.BusStopWaitPosition;
                player.TeleportTo(waitPosition, Quaternion.Euler(0f, 90f, 0f));

                TrafficManager traffic = FindAnyObjectByType<TrafficManager>();
                int bodyColors = traffic != null ? Mathf.Max(1, traffic.GetBodyColorCount()) : 5;
                int candidatePrefabs = traffic != null ? Mathf.Max(1, traffic.GetBusStopCandidatePrefabCount()) : 5;
                BusStopChallengeSession.BeginOutdoorChallenge(bodyColors, candidatePrefabs);
                traffic?.NotifyOutdoorPhaseReady();

                Debug.Log("[Level1OutdoorBusStopPlayBootstrap] Started at outdoor bus stop.");
                Destroy(gameObject);
                return true;
            }
        }
    }
}
