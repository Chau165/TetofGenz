#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using ChoTet.Runtime;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.Character;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.FaceEditor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.MaterialManagement;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.FaceManagement;
using UnityEngine;

namespace ChoTet.Editor
{
    /// <summary>Shared Creative Characters FREE dress + mesh apply logic.</summary>
    public static class CreativeCharacterAssembly
    {
        public static GameObject BuildDressedCharacter(System.Action<CustomizableCharacter> applyLook, string rootName)
        {
            SlotLibrary slotLibrary = SlotLibraryLoader.LoadSlotLibrary();
            if (slotLibrary == null)
            {
                return null;
            }

            var customizable = new CustomizableCharacter(slotLibrary);
            applyLook?.Invoke(customizable);

            GameObject character = customizable.InstantiateCharacter();
            character.name = rootName;
            ApplyCustomizationToCharacter(character, customizable);
            return character;
        }

        public static void ApplyCustomizationToCharacter(GameObject character, CustomizableCharacter customizable)
        {
            Transform meshRoot = ResolveMeshRoot(character.transform);
            Material mainColor = new MaterialProvider().MainColor;
            if (mainColor == null)
            {
                Debug.LogWarning("[CreativeCharacters] Color.mat not found — character may appear gray.");
            }

            HideLegacyFullBodyMesh(meshRoot);

            HashSet<SlotType> activeMeshSlots = BuildActiveMeshSlots(customizable);
            DisableInactiveSlotRenderers(meshRoot, activeMeshSlots);

            SlotBase[] slots = customizable.Slots;
            for (int s = 0; s < slots.Length; s++)
            {
                SlotBase slot = slots[s];
                if (!slot.IsEnabled)
                {
                    continue;
                }

                (SlotType, Mesh)[] meshes = slot.Meshes;
                for (int m = 0; m < meshes.Length; m++)
                {
                    ApplyMeshToSlot(meshRoot, meshes[m].Item1, meshes[m].Item2, mainColor);
                }
            }

            FaceLoader.AddFaces(meshRoot.gameObject);
        }

        public static void ApplyMotherLook(CustomizableCharacter character)
        {
            character.ToDefault();
            character.PickGroup(GroupType.Faces, 2, true);
            character.PickGroup(GroupType.Body, 0, true);
            character.PickGroup(GroupType.Outwear, 0, true);
            character.PickGroup(GroupType.Pants, 0, true);
            character.PickGroup(GroupType.Shoes, 1, true);
            character.PickGroup(GroupType.HairstyleSingle, 0, true);
        }

        /// <summary>Assign saved pink shirt + jean materials to mother clothing slots.</summary>
        public static void ApplyMotherAppearanceMaterials(GameObject character)
        {
            Material pinkShirt = EnsureTintedPackMaterialAsset(
                PlayerCharacterVisualPaths.MotherPinkShirtMaterial,
                new Color(0.58f, 0.18f, 0.12f));
            Material jeanPants = EnsureTintedPackMaterialAsset(
                PlayerCharacterVisualPaths.MotherJeanPantsMaterial,
                new Color(0.32f, 0.44f, 0.74f));

            if (pinkShirt != null)
            {
                ApplyMaterialToSlotPrefix(character, "Outerwear", pinkShirt);
            }

            if (jeanPants != null)
            {
                ApplyMaterialToSlotPrefix(character, "Pants", jeanPants);
            }
        }

        private static Material EnsureTintedPackMaterialAsset(string assetPath, Color tint)
        {
            Material existing = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null)
            {
                ApplyMaterialTint(existing, tint);
                UnityEditor.EditorUtility.SetDirty(existing);
                return existing;
            }

            Material template = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/ithappy/Creative_Characters_FREE/Materials/Color.mat");
            if (template == null)
            {
                return null;
            }

            string folder = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(folder) && !UnityEditor.AssetDatabase.IsValidFolder(folder))
            {
                CreativeCharactersPlayerSetup.EnsureFolder(folder);
            }

