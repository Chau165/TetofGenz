using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Simple spinning marker at the bus pickup interact point.</summary>
    public class BusPickupMarker : MonoBehaviour
    {
        [SerializeField] private float spinSpeed = 42f;
        [SerializeField] private float pulseAmplitude = 0.07f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
            float pulse = 1f + Mathf.Sin(Time.time * 2.6f) * pulseAmplitude;
            transform.localScale = baseScale * pulse;
        }
    }

}
