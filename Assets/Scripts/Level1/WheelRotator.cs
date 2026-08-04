using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Rotates wheel transforms based on vehicle linear speed.</summary>
    public class WheelRotator : MonoBehaviour
    {
        [SerializeField] private float wheelRadius = 0.32f;
        [SerializeField] private Vector3 rotationAxis = Vector3.right;

        private readonly List<Transform> wheels = new List<Transform>();
        private float currentSpeed;
        private bool wheelsFound;

        public void Configure(float radius, Vector3 axis)
        {
            wheelRadius = Mathf.Max(0.05f, radius);
            rotationAxis = axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.right;
            EnsureWheelsFound();
        }

        public void SetSpeed(float speed)
        {
            currentSpeed = speed;
        }

        private void LateUpdate()
        {
            if (!EnsureWheelsFound() || wheelRadius <= 0.001f)
            {
                return;
            }

            if (Mathf.Abs(currentSpeed) <= 0.01f)
            {
                return;
            }

            float degrees = currentSpeed * Time.deltaTime / wheelRadius * Mathf.Rad2Deg;
            for (int i = 0; i < wheels.Count; i++)
            {
                Transform wheel = wheels[i];
                if (wheel != null)
                {
                    wheel.Rotate(rotationAxis, degrees, Space.Self);
                }
            }
        }

        private bool EnsureWheelsFound()
        {
            if (wheelsFound)
            {
                return wheels.Count > 0;
            }

            wheels.Clear();
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform)
                {
                    continue;
                }

                if (child.name.IndexOf("wheel", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || child.name.IndexOf("tire", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || child.name.IndexOf("tyre", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    wheels.Add(child);
                }
            }

            wheelsFound = true;
            return wheels.Count > 0;
        }

        public void ResetCache()
        {
            wheelsFound = false;
            wheels.Clear();
            currentSpeed = 0f;
        }
    }
}
