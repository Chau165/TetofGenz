using UnityEngine;

namespace ChoTet.Runtime
{
    public class MotherQuestGiver : NPCDialogueTrigger
    {
        [TextArea(2, 5)] public string[] questIntroLines =
        {
            "Sắp Tết rồi, con giúp mẹ đi chợ mua vài món nhé.",
            "Nhà mình cần bánh mứt, dưa hấu, hoa mai, thịt heo và giỏ quà.",
            "Mẹ đưa con 1.000.000đ, nhớ mua đủ nhưng đừng tiêu hoang nha.",
            "Mua xong thì gửi đồ về nhà cẩn thận, đường hôm nay hơi đông đó."
        };

        [TextArea(2, 5)] public string[] questReminderLines =
        {
            "Con nhớ mua đủ đồ rồi về sớm nha.",
            "Đi đường cẩn thận, đừng để rơi đồ."
        };

        public override string[] GetDialogueLines(GameFlowController flow)
        {
            if (flow != null && flow.dayState != null && flow.dayState.questStarted)
            {
                return GetRepeatLines(flow);
            }

            return questIntroLines;
        }

        public override string[] GetRepeatLines(GameFlowController flow)
        {
            if (questReminderLines != null && questReminderLines.Length > 0)
            {
                return questReminderLines;
            }

            return base.GetRepeatLines(flow);
        }

        public override void OnDialogueFinished(GameFlowController flow)
        {
            if (flow != null && flow.dayState != null && !flow.dayState.questStarted)
            {
                flow.StartMotherQuest();
            }
        }
    }
}
