#if UNITY_EDITOR
using System;
using ChoTet.Runtime;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.Character;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Editor
{
    /// <summary>Bakes vendor/shopper NPCs at market stalls and visitors around the temple courtyard.</summary>
    public static class ChoTetMarketCrowdBuilder
    {
        private const string CrowdRootName = "ChoTetMarketCrowd";
        private const string TempleCrowdRootName = "ChoTetTempleCrowd";
        private const float NpcScale = 1.05f;
        private const float FootY = 0.02f;

        private struct CrowdSpawn
        {
            public string Name;
            public Vector3 Position;
            public float Yaw;
            public Action<CustomizableCharacter> ApplyLook;
            public Color ShirtTint;
            public Color PantsTint;
        }

        public static void Build(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            if (SlotLibraryLoader.LoadSlotLibrary() == null)
            {
                Debug.LogWarning("[ChoTetMarketCrowdBuilder] Creative Characters pack missing — skip crowd.");
                return;
            }

            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                PlayerCharacterVisualPaths.CreativeCharacterTopDownController);
            if (controller == null)
            {
                Debug.LogWarning("[ChoTetMarketCrowdBuilder] Top-down animator missing — run Setup Creative Character Player.");
                return;
            }

            BuildCrowdGroup(worldRoot, CrowdRootName, CreateMarketSpawns(), controller);
            BuildCrowdGroup(worldRoot, TempleCrowdRootName, CreateTempleSpawns(), controller);
        }

        private static void BuildCrowdGroup(
            Transform worldRoot,
            string rootName,
            CrowdSpawn[] spawns,
            RuntimeAnimatorController controller)
        {
            Transform existing = worldRoot.Find(rootName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject(rootName);
            root.transform.SetParent(worldRoot, false);

            for (int i = 0; i < spawns.Length; i++)
            {
                SpawnNpc(root.transform, spawns[i], controller);
            }
        }

        private static CrowdSpawn[] CreateMarketSpawns()
        {
            // Vendors stand behind each stall counter, facing customers (south).
            // Shoppers stand in front of a few stalls, facing the vendor.
            return new[]
            {
                Make("Vendor_BanhMut", new Vector3(-12f, FootY, 87.4f), 180f, ApplyLookOrangeVest,
                    new Color(0.98f, 0.52f, 0.18f), new Color(0.42f, 0.28f, 0.18f)),
                Make("Vendor_DuaHau", new Vector3(0f, FootY, 87.4f), 180f, ApplyLookGreenHoodie,
                    new Color(0.22f, 0.72f, 0.38f), new Color(0.15f, 0.18f, 0.24f)),
                Make("Vendor_HoaMai", new Vector3(12f, FootY, 87.4f), 180f, ApplyLookMintDress,
                    new Color(0.92f, 0.42f, 0.62f), new Color(0.18f, 0.22f, 0.26f)),
                Make("Vendor_Thit", new Vector3(-8f, FootY, 98.4f), 180f, ApplyLookBrownCasual,
                    new Color(0.58f, 0.40f, 0.28f), new Color(0.22f, 0.24f, 0.30f)),
                Make("Vendor_QuaBieu", new Vector3(9.5f, FootY, 98.4f), 180f, ApplyLookYellowCoat,
                    new Color(0.98f, 0.82f, 0.22f), new Color(0.34f, 0.36f, 0.42f)),

                Make("Shopper_BanhMut", new Vector3(-10.8f, FootY, 82.4f), 0f, ApplyLookBlueDenim,
                    new Color(0.24f, 0.48f, 0.92f), new Color(0.30f, 0.44f, 0.72f)),
                Make("Shopper_DuaHau", new Vector3(1.2f, FootY, 82.6f), 0f, ApplyLookRedJacket,
                    new Color(0.92f, 0.28f, 0.24f), new Color(0.18f, 0.20f, 0.28f)),
                Make("Shopper_HoaMai", new Vector3(11.1f, FootY, 82.8f), 0f, ApplyLookPurpleShirt,
                    new Color(0.62f, 0.34f, 0.88f), new Color(0.28f, 0.30f, 0.36f)),
                Make("Shopper_Thit", new Vector3(-6.9f, FootY, 93.2f), 0f, ApplyLookBrownCasual,
                    new Color(0.72f, 0.48f, 0.28f), new Color(0.24f, 0.26f, 0.32f)),
            };
        }

        private static CrowdSpawn[] CreateTempleSpawns()
        {
            // Creative Characters FREE — courtyard visitors + hall worshippers facing the Buddha.
            const float hallY = 1.65f;
            return new[]
            {
                Make("TempleVisitor_GateL", new Vector3(48f, FootY, 98f), 20f, ApplyLookBlueDenim,
                    new Color(0.28f, 0.42f, 0.78f), new Color(0.22f, 0.24f, 0.30f)),
                Make("TempleVisitor_GateR", new Vector3(62f, FootY, 99f), -15f, ApplyLookMintDress,
                    new Color(0.88f, 0.55f, 0.62f), new Color(0.20f, 0.22f, 0.28f)),
                Make("TempleVisitor_PathA", new Vector3(52f, FootY, 108f), 0f, ApplyLookRedJacket,
                    new Color(0.86f, 0.24f, 0.22f), new Color(0.18f, 0.18f, 0.24f)),
                Make("TempleVisitor_CourtyardL", new Vector3(44f, FootY, 122f), 90f, ApplyLookPurpleShirt,
                    new Color(0.55f, 0.32f, 0.82f), new Color(0.26f, 0.28f, 0.34f)),
                Make("TempleVisitor_CourtyardR", new Vector3(66f, FootY, 124f), -90f, ApplyLookGreenHoodie,
                    new Color(0.20f, 0.62f, 0.36f), new Color(0.16f, 0.18f, 0.22f)),
                Make("TempleVisitor_BenchL", new Vector3(46f, FootY, 126f), 90f, ApplyLookOrangeVest,
                    new Color(0.92f, 0.48f, 0.20f), new Color(0.36f, 0.26f, 0.18f)),
                Make("TempleVisitor_Elder", new Vector3(61f, FootY, 132f), -40f, ApplyLookBrownCasual,
                    new Color(0.62f, 0.44f, 0.28f), new Color(0.20f, 0.22f, 0.28f)),

                // Hall worshippers (thắp hương) — leave center aisle clear for player stations.
                Make("TempleWorship_L1", new Vector3(49.2f, hallY, 144.8f), 0f, ApplyLookBlueDenim,
                    new Color(0.3f, 0.4f, 0.75f), new Color(0.22f, 0.24f, 0.3f)),
                Make("TempleWorship_L2", new Vector3(50.6f, hallY, 143.4f), 8f, ApplyLookRedJacket,
                    new Color(0.85f, 0.25f, 0.22f), new Color(0.18f, 0.18f, 0.24f)),
                Make("TempleWorship_L3", new Vector3(48.4f, hallY, 146.0f), -6f, ApplyLookPurpleShirt,
                    new Color(0.55f, 0.3f, 0.78f), new Color(0.24f, 0.26f, 0.32f)),
                Make("TempleWorship_R1", new Vector3(60.6f, hallY, 144.8f), 0f, ApplyLookMintDress,
                    new Color(0.9f, 0.45f, 0.58f), new Color(0.18f, 0.2f, 0.26f)),
                Make("TempleWorship_R2", new Vector3(59.2f, hallY, 143.4f), -8f, ApplyLookYellowCoat,
                    new Color(0.95f, 0.8f, 0.28f), new Color(0.28f, 0.3f, 0.36f)),
                Make("TempleWorship_R3", new Vector3(61.4f, hallY, 146.0f), 6f, ApplyLookOrangeVest,
                    new Color(0.92f, 0.48f, 0.2f), new Color(0.34f, 0.26f, 0.18f)),
                Make("TempleWorship_BackL", new Vector3(51.0f, hallY, 142.2f), 0f, ApplyLookGreenHoodie,
                    new Color(0.22f, 0.58f, 0.34f), new Color(0.16f, 0.18f, 0.22f)),
                Make("TempleWorship_BackR", new Vector3(58.8f, hallY, 142.2f), 0f, ApplyLookBrownCasual,
                    new Color(0.68f, 0.46f, 0.3f), new Color(0.22f, 0.24f, 0.3f)),
            };
        }

        private static CrowdSpawn Make(
            string name,
            Vector3 position,
            float yaw,
            Action<CustomizableCharacter> applyLook,
            Color shirt,
            Color pants)
        {
            return new CrowdSpawn
            {
                Name = name,
                Position = position,
                Yaw = yaw,
                ApplyLook = applyLook,
                ShirtTint = shirt,
                PantsTint = pants
            };
        }

        private static void SpawnNpc(Transform parent, CrowdSpawn spawn, RuntimeAnimatorController controller)
        {
            GameObject instance = CreativeCharacterAssembly.BuildDressedCharacter(spawn.ApplyLook, spawn.Name);
            if (instance == null)
            {
                Debug.LogWarning("[ChoTetMarketCrowdBuilder] Failed to build " + spawn.Name);
                return;
            }

            CreativeCharacterAssembly.ApplyTintedOutfit(instance, spawn.ShirtTint, spawn.PantsTint);
            CreativeCharacterAssembly.PrepareNpcInstance(instance, controller);
            instance.transform.SetParent(parent, false);
            instance.transform.position = spawn.Position;
            instance.transform.rotation = Quaternion.Euler(0f, spawn.Yaw, 0f);
            instance.transform.localScale = Vector3.one * NpcScale;

            ChoTetAmbientNpc ambient = instance.AddComponent<ChoTetAmbientNpc>();
            ambient.homePosition = spawn.Position;
            bool isVendor = spawn.Name.StartsWith("Vendor_", System.StringComparison.Ordinal);
            bool isWorship = spawn.Name.StartsWith("TempleWorship", System.StringComparison.Ordinal);
            ambient.wanderRadius = isVendor ? 0.35f : (isWorship ? 1.1f : 2.4f);
            ambient.walkSpeed = isWorship ? 0.75f : 1.15f;
            ambient.stayNearHome = isVendor;
        }

        private static void SafePick(CustomizableCharacter character, GroupType groupType, int index, bool enabled = true)
        {
            int count = character.GetVariantsCountInGroup(groupType);
            if (count <= 0)
            {
                return;
            }

            try
            {
                character.PickGroup(groupType, Mathf.Clamp(index, 0, count - 1), enabled);
            }
            catch (Exception)
            {
            }
        }

        private static void ApplyBaseBody(CustomizableCharacter character, int faceIndex)
        {
            character.ToDefault();
            SafePick(character, GroupType.Body, 0);
            SafePick(character, GroupType.Faces, faceIndex);
        }

        private static void ApplyCasualLook(
            CustomizableCharacter character,
            int face,
            int outwear,
            int pants,
            int shoes,
            int hairstyle = 0,
            bool useSingleHairstyle = false,
            int faceAccessory = -1,
            int glasses = -1)
        {
            ApplyBaseBody(character, face);
            SafePick(character, GroupType.Outwear, outwear);
            SafePick(character, GroupType.Pants, pants);
            SafePick(character, GroupType.Shoes, shoes);
            if (useSingleHairstyle)
            {
                SafePick(character, GroupType.HairstyleSingle, hairstyle);
            }
            else
            {
                SafePick(character, GroupType.Hairstyle, hairstyle);
            }

            if (faceAccessory >= 0)
            {
                SafePick(character, GroupType.FaceAccessories, faceAccessory);
            }

            if (glasses >= 0)
            {
                SafePick(character, GroupType.Glasses, glasses);
            }
        }

        private static void ApplyLookRedJacket(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 1, outwear: 0, pants: 1, shoes: 0, hairstyle: 1);

        private static void ApplyLookYellowCoat(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 2, outwear: 1, pants: 0, shoes: 1, hairstyle: 1, useSingleHairstyle: true);

        private static void ApplyLookGreenHoodie(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 0, outwear: 2, pants: 2, shoes: 2, hairstyle: 0, glasses: 0);

        private static void ApplyLookPurpleShirt(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 2, outwear: 0, pants: 0, shoes: 0, hairstyle: 2);

        private static void ApplyLookOrangeVest(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 1, outwear: 2, pants: 1, shoes: 1, hairstyle: 0, faceAccessory: 0);

        private static void ApplyLookBlueDenim(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 2, outwear: 1, pants: 2, shoes: 0, hairstyle: 2);

        private static void ApplyLookMintDress(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 2, outwear: 0, pants: 2, shoes: 1, hairstyle: 2, useSingleHairstyle: true);

        private static void ApplyLookBrownCasual(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 0, outwear: 1, pants: 0, shoes: 0, hairstyle: 1, faceAccessory: 1);
    }
}
#endif
