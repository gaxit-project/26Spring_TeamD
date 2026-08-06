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

    /// <summary>新しいバッチの注文が始まった瞬間に発火する。机の上をリセットする合図。</summary>
    public event System.Action OnBatchStarted;

    /// <summary>寿司が1つ配達されるたびに発火する。机の上にその1皿を追加する合図。</summary>
    public event System.Action<CustomerOrder> OnItemServed;

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
        OnBatchStarted?.Invoke(); // 新しいバッチが始まったので、机の上をリセットする
    }

    public bool TryDeliver(SushiData sushiData)
    {
        if (stateMachine.State != CustomerAI.CustomerState.Ordering) return false;
        if (!orderQueue.TryDeliver(sushiData, out var deliveredOrder)) return false;

        OnOrderUpdated?.Invoke();
        OnItemServed?.Invoke(deliveredOrder); // 配達された1皿を机に追加する

        if (orderQueue.IsBatchComplete)
        {
            stateMachine.SetPhase(CustomerAI.OrderPhase.BatchComplete);
            stateMachine.SetState(CustomerAI.CustomerState.Eating);
            float duration = data != null ? data.eatTime : 1.5f;
            timer.Schedule(EatingTimerKey, duration, FinishEating);
        }
        else
        {
            stateMachine.SetPhase(CustomerAI.OrderPhase.PartiallyServed);
            stateMachine.SetState(CustomerAI.CustomerState.Eating);
            float duration = data != null ? data.eatTime * 0.5f : 0.75f;
            timer.Schedule(EatingTimerKey, duration, FinishPartialEating);
        }

        return true;
    }

    private void FinishPartialEating()
    {
        // 同じバッチの続き(次の寿司を待つだけ)なので、机の上はクリアしない
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
            StartNextBatch(); // ここでOnBatchStartedが発火し、机の上がリセットされる
        }
    }

    /// <summary>
    /// 外部要因(Angryへの遷移など)でフローを中断させたい場合に、
    /// 進行中のタイマー(FinishEating/FinishPartialEatingの予約)をキャンセルする。
    /// </summary>
    public void CancelTimers() => timer.CancelAll();

    private void OnDestroy() => timer.CancelAll();
}