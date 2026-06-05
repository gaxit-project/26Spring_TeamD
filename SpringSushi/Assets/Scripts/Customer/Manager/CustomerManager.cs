using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Prefab")]
    public GameObject customerPrefab;

    [Header("スポーン設定")]
    public List<Transform> customerSpawnPoints = new();
    public List<Transform> entryPoints = new();

    [Header("HUD")]
    [SerializeField] private CustomerHUD customerHUD;

    [Header("Mood候補（ワイプの吹き出し用）")]
    [SerializeField] private List<CustomerMoodSO> availableMoods = new();

    // --- StageDataSO から設定される ---
    private int totalCustomerCount;
    private float spawnInterval;
    private List<CustomerData> customerVariations = new();
    private List<SushiData> availableSushiList = new();

    // --- カウンタ ---
    private int spawnedCount = 0;
    private int exitedCount = 0;
    private bool isRunning = false;

    private readonly HashSet<Transform> reservedSeats = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartStage(StageDataSO stageData)
    {
        totalCustomerCount = stageData.totalCustomerCount;
        spawnInterval = stageData.spawnInterval;
        customerVariations = stageData.customerVariations;
        availableSushiList = stageData.availableSushiList;

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
        StartCoroutine(CustomerEntryRoutine());
    }

    private IEnumerator CustomerEntryRoutine()
    {
        while (isRunning && spawnedCount < totalCustomerCount)
        {
            yield return new WaitForSeconds(spawnInterval);
            EnqueueNextCustomer();       // ★ ワイプに追加
            TryAdmitFromWipe();          // ★ 空席があれば即入店
        }

        // 全員生成後も待機列が残っていれば入店チェックを続ける
        while (isRunning && (WipeCanvas.Instance?.HasWaiting ?? false))
        {
            yield return new WaitForSeconds(1f);
            TryAdmitFromWipe();
        }
    }

    /// <summary>
    /// 待機列に客データを追加する。
    /// </summary>
    private void EnqueueNextCustomer()
    {
        if (spawnedCount >= totalCustomerCount) return;

        CustomerData data = customerVariations[Random.Range(0, customerVariations.Count)];
        CustomerMoodSO mood = availableMoods.Count > 0
            ? availableMoods[Random.Range(0, availableMoods.Count)]
            : null;

        var waitingData = new WaitingCustomerData
        {
            customerData = data,
            mood = mood,
            orders = GenerateOrders(data, mood),
        };

        WipeCanvas.Instance?.EnqueueCustomer(waitingData);
        spawnedCount++;

        Debug.Log($"<color=lime>[CustomerManager]</color> ワイプに追加（{spawnedCount}/{totalCustomerCount}）");
    }

    /// <summary>
    /// 空席があれば待機列の先頭を入店させる。
    /// </summary>
    public void TryAdmitFromWipe()
    {
        if (!(WipeCanvas.Instance?.HasWaiting ?? false)) return;

        Transform seat = GetEmptySeat();
        if (seat == null) return;

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
                // ★ 退場のたびに待機列から入店を試みる
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
            Debug.Log("<color=gold>[CustomerManager]</color> 全客退場 → ステージクリア");
            GameStateManager.Instance?.EnterResult();
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
                    foreach (var sushi in mood.fixedOrders)
                        orders.Add(sushi);
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

        // Random / Irritated → 通常生成
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