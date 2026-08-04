using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// Approach: behind the bus, sightline straight down the road (not the highway front-facing chase).
    /// Reunion: -X shoulder angle for the branch junction and parents.
    /// </summary>
    public static class HometownArrivalCamera
    {
        // Highway driving uses (0, 17, -13) / (0, 1.5, 0) — camera ahead of the car looking back.
        public static readonly Vector3 ApproachOffset = new Vector3(0f, 10.5f, 15f);
        public static readonly Vector3 ApproachLookOffset = new Vector3(0f, 1.25f, -8f);
        public const float ApproachSmoothSpeed = 9f;

        public static readonly Vector3 ReunionOffset = new Vector3(-2.6f, 8.2f, 11.5f);
        public static readonly Vector3 ReunionLookOffset = new Vector3(-1.55f, 1.35f, -7.2f);
        public const float ReunionSmoothSpeed = 9f;

        // Legacy names used by scene builder / tooling.
        public static Vector3 ChaseOffset => ReunionOffset;
        public static Vector3 ChaseLookOffset => ReunionLookOffset;
        public static float ChaseSmoothSpeed => ReunionSmoothSpeed;

        public static void ApplyApproachDefaults(TopDownCameraFollow follow)
        {
            if (follow == null)
            {
                return;
            }

            follow.offset = ApproachOffset;
            follow.lookOffset = ApproachLookOffset;
            follow.smoothSpeed = ApproachSmoothSpeed;
        }

        public static void ApplyReunionDefaults(TopDownCameraFollow follow)
        {
            if (follow == null)
            {
                return;
            }

            follow.offset = ReunionOffset;
            follow.lookOffset = ReunionLookOffset;
            follow.smoothSpeed = ReunionSmoothSpeed;
        }

        public static void ApplyChaseDefaults(TopDownCameraFollow follow) => ApplyReunionDefaults(follow);
    }
}
