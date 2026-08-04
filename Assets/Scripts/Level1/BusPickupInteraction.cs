using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChoTet.Level1
{
    /// <summary>
    /// Interactable at the Level 1 outdoor bus pickup point (Phase 2).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BusPickupInteraction : Interactable
    {
        [SerializeField] private string targetSceneName = "ChoTet_VerticalSlice";

        private bool isLoading;

        private void Awake()
        {
            SetPrompt("Nhấn E để về nhà");
        }

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }

            SetPrompt("Nhấn E để về nhà");
        }

        public override bool CanInteract()
        {
            if (!base.CanInteract())
            {
                return false;
            }

            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            return phase != null &&
                   phase.CurrentPhase == Level1PhaseController.Level1Phase.OutdoorStreet;
        }

        public override void Interact(PlayerTopDownController player)
        {
            if (isLoading)
            {
                return;
            }

            isLoading = true;
            InventoryUIController ui = FindAnyObjectByType<InventoryUIController>();
            ui?.ShowPickupPopup("Heading back home...");
            SceneManager.LoadScene(string.IsNullOrEmpty(targetSceneName) ? "ChoTet_VerticalSlice" : targetSceneName);
        }
    }

}
