using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Safe exit placement beside the stopped vehicle on walkable ground.</summary>
    public static class HometownArrivalExitSpawn
    {
        private const float DefaultLateralOffset = 2.6f;
        private const float DefaultForwardOffset = -1.25f;
        private const float GroundProbeUp = 6f;
        private const float GroundProbeDown = 8f;
        private const float FootClearance = 0.08f;
        private const float MinGroundNormalY = 0.45f;

        public static Transform EnsureExitPoint(Transform bus, Transform parent)
        {
            Transform existing = GameObject.Find("ArrivalExitPoint")?.transform;
            if (existing == null)
            {
                GameObject marker = new GameObject("ArrivalExitPoint");
                if (parent != null)
                {
                    marker.transform.SetParent(parent, false);
                }

                existing = marker.transform;
            }

            RefreshExitPoint(existing, bus);
            return existing;
        }

        public static void RefreshExitPoint(Transform exitPoint, Transform bus)
        {
            if (exitPoint == null || bus == null)
            {
                return;
            }

            Vector3 desired = ComputeBesideBus(bus);
            exitPoint.SetPositionAndRotation(
                SnapToGround(desired, HighwayLaneSettings.RoadSurfaceY),
                Quaternion.Euler(0f, bus.eulerAngles.y, 0f));
        }

        public static Vector3 ComputeBesideBus(Transform bus)
        {
            // Exit toward the curb: left shoulder parking → bus.left, right shoulder → bus.right.
            float curbSign = bus.position.x <= 0.01f ? -1f : 1f;
            return bus.position
                + bus.right * (DefaultLateralOffset * curbSign)
                + bus.forward * DefaultForwardOffset;
        }

        public static Vector3 SnapToGround(Vector3 desired, float fallbackY)
        {
            Vector3 origin = new Vector3(desired.x, desired.y + GroundProbeUp, desired.z);
            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    GroundProbeUp + GroundProbeDown,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore)
                && hit.normal.y >= MinGroundNormalY)
            {
                return new Vector3(desired.x, hit.point.y + FootClearance, desired.z);
            }

            return new Vector3(desired.x, fallbackY + FootClearance, desired.z);
        }
    }
}
