using UnityEngine;

namespace ChoTet.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class TrafficObstacle : MonoBehaviour
    {
        public float speed = 7.5f;
        public int damageAmount = 8;
        public float knockbackDistance = 0.85f;
        public Vector3 travelDirection = Vector3.right;
        public Vector3 roadBoundsCenter = new Vector3(0f, 0f, 56f);
        public Vector3 roadBoundsSize = new Vector3(24f, 8f, 48f);

        private GameFlowController flow;
        private TrafficSpawner spawner;
        private bool hitPlayer;
        private Vector3 movementDirection = Vector3.right;
        private bool hasHonked;
        private AudioSource engineAudio;

        private void Awake()
        {
            flow = FindAnyObjectByType<GameFlowController>();
            spawner = GetComponentInParent<TrafficSpawner>();

            Collider obstacleCollider = GetComponent<Collider>();
            if (obstacleCollider != null)
            {
                obstacleCollider.isTrigger = true;
            }

            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;

            engineAudio = gameObject.AddComponent<AudioSource>();
            engineAudio.playOnAwake = false;
            engineAudio.loop = true;
            engineAudio.spatialBlend = 1f; // 3D sound
            engineAudio.minDistance = 2f;
            engineAudio.maxDistance = 20f;
            engineAudio.volume = 0.28f;
            engineAudio.clip = ProceduralAudio.GetTrafficHumClip();
            engineAudio.pitch = Random.Range(0.9f, 1.15f);
            engineAudio.Play();
        }

        private void Start()
        {
            movementDirection = travelDirection.sqrMagnitude > 0.001f ? travelDirection.normalized : Vector3.right;
            transform.rotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        }

        public void Configure(
            TrafficSpawner owner,
            float movementSpeed,
            int damage,
            Vector3 boundsCenter,
            Vector3 boundsSize,
            Vector3 direction)
        {
            spawner = owner;
            speed = movementSpeed;
            damageAmount = damage;
            roadBoundsCenter = boundsCenter;
            roadBoundsSize = boundsSize;
            travelDirection = direction;
            movementDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.right;
            hitPlayer = false;
            hasHonked = false;
            transform.rotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        }

        private void Update()
        {
            transform.position += movementDirection * speed * Time.deltaTime;

            if (!hasHonked && flow != null && flow.player != null)
            {
                float dist = Vector3.Distance(transform.position, flow.player.transform.position);
                if (dist < 8.5f && Random.value < 0.003f)
                {
                    hasHonked = true;
                    PlayHorn();
                }
            }

            Vector3 delta = transform.position - roadBoundsCenter;
            if (Mathf.Abs(delta.x) > roadBoundsSize.x * 0.5f || Mathf.Abs(delta.z) > roadBoundsSize.z * 0.5f)
            {
                if (spawner != null)
                {
                    spawner.ReleaseObstacle(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        private void PlayHorn()
        {
            if (engineAudio != null)
            {
                AudioClip hornClip = ProceduralAudio.GetHornClip();
                if (hornClip != null)
                {
                    engineAudio.PlayOneShot(hornClip, 0.45f);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hitPlayer)
            {
                return;
            }

            PlayerTopDownController player = other.GetComponent<PlayerTopDownController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerTopDownController>();
            }

            if (player == null)
            {
                return;
            }

            if (flow == null)
            {
                flow = FindAnyObjectByType<GameFlowController>();
            }

            hitPlayer = true;
            Vector3 pushDirection = player.transform.position - transform.position;
            player.ApplyTrafficKnockback(pushDirection, knockbackDistance);

            if (flow != null)
            {
                // Always costs a life on the market road; delivery cargo also takes a hit on the ride home.
                flow.RegisterTrafficHit(damageAmount);
            }
        }
    }
}
