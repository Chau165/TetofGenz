using UnityEngine;

namespace ChoTet.Runtime
{
    public class TempleInteractionPoint : Interactable
    {
        public enum TempleInteractionKind
        {
            Offering,
            Incense
        }

        public TempleInteractionKind interactionKind;
        public TempleQuestController controller;

        public override bool CanInteract(GameFlowController flow)
        {
            flow = ResolveFlow(flow);
            return base.CanInteract(flow) && flow != null && flow.IsTemplePhase;
        }

        public override void Interact(GameFlowController flow, PlayerTopDownController player)
        {
            flow = ResolveFlow(flow);
            if (controller == null)
            {
                controller = FindAnyObjectByType<TempleQuestController>();
            }

            if (controller == null)
            {
                return;
            }

            switch (interactionKind)
            {
                case TempleInteractionKind.Offering:
                    controller.HandleOffering();
                    break;
                case TempleInteractionKind.Incense:
                    controller.HandleIncense();
                    break;
            }
        }

        private static GameFlowController ResolveFlow(GameFlowController flow)
        {
            return flow != null ? flow : FindAnyObjectByType<GameFlowController>();
        }
    }
}
