using ChoTet.Data;
using UnityEngine;

namespace ChoTet.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class VendorStall : Interactable
    {
        public VendorProfile vendorProfile;
        public string soldPrompt = "Mon nay da duoc mua";
        public string availablePrompt = "Nhan E de tra gia";

        [SerializeField] private bool sold;

        public bool IsSold
        {
            get { return sold; }
        }

        public VendorProfile VendorProfile
        {
            get { return vendorProfile; }
        }

        public override void Interact(GameFlowController flow, PlayerTopDownController player)
        {
            if (flow != null)
            {
                flow.HandleVendorInteraction(this);
            }
        }

        public void SetSold(bool value)
        {
            sold = value;
            SetPrompt(sold ? soldPrompt : availablePrompt);
        }

        private void Reset()
        {
            Collider targetCollider = GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetCollider.isTrigger = true;
            }

            SetPrompt(availablePrompt);
        }
    }
}
