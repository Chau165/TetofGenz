using System.Collections;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Short camera shake on traffic impact.</summary>
    public class HighwayImpactShake : MonoBehaviour
    {
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private float magnitude = 0.42f;

        private Vector3 restLocalPos;
        private Coroutine shakeRoutine;

        private void Awake()
        {
            restLocalPos = transform.localPosition;
        }

        public void Play()
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float falloff = 1f - Mathf.Clamp01(elapsed / duration);
                Vector3 offset = Random.insideUnitSphere * magnitude * falloff;
                offset.z *= 0.35f;
                transform.localPosition = restLocalPos + offset;
                yield return null;
            }

            transform.localPosition = restLocalPos;
            shakeRoutine = null;
        }
    }
}
