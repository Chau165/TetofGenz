using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Highway
{
    /// <summary>
    /// Highway river using LowPolyWater_Pack (Ocean mesh + LowPolyWater waves).
    /// Material is a saved URP asset (Built-in pack shader is pink in URP).
    /// </summary>
    public static class HighwayCountrysideRiverBuilder
    {
        public const float WaterSurfaceY = -14f;

        private const string PackMaterialPath =
            "Assets/LowPolyWater_Pack/Materials/LowPolyWaterMaterial.mat";
        private const string PackOceanMeshPath =
            "Assets/LowPolyWater_Pack/Plane Meshes/Ocean50x50W750H750.asset";
        private const float PackOceanSize = 750f;

        private static Material cachedWaterMaterial;
        private static Mesh cachedOceanSource;

        /// <summary>
        /// Persistent pack oceans (L/R/Under) under the elevated road. Call once from scenery pool.
        /// </summary>
        public static Transform EnsurePackOcean(Transform parent, Material preferredWaterMaterial = null)
        {
            const string rootName = "PackLowPolyWater";

            // Remove every leftover ocean root (duplicates from prior rebuilds → null mats / magenta).
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child != null && (child.name == rootName || child.name.StartsWith(rootName)))
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }

            // Orphan ocean tiles left elsewhere in the scenery hierarchy.
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                string n = child.name;
                if (n.StartsWith("Ocean_L") || n.StartsWith("Ocean_R") || n.StartsWith("Ocean_Under"))
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }

            GameObject root = new GameObject(rootName);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, WaterSurfaceY, 0f);

            Material mat = ResolveWaterMaterial(preferredWaterMaterial);
            float coverX = HighwayRoadLayout.ShoulderOuterX + 90f;
            float coverZ = 220f;

            BuildPackOceanTile(root.transform, "Ocean_L", mat,
                new Vector3(-coverX * 0.55f, 0f, coverZ * 0.35f),
                coverX, coverZ, 3);
            BuildPackOceanTile(root.transform, "Ocean_R", mat,
                new Vector3(coverX * 0.55f, 0f, coverZ * 0.35f),
                coverX, coverZ, 7);
            BuildPackOceanTile(root.transform, "Ocean_Under", mat,
                new Vector3(0f, -0.05f, coverZ * 0.35f),
                HighwayRoadLayout.ShoulderOuterX * 2.8f + 10f,
                coverZ * 0.95f,
                11);

            return root.transform;
        }

        public static void FollowCamera(Transform packOceanRoot, float cameraWorldZ)
        {
            if (packOceanRoot == null)
            {
                return;
            }

            Vector3 p = packOceanRoot.position;
            p.y = WaterSurfaceY;
            p.z = cameraWorldZ + 35f;
            packOceanRoot.position = p;
        }

        /// <summary>Segment decoration only (shores / grass) — water is the shared pack ocean.</summary>
        public static void BuildFullRiver(
            Transform root,
            float segmentLength,
            int seed,
            Material waterMaterial,
            Material bankMaterial,
            HighwayNaturePrefabLibrary library,
            HighwayNatureClusterBuilder.Palette palette)
        {
            HighwayPropUtility.ClearChildren(root);
            float centerZ = segmentLength * 0.5f;
            float length = segmentLength + 1.2f;
            float outer = HighwayRoadLayout.ShoulderOuterX;
            float edgeY = WaterSurfaceY + 0.08f;

            BuildSlab(root, "Shore_L",
                bankMaterial,
                new Vector3(-outer - 0.9f, edgeY, centerZ),
                new Vector3(1.4f, 0.1f, length * 0.7f));
            BuildSlab(root, "Shore_R",
                bankMaterial,
                new Vector3(outer + 0.9f, edgeY, centerZ),
                new Vector3(1.4f, 0.1f, length * 0.7f));

            if (seed % 3 == 0 && library != null && palette.GrassBladeA != null)
            {
                PlaceEdgeGrass(root, true, outer, centerZ, seed, library, palette);
            }

            if (seed % 4 == 0 && library != null && palette.GrassBladeA != null)
            {
                PlaceEdgeGrass(root, false, outer, centerZ, seed + 7, library, palette);
            }
        }

        public static void Build(
            Transform root,
            bool leftSide,
            float segmentLength,
            int seed,
            Material waterMaterial,
            Material bankMaterial,
            HighwayNaturePrefabLibrary library,
            HighwayNatureClusterBuilder.Palette palette)
        {
            BuildFullRiver(root, segmentLength, seed, waterMaterial, bankMaterial, library, palette);
        }

        private static void PlaceEdgeGrass(
            Transform parent,
            bool leftSide,
            float outer,
            float centerZ,
            int seed,
            HighwayNaturePrefabLibrary library,
            HighwayNatureClusterBuilder.Palette palette)
        {
            GameObject tuft = new GameObject(leftSide ? "EdgeGrass_L" : "EdgeGrass_R");
            tuft.transform.SetParent(parent, false);
            float sign = leftSide ? -1f : 1f;
            tuft.transform.localPosition = new Vector3(sign * (outer + 1.4f), WaterSurfaceY + 0.12f, centerZ);
            HighwayNatureClusterBuilder.Build(
                tuft.transform,
                HighwayNatureClusterBuilder.ClusterKind.GrassPatch,
                library,
                palette,
                seed);
        }

        private static void BuildPackOceanTile(
            Transform parent,
            string name,
            Material mat,
            Vector3 localPos,
            float width,
            float length,
            int seed)
        {
            Mesh source = LoadPackOceanMesh();
            if (source == null || mat == null)
            {
                Debug.LogError("[HighwayCountrysideRiverBuilder] Missing pack ocean mesh or water material.");
                return;
            }

            GameObject tile = new GameObject(name);
            tile.transform.SetParent(parent, false);
            tile.transform.localPosition = localPos;
            tile.transform.localRotation = Quaternion.identity;
            tile.transform.localScale = new Vector3(
                width / PackOceanSize,
                1f,
                length / PackOceanSize);

            var filter = tile.AddComponent<MeshFilter>();
            Mesh instanceMesh = Object.Instantiate(source);
            instanceMesh.name = "PackOcean_" + name;
            Bounds b = instanceMesh.bounds;
            b.Expand(new Vector3(1f, 5f, 1f));
            instanceMesh.bounds = b;
            filter.sharedMesh = instanceMesh;

            var renderer = tile.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            renderer.allowOcclusionWhenDynamic = false;

            var waves = tile.AddComponent<LowPolyWater.LowPolyWater>();
            waves.waveHeight = 0.45f + (seed % 5) * 0.04f;
            waves.waveFrequency = 0.35f;
            waves.waveLength = 0.75f;
            waves.waveOriginPosition = Vector3.zero;
        }

        private static Mesh LoadPackOceanMesh()
        {
            if (cachedOceanSource != null)
            {
                return cachedOceanSource;
            }

#if UNITY_EDITOR
            cachedOceanSource = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(PackOceanMeshPath);
#endif
            return cachedOceanSource;
        }

        private static Material ResolveWaterMaterial(Material preferred)
        {
            if (cachedWaterMaterial != null)
            {
                return cachedWaterMaterial;
            }

#if UNITY_EDITOR
            string assetPath = HighwaySceneAssetPaths.MaterialPath(HighwaySceneAssetPaths.PackWaterMat);
            Material saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (saved != null && saved.shader != null &&
                saved.shader.name.IndexOf("Universal", System.StringComparison.Ordinal) >= 0)
            {
                cachedWaterMaterial = saved;
                return cachedWaterMaterial;
            }

            cachedWaterMaterial = CreateOrUpdatePackWaterAsset(assetPath);
            return cachedWaterMaterial;
#else
            if (preferred != null && preferred.shader != null &&
                preferred.shader.name.IndexOf("Universal", System.StringComparison.Ordinal) >= 0)
            {
                return preferred;
            }

            cachedWaterMaterial = CreateRuntimeUrpWater();
            return cachedWaterMaterial;
#endif
        }

        /// <summary>Editor: ensure the saved URP pack-water material exists and return it.</summary>
        public static Material EnsurePackWaterMaterialAsset()
        {
#if UNITY_EDITOR
            cachedWaterMaterial = null;
            return ResolveWaterMaterial(null);
#else
            return ResolveWaterMaterial(null);
#endif
        }

