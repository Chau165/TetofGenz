#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Creates URP materials and traffic prefabs from Kenney Car Kit FBX models.</summary>
    public static class Level1KenneyCarKitTrafficBuilder
    {
        private static readonly (string fbx, string prefab)[] TrafficModels =
        {
            ("sedan.fbx", Level1KenneyCarKitPaths.SedanPrefab),
            ("sedan-sports.fbx", Level1KenneyCarKitPaths.SedanSportsPrefab),
            ("hatchback-sports.fbx", Level1KenneyCarKitPaths.HatchbackSportsPrefab),
            ("suv.fbx", Level1KenneyCarKitPaths.SuvPrefab),
            ("suv-luxury.fbx", Level1KenneyCarKitPaths.SuvLuxuryPrefab),
            ("taxi.fbx", Level1KenneyCarKitPaths.TaxiPrefab),
            ("van.fbx", Level1KenneyCarKitPaths.VanPrefab),
            ("delivery.fbx", Level1KenneyCarKitPaths.DeliveryPrefab),
            ("delivery-flat.fbx", Level1KenneyCarKitPaths.DeliveryFlatPrefab),
            ("police.fbx", Level1KenneyCarKitPaths.PolicePrefab),
            ("race.fbx", Level1KenneyCarKitPaths.RacePrefab),
            ("race-future.fbx", Level1KenneyCarKitPaths.RaceFuturePrefab),
            ("kart-oodi.fbx", Level1KenneyCarKitPaths.KartPrefab),
            ("tractor.fbx", Level1KenneyCarKitPaths.TractorPrefab),
            ("tractor-shovel.fbx", Level1KenneyCarKitPaths.TractorShovelPrefab),
            ("truck.fbx", Level1KenneyCarKitPaths.TruckPrefab),
            ("truck-flat.fbx", Level1KenneyCarKitPaths.TruckFlatPrefab)
        };

        public static void BuildPrefabsMenu()
        {
            EnsureTrafficPrefabs(forceRebuild: true);
            AssetDatabase.SaveAssets();
            Debug.Log("[Level1KenneyCarKitTrafficBuilder] Kenney traffic prefabs rebuilt (" + TrafficModels.Length + " types).");
        }

        public static void EnsureTrafficPrefabs(bool forceRebuild = false)
        {
            Material colormapMaterial = EnsureColormapMaterial();
            EnsureFolder(Level1KenneyCarKitPaths.PrefabRoot);

            for (int i = 0; i < TrafficModels.Length; i++)
            {
                (string fbx, string prefabPath) = TrafficModels[i];
                string fbxPath = Level1KenneyCarKitPaths.FbxRoot + "/" + fbx;
                if (forceRebuild || AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                {
                    CreateTrafficPrefab(fbxPath, prefabPath, colormapMaterial);
                }
            }
        }

        public static GameObject[] LoadAllTrafficPrefabs()
        {
            string[] paths = Level1KenneyCarKitPaths.TrafficPrefabPaths;
            GameObject[] prefabs = new GameObject[paths.Length];
            int count = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (prefab != null)
                {
                    prefabs[count++] = prefab;
                }
            }

            if (count == paths.Length)
            {
                return prefabs;
            }

            GameObject[] trimmed = new GameObject[count];
            for (int i = 0; i < count; i++)
            {
                trimmed[i] = prefabs[i];
            }

            return trimmed;
        }

        public static Material EnsureColormapMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(Level1KenneyCarKitPaths.ColormapMaterial);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder(Level1KenneyCarKitPaths.MaterialRoot);

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[Level1KenneyCarKitTrafficBuilder] URP Lit shader not found.");
                return null;
            }

            Texture2D colormap = AssetDatabase.LoadAssetAtPath<Texture2D>(Level1KenneyCarKitPaths.ColormapTexture);
            Material material = new Material(urpLit)
            {
                name = "KenneyCar_Colormap_URP"
            };

            if (colormap != null)
            {
                material.SetTexture("_BaseMap", colormap);
                material.SetTexture("_MainTex", colormap);
            }

            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.35f);
            material.SetFloat("_Metallic", 0f);
            material.enableInstancing = true;

            AssetDatabase.CreateAsset(material, Level1KenneyCarKitPaths.ColormapMaterial);
            return material;
        }

        private static void CreateTrafficPrefab(string fbxPath, string prefabPath, Material colormapMaterial)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (source == null)
            {
                Debug.LogWarning("[Level1KenneyCarKitTrafficBuilder] Missing FBX: " + fbxPath);
                return;
            }

            GameObject instance = Object.Instantiate(source);
            instance.name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;

            ApplyColormapMaterial(instance, colormapMaterial);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }

        public static void ApplyColormapMaterial(GameObject root, Material colormapMaterial)
        {
            if (root == null || colormapMaterial == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] slots = renderer.sharedMaterials;
                if (slots == null || slots.Length == 0)
                {
                    renderer.sharedMaterial = colormapMaterial;
                    continue;
                }

                for (int slot = 0; slot < slots.Length; slot++)
                {
                    slots[slot] = colormapMaterial;
                }

                renderer.sharedMaterials = slots;
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
