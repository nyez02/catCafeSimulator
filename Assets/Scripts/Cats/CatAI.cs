using UnityEngine;
using UnityEngine.AI; // Cần thiết để dùng NavMesh

[RequireComponent(typeof(NavMeshAgent))] // Tự động gắn NavMeshAgent nếu chưa có
public class CatAI : MonoBehaviour
{
    private NavMeshAgent agent;
    
    [Header("Cài đặt Mèo")]
    public float walkRadius = 10f; // Bán kính đi lang thang
    public float waitTime = 3f;    // Thời gian đứng chơi trước khi đi tiếp
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = waitTime;
        
        // Mèo đi chậm rãi thong dong
        agent.speed = 2f; 
    }

    void Update()
    {
        // Kiểm tra xem mèo đã đi tới đích chưa
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            timer += Time.deltaTime;
            
            // Nếu chờ đủ lâu, tìm điểm mới để đi
            if (timer >= waitTime)
            {
                WalkToRandomPoint();
                timer = 0; // Reset thời gian chờ
            }
        }
    }

    void WalkToRandomPoint()
    {
        // Lấy một hướng ngẫu nhiên
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        // Kiểm tra xem điểm ngẫu nhiên đó có nằm trên mặt sàn (NavMesh) không
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
