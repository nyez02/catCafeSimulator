using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Cat Cafe/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelId = 1;
    public string levelName = "Tiệm Mèo Khởi Nghiệp";
    [TextArea] public string description = "Quán cà phê mèo nhỏ ấm cúng cho người mới bắt đầu.";
    public Sprite thumbnail;

    [Header("Shift Settings")]
    public float shiftDurationSeconds = 90f; // Thời gian ca làm việc (90 giây)
    public float rushHourStartTime = 30f;    // Bắt đầu giờ cao điểm ở giây 30
    public float rushHourDuration = 25f;     // Kéo dài 25 giây
    public float rushHourSpawnInterval = 4f; // Khách đến dồn dập hơn

    [Header("3-Star Targets")]
    public float targetRevenue1Star = 100f;  // Đạt $100 được 1 sao
    public float targetRevenue2Star = 250f;  // Đạt $250 được 2 sao
    public float targetRevenue3Star = 400f;  // Đạt $400 được 3 sao

    [Header("Rewards")]
    public int completionGems = 10;
    public float completionBonusMoney = 50f;
}
