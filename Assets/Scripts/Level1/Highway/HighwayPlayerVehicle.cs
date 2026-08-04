using UnityEngine;
using UnityEngine.InputSystem;
using ChoTet.Runtime;

namespace ChoTet.Level1.Highway
{
    /// <summary>Player pickup van — arcade lane switching along computed lane center X positions.</summary>
    [DefaultExecutionOrder(200)]
    public class HighwayPlayerVehicle : MonoBehaviour
    {
        [Header("Lane movement")]
        [SerializeField] private float laneChangeDuration = 0.26f;
        [SerializeField] private bool debugLaneSwitching = true;

        [Header("Vehicle")]
        [SerializeField] private float vehicleLength = 4.8f;
        [SerializeField] private float defaultVisualScale = 1.55f;
        [SerializeField] private float highwayVisualYaw = 0f;
        [SerializeField] private GameObject defaultVanPrefab;

        private HighwayVisualPool visualPool;
        private Transform visualRoot;
        private GameObject visualPrefabSource;
        private WheelRotator wheelRotator;
        private AudioSource engineAudio;

        private int currentLaneIndex = HighwayPlayerLaneLayout.MiddleLaneIndex;
        private int targetLaneIndex = HighwayPlayerLaneLayout.MiddleLaneIndex;
        private float laneLerp;
        private float startLaneX;
        private float targetLaneX;
        private bool isChangingLane;
        private float forwardSpeed;
        private bool laneInputLocked;
        private bool highwayInputEnabled;

        private Vector2 touchStart;
        private bool touchTracking;

        public int CurrentLane => currentLaneIndex;
        public float VehicleLength => vehicleLength;

        public void SetLaneInputLocked(bool locked)
        {
            laneInputLocked = locked;
        }

        public void SetHighwayInputEnabled(bool enabled)
        {
            highwayInputEnabled = enabled;
        }

        public void SetBodyVisible(bool visible)
        {
            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(visible);
            }
        }

        private readonly System.Collections.Generic.List<Renderer> driverCulledRenderers = new System.Collections.Generic.List<Renderer>();

        /// <summary>Elevated chase cam — full car visible, pitched down to read traffic ahead.</summary>
        public bool TryGetDriverView(out Vector3 localOffset, out float pitchDegrees, out float fov)
        {
            localOffset = new Vector3(0f, 5.2f, -2.4f);
            pitchDegrees = 30f;
            fov = 74f;

            if (visualRoot == null || !TryComputePlayerLocalBounds(out Bounds bounds))
            {
                return false;
            }

            float length = Mathf.Max(0.5f, bounds.size.z);
            float height = Mathf.Max(0.4f, bounds.size.y);
            ResolveFrontRearZ(bounds, out float frontZ, out float rearZ);
            float travelSign = Mathf.Sign(frontZ - rearZ);

            float chaseZ = rearZ - travelSign * length * ResolveChaseRearPullback();
            float chaseY = bounds.max.y + height * 0.28f + ResolveChaseHeightLift(height);
            localOffset = new Vector3(bounds.center.x, chaseY, chaseZ);
            pitchDegrees = ResolveChasePitch(length, height);
            fov = ResolveChaseFieldOfView(length);
            ApplyPrefabChaseTuning(visualPrefabSource, ref localOffset, ref pitchDegrees, ref fov, length, travelSign);
            return true;
        }

        private static float ResolveChaseRearPullback()
        {
            return 0.38f;
        }

        private static float ResolveChasePitch(float length, float height)
        {
            float aspect = height / length;
            // Shallower pitch keeps sky + water readable beside the elevated deck.
            return Mathf.Lerp(34f, 28f, Mathf.InverseLerp(0.2f, 0.42f, aspect));
        }

        private static float ResolveChaseHeightLift(float height)
        {
            return Mathf.Lerp(1.7f, 2.4f, Mathf.InverseLerp(1.2f, 2.8f, height));
        }

        private static float ResolveChaseFieldOfView(float length)
        {
            return Mathf.Lerp(76f, 70f, Mathf.InverseLerp(4f, 6.5f, length));
        }

