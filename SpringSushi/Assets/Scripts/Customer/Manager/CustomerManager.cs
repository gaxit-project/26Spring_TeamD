using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージデータに基づいて客をスポーンし、
/// 全客の退場を検知してステージクリアを通知する。
/// </summary>
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

    // --- StageDataSO から設定される ---
    private int totalCustomerCount;
    private float spawnInterval;
    private List<CustomerData> customerVariations = new();
    private List<SushiData> availableSushiList = new();

    // --- カウンタ ---
    private int spawnedCount = 0;
    private int exitedCount = 0;
    private bool isRunning = false;

    // ★ 予約済み席を管理
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
        reservedSeats.Clear(); // ★ リセット

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
            TrySpawnCustomer();
        }
    }

    private void TrySpawnCustomer()
    {
        if (spawnedCount >= totalCustomerCount) return;

        Transform seat = GetEmptySeat();
        if (seat == null)
        {
            Debug.Log("<color=yellow>[CustomerManager]</color> 満席のためスキップ。");
            return;
        }

        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;

        // ★ スポーン時点で席を予約
        reservedSeats.Add(seat);

        CustomerData data = customerVariations[Random.Range(0, customerVariations.Count)];
        GameObject obj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        CustomerAI ai = obj.GetComponent<CustomerAI>();
        if (ai == null)
        {
            reservedSeats.Remove(seat); // 失敗時は予約を解放
            return;
        }

        List<SushiData> orders = GenerateOrders(data);
        ai.Initialize(data, seat, orders);
        customerHUD?.RegisterCustomer(ai);

        spawnedCount++;
        Debug.Log($"<color=lime>[CustomerManager]</color> {seat.name} に客を生成（{spawnedCount}/{totalCustomerCount}）");

        // 退場カウント
        ai.OnStateChanged += OnCustomerStateChanged;

        // ★ Leaving時に予約を解放
        ai.OnStateChanged += (customerAI) =>
        {
            if (customerAI.State == CustomerAI.CustomerState.Leaving)
                reservedSeats.Remove(seat);
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

    private List<SushiData> GenerateOrders(CustomerData data)
    {
        var orders = new List<SushiData>();
        int count = Random.Range(1, data.maxTotalOrders + 1);
        for (int i = 0; i < count; i++)
            orders.Add(availableSushiList[Random.Range(0, availableSushiList.Count)]);
        return orders;
    }

    private Transform GetEmptySeat()
    {
        foreach (var seat in entryPoints)
        {
            // ★ 予約済みならスキップ
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