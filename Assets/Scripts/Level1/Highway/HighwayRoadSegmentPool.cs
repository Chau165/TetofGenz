using System.Collections.Generic;

using UnityEngine;



namespace ChoTet.Level1.Highway

{

    /// <summary>3-lane road deck with ring-buffer recycling — asphalt always surrounds the player.</summary>

    public class HighwayRoadSegmentPool : MonoBehaviour

    {

        [SerializeField] private int segmentCount = 12;

        [SerializeField] private int minSegmentsAhead = 4;

        [SerializeField] private int minSegmentsBehind = 2;

        [SerializeField] private Material runtimeRoadSurface;

        [SerializeField] private Material runtimeLaneMark;

        [SerializeField] private Material runtimeShoulder;

        [SerializeField] private Material runtimeEdgeMark;

        [SerializeField] private Material runtimeGuardRail;

        [SerializeField] private Material runtimeTireWear;

        [SerializeField] private Material runtimeBridgeConcrete;

        [SerializeField] private Material runtimeBridgeUnderside;

        private Material laneMarkMaterial;

        private Material shoulderMaterial;

        private Material edgeMarkMaterial;

        private Material guardRailMaterial;

        private Material tireWearMaterial;

        private Material bridgeConcreteMaterial;

        private Material bridgeUndersideMaterial;



        private Material roadSurfaceMaterial;

        private readonly List<Transform> segments = new List<Transform>();

        private float playerWorldZ;

        private bool ringInitialized;



        public float SegmentLength => HighwayRoadLayout.SegmentLength;



        private void Awake()

        {

            transform.localPosition = Vector3.zero;

            transform.localRotation = Quaternion.identity;

            transform.localScale = Vector3.one;



            EnsureMaterialsAssigned();

            RestoreMaterialsFromSerializedFields();

            CollectSegmentsFromHierarchy();



            if (segments.Count != segmentCount || NeedsContentRebuild())

            {

                PurgeAllSegmentObjects();

                BuildSegments();

            }

            else

            {

                EnsureBuilt();

            }



            ValidateRoadIntegrity(0f);

        }



        private void LateUpdate()

        {

            if (!Application.isPlaying || segments.Count == 0)

            {

                return;

            }



            ValidateRoadIntegrity(playerWorldZ);

        }



        /// <summary>Rebuild road deck in the open scene (editor patch / menu).</summary>

        public void RebuildRoadContent()

        {

            EnsureMaterialsAssigned();

            RestoreMaterialsFromSerializedFields();

            PurgeAllSegmentObjects();

            ringInitialized = false;

            BuildSegments();

            ValidateRoadIntegrity(playerWorldZ);

        }



        public void Initialize(

            GameObject roadPrefabAsset,

            Material laneMark,

            Material medianMark,

            Material roadSurface = null,

            Material shoulder = null)

        {

            runtimeRoadSurface = roadSurface ?? ExtractRoadMaterial(roadPrefabAsset);

            runtimeLaneMark = laneMark;

            runtimeShoulder = shoulder ?? runtimeRoadSurface;

            runtimeEdgeMark = medianMark ?? laneMark;

            RestoreMaterialsFromSerializedFields();

            ringInitialized = false;

            BuildSegments();

        }



        public void SetScrollOffset(float distance)

        {

            playerWorldZ = distance;

            EnsureRoadPool();

            HighwayScrollLayout.RecycleRoadSegments(

                segments,

                playerWorldZ,

                HighwayRoadLayout.SegmentLength,

                minSegmentsBehind,

                minSegmentsAhead);

            ValidateRoadIntegrity(playerWorldZ);

        }



        public void SetShouldersVisible(bool visible)

        {

            for (int i = 0; i < segments.Count; i++)

            {

                SetSegmentShouldersVisible(segments[i], visible);

            }



            for (int i = 0; i < transform.childCount; i++)

            {

                Transform child = transform.GetChild(i);

                if (child != null && child.name.StartsWith("RoadSegment_"))

                {

                    SetSegmentShouldersVisible(child, visible);

                }

            }

        }



        private static void SetSegmentShouldersVisible(Transform segment, bool visible)

        {

            if (segment == null)

            {

                return;

            }



            Transform shoulders = segment.Find("Shoulders");

            if (shoulders != null)

            {

                shoulders.gameObject.SetActive(visible);

            }

        }



