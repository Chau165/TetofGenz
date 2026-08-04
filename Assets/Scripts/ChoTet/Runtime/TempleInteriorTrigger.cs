using UnityEngine;

namespace ChoTet.Runtime
{
    public class TempleInteriorTrigger : MonoBehaviour
    {
        public GameObject roofGroup;

        private void OnTriggerEnter(Collider other)
        {
            PlayerTopDownController player = other.GetComponent<PlayerTopDownController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerTopDownController>();
            }

            if (player == null)
            {
                return;
            }

            if (roofGroup != null)
            {
                roofGroup.SetActive(false);
            }

        }

        private void OnTriggerExit(Collider other)
        {
            PlayerTopDownController player = other.GetComponent<PlayerTopDownController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerTopDownController>();
            }

            if (player == null)
            {
                return;
            }

            if (roofGroup != null)
            {
                roofGroup.SetActive(true);
            }

        }
    }
}
