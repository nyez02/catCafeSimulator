using UnityEngine;

public enum StaffRole
{
    Waiter,  // Bưng bê, phục vụ bàn
    Barista, // Pha chế cà phê
    Cleaner  // Dọn dẹp vệ sinh quán
}

[CreateAssetMenu(fileName = "NewStaff", menuName = "CatCafe/StaffData")]
public class StaffData : ScriptableObject
{
    public string staffId;
    public string staffName;
    public StaffRole role;
    public float costMoney;
    public float passiveIncomePerSec; // Tiền kiếm tự động mỗi giây
    public string iconEmoji;
    [TextArea] public string description;
}
