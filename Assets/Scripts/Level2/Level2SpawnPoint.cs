using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level2
{
    /// <summary>Optional spawn marker for Level 2 scenes. Disabled by default on hometown arrival (controller handles spawn).</summary>
    public class Level2SpawnPoint : MonoBehaviour
    {
        [SerializeField] private bool applyOnStart;

        private void Start()
        {
            if (!applyOnStart)
            {
                return;
            }

            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            if (player != null)
            {
                player.TeleportTo(transform.position, transform.rotation);
            }
        }
    }
}
