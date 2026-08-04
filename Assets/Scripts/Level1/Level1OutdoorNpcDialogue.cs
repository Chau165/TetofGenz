using System.Collections;
using System.Collections.Generic;
using ChoTet.Level1.HometownArrival;
using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Press E near an outdoor crowd NPC to open the Retro Chocolate dialogue box.</summary>
    [DisallowMultipleComponent]
    public class Level1OutdoorNpcDialogue : Interactable
    {
        [SerializeField] private string dialogueKey = "StallSeller";
        [SerializeField] private float interactionRadius = 1.35f;
        [SerializeField] private Vector3 interactionCenter = new Vector3(0f, 1.1f, 0f);

        private bool hasSpoken;
        private bool isRunning;
        private SphereCollider trigger;

        public string DialogueKey => dialogueKey;

        private void Reset()
        {
            ConfigureTrigger();
            SetPrompt("Nhấn E để trò chuyện");
        }

        private void Awake()
        {
            ConfigureTrigger();
            if (string.IsNullOrWhiteSpace(Prompt))
            {
                SetPrompt("Nhấn E để trò chuyện");
            }
        }

        public void Configure(string key)
        {
            dialogueKey = key;
            ConfigureTrigger();
            SetPrompt("Nhấn E để trò chuyện");
        }

        public override bool CanInteract()
        {
            if (!base.CanInteract() || isRunning || RetroChocolateDialogueUI.IsOpen)
            {
                return false;
            }

            Level1PhaseController phase = Level1PhaseController.Instance;
            if (phase == null)
            {
                phase = FindAnyObjectByType<Level1PhaseController>();
            }

            return phase != null && phase.CurrentPhase == Level1PhaseController.Level1Phase.OutdoorStreet;
        }

        public override void Interact(PlayerTopDownController player)
        {
            if (isRunning || player == null)
            {
                return;
            }

            StartCoroutine(RunDialogue(player));
        }

        private IEnumerator RunDialogue(PlayerTopDownController player)
        {
            isRunning = true;
            player.MovementLocked = true;
            player.StopScriptedLocomotion();

            InventoryUIController ui = FindAnyObjectByType<InventoryUIController>();
            ui?.SetPrompt(string.Empty);

            IReadOnlyList<(string speaker, string line)> lines = hasSpoken
                ? Level1OutdoorNpcDialogueTable.GetRepeatLines(dialogueKey)
                : Level1OutdoorNpcDialogueTable.GetFirstLines(dialogueKey);

            RetroChocolateDialogueUI dialogue = RetroChocolateDialogueUI.Ensure();
            for (int i = 0; i < lines.Count; i++)
            {
                yield return dialogue.ShowLine(lines[i].speaker, lines[i].line);
            }

            hasSpoken = true;
            player.MovementLocked = false;
            isRunning = false;
        }

        private void ConfigureTrigger()
        {
            trigger = GetComponent<SphereCollider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<SphereCollider>();
            }

            trigger.isTrigger = true;
            trigger.radius = interactionRadius;
            trigger.center = interactionCenter;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.95f, 0.75f, 0.2f, 0.35f);
            Vector3 worldCenter = transform.TransformPoint(interactionCenter);
            float worldRadius = interactionRadius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            Gizmos.DrawWireSphere(worldCenter, worldRadius);
        }
#endif
    }
}
