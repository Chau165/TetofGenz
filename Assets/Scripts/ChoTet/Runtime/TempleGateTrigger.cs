using UnityEngine;

namespace ChoTet.Runtime
{
    [RequireComponent(typeof(BoxCollider))]
    public class TempleGateTrigger : MonoBehaviour
    {
        public TempleQuestController controller;

        [SerializeField] private bool triggered;

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
            if (triggered)
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

            if (controller == null)
            {
                controller = FindAnyObjectByType<TempleQuestController>();
            }

            if (controller == null)
            {
                return;
            }

            triggered = true;
            controller.HandleTempleGateReached();
        }
    }
}
