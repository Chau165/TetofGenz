using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Removes the temporary people/road barrier. Players may enter the highway lanes
    /// (with lives + car-hit rules) instead of being blocked by a wall.
    /// </summary>
    public static class Level1BusStopRoadsideBarrier
    {
        public const string RootName = "BusStopRoadsidePeopleBarrier";

        public static void EnsureInScene()
        {
            RemoveFromScene();
            Level1OutdoorRoadWalkFloor.EnsureInScene();
        }

        public static void EnsureUnder(Transform parent)
        {
            // Builder hook — never recreate the wall; only clean up if present.
            if (parent == null)
            {
                return;
            }

            Transform existing = parent.Find(RootName);
            if (existing != null)
            {
                DestroyGo(existing.gameObject);
            }

            SealRemoveSouthBlockers(parent);
            Level1OutdoorRoadWalkFloor.EnsureUnder(parent);
        }

        public static void RemoveFromScene()
        {
            DestroyNamedEverywhere(RootName);
            Transform layout = FindNamedTransformIncludingInactive("BusStopRoadLayout");
            if (layout != null)
            {
                SealRemoveSouthBlockers(layout);
            }
        }

        private static void SealRemoveSouthBlockers(Transform roadLayoutParent)
        {
            Transform bounds = FindDeep(roadLayoutParent, "BusStopRoadBoundaries");
            if (bounds == null)
            {
                return;
            }

            for (int i = bounds.childCount - 1; i >= 0; i--)
            {
                Transform child = bounds.GetChild(i);
                if (child != null && child.name.StartsWith("RoadEdgeBlockerS", System.StringComparison.Ordinal))
                {
                    DestroyGo(child.gameObject);
                }
            }
        }

        private static void DestroyNamedEverywhere(string objectName)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || go.name != objectName || !go.scene.IsValid() || !go.scene.isLoaded)
                {
                    continue;
                }

                DestroyGo(go);
            }
        }

        private static Transform FindNamedTransformIncludingInactive(string objectName)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || go.name != objectName)
                {
                    continue;
                }

                if (!go.scene.IsValid() || !go.scene.isLoaded)
                {
                    continue;
                }

                return go.transform;
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void DestroyGo(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
