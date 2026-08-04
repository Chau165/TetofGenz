using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Cross-quad NSK2 grass blades with alpha-cutout textures.</summary>
    public static class HighwayNatureGrassTuftMesh
    {
        private static Mesh sharedBladeMesh;

        public static Mesh SharedBladeMesh
        {
            get
            {
                if (sharedBladeMesh == null)
                {
                    sharedBladeMesh = CreateBladeMesh(1f, 0.5f);
                }

                return sharedBladeMesh;
            }
        }

        public static GameObject CreateTuft(
            Transform parent,
            Material grassA,
            Material grassB,
            int seed,
            float scale,
            bool addWindSway = true)
        {
            GameObject root = new GameObject("GrassTuft");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.Euler(0f, (seed * 37) % 360, 0f);
            root.transform.localScale = Vector3.one * scale;

            BuildCrossPlanes(root.transform, grassA, grassB, seed);
            if (addWindSway)
            {
                HighwayNatureWindSway sway = root.AddComponent<HighwayNatureWindSway>();
                sway.ConfigureForGrass(seed);
            }

            return root;
        }

        public static Mesh CreateBladeMesh(float height, float width)
        {
            var mesh = new Mesh { name = "GrassBlade" };
            float halfW = width * 0.5f;
            mesh.vertices = new[]
            {
                new Vector3(-halfW, 0f, 0f),
                new Vector3(halfW, 0f, 0f),
                new Vector3(halfW * 0.65f, height, 0f),
                new Vector3(-halfW * 0.65f, height, 0f)
            };
            mesh.uv = new[]
            {
                new Vector2(0.05f, 0.02f),
                new Vector2(0.95f, 0.02f),
                new Vector2(0.85f, 0.98f),
                new Vector2(0.15f, 0.98f)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void BuildCrossPlanes(Transform parent, Material grassA, Material grassB, int seed)
        {
            float height = 0.95f + (seed % 6) * 0.14f;
            float width = 0.55f + (seed % 4) * 0.08f;
            Mesh bladeMesh = CreateBladeMesh(height, width);
            int bladeCount = 5 + seed % 2;

            for (int i = 0; i < bladeCount; i++)
            {
                float yaw = i * (360f / bladeCount) + (seed % 11) * 4f;
                float tilt = ((seed + i * 3) % 5 - 2) * 2.5f;
                GameObject plane = new GameObject("Blade_" + i);
                plane.transform.SetParent(parent, false);
                plane.transform.localRotation = Quaternion.Euler(tilt, yaw, 0f);

                MeshFilter filter = plane.AddComponent<MeshFilter>();
                MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
                filter.sharedMesh = bladeMesh;
                renderer.sharedMaterial = (i + seed) % 2 == 0 ? grassA : grassB ?? grassA;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
}
