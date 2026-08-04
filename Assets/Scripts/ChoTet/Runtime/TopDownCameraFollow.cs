using UnityEngine;

namespace ChoTet.Runtime
{
    public class TopDownCameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 14f, -9f);
        public Vector3 lookOffset = new Vector3(0f, 1.4f, 0f);
        public float smoothSpeed = 5f;

        private void LateUpdate()
        {
            if (FirstPersonCameraController.IsFirstPersonActive)
            {
                return;
            }

            if (target == null)
            {
                PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.LookAt(target.position + lookOffset);
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            transform.position = target.position + offset;
            transform.LookAt(target.position + lookOffset);
        }
    }
}
