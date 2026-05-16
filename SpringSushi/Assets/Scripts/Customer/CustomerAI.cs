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
        Eating,    // 食事中（EatingTime経過後に次のバッチへ）
        Satisfied, // 全注文完了（SatisfiedSlider=1）
        Leaving,   // 退場中
    }

    [Header("設定")]
    [Tooltip("何注文以内で必ず帰るか")]
    public int maxOrderBatches = 3;
    [Tooltip("一度に注文する個数（実行時にmin?maxでランダム決定）")]
    public int batchSize = 1; // Initialize時にランダム決定される
    [Tooltip("着席後に注文開始するまでの待機時間")]
    public float seatedWaitTime = 1.5f;

    // --- 内部状態 ---
    private CustomerState state = CustomerState.Spawned;
    private CustomerData data;
    private NavMeshAgent agent;
    private Transform targetSeat;

    // ① フィールド追加
    private CustomerAnimator customerAnimator;

    private CustomerOrderQueue orderQueue = new();
    private float basePatienceTime;
    private float patienceDecayRate;
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

        // ② Initialize() の Instantiate 直後を差し替え
        if (data.customerPrefab != null)
        {
            GameObject visual = Instantiate(data.customerPrefab, transform);
            customerAnimator = visual.GetComponentInChildren<CustomerAnimator>();
        }

        // CustomerDataのパラメータを反映
        maxOrderBatches = data.maxOrderBatches;
        batchSize = Random.Range(data.batchSizeMin, data.batchSizeMax + 1);
        basePatienceTime = data.basePatienceTime;
        patienceDecayRate = data.patienceDecayRate;

        // 注文キューを初期化
        orderQueue.Initialize(orders);
        maxPatience = basePatienceTime;
        currentPatience = maxPatience;

        Debug.Log($"<color=lime>[Entry]</color> {data.customerType} が来店（全{orders.Count}注文）");

        // ★ ここで「へいらっしゃい！」などのスポーン音を鳴らす
        if (SoundPlayer.Instance != null)
        {
            SoundPlayer.Instance.PlayVoice(SoundKeys.CustomerSpawn);
        }

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

    // ④ Leave() のAngry分岐にも通知（Patience切れで帰る場合）
    private void UpdateOrdering()
    {
        currentPatience -= Time.deltaTime;
        OnPatienceChanged?.Invoke(this);

        if (currentPatience <= 0f)
        {
            Debug.Log($"<color=red>[Angry]</color> {data.customerType} が怒って帰りました。");
            customerAnimator?.PlayAngry(); // ★ Angryアニメ → Leaveへ
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
            // 全注文完了・未完了問わずEatingへ
            SetState(CustomerState.Eating);
            Invoke(nameof(FinishEating), data != null ? data.eatTime : 1.5f);
        }

        return true;
    }

    private void FinishEating()
    {
        if (orderQueue.IsAllDelivered)
        {
            SetState(CustomerState.Satisfied);
            Invoke(nameof(Leave), 0.5f);
        }
        else
        {
            StartNextBatch();
        }
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

    // ③ SetState() にアニメーション通知を追加
    private void SetState(CustomerState newState)
    {
        state = newState;
        OnStateChanged?.Invoke(this);
        customerAnimator?.ApplyState(newState); // ★ 1行追加するだけ
        Debug.Log($"<color=cyan>[State]</color> " +
                  $"{(data != null ? data.customerType : gameObject.name)}: {newState}");
    }
}