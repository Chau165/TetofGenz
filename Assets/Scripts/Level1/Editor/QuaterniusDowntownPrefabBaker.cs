#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using ChoTet.Level1.Highway;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    public static class QuaterniusDowntownPrefabBaker
    {
        public static void BuildKit()
        {
            EnsureFolder("Assets/Prefabs/Highway");
            EnsureFolder("Assets/Prefabs/Highway/QuaterniusDowntown");
            EnsureFolder(QuaterniusDowntownAssets.ModulePrefabRoot);
            EnsureFolder(QuaterniusDowntownAssets.BlockPrefabRoot);
            EnsureFolder("Assets/Settings/ChoTet");
            EnsureFolder("Assets/Materials/Highway");
            EnsureFolder(QuaterniusDowntownAssets.SidewalkDecorPrefabRoot);

            BakeModulePrefabs();
            BakeDecorPrefabs();
            QuaterniusDowntownPrefabLibrary library = BuildLibraryAsset();
            QuaterniusCompleteBuildingCatalog.ClearCache();
            BakeBlockVariants(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                "[QuaterniusDowntownPrefabBaker] Validated "
                + (library.completeBuildings?.Length ?? 0)
                + " complete building prefabs.");
        }

        private static void BakeModulePrefabs()
        {
            string[] gltfFiles = Directory.GetFiles(QuaterniusDowntownAssets.GltfRoot, "*.gltf");
            for (int i = 0; i < gltfFiles.Length; i++)
            {
                string projectPath = gltfFiles[i].Replace('\\', '/');
                int assetsIndex = projectPath.IndexOf("Assets/");
                if (assetsIndex >= 0)
                {
                    projectPath = projectPath.Substring(assetsIndex);
                }

                AssetDatabase.ImportAsset(projectPath, ImportAssetOptions.ForceUpdate);
                GameObject source = LoadGltfRoot(projectPath);
                if (source == null)
                {
                    continue;
                }

                string fileName = Path.GetFileNameWithoutExtension(projectPath);
                string prefabPath = QuaterniusDowntownAssets.ModulePrefabRoot + "/" + fileName + ".prefab";
                GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(source);
                }

                instance.name = fileName;
                HighwayPropUtility.StripColliders(instance);
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);
            }
        }

        private static QuaterniusDowntownPrefabLibrary BuildLibraryAsset()
        {
            QuaterniusDowntownPrefabLibrary library =
                AssetDatabase.LoadAssetAtPath<QuaterniusDowntownPrefabLibrary>(QuaterniusDowntownAssets.LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<QuaterniusDowntownPrefabLibrary>();
                AssetDatabase.CreateAsset(library, QuaterniusDowntownAssets.LibraryPath);
            }

            var validated = new List<GameObject>();
            AddValidatedPrefabs(validated, LoadAllCompleteBuildingPrefabs());

            library.completeBuildings = validated.ToArray();
            library.sidewalkPavementMaterial = HighwaySidewalkMaterialFactory.BakeOrUpdateMaterialAsset();
            library.trees = LoadModulePrefabs(new[] { "Prop_Planter_Single", "Sidewalk_Planter" });
            library.utilityPoles = LoadModulePrefabs(new[] { "Prop_Bollard" });
            library.streetLights = System.Array.Empty<GameObject>();
            library.trafficSigns = LoadModulePrefabs(new[] { "Decal_Stop", "Decal_Slow", "Decal_Only" });
            AssignSidewalkDecor(library);

            EditorUtility.SetDirty(library);
            return library;
        }

        private static void AssignSidewalkDecor(QuaterniusDowntownPrefabLibrary library)
        {
            library.sidewalkPlanters = LoadModulePrefabs(new[] { "Sidewalk_Planter" });
            library.sidewalkTrees = LoadModulePrefabs(new[] { "Sidewalk_Planter" });
            library.sidewalkBollards = LoadModulePrefabs(new[] { "Prop_Bollard" });
            library.sidewalkGroundDetails = LoadModulePrefabs(new[] { "Prop_ManholeCover", "Prop_Drain" });
            library.sidewalkBins = LoadDecorPrefabs(new[] { "Decor_TrashBin" });
            library.sidewalkStreetLights = LoadDecorPrefabs(new[] { "Decor_StreetLight" });
            library.sidewalkBusStops = LoadDecorPrefabs(new[] { "Decor_BusStop" });
        }

        private static void BakeDecorPrefabs()
        {
            BakeTrashBinPrefab();
            BakeStreetLightPrefab();
            BakeBusStopPrefab();
        }

        private static void BakeTrashBinPrefab()
        {
            BakeCompositePrefab("Decor_TrashBin", root =>
            {
                AddModuleChild(root, "Prop_ACUnit", Vector3.zero, Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f));
            });
        }

        private static void BakeStreetLightPrefab()
        {
            BakeCompositePrefab("Decor_StreetLight", root =>
            {
                AddModuleChild(root, "Metal_Column_Small_Bottom", Vector3.zero, Vector3.zero);
                AddModuleChild(root, "Metal_Column_Small_Center", new Vector3(0f, 1.05f, 0f), Vector3.zero);
                AddModuleChild(root, "Metal_Column_Small_Top", new Vector3(0f, 2.1f, 0f), Vector3.zero);
                AddModuleChild(
                    root,
                    "Trim_Wall_Guard",
                    new Vector3(0f, 2.35f, 0.35f),
                    new Vector3(0f, 90f, 0f),
                    new Vector3(0.75f, 0.75f, 0.75f));
            });
        }

        private static void BakeBusStopPrefab()
        {
            BakeCompositePrefab("Decor_BusStop", root =>
            {
                AddModuleChild(
                    root,
                    "Entrance_Concrete_2x1",
                    new Vector3(0f, 0f, -0.55f),
                    new Vector3(0f, 180f, 0f));
                AddModuleChild(root, "Metal_Column_Small_Bottom", new Vector3(-0.85f, 0f, 0.15f), Vector3.zero);
                AddModuleChild(root, "Metal_Column_Small_Center", new Vector3(-0.85f, 1.05f, 0.15f), Vector3.zero);
                AddModuleChild(root, "Metal_Column_Small_Top", new Vector3(-0.85f, 2.1f, 0.15f), Vector3.zero);
                AddModuleChild(root, "Metal_Column_Small_Bottom", new Vector3(0.85f, 0f, 0.15f), Vector3.zero);
                AddModuleChild(root, "Metal_Column_Small_Center", new Vector3(0.85f, 1.05f, 0.15f), Vector3.zero);
                AddModuleChild(root, "Metal_Column_Small_Top", new Vector3(0.85f, 2.1f, 0.15f), Vector3.zero);
                AddModuleChild(
                    root,
                    "Stairs_Rails_Metal_Straight_1",
                    new Vector3(0f, 0f, 0.35f),
                    new Vector3(0f, 90f, 0f));
                AddModuleChild(
                    root,
                    "Decal_Stop",
                    new Vector3(0f, 1.55f, -0.48f),
                    new Vector3(0f, 180f, 0f),
                    new Vector3(0.6f, 0.6f, 0.6f));
            });
        }

        private static GameObject BakeCompositePrefab(string prefabName, System.Action<GameObject> build)
        {
            var root = new GameObject(prefabName);
            build(root);
            HighwayPropUtility.StripColliders(root);

            string path = QuaterniusDowntownAssets.SidewalkDecorPrefabRoot + "/" + prefabName + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AddModuleChild(
            GameObject parent,
            string moduleName,
            Vector3 localPos,
            Vector3 localRotEuler,
            Vector3? localScale = null)
        {
            GameObject module = LoadModulePrefab(moduleName);
            if (module == null)
            {
                return;
            }

            GameObject child = PrefabUtility.InstantiatePrefab(module) as GameObject;
            if (child == null)
            {
                child = Object.Instantiate(module);
            }

            child.name = moduleName;
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = localPos;
            child.transform.localRotation = Quaternion.Euler(localRotEuler);
            child.transform.localScale = localScale ?? Vector3.one;
            HighwayPropUtility.StripColliders(child);
        }

        private static GameObject[] LoadDecorPrefabs(string[] names)
        {
            var list = new List<GameObject>();
            for (int i = 0; i < names.Length; i++)
            {
                string path = QuaterniusDowntownAssets.SidewalkDecorPrefabRoot + "/" + names[i] + ".prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    list.Add(prefab);
                }
            }

            return list.ToArray();
        }

        private static void AddValidatedPrefabs(List<GameObject> list, GameObject[] candidates)
        {
            if (candidates == null)
            {
                return;
            }

            for (int i = 0; i < candidates.Length; i++)
            {
                GameObject prefab = candidates[i];
                if (prefab == null || list.Contains(prefab))
                {
                    continue;
                }

                if (QuaterniusCompleteBuildingValidator.IsValidPrefabAsset(prefab))
                {
                    list.Add(prefab);
                }
            }
        }

        private static void BakeBlockVariants(QuaterniusDowntownPrefabLibrary library)
        {
            if (library == null || library.completeBuildings == null
                || library.completeBuildings.Length == 0)
            {
                return;
            }

            var leftVariants = new List<GameObject>();
            var rightVariants = new List<GameObject>();
            float blockLength = HighwayRoadLayout.SegmentLength;

            for (int i = 0; i < 6; i++)
            {
                int seed = i * 97 + 17;
                GameObject leftBlock = QuaterniusTownBlockAssembler.BuildBlock(
                    library, true, seed, blockLength);
                if (leftBlock != null)
                {
                    string leftPath = QuaterniusDowntownAssets.BlockPrefabRoot + "/Block_Left_" + i + ".prefab";
                    leftVariants.Add(SaveBlockPrefab(leftBlock, leftPath));
                }

                GameObject rightBlock = QuaterniusTownBlockAssembler.BuildBlock(
                    library, false, seed + 53, blockLength);
                if (rightBlock != null)
                {
                    string rightPath = QuaterniusDowntownAssets.BlockPrefabRoot + "/Block_Right_" + i + ".prefab";
                    rightVariants.Add(SaveBlockPrefab(rightBlock, rightPath));
                }
            }

            library.leftBlockVariants = leftVariants.ToArray();
            library.rightBlockVariants = rightVariants.ToArray();
            EditorUtility.SetDirty(library);
        }

        private static GameObject SaveBlockPrefab(GameObject block, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(block, path);
            Object.DestroyImmediate(block);
            return prefab;
        }

        private static GameObject[] LoadAllCompleteBuildingPrefabs()
        {
            var list = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { QuaterniusDowntownAssets.ModulePrefabRoot });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (!fileName.StartsWith("Building_", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    list.Add(prefab);
                }
            }

            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list.ToArray();
        }

        private static GameObject LoadModulePrefab(string name)
        {
            string path = QuaterniusDowntownAssets.ModulePrefabRoot + "/" + name + ".prefab";
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject[] LoadModulePrefabs(string[] names)
        {
            var list = new List<GameObject>();
            for (int i = 0; i < names.Length; i++)
            {
                string path = QuaterniusDowntownAssets.ModulePrefabRoot + "/" + names[i] + ".prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    list.Add(prefab);
                }
            }

            return list.ToArray();
        }

        private static GameObject LoadGltfRoot(string assetPath)
        {
            GameObject direct = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (direct != null)
            {
                return direct;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is GameObject go)
                {
                    return go;
                }
            }

            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
