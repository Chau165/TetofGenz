using UnityEngine;

namespace ChoTet.Runtime
{
    /// <summary>Press E at the temple gate to begin the courtyard visit.</summary>
    public class TempleGateInteractable : Interactable
    {
        public TempleQuestController controller;

        public override bool CanInteract(GameFlowController flow)
        {
            flow = ResolveFlow(flow);
            if (!base.CanInteract(flow) || flow == null || !flow.IsTemplePhase)
            {
                return false;
            }

            return !flow.dayState.templeGateVisited;
        }

        public override void Interact(GameFlowController flow, PlayerTopDownController player)
        {
            flow = ResolveFlow(flow);
            if (controller == null)
            {
                controller = FindAnyObjectByType<TempleQuestController>();
            }

            if (controller != null)
            {
                controller.HandleTempleGateReached();
            }
        }

        private static GameFlowController ResolveFlow(GameFlowController flow)
        {
            return flow != null ? flow : FindAnyObjectByType<GameFlowController>();
        }
    }
}
