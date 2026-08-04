using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Ring-buffer pool of Quaternius sidewalk segments synced with highway road tiles.</summary>
    [DefaultExecutionOrder(-85)]
    public class HighwayQuaterniusSidewalkPool : MonoBehaviour
    {
        [SerializeField] private int segmentCount = 12;
        [SerializeField] private int minSegmentsAhead = 4;
        [SerializeField] private int minSegmentsBehind = 2;
        [SerializeField] private QuaterniusDowntownPrefabLibrary library;

        private const string LibraryPath = QuaterniusDowntownAssets.LibraryPath;
        private const string LayoutMarkerPrefix = "QuaterniusSidewalk_v";

        private readonly List<Transform> leftSegments = new List<Transform>();
        private readonly List<Transform> rightSegments = new List<Transform>();
        private float playerWorldZ;
        private bool ringInitialized;

        public static HighwayQuaterniusSidewalkPool EnsureBootstrap(Transform parent)
        {
            HighwayQuaterniusSidewalkPool pool = FindAnyObjectByType<HighwayQuaterniusSidewalkPool>();
            if (pool == null)
            {
                GameObject poolGo = new GameObject("HighwayQuaterniusSidewalkPool");
                if (parent != null)
                {
                    poolGo.transform.SetParent(parent, false);
                }

                pool = poolGo.AddComponent<HighwayQuaterniusSidewalkPool>();
            }

            pool.EnsureLibrary();
            Transform leftRoot = pool.transform.Find("LeftQuaterniusSidewalks");
            if (leftRoot == null || leftRoot.childCount == 0)
            {
                pool.RebuildPool();
            }

            return pool;
        }

        private void Awake()
        {
            EnsureLibrary();
            CollectSegmentsFromHierarchy();

            if (NeedsRebuild())
            {
                RebuildPool();
            }
            else
            {
                LayoutInitialRing(playerWorldZ);
            }
        }

        public void Initialize(QuaterniusDowntownPrefabLibrary downtownLibrary)
        {
            if (downtownLibrary != null)
            {
                library = downtownLibrary;
            }

            EnsureLibrary();
            RebuildPool();
        }

        public void SetScrollOffset(float scrollOffset)
        {
            playerWorldZ = scrollOffset;

            if (!ringInitialized || leftSegments.Count == 0)
            {
                LayoutInitialRing(scrollOffset);
                ringInitialized = true;
            }

            RecycleSide(leftSegments);
            RecycleSide(rightSegments);
        }

        public void RebuildPool()
        {
            EnsureLibrary();
            PurgeSegments();
            leftSegments.Clear();
            rightSegments.Clear();
            ringInitialized = false;

            if (library == null || QuaterniusSidewalkAssembler.ResolvePavementMaterial(library) == null)
            {
                Debug.LogWarning(
                    "[HighwayQuaterniusSidewalkPool] No Quaternius pavement material. "
                    + "Run Tools/Cho Tet/Build Quaternius Downtown Kit.");
                return;
            }

            Transform leftRoot = EnsureSideRoot("LeftQuaterniusSidewalks");
            Transform rightRoot = EnsureSideRoot("RightQuaterniusSidewalks");
            float segmentLength = HighwayRoadLayout.SegmentLength;

            for (int i = 0; i < segmentCount; i++)
            {
                int seed = i * 53 + 7;
                leftSegments.Add(CreateSegment(leftRoot, true, seed, i, segmentLength));
                rightSegments.Add(CreateSegment(rightRoot, false, seed + 31, i, segmentLength));
            }

            LayoutInitialRing(playerWorldZ);
            ringInitialized = true;
            EnsureLayoutMarker();
        }

        private Transform CreateSegment(Transform parent, bool leftSide, int seed, int index, float segmentLength)
        {
            GameObject segment = QuaterniusSidewalkAssembler.BuildSegment(library, leftSide, seed, segmentLength);
            if (segment == null)
            {
                segment = new GameObject((leftSide ? "EmptySidewalk_L" : "EmptySidewalk_R") + "_" + index);
            }

            segment.transform.SetParent(parent, false);
            segment.name = (leftSide ? "SidewalkSegment_L" : "SidewalkSegment_R") + "_" + index;
            return segment.transform;
        }

        private void RecycleSide(List<Transform> segments)
        {
            HighwayScrollLayout.RecycleRoadSegments(
                segments,
                playerWorldZ,
                HighwayRoadLayout.SegmentLength,
                minSegmentsBehind,
                minSegmentsAhead);
        }

        private void LayoutInitialRing(float anchorZ)
        {
            float segmentLength = HighwayRoadLayout.SegmentLength;
            float startZ = Mathf.Floor((anchorZ - minSegmentsBehind * segmentLength) / segmentLength) * segmentLength;

            for (int i = 0; i < leftSegments.Count; i++)
            {
                SnapSegment(leftSegments[i], startZ + i * segmentLength);
                SnapSegment(rightSegments[i], startZ + i * segmentLength);
            }
        }

        private static void SnapSegment(Transform segment, float worldZ)
        {
            if (segment == null)
            {
                return;
            }

            segment.localPosition = new Vector3(0f, 0f, worldZ);
            segment.localRotation = Quaternion.identity;
            segment.localScale = Vector3.one;
            QuaterniusSidewalkAssembler.SyncSegmentWorldUvs(
                segment.gameObject,
                worldZ,
                HighwayRoadLayout.SegmentLength);
        }

        private void EnsureLibrary()
        {
#if UNITY_EDITOR
            if (library == null)
            {
                library = UnityEditor.AssetDatabase.LoadAssetAtPath<QuaterniusDowntownPrefabLibrary>(LibraryPath);
            }
#endif
        }

        private bool NeedsRebuild()
        {
            if (leftSegments.Count != segmentCount || rightSegments.Count != segmentCount)
            {
                return true;
            }

            return transform.Find(LayoutMarkerPrefix + QuaterniusDowntownAssets.SidewalkContentVersion) == null;
        }

        private void CollectSegmentsFromHierarchy()
        {
            leftSegments.Clear();
            rightSegments.Clear();
            CollectChildren(transform.Find("LeftQuaterniusSidewalks"), leftSegments);
            CollectChildren(transform.Find("RightQuaterniusSidewalks"), rightSegments);
        }

        private static void CollectChildren(Transform root, List<Transform> output)
        {
            if (root == null)
            {
                return;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child != null)
                {
                    output.Add(child);
                }
            }
        }

        private void PurgeSegments()
        {
            PurgeSide("LeftQuaterniusSidewalks");
            PurgeSide("RightQuaterniusSidewalks");
            ClearLayoutMarkers();
        }

        private void PurgeSide(string rootName)
        {
            Transform root = transform.Find(rootName);
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                HighwayPropUtility.SafeDestroy(root.GetChild(i).gameObject);
            }
        }

        private Transform EnsureSideRoot(string name)
        {
            Transform existing = transform.Find(name);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            return root.transform;
        }

        private void EnsureLayoutMarker()
        {
            ClearLayoutMarkers();
            GameObject marker = new GameObject(LayoutMarkerPrefix + QuaterniusDowntownAssets.SidewalkContentVersion);
            marker.transform.SetParent(transform, false);
            marker.hideFlags = HideFlags.HideInHierarchy;
        }

        private void ClearLayoutMarkers()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null && child.name.StartsWith(LayoutMarkerPrefix))
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }
        }
    }
}
