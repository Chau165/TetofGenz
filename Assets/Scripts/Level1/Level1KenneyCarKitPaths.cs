namespace ChoTet.Level1
{
    /// <summary>Asset paths for Kenney Car Kit traffic vehicles.</summary>
    public static class Level1KenneyCarKitPaths
    {
        public const string FbxRoot = "Assets/Models/FBX format";
        public const string ColormapTexture = FbxRoot + "/Textures/colormap.png";
        public const string TrafficRoot = "Assets/Art/Level1/Traffic/KenneyCarKit";
        public const string PrefabRoot = TrafficRoot + "/Prefabs";
        public const string MaterialRoot = TrafficRoot + "/Materials";
        public const string ColormapMaterial = MaterialRoot + "/KenneyCar_Colormap_URP.mat";

        public const string SedanPrefab = PrefabRoot + "/TrafficSedan.prefab";
        public const string SedanSportsPrefab = PrefabRoot + "/TrafficSedanSports.prefab";
        public const string HatchbackSportsPrefab = PrefabRoot + "/TrafficHatchbackSports.prefab";
        public const string SuvPrefab = PrefabRoot + "/TrafficSuv.prefab";
        public const string SuvLuxuryPrefab = PrefabRoot + "/TrafficSuvLuxury.prefab";
        public const string TaxiPrefab = PrefabRoot + "/TrafficTaxi.prefab";
        public const string VanPrefab = PrefabRoot + "/TrafficVan.prefab";
        public const string DeliveryPrefab = PrefabRoot + "/TrafficDelivery.prefab";
        public const string DeliveryFlatPrefab = PrefabRoot + "/TrafficDeliveryFlat.prefab";
        public const string PolicePrefab = PrefabRoot + "/TrafficPolice.prefab";
        public const string RacePrefab = PrefabRoot + "/TrafficRace.prefab";
        public const string RaceFuturePrefab = PrefabRoot + "/TrafficRaceFuture.prefab";
        public const string KartPrefab = PrefabRoot + "/TrafficKart.prefab";
        public const string TractorPrefab = PrefabRoot + "/TrafficTractor.prefab";
        public const string TractorShovelPrefab = PrefabRoot + "/TrafficTractorShovel.prefab";
        public const string TruckPrefab = PrefabRoot + "/TrafficTruck.prefab";
        public const string TruckFlatPrefab = PrefabRoot + "/TrafficTruckFlat.prefab";

        public static readonly string[] TrafficPrefabPaths =
        {
            SedanPrefab,
            SedanSportsPrefab,
            HatchbackSportsPrefab,
            SuvPrefab,
            SuvLuxuryPrefab,
            TaxiPrefab,
            VanPrefab,
            DeliveryPrefab,
            DeliveryFlatPrefab,
            PolicePrefab,
            RacePrefab,
            RaceFuturePrefab,
            KartPrefab,
            TractorPrefab,
            TractorShovelPrefab,
            TruckPrefab,
            TruckFlatPrefab
        };

        public const string PickupTrafficPrefab = VanPrefab;

        public static bool IsAlienTrafficPrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return false;
            }

            string lower = prefabName.ToLowerInvariant();
            return lower.Contains("racefuture")
                   || lower.Contains("kart")
                   || lower.Contains("alien")
                   || lower.Contains("ufo");
        }

        public static bool IsBusStopCandidatePrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName) || IsAlienTrafficPrefab(prefabName))
            {
                return false;
            }

            string lower = prefabName.ToLowerInvariant();
            if (lower.Contains("tractor") || lower.Contains("truck") || lower.Contains("kart")
                || lower.Contains("police") || lower.Contains("race"))
            {
                return false;
            }

            return lower.Contains("sedan") || lower.Contains("hatchback") || lower.Contains("suv")
                   || lower.Contains("taxi") || lower.Contains("van") || lower.Contains("delivery");
        }
    }
}
