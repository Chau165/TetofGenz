using System;
using System.Collections.Generic;
using System.Text;
using ChoTet.Data;
using UnityEngine;

namespace ChoTet.Runtime
{
    public class GameFlowController : MonoBehaviour, IGameplayPromptSource
    {
        public static GameFlowController Instance { get; private set; }

        public GameHudController hud;
        public DialogueManager dialogueManager;
        public PlayerTopDownController player;
        public Transform homeSpawnPoint;
        public ItemDefinition[] requiredItems;
        public DayState dayState = new DayState();
        public int startingBudget = 1000000;
        public int capacityLimit = 10;
        public int startMinutes = 7 * 60;
        public int closeMinutes = 18 * 60;
        public float realSecondsPerGameMinute = 0.45f;

        public event Action OnReturnHomePhaseStarted;

        private readonly Dictionary<string, int> familiarityByVendor = new Dictionary<string, int>();
        private readonly System.Random random = new System.Random();

        private VendorStall activeVendor;
        private int activeQuote;
        private int activePatience;
        private float timeAccumulator;
        private string currentZone = "Home";
        private string promptText = string.Empty;
        private TempleQuestController templeQuestController;
        private float trafficHitCooldownUntil;
        public bool IsModalOpen
        {
            get { return hud != null && hud.IsModalOpen; }
        }

        public bool IsReturnHomePhase
        {
            get { return dayState != null && dayState.currentPhase == GamePhase.ReturnHome; }
        }

        public bool IsTemplePhase
        {
            get { return dayState != null && dayState.currentPhase == GamePhase.Temple; }
        }

        private void Awake()
        {
            Instance = this;

            if (hud == null)
            {
                hud = FindAnyObjectByType<GameHudController>();
            }

            if (dialogueManager == null)
            {
                dialogueManager = FindAnyObjectByType<DialogueManager>();
            }

            if (player == null)
            {
                player = FindAnyObjectByType<PlayerTopDownController>();
            }

            if (homeSpawnPoint == null)
            {
                GameObject spawn = GameObject.Find("HomeSpawnPoint");
                if (spawn != null)
                {
                    homeSpawnPoint = spawn.transform;
                }
            }

            templeQuestController = FindAnyObjectByType<TempleQuestController>();

            ResetDay();
        }

        private void Start()
        {
            RefreshHud();
            ShowMissionDialogue();
        }

        private void Update()
        {
            if (!dayState.missionAccepted || !dayState.planningCompleted || dayState.evaluationShown || dayState.currentPhase == GamePhase.Evaluation)
            {
                return;
            }

            if (IsModalOpen)
            {
                return;
            }

            timeAccumulator += Time.deltaTime;
            while (timeAccumulator >= realSecondsPerGameMinute)
            {
                timeAccumulator -= realSecondsPerGameMinute;
                AdvanceTime(1);
            }

            TriggerTimedAtmosphereEvents();

            if (!dayState.closingWarningTriggered && dayState.currentMinutes >= 17 * 60 + 15)
            {
                dayState.closingWarningTriggered = true;
                hud.ShowDialog(
                    "Chợ sắp vãn",
                    "Loa phát thanh thông báo chợ Tết sắp sửa vãn. Có thể có một số mối giá hời muộn, nhưng hàng ngon thì không còn nhiều.",
                    null
                );
            }

            if (dayState.currentMinutes >= dayState.closeMinutes && !dayState.evaluationShown && dayState.currentPhase != GamePhase.Temple)
            {
                ShowEvaluation(
                    "Chợ Tết đã đóng cửa",
                    "Bạn phải ra về với những gì đã mua được."
                );
            }
        }

        public void SetPrompt(string value)
        {
            promptText = value;
            if (hud != null)
            {
                hud.SetPrompt(value);
            }
        }

        public void HandleStoryInteraction(StoryInteractable interactable)
        {
            if (interactable == null)
            {
                return;
            }

            if (interactable.interactionId == "mission_giver")
            {
                if (!dayState.missionAccepted)
                {
                    ShowMissionDialogue();
                    return;
                }

                if (!dayState.planningCompleted)
                {
                    ShowRoutePlanning();
                    return;
                }

                if (dayState.currentPhase == GamePhase.ReturnHome)
                {
                    ShowEvaluation(
                        "Về đến nhà",
                        "Bạn đã vượt qua con đường đông đúc để về nhà. Giờ là lúc cả nhà xem đồ bạn mua có ra không khí Tết không nhé."
                    );
                    return;
                }

                if (dayState.marketVisited && !dayState.isShoppingComplete)
                {
                    hud.ShowDialog(
                        "Chưa mua đủ đồ",
                        "Bạn quay về quá sớm rồi. Danh sách sắm Tết vẫn còn thiếu đồ kìa, hãy quay lại chợ mua cho đủ trước khi chợ đóng cửa nhé.",
                        null
                    );
                    return;
                }

                hud.ShowDialog(
                    "Lời dặn của mẹ",
                    "Hãy lên lịch trình đi chợ, chú ý thời gian và đừng mang quá nhiều đồ nặng cùng một lúc. Mua bán thông minh mới là người sắm Tết giỏi.",
                    null
                );
                return;
            }

            hud.ShowDialog(interactable.titleText, interactable.bodyText, null);
        }

        public void HandleNpcDialogue(NPCDialogueTrigger trigger)
        {
            if (trigger == null)
            {
                return;
            }

            if (dialogueManager == null)
            {
                dialogueManager = FindAnyObjectByType<DialogueManager>();
            }

            if (dialogueManager != null)
            {
                dialogueManager.BeginDialogue(trigger, this);
                return;
            }

            string[] lines = trigger.GetDialogueLines(this);
            string body = lines != null && lines.Length > 0 ? string.Join("\n\n", lines) : "...";
            hud.ShowDialog(trigger.npcName, body, () => trigger.OnDialogueFinished(this));
        }

        public void StartMotherQuest()
        {
            dayState.questStarted = true;
            dayState.missionAccepted = true;
            dayState.objectiveText = "Đi chợ mua đồ Tết";
            RefreshHud();

            if (!dayState.planningCompleted)
            {
                ShowRoutePlanning();
            }
        }

