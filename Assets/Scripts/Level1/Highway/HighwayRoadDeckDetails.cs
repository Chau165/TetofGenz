using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Procedural highway deck details — guardrails, seams, shoulder lines, tire wear.</summary>
    public static class HighwayRoadDeckDetails
    {
        private const float PostSpacing = 7f;
        private const float RailHeightBottom = 0.38f;
        private const float RailHeightTop = 0.72f;
        private const float RailThickness = 0.06f;
        private const float PostSize = 0.09f;
        private const float SeamInterval = 15f;
        private const float SeamWidth = 0.08f;

        public static void BuildAll(
            Transform segmentRoot,
            float segmentLength,
            Material guardMaterial,
            Material laneMarkMaterial,
            Material tireWearMaterial)
        {
            Transform guardRoot = segmentRoot.Find("GuardEdges");
            if (guardRoot == null)
            {
                GameObject go = new GameObject("GuardEdges");
                go.transform.SetParent(segmentRoot, false);
                go.transform.localRotation = Quaternion.identity;
                guardRoot = go.transform;
            }
            else
            {
                HighwayPropUtility.ClearChildren(guardRoot);
            }

            BuildGuardRails(guardRoot, segmentLength, guardMaterial);
            BuildShoulderEdgeLines(segmentRoot, segmentLength, laneMarkMaterial);
            BuildExpansionSeams(segmentRoot, segmentLength, tireWearMaterial ?? guardMaterial);
            BuildTireWearMarks(segmentRoot, segmentLength, tireWearMaterial);
        }

        public static void BuildGuardRails(Transform parent, float segmentLength, Material guardMaterial)
        {
            float outer = HighwayRoadLayout.ShoulderOuterX;
            float length = segmentLength + HighwayRoadLayout.SegmentOverlapZ;
            float baseY = HighwayLaneSettings.ShoulderSurfaceY;
            Material mat = guardMaterial;

            BuildSideGuardRail(parent, "GuardRail_L", -outer, baseY, length, mat);
            BuildSideGuardRail(parent, "GuardRail_R", outer, baseY, length, mat);
        }

        private static void BuildSideGuardRail(
            Transform parent,
            string name,
            float x,
            float baseY,
            float length,
            Material mat)
        {
            GameObject side = new GameObject(name);
            side.transform.SetParent(parent, false);
            side.transform.localRotation = Quaternion.identity;

            float centerZ = length * 0.5f;
            float inward = x < 0f ? 1f : -1f;
            float railX = x + inward * PostSize * 0.35f;

            for (float z = 0f; z <= length + 0.01f; z += PostSpacing)
            {
                BuildSlab(side.transform, "Post_" + Mathf.RoundToInt(z * 10f),
                    mat,
                    new Vector3(railX, baseY + RailHeightTop * 0.5f, z),
                    new Vector3(PostSize, RailHeightTop, PostSize));
            }

            BuildSlab(side.transform, "RailBottom",
                mat,
                new Vector3(railX, baseY + RailHeightBottom, centerZ),
                new Vector3(RailThickness, RailThickness, length));

            BuildSlab(side.transform, "RailTop",
                mat,
                new Vector3(railX, baseY + RailHeightTop, centerZ),
                new Vector3(RailThickness, RailThickness, length));
        }

        public static void BuildShoulderEdgeLines(Transform segmentRoot, float segmentLength, Material laneMarkMaterial)
        {
            Transform marks = segmentRoot.Find("RoadMarkings");
            if (marks == null || laneMarkMaterial == null)
            {
                return;
            }

            float outer = HighwayRoadLayout.ShoulderOuterX;
            float lineWidth = 0.12f;
            float y = HighwayRoadLayout.MarkingY - 0.01f;
            float centerZ = segmentLength * 0.5f;
            float length = segmentLength + HighwayRoadLayout.SegmentOverlapZ * 0.5f;

            BuildSlab(marks, "ShoulderEdge_L", laneMarkMaterial,
                new Vector3(-outer + lineWidth * 0.5f, y, centerZ),
                new Vector3(lineWidth, HighwayRoadLayout.MarkThickness, length));

            BuildSlab(marks, "ShoulderEdge_R", laneMarkMaterial,
                new Vector3(outer - lineWidth * 0.5f, y, centerZ),
                new Vector3(lineWidth, HighwayRoadLayout.MarkThickness, length));
        }

        public static void BuildExpansionSeams(Transform segmentRoot, float segmentLength, Material seamMaterial)
        {
            if (seamMaterial == null)
            {
                return;
            }

            Transform deck = segmentRoot.Find("RoadDeck");
            if (deck == null)
            {
                return;
            }

            float halfWidth = HighwayPlayerLaneLayout.PlayableRoadHalfWidth;
            float y = HighwayLaneSettings.RoadDeckY + 0.003f;

            for (float z = SeamInterval * 0.5f; z < segmentLength; z += SeamInterval)
            {
                BuildSlab(deck, "Seam_" + Mathf.RoundToInt(z),
                    seamMaterial,
                    new Vector3(0f, y, z),
                    new Vector3(halfWidth * 2f, 0.004f, SeamWidth));
            }
        }

        public static void BuildTireWearMarks(Transform segmentRoot, float segmentLength, Material tireWearMaterial)
        {
            if (tireWearMaterial == null)
            {
                return;
            }

            Transform deck = segmentRoot.Find("RoadDeck");
            if (deck == null)
            {
                return;
            }

            int segmentIndex = ParseSegmentIndex(segmentRoot.name);
            float y = HighwayLaneSettings.RoadDeckY + 0.002f;
            int markCount = 2 + (segmentIndex % 2);

            for (int i = 0; i < markCount; i++)
            {
                int seed = segmentIndex * 17 + i * 31;
                float laneX = HighwayRoadLayout.PlayerLaneCenters[seed % HighwayRoadLayout.PlayerLaneCenters.Length];
                float z = segmentLength * (0.25f + (seed % 50) * 0.01f);
                float length = 6f + (seed % 4);
                float width = 0.55f + (seed % 3) * 0.08f;

                BuildSlab(deck, "TireWear_" + i, tireWearMaterial,
                    new Vector3(laneX, y, z),
                    new Vector3(width, 0.003f, length));
            }
        }

        private static int ParseSegmentIndex(string segmentName)
        {
            int underscore = segmentName.LastIndexOf('_');
            if (underscore >= 0 && int.TryParse(segmentName.Substring(underscore + 1), out int index))
            {
                return index;
            }

            return 0;
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
    }
}
