using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CatCafe.Social;

namespace CatCafe.Social
{
    public class SocialUI : MonoBehaviour
    {
        public static SocialUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Tab Headers")]
        [SerializeField] private Button tabFriendsBtn;
        [SerializeField] private Button tabChatBtn;
        [SerializeField] private Button tabMailboxBtn;
        [SerializeField] private Button tabCoopBtn;
        [SerializeField] private Button closeBtn;

        [Header("Tab Panels")]
        [SerializeField] private GameObject panelFriends;
        [SerializeField] private GameObject panelChat;
        [SerializeField] private GameObject panelMailbox;
        [SerializeField] private GameObject panelCoop;

        [Header("Friends Tab Elements")]
        [SerializeField] private Text myCodeText;
        [SerializeField] private Button copyCodeBtn;
        [SerializeField] private InputField addFriendInput;
        [SerializeField] private Button addFriendBtn;
        [SerializeField] private Transform friendsContent;

        [Header("Chat Tab Elements")]
        [SerializeField] private Transform chatContent;
        [SerializeField] private InputField chatInputField;
        [SerializeField] private Button sendChatBtn;

        [Header("Mailbox Tab Elements")]
        [SerializeField] private Transform mailContent;
        [SerializeField] private Button claimAllBtn;

        [Header("Co-op Quests Elements")]
        [SerializeField] private Transform coopContent;

        private string currentTab = "friends";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUIHierarchyIfNeeded();
            RegisterEvents();
        }

        private void Start()
        {
            if (windowRoot != null) windowRoot.SetActive(false);
        }

        public static void Show(string defaultTab = "friends")
        {
            if (Instance == null)
            {
                // Tự sinh SocialUI nếu chưa có trên Canvas
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject obj = new GameObject("Social_Hub_System");
                    obj.transform.SetParent(canvas.transform, false);
                    Instance = obj.AddComponent<SocialUI>();
                    Instance.BuildUIHierarchyIfNeeded();
                    Instance.RegisterEvents();
                }
            }

