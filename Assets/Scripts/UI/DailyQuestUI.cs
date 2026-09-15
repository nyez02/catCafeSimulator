using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyQuestUI : MonoBehaviour
{
    public static DailyQuestUI Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject questPanel;
    public Transform questsContainer;
    public GameObject questCardTemplate;
    public Button closeButton;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (questCardTemplate != null)
        {
            questCardTemplate.SetActive(false);
        }

        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseQuestPanel);
        }
    }

    private void Start()
    {
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestsUpdated += RefreshQuestUI;
        }
    }

    private void OnDestroy()
    {
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.OnQuestsUpdated -= RefreshQuestUI;
        }
    }

    public void OpenQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }

        RefreshQuestUI();
    }

    public void CloseQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }

    public void RefreshQuestUI()
    {
        ClearCards();

        if (DailyQuestManager.Instance == null || questsContainer == null) return;

        foreach (var quest in DailyQuestManager.Instance.quests)
        {
            CreateQuestCard(quest);
        }
    }

    private void ClearCards()
    {
        foreach (var obj in spawnedCards)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedCards.Clear();
    }

    private void CreateQuestCard(DailyQuest quest)
    {
        if (quest == null) return;

        GameObject card;
        if (questCardTemplate != null)
        {
            card = Instantiate(questCardTemplate, questsContainer);
            card.SetActive(true);
        }
        else
        {
            card = new GameObject($"QuestCard_{quest.id}");
            card.transform.SetParent(questsContainer, false);
            var text = card.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
        }

        var tmPro = card.GetComponentInChildren<TextMeshProUGUI>();
        if (tmPro != null)
        {
            string status = quest.isClaimed 
                ? "<color=grey>✓ ĐÃ NHẬN</color>" 
                : (quest.IsCompleted ? "<color=green>NHẬN THƯỞNG</color>" : $"<color=yellow>{quest.currentCount}/{quest.targetCount}</color>");

            tmPro.text = $"<b>{quest.title}</b>\nTiến độ: {status}\nThưởng: +${quest.rewardMoney:0} & +{quest.rewardGems}🐾";
        }

        var btn = card.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.interactable = quest.IsCompleted && !quest.isClaimed;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                DailyQuestManager.Instance.ClaimQuestReward(quest.id);
            });
        }

        spawnedCards.Add(card);
    }
}
