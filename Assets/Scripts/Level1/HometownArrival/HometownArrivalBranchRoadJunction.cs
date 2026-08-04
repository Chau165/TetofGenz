using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// T-junction branch on the driver's-left side of the main road. Decor only.
    /// BranchSideSign = -1 places the branch on -X (driver's left).
    /// </summary>
    internal static class HometownArrivalBranchRoadJunction
    {
        /// <summary>-1 = branch on -X (left side of main road in world space).</summary>
        private const float BranchSideSign = -1f;

        private const float JunctionHalfLengthZ = 4.6f;
        private const float BranchHalfWidth = 4.35f;
        private const float DeckThickness = 0.07f;
        private const float DeckY = HighwayLaneSettings.RoadDeckY;
        /// <summary>Lift marks above the branch deck slab top — avoids z-fighting (branch deck is a cube, not a thin mesh).</summary>
        private static float BranchMarkCenterY => DeckY + DeckThickness * 0.5f + 0.03f;
        private const float ShoulderWidth = 1.1f;
        /// <summary>Extra shoulder slab beside the family car so parents can stand off the lane.</summary>
        private const float ParentsWaitingShoulderExtra = 1.9f;
        private const float ParentsWaitingPadLength = 5.4f;
        private const float SegmentOverlap = 0.2f;
        private const float CorridorPadding = 2.6f;
        private const float MouthClearDistance = 8.5f;

        private static readonly (float absX, float dzFromJunction)[] BranchNodeOffsets =
        {
            (24.5f, 0.3f),
            (20.0f, 0.22f),
            (16.0f, 0.12f),
            (12.5f, 0.04f),
            (MainHalfWidth + 0.22f, -0.06f)
        };

        private static float MainHalfWidth => HighwayPlayerLaneLayout.PlayableRoadHalfWidth;
        private static float MainBranchEdge => BranchSideSign * MainHalfWidth;

        private static float activeStopZ = HometownArrivalLayout.BusStopZ;
        private static float activeJunctionZ;
        private static (float x, float z)[] branchNodes;

        public static float JunctionZ => activeJunctionZ;
        public static float CountrysideBeltSideSign => BranchSideSign;

        /// <summary>World pose for the parents' car — parallel to branch, tucked to the tree line on the driver's right.</summary>
        public static bool TryGetFamilyCarStagingPose(out Vector3 position, out Quaternion rotation)
        {
            EnsureBranchNodes();

            int mergeIndex = branchNodes.Length - 1;
            int innerIndex = mergeIndex - 1;
            if (innerIndex < 0)
            {
                position = default;
                rotation = Quaternion.identity;
                return false;
            }

            Vector3 mergePos = new Vector3(branchNodes[mergeIndex].x, 0f, branchNodes[mergeIndex].z);
            Vector3 innerPos = new Vector3(branchNodes[innerIndex].x, 0f, branchNodes[innerIndex].z);
            Vector3 alongBranch = innerPos - mergePos;
            alongBranch.y = 0f;
            if (alongBranch.sqrMagnitude < 0.01f)
            {
                position = default;
                rotation = Quaternion.identity;
                return false;
            }

            alongBranch.Normalize();

            const float alongBranchOffset = 7.8f;
            const float rightTreesideOffset = BranchHalfWidth + ShoulderWidth * 0.15f - 0.48f;

            rotation = Quaternion.LookRotation(-alongBranch, Vector3.up);
            Vector3 onCenterline = mergePos + alongBranch * alongBranchOffset;
            Vector3 treesideDirection = rotation * Vector3.right;
            position = onCenterline + treesideDirection * rightTreesideOffset;
            position.y = HighwayLaneSettings.RoadSurfaceY;
            return true;
        }

        /// <summary>Legacy hook — parents now wait beside the shuttle bus, not the family car.</summary>
        public static bool TryGetParentWaitingPose(int parentIndex, out Vector3 position, out Quaternion rotation)
            => HometownArrivalParentsWaiting.TryGetShuttleBusWaitingPose(parentIndex, out position, out rotation);

        public static void PrepareForCountrysideSpawn(float stopWorldZ)
        {
            activeStopZ = stopWorldZ;
            RebuildBranchNodes();
        }

        internal static bool ShouldSuppressBranchFence(Vector3 world)
        {
            EnsureBranchNodes();
            if (!IsOnBranchSideOfRoad(world.x))
            {
                return false;
            }

            if (IsBeyondBranchOuter(world.x))
            {
                return false;
            }

            return IsOnBranchRoadEnvelope(world)
                || IsInJunctionZone(world)
                || IsInBranchCorridor(world)
                || IsInMainRoadFenceClearBandFull(world);
        }

        /// <summary>Strip any fence sitting on branch deck/shoulder (main-road belt or legacy spawn).</summary>
        private static bool IsOnBranchRoadEnvelope(Vector3 world)
        {
            if (!IsOnBranchSideOfRoad(world.x) || IsBeyondBranchOuter(world.x))
            {
                return false;
            }

            EnsureBranchNodes();
            float minZ = branchNodes[branchNodes.Length - 1].z - CorridorPadding;
            float maxZ = branchNodes[0].z + CorridorPadding;
            if (world.z < minZ || world.z > maxZ)
            {
                return false;
            }

            return DistanceToBranch(world) <= BranchHalfWidth + ShoulderWidth + 0.85f;
        }

        internal static bool ShouldSuppressBranchVegetation(Vector3 world)
        {
            return IsInJunctionZone(world) || IsInBranchCorridor(world);
        }

        // Back-compat aliases for prop factory
        internal static bool ShouldSuppressRightFence(Vector3 world) => ShouldSuppressBranchFence(world);
        internal static bool ShouldSuppressRightVegetation(Vector3 world) => ShouldSuppressBranchVegetation(world);

        public static void Build(Transform approachRoot, float stopWorldZ = HometownArrivalLayout.BusStopZ)
        {
            if (approachRoot == null)
            {
                return;
            }

            activeStopZ = stopWorldZ;
            RebuildBranchNodes();
            DestroyExisting(approachRoot);

            Material road = ResolveRoadMaterial();
            if (road == null)
            {
                Debug.LogWarning("[HometownArrival] Branch junction skipped — road material unavailable.");
                return;
            }

            Material shoulder = HometownArrivalResourceCache.LoadMaterial("Highway_Shoulder_Earth.mat")
                ?? HometownArrivalResourceCache.LoadMaterial("Highway_Shoulder_Concrete.mat");
            Material laneMark = HometownArrivalResourceCache.LoadMaterial(HighwayRoadMaterialDefaults.LaneMarkFile);
            Material grass = HometownArrivalResourceCache.LoadMaterial("Grass_Ground.mat")
                ?? HometownArrivalResourceCache.LoadMaterial("Highway_Grass_NSK2.mat");

            GameObject root = new GameObject("ArrivalBranchJunction");
            root.transform.SetParent(approachRoot, false);

            GameObject deckRoot = new GameObject("BranchDeck");
            deckRoot.transform.SetParent(root.transform, false);
            BuildMergeBlend(deckRoot.transform, road);
            BuildBranchArm(deckRoot.transform, road);
            BuildBranchLaneMarkings(deckRoot.transform, laneMark);
            BuildBranchShoulders(deckRoot.transform, shoulder ?? road);
            BuildParentsWaitingShoulder(deckRoot.transform, shoulder ?? road);

            GameObject decorRoot = new GameObject("BranchRoadside");
            decorRoot.transform.SetParent(root.transform, false);
            BuildBranchGrass(decorRoot.transform, grass);

            OpenCountrysideGap(approachRoot);
        }

        /// <summary>Strip pool props that overlap the branch after segment recycling moves them.</summary>
        public static void MaintainCountrysideGap(Transform approachRoot)
        {
            OpenCountrysideGap(approachRoot);
        }

        private static void DestroyExisting(Transform approachRoot)
        {
            Transform existing = approachRoot.Find("ArrivalBranchJunction");
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }

            Transform legacy = approachRoot.Find("ArrivalParentPath");
            if (legacy != null)
            {
                Object.Destroy(legacy.gameObject);
            }
        }

        private static void RebuildBranchNodes()
        {
            activeJunctionZ = HometownArrivalLayout.JunctionWorldZ;
            branchNodes = new (float x, float z)[BranchNodeOffsets.Length];
            for (int i = 0; i < BranchNodeOffsets.Length; i++)
            {
                (float absX, float dz) = BranchNodeOffsets[i];
                float nodeX = BranchSideSign * Mathf.Max(absX, MainHalfWidth + 0.35f);
                branchNodes[i] = (nodeX, activeJunctionZ + dz);
            }
        }

        private static bool IsOnBranchSideOfRoad(float worldX)
        {
            return BranchSideSign > 0f
                ? worldX >= MainHalfWidth - 0.4f
                : worldX <= -MainHalfWidth + 0.4f;
        }

        private static bool IsBeyondBranchOuter(float worldX)
        {
            EnsureBranchNodes();
            float farX = branchNodes[0].x;
            return BranchSideSign > 0f
                ? worldX > farX + CorridorPadding + 2f
                : worldX < farX - CorridorPadding - 2f;
        }

        private static bool IsInMainRoadFenceClearBandFull(Vector3 world)
        {
            float fenceX = HometownArrivalCountrysideLayout.FenceAbsX;
            const float tolerance = 2.4f;
            bool onFenceLine = BranchSideSign > 0f
                ? Mathf.Abs(world.x - fenceX) <= tolerance
                : Mathf.Abs(world.x + fenceX) <= tolerance;

            return onFenceLine
                && world.z >= activeJunctionZ - MouthClearDistance
                && world.z <= activeJunctionZ + MouthClearDistance;
        }

        private static Material ResolveRoadMaterial()
        {
            Material road = HometownArrivalResourceCache.LoadMaterial(HighwayRoadMaterialDefaults.RoadSurfaceFile);
            return road ?? CreateFallbackRoadMaterial();
        }

        private static Material CreateFallbackRoadMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }

            Material mat = new Material(shader);
            Color asphalt = new Color(0.42f, 0.42f, 0.4f);
            mat.color = asphalt;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", asphalt);
            }

            return mat;
        }

        private static void BuildMergeBlend(Transform parent, Material road)
        {
            EnsureBranchNodes();
            int mergeIndex = branchNodes.Length - 1;
            (float x, float z) merge = branchNodes[mergeIndex];
            (float x, float z) inner = branchNodes[mergeIndex - 1];

            Vector3 mergePos = new Vector3(merge.x, DeckY, merge.z);
            Vector3 innerPos = new Vector3(inner.x, DeckY, inner.z);
            Vector3 branchOut = innerPos - mergePos;
            branchOut.y = 0f;
            if (branchOut.sqrMagnitude < 0.01f)
            {
                return;
            }

            branchOut.Normalize();
            Vector3 towardCenter = BranchSideSign >= 0f ? Vector3.left : Vector3.right;

            // Short branch-side cap — avoids the old axis-aligned pad that striped across the main road.
            float capLength = BranchHalfWidth * 0.75f;
            Vector3 capCenter = mergePos + branchOut * (capLength * 0.32f);
            CreateDeckSlab(
                parent,
                road,
                "Junction_MergeCap",
                capCenter,
                new Vector3(BranchHalfWidth * 2f + 0.12f, DeckThickness, capLength),
                Quaternion.LookRotation(branchOut, Vector3.up));

            // Inner corner fill between branch mouth and main-road edge.
            Vector3 wedgeForward = (branchOut + towardCenter * 0.62f).normalized;
            Vector3 wedgeCenter = mergePos + towardCenter * (BranchHalfWidth * 0.38f) + branchOut * 0.18f;
            CreateDeckSlab(
                parent,
                road,
                "Junction_InnerWedge",
                wedgeCenter,
                new Vector3(BranchHalfWidth * 1.05f, DeckThickness, BranchHalfWidth * 0.82f),
                Quaternion.LookRotation(wedgeForward, Vector3.up));
        }

        private static void BuildBranchArm(Transform parent, Material road)
        {
            EnsureBranchNodes();
            int lastIndex = branchNodes.Length - 2;
            for (int i = 0; i < branchNodes.Length - 1; i++)
            {
                (float x, float z) a = branchNodes[i];
                (float x, float z) b = branchNodes[i + 1];
                Vector3 start = new Vector3(a.x, DeckY, a.z);
                Vector3 end = new Vector3(b.x, DeckY, b.z);

                float startOverlap = i == 0 ? 0f : SegmentOverlap * 0.5f;
                float endOverlap = i == lastIndex ? 0f : SegmentOverlap * 0.5f;
                CreateRoadSegment(parent, road, start, end, BranchHalfWidth * 2f, "BranchSeg_" + i.ToString("00"), startOverlap, endOverlap);
            }
        }

        private static void BuildBranchLaneMarkings(Transform parent, Material laneMark)
        {
            if (laneMark == null)
            {
                return;
            }

            EnsureBranchNodes();
            const float dividerWidth = 0.2f;

            GameObject marksRoot = new GameObject("BranchRoadMarkings");
            marksRoot.transform.SetParent(parent, false);

            float pathDistance = 0f;
            float nextDashCenter = HighwayRoadLayout.DashLength * 0.5f;

            for (int i = 0; i < branchNodes.Length - 1; i++)
            {
                (float x, float z) a = branchNodes[i];
                (float x, float z) b = branchNodes[i + 1];
                Vector3 start = new Vector3(a.x, 0f, a.z);
                Vector3 end = new Vector3(b.x, 0f, b.z);
                Vector3 delta = end - start;
                delta.y = 0f;
                float segmentLength = delta.magnitude;
                if (segmentLength < 0.1f)
                {
                    continue;
                }

                Vector3 forward = delta.normalized;
                Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
                float segmentStart = pathDistance;
                float segmentEnd = pathDistance + segmentLength;

                while (nextDashCenter < segmentEnd)
                {
                    if (nextDashCenter >= segmentStart)
                    {
                        float localAlong = nextDashCenter - segmentStart;
                        Vector3 center = start + forward * localAlong;
                        center.y = BranchMarkCenterY;
                        CreateLaneMarkQuad(
                            marksRoot.transform,
                            laneMark,
                            center,
                            rotation,
                            new Vector3(dividerWidth, HighwayRoadLayout.MarkThickness, HighwayRoadLayout.DashLength));
                    }

                    nextDashCenter += HighwayRoadLayout.DashCycle;
                }

                pathDistance = segmentEnd;
            }
        }

        private static void CreateLaneMarkQuad(
            Transform parent,
            Material laneMark,
            Vector3 worldCenter,
            Quaternion worldRotation,
            Vector3 size)
        {
            GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mark.name = "BranchLaneMark";
            mark.transform.SetParent(parent, false);
            mark.transform.position = worldCenter;
            mark.transform.rotation = worldRotation;
            mark.transform.localScale = size;

            Collider collider = mark.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            MeshRenderer renderer = mark.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = laneMark;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static void BuildBranchShoulders(Transform parent, Material shoulder)
        {
            BuildBranchEdgeStrip(parent, shoulder, "BranchShoulder_Outer_", BranchHalfWidth + ShoulderWidth * 0.5f, 0.85f);
        }

        private static void BuildParentsWaitingShoulder(Transform parent, Material shoulder)
        {
            if (shoulder == null || !TryGetFamilyCarStagingPose(out Vector3 carPos, out Quaternion carRot))
            {
                return;
            }

            Vector3 branchForward = carRot * Vector3.forward;
            Vector3 outer = GetBranchOuterDirection();
            float padWidth = ShoulderWidth + ParentsWaitingShoulderExtra;
            float lateral = BranchHalfWidth + padWidth * 0.52f;
            Vector3 padCenter = carPos + outer * lateral;

            CreateDeckSlab(
                parent,
                shoulder,
                "BranchShoulder_ParentsWaiting",
                new Vector3(padCenter.x, 0f, padCenter.z),
                new Vector3(padWidth, DeckThickness * 0.88f, ParentsWaitingPadLength),
                Quaternion.LookRotation(branchForward, Vector3.up));
        }

        private static void BuildBranchGrass(Transform parent, Material grass)
        {
            if (grass == null)
            {
                return;
            }

            BuildBranchEdgeStrip(parent, grass, "BranchGrass_", BranchHalfWidth + ShoulderWidth + 1.9f, 0.07f, 3.4f, -0.1f);
        }

        private static void BuildBranchEdgeStrip(
            Transform parent,
            Material material,
            string namePrefix,
            float lateralOffset,
            float heightScale,
            float stripWidth = 0f,
            float y = 0f)
        {
            EnsureBranchNodes();
            float width = stripWidth > 0f ? stripWidth : ShoulderWidth;
            for (int i = 0; i < branchNodes.Length - 1; i++)
            {
                (float x, float z) a = branchNodes[i];
                (float x, float z) b = branchNodes[i + 1];
                Vector3 start = new Vector3(a.x, y, a.z);
                Vector3 end = new Vector3(b.x, y, b.z);
                Vector3 delta = end - start;
                delta.y = 0f;
                if (delta.sqrMagnitude < 0.01f)
                {
                    continue;
                }

                Vector3 forward = delta.normalized;
                float length = delta.magnitude + (stripWidth > 0f ? 0.6f : SegmentOverlap);
                Vector3 center = (start + end) * 0.5f + GetBranchOuterDirection() * lateralOffset;
                CreateDeckSlab(
                    parent,
                    material,
                    namePrefix + i.ToString("00"),
                    center,
                    new Vector3(width, DeckThickness * heightScale, length),
                    Quaternion.LookRotation(forward, Vector3.up));
            }
        }

        private static Vector3 GetBranchOuterDirection()
        {
            return BranchSideSign >= 0f ? Vector3.right : Vector3.left;
        }

        private static void CreateRoadSegment(
            Transform parent,
            Material material,
            Vector3 start,
            Vector3 end,
            float width,
            string objectName,
            float startOverlap = 0f,
            float endOverlap = 0f)
        {
            Vector3 delta = end - start;
            delta.y = 0f;
            float length = delta.magnitude + startOverlap + endOverlap;
            if (length < 0.05f)
            {
                return;
            }

            Vector3 forward = delta.normalized;
            Vector3 center = (start + end) * 0.5f + forward * (startOverlap - endOverlap) * 0.5f;
            CreateDeckSlab(
                parent,
                material,
                objectName,
                center,
                new Vector3(width, DeckThickness, length),
                Quaternion.LookRotation(forward, Vector3.up));
        }

        private static void CreateDeckSlab(
            Transform parent,
            Material material,
            string objectName,
            Vector3 worldCenter,
            Vector3 scale,
            Quaternion worldRotation)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = objectName;
            slab.transform.SetParent(parent, false);
            slab.transform.position = worldCenter;
            slab.transform.rotation = worldRotation;
            slab.transform.localScale = scale;

            Collider collider = slab.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            MeshRenderer renderer = slab.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }
        }

        private static void OpenCountrysideGap(Transform approachRoot)
        {
            StripLegacyBranchFences(approachRoot);
            StripHighwayMarksAcrossBranch(approachRoot);

            HometownArrivalCountrysideRoadsidePool pool =
                approachRoot.GetComponentInChildren<HometownArrivalCountrysideRoadsidePool>(true);
            if (pool == null)
            {
                return;
            }

            string beltName = BranchSideSign > 0f ? "RightBelt" : "LeftBelt";
            Transform[] belts = pool.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < belts.Length; i++)
            {
                Transform belt = belts[i];
                if (belt == null || belt.name != beltName)
                {
                    continue;
                }

                for (int c = belt.childCount - 1; c >= 0; c--)
                {
                    Transform prop = belt.GetChild(c);
                    if (prop == null)
                    {
                        continue;
                    }

                    if (prop.name == "GrassSlab")
                    {
                        if (OverlapsBranchCorridor(prop))
                        {
                            Object.Destroy(prop.gameObject);
                        }

                        continue;
                    }

                    Vector3 world = prop.position;
                    if (ShouldSuppressBranchVegetation(world)
                        || (IsFenceName(prop.name) && ShouldSuppressBranchFence(world)))
                    {
                        Object.Destroy(prop.gameObject);
                    }
                }
            }
        }

        /// <summary>Remove dashed lane marks from the main highway that bleed across the branch deck.</summary>
        private static void StripHighwayMarksAcrossBranch(Transform approachRoot)
        {
            if (approachRoot == null)
            {
                return;
            }

            Transform roadRoot = approachRoot.Find("ArrivalHighwayRoad");
            if (roadRoot == null)
            {
                return;
            }

            Transform[] children = roadRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || child.name != "LaneDivider")
                {
                    continue;
                }

                if (OverlapsBranchCorridor(child))
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        private static bool OverlapsBranchCorridor(Transform prop)
        {
            if (prop == null)
            {
                return false;
            }

            Renderer renderer = prop.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                Vector3[] samples =
                {
                    bounds.center,
                    new Vector3(bounds.min.x, 0f, bounds.min.z),
                    new Vector3(bounds.max.x, 0f, bounds.min.z),
                    new Vector3(bounds.min.x, 0f, bounds.max.z),
                    new Vector3(bounds.max.x, 0f, bounds.max.z)
                };

                for (int i = 0; i < samples.Length; i++)
                {
                    if (IsOnBranchRoadEnvelope(samples[i]) || IsInBranchCorridor(samples[i]))
                    {
                        return true;
                    }
                }
            }

            Vector3 world = prop.position;
            return IsOnBranchRoadEnvelope(world) || IsInBranchCorridor(world);
        }

        private static void StripLegacyBranchFences(Transform approachRoot)
        {
            if (approachRoot == null)
            {
                return;
            }

            Transform junction = approachRoot.Find("ArrivalBranchJunction");
            if (junction == null)
            {
                return;
            }

            Transform[] children = junction.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || child == junction || !IsFenceName(child.name))
                {
                    continue;
                }

                Object.Destroy(child.gameObject);
            }
        }

        private static bool IsFenceName(string objectName)
        {
            return objectName == "Fence" || (objectName != null && objectName.StartsWith("Fence"));
        }

        private static bool IsInJunctionZone(Vector3 world)
        {
            EnsureBranchNodes();
            float minX;
            float maxX;
            if (BranchSideSign > 0f)
            {
                minX = MainBranchEdge - 2.2f;
                maxX = branchNodes[0].x + 2.2f;
            }
            else
            {
                minX = branchNodes[0].x - 2.2f;
                maxX = MainBranchEdge + 2.2f;
            }

            return world.x >= minX && world.x <= maxX
                && world.z >= activeJunctionZ - JunctionHalfLengthZ - 0.7f
                && world.z <= activeJunctionZ + JunctionHalfLengthZ + 0.7f;
        }

        private static bool IsInBranchCorridor(Vector3 world)
        {
            if (!IsOnBranchSideOfRoad(world.x))
            {
                return false;
            }

            EnsureBranchNodes();
            float minZ = branchNodes[branchNodes.Length - 1].z - CorridorPadding;
            float maxZ = branchNodes[0].z + CorridorPadding;
            if (world.z < minZ || world.z > maxZ)
            {
                return false;
            }

            if (BranchSideSign > 0f && world.x > branchNodes[0].x + CorridorPadding + 1f)
            {
                return false;
            }

            if (BranchSideSign < 0f && world.x < branchNodes[0].x - CorridorPadding - 1f)
            {
                return false;
            }

            return DistanceToBranch(world) <= BranchHalfWidth + CorridorPadding;
        }

        private static float DistanceToBranch(Vector3 world)
        {
            EnsureBranchNodes();
            float best = float.MaxValue;
            for (int i = 0; i < branchNodes.Length - 1; i++)
            {
                Vector3 a = new Vector3(branchNodes[i].x, 0f, branchNodes[i].z);
                Vector3 b = new Vector3(branchNodes[i + 1].x, 0f, branchNodes[i + 1].z);
                float d = DistancePointToSegmentXZ(world, a, b);
                if (d < best)
                {
                    best = d;
                }
            }

            return best;
        }

        private static void EnsureBranchNodes()
        {
            if (branchNodes == null || branchNodes.Length == 0)
            {
                RebuildBranchNodes();
            }
        }

        private static float DistancePointToSegmentXZ(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector2 p = new Vector2(point.x, point.z);
            Vector2 v = new Vector2(b.x - a.x, b.z - a.z);
            Vector2 w = new Vector2(p.x - a.x, p.y - a.z);
            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0f)
            {
                return Vector2.Distance(p, new Vector2(a.x, a.z));
            }

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
            {
                return Vector2.Distance(p, new Vector2(b.x, b.z));
            }

            float t = c1 / c2;
            Vector2 projection = new Vector2(a.x, a.z) + v * t;
            return Vector2.Distance(p, projection);
        }
    }
}
