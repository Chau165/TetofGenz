using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Simple automatic sliding glass door for low-poly storefront facades.</summary>
    public class SlidingGlassDoor : MonoBehaviour
    {
        [SerializeField] private Transform leftPanel;
        [SerializeField] private Transform rightPanel;
        [SerializeField] private float openDistance = 0.82f;
        [SerializeField] private float moveSpeed = 2.4f;
        [SerializeField] private Collider proximityTrigger;

        private Vector3 leftClosedLocalPos;
        private Vector3 rightClosedLocalPos;
        private bool isOpen;
        private int occupants;

        private void Awake()
        {
            if (leftPanel != null)
            {
                leftClosedLocalPos = leftPanel.localPosition;
            }

            if (rightPanel != null)
            {
                rightClosedLocalPos = rightPanel.localPosition;
            }

            if (proximityTrigger == null)
            {
                proximityTrigger = GetComponentInChildren<Collider>();
            }

            if (proximityTrigger != null)
            {
                proximityTrigger.isTrigger = true;
            }
        }

        private void Update()
        {
            if (leftPanel == null || rightPanel == null)
            {
                return;
            }

            Vector3 leftTarget = leftClosedLocalPos + (isOpen ? Vector3.left * openDistance : Vector3.zero);
            Vector3 rightTarget = rightClosedLocalPos + (isOpen ? Vector3.right * openDistance : Vector3.zero);
            float step = moveSpeed * Time.deltaTime;
            leftPanel.localPosition = Vector3.MoveTowards(leftPanel.localPosition, leftTarget, step);
            rightPanel.localPosition = Vector3.MoveTowards(rightPanel.localPosition, rightTarget, step);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            occupants++;
            isOpen = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            occupants = Mathf.Max(0, occupants - 1);
            if (occupants == 0)
            {
                isOpen = false;
            }
        }

        public void Configure(Transform left, Transform right, Collider trigger)
        {
            leftPanel = left;
            rightPanel = right;
            proximityTrigger = trigger;
            if (leftPanel != null)
            {
                leftClosedLocalPos = leftPanel.localPosition;
            }

            if (rightPanel != null)
            {
                rightClosedLocalPos = rightPanel.localPosition;
            }
        }
    }
}
