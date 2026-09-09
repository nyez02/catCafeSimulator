using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum CatState
{
    Idle,
    Wander,
    Sleeping,
    Eating,
    Greeting
}

[RequireComponent(typeof(NavMeshAgent))]
public class CatAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Cat Identity")]
    public string catName = "Miu Miu";
    public CatData catData;

    [Header("Wander Settings")]
    public float walkRadius = 8f;
    public float waitTime = 3f;
    private float stateTimer;

    [Header("Needs & Stats")]
    [Range(0, 100)] public float hunger = 100f;
    [Range(0, 100)] public float happiness = 80f;
    public float hungerDecayRate = 0.8f; // Mất 0.8 điểm đói mỗi giây
    public float happinessDecayRate = 0.3f;

    [Header("Current State")]
    public CatState currentState = CatState.Idle;

    // Animation state names in LittleCat.controller
    private const string ANIM_IDLE = "02_Idle_Cat_Copy";
    private const string ANIM_WALK = "03_Walk_Cat_Copy";
    private const string ANIM_EAT = "07_Eat_Cat_Copy";
    private const string ANIM_SLEEP = "08_Sleep01_Cat_Copy";
    private const string ANIM_GREET = "06_Greeting_Cat_Copy";

    private FoodBowl targetBowl;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        agent.speed = catData != null ? catData.baseMovementSpeed : 2f;
        stateTimer = waitTime;
        PlayAnimation(ANIM_IDLE);

        if (CatManager.Instance != null)
        {
            CatManager.Instance.RegisterCat(this);
        }
    }

    private void OnDestroy()
    {
        if (CatManager.Instance != null)
        {
            CatManager.Instance.UnregisterCat(this);
        }
    }

    private void Update()
    {
        UpdateNeeds();

        switch (currentState)
        {
            case CatState.Idle:
                HandleIdle();
                break;
            case CatState.Wander:
                HandleWander();
                break;
            case CatState.Eating:
                HandleEating();
                break;
            case CatState.Sleeping:
                HandleSleeping();
                break;
            case CatState.Greeting:
                // Greeting handled in coroutine
                break;
        }
    }

    private float needsCheckTimer = 0f;

    private void UpdateNeeds()
    {
        hunger = Mathf.Max(0, hunger - hungerDecayRate * Time.deltaTime);
        happiness = Mathf.Max(0, happiness - happinessDecayRate * Time.deltaTime);

        // Tối ưu CPU: Chỉ kiểm tra tìm bát thức ăn 3 lần mỗi giây thay vì 60 lần/giây
        needsCheckTimer += Time.deltaTime;
        if (needsCheckTimer >= 0.3f)
        {
            needsCheckTimer = 0f;

            if (hunger < 30f && (currentState == CatState.Idle || currentState == CatState.Wander))
            {
                FoodBowl bowl = FoodBowl.GetNearestAvailableBowl(transform.position);
                if (bowl != null)
                {
                    targetBowl = bowl;
                    agent.SetDestination(bowl.transform.position);
                    currentState = CatState.Wander;
                    PlayAnimation(ANIM_WALK);
                }
            }
        }
    }

    private void HandleIdle()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= waitTime)
        {
            DecideNextAction();
        }
    }

    private void HandleWander()
    {
        // Kiểm tra xem đã đến đích chưa
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (targetBowl != null && Vector3.Distance(transform.position, targetBowl.transform.position) < 1.5f)
            {
                // Bắt đầu ăn
                StartCoroutine(EatRoutine());
                return;
            }

            // Đến điểm đi dạo xong thì chuyển sang Idle hoặc Ngủ
            currentState = CatState.Idle;
            PlayAnimation(ANIM_IDLE);
            stateTimer = 0f;
            waitTime = Random.Range(2f, 5f);
        }
    }

    private void DecideNextAction()
    {
        stateTimer = 0f;
        float roll = Random.value;

        if (roll < 0.20f) // 20% cơ hội nằm ngủ
        {
            StartCoroutine(SleepRoutine(Random.Range(6f, 12f)));
        }
        else if (roll < 0.50f) // 30% cơ hội đi thăm bàn khách
        {
            TableSeat availableTable = TableManager.Instance != null ? TableManager.Instance.GetAvailableTable() : null;
            if (availableTable != null)
            {
                agent.SetDestination(availableTable.transform.position);
                currentState = CatState.Wander;
                PlayAnimation(ANIM_WALK);
            }
            else
            {
                WalkToRandomPoint();
            }
        }
        else // 50% đi dạo ngẫu nhiên
        {
            WalkToRandomPoint();
        }
    }

    private void WalkToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            currentState = CatState.Wander;
            PlayAnimation(ANIM_WALK);
        }
        else
        {
            currentState = CatState.Idle;
            PlayAnimation(ANIM_IDLE);
        }
    }

    private IEnumerator EatRoutine()
    {
        currentState = CatState.Eating;
        PlayAnimation(ANIM_EAT);

        yield return new WaitForSeconds(4f);

        if (targetBowl != null)
        {
            float eaten = targetBowl.Eat(40f);
            hunger = Mathf.Min(100f, hunger + eaten * 2f);
            targetBowl = null;
        }

        currentState = CatState.Idle;
        PlayAnimation(ANIM_IDLE);
        stateTimer = 0f;
    }

    private IEnumerator SleepRoutine(float duration)
    {
        currentState = CatState.Sleeping;
        PlayAnimation(ANIM_SLEEP);

        yield return new WaitForSeconds(duration);

        currentState = CatState.Idle;
        PlayAnimation(ANIM_IDLE);
        stateTimer = 0f;
    }

    /// <summary>
    /// Người chơi bấm vuốt ve mèo
    /// </summary>
    public void Pet()
    {
        happiness = Mathf.Min(100f, happiness + 25f);
        StartCoroutine(PetRoutine());

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCatMeow();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFloatingText("❤️ +25", transform.position + Vector3.up * 1.2f, Color.magenta);
        }

        if (CatVFXManager.Instance != null)
        {
            CatVFXManager.Instance.SpawnHeartBurst(transform.position + Vector3.up * 0.8f);
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnCatPetted();
        }
    }

    private IEnumerator PetRoutine()
    {
        CatState prevState = currentState;
        currentState = CatState.Greeting;
        agent.ResetPath();
        PlayAnimation(ANIM_GREET);

        yield return new WaitForSeconds(2.5f);

        currentState = CatState.Idle;
        PlayAnimation(ANIM_IDLE);
    }

    private void OnMouseDown()
    {
        Pet();
    }

    private void PlayAnimation(string stateName)
    {
        if (animator != null && animator.HasState(0, Animator.StringToHash(stateName)))
        {
            animator.CrossFade(stateName, 0.2f);
        }
    }
}
