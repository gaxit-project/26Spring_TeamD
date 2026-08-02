using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 注文バッチの開始・配達受付・食事終了までの進行フローを制御する。
/// 注文キューの実データ(CustomerOrderQueue)はこのクラスが直接保持する。
/// 我慢度はCustomerPatienceControllerに委譲する。
/// </summary>
[RequireComponent(typeof(CustomerPatienceController))]
public class CustomerOrderFlowService : MonoBehaviour
{
    private const string EatingTimerKey = "eating";

    private int maxOrderBatches;
    private int batchSize;
    private CustomerData data;

    private readonly CustomerOrderQueue orderQueue = new();
    private int batchCount = 0;

    private CustomerPatienceController patienceController;
    private CustomerStateMachine stateMachine;
    private CustomerActionTimer timer;

    public event System.Action OnOrderUpdated;
    public event System.Action OnAllSatisfied; // 全注文完了→退店してよい合図
    public event System.Action OnGiveUp;        // バッチ上限/完売→退店の合図

    public CustomerOrderQueue OrderQueue => orderQueue;
    public int BatchCount => batchCount;

    private void Awake()
    {
        patienceController = GetComponent<CustomerPatienceController>();
        timer = new CustomerActionTimer(this);
    }

    public void Bind(CustomerStateMachine sm) => stateMachine = sm;

    public void Setup(CustomerData newData, List<SushiData> orders, CustomerMoodSO mood, CustomerAI owner)
    {
        data = newData;
        maxOrderBatches = data.maxOrderBatches;
        batchSize = Random.Range(data.batchSizeMin, data.batchSizeMax + 1);

        batchCount = 0;
        orderQueue.Initialize(orders);
        patienceController.Initialize(data, owner, mood);
    }

    public void Tick(float deltaTime)
    {
        if (stateMachine.State == CustomerAI.CustomerState.Ordering)
            patienceController.Tick(deltaTime);
    }

    public void StartNextBatch()
    {
        if (orderQueue.IsAllDelivered || batchCount >= maxOrderBatches)
        {
            OnGiveUp?.Invoke();
            return;
        }

        batchCount++;
        orderQueue.PullNextBatch(batchSize);
        patienceController.ResetPatience(batchCount);

        stateMachine.SetState(CustomerAI.CustomerState.Ordering);
        stateMachine.SetPhase(CustomerAI.OrderPhase.Waiting);
        OnOrderUpdated?.Invoke();
    }

    public bool TryDeliver(SushiData sushiData)
    {
        if (stateMachine.State != CustomerAI.CustomerState.Ordering) return false;
        if (!orderQueue.TryDeliver(sushiData)) return false;

        OnOrderUpdated?.Invoke();

        if (orderQueue.IsBatchComplete)
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

    private void FinishPartialEating()
    {
        patienceController.ResetPatience(batchCount);
        stateMachine.SetPhase(CustomerAI.OrderPhase.Waiting);
        stateMachine.SetState(CustomerAI.CustomerState.Ordering);
        OnOrderUpdated?.Invoke();
    }

    private void FinishEating()
    {
        if (orderQueue.IsAllDelivered)
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