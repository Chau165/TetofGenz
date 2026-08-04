using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1.Highway
{
    /// <summary>Elevated chase camera — keeps the full car visible while looking ahead.</summary>
    public class HighwayFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 driverLocalOffset = new Vector3(0f, 4.8f, -2.05f);
            // Less pitch so sky stays above and water sits clearly below the canopy band.
            [SerializeField] private float pitchAngle = 32f;
        [SerializeField] private float fieldOfView = 74f;
        [SerializeField] private float positionSmooth = 14f;
        [SerializeField] private float rotationSmooth = 16f;

        private Camera cam;
        private HighwayPlayerVehicle playerVehicle;

        public static readonly Color HighwaySkyColor = new Color(0.53f, 0.78f, 0.97f, 1f);

        private void Awake()
        {
            FirstPersonCameraController firstPerson = GetComponent<FirstPersonCameraController>();
            if (firstPerson != null)
            {
                firstPerson.enabled = false;
                Destroy(firstPerson);
            }
        }

        public void Bind(Transform target)
        {
            followTarget = target;
            playerVehicle = target != null ? target.GetComponent<HighwayPlayerVehicle>() : null;
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = gameObject.AddComponent<Camera>();
            }

            // Skybox = blue sky + sun; water mesh below is separate (not solid clear-color).
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = HighwaySkyColor;
            cam.farClipPlane = 380f;
            cam.nearClipPlane = 0.12f;

            ApplyDriverViewForVehicle();
            cam.fieldOfView = fieldOfView;
            if (GetComponent<AudioListener>() == null)
            {
                gameObject.AddComponent<AudioListener>();
            }

            playerVehicle?.SetBodyVisible(true);
            playerVehicle?.RestoreDriverBodyCulling();
            Snap();
        }

        private void OnDisable()
        {
            playerVehicle?.RestoreDriverBodyCulling();
        }

        private void ApplyDriverViewForVehicle()
        {
            if (playerVehicle != null && playerVehicle.TryGetDriverView(out Vector3 offset, out float pitch, out float fov))
            {
                driverLocalOffset = offset;
                pitchAngle = pitch;
                fieldOfView = fov;
                return;
            }

            driverLocalOffset = new Vector3(0f, 5.4f, -2.6f);
            pitchAngle = 32f;
            fieldOfView = 74f;
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            Vector3 desiredPos = followTarget.TransformPoint(driverLocalOffset);
            transform.position = Vector3.Lerp(transform.position, desiredPos,
                1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

            Quaternion desiredRot = followTarget.rotation * Quaternion.Euler(pitchAngle, 0f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot,
                1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));
        }

        public void Snap()
        {
            if (followTarget == null)
            {
                return;
            }

            transform.position = followTarget.TransformPoint(driverLocalOffset);
            transform.rotation = followTarget.rotation * Quaternion.Euler(pitchAngle, 0f, 0f);
        }
    }
}