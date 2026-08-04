using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Follows behind the player at shoulder height for interior third-person scenes.
    /// </summary>
    public class ThirdPersonCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform facingSource;
        [SerializeField] private float followDistance = 3.2f;
        [SerializeField] private float followHeight = 1.55f;
        [SerializeField] private float lookAtHeight = 1.35f;
        [SerializeField] private float positionSmooth = 10f;
        [SerializeField] private float rotationSmooth = 12f;

        private float currentYaw;
        private bool yawInitialized;

        public Transform Target
        {
            get { return target; }
            set { target = value; }
        }

        public Transform FacingSource
        {
            get { return facingSource; }
            set { facingSource = value; }
        }

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
                    if (facingSource == null)
                    {
                        facingSource = player.visualRoot != null ? player.visualRoot : player.transform;
                    }
                }
            }

            if (target == null)
            {
                return;
            }

            Transform yawTransform = facingSource != null ? facingSource : target;
            float targetYaw = yawTransform.eulerAngles.y;
            if (!yawInitialized)
            {
                currentYaw = targetYaw;
                yawInitialized = true;
            }
            else
            {
                currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * rotationSmooth);
            }

            Quaternion orbitRotation = Quaternion.Euler(0f, currentYaw, 0f);
            Vector3 desiredPosition = target.position + orbitRotation * new Vector3(0f, followHeight, -followDistance);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionSmooth);

            Vector3 lookTarget = target.position + Vector3.up * lookAtHeight;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmooth);
        }
    }
}