        private static void ApplyPrefabChaseTuning(
            GameObject prefab,
            ref Vector3 localOffset,
            ref float pitchDegrees,
            ref float fov,
            float length,
            float travelSign)
        {
            if (prefab == null)
            {
                return;
            }

            float rearPullback = travelSign * length * 0.03f;
            string prefabName = prefab.name.ToLowerInvariant();
            if (prefabName.Contains("delivery") || prefabName.Contains("truck"))
            {
                localOffset.y += 0.35f;
                localOffset.z -= rearPullback;
                pitchDegrees += 2f;
                fov -= 2f;
            }
            else if (prefabName.Contains("van"))
            {
                localOffset.y += 0.25f;
                localOffset.z -= rearPullback;
                pitchDegrees += 1.5f;
            }
            else if (prefabName.Contains("suv"))
            {
                localOffset.y += 0.18f;
                pitchDegrees += 1f;
            }
            else if (prefabName.Contains("hatchback") || prefabName.Contains("sports"))
            {
                localOffset.y -= 0.08f;
                localOffset.z += rearPullback;
                pitchDegrees -= 1f;
                fov += 2f;
            }
        }

        public void ApplyDriverBodyCulling(Vector3 driverLocalOffset)
        {
            RestoreDriverBodyCulling();
        }

        public void RestoreDriverBodyCulling()
        {
            for (int i = 0; i < driverCulledRenderers.Count; i++)
            {
                Renderer renderer = driverCulledRenderers[i];
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }

            driverCulledRenderers.Clear();
        }

        private void ResolveFrontRearZ(Bounds bounds, out float frontZ, out float rearZ)
        {
            frontZ = bounds.max.z;
            rearZ = bounds.min.z;
            if (visualRoot != null && Vector3.Dot(visualRoot.forward, transform.forward) < 0f)
            {
                frontZ = bounds.min.z;
                rearZ = bounds.max.z;
            }
        }

        private bool TryComputePlayerLocalBounds(out Bounds bounds)
        {
            bounds = default;
            if (visualRoot == null)
            {
                return false;
            }

            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return false;
            }

