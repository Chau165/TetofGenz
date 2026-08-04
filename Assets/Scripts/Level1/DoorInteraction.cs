using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>
    /// Departure door for Level 1 Phase 1. When packing is complete, transitions to Phase 2
    /// (outdoor street) inside the same Level 1 scene — does not load Level 2.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DoorInteraction : Interactable
    {
        [SerializeField] private bool isLocked = true;
        [SerializeField] private bool transitionUsed;

        private void Reset()
        {
            Collider targetCollider = GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetCollider.isTrigger = true;
            }

            SetPrompt("Cửa đã khóa");
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;
            if (transitionUsed)
            {
                SetPrompt(string.Empty);
                return;
            }

            SetPrompt(locked ? "Cửa đã khóa" : "Nhấn E để ra ngoài");
        }

        public override bool CanInteract()
        {
            return enabled && gameObject.activeInHierarchy && !transitionUsed;
        }

        public override void Interact(PlayerTopDownController player)
        {
            InventoryManager inventory = InventoryManager.Instance;
            InventoryUIController ui = FindAnyObjectByType<InventoryUIController>();
            if (inventory == null || ui == null || player == null)
            {
                return;
            }

            if (!inventory.HasAllRequiredItems())
            {
                ui.ShowPickupPopup("Bạn vẫn cần dọn xong hành lý đã.");
                return;
            }

            Level1PhaseController phaseController = Level1PhaseController.Instance;
            if (phaseController == null)
            {
                phaseController = FindAnyObjectByType<Level1PhaseController>();
            }

            if (phaseController == null)
            {
                Debug.LogWarning("[DoorInteraction] Level1PhaseController missing — cannot start outdoor phase.");
                ui.ShowPickupPopup("Cửa chưa thể mở lúc này.");
                return;
            }

            if (!phaseController.BeginOutdoorTransition(player))
            {
                ui.ShowPickupPopup("Cửa chưa thể mở lúc này.");
                return;
            }

            transitionUsed = true;
            SetLocked(false);
            SetPrompt(string.Empty);
        }
    }
}
