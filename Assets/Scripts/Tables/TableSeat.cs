using UnityEngine;

public enum TableState
{
    Empty,
    Occupied,
    WaitingFood,
    Eating,
    NeedsCleaning
}

public class TableSeat : MonoBehaviour
{
    [Header("Table Settings")]
    public int tableId = 1;
    public bool isUnlocked = true;
    public Transform sitPoint;
    public Transform foodPoint;
    public GameObject foodVisual; // Model bánh/cafe hiển thị trên bàn

    [Header("Cat Affinity")]
    public float catDetectionRadius = 3.5f;

    public TableState CurrentState { get; private set; } = TableState.Empty;
    public CustomerAI CurrentCustomer { get; private set; }

    private void Awake()
    {
        if (sitPoint == null)
        {
            sitPoint = transform;
        }

        if (foodVisual != null)
        {
            foodVisual.SetActive(false);
        }

        // Tự động đăng ký với TableManager nếu có
        if (TableManager.Instance != null)
        {
            TableManager.Instance.RegisterTable(this);
        }
    }

    private void Start()
    {
        // Đảm bảo được đăng ký sau khi TableManager đã khởi tạo
        if (TableManager.Instance != null)
        {
            TableManager.Instance.RegisterTable(this);
        }
    }

    private void OnDestroy()
    {
        if (TableManager.Instance != null)
        {
            TableManager.Instance.UnregisterTable(this);
        }
    }

    public bool IsAvailable()
    {
        return isUnlocked && CurrentState == TableState.Empty;
    }

    public void Occupy(CustomerAI customer)
    {
        CurrentCustomer = customer;
        CurrentState = TableState.Occupied;
    }

    public void OrderPlaced()
    {
        CurrentState = TableState.WaitingFood;
    }

    public void ServeFood()
    {
        CurrentState = TableState.Eating;
        if (foodVisual != null)
        {
            foodVisual.SetActive(true);
        }
    }

    public void ReleaseSeat()
    {
        CurrentCustomer = null;
        CurrentState = TableState.Empty;
        if (foodVisual != null)
        {
            foodVisual.SetActive(false);
        }
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        gameObject.SetActive(unlocked);
    }

    private static readonly Collider[] catHitsBuffer = new Collider[16];

    /// <summary>
    /// Kiểm tra xem hiện có bao nhiêu chú mèo đang ở gần bàn này (Tối ưu Non-allocating, 0 rác bộ nhớ)
    /// </summary>
    public int GetNearbyCatCount()
    {
        int count = 0;
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, catDetectionRadius, catHitsBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = catHitsBuffer[i];
            if (col != null && (col.GetComponent<CatAI>() != null || col.GetComponentInParent<CatAI>() != null))
            {
                count++;
            }
        }
        return count;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, catDetectionRadius);
        if (sitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(sitPoint.position, 0.25f);
        }
    }
}
