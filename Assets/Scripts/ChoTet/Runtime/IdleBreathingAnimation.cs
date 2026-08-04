using UnityEngine;

namespace ChoTet.Runtime
{
    public class IdleBreathingAnimation : MonoBehaviour
    {
        public Transform visual;

        private float timer;
        private Vector3 initialScale;
        private Quaternion initialRotation;

        private void Start()
        {
            if (visual != null)
            {
                initialScale = visual.localScale;
                initialRotation = visual.localRotation;
            }

            timer = Random.Range(0f, 2f * Mathf.PI);
        }

        private void Update()
        {
            timer += Time.deltaTime * 2f;
            float wave = Mathf.Sin(timer);

            if (visual == null)
            {
                return;
            }

            visual.localScale = new Vector3(initialScale.x, initialScale.y + (wave * 0.015f), initialScale.z);
            visual.localRotation = initialRotation * Quaternion.Euler(wave * 1.5f, 0f, 0f);
        }
    }
}
