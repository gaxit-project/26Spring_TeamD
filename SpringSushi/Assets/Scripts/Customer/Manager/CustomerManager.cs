using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    // ★ 追加：客が全員帰ったことをInGameSequenceManagerに通知するイベント
    public event System.Action OnAllCustomersExited;

    [Header("Prefab")]
    public GameObject customerPrefab;

    [Header("スポーン設定")]
    public List<Transform> customerSpawnPoints = new();
    public List<Transform> entryPoints = new();

    [Header("HUD")]
    [SerializeField] private CustomerHUD customerHUD;

    private List<CustomerMoodSO> moodVariations = new();
    private List<ScheduledCustomerEntry> scheduledEntries = new();

    private int totalCustomerCount;
    private float spawnInterval;
    private List<CustomerData> customerVariations = new();
    private List<SushiData> availableSushiList = new();

    private int spawnedCount = 0;
    private int exitedCount = 0;
    private bool isRunning = false;

    private readonly HashSet<Transform> reservedSeats = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        // ★ 自動入店化後もLT入力は保険として残すため、解除は引き続き必要
        SpawnerInputManager.OnAdmitCustomerPressed -= HandleAdmitInput;
    }

    public void StartStage(StageDataSO stageData)
    {
        totalCustomerCount = stageData.totalCustomerCount;
        spawnInterval = stageData.spawnInterval;
        customerVariations = stageData.customerVariations;
        availableSushiList = stageData.availableSushiList;
        moodVariations = stageData.moodVariations;
        scheduledEntries = stageData.scheduledEntries;

        spawnedCount = 0;
        exitedCount = 0;
        isRunning = true;
        reservedSeats.Clear();

        GameStateManager.Instance.OnStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameStateManager.GameState prev, GameStateManager.GameState next)
    {
        if (next != GameStateManager.GameState.Playing) return;
        GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;

        // ★ LT入力は保険として残す（押しても自動判定と同じ処理が走るだけ）
        SpawnerInputManager.OnAdmitCustomerPressed += HandleAdmitInput;

        StartCoroutine(CustomerEntryRoutine());
    }

    private void HandleAdmitInput()
    {
        if (!isRunning) return;
        TryAdmitFromWipe();
    }

    private IEnumerator CustomerEntryRoutine()
    {
        // 時間経過でWipe（待機列）への追加のみを行う
        while (isRunning && spawnedCount < totalCustomerCount)
        {
            yield return new WaitForSeconds(spawnInterval);
            EnqueueNextCustomer();
        }
    }

    /// <summary>
    /// 待機列に客データを追加する。
    /// ★ 追加直後に空席があれば自動で即入店を試みる。
    /// </summary>
    private void EnqueueNextCustomer()
    {
        if (spawnedCount >= totalCustomerCount) return;

        CustomerData data;
        CustomerMoodSO mood;

        if (spawnedCount < scheduledEntries.Count)
        {
            var entry = scheduledEntries[spawnedCount];
            data = entry.customerData != null
                ? entry.customerData
                : customerVariations[Random.Range(0, customerVariations.Count)];
            mood = entry.mood != null
                ? entry.mood
                : (moodVariations.Count > 0 ? moodVariations[Random.Range(0, moodVariations.Count)] : null);
        }
        else
        {
            data = customerVariations[Random.Range(0, customerVariations.Count)];
            mood = moodVariations.Count > 0
                ? moodVariations[Random.Range(0, moodVariations.Count)]
                : null;
        }

        var waitingData = new WaitingCustomerData
        {
            customerData = data,
            mood = mood,
            orders = GenerateOrders(data, mood),
        };

        WipeCanvas.Instance?.EnqueueCustomer(waitingData);
        spawnedCount++;

        Debug.Log($"<color=lime>[CustomerManager]</color> ワイプに追加（{spawnedCount}/{totalCustomerCount}）");

        // ★ 追加直後に空席があれば即座に入店させる
        //   （空席が無ければWipeに残って待機 → 後で席が空いた時に自動入店される）
        TryAdmitFromWipe();
    }

    /// <summary>
    /// 待機列の先頭を入店させる。
    /// 空席・待機客のいずれかが無い場合は何もしない（自動呼び出しのため通常ログは出さない）。
    /// </summary>
    public void TryAdmitFromWipe()
    {
        if (!(WipeCanvas.Instance?.HasWaiting ?? false)) return;

        Transform seat = GetEmptySeat();
        if (seat == null) return; // ★ 自動呼び出しが頻発するため警告ログは出さない

        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;

        reservedSeats.Add(seat);

        WaitingCustomerData waitingData = WipeCanvas.Instance.DequeueCustomer();
        if (waitingData == null) { reservedSeats.Remove(seat); return; }

        GameObject obj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        CustomerAI ai = obj.GetComponent<CustomerAI>();
        if (ai == null) { reservedSeats.Remove(seat); return; }

        ai.Initialize(waitingData.customerData, seat, waitingData.orders, waitingData.mood);
        customerHUD?.RegisterCustomer(ai);

        Debug.Log($"<color=cyan>[CustomerManager]</color> {seat.name} に入店");

        ai.OnStateChanged += OnCustomerStateChanged;
        ai.OnStateChanged += (customerAI) =>
        {
            if (customerAI.State == CustomerAI.CustomerState.Leaving)
            {
                reservedSeats.Remove(seat);

                // ★ 席が空いた直後に次の待機客を自動入店させる
                TryAdmitFromWipe();
            }
        };
    }

    private void OnCustomerStateChanged(CustomerAI ai)
    {
        if (ai.State != CustomerAI.CustomerState.Leaving &&
            ai.State != CustomerAI.CustomerState.Satisfied) return;

        ai.OnStateChanged -= OnCustomerStateChanged;
        exitedCount++;
        Debug.Log($"<color=cyan>[CustomerManager]</color> 退場 {exitedCount}/{totalCustomerCount}");
        CheckStageClear();
    }

    private void CheckStageClear()
    {
        if (spawnedCount >= totalCustomerCount && exitedCount >= totalCustomerCount)
        {
            Debug.Log("<color=gold>[CustomerManager]</color> 全客退場 → ステージクリア通知");

            SpawnerInputManager.OnAdmitCustomerPressed -= HandleAdmitInput;

            // ★ 変更：直接EnterResult()を呼ばず、イベントで通知するだけにする
            //    （実際のResult遷移・閉店演出はInGameSequenceManagerが担当する）
            OnAllCustomersExited?.Invoke();

            // GameStateManager.Instance?.EnterResult(); ← 削除
        }
    }

    private List<SushiData> GenerateOrders(CustomerData data, CustomerMoodSO mood)
    {
        var orders = new List<SushiData>();

        if (mood != null)
        {
            switch (mood.moodType)
            {
                case CustomerMoodSO.MoodType.SpecificSushi:
                    if (mood.fixedOrders.Count > 0)
                    {
                        int specificCount = Random.Range(1, data.maxTotalOrders + 1);
                        for (int i = 0; i < specificCount; i++)
                        {
                            int idx = Random.Range(0, mood.fixedOrders.Count);
                            orders.Add(mood.fixedOrders[idx]);
                        }
                    }
                    return orders;

                case CustomerMoodSO.MoodType.Hungry:
                    int hungryCount = Mathf.RoundToInt(
                        Random.Range(1, data.maxTotalOrders + 1) * mood.orderCountMultiplier);
                    hungryCount = Mathf.Max(1, hungryCount);
                    for (int i = 0; i < hungryCount; i++)
                        orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
                    return orders;
            }
        }

        int count = Random.Range(1, data.maxTotalOrders + 1);
        for (int i = 0; i < count; i++)
            orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
        return orders;
    }

    private Transform GetEmptySeat()
    {
        foreach (var seat in entryPoints)
        {
            if (reservedSeats.Contains(seat)) continue;
            return seat;
        }
        return null;
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