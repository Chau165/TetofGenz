using UnityEngine;

namespace ChoTet.Runtime
{
    public class DialogueManager : MonoBehaviour
    {
        public GameHudController hud;

        private void Awake()
        {
            if (hud == null)
            {
                hud = FindAnyObjectByType<GameHudController>();
            }
        }

        public void BeginDialogue(NPCDialogueTrigger trigger, GameFlowController flow)
        {
            if (trigger == null || flow == null)
            {
                return;
            }

            if (hud == null)
            {
                hud = FindAnyObjectByType<GameHudController>();
            }

            if (hud == null)
            {
                return;
            }

            string[] lines = trigger.GetDialogueLines(flow);
            if (lines == null || lines.Length == 0)
            {
                lines = new[] { "..." };
            }

            hud.ShowNpcDialogue(
                trigger.npcName,
                trigger.portrait,
                lines,
                () => trigger.OnDialogueFinished(flow)
            );
        }
    }
}