        public void HandleVendorInteraction(VendorStall stall)
        {
            if (stall == null || stall.VendorProfile == null || stall.VendorProfile.stockedItem == null)
            {
                return;
            }

            if (!dayState.missionAccepted)
            {
                hud.ShowDialog(
                    "Chưa bắt đầu",
                    "Hãy nói chuyện với mẹ trước để biết gia đình cần mua những gì nhé.",
                    null
                );
                return;
            }

            if (!dayState.planningCompleted)
            {
                ShowRoutePlanning();
                return;
            }

            if (dayState.currentPhase == GamePhase.ReturnHome)
            {
                hud.ShowDialog(
                    "Về nhà thôi",
                    "Đã sắm đủ đồ Tết rồi. Đừng đi la cà nữa; đường về nhà đông đúc mới là thử thách thực sự lúc này.",
                    null
                );
                return;
            }

            if (dayState.blockedVendorId == stall.VendorProfile.vendorId)
            {
                hud.ShowDialog(
                    stall.VendorProfile.displayName,
                    "Gian hàng này đã phải dọn đi sau đợt giải tỏa trật tự đô thị. Bạn cần tìm phương án khác rồi.",
                    null
                );
                return;
            }

            if (dayState.HasPurchased(stall.VendorProfile.stockedItem))
            {
                stall.SetSold(true);
                hud.ShowDialog(
                    stall.VendorProfile.displayName,
                    "Bạn đã mua món này rồi. Hãy tiết kiệm thời gian và sức chở để mua những món khác.",
                    null
                );
                return;
            }

            activeVendor = stall;
            activePatience = stall.VendorProfile.patience;
            activeQuote = CalculateQuote(stall.VendorProfile);

            CoHuongBargainMinigame minigame = stall.GetComponent<CoHuongBargainMinigame>();
            if (minigame != null)
            {
                minigame.Begin(
                    this,
                    stall,
                    activeQuote,
                    BuildBargainMinigameProductInfo(stall.VendorProfile),
                    GetBargainSpeedMultiplier());
                return;
            }

            hud.ShowBargain(
                stall.VendorProfile.displayName,
                BuildVendorBody(stall.VendorProfile),
                activeQuote,
                "Hãy dò xét thái độ người bán và đưa ra lựa chọn.",
                ResolveBargainAction,
                CloseBargain
            );
        }

        public void CompleteCoHuongBargainSuccess(int discountPercent)
        {
            if (activeVendor == null || activeVendor.VendorProfile == null || activeVendor.VendorProfile.stockedItem == null)
            {
                return;
            }

            VendorProfile vendor = activeVendor.VendorProfile;
            ItemDefinition item = vendor.stockedItem;
            int oldQuote = activeQuote;
            int minimumQuote = RoundMoneyToThousand(Mathf.RoundToInt(item.basePrice * 0.6f));
            activeQuote = Mathf.Max(
                minimumQuote,
                RoundMoneyToThousand(Mathf.RoundToInt(activeQuote * (1f - discountPercent / 100f))));

            AddVendorFamiliarity(vendor.vendorId, 1);
            AdvanceTime(4);
            hud.ShowDialog(
                vendor.displayName,
                "Bạn canh trúng ô xanh " + discountPercent + "% rồi đó. Người bán bớt từ "
                    + FormatMoney(oldQuote) + " còn " + FormatMoney(activeQuote)
                    + ". Nếu ưng ý thì mua " + item.displayName + " với giá này.",
                TryPurchaseActiveVendor
            );
        }

        public void HandleCoHuongBargainFailure(int cooldownSeconds)
        {
            if (activeVendor == null || activeVendor.VendorProfile == null)
            {
                return;
            }

            VendorProfile vendor = activeVendor.VendorProfile;
            AdvanceTime(2);
            hud.ShowDialog(
                vendor.displayName,
                "Ô đỏ rồi. Giá này người bán chưa bớt được, quay lại sau khoảng "
                    + cooldownSeconds + " giây nữa để thử vận may lại.",
                CloseBargain
            );
        }

        public void ShowCoHuongCooldown(int secondsRemaining)
        {
            hud.ShowDialog(
                activeVendor != null && activeVendor.VendorProfile != null ? activeVendor.VendorProfile.displayName : "Người bán",
                "Người bán vẫn đang giữ giá sau lần trả giá trước. Đợi thêm khoảng "
                    + secondsRemaining + " giây rồi hãy thử lại.",
                null
            );
        }

        public void HandleStorageInteraction(StorageInteractable storage)
        {
            if (storage == null)
            {
                return;
            }

            if (!dayState.missionAccepted || !dayState.planningCompleted)
            {
                hud.ShowDialog(
                    storage.titleText,
                    "Bạn chưa cần dịch vụ giao hàng lúc này. Trước mắt hãy lên kế hoạch đi chợ đã.",
                    null
                );
                return;
            }

            if (dayState.currentPhase == GamePhase.ReturnHome)
            {
                hud.ShowDialog(
                    storage.titleText,
                    "Quá trình vận chuyển về nhà đã bắt đầu. Hãy tập trung lái xe về nhà an toàn và bảo vệ số đồ đã mua.",
                    null
                );
                return;
            }

            if (dayState.currentCarry <= 0)
            {
                hud.ShowDialog(
                    storage.titleText,
                    "Tay bạn đang trống trơn. Hiện tại không có đồ gì để gửi về nhà cả.",
                    null
                );
                return;
            }

            dayState.SendAllHome();
            AdvanceTime(12);
            RefreshHud();
            hud.ShowDialog(
                storage.titleText,
                "Xe ôm công nghệ đã nhận chuyển đồ của bạn về nhà. Bạn mất 12 phút trong game, nhưng giỏ đồ của bạn đã trống trở lại để sắm tiếp.",
                null
            );
        }

