using System.Collections.Generic;

namespace ChoTet.Level1
{
    /// <summary>Dialogue lines for outdoor street NPCs (Retro Chocolate chatbox).</summary>
    public static class Level1OutdoorNpcDialogueTable
    {
        public static IReadOnlyList<(string speaker, string line)> GetFirstLines(string dialogueKey)
        {
            return dialogueKey switch
            {
                "StallSeller" => new[]
                {
                    ("Con", "Chú cho con hỏi, quầy hàng chú bán được không ạ?"),
                    ("Chú Ba", "Ồ, cháu về quê đón Tết hả? Rau củ ở đây tươi lắm."),
                    ("Con", "Dạ, con đang về thăm ba mẹ. Họ đợi ở nhà rồi."),
                    ("Chú Ba", "Vậy thì mau đi đón xe. Đường về còn xa đó cháu!"),
                },
                "FlowerViewer" => new[]
                {
                    ("Con", "Hoa đẹp quá cô ơi!"),
                    ("Cô Lan", "Cô trồng mấy chậu này từ sáng. Tết đến ai cũng ghé ngắm."),
                    ("Con", "Con cũng về quê nên thấy hoa xứ nhà ấm lòng ghê."),
                    ("Cô Lan", "Về đúng dịp rồi đấy. Gia đình cháu chắc mong con lắm."),
                },
                "TalkPair" => new[]
                {
                    ("Minh", "Chào cậu! Cũng ra ngoài hóng mát à?"),
                    ("Con", "Ừ, mình vừa xong việc trong nhà. Còn hai bạn?"),
                    ("Lan", "Tụi mình là hàng xóm cũ, tranh thủ nói chuyện trước khi về quê."),
                    ("Con", "Hóa ra vậy. Chúc hai bạn Tết vui vẻ nhé!"),
                },
                "CasualAlone" => new[]
                {
                    ("Anh Tuấn", "Đường về quê đông người ghê nhỉ?"),
                    ("Con", "Anh cũng về quê đón Tết à?"),
                    ("Anh Tuấn", "Ừ, anh về giúp bố mẹ dọn nhà. Cậu nhớ mang theo đồ quan trọng đấy."),
                    ("Con", "Dạ, con có mang theo tờ giấy ghi thông tin xe rồi."),
                },
                "CasualSecond" => new[]
                {
                    ("Bé Na", "Chị ơi, nhà chị có nhiều bánh Tết không?"),
                    ("Con", "Có chứ! Ba mẹ con chuẩn bị từ tuần trước rồi."),
                    ("Bé Na", "Năm nay con được về quê, vui lắm!"),
                    ("Con", "Chúc em Tết sum vầy với gia đình nhé."),
                },
                "PatrolA" => new[]
                {
                    ("Bạn Hùng", "Ê, sắp tới giờ xe rồi đó!"),
                    ("Con", "Ừ, con có tờ giấy ghi thông tin xe trong túi rồi."),
                    ("Bạn Hùng", "Tốt đó, đừng vội mà lên nhầm xe nhé!"),
                    ("Con", "Con sẽ đối kỹ trước khi lên."),
                },
                "PatrolB" => new[]
                {
                    ("Chị Mai", "Chào em, em cũng đi đón xe hả?"),
                    ("Con", "Dạ, em về quê với ba mẹ. Chị đi đâu đây?"),
                    ("Chị Mai", "Chị ra đây gửi quà cho họ hàng. Phố hôm nay nhộn nhịp quá."),
                    ("Con", "Đúng rồi, không khí Tết ở đây ấm áp thật."),
                },
                _ => new[]
                {
                    ("Người dân", "Chào cháu! Tết về quê vui nhé."),
                    ("Con", "Dạ, cảm ơn ạ!"),
                },
            };
        }

        public static IReadOnlyList<(string speaker, string line)> GetRepeatLines(string dialogueKey)
        {
            return dialogueKey switch
            {
                "StallSeller" => new[] { ("Chú Ba", "Mua ít rau mang về ba mẹ đi cháu!") },
                "FlowerViewer" => new[] { ("Cô Lan", "Hoa nở đẹp lắm, cháu ghé ngắm thêm nhé.") },
                "TalkPair" => new[] { ("Minh", "Tết này tụi mình cùng về quê nhé!") },
                "CasualAlone" => new[] { ("Anh Tuấn", "Đi cẩn thận, đường đông đấy.") },
                "CasualSecond" => new[] { ("Bé Na", "Tết vui vẻ nha chị!") },
                "PatrolA" => new[] { ("Bạn Hùng", "Nhớ kiểm tra tờ giấy trước khi lên xe nhé!") },
                "PatrolB" => new[] { ("Chị Mai", "Chúc em về quê an toàn nhé.") },
                _ => new[] { ("Người dân", "Tết vui vẻ nhé cháu!") },
            };
        }
    }
}
