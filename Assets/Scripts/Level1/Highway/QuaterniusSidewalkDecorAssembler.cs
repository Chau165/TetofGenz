using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>
    /// Municipal sidewalk furniture: trees along buildings, bins along curb, occasional bus stops.
    /// Props use fixed rows and spacing so each object reads clearly from the driving camera.
    /// </summary>
    public static class QuaterniusSidewalkDecorAssembler
    {
        private const float SegmentMarginZ = 5f;
        private const float TreeSpacingZ = 14f;
        private const float BinSpacingZ = 21f;
        private const float BusStopBinOffsetZ = 2.6f;

        public static void BuildDecor(
            Transform parent,
            QuaterniusDowntownPrefabLibrary library,
            bool leftSide,
            int seed,
            float segmentLengthZ)
        {
            if (library == null)
            {
                return;
            }

            PlaceTreeRow(parent, library, leftSide, seed, segmentLengthZ);
            PlaceTrashBinRow(parent, library, leftSide, seed, segmentLengthZ);

            if ((seed % 2) == 0)
            {
                float stationZ = segmentLengthZ * 0.5f;
                PlaceBusStopStation(parent, library, leftSide, seed, stationZ);
            }
        }

        private static void PlaceTreeRow(
            Transform parent,
            QuaterniusDowntownPrefabLibrary library,
            bool leftSide,
            int seed,
            float segmentLengthZ)
        {
            float phase = leftSide ? 3f : 10f;
            float rowX = HighwayUrbanRoadsideLayout.TreeRowX;

            for (float z = SegmentMarginZ + phase; z <= segmentLengthZ - SegmentMarginZ; z += TreeSpacingZ)
            {
                int propSeed = seed + Mathf.RoundToInt(z);
                TryPlace(
                    parent,
                    library.PickSidewalkTree(propSeed),
                    leftSide,
                    rowX,
                    z,
                    DecorKind.StreetTree);
            }
        }

        private static void PlaceTrashBinRow(
            Transform parent,
            QuaterniusDowntownPrefabLibrary library,
            bool leftSide,
            int seed,
            float segmentLengthZ)
        {
            float phase = leftSide ? 7f : 16f;
            float rowX = HighwayUrbanRoadsideLayout.TrashBinRowX;
            float busStopZ = (seed % 2) == 0 ? segmentLengthZ * 0.5f : -1f;

            for (float z = SegmentMarginZ + phase; z <= segmentLengthZ - SegmentMarginZ; z += BinSpacingZ)
            {
                if (busStopZ >= 0f && Mathf.Abs(z - (busStopZ + BusStopBinOffsetZ)) < 1.2f)
                {
                    continue;
                }

                int propSeed = seed + Mathf.RoundToInt(z) + 17;
                TryPlace(
                    parent,
                    library.PickSidewalkBin(propSeed),
                    leftSide,
                    rowX,
                    z,
                    DecorKind.TrashBin);
            }
        }

        private static void PlaceBusStopStation(
            Transform parent,
            QuaterniusDowntownPrefabLibrary library,
            bool leftSide,
            int seed,
            float stationZ)
        {
            TryPlace(
                parent,
                library.PickSidewalkBusStop(seed + 101),
                leftSide,
                HighwayUrbanRoadsideLayout.BusStopRowX,
                stationZ,
                DecorKind.BusStop);

            TryPlace(
                parent,
                library.PickSidewalkBin(seed + 103),
                leftSide,
                HighwayUrbanRoadsideLayout.TrashBinRowX,
                stationZ + BusStopBinOffsetZ,
                DecorKind.TrashBin);
        }

        private enum DecorKind
        {
            StreetTree,
            TrashBin,
            BusStop
        }

        private static void TryPlace(
            Transform parent,
            GameObject prefab,
            bool leftSide,
            float rowAbsX,
            float localZ,
            DecorKind kind)
        {
            if (prefab == null || !IsRowInsideSidewalk(rowAbsX))
            {
                return;
            }

            float signedX = HighwayUrbanRoadsideLayout.SignedRowX(leftSide, rowAbsX);
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name;
            instance.transform.localPosition = new Vector3(
                signedX,
                HighwayUrbanRoadsideLayout.SidewalkSurfaceY,
                localZ);
            instance.transform.localRotation = RotationFor(kind, prefab, leftSide);
            instance.transform.localScale = Vector3.one * ScaleFor(kind);
            HighwayPropUtility.StripColliders(instance);
        }

        private static Quaternion RotationFor(DecorKind kind, GameObject prefab, bool leftSide)
        {
            switch (kind)
            {
                case DecorKind.TrashBin:
                case DecorKind.BusStop:
                    return HighwayUrbanBuildingOrientation.FaceRoadRotation(prefab, leftSide);
                case DecorKind.StreetTree:
                    float alongStreetYaw = leftSide ? 0f : 180f;
                    return Quaternion.Euler(0f, alongStreetYaw, 0f);
                default:
                    return Quaternion.identity;
            }
        }

        private static float ScaleFor(DecorKind kind)
        {
            switch (kind)
            {
                case DecorKind.TrashBin:
                    return 0.92f;
                case DecorKind.BusStop:
                    return 1f;
                case DecorKind.StreetTree:
                    return 1f;
                default:
                    return 1f;
            }
        }

        private static bool IsRowInsideSidewalk(float absRowX)
        {
            float inner = HighwayUrbanRoadsideLayout.SidewalkInnerX + 0.25f;
            float outer = HighwayUrbanRoadsideLayout.SidewalkOuterX - 0.25f;
            return absRowX >= inner && absRowX <= outer;
        }
    }
}