        public void HandleZoneTrigger(ZoneTrigger trigger)
        {
            if (trigger == null)
            {
                return;
            }

            currentZone = trigger.zoneId;
            if (currentZone == "Market")
            {
                dayState.marketVisited = true;
            }

            RefreshHud();

            if (currentZone == "Home" && dayState.currentPhase == GamePhase.ReturnHome && !dayState.evaluationShown)
            {
                TempleQuestController temple = GetTempleQuestController();
                if (temple != null && !dayState.templeCompleted)
                {
                    temple.BeginTempleQuest();
                }
                else
                {
                    ShowEvaluation(
                        "Back home",
                        "You make it back with the Tet goods. The road home left its mark, but the family can finally check the day."
                    );
                }
                return;
            }

            if (trigger.triggerOnce && trigger.HasTriggered)
            {
                return;
            }

            trigger.MarkTriggered();

            if (string.IsNullOrEmpty(trigger.eventId))
            {
                if (!string.IsNullOrEmpty(trigger.eventBody))
                {
                    hud.ShowDialog(trigger.eventTitle, trigger.eventBody, null);
                }

                return;
            }

            switch (trigger.eventId)
            {
                case "crowd":
                    if (!dayState.crowdTriggered)
                    {
                        dayState.crowdTriggered = true;
                        AdvanceTime(Mathf.Max(2, trigger.timePenaltyMinutes));
                    }
                    break;

                case "rain":
                    // Weather events removed from the vertical slice.
                    break;

                case "friend_discount":
                    if (!dayState.friendDiscountTriggered)
                    {
                        dayState.friendDiscountTriggered = true;
                        if (!string.IsNullOrEmpty(trigger.vendorBonusId))
                        {
                            AddVendorFamiliarity(trigger.vendorBonusId, Mathf.Max(1, trigger.familiarityBonus));
                        }

                        hud.ShowDialog(
                            "Mẹo người quen",
                            "Một người quen cùng làng chỉ cho bạn sạp hàng uy tín, thân thiện. Ngày Tết, quen biết và tình làng nghĩa xóm đôi khi quan trọng hơn cả giá cả.",
                            null
                        );
                    }
                    break;

                case "flash_sale":
                    if (!dayState.flashSaleTriggered)
                    {
                        dayState.flashSaleTriggered = true;
                        dayState.flashSaleVendorId = trigger.vendorBonusId;
                        hud.ShowDialog(
                            "Cơn sốt giảm giá",
                            "Một sạp hàng bất ngờ xả hàng giảm giá kịch sàn. Bạn có thể tiết kiệm được kha khá tiền tại đây, nhưng mọi người xung quanh cũng đang đổ xô vào tranh giành.",
                            null
                        );
                    }
                    break;

                case "police_clear":
                    if (!dayState.policeClearTriggered)
                    {
                        dayState.policeClearTriggered = true;
                        dayState.blockedVendorId = trigger.vendorBonusId;
                        HideVendor(trigger.vendorBonusId);
                        hud.ShowDialog(
                            "Giải tỏa lòng lề đường",
                            "Người bán hàng quen thuộc trên vỉa hè đột ngột biến mất khi lực lượng trật tự đi giải tỏa. Con đường đi chợ quen thuộc của bạn giờ không còn sạp hàng đó nữa.",
                            null
                        );
                    }
                    break;
            }
        }

        private void ResolveBargainAction(string actionId)
        {
            if (activeVendor == null || activeVendor.VendorProfile == null)
            {
                return;
            }

            VendorProfile vendor = activeVendor.VendorProfile;
            ItemDefinition item = vendor.stockedItem;
            int familiarity = GetVendorFamiliarity(vendor);

            if (actionId == "buy_now")
            {
                TryPurchaseActiveVendor();
                return;
            }

            if (actionId == "inspect_goods")
            {
                AdvanceTime(2);
                activePatience -= 1;
                hud.UpdateBargain(activeQuote, BuildInspectionNote(item, vendor));
                return;
            }

            string feedback = string.Empty;
            float discount = 0f;

            switch (actionId)
            {
                case "soft_bargain":
                    discount = 0.06f + (familiarity * 0.012f);
                    activePatience -= 4;
                    feedback = "Bạn khéo léo xin bớt lấy lộc đầu năm. Cách này hiệu quả nhất với những người bán niềm nở.";
                    break;

                case "compare_price":
                    discount = vendor.honesty < 0.72f ? 0.12f : 0.05f;
                    activePatience -= 9;
                    feedback = "Việc khảo giá xung quanh tạo áp lực lên người bán, nhưng nó sẽ làm họ nhanh mất kiên nhẫn.";
                    break;

                case "bulk_buy":
                    discount = dayState.currentCarry <= 4 ? 0.09f : 0.04f;
                    activePatience -= 5;
                    feedback = "Bạn ngỏ ý muốn mua nhiều món để có giá ưu đãi. Người bán sẽ thích hơn nếu trông bạn thực sự muốn mua.";
                    break;

                case "walk_away":
                    activePatience -= 11;
                    if (UnityEngine.Random.value > 0.42f + (familiarity * 0.07f))
                    {
                        AdvanceTime(4);
                        hud.UpdateBargain(activeQuote, "Bạn giả vờ quay đi, nhưng người bán lờ đi luôn. Bạn mất 4 phút vô ích.");
                        return;
                    }

                    discount = 0.14f;
                    feedback = "Người bán gọi với theo bớt thêm một chút trước khi bạn chìm vào đám đông.";
                    break;

                case "charm":
                    discount = 0.05f + (familiarity * 0.015f);
                    activePatience += 2;
                    AddVendorFamiliarity(vendor.vendorId, 1);
                    feedback = "Bạn khen ngợi sạp hàng làm người bán vui vẻ mát lòng. Không khí thương lượng trở nên dễ chịu hơn.";
                    break;
            }

            if (vendor.favoriteApproach == actionId)
            {
                discount += 0.04f;
                feedback += " Có vẻ đây chính là chiêu thức mà người bán này dễ xiêu lòng nhất.";
            }

            if (activePatience <= 8)
            {
                hud.UpdateBargain(activeQuote, "Người bán đã mất hết kiên nhẫn. Nếu bạn cố ép giá thêm, cuộc thương lượng sẽ đổ vỡ.");
                return;
            }

            int minimumQuote = RoundMoneyToThousand(Mathf.RoundToInt(item.basePrice * 0.6f));
            activeQuote = Mathf.Max(minimumQuote, RoundMoneyToThousand(Mathf.RoundToInt(activeQuote * (1f - discount))));
            AdvanceTime(5);
            hud.UpdateBargain(activeQuote, feedback);
        }

