using UnityEngine;

namespace ChoTet.Runtime
{
    public class SweepingAnimation : MonoBehaviour
    {
        public Transform broom;
        public Transform visual;

        private float timer;
        private Vector3 initialBroomPos;
        private Quaternion initialBroomRot;
        private Quaternion initialVisualRot;

        private void Start()
        {
            if (broom != null)
            {
                initialBroomPos = broom.localPosition;
                initialBroomRot = broom.localRotation;
            }

            if (visual != null)
            {
                initialVisualRot = visual.localRotation;
            }

            timer = Random.Range(0f, 2f * Mathf.PI);
        }

        private void Update()
        {
            timer += Time.deltaTime * 3.5f;
            float wave = Mathf.Sin(timer);

            if (visual != null)
            {
                visual.localRotation = initialVisualRot * Quaternion.Euler(0f, wave * 8f, wave * 3f);
            }

            if (broom != null)
            {
                Vector3 sweepOffset = new Vector3(wave * 0.12f, 0f, Mathf.Abs(wave) * -0.04f);
                broom.localPosition = initialBroomPos + sweepOffset;
                broom.localRotation = initialBroomRot * Quaternion.Euler(wave * 12f, wave * -8f, 0f);
            }
        }
    }
}
