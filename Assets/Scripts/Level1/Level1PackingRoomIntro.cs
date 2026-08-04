using System.Collections;
using ChoTet.Level1.HometownArrival;
using ChoTet.Runtime;
using UnityEngine;

namespace ChoTet.Level1
{
    /// <summary>Shows the Packing Room objective before the player can move.</summary>
    [DisallowMultipleComponent]
    public sealed class Level1PackingRoomIntro : MonoBehaviour
    {
        private bool hasPlayed;

        private IEnumerator Start()
        {
            if (hasPlayed)
            {
                yield break;
            }

            hasPlayed = true;
            yield return null;

            PlayerTopDownController player = FindAnyObjectByType<PlayerTopDownController>();
            if (player != null)
            {
                player.MovementLocked = true;
                player.StopScriptedLocomotion();
            }

            InventoryUIController inventoryUi = FindAnyObjectByType<InventoryUIController>();
            inventoryUi?.SetPrompt(string.Empty);

            RetroChocolateDialogueUI dialogue = RetroChocolateDialogueUI.Ensure();
            yield return dialogue.ShowSequence(
                ("Tôi", "Yeahhh, cuối cùng mình cũng đã được nghỉ Tết rồi! Mình phải nhanh thu dọn đồ đạc để còn ra đón xe về quê nữa."),
                ("Tôi", "Mình cần phải bắt đầu thu xếp, lấy 6 món đồ để bỏ vào balo."),
                ("Tôi", "Lấy đồ xong mình sẽ ra khỏi nhà đến bến xe, mình phải nhanh lên thôi."));

            if (player != null)
            {
                player.MovementLocked = false;
            }
        }
    }
}
