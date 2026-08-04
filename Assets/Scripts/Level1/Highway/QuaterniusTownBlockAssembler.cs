using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Spawns dense strips of validated complete building prefabs along the highway.</summary>
    public static class QuaterniusTownBlockAssembler
    {
        private struct PlacedBuilding
        {
            public Bounds LocalBounds;
            public float RowX;
        }

        public static GameObject BuildBlock(
            QuaterniusDowntownPrefabLibrary library,
            bool leftSide,
            int seed,
            float blockLengthZ)
        {
            GameObject[] catalog = QuaterniusCompleteBuildingCatalog.Resolve(library);
            if (catalog.Length == 0)
            {
                Debug.LogWarning("[QuaterniusTownBlockAssembler] No validated complete building prefabs found.");
                return null;
            }

            string blockName = leftSide ? "QuaterniusTownBlock_Left" : "QuaterniusTownBlock_Right";
            var root = new GameObject(blockName);
            var buildingRoot = new GameObject("Buildings");
            buildingRoot.transform.SetParent(root.transform, false);

            var placed = new List<PlacedBuilding>();
            float spacing = HighwayUrbanRoadsideLayout.BuildingLotStepZ + 1.4f;
            int slotCount = Mathf.Max(3, Mathf.FloorToInt(blockLengthZ / spacing));
            float startZ = -blockLengthZ * 0.5f + spacing * 0.5f;

            float[] rows =
            {
                HighwayUrbanRoadsideLayout.BuildingRowX,
                HighwayUrbanRoadsideLayout.BuildingRowSecondaryX,
                HighwayUrbanRoadsideLayout.BuildingRowTertiaryX
            };

            int lastBuildingIndex = -99;

            for (int slot = 0; slot < slotCount; slot++)
            {
                float localZ = startZ + slot * spacing;
                int slotSeed = seed + slot * 41;

                for (int row = 0; row < rows.Length; row++)
                {
                    int pickSeed = slotSeed + row * 17;
                    TryPlaceCompleteBuilding(
                        buildingRoot.transform,
                        catalog,
                        leftSide,
                        rows[row],
                        localZ + row * 0.35f,
                        pickSeed,
                        ref lastBuildingIndex,
                        placed);
                }
            }

            PurgeInvalidBuildings(buildingRoot.transform);
            ValidateBlock(root, leftSide);
            return root;
        }

        private static bool TryPlaceCompleteBuilding(
            Transform parent,
            GameObject[] catalog,
            bool leftSide,
            float rowX,
            float localZ,
            int seed,
            ref int lastBuildingIndex,
            List<PlacedBuilding> placed)
        {
            for (int attempt = 0; attempt < catalog.Length; attempt++)
            {
                GameObject prefab = QuaterniusCompleteBuildingCatalog.Pick(
                    catalog,
                    seed + attempt * 7,
                    ref lastBuildingIndex);
                if (prefab == null)
                {
                    continue;
                }

                float signedX = HighwayUrbanRoadsideLayout.SignedRowX(leftSide, rowX);
                GameObject instance = Object.Instantiate(prefab, parent);
                instance.name = prefab.name;
                instance.transform.localPosition = new Vector3(signedX, HighwayLaneSettings.RoadSurfaceY, localZ);
                float scale = Mathf.Lerp(0.88f, 1.02f, ((seed + attempt) % 100) / 100f);
                instance.transform.localScale = Vector3.one * scale;
                instance.transform.localRotation = HighwayUrbanBuildingOrientation.FaceRoadRotation(prefab, leftSide);
                HighwayPropUtility.StripColliders(instance);

                if (!QuaterniusCompleteBuildingValidator.IsValidSpawnedInstance(instance))
                {
                    HighwayPropUtility.SafeDestroy(instance);
                    continue;
                }

                Bounds localBounds = ComputeLocalBounds(instance, parent);
                if (OverlapsPlaced(localBounds, placed, rowX))
                {
                    HighwayPropUtility.SafeDestroy(instance);
                    continue;
                }

                placed.Add(new PlacedBuilding { LocalBounds = localBounds, RowX = rowX });
                return true;
            }

            return false;
        }

        private static void PurgeInvalidBuildings(Transform buildingRoot)
        {
            for (int i = buildingRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = buildingRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                Transform accents = child.Find("FacadeAccents");
                if (accents != null)
                {
                    HighwayPropUtility.SafeDestroy(accents.gameObject);
                }

                if (!QuaterniusCompleteBuildingValidator.IsValidSpawnedInstance(child.gameObject)
                    || QuaterniusCompleteBuildingValidator.IsRejectedName(child.name))
                {
                    HighwayPropUtility.SafeDestroy(child.gameObject);
                }
            }
        }

        private static bool OverlapsPlaced(Bounds candidate, List<PlacedBuilding> placed, float rowX)
        {
            const float padding = 0.15f;
            for (int i = 0; i < placed.Count; i++)
            {
                PlacedBuilding slice = placed[i];
                if (Mathf.Abs(slice.RowX - rowX) > 0.5f)
                {
                    continue;
                }

                Bounds padded = slice.LocalBounds;
                padded.Expand(padding);
                if (padded.Intersects(candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static Bounds ComputeLocalBounds(GameObject instance, Transform blockRoot)
        {
            Bounds world = QuaterniusCompleteBuildingValidator.ComputeWorldBounds(instance);
            Vector3 center = blockRoot.InverseTransformPoint(world.center);
            return new Bounds(center, world.size);
        }

        public static void ValidateBlock(GameObject root, bool leftSide)
        {
            if (root == null)
            {
                return;
            }

            Transform buildings = root.transform.Find("Buildings");
            if (buildings != null)
            {
                PurgeInvalidBuildings(buildings);
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds world = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                world.Encapsulate(renderers[i].bounds);
            }

            float roadLimit = HighwayLaneSettings.PlayerRoadHalfWidth + HighwayUrbanRoadsideLayout.MinClearanceFromRoadEdge;
            float sidewalkOuter = HighwayUrbanRoadsideLayout.SidewalkOuterX;

            if (leftSide)
            {
                if (world.max.x > -sidewalkOuter + 0.05f || world.max.x > -roadLimit)
                {
                    ShiftRootX(root, Mathf.Min(-sidewalkOuter - world.max.x - 0.2f, -roadLimit - world.max.x - 0.2f));
                }
            }
            else
            {
                if (world.min.x < sidewalkOuter - 0.05f || world.min.x < roadLimit)
                {
                    ShiftRootX(root, Mathf.Max(sidewalkOuter - world.min.x + 0.2f, roadLimit - world.min.x + 0.2f));
                }
            }
        }

        private static void ShiftRootX(GameObject root, float deltaX)
        {
            if (Mathf.Abs(deltaX) < 0.001f)
            {
                return;
            }

            Transform buildings = root.transform.Find("Buildings");
            if (buildings != null)
            {
                buildings.localPosition += new Vector3(deltaX, 0f, 0f);
            }
        }
    }
}
