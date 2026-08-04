using ChoTet.Level1;
using ChoTet.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChoTet.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerTopDownController : MonoBehaviour
    {
        public Transform visualRoot;
        public Transform interactionOrigin;
        public RuntimeAnimatorController fallbackAnimatorController;
        public float walkSpeed = 4.5f;
        public float runSpeed = 6.25f;
        public float interactionRadius = 3.0f;
        public float gravity = -18f;

        [SerializeField] private float characterHeight = 1.8f;
        [SerializeField] private float characterRadius = 0.38f;
        [SerializeField] private float modelScale = 1f;
        [SerializeField] private float feetGroundOffset = 0f;
        [SerializeField] private float groundProbeUp = 0.45f;
        [SerializeField] private float groundProbeDown = 0.85f;
        [SerializeField] private float maxGroundSnapDelta = 0.12f;
        [SerializeField] private float minGroundNormalY = 0.55f;

        public float CharacterHeight => characterHeight;
        public float ModelScale => modelScale;

        private CharacterController characterController;
        private IGameplayPromptSource promptSource;
        private Vector3 velocity;
        private float bobTimer;
        private Vector3 visualBaseLocalPosition;
        private Interactable currentInteractable;
        private Transform shadowTransform;
        private Transform modelTransform;
        private Animator modelAnimator;
        private bool visualAligned;
        private int alignAttempts;
        private float smoothedMoveAmount;
        private Vector3 previousHorizontalPosition;
        private float locomotionInputMagnitude;
        private float footstepDistanceAccumulator;
        private AudioSource footstepAudioSource;
        private bool locomotionIsRunning;
        private float locomotionHorizontalSpeed;
        private float locomotionMoveSpeed;

        private const float MoveSpeedThreshold = 0.1f;
        private const float InputMoveThreshold = 0.01f;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveAmountHash = Animator.StringToHash("MoveAmount");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

        /// <summary>When true, WASD movement is disabled (e.g. during Level 1 phase fade).</summary>
        public bool MovementLocked { get; set; }

        private bool forceHideCharacterVisual;
        private bool characterVisibleDesired = true;

        /// <summary>
        /// When true, character mesh stays hidden even if <see cref="SetCharacterVisible"/> requests show
        /// (Level 1 outdoor → highway; cleared for parents reunion).
        /// </summary>
        public bool ForceHideCharacterVisual
        {
            get => forceHideCharacterVisual;
            set
            {
                forceHideCharacterVisual = value;
                ApplyCharacterVisibility();
            }
        }

        public void SetCharacterVisible(bool visible)
        {
            characterVisibleDesired = visible;
            ApplyCharacterVisibility();
        }

        private void ApplyCharacterVisibility()
        {
            bool visible = characterVisibleDesired && !forceHideCharacterVisual;

            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(visible);
            }

            if (shadowTransform != null)
            {
                shadowTransform.gameObject.SetActive(visible);
            }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterController.height = characterHeight;
            characterController.radius = characterRadius;
            characterController.center = new Vector3(0f, characterHeight * 0.5f, 0f);
            characterController.skinWidth = 0.06f;
            characterController.stepOffset = 0.28f;
            characterController.minMoveDistance = 0f;

            if (visualRoot == null)
            {
                GameObject visualRootObject = new GameObject("VisualRoot");
                visualRootObject.transform.SetParent(transform, false);
                visualRootObject.transform.localPosition = new Vector3(0f, feetGroundOffset, 0f);
                visualRoot = visualRootObject.transform;
            }

            visualRoot.localScale = Vector3.one * modelScale;
            visualBaseLocalPosition = visualRoot.localPosition;

            if (interactionOrigin == null)
            {
                GameObject origin = new GameObject("InteractionOrigin");
                origin.transform.SetParent(transform, false);
                interactionOrigin = origin.transform;
            }

            interactionOrigin.localPosition = new Vector3(0f, characterHeight * 0.58f, 0f);

            EnsureShadow();
            EnsureModelTransform();
            EnsureModelAnimator();
            previousHorizontalPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        }

        private void Start()
        {
            ResolvePromptSource();
            StartCoroutine(AlignVisualWhenReady());
        }

        private IEnumerator AlignVisualWhenReady()
        {
            const int maxFrames = 90;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                EnsureModelTransform();
                if (TryAlignVisualToBody())
                {
                    yield break;
                }

                yield return null;
            }
        }

        private void ResolvePromptSource()
        {
            if (promptSource != null)
            {
                return;
            }

            if (promptSource == null)
            {
                promptSource = FindAnyObjectByType<InventoryManager>();
            }

            if (promptSource == null)
            {
                promptSource = FindAnyObjectByType<GameFlowController>();
            }
        }

        private void Update()
        {
            if (promptSource == null)
            {
                ResolvePromptSource();
            }

            EnsureModelTransform();
            EnsureModelAnimator();
            if (!visualAligned)
            {
                TryAlignVisualToBody();
            }

            HandleMovement();
            UpdateNearbyInteractable();
            HandleInteractInput();
        }

        private void LateUpdate()
        {
            EnsureModelAnimator();
            DriveLocomotionAnimator();
        }

        private void HandleMovement()
        {
            bool isModalOpen = promptSource != null && promptSource.IsModalOpen;
            Vector2 moveInput = ReadMovementInput();
            if (isModalOpen || MovementLocked)
            {
                moveInput = Vector2.zero;
            }

            Vector3 move = GetCameraAlignedMove(moveInput);

            bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            float speed = isRunning ? runSpeed : walkSpeed;
            ApplyLocomotion(move, speed, isRunning);
        }

        /// <summary>Scripted walk (e.g. reunion path) — same footstep bob + animator as player input.</summary>
        public void MoveScripted(Vector3 flatWorldDirection)
        {
            Vector3 move = flatWorldDirection;
            move.y = 0f;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            ApplyLocomotion(move, walkSpeed, isRunning: false);
        }

        public void StopScriptedLocomotion()
        {
            ApplyLocomotion(Vector3.zero, walkSpeed, isRunning: false);
        }

        private void ApplyLocomotion(Vector3 move, float speed, bool isRunning)
        {
            if (characterController.isGrounded)
            {
                velocity.y = -2.5f;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }

            Vector3 displacement = (move * speed) + new Vector3(0f, velocity.y, 0f);
            characterController.Move(displacement * Time.deltaTime);
            CorrectGroundPosition();
            RecoverIfFallen();
            UpdateVisuals(move, speed, isRunning);

            Vector3 currentHorizontal = new Vector3(transform.position.x, 0f, transform.position.z);
            float distanceMoved = Vector3.Distance(currentHorizontal, previousHorizontalPosition);
            if (characterController.isGrounded && distanceMoved > 0.001f)
            {
                footstepDistanceAccumulator += distanceMoved;
                float stepInterval = isRunning ? 1.55f : 1.15f;
                if (footstepDistanceAccumulator >= stepInterval)
                {
                    footstepDistanceAccumulator = 0f;
                    PlayFootstepSound();
                }
            }

            previousHorizontalPosition = currentHorizontal;
        }

        private void RecoverIfFallen()
        {
            const float fallThreshold = -0.65f;
            if (transform.position.y > fallThreshold)
            {
                return;
            }

            Vector3 safe = transform.position;
            if (IsOutdoorAlleyPosition(safe))
            {
                safe.x = previousHorizontalPosition.x;
                safe.z = previousHorizontalPosition.z;
                safe.y = Level1OutdoorSettings.WalkSurfaceY;
            }
            else if (safe.z > 80f)
            {
                safe = new Vector3(55f, 1.8f, 120f);
            }
            else
            {
                GameObject home = GameObject.Find("HomeSpawnPoint");
                safe = home != null ? home.transform.position : new Vector3(6f, 0.5f, 12f);
            }

            characterController.enabled = false;
            transform.position = safe;
            characterController.enabled = true;
            velocity = Vector3.zero;
            SnapToGroundImmediate();
        }

        private static bool IsOutdoorAlleyPosition(Vector3 position)
        {
            if (position.x >= Level1OutdoorMovementSettings.PlayMinX - 1f
                && position.x <= Level1OutdoorMovementSettings.PlayMaxX + 1f
                && position.z <= Level1OutdoorMovementSettings.PlayStartZ + 1.2f
                && position.z >= Level1OutdoorMovementSettings.PlayEndZ - 1.2f)
            {
                return true;
            }

            return position.x >= Level1OutdoorMovementSettings.BusStopWalkMinX - 0.8f
                && position.x <= Level1OutdoorMovementSettings.BusStopWalkMaxX + 0.8f
                && position.z <= Level1OutdoorMovementSettings.BusStopWalkMaxZ + 0.6f
                && position.z >= Level1OutdoorMovementSettings.BusStopWalkMinZ - 0.6f;
        }

        private float GetHorizontalSpeed()
        {
            Vector3 velocityFlat = characterController.velocity;
            velocityFlat.y = 0f;
            float controllerSpeed = velocityFlat.magnitude;

            Vector3 positionFlat = new Vector3(transform.position.x, 0f, transform.position.z);
            float deltaSpeed = Time.deltaTime > 0.0001f
                ? Vector3.Distance(positionFlat, previousHorizontalPosition) / Time.deltaTime
                : 0f;

            return Mathf.Max(controllerSpeed, deltaSpeed);
        }

        private Vector2 ReadMovementInput()
        {
            Vector2 moveInput = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                {
                    moveInput.y += 1f;
                }

                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                {
                    moveInput.y -= 1f;
                }

                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                {
                    moveInput.x -= 1f;
                }

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                {
                    moveInput.x += 1f;
                }
            }

            if (Gamepad.current != null)
            {
                moveInput += Gamepad.current.leftStick.ReadValue();
            }

            return moveInput;
        }

        private Vector3 GetCameraAlignedMove(Vector2 input)
        {
            if (input.sqrMagnitude <= 0f)
            {
                return Vector3.zero;
            }

            Camera activeCamera = Camera.main;
            if (activeCamera == null)
            {
                Vector3 fallback = new Vector3(input.x, 0f, input.y);
                return fallback.sqrMagnitude > 1f ? fallback.normalized : fallback;
            }

            Vector3 forward = activeCamera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            Vector3 right = activeCamera.transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 move = right * input.x + forward * input.y;
            return move.sqrMagnitude > 1f ? move.normalized : move;
        }

        private void UpdateNearbyInteractable()
        {
            currentInteractable = null;
            float bestDistance = float.MaxValue;

            Collider[] hits = Physics.OverlapSphere(
                interactionOrigin.position,
                interactionRadius,
                ~0,
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < hits.Length; i++)
            {
                Interactable interactable = hits[i].GetComponentInParent<Interactable>();
                if (interactable == null)
                {
                    continue;
                }

                if (!interactable.CanInteract())
                {
                    continue;
                }

                float distance = Vector3.Distance(interactionOrigin.position, interactable.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    currentInteractable = interactable;
                }
            }

            if (promptSource != null)
            {
                promptSource.SetPrompt(currentInteractable != null ? currentInteractable.Prompt : string.Empty);
            }
        }

        private void HandleInteractInput()
        {
            if (currentInteractable == null)
            {
                return;
            }

            if (promptSource != null && promptSource.IsModalOpen)
            {
                return;
            }

            bool pressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            if (!pressed && Gamepad.current != null)
            {
                pressed = Gamepad.current.buttonSouth.wasPressedThisFrame;
            }

            if (pressed)
            {
                currentInteractable.Interact(this);
            }
        }

        private void UpdateVisuals(Vector3 move, float speed, bool isRunning)
        {
            if (visualRoot == null)
            {
                return;
            }

            if (FirstPersonCameraController.IsFirstPersonActive)
            {
                return;
            }

            float horizontalSpeed = GetHorizontalSpeed();
            float inputMagnitude = Mathf.Clamp01(move.magnitude);
            bool hasMoveInput = inputMagnitude > InputMoveThreshold;
            bool isActuallyMoving = horizontalSpeed > MoveSpeedThreshold;
            bool isMoving = hasMoveInput || isActuallyMoving;
            smoothedMoveAmount = isMoving ? 1f : 0f;

            Vector3 faceDirection = move;
            faceDirection.y = 0f;
            if (faceDirection.sqrMagnitude < 0.0001f && isActuallyMoving)
            {
                faceDirection = characterController.velocity;
                faceDirection.y = 0f;
            }

            if (smoothedMoveAmount > 0.05f && faceDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(faceDirection.normalized, Vector3.up);
                visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, Time.deltaTime * 12f);
                bobTimer += Time.deltaTime * (speed * 1.25f);
            }
            else
            {
                bobTimer += Time.deltaTime * 2f;
            }

            bool useWalkAnimation = IsAnimatorReady();
            float bobScale = useWalkAnimation ? 0f : 1f;
            float bobOffset = (1f + Mathf.Sin(bobTimer)) * 0.045f * smoothedMoveAmount * bobScale;
            float tilt = Mathf.Sin(bobTimer) * 8f * smoothedMoveAmount * bobScale;
            visualRoot.localPosition = visualBaseLocalPosition + new Vector3(0f, bobOffset, 0f);
            visualRoot.localEulerAngles = new Vector3(-4f * smoothedMoveAmount * bobScale, visualRoot.localEulerAngles.y, -tilt);

            if (shadowTransform != null)
            {
                float squash = 1f - (Mathf.Abs(Mathf.Sin(bobTimer)) * 0.08f * smoothedMoveAmount * bobScale);
                shadowTransform.localScale = new Vector3(0.8f * squash, 0.02f, 0.8f * squash);
            }

            locomotionInputMagnitude = inputMagnitude;
            locomotionIsRunning = isRunning;
            locomotionHorizontalSpeed = horizontalSpeed;
            locomotionMoveSpeed = speed;
        }

        private void DriveLocomotionAnimator()
        {
            if (!IsAnimatorReady())
            {
                return;
            }

            modelAnimator.applyRootMotion = false;
            if (!modelAnimator.enabled)
            {
                modelAnimator.enabled = true;
            }

            bool hasMoveInput = locomotionInputMagnitude > InputMoveThreshold;
            bool isActuallyMoving = locomotionHorizontalSpeed > MoveSpeedThreshold;
            bool isMoving = hasMoveInput || isActuallyMoving;
            float speedParam = 0f;
            if (isMoving)
            {
                float velocityAmount = locomotionHorizontalSpeed / Mathf.Max(locomotionMoveSpeed, 0.1f);
                speedParam = Mathf.Clamp(Mathf.Max(locomotionInputMagnitude, velocityAmount), MoveSpeedThreshold, 1.35f);
            }

            modelAnimator.SetBool(IsMovingHash, isMoving);
            modelAnimator.SetFloat(SpeedHash, speedParam);
            modelAnimator.SetBool(IsRunningHash, locomotionIsRunning && isMoving);

            // Legacy controllers that still read MoveAmount.
            modelAnimator.SetFloat(MoveAmountHash, isMoving ? Mathf.Max(0.5f, speedParam) : 0f);
            modelAnimator.speed = 1f;
        }

        private bool IsAnimatorReady()
        {
            return modelAnimator != null
                && modelAnimator.isActiveAndEnabled
                && visualRoot != null
                && modelAnimator.transform.IsChildOf(visualRoot)
                && modelAnimator.runtimeAnimatorController != null;
        }

        private bool TryAlignVisualToBody()
        {
            if (visualRoot == null || visualAligned || alignAttempts > 120)
            {
                return visualAligned;
            }

            EnsureModelTransform();
            if (modelTransform == null)
            {
                return false;
            }

            alignAttempts++;

            if (alignAttempts == 1)
            {
                modelTransform.localPosition = Vector3.zero;
                modelTransform.localRotation = Quaternion.identity;
            }

            Renderer[] renderers = modelTransform.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            if (combinedBounds.size.y < 0.08f)
            {
                return false;
            }

            float targetFeetY = transform.position.y + feetGroundOffset;
            float lift = targetFeetY - combinedBounds.min.y;
            visualRoot.position += new Vector3(0f, lift, 0f);

            combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            float centerOffsetX = transform.position.x - combinedBounds.center.x;
            float centerOffsetZ = transform.position.z - combinedBounds.center.z;
            visualRoot.position += new Vector3(centerOffsetX, 0f, centerOffsetZ);

            visualBaseLocalPosition = visualRoot.localPosition;
            visualAligned = true;
            return true;
        }

        private void EnsureModelTransform()
        {
            if (visualRoot == null || visualRoot.childCount == 0)
            {
                return;
            }

            Transform child = visualRoot.GetChild(0);
            if (modelTransform == child)
            {
                return;
            }

            modelTransform = child;
            modelAnimator = null;
        }

        private void EnsureModelAnimator()
        {
            if (visualRoot == null)
            {
                return;
            }

            if (IsAnimatorReady())
            {
                return;
            }

            EnsureModelTransform();
            if (modelTransform == null)
            {
                modelAnimator = null;
                return;
            }

            modelAnimator = modelTransform.GetComponent<Animator>();
            if (modelAnimator == null)
            {
                modelAnimator = modelTransform.GetComponentInChildren<Animator>(true);
            }
            if (modelAnimator == null)
            {
                return;
            }

            if (modelAnimator.runtimeAnimatorController == null && fallbackAnimatorController != null)
            {
                modelAnimator.runtimeAnimatorController = fallbackAnimatorController;
            }

            modelAnimator.applyRootMotion = false;
            modelAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            modelAnimator.updateMode = AnimatorUpdateMode.Normal;
        }

        private void EnsureShadow()
        {
            Transform existingShadow = transform.Find("PlayerShadow");
            if (existingShadow != null)
            {
                shadowTransform = existingShadow;
                return;
            }

            GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "PlayerShadow";
            shadow.transform.SetParent(transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.012f, 0f);
            shadow.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
            Collider shadowCollider = shadow.GetComponent<Collider>();
            if (shadowCollider != null)
            {
                Destroy(shadowCollider);
            }

            Renderer renderer = shadow.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Unlit/Color");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                }

                if (shader != null)
                {
                    Material material = new Material(shader);
                    material.color = new Color(0f, 0f, 0f, 0.28f);
                    renderer.sharedMaterial = material;
                }
            }

            shadowTransform = shadow.transform;
        }

        public void ForceRealignVisual()
        {
            visualAligned = false;
            alignAttempts = 0;
            modelTransform = null;
            modelAnimator = null;
            smoothedMoveAmount = 0f;
            locomotionInputMagnitude = 0f;
            locomotionIsRunning = false;

            if (visualRoot != null)
            {
                visualRoot.localPosition = new Vector3(0f, feetGroundOffset, 0f);
                visualBaseLocalPosition = visualRoot.localPosition;
            }

            StartCoroutine(AlignVisualWhenReady());
        }

        /// <summary>Moves the player without CharacterController fighting the transform update.</summary>
        public void TeleportTo(Vector3 worldPosition, Quaternion worldRotation)
        {
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            transform.SetPositionAndRotation(worldPosition, worldRotation);
            velocity = Vector3.zero;
            Physics.SyncTransforms();
            SnapToGroundImmediate();

            if (characterController != null)
            {
                characterController.enabled = true;
            }

            ForceRealignVisual();
        }

        public void ApplyTrafficKnockback(Vector3 sourceDirection, float distance)
        {
            if (characterController == null)
            {
                return;
            }

            Vector3 flatDirection = new Vector3(sourceDirection.x, 0f, sourceDirection.z);
            if (flatDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            flatDirection.Normalize();
            float pushDistance = Mathf.Max(0.45f, distance);

            characterController.enabled = false;
            transform.position += flatDirection * pushDistance;
            Physics.SyncTransforms();
            characterController.enabled = true;
        }

        private void CorrectGroundPosition()
        {
            if (characterController == null || !TryGetGroundHit(out RaycastHit hit))
            {
                return;
            }

            float delta = hit.point.y - transform.position.y;
            if (characterController.isGrounded)
            {
                if (delta < -0.015f || (delta > 0.001f && delta <= maxGroundSnapDelta))
                {
                    characterController.Move(new Vector3(0f, delta, 0f));
                }

                return;
            }

            if (delta < -0.04f)
            {
                characterController.Move(new Vector3(0f, delta, 0f));
                velocity.y = -2.5f;
            }
        }

        private void SnapToGroundImmediate()
        {
            if (!TryGetGroundHit(out RaycastHit hit))
            {
                return;
            }

            Vector3 position = transform.position;
            position.y = hit.point.y;
            transform.position = position;
        }

        private bool TryGetGroundHit(out RaycastHit hit)
        {
            Vector3 origin = transform.position + Vector3.up * groundProbeUp;
            float distance = groundProbeUp + groundProbeDown;
            if (!Physics.Raycast(origin, Vector3.down, out hit, distance, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
            {
                return false;
            }

            return hit.normal.y >= minGroundNormalY;
        }

        private void PlayFootstepSound()
        {
            if (footstepAudioSource == null)
            {
                footstepAudioSource = GetComponent<AudioSource>();
                if (footstepAudioSource == null)
                {
                    footstepAudioSource = gameObject.AddComponent<AudioSource>();
                }
                footstepAudioSource.playOnAwake = false;
                footstepAudioSource.spatialBlend = 1f; // 3D sound
                footstepAudioSource.minDistance = 1f;
                footstepAudioSource.maxDistance = 15f;
            }

            AudioClip clip = ProceduralAudio.GetFootstepClip();
            if (clip != null)
            {
                footstepAudioSource.pitch = Random.Range(0.85f, 1.15f);
                footstepAudioSource.PlayOneShot(clip, 0.45f);
            }
        }
    }
}
