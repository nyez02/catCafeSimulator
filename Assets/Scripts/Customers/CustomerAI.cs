using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Settings")]
    public float cafeStayTime = 8f;
    public float basePayment = 15f;

    private Transform exitPoint;
    private TableSeat currentTable;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(Transform exit)
    {
        exitPoint = exit;
        StartCoroutine(CustomerLifecycleRoutine());
    }

    private IEnumerator CustomerLifecycleRoutine()
    {
        // 1. Tìm bàn trống
        TableSeat table = null;
        if (TableManager.Instance != null)
        {
            table = TableManager.Instance.GetAvailableTable();
        }

        if (table == null)
        {
            // Không có bàn, đợi một lát
            yield return new WaitForSeconds(3f);
            if (TableManager.Instance != null)
            {
                table = TableManager.Instance.GetAvailableTable();
            }

            if (table == null)
            {
                // Vẫn không có bàn -> Rời quán với thông báo
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFloatingText("Hết bàn rồi! :(", transform.position + Vector3.up * 2f, Color.red);
                }
                yield return LeaveCafe();
                yield break;
            }
        }

        // 2. Chiếm bàn và đi tới chỗ ngồi
        currentTable = table;
        currentTable.Occupy(this);

        Vector3 sitPos = currentTable.sitPoint != null ? currentTable.sitPoint.position : currentTable.transform.position;
        agent.SetDestination(sitPos);

        yield return new WaitUntil(HasArrived);

        // Xoay nhẹ theo hướng ghế
        if (currentTable.sitPoint != null)
        {
            transform.rotation = currentTable.sitPoint.rotation;
        }

        // 3. Gọi món & Chờ phục vụ
        currentTable.OrderPlaced();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText("☕ Order!", transform.position + Vector3.up * 2f, Color.yellow);
        }

        yield return new WaitForSeconds(2.5f); // Thời gian pha chế & bưng đồ

        // Đồ ăn/cafe được dọn lên bàn
        currentTable.ServeFood();

        // 4. Thưởng thức cafe & chơi với mèo
        yield return new WaitForSeconds(cafeStayTime);

        // 5. Tính tiền & Tiền tip mèo
        int nearbyCats = currentTable.GetNearbyCatCount();
        float tip = nearbyCats * 10f; // Mỗi chú mèo ở gần thưởng thêm $10
        float totalEarned = basePayment + tip;

        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(totalEarned);
        }

        if (LuckyPiggyBank.Instance != null && tip > 0)
        {
            LuckyPiggyBank.Instance.AddTipToHui(tip);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoin();
        }

        if (UIManager.Instance != null)
        {
            string msg = tip > 0 ? $"+${totalEarned} (Tip Mèo!)" : $"+${totalEarned}";
            UIManager.Instance.ShowFloatingText(msg, transform.position + Vector3.up * 2f, Color.green);
        }

        if (CatVFXManager.Instance != null)
        {
            CatVFXManager.Instance.SpawnCoinSparkle(transform.position + Vector3.up * 1f);
        }

        // 6. Rời bàn và về cửa
        currentTable.ReleaseSeat();
        currentTable = null;

        yield return LeaveCafe();
    }

    private IEnumerator LeaveCafe()
    {
        if (exitPoint != null)
        {
            agent.SetDestination(exitPoint.position);
            yield return new WaitUntil(HasArrived);
        }

        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.OnCustomerLeft();
        }

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnCustomer(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool HasArrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    private void OnDestroy()
    {
        if (currentTable != null)
        {
            currentTable.ReleaseSeat();
        }
    }
}
