using System;
using System.Text;
using UnityEngine;

namespace ChoTet.Runtime
{
    public class TempleQuestController : MonoBehaviour
    {
        public GameFlowController flow;
        public GameHudController hud;

        private ParticleSystem[] incenseSmoke;
        private Light[] incenseLights;
        private bool introShown;
        private readonly System.Random fortuneRandom = new System.Random();

        private void Awake()
        {
            if (flow == null)
            {
                flow = FindAnyObjectByType<GameFlowController>();
            }

            if (hud == null)
            {
                hud = FindAnyObjectByType<GameHudController>();
            }

            CacheEffects();
            // White incense smoke stays on by default; warm light turns on after the player lights incense.
            SetIncenseSmoke(true);
            SetIncenseLights(false);

            if (GetComponent<TempleQuestLayout>() == null && FindAnyObjectByType<TempleQuestLayout>() == null)
            {
                gameObject.AddComponent<TempleQuestLayout>();
            }
        }

        public void BeginTempleQuest()
        {
            if (flow == null)
            {
                flow = FindAnyObjectByType<GameFlowController>();
            }

            if (flow == null)
            {
                return;
            }

            if (!flow.dayState.templeQuestStarted)
            {
                flow.StartTempleQuest();
            }

            if (introShown)
            {
                return;
            }

            introShown = true;
            SpeakGuide("Mẹ", "Con giỏi lắm. Bây giờ cùng mẹ đi chùa đầu năm nhé. Đến cổng chùa rồi nhấn E.");
            ShowDialog(
                "Mẹ",
                "Con giỏi lắm. Bây giờ cùng mẹ đi chùa đầu năm nhé.",
                () => ShowDialog("Cập nhật nhiệm vụ", "Đến cổng chùa, nhấn E để vào. Trong chính điện có bàn dâng lễ, lư hương và ống xin quẻ.", null)
            );
        }

        public void HandleTempleGateReached()
        {
            if (!IsTempleActive() || flow.dayState.templeGateVisited)
            {
                return;
            }

            flow.SetCurrentZone("Temple");
            flow.MarkTempleGateVisited();
            SpeakGuide("Hướng dẫn", "Chào mừng đến chùa Xuân An. Hãy vào chính điện và dâng lễ phẩm Tết trên bàn lễ.");
            ShowDialog("Chùa Xuân An", "Chúc Mừng Năm Mới. Hãy vào chính điện dâng lễ phẩm Tết.", null);
        }

        public void HandleOffering()
        {
            if (!IsTempleActive())
            {
                return;
            }

            if (!flow.dayState.templeGateVisited)
            {
                SpeakGuide("Hướng dẫn", "Hãy qua cổng chùa trước đã nhé.");
                ShowDialog("Chưa qua cổng chùa", "Đứng gần cổng và nhấn E để vào chùa đầu năm.", null);
                return;
            }

            if (flow.dayState.templeOfferingDone)
            {
                SpeakGuide("Hướng dẫn", "Lễ đã dâng rồi. Hãy thắp hương cầu bình an.");
                ShowDialog("Đã dâng lễ", "Lễ đầu năm đã được đặt lên ban thờ rồi. Hãy thắp hương.", null);
                return;
            }

            flow.MarkTempleOfferingDone();
            SpeakGuide("Hướng dẫn", "Dâng lễ xong rồi. Tiếp theo hãy thắp hương trước tượng Phật.");
            ShowDialog("Dâng lễ phẩm Tết", BuildOfferingText(), () =>
            {
                ShowDialog("Sư thầy", "Lễ vật quý nhất chính là tấm lòng thành. Hãy thắp hương cầu bình an.", null);
            });
        }

        public void HandleIncense()
        {
            if (!IsTempleActive())
            {
                return;
            }

            if (!flow.dayState.templeOfferingDone)
            {
                SpeakGuide("Hướng dẫn", "Con hãy dâng lễ phẩm Tết trước khi thắp hương.");
                ShowDialog("Chưa dâng lễ", "Hãy dâng lễ phẩm Tết trên bàn lễ trước đã.", null);
                return;
            }

            if (flow.dayState.incenseLit)
            {
                SpeakGuide("Hướng dẫn", "Hương đã thắp. Hãy xin quẻ đầu năm bên ống quẻ.");
                ShowDialog("Hương đã thắp", "Khói hương đã bay lên rồi, giờ con có thể xin quẻ đầu năm.", null);
                return;
            }

            flow.MarkTempleIncenseLit();
            CacheEffects();
            SetIncenseSmoke(true);
            SetIncenseLights(true);
            SpeakGuide("Hướng dẫn", "Thắp hương xong. Hãy sang ống quẻ để xin quẻ đầu năm.");
            ShowDialog("Sư thầy", "Chúc con và gia đình một năm mới bình an, hạnh phúc. Hãy xin quẻ đầu năm.", null);
        }

