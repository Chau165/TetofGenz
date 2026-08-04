using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChoTet.Menu
{
    /// <summary>
    /// Builds dense festive 3D decoration around the title banner into back/front render layers.
    /// </summary>
    public static class MainMenuBannerDecorationBuilder
    {
        public const int DecorLayer = MainMenuTableDecorBuilder.DecorLayer;
        public const int RenderWidth = 820;
        public const int RenderHeight = 320;

        private const float FrameHalfHeight = 1.55f;
        private const float TitleSafeHalfWidth = 1.85f;
        private const float TitleSafeHalfHeight = 0.55f;

        private static readonly Color StemTint = new Color(0.92f, 1f, 0.88f, 1f);
        private static readonly Color LeafTint = new Color(0.95f, 1f, 0.9f, 1f);
        private static readonly Color BackTint = new Color(0.72f, 0.88f, 0.68f, 1f);
        private static readonly Color FlowerYellowTint = new Color(1f, 0.92f, 0.38f, 1f);
        private static readonly Color FlowerRedTint = new Color(0.95f, 0.32f, 0.28f, 1f);
        private static readonly Color PropTint = new Color(1f, 0.96f, 0.82f, 1f);

        private static Shader litShader;

        private enum PieceKind
        {
            StemA, StemB, StemC,
            LeafA, LeafB, LeafC, EdgeTuft,
            BlossomYellowA, BlossomYellowB, BlossomYellowC,
            BlossomRedA, BlossomRedB, BlossomPot,
            Lantern, LanternMarket, Coin, Coins, AppleRed, AppleYellow
        }

        private enum MotionKind { Sway, Float, Static }

        private readonly struct DecorPiece
        {
            public readonly PieceKind Kind;
            public readonly Vector3 Position;
            public readonly Vector3 Rotation;
            public readonly float Scale;
            public readonly bool FrontLayer;
            public readonly MotionKind Motion;
            public readonly float Phase;
            public readonly float Speed;

            public DecorPiece(PieceKind kind, Vector3 pos, Vector3 rot, float scale, bool front,
                MotionKind motion, float phase, float speed)
            {
                Kind = kind;
                Position = pos;
                Rotation = rot;
                Scale = scale;
                FrontLayer = front;
                Motion = motion;
                Phase = phase;
                Speed = speed;
            }
        }

        public readonly struct DecorationLayers
        {
            public readonly RenderTexture Back;
            public readonly RenderTexture Front;

            public DecorationLayers(RenderTexture back, RenderTexture front)
            {
                Back = back;
                Front = front;
            }

            public bool IsValid => Back != null && Front != null;
        }

        public static DecorationLayers Build(Transform root)
        {
            MainMenuBannerDecorationLibrary library = MainMenuBannerDecorationLibrary.LoadDefault();
            if (library == null || !HasAnyPrefab(library))
            {
                Debug.LogWarning("[MainMenuBannerDecorationBuilder] Missing decoration library.");
                return default;
            }

            GameObject decorRoot = new GameObject("BannerDecorationRoot");
            decorRoot.transform.SetParent(root, false);

            RenderTexture backRt = CreateRenderTexture("BannerDecorBackRT");
            RenderTexture frontRt = CreateRenderTexture("BannerDecorFrontRT");

            Transform backProps = new GameObject("BackDecor").transform;
            Transform frontProps = new GameObject("FrontDecor").transform;
            backProps.SetParent(decorRoot.transform, false);
            frontProps.SetParent(decorRoot.transform, false);

            int spawned = 0;
            foreach (DecorPiece piece in BuildFestiveLayout())
            {
                Transform parent = piece.FrontLayer ? frontProps : backProps;
                if (SpawnPiece(library, piece, parent))
                {
                    spawned++;
                }
            }

            if (spawned == 0)
            {
                Object.Destroy(decorRoot);
                ReleaseRenderTexture(backRt);
                ReleaseRenderTexture(frontRt);
                return default;
            }

            SetLayerRecursive(backProps.gameObject, DecorLayer);
            SetLayerRecursive(frontProps.gameObject, DecorLayer);

            Camera backCamera = CreateCamera(decorRoot.transform, backRt, "BackCamera");
            Camera frontCamera = CreateCamera(decorRoot.transform, frontRt, "FrontCamera");
            CreateLights(decorRoot.transform);

            RenderPass(backCamera, backProps, frontProps);
            RenderPass(frontCamera, frontProps, backProps);

            MainMenuBannerDecorationLifetime lifetime = decorRoot.AddComponent<MainMenuBannerDecorationLifetime>();
            lifetime.Initialize(backRt, frontRt, backCamera, frontCamera, backProps, frontProps);

            Debug.Log($"[MainMenuBannerDecorationBuilder] Spawned {spawned} festive banner decorations.");
            return new DecorationLayers(backRt, frontRt);
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

        private static DecorPiece[] BuildFestiveLayout()
        {
            var pieces = new System.Collections.Generic.List<DecorPiece>(256);
            System.Random rng = new System.Random(20260531);

            BuildCornerTopLeft(pieces, rng);
            BuildCornerTopRight(pieces, rng);
            BuildCornerBottomLeft(pieces, rng);
            BuildCornerBottomRight(pieces, rng);
            AddFestiveProps(pieces, rng);

            return pieces.ToArray();
        }

        // --- Organic vine path helpers ---

        private static Vector2 SampleQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        private static Vector2 SampleQuadraticDerivative(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        }

        private static Vector2 SampleCubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
        }

        private static Vector2 SampleCubicDerivative(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return 3f * u * u * (p1 - p0) + 6f * u * t * (p2 - p1) + 3f * t * t * (p3 - p2);
        }

        private static float NextFloat(System.Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }

        private static PieceKind PickStem(System.Random rng, int index)
        {
            return (PieceKind)((int)PieceKind.StemA + ((index + rng.Next(0, 3)) % 3));
        }

        private static PieceKind PickLeaf(System.Random rng, int index)
        {
            return (PieceKind)((int)PieceKind.LeafA + ((index + rng.Next(0, 3)) % 3));
        }

        /// <summary>
        /// Places short stem segments and leaf clusters along a curved quadratic path.
        /// </summary>
        private static void AddQuadraticVine(
            System.Collections.Generic.List<DecorPiece> pieces,
            System.Random rng,
            Vector2 start, Vector2 control, Vector2 end,
            int segmentCount,
            float phaseBase,
            int frontSeed)
        {
            AddCurvedVineInternal(pieces, rng, start, control, end, Vector2.zero, segmentCount,
                phaseBase, frontSeed, cubic: false);
        }

        /// <summary>
        /// Places short stem segments and leaf clusters along a curved cubic path.
        /// </summary>
        private static void AddCubicVine(
            System.Collections.Generic.List<DecorPiece> pieces,
            System.Random rng,
            Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3,
            int segmentCount,
            float phaseBase,
            int frontSeed)
        {
            AddCurvedVineInternal(pieces, rng, p0, p1, p2, p3, segmentCount,
                phaseBase, frontSeed, cubic: true);
        }

        private static void AddCurvedVineInternal(
            System.Collections.Generic.List<DecorPiece> pieces,
            System.Random rng,
            Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3,
            int segmentCount,
            float phaseBase,
            int frontSeed,
            bool cubic)
        {
            segmentCount = Mathf.Max(segmentCount, 4);
            for (int i = 0; i < segmentCount; i++)
            {
                float t = segmentCount == 1 ? 0.5f : i / (float)(segmentCount - 1);
                Vector2 pos2;
                Vector2 tangent2;
                if (cubic)
                {
                    pos2 = SampleCubicBezier(p0, p1, p2, p3, t);
                    tangent2 = SampleCubicDerivative(p0, p1, p2, p3, t);
                }
                else
                {
                    pos2 = SampleQuadraticBezier(p0, p1, p2, t);
                    tangent2 = SampleQuadraticDerivative(p0, p1, p2, t);
                }

                if (tangent2.sqrMagnitude < 0.0001f)
                {
                    tangent2 = (p2 - p0).normalized;
                }
                else
                {
                    tangent2.Normalize();
                }

                Vector2 normal = new Vector2(-tangent2.y, tangent2.x);
                float jitterAlong = NextFloat(rng, -0.06f, 0.06f);
                float jitterPerp = NextFloat(rng, -0.05f, 0.05f);
                Vector3 pos = new Vector3(
                    pos2.x + tangent2.x * jitterAlong + normal.x * jitterPerp,
                    pos2.y + tangent2.y * jitterAlong + normal.y * jitterPerp,
                    0f);

                if (IsInTitleSafeZone(pos))
                {
                    continue;
                }

                float rotZ = Mathf.Atan2(tangent2.y, tangent2.x) * Mathf.Rad2Deg
                    + NextFloat(rng, -22f, 22f);
                float cornerBias = 1f - t;
                float stemScale = NextFloat(rng, 0.14f, 0.28f) * (0.88f + cornerBias * 0.28f);
                bool front = (i + frontSeed) % 3 != 1;

                pieces.Add(new DecorPiece(
                    PickStem(rng, i), pos, new Vector3(NextFloat(rng, -8f, 8f), 0f, rotZ),
                    stemScale, front, MotionKind.Sway, phaseBase + i * 0.31f,
                    NextFloat(rng, 0.11f, 0.16f)));

                int leafCount = rng.Next(3, 6);
                for (int l = 0; l < leafCount; l++)
                {
                    float side = l % 2 == 0 ? 1f : -1f;
                    float alongT = NextFloat(rng, -0.08f, 0.08f);
                    float outDist = NextFloat(rng, 0.06f, 0.16f) * side;
                    Vector3 leafPos = pos + new Vector3(
                        tangent2.x * alongT + normal.x * outDist,
                        tangent2.y * alongT + normal.y * outDist,
                        0f);

                    if (IsInTitleSafeZone(leafPos))
                    {
                        continue;
                    }

                    float leafRot = rotZ + NextFloat(rng, 55f, 125f) * side
                        + NextFloat(rng, -18f, 18f);
                    bool leafFront = (l + frontSeed + i) % 3 != 2;
                    float leafScale = NextFloat(rng, 0.1f, 0.2f) * (0.92f + cornerBias * 0.22f);

                    pieces.Add(new DecorPiece(
                        PickLeaf(rng, l + i), leafPos, new Vector3(0f, 0f, leafRot),
                        leafScale, leafFront, MotionKind.Sway, phaseBase + i * 0.31f + l * 0.17f,
                        NextFloat(rng, 0.12f, 0.18f)));
                }
            }
        }

        /// <summary>Top-left corner: vines curl along top and left, one crosses in front.</summary>
        private static void BuildCornerTopLeft(System.Collections.Generic.List<DecorPiece> pieces, System.Random rng)
        {
            float phase = 0f;

            AddCubicVine(pieces, rng,
                new Vector2(-3.62f, 1.28f), new Vector2(-3.1f, 1.52f), new Vector2(-2.05f, 1.38f), new Vector2(-1.35f, 1.22f),
                9, phase, 0);

            AddQuadraticVine(pieces, rng,
                new Vector2(-3.58f, 1.22f), new Vector2(-3.78f, 0.55f), new Vector2(-3.52f, -0.08f),
                8, phase + 1.2f, 2);

            AddQuadraticVine(pieces, rng,
                new Vector2(-3.48f, 1.18f), new Vector2(-2.85f, 1.08f), new Vector2(-2.2f, 0.92f),
                6, phase + 2.4f, 1);

            // Front-layer curl creeping over the frame edge
            AddQuadraticVine(pieces, rng,
                new Vector2(-3.55f, 1.15f), new Vector2(-3.25f, 0.78f), new Vector2(-2.95f, 0.62f),
                5, phase + 3.1f, 0);

            pieces.Add(new DecorPiece(PieceKind.BlossomYellowA,
                new Vector3(-3.42f, 1.02f, 0f), new Vector3(0f, 18f, 108f), 0.26f, true,
                MotionKind.Float, phase + 4f, 0.17f));
            pieces.Add(new DecorPiece(PieceKind.BlossomYellowB,
                new Vector3(-3.28f, 0.68f, 0f), new Vector3(0f, -10f, 94f), 0.22f, false,
                MotionKind.Float, phase + 4.3f, 0.16f));
        }

        /// <summary>Top-right: asymmetric curves with lantern accent — no mirror of top-left.</summary>
        private static void BuildCornerTopRight(System.Collections.Generic.List<DecorPiece> pieces, System.Random rng)
        {
            float phase = 5f;

            AddQuadraticVine(pieces, rng,
                new Vector2(3.6f, 1.26f), new Vector2(2.95f, 1.48f), new Vector2(1.55f, 1.18f),
                8, phase, 1);

            AddCubicVine(pieces, rng,
                new Vector2(3.58f, 1.2f), new Vector2(3.72f, 0.42f), new Vector2(3.38f, -0.22f), new Vector2(3.12f, -0.62f),
                9, phase + 1.5f, 0);

            AddQuadraticVine(pieces, rng,
                new Vector2(3.45f, 1.14f), new Vector2(2.72f, 0.88f), new Vector2(2.05f, 0.58f),
                5, phase + 2.8f, 2);

            // Short front vine hooking over top edge
            AddQuadraticVine(pieces, rng,
                new Vector2(3.52f, 1.1f), new Vector2(3.18f, 0.72f), new Vector2(2.88f, 0.55f),
                4, phase + 3.5f, 1);

            pieces.Add(new DecorPiece(PieceKind.Lantern,
                new Vector3(3.38f, 0.95f, 0f), new Vector3(0f, 0f, -14f), 0.24f, true,
                MotionKind.Float, phase + 4f, 0.15f));
            pieces.Add(new DecorPiece(PieceKind.BlossomRedA,
                new Vector3(3.48f, 0.72f, 0f), new Vector3(0f, 12f, -98f), 0.24f, false,
                MotionKind.Float, phase + 4.4f, 0.18f));
        }

        /// <summary>Bottom-left: low curl along bottom edge, climbs left side slightly.</summary>
        private static void BuildCornerBottomLeft(System.Collections.Generic.List<DecorPiece> pieces, System.Random rng)
        {
            float phase = 10f;

            AddCubicVine(pieces, rng,
                new Vector2(-3.6f, -1.26f), new Vector2(-2.88f, -1.48f), new Vector2(-1.72f, -1.32f), new Vector2(-1.28f, -1.18f),
                8, phase, 2);

            AddQuadraticVine(pieces, rng,
                new Vector2(-3.55f, -1.2f), new Vector2(-3.68f, -0.55f), new Vector2(-3.42f, 0.05f),
                7, phase + 1.3f, 1);

            AddQuadraticVine(pieces, rng,
                new Vector2(-3.5f, -1.15f), new Vector2(-2.78f, -0.95f), new Vector2(-2.15f, -0.72f),
                5, phase + 2.6f, 0);

            pieces.Add(new DecorPiece(PieceKind.Coins,
                new Vector3(-3.22f, -1.02f, 0f), new Vector3(0f, 0f, 22f), 0.18f, true,
                MotionKind.Float, phase + 3.5f, 0.19f));
            pieces.Add(new DecorPiece(PieceKind.EdgeTuft,
                new Vector3(-3.58f, -0.82f, 0f), new Vector3(0f, 0f, 78f), 0.22f, false,
                MotionKind.Sway, phase + 3.8f, 0.12f));
        }

        /// <summary>Bottom-right: unique S-curve and fruit accent — distinct from bottom-left.</summary>
        private static void BuildCornerBottomRight(System.Collections.Generic.List<DecorPiece> pieces, System.Random rng)
        {
            float phase = 15f;

            AddQuadraticVine(pieces, rng,
                new Vector2(3.58f, -1.24f), new Vector2(2.65f, -1.42f), new Vector2(1.42f, -1.22f),
                7, phase, 0);

            AddCubicVine(pieces, rng,
                new Vector2(3.55f, -1.18f), new Vector2(3.68f, -0.48f), new Vector2(3.22f, 0.08f), new Vector2(2.92f, 0.48f),
                8, phase + 1.4f, 2);

            AddQuadraticVine(pieces, rng,
                new Vector2(3.48f, -1.12f), new Vector2(2.82f, -0.82f), new Vector2(2.18f, -0.58f),
                6, phase + 2.7f, 1);

            // Front vine wrapping corner
            AddQuadraticVine(pieces, rng,
                new Vector2(3.5f, -1.08f), new Vector2(3.22f, -0.68f), new Vector2(2.95f, -0.42f),
                4, phase + 3.4f, 0);

            pieces.Add(new DecorPiece(PieceKind.AppleRed,
                new Vector3(3.28f, -0.92f, 0f), new Vector3(0f, 0f, -10f), 0.2f, true,
                MotionKind.Float, phase + 4f, 0.18f));
            pieces.Add(new DecorPiece(PieceKind.Coin,
                new Vector3(3.44f, -0.68f, 0f), new Vector3(60f, 0f, -18f), 0.16f, false,
                MotionKind.Float, phase + 4.3f, 0.2f));
        }

        private static void AddFestiveProps(System.Collections.Generic.List<DecorPiece> pieces, System.Random rng)
        {
            // Sparse accent props tucked into corners only — vines carry most of the mass.
            pieces.Add(new DecorPiece(PieceKind.BlossomPot,
                new Vector3(-3.48f, -0.78f, 0f), new Vector3(0f, 0f, 68f), 0.16f, false,
                MotionKind.Static, 20f, 0f));
            pieces.Add(new DecorPiece(PieceKind.LanternMarket,
                new Vector3(3.52f, 0.38f, 0f), new Vector3(0f, 0f, -24f), 0.18f, false,
                MotionKind.Float, 20.5f, 0.15f));
            pieces.Add(new DecorPiece(PieceKind.BlossomYellowC,
                new Vector3(-3.18f, 0.88f, 0f), new Vector3(0f, 0f, 102f), 0.2f, true,
                MotionKind.Float, 21f, 0.16f));
            pieces.Add(new DecorPiece(PieceKind.AppleYellow,
                new Vector3(3.12f, -0.48f, 0f), new Vector3(0f, 0f, -8f), 0.18f, true,
                MotionKind.Float, 21.5f, 0.17f));
        }

        private static bool HasAnyPrefab(MainMenuBannerDecorationLibrary library)
        {
            return library.stemA != null || library.blossomYellowA != null || library.lantern != null;
        }

        private static RenderTexture CreateRenderTexture(string name)
        {
            RenderTexture rt = new RenderTexture(RenderWidth, RenderHeight, 24, RenderTextureFormat.ARGB32)
            {
                name = name,
                antiAliasing = 4,
                filterMode = FilterMode.Bilinear,
                useMipMap = false
            };
            rt.Create();
            return rt;
        }

        private static Camera CreateCamera(Transform parent, RenderTexture target, string name)
        {
            GameObject camObj = new GameObject(name);
            camObj.transform.SetParent(parent, false);
            camObj.transform.position = new Vector3(0f, 0f, -10f);

            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.cullingMask = 1 << DecorLayer;
            cam.orthographic = true;
            cam.orthographicSize = FrameHalfHeight;
            cam.aspect = (float)RenderWidth / RenderHeight;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 30f;
            cam.targetTexture = target;
            cam.depth = -40f;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            return cam;
        }

        private static void CreateLights(Transform parent)
        {
            CreateDirectionalLight(parent, "KeyLight", new Vector3(38f, -22f, 0f), 1.35f, Color.white);
            CreateDirectionalLight(parent, "FillLight", new Vector3(18f, 158f, 0f), 0.55f, new Color(0.9f, 1f, 0.78f));
            CreateDirectionalLight(parent, "RimLight", new Vector3(-24f, 12f, 0f), 0.35f, new Color(1f, 0.88f, 0.55f));

            GameObject pointObj = new GameObject("WarmPoint");
            pointObj.transform.SetParent(parent, false);
            pointObj.transform.localPosition = new Vector3(0f, 0.2f, -6f);
            Light point = pointObj.AddComponent<Light>();
            point.type = LightType.Point;
            point.color = new Color(1f, 0.88f, 0.55f);
            point.intensity = 1.6f;
            point.range = 20f;
            point.cullingMask = 1 << DecorLayer;
        }

        private static void CreateDirectionalLight(Transform parent, string name, Vector3 euler, float intensity, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.rotation = Quaternion.Euler(euler);
            Light light = obj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.cullingMask = 1 << DecorLayer;
        }

        private static bool SpawnPiece(MainMenuBannerDecorationLibrary library, DecorPiece piece, Transform parent)
        {
            if (IsInTitleSafeZone(piece.Position))
            {
                return false;
            }

            GameObject prefab = ResolvePrefab(library, piece.Kind);
            prefab = ResolvePrefabFallback(prefab, piece.Kind);
            if (prefab == null)
            {
                return false;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name + (piece.FrontLayer ? "_F" : "_B");
            instance.transform.localPosition = piece.Position;
            instance.transform.localRotation = Quaternion.Euler(piece.Rotation);
            instance.transform.localScale = Vector3.one * piece.Scale;
            StripColliders(instance);
            ApplyTint(instance, GetTint(piece.Kind, piece.FrontLayer));

            switch (piece.Motion)
            {
                case MotionKind.Sway:
                    AmbientSway sway = instance.AddComponent<AmbientSway>();
                    sway.Configure(Vector3.forward, piece.Speed, 1.6f, piece.Phase);
                    break;
                case MotionKind.Float:
                    FloatingDecoration floating = instance.AddComponent<FloatingDecoration>();
                    floating.Configure(0.035f, piece.Speed, 0.025f, piece.Speed * 0.7f, piece.Phase);
                    break;
            }

            return true;
        }

        private static bool IsInTitleSafeZone(Vector3 pos)
        {
            return Mathf.Abs(pos.x) < TitleSafeHalfWidth && Mathf.Abs(pos.y) < TitleSafeHalfHeight;
        }

        private static Color GetTint(PieceKind kind, bool front)
        {
            Color tint = kind switch
            {
                PieceKind.StemA or PieceKind.StemB or PieceKind.StemC => StemTint,
                PieceKind.LeafA or PieceKind.LeafB or PieceKind.LeafC or PieceKind.EdgeTuft => LeafTint,
                PieceKind.BlossomYellowA or PieceKind.BlossomYellowB or PieceKind.BlossomYellowC => FlowerYellowTint,
                PieceKind.BlossomRedA or PieceKind.BlossomRedB => FlowerRedTint,
                _ => PropTint
            };
            return front ? tint : BackTint * new Color(tint.r, tint.g, tint.b, 1f);
        }

        private static GameObject ResolvePrefab(MainMenuBannerDecorationLibrary library, PieceKind kind)
        {
            return kind switch
            {
                PieceKind.StemA => library.stemA,
                PieceKind.StemB => library.stemB,
                PieceKind.StemC => library.stemC,
                PieceKind.LeafA => library.leafA,
                PieceKind.LeafB => library.leafB,
                PieceKind.LeafC => library.leafC,
                PieceKind.EdgeTuft => library.edgeTuft,
                PieceKind.BlossomYellowA => library.blossomYellowA,
                PieceKind.BlossomYellowB => library.blossomYellowB,
                PieceKind.BlossomYellowC => library.blossomYellowC,
                PieceKind.BlossomRedA => library.blossomRedA,
                PieceKind.BlossomRedB => library.blossomRedB,
                PieceKind.BlossomPot => library.blossomPot,
                PieceKind.Lantern => library.lantern,
                PieceKind.LanternMarket => library.lanternMarket,
                PieceKind.Coin => library.coin,
                PieceKind.Coins => library.coins,
                PieceKind.AppleRed => library.appleRed,
                PieceKind.AppleYellow => library.appleYellow,
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
            string path = kind switch
            {
                PieceKind.StemA => MainMenuBannerDecorationPaths.StemA,
                PieceKind.StemB => MainMenuBannerDecorationPaths.StemB,
                PieceKind.StemC => MainMenuBannerDecorationPaths.StemC,
                PieceKind.LeafA => MainMenuBannerDecorationPaths.LeafA,
                PieceKind.LeafB => MainMenuBannerDecorationPaths.LeafB,
                PieceKind.LeafC => MainMenuBannerDecorationPaths.LeafC,
                PieceKind.EdgeTuft => MainMenuBannerDecorationPaths.EdgeTuft,
                PieceKind.BlossomYellowA => MainMenuBannerDecorationPaths.BlossomYellowA,
                PieceKind.BlossomYellowB => MainMenuBannerDecorationPaths.BlossomYellowB,
                PieceKind.BlossomYellowC => MainMenuBannerDecorationPaths.BlossomYellowC,
                PieceKind.BlossomRedA => MainMenuBannerDecorationPaths.BlossomRedA,
                PieceKind.BlossomRedB => MainMenuBannerDecorationPaths.BlossomRedB,
                PieceKind.BlossomPot => MainMenuBannerDecorationPaths.BlossomPot,
                PieceKind.Lantern => MainMenuBannerDecorationPaths.Lantern,
                PieceKind.LanternMarket => MainMenuBannerDecorationPaths.LanternMarket,
                PieceKind.Coin => MainMenuBannerDecorationPaths.Coin,
                PieceKind.Coins => MainMenuBannerDecorationPaths.Coins,
                PieceKind.AppleRed => MainMenuBannerDecorationPaths.AppleRed,
                PieceKind.AppleYellow => MainMenuBannerDecorationPaths.AppleYellow,
                _ => null
            };
            return path != null ? AssetDatabase.LoadAssetAtPath<GameObject>(path) : null;
#else
            return null;
#endif
        }

        private static void ApplyTint(GameObject target, Color tint)
        {
            // Preserve mesh textures — only multiply a subtle color shift.
            if (litShader == null)
            {
                litShader = Shader.Find("Universal Render Pipeline/Lit");
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

                Material material = new Material(source);
                Shader shader = litShader != null ? litShader : source.shader;
                material.shader = shader;

                if (material.HasProperty("_BaseColor"))
                {
                    Color baseColor = source.HasProperty("_BaseColor")
                        ? source.GetColor("_BaseColor")
                        : Color.white;
                    material.SetColor("_BaseColor", baseColor * tint);
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

        private static void ReleaseRenderTexture(RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            Object.Destroy(texture);
        }
    }
}
