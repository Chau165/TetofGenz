using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;

public static class PlayerVisualPolishFixer
{
    private const string PlayerPrefabPath = "Assets/Characters/Player/RiggedExplorer_FromOldProject/RiggedExplorer_PlayerVisual.prefab";
    private const string PolishFolder = "Assets/Materials/Characters/PlayerPolish";
    private const string AutoRunFlagPath = "Assets/Editor/RunPlayerVisualPolishFixer.flag";

    [InitializeOnLoadMethod]
    private static void AutoRunWhenFlagged()
    {
        if (!System.IO.File.Exists(AutoRunFlagPath))
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (!System.IO.File.Exists(AutoRunFlagPath))
            {
                return;
            }

            System.IO.File.Delete(AutoRunFlagPath);
            FixPlayerVisualPolish();
        };
    }

    [MenuItem("Tools/ChoTet/Fix Player Visual Polish")]
    public static void FixPlayerVisualPolish()
    {
        EnsureMaterials();

        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            Transform head = FindDeepChild(root.transform, "Head");
            if (head == null)
            {
                Debug.LogError("Player visual polish fix failed: Head bone was not found.");
                return;
            }

            DestroyOutlineClones(root.transform);

            Transform faceDetails = GetOrCreateChild(head, "FaceDetails");
            faceDetails.localPosition = Vector3.zero;
            faceDetails.localRotation = Quaternion.identity;
            faceDetails.localScale = Vector3.one;

            Material eyeMaterial = LoadMaterial("M_Player_Eye_URP.mat");
            Material eyebrowMaterial = LoadMaterial("M_Player_Eyebrow_URP.mat");
            Material noseMaterial = LoadMaterial("M_Player_Nose_URP.mat");
            Material mouthMaterial = LoadMaterial("M_Player_Mouth_URP.mat");
            Material hairMaterial = LoadMaterial("M_Player_HairAccent_URP.mat");
            Material outlineMaterial = LoadMaterial("M_Character_Outline_URP.mat");

            Transform leftEye = FindExistingOrCreatePrimitive(root.transform, faceDetails, "LeftEye", "StylizedEye_L", PrimitiveType.Sphere);
            ConfigurePart(leftEye, "StylizedEye_L", new Vector3(-4.2f, -0.35f, 17.15f), Quaternion.identity, new Vector3(1.45f, 1.05f, 0.35f), eyeMaterial);

            Transform rightEye = FindExistingOrCreatePrimitive(root.transform, faceDetails, "RightEye", "StylizedEye_R", PrimitiveType.Sphere);
            ConfigurePart(rightEye, "StylizedEye_R", new Vector3(4.2f, -0.35f, 17.15f), Quaternion.identity, new Vector3(1.45f, 1.05f, 0.35f), eyeMaterial);

            Transform leftEyebrow = FindExistingOrCreatePrimitive(root.transform, faceDetails, "Eyebrow_L", "LeftEyebrow", PrimitiveType.Cube);
            ConfigurePart(leftEyebrow, "Eyebrow_L", new Vector3(-4.25f, 1.05f, 17.28f), Quaternion.Euler(0f, 0f, 8f), new Vector3(2.2f, 0.22f, 0.32f), eyebrowMaterial);

            Transform rightEyebrow = FindExistingOrCreatePrimitive(root.transform, faceDetails, "Eyebrow_R", "RightEyebrow", PrimitiveType.Cube);
            ConfigurePart(rightEyebrow, "Eyebrow_R", new Vector3(4.25f, 1.05f, 17.28f), Quaternion.Euler(0f, 0f, -8f), new Vector3(2.2f, 0.22f, 0.32f), eyebrowMaterial);

            Transform nose = FindExistingOrCreatePrimitive(root.transform, faceDetails, "Nose", "Nose", PrimitiveType.Cube);
            ConfigurePart(nose, "Nose", new Vector3(0f, -1.75f, 17.25f), Quaternion.Euler(0f, 0f, 5f), new Vector3(0.55f, 0.45f, 0.35f), noseMaterial);

            Transform mouth = FindExistingOrCreatePrimitive(root.transform, faceDetails, "StylizedMouth", "Mouth", PrimitiveType.Cube);
            ConfigurePart(mouth, "StylizedMouth", new Vector3(0f, -3.4f, 17.28f), Quaternion.identity, new Vector3(2.6f, 0.28f, 0.28f), mouthMaterial);

            Transform hairFront = FindExistingOrCreatePrimitive(root.transform, faceDetails, "HairAccent_Front", "HairAccent_Front", PrimitiveType.Cube);
            ConfigurePart(hairFront, "HairAccent_Front", new Vector3(0f, 4.25f, 16.35f), Quaternion.Euler(0f, 0f, 0f), new Vector3(4.1f, 0.8f, 1.1f), hairMaterial);

            Transform hairLeft = FindExistingOrCreatePrimitive(root.transform, faceDetails, "HairAccent_Left", "HairAccent_Left", PrimitiveType.Cube);
            ConfigurePart(hairLeft, "HairAccent_Left", new Vector3(-2.95f, 3.45f, 16.3f), Quaternion.Euler(0f, 0f, 14f), new Vector3(1.45f, 0.8f, 1.0f), hairMaterial);

            Transform hairRight = FindExistingOrCreatePrimitive(root.transform, faceDetails, "HairAccent_Right", "HairAccent_Right", PrimitiveType.Cube);
            ConfigurePart(hairRight, "HairAccent_Right", new Vector3(2.95f, 3.45f, 16.3f), Quaternion.Euler(0f, 0f, -14f), new Vector3(1.45f, 0.8f, 1.0f), hairMaterial);

            CharacterOutline outline = root.GetComponent<CharacterOutline>();
            if (outline == null)
            {
                outline = root.AddComponent<CharacterOutline>();
            }

            SerializedObject outlineObject = new SerializedObject(outline);
            outlineObject.FindProperty("outlineMaterial").objectReferenceValue = outlineMaterial;
            outlineObject.FindProperty("fallbackOutlineWidth").floatValue = 0.012f;
            outlineObject.ApplyModifiedPropertiesWithoutUndo();

            DestroyOutlineClones(root.transform);
            EditorUtility.SetDirty(root);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Debug.Log("Player visual polish prefab fixed and saved: " + PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static void EnsureMaterials()
    {
        EnsureFolder("Assets/Materials/Characters");
        EnsureFolder(PolishFolder);
        EnsureMaterial("M_Player_Eye_URP.mat", new Color32(0x3B, 0x58, 0x66, 0xFF), "Universal Render Pipeline/Simple Lit");
        EnsureMaterial("M_Player_Eyebrow_URP.mat", new Color32(0x3D, 0x31, 0x2B, 0xFF), "Universal Render Pipeline/Simple Lit");
        EnsureMaterial("M_Player_Nose_URP.mat", new Color32(0xD2, 0x98, 0x85, 0xFF), "Universal Render Pipeline/Simple Lit");
        EnsureMaterial("M_Player_Mouth_URP.mat", new Color32(0x8F, 0x5A, 0x58, 0xFF), "Universal Render Pipeline/Simple Lit");
        EnsureMaterial("M_Player_HairAccent_URP.mat", new Color32(0x52, 0x43, 0x3B, 0xFF), "Universal Render Pipeline/Simple Lit");
        Material outline = EnsureMaterial("M_Character_Outline_URP.mat", new Color32(0x2A, 0x21, 0x1D, 0xFF), "ChoTet/CollectableOutline");
        if (outline.HasProperty("_OutlineWidth"))
        {
            outline.SetFloat("_OutlineWidth", 0.012f);
        }
    }

    private static Material EnsureMaterial(string fileName, Color color, string shaderName)
    {
        string path = PolishFolder + "/" + fileName;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find(shaderName) ?? Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material LoadMaterial(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(PolishFolder + "/" + fileName);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }

    private static Transform FindExistingOrCreatePrimitive(Transform root, Transform parent, string primaryName, string legacyName, PrimitiveType primitive)
    {
        Transform existing = parent.Find(primaryName) ?? parent.Find(legacyName) ?? FindDeepChild(root, primaryName) ?? FindDeepChild(root, legacyName);
        if (existing != null)
        {
            existing.SetParent(parent, false);
            return existing;
        }

        GameObject created = GameObject.CreatePrimitive(primitive);
        Collider collider = created.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        created.transform.SetParent(parent, false);
        return created.transform;
    }

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject created = new GameObject(name);
        created.transform.SetParent(parent, false);
        return created.transform;
    }

    private static void ConfigurePart(Transform part, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
    {
        part.name = name;
        part.localPosition = localPosition;
        part.localRotation = localRotation;
        part.localScale = localScale;

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindDeepChild(parent.GetChild(i), name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void DestroyOutlineClones(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            DestroyOutlineClones(child);
            if (child.name.EndsWith("_Outline"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
