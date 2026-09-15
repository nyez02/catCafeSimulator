using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance { get; private set; }

    [Header("Panels & Containers")]
    public GameObject leaderboardPanel;
    public Transform entriesContainer;
    public GameObject entryTemplate;

    [Header("Player Info Controls")]
    public TMP_InputField nameInputField;
    public Button updateNameButton;

    [Header("Status & Actions")]
    public TextMeshProUGUI statusText;
    public Button refreshButton;
    public Button closeButton;

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (entryTemplate != null)
        {
            entryTemplate.SetActive(false);
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        if (updateNameButton != null)
        {
            updateNameButton.onClick.AddListener(OnUpdateNameClicked);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshLeaderboard);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseLeaderboard);
        }
    }

    private void Start()
    {
        if (nameInputField != null && FirebaseManager.Instance != null)
        {
            nameInputField.text = FirebaseManager.Instance.playerName;
        }
    }

    public void OpenLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
        }

        if (nameInputField != null && FirebaseManager.Instance != null)
        {
            nameInputField.text = FirebaseManager.Instance.playerName;
        }

        RefreshLeaderboard();
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    public void OnUpdateNameClicked()
    {
        if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
        {
            string newName = nameInputField.text.Trim();
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.SetPlayerName(newName);
                // Cập nhật lại điểm số hiện tại với tên mới
                if (LevelManager.Instance != null && MoneyManager.Instance != null && CatManager.Instance != null)
                {
                    FirebaseManager.Instance.SubmitScoreToLeaderboard(
                        LevelManager.Instance.cafeLevel,
                        MoneyManager.Instance.CurrentMoney,
                        CatManager.Instance.GetCatCount()
                    );
                }
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText($"Đã đổi tên: {newName}!", Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 2f : Vector3.zero, Color.cyan);
            }
            RefreshLeaderboard();
        }
    }

    public void RefreshLeaderboard()
    {
        ClearEntries();

        if (statusText != null)
        {
            statusText.text = "Đang tải bảng xếp hạng từ đám mây...";
        }

        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.isInitialized)
        {
            if (statusText != null)
            {
                statusText.text = "Không có kết nối Firebase Cloud. Vui lòng kiểm tra mạng.";
            }
            return;
        }

        FirebaseManager.Instance.FetchLeaderboard(20, entries =>
        {
            if (statusText != null)
            {
                statusText.text = (entries == null || entries.Count == 0) ? "Chưa có dữ liệu bảng xếp hạng." : "";
            }

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    CreateEntryRow(i + 1, entries[i]);
                }
            }
        });
    }

    private void ClearEntries()
    {
        foreach (var obj in spawnedEntries)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedEntries.Clear();
    }

    private void CreateEntryRow(int rank, LeaderboardEntry entry)
    {
        if (entriesContainer == null) return;

        GameObject row;
        if (entryTemplate != null)
        {
            row = Instantiate(entryTemplate, entriesContainer);
            row.SetActive(true);
        }
        else
        {
            row = new GameObject($"Rank_{rank}");
            row.transform.SetParent(entriesContainer, false);
            var text = row.AddComponent<TextMeshProUGUI>();
            text.fontSize = 20;
            text.color = rank switch
            {
                1 => new Color(1f, 0.85f, 0.2f), // Vàng
                2 => new Color(0.85f, 0.85f, 0.9f), // Bạc
                3 => new Color(0.8f, 0.5f, 0.2f), // Đồng
                _ => Color.white
            };
        }

        var tmPro = row.GetComponentInChildren<TextMeshProUGUI>();
        if (tmPro != null)
        {
            string medal = rank switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => $"#{rank}"
            };

            tmPro.text = $"{medal} {entry.playerName} | Cấp: Lv.{entry.cafeLevel} | Tài sản: ${entry.totalMoney:0} | Mèo: {entry.catCount} 🐾";
        }

        spawnedEntries.Add(row);
    }
}
