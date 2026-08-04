using System.Collections.Generic;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Ensures outdoor crowd NPCs have dialogue triggers when the outdoor area activates.</summary>
    public class Level1OutdoorNpcDialogueBootstrap : MonoBehaviour
    {
        private static readonly Dictionary<string, string> NameToDialogueKey = new Dictionary<string, string>
        {
            { "CrowdNPC_StallSeller", "StallSeller" },
            { "CrowdNPC_FlowerViewer", "FlowerViewer" },
            { "CrowdNPC_TalkA", "TalkPair" },
            { "CrowdNPC_TalkB", "TalkPair" },
            { "CrowdNPC_CasualAlone", "CasualAlone" },
            { "CrowdNPC_CasualSecond", "CasualSecond" },
            { "CrowdNPC_PatrolA", "PatrolA" },
            { "CrowdNPC_PatrolB", "PatrolB" },
        };

        private void OnEnable()
        {
            EnsureDialogues();
        }

        public void EnsureDialogues()
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || child == transform)
                {
                    continue;
                }

                if (!NameToDialogueKey.TryGetValue(child.name, out string dialogueKey))
                {
                    continue;
                }

                Level1OutdoorNpcDialogue dialogue = child.GetComponent<Level1OutdoorNpcDialogue>();
                if (dialogue == null)
                {
                    dialogue = child.gameObject.AddComponent<Level1OutdoorNpcDialogue>();
                }

                dialogue.Configure(dialogueKey);
            }
        }
    }
}
