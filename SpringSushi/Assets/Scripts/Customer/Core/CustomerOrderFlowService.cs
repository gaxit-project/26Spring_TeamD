using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 注文バッチの開始・配達受付・食事終了までの進行フローを制御する。
/// 実データ操作はCustomerOrderControllerに、我慢度はCustomerPatienceControllerに委譲する。
/// </summary>
[RequireComponent(typeof(CustomerOrderController))]
[RequireComponent(typeof(CustomerPatienceController))]
public class CustomerOrderFlowService : MonoBehaviour
{
    private const string EatingTimerKey = "eating";

    private int maxOrderBatches;
    private int batchSize;
    private CustomerData data;

    private CustomerOrderController orderController;
    private CustomerPatienceController patienceController;
    private CustomerStateMachine stateMachine;
    private CustomerActionTimer timer;

    public event System.Action OnOrderUpdated;
    public event System.Action OnAllSatisfied; // 全注文完了→退店してよい合図
    public event System.Action OnGiveUp;        // バッチ上限/完売→退店の合図

    public CustomerOrderQueue OrderQueue => orderController.OrderQueue;
    public int BatchCount => orderController.BatchCount;

    private void Awake()
    {
        orderController = GetComponent<CustomerOrderController>();
        patienceController = GetComponent<CustomerPatienceController>();
        timer = new CustomerActionTimer(this);
    }

    public void Bind(CustomerStateMachine sm) => stateMachine = sm;

    public void Setup(CustomerData newData, List<SushiData> orders, CustomerMoodSO mood, CustomerAI owner)
    {
        data = newData;
        maxOrderBatches = data.maxOrderBatches;
        batchSize = Random.Range(data.batchSizeMin, data.batchSizeMax + 1);

        orderController.Initialize(orders);
        patienceController.Initialize(data, owner, mood);
    }

    public void Tick(float deltaTime)
    {
        if (stateMachine.State == CustomerAI.CustomerState.Ordering)
            patienceController.Tick(deltaTime);
    }

    public void StartNextBatch()
    {
        if (orderController.OrderQueue.IsAllDelivered || orderController.BatchCount >= maxOrderBatches)
        {
            OnGiveUp?.Invoke();
            return;
        }

        orderController.PullNextBatch(batchSize);
        patienceController.ResetPatience(orderController.BatchCount);

        stateMachine.SetState(CustomerAI.CustomerState.Ordering);
        stateMachine.SetPhase(CustomerAI.OrderPhase.Waiting);
        OnOrderUpdated?.Invoke();
    }

    public bool TryDeliver(SushiData sushiData)
    {
        if (stateMachine.State != CustomerAI.CustomerState.Ordering) return false;
        if (!orderController.TryDeliver(sushiData)) return false;

        OnOrderUpdated?.Invoke();

        if (orderController.OrderQueue.IsBatchComplete)
        {
            stateMachine.SetPhase(CustomerAI.OrderPhase.BatchComplete);
            stateMachine.SetState(CustomerAI.CustomerState.Eating);
            timer.Schedule(EatingTimerKey, data != null ? data.eatTime : 1.5f, FinishEating);
        }
        else
        {
            stateMachine.SetPhase(CustomerAI.OrderPhase.PartiallyServed);
            stateMachine.SetState(CustomerAI.CustomerState.Eating);
            timer.Schedule(EatingTimerKey, data != null ? data.eatTime * 0.5f : 0.75f, FinishPartialEating);
        }

        return true;
    }

    // 同じキー(EatingTimerKey)への予約は上書きされるため、
    // 「本当にEating状態か」を再チェックするガード節は不要になった。
    private void FinishPartialEating()
    {
        patienceController.ResetPatience(orderController.BatchCount);
        stateMachine.SetPhase(CustomerAI.OrderPhase.Waiting);
        stateMachine.SetState(CustomerAI.CustomerState.Ordering);
        OnOrderUpdated?.Invoke();
    }

    private void FinishEating()
    {
        if (orderController.OrderQueue.IsAllDelivered)
        {
            stateMachine.SetState(CustomerAI.CustomerState.Satisfied);
            OnAllSatisfied?.Invoke();
        }
        else
        {
            StartNextBatch();
        }
    }

    /// <summary>
    /// 外部要因(Angryへの遷移など)でフローを中断させたい場合に、
    /// 進行中のタイマー(FinishEating/FinishPartialEatingの予約)をキャンセルする。
    /// </summary>
    public void CancelTimers() => timer.CancelAll();

    private void OnDestroy() => timer.CancelAll();
}