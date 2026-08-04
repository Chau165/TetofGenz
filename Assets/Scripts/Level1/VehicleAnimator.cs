using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Visual attachment for traffic vehicles — no movement, no physics, no placeholder shadows.</summary>
    public class VehicleAnimator : MonoBehaviour
    {
        private Transform visualWrapper;
        private Transform visualRoot;
        private WheelRotator wheelRotator;
        private VehicleTurnSignals turnSignals;
        private GameObject visualPrefabSource;
        private TrafficPool ownerPool;

        public void BindPool(TrafficPool pool)
        {
            ownerPool = pool;
        }

        public void AttachVisual(
            GameObject prefab,
            float scale,
            Level1TrafficVehicleProfile profile)
        {
            ReleaseVisual();

            visualPrefabSource = prefab;
            if (prefab == null || ownerPool == null)
            {
                return;
            }

            visualWrapper = new GameObject("Visual").transform;
            visualWrapper.SetParent(transform, false);
            visualWrapper.localPosition = Vector3.zero;
            visualWrapper.localRotation = Quaternion.Euler(0f, profile.modelRotationOffsetY, 0f);
            visualWrapper.localScale = Vector3.one;

            visualRoot = ownerPool.AcquireVisual(prefab, visualWrapper);
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.Euler(profile.visualLocalEulerOffset);
            visualRoot.localScale = Vector3.one * scale;

            StripPhysicsAndColliders(visualRoot.gameObject);
            if (profile.flattenVisualTilt)
            {
                FlattenVisualTilt(visualRoot);
            }

            Level1TrafficVisualUtility.ApplyTransportLook(visualRoot.gameObject, profile);

            if (profile.enableWheelSpin)
            {
                wheelRotator = visualRoot.GetComponent<WheelRotator>();
                if (wheelRotator == null)
                {
                    wheelRotator = visualRoot.gameObject.AddComponent<WheelRotator>();
                }

                Vector3 spinAxis = profile.wheelSpinAxis.sqrMagnitude > 0.0001f
                    ? profile.wheelSpinAxis
                    : Vector3.up;
                wheelRotator.Configure(0.32f, spinAxis);
                wheelRotator.ResetCache();
            }

            if (turnSignals == null)
            {
                turnSignals = gameObject.GetComponent<VehicleTurnSignals>();
                if (turnSignals == null)
                {
                    turnSignals = gameObject.AddComponent<VehicleTurnSignals>();
                }
            }

            turnSignals.Build(visualWrapper, profile.scale);
        }

        public void SetTurnSignal(VehicleTurnSignalSide side)
        {
            turnSignals?.SetSide(side);
        }

        public void SetWheelSpeed(float speed)
        {
            wheelRotator?.SetSpeed(speed);
        }

        public void ReleaseVisual()
        {
            if (visualRoot != null && ownerPool != null && visualPrefabSource != null)
            {
                ownerPool.ReturnVisual(visualRoot, visualPrefabSource);
            }
            else if (visualRoot != null)
            {
                Destroy(visualRoot.gameObject);
            }

            visualRoot = null;
            visualPrefabSource = null;
            wheelRotator = null;
            turnSignals?.Clear();

            if (visualWrapper != null)
            {
                Destroy(visualWrapper.gameObject);
                visualWrapper = null;
            }
        }

        public void ResetForPool()
        {
            ReleaseVisual();
        }

        private static void StripPhysicsAndColliders(GameObject root)
        {
            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Destroy(bodies[i]);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Destroy(colliders[i]);
            }
        }

        private static void FlattenVisualTilt(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                FlattenVisualTiltRecursive(root.GetChild(i));
            }
        }

        private static void FlattenVisualTiltRecursive(Transform node)
        {
            Vector3 euler = node.localEulerAngles;
            node.localRotation = Quaternion.Euler(0f, euler.y, 0f);

            for (int i = 0; i < node.childCount; i++)
            {
                FlattenVisualTiltRecursive(node.GetChild(i));
            }
        }
    }
}
