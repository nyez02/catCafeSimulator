using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CatAlbumUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject albumPanel;
    public Transform catCardsContainer;
    public GameObject catCardPrefab;
    public TextMeshProUGUI totalCatsText;

    private void Start()
    {
        if (albumPanel != null)
        {
            albumPanel.SetActive(false);
        }
    }

    public void ToggleAlbum()
    {
        if (albumPanel != null)
        {
            bool isOpen = !albumPanel.activeSelf;
            albumPanel.SetActive(isOpen);
            if (isOpen)
            {
                RefreshAlbumView();
            }
        }
    }

    public void RefreshAlbumView()
    {
        if (CatManager.Instance == null) return;

        List<CatAI> allCats = CatManager.Instance.GetAllCats();
        if (totalCatsText != null)
        {
            totalCatsText.text = $"Tổng số bé mèo: {allCats.Count}";
        }

        // Dọn dẹp danh sách cũ nếu có container
        if (catCardsContainer != null && catCardPrefab != null)
        {
            foreach (Transform child in catCardsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var cat in allCats)
            {
                if (cat == null) continue;

                GameObject cardObj = Instantiate(catCardPrefab, catCardsContainer);
                TextMeshProUGUI[] texts = cardObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                {
                    texts[0].text = cat.catName;
                }
                if (texts.Length > 1)
                {
                    texts[1].text = $"Đói: {cat.hunger:0}% | Vui vẻ: {cat.happiness:0}%";
                }
            }
        }
    }

    public void CloseAlbum()
    {
        if (albumPanel != null)
        {
            albumPanel.SetActive(false);
        }
    }
}