        private void TryPurchaseActiveVendor()
        {
            VendorProfile vendor = activeVendor.VendorProfile;
            ItemDefinition item = vendor.stockedItem;

            if (dayState.currentCarry + item.capacityCost > dayState.capacityLimit)
            {
                hud.UpdateBargain(activeQuote, "Tay bạn đã xách quá nhiều đồ rồi. Hãy gửi bớt đồ về nhà trước hoặc chọn mua món nào nhẹ hơn.");
                return;
            }

            if (dayState.budget < activeQuote)
            {
                hud.UpdateBargain(activeQuote, "Bạn không đủ tiền để mua món này. Hãy trả giá khéo hơn hoặc điều chỉnh mức chi tiêu cho các món khác.");
                return;
            }

            int quality = RollQuality(item, vendor);
            int luck = RollLuck(item);
            string qualityNote = BuildQualitySummary(item, quality, luck);
            dayState.AddPurchase(item, activeQuote, quality, luck, vendor.displayName, qualityNote);
            activeVendor.SetSold(true);
            AddVendorFamiliarity(vendor.vendorId, 1);
            AdvanceTime(10);
            RefreshHud();

            bool shoppingComplete = IsRequiredShoppingComplete();
            hud.ShowDialog(
                shoppingComplete ? "Đã mua đủ đồ Tết" : "Đã mua hàng",
                shoppingComplete
                    ? "Bạn mua " + item.displayName + " với giá " + FormatMoney(activeQuote) + ". " + qualityNote + "\n\nĐã mua đủ đồ Tết! Chạy xe về nhà và tránh xe trên đường."
                    : "Bạn mua " + item.displayName + " với giá " + FormatMoney(activeQuote) + ". " + qualityNote,
                null
            );

            activeVendor = null;
            activeQuote = 0;
            activePatience = 0;

            if (shoppingComplete)
            {
                CompleteShopping();
            }
        }

        private void CloseBargain()
        {
            activeVendor = null;
            activeQuote = 0;
            activePatience = 0;
        }

        public float GetBargainSpeedMultiplier()
        {
            int completedCount = dayState != null && dayState.purchases != null ? dayState.purchases.Count : 0;
            return Mathf.Clamp(1f + completedCount * 0.25f, 1f, 2f);
        }

        private string BuildBargainMinigameProductInfo(VendorProfile vendor)
        {
            if (vendor == null || vendor.stockedItem == null)
            {
                return "Mặt hàng: không rõ | Tiêu chí: không rõ";
            }

            ItemDefinition item = vendor.stockedItem;
            return "Mặt hàng: " + item.displayName + " | Tiêu chí: " + item.qualityFocus;
        }

        private void ShowMissionDialogue()
        {
            hud.ShowDialog(
                "Nhiệm vụ mua sắm Tết",
                "Mẹ cần mua khay bánh mứt, dưa hấu, cành hoa mai, thịt heo ba chỉ và giỏ quà biếu. Không thể đi mua mù quáng được đâu. Hãy lên kế hoạch đi chợ trước nhé.",
                AcceptMission
            );
        }

        private void AcceptMission()
        {
            dayState.missionAccepted = true;
            RefreshHud();
            ShowRoutePlanning();
        }

        private void ShowRoutePlanning()
        {
            hud.ShowChoice(
                "Chọn lộ trình di chuyển",
                "Lộ trình bạn chọn sẽ ảnh hưởng tới mức độ ép giá của người bán, chất lượng hàng hóa và những mối quen tin cậy khi chợ trở nên đông đúc hỗn loạn.",
                new[]
                {
                    "1. Lộ trình hàng tươi ngon: Đi sớm hơn, hàng chất lượng tốt hơn, giá hơi cao hơn một chút.",
                    "2. Lộ trình mối quen thân: Giờ đi bình thường, người bán uy tín hơn, trả giá an toàn hơn.",
                    "3. Lộ trình xả hàng cuối ngày: Đi muộn hơn, giá rẻ hơn, nhưng rủi ro hàng ôi héo cao hơn."
                },
                ApplyRoutePlan,
                null
            );
        }

        private void ApplyRoutePlan(string choiceId)
        {
            dayState.planningCompleted = true;

            switch (choiceId)
            {
                case "choice_1":
                    dayState.routePlanId = "fresh";
                    dayState.routePlanLabel = "Lộ trình hàng tươi ngon";
                    dayState.currentMinutes = 6 * 60 + 30;
                    break;

                case "choice_2":
                    dayState.routePlanId = "familiar";
                    dayState.routePlanLabel = "Lộ trình mối quen thân";
                    dayState.currentMinutes = 7 * 60 + 30;
                    AddVendorFamiliarity("co_huong_banh", 1);
                    AddVendorFamiliarity("co_lan_hoa", 2);
                    break;

                default:
                    dayState.routePlanId = "late";
                    dayState.routePlanLabel = "Lộ trình xả hàng cuối ngày";
                    dayState.currentMinutes = 10 * 60 + 15;
                    break;
            }

            RefreshHud();
            hud.ShowDialog(
                "Kế hoạch đã chốt",
                "Bạn xuất phát theo " + dayState.routePlanLabel.ToLower() + ". Giờ đây mỗi phút trôi qua, mỗi cuộc thương lượng và mỗi món đồ đều vô cùng quan trọng.",
                null
            );
        }

        private void ResetDay()
        {
            dayState.Reset(startMinutes, closeMinutes, startingBudget, capacityLimit);
            dayState.objectiveText = "Nói chuyện với mẹ";
            familiarityByVendor.Clear();

            VendorStall[] stalls = FindObjectsByType<VendorStall>(FindObjectsInactive.Include);
            for (int i = 0; i < stalls.Length; i++)
            {
                stalls[i].gameObject.SetActive(true);
                stalls[i].enabled = true;
                stalls[i].SetSold(false);
                if (stalls[i].VendorProfile != null)
                {
                    familiarityByVendor[stalls[i].VendorProfile.vendorId] = stalls[i].VendorProfile.familiarity;
                }
            }

            activeVendor = null;
            activeQuote = 0;
            activePatience = 0;
            timeAccumulator = 0f;
            currentZone = "Home";
            promptText = string.Empty;
            SetPrompt(string.Empty);

            CrowdedRoadTrafficController[] trafficControllers = FindObjectsByType<CrowdedRoadTrafficController>(FindObjectsInactive.Include);
            for (int i = 0; i < trafficControllers.Length; i++)
            {
                trafficControllers[i].RefreshFromGameState();
            }

            if (player != null && homeSpawnPoint != null)
            {
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                player.transform.position = homeSpawnPoint.position;

                if (controller != null)
                {
                    controller.enabled = true;
                }
            }
        }

