using UnityEngine;

namespace ChoTet.Runtime
{
    public class StoryInteractable : Interactable
    {
        public string interactionId = "mission_giver";
        public string titleText = "Me";
        [TextArea(2, 5)] public string bodyText = "Con nho di cho Tet.";

        public override void Interact(GameFlowController flow, PlayerTopDownController player)
        {
            if (flow != null)
            {
                flow.HandleStoryInteraction(this);
            }
        }
    }
}