#if UNITY_EDITOR
        private static Material CreateOrUpdatePackWaterAsset(string assetPath)
        {
            Color packColor = new Color(0.37f, 0.78f, 0.82f, 1f);
            Material pack = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(PackMaterialPath);
            if (pack != null && pack.HasProperty("_BaseColor"))
            {
                packColor = pack.GetColor("_BaseColor");
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            }

            Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                string folder = HighwaySceneAssetPaths.MaterialsFolder;
                if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
                {
                    HighwaySceneMaterialFactoryEnsureFolder(folder);
                }

                mat = new Material(shader);
                UnityEditor.AssetDatabase.CreateAsset(mat, assetPath);
            }
            else
            {
                mat.shader = shader;
            }

            Color surface = new Color(
                packColor.r * 0.6f,
                packColor.g * 0.75f,
                packColor.b * 0.82f,
                1f);

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", surface);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", surface);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.28f);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0.04f);
            }

            mat.name = "Highway_PackWater_URP";
            UnityEditor.EditorUtility.SetDirty(mat);
            UnityEditor.AssetDatabase.SaveAssets();
            return mat;
        }

        private static void HighwaySceneMaterialFactoryEnsureFolder(string folder)
        {
            if (UnityEditor.AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!UnityEditor.AssetDatabase.IsValidFolder(next))
                {
                    UnityEditor.AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
#endif

        private static Material CreateRuntimeUrpWater()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Universal Render Pipeline/Simple Lit");
            var mat = new Material(shader)
            {
                name = "Highway_PackWater_Runtime"
            };
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", new Color(0.22f, 0.55f, 0.62f, 1f));
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0.28f);
            }

            return mat;
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
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }
        }
    }
}
