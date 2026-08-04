using ChoTet.Level1;
using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Parents' sedan — fixed model, color, and pose on the branch shoulder.</summary>
    internal static class HometownArrivalFamilyCar
    {
        public const string RootName = "ParentsCar";
        private const string VisualChildName = "FamilyCarVisual";

        public static Transform EnsureParked(Transform worldRoot)
        {
            Transform existing = GameObject.Find(RootName)?.transform;
            if (existing == null)
            {
                GameObject root = new GameObject(RootName);
                if (worldRoot != null)
                {
                    root.transform.SetParent(worldRoot, false);
                }

                existing = root.transform;
            }

            ApplyFixedPose(existing);
            ApplyVisual(existing, forceRebuild: false);
            return existing;
        }

        private static void ApplyFixedPose(Transform carRoot)
        {
            if (carRoot == null)
            {
                return;
            }

            carRoot.SetPositionAndRotation(
                new Vector3(
                    HometownArrivalLayout.ParentsCarWorldX,
                    HometownArrivalLayout.ParentsCarWorldY,
                    HometownArrivalLayout.ParentsCarWorldZ),
                Quaternion.Euler(0f, HometownArrivalLayout.ParentsCarYaw, 0f));
        }

        private static void ApplyVisual(Transform carRoot, bool forceRebuild)
        {
            if (carRoot == null)
            {
                return;
            }

            Transform existing = carRoot.Find(VisualChildName);
            if (existing != null && !forceRebuild)
            {
                return;
            }

            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
            }

            EnsureTrafficMaterialsConfigured();

            GameObject prefab = HometownArrivalAssets.LoadParentsCarPrefab();
            if (prefab == null)
            {
                Debug.LogWarning("[HometownArrival] Parents car prefab unavailable.");
                return;
            }

            GameObject visual = Object.Instantiate(prefab, carRoot);
            visual.name = VisualChildName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * HometownArrivalLayout.ParentsCarVisualScale;

            CenterVisualOnPivot(visual.transform);
            StripPhysics(visual);
            Level1TrafficVisualUtility.ApplyCarLook(
                visual,
                HometownArrivalLayout.ParentsCarBodyColorIndex,
                isPickup: false);
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
