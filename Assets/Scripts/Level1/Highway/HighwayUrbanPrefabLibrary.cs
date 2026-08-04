using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Imported low-poly urban prefabs for static highway neighborhoods.</summary>
    [CreateAssetMenu(fileName = "HighwayUrbanPrefabLibrary", menuName = "Cho Tet/Highway Urban Prefab Library")]
    public class HighwayUrbanPrefabLibrary : ScriptableObject
    {
        public GameObject[] houses;
        public GameObject[] shops;
        public GameObject[] streetLights;
        public GameObject[] utilityPoles;
        public GameObject[] roadSigns;
        public GameObject[] roadsideProps;
        public GameObject[] skylineBuildings;
        public GameObject[] treeFillers;

        public bool HasBuildings =>
            (houses != null && houses.Length > 0)
            || (shops != null && shops.Length > 0)
            || (skylineBuildings != null && skylineBuildings.Length > 0);

        public GameObject PickHouse(int seed) => PickFrom(houses, seed) ?? PickFrom(shops, seed + 3);

        public GameObject PickShop(int seed) => PickFrom(shops, seed);

        public GameObject PickSkyline(int seed) => PickFrom(skylineBuildings, seed) ?? PickHouse(seed);

        public GameObject PickStreetLight(int seed) => PickFrom(streetLights, seed);

        public GameObject PickUtilityPole(int seed) =>
            PickFrom(utilityPoles, seed) ?? PickFrom(streetLights, seed + 1);

        public GameObject PickRoadSign(int seed) => PickFrom(roadSigns, seed);

        public GameObject PickRoadsideProp(int seed) => PickFrom(roadsideProps, seed);

        public GameObject PickTreeFiller(int seed) => PickFrom(treeFillers, seed);

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
