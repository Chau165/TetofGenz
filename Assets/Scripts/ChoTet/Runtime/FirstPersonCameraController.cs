using ChoTet.Level1;
using ChoTet.Level1.Highway;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Unified first-person camera used across all gameplay scenes.
    /// Disables legacy third-person / orbit camera components on the same GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(Camera))]
    public class FirstPersonCameraController : MonoBehaviour
    {
        public static FirstPersonCameraController Active { get; private set; }

        [SerializeField] private Transform followTarget;
        [SerializeField] private float eyeHeight = 1.05f;
        [SerializeField] private float eyeHeightRatioOfCharacter = 0.90f;
        [SerializeField] private Vector3 vehicleEyeOffset = new Vector3(0f, 1.35f, 0.35f);
        [SerializeField] private float mouseSensitivityX = 0.14f;
        [SerializeField] private float mouseSensitivityY = 0.12f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private float positionSmooth = 18f;
        [SerializeField] private bool lockCursorInPlay = true;
        [SerializeField] private bool hidePlayerBody = true;
        [SerializeField] private float defaultFieldOfView = 72f;

        private PlayerTopDownController playerController;
        private float yaw;
        private float pitch;
        private bool followVehicle;
        private bool bodyHidden;
        private Camera cam;
        private bool eyeHeightSynced;

        public bool IsActive => enabled && gameObject.activeInHierarchy;
        public float YawDegrees => yaw;
        public Transform FollowTarget => followTarget;

        public static bool IsFirstPersonActive => Active != null && Active.IsActive;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            DisableLegacyCameraComponents();
        }

        private void OnEnable()
        {
            Active = this;
        }

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }

            RestorePlayerBody();
            if (lockCursorInPlay)
            {
                UnlockCursor();
            }
        }

        private void Start()
        {
            ResolveFollowTarget();
            InitializeLookFromTarget();
            ApplyCameraDefaults();
            HidePlayerBodyIfNeeded();
            SnapToTarget();
            UnlockCursor();
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                ResolveFollowTarget();
                if (followTarget == null)
                {
                    return;
                }
            }

            if (!eyeHeightSynced && !followVehicle)
            {
                SyncEyeHeightToCharacters();
            }

            UpdateCursorLock();
            if (!IsLookBlocked() && Cursor.lockState == CursorLockMode.Locked)
            {
                HandleLookInput();
            }

            ApplyCameraTransform(false);
            SyncPlayerYaw();
        }

        public void Bind(Transform target)
        {
            followTarget = target;
            playerController = target != null ? target.GetComponent<PlayerTopDownController>() : null;
            followVehicle = playerController == null && target != null &&
                            target.GetComponent<HighwayPlayerVehicle>() != null;
            eyeHeightSynced = false;
            SyncEyeHeightToCharacters();
            InitializeLookFromTarget();
            HidePlayerBodyIfNeeded();
            SnapToTarget();
        }

        /// <summary>
        /// Lowers first-person eye height to match Level 1 / Creative Character NPC height
        /// (player + street NPCs share ~0.68 scale, ~1.18m capsule).
        /// </summary>
        /// <param name="extraHeight">Optional raise (e.g. boarding-house indoor only).</param>
        public void SyncEyeHeightToCharacters(float extraHeight = 0f)
        {
            if (followVehicle)
            {
                return;
            }

            float fromPlayer = 0f;
            if (playerController != null && playerController.CharacterHeight > 0.4f)
            {
                fromPlayer = playerController.CharacterHeight * eyeHeightRatioOfCharacter;
            }

            float fromPeers = SamplePeerCharacterEyeHeight();
            float target = fromPeers > 0.35f
                ? fromPeers
                : fromPlayer > 0.35f
                    ? fromPlayer
                    : 1.05f;

            // Never sit above a short Level-1 capsule (was 1.62 on a ~1.18m body).
            if (playerController != null && playerController.CharacterHeight > 0.4f
                && extraHeight <= 0.001f)
            {
                target = Mathf.Min(target, playerController.CharacterHeight * 0.94f);
            }

            eyeHeight = Mathf.Clamp(target + Mathf.Max(0f, extraHeight), 0.75f, 1.45f);
            eyeHeightSynced = true;
        }

        /// <summary>Slightly taller eye height inside the Level 1 boarding-house room only.</summary>
        public void SyncEyeHeightForBoardingHouseIndoor()
        {
            const float indoorBoost = 0.22f;
            SyncEyeHeightToCharacters(indoorBoost);
        }

        private float SamplePeerCharacterEyeHeight()
        {
            if (followTarget == null)
            {
                return 0f;
            }

            float feetY = followTarget.position.y;
            float sum = 0f;
            int count = 0;

            SkinnedMeshRenderer[] skins = Object.FindObjectsByType<SkinnedMeshRenderer>(
                FindObjectsInactive.Exclude);
            for (int i = 0; i < skins.Length; i++)
            {
                SkinnedMeshRenderer skin = skins[i];
                if (skin == null || !skin.enabled || skin.sharedMesh == null)
                {
                    continue;
                }

                if (skin.transform.IsChildOf(followTarget))
                {
                    continue;
                }

                // Prefer Creative Character / crowd NPCs, skip huge environment meshes.
                string rootName = skin.transform.root != null ? skin.transform.root.name : skin.name;
                bool looksLikeCharacter =
                    rootName.IndexOf("NPC", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || rootName.IndexOf("Crowd", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || rootName.IndexOf("Father", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || rootName.IndexOf("Mother", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || rootName.IndexOf("CreativeCharacter", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || skin.name.StartsWith("Outerwear", System.StringComparison.Ordinal)
                    || skin.name.StartsWith("Body", System.StringComparison.Ordinal);

                if (!looksLikeCharacter)
                {
                    continue;
                }

                Bounds bounds = skin.bounds;
                float height = bounds.max.y - feetY;
                if (height < 0.7f || height > 2.2f)
                {
                    continue;
                }

                // Eyes sit a bit below the top of the mesh bounds.
                sum += height * 0.88f;
                count++;
                if (count >= 8)
                {
                    break;
                }
            }

            return count > 0 ? sum / count : 0f;
        }

        public void SnapToTarget()
        {
            ApplyCameraTransform(true);
            SyncPlayerYaw(true);
        }

        /// <summary>Force first-person as the active camera mode and disable legacy follow rigs.</summary>
        public void EnsurePrimary()
        {
            enabled = true;
            DisableLegacyCameraComponents();
            TopDownCameraOcclusion occlusion = GetComponent<TopDownCameraOcclusion>();
            if (occlusion != null)
            {
                occlusion.SetActive(false);
            }

            ResolveFollowTarget();
            HidePlayerBodyIfNeeded();
            SnapToTarget();
        }

        public static void EnsurePrimary(Camera camera, Transform followTarget = null)
        {
            if (camera == null)
            {
                return;
            }

            FirstPersonCameraController firstPerson = camera.GetComponent<FirstPersonCameraController>();
            if (firstPerson == null)
            {
                firstPerson = camera.gameObject.AddComponent<FirstPersonCameraController>();
            }

            if (followTarget != null)
            {
                firstPerson.Bind(followTarget);
            }

            firstPerson.EnsurePrimary();
        }

        public void SetYawPitch(float newYaw, float newPitch)
        {
            yaw = newYaw;
            pitch = Mathf.Clamp(newPitch, minPitch, maxPitch);
            SnapToTarget();
        }

        private void ResolveFollowTarget()
        {
            if (followTarget != null)
            {
                if (playerController == null)
                {
                    playerController = followTarget.GetComponent<PlayerTopDownController>();
                }

                followVehicle = playerController == null &&
                                followTarget.GetComponent<HighwayPlayerVehicle>() != null;
                return;
            }

            playerController = FindAnyObjectByType<PlayerTopDownController>();
            if (playerController != null)
            {
                followTarget = playerController.transform;
                followVehicle = false;
                return;
            }

            HighwayPlayerVehicle vehicle = FindAnyObjectByType<HighwayPlayerVehicle>();
            if (vehicle != null)
            {
                followTarget = vehicle.transform;
                followVehicle = true;
            }
        }

        private void InitializeLookFromTarget()
        {
            if (followTarget == null)
            {
                return;
            }

            yaw = followTarget.eulerAngles.y;
            pitch = 0f;
        }

        private void ApplyCameraDefaults()
        {
            if (cam == null)
            {
                return;
            }

            if (followVehicle)
            {
                cam.fieldOfView = Mathf.Max(cam.fieldOfView, 68f);
            }
            else if (cam.fieldOfView < 55f)
            {
                cam.fieldOfView = defaultFieldOfView;
            }
        }

        private void HandleLookInput()
        {
            Vector2 delta = Vector2.zero;

            if (Mouse.current != null)
            {
                delta = Mouse.current.delta.ReadValue();
            }

            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.rightStick.ReadValue();
                delta += stick * 14f * Time.deltaTime;
            }

            yaw += delta.x * mouseSensitivityX;
            pitch -= delta.y * mouseSensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void ApplyCameraTransform(bool snap)
        {
            Vector3 eyePosition = GetEyeWorldPosition();
            Quaternion eyeRotation = Quaternion.Euler(pitch, yaw, 0f);

            if (snap)
            {
                transform.SetPositionAndRotation(eyePosition, eyeRotation);
                return;
            }

            float t = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, eyePosition, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, eyeRotation, t);
        }

        private Vector3 GetEyeWorldPosition()
        {
            if (followTarget == null)
            {
                return transform.position;
            }

            if (followVehicle)
            {
                return followTarget.TransformPoint(vehicleEyeOffset);
            }

            return followTarget.position + Vector3.up * eyeHeight;
        }

        private void SyncPlayerYaw(bool snap = false)
        {
            if (playerController == null || followVehicle)
            {
                return;
            }

            Quaternion bodyRotation = Quaternion.Euler(0f, yaw, 0f);
            if (snap)
            {
                playerController.transform.rotation = bodyRotation;
            }
            else
            {
                playerController.transform.rotation = Quaternion.Slerp(
                    playerController.transform.rotation,
                    bodyRotation,
                    1f - Mathf.Exp(-14f * Time.deltaTime));
            }

            if (playerController.visualRoot != null)
            {
                playerController.visualRoot.rotation = bodyRotation;
            }
        }

        private void HidePlayerBodyIfNeeded()
        {
            if (!hidePlayerBody || playerController == null || bodyHidden)
            {
                return;
            }

            playerController.SetCharacterVisible(false);
            bodyHidden = true;
        }

        private void RestorePlayerBody()
        {
            if (!bodyHidden || playerController == null)
            {
                return;
            }

            playerController.SetCharacterVisible(true);
            bodyHidden = false;
        }

        private void UpdateCursorLock()
        {
            if (!lockCursorInPlay)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                UnlockCursor();
                return;
            }

            if (IsLookBlocked() || IsPointerOverUi())
            {
                UnlockCursor();
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUi())
            {
                LockCursor();
            }
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return EventSystem.current.IsPointerOverGameObject();
        }

        private static bool IsLookBlocked()
        {
            IGameplayPromptSource prompt = FindAnyObjectByType<GameFlowController>();
            if (prompt != null && prompt.IsModalOpen)
            {
                return true;
            }

            prompt = FindAnyObjectByType<InventoryManager>();
            if (prompt != null && prompt.IsModalOpen)
            {
                return true;
            }

            InventoryUIController inventoryUi = FindAnyObjectByType<InventoryUIController>();
            if (inventoryUi != null && inventoryUi.IsModalOpen)
            {
                return true;
            }

            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase != null && phase.IsTransitioning)
            {
                return true;
            }

            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            return player != null && player.MovementLocked;
        }

        private void DisableLegacyCameraComponents()
        {
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour == this)
                {
                    continue;
                }

                if (behaviour is TopDownCameraFollow
                    || behaviour is IndoorHighAngleCamera
                    || behaviour is BoardingHouseStreetOrbitCamera
                    || behaviour is StreetLookAheadCamera
                    || behaviour is ThirdPersonCameraFollow
                    || behaviour is HighwayFollowCamera)
                {
                    behaviour.enabled = false;
                }
            }
        }
    }

    /// <summary>Installs first-person camera on gameplay scenes at load time.</summary>
    public static class FirstPersonSceneBootstrap
    {
        private static readonly string[] MenuSceneNames =
        {
            "MainMenu",
            "LevelSelectMenu"
        };

        private static readonly string[] CameraBootstrapExcludedScenes =
        {
            "Level1_Highway"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyFirstPersonToGameplayScenes()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            for (int i = 0; i < MenuSceneNames.Length; i++)
            {
                if (sceneName == MenuSceneNames[i])
                {
                    return;
                }
            }

            for (int i = 0; i < CameraBootstrapExcludedScenes.Length; i++)
            {
                if (sceneName == CameraBootstrapExcludedScenes[i])
                {
                    return;
                }
            }

            Camera main = Camera.main;
            if (main == null)
            {
                return;
            }

            if (main.GetComponent<FirstPersonCameraController>() is FirstPersonCameraController existing)
            {
                existing.Bind(FindFollowTarget());
                existing.EnsurePrimary();
                return;
            }

            FirstPersonCameraController firstPerson = main.gameObject.AddComponent<FirstPersonCameraController>();
            firstPerson.Bind(FindFollowTarget());
            firstPerson.EnsurePrimary();
        }

        private static Transform FindFollowTarget()
        {
            PlayerTopDownController player = Object.FindAnyObjectByType<PlayerTopDownController>();
            if (player != null)
            {
                return player.transform;
            }

            HighwayPlayerVehicle vehicle = Object.FindAnyObjectByType<HighwayPlayerVehicle>();
            return vehicle != null ? vehicle.transform : null;
        }
    }
}
