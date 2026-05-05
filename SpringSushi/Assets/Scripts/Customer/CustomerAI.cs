using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// お客さんのState管理とNavMesh移動を担当する。
///
/// State遷移：
/// Spawned → Walking → Seated → Ordering → Eating → Leaving
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState
    {
        Spawned,   // 生成直後
        Walking,   // EntryPointへ移動中
        Seated,    // 着席済み・注文前
        Ordering,  // 注文中（PatienceSlider動作）
        Satisfied, // 全注文完了（SatisfiedSlider=1）
        Leaving,   // 退場中
    }

    [Header("設定")]
    [Tooltip("何注文以内で必ず帰るか")]
    public int maxOrderBatches = 3;
    [Tooltip("一度に注文する数（バッチサイズ）")]
    public int batchSize = 1;
    [Tooltip("着席後に注文開始するまでの待機時間")]
    public float seatedWaitTime = 1.5f;

    [Header("Patience設定")]
    [Tooltip("最初のPatienceTime（秒）")]
    public float basePatienceTime = 10f;
    [Tooltip("注文が届くたびにPatienceTimeに掛ける係数（例：0.9）")]
    [Range(0.5f, 1f)]
    public float patienceDecayRate = 0.9f;

    // --- 内部状態 ---
    private CustomerState state = CustomerState.Spawned;
    private CustomerData data;
    private NavMeshAgent agent;
    private Transform targetSeat;

    private CustomerOrderQueue orderQueue = new();
    private float currentPatience;
    private float maxPatience;
    private int batchCount = 0;

    // --- イベント（HUDが購読する） ---
    public event System.Action<CustomerAI> OnStateChanged;
    public event System.Action<CustomerAI> OnOrderUpdated;
    public event System.Action<CustomerAI> OnPatienceChanged;

    // --- 公開プロパティ ---
    public CustomerState State => state;
    public CustomerOrderQueue OrderQueue => orderQueue;
    public float PatienceRate => maxPatience > 0 ? currentPatience / maxPatience : 0f;
    public float SatisfiedRate => orderQueue.SatisfiedRate;
    public Transform Seat => targetSeat;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(CustomerData newData, Transform seat, List<SushiData> orders)
    {
        data = newData;
        targetSeat = seat;

        // 見た目を生成
        if (data.customerPrefab != null)
            Instantiate(data.customerPrefab, transform);

        // CustomerDataのパラメータを反映
        maxOrderBatches = data.maxOrderBatches;
        batchSize = data.batchSize;
        basePatienceTime = data.basePatienceTime;
        patienceDecayRate = data.patienceDecayRate;

        // 注文キューを初期化
        orderQueue.Initialize(orders);
        maxPatience = basePatienceTime;
        currentPatience = maxPatience;

        Debug.Log($"<color=lime>[Entry]</color> {data.customerType} が来店（全{orders.Count}注文）");

        SetState(CustomerState.Walking);
        agent.SetDestination(targetSeat.position);
    }

    private void Update()
    {
        switch (state)
        {
            case CustomerState.Walking:
                UpdateWalking();
                break;
            case CustomerState.Ordering:
                UpdateOrdering();
                break;
        }
    }

    private void UpdateWalking()
    {
        if (agent.pathPending) return;
        if (agent.remainingDistance > agent.stoppingDistance) return;

        // 着席
        SetState(CustomerState.Seated);
        Invoke(nameof(StartNextBatch), seatedWaitTime);
    }

    private void UpdateOrdering()
    {
        currentPatience -= Time.deltaTime;
        OnPatienceChanged?.Invoke(this);

        if (currentPatience <= 0f)
        {
            Debug.Log($"<color=red>[Angry]</color> {data.customerType} が怒って帰りました。");
            Leave();
        }
    }

    private void StartNextBatch()
    {
        if (orderQueue.IsAllDelivered || batchCount >= maxOrderBatches)
        {
            Leave();
            return;
        }

        var batch = orderQueue.PullNextBatch(batchSize);
        batchCount++;

        // Patienceを更新（注文が届くたびに短くなる）
        maxPatience = basePatienceTime * Mathf.Pow(patienceDecayRate, batchCount - 1);
        currentPatience = maxPatience;

        SetState(CustomerState.Ordering);
        OnOrderUpdated?.Invoke(this);

        Debug.Log($"<color=orange>[Order]</color> {data.customerType} バッチ{batchCount}：{batch.Count}品注文");
    }

    /// <summary>
    /// 寿司が届いたときにSushiMovementから呼ばれる。
    /// </summary>
    public bool TryDeliver(SushiData sushiData)
    {
        if (state != CustomerState.Ordering) return false;

        bool delivered = orderQueue.TryDeliver(sushiData);
        if (!delivered) return false;

        OnOrderUpdated?.Invoke(this);

        // バッチ完了チェック
        if (orderQueue.IsBatchComplete)
        {
            if (orderQueue.IsAllDelivered)
            {
                SetState(CustomerState.Satisfied);
                Invoke(nameof(Leave), 1.5f);
            }
            else
            {
                StartNextBatch();
            }
        }

        return true;
    }

    private void Leave()
    {
        SetState(CustomerState.Leaving);
        // 退場アニメーション・移動などはここに追加
        Invoke(nameof(DestroySelf), 1f);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void SetState(CustomerState newState)
    {
        state = newState;
        OnStateChanged?.Invoke(this);
        Debug.Log($"<color=cyan>[State]</color> {(data != null ? data.customerType : gameObject.name)}: {newState}");
    }
}