        private void RestartDay()
        {
            ResetDay();
            RefreshHud();
            ShowMissionDialogue();
        }

        private void TriggerTimedAtmosphereEvents()
        {
            if (dayState.marketVisited && !dayState.flashSaleTriggered && dayState.currentMinutes >= 16 * 60)
            {
                dayState.flashSaleTriggered = true;
                dayState.flashSaleVendorId = "chi_sau_qua";
                hud.ShowDialog(
                    "Xả hàng cuối ngày",
                    "Một sạp bán giỏ quà đang tìm cách thanh lý nốt hàng trước giờ đóng cửa. Giá giảm thật sự, nhưng mọi người cũng đang đổ xô về đó rất đông.",
                    null
                );
            }

            if (dayState.marketVisited && !dayState.policeClearTriggered && dayState.currentMinutes >= 17 * 60 + 30)
            {
                dayState.policeClearTriggered = true;
                dayState.blockedVendorId = "chu_tu_thit";
                HideVendor(dayState.blockedVendorId);
                hud.ShowDialog(
                    "Biến động bên lề đường",
                    "Lực lượng dẹp lòng lề đường khiến người bán thịt heo phải thu dọn chạy vội. Tết không chờ đợi một kế hoạch hoàn hảo.",
                    null
                );
            }
        }

        private void HideVendor(string vendorId)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                return;
            }

