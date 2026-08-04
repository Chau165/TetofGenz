using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Idle wind motion for grass tufts and low vegetation.</summary>
    public class HighwayNatureWindSway : MonoBehaviour
    {
        [SerializeField] private float swayAngle = 6f;
        [SerializeField] private float swaySpeed = 1.05f;
        [SerializeField] private float phaseOffset;

        private Quaternion baseRotation;

        public void ConfigureForGrass(int seed)
        {
            swayAngle = 5.5f + (seed % 5) * 1.2f;
            swaySpeed = 0.9f + (seed % 7) * 0.08f;
            phaseOffset = seed * 0.31f;
        }

        public void ConfigureForCluster(int seed)
        {
            swayAngle = 4f + (seed % 4);
            swaySpeed = 0.75f + (seed % 5) * 0.06f;
            phaseOffset = seed * 0.23f;
        }

        private void Awake()
        {
            baseRotation = transform.localRotation;
            if (phaseOffset == 0f)
            {
                phaseOffset = (transform.position.x + transform.position.z) * 0.17f;
            }
        }

        private void Update()
        {
            float t = Time.time * swaySpeed + phaseOffset;
            float x = Mathf.Sin(t) * swayAngle;
            float z = Mathf.Cos(t * 0.73f) * swayAngle * 0.6f;
            transform.localRotation = baseRotation * Quaternion.Euler(x, 0f, z);
        }
    }
}
