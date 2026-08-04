using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Invisible walk floor under highway lanes so the player does not fall through asphalt.</summary>
    public static class Level1OutdoorRoadWalkFloor
    {
        public const string RootName = "BusStopRoadWalkFloor";

        public static void EnsureInScene()
        {
            Transform layout = FindNamed("BusStopRoadLayout");
            Transform outdoor = FindNamed("Level1_OutdoorArea") ?? FindNamed("OutdoorArea");
            Transform parent = layout != null ? layout : outdoor;
            if (parent != null)
            {
                EnsureUnder(parent);
            }
        }

        public static void EnsureUnder(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            Transform existing = parent.Find(RootName);
            if (existing != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(existing.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(existing.gameObject);
                }
            }

            GameObject root = new GameObject(RootName);
            root.transform.SetParent(parent, false);

            float halfX = Level1BusStopRoadSettings.PlayableHalfX + 2.5f;
            float minZ = Level1BusStopRoadSettings.LaneNorthEdgeZ - 0.35f;
            float maxZ = Level1BusStopRoadSettings.SidewalkNearZ + 0.15f;
            float depth = maxZ - minZ;
            float y = Level1OutdoorSettings.WalkSurfaceY
                      - Level1OutdoorMovementSettings.WalkFloorThickness * 0.5f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "RoadLaneWalkFloor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.position = new Vector3(0f, y, (minZ + maxZ) * 0.5f);
            floor.transform.localScale = new Vector3(halfX * 2f,
                Level1OutdoorMovementSettings.WalkFloorThickness, depth);

            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(renderer);
                }
                else
                {
                    Object.DestroyImmediate(renderer);
                }
            }
        }

        private static Transform FindNamed(string objectName)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || go.name != objectName || !go.scene.IsValid() || !go.scene.isLoaded)
                {
                    continue;
                }

                return go.transform;
            }

            return null;
        }
    }
}
