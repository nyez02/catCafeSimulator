using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public int targetProgress;
    public int currentProgress;
    public int rewardGems;
    public bool isClaimed;

    public bool IsCompleted => currentProgress >= targetProgress;
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Achievements List")]
    public List<Achievement> achievements = new List<Achievement>();

    public event Action<Achievement> OnAchievementCompleted;
    public event Action OnAchievementsUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeDefaultAchievements();
    }

    private void InitializeDefaultAchievements()
    {
        if (achievements.Count == 0)
        {
            achievements.Add(new Achievement { id = "pet_10", title = "Tín Đồ Mèo", description = "Vuốt ve các chú mèo 10 lần", targetProgress = 10, rewardGems = 5 });
            achievements.Add(new Achievement { id = "pet_50", title = "Bàn Tay Thần Kỳ", description = "Vuốt ve mèo 50 lần", targetProgress = 50, rewardGems = 15 });
            achievements.Add(new Achievement { id = "serve_10", title = "Phục Vụ Siêu Cấp", description = "Phục vụ thành công 10 khách hàng", targetProgress = 10, rewardGems = 10 });
            achievements.Add(new Achievement { id = "serve_50", title = "Chủ Quán Bận Rộn", description = "Phục vụ 50 lượt khách hàng", targetProgress = 50, rewardGems = 25 });
            achievements.Add(new Achievement { id = "tables_4", title = "Mở Rộng Không Gian", description = "Mở khóa ít nhất 4 bàn cafe", targetProgress = 4, rewardGems = 20 });
            achievements.Add(new Achievement { id = "break_hui_1", title = "Đập Hũ Đầu Tiên", description = "Đập Hũ Hụi Mèo Tài Lộc lần đầu tiên", targetProgress = 1, rewardGems = 15 });
        }
    }

    public void AddProgress(string achievementId, int amount = 1)
    {
        Achievement ach = achievements.Find(a => a.id == achievementId);
        if (ach != null && !ach.isClaimed)
        {
            bool wasCompleted = ach.IsCompleted;
            ach.currentProgress += amount;
            OnAchievementsUpdated?.Invoke();

            if (!wasCompleted && ach.IsCompleted)
            {
                OnAchievementCompleted?.Invoke(ach);
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingText($"🏆 Thành Tựu: {ach.title}!", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.yellow);
                }
            }
        }
    }

    public bool ClaimReward(string achievementId)
    {
        Achievement ach = achievements.Find(a => a.id == achievementId);
        if (ach != null && ach.IsCompleted && !ach.isClaimed)
        {
            ach.isClaimed = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGems(ach.rewardGems);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCoin();
            }

            OnAchievementsUpdated?.Invoke();
            SaveManager.Instance?.SaveGame();
            return true;
        }
        return false;
    }
}
