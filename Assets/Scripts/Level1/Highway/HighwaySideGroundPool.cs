using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChoTet.Level1.Highway
{
    /// <summary>Unified NSK2 grass ground with blade tufts on left/right belts only.</summary>
    public class HighwaySideGroundPool : MonoBehaviour
    {
        [SerializeField] private int segmentCount = 22;
        [FormerlySerializedAs("runtimeEarth")]
        [FormerlySerializedAs("runtimeGrassNear")]
        [SerializeField] private Material runtimeGrassGround;
        [SerializeField] private Material runtimeGrassBladeA;
        [SerializeField] private Material runtimeGrassBladeB;
        [SerializeField] private HighwayNaturePrefabLibrary prefabLibrary;

        private const string LibraryAssetPath = "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset";
        private const string LayoutMarkerName = "RoadsideLayout_v";
        private const float GroundY = -2.85f;
        private const float GroundThickness = 0.08f;
        private const float SegmentOverlapZ = 0.12f;

        private readonly List<Transform> leftSegments = new List<Transform>();
        private readonly List<Transform> rightSegments = new List<Transform>();
        private float scrollOffset;

        public HighwayNaturePrefabLibrary SharedPrefabLibrary => prefabLibrary;
        public Material SharedGrassBladeA => runtimeGrassBladeA;
        public Material SharedGrassBladeB => runtimeGrassBladeB;

        private void Awake()
        {
            EnsureMaterialsAssigned();
            EnsurePrefabLibrary();
            CollectSegmentsFromHierarchy();

            if (NeedsContentRebuild())
            {
                ClearSegments();
                BuildSegments();
            }
            else
            {
                RebuildAllGroundCovers();
            }

            RepositionSegments(leftSegments);
            RepositionSegments(rightSegments);
            ApplyMaterialsToAllSegments();
        }

        public void Initialize(
            Material grassGround,
            Material grassBladeA,
            Material grassBladeB = null,
            HighwayNaturePrefabLibrary library = null)
        {
            runtimeGrassGround = grassGround;
            runtimeGrassBladeA = grassBladeA;
            runtimeGrassBladeB = grassBladeB ?? grassBladeA;
            if (library != null)
            {
                prefabLibrary = library;
            }

            EnsureMaterialsAssigned();
            EnsurePrefabLibrary();
            BuildSegments();
            ApplyMaterialsToAllSegments();
        }

        public void RebuildSideGroundContent()
        {
            EnsureMaterialsAssigned();
            EnsurePrefabLibrary();
            ClearAllSegmentChildren();
            BuildSegments();
            RepositionSegments(leftSegments);
            RepositionSegments(rightSegments);
            ApplyMaterialsToAllSegments();
        }

        private void EnsureMaterialsAssigned()
        {
#if UNITY_EDITOR
            if (runtimeGrassGround == null || runtimeGrassGround.name.Contains("GroundEarth"))
            {
                runtimeGrassGround = HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassNearFile);
            }

            if (runtimeGrassBladeA == null)
            {
                runtimeGrassBladeA = HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassBlade01File);
            }

            if (runtimeGrassBladeB == null)
            {
                runtimeGrassBladeB = HighwayNatureMaterialDefaults.Load(HighwayNatureMaterialDefaults.GrassBlade02File);
            }
#endif
        }

        private void EnsurePrefabLibrary()
        {
#if UNITY_EDITOR
            if (prefabLibrary == null)
            {
                prefabLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(LibraryAssetPath);
            }
#endif
        }

        private bool NeedsContentRebuild()
        {
            if (leftSegments.Count == 0 && rightSegments.Count == 0)
            {
                return true;
            }

            Transform sample = leftSegments.Count > 0 ? leftSegments[0] : rightSegments[0];
            if (sample == null)
            {
                return true;
            }

            if (sample.Find("GrassGround") == null || sample.Find("GroundCover") == null)
            {
                return true;
            }

            if (sample.Find("Grass") != null || sample.Find("Sidewalk") != null)
            {
                return true;
            }

            Transform marker = transform.Find(LayoutMarkerName + HighwayRoadsideLayout.ContentVersion);
            return marker == null;
        }

        private void CollectSegmentsFromHierarchy()
        {
            leftSegments.Clear();
            rightSegments.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("LeftTerrain_"))
                {
                    leftSegments.Add(child);
                }
                else if (child.name.StartsWith("RightTerrain_"))
                {
                    rightSegments.Add(child);
                }
            }
        }

        public void SetScrollOffset(float distance)
        {
            scrollOffset = distance;
            RepositionSegments(leftSegments);
            RepositionSegments(rightSegments);
        }

        private void BuildSegments()
        {
            ClearSegments();
            ClearLayoutMarkers();

            for (int i = 0; i < segmentCount; i++)
            {
                leftSegments.Add(CreateSideSegment("LeftTerrain_" + i, true, i));
                rightSegments.Add(CreateSideSegment("RightTerrain_" + i, false, i));
            }

            EnsureLayoutMarker();
            RepositionSegments(leftSegments);
            RepositionSegments(rightSegments);
        }

        private void RebuildAllGroundCovers()
        {
            RebuildCoversOnList(leftSegments, true);
            RebuildCoversOnList(rightSegments, false);
            EnsureLayoutMarker();
        }

        private void RebuildCoversOnList(List<Transform> segments, bool leftSide)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                Transform cover = segment.Find("GroundCover");
                if (cover == null)
                {
                    continue;
                }

                int seed = i * 61 + (leftSide ? 13 : 29);
                HighwayNatureGroundCover.Build(
                    cover,
                    leftSide,
                    i,
                    prefabLibrary,
                    new HighwayNatureGroundCover.GrassPalette
                    {
                        BladeA = runtimeGrassBladeA,
                        BladeB = runtimeGrassBladeB ?? runtimeGrassBladeA
                    },
                    seed);
            }
        }

        private void RepositionSegments(List<Transform> segments)
        {
            HighwayScrollLayout.PlaceSegments(
                segments,
                scrollOffset,
                HighwayRoadLayout.SegmentLength,
                0f);
        }

        private Transform CreateSideSegment(string name, bool leftSide, int segmentIndex)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.localRotation = Quaternion.identity;

            float sign = leftSide ? -1f : 1f;
            float segmentLength = HighwayRoadLayout.SegmentLength;
            float segmentCenterZ = segmentLength * 0.5f;
            float elevation = Mathf.Sin(segmentIndex * 1.7f) * 0.18f + Mathf.Sin(segmentIndex * 0.55f) * 0.12f;
            float innerX = HighwayRoadsideLayout.GrassGroundInnerX;
            float outerX = HighwayRoadsideLayout.GrassGroundOuterX;
            float width = outerX - innerX;
            float centerX = innerX + width * 0.5f;
            float slabLength = segmentLength + SegmentOverlapZ;

            BuildSlab(root.transform, "GrassGround", runtimeGrassGround,
                new Vector3(sign * centerX, GroundY + elevation, segmentCenterZ),
                new Vector3(width, GroundThickness, slabLength));

            GameObject coverRoot = new GameObject("GroundCover");
            coverRoot.transform.SetParent(root.transform, false);
            int seed = segmentIndex * 61 + (leftSide ? 13 : 29);
            HighwayNatureGroundCover.Build(
                coverRoot.transform,
                leftSide,
                segmentIndex,
                prefabLibrary,
                new HighwayNatureGroundCover.GrassPalette
                {
                    BladeA = runtimeGrassBladeA,
                    BladeB = runtimeGrassBladeB ?? runtimeGrassBladeA
                },
                seed);

            return root.transform;
        }

        private void ApplyMaterialsToAllSegments()
        {
            ApplyBeltMaterial(leftSegments, "GrassGround", runtimeGrassGround);
            ApplyBeltMaterial(rightSegments, "GrassGround", runtimeGrassGround);
        }

        private static void ApplyBeltMaterial(List<Transform> segments, string beltName, Material mat)
        {
            if (mat == null)
            {
                return;
            }

            for (int i = 0; i < segments.Count; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                {
                    continue;
                }

                Transform belt = segment.Find(beltName);
                if (belt == null)
                {
                    continue;
                }

                MeshRenderer renderer = belt.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = mat;
                }
            }
        }

        private static void BuildSlab(Transform parent, string slabName, Material mat, Vector3 localPos, Vector3 size)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = slabName;
            slab.transform.SetParent(parent, false);
            slab.transform.localPosition = localPos;
            slab.transform.localRotation = Quaternion.identity;
            slab.transform.localScale = size;
            HighwayPropUtility.SafeDestroy(slab.GetComponent<Collider>());
            if (mat != null)
            {
                slab.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        private void EnsureLayoutMarker()
        {
            ClearLayoutMarkers();
            GameObject marker = new GameObject(LayoutMarkerName + HighwayRoadsideLayout.ContentVersion);
            marker.transform.SetParent(transform, false);
            marker.hideFlags = HideFlags.HideInHierarchy;
        }

        private void ClearLayoutMarkers()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null && child.name.StartsWith(LayoutMarkerName))
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }
        }

        private void ClearSegments()
        {
            ClearList(leftSegments);
            ClearList(rightSegments);
        }

        private void ClearAllSegmentChildren()
        {
            ClearSegments();
            ClearLayoutMarkers();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }

            leftSegments.Clear();
            rightSegments.Clear();
        }

        private static void ClearList(List<Transform> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    HighwayPropUtility.SafeDestroy(list[i].gameObject);
                }
            }

            list.Clear();
        }
    }
}
