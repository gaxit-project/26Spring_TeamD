using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CustomerOrderController))]
[RequireComponent(typeof(CustomerPatienceController))]
[RequireComponent(typeof(CustomerMovementController))]
[RequireComponent(typeof(CustomerOrderFlowService))]
[RequireComponent(typeof(CustomerPresentation))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState { Spawned, Walking, Seated, Ordering, Eating, Satisfied, Angry, Leaving }
    public enum OrderPhase { None, Waiting, PartiallyServed, BatchComplete }

    [Header("ê›íË")]
    public float seatedWaitTime = 1.5f;

    private CustomerData data;
    private Transform targetSeat;

    private CustomerStateMachine stateMachine;
    private CustomerMovementController movement;
    private CustomerOrderFlowService orderFlow;
    private CustomerPresentation presentation;
    private CustomerPatienceController patienceController;

    public event System.Action<CustomerAI> OnStateChanged;
    public event System.Action<CustomerAI> OnOrderPhaseChanged;
    public event System.Action<CustomerAI> OnOrderUpdated;
    public event System.Action<CustomerAI> OnPatienceChanged;
    public event System.Action<CustomerAI> OnAngryLeave;

    public CustomerState State => stateMachine.State;
    public OrderPhase Phase => stateMachine.Phase;
    public CustomerOrderQueue OrderQueue => orderFlow.OrderQueue;
    public float PatienceRate => patienceController.PatienceRate;
    public float SatisfiedRate => orderFlow.OrderQueue.SatisfiedRate;
    public Transform Seat => targetSeat;
    public CustomerData Data => data;

    private void Awake()
    {
        movement = GetComponent<CustomerMovementController>();
        orderFlow = GetComponent<CustomerOrderFlowService>();
        presentation = GetComponent<CustomerPresentation>();
        patienceController = GetComponent<CustomerPatienceController>();

        stateMachine = new CustomerStateMachine();
        stateMachine.OnStateChanged += _ => OnStateChanged?.Invoke(this);
        stateMachine.OnPhaseChanged += _ => OnOrderPhaseChanged?.Invoke(this);

        movement.Bind(stateMachine);
        presentation.Bind(stateMachine);
        orderFlow.Bind(stateMachine);

        orderFlow.OnOrderUpdated += () => OnOrderUpdated?.Invoke(this);
        orderFlow.OnGiveUp += Leave;
        orderFlow.OnAllSatisfied += () => Invoke(nameof(Leave), 0.5f);
    }

    public void Initialize(CustomerData newData, Transform seat, List<SushiData> orders, CustomerMoodSO mood = null)
    {
        data = newData;
        targetSeat = seat;

        if (data.customerPrefab != null)
        {
            var visual = Instantiate(data.customerPrefab, transform);
            presentation.SetVisual(visual.GetComponentInChildren<CustomerAnimator>());
        }

        orderFlow.Setup(data, orders, mood, this);
        presentation.PlaySpawnVoice();

        stateMachine.SetState(CustomerState.Walking);
        movement.MoveToSeat(targetSeat);
    }

    private void Update()
    {
        switch (State)
        {
            case CustomerState.Walking:
                if (movement.HasArrivedAtSeat())
                {
                    stateMachine.SetState(CustomerState.Seated);
                    Invoke(nameof(StartNextBatch), seatedWaitTime);
                }
                break;

            case CustomerState.Ordering:
                orderFlow.Tick(Time.deltaTime);
                break;
        }
    }

    private void StartNextBatch() => orderFlow.StartNextBatch();

    public bool TryDeliver(SushiData sushiData) => orderFlow.TryDeliver(sushiData);

    public void NotifyPatienceChanged() => OnPatienceChanged?.Invoke(this);

    public void NotifyAngry()
    {
        stateMachine.SetState(CustomerState.Angry);
        OnAngryLeave?.Invoke(this);
        presentation.PlayAngryVoice();
        Invoke(nameof(Leave), data != null ? data.angryTime : 3f);
    }

    public void Leave()
    {
        stateMachine.SetState(CustomerState.Leaving);
        Invoke(nameof(DestroySelf), 1f);
    }

    private void DestroySelf() => Destroy(gameObject);
}