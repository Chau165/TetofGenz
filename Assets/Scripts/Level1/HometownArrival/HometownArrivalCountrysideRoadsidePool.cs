using System.Collections.Generic;
using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// Ring-buffer countryside belts that recycle in sync with the road deck — endless fences/trees/grass.
    /// </summary>
    public class HometownArrivalCountrysideRoadsidePool : MonoBehaviour
    {
        [SerializeField] private int segmentCount = 12;
        [SerializeField] private int minSegmentsBehind = 2;
        [SerializeField] private int minSegmentsAhead = 4;

        private readonly List<Transform> segments = new List<Transform>();
        private float playerWorldZ;
        private bool ringInitialized;
        private Material grassMaterial;

        public void RebuildContent()
        {
            grassMaterial = HometownArrivalResourceCache.LoadMaterial("Grass_Ground.mat")
                ?? HometownArrivalResourceCache.LoadMaterial("Highway_Grass_NSK2.mat");
            HometownArrivalCountrysidePropFactory.EnsureCatalogLoaded();
            PurgeSegments();
            ringInitialized = false;
            BuildSegments();
            SetScrollOffset(playerWorldZ);
        }

        public void SetScrollOffset(float vehicleWorldZ)
        {
            playerWorldZ = vehicleWorldZ;
            EnsurePool();
            HighwayScrollLayout.RecycleRoadSegments(
                segments,
                playerWorldZ,
                HighwayRoadLayout.SegmentLength,
                minSegmentsBehind,
                minSegmentsAhead);

            Transform approachRoot = transform.parent;
            if (approachRoot != null)
            {
                HometownArrivalBranchRoadJunction.MaintainCountrysideGap(approachRoot);
            }
        }

        private void EnsurePool()
        {
            if (segments.Count == 0)
            {
                BuildSegments();
            }

            if (!ringInitialized)
            {
                LayoutInitialRing(playerWorldZ);
                ringInitialized = true;
            }
        }

        private void LayoutInitialRing(float anchorZ)
        {
            float segmentLength = HighwayRoadLayout.SegmentLength;
            float startZ = Mathf.Floor(
                (anchorZ - minSegmentsBehind * segmentLength) / segmentLength) * segmentLength;

            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    segment = CreateSegment(i);
                    segments[i] = segment;
                }

                HighwayRoadSegmentPool.SnapSegmentRoot(segment, startZ + i * segmentLength);
            }
        }

        private void BuildSegments()
        {
            PurgeSegments();
            for (int i = 0; i < segmentCount; i++)
            {
                segments.Add(CreateSegment(i));
            }
        }

        private Transform CreateSegment(int segmentIndex)
        {
            GameObject root = new GameObject("CountrysideSeg_" + segmentIndex.ToString("00"));
            root.transform.SetParent(transform, false);

            GameObject left = new GameObject("LeftBelt");
            left.transform.SetParent(root.transform, false);
            GameObject right = new GameObject("RightBelt");
            right.transform.SetParent(root.transform, false);

            float segmentLength = HighwayRoadLayout.SegmentLength;
            int layoutSeed = segmentIndex * 9973 + 41;

            // Skip grass slabs on the branch-side belt — their segment seams read as white lines across the turn-off.
            float branchSide = HometownArrivalBranchRoadJunction.CountrysideBeltSideSign;
            if (branchSide >= 0f)
            {
                HometownArrivalCountrysidePropFactory.BuildGrassSlab(left.transform, -1f, segmentLength, grassMaterial);
            }

            if (branchSide <= 0f)
            {
                HometownArrivalCountrysidePropFactory.BuildGrassSlab(right.transform, 1f, segmentLength, grassMaterial);
            }
            HometownArrivalCountrysidePropFactory.SpawnFenceRow(
                left.transform, -1f, 0f, segmentLength, HometownArrivalCountrysideLayout.FenceSpacing);
            HometownArrivalCountrysidePropFactory.SpawnFenceRow(
                right.transform, 1f, 0f, segmentLength, HometownArrivalCountrysideLayout.FenceSpacing);
            HometownArrivalCountrysidePropFactory.SpawnBushesForSegment(left.transform, -1f, segmentLength, layoutSeed);
            HometownArrivalCountrysidePropFactory.SpawnBushesForSegment(right.transform, 1f, segmentLength, layoutSeed + 1);
            HometownArrivalCountrysidePropFactory.SpawnForestForSegment(left.transform, -1f, segmentLength, layoutSeed);
            HometownArrivalCountrysidePropFactory.SpawnForestForSegment(right.transform, 1f, segmentLength, layoutSeed + 1);

            return root.transform;
        }

        private void PurgeSegments()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }

            segments.Clear();
        }
    }
}
