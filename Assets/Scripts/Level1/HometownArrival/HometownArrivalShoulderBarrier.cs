using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Removes the gray roadside wall that blocked the parents reunion shot.</summary>
    internal static class HometownArrivalShoulderBarrier
    {
        public const string RootName = "ParentsRoadsideBarrier";

        public static void Ensure(Transform worldRoot)
        {
            Remove();
        }

        public static void Remove()
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || go.name != RootName)
                {
                    continue;
                }

                if (!go.scene.IsValid() || !go.scene.isLoaded)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(go);
                }
                else
                {
                    Object.DestroyImmediate(go);
                }
            }
        }
    }
}
