using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Procedural door/window/roof/trim accents on single-mesh building prefabs.</summary>
    public static class HighwayUrbanFacadeAccents
    {
        private const string AccentsRootName = "FacadeAccents";

        public static void Apply(
            GameObject buildingRoot,
            bool leftSide,
            int seed,
            Material wallMat,
            Material roofMat,
            Material windowMat,
            Material doorMat,
            Material trimMat)
        {
            if (buildingRoot == null || wallMat == null)
            {
                return;
            }

            Transform meshHost = FindMeshHost(buildingRoot);
            if (meshHost == null)
            {
                return;
            }

            Transform existing = meshHost.Find(AccentsRootName);
            if (existing != null)
            {
                HighwayPropUtility.SafeDestroy(existing.gameObject);
            }

            Bounds bounds = ComputeBounds(meshHost.gameObject);
            if (bounds.size.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 towardRoad = leftSide ? Vector3.right : Vector3.left;
            float facadeX = leftSide ? bounds.max.x : bounds.min.x;
            float inset = 0.07f;
            float facadePlaneX = facadeX + (leftSide ? inset : -inset);

            float width = Mathf.Max(bounds.size.z, bounds.size.x * 0.55f);
            float height = bounds.size.y;
            float baseY = bounds.min.y;
            float centerZ = bounds.center.z;

            GameObject accentsRoot = new GameObject(AccentsRootName);
            accentsRoot.transform.SetParent(meshHost, false);
            accentsRoot.transform.localPosition = Vector3.zero;
            accentsRoot.transform.localRotation = Quaternion.identity;
            accentsRoot.transform.localScale = Vector3.one;

            float depth = 0.11f + (seed % 5) * 0.01f;
            float trimH = Mathf.Clamp(height * 0.07f, 0.12f, 0.35f);
            float roofH = Mathf.Clamp(height * 0.1f, 0.14f, 0.42f);
            float doorW = Mathf.Clamp(width * 0.2f, 0.45f, 1.1f);
            float doorH = Mathf.Clamp(height * 0.28f, 0.7f, 1.6f);
            float winW = Mathf.Clamp(width * 0.14f, 0.32f, 0.75f);
            float winH = Mathf.Clamp(height * 0.12f, 0.28f, 0.55f);

            SpawnSlab(
                accentsRoot.transform,
                new Vector3(facadePlaneX, baseY + trimH * 0.5f, centerZ),
                new Vector3(depth, trimH, width * 0.96f),
                trimMat ?? wallMat,
                "trim_base");

            SpawnSlab(
                accentsRoot.transform,
                new Vector3(facadePlaneX, bounds.max.y - roofH * 0.5f, centerZ),
                new Vector3(depth * 1.15f, roofH, width * 0.92f),
                roofMat ?? wallMat,
                "roof_lip");

            SpawnSlab(
                accentsRoot.transform,
                new Vector3(facadePlaneX, baseY + doorH * 0.5f + trimH, centerZ),
                new Vector3(depth * 1.05f, doorH, doorW),
                doorMat ?? trimMat ?? wallMat,
                "door");

            int windowColumns = width > 2.4f ? 3 : 2;
            int windowRows = height > 2.8f ? 2 : 1;
            float rowGap = (height - doorH - trimH - roofH) / (windowRows + 1);
            float colGap = width / (windowColumns + 1);

            for (int row = 0; row < windowRows; row++)
            {
                for (int col = 0; col < windowColumns; col++)
                {
                    if ((seed + row * 3 + col) % 11 == 0)
                    {
                        continue;
                    }

                    float z = bounds.min.z + colGap * (col + 1);
                    float y = baseY + trimH + doorH + rowGap * (row + 1);
                    SpawnSlab(
                        accentsRoot.transform,
                        new Vector3(facadePlaneX, y, z),
                        new Vector3(depth * 0.9f, winH, winW),
                        windowMat ?? wallMat,
                        $"window_{row}_{col}");
                }
            }

            if ((seed % 3) != 0)
            {
                float awningY = baseY + height * 0.62f;
                SpawnSlab(
                    accentsRoot.transform,
                    new Vector3(facadePlaneX + (leftSide ? depth * 0.35f : -depth * 0.35f), awningY, centerZ),
                    new Vector3(depth * 1.8f, 0.08f, width * 0.55f),
                    trimMat ?? wallMat,
                    "awning");
            }
        }

        private static Transform FindMeshHost(GameObject root)
        {
            if (root.transform.childCount > 0)
            {
                return root.transform.GetChild(0);
            }

            return root.transform;
        }

        private static void SpawnSlab(
            Transform parent,
            Vector3 worldCenter,
            Vector3 worldSize,
            Material material,
            string name)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, true);
            slab.transform.position = worldCenter;
            slab.transform.localScale = worldSize;
            Renderer renderer = slab.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            HighwayPropUtility.SafeDestroy(slab.GetComponent<Collider>());
        }

        private static Bounds ComputeBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].gameObject.name != AccentsRootName)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return bounds;
        }
    }
}
