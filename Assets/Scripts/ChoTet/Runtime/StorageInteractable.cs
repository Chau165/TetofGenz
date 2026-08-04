using UnityEngine;

namespace ChoTet.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class StorageInteractable : Interactable
    {
        public string storageId = "delivery_bike";
        public string titleText = "Xe ôm chuyển đồ";
        [TextArea(2, 4)] public string bodyText = "Gửi các túi đồ đã sắm về nhà trước để có chỗ sắm tiếp.";

        public override void Interact(GameFlowController flow, PlayerTopDownController player)
        {
            if (flow != null)
            {
                flow.HandleStorageInteraction(this);
            }
        }
    }
}
