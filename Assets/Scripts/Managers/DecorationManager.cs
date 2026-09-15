using System;
using System.Collections.Generic;
using UnityEngine;

public class DecorationManager : MonoBehaviour
{
    public static DecorationManager Instance { get; private set; }

    [Header("Catalog")]
    public List<DecorationItem> allDecorations = new List<DecorationItem>();

    [Header("Unlocked State")]
    public List<string> unlockedDecorationIds = new List<string>();

    public event Action<DecorationItem> OnDecorationUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureDefaultCatalog();
    }

    private void EnsureDefaultCatalog()
    {
        // Khởi tạo danh mục mặc định nếu chưa gán trong Inspector
        if (allDecorations.Count == 0)
        {
            var item1 = ScriptableObject.CreateInstance<DecorationItem>();
            item1.id = "scratch_post";
            item1.itemName = "Cột Cào Móng Gỗ Mun";
            item1.description = "Mèo thích cào móng vào đây. Tăng +50% hạnh phúc khi vuốt ve mèo.";
            item1.costMoney = 150f;
            item1.buffType = DecorationBuffType.CatHappinessGainBonus;
            item1.buffValue = 50f;
            allDecorations.Add(item1);

            var item2 = ScriptableObject.CreateInstance<DecorationItem>();
            item2.id = "cat_cushion";
            item2.itemName = "Đệm Lười Bông Xù";
            item2.description = "Chỗ ngủ êm ái cho mèo. Tăng +25% thu nhập khi bạn vắng nhà (Offline).";
            item2.costMoney = 250f;
            item2.buffType = DecorationBuffType.OfflineEarningsBonus;
            item2.buffValue = 25f;
            allDecorations.Add(item2);

            var item3 = ScriptableObject.CreateInstance<DecorationItem>();
            item3.id = "cozy_chandelier";
            item3.itemName = "Đèn Chùm Ấm Cúng";
            item3.description = "Không gian quán ấm áp hơn. Khách tip thêm +20% tiền cho mèo.";
            item3.costMoney = 400f;
            item3.buffType = DecorationBuffType.TipBonusPercent;
            item3.buffValue = 20f;
            allDecorations.Add(item3);

            var item4 = ScriptableObject.CreateInstance<DecorationItem>();
            item4.id = "royal_cat_tree";
            item4.itemName = "Tháp Mèo Hoàng Gia";
            item4.description = "Đỉnh cao vui chơi cho mèo. Tăng thêm +$100 sức chứa cho Hũ Hụi Mèo.";
            item4.costMoney = 650f;
            item4.costGems = 10;
            item4.buffType = DecorationBuffType.HuiBankCapacityBonus;
            item4.buffValue = 100f;
            allDecorations.Add(item4);
        }
    }

    public bool IsUnlocked(string decorId)
    {
        return unlockedDecorationIds.Contains(decorId);
    }

    public bool BuyDecoration(string decorId)
    {
        DecorationItem item = allDecorations.Find(d => d.id == decorId);
        if (item == null) return false;

        if (IsUnlocked(decorId))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("Đã sở hữu vật phẩm này rồi!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.yellow);
            }
            return false;
        }

        // Kiểm tra tiền
        if (MoneyManager.Instance != null && MoneyManager.Instance.CurrentMoney < item.costMoney)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("Không đủ tiền mua nội thất!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.red);
            }
            return false;
        }

        // Kiểm tra kim cương (nếu có yêu cầu)
        if (item.costGems > 0 && GameManager.Instance != null && GameManager.Instance.pawGems < item.costGems)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("Không đủ Kim Cương Mèo!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.red);
            }
            return false;
        }

        // Trừ tiền & kim cương
        if (MoneyManager.Instance != null && item.costMoney > 0)
        {
            MoneyManager.Instance.SpendMoney(item.costMoney);
        }

        if (GameManager.Instance != null && item.costGems > 0)
        {
            GameManager.Instance.SpendGems(item.costGems);
        }

        unlockedDecorationIds.Add(decorId);
        OnDecorationUnlocked?.Invoke(item);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoin();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText($"✨ Mở Khóa: {item.itemName}!", Camera.main.transform.position + Camera.main.transform.forward * 2f, Color.green);
        }

        // Lưu game ngay lập tức
        SaveManager.Instance?.SaveGame();
        return true;
    }

    // --- Buff Calculators ---

    public float GetTipBonusMultiplier()
    {
        float bonusPercent = 0f;
        foreach (var id in unlockedDecorationIds)
        {
            var item = allDecorations.Find(d => d.id == id);
            if (item != null && item.buffType == DecorationBuffType.TipBonusPercent)
            {
                bonusPercent += item.buffValue;
            }
        }
        return 1f + (bonusPercent / 100f);
    }

    public float GetOfflineBonusMultiplier()
    {
        float bonusPercent = 0f;
        foreach (var id in unlockedDecorationIds)
        {
            var item = allDecorations.Find(d => d.id == id);
            if (item != null && item.buffType == DecorationBuffType.OfflineEarningsBonus)
            {
                bonusPercent += item.buffValue;
            }
        }
        return 1f + (bonusPercent / 100f);
    }

    public float GetCatHappinessBonus()
    {
        float bonus = 0f;
        foreach (var id in unlockedDecorationIds)
        {
            var item = allDecorations.Find(d => d.id == id);
            if (item != null && item.buffType == DecorationBuffType.CatHappinessGainBonus)
            {
                bonus += item.buffValue;
            }
        }
        return bonus;
    }

    public float GetHuiCapacityBonus()
    {
        float bonus = 0f;
        foreach (var id in unlockedDecorationIds)
        {
            var item = allDecorations.Find(d => d.id == id);
            if (item != null && item.buffType == DecorationBuffType.HuiBankCapacityBonus)
            {
                bonus += item.buffValue;
            }
        }
        return bonus;
    }
}
