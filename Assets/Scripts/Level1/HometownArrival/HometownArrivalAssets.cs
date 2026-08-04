using ChoTet.Level1;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    internal static class HometownArrivalAssets
    {
        private static GameObject cachedParentsCarPrefab;

        public static GameObject LoadParentsCarPrefab()
        {
            if (cachedParentsCarPrefab != null)
            {
                return cachedParentsCarPrefab;
            }

#if UNITY_EDITOR
            cachedParentsCarPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                Level1KenneyCarKitPaths.SedanPrefab);
#else
            cachedParentsCarPrefab = HometownArrivalBootstrap.LoadRuntimePrefab(
                Level1KenneyCarKitPaths.SedanPrefab);
#endif
            return cachedParentsCarPrefab;
        }

        public static GameObject LoadFallbackVehiclePrefab()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(Level1KenneyCarKitPaths.PickupTrafficPrefab);
#else
            return null;
#endif
        }
    }
}
