using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChoTet.Level1
{
    /// <summary>
    /// Limited orbit camera for the Level 1 outdoor street (boarding-house exit) zone only.
    /// Replaces <see cref="IndoorHighAngleCamera"/> while active.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class BoardingHouseStreetOrbitCamera : MonoBehaviour
    {
        private enum OrbitState
        {
            Inactive,
            BlendingIn,
            Active,
            BlendingOut
        }

        [Header("Zone")]
        [SerializeField] private bool zoneOnlyEnabled = true;

        [Header("Orbit Rig")]
        [SerializeField] private float orbitHorizontalOffset = 5.2f;
        [SerializeField] private float orbitHeight = 6.8f;
        [SerializeField] private float lookAtHeight = 0.55f;
        [SerializeField] private float defaultYaw = 0f;
        [SerializeField] private float minYaw = -78f;
        [SerializeField] private float maxYaw = 68f;
        [SerializeField] private float orbitRotateSpeed = 92f;
        [SerializeField] private float yawSmoothSpeed = 11f;
        [SerializeField] private float positionSmoothSpeed = 8f;
        [SerializeField] private float orbitDistance = 1f;
        [SerializeField] private float orbitDistanceMin = 0.78f;
        [SerializeField] private float orbitDistanceMax = 1.35f;
        [SerializeField] private float scrollZoomSensitivity = 0.12f;

        [Header("Input")]
        [SerializeField] private bool allowRightMouseDrag = true;
        [SerializeField] private float mouseRotateSensitivity = 0.32f;

        [Header("Auto Return")]
        [SerializeField] private float autoReturnDelay = 6f;
        [SerializeField] private float autoReturnSpeed = 30f;

        [Header("Transition")]
        [SerializeField] private float transitionBlendTime = 0.85f;

        [Header("Collision")]
        [SerializeField] private float collisionRadius = 0.32f;
        [SerializeField] private float collisionPullback = 0.5f;
        [SerializeField] private LayerMask obstructionMask = ~0;

        [Header("Outdoor Bounds")]
        [SerializeField] private float boundsMinX = -11f;
        [SerializeField] private float boundsMaxX = 6f;
        [SerializeField] private float boundsMinZ = -34f;
        [SerializeField] private float boundsMaxZ = -3.6f;
        [SerializeField] private float boundsClampMargin = 0.72f;

        private float lookAheadBlend;
        private float lookAheadTargetHeight;
        private float lookAheadTargetForward;
        private float lookAheadTargetLookHeight;
        private float lookAheadTargetLookForward;

        private Transform followTarget;
        private IndoorHighAngleCamera gameplayCamera;
        private TopDownCameraOcclusion cameraOcclusion;
        private OrbitState state = OrbitState.Inactive;
        private float targetYaw;
        private float displayYaw;
        private float idleTimer;
        private float blendTimer;
        private Vector3 blendStartPosition;
        private Quaternion blendStartRotation;
        private Vector3 blendEndPosition;
        private Quaternion blendEndRotation;
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[8];

        public bool IsZoneActive => state == OrbitState.Active || state == OrbitState.BlendingIn;

        private void Awake()
        {
            gameplayCamera = GetComponent<IndoorHighAngleCamera>();
            cameraOcclusion = GetComponent<TopDownCameraOcclusion>();
        }

        private void OnDisable()
        {
            if (state != OrbitState.Inactive)
            {
                ForceInactive();
            }
        }

        public void ConfigureOutdoorBounds(float minX, float maxX, float minZ, float maxZ, float clampMargin)
        {
            boundsMinX = minX;
            boundsMaxX = maxX;
            boundsMinZ = minZ;
            boundsMaxZ = maxZ;
            boundsClampMargin = clampMargin;
        }

        public void ConfigureRig(float horizontalOffset, float height, float lookHeight, float yaw)
        {
            orbitHorizontalOffset = horizontalOffset;
            orbitHeight = height;
            this.lookAtHeight = lookHeight;
            defaultYaw = yaw;
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

        /// <summary>Blend from normal gameplay camera into the street orbit rig.</summary>
        public void EnterZone(Transform target)
        {
            if (!zoneOnlyEnabled || target == null)
            {
                return;
            }

            if (GetComponent<FirstPersonCameraController>() is { enabled: true })
            {
                return;
            }

            followTarget = target;
            targetYaw = defaultYaw;
            displayYaw = defaultYaw;
            orbitDistance = 1f;
            idleTimer = 0f;

            if (gameplayCamera == null)
            {
                gameplayCamera = GetComponent<IndoorHighAngleCamera>();
            }

            if (cameraOcclusion == null)
            {
                cameraOcclusion = GetComponent<TopDownCameraOcclusion>();
            }

            if (cameraOcclusion != null)
            {
                cameraOcclusion.SetTarget(target);
                cameraOcclusion.SetActive(true);
            }

            blendStartPosition = transform.position;
            blendStartRotation = transform.rotation;
            ComputeOrbitPose(displayYaw, out blendEndPosition, out blendEndRotation);

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled = false;
            }

            blendTimer = 0f;
            state = OrbitState.BlendingIn;
        }

        /// <summary>Blend back to the normal gameplay camera.</summary>
        public void ExitZone()
        {
            if (state == OrbitState.Inactive || state == OrbitState.BlendingOut)
            {
                return;
            }

            blendStartPosition = transform.position;
            blendStartRotation = transform.rotation;
            SampleGameplayCameraPose(out blendEndPosition, out blendEndRotation);

            blendTimer = 0f;
            state = OrbitState.BlendingOut;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || state == OrbitState.Inactive)
            {
                return;
            }

            if (followTarget == null)
            {
                ResolveFollowTarget();
                if (followTarget == null)
                {
                    return;
                }
            }

            switch (state)
            {
                case OrbitState.BlendingIn:
                    TickBlendIn();
                    break;
                case OrbitState.Active:
                    TickActive();
                    break;
                case OrbitState.BlendingOut:
                    TickBlendOut();
                    break;
            }
        }

        private void TickBlendIn()
        {
            blendTimer += Time.deltaTime;
            float t = transitionBlendTime <= 0.01f
                ? 1f
                : Mathf.Clamp01(blendTimer / transitionBlendTime);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            ComputeOrbitPose(displayYaw, out Vector3 orbitPosition, out Quaternion orbitRotation);
            transform.position = Vector3.Lerp(blendStartPosition, orbitPosition, eased);
            transform.rotation = Quaternion.Slerp(blendStartRotation, orbitRotation, eased);

            if (t >= 1f)
            {
                state = OrbitState.Active;
            }
        }

        private void TickActive()
        {
            HandleInput();
            displayYaw = Mathf.LerpAngle(displayYaw, targetYaw, Time.deltaTime * yawSmoothSpeed);

            ComputeOrbitPose(displayYaw, out Vector3 desiredPosition, out Quaternion desiredRotation);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * positionSmoothSpeed);
        }

        private void TickBlendOut()
        {
            blendTimer += Time.deltaTime;
            float t = transitionBlendTime <= 0.01f
                ? 1f
                : Mathf.Clamp01(blendTimer / transitionBlendTime);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(blendStartPosition, blendEndPosition, eased);
            transform.rotation = Quaternion.Slerp(blendStartRotation, blendEndRotation, eased);

            if (t >= 1f)
            {
                ForceInactive();
                if (gameplayCamera != null)
                {
                    gameplayCamera.enabled = true;
                    gameplayCamera.SnapToFollowTarget();
                }
            }
        }

        private void HandleInput()
        {
            bool rotated = false;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.qKey.isPressed)
                {
                    targetYaw -= orbitRotateSpeed * Time.deltaTime;
                    rotated = true;
                }

                if (keyboard.eKey.isPressed)
                {
                    targetYaw += orbitRotateSpeed * Time.deltaTime;
                    rotated = true;
                }

                if (keyboard.rKey.wasPressedThisFrame)
                {
                    targetYaw = defaultYaw;
                    rotated = true;
                    idleTimer = 0f;
                }
            }

            if (allowRightMouseDrag)
            {
                Mouse mouse = Mouse.current;
                if (mouse != null)
                {
                    if (mouse.rightButton.isPressed)
                    {
                        float deltaX = mouse.delta.ReadValue().x;
                        if (Mathf.Abs(deltaX) > 0.01f)
                        {
                            targetYaw += deltaX * mouseRotateSensitivity;
                            rotated = true;
                        }
                    }

                    float scrollY = mouse.scroll.ReadValue().y;
                    if (Mathf.Abs(scrollY) > 0.01f)
                    {
                        orbitDistance = Mathf.Clamp(
                            orbitDistance + scrollY * scrollZoomSensitivity * 0.001f,
                            orbitDistanceMin,
                            orbitDistanceMax);
                        rotated = true;
                    }
                }
            }

            targetYaw = Mathf.Clamp(targetYaw, defaultYaw + minYaw, defaultYaw + maxYaw);

            if (rotated)
            {
                idleTimer = 0f;
            }
            else
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= autoReturnDelay)
                {
                    targetYaw = Mathf.MoveTowardsAngle(targetYaw, defaultYaw, autoReturnSpeed * Time.deltaTime);
                }
            }
        }

        private void ComputeOrbitPose(float yawDegrees, out Vector3 position, out Quaternion rotation)
        {
            Vector3 focus = GetFocusPoint();
            Quaternion yawRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            float distanceScale = orbitDistance;

            float effectiveHeight = Mathf.Lerp(orbitHeight, lookAheadTargetHeight, lookAheadBlend) * distanceScale;
            float effectiveForward = Mathf.Lerp(orbitHorizontalOffset, lookAheadTargetForward, lookAheadBlend)
                                   * distanceScale;
            float effectiveLookHeight = Mathf.Lerp(lookAtHeight, lookAheadTargetLookHeight, lookAheadBlend);

            Vector3 horizontalOffset = yawRotation * new Vector3(0f, 0f, effectiveForward);
            Vector3 desiredPosition = focus + horizontalOffset + Vector3.up * effectiveHeight;
            desiredPosition = ResolveCollision(focus, desiredPosition);
            desiredPosition = ClampCameraPosition(desiredPosition);

            Vector3 lookTarget = focus + Vector3.up * effectiveLookHeight;
            if (lookAheadBlend > 0.001f && lookAheadTargetLookForward > 0.01f)
            {
                lookTarget += yawRotation * new Vector3(0f, 0f, lookAheadTargetLookForward * lookAheadBlend);
            }

            rotation = Quaternion.LookRotation(lookTarget - desiredPosition, Vector3.up);
            position = desiredPosition;
        }

        private void SampleGameplayCameraPose(out Vector3 position, out Quaternion rotation)
        {
            if (gameplayCamera != null && followTarget != null)
            {
                bool wasEnabled = gameplayCamera.enabled;
                gameplayCamera.enabled = true;
                gameplayCamera.SetFollowTarget(followTarget);
                gameplayCamera.SnapToFollowTarget();
                position = transform.position;
                rotation = transform.rotation;
                gameplayCamera.enabled = wasEnabled;
                return;
            }

            ComputeOrbitPose(defaultYaw, out position, out rotation);
        }

        private Vector3 GetFocusPoint()
        {
            Vector3 focus = followTarget.position;
            focus.y = 0f;
            float margin = boundsClampMargin;
            focus.x = Mathf.Clamp(focus.x, boundsMinX + margin, boundsMaxX - margin);
            focus.z = Mathf.Clamp(focus.z, boundsMinZ + margin, boundsMaxZ - margin);
            return focus;
        }

        private Vector3 ResolveCollision(Vector3 focus, Vector3 desiredPosition)
        {
            Vector3 lookTarget = focus + Vector3.up * lookAtHeight;
            Vector3 delta = desiredPosition - lookTarget;
            float distance = delta.magnitude;
            if (distance < 0.05f)
            {
                return desiredPosition;
            }

            Vector3 direction = delta / distance;
            int hitCount = Physics.SphereCastNonAlloc(
                lookTarget,
                collisionRadius,
                direction,
                HitBuffer,
                distance,
                obstructionMask,
                QueryTriggerInteraction.Ignore);

            float nearest = distance;
            for (int i = 0; i < hitCount; i++)
            {
                if (HitBuffer[i].collider == null)
                {
                    continue;
                }

                if (followTarget != null && HitBuffer[i].transform.IsChildOf(followTarget))
                {
                    continue;
                }

                nearest = Mathf.Min(nearest, HitBuffer[i].distance);
            }

            if (nearest < distance - 0.05f)
            {
                return lookTarget + direction * Mathf.Max(2.5f, nearest - collisionPullback);
            }

            return desiredPosition;
        }

        private Vector3 ClampCameraPosition(Vector3 position)
        {
            float margin = boundsClampMargin * 0.35f;
            position.x = Mathf.Clamp(position.x, boundsMinX + margin, boundsMaxX - margin);
            position.z = Mathf.Clamp(position.z, boundsMinZ + margin, boundsMaxZ - margin);
            return position;
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

        private void ForceInactive()
        {
            state = OrbitState.Inactive;
            blendTimer = 0f;
            idleTimer = 0f;
        }
    }
}
