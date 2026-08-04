using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>
    /// Legacy entry point — applies full-world backdrop from garden hook.
    /// </summary>
    public static class HometownArrivalGardenDressing
    {
        public static void Apply(Transform gardenAreaRoot)
        {
            Transform world = gardenAreaRoot != null ? gardenAreaRoot.parent : null;
            if (world == null)
            {
                world = GameObject.Find("ChoTetWorld")?.transform;
            }

            if (world != null)
            {
                ChoTetWorldBackdropDressing.Apply(world);
            }
        }
    }
}
