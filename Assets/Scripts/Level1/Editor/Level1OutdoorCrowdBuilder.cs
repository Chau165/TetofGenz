#if UNITY_EDITOR
using System;
using ChoTet.Editor;
using ChoTet.Level1;
using ChoTet.Runtime;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.Character;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Static Creative Characters crowd on the outdoor grass path — each NPC has a street role.</summary>
    public static class Level1OutdoorCrowdBuilder
    {
        private const string CrowdRootName = "Level1_OutdoorCrowd";
        private const float NpcScale = 0.68f;
        private const float FootY = Level1OutdoorSettings.WalkSurfaceY + 0.02f;
        /// <summary>Stay on the central grass strip, clear of sidewalks and building rows.</summary>
        private const float GrassPathMaxAbsX = 2.55f;

        private struct CrowdSpawn
        {
            public string Name;
            public Vector3 Position;
            public float Yaw;
            public Action<CustomizableCharacter> ApplyLook;
            public Color ShirtTint;
            public Color PantsTint;
            public bool EnablePatrol;
            public float PatrolRange;
            public float PatrolSpeed;
        }

        public static void Build(Transform outdoorRoot)
        {
            if (outdoorRoot == null)
            {
                return;
            }

            Transform existing = outdoorRoot.Find(CrowdRootName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            if (SlotLibraryLoader.LoadSlotLibrary() == null)
            {
                Debug.LogWarning("[Level1OutdoorCrowdBuilder] Creative Characters pack missing — skip crowd.");
                return;
            }

            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                PlayerCharacterVisualPaths.CreativeCharacterTopDownController);
            if (controller == null)
            {
                Debug.LogWarning("[Level1OutdoorCrowdBuilder] Top-down animator missing — run Setup Creative Character Player.");
                return;
            }

            GameObject root = new GameObject(CrowdRootName);
            root.transform.SetParent(outdoorRoot, false);
            root.AddComponent<Level1OutdoorNpcDialogueBootstrap>();

            CrowdSpawn[] spawns = CreateSpawns();
            for (int i = 0; i < spawns.Length; i++)
            {
                SpawnNpc(root.transform, spawns[i], controller);
            }
        }

        private static CrowdSpawn[] CreateSpawns()
        {
            Vector3 stallSeller = WithFootY(Level1OutdoorAmbientPropsBuilder.StallSellerSpot);
            Vector3 flowerViewer = WithFootY(Level1OutdoorAmbientPropsBuilder.FlowerViewerSpot);
            Vector3 talkA = ClampToGrassPath(WithFootY(Level1OutdoorAmbientPropsBuilder.TalkSpotA));
            Vector3 talkB = ClampToGrassPath(WithFootY(Level1OutdoorAmbientPropsBuilder.TalkSpotB));
            Vector3 casualAlone = ClampToGrassPath(WithFootY(Level1OutdoorAmbientPropsBuilder.CasualAloneSpot));
            Vector3 casualSecond = ClampToGrassPath(WithFootY(Level1OutdoorAmbientPropsBuilder.CasualSecondSpot));

            Vector3 flowerTarget = Level1OutdoorAmbientPropsBuilder.FlowerCornerCenter;
            Vector3 stallTarget = Level1OutdoorAmbientPropsBuilder.StallCenter;

            return new[]
            {
                Make("CrowdNPC_StallSeller", stallSeller, YawToward(stallSeller, new Vector3(0f, stallSeller.y, stallSeller.z)), ApplyLookOrangeVest,
                    new Color(0.98f, 0.52f, 0.18f), new Color(0.42f, 0.28f, 0.18f)),
                Make("CrowdNPC_FlowerViewer", flowerViewer, YawToward(flowerViewer, flowerTarget), ApplyLookMintDress,
                    new Color(0.42f, 0.88f, 0.78f), new Color(0.18f, 0.22f, 0.26f)),
                Make("CrowdNPC_TalkA", talkA, YawToward(talkA, talkB), ApplyLookRedJacket,
                    new Color(0.92f, 0.28f, 0.24f), new Color(0.18f, 0.20f, 0.28f)),
                Make("CrowdNPC_TalkB", talkB, YawToward(talkB, talkA), ApplyLookYellowCoat,
                    new Color(0.98f, 0.82f, 0.22f), new Color(0.34f, 0.36f, 0.42f)),
                Make("CrowdNPC_CasualAlone", casualAlone, -158f, ApplyLookBrownCasual,
                    new Color(0.58f, 0.40f, 0.28f), new Color(0.22f, 0.24f, 0.30f)),
                Make("CrowdNPC_CasualSecond", casualSecond, 24f, ApplyLookBlueDenim,
                    new Color(0.24f, 0.48f, 0.92f), new Color(0.30f, 0.44f, 0.72f)),
                Make("CrowdNPC_PatrolA", ClampToGrassPath(new Vector3(1.95f, FootY, -9.4f)), -155f, ApplyLookGreenHoodie,
                    new Color(0.22f, 0.72f, 0.38f), new Color(0.15f, 0.18f, 0.24f), enablePatrol: true, patrolRange: 1.25f, patrolSpeed: 1.75f),
                Make("CrowdNPC_PatrolB", ClampToGrassPath(new Vector3(-1.85f, FootY, -21.4f)), 22f, ApplyLookPurpleShirt,
                    new Color(0.62f, 0.34f, 0.88f), new Color(0.28f, 0.30f, 0.36f), enablePatrol: true, patrolRange: 1.05f, patrolSpeed: 1.6f),
            };
        }

        private static Vector3 WithFootY(Vector3 position)
        {
            position.y = FootY;
            return position;
        }

        private static CrowdSpawn Make(
            string name,
            Vector3 position,
            float yaw,
            Action<CustomizableCharacter> applyLook,
            Color shirt,
            Color pants,
            bool enablePatrol = false,
            float patrolRange = 1.2f,
            float patrolSpeed = 0.65f)
        {
            return new CrowdSpawn
            {
                Name = name,
                Position = position,
                Yaw = yaw,
                ApplyLook = applyLook,
                ShirtTint = shirt,
                PantsTint = pants,
                EnablePatrol = enablePatrol,
                PatrolRange = patrolRange,
                PatrolSpeed = patrolSpeed
            };
        }

        private static void SpawnNpc(Transform parent, CrowdSpawn spawn, RuntimeAnimatorController controller)
        {
            GameObject instance = CreativeCharacterAssembly.BuildDressedCharacter(spawn.ApplyLook, spawn.Name);
            if (instance == null)
            {
                Debug.LogWarning("[Level1OutdoorCrowdBuilder] Failed to build " + spawn.Name);
                return;
            }

            CreativeCharacterAssembly.ApplyTintedOutfit(instance, spawn.ShirtTint, spawn.PantsTint);
            CreativeCharacterAssembly.PrepareNpcInstance(instance, controller);
            instance.transform.SetParent(parent, false);
            instance.transform.position = spawn.Position;
            instance.transform.rotation = Quaternion.Euler(0f, spawn.Yaw, 0f);
            instance.transform.localScale = Vector3.one * NpcScale;

            AttachNpcDialogue(instance, spawn.Name);

            if (spawn.EnablePatrol)
            {
                TopDownScriptedLocomotion.PreparePatrolHierarchy(instance, NpcScale);

                Level1OutdoorNpcPatrol patrol = instance.AddComponent<Level1OutdoorNpcPatrol>();
                WritePatrolSettings(
                    patrol,
                    spawn.Position,
                    spawn.PatrolRange,
                    spawn.PatrolSpeed,
                    -GrassPathMaxAbsX,
                    GrassPathMaxAbsX,
                    Level1OutdoorAreaBuilder.OutdoorMinZ + 0.8f,
                    Level1OutdoorAreaBuilder.OutdoorMaxZ - 2f);
                EditorUtility.SetDirty(instance);
            }
        }

        private static void WritePatrolSettings(
            Level1OutdoorNpcPatrol patrol,
            Vector3 anchor,
            float range,
            float speed,
            float boundMinX,
            float boundMaxX,
            float boundMinZ,
            float boundMaxZ)
        {
            if (patrol == null)
            {
                return;
            }

            patrol.Configure(
                anchor,
                range,
                speed,
                boundMinX,
                boundMaxX,
                boundMinZ,
                boundMaxZ);

            SerializedObject serialized = new SerializedObject(patrol);
            serialized.FindProperty("anchor").vector3Value = anchor;
            serialized.FindProperty("patrolRange").floatValue = Mathf.Max(0.35f, range);
            serialized.FindProperty("moveSpeed").floatValue = Mathf.Max(0.8f, speed);
            serialized.FindProperty("minX").floatValue = boundMinX;
            serialized.FindProperty("maxX").floatValue = boundMaxX;
            serialized.FindProperty("minZ").floatValue = boundMinZ;
            serialized.FindProperty("maxZ").floatValue = boundMaxZ;
            serialized.FindProperty("phaseOffset").floatValue = UnityEngine.Random.Range(0f, 1f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(patrol);
        }

        private static void AttachNpcDialogue(GameObject npc, string npcName)
        {
            if (npc == null || string.IsNullOrEmpty(npcName))
            {
                return;
            }

            if (!TryResolveDialogueKey(npcName, out string dialogueKey))
            {
                return;
            }

            Level1OutdoorNpcDialogue dialogue = npc.GetComponent<Level1OutdoorNpcDialogue>();
            if (dialogue == null)
            {
                dialogue = npc.AddComponent<Level1OutdoorNpcDialogue>();
            }

            dialogue.Configure(dialogueKey);

            SerializedObject serialized = new SerializedObject(dialogue);
            serialized.FindProperty("dialogueKey").stringValue = dialogueKey;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(dialogue);
        }

        private static bool TryResolveDialogueKey(string npcName, out string dialogueKey)
        {
            dialogueKey = npcName switch
            {
                "CrowdNPC_StallSeller" => "StallSeller",
                "CrowdNPC_FlowerViewer" => "FlowerViewer",
                "CrowdNPC_TalkA" => "TalkPair",
                "CrowdNPC_TalkB" => "TalkPair",
                "CrowdNPC_CasualAlone" => "CasualAlone",
                "CrowdNPC_CasualSecond" => "CasualSecond",
                "CrowdNPC_PatrolA" => "PatrolA",
                "CrowdNPC_PatrolB" => "PatrolB",
                _ => null,
            };

            return !string.IsNullOrEmpty(dialogueKey);
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
            catch (System.Exception)
            {
                // Optional slot groups vary by pack version.
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

        private static Vector3 ClampToGrassPath(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, -GrassPathMaxAbsX, GrassPathMaxAbsX);
            position.y = FootY;

            if (Level1BusStopRoadSettings.OverlapsRoad(position.x, position.z, 0.9f, 0.9f))
            {
                position.x = Mathf.Clamp(position.x, -1.8f, 1.8f);
            }

            return position;
        }

        private static float YawToward(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.001f)
            {
                return 0f;
            }

            return Quaternion.LookRotation(delta.normalized, Vector3.up).eulerAngles.y;
        }
    }
}
#endif
