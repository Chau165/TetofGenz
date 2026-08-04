using UnityEngine;

namespace ChoTet.Runtime
{
    public class NPCDialogueTrigger : Interactable
    {
        public string npcName = "NPC";
        public Sprite portrait;
        [TextArea(2, 5)] public string[] dialogueLines;
        [TextArea(2, 5)] public string[] repeatLines;

        public override void Interact(GameFlowController flow, PlayerTopDownController player)
        {
            if (flow != null)
            {
                flow.HandleNpcDialogue(this);
            }
        }

        public virtual string[] GetDialogueLines(GameFlowController flow)
        {
            if (dialogueLines != null && dialogueLines.Length > 0)
            {
                return dialogueLines;
            }

            return new[] { "..." };
        }

        public virtual string[] GetRepeatLines(GameFlowController flow)
        {
            if (repeatLines != null && repeatLines.Length > 0)
            {
                return repeatLines;
            }

            return GetDialogueLines(flow);
        }

        public virtual void OnDialogueFinished(GameFlowController flow)
        {
        }
    }
}
