using System.Collections;
using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>After the shuttle stops: child walks to parents and greets them in Retro Chocolate dialogue.</summary>
    public static class HometownArrivalFamilyReunion
    {
        private const float StopDistance = 0.55f;
        private const float TurnSpeed = 12f;

        public static IEnumerator Run(PlayerTopDownController player, TopDownCameraFollow cameraFollow)
        {
            if (player == null)
            {
                yield break;
            }

            // Reveal + repair / restore Creative Character if the boarded clone broke.
            HometownArrivalPlayerVisualContext.EnsureVisibleRepairedPlayer(player);
            HometownArrivalShoulderBarrier.Remove();

            HometownArrivalParentsWaiting.EnsureWaiting(null);

            Transform bus = GameObject.Find("PickupBus")?.transform;
            if (!HometownArrivalParentsWaiting.TryBuildWalkPathToGreeting(
                    bus,
                    player.transform.position,
                    out Vector3[] waypoints))
            {
                yield break;
            }

            player.MovementLocked = true;
            if (cameraFollow != null)
            {
                cameraFollow.target = player.transform;
            }

            for (int i = 0; i < waypoints.Length; i++)
            {
                yield return WalkPlayerTo(player, waypoints[i]);
            }

            yield return FaceParents(player);
            player.StopScriptedLocomotion();
            yield return RunGreetingDialogue();
        }

        private static IEnumerator WalkPlayerTo(PlayerTopDownController player, Vector3 target)
        {
            while (true)
            {
                Vector3 flatDelta = target - player.transform.position;
                flatDelta.y = 0f;
                if (flatDelta.sqrMagnitude <= StopDistance * StopDistance)
                {
                    break;
                }

                player.MoveScripted(flatDelta);
                yield return null;
            }

            player.StopScriptedLocomotion();
        }

        private static IEnumerator FaceParents(PlayerTopDownController player)
        {
            if (!HometownArrivalParentsWaiting.TryGetParentsCenter(out Vector3 parentsCenter))
            {
                yield break;
            }

            Transform visualRoot = player.visualRoot;
            if (visualRoot == null)
            {
                yield break;
            }

            Vector3 look = parentsCenter - player.transform.position;
            look.y = 0f;
            if (look.sqrMagnitude < 0.02f)
            {
                yield break;
            }

            Quaternion targetRotation = Quaternion.LookRotation(look.normalized, Vector3.up);
            float elapsed = 0f;
            const float duration = 0.35f;
            Quaternion startRotation = visualRoot.rotation;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                visualRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                player.StopScriptedLocomotion();
                yield return null;
            }

            visualRoot.rotation = targetRotation;
        }

        private static IEnumerator RunGreetingDialogue()
        {
            RetroChocolateDialogueUI dialogue = RetroChocolateDialogueUI.Ensure();
            yield return dialogue.ShowSequence(
                ("Con", "Ba! Mẹ! Con về rồi!"),
                ("Mẹ", "Con về rồi hả? Mẹ đứng chờ từ sáng luôn đó!"),
                ("Bố", "Về kịp Tết rồi. Mau vào nhà, đường về còn dài lắm."),
                ("Con", "Dạ! Con về nhà với ba mẹ!"));
        }
    }
}
