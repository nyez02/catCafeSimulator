using System;
using UnityEngine;

[System.Serializable]
public class DailyRewardItem
{
    public int day;
    public string rewardName;
    public float rewardMoney;
    public int rewardGems;
}

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    public int currentStreak = 0;
    public string lastClaimDateStr = "";

    public System.Action OnDailyRewardUpdated;

    private readonly DailyRewardItem[] rewards = new DailyRewardItem[]
    {
        new DailyRewardItem { day = 1, rewardName = "$100 Khởi nghiệp", rewardMoney = 100f, rewardGems = 5 },
        new DailyRewardItem { day = 2, rewardName = "15 Kim Cương Mèo", rewardMoney = 50f, rewardGems = 15 },
        new DailyRewardItem { day = 3, rewardName = "Thức Ăn Mèo Hoàng Gia", rewardMoney = 150f, rewardGems = 10 },
        new DailyRewardItem { day = 4, rewardName = "$250 Doanh Thu", rewardMoney = 250f, rewardGems = 15 },
        new DailyRewardItem { day = 5, rewardName = "30 Kim Cương Mèo", rewardMoney = 100f, rewardGems = 30 },
        new DailyRewardItem { day = 6, rewardName = "$500 Quỹ Mở Quán", rewardMoney = 500f, rewardGems = 25 },
        new DailyRewardItem { day = 7, rewardName = "JACKPOT: $1000 & 50 🐾", rewardMoney = 1000f, rewardGems = 50 }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanClaimToday()
    {
        string todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
        return lastClaimDateStr != todayStr;
    }

    public bool ClaimDailyReward()
    {
        if (!CanClaimToday())
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("Hôm nay bạn đã nhận quà rồi!", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.yellow);
            }
            return false;
        }

        int rewardIndex = currentStreak % 7;
        DailyRewardItem reward = rewards[rewardIndex];

        if (MoneyManager.Instance != null && reward.rewardMoney > 0)
        {
            MoneyManager.Instance.AddMoney(reward.rewardMoney);
        }

        if (GameManager.Instance != null && reward.rewardGems > 0)
        {
            GameManager.Instance.AddGems(reward.rewardGems);
        }

        currentStreak++;
        lastClaimDateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoin();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText($"🎁 Nhận Quà Ngày {reward.day}: +${reward.rewardMoney} & +{reward.rewardGems} 🐾!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.green);
        }

        OnDailyRewardUpdated?.Invoke();
        SaveManager.Instance?.SaveGame();
        return true;
    }

    public DailyRewardItem GetRewardForDay(int dayIndex)
    {
        int idx = Mathf.Clamp(dayIndex, 0, rewards.Length - 1);
        return rewards[idx];
    }
}
