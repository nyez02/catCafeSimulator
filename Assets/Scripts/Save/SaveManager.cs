using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

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
    public long lastSaveTimestamp;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Save Settings")]
    public float autoSaveInterval = 45f;
    private float autoSaveTimer;

    private string saveFilePath;
    private DatabaseReference dbReference;
    private string userId = "cafe_owner_default";

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
        InitializeFirebase();
    }

    private void Start()
    {
        LoadGame();
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

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("[SaveManager] Firebase Realtime Database Initialized!");
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Firebase dependencies not available: {task.Result}");
            }
        });
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

        data.lastSaveTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string json = JsonUtility.ToJson(data, true);

        // Lưu local
        try
        {
            File.WriteAllText(saveFilePath, json);
            Debug.Log("[SaveManager] Game saved locally to: " + saveFilePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[SaveManager] Failed to save locally: " + ex.Message);
        }

        // Lưu Cloud Firebase
        if (dbReference != null)
        {
            dbReference.Child("users").Child(userId).Child("saveData").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("[SaveManager] Game saved to Firebase Cloud!");
                }
            });
        }
    }

    public void LoadGame()
    {
        LoadGameLocally();

        if (dbReference != null)
        {
            dbReference.Child("users").Child(userId).Child("saveData").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    string json = task.Result.GetRawJsonValue();
                    GameSaveData cloudData = JsonUtility.FromJson<GameSaveData>(json);
                    RestoreDataToManagers(cloudData);
                    Debug.Log("[SaveManager] Loaded & Synced from Firebase!");
                }
            });
        }
    }

    private void LoadGameLocally()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                RestoreDataToManagers(data);
                Debug.Log("[SaveManager] Game loaded from local save.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[SaveManager] Error loading local save: " + ex.Message);
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
            float offlineRevenue = Mathf.Round(cappedSeconds * earnedRatePerSecond);

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
