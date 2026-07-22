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
        spawnScheduler.OnCustomerEnqueued += HandleCustomerEnqueued;
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

        for (int i = 0; i < initialWaitingCustomerCount; i++)
        {
            spawnScheduler.EnqueueNextCustomer(tryAdmit: false);
        }
    }

    private void OnGameStateChanged(GameStateManager.GameState prev, GameStateManager.GameState next)
    {
        if (next != GameStateManager.GameState.Playing) return;
        GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;

        SpawnerInputManager.OnAdmitCustomerPressed += HandleAdmitInput;

        for (int i = 0; i < initialWaitingCustomerCount; i++)
        {
            TryAdmitFromWipe();
        }

        spawnScheduler.BeginRunning();
    }

    private void HandleAdmitInput() => TryAdmitFromWipe();

    private void HandleCustomerEnqueued(bool tryAdmit)
    {
        if (tryAdmit) TryAdmitFromWipe();
    }

    /// <summary>
    /// 待機列の先頭を入店させる。空席・待機客のいずれかが無い場合は何もしない。
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

            GameObject obj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
            CustomerAI ai = obj.GetComponent<CustomerAI>();
            if (ai == null) { seatAllocator.ReleaseSeat(seat); return; }

            ai.Initialize(waitingData.customerData, seat, waitingData.orders, waitingData.mood);
            customerHUD?.RegisterCustomer(ai);

            Debug.Log($"<color=cyan>[CustomerManager]</color> {seat.name} に入店");

            ai.OnChanged += HandleCustomerChanged;
            ai.OnChanged += (customerAI, type) =>
            {
                if (type != CustomerAI.CustomerChangeType.State) return;
                if (customerAI.State == CustomerAI.CustomerState.Leaving)
                {
                    seatAllocator.ReleaseSeat(seat);
                    TryAdmitFromWipe();
                }
            };
        });
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