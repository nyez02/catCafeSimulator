using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CatStaffAI : MonoBehaviour
{
    public StaffRole role = StaffRole.Waiter;
    public string staffName = "Mèo Phục Vụ";

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float waitAtTableTime = 2.0f;

    private NavMeshAgent agent;
    private Animator animator;
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int SitHash = Animator.StringToHash("Sit");

    private Vector3 initialPosition;
    private Coroutine staffRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.5f;

        animator = GetComponentInChildren<Animator>();
        initialPosition = transform.position;
    }

    private void Start()
    {
        staffRoutine = StartCoroutine(StaffRoutine());
    }

    private void OnDestroy()
    {
        if (staffRoutine != null) StopCoroutine(staffRoutine);
    }

    private IEnumerator StaffRoutine()
    {
        while (true)
        {
            // 1. Tìm bàn cần phục vụ hoặc tuần tra các vị trí trong quán
            Vector3 targetPos = GetNextTargetPosition();

            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(targetPos);
                if (animator != null) animator.SetBool(IsMovingHash, true);

                // Chờ di chuyển tới đích
                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    yield return new WaitForSeconds(0.2f);
                }

                if (animator != null) animator.SetBool(IsMovingHash, false);
            }

            // 2. Thực hiện công việc tại bàn / quầy
            PerformWorkEffect();
            yield return new WaitForSeconds(waitAtTableTime);
        }
    }

    private Vector3 GetNextTargetPosition()
    {
        if (TableManager.Instance != null)
        {
            TableSeat table = TableManager.Instance.GetAvailableTable();
            if (table != null)
            {
                return table.transform.position;
            }
        }

        // Nếu không có bàn trống, đi tuần quanh bán kính vị trí ban đầu
        Vector2 randomCircle = Random.insideUnitCircle * 4f;
        return initialPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }

    private void PerformWorkEffect()
    {
        if (CatVFXManager.Instance != null)
        {
            CatVFXManager.Instance.PlayCoinSparkle(transform.position + Vector3.up * 0.8f);
        }

        string actionIcon = role switch
        {
            StaffRole.Waiter => "☕ Đã dọn bàn!",
            StaffRole.Barista => "✨ Đã chuẩn bị cafe!",
            StaffRole.Cleaner => "🧹 Quán sáng bóng!",
            _ => "🐾 Chăm chỉ!"
        };

        if (UIManager.Instance != null && Random.value < 0.35f)
        {
            UIManager.Instance.ShowFloatingText(actionIcon, transform.position + Vector3.up * 1.5f, Color.cyan);
        }
    }
}
