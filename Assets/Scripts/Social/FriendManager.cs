using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CatCafe.Social;
using Firebase.Database;
using Firebase.Extensions;

namespace CatCafe.Social
{
    public class FriendManager : MonoBehaviour
    {
        public static FriendManager Instance { get; private set; }

        [Header("Player Identity")]
        public string myFriendCode = "CAT-8888";
        public bool isVisitingFriend = false;
        public string currentVisitingFriendUid = "";
        public string currentVisitingFriendName = "";

        [Header("Friends State")]
        public List<FriendProfile> friendsList = new List<FriendProfile>();
        public List<VisitorInteraction> recentVisitorLogs = new List<VisitorInteraction>();

        public event Action OnFriendsUpdated;
        public event Action<string> OnCafeVisitStarted;
        public event Action OnCafeVisitEnded;

        private const string PREF_FRIEND_CODE = "CatCafe_MyFriendCode";
        private const string PREF_FRIENDS_CACHE = "CatCafe_FriendsCache";

        // Backup dữ liệu quán của chính mình trước khi sang thăm quán bạn
        private string myCafeDataBackup = "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeFriendCode();
            LoadCachedFriends();
        }

        private void Start()
        {
            PublishMyProfile();
        }

        private void InitializeFriendCode()
        {
            if (PlayerPrefs.HasKey(PREF_FRIEND_CODE))
            {
                myFriendCode = PlayerPrefs.GetString(PREF_FRIEND_CODE);
            }
            else
            {
                int codeNum = UnityEngine.Random.Range(1000, 9999);
                myFriendCode = "CAT-" + codeNum;
                PlayerPrefs.SetString(PREF_FRIEND_CODE, myFriendCode);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Đồng bộ thông tin cá nhân lên Firebase để người khác có thể tìm kiếm
        /// </summary>
        public void PublishMyProfile()
        {
            string uid = FirebaseManager.Instance != null ? FirebaseManager.Instance.currentUserId : "guest_" + myFriendCode;
            string pName = FirebaseManager.Instance != null ? FirebaseManager.Instance.playerName : "Chủ Quán Mèo";

            int lvl = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : 1;
            int catCount = CatManager.Instance != null ? CatManager.Instance.spawnedCats.Count : 3;
            float money = MoneyManager.Instance != null ? MoneyManager.Instance.CurrentMoney : 100f;

            FriendProfile myProfile = new FriendProfile
            {
                userId = uid,
                friendCode = myFriendCode,
                playerName = pName,
                cafeLevel = lvl,
                catCount = catCount,
                totalMoney = money,
                isOnline = true,
                lastActiveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Nếu có Firebase Realtime Database
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized)
            {
                try
                {
                    var db = FirebaseDatabase.DefaultInstance.RootReference;
                    string json = JsonUtility.ToJson(myProfile);
                    db.Child("profiles").Child(uid).SetRawJsonValueAsync(json);
                    db.Child("friend_codes").Child(myFriendCode).SetValueAsync(uid);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[FriendManager] Publish profile error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Kết bạn bằng Mã Bạn Bè (Friend Code)
        /// </summary>
        public void AddFriendByCode(string targetCode, Action<bool, string> onComplete)
        {
            if (string.IsNullOrWhiteSpace(targetCode))
            {
                onComplete?.Invoke(false, "Vui lòng nhập Mã Bạn Bè!");
                return;
            }

            targetCode = targetCode.Trim().ToUpper();
            if (targetCode == myFriendCode)
            {
                onComplete?.Invoke(false, "Không thể tự kết bạn với chính mình!");
                return;
            }

            // Kiểm tra xem đã kết bạn chưa
            if (friendsList.Exists(f => f.friendCode == targetCode))
            {
                onComplete?.Invoke(false, "Bạn đã có người này trong danh sách bạn bè rồi!");
                return;
            }

            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized)
            {
                var db = FirebaseDatabase.DefaultInstance.RootReference;
                db.Child("friend_codes").Child(targetCode).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                    {
                        string targetUid = task.Result.Value.ToString();
                        // Lấy thông tin profile
                        db.Child("profiles").Child(targetUid).GetValueAsync().ContinueWithOnMainThread(profTask =>
                        {
                            if (profTask.IsCompleted && !profTask.IsFaulted && profTask.Result.Exists)
                            {
                                string json = profTask.Result.GetRawJsonValue();
                                FriendProfile newFriend = JsonUtility.FromJson<FriendProfile>(json);
                                if (newFriend != null)
                                {
                                    AddFriendToList(newFriend);
                                    onComplete?.Invoke(true, $"Đã kết bạn thành công với {newFriend.playerName}!");
                                    return;
                                }
                            }
                            onComplete?.Invoke(false, "Không thể tải thông tin bạn bè!");
                        });
                    }
                    else
                    {
                        onComplete?.Invoke(false, "Không tìm thấy Mã Bạn Bè này trên hệ thống!");
                    }
                });
            }
            else
            {
                // Giả lập kết bạn khi offline/chưa cấu hình Firebase
                FriendProfile mockFriend = new FriendProfile
                {
                    userId = "mock_" + targetCode,
                    friendCode = targetCode,
                    playerName = "Bạn Mới (" + targetCode + ")",
                    cafeLevel = UnityEngine.Random.Range(2, 6),
                    catCount = UnityEngine.Random.Range(3, 8),
                    totalMoney = UnityEngine.Random.Range(300, 1500),
                    isOnline = true,
                    lastActiveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                AddFriendToList(mockFriend);
                onComplete?.Invoke(true, $"Đã kết bạn thành công với {mockFriend.playerName}!");
            }
        }

        private void AddFriendToList(FriendProfile friend)
        {
            if (!friendsList.Exists(f => f.userId == friend.userId))
            {
                friendsList.Add(friend);
                SaveFriendsCache();
                OnFriendsUpdated?.Invoke();

                // Ghi nhận nhiệm vụ bạn bè
                DailyQuestManager.Instance?.AddQuestProgress("quest_friend", 1);
            }
        }

        /// <summary>
        /// Ghé thăm quán Cafe của Bạn Bè
        /// </summary>
        public void VisitFriendCafe(FriendProfile friend)
        {
            if (friend == null) return;
            if (isVisitingFriend)
            {
                ToastManager.Instance?.ShowToast("Bạn đang ở quán của một người bạn khác!", "⚠️");
                return;
            }

            isVisitingFriend = true;
            currentVisitingFriendUid = friend.userId;
            currentVisitingFriendName = friend.playerName;

            // 1. Sao lưu dữ liệu quán hiện tại của mình
            if (SaveManager.Instance != null)
            {
                var curSave = SaveManager.Instance.GetCurrentSaveData();
                myCafeDataBackup = JsonUtility.ToJson(curSave);
            }

            ToastManager.Instance?.ShowToast($"Đang dịch chuyển sang quán của {friend.playerName}...", "🚀", new Color(0.3f, 0.7f, 1f));

            // 2. Tải dữ liệu quán của bạn (nếu có Firebase) hoặc áp dụng quán mẫu của bạn bè
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized)
            {
                var db = FirebaseDatabase.DefaultInstance.RootReference;
                db.Child("users").Child(friend.userId).Child("saveData").GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                    {
                        string friendSaveJson = task.Result.GetRawJsonValue();
                        ApplyFriendCafeData(friendSaveJson);
                    }
                    else
                    {
                        // Dùng dữ liệu mô phỏng dựa trên level của bạn
                        ApplyMockFriendCafe(friend);
                    }
                });
            }
            else
            {
                ApplyMockFriendCafe(friend);
            }

            OnCafeVisitStarted?.Invoke(friend.playerName);

            // Ghi nhận nhiệm vụ "Ghé thăm quán bạn bè"
            DailyQuestManager.Instance?.AddQuestProgress("quest_visit_friend", 1);
        }

