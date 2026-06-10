using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    private NavMeshAgent agent;
    
    [Header("Cài đặt Khách hàng")]
    public float cafeStayTime = 10f; // Thời gian khách ngồi chơi ở quán
    public float moneyToPay = 15f;   // Số tiền khách sẽ trả
    
    private Transform exitPoint;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Hàm này được gọi từ CustomerManager khi sinh ra khách
    public void Initialize(Transform exit)
    {
        exitPoint = exit;
        StartCoroutine(CustomerRoutine());
    }

    private IEnumerator CustomerRoutine()
    {
        // 1. Tìm một chỗ trống ngẫu nhiên trong quán để đi tới
        Vector3 targetSpot = GetRandomCafeSpot();
        agent.SetDestination(targetSpot);

        // Đợi cho đến khi khách đi tới nơi
        yield return new WaitUntil(() => HasArrived());

        // 2. Ngồi chơi đùa với mèo / uống cà phê
        yield return new WaitForSeconds(cafeStayTime);

        // 3. Trả tiền trước khi về
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(moneyToPay);
        }

        // 4. Đi về cửa ra vào
        if (exitPoint != null)
        {
            agent.SetDestination(exitPoint.position);
            yield return new WaitUntil(() => HasArrived());
        }

        // Xóa object khi khách đã ra khỏi cửa
        Destroy(gameObject);
    }

    private bool HasArrived()
    {
        // Kiểm tra xem Agent đã tới đích chưa
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    private Vector3 GetRandomCafeSpot()
    {
        // Lấy ngẫu nhiên 1 điểm trên NavMesh để khách đi tới
        // Trong thực tế bạn có thể thay thế bằng việc tìm "Bàn trống" (Empty Table)
        Vector3 randomDirection = Random.insideUnitSphere * 15f;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 15f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position; // Trả về vị trí hiện tại nếu không tìm thấy
    }
}