            VendorStall[] stalls = FindObjectsByType<VendorStall>(FindObjectsInactive.Include);
            for (int i = 0; i < stalls.Length; i++)
            {
                if (stalls[i].VendorProfile != null && stalls[i].VendorProfile.vendorId == vendorId)
                {
                    stalls[i].gameObject.SetActive(false);
                }
            }
        }

        private void ShowEvaluation(string title, string preface)
        {
            dayState.evaluationShown = true;
            dayState.StartEvaluation();

            int purchasedCount = dayState.purchases.Count;
            int requiredCount = requiredItems != null ? requiredItems.Length : 0;
            int completeness = requiredCount == 0 ? 0 : Mathf.RoundToInt((purchasedCount / (float)requiredCount) * 100f);
            int qualityScore = 0;
            int presentationScore = 25;
            int traditionScore = 0;
            int socialScore = 0;
            int luckScore = 0;

            for (int i = 0; i < dayState.purchases.Count; i++)
            {
                PurchasedItemData purchase = dayState.purchases[i];
                qualityScore += purchase.quality;
                luckScore += purchase.luck;
                if (purchase.item != null)
                {
                    presentationScore += purchase.item.presentationBonus;
                    traditionScore += purchase.item.traditionBonus;
                    socialScore += purchase.item.socialBonus;
                }
            }

            if (dayState.purchases.Count > 0)
            {
                qualityScore = Mathf.RoundToInt(qualityScore / (float)dayState.purchases.Count);
                luckScore = Mathf.RoundToInt(luckScore / (float)dayState.purchases.Count);
            }

            int deliveryScore = Mathf.Clamp(100 - (dayState.deliveryCollisionCount * 16) - dayState.deliveryItemDamage, 0, 100);
            int deliveryQualityPenalty = Mathf.RoundToInt(dayState.deliveryItemDamage * 0.35f);
            qualityScore = Mathf.Clamp(qualityScore - deliveryQualityPenalty, 0, 100);
            traditionScore = Mathf.Clamp(traditionScore, 0, 100);
            socialScore = Mathf.Clamp(socialScore, 0, 100);
            presentationScore = Mathf.Clamp(presentationScore, 0, 100);
            int motherScore = CalculateBudgetScore();
            int fatherScore = Mathf.Clamp(Mathf.RoundToInt((qualityScore * 0.7f) + (luckScore * 0.3f)), 0, 100);
            int grandmaScore = Mathf.Clamp(Mathf.RoundToInt((traditionScore * 0.75f) + (completeness * 0.25f)), 0, 100);
            int familySpirit = Mathf.Clamp(Mathf.RoundToInt((motherScore + fatherScore + grandmaScore + presentationScore + completeness + deliveryScore) / 6f), 0, 100);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(preface);
            builder.AppendLine();
            builder.AppendLine("Số tiền còn lại: " + FormatMoney(dayState.budget));
            builder.AppendLine("Món đồ đã sắm: " + purchasedCount + "/" + requiredCount);
            builder.AppendLine("Chuyến gửi đồ về nhà: " + dayState.deliveriesUsed);
            builder.AppendLine("Lộ trình đã đi: " + dayState.routePlanLabel);
            builder.AppendLine("Va quẹt giao hàng: " + dayState.deliveryCollisionCount);
            builder.AppendLine("Thiệt hại hàng hóa: " + dayState.deliveryItemDamage);
            builder.AppendLine();
            builder.AppendLine("Mẹ (Ngân sách & Logistics): " + motherScore + "/100");
            builder.AppendLine("Hiệu quả trả giá: " + GetBudgetAssessment(motherScore));
            builder.AppendLine("Bố (Chất lượng & Hương vị): " + fatherScore + "/100");
            builder.AppendLine("Bà (Truyền thống): " + grandmaScore + "/100");
            builder.AppendLine("Trình bày: " + presentationScore + "/100");
            builder.AppendLine("Điểm lái xe giao đồ: " + deliveryScore + "/100");
            builder.AppendLine("Không khí Tết gia đình: " + familySpirit + "/100");
            builder.AppendLine();
            builder.AppendLine("Tổng kết sắm sửa:");

            if (requiredItems != null)
            {
                for (int i = 0; i < requiredItems.Length; i++)
                {
                    PurchasedItemData purchase = dayState.GetPurchase(requiredItems[i]);
                    if (purchase != null)
                    {
                        builder.AppendLine("[x] " + requiredItems[i].displayName + " - " + purchase.qualityNote);
                    }
                    else
                    {
                        builder.AppendLine("[ ] " + requiredItems[i].displayName);
                    }
                }
            }

            builder.AppendLine();
            if (familySpirit >= 80)
            {
                builder.AppendLine("Gia đình cảm nhận được sự chăm chút trong từng lựa chọn của bạn. Dù chưa thật hoàn hảo, nhưng đã mang đậm không khí Tết rồi.");
            }
            else if (purchasedCount < requiredCount)
            {
                builder.AppendLine("Bạn trở về nhà với sự nỗ lực hiện rõ trên khuôn mặt hơn là sự hoàn hảo trong giỏ đồ. Điều đó vẫn vô cùng đáng trân quý.");
            }
            else
            {
                builder.AppendLine("Một ngày sắm Tết khá trọn vẹn, nhưng lần sau bạn có thể cân đối chất lượng, tiền bạc và thời gian tự tin hơn nữa.");
            }

            hud.ShowEvaluation(title, builder.ToString(), RestartDay);
        }

        private int CalculateBudgetScore()
        {
            int startingBudget = Mathf.Max(1, dayState.startingBudget);
            float remainingRatio = Mathf.Clamp01(dayState.budget / (float)startingBudget);
            if (remainingRatio >= 0.40f) return 100;
            if (remainingRatio >= 0.35f) return 90;
            if (remainingRatio >= 0.30f) return 78;
            if (remainingRatio >= 0.25f) return 65;
            if (remainingRatio >= 0.20f) return 52;
            return 35;
        }

        private static string GetBudgetAssessment(int score)
        {
            if (score >= 90) return "Xuất sắc - bạn trả giá rất khéo, đem về nhiều khoản dư.";
            if (score >= 70) return "Tốt - bạn đã mua sắm hợp lý và giữ ngân sách an toàn.";
            if (score >= 55) return "Khá - bạn hoàn thành việc mua sắm nhưng còn có thể trả giá tốt hơn.";
            return "Cần cải thiện - tiền còn lại ít, hãy tập trung canh các ô giảm giá cao hơn.";
        }

        private void AdvanceTime(int minutes)
        {
            dayState.currentMinutes += minutes;
            if (dayState.currentMinutes > dayState.closeMinutes)
            {
                dayState.currentMinutes = dayState.closeMinutes;
            }

            RefreshHud();
        }

        private void RefreshHud()
        {
            if (hud == null)
            {
                return;
            }

            hud.UpdateHud(BuildStatusSummary(), BuildShoppingListSummary());
            hud.SetPrompt(promptText);
        }

        private string BuildStatusSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Thời gian: " + FormatClock(dayState.currentMinutes));
            builder.AppendLine("Giai đoạn: " + FormatPhase(dayState.currentPhase));
            string vietnameseZone = currentZone;
            switch (currentZone)
            {
                case "Garden": vietnameseZone = "Sân vườn"; break;
                case "Road": vietnameseZone = "Đường đi"; break;
                case "Market": vietnameseZone = "Chợ Tết"; break;
                case "Temple": vietnameseZone = "Chùa Xuân An"; break;
                case "Home": vietnameseZone = "Nhà"; break;
            }
            builder.AppendLine("Khu vực: " + vietnameseZone);
            builder.AppendLine("Nhiệm vụ: " + (string.IsNullOrWhiteSpace(dayState.objectiveText) ? "-" : dayState.objectiveText));
            builder.AppendLine("Lộ trình: " + dayState.routePlanLabel);
            builder.AppendLine("Tiền mặt: " + FormatMoney(dayState.budget));
            builder.AppendLine("Sức chở: " + dayState.currentCarry + "/" + dayState.capacityLimit);
            builder.AppendLine("Không khí chợ: " + (dayState.crowdTriggered ? "Đông đúc chen lấn" : "Dễ thở"));
            builder.AppendLine("Mạng: " + dayState.playerLives + "/" + DayState.MaxPlayerLives);
            if (dayState.currentPhase == GamePhase.ReturnHome)
            {
                builder.AppendLine("Va quẹt: " + dayState.deliveryCollisionCount);
                builder.AppendLine("Hao hụt hàng hóa: " + dayState.deliveryItemDamage);
            }
            else if (dayState.currentPhase == GamePhase.Temple)
            {
                builder.AppendLine("Viếng chùa: " + (dayState.templeCompleted ? "Đã hoàn thành" : "Đang thực hiện"));
            }

            return builder.ToString();
        }

        private string BuildShoppingListSummary()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(dayState.currentPhase == GamePhase.ReturnHome
                ? "Đường về nhà"
                : dayState.currentPhase == GamePhase.Temple
                    ? "Viếng chùa đầu năm"
                    : "Danh sách sắm Tết");
            builder.AppendLine();

            if (requiredItems == null)
            {
                return builder.ToString();
            }

            if (dayState.currentPhase == GamePhase.ReturnHome)
            {
                builder.AppendLine("Đã mua đủ đồ Tết!");
                builder.AppendLine("Chạy xe về nhà và tránh xe trên đường.");
                builder.AppendLine("Va chạm: " + dayState.deliveryCollisionCount + " | Hao hụt: " + dayState.deliveryItemDamage);
                return builder.ToString();
            }

            if (dayState.currentPhase == GamePhase.Temple)
            {
                builder.AppendLine(dayState.templeGateVisited ? "[x] Qua cổng chùa" : "[ ] Qua cổng chùa");
                builder.AppendLine(dayState.templeOfferingDone ? "[x] Dâng lễ đầu năm" : "[ ] Dâng lễ đầu năm");
                builder.AppendLine(dayState.incenseLit ? "[x] Thắp hương cầu bình an" : "[ ] Thắp hương cầu bình an");
                builder.AppendLine(dayState.fortuneDrawn ? "[x] Xin quẻ đầu năm" : "[ ] Xin quẻ đầu năm");
                return builder.ToString();
            }

            for (int i = 0; i < requiredItems.Length; i++)
            {
                PurchasedItemData purchase = dayState.GetPurchase(requiredItems[i]);
                if (purchase != null)
                {
                    builder.AppendLine("[x] " + requiredItems[i].displayName + (purchase.delivered ? " (đã gửi về)" : " (đang mang)"));
                }
                else
                {
                    builder.AppendLine("[ ] " + requiredItems[i].displayName);
                }
            }

            return builder.ToString();
        }

        private bool IsRequiredShoppingComplete()
        {
            if (requiredItems == null || requiredItems.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < requiredItems.Length; i++)
            {
                if (!dayState.HasPurchased(requiredItems[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void CompleteShopping()
        {
            if (dayState.isShoppingComplete)
            {
                return;
            }

            dayState.CompleteShopping();
            dayState.SendAllHome();
            dayState.objectiveText = "Về nhà giao đồ Tết";
            RefreshHud();

            Action handler = OnReturnHomePhaseStarted;
            if (handler != null)
            {
                handler();
            }
        }

        public void RegisterDeliveryCollision(int damageAmount)
        {
            RegisterTrafficHit(damageAmount);
        }

        /// <summary>
        /// Car hit on the market road. 3 lives — at 0 the ChoTet day restarts from the beginning.
        /// </summary>
        public void RegisterTrafficHit(int damageAmount)
        {
            if (dayState.evaluationShown || dayState.templeCompleted)
            {
                return;
            }

            // Brief i-frames so one car cannot drain all lives at once.
            if (Time.unscaledTime < trafficHitCooldownUntil)
            {
                return;
            }

            trafficHitCooldownUntil = Time.unscaledTime + 1.6f;

            if (dayState.currentPhase == GamePhase.ReturnHome)
            {
                dayState.RegisterDeliveryDamage(damageAmount);
            }

            dayState.playerLives = Mathf.Max(0, dayState.playerLives - 1);
            RefreshHud();

            if (dayState.playerLives <= 0)
            {
                if (hud != null)
                {
                    hud.ShowDialog(
                        "Hết mạng",
                        "Bạn bị xe tông 3 lần. Màn đi chợ sẽ bắt đầu lại từ đầu.",
                        RestartDay);
                }
                else
                {
                    RestartDay();
                }
            }
            else if (hud != null)
            {
                VoiceNarrator.Instance.Speak(
                    "Hướng dẫn",
                    "Cẩn thận xe cộ. Còn " + dayState.playerLives + " mạng.");
            }
        }
        public void SetCurrentZone(string zoneId)
        {
            if (string.IsNullOrWhiteSpace(zoneId))
            {
                return;
            }

            currentZone = zoneId;
            RefreshHud();
        }


        public void StartTempleQuest()
        {
            dayState.currentPhase = GamePhase.Temple;
            dayState.templeQuestStarted = true;
            dayState.objectiveText = "Qua cổng chùa (nhấn E)";
            RefreshHud();
        }

        public void MarkTempleGateVisited()
        {
            dayState.templeGateVisited = true;
            dayState.objectiveText = "Dâng lễ phẩm Tết (nhấn E)";
            RefreshHud();
        }

        public void MarkTempleOfferingDone()
        {
            dayState.templeOfferingDone = true;
            dayState.objectiveText = "Thắp hương (nhấn E)";
            RefreshHud();
        }

        public void MarkTempleIncenseLit()
        {
            dayState.incenseLit = true;
            dayState.objectiveText = "Xin quẻ đầu năm (nhấn E)";
            RefreshHud();
        }

        public void MarkTempleFortuneDrawn()
        {
            dayState.fortuneDrawn = true;
            dayState.objectiveText = "Hoàn tất lễ chùa đầu năm";
            RefreshHud();
        }

        public void ShowTempleFinalEvaluation(string preface)
        {
            dayState.templeCompleted = true;
            dayState.evaluationShown = true;
            dayState.StartEvaluation();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(preface);
            builder.AppendLine();
            builder.AppendLine("Bạn đã hoàn thành toàn bộ hành trình Tết:");
            builder.AppendLine("• Sắm đồ Tết");
            builder.AppendLine("• Về nhà an toàn");
            builder.AppendLine("• Viếng chùa đầu năm");
            builder.AppendLine();
            builder.AppendLine("HẾT GAME — Cảm ơn bạn đã chơi!");

            if (hud != null)
            {
                VoiceNarrator.Instance.Speak("Hướng dẫn", "Chúc mừng! Bạn đã hoàn thành toàn bộ hành trình Tết. Hết game.");
                hud.ShowEvaluation("Hoàn thành game", builder.ToString(), OnGameFullyComplete);
            }
        }

        private void OnGameFullyComplete()
        {
            // End-of-game screen: stay finished (no auto-restart of the market day).
            dayState.evaluationShown = true;
            if (hud != null)
            {
                hud.ShowDialog(
                    "Hết game",
                    "Bạn đã chơi xong màn đi chợ và viếng chùa. Cảm ơn bạn!",
                    null);
            }
        }

        private string FormatPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.ReturnHome:
                    return "Đường về nhà";
                case GamePhase.Temple:
                    return "Viếng chùa đầu năm";
                case GamePhase.Evaluation:
                    return "Tổng kết Tết";
                default:
                    return "Sắm đồ Tết";
            }
        }

        private TempleQuestController GetTempleQuestController()
        {
            if (templeQuestController == null)
            {
                templeQuestController = FindAnyObjectByType<TempleQuestController>();
            }

            return templeQuestController;
        }

        private string BuildVendorBody(VendorProfile vendor)
        {
            ItemDefinition item = vendor.stockedItem;
            string pressure = "ổn định";
            if (dayState.currentMinutes >= 16 * 60)
            {
                pressure = "áp lực cuối ngày đang làm dao động giá";
            }

            return vendor.greeting + "\n\n" +
                "Mặt hàng: " + item.displayName + "\n" +
                "Tiêu chí: " + item.qualityFocus + "\n" +
                "Thái độ người bán: " + vendor.mood + "\n" +
                "Sự kiên nhẫn: " + activePatience + "\n" +
                "Độ thân thiết: " + GetVendorFamiliarity(vendor) + "\n" +
                "Biến động giá: " + pressure;
        }

        private string BuildInspectionNote(ItemDefinition item, VendorProfile vendor)
        {
            int hintStrength = Mathf.RoundToInt(vendor.honesty * 100f) + GetVendorFamiliarity(vendor) * 5;
            if (item.category == "fruit")
            {
                return hintStrength >= 75
                    ? "Bạn gõ thử vào quả dưa và nghe thấy tiếng bình bịch chắc nịch. Quả này chắc chắn rất ngọt và đỏ."
                    : "Quả dưa trông có vẻ ổn, nhưng người bán khá kín tiếng. Mua quả này vẫn là một canh bạc.";
            }

            if (item.category == "flower")
            {
                return hintStrength >= 75
                    ? "Cánh hoa vẫn tươi tắn và cứng cáp. Nhánh mai này chắc chắn sẽ mang sắc xuân rực rỡ về nhà."
                    : "Một số cánh hoa đã bắt đầu héo. Nếu bạn chần chừ quá lâu, hoa sẽ tàn trước khi giá kịp giảm.";
            }

            if (item.category == "meat")
            {
                return hintStrength >= 75
                    ? "Miếng thịt trông vẫn tươi hồng, thớ thịt săn chắc. Rất thích hợp cho nồi thịt kho tàu của gia đình."
                    : "Bề ngoài trông tạm ổn, nhưng bạn không hoàn toàn tin tưởng vào độ tươi ngon của nó nữa.";
            }

            return hintStrength >= 75
                ? "Sạp hàng trông rất ngăn nắp và cách bày biện giỏ quà trông rất bắt mắt."
                : "Nhìn ngoài thì cũng được, nhưng chất lượng thực sự bên trong thì chưa biết thế nào.";
        }

        private string BuildQualitySummary(ItemDefinition item, int quality, int luck)
        {
            string qualityBand;
            if (quality >= 86)
            {
                qualityBand = "thượng hạng";
            }
            else if (quality >= 72)
            {
                qualityBand = "rất tốt";
            }
            else if (quality >= 58)
            {
                qualityBand = "tạm ổn";
            }
            else
            {
                qualityBand = "kém chất lượng";
            }

            if (item.category == "fruit")
            {
                return "Trông khá " + qualityBand + ", độ may mắn ước tính " + luck + "/100.";
            }

            return "Chất lượng được đánh giá ở mức " + qualityBand + " khoảng " + quality + "/100.";
        }

        private int CalculateQuote(VendorProfile vendor)
        {
            ItemDefinition item = vendor.stockedItem;
            float multiplier = 1f + (vendor.greed * 0.24f);

            if (dayState.routePlanId == "fresh")
            {
                multiplier += 0.08f;
            }
            else if (dayState.routePlanId == "late")
            {
                multiplier -= 0.09f;
            }

            if (dayState.currentMinutes <= 8 * 60)
            {
                multiplier += 0.08f;
            }
            else if (dayState.currentMinutes >= 16 * 60)
            {
                multiplier -= item.perishable ? 0.06f : 0.03f;
            }

            if (dayState.crowdTriggered)
            {
                multiplier += 0.05f;
            }

            if (dayState.flashSaleVendorId == vendor.vendorId)
            {
                multiplier -= 0.18f;
            }

            if (dayState.blockedVendorId != string.Empty && vendor.vendorId != dayState.blockedVendorId && item.category == "meat")
            {
                multiplier += 0.08f;
            }

            multiplier -= GetVendorFamiliarity(vendor) * 0.035f;
            return RoundMoneyToThousand(Mathf.RoundToInt(item.basePrice * Mathf.Clamp(multiplier, 0.62f, 1.55f)));
        }

        private int RollQuality(ItemDefinition item, VendorProfile vendor)
        {
            int low = Mathf.Min(item.minQuality, item.maxQuality);
            int high = Mathf.Max(item.minQuality, item.maxQuality) + 1;
            int quality = random.Next(low, high);

            quality += Mathf.RoundToInt(vendor.honesty * 10f);
            quality += GetVendorFamiliarity(vendor) * 2;

            if (dayState.routePlanId == "fresh")
            {
                quality += 8;
            }
            else if (dayState.routePlanId == "late")
            {
                quality -= 7;
            }

            if (dayState.currentMinutes >= 15 * 60)
            {
                quality -= item.perishable ? 10 : 4;
            }

            return Mathf.Clamp(quality, 35, 100);
        }

        private int RollLuck(ItemDefinition item)
        {
            int luck = random.Next(40, 96);
            luck += item.luckWeight;

            if (item.category == "fruit" && dayState.routePlanId == "fresh")
            {
                luck += 6;
            }

            return Mathf.Clamp(luck, 25, 100);
        }

        private int GetVendorFamiliarity(VendorProfile vendor)
        {
            if (vendor == null)
            {
                return 0;
            }

            int value;
            if (familiarityByVendor.TryGetValue(vendor.vendorId, out value))
            {
                return value;
            }

            familiarityByVendor[vendor.vendorId] = vendor.familiarity;
            return vendor.familiarity;
        }

        private void AddVendorFamiliarity(string vendorId, int amount)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                return;
            }

            int value = 0;
            familiarityByVendor.TryGetValue(vendorId, out value);
            familiarityByVendor[vendorId] = Mathf.Clamp(value + amount, 0, 10);
        }

        private int RoundMoneyToThousand(int amount)
        {
            return Mathf.RoundToInt(amount / 1000f) * 1000;
        }

        private string FormatClock(int minutes)
        {
            int hours = minutes / 60;
            int mins = minutes % 60;
            return hours.ToString("00") + ":" + mins.ToString("00");
        }

        private string FormatMoney(int amount)
        {
            string raw = Mathf.Abs(amount).ToString();
            string result = string.Empty;
            while (raw.Length > 3)
            {
                result = "." + raw.Substring(raw.Length - 3) + result;
                raw = raw.Substring(0, raw.Length - 3);
            }

            result = raw + result + "d";
            if (amount < 0)
            {
                result = "-" + result;
            }

            return result;
        }
    }
}
