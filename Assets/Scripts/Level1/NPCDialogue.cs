using System.Collections;
using System.Collections.Generic;
using ChoTet.Level1.HometownArrival;
using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Reusable outdoor NPC interaction — press E near the NPC to open dialogue.</summary>
    [DisallowMultipleComponent]
    public class NPCDialogue : Interactable
    {
        [SerializeField] private string npcName = "NPC";
        [SerializeField] private string[] dialogueLines;
        [SerializeField] private float interactionDistance = 1.55f;
        [SerializeField] private Vector3 interactionCenter = new Vector3(0f, 1.1f, 0f);
        [SerializeField] private Animator animator;

        private bool isRunning;
        private SphereCollider trigger;

        public string NpcName => npcName;
        public Animator Animator => animator;

        private void Reset()
        {
            ConfigureTrigger();
            SetPrompt("Nhấn E để nói chuyện");
        }

        private void Awake()
        {
            ConfigureTrigger();
            if (string.IsNullOrWhiteSpace(Prompt))
            {
                SetPrompt("Nhấn E để nói chuyện");
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }
        }

        public void Configure(string displayName, IReadOnlyList<string> lines, float radius = 1.55f)
        {
            npcName = displayName;
            dialogueLines = lines != null ? new List<string>(lines).ToArray() : System.Array.Empty<string>();
            interactionDistance = radius;
            ConfigureTrigger();
            SetPrompt("Nhấn E để nói chuyện");
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
            if (isRunning || player == null || dialogueLines == null || dialogueLines.Length == 0)
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

            RetroChocolateDialogueUI dialogue = RetroChocolateDialogueUI.Ensure();
            for (int i = 0; i < dialogueLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(dialogueLines[i]))
                {
                    continue;
                }

                yield return dialogue.ShowLine(npcName, dialogueLines[i]);
            }

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
            trigger.radius = interactionDistance;
            trigger.center = interactionCenter;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.85f, 0.45f, 0.35f);
            Vector3 worldCenter = transform.TransformPoint(interactionCenter);
            float worldRadius = interactionDistance *
                                Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            Gizmos.DrawWireSphere(worldCenter, worldRadius);
        }
#endif
    }
}
