#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>
    /// Bed-surface props (pillow, clothing box) and wall-mounted FPT shirt on a hanger at the bed head.
    /// </summary>
    internal static class Level1BedPropsBuilder
    {
        private const string MaterialFolder = "Assets/Settings/ChoTet/Materials/Level1";

        private const float RoomHalfDepth = 4.5f;
        private const float WallHalfThickness = 0.09f;
        private const float ShirtWallGap = 0.015f;

        private static readonly Vector3 PillowBedLocal = new(0f, 0f, 0.42f);
        private static readonly Vector3 ClothingBoxBedLocal = new(0.35f, 0f, -0.58f);

        private const float ClothingBoxYawExtra = 6f;
        private const float PropOverlapClearance = 0.05f;
        private const int MaxOverlapResolvePasses = 8;
        private const float WallHangerHeight = 1.55f;
        private const float WallHangerScale = 1.1f;
        private const float WallHangerClearanceFromBed = 0.08f;

        public static void PlaceAll(GameObject bed)
        {
            if (bed == null)
            {
                return;
            }

            Transform sleepingArea = bed.transform.parent;
            DestroyLegacyProps(bed, sleepingArea);
            PlaceMattressProps(bed, sleepingArea);
            PlaceWallHanger(bed, sleepingArea);
        }

        private static void PlaceMattressProps(GameObject bed, Transform sleepingArea)
        {
            if (!TryGetMattressSurfaceY(bed, out float surfaceY))
            {
                surfaceY = bed.transform.position.y;
            }

            float bedYaw = bed.transform.eulerAngles.y;

            GameObject pillow = PlaceFbxProp(bed, sleepingArea, Level1RoomAssetPaths.BedPillowFbx, "BedPillow",
                PillowBedLocal, surfaceY, Quaternion.Euler(0f, bedYaw, 0f));
            GameObject box = PlaceFbxProp(bed, sleepingArea, Level1RoomAssetPaths.BedClothingBoxFbx, "BedClothingBox",
                ClothingBoxBedLocal, surfaceY, Quaternion.Euler(0f, bedYaw + ClothingBoxYawExtra, 0f));

            if (pillow != null)
            {
                ApplyPillowMaterials(pillow);
                StripColliders(pillow);
            }

            if (box != null)
            {
                ApplyClothingBoxMaterials(box);
                StripColliders(box);
            }

            SnapToMattress(pillow, surfaceY);
            SnapToMattress(box, surfaceY);
            ResolveMattressPropOverlaps(bed, pillow, box, surfaceY);
            SnapToMattress(pillow, surfaceY);
            SnapToMattress(box, surfaceY);
        }

        private static void PlaceWallHanger(GameObject bed, Transform sleepingArea)
        {
            // Prefer the orange-baked prefab so Project/Scene always show solid cam, not white FBX Lit.
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(Level1RoomAssetPaths.FptWallHangerPrefab);
            if (source == null)
            {
                source = AssetDatabase.LoadAssetAtPath<GameObject>(Level1RoomAssetPaths.FptWallHangerFbx);
            }

            if (source == null)
            {
                Debug.LogWarning("[Level1BedPropsBuilder] Missing wall hanger at " +
                                 Level1RoomAssetPaths.FptWallHangerPrefab + " / " +
                                 Level1RoomAssetPaths.FptWallHangerFbx);
                return;
            }

            float frontWallInnerZ = RoomHalfDepth - WallHalfThickness;
            float bedHeadX = GetBedHeadWorldX(bed);

            Vector3 position = new Vector3(bedHeadX, WallHangerHeight, frontWallInnerZ);
            GameObject hanger = (GameObject)PrefabUtility.InstantiatePrefab(source);
            hanger.name = "FPT_WallHanger";
            hanger.transform.SetParent(sleepingArea != null ? sleepingArea : bed.transform, true);
            hanger.transform.position = position;
            hanger.transform.rotation = Quaternion.identity;
            hanger.transform.localScale = Vector3.one * WallHangerScale;

            ApplyWallHangerMaterials(hanger);
            StripColliders(hanger);
            SnapShirtBackToWall(hanger, frontWallInnerZ - ShirtWallGap);
            ResolveWallHangerBedOverlap(bed, hanger);
            SnapShirtBackToWall(hanger, frontWallInnerZ - ShirtWallGap);
            PinWallNailIntoWall(hanger, frontWallInnerZ);
            AlignLogoDecalInFrontOfShirt(hanger);
        }

        private const float LogoChestForwardOffset = 0.02f;

        private static void AlignLogoDecalInFrontOfShirt(GameObject hanger)
        {
            if (hanger == null)
            {
                return;
            }

            Transform logo = FindChildByName(hanger.transform, "FPT_LogoDecal");
            if (logo == null)
            {
                return;
            }

            Renderer shirtRenderer = null;
            Renderer[] renderers = hanger.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                string meshName = renderers[i].gameObject.name;
                if (meshName.IndexOf("Shirt", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && meshName.IndexOf("Logo", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    shirtRenderer = renderers[i];
                    break;
                }
            }

            if (shirtRenderer == null)
            {
                return;
            }

            float shirtFrontZ = shirtRenderer.bounds.min.z;
            Vector3 logoPos = logo.position;
            float targetZ = shirtFrontZ - LogoChestForwardOffset;
            if (logoPos.z > targetZ - 0.0005f)
            {
                logoPos.z = targetZ;
                logo.position = logoPos;
            }
        }

        private static float GetBedHeadWorldX(GameObject bed)
        {
            if (Level1ScenePlacement.TryGetKenneyBedCoverBounds(bed, out Bounds coverBounds))
            {
                return coverBounds.center.x;
            }

            return RoomLayout.Bed.x;
        }

        private const float WallNailIntoWallDepth = 0.022f;
        private const float WallNailHeadRoomOffset = 0.004f;

        /// <summary>
        /// Moves the whole hanger so the nail is pinned into the wall and the hook stays attached.
        /// </summary>
        private static void PinWallNailIntoWall(GameObject hanger, float wallInnerZ)
        {
            if (hanger == null)
            {
                return;
            }

            if (FindChildByName(hanger.transform, "WallNail") == null)
            {
                return;
            }

            float nailBackZ = float.MinValue;
            float nailFrontZ = float.MaxValue;
            Renderer[] renderers = hanger.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                string meshName = renderers[i].gameObject.name;
                if (meshName.IndexOf("WallNail", System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Bounds bounds = renderers[i].bounds;
                nailBackZ = Mathf.Max(nailBackZ, bounds.max.z);
                nailFrontZ = Mathf.Min(nailFrontZ, bounds.min.z);
            }

            if (nailBackZ == float.MinValue || nailFrontZ == float.MaxValue)
            {
                return;
            }

            float targetNailBackZ = wallInnerZ + WallNailIntoWallDepth;
            float deltaZ = targetNailBackZ - nailBackZ;
            if (Mathf.Abs(deltaZ) > 0.0001f)
            {
                hanger.transform.position += new Vector3(0f, 0f, deltaZ);
            }

            nailBackZ += deltaZ;
            nailFrontZ += deltaZ;
            float maxHeadProtrusion = wallInnerZ - WallNailHeadRoomOffset;
            if (nailFrontZ < maxHeadProtrusion)
            {
                float headDelta = maxHeadProtrusion - nailFrontZ;
                hanger.transform.position += new Vector3(0f, 0f, headDelta);
            }
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildByName(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SnapShirtBackToWall(GameObject hanger, float shirtBackZ)
        {
            if (hanger == null)
            {
                return;
            }

            Renderer[] renderers = hanger.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            float maxZ = float.MinValue;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].gameObject.name.IndexOf("WallNail", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || renderers[i].gameObject.name.IndexOf("LogoDecal", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                maxZ = Mathf.Max(maxZ, renderers[i].bounds.max.z);
            }

            if (maxZ == float.MinValue)
            {
                return;
            }

            float deltaZ = shirtBackZ - maxZ;
            if (Mathf.Abs(deltaZ) > 0.0001f)
            {
                hanger.transform.position += new Vector3(0f, 0f, deltaZ);
            }
        }

        private static void ResolveWallHangerBedOverlap(GameObject bed, GameObject hanger)
        {
            if (bed == null || hanger == null)
            {
                return;
            }

            if (!Level1ScenePlacement.TryGetKenneyBedCoverBounds(bed, out Bounds bedBounds))
            {
                return;
            }

            Bounds hangerBounds = GetCombinedBounds(hanger);
            float bedHeadZ = bedBounds.max.z;
            float minGapZ = bedHeadZ + WallHangerClearanceFromBed;

            if (hangerBounds.min.z < minGapZ + PropOverlapClearance)
            {
                float shiftX = bedBounds.center.x > 0f ? -0.12f : 0.12f;
                hanger.transform.position += new Vector3(shiftX, 0f, 0f);
            }

            hangerBounds = GetCombinedBounds(hanger);
            if (Level1ScenePlacement.OverlapsXZ(hanger, bed, PropOverlapClearance))
            {
                float shiftX = hanger.transform.position.x >= bedBounds.center.x ? 0.14f : -0.14f;
                hanger.transform.position += new Vector3(shiftX, 0f, 0f);
            }
        }

        private static Bounds GetCombinedBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void DestroyLegacyProps(GameObject bed, Transform sleepingArea)
        {
            string[] legacyNames =
            {
                "FPT_ShirtCutout", "FPT_BedShirt", "FPT_WallHanger", "BedPillow", "BedClothingBox", "ClothesPile"
            };
            for (int i = 0; i < legacyNames.Length; i++)
            {
                string name = legacyNames[i];
                Transform onBed = bed.transform.Find(name);
                if (onBed != null)
                {
                    Object.DestroyImmediate(onBed.gameObject);
                }

                if (sleepingArea != null)
                {
                    Transform onZone = sleepingArea.Find(name);
                    if (onZone != null)
                    {
                        Object.DestroyImmediate(onZone.gameObject);
                    }
                }
            }

            Transform kenneyPillow = bed.transform.Find("BedPillow");
            if (kenneyPillow != null)
            {
                Object.DestroyImmediate(kenneyPillow.gameObject);
            }
        }

        private static GameObject PlaceFbxProp(GameObject bed, Transform parent, string assetPath, string objectName,
            Vector3 bedLocalOffset, float surfaceY, Quaternion worldRotation)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (source == null)
            {
                Debug.LogWarning("[Level1BedPropsBuilder] Missing FBX at " + assetPath);
                return null;
            }

            Vector3 worldPoint = Level1ScenePlacement.TryGetBedPackableWorldPoint(bed, bedLocalOffset, out Vector3 anchored)
                ? anchored
                : bed.transform.TransformPoint(bedLocalOffset);
            worldPoint.y = surfaceY;

            GameObject prop = (GameObject)PrefabUtility.InstantiatePrefab(source);
            prop.name = objectName;
            prop.transform.SetParent(parent != null ? parent : bed.transform, true);
            prop.transform.position = worldPoint;
            prop.transform.rotation = worldRotation;
            prop.transform.localScale = Vector3.one;
            return prop;
        }

        private static bool TryGetMattressSurfaceY(GameObject bed, out float surfaceY)
        {
            surfaceY = bed.transform.position.y;
            if (Level1ScenePlacement.TryGetBedMattressTopWorldY(bed, "Bed", out float mattressTopY))
            {
                surfaceY = mattressTopY + Level1ScenePlacement.SurfaceContactLift;
                return true;
            }

            if (Level1ScenePlacement.TryGetFurnitureTopWorldY(bed, "Bed", out float bedTopY))
            {
                surfaceY = bedTopY + Level1ScenePlacement.SurfaceContactLift;
                return true;
            }

            return false;
        }

        private static void ResolveMattressPropOverlaps(GameObject bed, GameObject pillow, GameObject box, float surfaceY)
        {
            for (int pass = 0; pass < MaxOverlapResolvePasses; pass++)
            {
                bool moved = false;

                if (pillow != null && box != null
                    && Level1ScenePlacement.OverlapsXZ(pillow, box, PropOverlapClearance))
                {
                    box.transform.position += bed.transform.TransformVector(new Vector3(0.05f, 0f, -0.04f));
                    moved = true;
                }

                if (!moved)
                {
                    break;
                }

                SnapToMattress(pillow, surfaceY);
                SnapToMattress(box, surfaceY);
            }
        }

        private static void SnapToMattress(GameObject prop, float surfaceY)
        {
            if (prop == null)
            {
                return;
            }

            Level1ScenePlacement.SnapPropBottomToWorldY(prop, surfaceY, true);
        }

        private static void ApplyWallHangerMaterials(GameObject root)
        {
            const string logoPath = "Assets/fpt_uni.png";
            // Solid FPT orange (no albedo UV map) so the full shirt mesh is opaque orange.
            // The old FPT_ShirtHanger_Albedo.png only paints a partial UV island → translucent holes.
            // Use a dedicated solid mat name so we never inherit leftover BaseMap from FPT_ShirtHanger.mat.
            Material shirtMat = GetOrCreateSolidMaterial("FPT_ShirtHanger_Solid.mat",
                new Color(0.933f, 0.447f, 0.075f));
            Material logoMat = GetOrCreatePortraitLogoMaterial("FPT_ShirtHanger_LogoDecal.mat", logoPath);

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Material nailMat = GetOrCreateSolidMaterial("WallNail_Metal.mat", new Color(0.28f, 0.29f, 0.32f));
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                string meshName = renderer.gameObject.name;
                if (meshName.IndexOf("LogoDecal", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Assign all slots — avoid leftover FBX materials on unused slots.
                    renderer.sharedMaterials = new[] { logoMat };
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                else if (meshName.IndexOf("WallNail", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterials = new[] { nailMat };
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
                else
                {
                    // Shirt + hanger mesh: every material slot must be solid orange
                    // (otherwise FBX default Lit looks white with partial albedo UV islands).
                    var slots = new Material[Mathf.Max(1, renderer.sharedMaterials.Length)];
                    for (int s = 0; s < slots.Length; s++)
                    {
                        slots[s] = shirtMat;
                    }

                    renderer.sharedMaterials = slots;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
            }
        }

        private static Material GetOrCreatePortraitLogoMaterial(string fileName, string texturePath)
        {
            EnsureFolder(MaterialFolder);
            string materialPath = MaterialFolder + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                            ?? Shader.Find("Unlit/Transparent")
                            ?? Shader.Find("Sprites/Default");

            if (material == null)
            {
                material = new Material(shader);
            }
            else
            {
                material.shader = shader;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }

                material.mainTexture = texture;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            ConfigureTransparentLogoMaterial(material);
            // URP may ignore _Blend writes on the asset; force premultiply keywords on the saved material.
            material.SetFloat("_Blend", 1f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

            if (!AssetDatabase.Contains(material))
            {
                AssetDatabase.CreateAsset(material, materialPath);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTransparentLogoMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Blend"))
            {
                // Use premultiplied alpha blending for the logo PNG.
                // fpt_uni.png contains pre-associated alpha, so non-premultiplied blending makes the logo appear washed out.
                material.SetFloat("_Blend", 1f);
            }

            if (material.HasProperty("_AlphaClip"))
            {
                material.SetFloat("_AlphaClip", 0f);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", 0f);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private static void ApplyPillowMaterials(GameObject pillowRoot)
        {
            Material pillowMat = GetOrCreateSolidMaterial("BedPillow_Cream.mat", new Color(0.96f, 0.94f, 0.88f));
            ApplySolidMaterial(pillowRoot, pillowMat);
        }

        private static void ApplyClothingBoxMaterials(GameObject boxRoot)
        {
            Material boxMat = GetOrCreateSolidMaterial("BedClothingBox_Cardboard.mat", new Color(0.72f, 0.58f, 0.40f));
            Material clothBlue = GetOrCreateSolidMaterial("BedCloth_Blue.mat", new Color(0.25f, 0.45f, 0.75f));
            Material clothRed = GetOrCreateSolidMaterial("BedCloth_Red.mat", new Color(0.78f, 0.22f, 0.18f));
            Material clothGreen = GetOrCreateSolidMaterial("BedCloth_Green.mat", new Color(0.30f, 0.65f, 0.35f));

            Renderer[] renderers = boxRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                string meshName = renderer.gameObject.name;
                if (meshName.IndexOf("Cloth_1", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterial = clothBlue;
                }
                else if (meshName.IndexOf("Cloth_2", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterial = clothRed;
                }
                else if (meshName.IndexOf("Cloth_3", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    renderer.sharedMaterial = clothGreen;
                }
                else
                {
                    renderer.sharedMaterial = boxMat;
                }

                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private static void ApplySolidMaterial(GameObject root, Material material)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
            }
        }

        private static void StripColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
        }

        private static Material GetOrCreateSolidMaterial(string fileName, Color color)
        {
            EnsureFolder(MaterialFolder);
            string materialPath = MaterialFolder + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                            ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader);
            }
            else
            {
                material.shader = shader;
            }

            // Clear any leftover textures so UV holes in old albedo maps no longer create
            // transparent patches — solid color fills the entire mesh.
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", null);
            }

            material.mainTexture = null;

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 0f);
            }

            if (material.HasProperty("_Cull"))
            {
                // Double-sided so thin sleeve/hem faces don't look transparent.
                material.SetFloat("_Cull", 0f);
            }

            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            if (!AssetDatabase.Contains(material))
            {
                AssetDatabase.CreateAsset(material, materialPath);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateTexturedMaterial(string fileName, string texturePath, Color fallbackColor)
        {
            EnsureFolder(MaterialFolder);
            string materialPath = MaterialFolder + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                            ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader);
            }
            else
            {
                material.shader = shader;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }

                material.mainTexture = texture;
            }

            // Always tint with the intended fallbackColor.
            // Some albedo textures in this project are not the correct shirt color, so using Color.white
            // would make the model appear grey instead of orange.
            Color color = fallbackColor;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            if (!AssetDatabase.Contains(material))
            {
                AssetDatabase.CreateAsset(material, materialPath);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(leaf))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        /// <summary>Bed layout anchor used when cover bounds are unavailable.</summary>
        private static class RoomLayout
        {
            public static readonly Vector3 Bed = new(2.68f, 0f, 2.78f);
        }
    }
}
#endif
