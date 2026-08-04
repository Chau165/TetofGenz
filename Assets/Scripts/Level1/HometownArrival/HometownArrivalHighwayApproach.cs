using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Arrival road deck plus recycling countryside belts synced to road chunks.</summary>
    [DefaultExecutionOrder(-300)]
    public class HometownArrivalHighwayApproach : MonoBehaviour
    {
        [SerializeField] private float anchorWorldZ = 58f;
        [SerializeField] private float pathStopWorldZ = HometownArrivalLayout.BusStopZ;

        private void Awake()
        {
            if (!HometownArrivalBootstrap.IsArrivalScene(gameObject.scene.name))
            {
                enabled = false;
                return;
            }

            BuildApproach();
        }

        public static void EnsureBuilt(Transform parent, float stopWorldZ = 30f)
        {
            if (parent == null)
            {
                return;
            }

            HometownArrivalHighwayApproach existing = parent.GetComponentInChildren<HometownArrivalHighwayApproach>(true);
            if (existing != null)
            {
                existing.SetPathStopWorldZ(stopWorldZ);
                existing.BuildApproach();
                return;
            }

            GameObject host = new GameObject("HometownHighwayApproach");
            host.transform.SetParent(parent, false);
            HometownArrivalHighwayApproach approach = host.AddComponent<HometownArrivalHighwayApproach>();
            approach.SetPathStopWorldZ(stopWorldZ);
            approach.BuildApproach();
        }

        public void SetPathStopWorldZ(float stopWorldZ)
        {
            pathStopWorldZ = stopWorldZ;
        }

        public void BuildApproach()
        {
            DestroyChild("ApproachBaseFill");
            DestroyChild("ArrivalHighwayRoad");
            DestroyChild("ArrivalSideGround");
            DestroyChild("ArrivalForestBelt");
            DestroyChild("ArrivalCountryside");
            DestroyChild("ArrivalCountrysidePool");
            DestroyChild("ArrivalParentPath");
            DestroyChild("ArrivalBranchJunction");

            HometownArrivalApproachGround.EnsureBaseFill(transform);
            BuildRoadDeck();
            HometownArrivalBranchRoadJunction.PrepareForCountrysideSpawn(pathStopWorldZ);
            BuildCountrysidePool();
            HometownArrivalBranchRoadJunction.Build(transform, pathStopWorldZ);
        }

        private void DestroyChild(string childName)
        {
            Transform existing = transform.Find(childName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }
        }

        private void BuildRoadDeck()
        {
            GameObject roadRoot = new GameObject("ArrivalHighwayRoad");
            roadRoot.transform.SetParent(transform, false);

            HighwayRoadSegmentPool pool = roadRoot.AddComponent<HighwayRoadSegmentPool>();
            pool.Initialize(
                null,
                LoadMat(HighwayRoadMaterialDefaults.LaneMarkFile),
                LoadMat(HighwayRoadMaterialDefaults.EdgeMarkFile),
                LoadMat(HighwayRoadMaterialDefaults.RoadSurfaceFile),
                LoadMat(HighwayRoadMaterialDefaults.ShoulderFile));
            pool.RebuildRoadContent();
            pool.SetShouldersVisible(false);
            pool.SetScrollOffset(anchorWorldZ);
        }

        private void BuildCountrysidePool()
        {
            GameObject decorRoot = new GameObject("ArrivalCountrysidePool");
            decorRoot.transform.SetParent(transform, false);

            HometownArrivalCountrysideRoadsidePool pool = decorRoot.AddComponent<HometownArrivalCountrysideRoadsidePool>();
            pool.RebuildContent();
            pool.SetScrollOffset(anchorWorldZ);
        }

        private static Material LoadMat(string fileName)
        {
#if UNITY_EDITOR
            return HighwayRoadMaterialDefaults.Load(fileName)
                ?? HometownArrivalResourceCache.LoadMaterial(fileName);
#else
            return HometownArrivalResourceCache.LoadMaterial(fileName);
#endif
        }
    }
}