        private void ApplyMockFriendCafe(FriendProfile friend)
        {
            // Tạm thời điều chỉnh tiền và level hiển thị sang thông số của bạn
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.currentLevel = friend.cafeLevel;
            }
            ToastManager.Instance?.ShowToast($"Chào mừng bạn đến với Quán Cafe của {friend.playerName}! ☕🐾", "🐱", new Color(1f, 0.6f, 0.8f));
        }

        private void ApplyFriendCafeData(string json)
        {
            try
            {
                if (SaveManager.Instance != null && !string.IsNullOrEmpty(json))
                {
                    SaveData friendData = JsonUtility.FromJson<SaveData>(json);
                    SaveManager.Instance.ApplyLoadedData(friendData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[FriendManager] Lỗi nạp quán bạn: " + ex.Message);
            }
            ToastManager.Instance?.ShowToast($"Đã đến quán của {currentVisitingFriendName}!", "✨");
        }

        /// <summary>
        /// Trở về quán Cafe của chính mình
        /// </summary>
        public void ReturnToOwnCafe()
        {
            if (!isVisitingFriend) return;

            isVisitingFriend = false;
            currentVisitingFriendUid = "";
            currentVisitingFriendName = "";

            if (!string.IsNullOrEmpty(myCafeDataBackup) && SaveManager.Instance != null)
            {
                try
                {
                    SaveData myData = JsonUtility.FromJson<SaveData>(myCafeDataBackup);
                    SaveManager.Instance.ApplyLoadedData(myData);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[FriendManager] Lỗi khôi phục quán: " + ex.Message);
                }
            }

            ToastManager.Instance?.ShowToast("Đã quay trở lại Quán Cafe của bạn! 🏠", "🏡", new Color(0.4f, 0.8f, 0.4f));
            OnCafeVisitEnded?.Invoke();
        }

        /// <summary>
        /// Ghi lại khi khách ghé thăm vuốt ve hoặc cho mèo ăn
        /// </summary>
        public void RecordVisitorInteraction(string catName, string actionType)
        {
            var log = new VisitorInteraction
            {
                visitorId = myFriendCode,
                visitorName = FirebaseManager.Instance != null ? FirebaseManager.Instance.playerName : "Bạn Tốt",
                targetCatName = catName,
                actionType = actionType,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            recentVisitorLogs.Add(log);

            // Tặng Xu Tình Bạn / Tim hữu nghị
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGems(1);
            }

            DailyQuestManager.Instance?.AddQuestProgress("quest_coop_pet", 1);
            ToastManager.Instance?.ShowToast($"Đã { (actionType == "pet" ? "vuốt ve" : "cho ăn") } bé {catName} giúp bạn! +1 💎", "💖");
        }

        private void SaveFriendsCache()
        {
            try
            {
                string json = JsonHelper.ToJson(friendsList.ToArray());
                PlayerPrefs.SetString(PREF_FRIENDS_CACHE, json);
                PlayerPrefs.Save();
            }
            catch {}
        }

        private void LoadCachedFriends()
        {
            if (PlayerPrefs.HasKey(PREF_FRIENDS_CACHE))
            {
                try
                {
                    string json = PlayerPrefs.GetString(PREF_FRIENDS_CACHE);
                    FriendProfile[] arr = JsonHelper.FromJson<FriendProfile>(json);
                    if (arr != null)
                    {
                        friendsList = new List<FriendProfile>(arr);
                    }
                }
                catch {}
            }

            // Nếu danh sách trống, tặng sẵn 3 người bạn mẫu để trải nghiệm ngay
            if (friendsList.Count == 0)
            {
                friendsList.Add(new FriendProfile { userId = "bot_1", friendCode = "CAT-1024", playerName = "Sen Mèo Mẫu Mực", cafeLevel = 4, catCount = 6, isOnline = true });
                friendsList.Add(new FriendProfile { userId = "bot_2", friendCode = "CAT-2048", playerName = "Tiệm Trà & Mèo Miu", cafeLevel = 3, catCount = 4, isOnline = true });
                friendsList.Add(new FriendProfile { userId = "bot_3", friendCode = "CAT-4096", playerName = "Chủ Quán Bơ Sữa", cafeLevel = 5, catCount = 8, isOnline = false });
            }
        }
    }

    // Helper serialize array JSON
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper != null ? wrapper.Items : null;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T> { Items = array };
            return JsonUtility.ToJson(wrapper);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