            bool hasBounds = false;
            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Bounds worldBounds = renderer.bounds;
                Vector3 center = worldBounds.center;
                Vector3 extents = worldBounds.extents;
                for (int sx = -1; sx <= 1; sx += 2)
                {
                    for (int sy = -1; sy <= 1; sy += 2)
                    {
                        for (int sz = -1; sz <= 1; sz += 2)
                        {
                            Vector3 corner = center + Vector3.Scale(extents, new Vector3(sx, sy, sz));
                            Vector3 local = transform.InverseTransformPoint(corner);
                            min = Vector3.Min(min, local);
                            max = Vector3.Max(max, local);
                            hasBounds = true;
                        }
                    }
                }
            }

            if (!hasBounds)
            {
                return false;
            }

            bounds = new Bounds((min + max) * 0.5f, max - min);
            return true;
        }

        public void Initialize(HighwayVisualPool pool, GameObject vanPrefab, float scale, int bodyColorIndex, float modelRotationOffsetY = 0f)
        {
            visualPool = pool;
            defaultVanPrefab = vanPrefab != null ? vanPrefab : defaultVanPrefab;

            currentLaneIndex = HighwayPlayerLaneLayout.MiddleLaneIndex;
            targetLaneIndex = currentLaneIndex;
            isChangingLane = false;
            laneLerp = 0f;

            transform.rotation = Quaternion.identity;
            AttachVisual(defaultVanPrefab, scale, bodyColorIndex, modelRotationOffsetY);
            SnapToLane(currentLaneIndex);
            RealignVisualToLane();
            PlaceAtSpawn(currentLaneIndex, HighwayLaneSettings.PlayerSpawnZ);
        }

        public void SetForwardSpeed(float speed)
        {
            forwardSpeed = speed;
            wheelRotator?.SetSpeed(speed);
        }

        public void PlaceAtSpawn(int laneIndex, float worldZ)
        {
            currentLaneIndex = ClampLaneIndex(laneIndex);
            targetLaneIndex = currentLaneIndex;
            isChangingLane = false;

            Vector3 pos = transform.position;
            pos.x = LaneCenterXFor(currentLaneIndex);
            pos.y = HighwayLaneSettings.RoadSurfaceY;
            pos.z = worldZ;
            transform.position = pos;

            LogLaneState("spawn");
        }

        public void Tick(float deltaTime)
        {
            UpdateLaneMotion(deltaTime);
        }

        private void Awake()
        {
            EnsureEngineAudio();
        }

        private void OnEnable()
        {
            EnsureEngineAudio();
            if (engineAudio != null && !engineAudio.isPlaying)
            {
                engineAudio.Play();
            }
        }

        private void OnDisable()
        {
            if (engineAudio != null && engineAudio.isPlaying)
            {
                engineAudio.Stop();
            }
        }

        private void Start()
        {
            EnsureEngineAudio();
            if (engineAudio != null)
            {
                engineAudio.Play();
            }
        }

        private void Update()
        {
            if (engineAudio != null)
            {
                float speedFactor = Mathf.Clamp01(forwardSpeed / 40f);
                float targetPitch = 0.85f + speedFactor * 0.55f;
                if (isChangingLane)
                {
                    targetPitch += 0.12f;
                }

                engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, targetPitch, Time.deltaTime * 4f);
                engineAudio.volume = Mathf.Lerp(engineAudio.volume, 0.42f + speedFactor * 0.22f, Time.deltaTime * 3f);
            }

            if (!highwayInputEnabled)
            {
                return;
            }

            ReadLaneInput();
        }

        private void EnsureEngineAudio()
        {
            if (engineAudio == null)
            {
                engineAudio = GetComponent<AudioSource>();
                if (engineAudio == null)
                {
                    engineAudio = gameObject.AddComponent<AudioSource>();
                }
            }

            engineAudio.playOnAwake = false;
            engineAudio.loop = true;
            engineAudio.spatialBlend = 0f;
            engineAudio.priority = 80;
            engineAudio.mute = false;
            engineAudio.dopplerLevel = 0f;
            if (engineAudio.clip == null)
            {
                engineAudio.clip = ProceduralAudio.GetTrafficHumClip();
            }

            if (engineAudio.volume < 0.2f)
            {
                engineAudio.volume = 0.5f;
            }

            if (engineAudio.pitch < 0.5f)
            {
                engineAudio.pitch = 0.95f;
            }
        }

        private void UpdateLaneMotion(float deltaTime)
        {
            if (!isChangingLane)
            {
                return;
            }

            laneLerp += deltaTime / Mathf.Max(0.05f, laneChangeDuration);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(laneLerp));

            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(startLaneX, targetLaneX, t);
            transform.position = pos;

            if (laneLerp < 1f)
            {
                return;
            }

            currentLaneIndex = targetLaneIndex;
            isChangingLane = false;
            SnapToLane(currentLaneIndex);
            RealignVisualToLane();
            LogLaneState("arrived");
        }

        private void LateUpdate()
        {
            if (!isChangingLane)
            {
                SnapToLane(currentLaneIndex);
            }
        }

        private void RealignVisualToLane()
        {
            if (visualRoot == null)
            {
                return;
            }

            CenterVisualOnPivot();
        }

        private void ReadLaneInput()
        {
            if (laneInputLocked || isChangingLane)
            {
                return;
            }

            int? direction = null;
            int desiredLane = currentLaneIndex;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
                {
                    desiredLane = currentLaneIndex - 1;
                    direction = -1;
                }
                else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                {
                    desiredLane = currentLaneIndex + 1;
                    direction = 1;
                }
            }

            if (direction == null)
            {
                ReadSwipeLaneChange(ref desiredLane, ref direction);
            }

            if (direction != null)
            {
                BeginLaneChange(desiredLane, direction.Value);
            }
        }

        private void ReadSwipeLaneChange(ref int desiredLane, ref int? direction)
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                touchTracking = false;
                return;
            }

            UnityEngine.InputSystem.Controls.TouchControl touch = touchscreen.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                touchStart = touch.position.ReadValue();
                touchTracking = true;
                return;
            }

            if (!touchTracking || !touch.press.wasReleasedThisFrame)
            {
                if (!touch.press.isPressed)
                {
                    touchTracking = false;
                }

                return;
            }

            float deltaX = touch.position.ReadValue().x - touchStart.x;
            touchTracking = false;
            if (Mathf.Abs(deltaX) < 48f)
            {
                return;
            }

            direction = deltaX < 0f ? -1 : 1;
            desiredLane = currentLaneIndex + direction.Value;
        }

        public void TryChangeLane(int laneIndex)
        {
            if (laneInputLocked)
            {
                return;
            }

            int delta = laneIndex - currentLaneIndex;
            if (delta == 0)
            {
                return;
            }

            int direction = delta > 0 ? 1 : -1;
            BeginLaneChange(laneIndex, direction);
        }

        private void BeginLaneChange(int desiredLaneIndex, int inputDirection)
        {
            desiredLaneIndex = ClampLaneIndex(desiredLaneIndex);
            if (desiredLaneIndex == currentLaneIndex || isChangingLane)
            {
                LogLaneState(inputDirection < 0 ? "blocked-left" : "blocked-right");
                return;
            }

            targetLaneIndex = desiredLaneIndex;
            startLaneX = LaneCenterXFor(currentLaneIndex);
            targetLaneX = LaneCenterXFor(targetLaneIndex);
            laneLerp = 0f;
            isChangingLane = true;

            LogLaneState(inputDirection < 0 ? "left" : "right");
        }

        private void SnapToLane(int laneIndex)
        {
            Vector3 pos = transform.position;
            pos.x = LaneCenterXFor(laneIndex);
            pos.y = HighwayLaneSettings.RoadSurfaceY;
            transform.position = pos;
        }

        private static int ClampLaneIndex(int laneIndex)
        {
            return Mathf.Clamp(laneIndex, 0, HighwayPlayerLaneLayout.LaneCount - 1);
        }

        private static float LaneCenterXFor(int laneIndex)
        {
            return HighwayPlayerLaneLayout.LaneCenterX(laneIndex);
        }

        private void LogLaneState(string inputDirection)
        {
            if (!debugLaneSwitching)
            {
                return;
            }

            Debug.Log(
                "[HighwayPlayer] currentLaneIndex=" + currentLaneIndex
                + " targetLaneIndex=" + targetLaneIndex
                + " targetLaneX=" + LaneCenterXFor(targetLaneIndex).ToString("F2")
                + " dividerLeft=" + HighwayPlayerLaneLayout.LaneDividerX(0).ToString("F2")
                + " dividerRight=" + HighwayPlayerLaneLayout.LaneDividerX(1).ToString("F2")
                + " vehicleX=" + transform.position.x.ToString("F2")
                + " input=" + inputDirection);
        }

        private void AttachVisual(GameObject prefab, float scale, int bodyColorIndex, float modelRotationOffsetY)
        {
            ReleaseVisual();
            visualPrefabSource = prefab;
            if (prefab == null || visualPool == null)
            {
                return;
            }

            GameObject wrapper = new GameObject("PlayerVisualWrapper");
            wrapper.transform.SetParent(transform, false);
            wrapper.transform.localRotation = Quaternion.identity;

            visualRoot = visualPool.Acquire(prefab, wrapper.transform);
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localRotation = Quaternion.Euler(0f, ResolveVisualYaw(modelRotationOffsetY), 0f);
            visualRoot.localScale = Vector3.one * scale;
            CenterVisualOnPivot();

            StripPhysics(visualRoot.gameObject);
            Level1TrafficVisualUtility.ApplyCarLook(visualRoot.gameObject, bodyColorIndex, isPickup: true);

            wheelRotator = visualRoot.GetComponent<WheelRotator>();
            if (wheelRotator == null)
            {
                wheelRotator = visualRoot.gameObject.AddComponent<WheelRotator>();
            }

            wheelRotator.Configure(0.32f, Vector3.right);
            wheelRotator.ResetCache();
            wheelRotator.SetSpeed(forwardSpeed);
        }

        private float ResolveVisualYaw(float modelRotationOffsetY)
        {
            // Highway driving direction is +Z; keep this stable regardless of scene serialized yaw.
            return modelRotationOffsetY;
        }

        private void CenterVisualOnPivot()
        {
            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                visualRoot.localPosition = Vector3.zero;
                return;
            }

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < renderers.Length; i++)
            {
                sum += visualRoot.InverseTransformPoint(renderers[i].bounds.center);
            }

            Vector3 localCenter = sum / renderers.Length;
            visualRoot.localPosition = new Vector3(-localCenter.x, 0f, 0f);
        }

        private void ReleaseVisual()
        {
            RestoreDriverBodyCulling();
            if (visualRoot != null && visualPool != null && visualPrefabSource != null)
            {
                visualPool.Release(visualRoot, visualPrefabSource);
            }
            else if (visualRoot != null)
            {
                HighwayPropUtility.SafeDestroy(visualRoot.gameObject);
            }

            visualRoot = null;
            visualPrefabSource = null;
            wheelRotator = null;
        }

        private static void StripPhysics(GameObject root)
        {
            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                HighwayPropUtility.SafeDestroy(bodies[i]);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                HighwayPropUtility.SafeDestroy(colliders[i]);
            }
        }
    }
}
