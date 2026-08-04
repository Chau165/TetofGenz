using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Menu
{
    /// <summary>
    /// Coordinates tabletop rollers and lightweight nudge collisions between nearby props.
    /// </summary>
    public class MainMenuTableDecorRollSimulator : MonoBehaviour
    {
        public const float MenuIntroDelay = 5f;

        private readonly List<MainMenuTableDecorRollMotion> rollers = new List<MainMenuTableDecorRollMotion>();

        public void Register(MainMenuTableDecorRollMotion roller)
        {
            if (roller != null && !rollers.Contains(roller))
            {
                rollers.Add(roller);
            }
        }

        private void LateUpdate()
        {
            ResolveCollisions();
        }

        private void ResolveCollisions()
        {
            for (int i = 0; i < rollers.Count; i++)
            {
                MainMenuTableDecorRollMotion a = rollers[i];
                if (a == null || !a.IsRolling)
                {
                    continue;
                }

                for (int j = i + 1; j < rollers.Count; j++)
                {
                    MainMenuTableDecorRollMotion b = rollers[j];
                    if (b == null || !b.IsRolling)
                    {
                        continue;
                    }

                    a.TryNudge(b);
                }
            }
        }
    }

    /// <summary>
    /// Rolls produce freely on the tabletop with smooth velocity steering and edge bounces.
    /// </summary>
    public class MainMenuTableDecorRollMotion : MonoBehaviour
    {
        private const float PositionSmoothTime = 0.12f;
        private const float VelocitySmoothTime = 0.35f;
        private const float RotationSmoothTime = 0.1f;

        private static readonly Vector2 BoundsMinViewport = new Vector2(-0.08f, -0.08f);
        private static readonly Vector2 BoundsMaxViewport = new Vector2(1.08f, 1.08f);

        private enum MotionState
        {
            WaitingForIntro,
            Rolling
        }

        private Vector3 homePosition;
        private Quaternion baseRotation;
        private Vector3 displayPosition;
        private Vector3 positionVelocity;
        private Vector3 velocity;
        private Vector3 velocitySmoothRef;
        private Vector3 targetVelocity;
        private float rollRadius;
        private float contactRadius;
        private float cumulativeRollAngle;
        private float targetRollAngle;
        private float rollAngleVelocity;
        private float previousSpeedSample;
        private float introTimer;
        private float nextSteerTimer;
        private float nextBurstTimer;
        private Vector3 currentSpinAxis = Vector3.right;
        private MotionState state;
        private Camera decorCamera;
        private float tableY;
        private float minX;
        private float maxX;
        private float minZ;
        private float maxZ;

        public bool IsRolling => isActiveAndEnabled && state == MotionState.Rolling;
        public Vector3 DisplayPosition => displayPosition;

        public static void Attach(GameObject prop, MainMenuTableDecorRollSimulator simulator, Camera camera,
            int seed, float scale)
        {
            if (prop == null || simulator == null || camera == null)
            {
                return;
            }

            MainMenuTableDecorRollMotion motion = prop.AddComponent<MainMenuTableDecorRollMotion>();
            motion.Initialize(simulator, camera, seed, scale);
            simulator.Register(motion);
        }

        private void Initialize(MainMenuTableDecorRollSimulator simulator, Camera camera, int seed, float scale)
        {
            Random.State previous = Random.state;
            Random.InitState(seed);

            decorCamera = camera;
            homePosition = transform.position;
            tableY = homePosition.y;
            displayPosition = homePosition;
            baseRotation = transform.rotation;
            cumulativeRollAngle = 0f;
            targetRollAngle = 0f;

            rollRadius = Mathf.Lerp(0.08f, 0.22f, scale / 2f);
            contactRadius = rollRadius * Random.Range(1.05f, 1.35f);

            CacheTableBounds();

            state = MotionState.WaitingForIntro;
            introTimer = MainMenuTableDecorRollSimulator.MenuIntroDelay + Random.Range(0f, 2.5f);
            nextSteerTimer = Random.Range(0.4f, 1.2f);
            nextBurstTimer = Random.Range(1.5f, 3.5f);
            PickNewTargetVelocity(0.65f);

            Random.state = previous;
        }

        private void CacheTableBounds()
        {
            Vector3 min = MainMenuTableDecorBuilder.ViewportToTablePoint(decorCamera, BoundsMinViewport);
            Vector3 max = MainMenuTableDecorBuilder.ViewportToTablePoint(decorCamera, BoundsMaxViewport);
            minX = Mathf.Min(min.x, max.x);
            maxX = Mathf.Max(min.x, max.x);
            minZ = Mathf.Min(min.z, max.z);
            maxZ = Mathf.Max(min.z, max.z);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            switch (state)
            {
                case MotionState.WaitingForIntro:
                    UpdateWaitingForIntro(dt);
                    break;
                default:
                    UpdateRolling(dt);
                    break;
            }
        }

        private void UpdateWaitingForIntro(float dt)
        {
            introTimer -= dt;
            SnapToHome();

            if (introTimer <= 0f)
            {
                state = MotionState.Rolling;
                PickNewTargetVelocity(1f);
                nextSteerTimer = Random.Range(0.2f, 0.8f);
            }
        }

        private void SnapToHome()
        {
            displayPosition = homePosition;
            positionVelocity = Vector3.zero;
            velocity = Vector3.zero;
            velocitySmoothRef = Vector3.zero;
            transform.position = homePosition;
            transform.rotation = baseRotation;
            cumulativeRollAngle = 0f;
            targetRollAngle = 0f;
            rollAngleVelocity = 0f;
        }

        private void UpdateRolling(float dt)
        {
            nextSteerTimer -= dt;
            if (nextSteerTimer <= 0f)
            {
                PickNewTargetVelocity(Random.Range(0.45f, 1f));
                nextSteerTimer = Random.Range(0.8f, 2.4f);
            }

            nextBurstTimer -= dt;
            if (nextBurstTimer <= 0f)
            {
                ApplyRandomBurst();
                nextBurstTimer = Random.Range(1.2f, 3.2f);
            }

            velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref velocitySmoothRef, VelocitySmoothTime);
            velocity.y = 0f;

            displayPosition += velocity * dt;
            displayPosition = ResolveEdgeBounces(displayPosition);

            displayPosition.y = tableY;
            transform.position = Vector3.SmoothDamp(transform.position, displayPosition, ref positionVelocity,
                PositionSmoothTime);
            transform.position = new Vector3(transform.position.x, tableY, transform.position.z);

            ApplyRollRotation(dt);
        }

        private void PickNewTargetVelocity(float intensity)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Random.insideUnitCircle.normalized;
            }

            float speed = Random.Range(0.35f, 1.05f) * intensity;
            targetVelocity = new Vector3(direction.x, 0f, direction.y) * speed;
        }

        private void ApplyRandomBurst()
        {
            Vector3 burst = new Vector3(Random.Range(-0.35f, 0.35f), 0f, Random.Range(-0.35f, 0.35f));
            targetVelocity += burst;
            targetVelocity = Vector3.ClampMagnitude(targetVelocity, 1.35f);
        }

        private Vector3 ResolveEdgeBounces(Vector3 position)
        {
            bool bounced = false;

            if (position.x < minX)
            {
                position.x = minX;
                velocity.x = Mathf.Abs(velocity.x) * Random.Range(0.55f, 0.9f);
                targetVelocity.x = Mathf.Abs(targetVelocity.x) * Random.Range(0.65f, 1f);
                bounced = true;
            }
            else if (position.x > maxX)
            {
                position.x = maxX;
                velocity.x = -Mathf.Abs(velocity.x) * Random.Range(0.55f, 0.9f);
                targetVelocity.x = -Mathf.Abs(targetVelocity.x) * Random.Range(0.65f, 1f);
                bounced = true;
            }

            if (position.z < minZ)
            {
                position.z = minZ;
                velocity.z = Mathf.Abs(velocity.z) * Random.Range(0.55f, 0.9f);
                targetVelocity.z = Mathf.Abs(targetVelocity.z) * Random.Range(0.65f, 1f);
                bounced = true;
            }
            else if (position.z > maxZ)
            {
                position.z = maxZ;
                velocity.z = -Mathf.Abs(velocity.z) * Random.Range(0.55f, 0.9f);
                targetVelocity.z = -Mathf.Abs(targetVelocity.z) * Random.Range(0.65f, 1f);
                bounced = true;
            }

            if (bounced)
            {
                PickNewTargetVelocity(Random.Range(0.35f, 0.75f));
                nextSteerTimer = Random.Range(0.5f, 1.4f);
            }

            return position;
        }

        private void ApplyRollRotation(float dt)
        {
            Vector3 moveDir = velocity;
            moveDir.y = 0f;
            float speed = moveDir.magnitude;
            if (speed <= 0.02f)
            {
                return;
            }

            currentSpinAxis = Vector3.Cross(moveDir.normalized, Vector3.up).normalized;
            if (currentSpinAxis.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float deltaAngle = (speed - previousSpeedSample) * dt;
            previousSpeedSample = speed;
            targetRollAngle += speed * dt / rollRadius * Mathf.Rad2Deg;
            targetRollAngle += deltaAngle * 12f;

            cumulativeRollAngle = Mathf.SmoothDampAngle(cumulativeRollAngle, targetRollAngle, ref rollAngleVelocity,
                RotationSmoothTime);
            transform.rotation = baseRotation * Quaternion.AngleAxis(cumulativeRollAngle, currentSpinAxis);
        }

        public void TryNudge(MainMenuTableDecorRollMotion other)
        {
            if (other == null || other == this || !IsRolling || !other.IsRolling)
            {
                return;
            }

            Vector3 delta = other.displayPosition - displayPosition;
            delta.y = 0f;
            float distance = delta.magnitude;
            float minDistance = contactRadius + other.contactRadius;

            if (distance >= minDistance || distance < 0.0001f)
            {
                return;
            }

            Vector3 normal = delta / distance;
            float push = Random.Range(0.18f, 0.42f);
            targetVelocity -= normal * push;
            other.targetVelocity += normal * push * 0.85f;
            targetVelocity = Vector3.ClampMagnitude(targetVelocity, 1.45f);
            other.targetVelocity = Vector3.ClampMagnitude(other.targetVelocity, 1.45f);
        }
    }
}
