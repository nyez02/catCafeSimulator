using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DailyQuest
{
    public string id;
    public string title;
    public int targetCount;
    public int currentCount;
    public float rewardMoney;
    public int rewardGems;
    public bool isClaimed;

    public bool IsCompleted => currentCount >= targetCount;
}

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance { get; private set; }

    [Header("Today Quests")]
    public List<DailyQuest> quests = new List<DailyQuest>();
    public string lastQuestDateStr = "";

    public event Action OnQuestsUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CheckAndResetDailyQuests();
    }

    public void CheckAndResetDailyQuests()
    {
        string todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (lastQuestDateStr != todayStr)
        {
            lastQuestDateStr = todayStr;
            GenerateNewDailyQuests();
        }
    }

    private void GenerateNewDailyQuests()
    {
        quests.Clear();
        quests.Add(new DailyQuest { id = "quest_serve", title = "Phục vụ 5 lượt khách cafe", targetCount = 5, currentCount = 0, rewardMoney = 150f, rewardGems = 5, isClaimed = false });
        quests.Add(new DailyQuest { id = "quest_pet", title = "Vuốt ve mèo cưng 5 lần", targetCount = 5, currentCount = 0, rewardMoney = 100f, rewardGems = 5, isClaimed = false });
        quests.Add(new DailyQuest { id = "quest_visit_friend", title = "Ghé thăm 1 quán cafe của bạn bè", targetCount = 1, currentCount = 0, rewardMoney = 150f, rewardGems = 5, isClaimed = false });
        quests.Add(new DailyQuest { id = "quest_coop_pet", title = "Vuốt ve mèo ở quán bạn 2 lần", targetCount = 2, currentCount = 0, rewardMoney = 200f, rewardGems = 10, isClaimed = false });
        quests.Add(new DailyQuest { id = "quest_gift", title = "Gửi 1 hộp quà kết thân cho bạn", targetCount = 1, currentCount = 0, rewardMoney = 120f, rewardGems = 5, isClaimed = false });
        quests.Add(new DailyQuest { id = "quest_hui", title = "Tích lũy $50 vào Hũ Hụi Mèo", targetCount = 50, currentCount = 0, rewardMoney = 200f, rewardGems = 10, isClaimed = false });

        OnQuestsUpdated?.Invoke();
    }

    public void AddQuestProgress(string questId, int amount = 1)
    {
        DailyQuest quest = quests.Find(q => q.id == questId);
        if (quest != null && !quest.isClaimed && !quest.IsCompleted)
        {
            quest.currentCount = Mathf.Min(quest.targetCount, quest.currentCount + amount);
            OnQuestsUpdated?.Invoke();

            if (quest.IsCompleted)
            {
                ToastManager.Instance?.ShowToast($"Nhiệm vụ hoàn thành: {quest.title}!", "🎯", new Color(0.2f, 0.6f, 0.3f, 0.95f));
                if (UIManager.Instance != null && Camera.main != null)
                {
                    UIManager.Instance.ShowFloatingText($"🎯 Hoàn thành: {quest.title}!", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.green);
                }
            }
        }
    }

    public bool ClaimQuestReward(string questId)
    {
        DailyQuest quest = quests.Find(q => q.id == questId);
        if (quest != null && quest.IsCompleted && !quest.isClaimed)
        {
            quest.isClaimed = true;

            if (MoneyManager.Instance != null && quest.rewardMoney > 0)
            {
                MoneyManager.Instance.AddMoney(quest.rewardMoney);
            }

            if (GameManager.Instance != null && quest.rewardGems > 0)
            {
                GameManager.Instance.AddGems(quest.rewardGems);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCoin();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText($"🎁 Đã nhận +${quest.rewardMoney} & +{quest.rewardGems}🐾!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.yellow);
            }

            OnQuestsUpdated?.Invoke();
            SaveManager.Instance?.SaveGame();
            return true;
        }
        return false;
    }
}
