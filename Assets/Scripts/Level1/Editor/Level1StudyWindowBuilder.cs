#if UNITY_EDITOR
using ChoTet.Level1;
using ChoTet.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Open rectangular study window — frame, sill, skybox vista, and physical sunlight.
    /// </summary>
    internal static class Level1StudyWindowBuilder
    {
        private const string MaterialFolder = "Assets/Settings/ChoTet/Materials/Level1";
        private const string MorningSkyboxPath =
            "Assets/Settings/ChoTet/Materials/Level1/Level1_StudyMorningSky.mat";
        private const string MorningSkyboxTemplatePath =
            "Assets/Low-Poly Medieval Market/Materials/Skybox_Mat.mat";

        public const float CenterY = 1.72f;
        public const float HalfHeight = 0.82f;
        public const float HalfWidth = 0.62f;
        public static float IndoorElevationY => Level1RoomElevation.IndoorY;

        private const float RoomHalfWidth = 5.5f;
        private const float RoomHalfDepth = 4.5f;
        private const float WallHalfThickness = 0.09f;

        public static Vector3 GetWindowCenter(float deskWorldZ)
        {
            float leftWallX = -RoomHalfWidth + WallHalfThickness + 0.04f;
            return new Vector3(leftWallX + 0.18f, CenterY, deskWorldZ);
        }

        public static Vector3 GetOpenGapCenter(float deskWorldZ)
        {
            Vector3 local = GetWindowCenter(deskWorldZ);
            return new Vector3(local.x + 0.06f, local.y + IndoorElevationY, local.z);
        }

        public static void ApplyIndoorElevation(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            Transform indoor = worldRoot.Find("Level1_IndoorArea");
            if (indoor == null)
            {
                return;
            }

            Vector3 position = indoor.localPosition;
            position.y = IndoorElevationY;
            indoor.localPosition = position;
        }

        public static void SyncDetachedIndoorActors(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            Transform indoor = worldRoot.Find("Level1_IndoorArea");
            if (indoor == null)
            {
                return;
            }

            ApplyIndoorElevation(worldRoot);

            Vector3 playerSpawnLocal = new Vector3(0f, 0.02f, 0.15f);
            ReparentToIndoor(worldRoot, indoor, "Player", playerSpawnLocal);
            ReparentToIndoor(worldRoot, indoor, "SpawnPoint", playerSpawnLocal);
            ReparentToIndoor(worldRoot, indoor, "IndoorCameraRig", Vector3.zero);
            ResetIndoorCameraRig(indoor);
        }

        private static void ResetIndoorCameraRig(Transform indoor)
        {
            Transform cameraRig = indoor.Find("IndoorCameraRig");
            if (cameraRig == null)
            {
                return;
            }

            cameraRig.localPosition = Vector3.zero;
            cameraRig.localRotation = Quaternion.identity;

            Transform mainCamera = cameraRig.Find("Main Camera");
            if (mainCamera == null)
            {
                return;
            }

            mainCamera.localPosition = Vector3.zero;
            mainCamera.localRotation = Quaternion.identity;

            FirstPersonCameraController firstPerson = mainCamera.GetComponent<FirstPersonCameraController>();
            if (firstPerson != null)
            {
                firstPerson.enabled = true;
                firstPerson.EnsurePrimary();
            }

            ConfigureIndoorCameraForWindow();

            IndoorHighAngleCamera indoorCamera = mainCamera.GetComponent<IndoorHighAngleCamera>();
            if (indoorCamera != null)
            {
                indoorCamera.enabled = false;
            }
        }

        private static void ReparentToIndoor(Transform worldRoot, Transform indoor, string objectName,
            Vector3 localPosition)
        {
            Transform actor = worldRoot.Find(objectName);
            if (actor == null)
            {
                return;
            }

            if (actor.parent != indoor)
            {
                actor.SetParent(indoor, false);
            }

            actor.localPosition = localPosition;
            actor.localRotation = objectName == "Player"
                ? Quaternion.Euler(0f, 90f, 0f)
                : Quaternion.identity;
        }

        public static Vector3 GetWindowUpperRightWorld(float deskWorldZ)
        {
            Vector3 gap = GetOpenGapCenter(deskWorldZ);
            return gap + new Vector3(0f, HalfHeight - 0.06f, HalfWidth - 0.06f);
        }

        public static Vector3 GetWindowSunSourceWorld(float deskWorldZ)
        {
            return GetWindowUpperRightWorld(deskWorldZ) + new Vector3(-2.5f, 0.45f, 0.12f);
        }

        public static Vector3 GetSunBeamDirection(float deskWorldZ)
        {
            Vector3 gap = GetOpenGapCenter(deskWorldZ);
            Vector3 deskSurface = new Vector3(-4.22f, 0.72f + IndoorElevationY, deskWorldZ);
            Vector3 outside = GetWindowSunSourceWorld(deskWorldZ);
            return (deskSurface - outside).normalized;
        }

        public static void Build(Transform rentalRoomInterior, float deskWorldZ)
        {
            if (rentalRoomInterior == null)
            {
                return;
            }

            Transform existing = rentalRoomInterior.Find("StudyWindow");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            float windowZ = deskWorldZ;
            float windowX = -RoomHalfWidth + WallHalfThickness + 0.04f;
            float innerX = windowX + 0.045f;

            Material aluminum = EnsureAluminumFrameMaterial();

            GameObject root = new GameObject("StudyWindow");
            root.transform.SetParent(rentalRoomInterior, false);

            BuildFrame(root.transform, windowX, windowZ, aluminum);
            BuildWindowsill(root.transform, innerX, windowZ, aluminum);
            BuildGlassPane(root.transform, innerX, windowZ);
            BuildOpenShutters(root.transform, innerX, windowZ);
            BuildWindowSkyBackdrop(root.transform, windowZ);

            MarkStaticForGi(root);
        }

        /// <summary>Single light-sky fill placed just outside the window opening.</summary>
        private static void BuildWindowSkyBackdrop(Transform studyWindowRoot, float windowZ)
        {
            Transform existing = studyWindowRoot.Find("Window_OutsideSky");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            float backdropX = -RoomHalfWidth - 0.36f;
            float panelW = HalfWidth * 2f + 0.7f;
            float panelH = HalfHeight * 2f + 0.7f;
            Material skyMat = EnsureOutsideSkyMaterial();

            GameObject root = new GameObject("Window_OutsideSky");
            root.transform.SetParent(studyWindowRoot, false);

            CreateSkyPanel(root.transform, "Sky_Fill", skyMat,
                new Vector3(backdropX, CenterY, windowZ),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(panelW, panelH, 1f));

            MarkStaticForGi(root);
        }

        private static void CreateSkyPanel(Transform parent, string name, Material skyMat, Vector3 localPosition,
            Quaternion localRotation, Vector3 scale)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.name = name;
            panel.transform.SetParent(parent, false);
            panel.transform.localPosition = localPosition;
            panel.transform.localRotation = localRotation;
            panel.transform.localScale = scale;
            StripCollider(panel);

            Renderer renderer = panel.GetComponent<Renderer>();
            renderer.sharedMaterial = skyMat;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material EnsureOutsideSkyMaterial()
        {
            string path = MaterialFolder + "/Window_OutsideSky.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Color");
                }

                mat = new Material(shader);
                mat.name = "Window_OutsideSky";
                AssetDatabase.CreateAsset(mat, path);
            }

            Color sky = Level1IndoorSky.SkyColor;
            mat.SetColor("_BaseColor", sky);
            mat.SetColor("_Color", sky);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static void BuildFloorSunPatch(Transform worldRoot, float deskWorldZ)
        {
            if (worldRoot == null)
            {
                return;
            }

            DestroyIfExists(worldRoot, "FloorSunPatch");

            Vector3 gapCenter = GetOpenGapCenter(deskWorldZ);
            Vector3 intoRoom = GetSunBeamDirection(deskWorldZ);
            Vector3 patchCenter = gapCenter + intoRoom * 1.85f;
            patchCenter.y = Level1SceneBuilder.FloorTopY + IndoorElevationY + 0.008f;

            Material patchMat = EnsureSunPatchMaterial();
            GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Quad);
            patch.name = "FloorSunPatch";
            patch.transform.SetParent(worldRoot, false);
            patch.transform.position = patchCenter;
            patch.transform.rotation = Quaternion.Euler(90f, Mathf.Atan2(intoRoom.x, intoRoom.z) * Mathf.Rad2Deg, 0f);
            patch.transform.localScale = new Vector3(1.35f, 2.1f, 1f);

            Collider col = patch.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }

            Renderer renderer = patch.GetComponent<Renderer>();
            renderer.sharedMaterial = patchMat;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void BuildOpenShutters(Transform root, float innerX, float windowZ)
        {
            Material wood = EnsureMaterial("Window_ShutterWood.mat", new Color(0.62f, 0.48f, 0.34f), 0.28f, 0f, false);
            float openH = HalfHeight * 2f - 0.08f;
            float halfW = HalfWidth - 0.05f;
            float shutterThick = 0.018f;
            float hingeX = innerX + 0.02f;

            BuildShutterLeaf(root, "Window_Shutter_Left", wood, hingeX, windowZ - halfW, openH, halfW, shutterThick,
                -105f);
            BuildShutterLeaf(root, "Window_Shutter_Right", wood, hingeX, windowZ + halfW, openH, halfW, shutterThick,
                105f);
        }

        private static void BuildShutterLeaf(Transform root, string name, Material wood, float hingeX, float hingeZ,
            float height, float width, float thickness, float yawDegrees)
        {
            GameObject pivot = new GameObject(name + "_Pivot");
            pivot.transform.SetParent(root, false);
            pivot.transform.localPosition = new Vector3(hingeX, CenterY, hingeZ);
            pivot.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);

            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.name = name;
            leaf.transform.SetParent(pivot.transform, false);
            leaf.transform.localPosition = new Vector3(0f, 0f, -width * 0.5f);
            leaf.transform.localScale = new Vector3(thickness, height, width);
            StripCollider(leaf);
            ApplyMaterial(leaf, wood);
            leaf.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
        }

        private static Material EnsureSunPatchMaterial()
        {
            return EnsureMaterial("Floor_SunPatch.mat", new Color(1f, 0.92f, 0.62f, 0.16f), 0.05f, 0f, true);
        }

        /// <summary>Remove all exterior props — the open window shows skybox only.</summary>
        public static void ClearExteriorWorld(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            DestroyIfExists(worldRoot, "StudyWindowExterior");
        }

        public static void BuildLeftWallWithOpening(Transform room, Material wallMaterial, float deskWorldZ)
        {
            float wallX = -RoomHalfWidth;
            float thickness = 0.18f;
            float windowZ = deskWorldZ;
            float zMin = windowZ - HalfWidth - 0.1f;
            float zMax = windowZ + HalfWidth + 0.1f;
            float yMin = CenterY - HalfHeight - 0.06f;
            float yMax = CenterY + HalfHeight + 0.06f;
            float wallDepth = RoomHalfDepth * 2f;
            float ceilingBottomY = Level1SceneBuilder.WallHeight - 0.04f;
            float aboveHeight = ceilingBottomY - yMax;

            CreateWallCube(room, "Wall_Left_Below", wallX, yMin * 0.5f, 0f,
                thickness, yMin, wallDepth, wallMaterial);
            if (aboveHeight > 0.02f)
            {
                CreateWallCube(room, "Wall_Left_Above", wallX, yMax + aboveHeight * 0.5f, 0f,
                    thickness, aboveHeight, wallDepth, wallMaterial);
            }

            // NOTE: depth must span only from the room edge to the window opening edge (no *2 —
            // doubling it previously made these slabs overlap and completely seal the opening).
            float frontDepth = RoomHalfDepth + zMin;
            float frontCenterZ = -RoomHalfDepth + frontDepth * 0.5f;
            float openingHeight = yMax - yMin;
            CreateWallCube(room, "Wall_Left_Front", wallX, (yMin + yMax) * 0.5f, frontCenterZ,
                thickness, openingHeight, frontDepth, wallMaterial);

            float backDepth = RoomHalfDepth - zMax;
            float backCenterZ = RoomHalfDepth - backDepth * 0.5f;
            CreateWallCube(room, "Wall_Left_Back", wallX, (yMin + yMax) * 0.5f, backCenterZ,
                thickness, openingHeight, Mathf.Max(backDepth, 0.5f), wallMaterial);
        }

        public static void ConfigureMorningEnvironment()
        {
            Level1IndoorSky.Apply();
            ConfigureIndoorCameraForWindow();
        }

        private static Material EnsureStudyMorningSkybox()
        {
            Material skybox = AssetDatabase.LoadAssetAtPath<Material>(MorningSkyboxPath);
            if (skybox != null)
            {
                return skybox;
            }

            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets/Settings/ChoTet/Materials", "Level1");
            }

            Material template = AssetDatabase.LoadAssetAtPath<Material>(MorningSkyboxTemplatePath);
            if (template == null)
            {
                return null;
            }

            skybox = new Material(template);
            AssetDatabase.CreateAsset(skybox, MorningSkyboxPath);
            return skybox;
        }

        public static void ConfigureIndoorCameraForWindow()
        {
            Level1IndoorSky.Apply(Camera.main);
        }

        public static void BuildMorningLighting(Transform worldRoot, float deskWorldZ)
        {
            if (worldRoot == null)
            {
                return;
            }

            DestroyAllFakeWindowVisuals(worldRoot);

            DestroyIfExists(worldRoot, "WindowSunlight");
            DestroyIfExists(worldRoot, "WindowSunFromAbove");
            DestroyIfExists(worldRoot, "WindowSunBeam");
            DestroyIfExists(worldRoot, "WindowSkyDome");
            DestroyIfExists(worldRoot, "WindowExteriorSun");
            DestroyIfExists(worldRoot, "WindowExteriorInfill");
            DestroyIfExists(worldRoot, "WindowPortalArea");
            DestroyIfExists(worldRoot, "WindowAreaGlow");
            DestroyIfExists(worldRoot, "WindowMorningFill");
            DestroyIfExists(worldRoot, "RoomWarmFill");
            DestroyIfExists(worldRoot, "WindowDaylight");
            DestroyIfExists(worldRoot, "DeskWarm");

            Vector3 gapCenter = GetOpenGapCenter(deskWorldZ);
            Vector3 sunSource = GetWindowSunSourceWorld(deskWorldZ);
            Vector3 intoRoom = (gapCenter - sunSource).normalized;

            GameObject sunAbove = new GameObject("WindowSunFromAbove");
            sunAbove.transform.SetParent(worldRoot, false);
            sunAbove.transform.rotation = Quaternion.LookRotation(intoRoom, Vector3.up);
            Light topSun = sunAbove.AddComponent<Light>();
            topSun.type = LightType.Directional;
            topSun.color = new Color(1f, 0.96f, 0.82f);
            topSun.intensity = 1.35f;
            topSun.shadows = LightShadows.Soft;
            topSun.shadowStrength = 0.36f;

            BuildWindowMorningFill(worldRoot, deskWorldZ);
        }

        private static void BuildWindowMorningFill(Transform worldRoot, float deskWorldZ)
        {
            GameObject fill = new GameObject("WindowMorningFill");
            fill.transform.SetParent(worldRoot, false);
            fill.transform.position = new Vector3(-1.2f, 1.8f + IndoorElevationY, deskWorldZ);
            Light point = fill.AddComponent<Light>();
            point.type = LightType.Point;
            point.color = new Color(0.92f, 0.94f, 1f);
            point.intensity = 0.9f;
            point.range = 11f;
            point.shadows = LightShadows.None;
        }

        public static void DestroyAllFakeWindowVisuals(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            string[] fakeNames =
            {
                "ExteriorSkyBackdrop",
                "ExteriorSunGlow_Top",
                "ExteriorSunGlow_Core",
                "ExteriorHorizonGlow",
                "FloorSunPatch",
                "Window_SkyBackdrop",
                "Window_ExteriorSky",
                "Window_ExteriorSky_Deep",
                "Window_ExteriorView",
                "Window_LightShaft_Main",
                "Window_LightShaft_Soft",
                "Window_LightShafts"
            };

            Transform[] all = worldRoot.GetComponentsInChildren<Transform>(true);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                Transform t = all[i];
                if (t == worldRoot)
                {
                    continue;
                }

                for (int j = 0; j < fakeNames.Length; j++)
                {
                    if (t.name == fakeNames[j])
                    {
                        Object.DestroyImmediate(t.gameObject);
                        break;
                    }
                }
            }
        }

        private static void DestroyLegacyFakeWindowVisuals(Transform worldRoot)
        {
            DestroyAllFakeWindowVisuals(worldRoot);
        }

        private static void BuildFrame(Transform root, float windowX, float windowZ, Material aluminum)
        {
            float frameDepth = 0.055f;
            float frameThick = 0.042f;
            float h = HalfHeight * 2f + 0.1f;
            float w = HalfWidth * 2f + 0.08f;

            CreateDecorCube("Window_Frame_Left", root, new Vector3(windowX, CenterY, windowZ - HalfWidth - 0.04f),
                new Vector3(frameDepth, h, frameThick), aluminum);
            CreateDecorCube("Window_Frame_Right", root, new Vector3(windowX, CenterY, windowZ + HalfWidth + 0.04f),
                new Vector3(frameDepth, h, frameThick), aluminum);
            CreateDecorCube("Window_Frame_Top", root, new Vector3(windowX, CenterY + HalfHeight + 0.05f, windowZ),
                new Vector3(frameDepth, frameThick, w), aluminum);
        }

        private static void BuildWindowsill(Transform root, float innerX, float windowZ, Material aluminum)
        {
            float openW = HalfWidth * 2f - 0.06f;
            CreateDecorCube("Window_Sill", root,
                new Vector3(innerX + 0.012f, CenterY - HalfHeight + 0.028f, windowZ),
                new Vector3(0.048f, 0.058f, openW - 0.03f), aluminum);
        }

        private static void BuildGlassPane(Transform root, float innerX, float windowZ)
        {
            Material glass = EnsureWindowGlassMaterial();
            float openH = HalfHeight * 2f - 0.07f;
            float openW = HalfWidth * 2f - 0.07f;

            GameObject pane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pane.name = "Window_Glass_Pane";
            pane.transform.SetParent(root, false);
            pane.transform.localPosition = new Vector3(innerX + 0.014f, CenterY, windowZ);
            pane.transform.localScale = new Vector3(0.011f, openH, openW);
            StripCollider(pane);

            Renderer renderer = pane.GetComponent<Renderer>();
            renderer.sharedMaterial = glass;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material EnsureWindowGlassMaterial()
        {
            string path = MaterialFolder + "/Window_ClearGlass.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.name = "Window_ClearGlass";
                AssetDatabase.CreateAsset(mat, path);
            }

            Color glass = new Color(0.80f, 0.89f, 0.96f, 0.18f);
            mat.SetColor("_BaseColor", glass);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.SetFloat("_Metallic", 0.03f);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)RenderQueue.Transparent;
            return mat;
        }

        private static Material EnsureAluminumFrameMaterial()
        {
            return EnsureMaterial("Window_Aluminum.mat", new Color(0.86f, 0.88f, 0.91f), 0.42f, 0.48f, false);
        }

        private static Material EnsureMaterial(string fileName, Color color, float smoothness, float metallic,
            bool transparent)
        {
            string path = MaterialFolder + "/" + fileName;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.name = fileName.Replace(".mat", string.Empty);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)RenderQueue.Transparent;
            }

            return mat;
        }

        private static void CreateWallCube(Transform room, string name, float x, float y, float z,
            float sx, float sy, float sz, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(room, false);
            cube.transform.localPosition = new Vector3(x, y, z);
            cube.transform.localScale = new Vector3(sx, sy, sz);
            StripCollider(cube);
            ApplyMaterial(cube, material);
            MarkStaticForGi(cube);
        }

        private static void CreateDecorCube(string name, Transform parent, Vector3 center, Vector3 scale, Material mat)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = center;
            cube.transform.localScale = scale;
            StripCollider(cube);
            ApplyMaterial(cube, mat);
            cube.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
            cube.GetComponent<Renderer>().receiveShadows = true;
            MarkStaticForGi(cube);
        }


        private static void ApplyMaterial(GameObject go, Material mat)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mat;
            }
        }

        private static void StripCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }
        }

        private static void MarkStaticForGi(GameObject go)
        {
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            flags |= StaticEditorFlags.ContributeGI;
            flags |= StaticEditorFlags.BatchingStatic;
            GameObjectUtility.SetStaticEditorFlags(go, flags);
        }

        public static void DestroyLegacyWindowDecor(Transform interior)
        {
            if (interior == null)
            {
                return;
            }

            string[] legacyPaths =
            {
                "StudyWindow/Window_Curtain_Left",
                "StudyWindow/Window_Curtain_Right",
                "StudyWindow/Window_Curtain_Rod",
                "StudyWindow/Window_Blind",
                "StudyWindow/Window_Fabric",
                "StudyWindow/Window_Glass_Fixed",
                "StudyWindow/Window_Glass_Slide",
                "StudyWindow/Window_Glass_PanelA",
                "StudyWindow/Window_Glass_PanelB",
                "StudyWindow/Window_Glass_Pane",
                "StudyWindow/Window_Frame_Bottom",
                "StudyWindow/Window_Frame_Track",
                "StudyWindow/Window_Sill_Inner",
                "StudyWindow/Window_Frame_Mullion",
                "StudyWindow/Window_Opening",
                "StudyWindow/Window_ExteriorView",
                "StudyWindow/Window_ExteriorSky",
                "StudyWindow/Window_ExteriorSky_Deep",
                "StudyWindow/Window_ExteriorStreet",
                "StudyWindow/Window_LightShaft_Main",
                "StudyWindow/Window_LightShaft_Soft",
                "StudyWindow/Window_SkyBackdrop"
            };
            for (int i = 0; i < legacyPaths.Length; i++)
            {
                Transform t = interior.Find(legacyPaths[i]);
                if (t != null)
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
        }

        private static void DestroyIfExists(Transform root, string path)
        {
            Transform t = root.Find(path);
            if (t != null)
            {
                Object.DestroyImmediate(t.gameObject);
            }
        }
    }
}
#endif
