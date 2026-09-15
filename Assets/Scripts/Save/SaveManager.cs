using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public float currentMoney = 50f;
    public int pawGems = 20;
    public int unlockedTableCount = 2;
    public List<string> ownedCatNames = new List<string>();
    public float luckyPiggyBankBalance = 0f;
    public float luckyPiggyBankCapacity = 150f;
    public int cafeLevel = 1;
    public int currentExp = 0;
    public int dailyStreak = 0;
    public string lastClaimDateStr = "";
    public bool tutorialCompleted = false;
    public List<string> unlockedDecorations = new List<string>();
    public List<string> hiredStaffIds = new List<string>();
    public long lastSaveTimestamp;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Save Settings")]
    public float autoSaveInterval = 45f;
    private float autoSaveTimer;

    private string saveFilePath;
    private GameSaveData currentLocalData;

    public GameSaveData GetCurrentSaveData() => currentLocalData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    private void Start()
    {
        LoadGame();

        // Lắng nghe khi Firebase sẵn sàng để kiểm tra đồng bộ hai chiều
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnFirebaseStateChanged += OnFirebaseReady;
        }
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnFirebaseStateChanged -= OnFirebaseReady;
        }
    }

    private void Update()
    {
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnFirebaseReady(bool isReady)
    {
        if (isReady)
        {
            SyncWithCloud();
        }
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        // 1. Tiền & Kim Cương
        if (MoneyManager.Instance != null)
        {
            data.currentMoney = MoneyManager.Instance.CurrentMoney;
        }
        if (GameManager.Instance != null)
        {
            data.pawGems = GameManager.Instance.pawGems;
        }

        // 2. Bàn ghế
        if (TableManager.Instance != null)
        {
            data.unlockedTableCount = TableManager.Instance.GetUnlockedTableCount();
        }

        // 3. Mèo sở hữu
        if (CatManager.Instance != null)
        {
            data.ownedCatNames = CatManager.Instance.GetOwnedCatNames();
        }

        // 4. Hũ Hụi Mèo Tài Lộc
        if (LuckyPiggyBank.Instance != null)
        {
            data.luckyPiggyBankBalance = LuckyPiggyBank.Instance.currentBalance;
            data.luckyPiggyBankCapacity = LuckyPiggyBank.Instance.maxCapacity;
        }

        // 5. Cấp độ quán & EXP
        if (LevelManager.Instance != null)
        {
            data.cafeLevel = LevelManager.Instance.cafeLevel;
            data.currentExp = LevelManager.Instance.currentExp;
        }

        // 6. Điểm danh
        if (DailyRewardManager.Instance != null)
        {
            data.dailyStreak = DailyRewardManager.Instance.currentStreak;
            data.lastClaimDateStr = DailyRewardManager.Instance.lastClaimDateStr;
        }

        // 7. Hướng dẫn tân thủ
        if (TutorialManager.Instance != null)
        {
            data.tutorialCompleted = (TutorialManager.Instance.currentStep == TutorialStep.Completed) || !TutorialManager.Instance.isTutorialActive;
        }

        // 8. Nội thất trang trí
        if (DecorationManager.Instance != null)
        {
            data.unlockedDecorations = DecorationManager.Instance.unlockedDecorationIds;
        }

        // 9. Nhân viên mèo đã thuê
        if (StaffManager.Instance != null)
        {
            data.hiredStaffIds = StaffManager.Instance.hiredStaffIds;
        }

        data.lastSaveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        currentLocalData = data;

        string json = JsonUtility.ToJson(data, true);

        // Lưu local
        try
        {
            File.WriteAllText(saveFilePath, json);
            Debug.Log("[SaveManager] Đã lưu game nội bộ vào: " + saveFilePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[SaveManager] Lỗi lưu game nội bộ: " + ex.Message);
        }

        // Đồng bộ Cloud Firebase nếu sẵn sàng
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.isInitialized)
        {
            FirebaseManager.Instance.SaveSaveDataToCloud(json);

            // Cập nhật Bảng Xếp Hạng Toàn Cầu
            int catCount = (data.ownedCatNames != null) ? data.ownedCatNames.Count : 0;
            FirebaseManager.Instance.SubmitScoreToLeaderboard(data.cafeLevel, data.currentMoney, catCount);
        }
    }

    public void LoadGame()
    {
        // 1. Tải bản lưu local trước để người chơi vào game ngay lập tức (Zero Latency)
        LoadGameLocally();

        // 2. Thử đồng bộ với Cloud
        SyncWithCloud();
    }

    private void SyncWithCloud()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.isInitialized) return;

        FirebaseManager.Instance.LoadSaveDataFromCloud(cloudJson =>
        {
            if (string.IsNullOrEmpty(cloudJson))
            {
                // Chưa có dữ liệu trên Cloud -> Tải bản lưu local hiện tại lên Cloud
                if (currentLocalData != null)
                {
                    string localJson = JsonUtility.ToJson(currentLocalData, true);
                    FirebaseManager.Instance.SaveSaveDataToCloud(localJson);
                }
                return;
            }

            try
            {
                GameSaveData cloudData = JsonUtility.FromJson<GameSaveData>(cloudJson);
                if (cloudData == null) return;

                long localTimestamp = currentLocalData != null ? currentLocalData.lastSaveTimestamp : 0;
                long cloudTimestamp = cloudData.lastSaveTimestamp;

                // Phân giải xung đột theo dấu thời gian (Conflict Resolution)
                if (cloudTimestamp > localTimestamp)
                {
                    Debug.Log("[SaveManager] Dữ liệu Cloud mới hơn Local -> Cập nhật dữ liệu từ Cloud!");
                    File.WriteAllText(saveFilePath, cloudJson);
                    currentLocalData = cloudData;
                    RestoreDataToManagers(cloudData);
                }
                else if (localTimestamp > cloudTimestamp)
                {
                    Debug.Log("[SaveManager] Dữ liệu Local mới hơn Cloud (Chơi offline trước đó) -> Đẩy Local lên Cloud!");
                    string localJson = JsonUtility.ToJson(currentLocalData, true);
                    FirebaseManager.Instance.SaveSaveDataToCloud(localJson);
                }
                else
                {
                    Debug.Log("[SaveManager] Dữ liệu Local và Cloud đã đồng bộ.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[SaveManager] Lỗi xử lý dữ liệu từ Cloud: " + ex.Message);
            }
        });
    }

    private void LoadGameLocally()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                currentLocalData = data;
                RestoreDataToManagers(data);
                Debug.Log("[SaveManager] Đã tải game từ bộ nhớ thiết bị.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[SaveManager] Lỗi đọc file save local: " + ex.Message);
            }
        }
        else
        {
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.LoadMoney(50f);
            }

            // Người chơi mới tinh: Bắt đầu hướng dẫn tân thủ
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StartTutorial();
            }
        }
    }

    private void RestoreDataToManagers(GameSaveData data)
    {
        if (data == null) return;

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.LoadMoney(data.currentMoney);
        }

        if (GameManager.Instance != null && data.pawGems > 0)
        {
            GameManager.Instance.pawGems = data.pawGems;
            GameManager.Instance.AddGems(0);
        }

        if (TableManager.Instance != null && data.unlockedTableCount > 0)
        {
            TableManager.Instance.SetUnlockedTables(data.unlockedTableCount);
        }

        // Khôi phục tất cả các chú mèo đã mua từ trước
        if (CatManager.Instance != null && data.ownedCatNames != null && data.ownedCatNames.Count > 0)
        {
            CatManager.Instance.RestoreOwnedCats(data.ownedCatNames);
        }

        if (LuckyPiggyBank.Instance != null)
        {
            LuckyPiggyBank.Instance.LoadHuiData(data.luckyPiggyBankBalance, data.luckyPiggyBankCapacity);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.cafeLevel = Mathf.Max(1, data.cafeLevel);
            LevelManager.Instance.currentExp = data.currentExp;
        }

        if (DailyRewardManager.Instance != null)
        {
            DailyRewardManager.Instance.currentStreak = data.dailyStreak;
            DailyRewardManager.Instance.lastClaimDateStr = data.lastClaimDateStr;
        }

        if (TutorialManager.Instance != null)
        {
            if (!data.tutorialCompleted)
            {
                TutorialManager.Instance.StartTutorial();
            }
            else
            {
                TutorialManager.Instance.currentStep = TutorialStep.Completed;
            }
        }

        // Khôi phục các nội thất đã mở khóa
        if (DecorationManager.Instance != null && data.unlockedDecorations != null)
        {
            DecorationManager.Instance.unlockedDecorationIds = data.unlockedDecorations;
        }

        // Khôi phục nhân viên mèo
        if (StaffManager.Instance != null && data.hiredStaffIds != null)
        {
            StaffManager.Instance.hiredStaffIds = data.hiredStaffIds;
        }

        // Tính toán thu nhập nhàn rỗi khi vắng nhà (Offline Idle Earnings)
        CheckOfflineEarnings(data);
    }

    private void CheckOfflineEarnings(GameSaveData data)
    {
        if (data.lastSaveTimestamp <= 0) return;

        long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsedSeconds = currentTimestamp - data.lastSaveTimestamp;

        // Chỉ tính thu nhập nếu đã offline ít nhất 60 giây
        if (elapsedSeconds >= 60)
        {
            // Tối đa 8 tiếng tích lũy (28800 giây)
            long cappedSeconds = System.Math.Min(elapsedSeconds, 8 * 3600);

            // Tốc độ: Mỗi chú mèo kiếm được khoảng $0.05 / giây
            int catCount = (data.ownedCatNames != null && data.ownedCatNames.Count > 0) ? data.ownedCatNames.Count : 2;
            float earnedRatePerSecond = catCount * 0.05f;

            // Áp dụng bùa lợi tăng thu nhập offline từ nội thất decor
            float offlineMultiplier = DecorationManager.Instance != null ? DecorationManager.Instance.GetOfflineBonusMultiplier() : 1f;
            float offlineRevenue = Mathf.Round(cappedSeconds * earnedRatePerSecond * offlineMultiplier);

            if (offlineRevenue >= 5f && UIManager.Instance != null)
            {
                int hours = (int)(cappedSeconds / 3600);
                int minutes = (int)((cappedSeconds % 3600) / 60);
                string timeStr = hours > 0 ? $"{hours} giờ {minutes} phút" : $"{minutes} phút";

                UIManager.Instance.ShowOfflineEarningsPopup(offlineRevenue, timeStr);
            }
        }
    }
}
