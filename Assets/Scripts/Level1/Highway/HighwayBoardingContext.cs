using UnityEngine;



namespace ChoTet.Level1.Highway

{

    /// <summary>Carries pickup vehicle info from Level 1 boarding into the highway scene.</summary>

    public static class HighwayBoardingContext

    {

        public static bool HasBoardedVehicle { get; private set; }

        public static GameObject VehiclePrefab { get; private set; }

        public static float VehicleScale { get; private set; } = 1.55f;

        public static int BodyColorIndex { get; private set; }

        public static float ModelRotationOffsetY { get; private set; }



        public static void SetFromPickup(GameObject prefab, float scale, int bodyColorIndex, float modelRotationOffsetY = 0f)

        {

            HasBoardedVehicle = prefab != null;

            VehiclePrefab = prefab;

            VehicleScale = Mathf.Max(0.5f, scale);

            BodyColorIndex = bodyColorIndex;

            ModelRotationOffsetY = modelRotationOffsetY;

        }



        public static void Clear()

        {

            HasBoardedVehicle = false;

            VehiclePrefab = null;

            VehicleScale = 1.55f;

            BodyColorIndex = 0;

            ModelRotationOffsetY = 0f;

        }

    }

}

