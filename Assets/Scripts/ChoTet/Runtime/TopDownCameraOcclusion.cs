using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Hides tall outdoor props that sit between the camera and the follow target.
    /// Attach to the gameplay camera; pair occluders with <see cref="CameraViewOccluder"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class TopDownCameraOcclusion : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float targetHeightOffset = 0.55f;
        [SerializeField] private float rayRadius = 0.42f;
        [SerializeField] private bool active = true;

        private readonly HashSet<CameraViewOccluder> blocking = new HashSet<CameraViewOccluder>();
        private readonly HashSet<CameraViewOccluder> frameHits = new HashSet<CameraViewOccluder>();
        private readonly List<CameraViewOccluder> restoreBuffer = new List<CameraViewOccluder>(32);
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[32];

        private void LateUpdate()
        {
            if (!active)
            {
                ClearAllBlocking();
                return;
            }

            if (target == null)
            {
                ResolveTarget();
            }

            if (target == null)
            {
                ClearAllBlocking();
                return;
            }

            Vector3 origin = transform.position;
            Vector3 targetPoint = target.position + Vector3.up * targetHeightOffset;
            Vector3 delta = targetPoint - origin;
            float distance = delta.magnitude;
            if (distance < 0.05f)
            {
                return;
            }

            frameHits.Clear();
            int hitCount = Physics.SphereCastNonAlloc(origin, rayRadius, delta.normalized, HitBuffer, distance);
            for (int i = 0; i < hitCount; i++)
            {
                CameraViewOccluder occluder = HitBuffer[i].collider.GetComponentInParent<CameraViewOccluder>();
                if (occluder != null && occluder.IsRegistered)
                {
                    frameHits.Add(occluder);
                }
            }

            restoreBuffer.Clear();
            foreach (CameraViewOccluder occluder in blocking)
            {
                if (!frameHits.Contains(occluder))
                {
                    restoreBuffer.Add(occluder);
                }
            }

            for (int i = 0; i < restoreBuffer.Count; i++)
            {
                restoreBuffer[i].SetTargetStrength(0f);
                blocking.Remove(restoreBuffer[i]);
            }

            foreach (CameraViewOccluder occluder in frameHits)
            {
                occluder.SetTargetStrength(1f);
                blocking.Add(occluder);
            }
        }

        public void SetActive(bool enabled)
        {
            active = enabled;
            if (!active)
            {
                ClearAllBlocking();
            }
        }

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void ResolveTarget()
        {
            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        private void ClearAllBlocking()
        {
            foreach (CameraViewOccluder occluder in blocking)
            {
                if (occluder != null)
                {
                    occluder.SetTargetStrength(0f);
                }
            }

            blocking.Clear();
        }

        private void OnDisable()
        {
            ClearAllBlocking();
        }
    }
}
