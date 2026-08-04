using UnityEngine;

namespace ChoTet.Runtime
{
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private string prompt = "Nhan E de tuong tac";

        public string Prompt
        {
            get { return prompt; }
        }

        public void SetPrompt(string value)
        {
            prompt = value;
        }

        public virtual bool CanInteract(GameFlowController flow)
        {
            return enabled && gameObject.activeInHierarchy;
        }

        public virtual bool CanInteract()
        {
            return CanInteract(null);
        }

        public virtual void Interact(PlayerTopDownController player)
        {
            GameFlowController flow = FindFirstObjectByType<GameFlowController>();
            if (flow != null)
            {
                Interact(flow, player);
            }
        }

        public virtual void Interact(GameFlowController flow, PlayerTopDownController player)
        {
        }
    }
}
