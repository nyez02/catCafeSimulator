using UnityEngine;
using System;

public class LuckyPiggyBank : MonoBehaviour
{
    public static LuckyPiggyBank Instance { get; private set; }

    [Header("Hũ Hụi Mèo Tài Lộc Settings")]
    public float currentBalance = 0f;
    public float maxCapacity = 150f;
    [Range(0.05f, 0.5f)] public float tipCutRate = 0.15f; // Trích 15% tiền tip vào hũ
    public int bonusGemsOnBreak = 15; // Thưởng 15 kim cương khi đập hũ

    public event Action<float, float> OnHuiBalanceChanged; // (current, max)
    public event Action OnHuiBroken;

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
        OnHuiBalanceChanged?.Invoke(currentBalance, maxCapacity);
    }

    /// <summary>
    /// Trích tiền tip từ khách hàng bỏ vào hũ hụi mèo
    /// </summary>
    public void AddTipToHui(float tipAmount)
    {
        if (tipAmount <= 0) return;

        float added = tipAmount * tipCutRate;
        currentBalance = Mathf.Min(maxCapacity, currentBalance + added);
        OnHuiBalanceChanged?.Invoke(currentBalance, maxCapacity);

        if (IsFull())
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("🐷 Hũ Hụi Mèo Đã Đầy!", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.magenta);
            }
        }
    }

    public bool IsFull() => currentBalance >= maxCapacity;

    /// <summary>
    /// Người chơi đập hũ hụi để nhận Jackpot Tiền Mặt + Kim Cương Mèo
    /// </summary>
    public bool BreakHuiBank()
    {
        if (currentBalance < 10f)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("Hũ còn ít tiền quá, hãy tích lũy thêm!", Camera.main.transform.position + Camera.main.transform.forward * 2.5f, Color.yellow);
            }
            return false;
        }

        float jackpotMoney = currentBalance;
        int rewardedGems = IsFull() ? bonusGemsOnBreak : Mathf.RoundToInt(bonusGemsOnBreak * (currentBalance / maxCapacity));

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(jackpotMoney);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGems(rewardedGems);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoin();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText($"🎉 ĐẬP HŨ THÀNH CÔNG! +${jackpotMoney:0.0} & +{rewardedGems} 🐾", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.green);
        }

        // Reset hũ và nâng cấp sức chứa cho kỳ hụi kế tiếp
        currentBalance = 0f;
        maxCapacity = Mathf.Round(maxCapacity * 1.25f);
        OnHuiBalanceChanged?.Invoke(currentBalance, maxCapacity);
        OnHuiBroken?.Invoke();

        SaveManager.Instance?.SaveGame();
        return true;
    }

    public void LoadHuiData(float balance, float capacity)
    {
        currentBalance = balance;
        if (capacity > 0) maxCapacity = capacity;
        OnHuiBalanceChanged?.Invoke(currentBalance, maxCapacity);
    }
}
