using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Layout constants for the Level 1 two-lane horizontal bus stop road (Phase 2).</summary>
    public static class Level1BusStopRoadSettings
    {
        public const float AsphaltSurfaceY = 0.07f;
        public const float SidewalkSurfaceY = Level1OutdoorSettings.WalkSurfaceY;

        /// <summary>Traffic flows along X (left entry ↔ right exit).</summary>
        public const float RoadCenterZ = -28.8f;

        /// <summary>Highway extends past the camera frustum and terrain lips (±20).</summary>
        public const float RoadExtentHalfX = 22f;

        public const float PlayableHalfX = 7.2f;
        public const float BusStopPadHalfX = 4.6f;

        /// <summary>PF_ComTamShop scene anchor — bus shelter sits on its east apron.</summary>
        public const float ComTamShopCenterX = 9.89f;
        public const float ComTamShopCenterZ = -21.22f;
        public const float ComTamShopHalfWidth = 2.925f;

        public const float ShelterWidth = 2.5f;
        public const float BusShelterDepth = 1.05f;
        public const float ShelterBesideComTamGap = 1f;
        public const float ShelterSouthOfShopOffset = 0.15f;

        public static float ShelterHalfWidth => ShelterWidth * 0.5f;

        public static float BusShelterCenterX =>
            ComTamShopCenterX + ComTamShopHalfWidth + ShelterBesideComTamGap + ShelterHalfWidth;

        public static float BusShelterCenterZ => ComTamShopCenterZ - ShelterSouthOfShopOffset;

        public const float BusShelterRoadSetback = 0.42f;

        /// <summary>Gap between shelter east edge and pickup vehicle center on the highway shoulder.</summary>
        public const float PickupVehicleShelterClearanceX = 0.45f;

        /// <summary>Local -X from shelter root to the route sign post (toward Com Tam).</summary>
        public const float BusStopSignOffsetX = 2.73f;

        /// <summary>Each driving lane half-depth — wide enough for a bus.</summary>
        public const float SingleLaneHalfDepth = 2.12f;
        public const float CenterDividerWidth = 0.14f;
        public const float ShoulderDepth = 0.55f;

        /// <summary>Pickup X on the highway shoulder — east of the shelter footprint, no overlap.</summary>
        public static float PickupParkX =>
            BusShelterCenterX + ShelterHalfWidth
            + Level1PickupStopLayout.VehicleBoundsHalfLength
            + PickupVehicleShelterClearanceX;

        public static float BusParkX => PickupParkX;
        public const float BusFacingYaw = 90f;

        public const float CrosswalkCenterX = -1.15f;
        public const float CrosswalkHalfWidth = 0.95f;

        public static float BusRootY => AsphaltSurfaceY - 0.02f;

        /// <summary>North / upper lane — traffic moves right → left (-X).</summary>
        public static float LaneNorthCenterZ =>
            RoadCenterZ - SingleLaneHalfDepth - CenterDividerWidth * 0.5f;

        /// <summary>South / lower lane — traffic moves left → right (+X).</summary>
        public static float LaneSouthCenterZ =>
            RoadCenterZ + SingleLaneHalfDepth + CenterDividerWidth * 0.5f;

        public static float LaneNorthEdgeZ => RoadCenterZ - SingleLaneHalfDepth * 2f - CenterDividerWidth;
        public static float LaneSouthEdgeZ => RoadCenterZ + SingleLaneHalfDepth * 2f + CenterDividerWidth;

        public static float SidewalkNearZ => LaneSouthEdgeZ + ShoulderDepth + 0.08f;
        public static float SidewalkFarZ => SidewalkNearZ + 2.35f;
        public static float SidewalkCenterZ => (SidewalkNearZ + SidewalkFarZ) * 0.5f;

        /// <summary>Road/sidewalk boundary on the pickup side of the highway.</summary>
        public static float CurbEdgeZForDirection(VehicleTrafficDirection direction)
        {
            return direction == VehicleTrafficDirection.LeftToRight
                ? SidewalkNearZ
                : LaneNorthEdgeZ - ShoulderDepth;
        }

        /// <summary>Sidewalk center Z for the Vietnamese bus shelter cluster.</summary>
        public static float BusShelterSidewalkZ => BusShelterCenterZ;

        /// <summary>Where pickup vehicles stop on the highway shoulder — separate Z from the shelter.</summary>
        public static Vector3 PickupCurbStopPosition(float curbOffset = 0.35f) =>
            new Vector3(PickupParkX, BusRootY, SidewalkNearZ - curbOffset);

        public static float PickupStopZ(float curbOffset = 0.35f) =>
            PickupCurbStopPosition(curbOffset).z;

        public static bool OverlapsShelterFootprint(float x, float z, float margin = 0.12f)
        {
            float halfW = ShelterHalfWidth + margin;
            float halfD = BusShelterDepth * 0.5f + margin;
            return x >= BusShelterCenterX - halfW && x <= BusShelterCenterX + halfW
                && z >= BusShelterCenterZ - halfD && z <= BusShelterCenterZ + halfD;
        }

        public static float RoadStartX => -RoadExtentHalfX;
        public static float RoadEndX => RoadExtentHalfX;
        public static float RoadLength => RoadEndX - RoadStartX;
        public static float RoadCenterX => 0f;
        public static float TotalRoadHalfDepth => SingleLaneHalfDepth * 2f + CenterDividerWidth * 0.5f + ShoulderDepth;

        public static float BusStopSignWorldX => BusShelterCenterX - BusStopSignOffsetX;

        public static Vector3 BusStopSignStandPosition => new Vector3(
            BusStopSignWorldX,
            SidewalkSurfaceY,
            BusShelterCenterZ + 0.12f);

        public static float LaneCenterZForDirection(VehicleTrafficDirection direction)
        {
            return direction == VehicleTrafficDirection.LeftToRight
                ? LaneSouthCenterZ
                : LaneNorthCenterZ;
        }

        public static bool OverlapsRoad(float x, float z, float marginX = 0.35f, float marginZ = 0.35f)
        {
            return x >= RoadStartX - marginX && x <= RoadEndX + marginX
                && z >= LaneNorthEdgeZ - marginZ - ShoulderDepth
                && z <= LaneSouthEdgeZ + marginZ + ShoulderDepth;
        }

        public static bool OverlapsBusStopPad(float x, float z, float margin = 0.35f)
        {
            float padCenterX = BusShelterCenterX;
            return x >= padCenterX - BusStopPadHalfX - margin && x <= padCenterX + BusStopPadHalfX + margin
                && z >= SidewalkFarZ - margin && z <= SidewalkNearZ + margin;
        }
    }

    public enum VehicleTrafficDirection
    {
        LeftToRight = 1,
        RightToLeft = -1
    }
}
