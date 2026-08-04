using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Renders asymmetric 3D vine growth along the title banner frame with back/front depth layers.
    /// </summary>
    public static class MainMenuBannerVineBuilder
    {
        public const int DecorLayer = MainMenuTableDecorBuilder.DecorLayer;
        public const int RenderWidth = 760;
        public const int RenderHeight = 280;

        private const float FrameHalfHeight = 1.45f;
        private const float MaxSwayDegrees = 1.5f;

        private static readonly Color StemTint = new Color(0.55f, 0.88f, 0.38f, 1f);
        private static readonly Color LeafTint = new Color(0.62f, 0.96f, 0.42f, 1f);
        private static readonly Color BackTint = new Color(0.42f, 0.68f, 0.3f, 0.92f);

        private static Shader unlitShader;

        private enum PieceKind
        {
            StemA,
            StemB,
            StemC,
            LeafA,
            LeafB,
            LeafC,
            EdgeTuft
        }

        private readonly struct VinePiece
        {
            public readonly PieceKind Kind;
            public readonly Vector3 Position;
            public readonly Vector3 Rotation;
            public readonly float Scale;
            public readonly bool FrontLayer;
            public readonly float SwayPhase;
            public readonly float SwaySpeed;

            public VinePiece(PieceKind kind, Vector3 position, Vector3 rotation, float scale, bool frontLayer,
                float swayPhase, float swaySpeed)
            {
                Kind = kind;
                Position = position;
                Rotation = rotation;
                Scale = scale;
                FrontLayer = frontLayer;
                SwayPhase = swayPhase;
                SwaySpeed = swaySpeed;
            }
        }

        public readonly struct VineLayers
        {
            public readonly RenderTexture Back;
            public readonly RenderTexture Front;

            public VineLayers(RenderTexture back, RenderTexture front)
            {
                Back = back;
                Front = front;
            }

            public bool IsValid => Back != null && Front != null;
        }

        public static VineLayers Build(Transform root)
        {
            MainMenuBannerVineLibrary library = MainMenuBannerVineLibrary.LoadDefault();
            if (library == null || !HasAnyPrefab(library))
            {
                Debug.LogWarning("[MainMenuBannerVineBuilder] Missing vine library or prefabs. " +
                                 "Run Tools → Cho Tet → Setup Banner Vine Library.");
                return default;
            }

            GameObject decorRoot = new GameObject("MenuBannerVineDecor");
            decorRoot.transform.SetParent(root, false);

            RenderTexture backRt = CreateRenderTexture("MenuBannerVineBackRT");
            RenderTexture frontRt = CreateRenderTexture("MenuBannerVineFrontRT");

            Transform backProps = new GameObject("BackVines").transform;
            Transform frontProps = new GameObject("FrontVines").transform;
            backProps.SetParent(decorRoot.transform, false);
            frontProps.SetParent(decorRoot.transform, false);

            int spawned = 0;
            foreach (VinePiece piece in BuildAsymmetricLayout())
            {
                Transform parent = piece.FrontLayer ? frontProps : backProps;
                if (SpawnPiece(library, piece, parent))
                {
                    spawned++;
                }
            }

            if (spawned == 0)
            {
                Debug.LogError("[MainMenuBannerVineBuilder] No vine prefabs spawned.");
                Object.Destroy(decorRoot);
                ReleaseRenderTexture(backRt);
                ReleaseRenderTexture(frontRt);
                return default;
            }

            SetLayerRecursive(backProps.gameObject, DecorLayer);
            SetLayerRecursive(frontProps.gameObject, DecorLayer);

            Camera backCamera = CreateCamera(decorRoot.transform, backRt, "BackCamera");
            Camera frontCamera = CreateCamera(decorRoot.transform, frontRt, "FrontCamera");
            CreateSharedLight(decorRoot.transform);

            RenderPass(backCamera, backProps, frontProps);
            RenderPass(frontCamera, frontProps, backProps);

            MainMenuBannerVineLifetime lifetime = decorRoot.AddComponent<MainMenuBannerVineLifetime>();
            lifetime.Initialize(backRt, frontRt, backCamera, frontCamera, backProps, frontProps);

            Debug.Log($"[MainMenuBannerVineBuilder] Spawned {spawned} vine pieces on layer {DecorLayer}.");
            return new VineLayers(backRt, frontRt);
        }

        public static void RenderPass(Camera camera, Transform showRoot, Transform hideRoot)
        {
            if (camera == null || showRoot == null)
            {
                return;
            }

            bool hideWasActive = hideRoot != null && hideRoot.gameObject.activeSelf;
            if (hideRoot != null)
            {
                hideRoot.gameObject.SetActive(false);
            }

            if (!showRoot.gameObject.activeSelf)
            {
                showRoot.gameObject.SetActive(true);
            }

            camera.Render();

            if (hideRoot != null && hideWasActive)
            {
                hideRoot.gameObject.SetActive(true);
            }
        }

        private static bool HasAnyPrefab(MainMenuBannerVineLibrary library)
        {
            return library.stemA != null || library.stemB != null || library.stemC != null ||
                   library.leafTuftA != null || library.leafTuftB != null || library.leafTuftC != null ||
                   library.edgeTuft != null;
        }

        private static void ReleaseRenderTexture(RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            Object.Destroy(texture);
        }

        private static RenderTexture CreateRenderTexture(string name)
        {
            RenderTexture renderTexture = new RenderTexture(RenderWidth, RenderHeight, 24, RenderTextureFormat.ARGB32)
            {
                name = name,
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear,
                useMipMap = false,
                autoGenerateMips = false
            };
            renderTexture.Create();
            return renderTexture;
        }

        private static VinePiece[] BuildAsymmetricLayout()
        {
            return new[]
            {
                new VinePiece(PieceKind.StemB, new Vector3(-3.54f, -1.35f, 0f), new Vector3(18f, 0f, 8f), 0.58f, false,
                    0f, 0.13f),
                new VinePiece(PieceKind.StemA, new Vector3(-3.22f, -1.36f, 0f), new Vector3(12f, 0f, 4f), 0.52f, false,
                    0.45f, 0.14f),
                new VinePiece(PieceKind.StemC, new Vector3(-2.82f, -1.34f, 0f), new Vector3(8f, 0f, -2f), 0.48f, true,
                    0.9f, 0.15f),
                new VinePiece(PieceKind.LeafB, new Vector3(-3.08f, -1.28f, 0f), new Vector3(0f, 0f, 14f), 0.38f, true,
                    1.2f, 0.148f),
                new VinePiece(PieceKind.StemB, new Vector3(-3.48f, -1.06f, 0f), new Vector3(0f, 0f, 72f), 0.54f, true,
                    1.55f, 0.142f),
                new VinePiece(PieceKind.StemA, new Vector3(-3.42f, -0.78f, 0f), new Vector3(-4f, 0f, 84f), 0.5f, true,
                    1.95f, 0.136f),
                new VinePiece(PieceKind.LeafA, new Vector3(-3.28f, -0.62f, 0f), new Vector3(0f, 12f, 88f), 0.4f, false,
                    2.3f, 0.145f),

                new VinePiece(PieceKind.StemA, new Vector3(-0.95f, -1.38f, 0f), new Vector3(0f, 0f, -4f), 0.46f, true,
                    2.75f, 0.138f),
                new VinePiece(PieceKind.LeafC, new Vector3(0.55f, -1.35f, 0f), new Vector3(0f, 0f, 6f), 0.34f, false,
                    3.1f, 0.14f),

                new VinePiece(PieceKind.StemC, new Vector3(3.18f, -1.33f, 0f), new Vector3(0f, 0f, -14f), 0.48f, false,
                    3.5f, 0.135f),
                new VinePiece(PieceKind.StemB, new Vector3(3.46f, -1.08f, 0f), new Vector3(0f, 0f, -62f), 0.52f, true,
                    3.85f, 0.141f),

                new VinePiece(PieceKind.StemC, new Vector3(-3.62f, 0.12f, 0f), new Vector3(0f, 0f, 94f), 0.44f, false,
                    4.2f, 0.128f),

                new VinePiece(PieceKind.StemA, new Vector3(-3.44f, 1.26f, 0f), new Vector3(0f, 0f, 116f), 0.38f, false,
                    4.55f, 0.134f),

                new VinePiece(PieceKind.StemB, new Vector3(-0.42f, 1.33f, 0f), new Vector3(0f, 0f, 176f), 0.42f, true,
                    4.9f, 0.137f),

                new VinePiece(PieceKind.StemC, new Vector3(3.36f, 1.22f, 0f), new Vector3(0f, 0f, -112f), 0.5f, true,
                    5.25f, 0.139f),
                new VinePiece(PieceKind.StemB, new Vector3(3.52f, 0.98f, 0f), new Vector3(0f, 0f, -86f), 0.46f, true,
                    5.6f, 0.133f),
                new VinePiece(PieceKind.LeafA, new Vector3(3.38f, 1.14f, 0f), new Vector3(0f, 0f, -98f), 0.36f, false,
                    5.95f, 0.146f),

                new VinePiece(PieceKind.StemA, new Vector3(3.58f, 0.34f, 0f), new Vector3(0f, 0f, -92f), 0.48f, true,
                    6.3f, 0.136f),
                new VinePiece(PieceKind.EdgeTuft, new Vector3(3.46f, 0.56f, 0f), new Vector3(0f, 0f, -78f), 0.34f, false,
                    6.65f, 0.149f),
            };
        }

        private static Camera CreateCamera(Transform parent, RenderTexture target, string name)
        {
            GameObject cameraObject = new GameObject(name);
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.transform.rotation = Quaternion.identity;

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.cullingMask = 1 << DecorLayer;
            camera.orthographic = true;
            camera.orthographicSize = FrameHalfHeight;
            camera.aspect = (float)RenderWidth / RenderHeight;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 30f;
            camera.targetTexture = target;
            camera.depth = -40f;
            camera.useOcclusionCulling = false;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            return camera;
        }

        private static void CreateSharedLight(Transform parent)
        {
            GameObject keyLightObject = new GameObject("VineKeyLight");
            keyLightObject.transform.SetParent(parent, false);
            keyLightObject.transform.rotation = Quaternion.Euler(40f, -20f, 0f);
            Light key = keyLightObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = Color.white;
            key.intensity = 1.25f;
            key.shadows = LightShadows.None;
            key.cullingMask = 1 << DecorLayer;

            GameObject fillObject = new GameObject("VineFillLight");
            fillObject.transform.SetParent(parent, false);
            fillObject.transform.rotation = Quaternion.Euler(20f, 160f, 0f);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.85f, 1f, 0.75f);
            fill.intensity = 0.65f;
            fill.shadows = LightShadows.None;
            fill.cullingMask = 1 << DecorLayer;

            GameObject rimObject = new GameObject("VineRimLight");
            rimObject.transform.SetParent(parent, false);
            rimObject.transform.localPosition = new Vector3(0f, 0f, -6f);
            Light rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.color = new Color(0.9f, 1f, 0.8f);
            rim.intensity = 1.4f;
            rim.range = 18f;
            rim.shadows = LightShadows.None;
            rim.cullingMask = 1 << DecorLayer;
        }

        private static bool SpawnPiece(MainMenuBannerVineLibrary library, VinePiece piece, Transform parent)
        {
            GameObject prefab = ResolvePrefab(library, piece.Kind);
            prefab = ResolvePrefabFallback(prefab, piece.Kind);
            if (prefab == null)
            {
                return false;
            }

            bool isLeaf = piece.Kind is PieceKind.LeafA or PieceKind.LeafB or PieceKind.LeafC or PieceKind.EdgeTuft;
            Color tint = isLeaf ? LeafTint : StemTint;
            if (!piece.FrontLayer)
            {
                tint = BackTint;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name + (piece.FrontLayer ? "_F" : "_B");
            instance.transform.localPosition = piece.Position;
            instance.transform.localRotation = Quaternion.Euler(piece.Rotation);
            instance.transform.localScale = Vector3.one * piece.Scale;
            StripColliders(instance);
            ApplyUnlitTint(instance, tint);

            MainMenuTitleBannerSway sway = instance.AddComponent<MainMenuTitleBannerSway>();
            sway.Configure(Vector3.forward, piece.SwaySpeed, MaxSwayDegrees, piece.SwayPhase);
            return true;
        }

        private static GameObject ResolvePrefab(MainMenuBannerVineLibrary library, PieceKind kind)
        {
            return kind switch
            {
                PieceKind.StemA => library.stemA,
                PieceKind.StemB => library.stemB,
                PieceKind.StemC => library.stemC,
                PieceKind.LeafA => library.leafTuftA,
                PieceKind.LeafB => library.leafTuftB,
                PieceKind.LeafC => library.leafTuftC,
                PieceKind.EdgeTuft => library.edgeTuft,
                _ => null
            };
        }

        private static GameObject ResolvePrefabFallback(GameObject prefab, PieceKind kind)
        {
            if (prefab != null)
            {
                return prefab;
            }

#if UNITY_EDITOR
            return kind switch
            {
                PieceKind.StemA => AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuBannerVinePaths.StemA),
                PieceKind.StemB => AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuBannerVinePaths.StemB),
                PieceKind.StemC => AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuBannerVinePaths.StemC),
                PieceKind.LeafA => AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuBannerVinePaths.LeafTuftA),
                PieceKind.LeafB => AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuBannerVinePaths.LeafTuftB),
                PieceKind.LeafC => AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuBannerVinePaths.LeafTuftC),
                PieceKind.EdgeTuft => AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuBannerVinePaths.EdgeTuft),
                _ => null
            };
#else
            return null;
#endif
        }

        private static void ApplyUnlitTint(GameObject target, Color tint)
        {
            if (unlitShader == null)
            {
                unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                Material source = renderer.sharedMaterial;
                if (source == null)
                {
                    continue;
                }

                Shader shader = unlitShader != null ? unlitShader : source.shader;
                Material material = new Material(shader);

                if (source.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", source.GetTexture("_BaseMap"));
                }
                else if (source.HasProperty("_MainTex"))
                {
                    material.SetTexture("_BaseMap", source.GetTexture("_MainTex"));
                }

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", tint);
                }
                else
                {
                    material.color = tint;
                }

                if (material.HasProperty("_Surface"))
                {
                    material.SetFloat("_Surface", 0f);
                }

                renderer.material = material;
            }
        }

        private static void SetLayerRecursive(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private static void StripColliders(GameObject target)
        {
            Collider[] colliders = target.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(colliders[i]);
                }
                else
                {
                    Object.DestroyImmediate(colliders[i]);
                }
            }
        }
    }
}
