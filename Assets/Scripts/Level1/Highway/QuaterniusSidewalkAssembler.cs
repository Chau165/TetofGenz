using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>
    /// Continuous urban sidewalk slab using one shared Quaternius concrete material.
    /// </summary>
    public static class QuaterniusSidewalkAssembler
    {
        private const float CurbHeight = 0.14f;
        private const float PavementThickness = 0.08f;
        private const float UvMetersPerTile = HighwaySidewalkMaterialFactory.WorldUvMetersPerRepeat;

        public static GameObject BuildSegment(
            QuaterniusDowntownPrefabLibrary library,
            bool leftSide,
            int seed,
            float segmentLengthZ)
        {
            Material pavementMaterial = ResolvePavementMaterial(library);
            if (pavementMaterial == null)
            {
                return null;
            }

            string segmentName = leftSide ? "UrbanSidewalk_L" : "UrbanSidewalk_R";
            var root = new GameObject(segmentName);
            var pavementRoot = new GameObject("Pavement");
            pavementRoot.transform.SetParent(root.transform, false);
            var curbRoot = new GameObject("Curb");
            curbRoot.transform.SetParent(root.transform, false);

            BuildContinuousPavement(pavementRoot.transform, leftSide, segmentLengthZ, pavementMaterial, 0f);
            BuildCurbStrip(curbRoot.transform, leftSide, segmentLengthZ, pavementMaterial);

            var decorRoot = new GameObject("Decor");
            decorRoot.transform.SetParent(root.transform, false);
            QuaterniusSidewalkDecorAssembler.BuildDecor(decorRoot.transform, library, leftSide, seed, segmentLengthZ);

            return root;
        }

        public static Material ResolvePavementMaterial(QuaterniusDowntownPrefabLibrary library) =>
            HighwaySidewalkMaterialUtility.Resolve(library);

        public static void SyncSegmentWorldUvs(GameObject segment, float segmentWorldZ, float segmentLengthZ)
        {
            if (segment == null)
            {
                return;
            }

            Transform pavementRoot = segment.transform.Find("Pavement");
            if (pavementRoot == null)
            {
                return;
            }

            MeshFilter filter = pavementRoot.GetComponentInChildren<MeshFilter>(true);
            if (filter == null || filter.sharedMesh == null)
            {
                return;
            }

            bool leftSide = segment.name.Contains("_L");
            float innerX = HighwayUrbanRoadsideLayout.SidewalkInnerX;
            float outerX = HighwayUrbanRoadsideLayout.SidewalkOuterX;
            float width = outerX - innerX;
            float sign = leftSide ? -1f : 1f;
            float worldCenterX = sign * (innerX + width * 0.5f);

            float meshLength = segmentLengthZ + HighwayRoadLayout.SegmentOverlapZ;
            filter.sharedMesh = BuildPavementMesh(
                width,
                meshLength,
                worldCenterX,
                segmentWorldZ,
                segmentLengthZ);
        }

        private static void BuildContinuousPavement(
            Transform parent,
            bool leftSide,
            float segmentLengthZ,
            Material pavementMaterial,
            float segmentWorldZ)
        {
            float innerX = HighwayUrbanRoadsideLayout.SidewalkInnerX;
            float outerX = HighwayUrbanRoadsideLayout.SidewalkOuterX;
            float width = outerX - innerX;
            float centerAbsX = innerX + width * 0.5f;
            float sign = leftSide ? -1f : 1f;
            float meshLength = segmentLengthZ + HighwayRoadLayout.SegmentOverlapZ;
            float worldCenterX = sign * centerAbsX;

            GameObject slab = new GameObject("PavementSlab");
            slab.transform.SetParent(parent, false);
            slab.transform.localPosition = new Vector3(
                worldCenterX,
                HighwayUrbanRoadsideLayout.SidewalkSurfaceY + PavementThickness * 0.5f,
                segmentLengthZ * 0.5f);
            slab.transform.localRotation = Quaternion.identity;
            slab.transform.localScale = Vector3.one;

            MeshFilter filter = slab.AddComponent<MeshFilter>();
            MeshRenderer renderer = slab.AddComponent<MeshRenderer>();
            filter.sharedMesh = BuildPavementMesh(
                width,
                meshLength,
                worldCenterX,
                segmentWorldZ,
                segmentLengthZ);
            renderer.sharedMaterial = pavementMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static Mesh BuildPavementMesh(
            float width,
            float length,
            float worldCenterX,
            float segmentWorldZ,
            float segmentLengthZ)
        {
            float halfWidth = width * 0.5f;
            float halfLength = length * 0.5f;
            float slabCenterWorldZ = segmentWorldZ + segmentLengthZ * 0.5f;

            var vertices = new Vector3[]
            {
                new Vector3(-halfWidth, 0f, -halfLength),
                new Vector3(halfWidth, 0f, -halfLength),
                new Vector3(-halfWidth, 0f, halfLength),
                new Vector3(halfWidth, 0f, halfLength)
            };

            var uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                float worldX = worldCenterX + vertices[i].x;
                float worldZ = slabCenterWorldZ + vertices[i].z;
                uvs[i] = new Vector2(
                    worldX / UvMetersPerTile,
                    worldZ / UvMetersPerTile);
            }

            var triangles = new[] { 0, 2, 1, 1, 2, 3 };

            Mesh mesh = new Mesh { name = "UrbanPavementSlab" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void BuildCurbStrip(
            Transform parent,
            bool leftSide,
            float segmentLengthZ,
            Material curbMaterial)
        {
            float sign = leftSide ? -1f : 1f;
            float centerAbsX = HighwayUrbanRoadsideLayout.CurbInnerX + HighwayUrbanRoadsideLayout.CurbWidth * 0.5f;
            float length = segmentLengthZ + HighwayRoadLayout.SegmentOverlapZ;

            GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = "RoadCurb";
            curb.transform.SetParent(parent, false);
            curb.transform.localPosition = new Vector3(
                sign * centerAbsX,
                HighwayUrbanRoadsideLayout.SidewalkSurfaceY + CurbHeight * 0.5f,
                segmentLengthZ * 0.5f);
            curb.transform.localRotation = Quaternion.identity;
            curb.transform.localScale = new Vector3(
                HighwayUrbanRoadsideLayout.CurbWidth,
                CurbHeight,
                length);
            HighwayPropUtility.SafeDestroy(curb.GetComponent<Collider>());

            if (curbMaterial != null)
            {
                curb.GetComponent<Renderer>().sharedMaterial = curbMaterial;
            }
        }
    }
}
