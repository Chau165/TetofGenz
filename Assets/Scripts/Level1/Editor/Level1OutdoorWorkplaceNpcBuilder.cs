#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ChoTet.Editor;
using ChoTet.Level1;
using ChoTet.Level1.Highway;
using ChoTet.Runtime;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.Character;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>Role-based workplace NPCs for the Level 1 outdoor alley — one polished NPC per shop.</summary>
    public static class Level1OutdoorWorkplaceNpcBuilder
    {
        public const string RootName = "Level1_OutdoorWorkplaceNpcs";
        private const string LegacyCrowdName = "Level1_OutdoorCrowd";
        private const float DefaultNpcScale = 0.68f;
        private const float ElderlyNpcScale = 0.62f;

        private static readonly List<Vector3> PlacedNpcFeet = new List<Vector3>();

        private struct WorkplaceSpawn
        {
            public string ObjectName;
            public Vector3? WorldPosition;
            public Func<Transform, Vector3> ResolvePosition;
            public float Yaw;
            public Func<Transform, float> ResolveYaw;
            public float Scale;
            public Action<CustomizableCharacter> ApplyLook;
            public Color ShirtTint;
            public Color PantsTint;
            public string NpcName;
            public string[] Dialogue;
            public float LookPhase;
            public float LookAmount;
        }

        public static void RebuildAllOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorWorkplaceNpcBuilder] Exit Play mode before rebuilding NPCs.");
                return;
            }

            Transform outdoor = ResolveOutdoorRoot();
            if (outdoor == null)
            {
                return;
            }

            ClearLegacyHumans(outdoor);
            Transform root = EnsureRoot(outdoor);
            PlacedNpcFeet.Clear();
            ApplyCircleKClosedState(outdoor);

            BuildCoLien(root);
            BuildPawnShopOwner(root);
            BuildComTamOwner(root);
            BuildCarWashOwner(root);
            BuildMobileStoreStaff(root);
            BuildPharmacyEmployee(root);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Level1OutdoorWorkplaceNpcBuilder] All workplace NPCs rebuilt under " + RootName + ".");
        }

        public static void BuildCoLienMenu()
        {
            if (!PrepareSingleNpcBuild(out Transform root))
            {
                return;
            }

            BuildCoLien(root);
            MarkDirty();
        }

        public static void BuildPawnShopOwnerMenu()
        {
            if (!PrepareSingleNpcBuild(out Transform root))
            {
                return;
            }

            BuildPawnShopOwner(root);
            MarkDirty();
        }

        public static void BuildComTamOwnerMenu()
        {
            if (!PrepareSingleNpcBuild(out Transform root))
            {
                return;
            }

            BuildComTamOwner(root);
            MarkDirty();
        }

        public static void ApplyCircleKClosedMenu()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            Transform outdoor = ResolveOutdoorRoot();
            if (outdoor == null)
            {
                return;
            }

            ApplyCircleKClosedState(outdoor);
            MarkDirty();
        }

        public static void BuildCarWashOwnerMenu()
        {
            if (!PrepareSingleNpcBuild(out Transform root))
            {
                return;
            }

            BuildCarWashOwner(root);
            MarkDirty();
        }

        public static void BuildMobileStoreStaffMenu()
        {
            if (!PrepareSingleNpcBuild(out Transform root))
            {
                return;
            }

            BuildMobileStoreStaff(root);
            MarkDirty();
        }

        public static void BuildPharmacyEmployeeMenu()
        {
            if (!PrepareSingleNpcBuild(out Transform root))
            {
                return;
            }

            BuildPharmacyEmployee(root);
            MarkDirty();
        }

        public static void Build(Transform outdoorRoot)
        {
            if (outdoorRoot == null)
            {
                return;
            }

            ClearLegacyHumans(outdoorRoot);
            Transform root = EnsureRoot(outdoorRoot);
            PlacedNpcFeet.Clear();
            ApplyCircleKClosedState(outdoorRoot);
            BuildCoLien(root);
            BuildPawnShopOwner(root);
            BuildComTamOwner(root);
            BuildCarWashOwner(root);
            BuildMobileStoreStaff(root);
            BuildPharmacyEmployee(root);
        }

        private static bool PrepareSingleNpcBuild(out Transform root)
        {
            root = null;
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1OutdoorWorkplaceNpcBuilder] Exit Play mode before building NPCs.");
                return false;
            }

            Transform outdoor = ResolveOutdoorRoot();
            if (outdoor == null)
            {
                return false;
            }

            ClearLegacyHumans(outdoor);
            root = EnsureRoot(outdoor);
            return true;
        }

        private static void MarkDirty()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static Transform ResolveOutdoorRoot()
        {
            GameObject outdoor = GameObject.Find("Level1_OutdoorArea");
            if (outdoor != null)
            {
                return outdoor.transform;
            }

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform deep = FindDeep(roots[i].transform, "Level1_OutdoorArea");
                if (deep != null)
                {
                    return deep;
                }
            }

            Debug.LogError("[Level1OutdoorWorkplaceNpcBuilder] Level1_OutdoorArea not found.");
            return null;
        }

        private static void ClearLegacyHumans(Transform outdoor)
        {
            DestroyChild(outdoor, LegacyCrowdName);
            DestroyChild(outdoor, RootName);
        }

        private static Transform EnsureRoot(Transform outdoor)
        {
            GameObject root = new GameObject(RootName);
            root.transform.SetParent(outdoor, false);
            return root.transform;
        }

        private static void DestroyChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void BuildCoLien(Transform root)
        {
            Transform house = FindDeep(root.parent, "Level1_HouseExterior");
            WorkplaceSpawn spawn = new WorkplaceSpawn
            {
                ObjectName = "NPC_CoLien",
                ResolvePosition = _ =>
                {
                    if (house != null)
                    {
                        Transform door = house.Find("DoorPanel") ?? house.Find("DoorFrameRight");
                        if (door != null)
                        {
                            Vector3 beside = door.position + house.right * 1.15f + house.forward * 0.5f;
                            beside.y = Level1OutdoorSettings.WalkSurfaceY;
                            return beside;
                        }
                    }

                    return new Vector3(1.12f, Level1OutdoorSettings.WalkSurfaceY, -3.82f);
                },
                ResolveYaw = _ =>
                {
                    if (house != null)
                    {
                        return Quaternion.LookRotation(house.forward, Vector3.up).eulerAngles.y;
                    }

                    return 0f;
                },
                Scale = ElderlyNpcScale,
                ApplyLook = ApplyLookCoLien,
                ShirtTint = new Color(0.94f, 0.55f, 0.62f),
                PantsTint = new Color(0.16f, 0.17f, 0.2f),
                NpcName = "Cô Liên",
                Dialogue = new[]
                {
                    "Con tìm phòng trọ hả?",
                    "Phòng tầng trên vẫn còn một phòng trống.",
                    "Ở đây nhớ khóa cửa và giữ yên lặng sau mười giờ tối nha.",
                    "Có chuyện gì thì gọi cô.",
                },
                LookPhase = 0.4f,
                LookAmount = 16f,
            };
            SpawnWorkplaceNpc(root, spawn);
        }

        private static void BuildPawnShopOwner(Transform root)
        {
            Transform shop = FindShop(root.parent, Level1VietnamesePawnShopBuilder.RootName);
            WorkplaceSpawn spawn = new WorkplaceSpawn
            {
                ObjectName = "NPC_PawnShopOwner",
                ResolvePosition = t => BehindNamedAnchor(t, "Interior_Counter", inward: 0.58f, lateral: 0f),
                ResolveYaw = FacadeFaceYaw,
                Scale = DefaultNpcScale,
                ApplyLook = ApplyLookPawnOwner,
                ShirtTint = new Color(0.42f, 0.48f, 0.56f),
                PantsTint = new Color(0.14f, 0.16f, 0.2f),
                NpcName = "Chú chủ tiệm cầm đồ",
                Dialogue = new[]
                {
                    "Cần hỏi giá món gì thì cứ đem vào chú xem.",
                    "Có giấy tờ rõ ràng thì làm nhanh thôi.",
                    "Nhớ đọc kỹ giấy biên nhận trước khi ký.",
                },
                LookPhase = 1.15f,
                LookAmount = 11f,
            };
            SpawnWorkplaceNpc(root, spawn, shop);
        }

        private static void BuildComTamOwner(Transform root)
        {
            Transform shop = FindShop(root.parent, Level1VietnameseComTamShopBuilder.RootName);
            WorkplaceSpawn spawn = new WorkplaceSpawn
            {
                ObjectName = "NPC_ComTamOwner",
                ResolvePosition = t => BehindNamedAnchor(t, "Prep_Counter", inward: 0.58f, lateral: -0.12f),
                ResolveYaw = FacadeFaceYaw,
                Scale = DefaultNpcScale,
                ApplyLook = ApplyLookComTamOwner,
                ShirtTint = new Color(0.92f, 0.9f, 0.86f),
                PantsTint = new Color(0.2f, 0.22f, 0.26f),
                NpcName = "Chủ quán cơm tấm",
                Dialogue = new[]
                {
                    "Cơm tấm nóng đây, con ăn gì?",
                    "Có sườn, bì, chả với trứng ốp la.",
                    "Ngồi đợi cô chú làm một chút nha.",
                    "Ăn no rồi hãy đi tiếp.",
                },
                LookPhase = 2.05f,
                LookAmount = 10f,
            };
            SpawnWorkplaceNpc(root, spawn, shop);
        }

        private static void BuildCarWashOwner(Transform root)
        {
            Transform shop = FindShop(root.parent, Level1VietnameseCarWashBuilder.RootName);
            WorkplaceSpawn spawn = new WorkplaceSpawn
            {
                ObjectName = "NPC_CarWashOwner",
                ResolvePosition = t => BesideNamedAnchor(t, "Wash_Vehicle", lateral: 1.05f, forward: 0.35f),
                ResolveYaw = FacadeFaceYaw,
                Scale = DefaultNpcScale,
                ApplyLook = ApplyLookCarWashOwner,
                ShirtTint = new Color(0.55f, 0.58f, 0.6f),
                PantsTint = new Color(0.28f, 0.3f, 0.34f),
                NpcName = "Chú rửa xe",
                Dialogue = new[]
                {
                    "Rửa xe hả em?",
                    "Dựng xe sát bên này giúp chú.",
                    "Khoảng mười lăm phút là xong.",
                    "Có cần bơm bánh với tra dầu sên luôn không?",
                },
                LookPhase = 0.85f,
                LookAmount = 12f,
            };
            SpawnWorkplaceNpc(root, spawn, shop);
        }

        private static void BuildMobileStoreStaff(Transform root)
        {
            Transform shop = FindShop(root.parent, Level1TheGioiMobileStoreBuilder.RootName);
            string[] dialogue =
            {
                "Anh chị vào xem điện thoại không ạ?",
                "Bên em đang có chương trình trả góp.",
                "Mẫu này pin tốt, dùng hằng ngày ổn lắm.",
                "Anh chị cần em tư vấn theo tầm giá không?",
            };

            WorkplaceSpawn[] staff =
            {
                new WorkplaceSpawn
                {
                    ObjectName = "NPC_MobileStaff_Door",
                    ResolvePosition = t => BesideNamedAnchor(t, "Mobile_Glass_Door_Left", lateral: 0.78f, forward: 0.58f),
                    ResolveYaw = FacadeFaceYaw,
                    Scale = DefaultNpcScale * 0.98f,
                    ApplyLook = c => ApplyLookMobileStaff(c, face: 0, hair: 1),
                    ShirtTint = new Color(0.98f, 0.84f, 0.08f),
                    PantsTint = new Color(0.1f, 0.11f, 0.14f),
                    NpcName = "Nhân viên cửa hàng",
                    Dialogue = dialogue,
                    LookPhase = 0.2f,
                    LookAmount = 14f,
                },
                new WorkplaceSpawn
                {
                    ObjectName = "NPC_MobileStaff_Brochure",
                    ResolvePosition = t => BesideNamedAnchor(t, "DisplayCounter_1", lateral: -0.92f, forward: 0.62f),
                    ResolveYaw = t => FacadeFaceYaw(t) + 18f,
                    Scale = DefaultNpcScale * 0.96f,
                    ApplyLook = c => ApplyLookMobileStaff(c, face: 2, hair: 0),
                    ShirtTint = new Color(0.96f, 0.82f, 0.1f),
                    PantsTint = new Color(0.12f, 0.12f, 0.15f),
                    NpcName = "Nhân viên cửa hàng",
                    Dialogue = dialogue,
                    LookPhase = 1.7f,
                    LookAmount = 8f,
                },
            };

            for (int i = 0; i < staff.Length; i++)
            {
                SpawnWorkplaceNpc(root, staff[i], shop);
            }
        }

        private static void BuildPharmacyEmployee(Transform root)
        {
            Transform shop = FindShop(root.parent, Level1VietnamesePharmacyBuilder.RootName);
            WorkplaceSpawn spawn = new WorkplaceSpawn
            {
                ObjectName = "NPC_PharmacyEmployee",
                ResolvePosition = t => BehindNamedAnchor(t, "Cashier_Counter", inward: 0.55f, lateral: 0.1f),
                ResolveYaw = FacadeFaceYaw,
                Scale = DefaultNpcScale,
                ApplyLook = ApplyLookPharmacyEmployee,
                ShirtTint = new Color(0.94f, 0.96f, 0.98f),
                PantsTint = new Color(0.22f, 0.24f, 0.3f),
                NpcName = "Nhân viên nhà thuốc",
                Dialogue = new[]
                {
                    "Anh chị cần mua thuốc gì ạ?",
                    "Mình có đang dùng thuốc nào khác không?",
                    "Thuốc này nhớ dùng đúng liều ghi trên hướng dẫn.",
                    "Nếu triệu chứng kéo dài thì nên đi khám bác sĩ.",
                },
                LookPhase = 2.6f,
                LookAmount = 9f,
            };
            SpawnWorkplaceNpc(root, spawn, shop);
        }

        public static void ApplyCircleKClosedState(Transform outdoor)
        {
            Transform store = FindDeep(outdoor, Level1ConvenienceStoreFacadeBuilder.RootName);
            if (store == null)
            {
                Debug.LogWarning("[Level1OutdoorWorkplaceNpcBuilder] ConvenienceStoreFacade not found — skip closed overlay.");
                return;
            }

            SlidingGlassDoor[] doors = store.GetComponentsInChildren<SlidingGlassDoor>(true);
            for (int i = 0; i < doors.Length; i++)
            {
                doors[i].enabled = false;
            }

            Collider[] doorTriggers = store.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < doorTriggers.Length; i++)
            {
                Collider col = doorTriggers[i];
                if (col != null && col.gameObject.name.Contains("Door_ProximityTrigger"))
                {
                    col.enabled = false;
                }
            }

            Light[] lights = store.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = false;
            }

            Renderer[] renderers = store.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                string n = renderer.gameObject.name;
                if (n.Contains("WindowFill") || n.Contains("WindowGlow") || n.Contains("Interior_"))
                {
                    Material[] mats = renderer.sharedMaterials;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m] == null)
                        {
                            continue;
                        }

                        Material inst = new Material(mats[m]);
                        if (inst.HasProperty("_BaseColor"))
                        {
                            Color c = inst.GetColor("_BaseColor");
                            inst.SetColor("_BaseColor", c * 0.28f);
                        }
                        else if (inst.HasProperty("_Color"))
                        {
                            Color c = inst.GetColor("_Color");
                            inst.SetColor("_Color", c * 0.28f);
                        }

                        mats[m] = inst;
                    }

                    renderer.sharedMaterials = mats;
                }
            }

            Transform overlay = store.Find("ClosedOverlay");
            if (overlay != null)
            {
                UnityEngine.Object.DestroyImmediate(overlay.gameObject);
            }

            GameObject overlayRoot = new GameObject("ClosedOverlay");
            overlayRoot.transform.SetParent(store, false);

            float signY = Level1OutdoorSettings.WalkSurfaceY + 1.55f;
            Vector3 signLocal = store.InverseTransformPoint(store.position + store.forward * -0.95f + Vector3.up * signY);
            signLocal.y = signY - store.position.y;

            Bounds bounds = ComputeWorldBounds(store.gameObject);
            Vector3 signWorld = bounds.center - FacadeOutward(store) * (bounds.extents.z + 0.08f);
            signWorld.y = Level1OutdoorSettings.WalkSurfaceY + 1.62f;

            BuildClosedSign(overlayRoot.transform, signWorld, FacadeFaceYaw(store));

            EditorUtility.SetDirty(store.gameObject);
        }

        private static void BuildClosedSign(Transform parent, Vector3 worldPos, float yaw)
        {
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "ClosedSignBoard";
            UnityEngine.Object.DestroyImmediate(board.GetComponent<Collider>());
            board.transform.SetParent(parent, true);
            board.transform.position = worldPos;
            board.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            board.transform.localScale = new Vector3(1.35f, 0.52f, 0.03f);

            Material boardMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            boardMat.color = new Color(0.98f, 0.95f, 0.88f);
            board.GetComponent<Renderer>().sharedMaterial = boardMat;

            GameObject textObj = new GameObject("ClosedSignText");
            textObj.transform.SetParent(board.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.52f);
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one;

            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.font = HighwayHudTypography.RegularFont != null
                ? HighwayHudTypography.RegularFont
                : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Level1/Fonts/Nunito-Regular SDF.asset");
            tmp.text = "CỬA HÀNG ĐÃ ĐÓNG CỬA\n<size=70%>KHU VỰC SẮP GIẢI PHÓNG MẶT BẰNG</size>";
            tmp.fontSize = 2.8f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.78f, 0.12f, 0.1f);
            tmp.rectTransform.sizeDelta = new Vector2(3.2f, 1.2f);
            tmp.textWrappingMode = TextWrappingModes.Normal;

            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(board);
            GameObjectUtility.SetStaticEditorFlags(board, flags | StaticEditorFlags.BatchingStatic);
        }

        private static void SpawnWorkplaceNpc(Transform root, WorkplaceSpawn spawn, Transform shopAnchor = null)
        {
            if (SlotLibraryLoader.LoadSlotLibrary() == null)
            {
                Debug.LogWarning("[Level1OutdoorWorkplaceNpcBuilder] Creative Characters pack missing.");
                return;
            }

            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                PlayerCharacterVisualPaths.CreativeCharacterTopDownController);
            if (controller == null)
            {
                Debug.LogWarning("[Level1OutdoorWorkplaceNpcBuilder] Top-down animator missing.");
                return;
            }

            Transform existing = root.Find(spawn.ObjectName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject instance = CreativeCharacterAssembly.BuildDressedCharacter(spawn.ApplyLook, spawn.ObjectName);
            if (instance == null)
            {
                Debug.LogWarning("[Level1OutdoorWorkplaceNpcBuilder] Failed to build " + spawn.ObjectName);
                return;
            }

            CreativeCharacterAssembly.ApplyTintedOutfit(instance, spawn.ShirtTint, spawn.PantsTint);
            CreativeCharacterAssembly.PrepareStaticWorkplaceNpc(instance, controller, spawn.Scale);

            Vector3 desiredPosition = spawn.WorldPosition ?? spawn.ResolvePosition?.Invoke(shopAnchor) ?? Vector3.zero;
            desiredPosition.y = Level1OutdoorSettings.WalkSurfaceY;
            if (!NpcPlacementValidator.TryFindValidPosition(
                    desiredPosition,
                    spawn.Scale,
                    PlacedNpcFeet,
                    out Vector3 position))
            {
                position = desiredPosition;
                NpcPlacementValidator.TrySampleGround(position, out position);
            }

            PlacedNpcFeet.Add(position);

            float yaw = spawn.ResolveYaw != null && shopAnchor != null
                ? spawn.ResolveYaw(shopAnchor)
                : spawn.Yaw;

            instance.transform.SetParent(root, false);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = Vector3.one * spawn.Scale;

            AttachNpcComponents(instance, spawn);

            NpcGroundAlignment alignment = instance.AddComponent<NpcGroundAlignment>();
            Transform visualRoot = instance.transform.Find("VisualRoot");
            if (visualRoot != null)
            {
                visualRoot.localPosition = Vector3.zero;
            }

            alignment.Configure(visualRoot, runOnStart: false);
            alignment.AlignToGround();

            Debug.Log($"[Level1OutdoorWorkplaceNpcBuilder] Placed {spawn.ObjectName} at {position} yaw={yaw:F1}");
        }

        private static void AttachNpcComponents(GameObject npc, WorkplaceSpawn spawn)
        {
            NPCDialogue dialogue = npc.AddComponent<NPCDialogue>();
            dialogue.Configure(spawn.NpcName, spawn.Dialogue, 1.55f);

            NPCIdleLookVariation look = npc.AddComponent<NPCIdleLookVariation>();
            look.Configure(spawn.LookAmount, 0.26f + UnityEngine.Random.Range(0f, 0.12f), spawn.LookPhase);

            Animator animator = npc.GetComponentInChildren<Animator>(true);
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));
                animator.Update(0f);
            }

            SerializedObject serialized = new SerializedObject(dialogue);
            serialized.FindProperty("animator").objectReferenceValue = animator;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(npc);
        }

        private static Transform FindShop(Transform outdoor, string shopName)
        {
            Transform shop = FindDeep(outdoor, shopName);
            if (shop == null)
            {
                Debug.LogWarning("[Level1OutdoorWorkplaceNpcBuilder] Shop not found: " + shopName);
            }

            return shop;
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Vector3 FacadeOutward(Transform shop)
        {
            float x = shop.position.x;
            if (Mathf.Abs(x) > 0.35f)
            {
                return x > 0f ? Vector3.left : Vector3.right;
            }

            Vector3 forward = shop.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.01f ? forward.normalized : Vector3.forward;
        }

        private static float FacadeFaceYaw(Transform shop)
        {
            Vector3 face = FacadeOutward(shop);
            return Quaternion.LookRotation(face, Vector3.up).eulerAngles.y;
        }

        private static Vector3 ProjectBoundsCorner(Bounds bounds, Vector3 direction)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            Vector3 maxCorner = center;
            float maxDot = float.NegativeInfinity;
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    for (int sz = -1; sz <= 1; sz += 2)
                    {
                        Vector3 corner = center + new Vector3(extents.x * sx, extents.y * sy, extents.z * sz);
                        float dot = Vector3.Dot(corner - center, direction);
                        if (dot > maxDot)
                        {
                            maxDot = dot;
                            maxCorner = corner;
                        }
                    }
                }
            }

            return maxCorner;
        }

        private static Vector3 BesideNamedAnchor(Transform shop, string anchorName, float lateral, float forward)
        {
            Transform anchor = FindDeep(shop, anchorName);
            Vector3 outward = FacadeOutward(shop);
            Vector3 right = Vector3.Cross(Vector3.up, outward).normalized;
            Vector3 origin = anchor != null ? anchor.position : shop.position;
            Vector3 pos = origin + right * lateral + outward * forward;
            pos.y = Level1OutdoorSettings.WalkSurfaceY;
            return pos;
        }

        private static Vector3 BehindNamedAnchor(Transform shop, string anchorName, float inward, float lateral)
        {
            Transform anchor = FindDeep(shop, anchorName);
            Vector3 outward = FacadeOutward(shop);
            Vector3 right = Vector3.Cross(Vector3.up, outward).normalized;
            Vector3 origin = anchor != null ? anchor.position : shop.position;
            Vector3 pos = origin - outward * inward + right * lateral;
            pos.y = Level1OutdoorSettings.WalkSurfaceY;
            return pos;
        }

        private static Vector3 BesideFacade(Transform shop, float sideOffset, float frontOffset)
        {
            Vector3 outward = FacadeOutward(shop);
            Vector3 right = Vector3.Cross(Vector3.up, outward).normalized;
            Bounds bounds = ComputeWorldBounds(shop.gameObject);
            Vector3 facade = ProjectBoundsCorner(bounds, outward);
            Vector3 pos = facade + right * sideOffset + outward * frontOffset;
            pos.y = Level1OutdoorSettings.WalkSurfaceY;
            return pos;
        }

        private static Vector3 BehindCounter(Transform shop, float depth, float lateral)
        {
            Vector3 outward = FacadeOutward(shop);
            Vector3 right = Vector3.Cross(Vector3.up, outward).normalized;
            Bounds bounds = ComputeWorldBounds(shop.gameObject);
            Vector3 facade = ProjectBoundsCorner(bounds, outward);
            Vector3 pos = facade - outward * depth + right * lateral;
            pos.y = Level1OutdoorSettings.WalkSurfaceY;
            return pos;
        }

        private static Vector3 InsideShop(Transform shop, float depth, float lateral)
        {
            return BehindCounter(shop, depth, lateral);
        }

        private static Bounds ComputeWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                bounds = new Bounds(root.transform.position, Vector3.one);
            }

            return bounds;
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
            bool useSingleHairstyle = false)
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
        }

        private static void ApplyLookCoLien(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 2, outwear: 0, pants: 0, shoes: 1, hairstyle: 2, useSingleHairstyle: true);

        private static void ApplyLookPawnOwner(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 0, outwear: 1, pants: 1, shoes: 1, hairstyle: 0);

        private static void ApplyLookComTamOwner(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 1, outwear: 2, pants: 2, shoes: 2, hairstyle: 1);

        private static void ApplyLookCarWashOwner(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 0, outwear: 2, pants: 1, shoes: 0, hairstyle: 0);

        private static void ApplyLookMobileStaff(CustomizableCharacter c, int face, int hair) =>
            ApplyCasualLook(c, face: face, outwear: 0, pants: 0, shoes: 0, hairstyle: hair);

        private static void ApplyLookPharmacyEmployee(CustomizableCharacter c) =>
            ApplyCasualLook(c, face: 2, outwear: 0, pants: 1, shoes: 0, hairstyle: 2, useSingleHairstyle: true);
    }
}
#endif
