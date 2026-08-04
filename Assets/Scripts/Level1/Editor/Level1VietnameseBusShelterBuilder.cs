#if UNITY_EDITOR
using ChoTet.Level1;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace ChoTet.Level1.Editor
{
    /// <summary>Minimal Ho Chi Minh City neighborhood bus shelter — waiting point only, not a large station.</summary>
    public static class Level1VietnameseBusShelterBuilder
    {
        public const string RootName = "Level1_VietnameseBusShelter";

        private const float ShelterWidth = 2.5f;
        private const float ShelterHeight = 2.4f;
        private const float ShelterDepth = 1.05f;
        private const float PostSize = 0.055f;
        private const float RoofThickness = 0.045f;
        private const float RoofOverhang = 0.14f;
        private const float BenchSeatHeight = 0.46f;
        private const float SignPoleOffsetX = 1.48f;

        private const string SteelMaterialPath = "Assets/Settings/ChoTet/Materials/Wall_Shadow.mat";
        private const string RoofMaterialPath = "Assets/Settings/ChoTet/Materials/Wall_Light.mat";
        private const string WornSteelMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/BusShelter_WornSteel.mat";
        private const string SignBoardMaterialPath = "Assets/Settings/ChoTet/Materials/Level1/BusShelter_SignBoard.mat";
        private const string DirtMaterialPath = "Assets/Settings/ChoTet/Materials/Home_Dirt.mat";
        private const string TrashMaterialPath = "Assets/Settings/ChoTet/Materials/Wall_Shadow.mat";

        public static void PlaceOnCurrentScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Level1VietnameseBusShelterBuilder] Exit Play mode before placing the bus shelter.");
                return;
            }

            Transform busArea = Level1OutdoorSceneLookup.ResolveBusPickupArea();
            if (busArea == null)
            {
                Debug.LogError("[Level1VietnameseBusShelterBuilder] Level1_BusPickupArea not found.");
                return;
            }

            Build(busArea);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Level1VietnameseBusShelterBuilder] Vietnamese bus shelter placed.");
        }

        public static void Build(Transform busPickupRoot)
        {
            Material steel = EnsureWornSteelMaterial();
            Material roof = AssetDatabase.LoadAssetAtPath<Material>(RoofMaterialPath);
            Material signBoard = EnsureSignBoardMaterial();
            Material dirt = AssetDatabase.LoadAssetAtPath<Material>(DirtMaterialPath);
            Material trash = AssetDatabase.LoadAssetAtPath<Material>(TrashMaterialPath);

            if (roof == null || dirt == null || trash == null)
            {
                Debug.LogError("[Level1VietnameseBusShelterBuilder] Missing base materials.");
                return;
            }

            Vector3 expected = ComputeDefaultPosition();
            Vector3 position = expected;
            Quaternion rotation = Quaternion.identity;

            Transform existing = busPickupRoot.Find(RootName);
            if (existing != null)
            {
                Vector3 existingFlat = new Vector3(existing.position.x, 0f, existing.position.z);
                Vector3 expectedFlat = new Vector3(expected.x, 0f, expected.z);
                position = Vector3.Distance(existingFlat, expectedFlat) > 1.5f
                    ? expected
                    : existing.position;
                rotation = existing.rotation;
                Object.DestroyImmediate(existing.gameObject);
            }
            else
            {
                position = expected;
            }

            GameObject root = new GameObject(RootName);
            root.transform.SetParent(busPickupRoot, false);
            root.transform.position = position;
            root.transform.rotation = rotation;
            root.transform.SetAsLastSibling();

            GameObject frameRoot = new GameObject("Shelter_Frame");
            frameRoot.transform.SetParent(root.transform, false);
            GameObject benchRoot = new GameObject("Shelter_Bench");
            benchRoot.transform.SetParent(root.transform, false);
            GameObject signRoot = new GameObject("Shelter_Sign");
            signRoot.transform.SetParent(root.transform, false);
            GameObject detailRoot = new GameObject("Shelter_Details");
            detailRoot.transform.SetParent(root.transform, false);

            float surfaceY = Level1BusStopRoadSettings.SidewalkSurfaceY;
            float halfW = ShelterWidth * 0.5f;
            float halfD = ShelterDepth * 0.5f;
            float postHeight = ShelterHeight - RoofThickness - 0.02f;

            BuildFrame(frameRoot.transform, steel, surfaceY, halfW, halfD, postHeight);
            BuildRoof(frameRoot.transform, roof, steel, surfaceY, halfW, halfD, postHeight);
            BuildBench(benchRoot.transform, steel, surfaceY, halfW, halfD);
            BuildSign(signRoot.transform, steel, signBoard, surfaceY, halfW, halfD);
            BuildDetails(detailRoot.transform, steel, signBoard, dirt, trash, surfaceY, halfW, halfD);

            MarkStatic(root);
        }

        private static Vector3 ComputeDefaultPosition()
        {
            return new Vector3(
                Level1BusStopRoadSettings.BusShelterCenterX,
                0f,
                Level1BusStopRoadSettings.BusShelterCenterZ);
        }

        private static void BuildFrame(Transform parent, Material steel, float surfaceY, float halfW, float halfD,
            float postHeight)
        {
            Vector3[] corners =
            {
                new Vector3(-halfW, 0f, -halfD),
                new Vector3(halfW, 0f, -halfD),
                new Vector3(-halfW, 0f, halfD),
                new Vector3(halfW, 0f, halfD)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                CreateLocalCube("Post_" + i, parent,
                    corners[i] + new Vector3(0f, surfaceY + postHeight * 0.5f, 0f),
                    new Vector3(PostSize, postHeight, PostSize), steel);
            }

            float beamY = surfaceY + postHeight - 0.03f;
            CreateLocalCube("Beam_Front", parent, new Vector3(0f, beamY, -halfD),
                new Vector3(ShelterWidth + PostSize, PostSize * 0.9f, PostSize * 0.9f), steel);
            CreateLocalCube("Beam_Back", parent, new Vector3(0f, beamY, halfD),
                new Vector3(ShelterWidth + PostSize, PostSize * 0.9f, PostSize * 0.9f), steel);
            CreateLocalCube("Beam_Left", parent, new Vector3(-halfW, beamY, 0f),
                new Vector3(PostSize * 0.9f, PostSize * 0.9f, ShelterDepth), steel);
            CreateLocalCube("Beam_Right", parent, new Vector3(halfW, beamY, 0f),
                new Vector3(PostSize * 0.9f, PostSize * 0.9f, ShelterDepth), steel);
        }

        private static void BuildRoof(Transform parent, Material roof, Material steel, float surfaceY, float halfW,
            float halfD, float postHeight)
        {
            float roofY = surfaceY + postHeight + RoofThickness * 0.5f + 0.01f;
            CreateLocalCube("Roof_Slab", parent, new Vector3(0f, roofY, -0.02f),
                new Vector3(ShelterWidth + RoofOverhang * 2f, RoofThickness, ShelterDepth + RoofOverhang * 1.6f),
                roof);

            CreateLocalCube("Roof_Gutter", parent, new Vector3(0f, roofY - RoofThickness * 0.35f, -halfD - 0.05f),
                new Vector3(ShelterWidth + RoofOverhang * 1.4f, PostSize * 0.7f, PostSize * 0.8f), steel);
        }

        private static void BuildBench(Transform parent, Material steel, float surfaceY, float halfW, float halfD)
        {
            float seatY = surfaceY + BenchSeatHeight;
            float seatZ = halfD * 0.18f;
            float seatW = ShelterWidth * 0.62f;

            CreateLocalCube("Bench_Seat", parent, new Vector3(0f, seatY, seatZ),
                new Vector3(seatW, 0.05f, 0.34f), steel);
            CreateLocalCube("Bench_Back", parent, new Vector3(0f, seatY + 0.28f, seatZ + 0.18f),
                new Vector3(seatW, 0.34f, 0.04f), steel);

            float legInset = seatW * 0.38f;
            CreateLocalCube("Bench_Leg_L", parent, new Vector3(-legInset, surfaceY + BenchSeatHeight * 0.5f, seatZ),
                new Vector3(0.05f, BenchSeatHeight, 0.05f), steel);
            CreateLocalCube("Bench_Leg_R", parent, new Vector3(legInset, surfaceY + BenchSeatHeight * 0.5f, seatZ),
                new Vector3(0.05f, BenchSeatHeight, 0.05f), steel);
            CreateLocalCube("Bench_Leg_C", parent, new Vector3(0f, surfaceY + BenchSeatHeight * 0.5f, seatZ - 0.12f),
                new Vector3(0.05f, BenchSeatHeight, 0.05f), steel);
        }

        private static void BuildSign(Transform parent, Material steel, Material signBoard, float surfaceY, float halfW,
            float halfD)
        {
            float poleX = halfW + SignPoleOffsetX;
            float poleZ = -halfD * 0.15f;
            float poleHeight = 2.05f;

            CreateLocalCube("Sign_Post", parent, new Vector3(poleX, surfaceY + poleHeight * 0.5f, poleZ),
                new Vector3(0.06f, poleHeight, 0.06f), steel);
            CreateLocalCube("Sign_Base", parent, new Vector3(poleX, surfaceY + 0.03f, poleZ),
                new Vector3(0.18f, 0.06f, 0.18f), steel);

            float boardY = surfaceY + 1.72f;
            GameObject faceRoot = new GameObject("Sign_Face");
            faceRoot.transform.SetParent(parent, false);
            faceRoot.transform.localPosition = new Vector3(poleX, boardY, poleZ);
            faceRoot.transform.localRotation = Quaternion.identity;

            Material boardMat = EnsureSignBoardMaterial();
            CreateLocalCube("Sign_Board", faceRoot.transform, new Vector3(0f, 0f, -0.02f),
                new Vector3(0.44f, 0.64f, 0.035f), boardMat);
            CreateLocalCube("Sign_Frame_Top", faceRoot.transform, new Vector3(0f, 0.33f, -0.02f),
                new Vector3(0.46f, 0.025f, 0.038f), steel);
            CreateLocalCube("Sign_Frame_Bottom", faceRoot.transform, new Vector3(0f, -0.33f, -0.02f),
                new Vector3(0.46f, 0.025f, 0.038f), steel);
            CreateLocalCube("Sign_Frame_Left", faceRoot.transform, new Vector3(-0.215f, 0f, -0.02f),
                new Vector3(0.025f, 0.67f, 0.038f), steel);
            CreateLocalCube("Sign_Frame_Right", faceRoot.transform, new Vector3(0.215f, 0f, -0.02f),
                new Vector3(0.025f, 0.67f, 0.038f), steel);

            CreateBusIcon(faceRoot.transform, new Vector3(-0.11f, 0.12f, -0.045f));
            CreateSignLabel(faceRoot.transform, "Sign_Title", "TRẠM XE BUÝT", new Vector3(0f, -0.02f, -0.05f),
                0.075f, FontStyles.Bold);
            CreateSignLabel(faceRoot.transform, "Sign_Routes", "08    19    93", new Vector3(0f, -0.2f, -0.05f),
                0.095f, FontStyles.Normal);
        }

        private static void CreateBusIcon(Transform parent, Vector3 localPos)
        {
            Material busBlue = EnsureBusBlueMaterial();
            Material busDark = AssetDatabase.LoadAssetAtPath<Material>(SteelMaterialPath);

            CreateLocalCube("BusIcon_Body", parent, localPos, new Vector3(0.16f, 0.11f, 0.02f), busBlue);
            CreateLocalCube("BusIcon_Window", parent, localPos + new Vector3(0f, 0.02f, 0.012f),
                new Vector3(0.11f, 0.05f, 0.01f), busDark);
        }

        private static void BuildDetails(Transform parent, Material steel, Material signBoard, Material dirt,
            Material trash, float surfaceY, float halfW, float halfD)
        {
            float boardX = -halfW + 0.18f;
            float boardZ = halfD - 0.08f;
            CreateLocalCube("Timetable_Board", parent, new Vector3(boardX, surfaceY + 1.35f, boardZ),
                new Vector3(0.34f, 0.42f, 0.02f), signBoard);
            CreateLocalCube("Timetable_Frame", parent, new Vector3(boardX, surfaceY + 1.35f, boardZ - 0.01f),
                new Vector3(0.36f, 0.44f, 0.015f), steel);
            CreateSignLabel(parent, "Timetable_Label", "LỊCH TRÌNH", new Vector3(boardX, surfaceY + 1.48f, boardZ + 0.02f),
                0.045f, FontStyles.Bold);

            float binX = halfW - 0.22f;
            float binZ = halfD - 0.12f;
            CreateLocalCube("Trash_Bin", parent, new Vector3(binX, surfaceY + 0.28f, binZ),
                new Vector3(0.22f, 0.42f, 0.22f), trash);
            CreateLocalCube("Trash_Lid", parent, new Vector3(binX, surfaceY + 0.52f, binZ),
                new Vector3(0.24f, 0.04f, 0.24f), steel);

            CreateLocalCube("Dust_Base_L", parent, new Vector3(-halfW + 0.2f, surfaceY + 0.005f, -halfD + 0.12f),
                new Vector3(0.42f, 0.01f, 0.28f), dirt);
            CreateLocalCube("Dust_Base_R", parent, new Vector3(halfW - 0.25f, surfaceY + 0.005f, halfD - 0.1f),
                new Vector3(0.36f, 0.01f, 0.24f), dirt);
            CreateLocalCube("Wear_Scratch", parent, new Vector3(0.05f, surfaceY + 0.52f, -halfD + 0.04f),
                new Vector3(0.55f, 0.008f, 0.02f), dirt);
        }

        private static void CreateSignLabel(Transform parent, string name, string text, Vector3 localPos,
            float fontSize, FontStyles style)
        {
            GameObject label = new GameObject(name);
            label.transform.SetParent(parent, false);
            label.transform.localPosition = localPos;
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TextMeshPro tmp = label.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize * 10f;
            tmp.color = style == FontStyles.Bold ? new Color(0.12f, 0.14f, 0.16f, 1f) : new Color(0.2f, 0.22f, 0.24f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = style;
            tmp.rectTransform.sizeDelta = new Vector2(0.5f, 0.2f);
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            Renderer renderer = label.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material EnsureWornSteelMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(WornSteelMaterialPath);
            if (mat != null)
            {
                return mat;
            }

            Material baseSteel = AssetDatabase.LoadAssetAtPath<Material>(SteelMaterialPath);
            mat = baseSteel != null ? new Material(baseSteel) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "BusShelter_WornSteel";
            mat.SetColor("_BaseColor", new Color(0.28f, 0.3f, 0.33f, 1f));
            mat.SetFloat("_Smoothness", 0.18f);
            AssetDatabase.CreateAsset(mat, WornSteelMaterialPath);
            return mat;
        }

        private static Material EnsureSignBoardMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(SignBoardMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (mat == null)
            {
                mat = new Material(shader);
                mat.name = "BusShelter_SignBoard";
                AssetDatabase.CreateAsset(mat, SignBoardMaterialPath);
            }
            else if (mat.shader != shader && shader != null)
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", new Color(0.94f, 0.94f, 0.91f, 1f));
            mat.SetColor("_Color", new Color(0.94f, 0.94f, 0.91f, 1f));
            mat.SetFloat("_Cull", (float)CullMode.Off);
            mat.renderQueue = 3000;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material EnsureBusBlueMaterial()
        {
            const string path = "Assets/Settings/ChoTet/Materials/Level1/BusShelter_BusBlue.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                return mat;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            mat.name = "BusShelter_BusBlue";
            mat.SetColor("_BaseColor", new Color(0.12f, 0.36f, 0.72f, 1f));
            mat.SetFloat("_Smoothness", 0.2f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static GameObject CreateLocalCube(string name, Transform parent, Vector3 localPos, Vector3 scale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = scale;
            return FinishCube(cube, material);
        }

        private static GameObject FinishCube(GameObject cube, Material material)
        {
            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return cube;
        }

        private static void MarkStatic(GameObject root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObjectUtility.SetStaticEditorFlags(transforms[i].gameObject,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            }
        }
    }
}
#endif
