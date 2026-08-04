#if UNITY_EDITOR
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Normalized desk-top footprint in desk local space (u/v in -1..1).</summary>
    public readonly struct DeskTopLayout
    {
        public readonly Vector2 CenterLocalXz;
        public readonly Vector2 HalfSizeLocalXz;

        public DeskTopLayout(Vector2 centerLocalXz, Vector2 halfSizeLocalXz)
        {
            CenterLocalXz = centerLocalXz;
            HalfSizeLocalXz = halfSizeLocalXz;
        }

        public Vector3 LocalPoint(float u, float v)
        {
            return new Vector3(CenterLocalXz.x + u * HalfSizeLocalXz.x, 0f, CenterLocalXz.y + v * HalfSizeLocalXz.y);
        }
    }

    /// <summary>Bounds-based placement helpers for Level 1 room props.</summary>
    internal static class Level1ScenePlacement
    {
        public const float PropClearance = 0.2f;
        public const float FurnitureClearance = 0.4f;
        public const float WalkPathClearance = 0.6f;
        /// <summary>Tiny lift so props rest on surfaces without z-fighting.</summary>
        public const float DeskSurfaceLift = 0.001f;
        public const float SurfaceContactLift = 0.002f;
        /// <summary>Extra downward bias for office meshes whose bounds sit above the visible base.</summary>
        public const float DeskPropVisualSink = 0.012f;

        public static bool TryGetWorldBounds(GameObject root, out Bounds bounds)
        {
            bounds = default;
            if (root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return true;
        }

        public static void SnapPropBottomToWorldY(GameObject prop, float surfaceTopY)
        {
            SnapPropBottomToWorldY(prop, surfaceTopY, false);
        }

        public static void SnapPropBottomToWorldY(GameObject prop, float surfaceTopY, bool preferVisualBounds)
        {
            if (prop == null || !TryGetPropContactBounds(prop, preferVisualBounds, out Bounds propBounds))
            {
                return;
            }

            float deltaY = surfaceTopY - propBounds.min.y;
            if (Mathf.Abs(deltaY) < 0.0001f)
            {
                return;
            }

            Vector3 worldPos = prop.transform.position;
            worldPos.y += deltaY;
            prop.transform.position = worldPos;
        }

        public static bool TryGetPropContactBounds(GameObject prop, bool preferVisualBounds, out Bounds bounds)
        {
            bounds = default;
            if (prop == null)
            {
                return false;
            }

            if (preferVisualBounds)
            {
                Transform visual = prop.transform.Find("Visual");
                if (visual != null && TryGetWorldBounds(visual.gameObject, out bounds))
                {
                    return true;
                }
            }

            return TryGetWorldBounds(prop, out bounds);
        }

        public static void SnapPropBottomToSurface(GameObject surface, GameObject prop)
        {
            if (surface == null || prop == null || !TryGetWorldBounds(surface, out Bounds surfaceBounds))
            {
                return;
            }

            SnapPropBottomToWorldY(prop, surfaceBounds.max.y);
        }

        public static void SnapPropBottomToFloor(GameObject prop, float floorY, float surfaceLift = 0f)
        {
            SnapPropBottomToWorldY(prop, floorY + surfaceLift);
        }

        public static Transform FindInRoom(Transform worldRoot, string relativePath)
        {
            if (worldRoot == null || string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            Transform found = worldRoot.Find(relativePath);
            if (found != null)
            {
                return found;
            }

            Transform indoor = worldRoot.Find("Level1_IndoorArea");
            return indoor != null ? indoor.Find(relativePath) : null;
        }

        public static GameObject FindInRoomObject(Transform worldRoot, string relativePath)
        {
            return FindInRoom(worldRoot, relativePath)?.gameObject;
        }

        public static void SnapHierarchyFloorProps(Transform root, float floorY, float rugLift = 0.005f)
        {
            if (root == null)
            {
                return;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject child = root.GetChild(i).gameObject;
                if (child.GetComponentInChildren<Renderer>(true) == null)
                {
                    continue;
                }

                float lift = child.name.IndexOf("rug", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || child.name.IndexOf("mat", System.StringComparison.OrdinalIgnoreCase) >= 0
                    ? rugLift
                    : 0f;
                SnapPropBottomToFloor(child, floorY, lift);
            }
        }

        public static void ResolveFurnitureOverlaps(Transform worldRoot)
        {
            if (worldRoot == null)
            {
                return;
            }

            GameObject bed = FindInRoomObject(worldRoot, "SleepingArea/Bed");
            GameObject chair = FindInRoomObject(worldRoot, "StudyArea/DeskChair");
            GameObject desk = FindInRoomObject(worldRoot, "StudyArea/Desk");
            GameObject plant = FindInRoomObject(worldRoot, "RoomPersonality/CornerPlant");
            GameObject wardrobe = FindInRoomObject(worldRoot, "StorageArea/Wardrobe");
            GameObject shelf = FindInRoomObject(worldRoot, "StorageArea/Shelf");
            GameObject suitcase = FindInRoomObject(worldRoot, "ExitArea/Suitcase");

            if (chair != null && desk != null)
            {
                EnsureXZClearance(chair, desk, FurnitureClearance);
            }

            if (plant != null && chair != null)
            {
                EnsureXZClearance(plant, chair, PropClearance);
            }

            if (bed != null && plant != null)
            {
                EnsureXZClearance(bed, plant, PropClearance);
            }

            if (wardrobe != null && shelf != null)
            {
                EnsureXZClearance(wardrobe, shelf, PropClearance);
            }

            if (suitcase != null && bed != null)
            {
                EnsureXZClearance(suitcase, bed, WalkPathClearance);
            }
        }

        public static void ClampRoomFurnitureInside(Transform worldRoot, float halfWidth, float halfDepth, float margin)
        {
            if (worldRoot == null)
            {
                return;
            }

            string[] paths =
            {
                "SleepingArea/Bed",
                "SleepingArea/BedsideLamp",
                "StudyArea/Desk",
                "StudyArea/DeskChair",
                "StudyArea/StudyChairBackpack",
                "StorageArea/Wardrobe",
                "StorageArea/Shelf",
                "ExitArea/Suitcase",
                "RoomPersonality/CornerPlant"
            };

            for (int i = 0; i < paths.Length; i++)
            {
                GameObject piece = FindInRoomObject(worldRoot, paths[i]);
                if (piece != null)
                {
                    ClampInsideRoom(piece, halfWidth, halfDepth, margin);
                    SnapPropBottomToFloor(piece, 0f);
                }
            }
        }

        public static float GetSurfaceWorldTop(GameObject surface)
        {
            return TryGetWorldBounds(surface, out Bounds bounds) ? bounds.max.y : 0f;
        }

        /// <summary>Furniture-top Y — ignores decor subtrees; uses upper mesh band for the actual contact surface.</summary>
        public static bool TryGetFurnitureTopWorldY(GameObject furniture, string decorPrefix, out float topY)
        {
            topY = 0f;
            if (furniture == null)
            {
                return false;
            }

            Renderer[] renderers = furniture.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            float furnitureMinY = float.MaxValue;
            float furnitureMaxY = float.MinValue;
            int candidateCount = 0;
            Bounds[] candidates = new Bounds[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                Transform rendererTransform = renderers[i].transform;
                if (IsRendererFromDecorChild(furniture.transform, rendererTransform, decorPrefix))
                {
                    continue;
                }

                Bounds bounds = renderers[i].bounds;
                candidates[candidateCount++] = bounds;
                furnitureMinY = Mathf.Min(furnitureMinY, bounds.min.y);
                furnitureMaxY = Mathf.Max(furnitureMaxY, bounds.max.y);
            }

            if (candidateCount == 0)
            {
                return false;
            }

            float furnitureHeight = Mathf.Max(0.01f, furnitureMaxY - furnitureMinY);
            float surfaceBandFloor = furnitureMaxY - Mathf.Max(0.06f, furnitureHeight * 0.22f);
            float bestTop = float.MinValue;

            for (int i = 0; i < candidateCount; i++)
            {
                Bounds bounds = candidates[i];
                if (bounds.max.y < surfaceBandFloor - 0.001f)
                {
                    continue;
                }

                bestTop = Mathf.Max(bestTop, bounds.max.y);
            }

            topY = bestTop > float.MinValue + 1f ? bestTop : furnitureMaxY;
            return true;
        }

        public static bool TryGetDeskTopWorldY(GameObject desk, out float topY)
        {
            return TryGetFurnitureTopWorldY(desk, "Desk", out topY);
        }

        public static bool TryGetDeskSurfaceWorldY(GameObject desk, out float surfaceY)
        {
            if (!TryGetDeskTopWorldY(desk, out surfaceY))
            {
                return false;
            }

            surfaceY += DeskSurfaceLift - DeskPropVisualSink;
            return true;
        }

        public static void SnapPropBottomToFurnitureTop(GameObject furniture, string decorPrefix, GameObject prop,
            float surfaceLift = SurfaceContactLift)
        {
            if (furniture == null || prop == null)
            {
                return;
            }

            if (prop.transform.parent == furniture.transform
                && SnapPropBottomToParentLocalSurface(furniture, decorPrefix, prop, surfaceLift - DeskPropVisualSink))
            {
                return;
            }

            if (TryGetFurnitureTopWorldY(furniture, decorPrefix, out float topY))
            {
                SnapPropBottomToWorldY(prop, topY + surfaceLift - DeskPropVisualSink);
            }
            else
            {
                SnapPropBottomToSurface(furniture, prop);
            }
        }

        public static void SnapPropBottomToDeskTop(GameObject desk, GameObject prop)
        {
            if (desk == null || prop == null)
            {
                return;
            }

            if (prop.transform.parent == desk.transform
                && SnapPropBottomToParentLocalSurface(desk, "Desk", prop, DeskSurfaceLift))
            {
                return;
            }

            bool preferVisualBounds = true;
            if (TryGetDeskSurfaceWorldY(desk, out float surfaceY))
            {
                SnapPropBottomToWorldY(prop, surfaceY, preferVisualBounds);
            }
            else
            {
                SnapPropBottomToSurface(desk, prop);
            }
        }

        private static bool SnapPropBottomToParentLocalSurface(GameObject furniture, string decorPrefix, GameObject prop,
            float surfaceLiftWorld)
        {
            if (furniture == null || prop == null || prop.transform.parent != furniture.transform)
            {
                return false;
            }

            if (!TryGetFurnitureTopLocalY(furniture, decorPrefix, out float topLocalY))
            {
                return false;
            }

            if (!TryGetPropBoundsInParentLocalSpace(furniture.transform, prop, out Bounds localBounds))
            {
                return false;
            }

            float scaleY = Mathf.Max(0.0001f, Mathf.Abs(furniture.transform.lossyScale.y));
            float liftLocal = surfaceLiftWorld / scaleY;
            float sinkLocal = DeskPropVisualSink / scaleY;
            float deltaLocalY = topLocalY + liftLocal - sinkLocal - localBounds.min.y;
            if (Mathf.Abs(deltaLocalY) < 0.0001f)
            {
                return true;
            }

            Vector3 localPos = prop.transform.localPosition;
            prop.transform.localPosition = new Vector3(localPos.x, localPos.y + deltaLocalY, localPos.z);
            return true;
        }

        public static bool TryGetFurnitureTopLocalY(GameObject furniture, string decorPrefix, out float localTopY)
        {
            localTopY = 0f;
            if (furniture == null)
            {
                return false;
            }

            if (furniture.TryGetComponent<BoxCollider>(out BoxCollider boxCollider) && !boxCollider.isTrigger)
            {
                localTopY = boxCollider.center.y + boxCollider.size.y * 0.5f;
                return true;
            }

            Transform furnitureTransform = furniture.transform;
            Renderer[] renderers = furniture.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            float minLocalY = float.MaxValue;
            float maxLocalY = float.MinValue;
            int candidateCount = 0;
            float[] candidateMaxYs = new float[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                if (IsRendererFromDecorChild(furnitureTransform, renderers[i].transform, decorPrefix))
                {
                    continue;
                }

                Bounds localBounds = TransformBoundsToLocal(furnitureTransform, renderers[i].bounds);
                candidateMaxYs[candidateCount++] = localBounds.max.y;
                minLocalY = Mathf.Min(minLocalY, localBounds.min.y);
                maxLocalY = Mathf.Max(maxLocalY, localBounds.max.y);
            }

            if (candidateCount == 0)
            {
                return false;
            }

            float furnitureHeight = Mathf.Max(0.01f, maxLocalY - minLocalY);
            float surfaceBandFloor = maxLocalY - Mathf.Max(0.06f, furnitureHeight * 0.22f);
            float bestTop = float.MinValue;

            for (int i = 0; i < candidateCount; i++)
            {
                if (candidateMaxYs[i] < surfaceBandFloor - 0.001f)
                {
                    continue;
                }

                bestTop = Mathf.Max(bestTop, candidateMaxYs[i]);
            }

            localTopY = bestTop > float.MinValue + 1f ? bestTop : maxLocalY;
            return true;
        }

        public static bool TryGetPropBoundsInParentLocalSpace(Transform parent, GameObject prop, out Bounds localBounds)
        {
            localBounds = default;
            if (parent == null || prop == null)
            {
                return false;
            }

            Renderer[] renderers = prop.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            bool found = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Bounds rendererLocal = TransformBoundsToLocal(parent, renderers[i].bounds);
                if (!found)
                {
                    localBounds = rendererLocal;
                    found = true;
                }
                else
                {
                    localBounds.Encapsulate(rendererLocal.min);
                    localBounds.Encapsulate(rendererLocal.max);
                }
            }

            return found;
        }

        private static Bounds TransformBoundsToLocal(Transform parent, Bounds worldBounds)
        {
            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int xi = -1; xi <= 1; xi += 2)
            {
                for (int yi = -1; yi <= 1; yi += 2)
                {
                    for (int zi = -1; zi <= 1; zi += 2)
                    {
                        Vector3 corner = center + Vector3.Scale(extents, new Vector3(xi, yi, zi));
                        Vector3 local = parent.InverseTransformPoint(corner);
                        min = Vector3.Min(min, local);
                        max = Vector3.Max(max, local);
                    }
                }
            }

            Vector3 localCenter = (min + max) * 0.5f;
            Vector3 localSize = max - min;
            return new Bounds(localCenter, localSize);
        }

        /// <summary>Snaps every desk child prop and optional packables that live on the desk surface.</summary>
        public static void SnapAllDeskSurfaceProps(GameObject desk, Transform packablesRoot = null)
        {
            if (desk == null)
            {
                return;
            }

            SnapDeskSurfaceVertical(desk, packablesRoot);
            ResolveDeskTopOverlaps(desk, packablesRoot);
            SnapDeskSurfaceVertical(desk, packablesRoot);
        }

        public const float DeskPropOverlapClearance = 0.04f;

        private static void SnapDeskSurfaceVertical(GameObject desk, Transform packablesRoot)
        {
            for (int i = 0; i < desk.transform.childCount; i++)
            {
                GameObject child = desk.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Desk"))
                {
                    SnapPropBottomToDeskTop(desk, child);
                }
            }

            if (packablesRoot == null)
            {
                return;
            }

            string[] deskPackables = { "Wallet", "WaterBottle", "Documents" };
            for (int i = 0; i < deskPackables.Length; i++)
            {
                Transform item = packablesRoot.Find(deskPackables[i]);
                if (item != null)
                {
                    SnapPropBottomToDeskTop(desk, item.gameObject);
                }
            }
        }

        public static void ResolveDeskTopOverlaps(GameObject desk, Transform packablesRoot)
        {
            if (desk == null)
            {
                return;
            }

            System.Collections.Generic.List<GameObject> props = CollectDeskSurfaceProps(desk, packablesRoot);
            if (props.Count < 2)
            {
                return;
            }

            for (int pass = 0; pass < 18; pass++)
            {
                bool moved = false;
                for (int i = 0; i < props.Count; i++)
                {
                    for (int j = i + 1; j < props.Count; j++)
                    {
                        GameObject a = props[i];
                        GameObject b = props[j];
                        if (!OverlapsXZ(a, b, DeskPropOverlapClearance))
                        {
                            continue;
                        }

                        GameObject movable = ChooseDeskOverlapMovable(a, b);
                        GameObject anchor = movable == a ? b : a;
                        EnsureXZClearance(movable, anchor, DeskPropOverlapClearance);
                        moved = true;
                    }
                }

                if (!moved)
                {
                    break;
                }
            }

            ClampDeskSurfacePropsInsideTop(desk, props);
        }

        private static System.Collections.Generic.List<GameObject> CollectDeskSurfaceProps(GameObject desk,
            Transform packablesRoot)
        {
            var props = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < desk.transform.childCount; i++)
            {
                GameObject child = desk.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Desk"))
                {
                    props.Add(child);
                }
            }

            if (packablesRoot != null)
            {
                string[] deskPackables = { "Wallet", "WaterBottle", "Documents" };
                for (int i = 0; i < deskPackables.Length; i++)
                {
                    Transform item = packablesRoot.Find(deskPackables[i]);
                    if (item != null)
                    {
                        props.Add(item.gameObject);
                    }
                }
            }

            return props;
        }

        private static GameObject ChooseDeskOverlapMovable(GameObject a, GameObject b)
        {
            if (IsDeskOverlapAnchor(a.name))
            {
                return b;
            }

            if (IsDeskOverlapAnchor(b.name))
            {
                return a;
            }

            bool aPackable = IsDeskSurfacePackable(a.name);
            bool bPackable = IsDeskSurfacePackable(b.name);
            if (aPackable && !bPackable)
            {
                return a;
            }

            if (bPackable && !aPackable)
            {
                return b;
            }

            if (TryGetWorldBounds(a, out Bounds boundsA) && TryGetWorldBounds(b, out Bounds boundsB))
            {
                return boundsA.size.sqrMagnitude <= boundsB.size.sqrMagnitude ? a : b;
            }

            return a;
        }

        private static bool IsDeskOverlapAnchor(string objectName)
        {
            return objectName == "DeskLaptop" || objectName == "DeskLamp";
        }

        private static bool IsDeskSurfacePackable(string objectName)
        {
            return objectName == "Documents" || objectName == "Wallet" || objectName == "WaterBottle";
        }

        private static void ClampDeskSurfacePropsInsideTop(GameObject desk,
            System.Collections.Generic.List<GameObject> props)
        {
            if (!TryGetDeskTopLayout(desk, out DeskTopLayout layout))
            {
                return;
            }

            float maxU = layout.HalfSizeLocalXz.x * 0.92f;
            float maxV = layout.HalfSizeLocalXz.y * 0.92f;
            Transform deskTransform = desk.transform;

            for (int i = 0; i < props.Count; i++)
            {
                GameObject prop = props[i];
                if (prop == null)
                {
                    continue;
                }

                Vector3 local = deskTransform.InverseTransformPoint(prop.transform.position);
                float clampedX = Mathf.Clamp(local.x, layout.CenterLocalXz.x - maxU, layout.CenterLocalXz.x + maxU);
                float clampedZ = Mathf.Clamp(local.z, layout.CenterLocalXz.y - maxV, layout.CenterLocalXz.y + maxV);
                if (Mathf.Abs(clampedX - local.x) < 0.0001f && Mathf.Abs(clampedZ - local.z) < 0.0001f)
                {
                    continue;
                }

                if (prop.transform.parent == deskTransform)
                {
                    prop.transform.localPosition = new Vector3(clampedX, prop.transform.localPosition.y, clampedZ);
                }
                else
                {
                    Vector3 world = deskTransform.TransformPoint(new Vector3(clampedX, local.y, clampedZ));
                    prop.transform.position = new Vector3(world.x, prop.transform.position.y, world.z);
                }
            }
        }

        public static bool TryGetFurnitureMeshWorldBounds(GameObject furniture, string decorPrefix, out Bounds bounds)
        {
            bounds = default;
            if (furniture == null)
            {
                return false;
            }

            Renderer[] renderers = furniture.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            bool found = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Transform rendererTransform = renderers[i].transform;
                if (IsRendererFromDecorChild(furniture.transform, rendererTransform, decorPrefix))
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderers[i].bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return found;
        }

        private static bool IsRendererFromDecorChild(Transform furniture, Transform rendererTransform, string decorPrefix)
        {
            if (furniture == null || rendererTransform == null || string.IsNullOrEmpty(decorPrefix))
            {
                return false;
            }

            Transform current = rendererTransform;
            while (current != null && current != furniture)
            {
                if (current.parent == furniture && current.name.StartsWith(decorPrefix))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        public static bool TryGetDeskMeshWorldBounds(GameObject desk, out Bounds bounds)
        {
            return TryGetFurnitureMeshWorldBounds(desk, "Desk", out bounds);
        }

        /// <summary>Usable top surface in furniture local XZ (inset ~8% from mesh edges).</summary>
        public static bool TryGetFurnitureTopLayout(GameObject furniture, string decorPrefix, out DeskTopLayout layout)
        {
            layout = default;
            if (!TryGetFurnitureMeshWorldBounds(furniture, decorPrefix, out Bounds world))
            {
                return false;
            }

            Vector3[] corners =
            {
                new(world.min.x, world.max.y, world.min.z),
                new(world.max.x, world.max.y, world.min.z),
                new(world.min.x, world.max.y, world.max.z),
                new(world.max.x, world.max.y, world.max.z)
            };

            Vector3 localMin = furniture.transform.InverseTransformPoint(corners[0]);
            Vector3 localMax = localMin;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 local = furniture.transform.InverseTransformPoint(corners[i]);
                localMin = Vector3.Min(localMin, local);
                localMax = Vector3.Max(localMax, local);
            }

            const float inset = 0.9f;
            layout = new DeskTopLayout(
                new Vector2((localMin.x + localMax.x) * 0.5f, (localMin.z + localMax.z) * 0.5f),
                new Vector2((localMax.x - localMin.x) * 0.5f * inset, (localMax.z - localMin.z) * 0.5f * inset));
            return layout.HalfSizeLocalXz.x > 0.01f && layout.HalfSizeLocalXz.y > 0.01f;
        }

        public static bool TryGetDeskTopLayout(GameObject desk, out DeskTopLayout layout)
        {
            return TryGetFurnitureTopLayout(desk, "Desk", out layout);
        }

        public static bool TryGetBedTopLayout(GameObject bed, out DeskTopLayout layout)
        {
            return TryGetFurnitureTopLayout(bed, "Bed", out layout);
        }

        /// <summary>Kenney bed cover/mattress mesh bounds in world space.</summary>
        public static bool TryGetKenneyBedCoverBounds(GameObject bed, out Bounds bounds)
        {
            return TryGetFurnitureMeshWorldBounds(bed, "Bed", out bounds);
        }

        public static bool TryGetBedMattressTopWorldY(GameObject bed, string decorPrefix, out float topY)
        {
            return TryGetFurnitureTopWorldY(bed, decorPrefix, out topY);
        }

        /// <summary>World point on the mattress from a bed-local XZ offset.</summary>
        public static bool TryGetBedPackableWorldPoint(GameObject bed, Vector3 bedLocalOffset, out Vector3 worldPoint)
        {
            worldPoint = default;
            if (bed == null)
            {
                return false;
            }

            if (TryGetBedMattressLocalPoint(bed, "Bed", out Vector3 localAnchor))
            {
                localAnchor.x += bedLocalOffset.x;
                localAnchor.z += bedLocalOffset.z;
                worldPoint = bed.transform.TransformPoint(localAnchor);
                return true;
            }

            worldPoint = bed.transform.TransformPoint(bedLocalOffset);
            return true;
        }

        public static void SnapPropBottomToBedTop(GameObject bed, GameObject prop)
        {
            SnapPropBottomToFurnitureTop(bed, "Bed", prop);
        }

        /// <summary>Mattress-top center in bed local space (Kenney bed mesh is offset from transform origin).</summary>
        public static bool TryGetBedMattressLocalPoint(GameObject bed, string decorPrefix, out Vector3 localPoint)
        {
            localPoint = Vector3.zero;
            if (bed == null)
            {
                return false;
            }

            Renderer rootRenderer = bed.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                Bounds localBounds = rootRenderer.localBounds;
                float x = localBounds.center.x;
                float z = localBounds.center.z + localBounds.extents.z * -0.12f;
                x = Mathf.Clamp(x, localBounds.min.x + 0.35f, localBounds.max.x - 0.35f);
                z = Mathf.Clamp(z, localBounds.min.z + 0.55f, localBounds.max.z - 0.55f);
                localPoint = new Vector3(x, 0f, z);
                return localBounds.size.x > 0.05f && localBounds.size.z > 0.05f;
            }

            if (!TryGetBedTopLayout(bed, out DeskTopLayout layout))
            {
                return false;
            }

            localPoint = layout.LocalPoint(0f, 0f);
            return true;
        }

        public static float GetFurnitureMeshWorldWidth(GameObject furniture, string decorPrefix)
        {
            return TryGetFurnitureMeshWorldBounds(furniture, decorPrefix, out Bounds bounds)
                ? Mathf.Max(bounds.size.x, bounds.size.z)
                : 0.8f;
        }

        public static float GetDeskMeshWorldWidth(GameObject desk)
        {
            return GetFurnitureMeshWorldWidth(desk, "Desk");
        }

        public static void FitPropFootprintOnFurniture(GameObject furniture, string decorPrefix, GameObject prop,
            float widthFraction, float depthFraction = -1f, bool snapAfterFit = true)
        {
            if (furniture == null || prop == null || widthFraction <= 0f)
            {
                return;
            }

            if (depthFraction <= 0f)
            {
                depthFraction = widthFraction * 0.72f;
            }

            float furnitureWidth = GetFurnitureMeshWorldWidth(furniture, decorPrefix);
            float furnitureDepth = TryGetFurnitureMeshWorldBounds(furniture, decorPrefix, out Bounds furnitureBounds)
                ? Mathf.Min(furnitureBounds.size.x, furnitureBounds.size.z)
                : furnitureWidth * 0.65f;

            for (int attempt = 0; attempt < 4; attempt++)
            {
                if (!TryGetWorldBounds(prop, out Bounds propBounds))
                {
                    return;
                }

                float propWidth = Mathf.Max(propBounds.size.x, propBounds.size.z);
                float propDepth = Mathf.Min(propBounds.size.x, propBounds.size.z);
                if (propWidth < 0.0001f || propDepth < 0.0001f)
                {
                    return;
                }

                float targetWidth = furnitureWidth * widthFraction;
                float targetDepth = furnitureDepth * depthFraction;
                float scaleMul = Mathf.Min(targetWidth / propWidth, targetDepth / propDepth);
                prop.transform.localScale *= scaleMul;
            }

            if (snapAfterFit)
            {
                SnapPropBottomToFurnitureTop(furniture, decorPrefix, prop);
            }
        }

        public static void FitPropFootprintOnDesk(GameObject desk, GameObject prop, float widthFraction,
            float depthFraction = -1f)
        {
            if (desk == null || prop == null || widthFraction <= 0f)
            {
                return;
            }

            FitPropFootprintOnFurniture(desk, "Desk", prop, widthFraction, depthFraction, false);
            SnapPropBottomToDeskTop(desk, prop);
        }

        public static void FitPropFootprintOnShelf(GameObject shelf, GameObject prop, float widthFraction,
            float depthFraction = -1f)
        {
            FitPropFootprintOnFurniture(shelf, "Shelf", prop, widthFraction, depthFraction);
        }

        public static bool TryGetShelfMeshWorldBounds(GameObject shelf, out Bounds bounds)
        {
            bounds = default;
            if (shelf == null)
            {
                return false;
            }

            Renderer[] renderers = shelf.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            bool found = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Transform rendererTransform = renderers[i].transform;
                if (rendererTransform.parent == shelf.transform
                    && (rendererTransform.name.StartsWith("Shelf") || rendererTransform.name.StartsWith("Wardrobe")))
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderers[i].bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return found;
        }

        public static float GetShelfTierWorldY(GameObject shelf, float normalizedHeight)
        {
            if (!TryGetShelfMeshWorldBounds(shelf, out Bounds bounds)
                && !TryGetWorldBounds(shelf, out bounds))
            {
                return 0.5f;
            }

            normalizedHeight = Mathf.Clamp01(normalizedHeight);
            return Mathf.Lerp(bounds.min.y + 0.08f, bounds.max.y - 0.06f, normalizedHeight);
        }

        /// <summary>Inset center of shelf board area in shelf local space (Kenney meshes use offset localBounds).</summary>
        public static bool TryGetShelfBoardLocalPoint(GameObject shelf, string decorPrefix, out Vector3 localPoint)
        {
            localPoint = new Vector3(0f, 0f, 0.04f);
            if (shelf == null)
            {
                return false;
            }

            Renderer rootRenderer = shelf.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                Bounds localBounds = rootRenderer.localBounds;
                const float insetX = 0.38f;
                const float insetZFront = 0.22f;
                const float insetZBack = 0.32f;
                float x = localBounds.center.x;
                float z = Mathf.Lerp(localBounds.min.z + insetZBack, localBounds.max.z - insetZFront, 0.55f);
                x = Mathf.Clamp(x, localBounds.min.x + insetX, localBounds.max.x - insetX);
                localPoint = new Vector3(x, 0f, z);
                return localBounds.size.x > 0.05f && localBounds.size.z > 0.05f;
            }

            if (!TryGetShelfMeshWorldBounds(shelf, out Bounds worldBounds))
            {
                return false;
            }

            Vector3 worldCenter = new(worldBounds.center.x, worldBounds.min.y, worldBounds.center.z);
            localPoint = shelf.transform.InverseTransformPoint(worldCenter);
            localPoint.y = 0f;
            return true;
        }

        public static void SnapPropToShelfTier(GameObject shelf, GameObject prop, float normalizedTier)
        {
            if (shelf == null || prop == null)
            {
                return;
            }

            SnapPropBottomToWorldY(prop, GetShelfTierWorldY(shelf, normalizedTier));
        }

        public static Vector3 ClampXZInsideRoom(Vector3 position, float halfWidth, float halfDepth, float margin)
        {
            float maxX = halfWidth - margin;
            float maxZ = halfDepth - margin;
            position.x = Mathf.Clamp(position.x, -maxX, maxX);
            position.z = Mathf.Clamp(position.z, -maxZ, maxZ);
            return position;
        }

        /// <summary>Moves <paramref name="movable"/> on XZ until it no longer overlaps <paramref name="anchor"/>.</summary>
        public static void EnsureXZClearance(GameObject movable, GameObject anchor, float clearance)
        {
            if (movable == null || anchor == null)
            {
                return;
            }

            for (int attempt = 0; attempt < 16; attempt++)
            {
                if (!TryGetWorldBounds(movable, out Bounds movableBounds)
                    || !TryGetWorldBounds(anchor, out Bounds anchorBounds))
                {
                    return;
                }

                if (!OverlapsXZ(movableBounds, anchorBounds, clearance))
                {
                    return;
                }

                Vector3 away = movable.transform.position - anchor.transform.position;
                away.y = 0f;
                if (away.sqrMagnitude < 0.0001f)
                {
                    away = Vector3.left;
                }

                away.Normalize();
                movable.transform.position += away * 0.42f;
            }
        }

        public static void RemovePropsOverlappingBed(GameObject bed, Transform worldRoot)
        {
            if (bed == null || worldRoot == null)
            {
                return;
            }

            string[] removablePaths =
            {
                "ExitArea/TravelBag",
                "ExitArea/Suitcase",
                "ExitArea/SideTable",
                "RoomPersonality/CornerPlant",
                "SleepingArea/ClothesPile"
            };

            for (int i = 0; i < removablePaths.Length; i++)
            {
                Transform candidate = FindInRoom(worldRoot, removablePaths[i]);
                if (candidate == null)
                {
                    continue;
                }

                if (OverlapsXZ(bed, candidate.gameObject, 0.15f))
                {
                    Object.DestroyImmediate(candidate.gameObject);
                }
            }
        }

        public static bool OverlapsXZ(GameObject a, GameObject b, float clearance)
        {
            if (a == null || b == null
                || !TryGetWorldBounds(a, out Bounds boundsA)
                || !TryGetWorldBounds(b, out Bounds boundsB))
            {
                return false;
            }

            return OverlapsXZ(boundsA, boundsB, clearance);
        }

        public static void ClampInsideRoom(GameObject root, float halfWidth, float halfDepth, float margin)
        {
            if (root == null || !TryGetWorldBounds(root, out Bounds bounds))
            {
                return;
            }

            Vector3 position = root.transform.position;
            float maxX = halfWidth - margin;
            float maxZ = halfDepth - margin;

            float dx = 0f;
            if (bounds.min.x < -maxX)
            {
                dx = -maxX - bounds.min.x;
            }
            else if (bounds.max.x > maxX)
            {
                dx = maxX - bounds.max.x;
            }

            float dz = 0f;
            if (bounds.min.z < -maxZ)
            {
                dz = -maxZ - bounds.min.z;
            }
            else if (bounds.max.z > maxZ)
            {
                dz = maxZ - bounds.max.z;
            }

            root.transform.position = position + new Vector3(dx, 0f, dz);
        }

        private static bool OverlapsXZ(Bounds a, Bounds b, float clearance)
        {
            return a.min.x - clearance < b.max.x
                && a.max.x + clearance > b.min.x
                && a.min.z - clearance < b.max.z
                && a.max.z + clearance > b.min.z;
        }
    }
}
#endif
