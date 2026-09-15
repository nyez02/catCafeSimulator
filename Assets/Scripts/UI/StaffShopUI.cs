using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaffShopUI : MonoBehaviour
{
    public static StaffShopUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject panel;
    public Transform itemsContainer;
    public Button closeButton;

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
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }

        if (StaffManager.Instance != null)
        {
            StaffManager.Instance.OnStaffHired += (data) => RefreshUI();
        }
    }

    public void OpenShop()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            RefreshUI();
        }
    }

    public void CloseShop()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void RefreshUI()
    {
        if (itemsContainer == null || StaffManager.Instance == null) return;

        // Xóa các card cũ
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var staff in StaffManager.Instance.allStaff)
        {
            CreateStaffCard(staff);
        }
    }

    private void CreateStaffCard(StaffData staff)
    {
        GameObject card = new GameObject($"StaffCard_{staff.staffId}");
        card.transform.SetParent(itemsContainer, false);

        var bg = card.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.16f, 0.22f, 0.95f);

        var layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        bool isHired = StaffManager.Instance.IsHired(staff.staffId);

        // Header (Tên + Emoji)
        CreateText(card.transform, $"{staff.iconEmoji} {staff.staffName}", 20f, FontStyles.Bold, Color.white);

        // Mô tả & Lương tự động
        CreateText(card.transform, $"{staff.description}\n<color=#FFD700>Tự động: +${staff.passiveIncomePerSec}/giây</color>", 14f, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f));

        // Nút Thuê
        GameObject btnObj = new GameObject("HireButton");
        btnObj.transform.SetParent(card.transform, false);
        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = isHired ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.9f, 0.5f, 0.2f);

        var btn = btnObj.AddComponent<Button>();
        btn.interactable = !isHired;

        string btnLabel = isHired ? "Đã Đi Làm ✔" : $"Thuê (${staff.costMoney:0})";
        CreateText(btnObj.transform, btnLabel, 16f, FontStyles.Bold, Color.white);

        btn.onClick.AddListener(() =>
        {
            StaffManager.Instance.HireStaff(staff.staffId);
        });
    }

    private TextMeshProUGUI CreateText(Transform parent, string content, float size, FontStyles style, Color color)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }
}
