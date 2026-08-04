using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Carries boarded vehicle info from bus-stop boarding through highway into arrival.</summary>
    public static class HometownArrivalContext
    {
        public static bool HasVehicleData { get; private set; }
        public static GameObject VehiclePrefab { get; private set; }
        public static float VehicleScale { get; private set; } = 1.55f;
        public static int BodyColorIndex { get; private set; }
        public static float ModelRotationOffsetY { get; private set; }

        /// <summary>Snapshot the boarded ride as soon as the player leaves the bus stop (after trọ/outdoor).</summary>
        public static void PrimeFromBoarding(
            GameObject prefab,
            float scale,
            int bodyColorIndex,
            float modelRotationOffsetY)
        {
            HasVehicleData = prefab != null;
            VehiclePrefab = prefab;
            VehicleScale = Mathf.Max(0.5f, scale);
            BodyColorIndex = bodyColorIndex;
            ModelRotationOffsetY = modelRotationOffsetY;
        }

        public static void TransferFromHighway()
        {
            if (HighwayBoardingContext.HasBoardedVehicle)
            {
                PrimeFromBoarding(
                    HighwayBoardingContext.VehiclePrefab,
                    HighwayBoardingContext.VehicleScale,
                    HighwayBoardingContext.BodyColorIndex,
                    HighwayBoardingContext.ModelRotationOffsetY);
            }

            HighwayBoardingContext.Clear();
        }

        public static void Clear()
        {
            HasVehicleData = false;
            VehiclePrefab = null;
            VehicleScale = 1.55f;
            BodyColorIndex = 0;
            ModelRotationOffsetY = 0f;
        }
    }
}
