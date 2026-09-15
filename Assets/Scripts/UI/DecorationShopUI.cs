using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecorationShopUI : MonoBehaviour
{
    public static DecorationShopUI Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject decorPanel;
    public Transform itemsContainer;
    public GameObject itemCardTemplate;
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

        if (itemCardTemplate != null)
        {
            itemCardTemplate.SetActive(false);
        }

        if (decorPanel != null)
        {
            decorPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
    }

    public void OpenShop()
    {
        if (decorPanel != null)
        {
            decorPanel.SetActive(true);
        }

        RefreshItems();
    }

    public void CloseShop()
    {
        if (decorPanel != null)
        {
            decorPanel.SetActive(false);
        }
    }

    public void RefreshItems()
    {
        ClearCards();

        if (DecorationManager.Instance == null || itemsContainer == null) return;

        foreach (var decor in DecorationManager.Instance.allDecorations)
        {
            CreateItemCard(decor);
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

    private void CreateItemCard(DecorationItem decor)
    {
        if (decor == null) return;

        GameObject card;
        if (itemCardTemplate != null)
        {
            card = Instantiate(itemCardTemplate, itemsContainer);
            card.SetActive(true);
        }
        else
        {
            card = new GameObject($"DecorCard_{decor.id}");
            card.transform.SetParent(itemsContainer, false);
            var text = card.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
        }

        bool isUnlocked = DecorationManager.Instance.IsUnlocked(decor.id);

        var tmPro = card.GetComponentInChildren<TextMeshProUGUI>();
        if (tmPro != null)
        {
            string costStr = decor.costGems > 0 
                ? $"${decor.costMoney:0} & {decor.costGems}🐾" 
                : $"${decor.costMoney:0}";
            string statusStr = isUnlocked ? "<color=green>✓ ĐÃ SỞ HỮU</color>" : $"<color=yellow>{costStr}</color>";

            tmPro.text = $"<b>{decor.itemName}</b>\n{decor.description}\nTrạng thái: {statusStr}";
        }

        var btn = card.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.interactable = !isUnlocked;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (DecorationManager.Instance.BuyDecoration(decor.id))
                {
                    RefreshItems();
                }
            });
        }

        spawnedCards.Add(card);
    }
}
