using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance { get; private set; }

    [Header("Tables in Cafe")]
    [SerializeField] private List<TableSeat> allTables = new List<TableSeat>();

    public System.Action OnTableUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tự động tìm tất cả bàn trong scene nếu danh sách đang trống
        if (allTables.Count == 0)
        {
            allTables.AddRange(FindObjectsByType<TableSeat>(FindObjectsSortMode.None));
        }
    }

    public void RegisterTable(TableSeat table)
    {
        if (!allTables.Contains(table))
        {
            allTables.Add(table);
            OnTableUpdated?.Invoke();
        }
    }

    public void UnregisterTable(TableSeat table)
    {
        if (allTables.Contains(table))
        {
            allTables.Remove(table);
            OnTableUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Tìm một bàn còn trống và đã được mở khóa
    /// </summary>
    public TableSeat GetAvailableTable()
    {
        foreach (var table in allTables)
        {
            if (table != null && table.IsAvailable())
            {
                return table;
            }
        }
        return null;
    }

    /// <summary>
    /// Số bàn đã mở khóa
    /// </summary>
    public int GetUnlockedTableCount()
    {
        int count = 0;
        foreach (var table in allTables)
        {
            if (table != null && table.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Mở khóa số lượng bàn theo dữ liệu Save
    /// </summary>
    public void SetUnlockedTables(int count)
    {
        for (int i = 0; i < allTables.Count; i++)
        {
            if (allTables[i] != null)
            {
                allTables[i].SetUnlocked(i < count);
            }
        }
        OnTableUpdated?.Invoke();
    }

    /// <summary>
    /// Mở khóa thêm một bàn tiếp theo trong quán
    /// </summary>
    public bool UnlockNextTable()
    {
        foreach (var table in allTables)
        {
            if (table != null && !table.isUnlocked)
            {
                table.SetUnlocked(true);
                OnTableUpdated?.Invoke();
                return true;
            }
        }
        return false;
    }

    public int GetTotalTableCount() => allTables.Count;
}