            if (Instance != null)
            {
                Instance.OpenWindow(defaultTab);
            }
        }

        public void OpenWindow(string tabName = "friends")
        {
            if (windowRoot != null) windowRoot.SetActive(true);
            SwitchTab(tabName);
            RefreshAllTabs();
        }

        public void CloseWindow()
        {
            if (windowRoot != null) windowRoot.SetActive(false);
        }

        private void SwitchTab(string tabName)
        {
            currentTab = tabName;
            if (panelFriends) panelFriends.SetActive(tabName == "friends");
            if (panelChat) panelChat.SetActive(tabName == "chat");
            if (panelMailbox) panelMailbox.SetActive(tabName == "mailbox");
            if (panelCoop) panelCoop.SetActive(tabName == "coop");

            // Đổi màu highlight cho các nút tab
            HighlightTabButton(tabFriendsBtn, tabName == "friends");
            HighlightTabButton(tabChatBtn, tabName == "chat");
            HighlightTabButton(tabMailboxBtn, tabName == "mailbox");
            HighlightTabButton(tabCoopBtn, tabName == "coop");
        }

        private void HighlightTabButton(Button btn, bool isActive)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? new Color(0.98f, 0.45f, 0.35f, 0.95f) : new Color(0.18f, 0.22f, 0.32f, 0.8f);
            }
        }

        public void RefreshAllTabs()
        {
            RefreshFriendsTab();
            RefreshChatTab();
            RefreshMailboxTab();
            RefreshCoopTab();
        }

        #region Tab Friends
        private void RefreshFriendsTab()
        {
            if (myCodeText != null && FriendManager.Instance != null)
            {
                myCodeText.text = "Mã của bạn: " + FriendManager.Instance.myFriendCode;
            }

            if (friendsContent == null || FriendManager.Instance == null) return;

            // Xóa danh sách cũ
            foreach (Transform child in friendsContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var friend in FriendManager.Instance.friendsList)
            {
                CreateFriendCard(friend, friendsContent);
            }
        }

        private void CreateFriendCard(FriendProfile friend, Transform parent)
        {
            GameObject card = new GameObject("FriendCard_" + friend.friendCode);
            card.transform.SetParent(parent, false);

            var rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(560, 68);

            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.28f, 0.85f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Info Text
            GameObject infoObj = new GameObject("InfoText");
            infoObj.transform.SetParent(card.transform, false);
            var infoRt = infoObj.AddComponent<RectTransform>();
            infoRt.sizeDelta = new Vector2(280, 50);
            var infoTxt = infoObj.AddComponent<Text>();
            infoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (infoTxt.font == null) infoTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            infoTxt.fontSize = 15;
            infoTxt.color = Color.white;
            infoTxt.alignment = TextAnchor.MiddleLeft;
            infoTxt.text = $"<b>{friend.playerName}</b> ({friend.friendCode})\n<color=#FFD54F>Cấp {friend.cafeLevel} ☕</color> | <color=#81C784>{friend.catCount} Bé Mèo 🐱</color> | {friend.DisplayStatus}";

            // Nút Ghé Thăm
            GameObject visitBtnObj = CreateButton(card.transform, "🚀 Ghé Thăm", new Color(0.2f, 0.6f, 0.9f), 110, 42);
            visitBtnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                CloseWindow();
                FriendManager.Instance?.VisitFriendCafe(friend);
            });

            // Nút Tặng Quà
            GameObject giftBtnObj = CreateButton(card.transform, "🎁 Tặng Quà", new Color(0.9f, 0.45f, 0.2f), 110, 42);
            giftBtnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                GiftManager.Instance?.SendGift(friend, "treat", 5, "Tặng bạn hộp cá ngừ cho mèo! 🐟", null);
            });
        }

        private void OnAddFriendClicked()
        {
            if (addFriendInput == null || string.IsNullOrWhiteSpace(addFriendInput.text)) return;
            string code = addFriendInput.text.Trim();

            FriendManager.Instance?.AddFriendByCode(code, (success, msg) =>
            {
                ToastManager.Instance?.ShowToast(msg, success ? "✅" : "⚠️");
                if (success)
                {
                    addFriendInput.text = "";
                    RefreshFriendsTab();
                }
            });
        }

        private void OnCopyCodeClicked()
        {
            if (FriendManager.Instance != null)
            {
                GUIUtility.systemCopyBuffer = FriendManager.Instance.myFriendCode;
                ToastManager.Instance?.ShowToast($"Đã sao chép mã {FriendManager.Instance.myFriendCode} vào bộ nhớ tạm!", "📋", Color.cyan);
            }
        }
        #endregion

        #region Tab Chat
        private void RefreshChatTab()
        {
            if (chatContent == null || ChatManager.Instance == null) return;

            foreach (Transform child in chatContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var msg in ChatManager.Instance.messages)
            {
                CreateChatMessageItem(msg, chatContent);
            }
        }

        private void CreateChatMessageItem(ChatMessage msg, Transform parent)
        {
            GameObject item = new GameObject("ChatMsg");
            item.transform.SetParent(parent, false);

            var rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(560, 48);

            var bg = item.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.15f, 0.24f, 0.7f);

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(item.transform, false);
            var txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(12, 4);
            txtRt.offsetMax = new Vector2(-12, -4);

            var txt = txtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;

            string colorName = (msg.senderId == "system") ? "#FFD54F" : "#80D8FF";
            txt.text = $"<color={colorName}><b>[{msg.senderName}]</b></color> <color=#9E9E9E>({msg.FormattedTime}):</color> {msg.content}";
        }

        private void OnSendChatClicked()
        {
            if (chatInputField == null || string.IsNullOrWhiteSpace(chatInputField.text)) return;
            string txt = chatInputField.text;
            chatInputField.text = "";

            ChatManager.Instance?.SendChatMessage(txt);
        }
        #endregion

        #region Tab Mailbox
        private void RefreshMailboxTab()
        {
            if (mailContent == null || GiftManager.Instance == null) return;

            foreach (Transform child in mailContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var gift in GiftManager.Instance.inboxGifts)
            {
                CreateMailCard(gift, mailContent);
            }
        }

        private void CreateMailCard(GiftItem gift, Transform parent)
        {
            GameObject card = new GameObject("MailCard_" + gift.giftId);
            card.transform.SetParent(parent, false);

            var rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(560, 64);

            var bg = card.AddComponent<Image>();
            bg.color = gift.isClaimed ? new Color(0.12f, 0.14f, 0.2f, 0.5f) : new Color(0.18f, 0.22f, 0.35f, 0.9f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 6, 6);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Info
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);
            var infoRt = infoObj.AddComponent<RectTransform>();
            infoRt.sizeDelta = new Vector2(380, 48);
            var infoTxt = infoObj.AddComponent<Text>();
            infoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (infoTxt.font == null) infoTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            infoTxt.fontSize = 14;
            infoTxt.color = Color.white;
            infoTxt.alignment = TextAnchor.MiddleLeft;

            string status = gift.isClaimed ? "<color=#9E9E9E>[Đã nhận]</color>" : "<color=#FFD54F>[Chưa nhận]</color>";
            infoTxt.text = $"<b>Từ {gift.senderName}:</b> {gift.message}\n{status} Thưởng: +{gift.amount} {gift.giftType}";

            // Button
            if (!gift.isClaimed)
            {
                GameObject claimBtnObj = CreateButton(card.transform, "Nhận 🎁", new Color(0.2f, 0.7f, 0.3f), 100, 40);
                claimBtnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GiftManager.Instance?.ClaimGift(gift);
                    RefreshMailboxTab();
                });
            }
            else
            {
                GameObject claimedLabel = new GameObject("ClaimedLbl");
                claimedLabel.transform.SetParent(card.transform, false);
                var lblRt = claimedLabel.AddComponent<RectTransform>();
                lblRt.sizeDelta = new Vector2(100, 40);
                var lblTxt = claimedLabel.AddComponent<Text>();
                lblTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (lblTxt.font == null) lblTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                lblTxt.fontSize = 13;
                lblTxt.color = Color.gray;
                lblTxt.alignment = TextAnchor.MiddleCenter;
                lblTxt.text = "Đã nhận ✓";
            }
        }
        #endregion

        #region Tab Co-op Quests
        private void RefreshCoopTab()
        {
            if (coopContent == null) return;

            foreach (Transform child in coopContent)
            {
                Destroy(child.gameObject);
            }

            // Tạo các mục nhiệm vụ đồng đội / cộng đồng
            CreateCoopQuestCard(coopContent, "Ghé thăm 2 quán cafe của bạn bè", 2, DailyQuestManager.Instance != null ? 1 : 0, 10, 200f);
            CreateCoopQuestCard(coopContent, "Vuốt ve & chăm sóc 5 bé mèo ở quán bạn", 5, 2, 15, 350f);
            CreateCoopQuestCard(coopContent, "Gửi tặng 3 hộp quà kết thân", 3, 1, 10, 150f);
            CreateCoopQuestCard(coopContent, "Toàn server tích lũy 100 ly cafe", 100, 46, 50, 1000f);
        }

        private void CreateCoopQuestCard(Transform parent, string title, int target, int cur, int rewardGems, float rewardMoney)
        {
            GameObject card = new GameObject("CoopQuestCard");
            card.transform.SetParent(parent, false);

            var rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(560, 66);

            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.14f, 0.18f, 0.28f, 0.85f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 6, 6);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Info
            GameObject infoObj = new GameObject("Info");
            infoObj.transform.SetParent(card.transform, false);
            var infoRt = infoObj.AddComponent<RectTransform>();
            infoRt.sizeDelta = new Vector2(380, 50);
            var infoTxt = infoObj.AddComponent<Text>();
            infoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (infoTxt.font == null) infoTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            infoTxt.fontSize = 14;
            infoTxt.color = Color.white;
            infoTxt.alignment = TextAnchor.MiddleLeft;

            float pct = Mathf.Clamp01((float)cur / target) * 100f;
            infoTxt.text = $"<b>{title}</b>\nTiến độ: <color=#64B5F6>{cur}/{target} ({pct:F0}%)</color> | Thưởng: <color=#FFD54F>+{rewardGems} 💎</color> & <color=#81C784>+${rewardMoney}</color>";

            // Button
            bool canClaim = cur >= target;
            GameObject btnObj = CreateButton(card.transform, canClaim ? "Nhận 🎁" : "Chưa xong", canClaim ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.3f, 0.35f, 0.45f), 100, 40);
            if (canClaim)
            {
                btnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ToastManager.Instance?.ShowToast($"Đã nhận thưởng đồng đội: +{rewardGems} 💎!", "🎉", Color.green);
                    GameManager.Instance?.AddGems(rewardGems);
                    MoneyManager.Instance?.AddMoney(rewardMoney);
                });
            }
        }
        #endregion

        #region UI Generator & Helpers
        private void BuildUIHierarchyIfNeeded()
        {
            if (windowRoot != null) return;

            // Root Window Overlay
            windowRoot = new GameObject("SocialUI_WindowRoot");
            windowRoot.transform.SetParent(transform, false);
            RectTransform rootRt = windowRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            // Modal Dim Background
            var dimBg = windowRoot.AddComponent<Image>();
            dimBg.color = new Color(0f, 0f, 0f, 0.65f);

            // Center Panel
            GameObject panelObj = new GameObject("Center_Dialog");
            panelObj.transform.SetParent(windowRoot.transform, false);
            RectTransform panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(640, 520);

            var panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.09f, 0.11f, 0.18f, 0.96f);

            // Title Bar
            GameObject titleObj = new GameObject("TitleBar");
            titleObj.transform.SetParent(panelObj.transform, false);
            RectTransform titleRt = titleObj.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = Vector2.zero;
            titleRt.sizeDelta = new Vector2(0, 55);

            var titleTxt = titleObj.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (titleTxt.font == null) titleTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 18);
            titleTxt.fontSize = 20;
            titleTxt.color = new Color(1f, 0.85f, 0.4f);
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.text = "🐱 HỘI QUÁN BẠN BÈ & TRÒ CHUYỆN ☕";

            // Close Button
            GameObject closeObj = CreateButton(panelObj.transform, "✖", new Color(0.8f, 0.2f, 0.2f), 44, 44);
            RectTransform closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1, 1);
            closeRt.anchorMax = new Vector2(1, 1);
            closeRt.anchoredPosition = new Vector2(-28, -28);
            closeBtn = closeObj.GetComponent<Button>();
            closeBtn.onClick.AddListener(CloseWindow);

            // Tab Bar
            GameObject tabBarObj = new GameObject("TabBar");
            tabBarObj.transform.SetParent(panelObj.transform, false);
            RectTransform tabRt = tabBarObj.AddComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0, 1);
            tabRt.anchorMax = new Vector2(1, 1);
            tabRt.pivot = new Vector2(0.5f, 1);
            tabRt.anchoredPosition = new Vector2(0, -60);
            tabRt.sizeDelta = new Vector2(-40, 42);

            var tabHlg = tabBarObj.AddComponent<HorizontalLayoutGroup>();
            tabHlg.spacing = 8;
            tabHlg.childControlWidth = true;
            tabHlg.childControlHeight = true;

            tabFriendsBtn = CreateButton(tabBarObj.transform, "👥 Bạn Bè", Color.gray, 0, 42).GetComponent<Button>();
            tabChatBtn = CreateButton(tabBarObj.transform, "💬 Trò Chuyện", Color.gray, 0, 42).GetComponent<Button>();
            tabMailboxBtn = CreateButton(tabBarObj.transform, "📫 Hòm Thư", Color.gray, 0, 42).GetComponent<Button>();
            tabCoopBtn = CreateButton(tabBarObj.transform, "🤝 Đồng Đội", Color.gray, 0, 42).GetComponent<Button>();

            tabFriendsBtn.onClick.AddListener(() => SwitchTab("friends"));
            tabChatBtn.onClick.AddListener(() => SwitchTab("chat"));
            tabMailboxBtn.onClick.AddListener(() => SwitchTab("mailbox"));
            tabCoopBtn.onClick.AddListener(() => SwitchTab("coop"));

            // Container for panels
            GameObject contentContainer = new GameObject("ContentContainer");
            contentContainer.transform.SetParent(panelObj.transform, false);
            RectTransform contRt = contentContainer.AddComponent<RectTransform>();
            contRt.anchorMin = Vector2.zero;
            contRt.anchorMax = Vector2.one;
            contRt.offsetMin = new Vector2(20, 20);
            contRt.offsetMax = new Vector2(-20, -110);

            // 1. Panel Friends
            panelFriends = CreateScrollableTabPanel(contentContainer.transform, "Panel_Friends", out friendsContent);
            CreateFriendsHeaderControls(panelFriends.transform);

            // 2. Panel Chat
            panelChat = CreateScrollableTabPanel(contentContainer.transform, "Panel_Chat", out chatContent);
            CreateChatBottomControls(panelChat.transform);

            // 3. Panel Mailbox
            panelMailbox = CreateScrollableTabPanel(contentContainer.transform, "Panel_Mailbox", out mailContent);
            CreateMailboxTopControls(panelMailbox.transform);

            // 4. Panel Co-op
            panelCoop = CreateScrollableTabPanel(contentContainer.transform, "Panel_Coop", out coopContent);

            SwitchTab("friends");
        }

        private GameObject CreateScrollableTabPanel(Transform parent, string name, out Transform content)
        {
            GameObject p = new GameObject(name);
            p.transform.SetParent(parent, false);
            RectTransform prt = p.AddComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.sizeDelta = Vector2.zero;

            // ScrollRect
            ScrollRect sr = p.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;

            GameObject view = new GameObject("Viewport");
            view.transform.SetParent(p.transform, false);
            RectTransform vrt = view.AddComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.sizeDelta = Vector2.zero;
            view.AddComponent<Mask>().showMaskGraphic = false;
            view.AddComponent<Image>();

            GameObject contObj = new GameObject("Content");
            contObj.transform.SetParent(view.transform, false);
            RectTransform crt = contObj.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(0, 400);

            var vlg = contObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = contObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = vrt;
            sr.content = crt;
            content = contObj.transform;

            return p;
        }

        private void CreateFriendsHeaderControls(Transform parent)
        {
            GameObject headerObj = new GameObject("FriendsHeader");
            headerObj.transform.SetParent(parent, false);
            headerObj.transform.SetAsFirstSibling();
            var rt = headerObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(580, 48);

            var hlg = headerObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Code Text
            GameObject codeTxtObj = new GameObject("MyCodeTxt");
            codeTxtObj.transform.SetParent(headerObj.transform, false);
            var codeRt = codeTxtObj.AddComponent<RectTransform>();
            codeRt.sizeDelta = new Vector2(210, 42);
            myCodeText = codeTxtObj.AddComponent<Text>();
            myCodeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (myCodeText.font == null) myCodeText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            myCodeText.fontSize = 14;
            myCodeText.color = new Color(1f, 0.85f, 0.4f);
            myCodeText.alignment = TextAnchor.MiddleLeft;

            // Copy button
            GameObject copyObj = CreateButton(headerObj.transform, "📋 Chép", new Color(0.2f, 0.5f, 0.8f), 75, 42);
            copyCodeBtn = copyObj.GetComponent<Button>();
            copyCodeBtn.onClick.AddListener(OnCopyCodeClicked);

            // Add Friend input
            GameObject inObj = new GameObject("AddInput");
            inObj.transform.SetParent(headerObj.transform, false);
            var inRt = inObj.AddComponent<RectTransform>();
            inRt.sizeDelta = new Vector2(160, 42);
            var inBg = inObj.AddComponent<Image>();
            inBg.color = new Color(0.18f, 0.22f, 0.32f, 0.9f);

            GameObject inTxtObj = new GameObject("Text");
            inTxtObj.transform.SetParent(inObj.transform, false);
            var inTxtRt = inTxtObj.AddComponent<RectTransform>();
            inTxtRt.anchorMin = Vector2.zero;
            inTxtRt.anchorMax = Vector2.one;
            inTxtRt.offsetMin = new Vector2(8, 2);
            inTxtRt.offsetMax = new Vector2(-8, -2);
            var inTxt = inTxtObj.AddComponent<Text>();
            inTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (inTxt.font == null) inTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            inTxt.fontSize = 14;
            inTxt.color = Color.white;
            inTxt.alignment = TextAnchor.MiddleLeft;

            addFriendInput = inObj.AddComponent<InputField>();
            addFriendInput.textComponent = inTxt;

            // Add Button
            GameObject addObj = CreateButton(headerObj.transform, "➕ Kết Bạn", new Color(0.25f, 0.65f, 0.35f), 100, 42);
            addFriendBtn = addObj.GetComponent<Button>();
            addFriendBtn.onClick.AddListener(OnAddFriendClicked);
        }

        private void CreateChatBottomControls(Transform parent)
        {
            GameObject botObj = new GameObject("ChatControls");
            botObj.transform.SetParent(parent, false);
            var rt = botObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, -48);
            rt.sizeDelta = new Vector2(0, 44);

            var hlg = botObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            // Input
            GameObject inObj = new GameObject("ChatInput");
            inObj.transform.SetParent(botObj.transform, false);
            var inRt = inObj.AddComponent<RectTransform>();
            inRt.sizeDelta = new Vector2(460, 42);
            var inBg = inObj.AddComponent<Image>();
            inBg.color = new Color(0.16f, 0.20f, 0.30f, 0.9f);

            GameObject inTxtObj = new GameObject("Text");
            inTxtObj.transform.SetParent(inObj.transform, false);
            var inTxtRt = inTxtObj.AddComponent<RectTransform>();
            inTxtRt.anchorMin = Vector2.zero;
            inTxtRt.anchorMax = Vector2.one;
            inTxtRt.offsetMin = new Vector2(10, 2);
            inTxtRt.offsetMax = new Vector2(-10, -2);
            var inTxt = inTxtObj.AddComponent<Text>();
            inTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (inTxt.font == null) inTxt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            inTxt.fontSize = 14;
            inTxt.color = Color.white;
            inTxt.alignment = TextAnchor.MiddleLeft;

            chatInputField = inObj.AddComponent<InputField>();
            chatInputField.textComponent = inTxt;

            // Send Button
            GameObject sendObj = CreateButton(botObj.transform, "Gửi 🚀", new Color(0.95f, 0.45f, 0.3f), 100, 42);
            sendChatBtn = sendObj.GetComponent<Button>();
            sendChatBtn.onClick.AddListener(OnSendChatClicked);
        }

        private void CreateMailboxTopControls(Transform parent)
        {
            GameObject topObj = new GameObject("MailboxTopControls");
            topObj.transform.SetParent(parent, false);
            topObj.transform.SetAsFirstSibling();
            var rt = topObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(580, 44);

            var hlg = topObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;

            GameObject claimAllObj = CreateButton(topObj.transform, "🎁 Nhận Tất Cả Quà", new Color(0.2f, 0.7f, 0.3f), 180, 40);
            claimAllBtn = claimAllObj.GetComponent<Button>();
            claimAllBtn.onClick.AddListener(() =>
            {
                GiftManager.Instance?.ClaimAllGifts();
                RefreshMailboxTab();
            });
        }

        private GameObject CreateButton(Transform parent, string label, Color color, float width, float height)
        {
            GameObject btnObj = new GameObject("Btn_" + label);
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);

            var img = btnObj.AddComponent<Image>();
            img.color = color;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = color * 1.15f;
            colors.pressedColor = color * 0.85f;
            btn.colors = colors;

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var txt = txtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            txt.fontSize = 14;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = label;

            return btnObj;
        }

        private void RegisterEvents()
        {
            if (FriendManager.Instance != null)
            {
                FriendManager.Instance.OnFriendsUpdated += RefreshFriendsTab;
            }
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.OnMessageReceived += (msg) =>
                {
                    if (currentTab == "chat") RefreshChatTab();
                };
            }
            if (GiftManager.Instance != null)
            {
                GiftManager.Instance.OnMailboxUpdated += RefreshMailboxTab;
            }
        }
        #endregion
    }
}
