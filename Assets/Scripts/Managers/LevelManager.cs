using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Current Level")]
    public LevelData currentLevel;
    public List<LevelData> allLevels = new List<LevelData>();

    [Header("Shift Progress")]
    public bool isShiftRunning = false;
    public float currentShiftTimer = 0f;
    public float shiftStartingRevenue = 0f;
    public float currentShiftRevenue = 0f;
    public bool isRushHour = false;

    [Header("Player Level / EXP")]
    public int cafeLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    public System.Action<float, float> OnShiftTimeUpdated; // (current, total)
    public System.Action<bool> OnRushHourToggled;
    public System.Action<int, float> OnShiftCompleted; // (stars, revenueEarned)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (currentLevel != null)
        {
            StartShift(currentLevel);
        }
    }

    private void Update()
    {
        if (!isShiftRunning || currentLevel == null) return;

        currentShiftTimer += Time.deltaTime;
        OnShiftTimeUpdated?.Invoke(currentShiftTimer, currentLevel.shiftDurationSeconds);

        // Tính doanh thu trong ca
        if (MoneyManager.Instance != null)
        {
            currentShiftRevenue = Mathf.Max(0, MoneyManager.Instance.CurrentMoney - shiftStartingRevenue);
        }

        // Kiểm tra Giờ cao điểm (Rush Hour)
        bool rushHourActive = currentShiftTimer >= currentLevel.rushHourStartTime &&
                              currentShiftTimer <= (currentLevel.rushHourStartTime + currentLevel.rushHourDuration);

        if (rushHourActive != isRushHour)
        {
            isRushHour = rushHourActive;
            OnRushHourToggled?.Invoke(isRushHour);

            if (isRushHour && UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("🔥 GIỜ CAO ĐIỂM! KHÁCH TỚI ĐÔNG!", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.red);
            }
        }

        // Kết thúc ca làm việc
        if (currentShiftTimer >= currentLevel.shiftDurationSeconds)
        {
            EndShift();
        }
    }

    public void StartShift(LevelData level)
    {
        currentLevel = level;
        currentShiftTimer = 0f;
        isRushHour = false;
        isShiftRunning = true;

        if (MoneyManager.Instance != null)
        {
            shiftStartingRevenue = MoneyManager.Instance.CurrentMoney;
        }

        currentShiftRevenue = 0f;
        OnRushHourToggled?.Invoke(false);
    }

    public void EndShift()
    {
        isShiftRunning = false;
        int stars = CalculateStars(currentShiftRevenue);

        // Thưởng EXP và quà hoàn thành màn
        AddExp(stars * 40);

        if (stars >= 1 && currentLevel != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGems(currentLevel.completionGems);
            }

            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(currentLevel.completionBonusMoney);
            }
        }

        OnShiftCompleted?.Invoke(stars, currentShiftRevenue);

        SaveManager.Instance?.SaveGame();
    }

    public int CalculateStars(float revenue)
    {
        if (currentLevel == null) return 1;

        if (revenue >= currentLevel.targetRevenue3Star) return 3;
        if (revenue >= currentLevel.targetRevenue2Star) return 2;
        if (revenue >= currentLevel.targetRevenue1Star) return 1;
        return 0;
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            cafeLevel++;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.35f);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText($"🎉 LÊN CẤP QUÁN: LV.{cafeLevel}!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.cyan);
            }
        }
    }
}
