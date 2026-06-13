using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CustomerOrderController))]
[RequireComponent(typeof(CustomerPatienceController))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState
    {
        Spawned,
        Walking,
        Seated,
        Ordering,
        Eating,
        Satisfied,
        Angry,     // Åö í«â¡
        Leaving,
    }

    public enum OrderPhase
    {
        None,
        Waiting,
        PartiallyServed,
        BatchComplete,
    }

    [Header("ê›íË")]
    public int maxOrderBatches = 3;
    public int batchSize = 1;
    public float seatedWaitTime = 1.5f;

    private CustomerState state = CustomerState.Spawned;
    private OrderPhase orderPhase = OrderPhase.None;
    private CustomerData data;
    private NavMeshAgent agent;
    private NavMeshObstacle navObstacle;
    private Transform targetSeat;
    private CustomerAnimator customerAnimator;

    private CustomerOrderController orderController;
    private CustomerPatienceController patienceController;

    public event System.Action<CustomerAI> OnStateChanged;
    public event System.Action<CustomerAI> OnOrderPhaseChanged;
    public event System.Action<CustomerAI> OnOrderUpdated;
    public event System.Action<CustomerAI> OnPatienceChanged;
    public event System.Action<CustomerAI> OnAngryLeave;

    public CustomerState State => state;
    public OrderPhase Phase => orderPhase;
    public CustomerOrderQueue OrderQueue => orderController.OrderQueue;
    public float PatienceRate => patienceController.PatienceRate;
    public float SatisfiedRate => orderController.OrderQueue.SatisfiedRate;
    public Transform Seat => targetSeat;
    public CustomerData Data => data;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        navObstacle = GetComponent<NavMeshObstacle>();
        orderController = GetComponent<CustomerOrderController>();
        patienceController = GetComponent<CustomerPatienceController>();

        if (navObstacle != null)
        {
            navObstacle.enabled = false;
            navObstacle.carving = true;
        }
    }

    public void Initialize(CustomerData newData, Transform seat,
                       List<SushiData> orders, CustomerMoodSO mood = null)
    {
        data = newData;
        targetSeat = seat;

        if (data.customerPrefab != null)
        {
            var visual = Instantiate(data.customerPrefab, transform);
            customerAnimator = visual.GetComponentInChildren<CustomerAnimator>();
        }

        maxOrderBatches = data.maxOrderBatches;
        batchSize = Random.Range(data.batchSizeMin, data.batchSizeMax + 1);

        orderController.Initialize(orders);
        patienceController.Initialize(data, this, mood); // Åö mood ÇìnÇ∑

        if (SoundPlayer.Instance != null)
            SoundPlayer.Instance.PlayVoice(SoundKeys.CustomerSpawn);
            SoundPlayer.Instance.PlayVoice(SoundKeys.EntryVoice);

        SetState(CustomerState.Walking);
        agent.SetDestination(targetSeat.position);
    }

    private void Update()
    {
        switch (state)
        {
            case CustomerState.Walking: UpdateWalking(); break;
            case CustomerState.Ordering: patienceController.Tick(Time.deltaTime); break;
        }
    }

    private void UpdateWalking()
    {
        if (agent.pathPending) return;
        if (agent.remainingDistance > agent.stoppingDistance) return;
        SetState(CustomerState.Seated);
        Invoke(nameof(StartNextBatch), seatedWaitTime);
    }

    // ÑüÑüÑü íçï∂ÉtÉçÅ[ ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü

    public void StartNextBatch()
    {
        if (orderController.OrderQueue.IsAllDelivered || orderController.BatchCount >= maxOrderBatches)
        {
            Leave();
            return;
        }

        orderController.PullNextBatch(batchSize);
        patienceController.ResetPatience(orderController.BatchCount);

        SetState(CustomerState.Ordering);
        SetOrderPhase(OrderPhase.Waiting);
        OnOrderUpdated?.Invoke(this);
    }

    public bool TryDeliver(SushiData sushiData)
    {
        if (state != CustomerState.Ordering) return false;

        bool delivered = orderController.TryDeliver(sushiData);
        if (!delivered) return false;

        OnOrderUpdated?.Invoke(this);

        if (orderController.OrderQueue.IsBatchComplete)
        {
            SetOrderPhase(OrderPhase.BatchComplete);
            SetState(CustomerState.Eating);
            Invoke(nameof(FinishEating), data != null ? data.eatTime : 1.5f);
        }
        else
        {
            SetOrderPhase(OrderPhase.PartiallyServed);
            SetState(CustomerState.Eating);
            Invoke(nameof(FinishPartialEating), data != null ? data.eatTime * 0.5f : 0.75f);
        }

        return true;
    }

    private void FinishPartialEating()
    {
        if (state != CustomerState.Eating) return;
        patienceController.ResetPatience(orderController.BatchCount);
        SetOrderPhase(OrderPhase.Waiting);
        SetState(CustomerState.Ordering);
        OnOrderUpdated?.Invoke(this);
    }

    private void FinishEating()
    {
        if (state != CustomerState.Eating) return;
        if (orderController.OrderQueue.IsAllDelivered)
        {
            SetState(CustomerState.Satisfied);
            Invoke(nameof(Leave), 0.5f);
        }
        else
        {
            StartNextBatch();
        }
    }

    // ÑüÑüÑü Patience ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü

    public void NotifyPatienceChanged() => OnPatienceChanged?.Invoke(this);

    public void NotifyAngry()
    {
        SetState(CustomerState.Angry);          // Åö Angry StateÇ…ëJà⁄
        OnAngryLeave?.Invoke(this);
        if (SoundPlayer.Instance != null)
        {
            if(Random.Range(0,2) == 0)
            {
                SoundPlayer.Instance.PlayVoice(SoundKeys.CustomerAngry);
            }
            else
            {
                SoundPlayer.Instance.PlayVoice(SoundKeys.CustomerAngry2);
            }
            
        }
        Invoke(nameof(Leave), data != null ? data.angryTime : 3f); // Åö angryTimeå„Ç…ëﬁèÍ
    }

    // ÑüÑüÑü State / Phase ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü

    public void Leave()
    {
        SetState(CustomerState.Leaving);
        Invoke(nameof(DestroySelf), 1f);
    }

    private void DestroySelf() => Destroy(gameObject);

    private void SetState(CustomerState newState)
    {
        state = newState;

        switch (newState)
        {
            case CustomerState.Seated:
            case CustomerState.Ordering:
            case CustomerState.Eating:
            case CustomerState.Satisfied:
            case CustomerState.Angry:      // Åö í«â¡
                agent.enabled = false;
                if (navObstacle != null) navObstacle.enabled = true;
                break;

            case CustomerState.Leaving:
                if (navObstacle != null) navObstacle.enabled = false;
                agent.enabled = true;
                break;
        }

        OnStateChanged?.Invoke(this);
        customerAnimator?.ApplyState(newState);
        Debug.Log($"<color=cyan>[State]</color> {gameObject.name}: {newState}");
    }

    private void SetOrderPhase(OrderPhase newPhase)
    {
        orderPhase = newPhase;
        OnOrderPhaseChanged?.Invoke(this);
        Debug.Log($"<color=orange>[Phase]</color> {gameObject.name}: {newPhase}");
    }
}