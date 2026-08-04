using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Quaternius Downtown City Mega Kit prefabs grouped for Vietnamese roadside blocks.</summary>
    [CreateAssetMenu(fileName = "QuaterniusDowntownPrefabLibrary", menuName = "Cho Tet/Quaternius Downtown Prefab Library")]
    public class QuaterniusDowntownPrefabLibrary : ScriptableObject
    {
        [Header("Sidewalk decor (Quaternius props)")]
        public GameObject[] sidewalkPlanters;
        public GameObject[] sidewalkTrees;
        public GameObject[] sidewalkBollards;
        public GameObject[] sidewalkGroundDetails;
        public GameObject[] sidewalkStreetLights;
        public GameObject[] sidewalkBins;
        public GameObject[] sidewalkBusStops;

        [Header("Sidewalk — single Quaternius concrete material")]
        public Material sidewalkPavementMaterial;

        [Header("Validated complete buildings (shared by both roadside sides)")]
        public GameObject[] completeBuildings;

        [Header("Legacy module lists (unused by spawner)")]
        public GameObject[] houseFacades;
        public GameObject[] shopFacades;
        public GameObject[] apartmentFacades;
        public GameObject[] cornerFacades;
        public GameObject[] roofCaps;

        [Header("Pre-baked roadside blocks (optional cache)")]
        public GameObject[] leftBlockVariants;
        public GameObject[] rightBlockVariants;

        [Header("Buffer corridor props")]
        public GameObject[] trees;
        public GameObject[] utilityPoles;
        public GameObject[] streetLights;
        public GameObject[] trafficSigns;

        public bool HasFacades => HasAny(completeBuildings);

        public bool HasBlockVariants =>
            HasAny(leftBlockVariants) && HasAny(rightBlockVariants);

        public GameObject PickHouse(int seed) => PickFrom(houseFacades, seed) ?? PickFrom(completeBuildings, seed + 3);

        public GameObject PickShop(int seed) => PickFrom(shopFacades, seed) ?? PickFrom(houseFacades, seed + 5);

        public GameObject PickApartment(int seed) =>
            PickFrom(apartmentFacades, seed) ?? PickFrom(completeBuildings, seed + 7);

        public GameObject PickCorner(int seed) => PickFrom(cornerFacades, seed);

        public GameObject PickRoof(int seed) => PickFrom(roofCaps, seed);

        public GameObject PickLeftBlock(int seed) => PickFrom(leftBlockVariants, seed);

        public GameObject PickRightBlock(int seed) => PickFrom(rightBlockVariants, seed);

        public GameObject PickTree(int seed) => PickFrom(trees, seed);

        public GameObject PickUtilityPole(int seed) => PickFrom(utilityPoles, seed);

        public GameObject PickStreetLight(int seed) => PickFrom(streetLights, seed);

        public GameObject PickTrafficSign(int seed) => PickFrom(trafficSigns, seed);

        public GameObject PickSidewalkPlanter(int seed) => PickFrom(sidewalkPlanters, seed);

        public GameObject PickSidewalkTree(int seed) =>
            PickFrom(sidewalkTrees, seed) ?? PickSidewalkPlanter(seed);

        public GameObject PickSidewalkBollard(int seed) => PickFrom(sidewalkBollards, seed);

        public GameObject PickSidewalkGroundDetail(int seed) => PickFrom(sidewalkGroundDetails, seed);

        public GameObject PickSidewalkStreetLight(int seed) => PickFrom(sidewalkStreetLights, seed);

        public GameObject PickSidewalkBin(int seed) => PickFrom(sidewalkBins, seed);

        public GameObject PickSidewalkBusStop(int seed) => PickFrom(sidewalkBusStops, seed);

        private static bool HasAny(GameObject[] list) => list != null && list.Length > 0;

        private static GameObject PickFrom(GameObject[] options, int seed)
        {
            if (options == null || options.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < options.Length; i++)
            {
                GameObject candidate = options[(Mathf.Abs(seed) + i) % options.Length];
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
