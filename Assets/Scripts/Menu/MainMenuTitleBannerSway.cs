using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Gentle sine sway for frame vine foliage on the title banner.
    /// </summary>
    public class MainMenuTitleBannerSway : MonoBehaviour
    {
        [SerializeField] private float amplitudeDegrees = 1.2f;
        [SerializeField] private float speed = 0.16f;
        [SerializeField] private float phaseOffset;
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;

        private Quaternion baseLocalRotation;

        public void Configure(Vector3 axis, float swaySpeed, float amplitude, float phase)
        {
            rotationAxis = axis;
            speed = swaySpeed;
            amplitudeDegrees = amplitude;
            phaseOffset = phase;
        }

        private void Awake()
        {
            baseLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            float angle = Mathf.Sin((Time.unscaledTime + phaseOffset) * speed * Mathf.PI * 2f) * amplitudeDegrees;
            transform.localRotation = baseLocalRotation * Quaternion.AngleAxis(angle, rotationAxis.normalized);
        }
    }
}
