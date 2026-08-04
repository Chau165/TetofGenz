using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Applies URP materials to runtime-spawned traffic vehicle meshes.</summary>
    public static class Level1TrafficVisualUtility
    {
        private static Material[] bodyColors;
        private static Material pickupBody;
        private static Material wheelMaterial;
        private static Material glassMaterial;
        private static Material kenneyColormapBase;

        public static void Configure(
            Material[] bodies,
            Material pickup,
            Material wheel,
            Material glass,
            Material kenneyColormap = null)
        {
            bodyColors = FilterNulls(bodies);
            pickupBody = pickup;
            wheelMaterial = wheel;
            glassMaterial = glass;
            kenneyColormapBase = kenneyColormap;
        }

        public static void ApplyTransportLook(GameObject root, Level1TrafficVehicleProfile profile)
        {
            ApplyCarLook(
                root,
                profile.bodyColorIndex,
                profile.visualKind == Level1TrafficVehicleVisualKind.PickupCar);
        }

        public static void ApplyCarLook(GameObject root, int bodyColorIndex, bool isPickup = false)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                ApplyCarLookToRenderer(renderer, bodyColorIndex, isPickup);

                // Some imported prefabs use non-standard mesh names and were skipped before,
                // leaving the whole vehicle as an uncolored brown mesh.
                if (!IsTrafficCarPart(renderer.gameObject.name))
                {
                    ApplyGenericBodyTint(renderer, bodyColorIndex, isPickup);
                }
            }
        }

        public static string GetBodyColorName(int bodyColorIndex)
        {
            Material material = PickBodyColor(bodyColorIndex);
            string materialName = material != null ? material.name.ToLowerInvariant() : string.Empty;
            if (materialName.Contains("red")) return "Đỏ";
            if (materialName.Contains("brown")) return "Nâu";
            if (materialName.Contains("green")) return "Xanh lá";
            if (materialName.Contains("orange")) return "Cam";
            if (materialName.Contains("yellow") || materialName.Contains("gold")) return "Vàng";
            if (materialName.Contains("blue")) return "Xanh dương";
            if (materialName.Contains("white") || materialName.Contains("cream")) return "Trắng kem";
            if (materialName.Contains("black")) return "Đen";
            if (materialName.Contains("gray") || materialName.Contains("grey")) return "Xám";

            string[] fallbackNames = { "Đỏ", "Nâu", "Xanh lá", "Cam", "Đỏ đô", "Vàng đồng", "Vàng", "Trắng kem" };
            int safe = Mathf.Abs(bodyColorIndex) % fallbackNames.Length;
            return fallbackNames[safe];
        }

        public static void ApplyCarLookToRenderer(Renderer renderer, int bodyColorIndex, bool isPickup = false)
        {
            if (renderer == null)
            {
                return;
            }

            string objectName = renderer.gameObject.name;
            if (IsKenneyCarKitPart(objectName))
            {
                ApplyKenneyLookToRenderer(renderer, bodyColorIndex, isPickup);
                return;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.SetPropertyBlock(block);

            Material resolved = ResolveLegacyMaterial(objectName, bodyColorIndex, isPickup);
            if (resolved != null)
            {
                AssignMaterial(renderer, resolved);
            }
        }

        public static bool IsBrokenVectorCarPart(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return false;
            }

            string lower = objectName.ToLowerInvariant();
            return lower.Contains("car_") || lower.StartsWith("car ") || lower.Contains("trafficcar");
        }

        public static bool IsKenneyCarKitPart(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return false;
            }

            string lower = objectName.ToLowerInvariant();
            if (lower.Contains("wheel-") || lower.Contains("wheel_"))
            {
                return true;
            }

            if (lower == "body" || lower == "spoiler" || lower == "bumper")
            {
                return true;
            }

            if (lower.Contains("sedan") || lower.Contains("hatchback") || lower.Contains("suv")
                || lower.Contains("taxi") || lower.Contains("van") || lower.Contains("pickup")
                || lower.Contains("ambulance") || lower.Contains("police") || lower.Contains("delivery")
                || lower.Contains("race") || lower.Contains("kart") || lower.Contains("tractor")
                || lower.Contains("truck"))
            {
                return true;
            }

            if (lower.StartsWith("traffic"))
            {
                return true;
            }

            return false;
        }

        public static bool IsTrafficCarPart(string objectName)
        {
            return IsBrokenVectorCarPart(objectName) || IsKenneyCarKitPart(objectName);
        }

        private static void ApplyKenneyLookToRenderer(Renderer renderer, int bodyColorIndex, bool isPickup)
        {
            Material baseMaterial = EnsureKenneyColormapMaterial();
            if (baseMaterial != null)
            {
                AssignMaterial(renderer, baseMaterial);
            }

            string lower = renderer.gameObject.name.ToLowerInvariant();
            bool isWheel = lower.Contains("wheel") || lower.Contains("tire") || lower.Contains("tyre");
            Color tint;
            if (isWheel)
            {
                tint = wheelMaterial != null
                    ? ExtractMaterialColor(wheelMaterial)
                    : new Color(0.18f, 0.14f, 0.12f, 1f);
            }
            else
            {
                tint = ResolveFallbackTint(bodyColorIndex, isPickup);
                // Keep Kenney UV atlas detail visible instead of washing the whole mesh one flat tone.
                tint = Color.Lerp(tint, Color.white, 0.42f);
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", tint);
            block.SetColor("_Color", tint);
            renderer.SetPropertyBlock(block);
        }

        private static void ApplyGenericBodyTint(Renderer renderer, int bodyColorIndex, bool isPickup)
        {
            if (renderer == null || ShouldSkipGenericTint(renderer.gameObject.name))
            {
                return;
            }

            Color tint = ResolveFallbackTint(bodyColorIndex, isPickup);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", tint);
            block.SetColor("_Color", tint);
            renderer.SetPropertyBlock(block);
        }

        private static bool ShouldSkipGenericTint(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return false;
            }

            string lower = objectName.ToLowerInvariant();
            return lower.Contains("wheel") || lower.Contains("tire") || lower.Contains("tyre")
                || lower.Contains("glass") || lower.Contains("window") || lower.Contains("rim")
                || lower.Contains("light") || lower.Contains("headlight") || lower.Contains("taillight");
        }

        private static Color ResolveFallbackTint(int bodyColorIndex, bool isPickup)
        {
            Color[] palette =
            {
                new Color(0.76f, 0.20f, 0.16f, 1f), // red
                new Color(0.53f, 0.33f, 0.20f, 1f), // brown
                new Color(0.19f, 0.49f, 0.30f, 1f), // green
                new Color(0.82f, 0.44f, 0.15f, 1f), // orange
                new Color(0.69f, 0.19f, 0.21f, 1f), // deep red
                new Color(0.86f, 0.66f, 0.24f, 1f), // gold
                new Color(0.90f, 0.76f, 0.22f, 1f), // yellow
                new Color(0.86f, 0.81f, 0.68f, 1f)  // cream
            };

            int safe = Mathf.Abs(bodyColorIndex) % palette.Length;
            Color tint = palette[safe];
            if (isPickup)
            {
                tint = Color.Lerp(tint, Color.white, 0.06f);
            }

            return tint;
        }

        private static Material ResolveLegacyMaterial(string objectName, int bodyColorIndex, bool isPickup)
        {
            string lower = objectName.ToLowerInvariant();
            if (lower.Contains("wheel") || lower.Contains("tire") || lower.Contains("tyre"))
            {
                return FirstAvailable(wheelMaterial, PickBodyColor(bodyColorIndex));
            }

            if (lower.Contains("glass") || lower.Contains("window"))
            {
                return FirstAvailable(glassMaterial, PickBodyColor(bodyColorIndex));
            }

            if (isPickup)
            {
                return FirstAvailable(pickupBody, PickBodyColor(bodyColorIndex));
            }

            return PickBodyColor(bodyColorIndex);
        }

        private static void AssignMaterial(Renderer renderer, Material material)
        {
            Material[] slots = renderer.sharedMaterials;
            if (slots == null || slots.Length == 0)
            {
                renderer.sharedMaterial = material;
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = material;
            }

            renderer.sharedMaterials = slots;
        }

        private static Material PickBodyColor(int index)
        {
            if (bodyColors == null || bodyColors.Length == 0)
            {
                return FirstAvailable(pickupBody, wheelMaterial, glassMaterial);
            }

            int safe = index % bodyColors.Length;
            if (safe < 0)
            {
                safe += bodyColors.Length;
            }

            return bodyColors[safe] != null
                ? bodyColors[safe]
                : bodyColors[0];
        }

        private static Color ExtractMaterialColor(Material material)
        {
            if (material == null)
            {
                return Color.white;
            }

            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }

            return Color.white;
        }

        private static Material EnsureKenneyColormapMaterial()
        {
            if (kenneyColormapBase != null)
            {
                return kenneyColormapBase;
            }

#if UNITY_EDITOR
            kenneyColormapBase = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                Level1KenneyCarKitPaths.ColormapMaterial);
#endif
            return kenneyColormapBase;
        }

        private static Material FirstAvailable(params Material[] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] != null)
                {
                    return candidates[i];
                }
            }

            return null;
        }

        private static Material[] FilterNulls(Material[] source)
        {
            if (source == null || source.Length == 0)
            {
                return System.Array.Empty<Material>();
            }

            int count = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != null)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return System.Array.Empty<Material>();
            }

            Material[] filtered = new Material[count];
            int write = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != null)
                {
                    filtered[write++] = source[i];
                }
            }

            return filtered;
        }
    }
}