        public void HandleFortune()
        {
            if (!IsTempleActive())
            {
                return;
            }

            if (!flow.dayState.incenseLit)
            {
                SpeakGuide("Hướng dẫn", "Con hãy thắp hương cầu bình an trước khi xin quẻ.");
                ShowDialog("Chưa thắp hương", "Hãy thắp hương trước tượng Phật trước đã.", null);
                return;
            }

            if (flow.dayState.fortuneDrawn)
            {
                ShowDialog("Đã xin quẻ", "Quẻ đầu năm đã được rút rồi. Hãy mang lời chúc tốt đẹp này về nhà.", null);
                return;
            }

            flow.MarkTempleFortuneDrawn();
            string fortuneText = RollFortuneText();
            SpeakGuide("Hướng dẫn", "Xin quẻ xong. Chúc gia đình một năm mới an lành.");
            ShowDialog("Xin quẻ đầu năm", fortuneText, () =>
            {
                ShowDialog(
                    "Mẹ",
                    "Cảm ơn con đã cùng gia đình chuẩn bị một cái Tết thật ý nghĩa.",
                    () => flow.ShowTempleFinalEvaluation("Sau một ngày đi chợ Tết hối hả, chạy xe về nhà đông đúc và viếng chùa đầu năm thanh tịnh, ngày Tết của gia đình đã thật sự trọn vẹn và ý nghĩa.")
                );
            });
        }

        private static void SpeakGuide(string speaker, string text)
        {
            VoiceNarrator.Instance.Speak(speaker, text);
        }

        private bool IsTempleActive()
        {
            if (flow == null)
            {
                flow = FindAnyObjectByType<GameFlowController>();
            }

            return flow != null && flow.IsTemplePhase && !flow.dayState.evaluationShown;
        }

        private void CacheEffects()
        {
            incenseSmoke = Array.FindAll(
                FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include),
                particles => particles != null && particles.gameObject.name.Contains("TempleIncenseSmoke")
            );

            incenseLights = Array.FindAll(
                FindObjectsByType<Light>(FindObjectsInactive.Include),
                light => light != null && light.gameObject.name.Contains("TempleIncenseWarmLight")
            );
        }

        private void SetIncenseSmoke(bool active)
        {
            if (incenseSmoke == null)
            {
                return;
            }

            for (int i = 0; i < incenseSmoke.Length; i++)
            {
                if (incenseSmoke[i] == null)
                {
                    continue;
                }

                if (active)
                {
                    incenseSmoke[i].Play(true);
                }
                else
                {
                    incenseSmoke[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        private void SetIncenseLights(bool active)
        {
            if (incenseLights == null)
            {
                return;
            }

            for (int i = 0; i < incenseLights.Length; i++)
            {
                if (incenseLights[i] != null)
                {
                    incenseLights[i].enabled = active;
                }
            }
        }

        private string BuildOfferingText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("✓ Dưa hấu");
            builder.AppendLine("✓ Bánh chưng");
            builder.AppendLine("✓ Hoa mai");
            builder.AppendLine("✓ Giỏ quà");

            if (flow != null && !flow.dayState.isShoppingComplete)
            {
                builder.Clear();
                builder.AppendLine("✓ Lễ vật đầu năm");
            }

            return builder.ToString();
        }

        private string RollFortuneText()
        {
            switch (fortuneRandom.Next(0, 3))
            {
                case 0:
                    return "Quẻ Tài Lộc\nChăm chỉ và nỗ lực sẽ mang lại thành công.";
                case 1:
                    return "Quẻ Bình An\nGia đình hòa thuận, mọi việc hanh thông.";
                default:
                    return "Quẻ Học Hành\nKiên trì học tập sẽ đạt thành tích tốt.";
            }
        }

        private void ShowDialog(string title, string body, Action onContinue)
        {
            if (hud == null)
            {
                hud = FindAnyObjectByType<GameHudController>();
            }

            if (hud != null)
            {
                hud.ShowDialog(title, body, onContinue);
            }
        }
    }
}
