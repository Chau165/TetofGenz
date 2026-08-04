using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Fixed world-space sidewalks, buildings, and roadside props along the highway.</summary>
    [DefaultExecutionOrder(10)]
    public class HighwayStaticUrbanNeighborhood : MonoBehaviour
    {
        private enum PropKind
        {
            Sign,
            Pole,
            Building,
            Decoration,
            Tree
        }

        private struct PlacedProp
        {
            public GameObject Root;
            public Bounds WorldBounds;
            public PropKind Kind;
            public bool LeftSide;
        }

        [SerializeField] private float corridorStartZ = -80f;
        [SerializeField] private float corridorEndZ = 2320f;
        [SerializeField] private float lotSpacing = 2.2f;
        [SerializeField] private float sidewalkBlockLength = 160f;
        [SerializeField] private Material sidewalkMaterial;
        [SerializeField] private HighwayUrbanPrefabLibrary prefabLibrary;

        private const string LibraryAssetPath = "Assets/Settings/ChoTet/HighwayUrbanPrefabLibrary.asset";
        private const string LayoutMarkerPrefix = "UrbanLayout_v";
        private const float SidewalkY = -0.02f;
        private const float SidewalkThickness = 0.1f;
        private const float BoundsPadding = 0.18f;

        private readonly List<PlacedProp> placedProps = new List<PlacedProp>();

        private void Awake()
        {
            EnsureAssets();
            if (NeedsRebuild())
            {
                RebuildNeighborhood();
            }
        }

        public void Initialize(Material sidewalk, HighwayUrbanPrefabLibrary library)
        {
            sidewalkMaterial = sidewalk;
            if (library != null)
            {
                prefabLibrary = library;
            }

            EnsureAssets();
            RebuildNeighborhood();
        }

        public void RebuildNeighborhood()
        {
            EnsureAssets();
            ClearGeneratedContent();
            placedProps.Clear();
            HighwayUrbanMaterialUtility.BeginColorAssignmentPass();

            BuildUrbanLots();
            RunValidationPass();
            RunColorValidationPass();
            EnsureLayoutMarker();
        }

        private void EnsureAssets()
        {
            sidewalkMaterial = HighwaySidewalkMaterialUtility.Resolve();
#if UNITY_EDITOR
            if (prefabLibrary == null)
            {
                prefabLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<HighwayUrbanPrefabLibrary>(LibraryAssetPath);
            }
#endif
        }

        private bool NeedsRebuild()
        {
            return transform.Find(GetLayoutMarkerName()) == null;
        }

        private static string GetLayoutMarkerName() =>
            LayoutMarkerPrefix + HighwayUrbanRoadsideLayout.ContentVersion
            + "_c" + HighwayUrbanMaterialUtility.ColorPaletteVersion;

        private void BuildUrbanLots()
        {
            if (HighwayQuaterniusTownBlockPool.ShouldUseForBuildings())
            {
                return;
            }

            Transform leftLots = CreateRoot("LeftUrbanLots");
            Transform rightLots = CreateRoot("RightUrbanLots");

            int lotIndex = 0;
            for (float z = corridorStartZ; z <= corridorEndZ; z += lotSpacing)
            {
                int leftSeed = lotIndex * 53 + 11;
                int rightSeed = lotIndex * 53 + 29;
                BuildDenseBlock(leftLots, true, z, leftSeed, lotIndex);
                BuildDenseBlock(rightLots, false, z + lotSpacing * 0.5f, rightSeed, lotIndex);
                lotIndex++;
            }
        }

        private void BuildDenseBlock(Transform parent, bool leftSide, float z, int seed, int lotIndex)
        {
            if (prefabLibrary == null)
            {
                return;
            }

            float alignedZ = SnapToBuildingGrid(z);
            if ((seed % 4) != 0)
            {
                PlaceUtilityCorridorProps(parent, leftSide, alignedZ, seed);
            }
        }

        private static GameObject PickValidatedBuilding(GameObject prefab, int seed)
        {
            if (prefab != null && QuaterniusCompleteBuildingValidator.IsValidPrefabAsset(prefab))
            {
                return prefab;
            }

            return null;
        }

        private void PlaceUtilityCorridorProps(Transform parent, bool leftSide, float z, int seed)
        {
            if ((seed % 3) == 0)
            {
                TryPlacePrefab(
                    parent,
                    prefabLibrary.PickUtilityPole(seed),
                    leftSide,
                    HighwayUrbanRoadsideLayout.UtilityPoleRowX,
                    z,
                    seed,
                    0.85f,
                    0.95f,
                    faceRoad: true,
                    PropKind.Pole);
            }

            if ((seed % 4) == 0)
            {
                TryPlacePrefab(
                    parent,
                    prefabLibrary.PickStreetLight(seed + 2),
                    leftSide,
                    HighwayUrbanRoadsideLayout.StreetLightRowX,
                    z + 1.2f,
                    seed + 2,
                    0.9f,
                    1.05f,
                    faceRoad: true,
                    PropKind.Decoration);
            }

            if ((seed % 5) == 0)
            {
                TryPlacePrefab(
                    parent,
                    prefabLibrary.PickRoadSign(seed + 5),
                    leftSide,
                    HighwayUrbanRoadsideLayout.SignRowX,
                    z + 1.8f,
                    seed + 5,
                    0.95f * HighwayUrbanRoadsideLayout.SignScaleBoost,
                    1.05f * HighwayUrbanRoadsideLayout.SignScaleBoost,
                    faceRoad: true,
                    PropKind.Sign);
            }

            if ((seed % 6) == 0)
            {
                TryPlacePrefab(
                    parent,
                    prefabLibrary.PickTreeFiller(seed + 11),
                    leftSide,
                    HighwayUrbanRoadsideLayout.TreeRowX,
                    z + 0.6f,
                    seed + 11,
                    0.75f,
                    0.95f,
                    faceRoad: false,
                    PropKind.Tree);
            }

            if ((seed % 8) == 0)
            {
                TryPlaceTetBanner(parent, leftSide, z + 2.4f, seed + 31);
            }
        }

        private static float SnapToBuildingGrid(float z)
        {
            float grid = HighwayUrbanRoadsideLayout.BuildingGridSnap;
            return Mathf.Round(z / grid) * grid;
        }

        private bool TryPlaceBuilding(
            Transform parent,
            GameObject prefab,
            bool leftSide,
            float outwardX,
            float worldZ,
            int seed,
            int lotIndex)
        {
            for (int attempt = 0; attempt < 4; attempt++)
            {
                float tryZ = worldZ + attempt * HighwayUrbanRoadsideLayout.MinBuildingSpacingZ;
                if (TryPlacePrefab(
                        parent,
                        prefab,
                        leftSide,
                        outwardX,
                        tryZ,
                        seed + attempt * 3,
                        0.62f,
                        0.82f,
                        faceRoad: true,
                        PropKind.Building,
                        lotIndex))
                {
                    return true;
                }
            }

            return false;
        }
        private void TryPlaceTetBanner(Transform parent, bool leftSide, float worldZ, int seed)
        {
            float signedX = HighwayUrbanRoadsideLayout.SignedRowX(leftSide, HighwayUrbanRoadsideLayout.DecorationRowX);
            if (!CanPlaceAt(signedX, worldZ, HighwayUrbanRoadsideLayout.SignOccupancyRadius, PropKind.Decoration))
            {
                return;
            }

            GameObject bannerRoot = new GameObject("TetBanner");
            bannerRoot.transform.SetParent(parent, false);
            bannerRoot.transform.position = new Vector3(signedX, HighwayLaneSettings.RoadSurfaceY + 2.8f, worldZ);
            bannerRoot.transform.rotation = Quaternion.Euler(0f, leftSide ? 90f : -90f, 0f);

            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "BannerPole";
            pole.transform.SetParent(bannerRoot.transform, false);
            pole.transform.localPosition = new Vector3(0f, -1.2f, 0f);
            pole.transform.localScale = new Vector3(0.08f, 1.2f, 0.08f);
            HighwayPropUtility.SafeDestroy(pole.GetComponent<Collider>());

            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = "BannerCloth";
            cloth.transform.SetParent(bannerRoot.transform, false);
            cloth.transform.localPosition = new Vector3(0f, 0.2f, 0.35f);
            cloth.transform.localScale = new Vector3(1.6f, 0.55f, 0.06f);
            HighwayPropUtility.SafeDestroy(cloth.GetComponent<Collider>());

            HighwayUrbanMaterialUtility.ApplySignMaterials(bannerRoot, seed);
            RegisterPlacement(bannerRoot, PropKind.Decoration, leftSide);
        }

        private bool TryPlacePrefab(
            Transform parent,
            GameObject prefab,
            bool leftSide,
            float outwardX,
            float worldZ,
            int seed,
            float minScale,
            float maxScale,
            bool faceRoad,
            PropKind kind,
            int lotIndex = 0)
        {
            if (prefab == null)
            {
                return false;
            }

            float signedX = HighwayUrbanRoadsideLayout.SignedRowX(leftSide, outwardX);
            float absX = Mathf.Abs(signedX);
            if (HighwayUrbanRoadsideLayout.IsOnDrivingSurface(absX))
            {
                return false;
            }

            if (kind == PropKind.Building)
            {
                if (!HighwayUrbanRoadsideLayout.IsBuildingRowSafe(absX))
                {
                    return false;
                }
            }
            else if (HighwayUrbanRoadsideLayout.IsOnDrivingSurface(absX)
                || HighwayUrbanRoadsideLayout.IsInsideSidewalk(absX)
                || !HighwayUrbanRoadsideLayout.IsInsideUtilityCorridor(absX))
            {
                return false;
            }

            float occupancyRadius = kind switch
            {
                PropKind.Building => HighwayUrbanRoadsideLayout.BuildingOccupancyRadius,
                PropKind.Sign => HighwayUrbanRoadsideLayout.SignOccupancyRadius,
                _ => HighwayUrbanRoadsideLayout.OccupancyRadius
            };

            if (!CanPlaceAt(signedX, worldZ, occupancyRadius, kind))
            {
                return false;
            }

            GameObject slot = new GameObject(prefab.name);
            slot.transform.SetParent(parent, false);
            slot.transform.position = new Vector3(signedX, HighwayLaneSettings.RoadSurfaceY, worldZ);

            GameObject instance = Object.Instantiate(prefab, slot.transform);
            instance.name = prefab.name;
            instance.transform.localPosition = Vector3.zero;
            if (kind == PropKind.Building)
            {
                instance.transform.localRotation = HighwayUrbanBuildingOrientation.FaceRoadRotation(prefab, leftSide);
            }
            else if (faceRoad)
            {
                float yaw = leftSide ? 90f : -90f;
                instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }
            else
            {
                instance.transform.localRotation = Quaternion.identity;
            }

            float scaleT = (seed % 100) / 100f;
            float scale = Mathf.Lerp(minScale, maxScale, scaleT);
            instance.transform.localScale = Vector3.one * scale;
            HighwayPropUtility.StripColliders(instance);

            if (kind == PropKind.Building)
            {
                if (!QuaterniusCompleteBuildingValidator.IsValidSpawnedInstance(instance))
                {
                    HighwayPropUtility.SafeDestroy(slot);
                    return false;
                }

                HighwayUrbanMaterialUtility.ApplyBuildingMaterials(
                    instance, seed, lotIndex, leftSide, applyFacadeAccents: false);
                SnapBuildingToSafeRow(slot, leftSide);
            }
            else if (kind == PropKind.Sign)
            {
                HighwayUrbanMaterialUtility.ApplySignMaterials(instance, seed);
            }

            if (!RegisterPlacement(slot, kind, leftSide))
            {
                HighwayPropUtility.SafeDestroy(slot);
                return false;
            }

            return true;
        }

        private static void SnapBuildingToSafeRow(GameObject root, bool leftSide)
        {
            for (int pass = 0; pass < 4; pass++)
            {
                Bounds bounds = ComputeWorldBounds(root);
                float roadLimit = HighwayLaneSettings.PlayerRoadHalfWidth + HighwayUrbanRoadsideLayout.MinClearanceFromRoadEdge;
                float sidewalkLimit = HighwayUrbanRoadsideLayout.BuildingBlockInnerX;
                Vector3 shift = Vector3.zero;

                if (leftSide)
                {
                    if (bounds.max.x > -sidewalkLimit)
                    {
                        shift.x -= bounds.max.x + sidewalkLimit;
                    }

                    bounds = ShiftBounds(bounds, shift);
                    if (bounds.max.x > -roadLimit)
                    {
                        shift.x -= bounds.max.x + roadLimit;
                    }
                }
                else
                {
                    if (bounds.min.x < sidewalkLimit)
                    {
                        shift.x += sidewalkLimit - bounds.min.x;
                    }

                    bounds = ShiftBounds(bounds, shift);
                    if (bounds.min.x < roadLimit)
                    {
                        shift.x += roadLimit - bounds.min.x;
                    }
                }

                if (shift.sqrMagnitude < 0.0001f)
                {
                    break;
                }

                root.transform.position += shift;
            }
        }

        private static Bounds ShiftBounds(Bounds source, Vector3 shift)
        {
            Bounds shifted = source;
            shifted.center += shift;
            return shifted;
        }

        private bool RegisterPlacement(GameObject root, PropKind kind, bool leftSide)
        {
            Bounds bounds = ComputeWorldBounds(root);
            if (bounds.size.sqrMagnitude < 0.001f)
            {
                bounds = new Bounds(root.transform.position, Vector3.one * 2f);
            }

            if (IntersectsForbiddenZone(bounds, kind, leftSide))
            {
                return false;
            }

            if (kind == PropKind.Building && IntersectsUtilityProps(bounds))
            {
                return false;
            }

            if (kind == PropKind.Building && IntersectsOtherBuilding(bounds, leftSide))
            {
                return false;
            }

            placedProps.Add(new PlacedProp
            {
                Root = root,
                WorldBounds = bounds,
                Kind = kind,
                LeftSide = leftSide
            });
            return true;
        }

        private bool CanPlaceAt(float x, float z, float radius, PropKind kind)
        {
            for (int i = 0; i < placedProps.Count; i++)
            {
                PlacedProp slot = placedProps[i];
                if (kind == PropKind.Sign && slot.Kind == PropKind.Pole)
                {
                    continue;
                }

                if (kind == PropKind.Building && slot.Kind != PropKind.Building)
                {
                    continue;
                }

                float dx = x - slot.WorldBounds.center.x;
                float dz = z - slot.WorldBounds.center.z;
                float minDist = radius + ApproxRadius(slot.WorldBounds);
                if (dx * dx + dz * dz < minDist * minDist)
                {
                    return false;
                }
            }

            return true;
        }

        private static float ApproxRadius(Bounds bounds)
        {
            return Mathf.Max(bounds.extents.x, bounds.extents.z) + BoundsPadding;
        }

        private static Bounds ComputeWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private bool IntersectsForbiddenZone(Bounds bounds, PropKind kind, bool leftSide)
        {
            float roadLimit = HighwayLaneSettings.PlayerRoadHalfWidth + HighwayUrbanRoadsideLayout.MinClearanceFromRoadEdge;
            if (Mathf.Abs(bounds.center.x) - bounds.extents.x < roadLimit)
            {
                return true;
            }

            if (kind == PropKind.Building && IntersectsUtilityCorridor(bounds))
            {
                return true;
            }

            if (kind == PropKind.Building)
            {
                if (!HighwayUrbanRoadsideLayout.BuildingBoundsInsideBlock(bounds, leftSide))
                {
                    return true;
                }
            }

            if (kind == PropKind.Building)
            {
                float sidewalkOuter = HighwayUrbanRoadsideLayout.SidewalkOuterX;
                if (bounds.center.x > 0f)
                {
                    if (bounds.min.x < sidewalkOuter)
                    {
                        return true;
                    }
                }
                else
                {
                    if (bounds.max.x > -sidewalkOuter)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IntersectsOtherBuilding(Bounds bounds, bool leftSide)
        {
            for (int i = 0; i < placedProps.Count; i++)
            {
                PlacedProp other = placedProps[i];
                if (other.Kind != PropKind.Building || other.LeftSide != leftSide)
                {
                    continue;
                }

                Bounds padded = other.WorldBounds;
                padded.Expand(BoundsPadding * 2f);
                if (BoundsOverlapXZ(bounds, padded))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IntersectsUtilityCorridor(Bounds bounds)
        {
            float outer = HighwayUrbanRoadsideLayout.UtilityCorridorOuterX;
            if (bounds.center.x < 0f)
            {
                return bounds.max.x > -outer;
            }

            return bounds.min.x < outer;
        }

        private bool IntersectsUtilityProps(Bounds bounds)
        {
            for (int i = 0; i < placedProps.Count; i++)
            {
                PlacedProp other = placedProps[i];
                if (other.Kind == PropKind.Building)
                {
                    continue;
                }

                Bounds padded = other.WorldBounds;
                padded.Expand(BoundsPadding);
                if (BoundsOverlapXZ(bounds, padded))
                {
                    return true;
                }
            }

            return false;
        }

        private void RunColorValidationPass()
        {
            var leftSlots = new List<HighwayUrbanMaterialUtility.NeighborhoodColorSlot>();
            var rightSlots = new List<HighwayUrbanMaterialUtility.NeighborhoodColorSlot>();

            for (int i = 0; i < placedProps.Count; i++)
            {
                PlacedProp prop = placedProps[i];
                if (prop.Kind != PropKind.Building || prop.Root == null)
                {
                    continue;
                }

                int lotIndex = Mathf.Abs(Mathf.RoundToInt(prop.Root.transform.position.z / lotSpacing));
                int seed = lotIndex * 53 + (prop.LeftSide ? 11 : 29);
                var slot = new HighwayUrbanMaterialUtility.NeighborhoodColorSlot
                {
                    Root = prop.Root,
                    Seed = seed,
                    LotIndex = lotIndex,
                    LeftSide = prop.LeftSide,
                    DominantWall = HighwayUrbanMaterialUtility.GetDominantWallMaterial(prop.Root)
                };

                if (prop.LeftSide)
                {
                    leftSlots.Add(slot);
                }
                else
                {
                    rightSlots.Add(slot);
                }
            }

            leftSlots.Sort((a, b) => a.Root.transform.position.z.CompareTo(b.Root.transform.position.z));
            rightSlots.Sort((a, b) => a.Root.transform.position.z.CompareTo(b.Root.transform.position.z));
            HighwayUrbanMaterialUtility.ValidateNeighborhoodColorSequence(leftSlots);
            HighwayUrbanMaterialUtility.ValidateNeighborhoodColorSequence(rightSlots);
        }

        private void RunValidationPass()
        {
            for (int pass = 0; pass < 3; pass++)
            {
                for (int i = placedProps.Count - 1; i >= 0; i--)
                {
                    PlacedProp prop = placedProps[i];
                    if (prop.Root == null)
                    {
                        placedProps.RemoveAt(i);
                        continue;
                    }

                    if (prop.Kind == PropKind.Building)
                    {
                        HighwayUrbanBuildingOrientation.CorrectFacing(prop.Root, null, prop.LeftSide);
                        int lotIndex = Mathf.Abs(Mathf.RoundToInt(prop.Root.transform.position.z));
                        HighwayUrbanMaterialUtility.ValidateBuildingMaterials(
                            prop.Root,
                            lotIndex,
                            lotIndex,
                            prop.LeftSide);
                        SnapBuildingToSafeRow(prop.Root, prop.LeftSide);
                    }

                    Bounds bounds = ComputeWorldBounds(prop.Root);
                    bool remove = false;

                    if (prop.Kind == PropKind.Building && IntersectsAnyOtherBuilding(i, bounds, prop.LeftSide))
                    {
                        if (!TryNudgeBuildingAlongZ(prop, i, bounds))
                        {
                            remove = true;
                        }
                        else
                        {
                            bounds = ComputeWorldBounds(prop.Root);
                        }
                    }

                    if (!remove && IntersectsForbiddenZone(bounds, prop.Kind, prop.LeftSide))
                    {
                        if (prop.Kind == PropKind.Building)
                        {
                            SnapBuildingToSafeRow(prop.Root, prop.LeftSide);
                            bounds = ComputeWorldBounds(prop.Root);
                            remove = IntersectsForbiddenZone(bounds, prop.Kind, prop.LeftSide)
                                || IntersectsUtilityProps(bounds);
                        }
                        else
                        {
                            remove = true;
                        }
                    }

                    if (!remove && prop.Kind == PropKind.Building && IntersectsUtilityProps(bounds))
                    {
                        SnapBuildingToSafeRow(prop.Root, prop.LeftSide);
                        bounds = ComputeWorldBounds(prop.Root);
                        remove = IntersectsUtilityProps(bounds);
                    }

                    if (!remove && prop.Kind == PropKind.Building
                        && !HighwayUrbanBuildingOrientation.IsFacingRoad(prop.Root, prop.LeftSide))
                    {
                        HighwayUrbanBuildingOrientation.CorrectFacing(prop.Root, null, prop.LeftSide);
                    }

                    if (remove)
                    {
                        HighwayPropUtility.SafeDestroy(prop.Root);
                        placedProps.RemoveAt(i);
                    }
                    else
                    {
                        PlacedProp updated = prop;
                        updated.WorldBounds = bounds;
                        placedProps[i] = updated;
                    }
                }
            }
        }

        private bool TryNudgeBuildingAlongZ(PlacedProp prop, int index, Bounds bounds)
        {
            float[] offsets = { 2.4f, -2.4f, 4.8f, -4.8f };
            Vector3 basePos = prop.Root.transform.position;
            for (int i = 0; i < offsets.Length; i++)
            {
                prop.Root.transform.position = basePos + new Vector3(0f, 0f, offsets[i]);
                SnapBuildingToSafeRow(prop.Root, prop.LeftSide);
                Bounds shifted = ComputeWorldBounds(prop.Root);
                if (!IntersectsAnyOtherBuilding(index, shifted, prop.LeftSide)
                    && !IntersectsForbiddenZone(shifted, PropKind.Building, prop.LeftSide)
                    && !IntersectsUtilityProps(shifted))
                {
                    return true;
                }
            }

            prop.Root.transform.position = basePos;
            return false;
        }

        private bool IntersectsAnyOtherBuilding(int index, Bounds bounds, bool leftSide)
        {
            for (int i = 0; i < placedProps.Count; i++)
            {
                if (i == index)
                {
                    continue;
                }

                PlacedProp other = placedProps[i];
                if (other.Kind != PropKind.Building || other.LeftSide != leftSide)
                {
                    continue;
                }

                Bounds padded = other.WorldBounds;
                padded.Expand(BoundsPadding * 2f);
                if (BoundsOverlapXZ(bounds, padded))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool BoundsOverlapXZ(Bounds a, Bounds b)
        {
            return a.min.x <= b.max.x && a.max.x >= b.min.x
                && a.min.z <= b.max.z && a.max.z >= b.min.z;
        }

        private Transform CreateRoot(string name)
        {
            Transform existing = transform.Find(name);
            if (existing != null)
            {
                HighwayPropUtility.ClearChildren(existing);
                return existing;
            }

            GameObject root = new GameObject(name);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            return root.transform;
        }

        private void EnsureLayoutMarker()
        {
            ClearLayoutMarkers();
            GameObject marker = new GameObject(GetLayoutMarkerName());
            marker.transform.SetParent(transform, false);
            marker.hideFlags = HideFlags.HideInHierarchy;
        }

        private void ClearLayoutMarkers()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null && child.name.StartsWith(LayoutMarkerPrefix))
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }
        }

        private void ClearGeneratedContent()
        {
            PurgeLegacyRoot("LeftSidewalkBelt");
            PurgeLegacyRoot("RightSidewalkBelt");

            string[] roots =
            {
                "LeftUrbanLots",
                "RightUrbanLots"
            };

            for (int i = 0; i < roots.Length; i++)
            {
                Transform child = transform.Find(roots[i]);
                if (child != null)
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }

            ClearLayoutMarkers();
        }

        private void PurgeLegacyRoot(string rootName)
        {
            Transform child = transform.Find(rootName);
            if (child != null)
            {
                HighwayPropUtility.SafeDestroy(child.gameObject);
            }
        }
    }
}
