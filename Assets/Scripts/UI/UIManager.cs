using UnityEngine;
using TMPro; // Dùng cho TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("Player UI")]
    public TextMeshProUGUI moneyText;
    // public UnityEngine.UI.Image playerAvatar; // Nếu bạn có hình đại diện nhân vật

    private void Start()
    {
        // Khi game bắt đầu, lấy số tiền hiện tại hiển thị lên UI
        if (MoneyManager.Instance != null)
        {
            UpdateMoneyUI(MoneyManager.Instance.CurrentMoney);
            
            // Đăng ký sự kiện: Mỗi khi tiền thay đổi, tự động gọi hàm UpdateMoneyUI
            MoneyManager.Instance.OnMoneyChanged += UpdateMoneyUI;
        }
    }

    private void OnDestroy()
    {
        // Gỡ đăng ký sự kiện khi object bị hủy để tránh lỗi rò rỉ bộ nhớ
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
        }
    }

    private void UpdateMoneyUI(float currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = "Tiền: $" + currentMoney.ToString("0.00");
        }
    }

    // Gắn hàm này vào sự kiện OnClick của một Nút (Button) trên màn hình
    public void OnServeCoffeeButtonClicked()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(15f); // Mỗi lần phục vụ cafe được 15 đô
            Debug.Log("Đã phục vụ xong! Kiếm được $15");
        }
    }
}
