using UnityEngine;

namespace ChoTet.Level1.HometownArrival
{
    /// <summary>Legacy trigger from the old walk-to-door ending. Disabled in the arrival scene.</summary>
    [RequireComponent(typeof(Collider))]
    public class HometownHouseEntranceTrigger : MonoBehaviour
    {
        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }
    }
}
