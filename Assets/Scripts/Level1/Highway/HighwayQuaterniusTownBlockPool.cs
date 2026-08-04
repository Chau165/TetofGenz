using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Ring-buffer pool of Quaternius town blocks synced with highway road segments.</summary>
    [DefaultExecutionOrder(-80)]
    public class HighwayQuaterniusTownBlockPool : MonoBehaviour
    {
        [SerializeField] private int blockCount = 10;
        [SerializeField] private int minBlocksAhead = 4;
        [SerializeField] private int minBlocksBehind = 2;
        [SerializeField] private QuaterniusDowntownPrefabLibrary library;

        private const string LibraryPath = QuaterniusDowntownAssets.LibraryPath;
        private const string LayoutMarkerPrefix = "QuaterniusTown_v";

        private readonly List<Transform> leftBlocks = new List<Transform>();
        private readonly List<Transform> rightBlocks = new List<Transform>();
        private float playerWorldZ;
        private bool ringInitialized;

        public bool HasDeployedBlocks => leftBlocks.Count > 0 && rightBlocks.Count > 0;

        public static bool ShouldUseForBuildings()
        {
            HighwayQuaterniusTownBlockPool pool = FindAnyObjectByType<HighwayQuaterniusTownBlockPool>();
            return pool != null && pool.isActiveAndEnabled;
        }

        /// <summary>Creates the pool at runtime when the scene was not patched via the editor menu.</summary>
        public static HighwayQuaterniusTownBlockPool EnsureBootstrap(Transform parent)
        {
            HighwayQuaterniusTownBlockPool pool = FindAnyObjectByType<HighwayQuaterniusTownBlockPool>();
            if (pool == null)
            {
                GameObject poolGo = new GameObject("HighwayQuaterniusTownBlockPool");
                if (parent != null)
                {
                    poolGo.transform.SetParent(parent, false);
                }

                pool = poolGo.AddComponent<HighwayQuaterniusTownBlockPool>();
            }

            pool.EnsureLibrary();
            QuaterniusCompleteBuildingCatalog.ClearCache();
            if (!pool.HasDeployedBlocks)
            {
                pool.RebuildPool();
            }

            return pool;
        }

        private void Awake()
        {
            EnsureLibrary();
            CollectBlocksFromHierarchy();

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

            if (!ringInitialized || leftBlocks.Count == 0)
            {
                LayoutInitialRing(scrollOffset);
                ringInitialized = true;
            }

            RecycleSide(leftBlocks);
            RecycleSide(rightBlocks);
        }

        public void RebuildPool()
        {
            EnsureLibrary();
            QuaterniusCompleteBuildingCatalog.ClearCache();
            PurgeBlocks();
            leftBlocks.Clear();
            rightBlocks.Clear();
            ringInitialized = false;

            GameObject[] catalog = QuaterniusCompleteBuildingCatalog.Resolve(library);
            if (catalog.Length == 0)
            {
                Debug.LogWarning(
                    "[HighwayQuaterniusTownBlockPool] No validated complete buildings. "
                    + "Run Tools/Cho Tet/Build Quaternius Downtown Kit.");
                return;
            }

            Transform leftRoot = EnsureSideRoot("LeftQuaterniusBlocks");
            Transform rightRoot = EnsureSideRoot("RightQuaterniusBlocks");
            float blockLength = HighwayRoadLayout.SegmentLength;

            for (int i = 0; i < blockCount; i++)
            {
                int seed = i * 67 + 13;
                leftBlocks.Add(CreateBlock(leftRoot, true, seed, i, blockLength));
                rightBlocks.Add(CreateBlock(rightRoot, false, seed + 29, i, blockLength));
            }

            LayoutInitialRing(playerWorldZ);
            ringInitialized = true;
            EnsureLayoutMarker();
        }

        private Transform CreateBlock(Transform parent, bool leftSide, int seed, int index, float blockLength)
        {
            GameObject blockInstance = QuaterniusTownBlockAssembler.BuildBlock(
                library,
                leftSide,
                seed,
                blockLength);
            if (blockInstance == null)
            {
                blockInstance = new GameObject((leftSide ? "EmptyLeft" : "EmptyRight") + "_" + index);
            }

            blockInstance.transform.SetParent(parent, false);
            blockInstance.name = (leftSide ? "QuaterniusBlock_L" : "QuaterniusBlock_R") + "_" + index;
            return blockInstance.transform;
        }

        private void RecycleSide(List<Transform> blocks)
        {
            HighwayScrollLayout.RecycleRoadSegments(
                blocks,
                playerWorldZ,
                HighwayRoadLayout.SegmentLength,
                minBlocksBehind,
                minBlocksAhead);
        }

        private void LayoutInitialRing(float anchorZ)
        {
            float blockLength = HighwayRoadLayout.SegmentLength;
            float startZ = Mathf.Floor((anchorZ - minBlocksBehind * blockLength) / blockLength) * blockLength;

            for (int i = 0; i < leftBlocks.Count; i++)
            {
                SnapBlock(leftBlocks[i], startZ + i * blockLength);
                SnapBlock(rightBlocks[i], startZ + i * blockLength);
            }
        }

        private static void SnapBlock(Transform block, float worldZ)
        {
            if (block == null)
            {
                return;
            }

            block.localPosition = new Vector3(0f, 0f, worldZ);
            block.localRotation = Quaternion.identity;
            block.localScale = Vector3.one;
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
            if (leftBlocks.Count != blockCount || rightBlocks.Count != blockCount)
            {
                return true;
            }

            return transform.Find(LayoutMarkerPrefix + QuaterniusDowntownAssets.TownBlockContentVersion) == null;
        }

        private void CollectBlocksFromHierarchy()
        {
            leftBlocks.Clear();
            rightBlocks.Clear();
            Transform leftRoot = transform.Find("LeftQuaterniusBlocks");
            Transform rightRoot = transform.Find("RightQuaterniusBlocks");
            CollectChildren(leftRoot, leftBlocks);
            CollectChildren(rightRoot, rightBlocks);
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

        private void PurgeBlocks()
        {
            PurgeSide("LeftQuaterniusBlocks");
            PurgeSide("RightQuaterniusBlocks");
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
            GameObject marker = new GameObject(LayoutMarkerPrefix + QuaterniusDowntownAssets.TownBlockContentVersion);
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
