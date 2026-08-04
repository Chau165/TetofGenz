using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>Scripted top-down walk using the same locomotion feel as <see cref="PlayerTopDownController"/>.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class TopDownScriptedLocomotion : MonoBehaviour
    {
        public Transform visualRoot;
        public float modelScale = 1f;
        public float feetGroundOffset;
        public float gravity = -18f;

        [SerializeField] private float characterHeight = 1.8f;
        [SerializeField] private float characterRadius = 0.38f;

        private CharacterController characterController;
        private Transform shadowTransform;
        private Transform modelTransform;
        private Animator modelAnimator;
        private Vector3 velocity;
        private float bobTimer;
        private Vector3 visualBaseLocalPosition;
        private Vector3 previousHorizontalPosition;
        private float locomotionInputMagnitude;
        private float locomotionHorizontalSpeed;
        private float locomotionMoveSpeed;

        private const float MoveSpeedThreshold = 0.1f;
        private const float InputMoveThreshold = 0.01f;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveAmountHash = Animator.StringToHash("MoveAmount");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            ConfigureCharacterController();
            EnsureVisualRoot();
            EnsureShadow();
            EnsureModelTransform();
            EnsureModelAnimator();
            previousHorizontalPosition = FlatPosition(transform.position);
        }

        public void ConfigureForScale(float scale, float feetOffset = 0f)
        {
            modelScale = Mathf.Max(0.1f, scale);
            feetGroundOffset = feetOffset;
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            ConfigureCharacterController();

            if (visualRoot != null)
            {
                visualRoot.localScale = Vector3.one * modelScale;
                visualBaseLocalPosition = visualRoot.localPosition;
            }
        }

        public void MoveInDirection(Vector3 flatWorldDirection, float speed)
        {
            Vector3 move = flatWorldDirection;
            move.y = 0f;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            ApplyLocomotion(move, speed);
        }

        public void Stop()
        {
            ApplyLocomotion(Vector3.zero, locomotionMoveSpeed > 0.01f ? locomotionMoveSpeed : 1f);
        }

        private void Update()
        {
            EnsureModelTransform();
            EnsureModelAnimator();
        }

        private void LateUpdate()
        {
            EnsureModelAnimator();
            DriveLocomotionAnimator();
        }

        private void ConfigureCharacterController()
        {
            if (characterController == null)
            {
                return;
            }

            float height = characterHeight * modelScale;
            float radius = characterRadius * modelScale;
            characterController.height = height;
            characterController.radius = radius;
            characterController.center = new Vector3(0f, height * 0.5f, 0f);
            characterController.skinWidth = 0.06f;
            characterController.stepOffset = 0.22f * modelScale;
            characterController.minMoveDistance = 0f;
        }

        private void ApplyLocomotion(Vector3 move, float speed)
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
            SnapToWalkSurface();
            UpdateVisuals(move, speed);
            previousHorizontalPosition = FlatPosition(transform.position);
        }

        private void SnapToWalkSurface()
        {
            Vector3 origin = transform.position + Vector3.up * 0.45f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                Vector3 pos = transform.position;
                pos.y = hit.point.y;
                transform.position = pos;
            }
        }

        private void UpdateVisuals(Vector3 move, float speed)
        {
            if (visualRoot == null)
            {
                return;
            }

            float horizontalSpeed = GetHorizontalSpeed();
            float inputMagnitude = Mathf.Clamp01(move.magnitude);
            bool hasMoveInput = inputMagnitude > InputMoveThreshold;
            bool isActuallyMoving = horizontalSpeed > MoveSpeedThreshold;
            bool isMoving = hasMoveInput || isActuallyMoving;

            Vector3 faceDirection = move;
            faceDirection.y = 0f;
            if (faceDirection.sqrMagnitude < 0.0001f && isActuallyMoving)
            {
                faceDirection = characterController.velocity;
                faceDirection.y = 0f;
            }

            if (isMoving && faceDirection.sqrMagnitude > 0.0001f)
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
            float bobOffset = (1f + Mathf.Sin(bobTimer)) * 0.045f * (isMoving ? 1f : 0f) * bobScale;
            float tilt = Mathf.Sin(bobTimer) * 8f * (isMoving ? 1f : 0f) * bobScale;
            visualRoot.localPosition = visualBaseLocalPosition + new Vector3(0f, bobOffset, 0f);
            visualRoot.localEulerAngles = new Vector3(-4f * (isMoving ? 1f : 0f) * bobScale, visualRoot.localEulerAngles.y, -tilt);

            if (shadowTransform != null)
            {
                float squash = 1f - (Mathf.Abs(Mathf.Sin(bobTimer)) * 0.08f * (isMoving ? 1f : 0f) * bobScale);
                shadowTransform.localScale = new Vector3(0.8f * squash, 0.02f, 0.8f * squash);
            }

            locomotionInputMagnitude = inputMagnitude;
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
            modelAnimator.SetBool(IsRunningHash, false);
            modelAnimator.SetFloat(MoveAmountHash, isMoving ? Mathf.Max(0.5f, speedParam) : 0f);
            modelAnimator.speed = 1f;
        }

        private float GetHorizontalSpeed()
        {
            Vector3 velocityFlat = characterController.velocity;
            velocityFlat.y = 0f;
            float controllerSpeed = velocityFlat.magnitude;

            Vector3 positionFlat = FlatPosition(transform.position);
            float deltaSpeed = Time.deltaTime > 0.0001f
                ? Vector3.Distance(positionFlat, previousHorizontalPosition) / Time.deltaTime
                : 0f;

            return Mathf.Max(controllerSpeed, deltaSpeed);
        }

        private bool IsAnimatorReady()
        {
            return modelAnimator != null
                && modelAnimator.isActiveAndEnabled
                && modelAnimator.runtimeAnimatorController != null;
        }

        private void EnsureVisualRoot()
        {
            if (visualRoot == null)
            {
                Transform existing = transform.Find("VisualRoot");
                visualRoot = existing != null ? existing : CreateVisualRoot();
            }

            visualRoot.localScale = Vector3.one * modelScale;
            if (visualRoot.localPosition == Vector3.zero)
            {
                visualRoot.localPosition = new Vector3(0f, feetGroundOffset, 0f);
            }

            visualBaseLocalPosition = visualRoot.localPosition;
        }

        private Transform CreateVisualRoot()
        {
            GameObject visualRootObject = new GameObject("VisualRoot");
            visualRootObject.transform.SetParent(transform, false);
            visualRootObject.transform.localPosition = new Vector3(0f, feetGroundOffset, 0f);
            return visualRootObject.transform;
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
            if (IsAnimatorReady())
            {
                return;
            }

            modelAnimator = GetComponent<Animator>();
            if (modelAnimator == null)
            {
                modelAnimator = GetComponentInChildren<Animator>(true);
            }

            if (modelAnimator != null)
            {
                modelAnimator.applyRootMotion = false;
                modelAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }

        private void EnsureShadow()
        {
            Transform existingShadow = transform.Find("NpcShadow");
            if (existingShadow != null)
            {
                shadowTransform = existingShadow;
                return;
            }

            GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "NpcShadow";
            shadow.transform.SetParent(transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.012f, 0f);
            shadow.transform.localScale = new Vector3(0.55f * modelScale, 0.02f, 0.55f * modelScale);
            Collider shadowCollider = shadow.GetComponent<Collider>();
            if (shadowCollider != null)
            {
                Destroy(shadowCollider);
            }

            Renderer renderer = shadow.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Unlit/Color") ?? Shader.Find("Universal Render Pipeline/Unlit");
                if (shader != null)
                {
                    Material material = new Material(shader);
                    material.color = new Color(0f, 0f, 0f, 0.28f);
                    renderer.sharedMaterial = material;
                }
            }

            shadowTransform = shadow.transform;
        }

        private static Vector3 FlatPosition(Vector3 position)
        {
            return new Vector3(position.x, 0f, position.z);
        }

        public static void EnsurePatrolSetup(GameObject root, float scale, float feetOffset = 0f)
        {
            if (root == null)
            {
                return;
            }

            StripFacePicker(root);

            Transform visualRootTransform = root.transform.Find("VisualRoot");
            if (visualRootTransform == null)
            {
                GameObject visualRootObject = new GameObject("VisualRoot");
                visualRootTransform = visualRootObject.transform;
                visualRootTransform.SetParent(root.transform, false);
                visualRootTransform.localPosition = new Vector3(0f, feetOffset, 0f);
            }

            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = root.transform.GetChild(i);
                if (child == visualRootTransform || child.name == "NpcShadow")
                {
                    continue;
                }

                child.SetParent(visualRootTransform, true);
            }

            CharacterController controller = root.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = root.AddComponent<CharacterController>();
            }

            TopDownScriptedLocomotion locomotion = root.GetComponent<TopDownScriptedLocomotion>();
            if (locomotion == null)
            {
                locomotion = root.AddComponent<TopDownScriptedLocomotion>();
            }

            locomotion.visualRoot = visualRootTransform;
            locomotion.ConfigureForScale(scale, feetOffset);
        }

        private static void StripFacePicker(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour.GetType().Name != "FacePicker")
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(behaviour);
                }
                else
#endif
                {
                    Object.Destroy(behaviour);
                }
            }
        }

#if UNITY_EDITOR
        public static void PreparePatrolHierarchy(GameObject root, float scale, float feetOffset = 0f)
        {
            EnsurePatrolSetup(root, scale, feetOffset);
        }
#endif
    }
}
