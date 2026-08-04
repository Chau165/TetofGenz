using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Scrolls road deck and countryside belts together with the vehicle.</summary>
    [DefaultExecutionOrder(-100)]
    public class HometownArrivalInfiniteScroll : MonoBehaviour
    {
        private Transform followTarget;
        private HighwayRoadSegmentPool roadPool;
        private HometownArrivalCountrysideRoadsidePool countrysidePool;

        public void Bind(Transform vehicle, Transform approachRoot)
        {
            followTarget = vehicle;
            if (approachRoot == null)
            {
                return;
            }

            roadPool = approachRoot.GetComponentInChildren<HighwayRoadSegmentPool>(true);
            countrysidePool = approachRoot.GetComponentInChildren<HometownArrivalCountrysideRoadsidePool>(true);

            if (followTarget != null)
            {
                float scrollZ = followTarget.position.z;
                roadPool?.SetScrollOffset(scrollZ);
                countrysidePool?.SetScrollOffset(scrollZ);
            }
        }

        private void LateUpdate()
        {
            if (followTarget == null || roadPool == null)
            {
                return;
            }

            float scrollZ = followTarget.position.z;
            roadPool.SetScrollOffset(scrollZ);
            countrysidePool?.SetScrollOffset(scrollZ);
        }
    }
}