        public static void SnapSegmentRoot(Transform segment, float worldZ)

        {

            if (segment == null)

            {

                return;

            }



            segment.localPosition = new Vector3(0f, HighwayLaneSettings.RoadSurfaceY, worldZ);

            segment.localRotation = Quaternion.identity;

            segment.localScale = Vector3.one;

        }



        private void EnsureRoadPool()

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



                SnapSegmentRoot(segment, startZ + i * segmentLength);

            }

        }



        private void ValidateRoadIntegrity(float playerZ)

        {

            transform.localPosition = Vector3.zero;

            transform.localRotation = Quaternion.identity;

            transform.localScale = Vector3.one;



            if (segments.Count < segmentCount)

            {

                int existing = segments.Count;

                for (int i = existing; i < segmentCount; i++)

                {

                    segments.Add(CreateSegment(i));

                }



                ringInitialized = false;

                LayoutInitialRing(playerZ);

                ringInitialized = true;

            }



            float segmentLength = HighwayRoadLayout.SegmentLength;

            for (int i = segments.Count - 1; i >= 0; i--)

            {

                if (segments[i] == null)

                {

                    Transform replacement = CreateSegment(i);

                    float fallbackZ = playerZ - minSegmentsBehind * segmentLength + i * segmentLength;

                    SnapSegmentRoot(replacement, fallbackZ);

                    segments[i] = replacement;

                }

            }



            for (int i = 0; i < segments.Count; i++)

            {

                Transform segment = segments[i];

                if (segment == null)

                {

                    continue;

                }



                Vector3 pos = segment.localPosition;

                if (Mathf.Abs(pos.x) > 0.001f

                    || segment.localRotation != Quaternion.identity

                    || segment.localScale != Vector3.one)

                {

                    SnapSegmentRoot(segment, pos.z);

                }



                if (segment.Find("RoadDeck") == null)

                {

                    SafeDestroy(segment.gameObject);

                    segment = CreateSegment(i);

                    segments[i] = segment;

                    SnapSegmentRoot(segment, playerZ - minSegmentsBehind * segmentLength + i * segmentLength);

                }



                ApplyMaterialsToSegment(segment);

            }



            HighwayScrollLayout.RecycleRoadSegments(

                segments,

                playerZ,

                segmentLength,

                minSegmentsBehind,

                minSegmentsAhead);

        }



        private void EnsureMaterialsAssigned()

        {

            HighwayRoadMaterialDefaults.ApplyIfMissing(

                ref runtimeRoadSurface,

                ref runtimeLaneMark,

                ref runtimeShoulder,

                ref runtimeEdgeMark);

        }



        private void RestoreMaterialsFromSerializedFields()

        {

            roadSurfaceMaterial = ResolveRoadSurfaceMaterial(runtimeRoadSurface);

            laneMarkMaterial = runtimeLaneMark;

            shoulderMaterial = runtimeShoulder ?? runtimeRoadSurface;

            edgeMarkMaterial = runtimeEdgeMark ?? runtimeLaneMark;

            guardRailMaterial = runtimeGuardRail ?? runtimeEdgeMark ?? runtimeLaneMark;

            tireWearMaterial = runtimeTireWear ?? runtimeRoadSurface;

            bridgeConcreteMaterial = runtimeBridgeConcrete ?? runtimeShoulder ?? runtimeRoadSurface;

            bridgeUndersideMaterial = runtimeBridgeUnderside ?? bridgeConcreteMaterial;

        }



        private static Material ResolveRoadSurfaceMaterial(Material source)

        {

            if (source != null && source.name.Contains("Highway_Asphalt"))

            {

                return source;

            }



            return CreateFlatRoadMaterial(source);

        }



        private static Material CreateFlatRoadMaterial(Material source)

        {

            Material instance = source != null

                ? new Material(source)

                : new Material(Shader.Find("Universal Render Pipeline/Lit"));



            Color asphalt = new Color(0.32f, 0.32f, 0.34f);

            if (instance.HasProperty("_BaseColor"))

            {

                instance.SetColor("_BaseColor", asphalt);

            }



            if (instance.HasProperty("_Color"))

            {

                instance.SetColor("_Color", asphalt);

            }



            if (instance.HasProperty("_BaseMap"))

            {

                instance.SetTexture("_BaseMap", null);

            }



            if (instance.HasProperty("_MainTex"))

            {

                instance.SetTexture("_MainTex", null);

            }



            return instance;

        }



        private void ApplyMaterialsToSegment(Transform segment)

        {

            if (segment == null || roadSurfaceMaterial == null)

            {

                return;

            }



            Transform deck = segment.Find("RoadDeck");

            if (deck != null)

            {

                MeshRenderer renderer = deck.GetComponent<MeshRenderer>();

                if (renderer != null)

                {

                    renderer.sharedMaterial = roadSurfaceMaterial;

                    renderer.enabled = true;

                }

            }



            Transform shoulders = segment.Find("Shoulders");

            if (shoulders != null && shoulderMaterial != null)

            {

                ApplyMaterialToChildren(shoulders, shoulderMaterial);

            }



            Transform markings = segment.Find("RoadMarkings");

            if (markings != null && laneMarkMaterial != null)

            {

                ApplyMaterialToChildren(markings, laneMarkMaterial);

            }



            Transform guards = segment.Find("GuardEdges");

            if (guards != null)

            {

                Material guardMat = shoulderMaterial != null ? shoulderMaterial : edgeMarkMaterial;

                if (guardMat != null)

                {

                    ApplyMaterialToChildren(guards, guardMat);

                }

            }

        }



        private static void ApplyMaterialToChildren(Transform root, Material material)

        {

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)

            {

                Renderer renderer = renderers[i];

                if (renderer == null)

                {

                    continue;

                }



                renderer.sharedMaterial = material;

                renderer.enabled = true;

            }

        }



        private void EnsureBuilt()

        {

            if (segments.Count > 0)

            {

                return;

            }



            if (roadSurfaceMaterial == null)

            {

                return;

            }



            BuildSegments();

        }



        private static Material ExtractRoadMaterial(GameObject roadPrefabAsset)

        {

            if (roadPrefabAsset == null)

            {

                return null;

            }



            Renderer renderer = roadPrefabAsset.GetComponentInChildren<Renderer>(true);

            return renderer != null ? renderer.sharedMaterial : null;

        }



        private bool NeedsContentRebuild()

        {

            if (segments.Count == 0)

            {

                return false;

            }



            for (int i = 0; i < segments.Count; i++)

            {

                Transform segment = segments[i];

                if (segment == null)

                {

                    return true;

                }



                if (segment.Find("RoadTiles") != null)

                {

                    return true;

                }



                if (segment.Find("RoadDeck") == null)

                {

                    return true;

                }



                if (segment.Find("GuardEdges") == null)

                {

                    return true;

                }



                if (segment.Find("RoadMarkings") == null)

                {

                    return true;

                }



                Transform marker = segment.Find(HighwayRoadLayout.RoadLayoutMarkerPrefix + HighwayRoadLayout.RoadContentVersion);

                if (marker == null)

                {

                    return true;

                }

            }



            return false;

        }



        private void CollectSegmentsFromHierarchy()

        {

            segments.Clear();

            for (int i = 0; i < transform.childCount; i++)

            {

                Transform child = transform.GetChild(i);

                if (child != null && child.name.StartsWith("RoadSegment_"))

                {

                    segments.Add(child);

                }

            }

        }



        private void BuildSegments()

        {

            PurgeAllSegmentObjects();

            ringInitialized = false;



            for (int i = 0; i < segmentCount; i++)

            {

                segments.Add(CreateSegment(i));

            }



            LayoutInitialRing(playerWorldZ);

            ringInitialized = true;

        }



        private void PurgeAllSegmentObjects()

        {

            segments.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)

            {

                Transform child = transform.GetChild(i);

                if (child != null && child.name.StartsWith("RoadSegment_"))

                {

                    SafeDestroy(child.gameObject);

                }

            }

        }



        private Transform CreateSegment(int index)

        {

            GameObject root = new GameObject("RoadSegment_" + index);

            root.transform.SetParent(transform, false);

            SnapSegmentRoot(root.transform, 0f);



            if (roadSurfaceMaterial == null)

            {

                Debug.LogWarning("[HighwayRoadSegmentPool] Road surface material is not assigned.");

                return root.transform;

            }



            float segmentLength = HighwayRoadLayout.SegmentLength;



            GameObject deck = new GameObject("RoadDeck");

            deck.transform.SetParent(root.transform, false);

            deck.transform.localRotation = Quaternion.identity;

            BuildRoadDeck(deck, segmentLength);



            GameObject shoulders = new GameObject("Shoulders");

            shoulders.transform.SetParent(root.transform, false);

            shoulders.transform.localRotation = Quaternion.identity;

            BuildShoulders(shoulders.transform, segmentLength);



            GameObject marksRoot = new GameObject("RoadMarkings");

            marksRoot.transform.SetParent(root.transform, false);

            marksRoot.transform.localRotation = Quaternion.identity;

            BuildLaneMarkings(marksRoot.transform, segmentLength);



            GameObject layoutMarker = new GameObject(HighwayRoadLayout.RoadLayoutMarkerPrefix + HighwayRoadLayout.RoadContentVersion);

            layoutMarker.transform.SetParent(root.transform, false);

            layoutMarker.hideFlags = HideFlags.HideInHierarchy;



            GameObject guardRoot = new GameObject("GuardEdges");

            guardRoot.transform.SetParent(root.transform, false);

            guardRoot.transform.localRotation = Quaternion.identity;

            HighwayRoadDeckDetails.BuildAll(
                root.transform,
                segmentLength,
                guardRailMaterial,
                laneMarkMaterial,
                tireWearMaterial);

            HighwayElevatedStructureBuilder.Build(
                root.transform,
                segmentLength,
                bridgeConcreteMaterial,
                bridgeUndersideMaterial);

            ApplyMaterialsToSegment(root.transform);

            return root.transform;

        }



        private void BuildRoadDeck(GameObject deckObject, float segmentLength)

        {

            float halfWidth = HighwayPlayerLaneLayout.PlayableRoadHalfWidth;



            MeshFilter filter = deckObject.AddComponent<MeshFilter>();

            MeshRenderer renderer = deckObject.AddComponent<MeshRenderer>();

            filter.sharedMesh = BuildDeckMesh(segmentLength, halfWidth);

            renderer.sharedMaterial = roadSurfaceMaterial;

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            renderer.receiveShadows = true;

            deckObject.transform.localPosition = new Vector3(0f, HighwayLaneSettings.RoadDeckY, 0f);

            deckObject.transform.localRotation = Quaternion.identity;

            deckObject.transform.localScale = Vector3.one;

        }



        private static Mesh BuildDeckMesh(float length, float halfWidth)

        {

            float overlap = HighwayRoadLayout.SegmentOverlapZ;

            float meshLength = length + overlap;

            float vTiles = meshLength / HighwayRoadLayout.TileLength;

            float uTiles = halfWidth * 2f / HighwayRoadLayout.TileLength;

            var vertices = new Vector3[]

            {

                new Vector3(-halfWidth, 0f, -overlap * 0.5f),

                new Vector3(halfWidth, 0f, -overlap * 0.5f),

                new Vector3(-halfWidth, 0f, meshLength - overlap * 0.5f),

                new Vector3(halfWidth, 0f, meshLength - overlap * 0.5f)

            };

            var uvs = new Vector2[]

            {

                new Vector2(0f, 0f),

                new Vector2(uTiles, 0f),

                new Vector2(0f, vTiles),

                new Vector2(uTiles, vTiles)

            };

            var triangles = new[] { 0, 2, 1, 1, 2, 3 };



            Mesh mesh = new Mesh { name = "RoadDeck" };

            mesh.vertices = vertices;

            mesh.uv = uvs;

            mesh.triangles = triangles;

            mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            return mesh;

        }



        private void BuildShoulders(Transform parent, float segmentLength)

        {

            float inner = HighwayPlayerLaneLayout.PlayableRoadHalfWidth;

            float outer = HighwayRoadLayout.ShoulderOuterX;

            float shoulderWidth = outer - inner;

            float centerX = inner + shoulderWidth * 0.5f;

            float centerZ = segmentLength * 0.5f;

            float shoulderHeight = 0.08f;

            float shoulderCenterY = HighwayLaneSettings.ShoulderSurfaceY + shoulderHeight * 0.5f;

            float shoulderLength = segmentLength + HighwayRoadLayout.SegmentOverlapZ;



            BuildSlab(parent, "Shoulder_L", shoulderMaterial,

                new Vector3(-centerX, shoulderCenterY, centerZ),

                new Vector3(shoulderWidth, shoulderHeight, shoulderLength));



            BuildSlab(parent, "Shoulder_R", shoulderMaterial,

                new Vector3(centerX, shoulderCenterY, centerZ),

                new Vector3(shoulderWidth, shoulderHeight, shoulderLength));

        }



        private void BuildGuardEdges(Transform parent, float segmentLength)

        {

            float outer = HighwayRoadLayout.ShoulderOuterX;

            float centerZ = segmentLength * 0.5f;

            float guardY = HighwayLaneSettings.ShoulderSurfaceY + HighwayRoadLayout.GuardEdgeHeight * 0.5f;

            Material guardMat = shoulderMaterial != null ? shoulderMaterial : edgeMarkMaterial;

            float guardLength = segmentLength + HighwayRoadLayout.SegmentOverlapZ;



            BuildSlab(parent, "GuardEdge_L", guardMat,

                new Vector3(-outer, guardY, centerZ),

                new Vector3(HighwayRoadLayout.GuardEdgeWidth, HighwayRoadLayout.GuardEdgeHeight, guardLength));



            BuildSlab(parent, "GuardEdge_R", guardMat,

                new Vector3(outer, guardY, centerZ),

                new Vector3(HighwayRoadLayout.GuardEdgeWidth, HighwayRoadLayout.GuardEdgeHeight, guardLength));

        }



        private void BuildLaneMarkings(Transform parent, float segmentLength)

        {

            float halfWidth = HighwayPlayerLaneLayout.PlayableRoadHalfWidth;

            float edgeWidth = HighwayRoadLayout.EdgeLineWidth;

            float edgeY = HighwayRoadLayout.MarkingY;

            float centerZ = segmentLength * 0.5f;



            BuildSlab(parent, "EdgeLine_L", laneMarkMaterial,

                new Vector3(-halfWidth + edgeWidth * 0.5f, edgeY, centerZ),

                new Vector3(edgeWidth, HighwayRoadLayout.MarkThickness, segmentLength));



            BuildSlab(parent, "EdgeLine_R", laneMarkMaterial,

                new Vector3(halfWidth - edgeWidth * 0.5f, edgeY, centerZ),

                new Vector3(edgeWidth, HighwayRoadLayout.MarkThickness, segmentLength));



            float dividerWidth = HighwayRoadLayout.DividerLineWidth;



            for (float z = HighwayRoadLayout.DashLength * 0.5f;

                 z < segmentLength;

                 z += HighwayRoadLayout.DashCycle)

            {

                for (int i = 0; i < HighwayRoadLayout.PlayerLaneDividers.Length; i++)

                {

                    AddMarkQuad(

                        parent,

                        laneMarkMaterial,

                        new Vector3(HighwayRoadLayout.PlayerLaneDividers[i], HighwayRoadLayout.MarkingY, z),

                        new Vector3(dividerWidth, HighwayRoadLayout.MarkThickness, HighwayRoadLayout.DashLength));

                }

            }

        }



        private static void BuildSlab(Transform parent, string name, Material mat, Vector3 localPos, Vector3 size)

        {

            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);

            slab.name = name;

            slab.transform.SetParent(parent, false);

            slab.transform.localPosition = localPos;

            slab.transform.localRotation = Quaternion.identity;

            slab.transform.localScale = size;

            HighwayPropUtility.SafeDestroy(slab.GetComponent<Collider>());

            if (mat != null)

            {

                var renderer = slab.GetComponent<Renderer>();

                renderer.sharedMaterial = mat;

                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                renderer.receiveShadows = false;

            }

        }



        private static void AddMarkQuad(Transform parent, Material mat, Vector3 localPos, Vector3 size)

        {

            if (mat == null)

            {

                return;

            }



            GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);

            mark.name = "LaneDivider";

            mark.transform.SetParent(parent, false);

            mark.transform.localPosition = localPos;

            mark.transform.localRotation = Quaternion.identity;

            mark.transform.localScale = size;

            HighwayPropUtility.SafeDestroy(mark.GetComponent<Collider>());

            var renderer = mark.GetComponent<Renderer>();

            renderer.sharedMaterial = mat;

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            renderer.receiveShadows = false;

        }



        private static void SafeDestroy(Object obj)

        {

            if (obj == null)

            {

                return;

            }



#if UNITY_EDITOR

            if (!Application.isPlaying)

            {

                DestroyImmediate(obj);

                return;

            }

#endif

            Destroy(obj);

        }

    }

}


