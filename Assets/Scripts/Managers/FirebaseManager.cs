using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

[System.Serializable]
public class LeaderboardEntry
{
    public string userId;
    public string playerName;
    public int cafeLevel;
    public float totalMoney;
    public int catCount;
    public long timestamp;
}

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    [Header("Firebase Status")]
    public bool isInitialized = false;
    public bool isAuthenticated = false;
    public string currentUserId = "offline_user";
    public string playerName = "Sen Mèo Mẫu Mực";

    // References
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private DatabaseReference dbRef;

    // Events
    public event Action<bool> OnFirebaseStateChanged;
    public event Action<bool> OnCloudSyncStatusChanged;

    private const string PREF_PLAYER_NAME = "CatCafe_PlayerName";
    private const string PREF_CACHED_UID = "CatCafe_CachedUID";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Nạp tên người chơi đã lưu hoặc tên mặc định
        playerName = PlayerPrefs.GetString(PREF_PLAYER_NAME, "Chủ Quán Mèo " + UnityEngine.Random.Range(100, 999));
        currentUserId = PlayerPrefs.GetString(PREF_CACHED_UID, "cafe_guest_" + Guid.NewGuid().ToString().Substring(0, 8));

        InitializeFirebase();
    }

    public void InitializeFirebase()
    {
        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                    isInitialized = true;

                    Debug.Log("[FirebaseManager] Firebase SDK Khởi tạo thành công!");
                    AuthenticateUser();
                }
                else
                {
                    Debug.LogWarning($"[FirebaseManager] Không thể khởi tạo Firebase: {task.Result}. Chuyển sang chế độ Local/Offline.");
                    isInitialized = false;
                    OnFirebaseStateChanged?.Invoke(false);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[FirebaseManager] Lỗi khởi tạo Firebase (Có thể thiếu config mạng): {ex.Message}. Sử dụng lưu cục bộ an toàn.");
            isInitialized = false;
            OnFirebaseStateChanged?.Invoke(false);
        }
    }

    private void AuthenticateUser()
    {
        if (auth == null) return;

        currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            currentUserId = currentUser.UserId;
            PlayerPrefs.SetString(PREF_CACHED_UID, currentUserId);
            PlayerPrefs.Save();
            isAuthenticated = true;
            Debug.Log($"[FirebaseManager] Đã đăng nhập tự động với UID: {currentUserId}");
            OnFirebaseStateChanged?.Invoke(true);
        }
        else
        {
            // Tự động xác thực ẩn danh để người chơi có UID riêng biệt không trùng lặp
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCompleted && !authTask.IsFaulted)
                {
                    currentUser = authTask.Result.User;
                    currentUserId = currentUser.UserId;
                    PlayerPrefs.SetString(PREF_CACHED_UID, currentUserId);
                    PlayerPrefs.Save();
                    isAuthenticated = true;
                    Debug.Log($"[FirebaseManager] Xác thực ẩn danh thành công! UID: {currentUserId}");
                    OnFirebaseStateChanged?.Invoke(true);
                }
                else
                {
                    Debug.LogWarning("[FirebaseManager] Xác thực ẩn danh thất bại hoặc đang offline. Sử dụng UID cục bộ.");
                    isAuthenticated = false;
                    OnFirebaseStateChanged?.Invoke(false);
                }
            });
        }
    }

    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        playerName = newName.Trim();
        PlayerPrefs.SetString(PREF_PLAYER_NAME, playerName);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Đồng bộ dữ liệu lên Firebase Realtime Database
    /// </summary>
    public void SaveSaveDataToCloud(string json, Action<bool> callback = null)
    {
        if (!isInitialized || dbRef == null)
        {
            callback?.Invoke(false);
            return;
        }

        string uid = currentUserId;
        dbRef.Child("users").Child(uid).Child("saveData").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            bool success = task.IsCompleted && !task.IsFaulted;
            if (success)
            {
                Debug.Log("[FirebaseManager] Đã lưu thành công dữ liệu lên Cloud!");
                OnCloudSyncStatusChanged?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("[FirebaseManager] Lưu Cloud thất bại: " + task.Exception?.Message);
                OnCloudSyncStatusChanged?.Invoke(false);
            }
            callback?.Invoke(success);
        });
    }

    /// <summary>
    /// Tải dữ liệu lưu game từ Firebase Realtime Database
    /// </summary>
    public void LoadSaveDataFromCloud(Action<string> onComplete)
    {
        if (!isInitialized || dbRef == null)
        {
            onComplete?.Invoke(null);
            return;
        }

        string uid = currentUserId;
        dbRef.Child("users").Child(uid).Child("saveData").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                string json = task.Result.GetRawJsonValue();
                onComplete?.Invoke(json);
            }
            else
            {
                onComplete?.Invoke(null);
            }
        });
    }

    /// <summary>
    /// Cập nhật điểm số lên Bảng Xếp Hạng Toàn Cầu
    /// </summary>
    public void SubmitScoreToLeaderboard(int cafeLevel, float currentMoney, int catCount)
    {
        if (!isInitialized || dbRef == null) return;

        var entry = new LeaderboardEntry
        {
            userId = currentUserId,
            playerName = playerName,
            cafeLevel = cafeLevel,
            totalMoney = Mathf.Round(currentMoney),
            catCount = catCount,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        string json = JsonUtility.ToJson(entry);
        dbRef.Child("leaderboard").Child(currentUserId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("[FirebaseManager] Đã cập nhật Bảng Xếp Hạng thành công!");
            }
        });
    }

    /// <summary>
    /// Lấy danh sách Top người chơi từ Bảng Xếp Hạng Toàn Cầu
    /// </summary>
    public void FetchLeaderboard(int topLimit, Action<List<LeaderboardEntry>> onLoaded)
    {
        if (!isInitialized || dbRef == null)
        {
            onLoaded?.Invoke(new List<LeaderboardEntry>());
            return;
        }

        dbRef.Child("leaderboard").OrderByChild("cafeLevel").LimitToLast(topLimit).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            List<LeaderboardEntry> list = new List<LeaderboardEntry>();
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    try
                    {
                        string json = child.GetRawJsonValue();
                        LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(json);
                        if (entry != null)
                        {
                            list.Add(entry);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[FirebaseManager] Lỗi phân tích entry leaderboard: " + ex.Message);
                    }
                }
                // Sắp xếp giảm dần theo Cấp Quán, sau đó theo Tổng Tiền
                list.Sort((a, b) =>
                {
                    int comp = b.cafeLevel.CompareTo(a.cafeLevel);
                    return comp != 0 ? comp : b.totalMoney.CompareTo(a.totalMoney);
                });
            }
            onLoaded?.Invoke(list);
        });
    }
}
