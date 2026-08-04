using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Persists the boarded player's live visual so arrival scene uses the same character model.</summary>
    internal static class HometownArrivalPlayerVisualContext
    {
        private static GameObject capturedVisualTemplate;
        private static RuntimeAnimatorController capturedAnimatorController;

        public static void CaptureFromPlayer(PlayerTopDownController player)
        {
            if (player == null || player.visualRoot == null || player.visualRoot.childCount == 0)
            {
                return;
            }

            Transform sourceVisual = player.visualRoot.GetChild(0);
            if (sourceVisual == null)
            {
                return;
            }

            if (capturedVisualTemplate != null)
            {
                Object.Destroy(capturedVisualTemplate);
            }

            capturedVisualTemplate = Object.Instantiate(sourceVisual.gameObject);
            capturedVisualTemplate.name = sourceVisual.gameObject.name;
            capturedVisualTemplate.SetActive(false);
            Object.DontDestroyOnLoad(capturedVisualTemplate);

            // Clone materials so DDOL / scene unload cannot null shared asset refs.
            BakeDurableMaterials(capturedVisualTemplate);

            Animator sourceAnimator = sourceVisual.GetComponent<Animator>();
            if (sourceAnimator == null)
            {
                sourceAnimator = sourceVisual.GetComponentInChildren<Animator>(true);
            }

            capturedAnimatorController = sourceAnimator != null ? sourceAnimator.runtimeAnimatorController : null;
        }

        public static void ApplyToPlayer(PlayerTopDownController player)
        {
            if (player == null || player.visualRoot == null)
            {
                return;
            }

            for (int i = player.visualRoot.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(player.visualRoot.GetChild(i).gameObject);
            }

            GameObject visual = null;
            if (capturedVisualTemplate != null)
            {
                visual = Object.Instantiate(capturedVisualTemplate, player.visualRoot);
                visual.name = capturedVisualTemplate.name;
            }
            else
            {
                visual = TrySpawnCreativeCharacterFallback(player.visualRoot);
            }

            if (visual == null)
            {
                return;
            }

            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.SetActive(true);
            BakeDurableMaterials(visual);

            if (capturedAnimatorController != null && player.fallbackAnimatorController == null)
            {
                player.fallbackAnimatorController = capturedAnimatorController;
            }

            player.ForceRealignVisual();
        }

        public static void EnsureVisibleRepairedPlayer(PlayerTopDownController player)
        {
            if (player == null)
            {
                return;
            }

            player.ForceHideCharacterVisual = false;
            player.SetCharacterVisible(true);

            if (player.visualRoot == null || player.visualRoot.childCount == 0
                || !HasRenderableSkins(player.visualRoot))
            {
                ApplyToPlayer(player);
            }

            HometownArrivalCharacterMaterialRepair.RepairPlayer(player);
            player.ForceRealignVisual();
        }

        private static bool HasRenderableSkins(Transform visualRoot)
        {
            SkinnedMeshRenderer[] skins = visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skins.Length; i++)
            {
                if (skins[i] != null && skins[i].enabled && skins[i].sharedMesh != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static GameObject TrySpawnCreativeCharacterFallback(Transform parent)
        {
            GameObject prefab = HometownArrivalResourceCache.LoadPrefab(
                PlayerCharacterVisualPaths.CreativeCharacterVisualPrefab);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("Characters/CreativeCharacter_PlayerVisual");
            }

            if (prefab == null)
            {
                return null;
            }

            GameObject visual = Object.Instantiate(prefab, parent);
            visual.name = PlayerCharacterVisualPaths.VisualChildName;
            return visual;
        }

        private static void BakeDurableMaterials(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Material body = ResolveBodyMaterial();
            Material pants = ResolvePantsMaterial();

            SkinnedMeshRenderer[] skins = visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skins.Length; i++)
            {
                if (skins[i] == null)
                {
                    continue;
                }

                Material chosen = skins[i].name.StartsWith("Pants", System.StringComparison.Ordinal)
                    ? pants
                    : body;
                CloneAssign(skins[i], chosen);
            }

            MeshRenderer[] meshes = visual.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] == null
                    || meshes[i].name.IndexOf("Shadow", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                Material chosen = meshes[i].name.StartsWith("Pants", System.StringComparison.Ordinal)
                    ? pants
                    : body;
                CloneAssign(meshes[i], chosen);
            }
        }

        private static void CloneAssign(Renderer renderer, Material source)
        {
            if (renderer == null || source == null)
            {
                return;
            }

            Material[] mats = renderer.sharedMaterials;
            if (mats == null || mats.Length == 0)
            {
                renderer.sharedMaterial = new Material(source);
                return;
            }

            bool needsReplace = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (IsBrokenOrPalette(mats[i]))
                {
                    needsReplace = true;
                    break;
                }
            }

            // Always clone so DDOL keeps private instances.
            Material[] clones = new Material[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                Material src = needsReplace || IsBrokenOrPalette(mats[i]) ? source : mats[i];
                clones[i] = src != null ? new Material(src) : new Material(source);
                clones[i].name = (src != null ? src.name : source.name) + "_Runtime";
            }

            renderer.sharedMaterials = clones;
        }

        private static bool IsBrokenOrPalette(Material material)
        {
            if (material == null || material.shader == null)
            {
                return true;
            }

            string shaderName = material.shader.name;
            string matName = material.name;
            return !material.shader.isSupported
                   || shaderName.Contains("Hidden/InternalErrorShader")
                   || shaderName == "Standard"
                   || shaderName.Contains("VertexColorLit")
                   || matName.IndexOf("Explorer_Palette", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Material ResolveBodyMaterial()
        {
            Material teal = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.PlayerTealShirtMaterial);
            if (teal != null)
            {
                return teal;
            }

            Material pack = HometownArrivalResourceCache.LoadMaterialAtPath(
                "Assets/ithappy/Creative_Characters_FREE/Materials/Color.mat");
            return pack != null ? pack : CreateUrp(new Color(0.28f, 0.55f, 0.58f), "PlayerBody_Runtime");
        }

        private static Material ResolvePantsMaterial()
        {
            Material pants = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.PlayerDarkPantsMaterial);
            if (pants != null)
            {
                return pants;
            }

            return CreateUrp(new Color(0.16f, 0.18f, 0.22f), "PlayerPants_Runtime");
        }

        private static Material CreateUrp(Color color, string name)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
            {
                return null;
            }

            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }

            return mat;
        }
    }
}
