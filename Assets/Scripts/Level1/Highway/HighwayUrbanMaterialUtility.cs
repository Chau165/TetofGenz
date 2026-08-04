using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Applies RoadsideTownPalette materials with multi-tone semantic assignment.</summary>
    public static class HighwayUrbanMaterialUtility
    {
        public const int ColorPaletteVersion = 4;

        private const int NeighborMemory = 6;

        private static readonly Queue<int> LeftRecentWallBuckets = new Queue<int>();
        private static readonly Queue<int> RightRecentWallBuckets = new Queue<int>();

        private static Material[] wallPalette;
        private static Material[] roofPalette;
        private static Material[] windowPalette;
        private static Material[] doorPalette;
        private static Material[] trimPalette;
        private static Material[] signPalette;

        private enum BuildingPart
        {
            Wall,
            Roof,
            Window,
            Door,
            Trim
        }

        public static void BeginColorAssignmentPass()
        {
            LeftRecentWallBuckets.Clear();
            RightRecentWallBuckets.Clear();
            wallPalette = null;
            EnsurePalette();
        }

        public static void ApplyBuildingMaterials(
            GameObject root,
            int seed,
            int lotIndex = 0,
            bool leftSide = true,
            bool applyFacadeAccents = true)
        {
            if (root == null || QuaterniusCompleteBuildingValidator.IsQuaterniusCompleteBuildingName(root.name))
            {
                return;
            }

            EnsurePalette();
            int wallSeed = seed + lotIndex * 17 + (leftSide ? 11 : 23);
            Material primaryWall = PickVariedWall(wallSeed, leftSide);

            Material roofMat = PickFromPalette(roofPalette, wallSeed + 41);
            Material windowMat = PickFromPalette(windowPalette, wallSeed + 67);
            Material doorMat = PickFromPalette(doorPalette, wallSeed + 89);
            Material trimMat = PickFromPalette(trimPalette, wallSeed + 103);

            Bounds buildingBounds = ComputeAggregateBounds(root);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name == "FacadeAccents")
                {
                    continue;
                }

                BuildingPart part = ClassifyPart(renderer, buildingBounds);
                Material assigned = PickForPart(part, wallSeed + i * 7, primaryWall, roofMat, windowMat, doorMat, trimMat);
                renderer.sharedMaterial = assigned;
            }

            Transform meshHost = root.transform.childCount > 0 ? root.transform.GetChild(0) : root.transform;
            if (applyFacadeAccents
                && !QuaterniusCompleteBuildingValidator.IsWhitelistedCompleteName(root.name))
            {
                HighwayUrbanFacadeAccents.Apply(
                    meshHost.gameObject,
                    leftSide,
                    wallSeed,
                    primaryWall,
                    roofMat,
                    windowMat,
                    doorMat,
                    trimMat);
            }
        }

        public static void ValidateBuildingMaterials(
            GameObject root,
            int seed,
            int lotIndex,
            bool leftSide = true)
        {
            if (root == null || QuaterniusCompleteBuildingValidator.IsQuaterniusCompleteBuildingName(root.name))
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool needsFullPass = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Material current = renderers[i]?.sharedMaterial;
                if (!IsPaletteMaterial(current) || IsInvalidMaterial(current) || IsVisuallyAggressive(current))
                {
                    needsFullPass = true;
                    break;
                }
            }

            if (needsFullPass)
            {
                ApplyBuildingMaterials(root, seed, lotIndex, leftSide);
            }
        }

        public static void ValidateNeighborhoodColorSequence(List<NeighborhoodColorSlot> slots)
        {
            if (slots == null || slots.Count == 0)
            {
                return;
            }

            EnsurePalette();
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    NeighborhoodColorSlot slot = slots[i];
                    if (slot.Root == null)
                    {
                        continue;
                    }

                    int bucket = GetWallBucket(slot.DominantWall);
                    if (i > 0 && GetWallBucket(slots[i - 1].DominantWall) == bucket)
                    {
                        int newSeed = slot.Seed + pass * 97 + i * 11;
                        ApplyBuildingMaterials(slot.Root, newSeed, slot.LotIndex, slot.LeftSide);
                        slots[i] = new NeighborhoodColorSlot
                        {
                            Root = slot.Root,
                            Seed = newSeed,
                            LotIndex = slot.LotIndex,
                            LeftSide = slot.LeftSide,
                            DominantWall = GetDominantWallMaterial(slot.Root)
                        };
                    }
                }
            }
        }

        public static Material GetDominantWallMaterial(GameObject root)
        {
            if (root == null)
            {
                return null;
            }

            Bounds buildingBounds = ComputeAggregateBounds(root);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Material best = null;
            float bestArea = 0f;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || ClassifyPart(renderer, buildingBounds) != BuildingPart.Wall)
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;
                float area = bounds.size.x * bounds.size.y + bounds.size.y * bounds.size.z + bounds.size.x * bounds.size.z;
                if (area > bestArea)
                {
                    bestArea = area;
                    best = renderer.sharedMaterial;
                }
            }

            return best;
        }

        public static void ApplySignMaterials(GameObject root, int seed)
        {
            if (root == null)
            {
                return;
            }

            EnsurePalette();
            Material accent = PickFromPalette(trimPalette, seed + 17);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                string hint = renderer.gameObject.name.ToLowerInvariant();
                bool signFace = hint.Contains("sign") || hint.Contains("board") || hint.Contains("panel")
                    || hint.Contains("face") || hint.Contains("bill") || hint.Contains("banner")
                    || hint.Contains("cloth");
                if (!signFace && !IsInvalidMaterial(renderer.sharedMaterial))
                {
                    continue;
                }

                renderer.sharedMaterial = accent;
            }
        }

        public struct NeighborhoodColorSlot
        {
            public GameObject Root;
            public int Seed;
            public int LotIndex;
            public bool LeftSide;
            public Material DominantWall;
        }

        private static BuildingPart ClassifyPart(Renderer renderer, Bounds buildingBounds)
        {
            string hint = renderer.gameObject.name.ToLowerInvariant();
            if (IsRoofHint(hint))
            {
                return BuildingPart.Roof;
            }

            if (IsWindowHint(hint))
            {
                return BuildingPart.Window;
            }

            if (IsDoorHint(hint))
            {
                return BuildingPart.Door;
            }

            if (IsTrimHint(hint))
            {
                return BuildingPart.Trim;
            }

            return InferPartFromGeometry(renderer.bounds, buildingBounds);
        }

        private static BuildingPart InferPartFromGeometry(Bounds part, Bounds building)
        {
            if (building.size.sqrMagnitude < 0.001f)
            {
                return BuildingPart.Wall;
            }

            Vector3 size = part.size;
            float height = size.y;
            float buildingHeight = Mathf.Max(building.size.y, 0.01f);
            float relTop = (part.max.y - building.min.y) / buildingHeight;
            float relBottom = (part.min.y - building.min.y) / buildingHeight;
            float volume = size.x * size.y * size.z;
            float buildingVolume = building.size.x * building.size.y * building.size.z;
            float volumeRatio = volume / Mathf.Max(buildingVolume, 0.001f);
            float footprint = Mathf.Max(size.x, size.z);
            float minFootprint = Mathf.Min(size.x, size.z);

            if (relTop > 0.76f && height <= buildingHeight * 0.34f && footprint > minFootprint * 1.15f)
            {
                return BuildingPart.Roof;
            }

            if (volumeRatio < 0.09f)
            {
                if (relBottom < 0.42f && height > footprint * 0.85f && minFootprint < building.size.x * 0.35f)
                {
                    return BuildingPart.Door;
                }

                if (height < buildingHeight * 0.28f || minFootprint < building.size.x * 0.22f)
                {
                    return BuildingPart.Window;
                }

                return BuildingPart.Trim;
            }

            if (relTop > 0.88f && volumeRatio < 0.2f)
            {
                return BuildingPart.Trim;
            }

            return BuildingPart.Wall;
        }

        private static Material PickForPart(
            BuildingPart part,
            int seed,
            Material primaryWall,
            Material roofMat,
            Material windowMat,
            Material doorMat,
            Material trimMat)
        {
            return part switch
            {
                BuildingPart.Roof => roofMat,
                BuildingPart.Window => windowMat,
                BuildingPart.Door => doorMat,
                BuildingPart.Trim => trimMat,
                _ => primaryWall ?? PickFromPalette(wallPalette, seed)
            };
        }

        private static bool IsRoofHint(string hint) =>
            hint.Contains("roof") || hint.Contains("top") || hint.Contains("eave") || hint.Contains("dach");

        private static bool IsWindowHint(string hint) =>
            hint.Contains("window") || hint.Contains("glass") || hint.Contains("pane") || hint.Contains("fenster");

        private static bool IsDoorHint(string hint) =>
            hint.Contains("door") || hint.Contains("gate") || hint.Contains("entry") || hint.Contains("portal");

        private static bool IsTrimHint(string hint) =>
            hint.Contains("frame") || hint.Contains("trim") || hint.Contains("awning")
            || hint.Contains("ledge") || hint.Contains("balcon") || hint.Contains("column")
            || hint.Contains("pillar") || hint.Contains("sill") || hint.Contains("cornice")
            || hint.Contains("mould") || hint.Contains("mold");

        private static Bounds ComputeAggregateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one * 2f);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static Material PickVariedWall(int seed, bool leftSide)
        {
            Queue<int> recent = leftSide ? LeftRecentWallBuckets : RightRecentWallBuckets;
            int count = wallPalette.Length;
            int start = Mathf.Abs(seed) % count;

            for (int attempt = 0; attempt < count; attempt++)
            {
                int index = (start + attempt) % count;
                if (!recent.Contains(index))
                {
                    RecordWallIndex(leftSide, index);
                    return wallPalette[index];
                }
            }

            int fallbackIndex = (start + 1) % count;
            RecordWallIndex(leftSide, fallbackIndex);
            return wallPalette[fallbackIndex];
        }

        private static void RecordWallIndex(bool leftSide, int index)
        {
            Queue<int> recent = leftSide ? LeftRecentWallBuckets : RightRecentWallBuckets;
            recent.Enqueue(index);
            while (recent.Count > NeighborMemory)
            {
                recent.Dequeue();
            }
        }

        private static int GetWallBucket(Material material)
        {
            if (material == null || wallPalette == null)
            {
                return 0;
            }

            for (int i = 0; i < wallPalette.Length; i++)
            {
                if (wallPalette[i] == material)
                {
                    return i;
                }
            }

            return material.name.GetHashCode();
        }

        private static Material PickFromPalette(Material[] palette, int seed)
        {
            if (palette == null || palette.Length == 0)
            {
                return null;
            }

            return palette[Mathf.Abs(seed) % palette.Length];
        }

        private static bool IsPaletteMaterial(Material material)
        {
            if (material == null)
            {
                return false;
            }

            string name = material.name;
            return name.StartsWith("Wall_") || name.StartsWith("Roof_") || name.StartsWith("Window_")
                || name.StartsWith("Door_") || name.StartsWith("Trim_");
        }

        private static bool IsVisuallyAggressive(Material material)
        {
            if (material == null || !TryGetColor(material, out Color color))
            {
                return false;
            }

            Color.RGBToHSV(color, out float h, out float s, out float v);
            if (s > 0.52f)
            {
                return true;
            }

            bool redDominant = color.r > color.g + 0.18f && color.r > color.b + 0.18f && color.r > 0.55f;
            bool orangeDominant = color.r > 0.72f && color.g > 0.38f && color.g < 0.72f && color.b < 0.35f;
            bool neon = s > 0.45f && v > 0.78f;
            return redDominant || orangeDominant || neon;
        }

        private static bool IsInvalidMaterial(Material material)
        {
            if (material == null)
            {
                return true;
            }

            string shaderName = material.shader != null ? material.shader.name.ToLowerInvariant() : string.Empty;
            if (shaderName.Contains("error") || shaderName.Contains("hidden/internal"))
            {
                return true;
            }

            string name = material.name.ToLowerInvariant();
            if (name.Contains("default") || name.Contains("missing") || name.Contains("pink"))
            {
                return true;
            }

            if (!TryGetColor(material, out Color color))
            {
                return false;
            }

            if (color.r > 0.95f && color.g > 0.95f && color.b > 0.95f)
            {
                return true;
            }

            if (color.g < 0.25f && color.b > 0.65f && color.r > 0.75f)
            {
                return true;
            }

            float spread = Mathf.Max(color.r, Mathf.Max(color.g, color.b))
                - Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            return spread < 0.06f && color.r > 0.28f && color.r < 0.62f;
        }

        private static bool TryGetColor(Material material, out Color color)
        {
            color = default;
            if (material == null)
            {
                return false;
            }

            if (material.HasProperty("_BaseColor"))
            {
                color = material.GetColor("_BaseColor");
                return true;
            }

            if (material.HasProperty("_Color"))
            {
                color = material.GetColor("_Color");
                return true;
            }

            return false;
        }

        private static void EnsurePalette()
        {
            if (wallPalette != null && wallPalette.Length > 0)
            {
                return;
            }

#if UNITY_EDITOR
            wallPalette = LoadMany(RoadsideTownPalette.WallMaterials);
            roofPalette = LoadMany(RoadsideTownPalette.RoofMaterials);
            windowPalette = LoadMany(RoadsideTownPalette.WindowMaterials);
            doorPalette = LoadMany(RoadsideTownPalette.DoorMaterials);
            trimPalette = LoadMany(RoadsideTownPalette.TrimMaterials);
            signPalette = trimPalette;
            if (wallPalette.Length == 0)
            {
                wallPalette = BuildEmbeddedPalette(RoadsideTownPalette.WallColors);
                roofPalette = BuildEmbeddedPalette(RoadsideTownPalette.RoofColors);
                windowPalette = BuildEmbeddedPalette(RoadsideTownPalette.WindowColors);
                doorPalette = BuildEmbeddedPalette(RoadsideTownPalette.DoorColors);
                trimPalette = BuildEmbeddedPalette(RoadsideTownPalette.TrimColors);
                signPalette = trimPalette;
            }
#else
            wallPalette = BuildRuntimeFallbacks(RoadsideTownPalette.WallMaterials);
            roofPalette = BuildRuntimeFallbacks(RoadsideTownPalette.RoofMaterials);
            windowPalette = BuildRuntimeFallbacks(RoadsideTownPalette.WindowMaterials);
            doorPalette = BuildRuntimeFallbacks(RoadsideTownPalette.DoorMaterials);
            trimPalette = BuildRuntimeFallbacks(RoadsideTownPalette.TrimMaterials);
            signPalette = trimPalette;
#endif
        }

#if UNITY_EDITOR
        private static Material[] LoadMany(string[] paths)
        {
            var list = new List<Material>();
            for (int i = 0; i < paths.Length; i++)
            {
                Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(paths[i]);
                if (mat != null)
                {
                    list.Add(mat);
                }
            }

            return list.ToArray();
        }
#else
        private static Material[] BuildRuntimeFallbacks(string[] names)
        {
            var list = new List<Material>();
            for (int i = 0; i < names.Length; i++)
            {
                list.Add(CreateFallback(Color.HSVToRGB((i * 0.11f) % 1f, 0.22f, 0.78f)));
            }

            return list.ToArray();
        }
#endif

        private static Material[] BuildEmbeddedPalette(Color[] colors)
        {
            if (colors == null || colors.Length == 0)
            {
                return System.Array.Empty<Material>();
            }

            var list = new List<Material>(colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                list.Add(CreateFallback(colors[i]));
            }

            return list.ToArray();
        }

        private static Material CreateFallback(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.14f);
            }

            return material;
        }
    }
}
