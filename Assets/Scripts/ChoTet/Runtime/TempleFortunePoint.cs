using UnityEngine;

namespace ChoTet.Runtime
{
    public class TempleFortunePoint : Interactable
    {
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

            if (controller != null)
            {
                controller.HandleFortune();
            }
        }

        private static GameFlowController ResolveFlow(GameFlowController flow)
        {
            return flow != null ? flow : FindAnyObjectByType<GameFlowController>();
        }
    }
}
