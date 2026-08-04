using System;
using ChoTet.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ChoTet.Runtime
{
    public sealed class CoHuongBargainMinigame : MonoBehaviour
    {
        private const float MinAngle = 20f;
        private const float MaxAngle = 160f;

        [Header("NPC")]
        public Transform reactionRoot;

        [Header("Wheel")]
        public float arrowSpeed = 0.62f;
        public float cooldownSecondsMin = 30f;
        public float cooldownSecondsMax = 60f;

        private const int RedSlotCount = 4;
        private readonly System.Collections.Generic.List<Text> slotLabels = new System.Collections.Generic.List<Text>();
        private WheelSlot[] slots = Array.Empty<WheelSlot>();

        private GameFlowController flow;
        private VendorStall stall;
        private Canvas canvas;
        private GameObject panel;
        private Text dialogueTitle;
        private Text dialogueBody;
        private Text feedbackText;
        private RectTransform arrow;
        private Image flashImage;
        private AudioSource audioSource;
        private CrescentPriceWheelGraphic wheelGraphic;
        private float cooldownUntil;
        private float effectiveArrowSpeed;
        private bool running;
        private bool resolving;
        private float normalizedPosition;
        private Vector3 reactionStartEuler;

        public bool IsCoolingDown
        {
            get { return Time.time < cooldownUntil; }
        }

        public float CooldownRemaining
        {
            get { return Mathf.Max(0f, cooldownUntil - Time.time); }
        }

        private void Awake()
        {
            stall = GetComponent<VendorStall>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.38f;
            effectiveArrowSpeed = arrowSpeed;
        }

        private void Update()
        {
            if (!running || resolving)
            {
                return;
            }

            normalizedPosition = Mathf.PingPong(Time.unscaledTime * effectiveArrowSpeed, 1f);
            float angle = Mathf.Lerp(MinAngle, MaxAngle, normalizedPosition);
            arrow.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);

            if (WasStopPressed())
            {
                StopWheel();
            }
        }

        public void Begin(GameFlowController activeFlow, VendorStall activeStall, int quote, string productInfo, float speedMultiplier)
        {
            flow = activeFlow;
            stall = activeStall != null ? activeStall : stall;
            effectiveArrowSpeed = Mathf.Max(0.1f, arrowSpeed * Mathf.Max(1f, speedMultiplier));

            if (IsCoolingDown)
            {
                flow.ShowCoHuongCooldown(Mathf.CeilToInt(CooldownRemaining));
                return;
            }

            EnsureUi();
            BuildRandomizedSlots();
            RefreshWheelLayout();
            panel.SetActive(true);
            running = true;
            resolving = false;
            normalizedPosition = 0f;
            feedbackText.text = "Nhấn Stop đúng ô xanh để nhận giảm giá. Ô đỏ là người bán chưa chịu bớt.";
            SetDialogueText(productInfo, quote, speedMultiplier);
        }

        private void StopWheel()
        {
            resolving = true;
            running = false;

            WheelSlot selected = GetSelectedSlot();
            if (selected.isDiscount)
            {
                feedbackText.text = "Trúng ô xanh! Người bán đồng ý giảm " + selected.discountPercent + "%.";
                PlayTone(880f, 0.12f);
                StartCoroutine(FlashAndFinish(new Color(0.2f, 1f, 0.35f, 0.5f), 0.55f, () =>
                {
                    panel.SetActive(false);
                    StartCoroutine(React(true));
                    flow.CompleteCoHuongBargainSuccess(selected.discountPercent);
                }));
                return;
            }

            int cooldown = Mathf.RoundToInt(UnityEngine.Random.Range(cooldownSecondsMin, cooldownSecondsMax));
            cooldownUntil = Time.time + cooldown;
            feedbackText.text = "Ô đỏ! Người bán lắc đầu, cần đợi " + cooldown + " giây mới trả giá lại.";
            PlayTone(160f, 0.22f);
            StartCoroutine(FlashAndFinish(new Color(1f, 0.1f, 0.05f, 0.48f), 0.75f, () =>
            {
                panel.SetActive(false);
                StartCoroutine(React(false));
                flow.HandleCoHuongBargainFailure(cooldown);
            }));
        }

        private WheelSlot GetSelectedSlot()
        {
            float totalWeight = GetTotalSlotWeight();
            float targetWeight = normalizedPosition * totalWeight;
            float accumulatedWeight = 0f;
            for (int i = 0; i < slots.Length; i++)
            {
                accumulatedWeight += slots[i].weight;
                if (targetWeight <= accumulatedWeight || i == slots.Length - 1)
                {
                    return slots[i];
                }
            }

            return slots[slots.Length - 1];
        }

        private bool WasStopPressed()
        {
            if (Keyboard.current != null
                && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        private void EnsureUi()
        {
            if (canvas != null)
            {
                return;
            }

            EnsureEventSystem();

            GameObject canvasObject = new GameObject("VendorBargainCanvas");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 160;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            panel = CreatePanel("VendorBargainPanel", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.42f));
            GameObject dialogue = CreatePanel("DialoguePanel", panel.transform, new Vector2(0.12f, 0.66f), new Vector2(0.92f, 0.94f), Vector2.zero, Vector2.zero, new Color(0.36f, 0.2f, 0.12f, 0.96f));
            dialogueTitle = CreateText("DialogueTitle", dialogue.transform, font, 28, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            ApplyStretchRect(dialogueTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -58f), new Vector2(-28f, -16f), new Vector2(0f, 1f));

            dialogueBody = CreateText("DialogueBody", dialogue.transform, font, 19, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            dialogueBody.lineSpacing = 1.02f;
            dialogueBody.resizeTextForBestFit = true;
            dialogueBody.resizeTextMinSize = 14;
            dialogueBody.resizeTextMaxSize = 19;
            dialogueBody.verticalOverflow = VerticalWrapMode.Truncate;
            ApplyStretchRect(dialogueBody.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 18f), new Vector2(-28f, -66f), new Vector2(0f, 1f));

            GameObject wheelRoot = CreatePanel("CrescentWheelPanel", panel.transform, new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), new Vector2(0f, 0f), new Vector2(880f, 520f), new Color(0.11f, 0.08f, 0.07f, 0.94f));
            GameObject wheelGraphicObject = new GameObject("CrescentWheelGraphic", typeof(RectTransform), typeof(CanvasRenderer));
            wheelGraphicObject.transform.SetParent(wheelRoot.transform, false);
            wheelGraphic = wheelGraphicObject.AddComponent<CrescentPriceWheelGraphic>();

            RectTransform wheelRect = wheelGraphic.GetComponent<RectTransform>();
            wheelRect.anchorMin = Vector2.zero;
            wheelRect.anchorMax = Vector2.one;
            wheelRect.offsetMin = Vector2.zero;
            wheelRect.offsetMax = Vector2.zero;

            for (int i = 0; i < slots.Length; i++)
            {
                float t = (i + 0.5f) / slots.Length;
                float angle = Mathf.Lerp(MinAngle, MaxAngle, t) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(angle) * 252f, Mathf.Sin(angle) * 252f - 165f);
                Text label = CreateText("SlotLabel_" + i, wheelRoot.transform, font, 22, TextAnchor.MiddleCenter, pos, new Vector2(110f, 42f));
                label.text = slots[i].isDiscount ? "-" + slots[i].discountPercent + "%" : "TRƯỢT";
                label.color = Color.white;
                label.fontStyle = FontStyle.Bold;
            }

            arrow = CreatePanel("WheelArrow", wheelRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -165f), new Vector2(12f, 275f), new Color(1f, 0.92f, 0.18f, 1f)).GetComponent<RectTransform>();
            arrow.pivot = new Vector2(0.5f, 0f);

            Button stopButton = CreateButton("StopButton", wheelRoot.transform, font, new Vector2(0f, -350f), new Vector2(250f, 72f));
            stopButton.onClick.AddListener(StopWheel);

            feedbackText = CreateText("BargainFeedback", wheelRoot.transform, font, 24, TextAnchor.MiddleCenter, new Vector2(0f, -430f), new Vector2(760f, 70f));
            feedbackText.color = new Color(1f, 0.91f, 0.68f, 1f);

            flashImage = CreatePanel("ResultFlash", panel.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear).GetComponent<Image>();
            flashImage.raycastTarget = false;
            panel.SetActive(false);
        }

        private void BuildRandomizedSlots()
        {
            int[] discountValues = { 5, 10, 15, 20 };
            float[] discountWeights = { 1.25f, 1.05f, 0.85f, 0.65f };
            System.Collections.Generic.List<WheelSlot> discounts = new System.Collections.Generic.List<WheelSlot>(discountValues.Length);
            System.Collections.Generic.List<WheelSlot> layout = new System.Collections.Generic.List<WheelSlot>(discountValues.Length + RedSlotCount);

            for (int i = 0; i < discountValues.Length; i++)
            {
                discounts.Add(new WheelSlot(true, discountValues[i], discountWeights[i]));
            }

            for (int i = discounts.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                WheelSlot current = discounts[i];
                discounts[i] = discounts[swapIndex];
                discounts[swapIndex] = current;
            }

            bool startsWithDiscount = UnityEngine.Random.value >= 0.5f;
            for (int i = 0; i < discounts.Count; i++)
            {
                WheelSlot redSlot = new WheelSlot(false, 0, 1.35f);
                if (startsWithDiscount)
                {
                    layout.Add(discounts[i]);
                    layout.Add(redSlot);
                }
                else
                {
                    layout.Add(redSlot);
                    layout.Add(discounts[i]);
                }
            }

            slots = layout.ToArray();
        }

        private void RefreshWheelLayout()
        {
            wheelGraphic.SetSlots(slots);
            float totalWeight = GetTotalSlotWeight();
            float accumulatedWeight = 0f;

            while (slotLabels.Count < slots.Length)
            {
                slotLabels.Add(CreateText("SlotLabel_" + slotLabels.Count, wheelGraphic.transform.parent, dialogueTitle.font, 22, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(110f, 42f)));
            }

            for (int i = 0; i < slotLabels.Count; i++)
            {
                Text label = slotLabels[i];
                bool visible = i < slots.Length;
                label.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                WheelSlot slot = slots[i];
                float midpoint = (accumulatedWeight + slot.weight * 0.5f) / totalWeight;
                float angle = Mathf.Lerp(MinAngle, MaxAngle, midpoint) * Mathf.Deg2Rad;
                label.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle) * 252f, Mathf.Sin(angle) * 252f - 165f);
                label.text = slot.isDiscount ? "-" + slot.discountPercent + "%" : "TRƯỢT";
                label.color = Color.white;
                label.fontStyle = FontStyle.Bold;
                accumulatedWeight += slot.weight;
            }
        }

        private float GetTotalSlotWeight()
        {
            float totalWeight = 0f;
            for (int i = 0; i < slots.Length; i++)
            {
                totalWeight += slots[i].weight;
            }

            return Mathf.Max(0.01f, totalWeight);
        }

        private void SetDialogueText(string productInfo, int quote, float speedMultiplier)
        {
            VendorProfile profile = stall != null ? stall.VendorProfile : null;
            dialogueTitle.text = profile != null ? profile.displayName : "Người bán";
            string greeting = profile != null ? profile.greeting : "Mời con xem hàng.";
            dialogueBody.text = greeting
                + "\n" + productInfo
                + "\nGiá đưa ra: " + quote.ToString("N0") + "đ. Bấm Stop để thử trả giá."
                + "\nTốc độ kim: x" + speedMultiplier.ToString("0.00");
        }

        private System.Collections.IEnumerator FlashAndFinish(Color color, float duration, Action onDone)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(color.a, 0f, elapsed / duration);
                flashImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            flashImage.color = Color.clear;
            onDone?.Invoke();
        }

        private System.Collections.IEnumerator React(bool success)
        {
            if (reactionRoot == null)
            {
                yield break;
            }

            reactionStartEuler = reactionRoot.localEulerAngles;
            float duration = 0.48f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float wave = Mathf.Sin(elapsed * 26f) * (1f - elapsed / duration);
                Vector3 euler = reactionStartEuler;
                if (success)
                {
                    euler.x += wave * 8f;
                }
                else
                {
                    euler.y += wave * 13f;
                }

                reactionRoot.localEulerAngles = euler;
                yield return null;
            }

            reactionRoot.localEulerAngles = reactionStartEuler;
        }

        private void PlayTone(float frequency, float seconds)
        {
            int sampleRate = 24000;
            int count = Mathf.CeilToInt(sampleRate * seconds);
            AudioClip clip = AudioClip.Create("VendorBargainTone", count, 1, sampleRate, false);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float fade = 1f - (i / (float)count);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * 0.16f * fade;
            }

            clip.SetData(data, 0);
            audioSource.PlayOneShot(clip);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        private static Text CreateText(string name, Transform parent, Font font, int fontSize, TextAnchor alignment, Vector2 anchoredPosition, Vector2 rectSize)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = rectSize;

            Text text = obj.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            return text;
        }

        private static void ApplyStretchRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static Button CreateButton(string name, Transform parent, Font font, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject obj = CreatePanel(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size, new Color(0.82f, 0.26f, 0.12f, 1f));
            Button button = obj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.42f, 0.2f, 1f);
            colors.pressedColor = new Color(0.55f, 0.12f, 0.07f, 1f);
            button.colors = colors;

            Text text = CreateText("Text", obj.transform, font, 28, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.text = "STOP";
            text.fontStyle = FontStyle.Bold;
            return button;
        }

        private readonly struct WheelSlot
        {
            public readonly bool isDiscount;
            public readonly int discountPercent;
            public readonly float weight;

            public WheelSlot(bool isDiscount, int discountPercent, float weight)
            {
                this.isDiscount = isDiscount;
                this.discountPercent = discountPercent;
                this.weight = weight;
            }
        }

        private sealed class CrescentPriceWheelGraphic : MaskableGraphic
        {
            private WheelSlot[] currentSlots = Array.Empty<WheelSlot>();

            public void SetSlots(WheelSlot[] newSlots)
            {
                currentSlots = newSlots ?? Array.Empty<WheelSlot>();
                SetVerticesDirty();
            }

            protected override void OnPopulateMesh(VertexHelper vh)
            {
                vh.Clear();
                if (currentSlots.Length == 0)
                {
                    return;
                }

                Vector2 center = new Vector2(0f, -165f);
                float innerRadius = 155f;
                float outerRadius = 290f;
                float totalWeight = 0f;
                for (int i = 0; i < currentSlots.Length; i++)
                {
                    totalWeight += currentSlots[i].weight;
                }

                float accumulatedWeight = 0f;
                float range = MaxAngle - MinAngle;
                for (int i = 0; i < currentSlots.Length; i++)
                {
                    float start = MinAngle + range * accumulatedWeight / totalWeight;
                    accumulatedWeight += currentSlots[i].weight;
                    float end = MinAngle + range * accumulatedWeight / totalWeight;
                    Color32 slotColor = currentSlots[i].isDiscount
                        ? new Color32(45, 176, 84, 255)
                        : new Color32(194, 49, 42, 255);
                    AddSegment(vh, center, innerRadius, outerRadius, start, end, slotColor);
                }
            }

            private static void AddSegment(VertexHelper vh, Vector2 center, float innerRadius, float outerRadius, float startDeg, float endDeg, Color32 slotColor)
            {
                int steps = 6;
                for (int i = 0; i < steps; i++)
                {
                    float a0 = Mathf.Lerp(startDeg, endDeg, i / (float)steps) * Mathf.Deg2Rad;
                    float a1 = Mathf.Lerp(startDeg, endDeg, (i + 1) / (float)steps) * Mathf.Deg2Rad;
                    int index = vh.currentVertCount;
                    vh.AddVert(center + new Vector2(Mathf.Cos(a0) * innerRadius, Mathf.Sin(a0) * innerRadius), slotColor, Vector2.zero);
                    vh.AddVert(center + new Vector2(Mathf.Cos(a0) * outerRadius, Mathf.Sin(a0) * outerRadius), slotColor, Vector2.zero);
                    vh.AddVert(center + new Vector2(Mathf.Cos(a1) * outerRadius, Mathf.Sin(a1) * outerRadius), slotColor, Vector2.zero);
                    vh.AddVert(center + new Vector2(Mathf.Cos(a1) * innerRadius, Mathf.Sin(a1) * innerRadius), slotColor, Vector2.zero);
                    vh.AddTriangle(index, index + 1, index + 2);
                    vh.AddTriangle(index, index + 2, index + 3);
                }
            }
        }
    }
}
