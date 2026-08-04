using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Measures the live bus-stop asphalt span so traffic spawn/despawn follows scaled/duplicated road meshes.
    /// </summary>
    public static class Level1BusStopRoadExtents
    {
        public static float RoadStartX { get; private set; } = Level1BusStopRoadSettings.RoadStartX;
        public static float RoadEndX { get; private set; } = Level1BusStopRoadSettings.RoadEndX;

        public static float RoadLength => RoadEndX - RoadStartX;
        public static float RoadCenterX => (RoadStartX + RoadEndX) * 0.5f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            RoadStartX = Level1BusStopRoadSettings.RoadStartX;
            RoadEndX = Level1BusStopRoadSettings.RoadEndX;
        }

        public static void RefreshFromScene()
        {
            if (TryMeasureAsphalt(out float minX, out float maxX))
            {
                RoadStartX = minX;
                RoadEndX = maxX;
                return;
            }

            RoadStartX = Level1BusStopRoadSettings.RoadStartX;
            RoadEndX = Level1BusStopRoadSettings.RoadEndX;
        }

        public static bool TryMeasureAsphalt(out float minX, out float maxX)
        {
            minX = float.PositiveInfinity;
            maxX = float.NegativeInfinity;
            bool found = false;

            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.transform == null)
                {
                    continue;
                }

                if (!renderer.transform.name.StartsWith("AsphaltRoadSurface"))
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;
                minX = Mathf.Min(minX, bounds.min.x);
                maxX = Mathf.Max(maxX, bounds.max.x);
                found = true;
            }

            return found && maxX > minX + 1f;
        }
    }
}
