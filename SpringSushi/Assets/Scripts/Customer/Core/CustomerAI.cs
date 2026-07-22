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

    /// <summary>CustomerAIに何が起きたかを表す種類。購読側はこれで分岐する。</summary>
    public enum CustomerChangeType { State, OrderPhase, Order, Patience, AngryLeave }

    private const string SeatedTimerKey = "seated";
    private const string AngryTimerKey = "angry";
    private const string LeaveTimerKey = "leave";
    private const string SatisfiedLeaveTimerKey = "satisfiedLeave";

    [Header("設定")]
    public float seatedWaitTime = 1.5f;

    private CustomerData data;
    private Transform targetSeat;

    private CustomerStateMachine stateMachine;
    private CustomerMovementController movement;
    private CustomerOrderFlowService orderFlow;
    private CustomerPresentation presentation;
    private CustomerPatienceController patienceController;
    private CustomerActionTimer timer;

    /// <summary>状態・注文・我慢度など、このCustomerAIに関する変化を一括通知するイベント。</summary>
    public event System.Action<CustomerAI, CustomerChangeType> OnChanged;

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
        timer = new CustomerActionTimer(this);

        stateMachine = new CustomerStateMachine();
        stateMachine.OnStateChanged += _ => RaiseChanged(CustomerChangeType.State);
        stateMachine.OnPhaseChanged += _ => RaiseChanged(CustomerChangeType.OrderPhase);

        movement.Bind(stateMachine);
        presentation.Bind(stateMachine);
        orderFlow.Bind(stateMachine);

        orderFlow.OnOrderUpdated += () => RaiseChanged(CustomerChangeType.Order);
        orderFlow.OnGiveUp += Leave;
        orderFlow.OnAllSatisfied += () => timer.Schedule(SatisfiedLeaveTimerKey, 0.5f, Leave);
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
                    timer.Schedule(SeatedTimerKey, seatedWaitTime, StartNextBatch);
                }
                break;

            case CustomerState.Ordering:
                orderFlow.Tick(Time.deltaTime);
                break;
        }
    }

    private void StartNextBatch() => orderFlow.StartNextBatch();

    public bool TryDeliver(SushiData sushiData) => orderFlow.TryDeliver(sushiData);

    public void NotifyPatienceChanged() => RaiseChanged(CustomerChangeType.Patience);

    public void NotifyAngry()
    {
        orderFlow.CancelTimers();

        stateMachine.SetState(CustomerState.Angry); // ここでState変化が通知される
        presentation.PlayAngryVoice();
        RaiseChanged(CustomerChangeType.AngryLeave); // 「怒って退店」専用の通知を追加で発火

        timer.Schedule(AngryTimerKey, data != null ? data.angryTime : 3f, Leave);
    }

    public void Leave()
    {
        stateMachine.SetState(CustomerState.Leaving);
        timer.Schedule(LeaveTimerKey, 1f, DestroySelf);
    }

    private void DestroySelf() => Destroy(gameObject);

    private void RaiseChanged(CustomerChangeType type) => OnChanged?.Invoke(this, type);

    private void OnDestroy() => timer.CancelAll();
}