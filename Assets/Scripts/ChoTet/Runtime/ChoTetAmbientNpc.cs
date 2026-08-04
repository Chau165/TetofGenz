using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>
    /// Idle / wander / chat behaviour for market and temple crowd NPCs.
    /// </summary>
    public class ChoTetAmbientNpc : MonoBehaviour
    {
        public Vector3 homePosition;
        public float wanderRadius = 2.4f;
        public float walkSpeed = 1.15f;
        public bool stayNearHome;

        private static readonly string[] MarketChat =
        {
            "Năm mới vui vẻ!",
            "Hàng này ngon quá!",
            "Giá hôm nay ổn đấy.",
            "Mua về cúng ông bà thôi.",
            "Đông vui quá ha!"
        };

        private static readonly string[] TempleChat =
        {
            "Cầu bình an năm mới.",
            "Thắp hương xong rồi.",
            "Chùa đông quá.",
            "Mình xin quẻ đi.",
            "Năm nay hanh thông."
        };

        private enum State
        {
            Idle,
            Walk,
            Talk
        }

        private Animator animator;
        private State state = State.Idle;
        private float stateTimer;
        private Vector3 walkTarget;
        private ChoTetAmbientNpc talkPartner;
        private TextMesh chatBubble;
        private Transform bubbleRoot;
        private bool isTempleNpc;
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveAmountHash = Animator.StringToHash("MoveAmount");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

        private void Start()
        {
            homePosition = transform.position;
            animator = GetComponentInChildren<Animator>(true);
            isTempleNpc = name.StartsWith("Temple", System.StringComparison.Ordinal);
            stayNearHome = name.StartsWith("Vendor_", System.StringComparison.Ordinal);
            if (stayNearHome)
            {
                wanderRadius = 0.35f;
            }

            EnsureChatBubble();
            EnterIdle(Random.Range(1.5f, 4f));
            ForceIdlePose();
        }

        private void Update()
        {
            stateTimer -= Time.deltaTime;
            switch (state)
            {
                case State.Idle:
                    TickIdle();
                    break;
                case State.Walk:
                    TickWalk();
                    break;
                case State.Talk:
                    TickTalk();
                    break;
            }
        }

        private void TickIdle()
        {
            SetMoving(false);
            if (stateTimer > 0f)
            {
                return;
            }

            if (!stayNearHome && Random.value < 0.45f && TryBeginTalk())
            {
                return;
            }

            if (wanderRadius > 0.4f && Random.value < 0.7f)
            {
                EnterWalk();
            }
            else
            {
                EnterIdle(Random.Range(2f, 5f));
            }
        }

        private void TickWalk()
        {
            Vector3 flat = walkTarget - transform.position;
            flat.y = 0f;
            float distance = flat.magnitude;
            if (distance < 0.12f || stateTimer <= 0f)
            {
                EnterIdle(Random.Range(1.2f, 3.5f));
                return;
            }

            Vector3 step = flat.normalized * (walkSpeed * Time.deltaTime);
            transform.position += step;
            if (flat.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(flat.normalized, Vector3.up),
                    Time.deltaTime * 8f);
            }

            SetMoving(true);
        }

        private void TickTalk()
        {
            SetMoving(false);
            if (talkPartner != null)
            {
                Vector3 toPartner = talkPartner.transform.position - transform.position;
                toPartner.y = 0f;
                if (toPartner.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(toPartner.normalized, Vector3.up),
                        Time.deltaTime * 6f);
                }
            }

            if (stateTimer <= 0f)
            {
                HideBubble();
                talkPartner = null;
                EnterIdle(Random.Range(1.5f, 3f));
            }
        }

        private bool TryBeginTalk()
        {
            ChoTetAmbientNpc[] others = FindObjectsByType<ChoTetAmbientNpc>(FindObjectsInactive.Exclude);
            ChoTetAmbientNpc nearest = null;
            float best = 3.2f;
            for (int i = 0; i < others.Length; i++)
            {
                ChoTetAmbientNpc other = others[i];
                if (other == null || other == this || other.state == State.Talk)
                {
                    continue;
                }

                float d = Vector3.Distance(transform.position, other.transform.position);
                if (d < best)
                {
                    best = d;
                    nearest = other;
                }
            }

            if (nearest == null)
            {
                return false;
            }

            talkPartner = nearest;
            nearest.talkPartner = this;
            string line = PickLine();
            string reply = PickLine();
            ShowBubble(line);
            nearest.ShowBubble(reply);
            EnterTalk(Random.Range(2.5f, 4.5f));
            nearest.EnterTalk(Random.Range(2.5f, 4.5f));
            return true;
        }

        private void EnterIdle(float duration)
        {
            state = State.Idle;
            stateTimer = duration;
            SetMoving(false);
            HideBubble();
        }

        private void EnterWalk()
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            walkTarget = homePosition + new Vector3(offset.x, 0f, offset.y);
            walkTarget.y = homePosition.y;
            state = State.Walk;
            stateTimer = Random.Range(2f, 5f);
            SetMoving(true);
        }

        private void EnterTalk(float duration)
        {
            state = State.Talk;
            stateTimer = duration;
            SetMoving(false);
        }

        private void SetMoving(bool moving)
        {
            if (animator == null)
            {
                return;
            }

            float amount = moving ? 0.55f : 0f;
            animator.SetBool(IsMovingHash, moving);
            animator.SetFloat(SpeedHash, amount);
            animator.SetFloat(MoveAmountHash, amount);
            animator.SetBool(IsRunningHash, false);
        }

        private void ForceIdlePose()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            animator.applyRootMotion = false;
            SetMoving(false);
            animator.Update(0f);
        }

        private string PickLine()
        {
            string[] pool = isTempleNpc ? TempleChat : MarketChat;
            return pool[Random.Range(0, pool.Length)];
        }

        private void EnsureChatBubble()
        {
            GameObject root = new GameObject("ChatBubble");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            bubbleRoot = root.transform;

            chatBubble = root.AddComponent<TextMesh>();
            chatBubble.anchor = TextAnchor.MiddleCenter;
            chatBubble.alignment = TextAlignment.Center;
            chatBubble.fontSize = 28;
            chatBubble.characterSize = 0.045f;
            chatBubble.color = new Color(1f, 0.98f, 0.9f, 1f);
            chatBubble.fontStyle = FontStyle.Bold;
            chatBubble.text = string.Empty;
            root.AddComponent<QuestLabelBillboard>();
            root.SetActive(false);
        }

        private void ShowBubble(string text)
        {
            if (bubbleRoot == null || chatBubble == null)
            {
                return;
            }

            chatBubble.text = text;
            bubbleRoot.gameObject.SetActive(true);
        }

        private void HideBubble()
        {
            if (bubbleRoot != null)
            {
                bubbleRoot.gameObject.SetActive(false);
            }
        }
    }
}
