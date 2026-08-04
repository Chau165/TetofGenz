using ChoTet.Level1.HometownArrival;
using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Replaces decorative lamp/pot props with low, readable flower beds and wooden flower trellises.
    /// </summary>
    public static class ChoTetFloralDressing
    {
        private static Material woodMaterial;
        private static Material leafMaterial;
        private static Material flowerYellowMaterial;
        private static Material flowerPinkMaterial;
        private static Material flowerRedMaterial;
        private static Material flowerPurpleMaterial;

        public static void Apply(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            DestroyChild(worldRoot, "ChoTetFloralDressing");
            RemoveLampAndPotProps(worldRoot);
            ResolveMaterials();

            GameObject root = new GameObject("ChoTetFloralDressing");
            root.transform.SetParent(worldRoot, false);
            root.transform.localPosition = Vector3.zero;

            SpawnGroundFlowers(root.transform);
            SpawnFlowerTrellises(root.transform);
        }

        private static void ResolveMaterials()
        {
            woodMaterial = HometownArrivalResourceCache.LoadMaterial("Wood_Warm.mat")
                ?? CreateRuntimeMaterial("ChoTetFlowerWood", new Color(0.38f, 0.22f, 0.12f));
            leafMaterial = HometownArrivalResourceCache.LoadMaterial("Foliage_Green.mat")
                ?? CreateRuntimeMaterial("ChoTetFlowerLeaves", new Color(0.22f, 0.54f, 0.2f));
            flowerYellowMaterial = HometownArrivalResourceCache.LoadMaterial("Flower_Yellow.mat")
                ?? CreateRuntimeMaterial("ChoTetFlowerYellow", new Color(0.95f, 0.76f, 0.16f));
            flowerPinkMaterial = CreateRuntimeMaterial("ChoTetFlowerPink", new Color(0.95f, 0.34f, 0.55f));
            flowerRedMaterial = CreateRuntimeMaterial("ChoTetFlowerRed", new Color(0.82f, 0.18f, 0.16f));
            flowerPurpleMaterial = CreateRuntimeMaterial("ChoTetFlowerPurple", new Color(0.55f, 0.3f, 0.78f));
        }

        private static Material CreateRuntimeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }

        private static void RemoveLampAndPotProps(Transform worldRoot)
        {
            Transform[] all = worldRoot.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                Transform node = all[i];
                if (node == null || node == worldRoot)
                {
                    continue;
                }

                if (HasAncestorNamed(node, "HoaMaiStall"))
                {
                    continue;
                }

                string name = node.name;
                bool remove = name.Contains("Lantern")
                    || name.Contains("RoadPole")
                    || name.Contains("electric pole")
                    || name.Contains("FlowerPot")
                    || name.Contains("Flower Pot")
                    || name.StartsWith("TempleFlower_");

                if (remove)
                {
                    DestroyObject(node.gameObject);
                }
            }
        }

        private static bool HasAncestorNamed(Transform node, string ancestorName)
        {
            Transform current = node;
            while (current != null)
            {
                if (current.name == ancestorName)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static void SpawnGroundFlowers(Transform parent)
        {
            // Dense yellow-first beds around the hoa mai seller, plus a small east edge garden.
            SpawnFlowerBed(parent, "HoaMaiFlowerGarden", new Vector3(12f, 0f, 85.5f), new Vector2(3.4f, 2.8f), 36, 223, preferYellow: true);
            SpawnFlowerBed(parent, "MarketFlowerGarden", new Vector3(20.5f, 0f, 96f), new Vector2(3.6f, 4.5f), 28, 251, preferYellow: true);
        }

        private static void SpawnFlowerBed(
            Transform parent,
            string groupName,
            Vector3 center,
            Vector2 radius,
            int count,
            int seed,
            bool preferYellow = false)
        {
            GameObject group = new GameObject(groupName);
            group.transform.SetParent(parent, false);
            System.Random random = new System.Random(seed);
            int spawned = 0;
            int attempts = 0;

            while (spawned < count && attempts < count * 8)
            {
                attempts++;
                float angle = NextRange(random, 0f, Mathf.PI * 2f);
                float distance = Mathf.Sqrt(NextRange(random, 0f, 1f));
                Vector3 position = new Vector3(
                    center.x + Mathf.Cos(angle) * radius.x * distance,
                    0.03f,
                    center.z + Mathf.Sin(angle) * radius.y * distance);

                if (IsInWalkClearance(position))
                {
                    continue;
                }

                SpawnFlowerCluster(
                    group.transform,
                    position,
                    NextRange(random, 0f, 360f),
                    NextRange(random, 0.9f, 1.35f),
                    spawned + seed,
                    preferYellow);
                spawned++;
            }
        }

        private static void SpawnFlowerCluster(Transform parent, Vector3 position, float yaw, float scale, int seed, bool preferYellow)
        {
            GameObject root = new GameObject("GroundFlowerCluster_" + seed);
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            root.transform.localScale = Vector3.one * scale;

            CreateFlowerStem(root.transform, new Vector3(0f, 0.18f, 0f), 0.36f);
            CreateFlowerHead(root.transform, new Vector3(0f, 0.4f, 0f), PickFlowerMaterial(seed, preferYellow), 0.24f);
            CreateLeaf(root.transform, new Vector3(-0.12f, 0.17f, 0.05f), Quaternion.Euler(0f, 0f, 28f), 0.26f);
            CreateLeaf(root.transform, new Vector3(0.13f, 0.19f, -0.03f), Quaternion.Euler(0f, 0f, -28f), 0.24f);

            CreateFlowerStem(root.transform, new Vector3(0.2f, 0.15f, 0.16f), 0.3f);
            CreateFlowerHead(root.transform, new Vector3(0.2f, 0.34f, 0.16f), PickFlowerMaterial(seed + 1, preferYellow), 0.2f);
            CreateLeaf(root.transform, new Vector3(0.08f, 0.14f, 0.2f), Quaternion.Euler(0f, 38f, 30f), 0.2f);

            CreateFlowerStem(root.transform, new Vector3(-0.18f, 0.13f, -0.14f), 0.28f);
            CreateFlowerHead(root.transform, new Vector3(-0.18f, 0.31f, -0.14f), PickFlowerMaterial(seed + 2, preferYellow), 0.18f);
            CreateLeaf(root.transform, new Vector3(-0.26f, 0.13f, -0.03f), Quaternion.Euler(0f, -35f, -28f), 0.2f);

            if (seed % 3 == 0)
            {
                CreateFlowerStem(root.transform, new Vector3(0.02f, 0.12f, -0.24f), 0.26f);
                CreateFlowerHead(root.transform, new Vector3(0.02f, 0.29f, -0.24f), PickFlowerMaterial(seed + 3, preferYellow), 0.17f);
            }
        }

        private static void SpawnFlowerTrellises(Transform parent)
        {
            CreateFlowerTrellis(parent, "MarketFlowerTrellis_Front", new Vector3(20.5f, 0f, 91.5f), Quaternion.Euler(0f, 0f, 0f), 1.2f);
            CreateFlowerTrellis(parent, "MarketFlowerTrellis_Back", new Vector3(20.5f, 0f, 100.5f), Quaternion.Euler(0f, 0f, 0f), 1.2f);
        }

        private static void CreateFlowerTrellis(Transform parent, string name, Vector3 position, Quaternion rotation, float scale)
        {
            if (IsInWalkClearance(position))
            {
                return;
            }

            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = rotation;
            root.transform.localScale = Vector3.one * scale;

            CreateCube(root.transform, "Post_L", new Vector3(-1.85f, 1.05f, 0f), new Vector3(0.16f, 2.1f, 0.16f), woodMaterial);
            CreateCube(root.transform, "Post_R", new Vector3(1.85f, 1.05f, 0f), new Vector3(0.16f, 2.1f, 0.16f), woodMaterial);
            CreateCube(root.transform, "Beam_Top", new Vector3(0f, 2.1f, 0f), new Vector3(4.05f, 0.14f, 0.14f), woodMaterial);
            CreateCube(root.transform, "Beam_Mid", new Vector3(0f, 1.35f, 0f), new Vector3(3.75f, 0.1f, 0.1f), woodMaterial);
            CreateCube(root.transform, "Vine_Top", new Vector3(0f, 2.24f, 0f), new Vector3(3.85f, 0.1f, 0.1f), leafMaterial);
            CreateCube(root.transform, "Vine_Mid", new Vector3(0f, 1.52f, 0f), new Vector3(3.6f, 0.08f, 0.08f), leafMaterial);

            for (int i = 0; i < 5; i++)
            {
                float x = -1.45f + i * 0.72f;
                CreateCube(root.transform, "BambooSlat_" + i, new Vector3(x, 1.46f, 0f), new Vector3(0.08f, 1.2f, 0.08f), woodMaterial);
            }

            for (int i = 0; i < 19; i++)
            {
                float x = -1.75f + i * 0.195f;
                float y = 2.34f + ((i % 3) * 0.08f);
                CreateFlowerHead(root.transform, new Vector3(x, y, 0f), PickFlowerMaterial(i, preferYellow: true), 0.17f);
                CreateLeaf(root.transform, new Vector3(x + 0.04f, y - 0.16f, 0.04f), Quaternion.Euler(0f, 0f, i % 2 == 0 ? 35f : -35f), 0.2f);

                if (i % 3 == 0)
                {
                    CreateFlowerStem(root.transform, new Vector3(x, 1.82f, 0.02f), 0.48f);
                    CreateFlowerHead(root.transform, new Vector3(x, 1.55f, 0.02f), PickFlowerMaterial(i + 7, preferYellow: true), 0.14f);
                }
            }
        }

        private static void CreateFlowerStem(Transform parent, Vector3 localPosition, float height)
        {
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "FlowerStem";
            stem.transform.SetParent(parent, false);
            stem.transform.localPosition = localPosition;
            stem.transform.localScale = new Vector3(0.025f, height * 0.5f, 0.025f);
            ApplyMaterialAndStripCollider(stem, leafMaterial);
        }

        private static void CreateFlowerHead(Transform parent, Vector3 localPosition, Material material, float size)
        {
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "FlowerHead";
            head.transform.SetParent(parent, false);
            head.transform.localPosition = localPosition;
            head.transform.localScale = Vector3.one * size;
            ApplyMaterialAndStripCollider(head, material);
        }

        private static void CreateLeaf(Transform parent, Vector3 localPosition, Quaternion localRotation, float size)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.name = "FlowerLeaf";
            leaf.transform.SetParent(parent, false);
            leaf.transform.localPosition = localPosition;
            leaf.transform.localRotation = localRotation;
            leaf.transform.localScale = new Vector3(size, size * 0.32f, size * 0.52f);
            ApplyMaterialAndStripCollider(leaf, leafMaterial);
        }

        private static void CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;
            ApplyMaterialAndStripCollider(cube, material);
        }

        private static void ApplyMaterialAndStripCollider(GameObject instance, Material material)
        {
            Renderer renderer = instance.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = instance.GetComponent<Collider>();
            if (collider != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(collider);
                    return;
                }
#endif
                Object.Destroy(collider);
            }
        }

        private static Material PickFlowerMaterial(int seed, bool preferYellow = false)
        {
            if (preferYellow)
            {
                // Mostly yellow mai blooms, with a few accent colors.
                switch (Mathf.Abs(seed) % 6)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        return flowerYellowMaterial;
                    case 4:
                        return flowerPinkMaterial;
                    default:
                        return flowerRedMaterial;
                }
            }

            switch (Mathf.Abs(seed) % 4)
            {
                case 0: return flowerYellowMaterial;
                case 1: return flowerPinkMaterial;
                case 2: return flowerRedMaterial;
                default: return flowerPurpleMaterial;
            }
        }

        private static bool IsInWalkClearance(Vector3 position)
        {
            bool centralPath = Mathf.Abs(position.x) < 7f && position.z > 8f && position.z < 118f;
            // Keep the main aisle open, but allow flowers around the hoa mai stall (~x=12).
            bool marketCore = Mathf.Abs(position.x) < 9.5f && position.z > 78f && position.z < 104f;
            bool templePath = position.x > 48f && position.x < 62f && position.z > 88f && position.z < 146f;
            return centralPath || marketCore || templePath;
        }

        private static float NextRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static void DestroyChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(child.gameObject);
                return;
            }
#endif
            Object.Destroy(child.gameObject);
        }

        private static void DestroyObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(target);
                return;
            }
#endif
            Object.Destroy(target);
        }
    }
}
