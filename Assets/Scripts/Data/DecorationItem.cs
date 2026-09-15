using UnityEngine;

public enum DecorationBuffType
{
    TipBonusPercent,            // Tăng % tiền tip từ khách
    OfflineEarningsBonus,       // Tăng % thu nhập khi vắng nhà
    CatHappinessGainBonus,      // Tăng điểm hạnh phúc khi vuốt ve
    HuiBankCapacityBonus        // Tăng sức chứa của Hũ Hụi Mèo Tài Lộc
}

[CreateAssetMenu(fileName = "NewDecoration", menuName = "Cat Cafe/Decoration Item")]
public class DecorationItem : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "decor_item";
    public string itemName = "Bàn Trà Gỗ Ấm Cúng";
    [TextArea(2, 4)]
    public string description = "Tăng 15% tiền tip từ khách hàng tới quán.";
    public Sprite icon;
    public GameObject prefab;

    [Header("Price")]
    public float costMoney = 200f;
    public int costGems = 0;

    [Header("Buff")]
    public DecorationBuffType buffType = DecorationBuffType.TipBonusPercent;
    public float buffValue = 15f; // Ví dụ 15% hoặc 50 điểm
}
