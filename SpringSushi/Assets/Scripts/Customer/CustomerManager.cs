using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// お客さんのスポーンと椅子（EntryPoint）管理を担当する。
///
/// 生成フロー：
/// CustomerSpawnPoint → (NavMesh移動) → EntryPoint（椅子）→ 注文開始
/// </summary>
public class CustomerManager : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject customerPrefab;

    [Header("スポーン設定")]
    public List<Transform> customerSpawnPoints = new();
    public List<Transform> entryPoints = new();         // 椅子の位置
    public float spawnInterval = 5f;

    [Header("お客さんのバリエーション")]
    public List<CustomerData> customerVariationList = new();

    [Header("HUD")]
    [SerializeField] private CustomerHUD customerHUD;

    [Header("注文候補")]
    public List<SushiData> availableSushiList = new();

    private void Start()
    {
        if (customerVariationList == null || customerVariationList.Count == 0) return;
        StartCoroutine(CustomerEntryRoutine());
    }

    private IEnumerator CustomerEntryRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawnCustomer();
        }
    }

    private void TrySpawnCustomer()
    {
        Transform seat = GetEmptySeat();
        if (seat == null)
        {
            Debug.Log("<color=yellow>[Manager]</color> 満席のためスキップ。");
            return;
        }

        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;

        CustomerData data = customerVariationList[Random.Range(0, customerVariationList.Count)];

        GameObject obj = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        CustomerAI ai = obj.GetComponent<CustomerAI>();
        if (ai == null) return;

        // 注文リストを生成
        List<SushiData> orders = GenerateOrders(data);
        ai.Initialize(data, seat, orders);

        // HUDに登録
        customerHUD?.RegisterCustomer(ai);

        Debug.Log($"<color=lime>[Manager]</color> {seat.name} に客を生成しました。");
    }

    /// <summary>
    /// CustomerDataを元に注文リストを生成する。
    /// </summary>
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
            Collider[] colliders = Physics.OverlapSphere(seat.position, 0.5f);
            bool occupied = false;
            foreach (var col in colliders)
            {
                if (col.CompareTag("Customer")) { occupied = true; break; }
            }
            if (!occupied) return seat;
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