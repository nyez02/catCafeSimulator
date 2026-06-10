using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public float CurrentMoney { get; private set; }

    // Event triggered whenever money changes (UI can listen to this)
    public event Action<float> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddMoney(float amount)
    {
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        Debug.Log($"Earned ${amount}. Total: ${CurrentMoney}");
    }

    public bool SpendMoney(float amount)
    {
        if (CurrentMoney >= amount)
        {
            CurrentMoney -= amount;
            OnMoneyChanged?.Invoke(CurrentMoney);
            Debug.Log($"Spent ${amount}. Remaining: ${CurrentMoney}");
            return true;
        }
        
        Debug.Log("Not enough money!");
        return false;
    }

    public void LoadMoney(float savedMoney)
    {
        CurrentMoney = savedMoney;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }
}
