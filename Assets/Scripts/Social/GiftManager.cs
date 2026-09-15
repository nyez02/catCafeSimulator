using System;
using System.Collections.Generic;
using UnityEngine;
using CatCafe.Social;
using Firebase.Database;
using Firebase.Extensions;

namespace CatCafe.Social
{
    public class GiftManager : MonoBehaviour
    {
        public static GiftManager Instance { get; private set; }

        [Header("Mailbox State")]
        public List<GiftItem> inboxGifts = new List<GiftItem>();

        public event Action OnMailboxUpdated;

        private const string PREF_GIFTS_CACHE = "CatCafe_InboxGifts";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadCachedGifts();
        }

        private void Start()
        {
            FetchCloudGifts();
        }

        public void SendGift(FriendProfile friend, string giftType, int amount, string message, Action<bool> onComplete)
        {
            if (friend == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            string myUid = FirebaseManager.Instance != null ? FirebaseManager.Instance.currentUserId : "my_uid";
            string myName = FirebaseManager.Instance != null ? FirebaseManager.Instance.playerName : "Bạn Tốt";

            GiftItem gift = new GiftItem
            {
                giftId = Guid.NewGuid().ToString().Substring(0, 8),
                senderId = myUid,
                senderName = myName,
                giftType = giftType,
                amount = amount,
                message = string.IsNullOrWhiteSpace(message) ? "Tặng bạn món quà ấm áp! Chúc quán bạn luôn đông khách nhé! ☕🐾" : message,
                isClaimed = false,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized)
            {
                try
                {
                    var db = FirebaseDatabase.DefaultInstance.RootReference;
                    string json = JsonUtility.ToJson(gift);
                    db.Child("gifts").Child(friend.userId).Child(gift.giftId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                    {
                        bool ok = task.IsCompleted && !task.IsFaulted;
                        onComplete?.Invoke(ok);
                    });
                }
                catch
                {
                    onComplete?.Invoke(true);
                }
            }
            else
            {
                onComplete?.Invoke(true);
            }

            ToastManager.Instance?.ShowToast($"Đã gửi tặng hộp quà đến {friend.playerName}! 🎁", "✨", new Color(0.9f, 0.5f, 0.9f));
            DailyQuestManager.Instance?.AddQuestProgress("quest_gift", 1);
        }

        public void FetchCloudGifts()
        {
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized)
            {
                string myUid = FirebaseManager.Instance.currentUserId;
                var db = FirebaseDatabase.DefaultInstance.RootReference;
                db.Child("gifts").Child(myUid).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                    {
                        foreach (var child in task.Result.Children)
                        {
                            try
                            {
                                string json = child.GetRawJsonValue();
                                GiftItem item = JsonUtility.FromJson<GiftItem>(json);
                                if (item != null && !inboxGifts.Exists(g => g.giftId == item.giftId))
                                {
                                    inboxGifts.Add(item);
                                }
                            }
                            catch {}
                        }
                        SaveGiftsCache();
                        OnMailboxUpdated?.Invoke();
                    }
                });
            }
        }

        public bool ClaimGift(GiftItem gift)
        {
            if (gift == null || gift.isClaimed) return false;

            gift.isClaimed = true;

            // Trao thưởng
            switch (gift.giftType)
            {
                case "gems":
                    GameManager.Instance?.AddGems(gift.amount);
                    break;
                case "money":
                    MoneyManager.Instance?.AddMoney(gift.amount);
                    break;
                default:
                    MoneyManager.Instance?.AddMoney(gift.amount * 20f);
                    GameManager.Instance?.AddGems(2);
                    break;
            }

            SoundManager.Instance?.PlayCoin();
            ToastManager.Instance?.ShowToast($"Đã nhận quà từ {gift.senderName}: +{gift.amount} {gift.giftType}! 🎁", "💖");

            SaveGiftsCache();
            OnMailboxUpdated?.Invoke();
            return true;
        }

        public void ClaimAllGifts()
        {
            int claimedCount = 0;
            foreach (var gift in inboxGifts)
            {
                if (!gift.isClaimed)
                {
                    ClaimGift(gift);
                    claimedCount++;
                }
            }

            if (claimedCount > 0)
            {
                ToastManager.Instance?.ShowToast($"Đã nhận tất cả {claimedCount} hộp quà từ bạn bè!", "🎉", Color.green);
            }
        }

        private void SaveGiftsCache()
        {
            try
            {
                string json = JsonHelper.ToJson(inboxGifts.ToArray());
                PlayerPrefs.SetString(PREF_GIFTS_CACHE, json);
                PlayerPrefs.Save();
            }
            catch {}
        }

        private void LoadCachedGifts()
        {
            if (PlayerPrefs.HasKey(PREF_GIFTS_CACHE))
            {
                try
                {
                    string json = PlayerPrefs.GetString(PREF_GIFTS_CACHE);
                    GiftItem[] arr = JsonHelper.FromJson<GiftItem>(json);
                    if (arr != null)
                    {
                        inboxGifts = new List<GiftItem>(arr);
                    }
                }
                catch {}
            }

            // Quà chào mừng tân thủ nếu rỗng
            if (inboxGifts.Count == 0)
            {
                inboxGifts.Add(new GiftItem
                {
                    giftId = "gift_welcome_1",
                    senderId = "system",
                    senderName = "Hội Trưởng Cafe Mèo",
                    giftType = "gems",
                    amount = 10,
                    message = "Chào mừng bạn đến với cộng đồng Cat Cafe Simulator! Tặng bạn 10 Kim Cương may mắn.",
                    isClaimed = false,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
                inboxGifts.Add(new GiftItem
                {
                    giftId = "gift_welcome_2",
                    senderId = "friend_bot",
                    senderName = "Sen Mèo Mẫu Mực",
                    giftType = "money",
                    amount = 200,
                    message = "Chúc quán mới khai trương hồng phát! Gửi bạn $200 mua thêm hạt cafe thơm ngon nhé!",
                    isClaimed = false,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            }
        }
    }
}
