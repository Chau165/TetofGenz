#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChoTet.Level1.Editor
{
    /// <summary>Loads low-poly urban and nature prefabs for the Level 1 highway scene.</summary>
    public static class HighwayPrefabLoader
    {
        private const string UrbanRoot = "Assets/Low Poly Simple Urban City 3D Asset Pack/Prefabs";
        private const string NatureRoot = "Assets/lowpoly-nature/lowpoly-nature/models & prefabs/prefabs";
        private const string AiRiRoot = "Assets/Assets/AIRIDev/Low-Poly Urban Assets/Prefabs";
        private const string OccaRoot = "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs";

        public static ChoTet.Level1.Highway.HighwayPrefabSet LoadDefaultSet()
        {
            var set = new ChoTet.Level1.Highway.HighwayPrefabSet
            {
                houses = LoadMany(
                    UrbanRoot + "/Buildings/Building_1.prefab",
                    UrbanRoot + "/Buildings/Building_2.prefab",
                    UrbanRoot + "/Buildings/Building_3.prefab",
                    UrbanRoot + "/Buildings/Building_4.prefab",
                    UrbanRoot + "/Buildings/Building_5.prefab",
                    UrbanRoot + "/Buildings/Building_6.prefab",
                    OccaRoot + "/House_2.prefab",
                    OccaRoot + "/House_3.prefab"),
                shops = LoadMany(
                    UrbanRoot + "/Props/Other/Hot_Dog.prefab",
                    UrbanRoot + "/Props/Other/Newspaper_Stand.prefab",
                    UrbanRoot + "/Props/Other/Tram_Stop.prefab",
                    AiRiRoot + "/book stall.prefab",
                    AiRiRoot + "/vending machine.prefab",
                    UrbanRoot + "/Props/Boxes/Box_1.prefab",
                    UrbanRoot + "/Props/Boxes/Box_2.prefab"),
                trees = LoadMany(
                    NatureRoot + "/tree1_1_prefab.prefab",
                    NatureRoot + "/tree2_2_prefab.prefab",
                    NatureRoot + "/tree3_1_prefab.prefab",
                    NatureRoot + "/tree4_2_prefab.prefab",
                    NatureRoot + "/tree5_3_prefab.prefab",
                    NatureRoot + "/tree6_1_prefab.prefab",
                    NatureRoot + "/smalltree1.prefab",
                    NatureRoot + "/smalltree2.prefab",
                    UrbanRoot + "/Props/Other/Tree_1.prefab",
                    UrbanRoot + "/Props/Other/Tree_2.prefab"),
                streetLights = LoadMany(
                    UrbanRoot + "/Props/Road_Signs/Light.prefab",
                    UrbanRoot + "/Props/Road_Signs/Traffic_Light_1.prefab",
                    UrbanRoot + "/Props/Road_Signs/Traffic_Light_2.prefab",
                    AiRiRoot + "/parking light.prefab"),
                utilityPoles = LoadMany(
                    AiRiRoot + "/electric pole.prefab",
                    UrbanRoot + "/Props/Road_Signs/Light.prefab"),
                fences = LoadMany(
                    UrbanRoot + "/Props/Other/Fence.prefab",
                    UrbanRoot + "/Props/Other/Barrier.prefab",
                    UrbanRoot + "/Props/Other/Road_Barrier.prefab"),
                vegetation = LoadMany(
                    NatureRoot + "/bush1.prefab",
                    NatureRoot + "/bush2.prefab",
                    NatureRoot + "/bush3.prefab",
                    NatureRoot + "/grass1.prefab",
                    NatureRoot + "/grass2.prefab",
                    NatureRoot + "/grass3.prefab",
                    NatureRoot + "/flower1-1.prefab",
                    NatureRoot + "/flower2-1.prefab",
                    NatureRoot + "/flower3-2.prefab"),
                roadsideProps = LoadMany(
                    AiRiRoot + "/BENCH.prefab",
                    UrbanRoot + "/Props/Other/Bus Stop.prefab",
                    UrbanRoot + "/Props/Boxes/Box_1.prefab",
                    UrbanRoot + "/Props/Boxes/Box_2.prefab"),
                roadSigns = LoadMany(
                    UrbanRoot + "/Props/Road_Signs/Road_Sign_1.prefab",
                    UrbanRoot + "/Props/Road_Signs/Road_Sign_2.prefab",
                    UrbanRoot + "/Props/Road_Signs/Big_Road_Sing_1.prefab",
                    AiRiRoot + "/sign board.prefab",
                    AiRiRoot + "/bill board.prefab"),
                skylineBuildings = LoadMany(
                    UrbanRoot + "/Buildings/Building_7.prefab",
                    UrbanRoot + "/Buildings/Building_8.prefab",
                    UrbanRoot + "/Buildings/Building_9.prefab",
                    UrbanRoot + "/Buildings/Building_5.prefab",
                    UrbanRoot + "/Buildings/Building_6.prefab")
            };

            return set;
        }

        public static GameObject LoadUrbanRoadPrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(UrbanRoot + "/Roads/Road_1.prefab");
        }

        private static GameObject[] LoadMany(params string[] paths)
        {
            var list = new List<GameObject>();
            for (int i = 0; i < paths.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (prefab != null)
                {
                    list.Add(prefab);
                }
                else
                {
                    Debug.LogWarning("[HighwayPrefabLoader] Missing prefab: " + paths[i]);
                }
            }

            return list.ToArray();
        }
    }
}
#endif
