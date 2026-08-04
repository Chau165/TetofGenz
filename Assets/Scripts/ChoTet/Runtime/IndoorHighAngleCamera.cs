using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// High-angle indoor camera that follows the player while keeping a fixed yaw/offset.
    /// Position is clamped inside the room so the view stays on interior space.
    /// </summary>
    [ExecuteAlways]
    public class IndoorHighAngleCamera : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private float cameraHeight = 6f;
        [SerializeField] private float forwardOffset = 5f;
        [SerializeField] private float lookAtHeight = 0.55f;
        [SerializeField] private float yawAngle = 0f;
        [SerializeField] private float smoothSpeed = 7f;
        [SerializeField] private bool clampToRoom = true;
        [SerializeField] private bool followTargetFacing = false;
        [SerializeField] private float roomHalfWidth = 5.5f;
        [SerializeField] private float roomHalfDepth = 4.5f;
        [SerializeField] private float roomClampMargin = 0.45f;

        // Optional rectangular bounds (used for Level 1 outdoor street path).
        private bool useRectBounds;
        private float boundsMinX;
        private float boundsMaxX;
        private float boundsMinZ;
        private float boundsMaxZ;
        private float lookAheadBlend;
        private float lookAheadTargetHeight;
        private float lookAheadTargetForward;
        private float lookAheadTargetLookHeight;
        private float lookAheadTargetLookForward;

        private Vector3 initialLookTarget;

        private void OnEnable()
        {
            initialLookTarget = GetFocusPoint();
            ApplyTransform(initialLookTarget, snap: true);
        }

        public void Configure(Vector3 target, float height, float forward, float yaw, float lookHeight = 0.55f)
        {
            initialLookTarget = target;
            cameraHeight = height;
            forwardOffset = forward;
            yawAngle = yaw;
            lookAtHeight = lookHeight;
            ApplyTransform(initialLookTarget, snap: true);
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        public void SetFollowTargetFacing(bool enabled)
        {
            followTargetFacing = enabled;
        }

        /// <summary>Snap camera to the current follow target (e.g. after a teleport).</summary>
        public void SnapToFollowTarget()
        {
            if (followTarget == null)
            {
                return;
            }

            ApplyTransform(GetFocusPoint(), snap: true);
        }

        public void SetRoomBounds(float halfWidth, float halfDepth, float clampMargin)
        {
            useRectBounds = false;
            roomHalfWidth = halfWidth;
            roomHalfDepth = halfDepth;
            roomClampMargin = clampMargin;
            clampToRoom = true;
        }

        /// <summary>Clamp the camera focus inside an axis-aligned outdoor path corridor.</summary>
        public void SetRectBounds(float minX, float maxX, float minZ, float maxZ, float clampMargin)
        {
            useRectBounds = true;
            boundsMinX = minX;
            boundsMaxX = maxX;
            boundsMinZ = minZ;
            boundsMaxZ = maxZ;
            roomClampMargin = clampMargin;
            clampToRoom = true;
        }

        public void SetLookAheadRig(
            float blend,
            float aheadHeight,
            float aheadForward,
            float aheadLookHeight,
            float aheadLookForward)
        {
            lookAheadBlend = Mathf.Clamp01(blend);
            lookAheadTargetHeight = aheadHeight;
            lookAheadTargetForward = aheadForward;
            lookAheadTargetLookHeight = aheadLookHeight;
            lookAheadTargetLookForward = aheadLookForward;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || FirstPersonCameraController.IsFirstPersonActive)
            {
                return;
            }

            ResolveFollowTarget();
            if (followTarget == null)
            {
                return;
            }

            ApplyTransform(GetFocusPoint(), snap: false);
        }

        private void ResolveFollowTarget()
        {
            if (followTarget != null)
            {
                return;
            }

            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            if (player != null)
            {
                followTarget = player.transform;
            }
        }

        private Vector3 GetFocusPoint()
        {
            if (followTarget != null)
            {
                Vector3 focus = followTarget.position;
                focus.y = 0f;
                return clampToRoom ? ClampFocus(focus) : focus;
            }

            return initialLookTarget;
        }

        private void ApplyTransform(Vector3 focusPoint, bool snap)
        {
            float effectiveHeight = Mathf.Lerp(cameraHeight, lookAheadTargetHeight, lookAheadBlend);
            float effectiveForward = Mathf.Lerp(forwardOffset, lookAheadTargetForward, lookAheadBlend);
            float effectiveLookHeight = Mathf.Lerp(lookAtHeight, lookAheadTargetLookHeight, lookAheadBlend);

            Vector3 horizontalOffset;
            if (followTargetFacing && followTarget != null)
            {
                Vector3 forward = followTarget.forward;

                forward.y = 0f;
                if (forward.sqrMagnitude > 0.0001f)
                {
                    forward.Normalize();
                    horizontalOffset = -forward * effectiveForward;
                }
                else
                {
                    Quaternion yawRotation = Quaternion.Euler(0f, yawAngle, 0f);
                    horizontalOffset = yawRotation * new Vector3(0f, 0f, effectiveForward);
                }
            }
            else
            {
                Quaternion yawRotation = Quaternion.Euler(0f, yawAngle, 0f);
                horizontalOffset = yawRotation * new Vector3(0f, 0f, effectiveForward);
            }

            Vector3 desiredPosition = focusPoint + horizontalOffset + Vector3.up * effectiveHeight;
            if (clampToRoom)
            {
                desiredPosition = ClampCameraPosition(desiredPosition);
            }

            if (snap)
            {
                transform.position = desiredPosition;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            }

            Vector3 lookTarget = focusPoint + Vector3.up * effectiveLookHeight;
            if (lookAheadBlend > 0.001f && lookAheadTargetLookForward > 0.01f)
            {
                Quaternion yawRotation = Quaternion.Euler(0f, yawAngle, 0f);
                lookTarget += yawRotation * new Vector3(0f, 0f, lookAheadTargetLookForward * lookAheadBlend);
            }

            transform.LookAt(lookTarget);
        }

        private Vector3 ClampFocus(Vector3 focus)
        {
            if (useRectBounds)
            {
                float margin = roomClampMargin;
                focus.x = Mathf.Clamp(focus.x, boundsMinX + margin, boundsMaxX - margin);
                focus.z = Mathf.Clamp(focus.z, boundsMinZ + margin, boundsMaxZ - margin);
                return focus;
            }

            float maxX = roomHalfWidth - roomClampMargin;
            float maxZ = roomHalfDepth - roomClampMargin;
            focus.x = Mathf.Clamp(focus.x, -maxX, maxX);
            focus.z = Mathf.Clamp(focus.z, -maxZ, maxZ);
            return focus;
        }

        private Vector3 ClampCameraPosition(Vector3 position)
        {
            if (useRectBounds)
            {
                float margin = roomClampMargin * 0.35f;
                position.x = Mathf.Clamp(position.x, boundsMinX + margin, boundsMaxX - margin);
                position.z = Mathf.Clamp(position.z, boundsMinZ + margin, boundsMaxZ - margin);
                return position;
            }

            float maxX = roomHalfWidth - roomClampMargin * 0.35f;
            float maxZ = roomHalfDepth - roomClampMargin * 0.35f;
            position.x = Mathf.Clamp(position.x, -maxX, maxX);
            position.z = Mathf.Clamp(position.z, -maxZ, maxZ);
            return position;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            ApplyTransform(initialLookTarget, snap: true);
        }
#endif
    }
}