            Material material = new Material(template);
            ApplyMaterialTint(material, tint);
            UnityEditor.AssetDatabase.CreateAsset(material, assetPath);
            return material;
        }

        private static void ApplyMaterialTint(Material material, Color tint)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }
        }

        private static void ApplyMaterialToSlotPrefix(GameObject character, string slotPrefix, Material material)
        {
            Transform meshRoot = ResolveMeshRoot(character.transform);
            for (int i = 0; i < meshRoot.childCount; i++)
            {
                Transform child = meshRoot.GetChild(i);
                if (!child.name.StartsWith(slotPrefix, System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (child.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer))
                {
                    renderer.sharedMaterial = material;
                }
            }
        }

        public static void ApplyFatherLook(CustomizableCharacter character)
        {
            character.ToDefault();
            character.PickGroup(GroupType.Faces, 0, true);
            character.PickGroup(GroupType.Body, 0, true);
            character.PickGroup(GroupType.Outwear, 1, true);
            character.PickGroup(GroupType.Pants, 1, true);
            character.PickGroup(GroupType.Shoes, 0, true);
            character.PickGroup(GroupType.Hairstyle, 0, true);
            character.PickGroup(GroupType.FaceAccessories, 1, true);
        }

        public static void ApplyPlayerLook(CustomizableCharacter character)
        {
            character.ToDefault();
            character.PickGroup(GroupType.Faces, 0, true);
            character.PickGroup(GroupType.Body, 0, true);
            character.PickGroup(GroupType.Outwear, 2, true);
            character.PickGroup(GroupType.Pants, 1, true);
            character.PickGroup(GroupType.Shoes, 0, true);
            character.PickGroup(GroupType.Hairstyle, 0, true);
        }

        public static void ApplyTintedOutfit(GameObject character, Color outerwearTint, Color pantsTint)
        {
            Material template = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/ithappy/Creative_Characters_FREE/Materials/Color.mat");
            if (template == null)
            {
                return;
            }

            Material outerwear = new Material(template);
            ApplyMaterialTint(outerwear, outerwearTint);
            Material pants = new Material(template);
            ApplyMaterialTint(pants, pantsTint);
            ApplyMaterialToSlotPrefix(character, "Outerwear", outerwear);
            ApplyMaterialToSlotPrefix(character, "Outwear", outerwear);
            ApplyMaterialToSlotPrefix(character, "Pants", pants);
            ApplyMaterialToSlotPrefix(character, "Shorts", pants);
        }

        /// <summary>Teal shirt + dark pants so the child reads clearly different from mother.</summary>
        public static void ApplyPlayerAppearanceMaterials(GameObject character)
        {
            Material tealShirt = EnsureTintedPackMaterialAsset(
                PlayerCharacterVisualPaths.PlayerTealShirtMaterial,
                new Color(0.28f, 0.78f, 0.88f));
            Material darkPants = EnsureTintedPackMaterialAsset(
                PlayerCharacterVisualPaths.PlayerDarkPantsMaterial,
                new Color(0.22f, 0.24f, 0.30f));

            if (tealShirt != null)
            {
                ApplyMaterialToSlotPrefix(character, "Outerwear", tealShirt);
            }

            if (darkPants != null)
            {
                ApplyMaterialToSlotPrefix(character, "Pants", darkPants);
            }
        }

        /// <summary>Static workplace NPC — animator + capsule blocker, no locomotion components.</summary>
        public static void PrepareStaticWorkplaceNpc(GameObject root, RuntimeAnimatorController controller, float scale)
        {
            PrepareNpcInstance(root, controller);
            EnsureStaticVisualRoot(root, scale);
            EnsureNpcCapsuleCollider(root, scale);
        }

        public static void PrepareNpcInstance(GameObject root, RuntimeAnimatorController controller)
        {
            StripGameplayComponents(root);
            // Drop unused clothing slots — each idle SkinnedMeshRenderer costs domain-reload time.
            DestroyUnusedMeshParts(root);

            Animator animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                animator = root.AddComponent<Animator>();
            }

            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            // Keep idle pose updating when on-screen (CullCompletely left NPCs in A-pose).
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.SetBool("IsMoving", false);
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MoveAmount", 0f);
            animator.SetBool("IsRunning", false);
            if (animator.runtimeAnimatorController != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Object.DestroyImmediate(bodies[i]);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }

            FacePicker[] facePickers = root.GetComponentsInChildren<FacePicker>(true);
            for (int i = 0; i < facePickers.Length; i++)
            {
                Object.DestroyImmediate(facePickers[i]);
            }
        }

        private static void DestroyUnusedMeshParts(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            SkinnedMeshRenderer[] skinned = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinned.Length; i++)
            {
                SkinnedMeshRenderer renderer = skinned[i];
                if (renderer == null)
                {
                    continue;
                }

                bool unused = !renderer.enabled
                    || renderer.sharedMesh == null
                    || renderer.gameObject.name.StartsWith("Full_body", System.StringComparison.Ordinal);
                if (unused)
                {
                    Object.DestroyImmediate(renderer.gameObject);
                }
            }

            // FaceLoader may leave extra picker/helper objects; keep only active renderers.
            MeshRenderer[] meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                MeshRenderer renderer = meshRenderers[i];
                if (renderer != null && !renderer.enabled)
                {
                    Object.DestroyImmediate(renderer.gameObject);
                }
            }
        }

        private static HashSet<SlotType> BuildActiveMeshSlots(CustomizableCharacter customizable)
        {
            HashSet<SlotType> active = new HashSet<SlotType>();
            SlotBase[] slots = customizable.Slots;
            for (int s = 0; s < slots.Length; s++)
            {
                SlotBase slot = slots[s];
                if (!slot.IsEnabled)
                {
                    continue;
                }

                (SlotType, Mesh)[] meshes = slot.Meshes;
                for (int m = 0; m < meshes.Length; m++)
                {
                    active.Add(meshes[m].Item1);
                }
            }

            return active;
        }

        private static void DisableInactiveSlotRenderers(Transform meshRoot, HashSet<SlotType> activeMeshSlots)
        {
            for (int i = 0; i < meshRoot.childCount; i++)
            {
                Transform child = meshRoot.GetChild(i);
                if (!child.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer))
                {
                    continue;
                }

                if (!TryResolveSlotType(child.name, out SlotType slotType))
                {
                    continue;
                }

                if (!activeMeshSlots.Contains(slotType))
                {
                    renderer.enabled = false;
                }
            }
        }

        private static void ApplyMeshToSlot(
            Transform meshRoot,
            SlotType slotType,
            Mesh mesh,
            Material mainColor)
        {
            if (mesh == null)
            {
                return;
            }

            Transform child = FindDirectSlotChild(meshRoot, slotType);
            if (child == null
                || !child.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer))
            {
                return;
            }

            renderer.enabled = true;
            renderer.sharedMesh = mesh;
            if (mainColor != null)
            {
                renderer.sharedMaterial = mainColor;
            }

            renderer.localBounds = mesh.bounds;
        }

        private static bool TryResolveSlotType(string childName, out SlotType slotType)
        {
            foreach (SlotType candidate in System.Enum.GetValues(typeof(SlotType)))
            {
                if (childName.StartsWith(GetSlotChildPrefix(candidate), System.StringComparison.Ordinal))
                {
                    slotType = candidate;
                    return true;
                }
            }

            slotType = default;
            return false;
        }

        private static Transform ResolveMeshRoot(Transform characterRoot)
        {
            Transform nestedBaseMesh = characterRoot.Find("Base_Mesh");
            return nestedBaseMesh != null ? nestedBaseMesh : characterRoot;
        }

        private static void HideLegacyFullBodyMesh(Transform meshRoot)
        {
            Transform fullBody = meshRoot.Cast<Transform>()
                .FirstOrDefault(t => t.name.StartsWith("Full_body", System.StringComparison.Ordinal));
            if (fullBody != null
                && fullBody.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer))
            {
                renderer.enabled = false;
            }
        }

        private static Transform FindDirectSlotChild(Transform meshRoot, SlotType slotType)
        {
            string prefix = GetSlotChildPrefix(slotType);
            for (int i = 0; i < meshRoot.childCount; i++)
            {
                Transform child = meshRoot.GetChild(i);
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static string GetSlotChildPrefix(SlotType slotType)
        {
            switch (slotType)
            {
                case SlotType.TShirt:
                    return "T_Shirt";
                default:
                    return slotType.ToString();
            }
        }

        private static void EnsureStaticVisualRoot(GameObject root, float scale)
        {
            Transform visualRoot = root.transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                GameObject visualRootObject = new GameObject("VisualRoot");
                visualRoot = visualRootObject.transform;
                visualRoot.SetParent(root.transform, false);
                visualRoot.localPosition = Vector3.zero;
            }

            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = root.transform.GetChild(i);
                if (child == visualRoot || child.name == "NpcShadow")
                {
                    continue;
                }

                child.SetParent(visualRoot, true);
            }

            visualRoot.localScale = Vector3.one;
        }

        private static void EnsureNpcCapsuleCollider(GameObject root, float scale)
        {
            CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = root.AddComponent<CapsuleCollider>();
            }

            float height = 1.8f * scale;
            float radius = 0.26f * scale;
            capsule.height = height;
            capsule.radius = radius;
            capsule.center = new Vector3(0f, height * 0.5f, 0f);
            capsule.direction = 1;
            capsule.isTrigger = false;
        }

        private static void StripGameplayComponents(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour is Animator)
                {
                    continue;
                }

                // Crowd NPCs only need a posed mesh + idle Animator.
                Object.DestroyImmediate(behaviour);
            }
        }
    }
}
#endif
