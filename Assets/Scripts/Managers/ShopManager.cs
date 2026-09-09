using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Upgrade Costs")]
    public float tableUnlockCost = 100f;
    public float premiumFoodCost = 30f;

    public System.Action OnShopPurchased;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Mua một chú mèo mới
    /// </summary>
    public bool BuyCat(CatData data)
    {
        if (data == null) return false;

        if (MoneyManager.Instance != null && MoneyManager.Instance.SpendMoney(data.purchaseCost))
        {
            if (CatManager.Instance != null)
            {
                CatManager.Instance.SpawnCat(data);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCatMeow();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText($"Đã nhận mèo {data.catName}!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.cyan);
            }

            OnShopPurchased?.Invoke();
            SaveManager.Instance?.SaveGame();
            return true;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText("Không đủ tiền!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.red);
        }
        return false;
    }

    /// <summary>
    /// Mở khóa thêm bàn cafe mới
    /// </summary>
    public bool BuyTableUnlock()
    {
        if (MoneyManager.Instance != null && MoneyManager.Instance.SpendMoney(tableUnlockCost))
        {
            if (TableManager.Instance != null && TableManager.Instance.UnlockNextTable())
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayCoin();
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingText("Mở thêm bàn mới thành công!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.green);
                }

                tableUnlockCost = Mathf.Round(tableUnlockCost * 1.5f); // Tăng giá cho bàn kế tiếp
                OnShopPurchased?.Invoke();
                SaveManager.Instance?.SaveGame();
                return true;
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText("Không đủ tiền hoặc đã hết bàn!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.red);
        }
        return false;
    }

    /// <summary>
    /// Mua gói thức ăn cao cấp làm no và tăng tối đa hạnh phúc cho toàn bộ mèo
    /// </summary>
    public bool BuyPremiumCatFood()
    {
        if (MoneyManager.Instance != null && MoneyManager.Instance.SpendMoney(premiumFoodCost))
        {
            if (CatManager.Instance != null)
            {
                CatManager.Instance.FeedAllCats();
                foreach (var cat in CatManager.Instance.GetAllCats())
                {
                    if (cat != null)
                    {
                        cat.happiness = 100f;
                    }
                }
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFloatingText("Tất cả mèo đã được no nê & vui vẻ!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.yellow);
            }

            OnShopPurchased?.Invoke();
            return true;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText("Không đủ tiền mua thức ăn!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.red);
        }
        return false;
    }
}
