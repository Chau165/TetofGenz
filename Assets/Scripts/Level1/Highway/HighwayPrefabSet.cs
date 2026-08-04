using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Prefab groups for scrolling Vietnamese highway roadside scenery.</summary>
    [System.Serializable]
    public class HighwayPrefabSet
    {
        public GameObject[] houses;
        public GameObject[] shops;
        public GameObject[] trees;
        public GameObject[] streetLights;
        public GameObject[] utilityPoles;
        public GameObject[] fences;
        public GameObject[] vegetation;
        public GameObject[] roadsideProps;
        public GameObject[] roadSigns;
        public GameObject[] skylineBuildings;

        public bool HasAnyContent =>
            HasEntries(houses) || HasEntries(shops) || HasEntries(trees) || HasEntries(streetLights)
            || HasEntries(utilityPoles) || HasEntries(fences) || HasEntries(vegetation)
            || HasEntries(roadsideProps) || HasEntries(skylineBuildings);

        private static bool HasEntries(GameObject[] array) => array != null && array.Length > 0;
    }
}
