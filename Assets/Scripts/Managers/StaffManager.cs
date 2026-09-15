using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffManager : MonoBehaviour
{
    public static StaffManager Instance { get; private set; }

    [Header("Staff Catalog")]
    public List<StaffData> allStaff = new List<StaffData>();

    [Header("Hired Staff")]
    public List<string> hiredStaffIds = new List<string>();

    [Header("Prefab for Staff Cats (Optional)")]
    public GameObject staffCatPrefab;

    public event Action<StaffData> OnStaffHired;
    private const string PREFS_HIRED_STAFF = "CatCafe_HiredStaffIds";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureDefaultStaffCatalog();
        LoadHiredStaff();
    }

    private void Start()
    {
        StartCoroutine(PassiveIncomeRoutine());
        SpawnHiredStaffVisuals();
    }

    private void EnsureDefaultStaffCatalog()
    {
        if (allStaff.Count == 0)
        {
            var staff1 = ScriptableObject.CreateInstance<StaffData>();
            staff1.staffId = "staff_waiter_milo";
            staff1.staffName = "Mèo Phục Vụ Milo";
            staff1.role = StaffRole.Waiter;
            staff1.costMoney = 200f;
            staff1.passiveIncomePerSec = 2f;
            staff1.iconEmoji = "☕";
            staff1.description = "Bưng bê cà phê nhanh nhẹn, mang lại $2/giây thu nhập tự động.";
            allStaff.Add(staff1);

            var staff2 = ScriptableObject.CreateInstance<StaffData>();
            staff2.staffId = "staff_barista_luna";
            staff2.staffName = "Mèo Pha Chế Luna";
            staff2.role = StaffRole.Barista;
            staff2.costMoney = 450f;
            staff2.passiveIncomePerSec = 5f;
            staff2.iconEmoji = "🍳";
            staff2.description = "Tay nghề pha latte nghệ thuật tuyệt đỉnh, mang lại $5/giây tự động.";
            allStaff.Add(staff2);

            var staff3 = ScriptableObject.CreateInstance<StaffData>();
            staff3.staffId = "staff_cleaner_simba";
            staff3.staffName = "Mèo Tạp Vụ Simba";
            staff3.role = StaffRole.Cleaner;
            staff3.costMoney = 750f;
            staff3.passiveIncomePerSec = 10f;
            staff3.iconEmoji = "🧹";
            staff3.description = "Dọn dẹp quán sạch bong kin kít, thu hút khách VIP và mang lại $10/giây.";
            allStaff.Add(staff3);
        }
    }

    public bool IsHired(string staffId)
    {
        return hiredStaffIds.Contains(staffId);
    }

    public bool HireStaff(string staffId)
    {
        StaffData data = allStaff.Find(s => s.staffId == staffId);
        if (data == null) return false;

        if (IsHired(staffId))
        {
            ToastManager.Instance?.ShowToast($"Bạn đã thuê nhân viên {data.staffName} rồi!", "ℹ️");
            return false;
        }

        if (MoneyManager.Instance != null && MoneyManager.Instance.CurrentMoney < data.costMoney)
        {
            ToastManager.Instance?.ShowToast($"Không đủ tiền thuê nhân viên! Cần ${data.costMoney}", "❌", new Color(0.8f, 0.2f, 0.2f, 0.95f));
            return false;
        }

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(-data.costMoney);
        }

        hiredStaffIds.Add(staffId);
        SaveHiredStaff();
        SpawnStaffCat(data);

        OnStaffHired?.Invoke(data);
        ToastManager.Instance?.ShowToast($"Đã thuê thành công {data.staffName}! {data.iconEmoji}", "🎉", new Color(0.2f, 0.6f, 0.3f, 0.95f));
        SoundManager.Instance?.PlayCoin();

        return true;
    }

    private void SpawnHiredStaffVisuals()
    {
        foreach (var id in hiredStaffIds)
        {
            StaffData data = allStaff.Find(s => s.staffId == id);
            if (data != null)
            {
                SpawnStaffCat(data);
            }
        }
    }

    private void SpawnStaffCat(StaffData data)
    {
        Vector3 spawnPos = Vector3.zero;
        if (Camera.main != null)
        {
            spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 4f;
            spawnPos.y = 0f;
        }

        GameObject catObj = null;
        if (staffCatPrefab != null)
        {
            catObj = Instantiate(staffCatPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Fallback tạo một mèo nhân viên tượng trưng nếu chưa gán prefab
            catObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            catObj.transform.position = spawnPos;
            catObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            var rend = catObj.GetComponent<Renderer>();
            if (rend != null) rend.material.color = new Color(1f, 0.6f, 0.2f);
        }

        catObj.name = $"Staff_{data.staffName}";
        var staffAI = catObj.AddComponent<CatStaffAI>();
        staffAI.role = data.role;
        staffAI.staffName = data.staffName;

        CatVFXManager.Instance?.PlayConfetti(spawnPos + Vector3.up * 0.5f);
    }

    private IEnumerator PassiveIncomeRoutine()
    {
        var waitOneSec = new WaitForSeconds(1f);
        while (true)
        {
            yield return waitOneSec;

            float totalPassive = 0f;
            foreach (var id in hiredStaffIds)
            {
                StaffData data = allStaff.Find(s => s.staffId == id);
                if (data != null)
                {
                    totalPassive += data.passiveIncomePerSec;
                }
            }

            if (totalPassive > 0f && MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(totalPassive);
            }
        }
    }

    private void SaveHiredStaff()
    {
        string serialized = string.Join(",", hiredStaffIds);
        PlayerPrefs.SetString(PREFS_HIRED_STAFF, serialized);
        PlayerPrefs.Save();
    }

    private void LoadHiredStaff()
    {
        if (PlayerPrefs.HasKey(PREFS_HIRED_STAFF))
        {
            string serialized = PlayerPrefs.GetString(PREFS_HIRED_STAFF);
            if (!string.IsNullOrEmpty(serialized))
            {
                hiredStaffIds = new List<string>(serialized.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}
