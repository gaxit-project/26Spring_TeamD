using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    public event System.Action OnAllCustomersExited;

    [Header("Prefab")]
    public GameObject customerPrefab;

    [Header("スポーン設定")]
    public List<Transform> customerSpawnPoints = new();
    public List<Transform> entryPoints = new();

    [Header("HUD")]
    [SerializeField] private CustomerHUD customerHUD;

    private CustomerSpawnScheduler spawnScheduler;
    private readonly OrderGenerator orderGenerator = new();
    private readonly StageProgressTracker progressTracker = new();
    private SeatAllocator seatAllocator;

    private int initialWaitingCustomerCount;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        seatAllocator = new SeatAllocator(entryPoints);
        progressTracker.OnStageClear += HandleStageClear;

        spawnScheduler = gameObject.AddComponent<CustomerSpawnScheduler>();
        spawnScheduler.OnWaveEnqueued += HandleWaveEnqueued;
    }

    private void OnDestroy()
    {
        SpawnerInputManager.OnAdmitCustomerPressed -= HandleAdmitInput;
    }

    public void StartStage(StageDataSO stageData)
    {
        progressTracker.Initialize(stageData.totalCustomerCount);
        orderGenerator.Initialize(stageData.availableSushiList);
        spawnScheduler.Initialize(stageData, orderGenerator, progressTracker);

        initialWaitingCustomerCount = Mathf.Clamp(stageData.initialWaitingCustomerCount, 0, stageData.totalCustomerCount);
        seatAllocator.Clear();

        GameStateManager.Instance.OnStateChanged += OnGameStateChanged;

        /*for (int i = 0; i < initialWaitingCustomerCount; i++)
        {
            spawnScheduler.PrefillCustomer();
        }*/
        spawnScheduler.PrefillCustomers(initialWaitingCustomerCount);
    }

    private void OnGameStateChanged(GameStateManager.GameState prev, GameStateManager.GameState next)
    {
        if (next != GameStateManager.GameState.Playing) return;
        GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;

        SpawnerInputManager.OnAdmitCustomerPressed += HandleAdmitInput;

        // 開店前に並べておいたN人をまとめて雪崩式に入店させる
        TryAdmitBurstFromWipe(initialWaitingCustomerCount);

        spawnScheduler.BeginRunning();
    }

    private void HandleAdmitInput() => TryAdmitFromWipe();

    /// <summary>
    /// 1つの波の客がワイプに追加されるたびに呼ばれる。
    /// waveSizeが2以上ならグループ入店、1なら通常の個別入店を試みる。
    /// </summary>
    private void HandleWaveEnqueued(int waveSize)
    {
        if (waveSize > 1)
            TryAdmitBurstFromWipe(waveSize);
        else
            TryAdmitFromWipe();
    }

    /// <summary>
    /// 待機列の先頭を1人だけ入店させる。空席・待機客のいずれかが無い場合は何もしない。
    /// </summary>
    public void TryAdmitFromWipe()
    {
        if (!(WipeCanvas.Instance?.HasWaiting ?? false)) return;

        Transform seat = seatAllocator.TryReserveSeat();
        if (seat == null) return;

        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) { seatAllocator.ReleaseSeat(seat); return; }

        WipeCanvas.Instance.BeginAdmit(waitingData =>
        {
            if (waitingData == null) { seatAllocator.ReleaseSeat(seat); return; }
            SpawnCustomer(waitingData, seat, spawnPoint);
        });
    }

    /// <summary>
    /// 待機列の先頭からcount人を、まとめて雪崩式に入店させる(バースト入店)。
    /// 開店直後や、波(wave)のcountが2以上のタイミングで使う。
    /// 空席・待機客が足りない場合は、可能な人数分だけ入店させる。
    /// </summary>
    public void TryAdmitBurstFromWipe(int count)
    {
        if (WipeCanvas.Instance == null || count <= 0) return;

        int available = Mathf.Min(count, WipeCanvas.Instance.WaitingCount);
        if (available <= 0) return;

        var reserved = new List<(Transform seat, Transform spawnPoint)>();

        for (int i = 0; i < available; i++)
        {
            Transform seat = seatAllocator.TryReserveSeat();
            if (seat == null) break; // 空席が尽きたらそこで打ち切る

            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null) { seatAllocator.ReleaseSeat(seat); break; }

            reserved.Add((seat, spawnPoint));
        }

        if (reserved.Count == 0) return;

        var callbacks = new List<System.Action<WaitingCustomerData>>();
        foreach (var (seat, spawnPoint) in reserved)
        {
            callbacks.Add(waitingData =>
            {
                if (waitingData == null) { seatAllocator.ReleaseSeat(seat); return; }
                SpawnCustomer(waitingData, seat, spawnPoint);
            });
        }

        WipeCanvas.Instance.BeginAdmitBurst(reserved.Count, callbacks);
    }

    /// <summary>
    /// 予約済みの座席・スポーン地点に客を実際に生成し、HUD登録・イベント購読までを行う。
    /// TryAdmitFromWipe / TryAdmitBurstFromWipe の共通処理。
    /// </summary>
    private void SpawnCustomer(WaitingCustomerData waitingData, Transform seat, Transform spawnPoint)
    {
        GameObject obj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        CustomerAI ai = obj.GetComponent<CustomerAI>();
        if (ai == null) { seatAllocator.ReleaseSeat(seat); return; }

        ai.Initialize(waitingData.customerData, seat, waitingData.orders, waitingData.mood);
        customerHUD?.RegisterCustomer(ai);

        Debug.Log($"<color=cyan>[CustomerManager]</color> {seat.name} に入店");

        ai.OnChanged += HandleCustomerChanged;

        void HandleSeatRelease(CustomerAI customerAI, CustomerAI.CustomerChangeType type)
        {
            if (type != CustomerAI.CustomerChangeType.State) return;
            if (customerAI.State != CustomerAI.CustomerState.Leaving) return;

            seatAllocator.ReleaseSeat(seat);
            TryAdmitFromWipe();
            customerAI.OnChanged -= HandleSeatRelease;
        }

        ai.OnChanged += HandleSeatRelease;
    }

    private void HandleCustomerChanged(CustomerAI ai, CustomerAI.CustomerChangeType type)
    {
        if (type != CustomerAI.CustomerChangeType.State) return;
        if (ai.State != CustomerAI.CustomerState.Leaving &&
            ai.State != CustomerAI.CustomerState.Satisfied) return;

        ai.OnChanged -= HandleCustomerChanged;
        progressTracker.NotifyExited();
        Debug.Log($"<color=cyan>[CustomerManager]</color> 退場 {progressTracker.ExitedCount}/{progressTracker.TotalCustomerCount}");
    }

    private void HandleStageClear()
    {
        Debug.Log("<color=gold>[CustomerManager]</color> 全客退場 → ステージクリア通知");
        SpawnerInputManager.OnAdmitCustomerPressed -= HandleAdmitInput;
        spawnScheduler.Stop();
        OnAllCustomersExited?.Invoke();
    }

    private Transform GetRandomSpawnPoint()
    {
        if (customerSpawnPoints.Count == 0) return null;
        return customerSpawnPoints[Random.Range(0, customerSpawnPoints.Count)];
    }

#if UNITY_EDITOR
    public void AddEntryPoint(Transform t) => entryPoints.Add(t);
#endif
}