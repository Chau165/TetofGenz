using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Capsule-based placement checks for outdoor NPC spawn and shop stand positions.</summary>
    public static class NpcPlacementValidator
    {
        public const float DefaultBodyHeight = 1.8f;
        public const float DefaultBodyRadius = 0.26f;
        public const float DefaultMinNpcSeparation = 0.82f;
        public const int MaxPlacementAttempts = 16;

        private static readonly Vector2[] OffsetRing =
        {
            Vector2.zero,
            new Vector2(0.45f, 0f),
            new Vector2(-0.45f, 0f),
            new Vector2(0f, 0.45f),
            new Vector2(0f, -0.45f),
            new Vector2(0.42f, 0.42f),
            new Vector2(-0.42f, 0.42f),
            new Vector2(0.42f, -0.42f),
            new Vector2(-0.42f, -0.42f),
            new Vector2(0.85f, 0f),
            new Vector2(-0.85f, 0f),
            new Vector2(0f, 0.85f),
            new Vector2(0f, -0.85f),
            new Vector2(0.72f, 0.55f),
            new Vector2(-0.72f, 0.55f),
            new Vector2(0.72f, -0.55f),
        };

        public static bool TryFindValidPosition(
            Vector3 desiredFeetPosition,
            float scale,
            IReadOnlyList<Vector3> placedNpcFeet,
            out Vector3 resolvedFeetPosition,
            Collider[] ignoreColliders = null,
            float minNpcSeparation = DefaultMinNpcSeparation)
        {
            resolvedFeetPosition = desiredFeetPosition;

            for (int i = 0; i < MaxPlacementAttempts && i < OffsetRing.Length; i++)
            {
                Vector2 offset = OffsetRing[i];
                Vector3 candidate = desiredFeetPosition + new Vector3(offset.x, 0f, offset.y);
                if (!TrySampleGround(candidate, out candidate))
                {
                    continue;
                }

                if (!IsCapsuleClear(candidate, scale, ignoreColliders))
                {
                    continue;
                }

                if (!HasNpcSeparation(candidate, placedNpcFeet, minNpcSeparation))
                {
                    continue;
                }

                resolvedFeetPosition = candidate;
                return true;
            }

            Debug.LogWarning(
                $"[NpcPlacementValidator] No clear placement near {desiredFeetPosition} after {MaxPlacementAttempts} attempts.");
            return false;
        }

        public static bool TrySampleGround(Vector3 worldPosition, out Vector3 feetPosition)
        {
            feetPosition = worldPosition;
            Vector3 origin = new Vector3(worldPosition.x, worldPosition.y + 2.5f, worldPosition.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                feetPosition = new Vector3(worldPosition.x, hit.point.y, worldPosition.z);
                return true;
            }

            feetPosition = new Vector3(worldPosition.x, Level1OutdoorSettings.WalkSurfaceY, worldPosition.z);
            return false;
        }

        public static bool IsCapsuleClear(Vector3 feetPosition, float scale, Collider[] ignoreColliders = null)
        {
            ComputeCapsule(feetPosition, scale, out Vector3 bottom, out Vector3 top, out float radius);
            Collider[] overlaps = Physics.OverlapCapsule(bottom, top, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < overlaps.Length; i++)
            {
                Collider overlap = overlaps[i];
                if (overlap == null || overlap.isTrigger)
                {
                    continue;
                }

                if (IsIgnored(overlap, ignoreColliders))
                {
                    continue;
                }

                if (overlap.GetComponentInParent<NPCDialogue>() != null)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public static bool HasNpcSeparation(
            Vector3 feetPosition,
            IReadOnlyList<Vector3> placedNpcFeet,
            float minSeparation)
        {
            if (placedNpcFeet == null)
            {
                return true;
            }

            float minSqr = minSeparation * minSeparation;
            for (int i = 0; i < placedNpcFeet.Count; i++)
            {
                Vector3 delta = placedNpcFeet[i] - feetPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude < minSqr)
                {
                    return false;
                }
            }

            return true;
        }

        public static void ComputeCapsule(Vector3 feetPosition, float scale, out Vector3 bottom, out Vector3 top, out float radius)
        {
            float height = DefaultBodyHeight * scale;
            radius = DefaultBodyRadius * scale;
            float cylinder = Mathf.Max(0.05f, height - radius * 2f);
            bottom = feetPosition + Vector3.up * radius;
            top = bottom + Vector3.up * cylinder;
        }

        private static bool IsIgnored(Collider candidate, Collider[] ignoreColliders)
        {
            if (ignoreColliders == null)
            {
                return false;
            }

            for (int i = 0; i < ignoreColliders.Length; i++)
            {
                if (ignoreColliders[i] == candidate)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        public static void DrawCapsuleGizmo(Vector3 feetPosition, float scale, Color color)
        {
            ComputeCapsule(feetPosition, scale, out Vector3 bottom, out Vector3 top, out float radius);
            Gizmos.color = color;
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
            Gizmos.DrawLine(bottom - Vector3.forward * radius, top - Vector3.forward * radius);
            Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
            Gizmos.DrawLine(bottom - Vector3.right * radius, top - Vector3.right * radius);
        }
#endif
    }
}
