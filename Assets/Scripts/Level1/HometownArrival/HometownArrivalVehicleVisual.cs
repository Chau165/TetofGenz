using ChoTet.Level1;
using ChoTet.Level1.Highway;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Applies the same boarded-vehicle look as the highway mini-game.</summary>
    public static class HometownArrivalVehicleVisual
    {
        private const float HighwayVisualYaw = 0f;
        private const string VisualChildName = "BusVisual";

        public static void Apply(Transform busRoot, bool forceRebuild = false)
        {
            if (busRoot == null)
            {
                return;
            }

            Transform existing = busRoot.Find(VisualChildName);
            if (existing != null && !forceRebuild)
            {
                return;
            }

            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }

            EnsureTrafficMaterialsConfigured();

            GameObject prefab = ResolveVehiclePrefab();
            if (prefab == null)
            {
                return;
            }

            float scale = HometownArrivalContext.HasVehicleData ? HometownArrivalContext.VehicleScale : 1.55f;
            int bodyColor = HometownArrivalContext.BodyColorIndex;
            float rotationY = HometownArrivalContext.ModelRotationOffsetY;

            GameObject visual = Object.Instantiate(prefab, busRoot);
            visual.name = VisualChildName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, HighwayVisualYaw + rotationY, 0f);
            visual.transform.localScale = Vector3.one * scale;

            CenterVisualOnPivot(visual.transform);
            StripPhysics(visual);
            Level1TrafficVisualUtility.ApplyCarLook(visual, bodyColor, isPickup: true);

            WheelRotator wheels = visual.GetComponent<WheelRotator>();
            if (wheels == null)
            {
                wheels = visual.AddComponent<WheelRotator>();
            }

            wheels.Configure(0.32f, Vector3.right);
            wheels.ResetCache();
            wheels.SetSpeed(12f);
        }

        private static GameObject ResolveVehiclePrefab()
        {
            if (HometownArrivalContext.HasVehicleData && HometownArrivalContext.VehiclePrefab != null)
            {
                return HometownArrivalContext.VehiclePrefab;
            }

            return HometownArrivalAssets.LoadFallbackVehiclePrefab();
        }

        private static void EnsureTrafficMaterialsConfigured()
        {
            Level1TrafficVisualBootstrap.EnsureConfigured();
        }

        private static void CenterVisualOnPivot(Transform visualRoot)
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                visualRoot.localPosition = Vector3.zero;
                return;
            }

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < renderers.Length; i++)
            {
                sum += visualRoot.InverseTransformPoint(renderers[i].bounds.center);
            }

            Vector3 localCenter = sum / renderers.Length;
            visualRoot.localPosition = new Vector3(-localCenter.x, 0f, 0f);
        }

        private static void StripPhysics(GameObject root)
        {
            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Object.Destroy(bodies[i]);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.Destroy(colliders[i]);
            }
        }
    }
}