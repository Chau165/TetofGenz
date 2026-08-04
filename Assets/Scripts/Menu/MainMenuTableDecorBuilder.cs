using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Builds corner tabletop props from the Medieval Market pack and renders them into a UI overlay texture.
    /// </summary>
    public static class MainMenuTableDecorBuilder
    {
        public const int RenderWidth = 1920;
        public const int RenderHeight = 1080;
        public const int DecorLayer = 6; // MenuTableDecor

        private readonly struct DecorPlacement
        {
            public readonly GameObject Prefab;
            public readonly Vector2 Viewport;
            public readonly Vector3 Rotation;
            public readonly float Scale;

            public DecorPlacement(GameObject prefab, Vector2 viewport, Vector3 rotation, float scale)
            {
                Prefab = prefab;
                Viewport = viewport;
                Rotation = rotation;
                Scale = scale;
            }
        }

        public static RenderTexture Build(Transform root)
        {
            MainMenuTableDecorLibrary library = MainMenuTableDecorLibrary.LoadDefault();
            if (library == null || !HasAnyPrefab(library))
            {
                return null;
            }

            GameObject decorRoot = new GameObject("MenuTableDecor");
            decorRoot.transform.SetParent(root, false);

            RenderTexture renderTexture = new RenderTexture(RenderWidth, RenderHeight, 24, RenderTextureFormat.ARGB32)
            {
                name = "MenuTableDecorRT",
                antiAliasing = 2,
                filterMode = FilterMode.Bilinear
            };
            renderTexture.Create();

            Camera decorCamera = CreateDecorCamera(decorRoot.transform, renderTexture);
            CreateDecorLight(decorRoot.transform);

            Transform propsRoot = new GameObject("Props").transform;
            propsRoot.SetParent(decorRoot.transform, false);

            MainMenuTableDecorRollSimulator rollSimulator =
                propsRoot.gameObject.AddComponent<MainMenuTableDecorRollSimulator>();

            SpawnPlacements(propsRoot, decorCamera, rollSimulator, library, BuildTopLeftPlacements(library));
            SpawnPlacements(propsRoot, decorCamera, rollSimulator, library, BuildTopRightPlacements(library));
            SpawnPlacements(propsRoot, decorCamera, rollSimulator, library, BuildBottomLeftPlacements(library));
            SpawnPlacements(propsRoot, decorCamera, rollSimulator, library, BuildBottomRightPlacements(library));
            SpawnPlacements(propsRoot, decorCamera, rollSimulator, library, BuildEdgeScatterPlacements(library));

            decorRoot.AddComponent<MainMenuTableDecorLifetime>().Initialize(renderTexture);
            return renderTexture;
        }

        private static bool HasAnyPrefab(MainMenuTableDecorLibrary library)
        {
            return library.fruitBasket != null || library.basket != null || library.appleRed != null;
        }

        private static Camera CreateDecorCamera(Transform parent, RenderTexture target)
        {
            GameObject cameraObject = new GameObject("DecorCamera");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 7.4f, -5.8f);
            cameraObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.cullingMask = 1 << DecorLayer;
            camera.orthographic = true;
            camera.orthographicSize = 4.35f;
            camera.aspect = (float)RenderWidth / RenderHeight;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 30f;
            camera.targetTexture = target;
            camera.depth = -50f;
            camera.useOcclusionCulling = false;
            return camera;
        }

        private static void CreateDecorLight(Transform parent)
        {
            GameObject lightObject = new GameObject("DecorLight");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(52f, -28f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.88f, 0.72f);
            light.intensity = 1.12f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.45f;
            light.cullingMask = 1 << DecorLayer;

            GameObject fillObject = new GameObject("DecorFillLight");
            fillObject.transform.SetParent(parent, false);
            fillObject.transform.rotation = Quaternion.Euler(24f, 148f, 0f);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.92f, 0.82f, 0.68f);
            fill.intensity = 0.42f;
            fill.shadows = LightShadows.None;
            fill.cullingMask = 1 << DecorLayer;
        }

        private static DecorPlacement[] BuildTopLeftPlacements(MainMenuTableDecorLibrary library)
        {
            return new[]
            {
                P(library.fruitBasket ?? library.breadBasket, new Vector2(-0.1f, 1.06f), new Vector3(6f, 128f, 5f), 2.75f),
                P(library.basketSmall ?? library.basket, new Vector2(0.04f, 1.02f), new Vector3(0f, -42f, 0f), 2.2f),
                P(library.woodenCrate, new Vector2(0.16f, 0.96f), new Vector3(0f, 24f, 0f), 1.65f),
                P(library.appleRed, new Vector2(0.1f, 0.92f), new Vector3(14f, 208f, 16f), 1.55f),
                P(library.appleRed, new Vector2(0.17f, 0.98f), new Vector3(8f, 162f, -10f), 1.42f),
                P(library.appleGreen, new Vector2(0.13f, 0.88f), new Vector3(-12f, 184f, 12f), 1.48f),
                P(library.appleYellow, new Vector2(0.2f, 0.93f), new Vector3(6f, 138f, -6f), 1.45f),
                P(library.appleYellow, new Vector2(0.08f, 0.86f), new Vector3(-8f, 96f, 14f), 1.38f),
                P(library.tomato, new Vector2(0.19f, 0.86f), new Vector3(10f, 72f, -8f), 1.35f),
                P(library.carrot, new Vector2(0.06f, 0.82f), new Vector3(4f, 118f, 6f), 1.32f),
                P(library.banana, new Vector2(0.15f, 0.8f), new Vector3(0f, -56f, 0f), 1.55f),
            };
        }

        private static DecorPlacement[] BuildTopRightPlacements(MainMenuTableDecorLibrary library)
        {
            return new[]
            {
                P(library.basket, new Vector2(0.92f, 0.98f), new Vector3(0f, -48f, 0f), 2.15f),
                P(library.basketSmall ?? library.basket, new Vector2(0.96f, 0.92f), new Vector3(0f, 62f, 0f), 1.85f),
                P(library.roundBread, new Vector2(0.88f, 0.84f), new Vector3(12f, 44f, 8f), 1.62f),
                P(library.pretzel ?? library.cookie, new Vector2(0.84f, 0.86f), new Vector3(-6f, 58f, -6f), 1.48f),
                P(library.cookie, new Vector2(0.94f, 0.84f), new Vector3(8f, 72f, 10f), 1.38f),
                P(library.roundCheese ?? library.roundBread, new Vector2(0.86f, 0.8f), new Vector3(0f, 36f, 0f), 1.55f),
                P(library.woodenBowl, new Vector2(0.9f, 0.78f), new Vector3(0f, -18f, 0f), 1.72f),
            };
        }

        private static DecorPlacement[] BuildBottomLeftPlacements(MainMenuTableDecorLibrary library)
        {
            return new[]
            {
                P(library.woodenCrate, new Vector2(-0.08f, -0.06f), new Vector3(0f, 14f, 0f), 2.85f),
                P(library.woodenCrate, new Vector2(0.12f, 0.02f), new Vector3(0f, -22f, 0f), 1.75f),
                P(library.sack, new Vector2(0.2f, -0.04f), new Vector3(0f, -18f, 0f), 2.25f),
                P(library.sackOpen ?? library.sack, new Vector2(0.06f, 0.08f), new Vector3(0f, 32f, 0f), 1.95f),
                P(library.appleRed, new Vector2(0.14f, 0.1f), new Vector3(16f, 118f, 14f), 1.42f),
                P(library.appleGreen, new Vector2(0.18f, 0.04f), new Vector3(-10f, 92f, -12f), 1.38f),
                P(library.appleYellow, new Vector2(0.1f, 0.12f), new Vector3(8f, 68f, 16f), 1.35f),
                P(library.beet ?? library.tomato, new Vector2(0.16f, 0.14f), new Vector3(12f, 44f, -8f), 1.42f),
                P(library.roundBread, new Vector2(0.04f, 0.02f), new Vector3(0f, 48f, 0f), 1.68f),
                P(library.pretzel ?? library.cookie, new Vector2(0.2f, 0.1f), new Vector3(-8f, 62f, 6f), 1.45f),
                P(library.potato, new Vector2(0.08f, 0.16f), new Vector3(6f, 84f, -4f), 1.32f),
                P(library.eggsBasket ?? library.basketSmall, new Vector2(-0.02f, 0.1f), new Vector3(0f, -36f, 0f), 1.85f),
            };
        }

        private static DecorPlacement[] BuildBottomRightPlacements(MainMenuTableDecorLibrary library)
        {
            return new[]
            {
                P(library.watermelon, new Vector2(1.04f, 0.1f), new Vector3(8f, 34f, 4f), 2.15f),
                P(library.beet ?? library.tomato, new Vector2(0.96f, 0.16f), new Vector3(0f, 48f, 0f), 1.72f),
                P(library.fruitBasket ?? library.breadBasket, new Vector2(0.9f, -0.02f), new Vector3(0f, -24f, 0f), 2.45f),
                P(library.basket, new Vector2(1.02f, 0.02f), new Vector3(0f, -52f, 0f), 2.2f),
                P(library.roundBread, new Vector2(0.84f, 0.04f), new Vector3(14f, 52f, 12f), 1.68f),
                P(library.cookie, new Vector2(0.98f, 0.14f), new Vector3(-8f, 64f, -10f), 1.42f),
                P(library.pretzel ?? library.cookie, new Vector2(0.88f, 0.12f), new Vector3(6f, 78f, 8f), 1.38f),
                P(library.tomato, new Vector2(0.94f, 0.06f), new Vector3(-12f, 36f, 14f), 1.35f),
                P(library.carrot, new Vector2(0.86f, 0.08f), new Vector3(4f, 102f, -6f), 1.32f),
                P(library.appleBox ?? library.woodenCrate, new Vector2(0.82f, -0.04f), new Vector3(0f, 18f, 0f), 1.78f),
            };
        }

        private static DecorPlacement[] BuildEdgeScatterPlacements(MainMenuTableDecorLibrary library)
        {
            return new[]
            {
                P(library.appleRed, new Vector2(-0.04f, 0.62f), new Vector3(10f, 194f, 12f), 1.28f),
                P(library.appleYellow, new Vector2(0.02f, 0.48f), new Vector3(-6f, 168f, -8f), 1.22f),
                P(library.beet ?? library.tomato, new Vector2(-0.02f, 0.34f), new Vector3(0f, 42f, 0f), 1.35f),
                P(library.basketSmall ?? library.basket, new Vector2(0.06f, 0.26f), new Vector3(0f, -64f, 0f), 1.55f),
                P(library.appleGreen, new Vector2(1.02f, 0.58f), new Vector3(8f, 142f, 10f), 1.25f),
                P(library.beet ?? library.tomato, new Vector2(0.98f, 0.42f), new Vector3(-4f, 88f, 6f), 1.32f),
                P(library.basketSmall ?? library.basket, new Vector2(0.94f, 0.28f), new Vector3(0f, 52f, 0f), 1.48f),
                P(library.appleGreen, new Vector2(0.18f, -0.02f), new Vector3(6f, 112f, -10f), 1.22f),
                P(library.appleRed, new Vector2(0.62f, -0.04f), new Vector3(-10f, 96f, 12f), 1.2f),
                P(library.basketSmall ?? library.basket, new Vector2(0.48f, -0.06f), new Vector3(0f, -28f, 0f), 1.42f),
                P(library.carrot, new Vector2(0.32f, 0.02f), new Vector3(4f, 74f, 8f), 1.18f),
                P(library.tomato, new Vector2(0.72f, 0.0f), new Vector3(-6f, 58f, -6f), 1.15f),
            };
        }

        private static DecorPlacement P(GameObject prefab, Vector2 viewport, Vector3 rotation, float scale)
        {
            return new DecorPlacement(prefab, viewport, rotation, scale);
        }

        private static void SpawnPlacements(Transform parent, Camera camera, MainMenuTableDecorRollSimulator rollSimulator,
            MainMenuTableDecorLibrary library, DecorPlacement[] placements)
        {
            for (int i = 0; i < placements.Length; i++)
            {
                DecorPlacement placement = placements[i];
                SpawnProp(placement.Prefab, parent, camera, rollSimulator, library, placement.Viewport,
                    placement.Rotation, placement.Scale);
            }
        }

        private static GameObject SpawnProp(GameObject prefab, Transform parent, Camera camera,
            MainMenuTableDecorRollSimulator rollSimulator, MainMenuTableDecorLibrary library, Vector2 viewport,
            Vector3 rotationEuler, float scale)
        {
            if (prefab == null)
            {
                return null;
            }

            Vector3 position = ViewportToTablePoint(camera, viewport);
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(rotationEuler);
            instance.transform.localScale = Vector3.one * scale;
            SetLayerRecursive(instance, DecorLayer);
            StripColliders(instance);
            MainMenuPropShadow.Attach(instance, scale);

            if (IsRollableProduce(prefab, library))
            {
                int seed = viewport.GetHashCode() ^ prefab.name.GetHashCode() ^ ((int)(scale * 1000f));
                MainMenuTableDecorRollMotion.Attach(instance, rollSimulator, camera, seed, scale);
            }

            return instance;
        }

        private static bool IsRollableProduce(GameObject prefab, MainMenuTableDecorLibrary library)
        {
            if (prefab == null || library == null)
            {
                return false;
            }

            return prefab == library.appleRed
                || prefab == library.appleGreen
                || prefab == library.appleYellow
                || prefab == library.beet
                || prefab == library.tomato
                || prefab == library.carrot
                || prefab == library.banana
                || prefab == library.potato
                || prefab == library.cookie
                || prefab == library.pretzel;
        }

        public static Vector3 ViewportToTablePoint(Camera camera, Vector2 viewport)
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
            if (Mathf.Abs(ray.direction.y) < 0.0001f)
            {
                return ray.origin;
            }

            float distance = -ray.origin.y / ray.direction.y;
            return ray.origin + ray.direction * distance;
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
