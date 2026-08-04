using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Patrol between two fixed points using the same locomotion as the player.</summary>
    public class Level1OutdoorNpcPatrol : MonoBehaviour
    {
        [SerializeField] private Vector3 anchor;
        [SerializeField] private float patrolRange = 1.2f;
        [SerializeField] private float moveSpeed = 1.6f;
        [SerializeField] private float minX = -2.55f;
        [SerializeField] private float maxX = 2.55f;
        [SerializeField] private float minZ = -33f;
        [SerializeField] private float maxZ = -6f;
        [SerializeField] private float phaseOffset;
        [SerializeField] private float arriveDistance = 0.18f;

        private Vector3 pointA;
        private Vector3 pointB;
        private Vector3 currentTarget;
        private TopDownScriptedLocomotion locomotion;

        public void Configure(
            Vector3 anchorPosition,
            float range,
            float speed,
            float boundMinX,
            float boundMaxX,
            float boundMinZ,
            float boundMaxZ)
        {
            anchor = anchorPosition;
            patrolRange = Mathf.Max(0.35f, range);
            moveSpeed = Mathf.Max(0.8f, speed);
            minX = boundMinX;
            maxX = boundMaxX;
            minZ = boundMinZ;
            maxZ = boundMaxZ;
            phaseOffset = Random.Range(0f, 1f);
            CacheEndpoints();
            currentTarget = phaseOffset < 0.5f ? pointB : pointA;
        }

        /// <summary>Legacy entry point used by editor setup.</summary>
        public void Setup(
            Vector3 anchorPosition,
            float range,
            float speed,
            float boundMinX,
            float boundMaxX,
            float boundMinZ,
            float boundMaxZ)
        {
            Configure(anchorPosition, range, speed, boundMinX, boundMaxX, boundMinZ, boundMaxZ);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            EnsurePatrolDefaults();
            CacheEndpoints();
            if (currentTarget == Vector3.zero)
            {
                currentTarget = phaseOffset < 0.5f ? pointB : pointA;
            }
        }

        private void Start()
        {
            EnsurePatrolDefaults();
            CacheEndpoints();
            if (currentTarget == Vector3.zero)
            {
                currentTarget = phaseOffset < 0.5f ? pointB : pointA;
            }

            locomotion = GetComponent<TopDownScriptedLocomotion>();
            if (locomotion == null)
            {
                TopDownScriptedLocomotion.EnsurePatrolSetup(gameObject, transform.localScale.x);
                locomotion = GetComponent<TopDownScriptedLocomotion>();
            }
        }

        private void Update()
        {
            if (locomotion == null)
            {
                locomotion = GetComponent<TopDownScriptedLocomotion>();
            }

            if (locomotion == null)
            {
                return;
            }

            EnsurePatrolDefaults();

            Vector3 toTarget = currentTarget - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= arriveDistance * arriveDistance)
            {
                currentTarget = currentTarget == pointA ? pointB : pointA;
                toTarget = currentTarget - transform.position;
                toTarget.y = 0f;
            }

            if (toTarget.sqrMagnitude > 0.0004f)
            {
                locomotion.MoveInDirection(toTarget, moveSpeed);
            }
            else
            {
                locomotion.Stop();
            }
        }

        private void EnsurePatrolDefaults()
        {
            if (patrolRange <= 0.01f)
            {
                patrolRange = 1.2f;
            }

            if (moveSpeed <= 0.01f)
            {
                moveSpeed = 1.6f;
            }

            if (anchor == Vector3.zero)
            {
                anchor = transform.position;
            }

            if (maxX <= minX)
            {
                minX = -2.55f;
                maxX = 2.55f;
            }

            if (maxZ <= minZ)
            {
                minZ = -33f;
                maxZ = -6f;
            }
        }

        private void CacheEndpoints()
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            pointA = ClampPoint(anchor - forward * patrolRange);
            pointB = ClampPoint(anchor + forward * patrolRange);
        }

        private Vector3 ClampPoint(Vector3 point)
        {
            point.x = Mathf.Clamp(point.x, minX, maxX);
            point.z = Mathf.Clamp(point.z, minZ, maxZ);
            point.y = anchor.y;
            return point;
        }
    }
}
