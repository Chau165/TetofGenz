using UnityEngine;

namespace ChoTet.Runtime
{
    [RequireComponent(typeof(BoxCollider))]
    public class ZoneTrigger : MonoBehaviour
    {
        public string zoneId = "Zone";
        public string eventId = "";
        public string eventTitle = "";
        [TextArea(2, 5)] public string eventBody = "";
        public int timePenaltyMinutes = 0;
        public bool triggerOnce = true;
        public bool applyRain = false;
        public string vendorBonusId = "";
        public int familiarityBonus = 1;

        [SerializeField] private bool hasTriggered;

        public bool HasTriggered
        {
            get { return hasTriggered; }
        }

        public void MarkTriggered()
        {
            hasTriggered = true;
        }

        private void Reset()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                box.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerTopDownController player = other.GetComponent<PlayerTopDownController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerTopDownController>();
            }

            if (player == null)
            {
                return;
            }

            GameFlowController flow = FindAnyObjectByType<GameFlowController>();
            if (flow != null)
            {
                flow.HandleZoneTrigger(this);
            }
        }
    }
}
