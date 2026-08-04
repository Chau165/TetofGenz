using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Infinite-scroll layout for roadside belts and ring-buffer recycling for the road deck.</summary>
    public static class HighwayScrollLayout
    {
        public const int SegmentsBehindPlayer = 2;
        public const int SegmentsAheadOfPlayer = 4;

        /// <summary>Legacy sliding layout for grass belts (world scroll illusion).</summary>
        public static void PlaceSegments(IList<Transform> segments, float scrollOffset, float segmentLength, float y)
        {
            if (segments == null || segments.Count == 0)
            {
                return;
            }

            float startWorldZ = Mathf.Floor(
                (scrollOffset - segmentLength * SegmentsBehindPlayer) / segmentLength) * segmentLength;

            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                float localZ = startWorldZ + i * segmentLength - scrollOffset;
                segment.localPosition = new Vector3(0f, y, localZ);
                segment.localRotation = Quaternion.identity;
                segment.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Ring-buffer road layout: segment local Z is world Z. Segments that fall behind the player
        /// jump forward by one pool length so coverage never runs out (required for 2.2 km runs).
        /// </summary>
        public static void RecycleRoadSegments(
            IList<Transform> segments,
            float playerWorldZ,
            float segmentLength,
            int minBehind,
            int minAhead)
        {
            if (segments == null || segments.Count == 0)
            {
                return;
            }

            float poolLength = segments.Count * segmentLength;
            float recycleBehindZ = playerWorldZ - minBehind * segmentLength;

            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                float worldZ = segment.localPosition.z;
                while (worldZ + segmentLength < recycleBehindZ)
                {
                    worldZ += poolLength;
                }

                HighwayRoadSegmentPool.SnapSegmentRoot(segment, worldZ);
            }

            EnsureRoadCoverage(segments, playerWorldZ, segmentLength, minBehind, minAhead, poolLength);
        }

        private static void EnsureRoadCoverage(
            IList<Transform> segments,
            float playerWorldZ,
            float segmentLength,
            int minBehind,
            int minAhead,
            float poolLength)
        {
            int ahead = 0;
            int behind = 0;
            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                float startZ = segment.localPosition.z;
                float endZ = startZ + segmentLength;
                if (endZ <= playerWorldZ)
                {
                    behind++;
                }

                if (startZ >= playerWorldZ)
                {
                    ahead++;
                }
            }

            int guard = 0;
            while (ahead < minAhead && guard++ < segments.Count)
            {
                Transform oldest = FindRearmostSegment(segments);
                if (oldest == null)
                {
                    break;
                }

                float movedZ = oldest.localPosition.z + poolLength;
                HighwayRoadSegmentPool.SnapSegmentRoot(oldest, movedZ);
                ahead++;
                behind = Mathf.Max(0, behind - 1);
            }

            guard = 0;
            while (behind < minBehind && guard++ < segments.Count)
            {
                Transform newest = FindForemostSegment(segments);
                if (newest == null)
                {
                    break;
                }

                float movedZ = newest.localPosition.z - poolLength;
                HighwayRoadSegmentPool.SnapSegmentRoot(newest, movedZ);
                behind++;
                ahead = Mathf.Max(0, ahead - 1);
            }
        }

        private static Transform FindRearmostSegment(IList<Transform> segments)
        {
            Transform best = null;
            float bestZ = float.MaxValue;
            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                if (segment.localPosition.z < bestZ)
                {
                    bestZ = segment.localPosition.z;
                    best = segment;
                }
            }

            return best;
        }

        private static Transform FindForemostSegment(IList<Transform> segments)
        {
            Transform best = null;
            float bestZ = float.MinValue;
            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                if (segment.localPosition.z > bestZ)
                {
                    bestZ = segment.localPosition.z;
                    best = segment;
                }
            }

            return best;
        }
    }
}
