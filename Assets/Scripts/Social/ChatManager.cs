using System;
using System.Collections.Generic;
using UnityEngine;
using CatCafe.Social;
using Firebase.Database;
using Firebase.Extensions;

namespace CatCafe.Social
{
    public class ChatManager : MonoBehaviour
    {
        public static ChatManager Instance { get; private set; }

        [Header("Chat State")]
        public List<ChatMessage> messages = new List<ChatMessage>();
        public int maxHistory = 60;

        public event Action<ChatMessage> OnMessageReceived;

        private DatabaseReference chatDbRef;
        private bool isListening = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetupFirebaseChatListener();

            // Tin nhắn chào mừng mặc định
            AddLocalMessage("Hệ Thống", "Chào mừng bạn đến với Kênh Trò Chuyện Hội Quán Mèo! Hãy chào hỏi và giao lưu cùng các chủ quán khác nhé! 🐱☕", "system");
        }

        private void SetupFirebaseChatListener()
        {
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized && !isListening)
            {
                try
                {
                    chatDbRef = FirebaseDatabase.DefaultInstance.RootReference.Child("chat").Child("global");
                    chatDbRef.LimitToLast(30).ChildAdded += HandleFirebaseChildAdded;
                    isListening = true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[ChatManager] Không thể gắn listener chat Firebase: " + ex.Message);
                }
            }
        }

        private void HandleFirebaseChildAdded(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null) return;

            try
            {
                string json = args.Snapshot.GetRawJsonValue();
                ChatMessage msg = JsonUtility.FromJson<ChatMessage>(json);
                if (msg != null && !messages.Exists(m => m.messageId == msg.messageId))
                {
                    messages.Add(msg);
                    if (messages.Count > maxHistory) messages.RemoveAt(0);
                    OnMessageReceived?.Invoke(msg);

                    // Hiển thị bong bóng thoại nếu có người chơi xung quanh
                    ShowSpeechBubbleForSender(msg.senderName, msg.content);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ChatManager] Parse chat error: " + ex.Message);
            }
        }

        public void SendChatMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            text = text.Trim();

            string senderUid = FirebaseManager.Instance != null ? FirebaseManager.Instance.currentUserId : "local_user";
            string senderName = FirebaseManager.Instance != null ? FirebaseManager.Instance.playerName : "Tôi";

            ChatMessage newMsg = new ChatMessage
            {
                messageId = Guid.NewGuid().ToString().Substring(0, 10),
                senderId = senderUid,
                senderName = senderName,
                content = text,
                channel = "global",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Nếu có kết nối Firebase
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized && chatDbRef != null)
            {
                string json = JsonUtility.ToJson(newMsg);
                chatDbRef.Child(newMsg.messageId).SetRawJsonValueAsync(json);
            }
            else
            {
                // Xử lý cục bộ
                messages.Add(newMsg);
                if (messages.Count > maxHistory) messages.RemoveAt(0);
                OnMessageReceived?.Invoke(newMsg);
                ShowSpeechBubbleForSender(senderName, text);

                // Giả lập bot phản hồi thân thiện sau 2 giây
                Invoke(nameof(SimulateFriendlyReply), 2.5f);
            }

            // Ghi nhận nhiệm vụ tương tác
            DailyQuestManager.Instance?.AddQuestProgress("quest_chat", 1);
        }

        public void AddLocalMessage(string sender, string content, string type = "local")
        {
            ChatMessage localMsg = new ChatMessage
            {
                messageId = Guid.NewGuid().ToString().Substring(0, 8),
                senderId = type,
                senderName = sender,
                content = content,
                channel = "global",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            messages.Add(localMsg);
            if (messages.Count > maxHistory) messages.RemoveAt(0);
            OnMessageReceived?.Invoke(localMsg);
        }

        private void SimulateFriendlyReply()
        {
            string[] friendlyBots = { "Sen Mèo Mẫu Mực", "Tiệm Trà Miu Miu", "Chủ Quán Bơ Sữa", "Bảo An Barista" };
            string[] replies = {
                "Quán bạn trang trí xinh quá! Cho mình xin bí quyết nuôi nhiều mèo nhé! 💕",
                "Chào chủ quán! Nay khách đông không bạn ơi? ☕",
                "Bé mèo Anh Lông Ngắn đáng yêu xỉu luôn á! 🥰",
                "Vừa sang thăm quán bạn nè, mình đã vuốt ve hộ bạn một bé rồi nhé! 🐱✨"
            };

            string botName = friendlyBots[UnityEngine.Random.Range(0, friendlyBots.Length)];
            string reply = replies[UnityEngine.Random.Range(0, replies.Length)];

            AddLocalMessage(botName, reply);
        }

        private void ShowSpeechBubbleForSender(string sender, string text)
        {
            // Tìm Player trong scene để hiện bong bóng thoại
            var player = FindFirstObjectByType<PlayerMove>();
            if (player != null)
            {
                ChatBubble.ShowBubble(player.transform, text);
            }
        }

        private void OnDestroy()
        {
            if (chatDbRef != null && isListening)
            {
                chatDbRef.ChildAdded -= HandleFirebaseChildAdded;
            }
        }
    }
}
