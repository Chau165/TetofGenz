#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    public static class HighwayNatureStarterKitBaker
    {
        private const string Nsk2Root = "Assets/NatureStarterKit2/Nature";
        private const string BakeRoot = "Assets/Prefabs/Highway/NatureBaked";
        private const string LibraryPath = "Assets/Settings/ChoTet/HighwayNaturePrefabLibrary.asset";
        private const string SharedBladeMeshPath = BakeRoot + "/GrassBlade_Shared.asset";

        private static readonly string[] BushFiles =
        {
            "bush01.prefab", "bush02.prefab", "bush03.prefab",
            "bush04.prefab", "bush05.prefab", "bush06.prefab"
        };

        private static readonly string[] TreeFiles =
        {
            "tree01.prefab", "tree02.prefab", "tree03.prefab", "tree04.prefab"
        };

        public static void BakeVegetation()
        {
            EnsureFolder(BakeRoot);

            Material foliage = HighwayNatureMaterialFactory.LoadOrCreate(
                HighwayNatureMaterialDefaults.FoliageFile,
                "Assets/NatureStarterKit2/Textures/bush01.tga",
                new Color(0.55f, 0.72f, 0.42f),
                Vector2.one);
            Material bark = HighwayNatureMaterialFactory.LoadOrCreate(
                HighwayNatureMaterialDefaults.BarkFile,
                "Assets/NatureStarterKit2/Textures/bark01.tga",
                new Color(0.62f, 0.52f, 0.4f),
                new Vector2(1f, 2f));

            HighwayNatureMaterialFactory.LoadOrCreateGrassBlade(
                HighwayNatureMaterialDefaults.GrassBlade01File,
                "Assets/NatureStarterKit2/Textures/grass01.tga",
                Color.white);
            HighwayNatureMaterialFactory.LoadOrCreateGrassBlade(
                HighwayNatureMaterialDefaults.GrassBlade02File,
                "Assets/NatureStarterKit2/Textures/grass02.tga",
                new Color(0.92f, 1f, 0.88f));
            HighwayNatureMaterialFactory.LoadOrCreate(
                HighwayNatureMaterialDefaults.GroundEarthFile,
                "Assets/NatureStarterKit2/Textures/ground01.tga",
                new Color(0.48f, 0.44f, 0.34f),
                new Vector2(12f, 10f));

            EnsureSharedBladeMesh();

            var bushes = new List<GameObject>();
            var trees = new List<GameObject>();

            for (int i = 0; i < BushFiles.Length; i++)
            {
                GameObject baked = BakeMeshPrefab(Nsk2Root + "/" + BushFiles[i], foliage, bark, true);
                if (baked != null)
                {
                    bushes.Add(baked);
                }
            }

            for (int i = 0; i < TreeFiles.Length; i++)
            {
                GameObject baked = BakeMeshPrefab(Nsk2Root + "/" + TreeFiles[i], foliage, bark, true);
                if (baked != null)
                {
                    trees.Add(baked);
                }
            }

            SaveLibrary(System.Array.Empty<GameObject>(), bushes.ToArray(), trees.ToArray());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HighwayNatureStarterKitBaker] Baked "
                        + bushes.Count + " bushes, "
                        + trees.Count + " trees. Grass uses runtime NSK2 blade meshes.");
        }

        private static Mesh EnsureSharedBladeMesh()
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(SharedBladeMeshPath);
            if (mesh != null)
            {
                return mesh;
            }

            mesh = HighwayNatureGrassTuftMesh.CreateBladeMesh(1f, 0.5f);
            AssetDatabase.CreateAsset(mesh, SharedBladeMeshPath);
            return mesh;
        }

        private static GameObject BakeMeshPrefab(string sourcePath, Material foliage, Material bark, bool addWind)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null)
            {
                return null;
            }

            Mesh mesh = FindEmbeddedMesh(sourcePath);
            if (mesh == null)
            {
                Debug.LogWarning("[HighwayNatureStarterKitBaker] No mesh in " + sourcePath);
                return null;
            }

            string name = Path.GetFileNameWithoutExtension(sourcePath);
            string outPath = BakeRoot + "/" + name + "_Baked.prefab";

            GameObject root = new GameObject(name + "_Baked");
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            MeshFilter filter = visual.AddComponent<MeshFilter>();
            MeshRenderer renderer = visual.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;

            Material[] mats = mesh.subMeshCount > 1
                ? new[] { bark, foliage }
                : new[] { foliage };
            renderer.sharedMaterials = mats;

            if (addWind)
            {
                visual.AddComponent<HighwayNatureWindSway>();
            }

            HighwayPropUtility.StripColliders(root);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, outPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Mesh FindEmbeddedMesh(string assetPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            Mesh best = null;
            int bestVerts = 0;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Mesh mesh && mesh.vertexCount > bestVerts)
                {
                    best = mesh;
                    bestVerts = mesh.vertexCount;
                }
            }

            return best;
        }

        private static void SaveLibrary(GameObject[] grass, GameObject[] bushes, GameObject[] trees)
        {
            HighwayNaturePrefabLibrary library =
                AssetDatabase.LoadAssetAtPath<HighwayNaturePrefabLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<HighwayNaturePrefabLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            library.grassTufts = grass;
            library.bushes = bushes;
            library.trees = trees;
            EditorUtility.SetDirty(library);
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
                string leaf = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(parent))
                {
                    EnsureFolder(parent);
                }

                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}
#endif
