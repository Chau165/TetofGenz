using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Slow bob and drift for flowers, coins, and small festive props.
    /// </summary>
    public class FloatingDecoration : MonoBehaviour
    {
        [SerializeField] private float bobAmplitude = 0.04f;
        [SerializeField] private float bobSpeed = 0.22f;
        [SerializeField] private float driftAmplitude = 0.03f;
        [SerializeField] private float driftSpeed = 0.11f;
        [SerializeField] private float phaseOffset;

        private Vector3 baseLocalPosition;

        public void Configure(float bobAmp, float bobSpd, float driftAmp, float driftSpd, float phase)
        {
            bobAmplitude = bobAmp;
            bobSpeed = bobSpd;
            driftAmplitude = driftAmp;
            driftSpeed = driftSpd;
            phaseOffset = phase;
        }

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            float t = Time.unscaledTime + phaseOffset;
            float y = Mathf.Sin(t * bobSpeed * Mathf.PI * 2f) * bobAmplitude;
            float x = Mathf.Sin(t * driftSpeed * Mathf.PI * 2f + 0.7f) * driftAmplitude;
            transform.localPosition = baseLocalPosition + new Vector3(x, y, 0f);
        }
    }
}
