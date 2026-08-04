using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChoTet.Level1
{
    /// <summary>
    /// Temporary street "peek ahead" camera: hold X to lower pitch and see farther down the road.
    /// Works with <see cref="IndoorHighAngleCamera"/> and <see cref="BoardingHouseStreetOrbitCamera"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)]
    public class StreetLookAheadCamera : MonoBehaviour
    {
        [Header("Normal Gameplay Rig")]
        [SerializeField] private float normalHeight = 6.8f;
        [SerializeField] private float normalForwardOffset = 5.2f;
        [SerializeField] private float normalLookHeight = 0.55f;

        [Header("Look Ahead Rig (hold X)")]
        [SerializeField] private float lookAheadHeight = 5.05f;
        [SerializeField] private float lookAheadForwardOffset = 6.65f;
        [SerializeField] private float lookAheadLookHeight = 0.38f;
        [SerializeField] private float lookAheadLookForward = 5.2f;
        [SerializeField] private float extraHeight = 0f;
        [SerializeField] private float extraDistance = 0f;

        [Header("Blend")]
        [SerializeField] private float transitionSpeed = 5.5f;
        [SerializeField] private float minLookAheadHeight = 4.55f;

        [Header("References")]
        [SerializeField] private IndoorHighAngleCamera gameplayCamera;
        [SerializeField] private BoardingHouseStreetOrbitCamera streetOrbitCamera;

        private float currentBlend;

        public float CurrentBlend => currentBlend;

        private void Awake()
        {
            ResolveReferences();
        }

        public void ConfigureOutdoorRig(float height, float forwardOffset, float lookHeight)
        {
            normalHeight = height;
            normalForwardOffset = forwardOffset;
            normalLookHeight = lookHeight;

            lookAheadHeight = Mathf.Max(minLookAheadHeight, height - 1.75f);
            lookAheadForwardOffset = forwardOffset + 1.45f;
            lookAheadLookHeight = Mathf.Max(0.28f, lookHeight - 0.17f);
            lookAheadLookForward = 5.2f;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!CanUseLookAhead())
            {
                SmoothBlendToward(0f);
                ApplyRig(currentBlend);
                return;
            }

            bool holdLookAhead = IsLookAheadHeld();
            SmoothBlendToward(holdLookAhead ? 1f : 0f);
            ApplyRig(currentBlend);
        }

        private void OnDisable()
        {
            currentBlend = 0f;
            ApplyRig(0f);
        }

        private bool CanUseLookAhead()
        {
            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            if (phase == null
                || phase.CurrentPhase != Level1PhaseController.Level1Phase.OutdoorStreet
                || phase.IsTransitioning)
            {
                return false;
            }

            if (streetOrbitCamera != null && streetOrbitCamera.IsZoneActive)
            {
                return true;
            }

            return gameplayCamera != null && gameplayCamera.enabled;
        }

        private static bool IsLookAheadHeld()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.xKey.isPressed;
        }

        private void SmoothBlendToward(float target)
        {
            currentBlend = Mathf.MoveTowards(currentBlend, target, transitionSpeed * Time.deltaTime);
        }

        private void ApplyRig(float blend)
        {
            float targetHeight = Mathf.Max(minLookAheadHeight, lookAheadHeight + extraHeight);
            float targetForward = lookAheadForwardOffset + extraDistance;

            ResolveReferences();

            if (gameplayCamera != null)
            {
                gameplayCamera.SetLookAheadRig(
                    blend,
                    targetHeight,
                    targetForward,
                    lookAheadLookHeight,
                    lookAheadLookForward);
            }

            if (streetOrbitCamera != null)
            {
                streetOrbitCamera.SetLookAheadRig(
                    blend,
                    targetHeight,
                    targetForward,
                    lookAheadLookHeight,
                    lookAheadLookForward);
            }
        }

        private void ResolveReferences()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = GetComponent<IndoorHighAngleCamera>();
            }

            if (streetOrbitCamera == null)
            {
                streetOrbitCamera = GetComponent<BoardingHouseStreetOrbitCamera>();
            }
        }
    }
}
