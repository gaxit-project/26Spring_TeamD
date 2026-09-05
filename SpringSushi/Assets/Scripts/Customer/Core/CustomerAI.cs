using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CustomerPatienceController))]
[RequireComponent(typeof(CustomerMovementController))]
[RequireComponent(typeof(CustomerOrderFlowService))]
[RequireComponent(typeof(CustomerPresentation))]
[RequireComponent(typeof(CustomerEatingDisplay))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState { Spawned, Walking, Seated, Ordering, Eating, Satisfied, Angry, Leaving }
    public enum OrderPhase { None, Waiting, PartiallyServed, BatchComplete }

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
    private CustomerEatingDisplay eatingDisplay;
    private CustomerActionTimer timer;

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
        eatingDisplay = GetComponent<CustomerEatingDisplay>();
        timer = new CustomerActionTimer(this);

        stateMachine = new CustomerStateMachine();
        stateMachine.OnStateChanged += _ => RaiseChanged(CustomerChangeType.State);
        stateMachine.OnPhaseChanged += _ => RaiseChanged(CustomerChangeType.OrderPhase);

        movement.Bind(stateMachine);
        presentation.Bind(stateMachine);
        orderFlow.Bind(stateMachine);
        eatingDisplay.Bind(stateMachine);

        orderFlow.OnOrderUpdated += () => RaiseChanged(CustomerChangeType.Order);
        orderFlow.OnGiveUp += Leave;

        // ★ 変更: 専用メソッドを登録
        orderFlow.OnAllSatisfied += HandleAllSatisfied;

        orderFlow.OnBatchStarted += eatingDisplay.ClearForNewBatch;
        orderFlow.OnItemServed += eatingDisplay.AddServedItem;
    }

    public void Initialize(CustomerData newData, Transform seat, List<SushiData> orders, CustomerMoodSO mood = null)
    {
        data = newData;
        targetSeat = seat;

        var plateAnchor = seat.GetComponentInChildren<SeatPlateAnchor>();
        eatingDisplay.SetPlateSlots(plateAnchor != null ? plateAnchor.plateSlots : null);

        if (data.customerPrefab != null)
        {
            var visual = Instantiate(data.customerPrefab, transform);
            presentation.SetVisual(visual.GetComponentInChildren<CustomerAnimator>(), seat);
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

    /// <summary>
    /// ★ 全ての注文が満足したときの処理
    /// </summary>
    private void HandleAllSatisfied()
    {
        // 状態をSatisfiedに変更（CustomerAnimatorのSatisfiedアニメーションと連動）
        stateMachine.SetState(CustomerState.Satisfied);

        // Presentation経由でボイスを再生
        presentation.PlaySatisfiedVoice();

        // 0.5秒後に退店
        timer.Schedule(SatisfiedLeaveTimerKey, 0.5f, Leave);
    }

    public void NotifyAngry()
    {
        orderFlow.CancelTimers();

        stateMachine.SetState(CustomerState.Angry);
        presentation.PlayAngryVoice();
        RaiseChanged(CustomerChangeType.AngryLeave);

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