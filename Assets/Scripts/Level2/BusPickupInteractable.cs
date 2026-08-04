using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level2
{
    /// <summary>Legacy street pickup interactable kept for scene compatibility (inactive in hometown arrival).</summary>
    [RequireComponent(typeof(Collider))]
    public class BusPickupInteractable : Interactable
    {
        [SerializeField] private GameObject highlightObject;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        public override bool CanInteract()
        {
            return false;
        }

        public override void Interact(PlayerTopDownController player)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }
    }
}
