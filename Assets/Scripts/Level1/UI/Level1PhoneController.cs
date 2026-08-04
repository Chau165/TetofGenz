using ChoTet.Menu;
using ChoTet.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace ChoTet.Level1.UI
{
    /// <summary>Runtime phone used in Level 1 for live directions and ride details.</summary>
    public class Level1PhoneController : MonoBehaviour
    {
        private static readonly Color PhoneBlack = new Color(0.025f, 0.03f, 0.035f, 0.98f);
        private static readonly Color ScreenCream = new Color(0.96f, 0.92f, 0.82f, 1f);
        private static readonly Color TetRed = new Color(0.72f, 0.13f, 0.12f, 1f);
        private static readonly Color MapGreen = new Color(0.18f, 0.48f, 0.29f, 1f);

        private InventoryUIController ownerUi;
        private GameObject phoneRoot;
        private GameObject homePanel;
        private GameObject mapPanel;
        private GameObject ticketPanel;
        private Text headerText;
        private Text mapStatusText;
        private Text ticketText;
        private readonly RectTransform[] routeSegments = new RectTransform[3];
        private readonly RectTransform[] routeArrows = new RectTransform[3];
        private RectTransform destinationRoad;
        private RectTransform stationMarker;
        private GameObject phoneModel;
        private Material phoneMaterial;
        private Transform player;
        private bool built;

        public bool IsOpen => phoneRoot != null && phoneRoot.activeSelf;

        public void EnsureBuilt(Transform uiRoot, InventoryUIController owner)
        {
            if (built || uiRoot == null)
            {
                return;
            }

            ownerUi = owner;
            BuildPhoneUi(uiRoot);
            built = true;
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (!built || phoneRoot == null)
            {
                return;
            }

            player = FindAnyObjectByType<PlayerTopDownController>()?.transform;
            if (player != null)
            {
                player.GetComponent<PlayerTopDownController>().MovementLocked = true;
            }

            EnsurePhoneModel();
            phoneRoot.SetActive(true);
            if (phoneModel != null)
            {
                phoneModel.SetActive(true);
            }

            ShowHome();
            ownerUi?.RefreshGameplayPrompts();
        }

        public void Close()
        {
            if (phoneRoot != null)
            {
                phoneRoot.SetActive(false);
            }

            if (phoneModel != null)
            {
                phoneModel.SetActive(false);
            }

            PlayerTopDownController controller = player != null
                ? player.GetComponent<PlayerTopDownController>()
                : FindAnyObjectByType<PlayerTopDownController>();
            Level1PhaseController phase = Level1PhaseController.Instance;
            if (controller != null && (phase == null || !phase.IsTransitioning))
            {
                controller.MovementLocked = false;
            }

            ownerUi?.RefreshGameplayPrompts();
        }

        private void Update()
        {
            if (IsOpen && mapPanel != null && mapPanel.activeSelf)
            {
                RefreshMap();
            }
        }

        private void OnDestroy()
        {
            if (phoneMaterial != null)
            {
                Destroy(phoneMaterial);
            }
        }

        private void BuildPhoneUi(Transform uiRoot)
        {
            phoneRoot = CreateImage("Level1Phone", uiRoot, new Color(0f, 0f, 0f, 0.32f));
            Stretch(phoneRoot.GetComponent<RectTransform>());

            Button backdrop = phoneRoot.AddComponent<Button>();
            backdrop.transition = Selectable.Transition.None;
            backdrop.targetGraphic = phoneRoot.GetComponent<Image>();
            backdrop.onClick.AddListener(Close);

            GameObject frame = CreateImage("PhoneFrame", phoneRoot.transform, PhoneBlack);
            Layout(frame.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(470f, 790f));

            GameObject screen = CreateImage("PhoneScreen", frame.transform, ScreenCream);
            Layout(screen.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -5f), new Vector2(420f, 715f));

            GameObject speaker = CreateImage("Speaker", frame.transform, new Color(0.2f, 0.22f, 0.23f, 1f));
            Layout(speaker.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(86f, 8f));

            headerText = CreateText("Header", screen.transform, "ĐIỆN THOẠI", 23, FontStyle.Bold, TextAnchor.MiddleCenter, PhoneBlack);
            Layout(headerText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(350f, 54f));

            Button closeButton = CreateButton("ClosePhone", frame.transform, "ĐÓNG", TetRed, Close);
            Layout(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 23f), new Vector2(150f, 42f));

            homePanel = CreatePanel("Home", screen.transform);
            StretchWithInsets(homePanel.GetComponent<RectTransform>(), 24f, 76f, 24f, 28f);

            Text greeting = CreateText("Greeting", homePanel.transform, "Đi đâu Tết này?", 26, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.28f, 0.18f, 0.1f, 1f));
            Layout(greeting.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(340f, 70f));

            Button mapButton = CreateAppButton("MapApp", homePanel.transform, "Map", MapGreen, true, ShowMap);
            Layout(mapButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(-92f, 55f), new Vector2(160f, 180f));

            Button ticketButton = CreateAppButton("TicketApp", homePanel.transform, "Ticket", TetRed, false, ShowTicket);
            Layout(ticketButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(92f, 55f), new Vector2(160f, 180f));

            Text hint = CreateText("PhoneHint", homePanel.transform,
                "Mở Map để tìm đường. Mở Ticket để đối chiếu đúng xe.", 19, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Color(0.3f, 0.25f, 0.2f, 1f));
            Layout(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(350f, 90f));

            mapPanel = CreatePanel("MapPanel", screen.transform);
            StretchWithInsets(mapPanel.GetComponent<RectTransform>(), 20f, 76f, 20f, 24f);
            BuildMapPanel(mapPanel.transform);

            ticketPanel = CreatePanel("TicketPanel", screen.transform);
            StretchWithInsets(ticketPanel.GetComponent<RectTransform>(), 20f, 76f, 20f, 24f);
            BuildTicketPanel(ticketPanel.transform);

            phoneRoot.SetActive(false);
        }

        private void BuildMapPanel(Transform parent)
        {
            Button back = CreateButton("MapBack", parent, "< ỨNG DỤNG", new Color(0.36f, 0.29f, 0.2f, 1f), ShowHome);
            Layout(back.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(80f, -28f), new Vector2(150f, 42f));

            GameObject mapArea = CreateImage("LiveMap", parent, new Color(0.78f, 0.86f, 0.69f, 1f));
            Layout(mapArea.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(350f, 460f));

            Color roadColor = new Color(0.32f, 0.34f, 0.33f, 1f);
            GameObject mainRoad = CreateImage("MainRoad", mapArea.transform, roadColor);
            Layout(mainRoad.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -105f), new Vector2(86f, 250f));

            GameObject crossRoad = CreateImage("CrossRoad", mapArea.transform, roadColor);
            Layout(crossRoad.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), new Vector2(330f, 76f));

            destinationRoad = CreateImage("DestinationRoad", mapArea.transform, roadColor).GetComponent<RectTransform>();
            Layout(destinationRoad, new Vector2(0.5f, 0.5f), new Vector2(100f, 70f), new Vector2(86f, 250f));

            Color routeColor = new Color(1f, 0.78f, 0.12f, 1f);
            string[] segmentNames = { "RouteStraight", "RouteTurn", "RouteFinal" };
            for (int i = 0; i < routeSegments.Length; i++)
            {
                routeSegments[i] = CreateImage(segmentNames[i], mapArea.transform, routeColor).GetComponent<RectTransform>();
                Text arrow = CreateText($"RouteArrow{i + 1}", mapArea.transform, "▲", 25, FontStyle.Bold,
                    TextAnchor.MiddleCenter, routeColor);
                arrow.rectTransform.sizeDelta = new Vector2(30f, 30f);
                routeArrows[i] = arrow.rectTransform;
            }

            stationMarker = CreateImage("BusStation", mapArea.transform, TetRed).GetComponent<RectTransform>();
            stationMarker.sizeDelta = new Vector2(26f, 26f);

            RectTransform playerMarker = CreateImage("PlayerMarker", mapArea.transform, new Color(0.12f, 0.38f, 0.86f, 1f)).GetComponent<RectTransform>();
            playerMarker.anchorMin = playerMarker.anchorMax = new Vector2(0.5f, 0.5f);
            playerMarker.anchoredPosition = new Vector2(0f, -165f);
            playerMarker.sizeDelta = new Vector2(24f, 24f);

            Text you = CreateText("YouLabel", mapArea.transform, "BẠN", 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Layout(you.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -165f), new Vector2(52f, 24f));

            Text station = CreateText("StationLabel", stationMarker, "BẾN", 13, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(station.rectTransform);

            mapStatusText = CreateText("MapStatus", parent, string.Empty, 20, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Color(0.24f, 0.2f, 0.15f, 1f));
            Layout(mapStatusText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(355f, 78f));
        }

        private void BuildTicketPanel(Transform parent)
        {
            Button back = CreateButton("TicketBack", parent, "< ỨNG DỤNG", new Color(0.36f, 0.29f, 0.2f, 1f), ShowHome);
            Layout(back.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(80f, -28f), new Vector2(150f, 42f));

            GameObject card = CreateImage("TicketCard", parent, new Color(1f, 0.98f, 0.88f, 1f));
            Layout(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(350f, 455f));

            Text title = CreateText("TicketTitle", card.transform, "VÉ XE VỀ QUÊ", 27, FontStyle.Bold,
                TextAnchor.MiddleCenter, TetRed);
            Layout(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(310f, 66f));

            ticketText = CreateText("TicketDetails", card.transform, string.Empty, 22, FontStyle.Normal,
                TextAnchor.UpperLeft, new Color(0.18f, 0.15f, 0.12f, 1f));
            StretchWithInsets(ticketText.rectTransform, 28f, 105f, 28f, 30f);
        }

        private void ShowHome()
        {
            SetPanel(homePanel);
            if (headerText != null)
            {
                headerText.text = "ĐIỆN THOẠI";
            }
        }

        private void ShowMap()
        {
            SetPanel(mapPanel);
            headerText.text = "MAP - ĐẾN BẾN XE";
            RefreshMap();
        }

        private void ShowTicket()
        {
            SetPanel(ticketPanel);
            headerText.text = "TICKET";
            RefreshTicket();
        }

        private void SetPanel(GameObject activePanel)
        {
            homePanel.SetActive(activePanel == homePanel);
            mapPanel.SetActive(activePanel == mapPanel);
            ticketPanel.SetActive(activePanel == ticketPanel);
        }

        private void RefreshMap()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<PlayerTopDownController>()?.transform;
            }

            if (player == null || routeSegments[0] == null || stationMarker == null || mapStatusText == null)
            {
                return;
            }

            Vector3 delta = Level1PickupStopLayout.BusStopWaitPosition - player.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            float side = distance > 0.01f ? Vector3.Dot(delta.normalized, player.right) : 0f;
            if (Mathf.Abs(side) < 0.12f)
            {
                side = delta.x < 0f ? -1f : 1f;
            }

            float turnX = Mathf.Sign(side) * 100f;
            Vector2 start = new Vector2(0f, -165f);
            Vector2 firstTurn = new Vector2(0f, -55f);
            Vector2 secondTurn = new Vector2(turnX, -55f);
            Vector2 end = new Vector2(turnX, 135f);

            SetLine(routeSegments[0], start, firstTurn, 10f);
            SetLine(routeSegments[1], firstTurn, secondTurn, 10f);
            SetLine(routeSegments[2], secondTurn, end, 10f);
            SetArrow(routeArrows[0], start, firstTurn);
            SetArrow(routeArrows[1], firstTurn, secondTurn);
            SetArrow(routeArrows[2], secondTurn, end);

            destinationRoad.anchoredPosition = new Vector2(turnX, 70f);
            stationMarker.anchorMin = stationMarker.anchorMax = new Vector2(0.5f, 0.5f);
            stationMarker.anchoredPosition = end;

            mapStatusText.text = distance <= 3f
                ? "Bạn đã đến điểm đón xe."
                : $"Đi thẳng rồi rẽ {(turnX < 0f ? "trái" : "phải")}\nCòn khoảng {Mathf.CeilToInt(distance)} m đến bến xe";
        }

        private void RefreshTicket()
        {
            string colorName = "Đang cập nhật";
            string vehicleType = "Xe đón về quê";
            if (BusStopChallengeSession.TryGetHomeRideSlot(out BusStopCandidateSlot slot))
            {
                colorName = Level1TrafficVisualUtility.GetBodyColorName(slot.BodyColorIndex);
                vehicleType = $"Xe đón số {slot.PrefabIndex + 1}";
            }

            ticketText.text =
                $"Biển số:  {VehicleBookingSession.LicensePlate}\n\n" +
                $"Giờ đón:  {VehicleBookingSession.PickupTime}\n\n" +
                $"Màu xe:   {colorName}\n\n" +
                $"Loại xe:  {vehicleType}\n\n" +
                "Điểm đón: Bến xe cuối đường\n\n" +
                "Hãy đối chiếu đúng biển số trước khi lên xe.";
        }

        private void EnsurePhoneModel()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            if (phoneModel == null)
            {
                GameObject source = Resources.Load<GameObject>("Level1/Phone/IPhone8");
                if (source == null)
                {
                    return;
                }

                phoneModel = Instantiate(source, camera.transform);
                phoneModel.name = "Level1PhoneModel";
                foreach (Collider collider in phoneModel.GetComponentsInChildren<Collider>(true))
                {
                    Destroy(collider);
                }

                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    phoneMaterial = new Material(shader)
                    {
                        color = new Color(0.025f, 0.028f, 0.032f, 1f)
                    };
                    foreach (Renderer renderer in phoneModel.GetComponentsInChildren<Renderer>(true))
                    {
                        renderer.sharedMaterial = phoneMaterial;
                    }
                }
            }

            phoneModel.transform.SetParent(camera.transform, false);
            phoneModel.transform.localPosition = new Vector3(0.07f, -0.03f, 0.86f);
            phoneModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            phoneModel.transform.localScale = Vector3.one * 0.39f;
        }

        private static string ResolveDirection(Vector3 delta)
        {
            float angle = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            if (angle < 0f)
            {
                angle += 360f;
            }

            string[] directions = { "Bắc", "Đông Bắc", "Đông", "Đông Nam", "Nam", "Tây Nam", "Tây", "Tây Bắc" };
            int index = Mathf.RoundToInt(angle / 45f) % directions.Length;
            return directions[index];
        }

        private static void SetLine(RectTransform line, Vector2 start, Vector2 end, float width)
        {
            Vector2 delta = end - start;
            line.anchorMin = line.anchorMax = new Vector2(0.5f, 0.5f);
            line.anchoredPosition = (start + end) * 0.5f;
            line.sizeDelta = new Vector2(delta.magnitude, width);
            line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private static void SetArrow(RectTransform arrow, Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            arrow.anchorMin = arrow.anchorMax = new Vector2(0.5f, 0.5f);
            arrow.anchoredPosition = Vector2.Lerp(start, end, 0.62f);
            arrow.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, delta.normalized));
        }

        private static GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            return panel;
        }

        private static GameObject CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return imageObject;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color, UnityEngine.Events.UnityAction callback)
        {
            GameObject buttonObject = CreateImage(name, parent, color);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            button.onClick.AddListener(callback);

            Text text = CreateText("Label", buttonObject.transform, label, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            StretchWithInsets(text.rectTransform, 10f, 8f, 10f, 8f);
            return button;
        }

        private static Button CreateAppButton(string name, Transform parent, string label, Color iconColor, bool isMap,
            UnityEngine.Events.UnityAction callback)
        {
            GameObject buttonObject = CreateImage(name, parent, Color.clear);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            button.onClick.AddListener(callback);

            GameObject icon = CreateImage(name + "Icon", buttonObject.transform, iconColor);
            Layout(icon.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(112f, 112f));

            if (isMap)
            {
                GameObject road = CreateImage("MapRoad", icon.transform, Color.white);
                Layout(road.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 86f));
                GameObject route = CreateImage("MapRoute", icon.transform, new Color(1f, 0.78f, 0.12f, 1f));
                Layout(route.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0f, -5f), new Vector2(8f, 58f));
                Text marker = CreateText("MapPin", icon.transform, "●", 24, FontStyle.Bold, TextAnchor.MiddleCenter, TetRed);
                Layout(marker.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(30f, 30f));
            }
            else
            {
                GameObject ticket = CreateImage("TicketShape", icon.transform, Color.white);
                Layout(ticket.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(78f, 54f));
                GameObject stripe = CreateImage("TicketStripe", ticket.transform, iconColor);
                Layout(stripe.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 54f));
                Text ticketMark = CreateText("TicketMark", ticket.transform, "T", 24, FontStyle.Bold,
                    TextAnchor.MiddleCenter, iconColor);
                Layout(ticketMark.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-22f, 0f), new Vector2(28f, 32f));
            }

            Text appName = CreateText("AppName", buttonObject.transform, label, 20, FontStyle.Bold,
                TextAnchor.MiddleCenter, PhoneBlack);
            Layout(appName.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -57f), new Vector2(140f, 36f));
            return button;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle style,
            TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void Layout(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchWithInsets(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
