using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// Repairs null / broken character materials after visual capture (Meshy GLB clones often lose mats).
    /// Uses URP Lit Creative Character mats — VertexColorLit Explorer_Palette reads white without vertex colors.
    /// </summary>
    public static class HometownArrivalCharacterMaterialRepair
    {
        private const string PackColorPath = "Assets/ithappy/Creative_Characters_FREE/Materials/Color.mat";
        private const string ExplorerPaletteMeshPath =
            "Assets/Characters/Meshy_AI_Casual_Explorer_biped_Palette.asset";
        private const string ExplorerPaletteMaterialPath =
            "Assets/Settings/ChoTet/Materials/Explorer_Palette.mat";

        public static void RepairAll()
        {
            PlayerTopDownController player = Object.FindFirstObjectByType<PlayerTopDownController>();
            if (player != null)
            {
                RepairPlayer(player);
            }

            RepairNamedParent("Mother", isMother: true);
            RepairNamedParent("Father", isMother: false);
        }

        public static void RepairPlayer(PlayerTopDownController player)
        {
            if (player == null || player.visualRoot == null)
            {
                return;
            }

            Material teal = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.PlayerTealShirtMaterial);
            Material pants = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.PlayerDarkPantsMaterial);
            Material packColor = HometownArrivalResourceCache.LoadMaterialAtPath(PackColorPath);
            Material bodyFallback = packColor != null
                ? packColor
                : CreateUrpFallback(new Color(0.76f, 0.58f, 0.48f));
            Material outerwearFallback = teal != null ? teal : bodyFallback;
            Material pantsFallback = pants != null
                ? pants
                : packColor != null
                    ? packColor
                    : CreateUrpFallback(new Color(0.18f, 0.20f, 0.24f));

            if (bodyFallback == null)
            {
                return;
            }

            SkinnedMeshRenderer[] skins = player.visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skins.Length; i++)
            {
                if (skins[i] == null)
                {
                    continue;
                }

                if (TryAssignExplorerPalette(skins[i]))
                {
                    continue;
                }

                Material chosen = ResolvePlayerSlot(
                    skins[i].name,
                    bodyFallback,
                    outerwearFallback,
                    pantsFallback) ?? bodyFallback;
                ReplaceBrokenMaterials(skins[i], chosen);
            }

            MeshRenderer[] meshes = player.visualRoot.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] == null
                    || meshes[i].name.IndexOf("Shadow", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                Material chosen = ResolvePlayerSlot(
                    meshes[i].name,
                    bodyFallback,
                    outerwearFallback,
                    pantsFallback) ?? bodyFallback;
                ReplaceBrokenMaterials(meshes[i], chosen);
            }

            player.ForceRealignVisual();
        }

        private static void ReplaceBrokenMaterials(Renderer renderer, Material replacement)
        {
            if (renderer == null || replacement == null)
            {
                return;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                renderer.sharedMaterial = replacement;
                return;
            }

            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                if (!IsBrokenOrWhitePalette(materials[i]))
                {
                    continue;
                }

                materials[i] = replacement;
                changed = true;
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
            }
        }

        private static bool TryAssignExplorerPalette(SkinnedMeshRenderer renderer)
        {
            if (renderer == null
                || renderer.sharedMesh == null
                || !renderer.name.Equals("char1", System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Mesh paletteMesh = HometownArrivalResourceCache.LoadMeshAtPath(ExplorerPaletteMeshPath);
            Material paletteMaterial = HometownArrivalResourceCache.LoadMaterialAtPath(ExplorerPaletteMaterialPath);
            if (paletteMesh == null
                || paletteMaterial == null
                || paletteMaterial.shader == null
                || !paletteMaterial.shader.isSupported
                || paletteMesh.vertexCount != renderer.sharedMesh.vertexCount
                || paletteMesh.colors == null
                || paletteMesh.colors.Length != paletteMesh.vertexCount)
            {
                return false;
            }

            renderer.sharedMesh = paletteMesh;
            ForceAssign(renderer, paletteMaterial);
            return true;
        }

        private static void ForceAssign(Renderer renderer, Material material)
        {
            if (renderer == null || material == null)
            {
                return;
            }

            Material[] mats = renderer.sharedMaterials;
            if (mats == null || mats.Length == 0)
            {
                renderer.sharedMaterial = material;
                return;
            }

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = material;
            }

            renderer.sharedMaterials = mats;
        }

        private static Material ResolvePlayerSlot(
            string slotName,
            Material body,
            Material outerwear,
            Material pantsMat)
        {
            if (slotName.StartsWith("Pants", System.StringComparison.Ordinal) && pantsMat != null)
            {
                return pantsMat;
            }

            if (slotName.StartsWith("Outerwear", System.StringComparison.Ordinal) && outerwear != null)
            {
                return outerwear;
            }

            return body;
        }

        private static void RepairNamedParent(string objectName, bool isMother)
        {
            GameObject root = GameObject.Find(objectName);
            if (root == null)
            {
                return;
            }

            Material colorMat = HometownArrivalResourceCache.LoadMaterialAtPath(PackColorPath);
            Material pinkShirt = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.MotherPinkShirtMaterial);
            Material jeanPants = HometownArrivalResourceCache.LoadMaterialAtPath(
                PlayerCharacterVisualPaths.MotherJeanPantsMaterial);

            SkinnedMeshRenderer[] skins = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skins.Length; i++)
            {
                SkinnedMeshRenderer skin = skins[i];
                if (skin == null)
                {
                    continue;
                }

                Material[] mats = skin.sharedMaterials;
                bool changed = false;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (!IsBrokenOrWhitePalette(mats[m]))
                    {
                        continue;
                    }

                    mats[m] = ResolveParentSlot(skin.name, isMother, colorMat, pinkShirt, jeanPants)
                              ?? colorMat
                              ?? CreateUrpFallback(Color.white);
                    changed = true;
                }

                if (changed)
                {
                    skin.sharedMaterials = mats;
                }
            }

            MeshRenderer rootMesh = root.GetComponent<MeshRenderer>();
            if (rootMesh != null && IsBrokenOrWhitePalette(rootMesh.sharedMaterial))
            {
                rootMesh.enabled = false;
            }
        }

        private static Material ResolveParentSlot(
            string slotName,
            bool isMother,
            Material colorMat,
            Material pinkShirt,
            Material jeanPants)
        {
            if (isMother && slotName.StartsWith("Outerwear", System.StringComparison.Ordinal) && pinkShirt != null)
            {
                return pinkShirt;
            }

            if (isMother && slotName.StartsWith("Pants", System.StringComparison.Ordinal) && jeanPants != null)
            {
                return jeanPants;
            }

            return colorMat;
        }

        private static bool IsBrokenOrWhitePalette(Material material)
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
                   || shaderName == "Standard (Specular setup)"
                   || shaderName.Contains("VertexColorLit")
                   || matName.IndexOf("Explorer_Palette", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Material CreateUrpFallback(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
            {
                return null;
            }

            var mat = new Material(shader) { name = "HometownArrival_CharFallback" };
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
